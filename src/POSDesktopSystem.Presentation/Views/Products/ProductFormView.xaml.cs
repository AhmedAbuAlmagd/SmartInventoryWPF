using POSDesktopSystem.Presentation.ViewModels.Products;
using System.Windows;

namespace POSDesktopSystem.Presentation.Views.Products;

public partial class ProductFormView : Window
{
    public ProductFormView(ProductFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (s, e) => Close();
    }
}
