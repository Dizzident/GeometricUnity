using Gu.Branching;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Reporting;
using Gu.ReferenceCpu;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// Tests for P11-M9: Shiab family scope record and classifier.
///
/// Verifies:
/// - ShiabFamilyScopeRecord carries the required fields
/// - ShiabFamilyScopeClassifier correctly identifies identical operators as non-distinct
/// - The family-open statement always begins with "FAMILY-OPEN:"
/// - The expansion-blocked reason cites artifact-backed evidence
/// - Phase5Report carries the shiabFamilyScope field
/// - Phase5ReportGenerator.ToMarkdown includes the Shiab scope section
/// </summary>
public sealed class ShiabFamilyScopeTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase11-m9-v1",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static BranchRobustnessRecord MakeMixedRecord(string studyId)
    {
        var spec = new BranchRobustnessStudySpec
        {
            StudyId = studyId,
            BranchVariantIds = ["V1", "V2", "V3", "V4"],
            TargetQuantityIds = ["residual-norm", "stationarity-norm", "objective-value", "gauge-violation", "solver-iterations"],
        };
        var engine = new BranchRobustnessEngine(spec);
        // Make gauge-violation and solver-iterations fragile, others invariant
        return engine.Run(
            new Dictionary<string, double[]>
            {
                ["residual-norm"]      = [1.0, 1.0, 1.0, 1.0],
                ["stationarity-norm"]  = [0.001, 0.001, 0.001, 0.001],
                ["objective-value"]    = [0.0, 0.0, 0.0, 0.0],
                ["gauge-violation"]    = [0.1, 5.0, 0.2, 8.0],   // fragile
                ["solver-iterations"]  = [100.0, 1.0, 200.0, 1.0], // fragile
            },
            MakeProvenance());
    }

    // ===== ShiabFamilyScopeRecord field tests =====

    [Fact]
    public void ShiabFamilyScopeRecord_HasRequiredFields()
    {
        var record = new ShiabFamilyScopeRecord
        {
            RecordId = "test-scope-001",
            StandardPathShiabIds = ["identity-shiab"],
            PairedPathShiabIds = ["first-order-curvature"],
            OperatorsAreMathematicallyDistinct = false,
            StandardPathBranchResult = "mixed",
            PairedPathBranchResult = "mixed",
            PairedPathChangesConclusion = false,
            FamilyOpenStatement = "FAMILY-OPEN: test statement",
            Provenance = MakeProvenance(),
        };

        Assert.Equal("test-scope-001", record.RecordId);
        Assert.Equal("1.0.0", record.SchemaVersion);
        Assert.Single(record.StandardPathShiabIds);
        Assert.Single(record.PairedPathShiabIds);
        Assert.False(record.OperatorsAreMathematicallyDistinct);
        Assert.Equal("mixed", record.StandardPathBranchResult);
        Assert.Equal("mixed", record.PairedPathBranchResult);
        Assert.False(record.PairedPathChangesConclusion);
        Assert.StartsWith("FAMILY-OPEN:", record.FamilyOpenStatement);
    }

    [Fact]
    public void ShiabFamilyScopeRecord_ExpansionBlockedReason_CanBeNull()
    {
        var record = new ShiabFamilyScopeRecord
        {
            RecordId = "test-scope-002",
            StandardPathShiabIds = ["identity-shiab"],
            PairedPathShiabIds = [],
            OperatorsAreMathematicallyDistinct = false,
            StandardPathBranchResult = "mixed",
            PairedPathBranchResult = "not-evaluated",
            PairedPathChangesConclusion = false,
            ExpansionBlockedReason = null,
            FamilyOpenStatement = "FAMILY-OPEN: test",
            Provenance = MakeProvenance(),
        };

        Assert.Null(record.ExpansionBlockedReason);
    }

    // ===== ShiabFamilyScopeClassifier tests =====

    [Fact]
    public void Classifier_BothMixed_OperatorsNotDistinct_PairedDoesNotChangeConclusion()
    {
        var standardRecord = MakeMixedRecord("phase9-standard");
        var pairedRecord = MakeMixedRecord("phase9-paired");

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord,
            pairedRecord,
            standardShiabIds: ["identity-shiab"],
            pairedShiabIds: ["first-order-curvature"],
            supportingArtifactPaths: null,
            provenance: MakeProvenance());

        Assert.False(scope.OperatorsAreMathematicallyDistinct,
            "identity-shiab and first-order-curvature both implement S=F and are not mathematically distinct.");
        Assert.Equal("mixed", scope.StandardPathBranchResult);
        Assert.Equal("mixed", scope.PairedPathBranchResult);
        Assert.False(scope.PairedPathChangesConclusion,
            "Both paths produce the same 'mixed' result, so paired does not change the conclusion.");
    }

    [Fact]
    public void Classifier_FamilyOpenStatement_StartsWithFamilyOpen()
    {
        var standardRecord = MakeMixedRecord("standard");
        var pairedRecord = MakeMixedRecord("paired");

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        Assert.StartsWith("FAMILY-OPEN:", scope.FamilyOpenStatement);
    }

    [Fact]
    public void Classifier_ExpansionBlockedReason_MentionsIdenticalImplementation()
    {
        var standardRecord = MakeMixedRecord("standard");
        var pairedRecord = MakeMixedRecord("paired");

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        Assert.NotNull(scope.ExpansionBlockedReason);
        Assert.Contains("S=F", scope.ExpansionBlockedReason);
    }

    [Fact]
    public void Classifier_ExpansionBlockedReason_MentionsPhysicsGuidance()
    {
        var standardRecord = MakeMixedRecord("standard");
        var pairedRecord = MakeMixedRecord("paired");

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        Assert.NotNull(scope.ExpansionBlockedReason);
        Assert.Contains("physics guidance", scope.ExpansionBlockedReason);
    }

    [Fact]
    public void Classifier_SupportingArtifactPaths_ArePropagated()
    {
        var standardRecord = MakeMixedRecord("standard");
        var pairedRecord = MakeMixedRecord("paired");
        var paths = new[] { "path/to/standard_branch.json", "path/to/paired_branch.json" };

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            paths, MakeProvenance());

        Assert.NotNull(scope.SupportingArtifactPaths);
        Assert.Equal(2, scope.SupportingArtifactPaths!.Count);
    }

    [Fact]
    public void ClassifyStandardOnly_PairedResult_IsNotEvaluated()
    {
        var standardRecord = MakeMixedRecord("standard");

        var scope = ShiabFamilyScopeClassifier.ClassifyStandardOnly(
            standardRecord,
            standardShiabIds: ["identity-shiab"],
            supportingArtifactPaths: null,
            provenance: MakeProvenance());

        Assert.Equal("not-evaluated", scope.PairedPathBranchResult);
        Assert.Empty(scope.PairedPathShiabIds);
        Assert.False(scope.PairedPathChangesConclusion);
        Assert.StartsWith("FAMILY-OPEN:", scope.FamilyOpenStatement);
    }

    [Fact]
    public void Classifier_RecordId_IsGenerated()
    {
        var standardRecord = MakeMixedRecord("s");
        var pairedRecord = MakeMixedRecord("p");

        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        Assert.NotEmpty(scope.RecordId);
        Assert.StartsWith("shiab-scope-", scope.RecordId);
    }

    // ===== Phase5Report carries ShiabFamilyScope =====

    [Fact]
    public void Phase5Report_ShiabFamilyScope_IsNullByDefault()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());

        Assert.Null(report.ShiabFamilyScope);
    }

    [Fact]
    public void Phase5ReportGenerator_WithShiabScope_PopulatesField()
    {
        var standardRecord = MakeMixedRecord("std");
        var pairedRecord = MakeMixedRecord("paired");
        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            shiabFamilyScope: scope);

        Assert.NotNull(report.ShiabFamilyScope);
        Assert.Equal("mixed", report.ShiabFamilyScope!.StandardPathBranchResult);
        Assert.False(report.ShiabFamilyScope.OperatorsAreMathematicallyDistinct);
    }

    [Fact]
    public void ToMarkdown_WithShiabScope_IncludesShiabFamilyScopeSection()
    {
        var standardRecord = MakeMixedRecord("std");
        var pairedRecord = MakeMixedRecord("paired");
        var scope = ShiabFamilyScopeClassifier.Classify(
            standardRecord, pairedRecord,
            ["identity-shiab"], ["first-order-curvature"],
            null, MakeProvenance());

        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            shiabFamilyScope: scope);

        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("## Shiab Family Scope", md);
        Assert.Contains("identity-shiab", md);
        Assert.Contains("first-order-curvature", md);
        Assert.Contains("FAMILY-OPEN:", md);
    }

    [Fact]
    public void ToMarkdown_WithoutShiabScope_NoShiabFamilyScopeSection()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());
        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.DoesNotContain("## Shiab Family Scope", md);
    }

    // ===== ShiabFamilyScopeChecker tests (architect spec, P11-M9) =====

    private static (SimplicialMesh mesh, LieAlgebra algebra) MakeCheckerContext()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        return (mesh, algebra);
    }

    private static (FieldTensor curvature, FieldTensor omega) MakeTestFields(
        SimplicialMesh mesh, LieAlgebra algebra)
    {
        int dimG = algebra.Dimension;
        int edgeCount = mesh.EdgeCount;

        // Nontrivial omega so curvature is nonzero
        var omegaCoeffs = new double[edgeCount * dimG];
        for (int i = 0; i < omegaCoeffs.Length; i++)
            omegaCoeffs[i] = (i % dimG == 0) ? 0.3 : 0.1;

        var omegaConn = new ConnectionField(mesh, algebra, omegaCoeffs);
        var curvature = CurvatureAssembler.Assemble(omegaConn).ToFieldTensor();

        var connectionSignature = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = algebra.BasisOrderId,
            ComponentOrderId = "edge-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };
        var omega = new FieldTensor
        {
            Label = "omega",
            Signature = connectionSignature,
            Coefficients = omegaCoeffs,
            Shape = new[] { edgeCount, dimG },
        };

        return (curvature, omega);
    }

    private static (BranchManifest manifest, GeometryContext geometry) MakeManifestAndGeometry()
    {
        var manifest = BranchManifestFactory.CreateEmpty("shiab-checker-test");
        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            Patches = Array.Empty<PatchInfo>(),
        };
        return (manifest, geometry);
    }

    [Fact]
    public void ShiabFamilyScopeChecker_IdentityAndFirstOrder_AreIdentical()
    {
        // Per D-P11-009: both current Shiab operators implement S=F.
        // The numerical checker must confirm they produce identical output (max diff < 1e-14).
        var (mesh, algebra) = MakeCheckerContext();
        var (curvature, omega) = MakeTestFields(mesh, algebra);
        var (manifest, geometry) = MakeManifestAndGeometry();

        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var record = checker.Check(
            standardOp, pairedOp, curvature, omega, manifest, geometry, MakeProvenance());

        // Both operators implement S=F: output must be numerically identical
        Assert.False(record.OperatorsAreMathematicallyDistinct,
            "identity-shiab and first-order-curvature both implement S=F; they must not be distinct.");
        Assert.Contains("identity-equivalent", record.FamilyOpenStatement);
        Assert.StartsWith("FAMILY-OPEN:", record.FamilyOpenStatement);
    }

    [Fact]
    public void ShiabFamilyScopeRecord_Serialization_RoundTrips()
    {
        var (mesh, algebra) = MakeCheckerContext();
        var (curvature, omega) = MakeTestFields(mesh, algebra);
        var (manifest, geometry) = MakeManifestAndGeometry();

        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var original = checker.Check(
            standardOp, pairedOp, curvature, omega, manifest, geometry, MakeProvenance());

        var json = GuJsonDefaults.Serialize(original);
        var roundTripped = GuJsonDefaults.Deserialize<ShiabFamilyScopeRecord>(json);

        Assert.NotNull(roundTripped);
        Assert.Equal(original.RecordId, roundTripped!.RecordId);
        Assert.Equal(original.OperatorsAreMathematicallyDistinct, roundTripped.OperatorsAreMathematicallyDistinct);
        Assert.Equal(original.FamilyOpenStatement, roundTripped.FamilyOpenStatement);
        Assert.Equal(original.StandardPathShiabIds.Count, roundTripped.StandardPathShiabIds.Count);
        Assert.Equal(original.PairedPathShiabIds.Count, roundTripped.PairedPathShiabIds.Count);
    }

    [Fact]
    public void ShiabFamilyScopeRecord_BlockerIsNonEmpty()
    {
        // The expansion-blocked reason must be non-empty and cite the S=F implementation
        // and the need for physics guidance (per D-P11-009 and M9 DoD).
        var (mesh, algebra) = MakeCheckerContext();
        var (curvature, omega) = MakeTestFields(mesh, algebra);
        var (manifest, geometry) = MakeManifestAndGeometry();

        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var record = checker.Check(
            standardOp, pairedOp, curvature, omega, manifest, geometry, MakeProvenance());

        Assert.NotNull(record.ExpansionBlockedReason);
        Assert.NotEmpty(record.ExpansionBlockedReason!);
        Assert.Contains("S=F", record.ExpansionBlockedReason,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("physics guidance", record.ExpansionBlockedReason,
            StringComparison.OrdinalIgnoreCase);
    }

    // ===== MetricScaledShiabOperator + CheckThree tests (P11-M6) =====

    private static (SimplicialMesh mesh, LieAlgebra algebra, FieldTensor curvature, FieldTensor omega,
        BranchManifest manifest, GeometryContext geometry) BuildThreeOpTestFixture()
    {
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        int dimG = algebra.Dimension;
        var omegaCoeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < omegaCoeffs.Length; i++)
            omegaCoeffs[i] = (i % dimG == 0) ? 0.3 : 0.1;

        var omegaConn = new ConnectionField(mesh, algebra, omegaCoeffs);
        var curvature = CurvatureAssembler.Assemble(omegaConn).ToFieldTensor();

        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
            LieAlgebraBasisId = algebra.BasisOrderId, ComponentOrderId = "edge-major",
            NumericPrecision = "float64", MemoryLayout = "dense-row-major",
        };
        var omega = new FieldTensor
        {
            Label = "omega", Signature = sig, Coefficients = omegaCoeffs,
            Shape = new[] { mesh.EdgeCount, dimG },
        };

        var manifest = BranchManifestFactory.CreateEmpty("test-shiab-scope");
        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            Patches = Array.Empty<PatchInfo>(),
        };

        return (mesh, algebra, curvature, omega, manifest, geometry);
    }

    [Fact]
    public void MetricScaledShiabOperator_Lambda2_IsDistinctFromIdentity()
    {
        var (mesh, algebra, curvature, omega, manifest, geometry) = BuildThreeOpTestFixture();
        var identity = new IdentityShiabCpu(mesh, algebra);
        var scaled = new MetricScaledShiabOperator(mesh, algebra, 2.0);

        var identityOut = identity.Evaluate(curvature, omega, manifest, geometry);
        var scaledOut = scaled.Evaluate(curvature, omega, manifest, geometry);

        // lambda=2 means each coefficient is 2x, so max diff should equal max magnitude
        double maxDiff = 0.0;
        for (int i = 0; i < identityOut.Coefficients.Length; i++)
            maxDiff = System.Math.Max(maxDiff, System.Math.Abs(identityOut.Coefficients[i] - scaledOut.Coefficients[i]));

        Assert.True(maxDiff > ShiabFamilyScopeChecker.IdentityThreshold,
            "MetricScaledShiabOperator(lambda=2) must be numerically distinct from identity-shiab.");
    }

    [Fact]
    public void MetricScaledShiabOperator_Lambda1_IsIdenticalToIdentityShiab()
    {
        var (mesh, algebra, curvature, omega, manifest, geometry) = BuildThreeOpTestFixture();
        var identity = new IdentityShiabCpu(mesh, algebra);
        var scaled = new MetricScaledShiabOperator(mesh, algebra, 1.0);

        var identityOut = identity.Evaluate(curvature, omega, manifest, geometry);
        var scaledOut = scaled.Evaluate(curvature, omega, manifest, geometry);

        double maxDiff = 0.0;
        for (int i = 0; i < identityOut.Coefficients.Length; i++)
            maxDiff = System.Math.Max(maxDiff, System.Math.Abs(identityOut.Coefficients[i] - scaledOut.Coefficients[i]));

        Assert.True(maxDiff < ShiabFamilyScopeChecker.IdentityThreshold,
            "MetricScaledShiabOperator(lambda=1) must be identity-equivalent to identity-shiab.");
    }

    [Fact]
    public void CheckThree_WithDistinctThird_ReturnsOperatorsAreMathematicallyDistinctTrue()
    {
        var (mesh, algebra, curvature, omega, manifest, geometry) = BuildThreeOpTestFixture();
        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var thirdOp = new MetricScaledShiabOperator(mesh, algebra, 2.0);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var record = checker.CheckThree(standardOp, pairedOp, thirdOp, curvature, omega, manifest, geometry, MakeProvenance());

        Assert.True(record.OperatorsAreMathematicallyDistinct,
            "Three-operator check with lambda=2 must be mathematically distinct.");
        Assert.Contains("metric-scaled-shiab", record.PairedPathShiabIds);
        Assert.StartsWith("FAMILY-OPEN:", record.FamilyOpenStatement);
    }

    [Fact]
    public void CheckThree_WithDistinctThird_ExpansionBlockedReasonIsNull()
    {
        var (mesh, algebra, curvature, omega, manifest, geometry) = BuildThreeOpTestFixture();
        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var thirdOp = new MetricScaledShiabOperator(mesh, algebra, 2.0);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var record = checker.CheckThree(standardOp, pairedOp, thirdOp, curvature, omega, manifest, geometry, MakeProvenance());

        // When hasGenuinelyDistinctThird=true, ExpansionBlockedReason is set to null
        Assert.Null(record.ExpansionBlockedReason);
    }

    [Fact]
    public void CheckThree_FamilyOpenStatement_MentionsDimXConstraint()
    {
        var (mesh, algebra, curvature, omega, manifest, geometry) = BuildThreeOpTestFixture();
        var standardOp = new IdentityShiabCpu(mesh, algebra);
        var pairedOp = new FirstOrderShiabOperator(mesh, algebra);
        var thirdOp = new MetricScaledShiabOperator(mesh, algebra, 2.0);
        var checker = new ShiabFamilyScopeChecker(mesh, algebra);

        var record = checker.CheckThree(standardOp, pairedOp, thirdOp, curvature, omega, manifest, geometry, MakeProvenance());

        Assert.Contains("dimX=2", record.FamilyOpenStatement, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("canonically selected draft operator", record.FamilyOpenStatement, StringComparison.OrdinalIgnoreCase);
    }

}
