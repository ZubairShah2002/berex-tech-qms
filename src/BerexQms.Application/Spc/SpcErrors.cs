using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc;

public static class SpcErrors
{
    public static readonly Error ChartNotFound = Error.NotFound(
        "Spc.ChartNotFound", "Control chart not found.");

    public static readonly Error ChartCodeExists = Error.Conflict(
        "Spc.ChartCodeExists", "A control chart with this code already exists.");

    public static readonly Error ChartInactive = Error.Validation(
        "Spc.ChartInactive", "The control chart is not active.");

    public static readonly Error NoDataPoints = Error.Validation(
        "Spc.NoDataPoints", "The control chart has no data points.");

    public static readonly Error InsufficientDataForCapability = Error.Validation(
        "Spc.InsufficientDataForCapability",
        "At least 25 data points are required to calculate process capability.");

    public static readonly Error SpecLimitsRequiredForCapability = Error.Validation(
        "Spc.SpecLimitsRequiredForCapability",
        "Upper and/or lower specification limits must be set to calculate process capability.");

    public static readonly Error InvalidSubgroupSize = Error.Validation(
        "Spc.InvalidSubgroupSize", "Subgroup size is invalid for the selected chart type.");

    public static readonly Error InvalidChartType = Error.Validation(
        "Spc.InvalidChartType", "The specified chart type is not a recognized SPC chart type.");
}
