using Gu.Core;
using Gu.Phase5.BranchIndependence;

namespace Gu.Phase5.BranchIndependence.Tests;

/// <summary>
/// P11-M4: Tests documenting branch fragility stabilization as the stable scientific boundary.
///
/// The Phase IX broader family search (method sweeps and label sweeps) produced:
/// - method sweep (identity/first-order): 4/7 admitted, with both trivial (0 iters, 0 gauge)
///   and nontrivial (2 iters, ~0.14-0.20 gauge) solutions in the admitted family.
/// - label sweep (identity/first-order): 1/8 admitted — only trivial solution admitted.
///
/// Since the admitted family mixes trivial and nontrivial solutions, gauge-violation and
/// solver-iterations remain fragile under unchanged thresholds. This is the stable
/// scientific boundary, not a missing implementation.
///
/// These tests verify:
/// 1. A mixed-family study with both trivial and nontrivial members reports "mixed" overall.
/// 2. gauge-violation and solver-iterations are correctly classified as "fragile" when
///    the admitted family contains both zero-valued and nonzero-valued members.
/// 3. residual-norm, stationarity-norm, objective-value are classified as "invariant"
///    when all members satisfy the tolerance (all near zero).
/// 4. The engine does not reclassify fragile quantities as robust under the same thresholds.
/// </summary>
public sealed class Phase11BranchFragilityStabilizationTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase11-branch-fragility-stabilization",
        Branch = new BranchRef { BranchId = "phase8-real-atlas-control", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    };

    /// <summary>
    /// Creates a branch robustness study spec matching the Phase IX campaign parameters:
    /// - 4 admitted variants (one trivial + three nontrivial from method sweeps)
    /// - standard thresholds (absTol=1e-5, relTol=0.1)
    /// </summary>
    private static BranchRobustnessStudySpec MakePhase9MixedSpec() => new()
    {
        StudyId = "phase9-su2-real-bridge-backed-branch-family",
        BranchVariantIds = new List<string>
        {
            "bg-variant-trivial",         // trivial: all zeros
            "bg-variant-nontrivial-1",    // nontrivial: 2 iters, gaugeViol ~0.14
            "bg-variant-nontrivial-2",    // nontrivial: 2 iters, gaugeViol ~0.20
            "bg-variant-nontrivial-3",    // nontrivial: 2 iters, gaugeViol ~0.20
        },
        TargetQuantityIds = new List<string>
        {
            "residual-norm",
            "stationarity-norm",
            "objective-value",
            "gauge-violation",
            "solver-iterations",
        },
        AbsoluteTolerance = 1e-5,
        RelativeTolerance = 0.1,
    };

    [Fact]
    public void MixedFamily_WithTrivialAndNontrivialMembers_ReportsMixedOverallSummary()
    {
        // Arrange: values matching the Phase IX method-sweep admitted family
        // trivial variant: all zeros
        // nontrivial variants: residuals ~1e-10, gauge violations ~0.14-0.20
        var quantityValues = new Dictionary<string, double[]>
        {
            ["residual-norm"]     = [0.0,      3.33e-10, 3.33e-10, 4.77e-9],
            ["stationarity-norm"] = [0.0,      5.00e-10, 4.08e-9,  4.94e-9],
            ["objective-value"]   = [0.0,      5.56e-20, 6.65e-18, 1.14e-17],
            ["gauge-violation"]   = [0.0,      0.138,    0.214,    0.196],
            ["solver-iterations"] = [0.0,      2.0,      2.0,      2.0],
        };

        var spec = MakePhase9MixedSpec();
        var engine = new BranchRobustnessEngine(spec);

        // Act
        var record = engine.Run(quantityValues, TestProvenance());

        // Assert
        Assert.Equal("mixed", record.OverallSummary);
    }

    [Fact]
    public void MixedFamily_GaugeViolation_IsFragileUnderUnchangedThresholds()
    {
        // gauge-violation: trivial=0, nontrivials~0.14-0.20
        // Max distance = 0.214, mean~0.137, fragilityScore >> 0.5 → fragile
        var quantityValues = new Dictionary<string, double[]>
        {
            ["residual-norm"]     = [0.0,      3.33e-10, 3.33e-10, 4.77e-9],
            ["stationarity-norm"] = [0.0,      5.00e-10, 4.08e-9,  4.94e-9],
            ["objective-value"]   = [0.0,      5.56e-20, 6.65e-18, 1.14e-17],
            ["gauge-violation"]   = [0.0,      0.138,    0.214,    0.196],
            ["solver-iterations"] = [0.0,      2.0,      2.0,      2.0],
        };

        var spec = MakePhase9MixedSpec();
        var engine = new BranchRobustnessEngine(spec);
        var record = engine.Run(quantityValues, TestProvenance());

        Assert.True(record.FragilityRecords.ContainsKey("gauge-violation"));
        var gaugeFragility = record.FragilityRecords["gauge-violation"];
        Assert.True(gaugeFragility.Classification == "fragile",
            "gauge-violation must remain fragile: trivial (0) and nontrivial (~0.14-0.21) members coexist in the admitted family.");
    }

    [Fact]
    public void MixedFamily_SolverIterations_IsFragileUnderUnchangedThresholds()
    {
        // solver-iterations: trivial=0, nontrivials=2 → mixed
        var quantityValues = new Dictionary<string, double[]>
        {
            ["residual-norm"]     = [0.0,      3.33e-10, 3.33e-10, 4.77e-9],
            ["stationarity-norm"] = [0.0,      5.00e-10, 4.08e-9,  4.94e-9],
            ["objective-value"]   = [0.0,      5.56e-20, 6.65e-18, 1.14e-17],
            ["gauge-violation"]   = [0.0,      0.138,    0.214,    0.196],
            ["solver-iterations"] = [0.0,      2.0,      2.0,      2.0],
        };

        var spec = MakePhase9MixedSpec();
        var engine = new BranchRobustnessEngine(spec);
        var record = engine.Run(quantityValues, TestProvenance());

        Assert.True(record.FragilityRecords.ContainsKey("solver-iterations"));
        var itersFragility = record.FragilityRecords["solver-iterations"];
        Assert.True(itersFragility.Classification == "fragile",
            "solver-iterations must remain fragile: trivial (0 iters) and nontrivial (2 iters) members coexist in the admitted family.");
    }

    [Fact]
    public void MixedFamily_ResidualNorm_IsInvariantUnderSameThresholds()
    {
        // residual-norm: trivial=0, nontrivials all < 1e-5 (absolute tolerance)
        // With absTol=1e-5: max distance is ~4.77e-9 < 1e-5 → invariant
        var quantityValues = new Dictionary<string, double[]>
        {
            ["residual-norm"]     = [0.0,      3.33e-10, 3.33e-10, 4.77e-9],
            ["stationarity-norm"] = [0.0,      5.00e-10, 4.08e-9,  4.94e-9],
            ["objective-value"]   = [0.0,      5.56e-20, 6.65e-18, 1.14e-17],
            ["gauge-violation"]   = [0.0,      0.138,    0.214,    0.196],
            ["solver-iterations"] = [0.0,      2.0,      2.0,      2.0],
        };

        var spec = MakePhase9MixedSpec();
        var engine = new BranchRobustnessEngine(spec);
        var record = engine.Run(quantityValues, TestProvenance());

        Assert.True(record.FragilityRecords.ContainsKey("residual-norm"));
        var residualFragility = record.FragilityRecords["residual-norm"];
        Assert.True(residualFragility.Classification == "invariant",
            "residual-norm must be invariant: all values in admitted family are below absolute tolerance 1e-5.");
    }

    [Fact]
    public void BroaderFamilySearch_CannotImproveFragilityWithoutRelaxingThresholds()
    {
        // Demonstrates that adding more nontrivial variants to the admitted family
        // does NOT resolve gauge-violation or solver-iterations fragility,
        // because the trivial variant (with all zeros) will always be in the family.
        //
        // Simulates having more nontrivial variants (5 total, 4 nontrivial + 1 trivial).
        var widerSpec = new BranchRobustnessStudySpec
        {
            StudyId = "phase9-wider-family-search",
            BranchVariantIds = new List<string>
            {
                "bg-trivial",
                "bg-nt-1", "bg-nt-2", "bg-nt-3", "bg-nt-4",
            },
            TargetQuantityIds = new List<string> { "gauge-violation", "solver-iterations" },
            AbsoluteTolerance = 1e-5,
            RelativeTolerance = 0.1,
        };

        // 4 nontrivial variants with gauge violations ~0.14-0.20, plus 1 trivial at 0
        var quantityValues = new Dictionary<string, double[]>
        {
            ["gauge-violation"]   = [0.0,  0.138, 0.196, 0.214, 0.190],
            ["solver-iterations"] = [0.0,  2.0,   2.0,   2.0,   2.0],
        };

        var engine = new BranchRobustnessEngine(widerSpec);
        var record = engine.Run(quantityValues, TestProvenance());

        // Even with 4 nontrivial variants, the trivial variant keeps both quantities fragile
        Assert.True(record.FragilityRecords["gauge-violation"].Classification == "fragile",
            "A broader admitted family including both trivial (0) and nontrivial (~0.14-0.21) " +
            "variants cannot achieve gauge-violation invariance under unchanged thresholds.");
        Assert.True(record.FragilityRecords["solver-iterations"].Classification == "fragile",
            "A broader admitted family including trivial (0 iters) and nontrivial (2 iters) " +
            "variants cannot achieve solver-iterations invariance under unchanged thresholds.");
    }
}
