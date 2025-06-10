using Autofac;
using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Services;
using DAL.Repositories;
using DAL.UnitOfWork;
using Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using NSubstitute.ExceptionExtensions;

namespace Tests.Services
{
    [TestFixture]
    public class GenreServiceTests
    {
        private Autofac.IContainer _container = null!;

        [SetUp]
        public void Setup()
        {
            // 1) Мок IUnitOfWork і його Genres-репозиторій
            var uowMock = Substitute.For<IUnitOfWork>();
            var genreRepoMock = Substitute.For<IGenericRepository<Genre>>();
            uowMock.Genres.Returns(genreRepoMock);

            // 2) Приклад доменних Genre-сутностей
            var domainGenres = new List<Genre>
            {
                new Genre { Id = 1, Name = "Science Fiction" },
                new Genre { Id = 2, Name = "Fantasy" },
                new Genre { Id = 3, Name = "Mystery" }
            };

            // Налаштування ReadAllAsync() повертає domainGenres
            genreRepoMock.ReadAllAsync()
                         .Returns(Task.FromResult<IEnumerable<Genre>>(domainGenres));

            genreRepoMock.ReadByIdAsync(1).Returns(Task.FromResult(domainGenres[0]));
            genreRepoMock.ReadByIdAsync(2).Returns(Task.FromResult(domainGenres[1]));
            // Для будь-якого іншого id повертаємо null (щоб сервіс кинув KeyNotFoundException)
            genreRepoMock
                .ReadByIdAsync(Arg.Is<int>(i => i != 1 && i != 2))
                .Returns(Task.FromResult<Genre>(null));


            // 3) Мок IMapper
            var mapperMock = Substitute.For<IMapper>();
            mapperMock
                .Map<Genre>(Arg.Any<GenreDto>())
                .Returns(call =>
                {
                    var dto = call.Arg<GenreDto>();
                    return new Genre { Id = dto.Id, Name = dto.Name };
                });

            // 3.2) Мапінг колекції IEnumerable<Genre> → IEnumerable<GenreDto>
            mapperMock
                .Map<IEnumerable<GenreDto>>(Arg.Any<IEnumerable<Genre>>())
                .Returns(call =>
                {
                    var listArg = call.Arg<IEnumerable<Genre>>();
                    return listArg.Select(x => new GenreDto { Id = x.Id, Name = x.Name });
                });

            // 4) Будуємо Autofac-контейнер
            var builder = new ContainerBuilder();
            builder.RegisterInstance(uowMock).As<IUnitOfWork>().SingleInstance();
            builder.RegisterInstance(mapperMock).As<IMapper>().SingleInstance();
            builder.RegisterType<GenreService>()
                   .As<IGenreService>()
                   .InstancePerLifetimeScope();
            _container = builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public async Task ReadAllAsync_Returns_All_GenreDtos()
        {
            // Arrange: налаштування виконано в SetUp()

            // Act
            var service = _container.Resolve<IGenreService>();
            var result = (await service.ReadAllAsync()).ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Select(r => r.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Science Fiction", "Fantasy", "Mystery" });
        }

        // Тест для ReadByIdAsync()
        [Test]
        public async Task ReadByIdAsync_ExistingId_Returns_Correct_GenreDto()
        {
            // 1. Arrange: три жанри, налаштовуємо ReadByIdAsync(1) і ReadByIdAsync(2)
            var genreRepoMock = Substitute.For<IGenericRepository<Genre>>();
            var domainGenres = new List<Genre>
            {
                new Genre { Id = 1, Name = "Science Fiction" },
                new Genre { Id = 2, Name = "Fantasy" }
            };
            genreRepoMock.ReadByIdAsync(1).Returns(Task.FromResult(domainGenres[0]));
            genreRepoMock.ReadByIdAsync(2).Returns(Task.FromResult(domainGenres[1]));
            // Інші id → null
            genreRepoMock
                .ReadByIdAsync(Arg.Is<int>(i => i != 1 && i != 2))
                .Returns(Task.FromResult<Genre>(null));

            // 2. Мок UoW
            var uowMock = Substitute.For<IUnitOfWork>();
            uowMock.Genres.Returns(genreRepoMock);

            // 3. Мок IMapper для одиничної сутності
            var mapperMock = Substitute.For<IMapper>();
            foreach (var g in domainGenres)
            {
                var dto = new GenreDto { Id = g.Id, Name = g.Name };
                mapperMock.Map<GenreDto>(g).Returns(dto);
            }

            var service = new GenreService(uowMock, mapperMock);

            // Act для id = 1
            var result1 = await service.ReadByIdAsync(1);
            result1.Should().NotBeNull();
            result1.Id.Should().Be(1);
            result1.Name.Should().Be("Science Fiction");

            // Act для id = 2
            var result2 = await service.ReadByIdAsync(2);
            result2.Should().NotBeNull();
            result2.Id.Should().Be(2);
            result2.Name.Should().Be("Fantasy");
        }

        [Test]
        public async Task ReadByIdAsync_NonExistingId_Throws_KeyNotFoundException()
        {
            // Arrange: аналогічно, але без жодного genreRepoMock.ReadByIdAsync(x)-> non-existing
            var genreRepoMock = Substitute.For<IGenericRepository<Genre>>();
            // Налаштуємо всі id -> null
            genreRepoMock
                .ReadByIdAsync(Arg.Any<int>())
                .Returns(Task.FromResult<Genre>(null));

            var uowMock = Substitute.For<IUnitOfWork>();
            uowMock.Genres.Returns(genreRepoMock);

            var mapperMock = Substitute.For<IMapper>();

            var service = new GenreService(uowMock, mapperMock);

            // Act & Assert
            await FluentActions
                .Invoking(() => service.ReadByIdAsync(999))
                .Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Genre 999 not found");
        }

        // Тест для CreateAsync()
        [Test]
        public async Task CreateAsync_WithValidDto_Calls_CreateAsync_Then_CommitAsync()
        {
            //  Arrange: мок репозиторію, налаштуємо CreateAsync і CommitAsync
            var genreRepoMock = Substitute.For<IGenericRepository<Genre>>();
            genreRepoMock.CreateAsync(Arg.Any<Genre>()).Returns(Task.CompletedTask);

            var uowMock = Substitute.For<IUnitOfWork>();
            uowMock.Genres.Returns(genreRepoMock);
            uowMock.CommitAsync().Returns(Task.FromResult(1));

            // 2. Мок IMapper для GenreDto -> Genre
            var mapperMock = Substitute.For<IMapper>();
            mapperMock
                .Map<Genre>(Arg.Any<GenreDto>())
                .Returns(call =>
                {
                    var dto = call.Arg<GenreDto>();
                    return new Genre { Id = dto.Id, Name = dto.Name };
                });

            var service = new GenreService(uowMock, mapperMock);

            var dto = new GenreDto { Id = 5, Name = "Horror" };

            // Act
            await service.CreateAsync(dto);

            // Assert
            mapperMock.Received(1).Map<Genre>(Arg.Is<GenreDto>(d => d.Id == 5 && d.Name == "Horror"));
            await genreRepoMock.Received(1).CreateAsync(Arg.Is<Genre>(g => g.Id == 5 && g.Name == "Horror"));
            await uowMock.Received(1).CommitAsync();
        }

        [Test]
        public async Task UpdateAsync_ExistingGenre_UpdatesEntityAndCommits()
        {
            // Arrange
            var uowMock = _container.Resolve<IUnitOfWork>();
            var genreRepoMock = uowMock.Genres;

            // Отримаємо сервіс
            var service = _container.Resolve<IGenreService>();

            // Готуюмо новий DTO для оновлення існуючого жанру (id=1)
            var updateDto = new GenreDto { Id = 1, Name = "Sci-Fi" };

            // Act
            await service.UpdateAsync(updateDto);

            // Assert
            // Перевіряємо, що IMapper.Map(dto, existing) було викликано
            var mapperMock = _container.Resolve<AutoMapper.IMapper>();
            mapperMock.Received(1).Map(Arg.Is<GenreDto>(d => d.Id == 1 && d.Name == "Sci-Fi"),
                                      Arg.Any<Genre>());

            //  Перевіряємо, що Update(...) було викликано з сутністю, у якої Id == 1
            genreRepoMock.Received(1).Update(Arg.Is<Genre>(g => g.Id == 1));

            // Перевіряємо, що CommitAsync() викликався
            await uowMock.Received(1).CommitAsync();
        }


        [Test]
        public void UpdateAsync_NonExistingGenre_ThrowsKeyNotFoundException()
        {
            // Arrange
            var uowMock = _container.Resolve<IUnitOfWork>();
            // Налаштуємо, що ReadByIdAsync для id = 999 повертає null
            var genreRepoMock = uowMock.Genres;
            genreRepoMock.ReadByIdAsync(999).Returns(Task.FromResult<Genre>(null));

            var mapperMock = _container.Resolve<AutoMapper.IMapper>();
            var service = _container.Resolve<IGenreService>();

            var nonExistingDto = new GenreDto { Id = 999, Name = "NonExistent" };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await service.UpdateAsync(nonExistingDto),
                "Якщо жанр не знайдено (ReadByIdAsync повертає null), маємо отримати KeyNotFoundException"
            );
        }

        [Test]
        public async Task DeleteAsync_ExistingGenre_DeletesEntityAndCommits()
        {
            // Arrange
            var uowMock = _container.Resolve<IUnitOfWork>();
            var genreRepoMock = uowMock.Genres;

            // Сервіс
            var service = _container.Resolve<IGenreService>();

            // Дія (для id = 1, який існує завдяки налаштуванню ReadByIdAsync у SetUp)
            await service.DeleteAsync(1);

            // Assert
            // Перевіряємо, що Delete(...) викликано для сутності з Id == 1
            genreRepoMock.Received(1).Delete(Arg.Is<Genre>(g => g.Id == 1));

            // Перевіряємо, що CommitAsync() викликано
            await uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void DeleteAsync_NonExistingGenre_ThrowsKeyNotFoundException()
        {
            // Arrange
            var uowMock = _container.Resolve<IUnitOfWork>();
            var genreRepoMock = uowMock.Genres;

            // Переналаштовуємо: ReadByIdAsync(999) повертає null
            genreRepoMock.ReadByIdAsync(999).Returns(Task.FromResult<Genre>(null));

            var service = _container.Resolve<IGenreService>();

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await service.DeleteAsync(999),
                "Якщо жанр не знайдено, DeleteAsync має кидати KeyNotFoundException"
            );
        }
    }
}
