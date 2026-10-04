using ClubHub.Core.Enums;

namespace ClubHub.Core.Models;

public class Rsvp
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }
    public RsvpStatus Status { get; set; } = RsvpStatus.Going;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool CheckedIn { get; set; }
}
