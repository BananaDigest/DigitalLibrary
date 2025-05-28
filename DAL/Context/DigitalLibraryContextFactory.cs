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
            // Шлях до конфігурації
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json") // шукає в корені рішення
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DigitalLibraryContext>();

            // Підключення з appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new DigitalLibraryContext(optionsBuilder.Options);
        }
    }
}
