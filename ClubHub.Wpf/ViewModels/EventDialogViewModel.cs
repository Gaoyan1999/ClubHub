using System.Globalization;
using ClubHub.Core.Enums;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubHub.Wpf.ViewModels;

/// <summary>Form data for creating or editing an event. Works on text fields until the user saves.</summary>
public partial class EventDialogViewModel : ObservableObject
{
    private readonly int _eventId;
    private readonly IReadOnlyList<Event> _allEvents;
    private readonly IClashChecker _clashChecker;

    // The checked event built by Save(); copied onto the real event by ApplyTo()
    private Event? _validated;

    public string Title { get; }

    /// <summary>The event type can only be picked when creating; an existing event keeps its type.</summary>
    public bool IsNew { get; }

    public IReadOnlyList<EventType> EventTypes { get; } = Enum.GetValues<EventType>();

    public IReadOnlyList<Room> Rooms { get; }

    /// <summary>Start/end time choices: 07:00 to 23:30 in 30 minute steps.</summary>
    public IReadOnlyList<TimeSpan> TimeSlots { get; } =
        Enumerable.Range(14, 34).Select(i => TimeSpan.FromMinutes(i * 30)).ToList();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCostPerHead), nameof(IsCompetition), nameof(CostPerHeadLabel))]
    private EventType _eventType;

    [ObservableProperty] private string _eventTitle = string.Empty;
    [ObservableProperty] private Room? _room;
    [ObservableProperty] private DateTime? _date;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private string _capacityText = string.Empty;
    [ObservableProperty] private string _ticketPriceText = string.Empty;
    [ObservableProperty] private string _costPerHeadText = "0";
    [ObservableProperty] private string _prizePoolText = "0";
    [ObservableProperty] private string _teamSizeText = "1";
    [ObservableProperty] private string _estimateText = string.Empty;
    [ObservableProperty] private string _errorText = string.Empty;

    public bool HasCostPerHead => EventType != EventType.Competition;
    public bool IsCompetition => EventType == EventType.Competition;
    public string CostPerHeadLabel => EventType == EventType.Workshop ? "Materials / person" : "Food / person";

    public event Action<bool>? CloseRequested;

    /// <param name="evt">The event to edit, or null to create a new one.</param>
    /// <param name="allEvents">Every event in the database, used for the room clash check.</param>
    public EventDialogViewModel(Event? evt, IReadOnlyList<Room> rooms, IReadOnlyList<Event> allEvents, IClashChecker clashChecker)
    {
        Rooms = rooms;
        _allEvents = allEvents;
        _clashChecker = clashChecker;
        IsNew = evt is null;
        Title = IsNew ? "New event" : "Edit event";

        if (evt is null)
        {
            _eventType = EventType.Workshop;
            _room = rooms.FirstOrDefault();
            _date = DateTime.Today.AddDays(7);
            _startTime = TimeSpan.FromHours(17);
            _endTime = TimeSpan.FromHours(19);
            _capacityText = "30";
            _ticketPriceText = "0";
        }
        else
        {
            _eventId = evt.Id;
            _eventType = evt.Type;
            _eventTitle = evt.Title;
            _room = rooms.FirstOrDefault(r => r.Id == evt.RoomId);
            _date = evt.Start.Date;
            _startTime = evt.Start.TimeOfDay;
            _endTime = evt.End.TimeOfDay;
            _capacityText = evt.Capacity.ToString();
            _ticketPriceText = evt.TicketPrice.ToString(CultureInfo.CurrentCulture);

            switch (evt)
            {
                case Workshop w:
                    _costPerHeadText = w.MaterialsCostPerHead.ToString(CultureInfo.CurrentCulture);
                    break;
                case Social s:
                    _costPerHeadText = s.CateringCostPerHead.ToString(CultureInfo.CurrentCulture);
                    break;
                case Competition c:
                    _prizePoolText = c.PrizePool.ToString(CultureInfo.CurrentCulture);
                    _teamSizeText = c.TeamSize.ToString();
                    break;
            }
        }

        UpdateEstimate();
    }

    // Keep the cost estimate up to date while the user types
    partial void OnEventTypeChanged(EventType value) => UpdateEstimate();
    partial void OnCapacityTextChanged(string value) => UpdateEstimate();
    partial void OnCostPerHeadTextChanged(string value) => UpdateEstimate();
    partial void OnPrizePoolTextChanged(string value) => UpdateEstimate();

    private void UpdateEstimate()
    {
        var errors = new List<string>();
        var candidate = BuildCandidate(errors);
        EstimateText = errors.Count == 0
            ? $"Estimated cost if full: {candidate.EstimateCost(candidate.Capacity):C}"
            : string.Empty;
    }

    [RelayCommand]
    private void Save()
    {
        var errors = new List<string>();
        var candidate = BuildCandidate(errors);

        if (errors.Count == 0)
            errors.AddRange(EventValidator.Validate(candidate, Room));

        if (errors.Count == 0)
        {
            foreach (var clash in _clashChecker.FindRoomClashes(candidate, _allEvents))
                errors.Add($"{Room!.Name} is already booked for \"{clash.Title}\" ({clash.Start:ddd d MMM, HH:mm}–{clash.End:HH:mm}).");
        }

        if (errors.Count > 0)
        {
            ErrorText = string.Join(Environment.NewLine, errors);
            return;
        }

        _validated = candidate;
        CloseRequested?.Invoke(true);
    }

    /// <summary>Creates a new event of the chosen type from the saved form.</summary>
    public Event CreateEvent(int clubId)
    {
        var evt = NewEventOfType(EventType);
        evt.ClubId = clubId;
        ApplyTo(evt);
        return evt;
    }

    /// <summary>Copies the saved form values onto an event of the same type.</summary>
    public void ApplyTo(Event target)
    {
        var source = _validated ?? throw new InvalidOperationException("Save the form before applying it.");

        target.Title = source.Title;
        target.Start = source.Start;
        target.End = source.End;
        target.RoomId = source.RoomId;
        target.Room = source.Room;
        target.Capacity = source.Capacity;
        target.TicketPrice = source.TicketPrice;

        switch (target, source)
        {
            case (Workshop t, Workshop s):
                t.MaterialsCostPerHead = s.MaterialsCostPerHead;
                break;
            case (Social t, Social s):
                t.CateringCostPerHead = s.CateringCostPerHead;
                break;
            case (Competition t, Competition s):
                t.PrizePool = s.PrizePool;
                t.TeamSize = s.TeamSize;
                break;
        }
    }

    private static Event NewEventOfType(EventType type) => type switch
    {
        EventType.Workshop => new Workshop(),
        EventType.Social => new Social(),
        _ => new Competition()
    };

    // Turns the text fields into an event; adds an error for every field that is not a number
    private Event BuildCandidate(List<string> errors)
    {
        var evt = NewEventOfType(EventType);
        evt.Id = _eventId;
        evt.Title = EventTitle.Trim();
        evt.Room = Room;
        evt.RoomId = Room?.Id ?? 0;

        var day = Date?.Date ?? DateTime.Today;
        if (Date is null)
            errors.Add("Please choose a date.");
        evt.Start = day + StartTime;
        evt.End = day + EndTime;

        evt.Capacity = ParseInt(CapacityText, "Capacity", errors);
        evt.TicketPrice = ParseMoney(TicketPriceText, "Ticket price", errors);

        switch (evt)
        {
            case Workshop w:
                w.MaterialsCostPerHead = ParseMoney(CostPerHeadText, CostPerHeadLabel, errors);
                break;
            case Social s:
                s.CateringCostPerHead = ParseMoney(CostPerHeadText, CostPerHeadLabel, errors);
                break;
            case Competition c:
                c.PrizePool = ParseMoney(PrizePoolText, "Prize money", errors);
                c.TeamSize = ParseInt(TeamSizeText, "Team size", errors);
                break;
        }

        return evt;
    }

    private static int ParseInt(string text, string field, List<string> errors)
    {
        if (int.TryParse(text.Trim(), out var value))
            return value;

        errors.Add($"{field} must be a whole number.");
        return 0;
    }

    private static decimal ParseMoney(string text, string field, List<string> errors)
    {
        if (decimal.TryParse(text.Trim().TrimStart('$'), NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
            return value;

        errors.Add($"{field} must be a number.");
        return 0;
    }
}
