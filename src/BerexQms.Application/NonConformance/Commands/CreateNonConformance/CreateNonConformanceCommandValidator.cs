using BerexQms.Domain.NonConformance.Enums;
using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.CreateNonConformance;

public sealed class CreateNonConformanceCommandValidator : AbstractValidator<CreateNonConformanceCommand>
{
    public CreateNonConformanceCommandValidator()
    {
        RuleFor(x => x.NcrNumber)
            .NotEmpty().WithMessage("NCR number is required.")
            .MaximumLength(50).WithMessage("NCR number cannot exceed 50 characters.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .Must(v => Enum.TryParse<NCSeverity>(v, true, out _))
            .WithMessage("Invalid severity. Valid values: Minor, Major, Critical.");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Source is required.")
            .Must(v => Enum.TryParse<NCSource>(v, true, out _))
            .WithMessage("Invalid source. Valid values: Inspection, LineFinding, CustomerComplaint, AuditFinding, SupplierNotification.");

        RuleFor(x => x.DetectionPoint)
            .NotEmpty().WithMessage("Detection point is required.")
            .Must(v => Enum.TryParse<DetectionPoint>(v, true, out _))
            .WithMessage("Invalid detection point. Valid values: IncomingInspection, InProcess, FinalInspection, CustomerSite, FieldReturn.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");

        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.QuantityAffected)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity affected cannot be negative.");

        RuleFor(x => x.QuantityDefective)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity defective cannot be negative.")
            .LessThanOrEqualTo(x => x.QuantityAffected)
            .WithMessage("Quantity defective cannot exceed quantity affected.");

        RuleFor(x => x.LotNumber)
            .MaximumLength(100).WithMessage("Lot number cannot exceed 100 characters.")
            .When(x => x.LotNumber is not null);

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100).WithMessage("Serial number cannot exceed 100 characters.")
            .When(x => x.SerialNumber is not null);

        RuleFor(x => x.SupplierLotNumber)
            .MaximumLength(100).WithMessage("Supplier lot number cannot exceed 100 characters.")
            .When(x => x.SupplierLotNumber is not null);

        RuleFor(x => x.WorkOrderNumber)
            .MaximumLength(100).WithMessage("Work order number cannot exceed 100 characters.")
            .When(x => x.WorkOrderNumber is not null);
    }
}
