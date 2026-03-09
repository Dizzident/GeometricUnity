using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.IntegrationTests;

/// <summary>
/// Recovery DAG + gate enforcement integration test (GAP-14).
/// Builds a valid 4-node RecoveryGraph (Native -> Obs -> Ext -> Interp),
/// runs IdentificationGate with missing falsifier,
/// asserts Inadmissible result, and validates the DAG.
/// </summary>
public class RecoveryDagGateTests
{
    [Fact]
    public void RecoveryGraph_Valid4NodeDAG_PassesValidation()
    {
        var graph = Build4NodeGraph();

        bool isValid = graph.Validate(out var errors);

        Assert.True(isValid, $"Validation errors: {string.Join("; ", errors)}");
        Assert.Empty(errors);
    }

    [Fact]
    public void IdentificationGate_MissingFalsifier_ReturnsInadmissible()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "pid-missing-falsifier",
            FormalSource = "native-state-1",
            ObservationExtractionMap = "sigma-pullback-standard",
            SupportStatus = "numerical-only",
            ApproximationStatus = "leading-order",
            ComparisonTarget = "mass-ratio",
            Falsifier = "",
            ResolvedClaimClass = ClaimClass.ExactStructuralConsequence,
        };

        var result = IdentificationGate.Evaluate(record);

        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void IdentificationGate_MissingFormalSource_ReturnsInadmissible()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "pid-missing-source",
            FormalSource = "",
            ObservationExtractionMap = "sigma-pullback-standard",
            SupportStatus = "numerical-only",
            ApproximationStatus = "leading-order",
            ComparisonTarget = "mass-ratio",
            Falsifier = "mass-ratio > 10",
            ResolvedClaimClass = ClaimClass.ExactStructuralConsequence,
        };

        var result = IdentificationGate.Evaluate(record);

        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void IdentificationGate_TheoremSupported_AllowsExact()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "pid-theorem",
            FormalSource = "D_omega* Upsilon = 0",
            ObservationExtractionMap = "sigma-pullback-standard",
            SupportStatus = "theorem-supported",
            ApproximationStatus = "exact",
            ComparisonTarget = "gauge-invariant-mass",
            Falsifier = "mass deviates by > 1%",
            ResolvedClaimClass = ClaimClass.SpeculativeInterpretation,
        };

        var result = IdentificationGate.Evaluate(record);

        Assert.Equal(ClaimClass.ExactStructuralConsequence, result.ResolvedClaimClass);
    }

    [Fact]
    public void IdentificationGate_Conjectural_CapsAtSpeculative()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "pid-conjectural",
            FormalSource = "conjectural field equation",
            ObservationExtractionMap = "sigma-pullback-standard",
            SupportStatus = "conjectural",
            ApproximationStatus = "leading-order",
            ComparisonTarget = "coupling-constant",
            Falsifier = "coupling deviates by > 10%",
            ResolvedClaimClass = ClaimClass.ExactStructuralConsequence,
        };

        var result = IdentificationGate.Evaluate(record);

        Assert.Equal(ClaimClass.SpeculativeInterpretation, result.ResolvedClaimClass);
    }

    [Fact]
    public void RecoveryGraph_SkippedLevel_FailsValidation()
    {
        // Native -> Interpretation (skipping Observation and Extraction)
        var graph = new RecoveryGraph
        {
            Nodes = new[]
            {
                new RecoveryNode { NodeId = "native", Kind = RecoveryNodeKind.Native, BranchProvenanceId = "b1", SourceObjectType = "curvature-2form", GaugeDependent = true, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
                new RecoveryNode { NodeId = "interp", Kind = RecoveryNodeKind.Interpretation, BranchProvenanceId = "b1", SourceObjectType = "derived-tensor", GaugeDependent = false, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
            },
            Edges = new[]
            {
                new RecoveryEdge { FromNodeId = "native", ToNodeId = "interp", OperatorId = "bad-skip" },
            },
        };

        bool isValid = graph.Validate(out var errors);

        Assert.False(isValid);
        Assert.NotEmpty(errors);
    }

    // --- Helpers ---

    private static RecoveryGraph Build4NodeGraph()
    {
        return new RecoveryGraph
        {
            Nodes = new[]
            {
                new RecoveryNode { NodeId = "native-1", Kind = RecoveryNodeKind.Native, BranchProvenanceId = "branch-001", SourceObjectType = "curvature-2form", GaugeDependent = true, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
                new RecoveryNode { NodeId = "obs-1", Kind = RecoveryNodeKind.Observation, BranchProvenanceId = "branch-001", SourceObjectType = "observed-tensor", GaugeDependent = false, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
                new RecoveryNode { NodeId = "ext-1", Kind = RecoveryNodeKind.Extraction, BranchProvenanceId = "branch-001", SourceObjectType = "extracted-observable", GaugeDependent = false, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
                new RecoveryNode { NodeId = "interp-1", Kind = RecoveryNodeKind.Interpretation, BranchProvenanceId = "branch-001", SourceObjectType = "physical-quantity", GaugeDependent = false, Dimensionality = 4, NumericalDependencyStatus = "converged", TheoremDependencyStatus = "unknown" },
            },
            Edges = new[]
            {
                new RecoveryEdge { FromNodeId = "native-1", ToNodeId = "obs-1", OperatorId = "pullback" },
                new RecoveryEdge { FromNodeId = "obs-1", ToNodeId = "ext-1", OperatorId = "extraction" },
                new RecoveryEdge { FromNodeId = "ext-1", ToNodeId = "interp-1", OperatorId = "interpretation" },
            },
        };
    }
}
