using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.RegisterEquipment;

public sealed class RegisterEquipmentCommandValidator : AbstractValidator<RegisterEquipmentCommand>
{
    public RegisterEquipmentCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
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
