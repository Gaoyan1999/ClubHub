using System.Collections.ObjectModel;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using ClubHub.Data;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubHub.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<Club> Clubs { get; }

    public ObservableCollection<PageViewModel> Pages { get; }

    [ObservableProperty]
    private Club? _selectedClub;

    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainViewModel(ClubHubDbContext context, IDialogService dialogs, IAttendancePredictor predictor,
        IWeatherService weather)
    {
        var clashChecker = new ClashChecker();
        var events = new Repository<Event>(context);
        var members = new Repository<Member>(context);
        var rsvps = new Repository<Rsvp>(context);

        var eventDetail = new EventDetailViewModel(events, members, rsvps, new WaitlistService(), clashChecker, predictor, weather, dialogs);

        // "Open details" on the Events screen jumps to the Event Detail screen for that event
        void OpenEventDetail(Event evt)
        {
            eventDetail.ShowEvent(evt.Id);
            CurrentPage = eventDetail;
        }

        Pages = new ObservableCollection<PageViewModel>
        {
            new MembersViewModel(members, dialogs),
            new EventsViewModel(events, new Repository<Room>(context), clashChecker, dialogs, OpenEventDetail),
            eventDetail,
            new BudgetViewModel(events, new Repository<BudgetEntry>(context), rsvps, dialogs),
            new DashboardViewModel(events, members, dialogs)
        };
        _currentPage = Pages[0];

        Clubs = new ObservableCollection<Club>(new Repository<Club>(context).GetAll().OrderBy(c => c.Name));
        SelectedClub = Clubs.FirstOrDefault();
    }

    // Every screen shows data for the club picked in the side menu
    partial void OnSelectedClubChanged(Club? value)
    {
        if (value is null)
            return;

        foreach (var page in Pages)
            page.SetClub(value.Id);
    }

    partial void OnCurrentPageChanged(PageViewModel value) => value.OnNavigatedTo();
}
