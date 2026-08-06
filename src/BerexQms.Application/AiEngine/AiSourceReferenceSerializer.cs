using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine;

/// <summary>
/// <see cref="Domain.AiEngine.Entities.AiInteraction"/> persists its source references as
/// an opaque string (the domain has no dependency on a serialization format). This helper
/// owns the Application-layer convention — a JSON array of source reference objects — used
/// to round-trip <see cref="AiSourceReferenceDto"/> lists through that string column.
/// </summary>
internal static class AiSourceReferenceSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string? Serialize(IReadOnlyCollection<AiSourceReferenceDto> sourceReferences)
    {
        return sourceReferences.Count == 0
            ? null
            : JsonSerializer.Serialize(sourceReferences, Options);
    }

    public static IReadOnlyList<AiSourceReferenceDto> Deserialize(string? sourceReferences)
    {
        if (string.IsNullOrWhiteSpace(sourceReferences))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<AiSourceReferenceDto>>(sourceReferences, Options)
                   ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
