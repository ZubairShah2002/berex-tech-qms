using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Calibration.Entities;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.RegisterEquipment;

internal sealed class RegisterEquipmentCommandHandler
    : ICommandHandler<RegisterEquipmentCommand, Guid>
{
    private readonly IEquipmentRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterEquipmentCommandHandler(IEquipmentRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(RegisterEquipmentCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return CalibrationErrors.CodeExists;

        var equipment = Equipment.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Code,
            request.Name,
            request.Type,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.Location,
            request.Department,
            request.Area,
            request.CustodianId);

        await _repository.AddAsync(equipment, cancellationToken);

        return equipment.Id;
    }
}
