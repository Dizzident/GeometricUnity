using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Continuation;

/// <summary>
/// Callback for computing the residual G(u, lambda) at a given (state, parameter) pair.
/// Returns the residual vector and its norm.
/// </summary>
public delegate (FieldTensor residual, double norm) ResidualEvaluator(
    FieldTensor state, double lambda);

/// <summary>
/// Callback for correcting a predicted state to satisfy G(u, lambda) = 0.
/// Returns the corrected state and convergence info.
/// </summary>
public delegate (FieldTensor correctedState, int iterations, bool converged) CorrectorSolver(
    FieldTensor predictedState, double lambda, double tolerance, int maxIterations);

/// <summary>
/// Pseudo-arclength continuation runner.
/// Per IMPLEMENTATION_PLAN_P2.md Section 10.2.
///
/// Tracks a solution family G(u, lambda) = 0 using predictor-corrector steps.
/// The caller provides the residual evaluator and corrector solver as delegates,
/// keeping the runner independent of specific physics implementations.
/// </summary>
public sealed class ContinuationRunner
{
    private readonly ResidualEvaluator _evaluateResidual;
    private readonly CorrectorSolver _solveCorrector;
    private readonly ISpectrumProbe? _spectrumProbe;
    private readonly ILinearOperator? _hessianProvider;
    private readonly Func<double[], bool>? _extractorDelegate;
    private readonly Func<double[], double>? _gaugeResidualNormDelegate;

    /// <summary>
    /// Create a continuation runner.
    /// </summary>
    /// <param name="evaluateResidual">Evaluates G(u, lambda) and returns (residual, norm).</param>
    /// <param name="solveCorrector">Corrects a predicted state to satisfy G ≈ 0.</param>
    /// <param name="spectrumProbe">Optional spectrum probe for stability diagnostics.</param>
    /// <param name="hessianProvider">Optional Hessian operator for event detection.</param>
    /// <param name="extractorDelegate">Optional extractor: returns false if extraction fails for the given state coefficients.</param>
    /// <param name="gaugeResidualNormDelegate">Optional gauge residual norm evaluator for gauge-slice breakdown detection.</param>
    public ContinuationRunner(
        ResidualEvaluator evaluateResidual,
        CorrectorSolver solveCorrector,
        ISpectrumProbe? spectrumProbe = null,
        ILinearOperator? hessianProvider = null,
        Func<double[], bool>? extractorDelegate = null,
        Func<double[], double>? gaugeResidualNormDelegate = null)
    {
        _evaluateResidual = evaluateResidual ?? throw new ArgumentNullException(nameof(evaluateResidual));
        _solveCorrector = solveCorrector ?? throw new ArgumentNullException(nameof(solveCorrector));
        _spectrumProbe = spectrumProbe;
        _hessianProvider = hessianProvider;
        _extractorDelegate = extractorDelegate;
        _gaugeResidualNormDelegate = gaugeResidualNormDelegate;
    }

    /// <summary>
    /// Run continuation from an initial state.
    /// </summary>
    public ContinuationResult Run(ContinuationSpec spec, FieldTensor initialState)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(initialState);

        var steps = new List<ContinuationStep>();
        var allEvents = new List<ContinuationEvent>();
        double lambda = spec.LambdaStart;
        double stepSize = spec.InitialStepSize;
        double arclength = 0;
        var currentState = initialState;
        string terminationReason = "max-steps";
        double direction = spec.LambdaEnd >= spec.LambdaStart ? 1.0 : -1.0;

        // Record initial step
        var (initResidual, initNorm) = _evaluateResidual(currentState, lambda);
        steps.Add(MakeStep(0, lambda, arclength, stepSize, initNorm, 0, true,
            Array.Empty<ContinuationEvent>()));

        int rejectionCount = 0;
        double? prevSmallestEigenvalue = null;
        double[]? prevTangent = null;

