using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection;

public static class InspectionErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Inspection.NotFound", "The specified inspection was not found.");

    public static readonly Error InspectionNumberExists = Error.Conflict(
        "Inspection.NumberExists", "An inspection with this number already exists.");

    public static readonly Error SamplingPlanNotFound = Error.NotFound(
        "Inspection.SamplingPlanNotFound", "The specified sampling plan was not found.");

    public static readonly Error PartNotFound = Error.NotFound(
        "Inspection.PartNotFound", "The specified part was not found.");

    public static readonly Error InvalidInspectionType = Error.Validation(
        "Inspection.InvalidType", "Invalid inspection type.");

    public static readonly Error InvalidDispositionType = Error.Validation(
        "Inspection.InvalidDispositionType", "Invalid disposition type.");

    public static readonly Error InvalidSamplingLevel = Error.Validation(
        "Inspection.InvalidSamplingLevel", "Invalid sampling level.");

    public static readonly Error InvalidMeasurementResult = Error.Validation(
        "Inspection.InvalidMeasurementResult", "Invalid measurement result.");
}
