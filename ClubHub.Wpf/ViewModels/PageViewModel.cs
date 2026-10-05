using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubHub.Wpf.ViewModels;

/// <summary>Base class for every screen shown in the main window's content area.</summary>
public abstract class PageViewModel : ObservableObject
{
    public abstract string Title { get; }

    /// <summary>The club picked in the side menu. 0 until a club is picked.</summary>
    protected int ClubId { get; private set; }

    /// <summary>Called by the main window when the user picks a club.</summary>
    public void SetClub(int clubId)
    {
        ClubId = clubId;
        Reload();
    }

    /// <summary>Called each time the screen is opened, so it shows changes made on other screens.</summary>
    public void OnNavigatedTo()
    {
        if (ClubId != 0)
            Reload();
    }

    /// <summary>Loads the screen's data for <see cref="ClubId"/>.</summary>
    protected virtual void Reload() { }
}
