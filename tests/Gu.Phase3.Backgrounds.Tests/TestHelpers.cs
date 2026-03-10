using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

internal static class TestHelpers
{
    public static BackgroundSpec MakeSpec(
        string specId,
        string envId = "env-1",
        string branchId = "branch-1",
        BackgroundSeedKind seedKind = BackgroundSeedKind.Trivial)
    {
        return new BackgroundSpec
        {
            SpecId = specId,
            EnvironmentId = envId,
            BranchManifestId = branchId,
            Seed = new BackgroundSeed { Kind = seedKind },
            SolveOptions = new BackgroundSolveOptions
            {
                SolveMode = SolveMode.ResidualOnly,
            },
        };
    }

    public static BackgroundRecord MakeRecord(
        string bgId,
        AdmissibilityLevel level,
        double residualNorm = 1e-5,
        double stationarityNorm = 1e-7)
    {
        return new BackgroundRecord
        {
            BackgroundId = bgId,
            EnvironmentId = "env-1",
            BranchManifestId = "branch-1",
            GeometryFingerprint = "test-fingerprint",
            StateArtifactRef = $"state-{bgId}",
            ResidualNorm = residualNorm,
            StationarityNorm = stationarityNorm,
            AdmissibilityLevel = level,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = residualNorm,
                StationarityNorm = stationarityNorm,
                ObjectiveValue = residualNorm * residualNorm / 2,
                GaugeViolation = 0,
                SolverIterations = 10,
                SolverConverged = true,
                TerminationReason = "converged",
                GaussNewtonValid = level == AdmissibilityLevel.B2,
            },
            ReplayTierAchieved = "R2",
            Provenance = MakeProvenance(),
            RejectionReason = level == AdmissibilityLevel.Rejected ? "test rejection" : null,
        };
    }

    public static ProvenanceMeta MakeProvenance()
    {
        return new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "test-rev",
            Branch = new BranchRef
            {
                BranchId = "test-branch",
                SchemaVersion = "1.0.0",
            },
        };
    }

    public static FieldTensor MakeFieldTensor(int length)
    {
        return new FieldTensor
        {
            Label = "test-tensor",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[length],
            Shape = new[] { length },
        };
    }

    public static BranchManifest MakeManifest(string branchId = "branch-1")
    {
        return new BranchManifest
        {
            BranchId = branchId,
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "test-rev",
            LieAlgebraId = "su2",
            BaseDimension = 2,
            AmbientDimension = 5,
            ActiveGeometryBranch = "simplicial",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "trivial",
            ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            PairingConventionId = "pairing-trace",
            BasisConventionId = "canonical",
            ComponentOrderId = "face-major",
            AdjointConventionId = "adjoint-explicit",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = new[] { "IX-1", "IX-2" },
        };
    }
}
