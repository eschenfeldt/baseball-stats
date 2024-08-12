using System;

namespace BaseballApi.Contracts;

public struct ThumbnailParams : PagedApiParameters
{
    public long? GameId { get; set; }
    public long? PlayerId { get; set; }

    public int? Skip { get; set; }
    public int? Take { get; set; }
    public string? Sort { get; set; }
    public bool? Asc { get; set; }
}
