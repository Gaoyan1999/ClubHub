using System.Collections.ObjectModel;
using ClubHub.Core.Enums;
using ClubHub.Core.Extensions;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubHub.Wpf.ViewModels;

public partial class EventDetailViewModel : PageViewModel
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<Rsvp> _rsvpRepository;
    private readonly WaitlistService _waitlist;
    private readonly IClashChecker _clashChecker;
    private readonly IAttendancePredictor _predictor;
    private readonly IDialogService _dialogs;

    private List<Event> _allEvents = new();
    private List<Member> _clubMembers = new();
    private List<Rsvp> _eventRsvps = new();
    private int? _requestedEventId;
    private Rsvp? _lastPromoted;

    public override string Title => "Event Detail & Check-in";

    public ObservableCollection<Event> Events { get; } = new();
    public ObservableCollection<Rsvp> Going { get; } = new();
    public ObservableCollection<Rsvp> Waitlist { get; } = new();
    public ObservableCollection<Rsvp> Cancelled { get; } = new();
    public ObservableCollection<Rsvp> CheckInList { get; } = new();
    public ObservableCollection<Member> AvailableMembers { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEvent))]
    [NotifyCanExecuteChangedFor(nameof(ExportAttendanceCommand))]
    private Event? _selectedEvent;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private Member? _memberToRegister;

    [ObservableProperty] private string _checkInSearch = string.Empty;
    [ObservableProperty] private int _goingCount;
    [ObservableProperty] private int _capacity = 1;
    [ObservableProperty] private string _capacityText = string.Empty;
    [ObservableProperty] private string _checkInText = string.Empty;
    [ObservableProperty] private string _predictionText = string.Empty;

    public bool HasEvent => SelectedEvent is not null;

    public EventDetailViewModel(IRepository<Event> eventRepository, IRepository<Member> memberRepository,
        IRepository<Rsvp> rsvpRepository, WaitlistService waitlist, IClashChecker clashChecker,
        IAttendancePredictor predictor, IDialogService dialogs)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
        _rsvpRepository = rsvpRepository;
        _waitlist = waitlist;
        _clashChecker = clashChecker;
        _predictor = predictor;
        _dialogs = dialogs;

        // Remember who moved up; the message is shown after the change is saved
        _waitlist.MemberPromoted += (_, e) => _lastPromoted = e.Rsvp;
    }

    /// <summary>Opens this screen on a given event (used by the Events screen).</summary>
    public void ShowEvent(int eventId)
    {
        _requestedEventId = eventId;
        var match = Events.FirstOrDefault(e => e.Id == eventId);
        if (match is not null)
            SelectedEvent = match;
    }

    protected override void Reload()
    {
        var keepId = _requestedEventId ?? SelectedEvent?.Id;
        _requestedEventId = null;

        try
        {
            _allEvents = _eventRepository.Find(e => e.ClubId == ClubId, e => e.Room, e => e.Rsvps);
            _clubMembers = _memberRepository.Find(m => m.ClubId == ClubId);
        }
        catch (Exception ex)
        {
            _allEvents = new List<Event>();
            _clubMembers = new List<Member>();
            _dialogs.ShowError($"Could not load events.\n\n{ex.Message}");
        }

        Events.Clear();
        foreach (var evt in _allEvents.OrderBy(e => e.Start))
            Events.Add(evt);

        SelectedEvent = Events.FirstOrDefault(e => e.Id == keepId)
                        ?? Events.FirstOrDefault(e => e.Start >= DateTime.Today)
                        ?? Events.FirstOrDefault();
        RefreshRsvps();
    }

    partial void OnSelectedEventChanged(Event? value) => RefreshRsvps();

    partial void OnCheckInSearchChanged(string value) => FillCheckInList();

    private void RefreshRsvps()
    {
        Going.Clear();
        Waitlist.Clear();
        Cancelled.Clear();
        AvailableMembers.Clear();

        if (SelectedEvent is not { } evt)
        {
            _eventRsvps = new List<Rsvp>();
            FillCheckInList();
            CapacityText = CheckInText = PredictionText = string.Empty;
            return;
        }

        try
        {
            _eventRsvps = _rsvpRepository.Find(r => r.EventId == evt.Id, r => r.Member);
        }
        catch (Exception ex)
        {
            _eventRsvps = new List<Rsvp>();
            _dialogs.ShowError($"Could not load registrations.\n\n{ex.Message}");
        }

        foreach (var r in _eventRsvps.Where(r => r.Status == RsvpStatus.Going).OrderBy(r => r.Member?.LastName))
            Going.Add(r);
        foreach (var r in _eventRsvps.Where(r => r.Status == RsvpStatus.Waitlisted).OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
            Waitlist.Add(r);
        foreach (var r in _eventRsvps.Where(r => r.Status == RsvpStatus.Cancelled).OrderByDescending(r => r.CreatedAt))
            Cancelled.Add(r);

        // Members who are not already going or waiting can be registered
        var activeIds = _eventRsvps.Where(r => r.Status != RsvpStatus.Cancelled).Select(r => r.MemberId).ToHashSet();
        foreach (var m in _clubMembers.Where(m => !activeIds.Contains(m.Id)).OrderBy(m => m.LastName))
            AvailableMembers.Add(m);
        MemberToRegister = null;

        GoingCount = Going.Count;
        Capacity = Math.Max(evt.Capacity, 1);
        CapacityText = $"{Going.Count} going / {evt.Capacity} places · {Waitlist.Count} on waitlist";
        PredictionText = $"Prediction: {Going.Count} registered → about {_predictor.PredictAttendance(evt, Going.Count)} expected to attend";

        FillCheckInList();
    }

    private void FillCheckInList()
    {
        var search = CheckInSearch.Trim();
        CheckInList.Clear();
        foreach (var r in Going.Where(r => search.Length == 0
                                           || r.Member?.FullName.ContainsIgnoreCase(search) == true
                                           || r.Member?.StudentId.ContainsIgnoreCase(search) == true))
            CheckInList.Add(r);

        CheckInText = $"{Going.Count(r => r.CheckedIn)} of {Going.Count} checked in";
    }

    private bool CanRegister() => MemberToRegister is not null && SelectedEvent is not null;

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private void Register()
    {
        if (SelectedEvent is not { } evt || MemberToRegister is not { } member)
            return;

        // Warn when the member is already going to another event at the same time
        var membersOtherEvents = _allEvents.Where(e => e.Id != evt.Id
            && e.Rsvps.Any(r => r.MemberId == member.Id && r.Status == RsvpStatus.Going));
        var clashes = _clashChecker.FindTimeClashes(evt, membersOtherEvents);
        if (clashes.Count > 0)
        {
            var names = string.Join(", ", clashes.Select(c => $"\"{c.Title}\""));
            if (!_dialogs.Confirm($"{member.FullName} is already registered for {names} at the same time.\n\nRegister anyway?", "Time clash"))
                return;
        }

        Rsvp rsvp;
        try
        {
            rsvp = _waitlist.Register(evt, _eventRsvps, member);
            _rsvpRepository.Add(rsvp);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"{member.FullName} could not be registered.\n\n{ex.Message}");
            return;
        }

        RefreshRsvps();

        if (rsvp.Status == RsvpStatus.Waitlisted)
            _dialogs.ShowInfo($"{evt.Title} is full.\n{member.FullName} was added to the waitlist (position {Waitlist.Count}).", "Event full");
    }

    [RelayCommand]
    private void CancelRsvp(Rsvp? rsvp)
    {
        if (rsvp is null || SelectedEvent is not { } evt)
            return;

        var name = rsvp.Member?.FullName ?? "this member";
        if (!_dialogs.Confirm($"Cancel {name}'s registration for {evt.Title}?", "Cancel registration"))
            return;

        _lastPromoted = null;
        try
        {
            _waitlist.Cancel(rsvp, _eventRsvps);
            _rsvpRepository.Update(rsvp);   // saves the promoted member too
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The registration could not be cancelled.\n\n{ex.Message}");
            RefreshRsvps();
            return;
        }

        RefreshRsvps();

        if (_lastPromoted?.Member is { } promoted)
            _dialogs.ShowInfo($"{promoted.FullName} moved from the waitlist to Going.", "Waitlist update");
    }

    [RelayCommand]
    private void ToggleCheckIn(Rsvp? rsvp)
    {
        if (rsvp is null)
            return;

        rsvp.CheckedIn = !rsvp.CheckedIn;
        try
        {
            _rsvpRepository.Update(rsvp);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Check-in could not be saved.\n\n{ex.Message}");
        }

        FillCheckInList();
    }

    [RelayCommand(CanExecute = nameof(HasEvent))]
    private void ExportAttendance()
    {
        if (SelectedEvent is not { } evt)
            return;

        var path = _dialogs.AskSaveFilePath($"{evt.Title} attendance.csv", "CSV files (*.csv)|*.csv");
        if (path is null)
            return;

        var exporter = new CsvExporter<Rsvp>(
            ("Student ID", r => r.Member?.StudentId),
            ("Name", r => r.Member?.FullName),
            ("Email", r => r.Member?.Email),
            ("Checked In", r => r.CheckedIn ? "Yes" : "No"));

        try
        {
            exporter.Export(Going, path);
            _dialogs.ShowInfo($"Exported {Going.Count} attendees to:\n{path}", "Export complete");
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The file could not be written.\n\n{ex.Message}");
        }
    }
}
