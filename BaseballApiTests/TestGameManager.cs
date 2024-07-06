using BaseballApi;
using BaseballApi.Models;

namespace BaseballApiTests;

public class TestGameManager
{
    private BaseballContext Context { get; }
    private Player Batter1 { get; }
    private Player Batter2 { get; }
    private Player Batter3 { get; }
    private Player Pitcher1 { get; }
    private Player Pitcher2 { get; }

    public TestGameManager(BaseballContext context)
    {
        Context = context;
        Batter1 = context.Players.First(p => p.Name == "Test Batter 1");
        Batter2 = context.Players.First(p => p.Name == "Test Batter 2");
        Batter3 = context.Players.First(p => p.Name == "Test Batter 3");
        Pitcher1 = context.Players.First(p => p.Name == "Test Pitcher 1");
        Pitcher2 = context.Players.First(p => p.Name == "Test Pitcher 2");
    }

    public void AddBoxScore(int gameId, bool home)
    {
        var boxScore = Context.BoxScores.First(bs => bs.GameId == gameId);

        Context.AddRange(
            new Batter
            {
                BoxScore = boxScore,
                Player = Batter1,
                Games = 1,
                AtBats = 3,
                Hits = 1
            }
        );

        Context.SaveChanges();
    }
}
