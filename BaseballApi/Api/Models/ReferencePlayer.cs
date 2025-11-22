using System;

namespace BaseballApi.Models;

public class ReferencePlayer
{
    public Guid Id { get; set; }
    public long? PlayerId { get; set; }
    public Player? Player { get; set; }
    public required string Name { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public string? FangraphsId { get; set; }
    public string? BaseballReferenceId { get; set; }
    public int? MLBAMId { get; set; }
    public string? RetrosheetId { get; set; }
    public int? CurrentNumber { get; set; }
    public long? CurrentTeamId { get; set; }
    public Team? CurrentTeam { get; set; }
}
