using System.Security.Policy;
using BaseballApi.Models;

namespace BaseballApi;

public class Player
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public Uri? FangraphsPage { get; set; }

    public ICollection<MediaResource> Media { get; set; } = [];
}
