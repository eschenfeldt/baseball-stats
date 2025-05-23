using System;
using BaseballApi;
using BaseballApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseballApiTests;

public class ConstantTests : BaseballTests
{
    private ConstantsController Controller { get; }

    public ConstantTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new ConstantsController(Context);
    }

    [Fact]
    public async void TestRefreshConstants()
    {
        var filePath = Path.Join("data", "FanGraphs Leaderboard.csv");
        var fileName = Path.GetFileName(filePath);
        using var fileStream = File.OpenRead(filePath);
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName);

        var maxYear = await Controller.RefreshFangraphsConstants(formFile);

        Assert.Equal(2024, maxYear);

        var c1989 = await Context.Constants.FirstOrDefaultAsync(y => y.Year == 1989);
        Assert.NotNull(c1989);
        Assert.Equal(1989, c1989.Year);
        Assert.Equal(2.763m, c1989.CFIP);
        Assert.Equal(0.910m, c1989.W1B);
    }
}
