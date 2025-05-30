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

            var connectionString =
                "Server=(localdb)\\MSSQLLocalDB;Database=DigitalLibraryDB;Trusted_Connection=True;";

            var optionsBuilder = new DbContextOptionsBuilder<DigitalLibraryContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DigitalLibraryContext(optionsBuilder.Options);
        }
    }
}
