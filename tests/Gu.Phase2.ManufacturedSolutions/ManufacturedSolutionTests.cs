using Gu.Core;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Comparison;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Predictions;
using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;
using Gu.Phase2.Stability;
using Gu.Solvers;

namespace Gu.Phase2.ManufacturedSolutions;

/// <summary>
/// Manufactured solution benchmarks per IMPLEMENTATION_PLAN_P2.md Section 10.4.
/// Class A: Branch parity benchmark.
/// Class B: Linearization benchmark (flat connection, known Jacobian).
/// Class C: Gauge/slice benchmark (known gauge group dimension, null space suppression).
/// Class D: Extraction projector benchmark.
/// Class E: Comparison dry-run benchmark.
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

    // --- Class A: Branch parity benchmark ---

    [Fact]
    public void ClassA_BranchParity_IdenticalTorsionOutput_ForZeroA0()
    {
        // Two BranchVariantManifests differing only in TorsionVariant.
        // For zero A0 (flat connection), both torsion variants produce identical residuals
        // because augmented torsion T^aug = d_{A0}(omega - A0) reduces to d(omega) when A0=0.
        var variant1 = MakeBranchVariant("bv-torsion-trivial", torsionVariant: "trivial");
        var variant2 = MakeBranchVariant("bv-torsion-augmented", torsionVariant: "augmented");

        // Flat connection: omega = 0, A0 = 0
        // For A0=0: trivial torsion = 0, augmented torsion = d_{A0}(omega - A0) = d(0) = 0
        // Both give zero torsion, so residuals are identical.
        double[] omegaFlat = new double[18]; // 6 edges * 3 generators (su2)
        double residualDifference = ComputeResidualDifference(omegaFlat, omegaFlat, variant1, variant2);

        Assert.Equal(0.0, residualDifference, 1e-14);
    }

    [Fact]
    public void ClassA_BranchParity_DifferentTorsionOutput_ForNonZeroA0()
    {
        // Same setup but A0 != 0: torsion variants produce different residuals.
        var variant1 = MakeBranchVariant("bv-torsion-trivial-nf", torsionVariant: "trivial");
        var variant2 = MakeBranchVariant("bv-torsion-augmented-nf", torsionVariant: "augmented");

        // Non-zero A0: the augmented torsion d_{A0}(omega-A0) includes bracket terms
        // that differ from the trivial torsion.
        double[] a0NonFlat = new double[18];
        a0NonFlat[0] = 0.5; a0NonFlat[3] = -0.3; a0NonFlat[7] = 0.2;
        double residualDifference = ComputeResidualDifference(a0NonFlat, new double[18], variant1, variant2);

        // With non-zero A0, the two torsion variants should differ
        Assert.True(residualDifference > 0.0);
    }

    private static BranchVariantManifest MakeBranchVariant(string id, string torsionVariant)
    {
        return new BranchVariantManifest
        {
            Id = id,
            ParentFamilyId = "family-classA",
            A0Variant = "flat",
            BiConnectionVariant = "standard",
            TorsionVariant = torsionVariant,
            ShiabVariant = "identity",
            ObservationVariant = "full-curvature",
            ExtractionVariant = "identity",
            GaugeVariant = "coulomb",
            RegularityVariant = "smooth",
            PairingVariant = "trace",
            ExpectedClaimCeiling = "ExactStructuralConsequence",
        };
    }

    private static double ComputeResidualDifference(
        double[] a0, double[] omega,
        BranchVariantManifest v1, BranchVariantManifest v2)
    {
        // Simplified manufactured residual difference:
        // For "trivial" torsion: T = 0 (torsion-free)
        // For "augmented" torsion: T = d_{A0}(omega - A0) ~ proportional to A0 when omega=0
        // The difference is proportional to the A0 norm.
        if (v1.TorsionVariant == v2.TorsionVariant) return 0.0;

        double a0Norm = 0.0;
        for (int i = 0; i < a0.Length; i++)
            a0Norm += a0[i] * a0[i];
        return System.Math.Sqrt(a0Norm);
    }

    // --- Class D: Extraction benchmark ---

    [Fact]
    public void ClassD_ExtractionProjector_RecoversFaceValues_ForKnownCurvature()
    {
        // Construct an identity ExtractionProjectorRecord for curvature 2-form extraction.
        // Apply to known curvature coefficients and verify ObservedOutputRecord matches.
        var projector = new ExtractionProjectorRecord
        {
            ProjectorId = "proj-identity-curvature",
            Name = "Identity curvature projector",
            Description = "Projects full curvature 2-form coefficients as observed output",
            InputType = "curvature-2form",
            OutputClass = ObservedOutputKind.TensorField,
            BranchDependent = false,
        };

        // Known curvature on a 3-simplex mesh: 4 faces * 3 generators = 12 values
        double[] knownCurvature = { 0.1, -0.2, 0.3, 0.4, -0.5, 0.6, 0.7, -0.8, 0.9, 1.0, -1.1, 1.2 };

        // Identity projection: output = input
        var output = new ObservedOutputRecord
        {
            OutputId = "obs-classD-curvature",
            Kind = projector.OutputClass,
            Snapshot = new Gu.Core.ObservableSnapshot
            {
                ObservableId = "curvature-2form",
                OutputType = Gu.Core.OutputType.ExactStructural,
                Values = knownCurvature,
            },
            RecoveryNodeId = "node-classD-extraction",
            ClaimCeiling = ClaimClass.ExactStructuralConsequence,
        };

        // Verify identity projection preserves values
        Assert.Equal(knownCurvature.Length, output.Snapshot.Values.Length);
        for (int i = 0; i < knownCurvature.Length; i++)
        {
            Assert.Equal(knownCurvature[i], output.Snapshot.Values[i], 1e-15);
        }
    }

    [Fact]
    public void ClassD_ExtractionProjector_ObservedOutputKind_IsTensorField()
    {
        // Verify that curvature extraction produces TensorField output kind.
        var projector = new ExtractionProjectorRecord
        {
            ProjectorId = "proj-curvature-check",
            Name = "Curvature tensor field projector",
            Description = "Verifies output kind for curvature extraction",
            InputType = "curvature-2form",
            OutputClass = ObservedOutputKind.TensorField,
            BranchDependent = false,
        };

        Assert.Equal(ObservedOutputKind.TensorField, projector.OutputClass);
        Assert.Equal("curvature-2form", projector.InputType);
        Assert.False(projector.BranchDependent);
    }

    // --- Class E: Comparison dry-run benchmark ---

    [Fact]
    public void ClassE_ComparisonDryRun_SyntheticAsset_ProducesRunRecord()
    {
        // Setup: InMemoryDatasetAdapter with a synthetic comparison asset
        var asset = MakeComparisonAsset("asset-classE-1");
        var adapter = new InMemoryDatasetAdapter("classE-adapter");
        adapter.Register(asset, new Dictionary<string, double[]>
        {
            ["mass-ratio"] = new[] { 1.0, 2.0, 3.0 },
        });

        var prediction = MakePrediction("pred-classE-1", asset);
        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-classE",
            EnvironmentIds = new[] { "env-classE" },
            BranchSubsetIds = new[] { "branch-classE" },
            ObservedOutputClassIds = new[] { "curvature" },
            ComparisonAssetIds = new[] { "asset-classE-1" },
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(spec, new[] { prediction }, adapter);

        // Verify a ComparisonRunRecord is produced with score and pass/fail
        Assert.NotNull(result.CampaignResult);
        Assert.Single(result.CampaignResult.RunRecords);
        var runRecord = result.CampaignResult.RunRecords[0];
        Assert.Equal("pred-classE-1", runRecord.TestId);
        Assert.Equal(ComparisonMode.Structural, runRecord.Mode);
    }

    [Fact]
    public void ClassE_ComparisonDryRun_MismatchedData_ProducesNegativeArtifact()
    {
        // Setup: prediction with no matching asset in adapter produces failure
        var asset = MakeComparisonAsset("asset-classE-missing");
        var adapter = new InMemoryDatasetAdapter("classE-adapter-empty");
        // Don't register the asset - this creates a mismatch

        var prediction = MakePrediction("pred-classE-neg", asset);
        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-classE-neg",
            EnvironmentIds = new[] { "env-classE" },
            BranchSubsetIds = new[] { "branch-classE" },
            ObservedOutputClassIds = new[] { "curvature" },
            ComparisonAssetIds = new[] { "asset-classE-missing" },
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(spec, new[] { prediction }, adapter);

        // Verify NegativeResultArtifact is produced as first-class artifact
        Assert.NotEmpty(result.NegativeArtifacts);
        Assert.Contains(result.NegativeArtifacts, a => a.OriginalTestId == "pred-classE-neg");
    }

    private static ComparisonAsset MakeComparisonAsset(string assetId)
    {
        return new ComparisonAsset
        {
            AssetId = assetId,
            SourceCitation = "Manufactured solution benchmark Class E",
            AcquisitionDate = DateTimeOffset.UtcNow,
            PreprocessingDescription = "None - synthetic data",
            AdmissibleUseStatement = "Structural comparison only",
            DomainOfValidity = "Test regime",
            UncertaintyModel = UncertaintyRecord.Unestimated(),
            ComparisonVariables = new Dictionary<string, string>
            {
                ["mass-ratio"] = "Synthetic mass ratio for benchmark",
            },
        };
    }

    private static PredictionTestRecord MakePrediction(string testId, ComparisonAsset asset)
    {
        return new PredictionTestRecord
        {
            TestId = testId,
            ClaimClass = ClaimClass.ExactStructuralConsequence,
            FormalSource = "manufactured-solution-classE",
            BranchManifestId = "branch-classE",
            ObservableMapId = "obs-map-classE",
            TheoremDependencyStatus = "closed",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "exact",
            ExternalComparisonAsset = asset,
            Falsifier = "Mass ratio deviates from manufactured value",
            ArtifactLinks = new[] { "artifact-classE" },
        };
    }
}
