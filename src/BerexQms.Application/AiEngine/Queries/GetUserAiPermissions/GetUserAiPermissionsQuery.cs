using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetUserAiPermissions;

public sealed record GetUserAiPermissionsQuery(Guid UserId) : IQuery<AiUserPermissionsDto>;
