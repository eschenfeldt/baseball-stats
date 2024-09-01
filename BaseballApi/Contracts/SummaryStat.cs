using System;

namespace BaseballApi.Contracts;

public struct SummaryStat
{
    public Stat Definition { get; set; }
    public decimal? Value { get; set; }
}
