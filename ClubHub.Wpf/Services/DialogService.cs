using System.Windows;
using ClubHub.Wpf.ViewModels;
using ClubHub.Wpf.Views;
using Microsoft.Win32;

namespace ClubHub.Wpf.Services;

public class DialogService : IDialogService
{
    public bool ShowMemberDialog(MemberDialogViewModel viewModel)
    {
        var dialog = new MemberDialog(viewModel) { Owner = Application.Current.MainWindow };
        return dialog.ShowDialog() == true;
    }

    public bool Confirm(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void ShowInfo(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message)
        => MessageBox.Show(message, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);

    public string? AskSaveFilePath(string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog { FileName = defaultFileName, Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
