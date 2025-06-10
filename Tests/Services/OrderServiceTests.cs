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
            // Налаштовуємо AutoFixture з AutoNSubstituteCustomization
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            _uowMock = Substitute.For<IUnitOfWork>();
            // Мок IUnitOfWork і репозиторій Orders
            _uowMock = _fixture.Freeze<IUnitOfWork>();
            var ordersRepoMock = Substitute.For<IGenericRepository<Order>>();
            _uowMock.Orders.Returns(ordersRepoMock);

            _uowMock.BookCopies.Returns(Substitute.For<IGenericRepository<BookCopy>>());
            _uowMock.Books.Returns(Substitute.For<IGenericRepository<Book>>());


            // Мок IMapper: Map<List<OrderDto>>(List<Order>) -> List<OrderDto>
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

            //Створюємо сам сервіс
            _service = new OrderService(_uowMock, _mapperMock);

            // Налаштовуємо InMemory DbContext (роздільна база для кожного тесту)
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

            // Перетворюємо у асинхронний IQueryable
            var asyncOrders = new TestAsyncEnumerable<Order>(domainOrders);

            // Налаштовуємо репозиторій
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
        // Тест 2: порожня колекція -> повернення пустого списку
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

        // -------------------------------------------------------------------
        // Тест 3: ReadAllOrder() повертає null -> NullReferenceException
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

            // Фейковий IQueryable<Order> зі всіма замовленнями
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
        // Тест #4: Якщо Book не знайдено -> KeyNotFoundException
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
        // Тест #5: Якщо немає вільних Paper-копій -> InvalidOperationException
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
        public void DeleteAsync_NonExistingOrder_ThrowsKeyNotFoundException()
        {
            // Arrange: In-Memory Orders порожні
            StubReadAllOrderToReturnInMemoryOrders();
            // (Нічого не додаємо в _inMemoryContext.Orders)

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async ()
                => await _service.DeleteAsync(orderId: 123, isAdmin: false));

            ex.Message.Should().Be("Order with Id = 123 not found.");
            // CommitAsync не мав викликатися, оскільки вже викинуто помилку
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task DeleteAsync_StatusAwaiting_NonAdmin_DeletesOrder()
        {
            // Arrange:
            // Створюємо Book зі всіма обов’язковими властивостями
            var book = new Book
            {
                Id = 5,
                Title = "dummy",
                Author = "dummy",
                Publisher = "dummy",
                Description = "dummy",
                AvailableCopies = 0
            };
            _inMemoryContext.Books.Add(book);

            // Створюємо Order у статусі Awaiting (без паперової копії)
            var order = new Order
            {
                Id = 1,
                UserId = 10,
                OrderTypeId = (int)BookType.Paper,
                BookCopyId = null,
                Status = OrderStatus.Awaiting,
                Book = book,
                BookCopy = null
            };
            _inMemoryContext.Orders.Add(order);
            _inMemoryContext.SaveChanges();

            // Підміна ReadAllOrder() → In-Memory Orders
            StubReadAllOrderToReturnInMemoryOrders();

            // Act:
            await _service.DeleteAsync(orderId: 1, isAdmin: false);

            // Assert:
            _uowMock.Orders.Received(1).Delete(order);
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void DeleteAsync_StatusWithUser_NonAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange:
            // Створюємо Book зі всіма обов’язковими властивостями
            var book = new Book
            {
                Id = 7,
                Title = "dummy",
                Author = "dummy",
                Publisher = "dummy",
                Description = "dummy",
                AvailableCopies = 0
            };
            _inMemoryContext.Books.Add(book);

            // Створюємо Order у статусі WithUser (без паперової копії)
            var order = new Order
            {
                Id = 2,
                UserId = 20,
                OrderTypeId = (int)BookType.Audio,
                BookCopyId = null,
                Status = OrderStatus.WithUser,
                Book = book,
                BookCopy = null
            };
            _inMemoryContext.Orders.Add(order);
            _inMemoryContext.SaveChanges();

            // Підміна ReadAllOrder() → In-Memory Orders
            StubReadAllOrderToReturnInMemoryOrders();

            // Act & Assert:
            Assert.ThrowsAsync<UnauthorizedAccessException>(async ()
                => await _service.DeleteAsync(orderId: 2, isAdmin: false));

            // CommitAsync не мав викликатися:
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task DeleteAsync_StatusWithUser_AdminDeletesAndFreesCopy()
        {
            // Arrange:
            // Створюємо Book та BookCopy зі всіма обов’язковими параметрами
            var book = new Book
            {
                Id = 30,
                Title = "dummy",
                Author = "dummy",
                Publisher = "dummy",
                Description = "dummy",
                AvailableCopies = 0
            };
            _inMemoryContext.Books.Add(book);
            var copy = new BookCopy
            {
                Id = 99,
                BookId = 30,
                IsAvailable = false
            };
            _inMemoryContext.BookCopies.Add(copy);

            // Створюємо Order у статусі WithUser із паперовою копією
            var order = new Order
            {
                Id = 3,
                UserId = 30,
                OrderTypeId = (int)BookType.Paper,
                BookCopyId = 99,
                Status = OrderStatus.WithUser,
                Book = book,
                BookCopy = copy
            };
            _inMemoryContext.Orders.Add(order);
            _inMemoryContext.SaveChanges();

            // Підміна ReadAllOrder() → In-Memory Orders
            StubReadAllOrderToReturnInMemoryOrders();

            // Act:
            await _service.DeleteAsync(orderId: 3, isAdmin: true);

            // Assert:
            // Копія стала вільною
            copy.IsAvailable.Should().BeTrue();
            // AvailableCopies книги інкрементовано
            book.AvailableCopies.Should().Be(1);

            _uowMock.BookCopies.Received(1).Update(copy);
            _uowMock.Books.Received(1).Update(book);
            _uowMock.Orders.Received(1).Delete(order);
            await _uowMock.Received(1).CommitAsync();
        }

        // ===========================================================
        // TESTS for UpdateStatusAsync(int orderId)
        // ===========================================================

        [Test]
        public void UpdateStatusAsync_NonExistingOrder_ThrowsKeyNotFoundException()
        {
            // Arrange: жодного замовлення в In-Memory
            StubReadAllOrderToReturnInMemoryOrders();

            // Act & Assert:
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async ()
                => await _service.UpdateStatusAsync(orderId: 50));

            ex.Message.Should().Be("Order with Id = 50 not found.");
            // Жодного оновлення й коміту не мало бути
            _uowMock.Orders.DidNotReceive().Update(Arg.Any<Order>());
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public void UpdateStatusAsync_StatusNotAwaiting_ThrowsInvalidOperationException()
        {
            // Arrange: додаємо Order зі статусом NoPaper
            var order = new Order
            {
                Id = 4,
                Status = OrderStatus.NoPaper
            };
            _inMemoryContext.Orders.Add(order);
            _inMemoryContext.SaveChanges();
            StubReadAllOrderToReturnInMemoryOrders();

            // Act & Assert:
            Assert.ThrowsAsync<InvalidOperationException>(async ()
                => await _service.UpdateStatusAsync(orderId: 4));

            _uowMock.Orders.DidNotReceive().Update(Arg.Any<Order>());
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task UpdateStatusAsync_OrderIsAwaiting_ChangesToWithUserAndCommits()
        {
            // Arrange: додаємо Order зі статусом Awaiting
            var order = new Order
            {
                Id = 5,
                Status = OrderStatus.Awaiting
            };
            _inMemoryContext.Orders.Add(order);
            _inMemoryContext.SaveChanges();
            StubReadAllOrderToReturnInMemoryOrders();

            // Act:
            await _service.UpdateStatusAsync(orderId: 5);

            // Assert:
            order.Status.Should().Be(OrderStatus.WithUser);
            _uowMock.Orders.Received(1).Update(order);
            await _uowMock.Received(1).CommitAsync();
        }

        // ===========================================================
        // HELPERS: Кілька приватних методів, щоб не дублювати код
        // ===========================================================

        // Підставляє _uowMock.Orders.ReadAllOrder() -> In-Memory DbSet<Order>
        private void StubReadAllOrderToReturnInMemoryOrders()
        {
            // Нам потрібно, щоб GenericRepository<Order>.ReadAllOrder() повертав IQueryable<Order>,
            // яке під капотом — In-Memory DbSet<Order>.
            // Використаємо NSubstitute: .Returns(ci => _inMemoryContext.Orders)
            _uowMock.Orders
                .ReadAllOrder()
                .Returns(_inMemoryContext.Orders);
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
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
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
                // Drop Include(...)
                if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                    && mce.Method.Name.StartsWith("Include", StringComparison.Ordinal))
                {
                    return StripEfMethods(mce.Arguments[0]);
                }

                // Transform FirstOrDefaultAsync(...) -> Queryable.FirstOrDefault(...)
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

                // Інші студентські виклики – лише рекурсивно обробляємо аргументи
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
        public DbSet<Order> Orders { get; set; } = null!;
    }
}

