using Microsoft.Extensions.DependencyInjection;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.Infrastructure;
using POSDesktopSystem.Presentation.ViewModels.Auth;
using POSDesktopSystem.Presentation.ViewModels.Invoices;
using POSDesktopSystem.Presentation.ViewModels.POS;
using POSDesktopSystem.Presentation.ViewModels.Products;
using POSDesktopSystem.Presentation.ViewModels.Shell;
using POSDesktopSystem.Presentation.Views.Auth;
using POSDesktopSystem.Presentation.Views.Invoices;
using POSDesktopSystem.Presentation.Views.POS;
using POSDesktopSystem.Presentation.Views.Products;
using POSDesktopSystem.Presentation.Views.Shell;
using POSDesktopSystem.Presentation.ViewModels.Base;

namespace POSDesktopSystem.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<ProductFormViewModel>();
            services.AddTransient<PosViewModel>();
            services.AddTransient<InvoicesViewModel>();
            services.AddTransient<InvoiceDetailViewModel>();

            // Register Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<ProductsView>();
            services.AddTransient<ProductFormView>();
            services.AddTransient<POSView>();
            services.AddTransient<InvoicesView>();
            services.AddTransient<InvoiceDetailView>();

            // Register Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            return services;
        }
    }
}
