using System;

namespace BaseballApi.Models;

/**
 * Represents a player in a box score, regardless of specific role. Only used as a computed property.
 */
public class BoxScorePlayer
{
    public long PlayerId => Player.Id;
    public required Player Player { get; set; }
    public long BoxScoreId => BoxScore.Id;
    public required BoxScore BoxScore { get; set; }
}
