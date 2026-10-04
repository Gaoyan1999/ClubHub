using ClubHub.Core.Enums;
using ClubHub.Core.Models;
using ClubHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClubHub.Wpf.ViewModels;

/// <summary>Form data for adding or editing one member. Works on a copy until the user saves.</summary>
public partial class MemberDialogViewModel : ObservableObject
{
    private readonly int _memberId;
    private readonly IReadOnlyList<Member> _existingMembers;

    public string Title { get; }

    public IReadOnlyList<MemberRole> Roles { get; } = Enum.GetValues<MemberRole>();

    [ObservableProperty] private string _studentId;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private MemberRole _role;
    [ObservableProperty] private DateTime? _joinedDate;
    [ObservableProperty] private string _errorText = string.Empty;

    /// <summary>Raised with true when the form is valid and saved, so the window can close.</summary>
    public event Action<bool>? CloseRequested;

    public MemberDialogViewModel(string title, Member member, IReadOnlyList<Member> existingMembers)
    {
        Title = title;
        _memberId = member.Id;
        _existingMembers = existingMembers;

        _studentId = member.StudentId;
        _firstName = member.FirstName;
        _lastName = member.LastName;
        _email = member.Email;
        _role = member.Role;
        _joinedDate = member.JoinedDate;
    }

    [RelayCommand]
    private void Save()
    {
        if (JoinedDate is null)
        {
            ErrorText = "Joined date is required.";
            return;
        }

        var candidate = new Member { Id = _memberId };
        ApplyTo(candidate);

        var errors = MemberValidator.Validate(candidate, _existingMembers);
        if (errors.Count > 0)
        {
            ErrorText = string.Join(Environment.NewLine, errors);
            return;
        }

        CloseRequested?.Invoke(true);
    }

    /// <summary>Copies the form values onto a member (trimmed).</summary>
    public void ApplyTo(Member member)
    {
        member.StudentId = StudentId.Trim();
        member.FirstName = FirstName.Trim();
        member.LastName = LastName.Trim();
        member.Email = Email.Trim();
        member.Role = Role;
        member.JoinedDate = JoinedDate?.Date ?? DateTime.Today;
    }
}
