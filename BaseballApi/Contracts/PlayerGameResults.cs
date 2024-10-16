using System;

namespace BaseballApi.Contracts;

public class PlayerGameResults : PagedResult<PlayerGame>
{
    public required IReadOnlyDictionary<string, Stat> PitchingStats { get; set; }
    public required IReadOnlyDictionary<string, Stat> BattingStats { get; set; }
}
