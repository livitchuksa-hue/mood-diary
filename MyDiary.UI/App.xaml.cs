using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyDiary.Data.SqlServer;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Data.SqlServer.Repositories;
using MyDiary.UI.Properties;
using MyDiary.UI.Themes;

namespace MyDiary.UI;

public partial class App : Application
{
    private MyDiaryDbContext? _dbContext;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var themeSetting = Settings.Default.Theme;
        var theme = string.Equals(themeSetting, "dark", System.StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Dark
            : AppTheme.Light;
        ThemeManager.ApplyTheme(theme);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.database.json", optional: false, reloadOnChange: false)
            .Build();

        var factory = new MyDiaryDbContextFactory();
        _dbContext = factory.CreateDbContext(configuration);

        //userRepo, entryRepo, activityRepo, subscriptionRepo
        _dbContext.Database.Migrate();
        IUserRepository userRepo = new SqlServerUserRepository(_dbContext);
        UiServices.UserRepository = userRepo;
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _dbContext?.Dispose();
        base.OnExit(e);
    }
}

