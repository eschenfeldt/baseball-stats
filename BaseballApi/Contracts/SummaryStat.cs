using System;

namespace BaseballApi.Contracts;

public struct SummaryStat
{
    public StatCategory Category { get; set; }
    public Stat Definition { get; set; }
    public decimal? Value { get; set; }
}
