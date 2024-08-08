using System;

namespace BaseballApi.Contracts;

public struct GameFielder(Fielder fielder)
{
    public Player Player { get; set; } = fielder.Player;

    public int Number { get; set; } = fielder.Number;
    public int Games { get; set; } = fielder.Games;
    public int Errors { get; set; } = fielder.Errors;
    public int ErrorsThrowing { get; set; } = fielder.ErrorsThrowing;
    public int ErrorsFielding { get; set; } = fielder.ErrorsFielding;
    public int Putouts { get; set; } = fielder.Putouts;
    public int Assists { get; set; } = fielder.Assists;
    public int StolenBaseAttempts { get; set; } = fielder.StolenBaseAttempts;
    public int CaughtStealing { get; set; } = fielder.CaughtStealing;
    public int DoublePlays { get; set; } = fielder.DoublePlays;
    public int TriplePlays { get; set; } = fielder.TriplePlays;
    public int PassedBalls { get; set; } = fielder.PassedBalls;
    public int PickoffFailed { get; set; } = fielder.PickoffFailed;
    public int PickoffSuccess { get; set; } = fielder.PickoffSuccess;
}
