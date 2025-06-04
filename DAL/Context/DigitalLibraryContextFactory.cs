using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DAL.Context
{
    public class DigitalLibraryContextFactory : IDesignTimeDbContextFactory<DigitalLibraryContext>
    {
        public DigitalLibraryContext CreateDbContext(string[] args)
        {
            // Шлях до папки проекту API (де буде App_Data)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../API"));
            var appDataPath = Path.Combine(basePath, "App_Data");

            // Створити папку, якщо не існує
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            // Формуємо рядок підключення
            var connectionString = $"Server=(localdb)\\MSSQLLocalDB;AttachDbFilename={Path.Combine(appDataPath, "DigitalLibraryDB.mdf")};Database=DigitalLibraryDB;Trusted_Connection=True";
            var optionsBuilder = new DbContextOptionsBuilder<DigitalLibraryContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DigitalLibraryContext(optionsBuilder.Options);
        }
    }
}
