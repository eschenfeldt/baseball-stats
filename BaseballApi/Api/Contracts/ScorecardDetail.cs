using BaseballApi.Models;

namespace BaseballApi.Contracts;

public readonly struct ScorecardDetail(Scorecard scorecard)
{
    public RemoteFileDetail File { get; } = new(scorecard.Files.Single(f => f.Purpose == RemoteFilePurpose.Original));
}
