using System.Collections.ObjectModel;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubHub.Wpf.ViewModels;

public partial class EventsViewModel : PageViewModel
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Room> _roomRepository;
    private readonly IClashChecker _clashChecker;
    private readonly IDialogService _dialogs;
    private readonly Action<Event> _openEventDetail;

    private List<Event> _allEvents = new();

    public override string Title => "Events & Calendar";

    public ObservableCollection<Event> Events { get; } = new();

    /// <summary>Days that have at least one event; the calendar highlights them.</summary>
    [ObservableProperty] private ISet<DateTime> _eventDays = new HashSet<DateTime>();

    /// <summary>When set, only events on this day are listed.</summary>
    [ObservableProperty] private DateTime? _selectedDate;

    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditEventCommand), nameof(DeleteEventCommand), nameof(OpenDetailsCommand))]
    private Event? _selectedEvent;

    public EventsViewModel(IRepository<Event> eventRepository, IRepository<Room> roomRepository,
        IClashChecker clashChecker, IDialogService dialogs, Action<Event> openEventDetail)
    {
        _eventRepository = eventRepository;
        _roomRepository = roomRepository;
        _clashChecker = clashChecker;
        _dialogs = dialogs;
        _openEventDetail = openEventDetail;
    }

    partial void OnSelectedDateChanged(DateTime? value) => ApplyFilter();

    protected override void Reload()
    {
        try
        {
            _allEvents = _eventRepository.Find(e => e.ClubId == ClubId, e => e.Room, e => e.Rsvps);
        }
        catch (Exception ex)
        {
            _allEvents = new List<Event>();
            _dialogs.ShowError($"Could not load events.\n\n{ex.Message}");
        }

        EventDays = _allEvents.Select(e => e.Start.Date).ToHashSet();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var selected = SelectedEvent;
        var matches = _allEvents
            .Where(e => SelectedDate is null || e.Start.Date == SelectedDate.Value.Date)
            .OrderBy(e => e.Start)
            .ToList();

        Events.Clear();
        foreach (var evt in matches)
            Events.Add(evt);

        SelectedEvent = selected is not null && Events.Contains(selected) ? selected : null;

        var upcoming = _allEvents.Count(e => e.Start >= DateTime.Now);
        StatusText = SelectedDate is null
            ? $"{_allEvents.Count} events ({upcoming} upcoming)"
            : $"{Events.Count} events on {SelectedDate:dddd d MMMM}";
    }

    [RelayCommand]
    private void ShowAllDates() => SelectedDate = null;

    [RelayCommand]
    private void AddEvent()
    {
        if (!TryCreateForm(null, out var form) || !_dialogs.ShowEventDialog(form))
            return;

        var evt = form.CreateEvent(ClubId);
        try
        {
            _eventRepository.Add(evt);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The event could not be saved.\n\n{ex.Message}");
            return;
        }

        Reload();
        SelectedEvent = evt;
    }

    private bool HasSelection() => SelectedEvent is not null;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void EditEvent()
    {
        if (SelectedEvent is not { } evt || !TryCreateForm(evt, out var form) || !_dialogs.ShowEventDialog(form))
            return;

        form.ApplyTo(evt);
        try
        {
            _eventRepository.Update(evt);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The changes could not be saved.\n\n{ex.Message}");
        }

        Reload();
        SelectedEvent = evt;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteEvent()
    {
        if (SelectedEvent is not { } evt)
            return;

        var question = $"Delete \"{evt.Title}\"?\n\nIts {evt.Rsvps.Count} registrations and all budget items will also be deleted.";
        if (!_dialogs.Confirm(question, "Delete event"))
            return;

        try
        {
            _eventRepository.Delete(evt);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The event could not be deleted.\n\n{ex.Message}");
            return;
        }

        Reload();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void OpenDetails()
    {
        if (SelectedEvent is { } evt)
            _openEventDetail(evt);
    }

    // Loads rooms and all events (every club shares the rooms) for the clash check
    private bool TryCreateForm(Event? evt, out EventDialogViewModel form)
    {
        form = null!;
        try
        {
            var rooms = _roomRepository.GetAll().OrderBy(r => r.Name).ToList();
            var allEvents = _eventRepository.GetAll();
            form = new EventDialogViewModel(evt, rooms, allEvents, _clashChecker);
            return true;
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Could not open the event form.\n\n{ex.Message}");
            return false;
        }
    }
}
