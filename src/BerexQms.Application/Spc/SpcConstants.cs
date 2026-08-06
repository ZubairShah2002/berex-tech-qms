namespace BerexQms.Application.Spc;

/// <summary>
/// Standard statistical process control constants (Shewhart control chart factors)
/// for subgroup sizes 2 through 25, as tabulated in Montgomery's
/// "Introduction to Statistical Quality Control".
/// </summary>
public static class SpcConstants
{
    /// <summary>
    /// A2 factor: multiplies the mean range (R-bar) to derive X-bar chart limits
    /// from an R chart (X-bar/R method).
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> A2 = new Dictionary<int, decimal>
    {
        [2] = 1.880m, [3] = 1.023m, [4] = 0.729m, [5] = 0.577m,
        [6] = 0.483m, [7] = 0.419m, [8] = 0.373m, [9] = 0.337m, [10] = 0.308m,
        [11] = 0.285m, [12] = 0.266m, [13] = 0.249m, [14] = 0.235m, [15] = 0.223m,
        [16] = 0.212m, [17] = 0.203m, [18] = 0.194m, [19] = 0.187m, [20] = 0.180m,
        [21] = 0.173m, [22] = 0.167m, [23] = 0.162m, [24] = 0.157m, [25] = 0.153m,
    };

    /// <summary>
    /// A3 factor: multiplies the mean standard deviation (S-bar) to derive X-bar
    /// chart limits from an S chart (X-bar/S method).
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> A3 = new Dictionary<int, decimal>
    {
        [2] = 2.659m, [3] = 1.954m, [4] = 1.628m, [5] = 1.427m,
        [6] = 1.287m, [7] = 1.182m, [8] = 1.099m, [9] = 1.032m, [10] = 0.975m,
        [11] = 0.927m, [12] = 0.886m, [13] = 0.850m, [14] = 0.817m, [15] = 0.789m,
        [16] = 0.763m, [17] = 0.739m, [18] = 0.718m, [19] = 0.698m, [20] = 0.680m,
        [21] = 0.663m, [22] = 0.647m, [23] = 0.633m, [24] = 0.619m, [25] = 0.606m,
    };

    /// <summary>
    /// B3 factor: lower control limit multiplier for the S chart.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> B3 = new Dictionary<int, decimal>
    {
        [2] = 0m, [3] = 0m, [4] = 0m, [5] = 0m,
        [6] = 0.030m, [7] = 0.118m, [8] = 0.185m, [9] = 0.239m, [10] = 0.284m,
        [11] = 0.321m, [12] = 0.354m, [13] = 0.382m, [14] = 0.406m, [15] = 0.428m,
        [16] = 0.448m, [17] = 0.466m, [18] = 0.482m, [19] = 0.497m, [20] = 0.510m,
        [21] = 0.523m, [22] = 0.534m, [23] = 0.545m, [24] = 0.555m, [25] = 0.565m,
    };

    /// <summary>
    /// B4 factor: upper control limit multiplier for the S chart.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> B4 = new Dictionary<int, decimal>
    {
        [2] = 3.267m, [3] = 2.568m, [4] = 2.266m, [5] = 2.089m,
        [6] = 1.970m, [7] = 1.882m, [8] = 1.815m, [9] = 1.761m, [10] = 1.716m,
        [11] = 1.679m, [12] = 1.646m, [13] = 1.618m, [14] = 1.594m, [15] = 1.572m,
        [16] = 1.552m, [17] = 1.534m, [18] = 1.518m, [19] = 1.503m, [20] = 1.490m,
        [21] = 1.477m, [22] = 1.466m, [23] = 1.455m, [24] = 1.445m, [25] = 1.435m,
    };

    /// <summary>
    /// D3 factor: lower control limit multiplier for the R chart.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> D3 = new Dictionary<int, decimal>
    {
        [2] = 0m, [3] = 0m, [4] = 0m, [5] = 0m,
        [6] = 0m, [7] = 0.076m, [8] = 0.136m, [9] = 0.184m, [10] = 0.223m,
        [11] = 0.256m, [12] = 0.283m, [13] = 0.307m, [14] = 0.328m, [15] = 0.347m,
        [16] = 0.363m, [17] = 0.378m, [18] = 0.391m, [19] = 0.403m, [20] = 0.415m,
        [21] = 0.425m, [22] = 0.434m, [23] = 0.443m, [24] = 0.451m, [25] = 0.459m,
    };

