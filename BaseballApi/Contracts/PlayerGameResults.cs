using System;

namespace BaseballApi.Contracts;

public class PlayerGameResults : PagedResult<PlayerGame>
{
    public required IReadOnlyDictionary<string, Stat> Stats { get; set; }
}
