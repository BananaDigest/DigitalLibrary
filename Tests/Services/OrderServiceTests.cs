using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using BLL.Services;
using BLL.DTOs;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;


namespace Tests.Services
{
    [TestFixture]
    public class OrderServiceTests
    {
        private IFixture _fixture = null!;
        private IUnitOfWork _uowMock = null!;
        private AutoMapper.IMapper _mapperMock = null!;
        private OrderService _service = null!;

        private TestAppDbContext _inMemoryContext = null!;

        [SetUp]
        public void SetUp()
        {
            // 1) Налаштовуємо AutoFixture з AutoNSubstituteCustomization
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            // 2) Мок IUnitOfWork і репозиторій Orders
            _uowMock = _fixture.Freeze<IUnitOfWork>();
            var ordersRepoMock = Substitute.For<IGenericRepository<Order>>();
            _uowMock.Orders.Returns(ordersRepoMock);


            // 4) Мок IMapper: Map<List<OrderDto>>(List<Order>) → List<OrderDto>
            _mapperMock = Substitute.For<AutoMapper.IMapper>();
            _mapperMock
                .Map<List<OrderDto>>(Arg.Any<List<Order>>())
                .Returns(call =>
                {
                    var listArg = call.Arg<List<Order>>();
                    return listArg.Select(o => new OrderDto
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        BookId = o.BookId,
                        BookCopyId = o.BookCopyId,
                        OrderType = (BookType)o.OrderTypeId,
                        OrderDate = o.OrderDate
                    }).ToList();
                });

            // 5) Створюємо сам сервіс
            _service = new OrderService(_uowMock, _mapperMock);

            // 5) Налаштовуємо InMemory DbContext (роздільна база для кожного тесту)
            var options = new DbContextOptionsBuilder<TestAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _inMemoryContext = new TestAppDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _inMemoryContext.Dispose();
            if (_uowMock is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public async Task ReadAllAsync_WithExistingOrders_ReturnsMappedDtos()
        {
            // Arrange
            var domainOrders = new List<Order>
            {
                new Order
                {
                    Id = 10,
                    UserId = 100,
                    BookId = 200,
                    BookCopyId = 300,
                    OrderTypeId = (int)BookType.Paper,
                    OrderDate = new DateTime(2022, 1, 1),
                    Book = null,
                    BookCopy = null,
                    OrderType = null
                },
                new Order
                {
                    Id = 11,
                    UserId = 101,
                    BookId = 201,
                    BookCopyId = null,
                    OrderTypeId = (int)BookType.Electronic,
                    OrderDate = new DateTime(2022, 2, 2),
                    Book = null,
                    BookCopy = null,
                    OrderType = null
                }
            };

            // 1) Перетворюємо у асинхронний IQueryable
            var asyncOrders = new TestAsyncEnumerable<Order>(domainOrders);

            // 2) Налаштовуємо репозиторій
            _uowMock.Orders.ReadAllOrder().Returns(asyncOrders);

            // Act
            var result = (await _service.ReadAllAsync()).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(10);
            result[0].UserId.Should().Be(100);
            result[0].BookId.Should().Be(200);
            result[0].BookCopyId.Should().Be(300);
            result[0].OrderType.Should().Be(BookType.Paper);
            result[0].OrderDate.Should().Be(new DateTime(2022, 1, 1));

            result[1].Id.Should().Be(11);
            result[1].UserId.Should().Be(101);
            result[1].BookId.Should().Be(201);
            result[1].BookCopyId.Should().BeNull();
            result[1].OrderType.Should().Be(BookType.Electronic);
            result[1].OrderDate.Should().Be(new DateTime(2022, 2, 2));
        }

        // ----------------------------------------------
        // Тест 2: порожня колекція → повернення пустого списку
        // ----------------------------------------------
        [Test]
        public async Task ReadAllAsync_WhenNoOrders_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<Order>();
            var asyncEmpty = new TestAsyncEnumerable<Order>(emptyList);

            _uowMock.Orders.ReadAllOrder().Returns(asyncEmpty);

            // Act
            var result = (await _service.ReadAllAsync()).ToList();

            // Assert
            result.Should().BeEmpty("тому що репозиторій повернув порожню колекцію");
        }

        //-----------------------------------------------------------------------
        // Тест 3: якщо ReadAllOrder() поверне null, має викликатися NullReferenceException
        //-----------------------------------------------------------------------
        [Test]
        public void ReadAllAsync_WhenReadAllOrderReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            _uowMock.Orders.ReadAllOrder().Returns((IQueryable<Order>)null!);

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(
                async () => await _service.ReadAllAsync(),
                "Якщо ReadAllOrder() повертає null, то спроба Include/ToListAsync має дати NullReferenceException"
            );
        }

