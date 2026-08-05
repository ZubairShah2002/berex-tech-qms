using System.Globalization;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.Spc.Enums;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Commands.RecalculateLimits;

/// <summary>
/// Recalculates a control chart's control limits from its recorded data points using
/// standard Shewhart chart formulas, and — when specification limits are configured and
/// enough data exists — recalculates its process capability indices (Cp/Cpk/Pp/Ppk).
/// </summary>
internal sealed class RecalculateLimitsCommandHandler : ICommandHandler<RecalculateLimitsCommand>
{
    private const int MinDataPointsForCapability = 25;

    private readonly IControlChartRepository _repository;

    public RecalculateLimitsCommandHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RecalculateLimitsCommand request, CancellationToken cancellationToken)
    {
        var chart = await _repository.GetWithDataPointsAsync(request.ChartId, cancellationToken);
        if (chart is null)
            return Result.Failure(SpcErrors.ChartNotFound);

        var points = chart.DataPoints.OrderBy(p => p.Timestamp).ToList();
        if (points.Count == 0)
            return Result.Failure(SpcErrors.NoDataPoints);

        if (!Enum.TryParse<ChartType>(chart.ChartType, out var chartType))
            return Result.Failure(SpcErrors.InvalidChartType);

        var (ucl, cl, lcl) = CalculateControlLimits(chartType, chart.SubgroupSize, points);
        chart.SetControlLimits(ucl, cl, lcl);

        // Process capability requires specification limits and a meaningful sample.
        // Limits are always persisted; capability is computed only when feasible.
        var hasSpecLimits = chart.UpperSpecLimit.HasValue || chart.LowerSpecLimit.HasValue;
        if (hasSpecLimits && points.Count >= MinDataPointsForCapability)
        {
            var (cp, cpk, pp, ppk, mean, stdDev) = CalculateCapability(
                chartType, chart.SubgroupSize, points, chart.UpperSpecLimit, chart.LowerSpecLimit);

            chart.RecalculateCapability(cp, cpk, pp, ppk, mean, stdDev, points.Count);
        }

        return Result.Success();
    }

    // ---- Control limit calculations -----------------------------------------------------

    private static (decimal Ucl, decimal Cl, decimal Lcl) CalculateControlLimits(
        ChartType chartType, int subgroupSize, IReadOnlyList<DataPoint> points)
    {
        return chartType switch
        {
            ChartType.XBarR => CalculateXBarR(subgroupSize, points),
            ChartType.XBarS => CalculateXBarS(subgroupSize, points),
            ChartType.IndividualMovingRange => CalculateIndividualMovingRange(points),
            ChartType.PChart => CalculatePChart(points),
            ChartType.NpChart => CalculateNpChart(points),
            ChartType.CChart => CalculateCChart(points),
            ChartType.UChart => CalculateUChart(points),
            _ => throw new ArgumentOutOfRangeException(nameof(chartType), chartType, "Unsupported chart type."),
        };
    }

    private static (decimal, decimal, decimal) CalculateXBarR(int subgroupSize, IReadOnlyList<DataPoint> points)
    {
        var subgroupMeans = new List<decimal>();
        var ranges = new List<decimal>();

        foreach (var point in points)
        {
            var raw = ParseSubgroupValues(point.SubgroupValues);
            if (raw.Length >= 2)
            {
                subgroupMeans.Add(raw.Average());
                ranges.Add(raw.Max() - raw.Min());
            }
            else
            {
                // No raw subgroup values recorded — fall back to the point's own value
                // and treat its contribution to the range as zero.
                subgroupMeans.Add(point.Value);
                ranges.Add(0m);
            }
        }

        var grandMean = subgroupMeans.Average();
        var meanRange = ranges.Average();
        var a2 = SpcConstants.A2[SpcConstants.ClampSubgroupSize(subgroupSize)];

        return (grandMean + a2 * meanRange, grandMean, grandMean - a2 * meanRange);
    }

    private static (decimal, decimal, decimal) CalculateXBarS(int subgroupSize, IReadOnlyList<DataPoint> points)
    {
        var subgroupMeans = new List<decimal>();
        var stdDevs = new List<decimal>();

        foreach (var point in points)
        {
            var raw = ParseSubgroupValues(point.SubgroupValues);
            if (raw.Length >= 2)
            {
                var mean = raw.Average();
                subgroupMeans.Add(mean);
                stdDevs.Add(SampleStdDev(raw, mean));
            }
            else
            {
                subgroupMeans.Add(point.Value);
                stdDevs.Add(0m);
            }
        }

        var grandMean = subgroupMeans.Average();
        var meanStdDev = stdDevs.Average();
        var a3 = SpcConstants.A3[SpcConstants.ClampSubgroupSize(subgroupSize)];

        return (grandMean + a3 * meanStdDev, grandMean, grandMean - a3 * meanStdDev);
    }

    private static (decimal, decimal, decimal) CalculateIndividualMovingRange(IReadOnlyList<DataPoint> points)
    {
        var values = points.Select(p => p.Value).ToList();
        var mean = values.Average();
        var meanMovingRange = MeanMovingRange(values);

        return (mean + SpcConstants.E2 * meanMovingRange, mean, mean - SpcConstants.E2 * meanMovingRange);
    }

    private static (decimal, decimal, decimal) CalculatePChart(IReadOnlyList<DataPoint> points)
    {
        var totalDefectives = points.Sum(p => p.Value * p.SampleSize);
        var totalSampled = points.Sum(p => (decimal)p.SampleSize);
        var pBar = totalSampled > 0 ? totalDefectives / totalSampled : 0m;
        var meanSampleSize = points.Average(p => (decimal)p.SampleSize);

        var sigma = meanSampleSize > 0
            ? DecimalSqrt(pBar * (1 - pBar) / meanSampleSize)
            : 0m;

        return (pBar + 3 * sigma, pBar, Math.Max(0m, pBar - 3 * sigma));
    }

    private static (decimal, decimal, decimal) CalculateNpChart(IReadOnlyList<DataPoint> points)
    {
        var meanDefective = points.Average(p => p.Value);
        var meanSampleSize = points.Average(p => (decimal)p.SampleSize);
        var pBar = meanSampleSize > 0 ? meanDefective / meanSampleSize : 0m;

        var sigma = DecimalSqrt(meanSampleSize * pBar * (1 - pBar));

        return (meanDefective + 3 * sigma, meanDefective, Math.Max(0m, meanDefective - 3 * sigma));
    }

    private static (decimal, decimal, decimal) CalculateCChart(IReadOnlyList<DataPoint> points)
    {
        var cBar = points.Average(p => p.Value);
        var sigma = DecimalSqrt(cBar);

        return (cBar + 3 * sigma, cBar, Math.Max(0m, cBar - 3 * sigma));
    }

    private static (decimal, decimal, decimal) CalculateUChart(IReadOnlyList<DataPoint> points)
    {
        var totalDefects = points.Sum(p => p.Value * p.SampleSize);
        var totalUnits = points.Sum(p => (decimal)p.SampleSize);
        var uBar = totalUnits > 0 ? totalDefects / totalUnits : 0m;
        var meanSampleSize = points.Average(p => (decimal)p.SampleSize);

        var sigma = meanSampleSize > 0 ? DecimalSqrt(uBar / meanSampleSize) : 0m;

        return (uBar + 3 * sigma, uBar, Math.Max(0m, uBar - 3 * sigma));
    }

    // ---- Process capability calculations -------------------------------------------------

    private static (decimal Cp, decimal Cpk, decimal Pp, decimal Ppk, decimal Mean, decimal StdDev) CalculateCapability(
        ChartType chartType, int subgroupSize, IReadOnlyList<DataPoint> points, decimal? usl, decimal? lsl)
    {
        var values = points.Select(p => p.Value).ToList();
        var overallMean = values.Average();
        var overallStdDev = SampleStdDev(values, overallMean);

        // Short-term (within-subgroup) sigma estimate for Cp/Cpk. Falls back to the
        // overall (long-term) sigma for attribute charts, where no subgroup-based
        // within estimate applies.
        var withinStdDev = CalculateWithinStdDev(chartType, subgroupSize, points) ?? overallStdDev;

        var pp = CalculateCapabilityIndex(usl, lsl, overallMean, overallStdDev, isCp: true);
        var ppk = CalculateCapabilityIndex(usl, lsl, overallMean, overallStdDev, isCp: false);
        var cp = CalculateCapabilityIndex(usl, lsl, overallMean, withinStdDev, isCp: true);
        var cpk = CalculateCapabilityIndex(usl, lsl, overallMean, withinStdDev, isCp: false);

        return (cp, cpk, pp, ppk, overallMean, overallStdDev);
    }

    private static decimal? CalculateWithinStdDev(ChartType chartType, int subgroupSize, IReadOnlyList<DataPoint> points)
    {
        switch (chartType)
        {
            case ChartType.XBarR:
            {
                var ranges = points
                    .Select(p => ParseSubgroupValues(p.SubgroupValues))
                    .Where(raw => raw.Length >= 2)
                    .Select(raw => raw.Max() - raw.Min())
                    .ToList();

                if (ranges.Count == 0)
                    return null;

                var d2 = SpcConstants.d2[SpcConstants.ClampSubgroupSize(subgroupSize)];
                return ranges.Average() / d2;
            }

            case ChartType.XBarS:
            {
                var stdDevs = points
                    .Select(p => ParseSubgroupValues(p.SubgroupValues))
                    .Where(raw => raw.Length >= 2)
                    .Select(raw => SampleStdDev(raw, raw.Average()))
                    .ToList();

                if (stdDevs.Count == 0)
                    return null;

                var c4 = SpcConstants.c4[SpcConstants.ClampSubgroupSize(subgroupSize)];
                return stdDevs.Average() / c4;
            }

            case ChartType.IndividualMovingRange:
            {
                var values = points.Select(p => p.Value).ToList();
                if (values.Count < 2)
                    return null;

                return MeanMovingRange(values) / SpcConstants.MovingRangeD2;
            }

            default:
                // Attribute charts (P/Np/C/U) have no subgroup-based within-estimate;
                // the caller falls back to the overall sigma.
                return null;
        }
    }

    private static decimal CalculateCapabilityIndex(
        decimal? usl, decimal? lsl, decimal mean, decimal stdDev, bool isCp)
    {
        if (stdDev <= 0m)
            return 0m;

        if (isCp)
        {
            if (usl.HasValue && lsl.HasValue)
                return (usl.Value - lsl.Value) / (6m * stdDev);

            // Cp is only formally defined two-sided; fall back to the one-sided
            // capability index when only a single spec limit is configured.
            if (usl.HasValue)
                return (usl.Value - mean) / (3m * stdDev);

            return lsl.HasValue ? (mean - lsl.Value) / (3m * stdDev) : 0m;
        }

        decimal? upperIndex = usl.HasValue ? (usl.Value - mean) / (3m * stdDev) : null;
        decimal? lowerIndex = lsl.HasValue ? (mean - lsl.Value) / (3m * stdDev) : null;

        if (upperIndex.HasValue && lowerIndex.HasValue)
            return Math.Min(upperIndex.Value, lowerIndex.Value);

        return upperIndex ?? lowerIndex ?? 0m;
    }

    // ---- Shared statistical helpers ------------------------------------------------------

    private static decimal[] ParseSubgroupValues(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => decimal.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static decimal SampleStdDev(IReadOnlyCollection<decimal> values, decimal mean)
    {
        if (values.Count < 2)
            return 0m;

        var sumSquares = values.Sum(v => (v - mean) * (v - mean));
        var variance = sumSquares / (values.Count - 1);
        return DecimalSqrt(variance);
    }

    private static decimal MeanMovingRange(IReadOnlyList<decimal> orderedValues)
    {
        if (orderedValues.Count < 2)
            return 0m;

        var movingRanges = new List<decimal>(orderedValues.Count - 1);
        for (var i = 1; i < orderedValues.Count; i++)
            movingRanges.Add(Math.Abs(orderedValues[i] - orderedValues[i - 1]));

        return movingRanges.Average();
    }

    private static decimal DecimalSqrt(decimal value)
    {
        if (value <= 0m)
            return 0m;

        return (decimal)Math.Sqrt((double)value);
    }
}
