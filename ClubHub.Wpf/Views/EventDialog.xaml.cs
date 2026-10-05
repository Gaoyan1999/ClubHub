using System.Windows;
using ClubHub.Wpf.ViewModels;

namespace ClubHub.Wpf.Views;

public partial class EventDialog : Window
{
    public EventDialog(EventDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += result => DialogResult = result;
    }
}
