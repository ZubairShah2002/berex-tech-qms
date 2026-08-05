using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.UpdateEquipment;

internal sealed class UpdateEquipmentCommandHandler
    : ICommandHandler<UpdateEquipmentCommand>
{
    private readonly IEquipmentRepository _repository;

    public UpdateEquipmentCommandHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return Result.Failure(CalibrationErrors.EquipmentNotFound);

        equipment.UpdateDetails(
            request.Name,
            request.Type,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.Location,
            request.Department,
            request.Area,
            request.CustodianId);

        return Result.Success();
    }
}
