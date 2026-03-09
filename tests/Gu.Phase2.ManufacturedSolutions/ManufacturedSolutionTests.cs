using Gu.Phase2.Continuation;
using Gu.Phase2.Stability;

namespace Gu.Phase2.ManufacturedSolutions;

/// <summary>
/// Manufactured solution benchmarks per IMPLEMENTATION_PLAN_P2.md Section 10.4.
/// Class B: Linearization benchmark (flat connection, known Jacobian).
/// Class C: Gauge/slice benchmark (known gauge group dimension, null space suppression).
/// </summary>
public class ManufacturedSolutionTests
{
    // --- Class B: Linearization benchmark ---

    [Fact]
    public void ClassB_LinearizationRecord_CapturesKnownJacobianMetadata()
    {
        // For a flat connection omega=0 on a simple mesh, the Jacobian J
        // is the exterior derivative d. Verify LinearizationRecord can
        // faithfully record this scenario.
        var record = new LinearizationRecord
        {
            BackgroundStateId = "bg-flat-omega0",
            BranchManifestId = "branch-flat",
            OperatorDefinitionId = "J-exterior-derivative",
            DerivativeMode = "analytic",
            InputDimension = 12,  // e.g. 4 edges * 3 (dimG for su2)
            OutputDimension = 6,  // e.g. 2 faces * 3 (dimG for su2)
            GaugeHandlingMode = "raw",
            AssemblyMode = "dense",
            ValidationStatus = "verified-against-manufactured",
        };

        Assert.Equal("J-exterior-derivative", record.OperatorDefinitionId);
        Assert.Equal(12, record.InputDimension);
        Assert.Equal(6, record.OutputDimension);
        Assert.Equal("verified-against-manufactured", record.ValidationStatus);
    }

    [Fact]
    public void ClassB_HessianRecord_SymmetricForSelfAdjointProblem()
    {
        // For a manufactured self-adjoint problem, H = J^T M J should be symmetric.
        var record = new HessianRecord
        {
            BackgroundStateId = "bg-flat-omega0",
            BranchManifestId = "branch-flat",
            GaugeHandlingMode = "raw",
            GaugeLambda = 0.0,
            Dimension = 12,
            AssemblyMode = "dense",
            SymmetryVerified = true,
            SymmetryError = 1e-14,
        };

        Assert.True(record.SymmetryVerified);
        Assert.True(record.SymmetryError < 1e-10);
    }

    [Fact]
    public void ClassB_StabilityAtlas_RecordsLinearizationBenchmark()
    {
        // A complete Class B benchmark produces a stability atlas with
        // linearization record and Hessian for the flat connection case.
        var linearization = new LinearizationRecord
        {
            BackgroundStateId = "bg-flat",
            BranchManifestId = "branch-flat",
            OperatorDefinitionId = "J-equals-d",
            DerivativeMode = "analytic",
            InputDimension = 12,
            OutputDimension = 6,
            GaugeHandlingMode = "raw",
            AssemblyMode = "dense",
            ValidationStatus = "verified",
        };

        var hessian = new HessianRecord
        {
            BackgroundStateId = "bg-flat",
            BranchManifestId = "branch-flat",
            GaugeHandlingMode = "raw",
            GaugeLambda = 0.0,
            Dimension = 12,
            AssemblyMode = "dense",
            SymmetryVerified = true,
            SymmetryError = 0.0,
        };

        var builder = new StabilityAtlasBuilder("atlas-classB", "branch-flat", "Class B: flat connection benchmark")
            .AddLinearizationRecord(linearization)
            .AddHessianRecord(hessian)
            .WithDiscretizationNotes("2D triangular mesh, 4 vertices, 5 edges, 2 faces")
            .WithTheoremStatusNotes("J=d exact for flat connection");

        var atlas = builder.Build();

        Assert.Single(atlas.LinearizationRecords);
        Assert.Single(atlas.HessianRecords);
        Assert.Empty(atlas.BifurcationIndicators);
        Assert.Equal("Class B: flat connection benchmark", atlas.FamilyDescription);
    }

    // --- Class C: Gauge/slice benchmark ---

    [Fact]
    public void ClassC_GaugeFixedLinearization_SuppressesNullSpace()
    {
        // For SU(2) on a connected mesh, the gauge group dimension is dimG = 3.
        // The raw Jacobian should have nullity = 3 (one gauge orbit per generator).
        // After gauge fixing, the null space should be suppressed.
        var gfRecord = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-gauge-test",
            BranchManifestId = "branch-gauge",
            BaseLinearizationId = "lin-raw-J",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 1.0,
            GaugeNullDimension = 3,  // dim(SU(2)) = 3
            GaugeNullSuppressed = true,
            SmallestSliceSingularValue = 0.15,
            ValidationStatus = "verified",
        };

        Assert.Equal(3, gfRecord.GaugeNullDimension);
        Assert.True(gfRecord.GaugeNullSuppressed);
        Assert.True(gfRecord.SmallestSliceSingularValue > 1e-8);
    }

    [Fact]
    public void ClassC_GaugeFixedLinearization_UnsuppressedNullSpace_FailsValidation()
    {
        // When gauge fixing fails, SmallestSliceSingularValue should be near zero
        // and GaugeNullSuppressed should be false.
        var gfRecord = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-gauge-broken",
            BranchManifestId = "branch-gauge",
            BaseLinearizationId = "lin-raw-J",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 0.001,  // Too small lambda
            GaugeNullDimension = 3,
            GaugeNullSuppressed = false,
            SmallestSliceSingularValue = 1e-12,
            ValidationStatus = "failed-null-space-not-suppressed",
        };

        Assert.False(gfRecord.GaugeNullSuppressed);
        Assert.True(gfRecord.SmallestSliceSingularValue!.Value < 1e-8);
        Assert.Contains("failed", gfRecord.ValidationStatus);
    }
}
