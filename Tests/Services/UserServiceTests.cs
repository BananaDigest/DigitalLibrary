using AutoMapper;
using BLL.DTOs;
using BLL.Services;
using DAL.Repositories;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using FluentAssertions;


namespace Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUnitOfWork _uowMock = null!;
        private IMapper _mapperMock = null!;
        private UserService _service = null!;
        private TestAppDbContext _inMemoryContext = null!;

        [SetUp]
        public void SetUp()
        {
            // Створюємо InMemory‐контекст для «Users»
            var options = new DbContextOptionsBuilder<TestAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _inMemoryContext = new TestAppDbContext(options);

            // Мокаємо IUnitOfWork
            _uowMock = Substitute.For<IUnitOfWork>();

            // Мокаємо репозиторій Users (IGenericRepository<User>)
            var usersRepo = Substitute.For<IGenericRepository<User>>();
            _uowMock.Users.Returns(usersRepo);

            // CreateAsync у Users повертає просто Task
            usersRepo.CreateAsync(Arg.Any<User>()).Returns(Task.CompletedTask);

            // CommitAsync повертає Task<int> (має бути Task.FromResult<int>)
            _uowMock.CommitAsync().Returns(Task.FromResult(0));

            // Мокаємо AutoMapper
            _mapperMock = Substitute.For<IMapper>();

            // Створюємо сервіс
            _service = new UserService(_uowMock, _mapperMock);
        }

        [TearDown]
        public void TearDown()
        {
            _inMemoryContext.Dispose();
            if (_uowMock is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public async Task RegisterAsync_ValidDto_CreatesUserWithRegisteredRoleAndCommits()
        {
            // ARRANGE
            var dto = new UserDto
            {
                Id = 0,
                Email = "test@example.com",
                Password = "Secure123!"
            };

            // Налаштовуємо AutoMapper: з UserDto -> User (setting Role = Guest для початку)
            _mapperMock
                .Map<User>(Arg.Is<UserDto>(d => d == dto))
                .Returns(call =>
                {
                    var inDto = call.Arg<UserDto>();
                    return new User
                    {
                        Email = inDto.Email,
                        Password = inDto.Password,
                        Role = UserRole.Guest 
                    };
                });

            // ACT
            await _service.RegisterAsync(dto);

            // ASSERT
            await _uowMock.Users.Received(1).CreateAsync(Arg.Is<User>(u =>
                u.Email == dto.Email &&
                u.Password == dto.Password &&
                u.Role == UserRole.Registered
            ));

            // CommitAsync має повертати Task<int>, перевіряємо виклик
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void RegisterAsync_WhenCommitThrowsDbUpdateException_PropagatesException()
        {
            // ARRANGE
            var dto = new UserDto
            {
                Id = 0,
                Email = "fail@example.com",
                Password = "BadPass"
            };

            _mapperMock
                .Map<User>(Arg.Is<UserDto>(d => d == dto))
                .Returns(call => new User
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    Role = UserRole.Guest
                });

            // CommitAsync має повертати Task.FromException<int>(...)
            _uowMock.CommitAsync().Returns(Task.FromException<int>(new DbUpdateException("DB error")));

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () => await _service.RegisterAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("DB error"));

            _uowMock.Users.Received(1).CreateAsync(Arg.Any<User>());
            _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public async Task ReadByIdAsync_ExistingUser_ReturnsMappedDto()
        {
            // ARRANGE
            var userId = 42;

            // 1) Створюємо доменну сутність User
            var userEntity = new User
            {
                Id = userId,
                Email = "user@example.com",
                Password = "Secret",
                Role = UserRole.Registered
            };

            // 2) Налаштовуємо ReadByIdAsync(id) повертати userEntity
            _uowMock.Users.ReadByIdAsync(userId).Returns(Task.FromResult(userEntity));

            // 3) Налаштовуємо AutoMapper: при Map<UserDto>(userEntity) повертаємо UserDto з Role як рядок
            var expectedDto = new UserDto
            {
                Id = userId,
                Email = "user@example.com",
                Password = "Secret",
                Role = UserRole.Registered.ToString()
            };
            _mapperMock
                .Map<UserDto>(Arg.Is<User>(u => u == userEntity))
                .Returns(expectedDto);

            // ACT
            var actual = await _service.ReadByIdAsync(userId);

            // ASSERT
            Assert.That(actual, Is.EqualTo(expectedDto));
            _uowMock.Users.Received(1).ReadByIdAsync(userId);
            _mapperMock.Received(1).Map<UserDto>(userEntity);
        }

        [Test]
        public void ReadByIdAsync_NonExistingUser_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var missingId = 99;
            _uowMock.Users.ReadByIdAsync(missingId).Returns(Task.FromResult<User?>(null));

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.ReadByIdAsync(missingId));
            Assert.That(ex.Message, Is.EqualTo($"User {missingId} not found"));

            _uowMock.Users.Received(1).ReadByIdAsync(missingId);
            _mapperMock.DidNotReceive().Map<UserDto>(Arg.Any<User>());
        }

        [Test]
        public async Task DeleteAsync_ExistingUser_DeletesAndCommits()
        {
            // ARRANGE
            var userId = 5;
            var userEntity = new User
            {
                Id = userId,
                Email = "a@b.com",
                Password = "pwd",
                Role = UserRole.Registered
            };

            // ReadByIdAsync повертає userEntity
            _uowMock.Users.ReadByIdAsync(userId).Returns(Task.FromResult(userEntity));

            // ACT
            await _service.DeleteAsync(userId);

            // ASSERT
            _uowMock.Users.Received(1).ReadByIdAsync(userId);
            _uowMock.Users.Received(1).Delete(Arg.Is<User>(u => u == userEntity));
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void DeleteAsync_NonExistingUser_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var missingId = 99;
            _uowMock.Users.ReadByIdAsync(missingId).Returns(Task.FromResult<User?>(null));

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteAsync(missingId));
            Assert.That(ex.Message, Is.EqualTo($"User {missingId} not found"));

            _uowMock.Users.Received(1).ReadByIdAsync(missingId);
            _uowMock.Users.DidNotReceive().Delete(Arg.Any<User>());
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsMappedDto()
        {
            // ARRANGE
            var email = "User@Example.com";
            var password = "Secret123";

            // Список користувачів, серед яких є той, що шукаємо
            var userEntity = new User
            {
                Id = 1,
                Email = "user@example.com",    // різний регістр, щоб перевірити IgnoreCase
                Password = password,
                Role = UserRole.Registered
            };
            var allUsers = new List<User> { userEntity };
            _uowMock.Users.ReadAllAsync().Returns(Task.FromResult<IEnumerable<User>>(allUsers));

            // Налаштовуємо AutoMapper: з User -> UserDto
            var expectedDto = new UserDto
            {
                Id = userEntity.Id,
                Email = userEntity.Email,
                Password = userEntity.Password,
                Role = userEntity.Role.ToString()
            };
            _mapperMock
                .Map<UserDto>(Arg.Is<User>(u => u == userEntity))
                .Returns(expectedDto);

            // ACT
            var actual = await _service.AuthenticateAsync(email, password);

            // ASSERT
            Assert.That(actual, Is.EqualTo(expectedDto));
            await _uowMock.Users.Received(1).ReadAllAsync();
            _mapperMock.Received(1).Map<UserDto>(userEntity);
        }

        [Test]
        public void AuthenticateAsync_InvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE
            var wrongEmail = "noone@nowhere.test";
            var password = "whatever";

            // ACT
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _service.AuthenticateAsync(wrongEmail, password));

            // ASSERT
            Assert.That(ex.Message, Is.EqualTo("Невірний email"));
        }

        [Test]
        public void AuthenticateAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE
            var email = "valid@domain.test";
            var wrongPassword = "oops";

            // ACT
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.AuthenticateAsync(email, wrongPassword));

            // ASSERT
            Assert.That(ex.Message, Is.EqualTo("Невірний email"));
        }

        [Test]
        public async Task UpdateUserAsync_ExistingUser_MapsFieldsAndCommits()
        {
            // ARRANGE
            var userId = 7;
            var originalUser = new User
            {
                Id = userId,
                Email = "old@example.com",
                Password = "oldpass",
                Role = UserRole.Registered,
                FirstName = "First",
                LastName = "Last"
            };

            // Додаємо користувача в InMemory‐контекст
            _inMemoryContext.Users.Add(originalUser);
            _inMemoryContext.SaveChanges();

            // Налаштовуємо репозиторій так, щоб ReadAllUser() повертав "живий" DbSet<User>
            _uowMock.Users.ReadAllUser().Returns(_inMemoryContext.Users);

            // DTO із новими значеннями
            var dto = new UserDto
            {
                Id = userId,
                Email = "new@example.com",
                Password = "newpass",
                Role = UserRole.Registered.ToString()
            };

            // AutoMapper.Map(dto, originalUser) просто переписує поля
            _mapperMock
                .When(m => m.Map(dto, originalUser))
                .Do(call =>
                {
                    originalUser.Email = dto.Email;
                    originalUser.Password = dto.Password;
                });

            // ACT
            await _service.UpdateUserAsync(dto);

            // ASSERT
            Assert.That(originalUser.Email, Is.EqualTo("new@example.com"));
            Assert.That(originalUser.Password, Is.EqualTo("newpass"));

            _uowMock.Users.Received(1).Update(originalUser);
            await _uowMock.Received(1).CommitAsync();
        }

        [Test]
        public void UpdateUserAsync_NonExistingUser_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var missingId = 99;
            // Не додаємо жодного користувача в InMemoryContext -> Users порожня
            // Налаштовуємо ReadAllUser() повернути саме DbSet<User> (порожній)
            _uowMock.Users.ReadAllUser().Returns(_inMemoryContext.Users);

            var dto = new UserDto
            {
                Id = missingId,
                Email = "nouser@example.com",
                Password = "nopass",
                Role = UserRole.Registered.ToString()
            };

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateUserAsync(dto));
            Assert.That(ex.Message, Is.EqualTo($"User with Id = {missingId} not found."));

            _uowMock.Users.DidNotReceive().Update(Arg.Any<User>());
            _uowMock.DidNotReceive().CommitAsync();
        }

        [Test]
        public async Task ReadAllUsersAsync_WithExistingUsers_ReturnsDtosWithPasswordNull()
        {
            // ARRANGE
            // Готуємо список доменних користувачів
            var domainUsers = new List<User>
            {
                new User
                {
                    Id = 1,
                    Email = "a@example.com",
                    Password = "pass1",
                    Role = UserRole.Registered,
                    FirstName = "Alice",
                    LastName = "Anderson"
                },
                new User
                {
                    Id = 2,
                    Email = "b@example.com",
                    Password = "pass2",
                    Role = UserRole.Manager,
                    FirstName = "Bob",
                    LastName = "Brown"
                }
            };

            // Налаштовуємо ReadAllAsync() репозиторію повернути цей список
            _uowMock.Users
                .ReadAllAsync()
                .Returns(Task.FromResult<IEnumerable<User>>(domainUsers));

            // Налаштовуємо AutoMapper так, щоб він перетворював User -> UserDto
            _mapperMock
                .Map<List<UserDto>>(Arg.Any<List<User>>())
                .Returns(call =>
                {
                    var sourceList = call.Arg<List<User>>();
                    return sourceList.Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Role = u.Role.ToString(),
                        Password = u.Password
                    }).ToList();
                });

            // ACT
            var result = (await _service.ReadAllUsersAsync()).ToList();

            // ASSERT
            result.Should().HaveCount(2);

            // Перевіряємо перший DTO
            result[0].Id.Should().Be(1);
            result[0].Email.Should().Be("a@example.com");
            result[0].FirstName.Should().Be("Alice");
            result[0].LastName.Should().Be("Anderson");
            result[0].Role.Should().Be(UserRole.Registered.ToString());
            result[0].Password.Should().BeNull("бо метод має обнулити пароль");

            // Перевіряємо другий DTO
            result[1].Id.Should().Be(2);
            result[1].Email.Should().Be("b@example.com");
            result[1].FirstName.Should().Be("Bob");
            result[1].LastName.Should().Be("Brown");
            result[1].Role.Should().Be(UserRole.Manager.ToString());
            result[1].Password.Should().BeNull("бо метод має обнулити пароль");
        }

        [Test]
        public async Task ReadAllUsersAsync_WhenNoUsers_ReturnsEmptyList()
        {
            // ARRANGE
            var emptyList = new List<User>();
            _uowMock.Users
                .ReadAllAsync()
                .Returns(Task.FromResult<IEnumerable<User>>(emptyList));

            // Навіть якщо AutoMapper було б налаштовано – сюди воно не потрапить,
            // бо список порожній
            _mapperMock
                .Map<List<UserDto>>(Arg.Any<List<User>>())
                .Returns(new List<UserDto>());

            // ACT
            var result = (await _service.ReadAllUsersAsync()).ToList();

            // ASSERT
            result.Should().BeEmpty("тому що в репозиторії немає користувачів");
        }


    // -------------------------------------------
    // Реалізація TestAsyncEnumerable та TestAsyncQueryProvider
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

            // Для FirstOrDefaultAsync/ToListAsync без CancellationToken
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                var stripped = StripEfMethods(expression);
                return new TestAsyncEnumerable<TResult>(stripped);
            }

            // Враховуємо overload FirstOrDefaultAsync(source, predicate, token)
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var stripped = StripEfMethods(expression);
                return _inner.Execute<TResult>(stripped);
            }

            private static Expression StripEfMethods(Expression expression)
            {
                if (expression is MethodCallExpression mce)
                {
                    // 1) If it's Include(...), drop it
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name.StartsWith("Include", StringComparison.Ordinal))
                    {
                        return StripEfMethods(mce.Arguments[0]);
                    }

                    // 2) If it's FirstOrDefaultAsync(...) with 2 or 3 arguments, transform to Queryable.FirstOrDefault
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync))
                    {
                        // source is always first argument
                        var sourceExpr = StripEfMethods(mce.Arguments[0]);

                        // predicate might be second argument
                        LambdaExpression? predicateExpr = null;
                        if (mce.Arguments.Count >= 2 && mce.Arguments[1] is LambdaExpression le)
                        {
                            predicateExpr = le;
                        }

                        // Build a Where(...).FirstOrDefault() call
                        Expression whereCall = sourceExpr!;
                        if (predicateExpr != null)
                        {
                            whereCall = Expression.Call(
                                typeof(Queryable),
                                nameof(Queryable.Where),
                                new[] { typeof(TEntity) },
                                sourceExpr,
                                predicateExpr);
                        }

                        var firstCall = Expression.Call(
                            typeof(Queryable),
                            nameof(Queryable.FirstOrDefault),
                            new[] { typeof(TEntity) },
                            whereCall);

                        return firstCall;
                    }

                    // Якщо ToListAsync(...), спростимо до Queryable.ToList
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync))
                    {
                        var sourceExpr = StripEfMethods(mce.Arguments[0]);
                        var toListCall = Expression.Call(
                            typeof(Enumerable),
                            nameof(Enumerable.ToList),
                            new[] { typeof(TEntity) },
                            sourceExpr);
                        return toListCall;
                    }

                    // Інші виклики – рекурсивно чистимо аргументи
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
            public DbSet<User> Users { get; set; } = null!; 

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
