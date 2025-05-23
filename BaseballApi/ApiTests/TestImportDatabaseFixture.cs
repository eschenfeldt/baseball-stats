using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseballApiTests;

public class TestImportDatabaseFixture : IDisposable
{
    private static readonly object _lock = new();
    private static bool _dbInitialized;
    private string DbName { get; }

    public TestImportDatabaseFixture()
    {
        DbName = $"BaseballImportUnitTest{Guid.NewGuid()}";
        lock (_lock)
        {
            if (!_dbInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
                _dbInitialized = true;
            }
        }
    }

    public BaseballContext CreateContext()
    {
        var configPath = Path.Join("/", "run", "secrets", "app_settings");
        var builder = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true)
            .AddUserSecrets<TestImportDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        var ownerConnectionString = configuration["Baseball:OwnerConnectionString"];
        // separate database for each fixture
        ownerConnectionString = ownerConnectionString?.Replace(
            "BaseballUnitTest",
            DbName
        );
        var options = new DbContextOptionsBuilder<BaseballContext>()
                                    .UseNpgsql(ownerConnectionString)
                                    .Options;
        return new BaseballContext(options);
    }

    public void Dispose()
    {
        if (_dbInitialized)
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }
    }
}
