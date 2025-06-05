using Autofac;
using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Services;
using DAL.Repositories;
using DAL.UnitOfWork;
using Domain.Entities;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Services
{
    [TestFixture]
    public class BookTypeServiceTests
    {
        private Autofac.IContainer _container = null!;

        [SetUp]
        public void Setup()
        {
            // 1) Мок IUnitOfWork і його BookTypes-репозиторій
            var uowMock = Substitute.For<IUnitOfWork>();
            var typeRepoMock = Substitute.For<IGenericRepository<BookTypeEntity>>();
            uowMock.BookTypes.Returns(typeRepoMock);

            // 2) Приклад доменних BookTypeEntity
            var domainTypes = new List<BookTypeEntity>
            {
                new BookTypeEntity { Id = 1, Name = "Electronic" },
                new BookTypeEntity { Id = 2, Name = "Paper" },
                new BookTypeEntity { Id = 3, Name = "Audio" }
            };

            // — налаштування ReadAllAsync()
            typeRepoMock.ReadAllAsync().Returns(Task.FromResult<IEnumerable<BookTypeEntity>>(domainTypes));


            // 3) Мок IMapper
            var mapperMock = Substitute.For<IMapper>();
            // ―― Мапінг одиничної сутності ――
            foreach (var t in domainTypes)
            {
                var dto = new BookTypeDto { Id = t.Id, Name = t.Name };
                mapperMock.Map<BookTypeDto>(t).Returns(dto);
            }
            // ―― Мапінг колекції ――
            mapperMock
                .Map<List<BookTypeDto>>(Arg.Any<List<BookTypeEntity>>())
                .Returns(call =>
                {
                    var listArg = call.Arg<List<BookTypeEntity>>();
                    return listArg
                        .Select(x => new BookTypeDto { Id = x.Id, Name = x.Name })
                        .ToList();
                });


            // 4) Будуємо Autofac-контейнер
            var builder = new ContainerBuilder();
            builder.RegisterInstance(uowMock).As<IUnitOfWork>().SingleInstance();
            builder.RegisterInstance(mapperMock).As<IMapper>().SingleInstance();
            builder.RegisterType<BookTypeService>()
                   .As<IBookTypeService>()
                   .InstancePerLifetimeScope();

            _container = builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public async Task ReadAllBookTypesAsync_Returns_All_BookTypeDtos()
        {
            // Arrange: налаштування виконано в SetUp()

            // Act
            var service = _container.Resolve<IBookTypeService>();
            var result = await service.ReadAllBookTypesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(r => r.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Electronic", "Paper", "Audio" });
        }
    }
}
