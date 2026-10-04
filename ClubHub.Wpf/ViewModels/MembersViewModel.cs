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

public partial class MembersViewModel : PageViewModel
{
    private const string AllRoles = "All roles";

    private readonly IRepository<Member> _memberRepository;
    private readonly IDialogService _dialogs;
    private readonly int _clubId;

    // Every member of the club; Members holds only the ones that match the current filters
    private List<Member> _allMembers = new();

    public override string Title => "Members";

    public ObservableCollection<Member> Members { get; } = new();

    public List<string> RoleFilters { get; } = new[] { AllRoles }.Concat(Enum.GetNames<MemberRole>()).ToList();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedRoleFilter = AllRoles;
    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditMemberCommand), nameof(DeleteMemberCommand))]
    private Member? _selectedMember;

    public MembersViewModel(IRepository<Member> memberRepository, IDialogService dialogs, int clubId)
    {
        _memberRepository = memberRepository;
        _dialogs = dialogs;
        _clubId = clubId;
        LoadMembers();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedRoleFilterChanged(string value) => ApplyFilter();

    private void LoadMembers()
    {
        try
        {
            _allMembers = _memberRepository.Find(m => m.ClubId == _clubId);
        }
        catch (Exception ex)
        {
            _allMembers = new List<Member>();
            _dialogs.ShowError($"Could not load members.\n\n{ex.Message}");
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = SearchText.Trim();
        var hasRole = Enum.TryParse<MemberRole>(SelectedRoleFilter, out var role);

        var matches = _allMembers
            .Where(m => !hasRole || m.Role == role)
            .Where(m => search.Length == 0
                        || m.FullName.ContainsIgnoreCase(search)
                        || m.StudentId.ContainsIgnoreCase(search)
                        || m.Email.ContainsIgnoreCase(search))
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToList();

        Members.Clear();
        foreach (var member in matches)
            Members.Add(member);

        StatusText = $"Showing {Members.Count} of {_allMembers.Count} members";
    }

    [RelayCommand]
    private void AddMember()
    {
        var newMember = new Member { ClubId = _clubId, JoinedDate = DateTime.Today };
        var form = new MemberDialogViewModel("Add member", newMember, _allMembers);
        if (!_dialogs.ShowMemberDialog(form))
            return;

        form.ApplyTo(newMember);
        try
        {
            _memberRepository.Add(newMember);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The member could not be saved.\n\n{ex.Message}");
            return;
        }

        _allMembers.Add(newMember);
        ApplyFilter();
        SelectedMember = newMember;
    }

    private bool HasSelection() => SelectedMember is not null;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void EditMember()
    {
        if (SelectedMember is not { } member)
            return;

        var form = new MemberDialogViewModel("Edit member", member, _allMembers);
        if (!_dialogs.ShowMemberDialog(form))
            return;

        form.ApplyTo(member);
        try
        {
            _memberRepository.Update(member);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The changes could not be saved.\n\n{ex.Message}");
        }

        // Rebuild the list so the edited row shows its new values
        ApplyFilter();
        SelectedMember = member;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteMember()
    {
        if (SelectedMember is not { } member)
            return;

        var question = $"Delete {member.FullName} ({member.StudentId})?\n\nTheir event registrations will also be removed.";
        if (!_dialogs.Confirm(question, "Delete member"))
            return;

        try
        {
            _memberRepository.Delete(member);
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The member could not be deleted.\n\n{ex.Message}");
            return;
        }

        _allMembers.Remove(member);
        ApplyFilter();
    }

    [RelayCommand]
    private void ExportMembers()
    {
        var path = _dialogs.AskSaveFilePath("members.csv", "CSV files (*.csv)|*.csv");
        if (path is null)
            return;

        var exporter = new CsvExporter<Member>(
            ("Student ID", m => m.StudentId),
            ("First Name", m => m.FirstName),
            ("Last Name", m => m.LastName),
            ("Email", m => m.Email),
            ("Role", m => m.Role),
            ("Joined", m => m.JoinedDate.ToString("yyyy-MM-dd")));

        try
        {
            // Export what is on screen, so search and role filters apply to the file too
            exporter.Export(Members, path);
            _dialogs.ShowInfo($"Exported {Members.Count} members to:\n{path}", "Export complete");
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"The file could not be written.\n\n{ex.Message}");
        }
    }
}
