namespace BaseballApi.Models;

public class Team
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public required string City { get; set; }
    public required string Name { get; set; }
    public string? Abbreviation { get; set; }
    public string? ColorHex { get; set; }
    public Park? HomePark { get; set; }
    public ICollection<AlternateTeamName> AlternateTeamNames { get; } = [];
}