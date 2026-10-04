using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class MemberValidatorTests
{
    private static Member ValidMember() => new("24000099", "Grace", "Hopper", "grace.hopper@student.uts.edu.au")
    {
        Id = 99,
        JoinedDate = DateTime.Today
    };

    [Test]
    public void ValidMember_HasNoErrors()
    {
        var errors = MemberValidator.Validate(ValidMember(), Array.Empty<Member>());

        Assert.That(errors, Is.Empty);
    }

    [TestCase("")]
    [TestCase("1234567")]
    [TestCase("123456789")]
    [TestCase("abcdefgh")]
    public void StudentIdNotEightDigits_IsError(string studentId)
    {
        var member = ValidMember();
        member.StudentId = studentId;

        var errors = MemberValidator.Validate(member, Array.Empty<Member>());

        Assert.That(errors, Has.Some.Contains("8 digits"));
    }

    [Test]
    public void DuplicateStudentId_IsError()
    {
        var existing = new Member("24000099", "Other", "Person", "other@uts.edu.au") { Id = 1 };

        var errors = MemberValidator.Validate(ValidMember(), new[] { existing });

        Assert.That(errors, Has.Some.Contains("already has this student ID"));
    }

    [Test]
    public void EditingSameMember_IsNotDuplicate()
    {
        var member = ValidMember();

        var errors = MemberValidator.Validate(member, new[] { member });

        Assert.That(errors, Is.Empty);
    }

    [TestCase("not-an-email")]
    [TestCase("missing@domain")]
    [TestCase("two words@uts.edu.au")]
    public void BadEmail_IsError(string email)
    {
        var member = ValidMember();
        member.Email = email;

        var errors = MemberValidator.Validate(member, Array.Empty<Member>());

        Assert.That(errors, Has.Some.Contains("Email"));
    }

    [Test]
    public void BlankNamesAndFutureDate_AreAllReported()
    {
        var member = ValidMember();
        member.FirstName = " ";
        member.LastName = "";
        member.JoinedDate = DateTime.Today.AddDays(1);

        var errors = MemberValidator.Validate(member, Array.Empty<Member>());

        Assert.That(errors, Has.Count.EqualTo(3));
    }
}
