using AutoMapper;
using BLL.DTOs;
using BLL.Services;
using BLL.Factory;
using DAL.Repositories;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Services
{
    [TestFixture]
    public class BookServiceTests
    {
        private IUnitOfWork _uowMock = null!;
        private IMapper _mapperMock = null!;
        private BookService _service = null!;

        private TestAppDbContext _inMemoryContext = null!;
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TestAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _inMemoryContext = new TestAppDbContext(options);

            // Mock IUnitOfWork
            _uowMock = Substitute.For<IUnitOfWork>();

            // Mock Books repository
            var booksRepo = Substitute.For<IGenericRepository<Book>>();
            var bookTypesRepo = Substitute.For<IGenericRepository<BookTypeEntity>>();
            _uowMock.Books.Returns(booksRepo);
            _uowMock.BookTypes.Returns(bookTypesRepo);

            // ReadAll() повертає фактичний InMemory DbSet (категорія з Include/... підтримується)
            booksRepo.ReadAll().Returns(_inMemoryContext.Books);

            // CommitAsync повертає Task<int>
            _uowMock.CommitAsync().Returns(Task.FromResult(0));

            // Mock AutoMapper
            _mapperMock = Substitute.For<IMapper>();

            // Create BookService with mocked UoW, Mapper, and a dummy IBookFactory
            _service = new BookService(
                _uowMock,
                _mapperMock,
                Substitute.For<IBookFactory>()
            );
        }

        [TearDown]
        public void TearDown()
        {
            _inMemoryContext.Dispose();
            if (_uowMock is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [Test]
        public async Task ReadAllAsync_WithExistingBooks_ReturnsMappedDtos()
        {
            // ARRANGE
            var domainBooks = new List<Book>
            {
                new Book
                {
                    Id = 10,
                    Title = "C# in Depth",
                    Author = "Jon Skeet",
                    Description = "Deep dive into C#",
                    Publisher = "Manning",
                    PublicationYear = 2020,
                    GenreId = 1,
                    Genre = null,
                    AvailableTypes = new List<BookTypeEntity>(),
                    Copies = new List<BookCopy>()
                },
                new Book
                {
                    Id = 11,
                    Title = "Effective Java",
                    Author = "Joshua Bloch",
                    Description = "Best practices for Java",
                    Publisher = "Addison-Wesley",
                    PublicationYear = 2018,
                    GenreId = 2,
                    Genre = null,
                    AvailableTypes = new List<BookTypeEntity>(),
                    Copies = new List<BookCopy>()
                }
            };

            // Конвертуємо в async IQueryable
            var asyncBooks = new TestAsyncEnumerable<Book>(domainBooks);

            //налаштовуємо ReadAll() щоб повертало цей async IQueryable
            _uowMock.Books.ReadAll().Returns(asyncBooks);

            // Налаштовуємо AutoMapper щоб мапити Book в BookDto
            _mapperMock
                .Map<IEnumerable<BookDto>>(Arg.Any<IEnumerable<Book>>())
                .Returns(call =>
                {
                    var source = call.Arg<IEnumerable<Book>>();
                    return source.Select(b => new BookDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Author = b.Author,
                        Description = b.Description,
                        Publisher = b.Publisher,
                        PublicationYear = b.PublicationYear,
                        GenreId = b.GenreId
                    }).ToList();
                });

            // ACT
            var result = (await _service.ReadAllAsync()).ToList();

            // ASSERT
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(10);
            result[0].Title.Should().Be("C# in Depth");
            result[0].Author.Should().Be("Jon Skeet");
            result[0].Description.Should().Be("Deep dive into C#");
            result[0].Publisher.Should().Be("Manning");
            result[0].PublicationYear.Should().Be(2020);
            result[0].GenreId.Should().Be(1);

            result[1].Id.Should().Be(11);
            result[1].Title.Should().Be("Effective Java");
            result[1].Author.Should().Be("Joshua Bloch");
            result[1].Description.Should().Be("Best practices for Java");
            result[1].Publisher.Should().Be("Addison-Wesley");
            result[1].PublicationYear.Should().Be(2018);
            result[1].GenreId.Should().Be(2);
        }

        [Test]
        public async Task ReadAllAsync_WhenNoBooks_ReturnsEmptyList()
        {
            // ARRANGE
            var emptyList = new List<Book>();
            var asyncEmpty = new TestAsyncEnumerable<Book>(emptyList);
            _uowMock.Books.ReadAll().Returns(asyncEmpty);

            _mapperMock
                .Map<IEnumerable<BookDto>>(Arg.Any<IEnumerable<Book>>())
                .Returns(new List<BookDto>());

            // ACT
            var result = (await _service.ReadAllAsync()).ToList();

            // ASSERT
            result.Should().BeEmpty();
        }

        [Test]
        public void ReadByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            // Нічого не додаємо до InMemoryContext -> таблиця Books порожня
            _uowMock.Books
                .ReadAll()
                .Returns(_inMemoryContext.Books);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.ReadByIdAsync(99));

            Assert.That(ex.Message, Is.EqualTo("Book with Id = 99 not found."));

            _uowMock.Books.Received(1).ReadAll();
            _mapperMock.DidNotReceive().Map<BookDto>(Arg.Any<Book>());
        }

        [Test]
        public async Task ReadByTypeAsync_WithMatchingBooks_ReturnsMappedDtos()
        {
            // ARRANGE
            // Створюємо кілька книжок, дві з яких мають AvailableTypes typeId=1
            var domainBooks = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "Paper Book A",
                    Author = "Author A",
                    Description = "Desc A",
                    Publisher = "Pub A",
                    PublicationYear = 2001,
                    GenreId = 10,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = 1, Name = "Paper" }
                    },
                    Copies = new List<BookCopy>()
                },
                new Book
                {
                    Id = 2,
                    Title = "E-Book B",
                    Author = "Author B",
                    Description = "Desc B",
                    Publisher = "Pub B",
                    PublicationYear = 2002,
                    GenreId = 20,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = 2, Name = "Electronic" }
                    },
                    Copies = new List<BookCopy>()
                },
                new Book
                {
                    Id = 3,
                    Title = "Paper Book C",
                    Author = "Author C",
                    Description = "Desc C",
                    Publisher = "Pub C",
                    PublicationYear = 2003,
                    GenreId = 10,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = 1, Name = "Paper" },
                        new BookTypeEntity { Id = 2, Name = "Electronic" }
                    },
                    Copies = new List<BookCopy>()
                }
            };

            var asyncBooks = new TestAsyncEnumerable<Book>(domainBooks);

            // налаштовуємо ReadAll() щоб повенути async IQueryable
            _uowMock.Books.ReadAll().Returns(asyncBooks);

            // Налаштовуємо AutoMapper щоб мапити List<Book> -> List<BookDto>
            _mapperMock
                .Map<List<BookDto>>(Arg.Any<List<Book>>())
                .Returns(call =>
                {
                    var source = call.Arg<List<Book>>();
                    return source.Select(b => new BookDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Author = b.Author,
                        Description = b.Description,
                        Publisher = b.Publisher,
                        PublicationYear = b.PublicationYear,
                        GenreId = b.GenreId
                    }).ToList();
                });

            // ACT: знайти тільки typeId = 1 (Paper)
            var result = await _service.ReadByTypeAsync(1);

            // ASSERT
            // Має повернути 2 книжки: Id=1 and Id=3
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(new BookDto
            {
                Id = 1,
                Title = "Paper Book A",
                Author = "Author A",
                Description = "Desc A",
                Publisher = "Pub A",
                PublicationYear = 2001,
                GenreId = 10
            });
            result.Should().ContainEquivalentOf(new BookDto
            {
                Id = 3,
                Title = "Paper Book C",
                Author = "Author C",
                Description = "Desc C",
                Publisher = "Pub C",
                PublicationYear = 2003,
                GenreId = 10
            });

            _uowMock.Books.Received(1).ReadAll();
            _mapperMock.Received(1).Map<List<BookDto>>(Arg.Any<List<Book>>());
        }

        [Test]
        public async Task ReadByTypeAsync_WhenNoMatchingBooks_ReturnsEmptyList()
        {
            // ARRANGE
            var domainBooks = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "Audio Book",
                    Author = "Author X",
                    Description = "Desc X",
                    Publisher = "Pub X",
                    PublicationYear = 2010,
                    GenreId = 5,
                    AvailableTypes = new List<BookTypeEntity>
                    {
                        new BookTypeEntity { Id = 3, Name = "Audio" }
                    },
                    Copies = new List<BookCopy>()
                }
            };

            var asyncBooks = new TestAsyncEnumerable<Book>(domainBooks);
            _uowMock.Books.ReadAll().Returns(asyncBooks);

            _mapperMock
                .Map<List<BookDto>>(Arg.Any<List<Book>>())
                .Returns(new List<BookDto>());

            // ACT: шукаємо typeId = 1 (Paper), але жоден не підходить
            var result = await _service.ReadByTypeAsync(1);

            // ASSERT
            result.Should().BeEmpty();
            _uowMock.Books.Received(1).ReadAll();
            _mapperMock.Received(1).Map<List<BookDto>>(Arg.Any<List<Book>>());
        }

        [Test]
        public async Task CreateAsync_PaperType_CreatesAvailableTypesAndCopiesAndSaves()
        {
            // ARRANGE
            var dto = new ActionBookDto
            {
                Title = "Test Book",
                Author = "Author X",
                Description = "Desc",
                Publisher = "Pub",
                PublicationYear = 2021,
                GenreId = 5,
                AvailableTypeIds = new List<int> { (int)BookType.Paper, (int)BookType.Electronic, (int)BookType.Paper },
                CopyCount = 3
            };

            // Мокаємо AutoMapper: Map<ActionBookDto, Book>
            _mapperMock
                .Map<Book>(Arg.Is<ActionBookDto>(a => a == dto))
                .Returns(call =>
                {
                    var inDto = call.Arg<ActionBookDto>();
                    return new Book
                    {
                        Title = inDto.Title,
                        Author = inDto.Author,
                        Description = inDto.Description,
                        Publisher = inDto.Publisher,
                        PublicationYear = inDto.PublicationYear,
                        GenreId = inDto.GenreId
                        // AvailableTypes і Copies залишаються null – сервіс їх створить
                    };
                });

            // Мокаємо BookTypes.ReadByIdAsync:
            var paperType = new BookTypeEntity { Id = (int)BookType.Paper, Name = "Paper" };
            var electronicType = new BookTypeEntity { Id = (int)BookType.Electronic, Name = "Electronic" };

            _uowMock.BookTypes
                .ReadByIdAsync((int)BookType.Paper)
                .Returns(Task.FromResult<BookTypeEntity?>(paperType));
            _uowMock.BookTypes
                .ReadByIdAsync((int)BookType.Electronic)
                .Returns(Task.FromResult<BookTypeEntity?>(electronicType));

            // Захоплюємо аргумент, переданий до CreateAsync
            Book? createdBook = null;
            _uowMock.Books
                .When(r => r.CreateAsync(Arg.Any<Book>()))
                .Do(ci => createdBook = ci.Arg<Book>());
            _uowMock.Books.CreateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            // Мокаємо CommitAsync як Task<int>
            _uowMock.CommitAsync().Returns(Task.FromResult(0));

            // ACT
            await _service.CreateAsync(dto);

            // ASSERT
            createdBook.Should().NotBeNull();
            // AvailableTypes має містити саме paperType та electronicType (distinct)
            createdBook!.AvailableTypes.Should().HaveCount(2);
            createdBook.AvailableTypes.Select(t => t.Id)
                        .Should().BeEquivalentTo(new[] { paperType.Id, electronicType.Id });

            // Copies має містити 3 записи з CopyNumber 1,2,3 і IsAvailable = true
            createdBook.Copies.Should().HaveCount(3);
            createdBook.Copies.Select(c => c.CopyNumber).OrderBy(n => n)
                        .Should().Equal(new[] { 1, 2, 3 });
            createdBook.Copies.All(c => c.IsAvailable).Should().BeTrue();

            // CommitAsync мав викликатися один раз
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public async Task CreateAsync_NoPaperType_CopiesRemainsEmptyAndSaves()
        {
            // ARRANGE
            var dto = new ActionBookDto
            {
                Title = "Audio Book",
                Author = "Author Y",
                Description = "DescY",
                Publisher = "PubY",
                PublicationYear = 2022,
                GenreId = 6,
                AvailableTypeIds = new List<int> { (int)BookType.Audio },
                CopyCount = 5 
            };

            _mapperMock
                .Map<Book>(Arg.Any<ActionBookDto>())
                .Returns(call =>
                {
                    var inDto = call.Arg<ActionBookDto>();
                    return new Book
                    {
                        Title = inDto.Title,
                        Author = inDto.Author,
                        Description = inDto.Description,
                        Publisher = inDto.Publisher,
                        PublicationYear = inDto.PublicationYear,
                        GenreId = inDto.GenreId
                    };
                });

            var audioType = new BookTypeEntity { Id = (int)BookType.Audio, Name = "Audio" };
            _uowMock.BookTypes
                .ReadByIdAsync((int)BookType.Audio)
                .Returns(Task.FromResult<BookTypeEntity?>(audioType));

            Book? createdBook = null;
            _uowMock.Books
                .When(r => r.CreateAsync(Arg.Any<Book>()))
                .Do(ci => createdBook = ci.Arg<Book>());
            _uowMock.Books.CreateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            // Мокаємо CommitAsync як Task<int>
            _uowMock.CommitAsync().Returns(Task.FromResult(0));

            // ACT
            await _service.CreateAsync(dto);

            // ASSERT
            createdBook.Should().NotBeNull();
            // AvailableTypes містить лише audioType
            createdBook!.AvailableTypes.Should().HaveCount(1);
            createdBook.AvailableTypes.First().Id.Should().Be(audioType.Id);

            // Copies повинно бути пустим
            createdBook.Copies.Should().BeEmpty();

            // CommitAsync викликаний
            await _uowMock.Received(1).CommitAsync();
        }


        [Test]
        public void CreateAsync_MissingType_ThrowsKeyNotFoundException()
        {
            // ARRANGE: AvailableTypeIds містить id=99, але ReadByIdAsync(99) повертає null
            var dto = new ActionBookDto
            {
                Title = "Unknown Type Book",
                Author = "Author Z",
                Description = "DescZ",
                Publisher = "PubZ",
                PublicationYear = 2023,
                GenreId = 7,
                AvailableTypeIds = new List<int> { 99 },
                CopyCount = 2
            };

            _mapperMock
                .Map<Book>(Arg.Any<ActionBookDto>())
                .Returns(new Book());

            _uowMock.BookTypes
                .ReadByIdAsync(99)
                .Returns(Task.FromResult<BookTypeEntity?>(null));

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.CreateAsync(dto)
            );
            Assert.That(ex.Message, Is.EqualTo("BookTypeEntity with Id = 99 not found."));

            // CreateAsync книжки не мав викликатися
            _uowMock.Books.DidNotReceive().CreateAsync(Arg.Any<Book>());
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task UpdateAsync_ExistingBook_UpdatesFieldsAndTypesAndCopies()
        {
            // ARRANGE
            // Додаємо BookTypeEntity для Id=1 і Id=2 у контекст
            var initialType1 = new BookTypeEntity { Id = 1, Name = "Paper" };
            var initialType2 = new BookTypeEntity { Id = 2, Name = "Electronic" };
            _inMemoryContext.BookTypes.AddRange(initialType1, initialType2);
            _inMemoryContext.SaveChanges();

            // Додаємо доменну Book із початковими властивостями, включаючи AvailableTypes і Copies
            var domainBook = new Book
            {
                Id = 100,
                Title = "Old Title",
                Author = "Old Author",
                Publisher = "Old Pub",
                PublicationYear = 2000,
                GenreId = 10,
                InitialCopies = 2,
                AvailableCopies = 2,
                Description = "Old Desc",
                AvailableTypes = new List<BookTypeEntity> { initialType1, initialType2 },
                Copies = new List<BookCopy>
                {
                    new BookCopy { BookId = 100, CopyNumber = 1, IsAvailable = true },
                    new BookCopy { BookId = 100, CopyNumber = 2, IsAvailable = false }
                }
            };
            _inMemoryContext.Books.Add(domainBook);
            _inMemoryContext.SaveChanges();

            // Додаємо новий тип Id=3 у контекст
            var newType = new BookTypeEntity { Id = 3, Name = "Audio" };
            _inMemoryContext.BookTypes.Add(newType);
            _inMemoryContext.SaveChanges();

            // Стабізуємо BookTypes.ReadByIdAsync(3) через реальний контекст
            _uowMock.BookTypes
                .ReadByIdAsync(3)
                .Returns(_inMemoryContext.BookTypes.FindAsync(3).AsTask());

            // Готуємо ActionBookDto із новими полями:
            //    — прибираємо тип 2, залишаємо 1, додаємо 3; CopyCount = 4
            var dto = new ActionBookDto
            {
                Title = "New Title",
                Author = "New Author",
                Publisher = "New Pub",
                PublicationYear = 2021,
                GenreId = 20,
                Description = "New Desc",
                CopyCount = 4,
                AvailableTypeIds = new List<int> { 1, 3 }
            };

            // ACT
            await _service.UpdateAsync(domainBook.Id, dto);

            // ASSERT
            // Примітивні поля оновлено
            domainBook.Title.Should().Be("New Title");
            domainBook.Author.Should().Be("New Author");
            domainBook.Publisher.Should().Be("New Pub");
            domainBook.PublicationYear.Should().Be(2021);
            domainBook.GenreId.Should().Be(20);
            domainBook.Description.Should().Be("New Desc");
            domainBook.InitialCopies.Should().Be(4);
            domainBook.AvailableCopies.Should().Be(4);

            // AvailableTypes: мають бути лише типи 1 та 3
            domainBook.AvailableTypes.Select(t => t.Id)
                .OrderBy(id => id)
                .Should().Equal(new[] { 1, 3 });

            // Copies: створено 4 нові копії з CopyNumber 1..4, усі IsAvailable = true
            domainBook.Copies.Should().HaveCount(4);
            domainBook.Copies.Select(c => c.CopyNumber).OrderBy(n => n)
                .Should().Equal(new[] { 1, 2, 3, 4 });
            domainBook.Copies.All(c => c.IsAvailable).Should().BeTrue();

            // Перевіряємо виклик CommitAsync
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void UpdateAsync_NonExistingBook_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            // Нічого не додаємо у _inMemoryContext.Books -> колекція порожня
            var dto = new ActionBookDto
            {
                Title = "Whatever",
                Author = "Author",
                Publisher = "Pub",
                PublicationYear = 2022,
                GenreId = 5,
                Description = "Desc",
                CopyCount = 1,
                AvailableTypeIds = new List<int> { 1 }
            };

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateAsync(999, dto));
            Assert.That(ex.Message, Is.EqualTo("Book with Id = 999 not found."));

            _uowMock.Books.Received(1).ReadAll();
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public void UpdateAsync_MissingType_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            // Додаємо один тип (Id=1) та книгу без типів
            var existingBook = new Book
            {
                Id = 50,
                Title = "Title",
                Author = "Author",
                Publisher = "Pub",
                PublicationYear = 2005,
                GenreId = 10,
                Description = "Desc",
                InitialCopies = 2,
                AvailableCopies = 2,
                AvailableTypes = new List<BookTypeEntity>(),
                Copies = new List<BookCopy>()
            };
            _inMemoryContext.Books.Add(existingBook);
            _inMemoryContext.SaveChanges();

            // Стабізуємо BookTypes.ReadByIdAsync(99) -> null
            _uowMock.BookTypes.ReadByIdAsync(99)
                .Returns(Task.FromResult<BookTypeEntity?>(null));

            // ReadAll() повертає контекстову колекцію
            _uowMock.Books.ReadAll().Returns(_inMemoryContext.Books);

            var dto = new ActionBookDto
            {
                Title = "Updated",
                Author = "NewAuth",
                Publisher = "NewPub",
                PublicationYear = 2023,
                GenreId = 15,
                Description = "NewDesc",
                CopyCount = 2,
                AvailableTypeIds = new List<int> { 99 }
            };

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateAsync(existingBook.Id, dto));
            Assert.That(ex.Message, Is.EqualTo("BookTypeEntity with Id = 99 not found."));

            _uowMock.Books.Received(1).ReadAll();
            _uowMock.BookTypes.Received(1).ReadByIdAsync(99);
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task DeleteAsync_ExistingBook_DeletesAndCommits()
        {
            // ARRANGE
            var bookId = 123;
            var existingBook = new Book
            {
                Id = bookId,
                Title = "Title"
            };

            // Налаштовуємо ReadByIdAsync назад existingBook
            _uowMock.Books
                .ReadByIdAsync(bookId)
                .Returns(Task.FromResult(existingBook));

            // ACT
            await _service.DeleteAsync(bookId);

            // ASSERT
            _uowMock.Books.Received(1).ReadByIdAsync(bookId);
            _uowMock.Books.Received(1).Delete(existingBook);
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void DeleteAsync_NonExistingBook_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var missingId = 999;
            _uowMock.Books
                .ReadByIdAsync(missingId)
                .Returns(Task.FromResult<Book?>(null));

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.DeleteAsync(missingId));
            Assert.That(ex.Message, Is.EqualTo($"Book with Id = {missingId} not found."));

            _uowMock.Books.Received(1).ReadByIdAsync(missingId);
            _uowMock.Books.DidNotReceive().Delete(Arg.Any<Book>());
            _uowMock.DidNotReceive().CommitAsync();
        }
    // -------------------------------------------
    // Async helper implementation for IQueryable
    // -------------------------------------------
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }

        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return new TestAsyncEnumerable<TEntity>(stripped);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return new TestAsyncEnumerable<TElement>(stripped);
            }

            public object Execute(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return _inner.Execute(stripped);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return _inner.Execute<TResult>(stripped);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return new TestAsyncEnumerable<TResult>(stripped);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var stripped = StripEfMethods(expression);
                return _inner.Execute<TResult>(stripped);
            }

            private static Expression StripEfMethods(Expression expression)
            {
                if (expression is MethodCallExpression mce)
                {
                    // Якщо Include(...), drop it
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name.StartsWith("Include", StringComparison.Ordinal))
                    {
                        return StripEfMethods(mce.Arguments[0]);
                    }

                    // Якщо FirstOrDefaultAsync(...), transform to Queryable.FirstOrDefault(...)
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync))
                    {
                        var sourceExpr = StripEfMethods(mce.Arguments[0]);
                        var predicateExpr = (LambdaExpression)mce.Arguments[1];
                        var whereCall = Expression.Call(
                            typeof(Queryable),
                            nameof(Queryable.Where),
                            new[] { typeof(TEntity) },
                            sourceExpr,
                            predicateExpr);
                        var firstCall = Expression.Call(
                            typeof(Queryable),
                            nameof(Queryable.FirstOrDefault),
                            new[] { typeof(TEntity) },
                            whereCall);
                        return firstCall;
                    }

                    // Other calls: recurse
                    var newArgs = mce.Arguments.Select(arg => StripEfMethods(arg)).ToArray();
                    var newObj = mce.Object != null ? StripEfMethods(mce.Object) : null;
                    return mce.Update(newObj, newArgs);
                }
                return expression;
            }
        }

        // ----------------------------------------------------
        // InMemory DbContext (not directly used in these tests)
        // ----------------------------------------------------
        public class TestAppDbContext : DbContext
        {
            public TestAppDbContext(DbContextOptions<TestAppDbContext> options)
                : base(options)
            { }

            public DbSet<Book> Books { get; set; } = null!;
            public DbSet<BookCopy> BookCopies { get; set; } = null!;
            public DbSet<BookTypeEntity> BookTypes { get; set; } = null!;
            public DbSet<Genre> Genres { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<BookCopy>()
                    .HasOne<Book>()
                    .WithMany(b => b.Copies)
                    .HasForeignKey(bc => bc.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
}
