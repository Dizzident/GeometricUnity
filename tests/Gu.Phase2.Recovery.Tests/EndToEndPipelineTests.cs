using Gu.Core;
using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery.Tests;

public sealed class EndToEndPipelineTests
{
    private static RecoveryNode MakeNode(string id, RecoveryNodeKind kind) => new()
    {
        NodeId = id,
        Kind = kind,
        SourceObjectType = "curvature-2form",
        BranchProvenanceId = "branch-1",
        GaugeDependent = false,
        Dimensionality = 4,
        NumericalDependencyStatus = "converged",
        TheoremDependencyStatus = "unknown",
    };

    [Fact]
    public void FullPipeline_CreateGraph_EvaluateGate_VerifyClaimClass()
    {
        // 1. Build valid recovery graph
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("o1", RecoveryNodeKind.Observation),
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "o1", OperatorId = "sigma_h_star" },
                new RecoveryEdge { FromNodeId = "o1", ToNodeId = "e1", OperatorId = "trace-projector" },
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "identification-gate" },
            ],
        };

        Assert.True(graph.Validate(out var errors));
        Assert.Empty(errors);

        // 2. Evaluate physical-identification gate with all fields
        var identification = new PhysicalIdentificationRecord
        {
            IdentificationId = "phys-id-1",
            FormalSource = "F_{A_omega} on Y",
            ObservationExtractionMap = "sigma_h_star -> trace",
            SupportStatus = "theorem-supported",
            ApproximationStatus = "flat background, su(2), dim=4",
            ComparisonTarget = "Yang-Mills field strength trace",
            Falsifier = "wrong gauge group dimension or sign",
            ResolvedClaimClass = ClaimClass.Inadmissible, // will be resolved by gate
        };

        var resolved = IdentificationGate.Evaluate(identification);
        Assert.Equal(ClaimClass.ExactStructuralConsequence, resolved.ResolvedClaimClass);
        Assert.NotSame(identification, resolved); // immutable — new record returned

        // 3. Create observed output record
        var output = new ObservedOutputRecord
        {
            OutputId = "out-1",
            Kind = ObservedOutputKind.ScalarInvariant,
            Snapshot = new ObservableSnapshot
            {
                ObservableId = "trace-curvature",
                OutputType = OutputType.ExactStructural,
                Values = [42.0],
            },
            RecoveryNodeId = "e1",
            ClaimCeiling = resolved.ResolvedClaimClass,
        };

        Assert.Equal(ClaimClass.ExactStructuralConsequence, output.ClaimCeiling);
    }

    [Fact]
    public void DemotedPipeline_ConjecturalSupport_CappedAtSpeculative()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("o1", RecoveryNodeKind.Observation),
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "o1", OperatorId = "sigma_h_star" },
                new RecoveryEdge { FromNodeId = "o1", ToNodeId = "e1", OperatorId = "projector" },
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate" },
            ],
        };

        Assert.True(graph.Validate(out _));

        var identification = new PhysicalIdentificationRecord
        {
            IdentificationId = "phys-id-2",
            FormalSource = "residual on Y",
            ObservationExtractionMap = "sigma_h_star -> norm",
            SupportStatus = "conjectural",
            ApproximationStatus = "low-order discretization",
            ComparisonTarget = "vacuum energy density",
            Falsifier = "order-of-magnitude mismatch",
            ResolvedClaimClass = ClaimClass.Inadmissible,
        };

        var resolved = IdentificationGate.Evaluate(identification);
        Assert.Equal(ClaimClass.SpeculativeInterpretation, resolved.ResolvedClaimClass);
    }

    [Fact]
    public void InvalidGraph_BlocksPipeline()
    {
        // Native -> Interpretation (skip levels) should fail validation
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "i1", OperatorId = "shortcut" },
            ],
        };

        Assert.False(graph.Validate(out var errors));
        Assert.NotEmpty(errors);
    }
}
