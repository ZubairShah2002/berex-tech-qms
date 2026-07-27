using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Application.NonConformance.DTOs;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.AddContainmentAction;

public sealed class AddContainmentActionCommandHandler
    : ICommandHandler<AddContainmentActionCommand, ContainmentActionDto>
{
    private readonly INonConformanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public AddContainmentActionCommandHandler(
        INonConformanceRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ContainmentActionDto>> Handle(
        AddContainmentActionCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithContainmentsAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return NonConformanceErrors.NotFound;

        var action = record.AddContainmentAction(request.Description, _currentUserService.Email);
        await _repository.UpdateAsync(record, cancellationToken);

        return new ContainmentActionDto(
            action.Id,
            action.Description,
            action.ActionTakenBy,
            action.ActionTakenAt,
            action.IsVerified,
            action.VerifiedBy,
            action.VerifiedAt);
    }
}
