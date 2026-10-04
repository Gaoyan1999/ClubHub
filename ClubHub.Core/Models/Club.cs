namespace ClubHub.Core.Models;

public class Club
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoPath { get; set; }

    public List<Member> Members { get; set; } = new();
    public List<Event> Events { get; set; } = new();
}
