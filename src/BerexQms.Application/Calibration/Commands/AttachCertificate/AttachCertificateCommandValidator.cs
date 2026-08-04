using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.AttachCertificate;

public sealed class AttachCertificateCommandValidator : AbstractValidator<AttachCertificateCommand>
{
    public AttachCertificateCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.CalibrationId).NotEmpty();
        RuleFor(x => x.IssuingLab).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AccreditationRef).MaximumLength(200);
        RuleFor(x => x.FileRef).MaximumLength(500);
        RuleFor(x => x.ValidUntil).GreaterThan(x => x.ValidFrom);
    }
}
