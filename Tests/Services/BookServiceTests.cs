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

        [SetUp]
        public void SetUp()
        {
            // 1) Mock IUnitOfWork
            _uowMock = Substitute.For<IUnitOfWork>();

            // 2) Mock Books repository
            var booksRepo = Substitute.For<IGenericRepository<Book>>();
            _uowMock.Books.Returns(booksRepo);

            // 3) Mock AutoMapper
            _mapperMock = Substitute.For<IMapper>();

            // 4) Create BookService with mocked UoW, Mapper, and a dummy IBookFactory
            _service = new BookService(
                _uowMock,
                _mapperMock,
                Substitute.For<IBookFactory>()
            );
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

            // Convert to an async IQueryable
            var asyncBooks = new TestAsyncEnumerable<Book>(domainBooks);

            // Configure ReadAll() to return this async IQueryable
            _uowMock.Books.ReadAll().Returns(asyncBooks);

            // Configure AutoMapper to map each Book to BookDto
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

        [TearDown]
        public void TearDown()
        {
            if (_uowMock is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // ----------------------------------------------------
        // Async helper implementations for IQueryable in tests
        // ----------------------------------------------------
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
                    // Drop Include(...) calls
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name.StartsWith("Include", StringComparison.Ordinal))
                    {
                        return StripEfMethods(mce.Arguments[0]);
                    }

                    // Replace ToListAsync(...) with Enumerable.ToList
                    if (mce.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                        && mce.Method.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync))
                    {
                        var sourceExpr = StripEfMethods(mce.Arguments[0]);
                        return Expression.Call(
                            typeof(Enumerable),
                            nameof(Enumerable.ToList),
                            new[] { typeof(TEntity) },
                            sourceExpr);
                    }

                    // Recurse on other calls
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
            public DbSet<User> Users { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
