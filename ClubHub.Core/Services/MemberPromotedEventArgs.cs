using ClubHub.Core.Models;

namespace ClubHub.Core.Services;

public class MemberPromotedEventArgs : EventArgs
{
    public Rsvp Rsvp { get; }

    public MemberPromotedEventArgs(Rsvp rsvp)
    {
        Rsvp = rsvp;
    }
}
