using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubHub.Wpf.ViewModels;

/// <summary>Base class for every screen shown in the main window's content area.</summary>
public abstract class PageViewModel : ObservableObject
{
    public abstract string Title { get; }
}
