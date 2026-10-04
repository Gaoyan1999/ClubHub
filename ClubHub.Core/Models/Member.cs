using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class Member
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTime JoinedDate { get; set; } = DateTime.Today;

    public string FullName => $"{FirstName} {LastName}";

    // Parameterless constructor is needed by EF Core
    public Member() { }

    public Member(string studentId, string firstName, string lastName, string email)
    {
        StudentId = studentId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public Member(string studentId, string firstName, string lastName, string email, MemberRole role)
        : this(studentId, firstName, lastName, email)
    {
        Role = role;
    }
}
