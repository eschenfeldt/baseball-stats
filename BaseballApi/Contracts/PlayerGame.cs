using System;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct PlayerGame
{
    public GameSummary Game { get; set; }
    public GameBatter? Batter { get; set; }
    public GamePitcher? Pitcher { get; set; }
    public GameFielder? Fielder { get; set; }

    public PlayerGame(Game game, long playerId)
    {
        Game = new GameSummary(game);
        var awayBatter = game.AwayBoxScore?.Batters.FirstOrDefault(b => b.PlayerId == playerId);
        var homeBatter = game.HomeBoxScore?.Batters.FirstOrDefault(b => b.PlayerId == playerId);
        if (awayBatter != null && homeBatter != null)
        {
            throw new NotImplementedException("Danny Jansen situation not implemented");
        }
        else if (awayBatter != null)
        {
            Batter = new GameBatter(awayBatter);
        }
        else if (homeBatter != null)
        {
            Batter = new GameBatter(homeBatter);
        }

        var awayPitcher = game.AwayBoxScore?.Pitchers.FirstOrDefault(p => p.PlayerId == playerId);
        var homePitcher = game.HomeBoxScore?.Pitchers.FirstOrDefault(p => p.PlayerId == playerId);
        if (awayPitcher != null && homePitcher != null)
        {
            throw new NotImplementedException("Danny Jansen situation not implemented");
        }
        else if (awayPitcher != null)
        {
            Pitcher = new GamePitcher(awayPitcher);
        }
        else if (homePitcher != null)
        {
            Pitcher = new GamePitcher(homePitcher);
        }

        var awayFielder = game.AwayBoxScore?.Fielders.FirstOrDefault(f => f.PlayerId == playerId);
        var homeFielder = game.HomeBoxScore?.Fielders.FirstOrDefault(f => f.PlayerId == playerId);
        if (awayFielder != null && homeFielder != null)
        {
            throw new NotImplementedException("Danny Jansen situation not implemented");
        }
        else if (awayFielder != null)
        {
            Fielder = new GameFielder(awayFielder);
        }
        else if (homeFielder != null)
        {
            Fielder = new GameFielder(homeFielder);
        }
    }
}
