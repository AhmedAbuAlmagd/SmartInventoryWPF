using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Application.Services;
using POSDesktopSystem.Domain.Interfaces;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure;
using POSDesktopSystem.Infrastructure.Data;
using POSDesktopSystem.Infrastructure.Logging;
using POSDesktopSystem.Infrastructure.Printing;
using POSDesktopSystem.Infrastructure.Repositories;
using POSDesktopSystem.Presentation.Infrastructure;
using POSDesktopSystem.Presentation.ViewModels.Auth;
using POSDesktopSystem.Presentation.ViewModels.Invoices;
using POSDesktopSystem.Presentation.ViewModels.POS;
using POSDesktopSystem.Presentation.ViewModels.Products;
using POSDesktopSystem.Presentation.ViewModels.Shell;
using POSDesktopSystem.Presentation.Views.Auth;
using POSDesktopSystem.Presentation.Views.Invoices;
using POSDesktopSystem.Presentation.Views.Products;
using POSDesktopSystem.Presentation.Views.Shell;
using System;
using System.Windows;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;
using System.IO;

namespace POSDesktopSystem.Presentation;

public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; } = default!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "POSDesktopSystem", "logs");
        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.File(Path.Combine(logDir, "pos-log-.txt"), 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application starting...");

        // Global Exception Handling
        AppDomain.CurrentDomain.UnhandledException += (s, args) => 
            LogUnhandledException((Exception)args.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (s, args) => {
            LogUnhandledException(args.Exception, "DispatcherUnhandledException");
            args.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, args) => {
            LogUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException");
            args.SetObserved();
        };

        var services = new ServiceCollection();
        // ... rest of the method

        // Infrastructure
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(GetConnectionString()));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddSingleton<ISessionService, SessionService>();

        // Presentation Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IReceiptService, ReceiptPrinter>();

        // Logging
        services.AddLogging(builder => 
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ProductFormViewModel>();
        services.AddTransient<PosViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();

        // Views
        services.AddTransient<InvoiceDetailView>();
        services.AddTransient<ProductFormView>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var loginWindow = new LoginWindow(Services.GetRequiredService<LoginViewModel>());
        loginWindow.Show();
    }

    private static string GetConnectionString() =>
        "Server=.;Database=POSDesktopDb;Trusted_Connection=True;TrustServerCertificate=True;";

    private void LogUnhandledException(Exception ex, string source)
    {
        var message = $"Unhandled exception in {source}: {ex.Message}";
        
        Log.Fatal(ex, "Unhandled exception in {Source}", source);

        MessageBox.Show($"{message}\n\nCheck logs for details.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
