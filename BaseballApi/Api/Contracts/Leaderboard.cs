using System;

namespace BaseballApi.Contracts;

public class Leaderboard<T> : PagedResult<T>
{
    public required Dictionary<string, Stat> Stats { get; set; }
}
