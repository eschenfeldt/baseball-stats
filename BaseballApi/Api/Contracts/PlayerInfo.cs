using System;
using System.Diagnostics.CodeAnalysis;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct PlayerInfo(Player player)
{
    public long Id { get; set; } = player.Id;
    public Guid ExternalId { get; set; } = player.ExternalId;
    public string Name { get; set; } = player.Name;
    public DateOnly? DateOfBirth { get; set; } = player.DateOfBirth;
    public string? FirstName { get; set; } = player.FirstName;
    public string? MiddleName { get; set; } = player.MiddleName;
    public string? LastName { get; set; } = player.LastName;
    public string? Suffix { get; set; } = player.Suffix;
    public Uri? FangraphsPage { get; set; } = player.FangraphsPage;
}
