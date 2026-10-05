using ClubHub.Core.Enums;
using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

/// <summary>
/// Registers members for events and runs the waitlist. Works only on the objects passed in;
/// the caller saves the changes.
/// </summary>
public class WaitlistService
{
    private readonly Func<DateTime> _clock;

    /// <summary>Raised when a waitlisted member moves up to Going after someone cancels.</summary>
    public event EventHandler<MemberPromotedEventArgs>? MemberPromoted;

    public WaitlistService() : this(() => DateTime.Now) { }

    // Tests pass their own clock so the waitlist order is predictable
    public WaitlistService(Func<DateTime> clock)
    {
        _clock = clock;
    }

    /// <summary>
    /// Creates a registration for the member: Going if there is space, otherwise Waitlisted.
    /// Throws if the member is already registered or waitlisted.
    /// </summary>
    public Rsvp Register(Event evt, IReadOnlyCollection<Rsvp> eventRsvps, Member member)
    {
        if (eventRsvps.Any(r => r.MemberId == member.Id && r.Status != RsvpStatus.Cancelled))
            throw new InvalidOperationException($"{member.FullName} is already registered for {evt.Title}.");

        var goingCount = eventRsvps.Count(r => r.Status == RsvpStatus.Going);

        return new Rsvp
        {
            EventId = evt.Id,
            MemberId = member.Id,
            Member = member,
            Status = goingCount < evt.Capacity ? RsvpStatus.Going : RsvpStatus.Waitlisted,
            CreatedAt = _clock()
        };
    }

    /// <summary>
    /// Cancels a registration. If the person was Going, the first person on the waitlist
    /// moves up. Returns the promoted registration, or null if nobody moved up.
    /// </summary>
    public Rsvp? Cancel(Rsvp rsvp, IEnumerable<Rsvp> eventRsvps)
    {
        if (rsvp.Status == RsvpStatus.Cancelled)
            return null;

        var wasGoing = rsvp.Status == RsvpStatus.Going;
        rsvp.Status = RsvpStatus.Cancelled;
        rsvp.CheckedIn = false;

        if (!wasGoing)
            return null;

        // First come, first served
        var waitlist = new Queue<Rsvp>(eventRsvps
            .Where(r => r.Status == RsvpStatus.Waitlisted)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id));

        if (waitlist.Count == 0)
            return null;

        var promoted = waitlist.Dequeue();
        promoted.Status = RsvpStatus.Going;
        MemberPromoted?.Invoke(this, new MemberPromotedEventArgs(promoted));
        return promoted;
    }
}
