using NSubstitute;
using FluentAssertions;
using Autofac;
using AutoMapper;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using BLL.DTOs;
using BLL.Services;
using DAL.UnitOfWork;
using DAL.Repositories;
using BLL.Factory;
using BLL.Interfaces;

namespace Tests.Services
{
    [TestFixture]
    public class BookServiceTests
    {
        private Autofac.IContainer _container = null!;

        [SetUp]
        public void Setup()
        {
            // 1) Мок для IUnitOfWork і вкладеного IBookRepository
            var unitOfWorkMock = Substitute.For<IUnitOfWork>();
            var bookRepoMock = Substitute.For<IGenericRepository<Book>>();
            unitOfWorkMock.Books.Returns(bookRepoMock);

            // 2) Приклад двох доменних книг із різними AvailableTypes
            var domainBooks = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "First Book",
                    Author = "Author A",
                    Publisher = "Publisher X",
                    PublicationYear = 2019,
                    GenreId = 1,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = (int)BookType.Electronic, Name = "Electronic" },
                        new BookTypeEntity { Id = (int)BookType.Paper, Name = "Paper" }
                    },
                    InitialCopies = 5,
                    AvailableCopies = 5,
                    DownloadCount = 12,
                    ListenCount = 3,
                    Description = "Description A"
                },
                new Book
                {
                    Id = 2,
                    Title = "Second Book",
                    Author = "Author B",
                    Publisher = "Publisher Y",
                    PublicationYear = 2021,
                    GenreId = 2,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = (int)BookType.Audio, Name = "Audio" }
                    },
                    InitialCopies = 3,
                    AvailableCopies = 0,
                    DownloadCount = 7,
                    ListenCount = 15,
                    Description = "Description B"
                }
            };

            // Налаштовуємо bookRepoMock.ReadAll() -> IQueryable<domainBooks>
            bookRepoMock.ReadAll().Returns(domainBooks.AsQueryable());

            // 3) Мок для IBookFactory
            var factoryMock = Substitute.For<IBookFactory>();
            factoryMock.CreateAsync(Arg.Any<ActionBookDto>())
                .Returns(callInfo =>
                {
                    var dto = callInfo.ArgAt<ActionBookDto>(0);
                    // тут будуємо і повертаємо новий Domain.Entities.Book
                    // разом із заповненими копіями (Book.Copies), бо фабрика повертає Book
                    var newBook = new Book
                    {
                        Id = 0,
                        Title = dto.Title,
                        Author = dto.Author,
                        Publisher = dto.Publisher,
                        PublicationYear = dto.PublicationYear,
                        GenreId = dto.GenreId,
                        AvailableTypes = dto.AvailableTypeIds
                            .Select(id => new BookTypeEntity { Id = id, Name = id.ToString() })
                            .ToList(),
                        Copies = dto.AvailableTypeIds.Contains((int)BookType.Paper)
                            ? Enumerable.Range(1, dto.CopyCount)
                                .Select(i => new BookCopy { CopyNumber = i, IsAvailable = true })
                                .ToList()
                            : new List<BookCopy>(),
                        InitialCopies = dto.CopyCount,
                        AvailableCopies = dto.CopyCount,
                        DownloadCount = 0,
                        ListenCount = 0,
                        Description = dto.Description
                    };
                    return Task.FromResult(newBook);
                });

            // 4) Мок для IMapper
            var mapperMock = Substitute.For<IMapper>();
            // Map<Book, BookDto>
            foreach (var book in domainBooks)
            {
                var dto = new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Publisher = book.Publisher,
                    PublicationYear = book.PublicationYear,
                    GenreId = book.GenreId,
                    AvailableTypeIds = book.AvailableTypes.Select(at => at.Id).ToList(),
                    InitialCopies = book.InitialCopies,
                    AvailableCopies = book.AvailableCopies,
                    DownloadCount = book.DownloadCount,
                    ListenCount = book.ListenCount,
                    Description = book.Description
                };
                mapperMock.Map<BookDto>(book).Returns(dto);
            }

            // Map<ActionBookDto, Book> для CreateAsync
            mapperMock
                .Map<Book>(Arg.Any<ActionBookDto>())
                .Returns(callInfo =>
                {
                    var createDto = callInfo.ArgAt<ActionBookDto>(0);
                    return new Book
                    {
                        Id = 0,
                        Title = createDto.Title,
                        Author = createDto.Author,
                        Publisher = createDto.Publisher,
                        PublicationYear = createDto.PublicationYear,
                        GenreId = createDto.GenreId,
                        AvailableTypes = createDto.AvailableTypeIds
                            .Select(id => new BookTypeEntity { Id = id, Name = id.ToString() })
                            .ToList(),
                        InitialCopies = createDto.CopyCount,
                        AvailableCopies = createDto.CopyCount,
                        DownloadCount = 0,
                        ListenCount = 0,
                        Description = createDto.Description
                    };
                });

            // Map<ActionBookDto, Book> для UpdateAsync
            mapperMock
                .Map<Book>(Arg.Any<BookDto>())
                .Returns(callInfo =>
                {
                    var updateDto = callInfo.ArgAt<BookDto>(0);
                    return new Book
                    {
                        Id = updateDto.Id,
                        Title = updateDto.Title,
                        Author = updateDto.Author,
                        Publisher = updateDto.Publisher,
                        PublicationYear = updateDto.PublicationYear,
                        GenreId = updateDto.GenreId,
                        AvailableTypes = updateDto.AvailableTypeIds
                            .Select(id => new BookTypeEntity { Id = id, Name = id.ToString() })
                            .ToList(),
                        InitialCopies = updateDto.InitialCopies,
                        AvailableCopies = updateDto.AvailableCopies,
                        DownloadCount = updateDto.DownloadCount,
                        ListenCount = updateDto.ListenCount,
                        Description = updateDto.Description
                    };
                });

            // 5) Налаштуємо CreateAsync/Update/Delete на bookRepoMock
            bookRepoMock.CreateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);
            bookRepoMock.Update(Arg.Any<Book>());
            bookRepoMock.Delete(Arg.Any<Book>());

            // 6) Налаштуємо unitOfWorkMock.CommitAsync()
            unitOfWorkMock.CommitAsync().Returns(Task.FromResult(1));

            // 7) Будуємо Autofac-контейнер
            var builder = new ContainerBuilder();
            builder.RegisterInstance(unitOfWorkMock).As<IUnitOfWork>().SingleInstance();
            builder.RegisterInstance(factoryMock).As<IBookFactory>().SingleInstance();
            builder.RegisterInstance(mapperMock).As<IMapper>().SingleInstance();
            builder.RegisterType<BookService>().As<IBookService>().InstancePerLifetimeScope();

            _container = builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public async Task ReadAllAsync_Returns_All_BookDtos()
        {
            // Arrange: див. Setup

            // Act
            var service = _container.Resolve<IBookService>();
            var result = await service.ReadAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            result.Select(x => x.Title).Should().BeEquivalentTo(new[] { "First Book", "Second Book" });
        }

        [Test]
        public async Task ReadByIdAsync_ExistingId_Returns_Correct_BookDto()
        {
            // Arrange
            var service = _container.Resolve<IBookService>();

            // Act
            var result = await service.ReadByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("First Book");
        }

        [Test]
        public async Task ReadByIdAsync_NonExistingId_Returns_Null()
        {
            // Arrange
            var service = _container.Resolve<IBookService>();

            // Act
            var result = await service.ReadByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ReadByTypeAsync_Electronic_Returns_Only_Electronic_Books()
        {
            // Arrange
            var service = _container.Resolve<IBookService>();
            var electronicTypeId = (int)BookType.Electronic;

            // Act
            var result = await service.ReadByTypeAsync(electronicTypeId);

            // Assert
            result.Should().HaveCount(1);
            result.Single().Id.Should().Be(1);
            result.Single().AvailableTypeIds.Should().Contain(electronicTypeId);
        }

        [Test]
        public async Task CreateAsync_ValidDto_Creates_Book_And_Saves()
        {
            // Arrange
            var createDto = new ActionBookDto
            {
                Title = "New Book",
                Author = "Author New",
                Publisher = "Publisher New",
                PublicationYear = 2022,
                GenreId = 3,
                AvailableTypeIds = new List<int> { (int)BookType.Paper },
                CopyCount = 4,
                Description = "New Description"
            };

            var uow = _container.Resolve<IUnitOfWork>();
            var bookRepo = uow.Books;

            // Act
            var service = _container.Resolve<IBookService>();
            await service.CreateAsync(createDto);

            // Assert
            await bookRepo.Received(1).CreateAsync(Arg.Is<Book>(b =>
                b.Title == createDto.Title &&
                b.Author == createDto.Author &&
                b.Publisher == createDto.Publisher &&
                b.PublicationYear == createDto.PublicationYear &&
                b.GenreId == createDto.GenreId &&
                b.InitialCopies == createDto.CopyCount &&
                b.AvailableCopies == createDto.CopyCount &&
                b.Description == createDto.Description
            ));

            await uow.Received(1).CommitAsync();
        }

        [Test]
        public async Task UpdateAsync_ValidDto_Updates_Book_And_Saves()
        {
            // Arrange
            var updateDto = new BookDto
            {
                Id = 1,
                Title = "Updated Title",
                Author = "Updated Author",
                Publisher = "Updated Publisher",
                PublicationYear = 2020,
                GenreId = 1,
                AvailableTypeIds = new List<int> { (int)BookType.Electronic },
                InitialCopies = 5,
                AvailableCopies = 3,
                DownloadCount = 13,
                ListenCount = 4,
                Description = "Updated Description"
            };

            var uow = _container.Resolve<IUnitOfWork>();
            var bookRepo = uow.Books;

            // Act
            var service = _container.Resolve<IBookService>();
            await service.UpdateAsync(updateDto.Id, new ActionBookDto
            {
                Title = updateDto.Title,
                Author = updateDto.Author,
                Publisher = updateDto.Publisher,
                PublicationYear = updateDto.PublicationYear,
                GenreId = updateDto.GenreId,
                AvailableTypeIds = updateDto.AvailableTypeIds,
                CopyCount = updateDto.InitialCopies,
                Description = updateDto.Description
            });

            // Assert
            bookRepo.Received(1).Update(Arg.Is<Book>(b =>
                b.Id == updateDto.Id &&
                b.Title == updateDto.Title &&
                b.Author == updateDto.Author &&
                b.Publisher == updateDto.Publisher &&
                b.PublicationYear == updateDto.PublicationYear &&
                b.GenreId == updateDto.GenreId &&
                b.InitialCopies == updateDto.InitialCopies &&
                b.AvailableCopies == updateDto.AvailableCopies &&
                b.DownloadCount == updateDto.DownloadCount &&
                b.ListenCount == updateDto.ListenCount &&
                b.Description == updateDto.Description
            ));

            await uow.Received(1).CommitAsync();
        }

        [Test]
        public async Task DeleteAsync_ValidId_Deletes_Book_And_Saves()
        {
            // Arrange
            var uow = _container.Resolve<IUnitOfWork>();
            var bookRepo = uow.Books;

            // Налаштуємо ReadAll() так, щоб містити книгу з Id = 1
            var domainBook = new Book { Id = 1 };
            bookRepo.ReadAll().Returns(new[] { domainBook }.AsQueryable());

            // Act
            var service = _container.Resolve<IBookService>();
            await service.DeleteAsync(1);

            // Assert
            bookRepo.Received(1).Delete(Arg.Is<Book>(b => b.Id == 1));
            await uow.Received(1).CommitAsync();
        }
    }
}