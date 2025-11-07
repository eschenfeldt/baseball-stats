using System.Text.Json.Serialization;

namespace BaseballApi.Contracts;

public record struct FangraphsPlayerSearchResult(
    [property: JsonPropertyName("hits")] IReadOnlyList<FangraphsPlayer> Hits,
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("processingTimeMs")] int ProcessingTimeMs,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("estimatedTotalHits")] int EstimatedTotalHits
);
