using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Desktop.ViewModels;
using CareFlow.Desktop.Views;
using CareFlow.Infrastructure.Persistence;
using CareFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlow.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CareFlow");
            Directory.CreateDirectory(dataDirectory);
            string databasePath = Path.Combine(dataDirectory, "careflow.db");

            var services = new ServiceCollection();
            services.AddDbContextFactory<CareFlowDbContext>(options =>
                options.UseSqlite($"Data Source={databasePath};Default Timeout=30"));

            services.AddSingleton<IPatientRepository, EfPatientRepository>();
            services.AddSingleton<IDiagnosticServiceRepository, EfDiagnosticServiceRepository>();
            services.AddSingleton<IBookingRepository, EfBookingRepository>();
            services.AddSingleton<IAvailabilityService, AvailabilityApplicationService>();
            services.AddSingleton<BookingApplicationService>();
            services.AddSingleton<DatabaseInitializer>();
            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
            InitializeApplicationAsync(viewModel, desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void InitializeApplicationAsync(
        MainViewModel viewModel, IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            await _serviceProvider!.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            await viewModel.InitializeAsync();
        }
        catch (Exception)
        {
            viewModel.ShowStartupError(
                "CareFlow could not initialize its database. Check the database file permissions and restart the application.");
        }
    }
}