        for (int step = 1; step <= spec.MaxSteps; step++)
        {
            // Predict next parameter value
            double nextLambda = lambda + direction * stepSize;

            // Check if we've reached the end
            if ((direction > 0 && nextLambda > spec.LambdaEnd) ||
                (direction < 0 && nextLambda < spec.LambdaEnd))
            {
                nextLambda = spec.LambdaEnd;
            }

            // Corrector: solve G(u, nextLambda) ≈ 0 starting from current state
            var (corrected, correctorIters, converged) = _solveCorrector(
                currentState, nextLambda, spec.CorrectorTolerance, spec.MaxCorrectorIterations);

            var stepEvents = new List<ContinuationEvent>();

            if (!converged)
            {
                rejectionCount++;
                if (rejectionCount >= 3)
                {
                    var burstEvent = new ContinuationEvent
                    {
                        Kind = ContinuationEventKind.StepRejectionBurst,
                        Lambda = nextLambda,
                        Description = $"Corrector failed {rejectionCount} times in succession.",
                        Severity = "warning",
                    };
                    allEvents.Add(burstEvent);
                }

                // Halve step size and retry
                stepSize *= 0.5;
                if (stepSize < spec.MinStepSize)
                {
                    terminationReason = "min-step-size";
                    break;
                }
                continue;
            }

            // Step accepted
            rejectionCount = 0;
            var (residual, residualNorm) = _evaluateResidual(corrected, nextLambda);
            double ds = System.Math.Abs(nextLambda - lambda);
            arclength += ds;

            // Event detection: extractor failure
            if (_extractorDelegate != null && !_extractorDelegate(corrected.Coefficients))
            {
                stepEvents.Add(new ContinuationEvent
                {
                    Kind = ContinuationEventKind.ExtractorFailure,
                    Lambda = nextLambda,
                    Description = $"Extractor returned false at lambda={nextLambda:G6}",
                    Severity = "warning",
                });
            }

            // Event detection: branch merge/split via tangent direction change
            double[] currentTangent = new double[corrected.Coefficients.Length];
            if (steps.Count > 0)
            {
                var prevCoeffs = currentState.Coefficients;
                double tangentNorm = 0;
                for (int i = 0; i < currentTangent.Length; i++)
                {
                    currentTangent[i] = corrected.Coefficients[i] - prevCoeffs[i];
                    tangentNorm += currentTangent[i] * currentTangent[i];
                }
                tangentNorm = System.Math.Sqrt(tangentNorm);
                if (tangentNorm > 1e-15)
                {
                    for (int i = 0; i < currentTangent.Length; i++)
                        currentTangent[i] /= tangentNorm;
                }

                if (prevTangent != null && tangentNorm > 1e-15)
                {
                    double dot = 0;
                    for (int i = 0; i < currentTangent.Length; i++)
                        dot += currentTangent[i] * prevTangent[i];

                    if (dot < 0.5)
                    {
                        stepEvents.Add(new ContinuationEvent
                        {
                            Kind = ContinuationEventKind.BranchMergeSplitCandidate,
                            Lambda = nextLambda,
                            Description = $"Sharp tangent change detected (dot={dot:F4}) at lambda={nextLambda:G6}",
                            Severity = "warning",
                        });
                    }
                }

                prevTangent = currentTangent;
            }

            // Event detection: gauge-slice breakdown
            if (_gaugeResidualNormDelegate != null)
            {
                double gaugeNorm = _gaugeResidualNormDelegate(corrected.Coefficients);
                if (gaugeNorm > spec.CorrectorTolerance)
                {
                    stepEvents.Add(new ContinuationEvent
                    {
                        Kind = ContinuationEventKind.GaugeSliceBreakdown,
                        Lambda = nextLambda,
                        Description = $"Gauge residual norm {gaugeNorm:E3} exceeds tolerance at lambda={nextLambda:G6}",
                        Severity = "critical",
                    });
                }
            }

            // Event detection: spectrum probe
            if (spec.ProbeSpectrum && _spectrumProbe != null && _hessianProvider != null)
            {
                var probeResult = _spectrumProbe.ComputeSmallestEigenvalues(
                    _hessianProvider, spec.SpectrumProbeCount);

                if (probeResult.Values.Length > 0)
                {
                    double smallestEv = probeResult.Values[0];

                    // Detect Hessian sign change
                    if (prevSmallestEigenvalue.HasValue &&
                        prevSmallestEigenvalue.Value * smallestEv < 0)
                    {
                        stepEvents.Add(new ContinuationEvent
                        {
                            Kind = ContinuationEventKind.HessianSignChange,
                            Lambda = nextLambda,
                            Description = $"Smallest eigenvalue changed sign: {prevSmallestEigenvalue.Value:E3} -> {smallestEv:E3}",
                            Severity = "critical",
                        });
                    }

                    // Detect singular value collapse
                    if (System.Math.Abs(smallestEv) < 1e-10)
                    {
                        stepEvents.Add(new ContinuationEvent
                        {
                            Kind = ContinuationEventKind.SingularValueCollapse,
                            Lambda = nextLambda,
                            Description = $"Near-zero eigenvalue detected: {smallestEv:E3}",
                            Severity = "critical",
                        });
                    }

                    prevSmallestEigenvalue = smallestEv;
                }
            }

            allEvents.AddRange(stepEvents);
            steps.Add(MakeStep(step, nextLambda, arclength, stepSize, residualNorm,
                correctorIters, converged, stepEvents));

            lambda = nextLambda;
            currentState = corrected;

            // Check if reached target
            if (System.Math.Abs(lambda - spec.LambdaEnd) < 1e-15)
            {
                terminationReason = "reached-end";
                break;
            }

            // Adaptive step size: grow if corrector converged quickly
            if (correctorIters <= spec.MaxCorrectorIterations / 3)
                stepSize = System.Math.Min(stepSize * 1.5, spec.MaxStepSize);
            else if (correctorIters > spec.MaxCorrectorIterations * 2 / 3)
                stepSize = System.Math.Max(stepSize * 0.75, spec.MinStepSize);
        }

        double lambdaMin = steps.Min(s => s.Lambda);
        double lambdaMax = steps.Max(s => s.Lambda);

        return new ContinuationResult
        {
            Spec = spec,
            Path = steps,
            TerminationReason = terminationReason,
            TotalSteps = steps.Count,
            TotalArclength = arclength,
            LambdaMin = lambdaMin,
            LambdaMax = lambdaMax,
            AllEvents = allEvents,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private static ContinuationStep MakeStep(
        int index, double lambda, double arclength, double stepSize,
        double residualNorm, int correctorIters, bool converged,
        IReadOnlyList<ContinuationEvent> events)
    {
        return new ContinuationStep
        {
            StepIndex = index,
            Lambda = lambda,
            Arclength = arclength,
            StepSize = stepSize,
            ResidualNorm = residualNorm,
            CorrectorIterations = correctorIters,
            CorrectorConverged = converged,
            Events = events,
        };
    }
}
