namespace BaseballApi;

public struct GameMetadata
{
    public Team Home { get; set; }
    public Team Away { get; set; }
    public DateTimeOffset? ScheduledStart { get; set; }
    public DateTimeOffset? ActualStart { get; set; }
    public DateTimeOffset? End { get; set; }
}
