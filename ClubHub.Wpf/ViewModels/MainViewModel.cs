using System.Collections.ObjectModel;
using ClubHub.Core.Models;
using ClubHub.Data;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubHub.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<PageViewModel> Pages { get; }

    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainViewModel(ClubHubDbContext context, IDialogService dialogs)
    {
        // TODO (F1): replace with the club picked in the side menu dropdown
        var clubId = context.Clubs.Select(c => c.Id).First();

        Pages = new ObservableCollection<PageViewModel>
        {
            new MembersViewModel(new Repository<Member>(context), dialogs, clubId),
            new EventsViewModel(),
            new EventDetailViewModel(),
            new BudgetViewModel(),
            new DashboardViewModel()
        };
        _currentPage = Pages[0];
    }
}
