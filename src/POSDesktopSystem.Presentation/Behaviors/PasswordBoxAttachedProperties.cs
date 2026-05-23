using System.Windows;
using System.Windows.Controls;

namespace POSDesktopSystem.Presentation.Behaviors;

public static class PasswordBoxAttachedProperties
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword", typeof(string), typeof(PasswordBoxAttachedProperties),
            new UIPropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox box)
        {
            box.PasswordChanged -= PasswordChanged;

            if (!_isUpdating)
            {
                var newPassword = (string)e.NewValue ?? string.Empty;
                if (box.Password != newPassword)
                {
                    box.Password = newPassword;
                }
            }

            box.PasswordChanged += PasswordChanged;
        }
    }

    private static bool _isUpdating;

    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
        {
            _isUpdating = true;
            SetBoundPassword(box, box.Password);
            _isUpdating = false;
        }
    }
}
