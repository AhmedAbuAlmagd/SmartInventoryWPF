namespace POSDesktopSystem.Application.Interfaces;

public interface INavigationService
{
    void NavigateTo(string viewKey);
    void GoBack();
    string CurrentView { get; }
}
