using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.UpdateEquipment;

public sealed class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
{
    public UpdateEquipmentCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).MaximumLength(100);
        RuleFor(x => x.Manufacturer).MaximumLength(200);
        RuleFor(x => x.Model).MaximumLength(200);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Department).MaximumLength(200);
        RuleFor(x => x.Area).MaximumLength(200);
    }
}
