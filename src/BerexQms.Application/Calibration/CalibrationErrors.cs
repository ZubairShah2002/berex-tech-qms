using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration;

public static class CalibrationErrors
{
    public static readonly Error EquipmentNotFound = Error.NotFound(
        "Calibration.EquipmentNotFound", "Equipment not found.");

    public static readonly Error CodeExists = Error.Conflict(
        "Calibration.CodeExists", "An equipment record with this code already exists.");

    public static readonly Error AssessmentNotFound = Error.NotFound(
        "Calibration.AssessmentNotFound", "Impact assessment not found.");

    public static readonly Error InvalidStatus = Error.Validation(
        "Calibration.InvalidStatus", "The operation is not valid for the current equipment status.");
}
