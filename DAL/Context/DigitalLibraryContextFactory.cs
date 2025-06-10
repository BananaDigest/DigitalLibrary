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
            var solutionFolder = Directory.GetCurrentDirectory();
            var projectRoot = Path.GetFullPath(Path.Combine(solutionFolder, "../API"));
            var appDataFolder = Path.Combine(projectRoot, "App_Data");

            // Якщо папки App_Data ще немає створюємо
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            //Файл бази лежить у API/App_Data/DigitalLibraryDB.mdf
            var mdfPath = Path.Combine(appDataFolder, "DigitalLibraryDB.mdf");

            // Формуємо рядок підключення з AttachDbFilename 
            var connectionString =
                $"Server=(localdb)\\MSSQLLocalDB;" +
                $"AttachDbFilename={mdfPath};" +
                $"Initial Catalog=DigitalLibraryDB;" +
                $"Trusted_Connection=True;" +
                $"MultipleActiveResultSets=true;";

            var optionsBuilder = new DbContextOptionsBuilder<DigitalLibraryContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DigitalLibraryContext(optionsBuilder.Options);
        }
    }
}