        //-----------------------------------------------------------------------
        // Тест 4: якщо Map<List<OrderDto>>() кидає помилку – користувач бачить той самий виняток
        //-----------------------------------------------------------------------
        [Test]
        public void ReadAllAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            var domainOrders = new List<Order>
            {
                new Order
                {
                    Id = 5,
                    UserId = 20,
                    BookId = 30,
                    BookCopyId = null,
                    OrderTypeId = (int)BookType.Audio,
                    OrderDate = DateTime.UtcNow,
                    Book = null,
                    BookCopy = null,
                    OrderType = null
                }
            };
            var asyncOrders = new TestAsyncEnumerable<Order>(domainOrders);
            _uowMock.Orders.ReadAllOrder().Returns(asyncOrders);

            _mapperMock
                .When(m => m.Map<List<OrderDto>>(Arg.Any<List<Order>>()))
                .Do(call => throw new AutoMapper.AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<AutoMapper.AutoMapperMappingException>(
                async () => await _service.ReadAllAsync()
            );
            Assert.That(ex.Message, Is.EqualTo("Mapping failed"));
        }

        //[Test]
        //public async Task ReadByIdAsync_ExistingId_ReturnsMappedDto()
        //{
        //    // ARRANGE
        //    var domainOrder = new Order
        //    {
        //        Id = 42,
        //        UserId = 7,
        //        BookId = 13,
        //        BookCopyId = 21,
        //        OrderTypeId = (int)BookType.Audio,
        //        OrderDate = new DateTime(2023, 3, 15),
        //        Book = null,
        //        BookCopy = null,
        //        OrderType = null
        //    };

        //    // 1) Додаємо запис у InMemory‐таблицю
        //    _inMemoryContext.Orders.Add(domainOrder);
        //    _inMemoryContext.SaveChanges();

        //    // 2) Підсовуємо репозиторію “живий” DbSet для коректного FirstOrDefaultAsync
        //    _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders);

        //    // ACT
        //    var dto = await _service.ReadByIdAsync(42);

        //    // ASSERT
        //    dto.Should().NotBeNull();
        //    dto.Id.Should().Be(42);
        //    dto.UserId.Should().Be(7);
        //    dto.BookId.Should().Be(13);
        //    dto.BookCopyId.Should().Be(21);
        //    dto.OrderType.Should().Be(BookType.Audio);
        //    dto.OrderDate.Should().Be(new DateTime(2023, 3, 15));
        //}

        //#TODO спитати у Віки, що робити з цими тестами GetById в Orders,
        //тобто залишити так, як є (лише ReadByIdAsync_WhenReadAllOrderReturnsNull_ThrowsNullReferenceException)
        //чи додати ще тестів

        // -------------------------------------------------------------------
        // Тест 3: ReadAllOrder() повертає null → NullReferenceException
        // -------------------------------------------------------------------
        [Test]
        public void ReadByIdAsync_WhenReadAllOrderReturnsNull_ThrowsNullReferenceException()
        {
            // ARRANGE
            _uowMock.Orders.ReadAllOrder().Returns((IQueryable<Order>)null!);

            // ACT & ASSERT
            Assert.ThrowsAsync<NullReferenceException>(
                async () => await _service.ReadByIdAsync(5),
                "Якщо ReadAllOrder() повертає null, виклик Include/FirstOrDefaultAsync дасть NullReferenceException"
            );
        }

