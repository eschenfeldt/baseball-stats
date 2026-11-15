namespace BaseballApiTests;

/// <summary>
/// Names for xUnit collections. Test classes in the same collection never run in parallel.
/// </summary>
public static class TestCollections
{
    /// <summary>
    /// Classes that write team MLBAM ids to the shared test db (via UpdateTeamReferences).
    /// </summary>
    public const string MLBAMTeamReferences = "MLBAM team references";
}