    /// <summary>
    /// D4 factor: upper control limit multiplier for the R chart.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> D4 = new Dictionary<int, decimal>
    {
        [2] = 3.267m, [3] = 2.574m, [4] = 2.282m, [5] = 2.114m,
        [6] = 2.004m, [7] = 1.924m, [8] = 1.864m, [9] = 1.816m, [10] = 1.777m,
        [11] = 1.744m, [12] = 1.717m, [13] = 1.693m, [14] = 1.672m, [15] = 1.653m,
        [16] = 1.637m, [17] = 1.622m, [18] = 1.608m, [19] = 1.597m, [20] = 1.585m,
        [21] = 1.575m, [22] = 1.566m, [23] = 1.557m, [24] = 1.548m, [25] = 1.541m,
    };

    /// <summary>
    /// d2 factor: relates the mean range (R-bar) to the process standard deviation
    /// (sigma_within = R-bar / d2). Used for X-bar/R and Individual/Moving-Range charts.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> d2 = new Dictionary<int, decimal>
    {
        [2] = 1.128m, [3] = 1.693m, [4] = 2.059m, [5] = 2.326m,
        [6] = 2.534m, [7] = 2.704m, [8] = 2.847m, [9] = 2.970m, [10] = 3.078m,
        [11] = 3.173m, [12] = 3.258m, [13] = 3.336m, [14] = 3.407m, [15] = 3.472m,
        [16] = 3.532m, [17] = 3.588m, [18] = 3.640m, [19] = 3.689m, [20] = 3.735m,
        [21] = 3.778m, [22] = 3.819m, [23] = 3.858m, [24] = 3.895m, [25] = 3.931m,
    };

    /// <summary>
    /// c4 factor: relates the mean standard deviation (S-bar) to the process standard
    /// deviation (sigma_within = S-bar / c4). Used for X-bar/S charts.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, decimal> c4 = new Dictionary<int, decimal>
    {
        [2] = 0.7979m, [3] = 0.8862m, [4] = 0.9213m, [5] = 0.9400m,
        [6] = 0.9515m, [7] = 0.9594m, [8] = 0.9650m, [9] = 0.9693m, [10] = 0.9727m,
        [11] = 0.9754m, [12] = 0.9776m, [13] = 0.9794m, [14] = 0.9810m, [15] = 0.9823m,
        [16] = 0.9835m, [17] = 0.9845m, [18] = 0.9854m, [19] = 0.9862m, [20] = 0.9869m,
        [21] = 0.9876m, [22] = 0.9882m, [23] = 0.9887m, [24] = 0.9892m, [25] = 0.9896m,
    };

    /// <summary>
    /// E2 factor for the Individual/Moving-Range chart, using a moving range window
    /// of 2 consecutive observations (n = 2 for the moving range, per standard practice).
    /// UCL = X-bar + E2 * MR-bar, LCL = X-bar - E2 * MR-bar.
    /// </summary>
    public const decimal E2 = 2.660m;

    /// <summary>
    /// d2 factor for a moving-range window of 2, used to derive the within-subgroup
    /// standard deviation for Individual/Moving-Range charts: sigma = MR-bar / 1.128.
    /// </summary>
    public const decimal MovingRangeD2 = 1.128m;

    /// <summary>
    /// Lower bound of the supported subgroup size range for tabulated factors.
    /// </summary>
    public const int MinSubgroupSize = 2;

    /// <summary>
    /// Upper bound of the supported subgroup size range for tabulated factors.
    /// </summary>
    public const int MaxSubgroupSize = 25;

    /// <summary>
    /// Clamps a subgroup size into the range covered by the tabulated factors,
    /// so lookups never fail for out-of-range subgroup sizes.
    /// </summary>
    public static int ClampSubgroupSize(int subgroupSize) =>
        Math.Clamp(subgroupSize, MinSubgroupSize, MaxSubgroupSize);
}
