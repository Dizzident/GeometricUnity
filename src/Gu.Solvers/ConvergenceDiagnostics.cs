namespace Gu.Solvers;

/// <summary>
/// Tracks convergence history and detects stagnation.
/// Provides diagnostics on objective reduction rate,
/// gradient norm trends, and gauge penalty magnitude.
/// </summary>
public sealed class ConvergenceDiagnostics
{
    private readonly List<ConvergenceRecord> _history = new();
    private readonly int _stagnationWindow;
    private readonly double _stagnationThreshold;

    /// <summary>
    /// Create convergence diagnostics tracker.
    /// </summary>
    /// <param name="stagnationWindow">
    /// Number of consecutive iterations with insufficient objective change
    /// before stagnation is declared. Default: 5.
    /// </param>
    /// <param name="stagnationThreshold">
    /// Minimum relative objective change required to avoid stagnation.
    /// Default: 1e-12.
    /// </param>
    public ConvergenceDiagnostics(int stagnationWindow = 5, double stagnationThreshold = 1e-12)
    {
        if (stagnationWindow < 1)
            throw new ArgumentOutOfRangeException(nameof(stagnationWindow), "Must be at least 1.");
        if (stagnationThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(stagnationThreshold), "Must be non-negative.");

        _stagnationWindow = stagnationWindow;
        _stagnationThreshold = stagnationThreshold;
    }

    /// <summary>Full convergence history.</summary>
    public IReadOnlyList<ConvergenceRecord> History => _history;

    /// <summary>Objective values over iterations.</summary>
    public IReadOnlyList<double> ObjectiveHistory => _history.Select(r => r.Objective).ToList();

    /// <summary>Gradient norms over iterations.</summary>
    public IReadOnlyList<double> GradientNormHistory => _history.Select(r => r.GradientNorm).ToList();

    /// <summary>Gauge violation values over iterations.</summary>
    public IReadOnlyList<double> GaugeViolationHistory => _history.Select(r => r.GaugeViolation).ToList();

    /// <summary>
    /// Record a new iteration's convergence data.
    /// </summary>
    public void Record(ConvergenceRecord record)
    {
        _history.Add(record);
    }

    /// <summary>
    /// Detect whether the solver has stagnated.
    /// Stagnation is defined as the objective changing by less than
    /// the stagnation threshold (relative) for the last N consecutive iterations.
    /// </summary>
    public bool IsStagnated()
    {
        if (_history.Count < _stagnationWindow + 1)
            return false;

        int start = _history.Count - _stagnationWindow;
        double referenceObjective = _history[start - 1].Objective;

        if (referenceObjective == 0)
        {
            // If the reference objective is exactly zero, check if all recent
            // objectives are also essentially zero.
            for (int i = start; i < _history.Count; i++)
            {
                if (System.Math.Abs(_history[i].Objective) > _stagnationThreshold)
                    return false;
            }
            return true;
        }

        for (int i = start; i < _history.Count; i++)
        {
            double relativeChange = System.Math.Abs(_history[i].Objective - referenceObjective) / System.Math.Abs(referenceObjective);
            if (relativeChange > _stagnationThreshold)
                return false;
            referenceObjective = _history[i].Objective;
        }

        return true;
    }

    /// <summary>
    /// Compute the objective reduction ratio: last / first.
    /// Returns 1.0 if no history exists.
    /// </summary>
    public double ObjectiveReductionRatio()
    {
        if (_history.Count < 2) return 1.0;

        double first = _history[0].Objective;
        double last = _history[^1].Objective;

        if (first == 0) return last == 0 ? 1.0 : double.PositiveInfinity;
        return last / first;
    }

    /// <summary>
    /// Get a summary of the current convergence state.
    /// </summary>
    public ConvergenceSummary GetSummary()
    {
        if (_history.Count == 0)
        {
            return new ConvergenceSummary
            {
                TotalIterations = 0,
                InitialObjective = 0,
                FinalObjective = 0,
                ObjectiveReductionRatio = 1.0,
                FinalGradientNorm = 0,
                FinalGaugeViolation = 0,
                IsStagnated = false,
                StagnationDetectedAtIteration = null,
            };
        }

        int? stagnationIter = null;
        if (IsStagnated())
        {
            stagnationIter = _history.Count - _stagnationWindow;
        }

        return new ConvergenceSummary
        {
            TotalIterations = _history.Count,
            InitialObjective = _history[0].Objective,
            FinalObjective = _history[^1].Objective,
            ObjectiveReductionRatio = ObjectiveReductionRatio(),
            FinalGradientNorm = _history[^1].GradientNorm,
            FinalGaugeViolation = _history[^1].GaugeViolation,
            IsStagnated = IsStagnated(),
            StagnationDetectedAtIteration = stagnationIter,
        };
    }

    /// <summary>
    /// Generate diagnostic log lines for the convergence history.
    /// </summary>
    public List<string> GenerateLog()
    {
        var log = new List<string>();
        log.Add($"Convergence diagnostics: {_history.Count} iterations recorded");

        foreach (var record in _history)
        {
            log.Add(
                $"  iter={record.Iteration}: " +
                $"I2={record.Objective:E6}, " +
                $"||Upsilon||={record.ResidualNorm:E6}, " +
                $"||grad||={record.GradientNorm:E6}, " +
                $"gauge={record.GaugeViolation:E6}, " +
                $"step={record.StepSize:E4}");
        }

        var summary = GetSummary();
        log.Add($"Objective reduction: {summary.ObjectiveReductionRatio:E6}");
        if (summary.IsStagnated)
        {
            log.Add($"WARNING: Stagnation detected at iteration {summary.StagnationDetectedAtIteration}");
        }

        return log;
    }
}

/// <summary>
/// Summary of convergence state.
/// </summary>
public sealed class ConvergenceSummary
{
    /// <summary>Total iterations recorded.</summary>
    public required int TotalIterations { get; init; }

    /// <summary>Initial objective value.</summary>
    public required double InitialObjective { get; init; }

    /// <summary>Final objective value.</summary>
    public required double FinalObjective { get; init; }

    /// <summary>Ratio: final/initial objective.</summary>
    public required double ObjectiveReductionRatio { get; init; }

    /// <summary>Final gradient norm.</summary>
    public required double FinalGradientNorm { get; init; }

    /// <summary>Final gauge violation.</summary>
    public required double FinalGaugeViolation { get; init; }

    /// <summary>Whether stagnation was detected.</summary>
    public required bool IsStagnated { get; init; }

    /// <summary>Iteration at which stagnation was detected (null if not stagnated).</summary>
    public required int? StagnationDetectedAtIteration { get; init; }
}
