namespace POSDesktopSystem.Application.Interfaces;

public interface IDialogService
{
    bool Confirm(string title, string message);
    void ShowError(string title, string message);
    void ShowInfo(string title, string message);
    T? ShowDialog<T>(object viewModel) where T : class;
}
