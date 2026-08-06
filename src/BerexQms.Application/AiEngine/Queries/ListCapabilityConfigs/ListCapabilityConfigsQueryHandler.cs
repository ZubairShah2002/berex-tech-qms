using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListCapabilityConfigs;

internal sealed class ListCapabilityConfigsQueryHandler
    : IQueryHandler<ListCapabilityConfigsQuery, IReadOnlyList<AiCapabilityConfigDto>>
{
    private readonly IAiCapabilityConfigRepository _repository;

    public ListCapabilityConfigsQueryHandler(IAiCapabilityConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AiCapabilityConfigDto>>> Handle(
        ListCapabilityConfigsQuery request, CancellationToken cancellationToken)
    {
        var configs = await _repository.GetAllConfigsAsync(cancellationToken);

        var dtos = configs
            .Select(c => new AiCapabilityConfigDto(
                c.Id,
                c.Capability,
                c.IsEnabled,
                c.LowConfidenceThreshold,
                c.ModerateConfidenceThreshold,
                c.HighConfidenceThreshold))
            .ToList();

        return Result.Success<IReadOnlyList<AiCapabilityConfigDto>>(dtos);
    }
}
