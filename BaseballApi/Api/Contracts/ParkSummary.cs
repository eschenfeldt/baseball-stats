using System;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public class ParkSummary
{
    public required Park Park { get; set; }
    public int Games { get; set; }
    public int Teams { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateOnly? FirstGameDate { get; set; }
    public DateOnly? LastGameDate { get; set; }
}

public enum ParkSummaryOrder
{
    [ParamValue("park")]
    Park,
    [ParamValue("games")]
    Games,
    [ParamValue("teams")]
    Teams,
    [ParamValue("wins")]
    Wins,
    [ParamValue("losses")]
    Losses,
    [ParamValue("firstGame")]
    FirstGame,
    [ParamValue("lastGame")]
    LastGame
}