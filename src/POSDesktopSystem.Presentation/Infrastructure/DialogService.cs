using POSDesktopSystem.Application.Interfaces;
using System.Windows;
using System;
using Microsoft.Extensions.DependencyInjection;
using POSDesktopSystem.Presentation.Views.Shared;

namespace POSDesktopSystem.Presentation.Infrastructure;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool Confirm(string title, string message)
    {
        var dialog = new ConfirmDialogView(title, message);
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        var result = dialog.ShowDialog();
        return result == true;
    }

    public void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public T? ShowDialog<T>(object viewModel) where T : class
    {
        var viewTypeName = viewModel.GetType().FullName!.Replace("ViewModel", "View").Replace(".ViewModels.", ".Views.");
        var tType = Type.GetType(viewTypeName + ", POSDesktopSystem.Presentation");
        if (tType != null)
        {
            // Instead of Activator.CreateInstance, use the ServiceProvider to resolve dependencies
            var win = (Window)_serviceProvider.GetRequiredService(tType);
            win.DataContext = viewModel;
            win.Owner = System.Windows.Application.Current.MainWindow;
            
            // Subscribe to CloseRequested event if it exists
            var eventInfo = viewModel.GetType().GetEvent("CloseRequested");
            if (eventInfo != null)
            {
                EventHandler handler = null!;
                handler = (s, e) => {
                    win.Close();
                    eventInfo.RemoveEventHandler(viewModel, handler);
                };
                eventInfo.AddEventHandler(viewModel, handler);
            }

            win.ShowDialog();
        }
        return null;
    }
}
