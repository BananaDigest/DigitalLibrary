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

            // Book → BookCopies — Cascade OK
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Copies)
                .WithOne(c => c.Book)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Book → Orders — Заборонити каскадне видалення
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Book)
                .WithMany()
                .HasForeignKey(o => o.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → BookCopy — залишаємо SetNull, як було
            modelBuilder.Entity<Order>()
                .HasOne(o => o.BookCopy)
                .WithMany()
                .HasForeignKey(o => o.BookCopyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Book → Genre (як було)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → Orders (як було за замовчуванням Cascade)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders) // якщо є навігація в User
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Налаштування зв'язків та обмежень
        //    modelBuilder.Entity<Book>()
        //        .HasMany(b => b.Copies)
        //        .WithOne(c => c.Book)
        //        .HasForeignKey(c => c.BookId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<Book>()
        //        .HasOne(b => b.Genre)
        //        .WithMany(g => g.Books)
        //        .HasForeignKey(b => b.GenreId)
        //        .OnDelete(DeleteBehavior.Restrict);

        //    modelBuilder.Entity<Order>()
        //        .HasOne(o => o.BookCopy)
        //        .WithMany()
        //        .HasForeignKey(o => o.BookCopyId)
        //        .OnDelete(DeleteBehavior.SetNull);
        //}
    }
}
