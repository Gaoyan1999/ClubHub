using System.Text.RegularExpressions;
using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public static class MemberValidator
{
    private static readonly Regex StudentIdPattern = new(@"^\d{8}$");
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    /// <summary>
    /// Returns a list of problems with the member. An empty list means the member is valid.
    /// <paramref name="existingMembers"/> is used to block duplicate student IDs.
    /// </summary>
    public static List<string> Validate(Member member, IEnumerable<Member> existingMembers)
    {
        var errors = new List<string>();

        if (!StudentIdPattern.IsMatch(member.StudentId.Trim()))
            errors.Add("Student ID must be 8 digits.");
        else if (existingMembers.Any(m => m.Id != member.Id && m.StudentId == member.StudentId.Trim()))
            errors.Add("Another member already has this student ID.");

        if (string.IsNullOrWhiteSpace(member.FirstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(member.LastName))
            errors.Add("Last name is required.");

        if (!EmailPattern.IsMatch(member.Email.Trim()))
            errors.Add("Email address is not valid.");

        if (member.JoinedDate.Date > DateTime.Today)
            errors.Add("Joined date cannot be in the future.");

        return errors;
    }
}
