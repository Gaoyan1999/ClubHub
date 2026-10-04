using System.Collections.ObjectModel;
using ClubHub.Core.Models;
using ClubHub.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubHub.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<PageViewModel> Pages { get; }

    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainViewModel(ClubHubDbContext context)
    {
        Pages = new ObservableCollection<PageViewModel>
        {
            new MembersViewModel(new Repository<Member>(context)),
            new EventsViewModel(),
            new EventDetailViewModel(),
            new BudgetViewModel(),
            new DashboardViewModel()
        };
        _currentPage = Pages[0];
    }
}
