using System;

namespace BaseballApiTests;

public class TestImportDatabaseFixture : BaseTestImportDatabaseFixture, IDisposable
{
    public TestImportDatabaseFixture() : base(nameof(TestImportDatabaseFixture)) { }
}
