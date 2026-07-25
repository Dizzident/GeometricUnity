namespace Phase545;

public sealed class PilotEvaluation
{
    public PilotEvaluation(double action, double[] gradient)
    {
        Action = action;
        Gradient = gradient ?? throw new ArgumentNullException(nameof(gradient));
    }

    public double Action { get; }
    public double[] Gradient { get; }
}

public sealed class PilotProposal
{
    public PilotProposal(
        bool resourceRefused,
        bool finite,
        bool divergent,
        double initialHamiltonian,
        double finalHamiltonian,
        double deltaHamiltonian,
        double[]? position,
        double[]? momentum,
        int forceEvaluationCount,
        long estimatedWorkingBytes)
    {
        ResourceRefused = resourceRefused;
        Finite = finite;
        Divergent = divergent;
        InitialHamiltonian = initialHamiltonian;
        FinalHamiltonian = finalHamiltonian;
        DeltaHamiltonian = deltaHamiltonian;
        Position = position;
        Momentum = momentum;
        ForceEvaluationCount = forceEvaluationCount;
        EstimatedWorkingBytes = estimatedWorkingBytes;
    }

    public bool ResourceRefused { get; }
    public bool Finite { get; }
    public bool Divergent { get; }
    public double InitialHamiltonian { get; }
    public double FinalHamiltonian { get; }
    public double DeltaHamiltonian { get; }
    public double[]? Position { get; }
    public double[]? Momentum { get; }
    public int ForceEvaluationCount { get; }
    public long EstimatedWorkingBytes { get; }
}

public sealed class PilotDecision
{
    public PilotDecision(
        PilotProposal proposal,
        bool accepted,
        double injectedLogUniformThreshold,
        double logAcceptanceThreshold,
        double[]? selectedPosition)
    {
        Proposal = proposal;
        Accepted = accepted;
        InjectedLogUniformThreshold = injectedLogUniformThreshold;
        LogAcceptanceThreshold = logAcceptanceThreshold;
        SelectedPosition = selectedPosition;
    }

    public PilotProposal Proposal { get; }
    public bool Accepted { get; }
    public double InjectedLogUniformThreshold { get; }
    public double LogAcceptanceThreshold { get; }
    public double[]? SelectedPosition { get; }
}

public static class PilotKernel
{
    public const double DefaultDivergenceThreshold = 100.0;

    public static PilotDecision RunSingleProposal(
        ReadOnlySpan<double> position,
        ReadOnlySpan<double> momentum,
        double stepSize,
        int leapfrogSteps,
        double injectedLogUniformThreshold,
        long maximumWorkingBytes,
        Func<double[], PilotEvaluation> evaluate,
        double divergenceThreshold = DefaultDivergenceThreshold)
    {
        if (!double.IsFinite(injectedLogUniformThreshold) || injectedLogUniformThreshold > 0.0)
            throw new ArgumentOutOfRangeException(nameof(injectedLogUniformThreshold));

        PilotProposal proposal = ConstructProposal(
            position, momentum, stepSize, leapfrogSteps, maximumWorkingBytes, evaluate, divergenceThreshold);
        double logAcceptance = proposal.Finite
            ? System.Math.Min(0.0, -proposal.DeltaHamiltonian)
            : double.NegativeInfinity;
        bool accepted = !proposal.ResourceRefused
            && proposal.Finite
            && injectedLogUniformThreshold <= logAcceptance;
        double[]? selected = proposal.ResourceRefused
            ? null
            : accepted
                ? (double[]?)proposal.Position?.Clone()
                : position.ToArray();
        return new PilotDecision(proposal, accepted, injectedLogUniformThreshold, logAcceptance, selected);
    }

    public static PilotProposal ConstructProposal(
        ReadOnlySpan<double> position,
        ReadOnlySpan<double> momentum,
        double stepSize,
        int leapfrogSteps,
        long maximumWorkingBytes,
        Func<double[], PilotEvaluation> evaluate,
        double divergenceThreshold = DefaultDivergenceThreshold)
    {
        ArgumentNullException.ThrowIfNull(evaluate);
        if (position.Length == 0 || position.Length != momentum.Length)
            throw new ArgumentException("Position and momentum must have equal positive lengths.");
        if (!double.IsFinite(stepSize) || stepSize <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(stepSize));
        if (leapfrogSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(leapfrogSteps));
        if (maximumWorkingBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumWorkingBytes));
        if (!double.IsFinite(divergenceThreshold) || divergenceThreshold <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(divergenceThreshold));

        long estimatedWorkingBytes;
        try
        {
            estimatedWorkingBytes = checked((long)position.Length * sizeof(double) * 3L);
        }
        catch (OverflowException)
        {
            estimatedWorkingBytes = long.MaxValue;
        }

        if (estimatedWorkingBytes > maximumWorkingBytes)
            return new PilotProposal(true, false, false, double.NaN, double.NaN, double.NaN,
                null, null, 0, estimatedWorkingBytes);

        double[] q = position.ToArray();
        double[] p = momentum.ToArray();
        if (!AllFinite(q) || !AllFinite(p))
            return NonFinite(q, p, 0, estimatedWorkingBytes);
        PilotEvaluation initial = evaluate(q);
        int evaluations = 1;
        if (!Finite(initial, q.Length))
            return NonFinite(q, p, evaluations, estimatedWorkingBytes);

        double initialHamiltonian = Hamiltonian(initial.Action, p);
        double action = initial.Action;
        double[] gradient = initial.Gradient;
        for (int i = 0; i < p.Length; i++)
            p[i] -= 0.5 * stepSize * gradient[i];

        for (int leap = 0; leap < leapfrogSteps; leap++)
        {
            for (int i = 0; i < q.Length; i++)
                q[i] += stepSize * p[i];
            PilotEvaluation current = evaluate(q);
            evaluations++;
            if (!Finite(current, q.Length) || !AllFinite(q) || !AllFinite(p))
                return NonFinite(q, p, evaluations, estimatedWorkingBytes);
            action = current.Action;
            gradient = current.Gradient;
            double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
            for (int i = 0; i < p.Length; i++)
                p[i] -= kick * gradient[i];
        }

        double finalHamiltonian = Hamiltonian(action, p);
        double delta = finalHamiltonian - initialHamiltonian;
        bool finite = double.IsFinite(initialHamiltonian) && double.IsFinite(finalHamiltonian)
            && double.IsFinite(delta) && AllFinite(q) && AllFinite(p);
        bool divergent = !finite || System.Math.Abs(delta) > divergenceThreshold;
        return new PilotProposal(false, finite, divergent, initialHamiltonian, finalHamiltonian,
            delta, q, p, evaluations, estimatedWorkingBytes);
    }

    private static PilotProposal NonFinite(
        double[] position,
        double[] momentum,
        int forceEvaluationCount,
        long estimatedWorkingBytes) =>
        new(false, false, true, double.NaN, double.NaN, double.NaN,
            position, momentum, forceEvaluationCount, estimatedWorkingBytes);

    private static bool Finite(PilotEvaluation evaluation, int expectedLength) =>
        double.IsFinite(evaluation.Action)
        && evaluation.Gradient.Length == expectedLength
        && AllFinite(evaluation.Gradient);

    private static bool AllFinite(ReadOnlySpan<double> values)
    {
        foreach (double value in values)
            if (!double.IsFinite(value))
                return false;
        return true;
    }

    private static double Hamiltonian(double action, ReadOnlySpan<double> momentum)
    {
        double squaredNorm = 0.0;
        foreach (double value in momentum)
            squaredNorm += value * value;
        return action + 0.5 * squaredNorm;
    }
}
