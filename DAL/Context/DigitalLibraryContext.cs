using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace DAL.Context
{
    public class DigitalLibraryContext : DbContext
    {
        public DigitalLibraryContext(DbContextOptions<DigitalLibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primary keys: int IDENTITY
            modelBuilder.Entity<Book>(b =>
            {
                modelBuilder.Entity<Book>()
                    .HasMany(b => b.AvailableTypes)
                    .WithMany(t => t.Books)
                    .UsingEntity<Dictionary<string, object>>(
                    "BookBookType",
                    j => j.HasOne<BookTypeEntity>().WithMany().HasForeignKey("BookTypeId"),
                    j => j.HasOne<Book>().WithMany().HasForeignKey("BookId"),
                    j => j.HasKey("BookId", "BookTypeId")
                    );
                b.HasKey(x => x.Id);
                b.Property(x => x.Id)
                    .ValueGeneratedOnAdd()  // авто-інкремент
                    .UseIdentityColumn();   // SQL Server Identity

                // Book -> BookCopies — Cascade OK
                b.HasMany(x => x.Copies)
                 .WithOne(c => c.Book)
                 .HasForeignKey(c => c.BookId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Book -> Genre — Restrict
                b.HasOne(x => x.Genre)
                 .WithMany(g => g.Books)
                 .HasForeignKey(x => x.GenreId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BookCopy>(c =>
            {
                c.HasKey(x => x.Id);
                c.Property(x => x.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
            });

            modelBuilder.Entity<Genre>(g =>
            {
                g.HasKey(x => x.Id);
                g.Property(x => x.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
            });

            modelBuilder.Entity<User>(u =>
            {
                u.HasKey(x => x.Id);
                u.Property(x => x.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

                // User -> Orders — Cascade
                u.HasMany(x => x.Orders)
                 .WithOne(o => o.User)
                 .HasForeignKey(o => o.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(o =>
            {
                o.HasKey(x => x.Id);
                o.Property(x => x.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

                // Order -> Book — Restrict
                o.HasOne(x => x.Book)
                 .WithMany()
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Order -> BookCopy — SetNull
                o.HasOne(x => x.BookCopy)
                 .WithMany()
                 .HasForeignKey(x => x.BookCopyId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
