using System;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct PlayerSummary
{
    public PlayerInfo Info { get; set; }
    public RemoteFileDetail? Photo { get; set; }
    public List<SummaryStat> SummaryStats { get; set; }
}
