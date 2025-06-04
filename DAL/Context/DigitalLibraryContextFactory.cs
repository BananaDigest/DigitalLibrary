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
            // припустимо: .../DigitalLibrary/DAL    (бо цей клас у DAL)
            var projectRoot = Path.GetFullPath(Path.Combine(solutionFolder, "../API"));
            var appDataFolder = Path.Combine(projectRoot, "App_Data");

            // 2) Якщо папки App_Data ще немає (скажімо, перший запуск), створюємо
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            // 3) Файл бази тепер лежить у API/App_Data/DigitalLibraryDB.mdf
            var mdfPath = Path.Combine(appDataFolder, "DigitalLibraryDB.mdf");

            // 4) Формуємо рядок підключення з AttachDbFilename 
            //    (сьогодні при LocalDB досить Trusted_Connection=True).
            //    Обов’язково додаємо «Initial Catalog=DigitalLibraryDB», щоб БД мала правильне ім’я.
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
