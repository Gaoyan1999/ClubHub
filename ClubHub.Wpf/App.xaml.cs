using System.Windows;
using ClubHub.Data;
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
            _context = new ClubHubDbContext(ClubHubDbContext.DefaultConnectionString());
            _context.Database.EnsureCreated();
            DbSeeder.Seed(_context);

            var window = new MainWindow { DataContext = new MainViewModel(_context, new DialogService()) };
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ClubHub could not start:\n\n{ex.Message}", "Startup error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _context?.Dispose();
        base.OnExit(e);
    }
}
