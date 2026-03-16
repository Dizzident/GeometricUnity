using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Couplings;

/// <summary>
/// Builds finite-difference Dirac variation matrices from a bosonic background state.
/// </summary>
public sealed class FiniteDifferenceDiracVariationBuilder
{
    private readonly ISpinConnectionBuilder _spinConnectionBuilder;
    private readonly IDiracOperatorAssembler _diracAssembler;

    public FiniteDifferenceDiracVariationBuilder(
        ISpinConnectionBuilder spinConnectionBuilder,
        IDiracOperatorAssembler diracAssembler)
    {
        _spinConnectionBuilder = spinConnectionBuilder ?? throw new ArgumentNullException(nameof(spinConnectionBuilder));
        _diracAssembler = diracAssembler ?? throw new ArgumentNullException(nameof(diracAssembler));
    }

    public FiniteDifferenceDiracVariationResult BuildVariation(
        string variationId,
        string bosonModeId,
        BackgroundRecord background,
        double[] baseBosonicState,
        double[] bosonPerturbation,
        double epsilon,
        SpinorRepresentationSpec spinorSpec,
        FermionFieldLayout layout,
        GammaOperatorBundle gammas,
        SimplicialMesh mesh,
        ProvenanceMeta provenance,
        DiracOperatorBundle? baseDiracBundle = null)
    {
        ArgumentNullException.ThrowIfNull(variationId);
        ArgumentNullException.ThrowIfNull(bosonModeId);
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(baseBosonicState);
        ArgumentNullException.ThrowIfNull(bosonPerturbation);
        ArgumentNullException.ThrowIfNull(spinorSpec);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(provenance);

        if (epsilon <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Finite-difference epsilon must be positive.");

        if (baseBosonicState.Length != bosonPerturbation.Length)
        {
            throw new ArgumentException(
                $"Boson perturbation length {bosonPerturbation.Length} does not match base state length {baseBosonicState.Length}.",
                nameof(bosonPerturbation));
        }

        var baseBundle = baseDiracBundle ?? Assemble(background, baseBosonicState, spinorSpec, layout, gammas, mesh, provenance);
        if (!baseBundle.HasExplicitMatrix || baseBundle.ExplicitMatrix is null)
        {
            return Blocked(
                variationId,
                bosonModeId,
                background.BackgroundId,
                provenance,
                "Base Dirac operator has no explicit matrix; finite-difference variation is blocked.");
        }

        var perturbedState = new double[baseBosonicState.Length];
        for (int i = 0; i < baseBosonicState.Length; i++)
            perturbedState[i] = baseBosonicState[i] + epsilon * bosonPerturbation[i];

        var perturbedBundle = Assemble(background, perturbedState, spinorSpec, layout, gammas, mesh, provenance);
        if (!perturbedBundle.HasExplicitMatrix || perturbedBundle.ExplicitMatrix is null)
        {
            return Blocked(
                variationId,
                bosonModeId,
                background.BackgroundId,
                provenance,
                "Perturbed Dirac operator has no explicit matrix; finite-difference variation is blocked.");
        }

        var (baseRe, baseIm) = ExpandExplicitMatrix(baseBundle.ExplicitMatrix, baseBundle.TotalDof);
        var (perturbedRe, perturbedIm) = ExpandExplicitMatrix(perturbedBundle.ExplicitMatrix, perturbedBundle.TotalDof);
        var (variationRe, variationIm) = CouplingProxyEngine.ComputeVariationMatrix(
            baseRe,
            baseIm,
            perturbedRe,
            perturbedIm,
            epsilon);

        return new FiniteDifferenceDiracVariationResult
        {
            Variation = new DiracVariationBundle
            {
                VariationId = variationId,
                BosonModeId = bosonModeId,
                FermionBackgroundId = background.BackgroundId,
                NormalizationConvention = "raw-boson-vector",
                VariationMethod = "finite-difference",
                FiniteDifferenceEpsilon = epsilon,
                BaseOperatorId = baseBundle.OperatorId,
                PerturbedOperatorId = perturbedBundle.OperatorId,
                MatrixShape = new[] { baseBundle.TotalDof, baseBundle.TotalDof },
                Provenance = provenance,
                DiagnosticNotes =
                {
                    $"Finite-difference variation built from background {background.BackgroundId}.",
                    $"Base operator: {baseBundle.OperatorId}",
                    $"Perturbed operator: {perturbedBundle.OperatorId}",
                },
            },
            RealMatrix = variationRe,
            ImagMatrix = variationIm,
        };
    }

    public static (double[,] Re, double[,] Im) ExpandExplicitMatrix(double[] flatMatrix, int totalDof)
    {
        ArgumentNullException.ThrowIfNull(flatMatrix);

        if (flatMatrix.Length != totalDof * totalDof * 2)
        {
            throw new ArgumentException(
                $"Explicit matrix length {flatMatrix.Length} does not match expected {totalDof * totalDof * 2}.",
                nameof(flatMatrix));
        }

        var re = new double[totalDof, totalDof];
        var im = new double[totalDof, totalDof];
        for (int row = 0; row < totalDof; row++)
        {
            for (int col = 0; col < totalDof; col++)
            {
                int offset = (row * totalDof + col) * 2;
                re[row, col] = flatMatrix[offset];
                im[row, col] = flatMatrix[offset + 1];
            }
        }

        return (re, im);
    }

    private DiracOperatorBundle Assemble(
        BackgroundRecord background,
        double[] bosonicState,
        SpinorRepresentationSpec spinorSpec,
        FermionFieldLayout layout,
        GammaOperatorBundle gammas,
        SimplicialMesh mesh,
        ProvenanceMeta provenance)
    {
        var connection = _spinConnectionBuilder.Build(background, bosonicState, spinorSpec, layout, mesh, provenance);
        return _diracAssembler.Assemble(connection, gammas, layout, mesh, provenance);
    }

    private static FiniteDifferenceDiracVariationResult Blocked(
        string variationId,
        string bosonModeId,
        string backgroundId,
        ProvenanceMeta provenance,
        string reason)
    {
        return new FiniteDifferenceDiracVariationResult
        {
            Variation = new DiracVariationBundle
            {
                VariationId = variationId,
                BosonModeId = bosonModeId,
                FermionBackgroundId = backgroundId,
                NormalizationConvention = "raw-boson-vector",
                VariationMethod = "blocked",
                Blocked = true,
                BlockingReason = reason,
                Provenance = provenance,
                DiagnosticNotes = { reason },
            },
            RealMatrix = null,
            ImagMatrix = null,
        };
    }
}

public sealed class FiniteDifferenceDiracVariationResult
{
    public required DiracVariationBundle Variation { get; init; }

    public double[,]? RealMatrix { get; init; }

    public double[,]? ImagMatrix { get; init; }
}
