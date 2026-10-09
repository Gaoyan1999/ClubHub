using System.Collections.ObjectModel;
using System.Globalization;
using ClubHub.Core.Enums;
using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using ClubHub.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ClubHub.Wpf.ViewModels;

public partial class BudgetViewModel : PageViewModel
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<BudgetEntry> _entryRepository;
    private readonly IRepository<Rsvp> _rsvpRepository;
    private readonly IDialogService _dialogs;

    public override string Title => "Budget";

    public ObservableCollection<Event> Events { get; } = new();
    public ObservableCollection<BudgetEntry> Entries { get; } = new();
    public IReadOnlyList<EntryType> EntryTypes { get; } = Enum.GetValues<EntryType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEvent))]
    [NotifyCanExecuteChangedFor(nameof(SaveEntryCommand), nameof(NewEntryCommand))]
    private Event? _selectedEvent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyCanExecuteChangedFor(nameof(DeleteEntryCommand))]
    private BudgetEntry? _selectedEntry;

    // Form fields for adding or editing one item
    [ObservableProperty] private EntryType _formType = EntryType.Cost;
    [ObservableProperty] private string _formDescription = string.Empty;
    [ObservableProperty] private string _formAmountText = string.Empty;
    [ObservableProperty] private DateTime? _formDate = DateTime.Today;
    [ObservableProperty] private string _errorText = string.Empty;

    // Summary
    [ObservableProperty] private string _ticketIncomeText = string.Empty;
    [ObservableProperty] private string _otherIncomeText = string.Empty;
    [ObservableProperty] private string _totalCostText = string.Empty;
    [ObservableProperty] private string _estimatedCostText = string.Empty;
    [ObservableProperty] private string _profitText = string.Empty;
    [ObservableProperty] private bool _isLoss;

    // Whole club: balance and the income vs cost chart for every event
    [ObservableProperty] private string _clubBalanceText = string.Empty;
    [ObservableProperty] private bool _isClubLoss;
    [ObservableProperty] private ISeries[] _eventMoneySeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _eventAxes = { new Axis() };

    public Axis[] MoneyAxes { get; } = { new Axis { MinLimit = 0, Labeler = value => value.ToString("C0") } };

    public bool HasEvent => SelectedEvent is not null;

    public string FormTitle => SelectedEntry is null ? "Add item" : "Edit selected item";

    public BudgetViewModel(IRepository<Event> eventRepository, IRepository<BudgetEntry> entryRepository,
        IRepository<Rsvp> rsvpRepository, IDialogService dialogs)
    {
        _eventRepository = eventRepository;
        _entryRepository = entryRepository;
        _rsvpRepository = rsvpRepository;
        _dialogs = dialogs;
    }

    protected override void Reload()
    {
        var keepId = SelectedEvent?.Id;
        List<Event> events;
        try
        {
            events = _eventRepository.Find(e => e.ClubId == ClubId);
        }
        catch (Exception ex)
        {
            events = new List<Event>();
            _dialogs.ShowError($"Could not load events.\n\n{ex.Message}");
        }

        Events.Clear();
        foreach (var evt in events.OrderBy(e => e.Start))
            Events.Add(evt);

        SelectedEvent = Events.FirstOrDefault(e => e.Id == keepId) ?? Events.FirstOrDefault();
        Refresh();
        RefreshClubTotals();
    }

    private void RefreshClubTotals()
    {
        var eventIds = Events.Select(e => e.Id).ToList();
        List<Rsvp> rsvps;
        List<BudgetEntry> entries;
        try
        {
            rsvps = _rsvpRepository.Find(r => eventIds.Contains(r.EventId));
            entries = _entryRepository.Find(b => eventIds.Contains(b.EventId));
        }
        catch (Exception ex)
        {
            ClubBalanceText = string.Empty;
            EventMoneySeries = Array.Empty<ISeries>();
            _dialogs.ShowError($"Could not load the club totals.\n\n{ex.Message}");
            return;
        }

        // Events is already sorted by date, so the bars run left to right in time order
        var summaries = BudgetService.SummarizeEach(Events, rsvps, entries);
        var balance = BudgetService.ClubBalance(summaries.Select(s => s.Summary));
        ClubBalanceText = balance >= 0 ? $"Club balance {balance:C}" : $"Club balance −{-balance:C}";
        IsClubLoss = balance < 0;

        EventAxes = new[] { new Axis { Labels = summaries.Select(s => s.Event.Title).ToArray(), LabelsRotation = 15 } };
        EventMoneySeries = new ISeries[]
        {
            new ColumnSeries<double> { Name = "Income", Values = summaries.Select(s => (double)s.Summary.TotalIncome).ToArray() },
            new ColumnSeries<double> { Name = "Cost", Values = summaries.Select(s => (double)s.Summary.TotalCost).ToArray() }
        };
    }

    partial void OnSelectedEventChanged(Event? value) => Refresh();

    // Picking a row loads it into the form for editing
    partial void OnSelectedEntryChanged(BudgetEntry? value)
    {
        ErrorText = string.Empty;
        if (value is null)
            return;

        FormType = value.Type;
        FormDescription = value.Description;
        FormAmountText = value.Amount.ToString(CultureInfo.CurrentCulture);
        FormDate = value.Date;
    }

    private void Refresh()
    {
        Entries.Clear();
        if (SelectedEvent is not { } evt)
        {
            TicketIncomeText = OtherIncomeText = TotalCostText = EstimatedCostText = ProfitText = string.Empty;
            return;
        }

        List<BudgetEntry> entries;
        List<Rsvp> rsvps;
        try
        {
            entries = _entryRepository.Find(b => b.EventId == evt.Id);
            rsvps = _rsvpRepository.Find(r => r.EventId == evt.Id);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Could not load the budget.\n\n{ex.Message}");
            return;
        }

        foreach (var entry in entries.OrderBy(e => e.Date).ThenBy(e => e.Id))
            Entries.Add(entry);

        var summary = BudgetService.Summarize(evt, rsvps, entries);
        TicketIncomeText = $"{summary.TicketIncome:C}  ({summary.CheckedIn} checked in × {evt.TicketPrice:C})";
        OtherIncomeText = $"{summary.OtherIncome:C}";
        TotalCostText = $"{summary.TotalCost:C}";
        EstimatedCostText = $"{summary.EstimatedCost:C}  (rule for a {evt.Type.ToString().ToLower()}, people going)";
        ProfitText = summary.Profit >= 0 ? $"Profit {summary.Profit:C}" : $"Loss {-summary.Profit:C}";
        IsLoss = summary.Profit < 0;

        ClearForm();
    }

    private void ClearForm()
    {
        SelectedEntry = null;
        FormType = EntryType.Cost;
        FormDescription = string.Empty;
        FormAmountText = string.Empty;
        FormDate = DateTime.Today;
        ErrorText = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(HasEvent))]
    private void NewEntry() => ClearForm();

    [RelayCommand(CanExecute = nameof(HasEvent))]
    private void SaveEntry()
    {
        if (SelectedEvent is not { } evt)
            return;

        if (!decimal.TryParse(FormAmountText.Trim().TrimStart('$'), NumberStyles.Number, CultureInfo.CurrentCulture, out var amount))
        {
            ErrorText = "Amount must be a number.";
            return;
        }

        var candidate = new BudgetEntry
        {
            EventId = evt.Id,
            Type = FormType,
            Description = FormDescription.Trim(),
            Amount = amount,
            Date = FormDate?.Date ?? DateTime.Today
        };

        var errors = BudgetEntryValidator.Validate(candidate);
        if (errors.Count > 0)
        {
            ErrorText = string.Join(Environment.NewLine, errors);
            return;
        }

        try
        {
            if (SelectedEntry is { } existing)
            {
                existing.Type = candidate.Type;
                existing.Description = candidate.Description;
                existing.Amount = candidate.Amount;
                existing.Date = candidate.Date;
                _entryRepository.Update(existing);
            }
            else
            {
                _entryRepository.Add(candidate);
            }
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The item could not be saved.\n\n{ex.Message}");
            return;
        }

        Refresh();
        RefreshClubTotals();
    }

    private bool HasSelectedEntry() => SelectedEntry is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void DeleteEntry()
    {
        if (SelectedEntry is not { } entry)
            return;

        if (!_dialogs.Confirm($"Delete \"{entry.Description}\" ({entry.Amount:C})?", "Delete item"))
            return;

        try
        {
            _entryRepository.Delete(entry);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The item could not be deleted.\n\n{ex.Message}");
            return;
        }

        Refresh();
        RefreshClubTotals();
    }
}
