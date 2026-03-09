using Gu.Phase2.Recovery;

namespace Gu.Phase2.Recovery.Tests;

public sealed class RecoveryGraphTests
{
    private static RecoveryNode MakeNode(string id, RecoveryNodeKind kind, string branchProvenanceId = "branch-1") => new()
    {
        NodeId = id,
        Kind = kind,
        SourceObjectType = "curvature-2form",
        BranchProvenanceId = branchProvenanceId,
        GaugeDependent = false,
        Dimensionality = 4,
        NumericalDependencyStatus = "converged",
        TheoremDependencyStatus = "unknown",
    };

    private static RecoveryGraph MakeValidGraph()
    {
        return new RecoveryGraph
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
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate-eval" },
            ],
        };
    }

    [Fact]
    public void ValidGraph_Passes()
    {
        var graph = MakeValidGraph();
        bool valid = graph.Validate(out var errors);
        Assert.True(valid);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyGraph_IsValid()
    {
        var graph = new RecoveryGraph { Nodes = [], Edges = [] };
        bool valid = graph.Validate(out var errors);
        Assert.True(valid);
        Assert.Empty(errors);
    }

    [Fact]
    public void SkipLevel_NativeToInterpretation_Rejected()
    {
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

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("Invalid edge"));
    }

    [Fact]
    public void SkipLevel_NativeToExtraction_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "e1", OperatorId = "skip-obs" },
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("Invalid edge"));
    }

    [Fact]
    public void TerminalNonInterpretation_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("o1", RecoveryNodeKind.Observation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "o1", OperatorId = "sigma_h_star" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("Terminal node") && e.Contains("Observation"));
    }

    [Fact]
    public void InterpretationMissingUpstream_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("missing upstream"));
    }

    [Fact]
    public void DuplicateNodeIds_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("dup", RecoveryNodeKind.Native),
                MakeNode("dup", RecoveryNodeKind.Observation),
            ],
            Edges = [],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void EdgeToUnknownNode_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes = [MakeNode("n1", RecoveryNodeKind.Native)],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "missing", OperatorId = "op" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("unknown target"));
    }

    [Fact]
    public void EdgeFromUnknownNode_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes = [MakeNode("o1", RecoveryNodeKind.Observation)],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "missing", ToNodeId = "o1", OperatorId = "op" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("unknown source"));
    }

    [Fact]
    public void InterpretationFromInterpretation_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("i1", RecoveryNodeKind.Interpretation),
                MakeNode("i2", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "i1", ToNodeId = "i2", OperatorId = "chain" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("Invalid edge"));
    }

    [Fact]
    public void MultiplePaths_AllValid()
    {
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("n2", RecoveryNodeKind.Native),
                MakeNode("o1", RecoveryNodeKind.Observation),
                MakeNode("o2", RecoveryNodeKind.Observation),
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("e2", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
                MakeNode("i2", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "o1", OperatorId = "sigma_h_star" },
                new RecoveryEdge { FromNodeId = "o1", ToNodeId = "e1", OperatorId = "trace" },
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate" },
                new RecoveryEdge { FromNodeId = "n2", ToNodeId = "o2", OperatorId = "sigma_h_star" },
                new RecoveryEdge { FromNodeId = "o2", ToNodeId = "e2", OperatorId = "norm" },
                new RecoveryEdge { FromNodeId = "e2", ToNodeId = "i2", OperatorId = "gate" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.True(valid);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyBranchProvenanceId_Rejected()
    {
        var graph = new RecoveryGraph
        {
            Nodes = [MakeNode("n1", RecoveryNodeKind.Native, branchProvenanceId: "")],
            Edges = [],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("empty BranchProvenanceId"));
    }

    [Fact]
    public void CycleDetected_Rejected()
    {
        // Create a cycle by having two observation nodes reference each other
        // (this also violates level ordering, but cycle detection is a separate check)
        var graph = new RecoveryGraph
        {
            Nodes =
            [
                MakeNode("n1", RecoveryNodeKind.Native),
                MakeNode("o1", RecoveryNodeKind.Observation),
                MakeNode("o2", RecoveryNodeKind.Observation),
                MakeNode("e1", RecoveryNodeKind.Extraction),
                MakeNode("i1", RecoveryNodeKind.Interpretation),
            ],
            Edges =
            [
                new RecoveryEdge { FromNodeId = "n1", ToNodeId = "o1", OperatorId = "sigma" },
                new RecoveryEdge { FromNodeId = "o1", ToNodeId = "o2", OperatorId = "cycle-a" },
                new RecoveryEdge { FromNodeId = "o2", ToNodeId = "o1", OperatorId = "cycle-b" },
                new RecoveryEdge { FromNodeId = "o1", ToNodeId = "e1", OperatorId = "proj" },
                new RecoveryEdge { FromNodeId = "e1", ToNodeId = "i1", OperatorId = "gate" },
            ],
        };

        bool valid = graph.Validate(out var errors);
        Assert.False(valid);
        Assert.Contains(errors, e => e.Contains("cycle"));
    }
}
