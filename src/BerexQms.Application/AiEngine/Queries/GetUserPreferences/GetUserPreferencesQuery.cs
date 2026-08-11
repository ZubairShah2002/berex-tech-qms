using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Queries.GetUserPreferences;

/// <summary>
/// Get the current user's AI preferences.
/// Returns defaults if no explicit preference exists.
/// </summary>
public sealed record GetUserPreferencesQuery() : IQuery<AiUserPreferenceDto>;