        [Test]
        public async Task ReadByUserAsync_WithMixedUserIds_ReturnsOnlyMatchedDtos()
        {
            // ARRANGE: готуємо список із трьох замовлень, два з userId = 7, одне – з іншим
            var domainOrders = new List<Order>
            {
                new Order
            {
            Id = 1,
            UserId = 7,
            BookId = 10,
            BookCopyId = 100,
            OrderTypeId = (int)BookType.Paper,
            OrderDate = new DateTime(2023, 1, 1),
            Book = null,
            BookCopy = null,
            OrderType = null
        },
            new Order
            {
                Id = 2,
                UserId = 7,
                BookId = 11,
                BookCopyId = 101,
                OrderTypeId = (int)BookType.Electronic,
                OrderDate = new DateTime(2023, 2, 2),
                Book = null,
                BookCopy = null,
                OrderType = null
            },
            new Order
            {
                Id = 3,
                UserId = 8,
                BookId = 12,
                BookCopyId = 102,
                OrderTypeId = (int)BookType.Audio,
                OrderDate = new DateTime(2023, 3, 3),
                Book = null,
                BookCopy = null,
                OrderType = null
            }
        };

            // «Фейковий» IQueryable<Order> зі всіма замовленнями
            var asyncOrders = new TestAsyncEnumerable<Order>(domainOrders);
            _uowMock.Orders.ReadAllOrder().Returns(asyncOrders);

            // ACT: забираємо тільки ті, в яких UserId = 7
            var result = (await _service.ReadByUserAsync(7)).ToList();

            // ASSERT: матимемо рівно два DTO, і їхні поля співпадуть із domainOrders[0] та [1]
            result.Should().HaveCount(2);

            result[0].Id.Should().Be(1);
            result[0].UserId.Should().Be(7);
            result[0].BookId.Should().Be(10);
            result[0].BookCopyId.Should().Be(100);
            result[0].OrderType.Should().Be(BookType.Paper);
            result[0].OrderDate.Should().Be(new DateTime(2023, 1, 1));

            result[1].Id.Should().Be(2);
            result[1].UserId.Should().Be(7);
            result[1].BookId.Should().Be(11);
            result[1].BookCopyId.Should().Be(101);
            result[1].OrderType.Should().Be(BookType.Electronic);
            result[1].OrderDate.Should().Be(new DateTime(2023, 2, 2));
        }

        
        // ---------------------------------------------------
        // Тест #4: Якщо Book не знайдено → KeyNotFoundException
        // ---------------------------------------------------
        [Test]
        public void CreateAsync_BookNotFound_ThrowsKeyNotFoundException()
        {
            // ARRANGE: в InMemory не додаємо жодного Book
            // Books.ReadAll() повертає порожній DbSet<Book>
            _uowMock.Books.ReadAll().Returns(_inMemoryContext.Books.AsQueryable());

            var dto = new ActionOrderDto
            {
                UserId = 5,
                BookId = 999,
                OrderType = BookType.Paper
            };

            // ACT & ASSERT
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.CreateAsync(dto),
                "Book with Id = 999 not found."
            );
        }


        // ---------------------------------------------------
        // Тест #5: Якщо немає вільних Paper-копій → InvalidOperationException
        // ---------------------------------------------------
        [Test]
        public void CreateAsync_PaperType_NoFreeCopies_ThrowsInvalidOperationException()
        {
            // ARRANGE: Додаємо Book в InMemory, але копія зайнята
            var bookEntity = new Book
            {
                Id = 400,
                Title = "test",
                Author = "test",
                Description = "test",
                Publisher = "test",

                AvailableCopies = 1,
                ListenCount = 0,
                DownloadCount = 0,
                Copies = new List<BookCopy>
                {
                    new BookCopy { Id = 600, IsAvailable = false }
                },
                AvailableTypes = new List<BookTypeEntity>()
            };
            _inMemoryContext.Books.Add(bookEntity);
            _inMemoryContext.SaveChanges();

            _uowMock.Books.ReadAll().Returns(_inMemoryContext.Books.AsQueryable());

            var dto = new ActionOrderDto
            {
                UserId = 6,
                BookId = 400,
                OrderType = BookType.Paper
            };

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateAsync(dto),
                "No available paper copies to reserve."
            );
        }

    [Test]
        public async Task DeleteAsync_PaperOrder_ReleasesCopyAndUpdatesBookAndDeletesOrder()
        {
            // ARRANGE

            // 1) Додаємо Book і пов’язаний BookCopy в InMemory
            var bookEntity = new Book
            {
                Id = 10,
                Title = "t",
                Author = "a",
                Description = "d",
                Publisher = "p",
                AvailableCopies = 2,
                ListenCount = 0,
                DownloadCount = 0,
                Copies = new List<BookCopy>()
            };
            var copyEntity = new BookCopy
            {
                Id = 20,
                IsAvailable = false,
                Book = bookEntity
            };
            bookEntity.Copies.Add(copyEntity);
            _inMemoryContext.Books.Add(bookEntity);
            _inMemoryContext.BookCopies.Add(copyEntity);
            _inMemoryContext.SaveChanges();

            // 2) Додаємо Order, яке посилається на Book(10) і BookCopy(20), OrderType = Paper
            var orderEntity = new Order
            {
                Id = 100,
                UserId = 5,
                BookId = 10,
                Book = bookEntity,
                BookCopyId = 20,
                BookCopy = copyEntity,
                OrderTypeId = (int)BookType.Paper,
                OrderDate = DateTime.UtcNow
            };
            _inMemoryContext.Orders.Add(orderEntity);
            _inMemoryContext.SaveChanges();

            // 3) Підміняємо репозиторій ReadAllOrder() → “живий” DbSet<Order>
            _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders.AsQueryable());

            // ACT
            await _service.DeleteAsync(100);

            // ASSERT

            // • Копія 20 мала IsAvailable = false → після видалення мусить стати true
            copyEntity.IsAvailable.Should().BeTrue();

            // • AvailableCopies було 2 → після видалення має стати 3
            bookEntity.AvailableCopies.Should().Be(3);

            // • Видалення Order: перевіряємо виклик _uow.Orders.Delete(orderEntity)
            _uowMock.Orders.Received(1).Delete(Arg.Is<Order>(o => o.Id == 100));

            // • Оновлення BookCopy: перевіряємо виклик _uow.BookCopies.Update(copyEntity)
            _uowMock.BookCopies.Received(1).Update(Arg.Is<BookCopy>(c => c.Id == 20 && c.IsAvailable));

            // • Оновлення Book: перевіряємо виклик _uow.Books.Update(bookEntity)
            _uowMock.Books.Received(1).Update(Arg.Is<Book>(b => b.Id == 10 && b.AvailableCopies == 3));

            // • CommitAsync мав бути викликаний
            await _uowMock.Received(1).CommitAsync();
        }

        // ---------------------------------------------------
        // Тест #2: DeleteAsync для аудіокниги — просто видаляємо Order
        // ---------------------------------------------------
        [Test]
        public async Task DeleteAsync_AudioOrder_DeletesOrderOnly()
        {
            // ARRANGE

            // 1) Додаємо Book (з обов’язковими полями) без копій
            var bookEntity = new Book
            {
                Id = 30,
                Title = "t",
                Author = "a",
                Description = "d",
                Publisher = "p",
                AvailableCopies = 0,
                ListenCount = 1,
                DownloadCount = 0,
                Copies = new List<BookCopy>()
            };
            _inMemoryContext.Books.Add(bookEntity);
            _inMemoryContext.SaveChanges();

            // 2) Додаємо Order типу Audio (OrderTypeId = Audio), без BookCopy
            var orderEntity = new Order
            {
                Id = 200,
                UserId = 7,
                BookId = 30,
                Book = bookEntity,
                BookCopyId = null,
                BookCopy = null,
                OrderTypeId = (int)BookType.Audio,
                OrderDate = DateTime.UtcNow
            };
            _inMemoryContext.Orders.Add(orderEntity);
            _inMemoryContext.SaveChanges();

            // 3) Підміняємо ReadAllOrder() → DbSet<Order>
            _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders.AsQueryable());

            // ACT
            await _service.DeleteAsync(200);

            // ASSERT

            // • Це Audio, тому не повинно зміни AvailableCopies чи BookCopy
            _uowMock.BookCopies.DidNotReceive().Update(Arg.Any<BookCopy>());
            _uowMock.Books.DidNotReceive().Update(Arg.Any<Book>());

            // • Видалення Order: мав виклик _uow.Orders.Delete
            _uowMock.Orders.Received(1).Delete(Arg.Is<Order>(o => o.Id == 200));

            // • CommitAsync мав бути викликаний
            await _uowMock.Received(1).CommitAsync();
        }

        // ---------------------------------------------------
        // Тест #3: DeleteAsync для електронної книги — просто видаляємо Order
        // ---------------------------------------------------
        [Test]
        public async Task DeleteAsync_ElectronicOrder_DeletesOrderOnly()
        {
            // ARRANGE

            // 1) Додаємо Book (з обов’язковими полями)
            var bookEntity = new Book
            {
                Id = 40,
                Title = "t",
                Author = "a",
                Description = "d",
                Publisher = "p",
                AvailableCopies = 0,
                ListenCount = 0,
                DownloadCount = 1,
                Copies = new List<BookCopy>()
            };
            _inMemoryContext.Books.Add(bookEntity);
            _inMemoryContext.SaveChanges();

            // 2) Додаємо Order типу Electronic
            var orderEntity = new Order
            {
                Id = 300,
                UserId = 9,
                BookId = 40,
                Book = bookEntity,
                BookCopyId = null,
                BookCopy = null,
                OrderTypeId = (int)BookType.Electronic,
                OrderDate = DateTime.UtcNow
            };
            _inMemoryContext.Orders.Add(orderEntity);
            _inMemoryContext.SaveChanges();

            // 3) Підміняємо ReadAllOrder() → DbSet<Order>
            _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders.AsQueryable());

            // ACT
            await _service.DeleteAsync(300);

            // ASSERT

            // • Для Electronic теж не оновлюємо BookCopy або Book
            _uowMock.BookCopies.DidNotReceive().Update(Arg.Any<BookCopy>());
            _uowMock.Books.DidNotReceive().Update(Arg.Any<Book>());

            // • Delete(orderEntity) мав виклик
            _uowMock.Orders.Received(1).Delete(Arg.Is<Order>(o => o.Id == 300));

            // • CommitAsync мав виклик
            await _uowMock.Received(1).CommitAsync();
        }

        // ---------------------------------------------------
        // Тест #4: Якщо Order не знайдено → KeyNotFoundException
        // ---------------------------------------------------
        [Test]
        public void DeleteAsync_OrderNotFound_ThrowsKeyNotFoundException()
        {
            // ARRANGE: InMemory пустий, підміняємо ReadAllOrder()
            _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders.AsQueryable());

            // ACT & ASSERT
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteAsync(999),
                "Order with Id = 999 not found."
            );
        }

        // ---------------------------------------------------
        // Тест #6: Якщо Order.OrderType == Paper, але BookCopy == null → InvalidOperationException
        // ---------------------------------------------------
        [Test]
        public void DeleteAsync_PaperOrderHasNullCopy_ThrowsInvalidOperationException()
        {
            // ARRANGE

            // 1) Додаємо пов’язану книгу
            var bookEntity = new Book
            {
                Id = 60,
                Title = "t",
                Author = "a",
                Description = "d",
                Publisher = "p",
                AvailableCopies = 0,
                ListenCount = 0,
                DownloadCount = 0,
                Copies = new List<BookCopy>()
            };
            _inMemoryContext.Books.Add(bookEntity);
            _inMemoryContext.SaveChanges();

            // 2) Додаємо Order типу Paper, але не прив’язуємо BookCopy (BookCopyId заданий, але BookCopy = null)
            var orderEntity = new Order
            {
                Id = 500,
                UserId = 3,
                BookId = 60,
                Book = bookEntity,
                BookCopyId = 70,   // такого BookCopy в InMemory НЕ існує
                BookCopy = null,
                OrderTypeId = (int)BookType.Paper,
                OrderDate = DateTime.UtcNow
            };
            _inMemoryContext.Orders.Add(orderEntity);
            _inMemoryContext.SaveChanges();

            _uowMock.Orders.ReadAllOrder().Returns(_inMemoryContext.Orders.AsQueryable());

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.DeleteAsync(500),
                "Associated BookCopy not found."
            );
        }
    }

    // -------------------------------------------
    // Допоміжний код для асинхронного IQueryable
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
            internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

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

            // Для FirstOrDefaultAsync/ToListAsync без CancellationToken
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return new TestAsyncEnumerable<TResult>(stripped);
            }

            // Для FirstOrDefaultAsync з CancellationToken (повертає TResult, як вимагає ваша версія EF Core)
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var stripped = StripEfMethods(expression);
                return _inner.Execute<TResult>(stripped);
            }

            private static Expression StripEfMethods(Expression expression)
            {
                if (expression is MethodCallExpression mce)
                {
                    // 1) Якщо Include(...), повертаємо джерело без Include
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) &&
                        mce.Method.Name.StartsWith("Include", StringComparison.Ordinal))
                    {
                        return StripEfMethods(mce.Arguments[0]);
                    }

                    // 2) Якщо FirstOrDefaultAsync(source, predicate), трансформуємо в Queryable.FirstOrDefault(source.Where(predicate))
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) &&
                        mce.Method.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync))
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

                    // 3) Інші методи рекурсивно обробляємо аргументи
                    var newArgs = mce.Arguments.Select(arg => StripEfMethods(arg)).ToArray();
                    var newObj = mce.Object != null ? StripEfMethods(mce.Object) : null;
                    return mce.Update(newObj, newArgs);
                }

                return expression;
            }
        }

    // ---------------------------------------------
    // Допоміжний InMemory‐контекст для тестів
    // ---------------------------------------------
    public class TestAppDbContext : DbContext
    {
        public TestAppDbContext(DbContextOptions<TestAppDbContext> options)
            : base(options)
        { }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<BookCopy> BookCopies { get; set; } = null!;
    // Якщо потрібно, додайте інші DbSet, але для цих тестів достатньо двох
        public DbSet<Order> Orders { get; set; } = null!;
}
}

