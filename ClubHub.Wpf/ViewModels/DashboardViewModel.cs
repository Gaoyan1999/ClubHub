using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ClubHub.Wpf.ViewModels;

public partial class DashboardViewModel : PageViewModel
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IDialogService _dialogs;

    // All events of the club; the date range filters these in memory without asking the database again
    private List<Event> _allEvents = new();

    public override string Title => "Dashboard";

    // Date range. The slider sets both dates to "N months before and after today"; the dates can also be picked by hand.
    [ObservableProperty] private double _monthsAround = 6;
    [ObservableProperty] private DateTime? _fromDate = DateTime.Today.AddMonths(-6);
    [ObservableProperty] private DateTime? _toDate = DateTime.Today.AddMonths(6);
    [ObservableProperty] private string _errorText = string.Empty;

    // Stat cards
    [ObservableProperty] private string _memberCountText = "–";
    [ObservableProperty] private string _upcomingText = "–";
    [ObservableProperty] private string _attendanceRateText = "–";
    [ObservableProperty] private string _noShowRateText = "–";
    [ObservableProperty] private string _rateNote = string.Empty;

    // Charts
    [ObservableProperty] private bool _hasEvents;
    [ObservableProperty] private ISeries[] _typeSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _moneySeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _monthAxes = { new Axis() };

    public Axis[] MoneyAxes { get; } = { new Axis { MinLimit = 0, Labeler = value => value.ToString("C0") } };

    public DashboardViewModel(IRepository<Event> eventRepository, IRepository<Member> memberRepository, IDialogService dialogs)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
        _dialogs = dialogs;
    }

    protected override void Reload()
    {
        try
        {
            _allEvents = _eventRepository.Find(e => e.ClubId == ClubId, e => e.Rsvps, e => e.BudgetEntries);
            MemberCountText = _memberRepository.Find(m => m.ClubId == ClubId).Count.ToString();
        }
        catch (Exception ex)
        {
            _allEvents = new List<Event>();
            MemberCountText = "–";
            _dialogs.ShowError($"Could not load the dashboard.\n\n{ex.Message}");
        }

        Refresh();
    }

    partial void OnMonthsAroundChanged(double value)
    {
        var months = (int)Math.Round(value);
        FromDate = DateTime.Today.AddMonths(-months);
        ToDate = DateTime.Today.AddMonths(months);
    }

    partial void OnFromDateChanged(DateTime? value) => Refresh();

    partial void OnToDateChanged(DateTime? value) => Refresh();

    private void Refresh()
    {
        if (FromDate is not { } from || ToDate is not { } to)
        {
            ErrorText = "Pick both a From and a To date.";
            return;
        }
        if (from > to)
        {
            ErrorText = "The From date must be on or before the To date.";
            return;
        }
        ErrorText = string.Empty;

        var now = DateTime.Now;
        var events = StatsService.InRange(_allEvents, from, to);

        UpcomingText = StatsService.CountUpcoming(events, now).ToString();
        var attendance = StatsService.AttendanceRate(events, now);
        AttendanceRateText = attendance is { } a ? a.ToString("P0") : "–";
        NoShowRateText = StatsService.NoShowRate(events, now) is { } n ? n.ToString("P0") : "–";
        RateNote = attendance is null ? "No finished events with RSVPs in this range yet." : string.Empty;

        HasEvents = events.Count > 0;

        TypeSeries = StatsService.CountByType(events)
            .Select(pair => (ISeries)new PieSeries<int> { Name = pair.Key.ToString(), Values = new[] { pair.Value } })
            .ToArray();

        var months = StatsService.TotalsByMonth(events);
        MonthAxes = new[] { new Axis { Labels = months.Select(m => m.Month.ToString("MMM yy")).ToArray() } };
        MoneySeries = new ISeries[]
        {
            new LineSeries<double> { Name = "Income", Values = months.Select(m => (double)m.Income).ToArray(), Fill = null },
            new LineSeries<double> { Name = "Cost", Values = months.Select(m => (double)m.Cost).ToArray(), Fill = null }
        };
    }
}
