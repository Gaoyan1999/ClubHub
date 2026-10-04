using ClubHub.Wpf.ViewModels;

namespace ClubHub.Wpf.Services;

/// <summary>
/// Opens dialogs and message boxes for view models, so view models never create windows themselves.
/// </summary>
public interface IDialogService
{
    /// <summary>Shows the add/edit member dialog. Returns true when the user saved.</summary>
    bool ShowMemberDialog(MemberDialogViewModel viewModel);

    bool Confirm(string message, string title);

    void ShowInfo(string message, string title);

    void ShowError(string message);

    /// <summary>Asks where to save a file. Returns null when the user cancels.</summary>
    string? AskSaveFilePath(string defaultFileName, string filter);
}
