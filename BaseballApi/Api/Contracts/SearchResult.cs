using System;

namespace BaseballApi.Contracts;

public struct SearchResult
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SearchResultType Type { get; set; }
    public long Id { get; set; }
}

public enum SearchResultType
{
    Player,
    Team,
    Game,
}
