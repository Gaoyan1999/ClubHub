using System.IO;
using System.Text.Json;
using System.Windows;
using ClubHub.Core.Services;
using ClubHub.Data;
using ClubHub.ML;
using ClubHub.Wpf.Services;
using ClubHub.Wpf.ViewModels;

namespace ClubHub.Wpf;

public partial class App : Application
{
    private ClubHubDbContext? _context;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _context = ClubHubDbContext.ForPostgres(LoadConnectionString());
            _context.Database.EnsureCreated();
            DbSeeder.Seed(_context);

            var window = new MainWindow { DataContext = new MainViewModel(_context, new DialogService(), new PlaceholderAttendancePredictor(),
                new OpenMeteoWeatherService()) };
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ClubHub could not start:\n\n{ex.Message}", "Startup error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>Reads ConnectionStrings:ClubHub from appsettings.json next to the .exe (kept out of Git).</summary>
    private static string LoadConnectionString()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            throw new FileNotFoundException(
                "appsettings.json is missing. Copy appsettings.example.json to appsettings.json and add the database connection string.");

        using var json = JsonDocument.Parse(File.ReadAllText(path));
        return json.RootElement.GetProperty("ConnectionStrings").GetProperty("ClubHub").GetString()
               ?? throw new InvalidOperationException("ConnectionStrings:ClubHub is empty in appsettings.json.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _context?.Dispose();
        base.OnExit(e);
    }
}
