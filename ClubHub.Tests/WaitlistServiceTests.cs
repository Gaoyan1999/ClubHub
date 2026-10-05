using ClubHub.Core.Enums;
using ClubHub.Core.Models;
using ClubHub.Core.Services;

namespace ClubHub.Tests;

public class WaitlistServiceTests
{
    private DateTime _now;
    private WaitlistService _service = null!;
    private Workshop _event = null!;
    private List<Rsvp> _rsvps = null!;

    [SetUp]
    public void SetUp()
    {
        // Fake clock: each registration is one minute after the last
        _now = new DateTime(2026, 10, 1, 9, 0, 0);
        _service = new WaitlistService(() => _now = _now.AddMinutes(1));
        _event = new Workshop { Id = 1, Title = "Resume Clinic", Capacity = 2 };
        _rsvps = new List<Rsvp>();
    }

    private Rsvp Register(int memberId)
    {
        var member = new Member($"2400000{memberId}", $"First{memberId}", $"Last{memberId}", $"m{memberId}@uts.edu.au")
        {
            Id = memberId
        };
        var rsvp = _service.Register(_event, _rsvps, member);
        _rsvps.Add(rsvp);
        return rsvp;
    }

    [Test]
    public void Register_WithSpace_IsGoing()
    {
        var rsvp = Register(1);

        Assert.That(rsvp.Status, Is.EqualTo(RsvpStatus.Going));
        Assert.That(rsvp.EventId, Is.EqualTo(_event.Id));
    }

    [Test]
    public void Register_WhenFull_IsWaitlisted()
    {
        Register(1);
        Register(2);

        var third = Register(3);

        Assert.That(third.Status, Is.EqualTo(RsvpStatus.Waitlisted));
    }

    [Test]
    public void Register_Twice_Throws()
    {
        Register(1);

        Assert.That(() => Register(1), Throws.InvalidOperationException);
    }

    [Test]
    public void Register_AfterCancelling_IsAllowed()
    {
        var first = Register(1);
        _service.Cancel(first, _rsvps);

        Assert.That(() => Register(1), Throws.Nothing);
    }

    [Test]
    public void Cancel_Going_PromotesFirstWaitlistedAndRaisesEvent()
    {
        var going = Register(1);
        Register(2);
        var firstWaiting = Register(3);
        var secondWaiting = Register(4);

        Rsvp? raised = null;
        _service.MemberPromoted += (_, e) => raised = e.Rsvp;

        var promoted = _service.Cancel(going, _rsvps);

        Assert.Multiple(() =>
        {
            Assert.That(going.Status, Is.EqualTo(RsvpStatus.Cancelled));
            Assert.That(promoted, Is.SameAs(firstWaiting));
            Assert.That(firstWaiting.Status, Is.EqualTo(RsvpStatus.Going));
            Assert.That(secondWaiting.Status, Is.EqualTo(RsvpStatus.Waitlisted));
            Assert.That(raised, Is.SameAs(firstWaiting));
        });
    }

    [Test]
    public void Cancel_Waitlisted_DoesNotPromoteAnyone()
    {
        Register(1);
        Register(2);
        var waiting = Register(3);
        var lastWaiting = Register(4);

        var promoted = _service.Cancel(waiting, _rsvps);

        Assert.That(promoted, Is.Null);
        Assert.That(lastWaiting.Status, Is.EqualTo(RsvpStatus.Waitlisted));
    }

    [Test]
    public void Cancel_ClearsCheckIn()
    {
        var rsvp = Register(1);
        rsvp.CheckedIn = true;

        _service.Cancel(rsvp, _rsvps);

        Assert.That(rsvp.CheckedIn, Is.False);
    }

    [Test]
    public void Cancel_WithEmptyWaitlist_ReturnsNull()
    {
        var rsvp = Register(1);

        Assert.That(_service.Cancel(rsvp, _rsvps), Is.Null);
    }
}
