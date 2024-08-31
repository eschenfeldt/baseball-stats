using System;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct PlayerSummary
{
    public PlayerInfo Info { get; set; }
    public MediaResource? Photo { get; set; }
    public List<SummaryStat> SummaryStats { get; set; }
}
