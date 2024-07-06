using BaseballApi.Models;

namespace BaseballApiTests;

public abstract class BaseballTests : IClassFixture<TestDatabaseFixture>, IDisposable
{
    protected BaseballContext Context { get; }
    protected TestDatabaseFixture Fixture { get; }
    protected BaseballTests(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
        Context = Fixture.CreateContext();
        Context.Database.BeginTransaction(); // allow changes without persisting to the db
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}