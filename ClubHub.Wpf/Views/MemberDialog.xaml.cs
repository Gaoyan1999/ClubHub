using System.Windows;
using ClubHub.Wpf.ViewModels;

namespace ClubHub.Wpf.Views;

public partial class MemberDialog : Window
{
    public MemberDialog(MemberDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += result => DialogResult = result;
    }
}
