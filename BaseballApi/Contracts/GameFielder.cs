using System;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct GameFielder(Fielder fielder)
{
    public PlayerInfo Player { get; set; } = new(fielder.Player);
    public int Number { get; set; } = fielder.Number;

    public Dictionary<string, int> Stats { get; } = new()
    {
        { Stat.Games.Name, fielder.Games },
        { Stat.Errors.Name, fielder.Errors },
        { Stat.ErrorsThrowing.Name, fielder.ErrorsThrowing },
        { Stat.ErrorsFielding.Name, fielder.ErrorsFielding },
        { Stat.Putouts.Name, fielder.Putouts },
        { Stat.Assists.Name, fielder.Assists },
        { Stat.StolenBaseAttempts.Name, fielder.StolenBaseAttempts },
        { Stat.CaughtStealing.Name, fielder.CaughtStealing },
        { Stat.DoublePlays.Name, fielder.DoublePlays },
        { Stat.TriplePlays.Name, fielder.TriplePlays },
        { Stat.PassedBalls.Name, fielder.PassedBalls },
        { Stat.PickoffFailed.Name, fielder.PickoffFailed },
        { Stat.PickoffSuccess.Name, fielder.PickoffSuccess },
    };
}
