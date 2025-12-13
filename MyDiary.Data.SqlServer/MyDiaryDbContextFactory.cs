using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyDiary.Data.SqlServer;

public class MyDiaryDbContextFactory : IDesignTimeDbContextFactory<MyDiaryDbContext>
{
    public MyDiaryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.database.json")
            .Build();

        return CreateDbContext(configuration);
    }

    public MyDiaryDbContext CreateDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<MyDiaryDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MyDiaryDbContext(optionsBuilder.Options);
    }
}