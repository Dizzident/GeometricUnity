using System.Text.Json.Serialization;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;

namespace Gu.Phase5.Reporting;

public sealed class ReplayFermionModeSnapshot
{
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("eigenvectorCoefficients")]
    public required IReadOnlyList<double> EigenvectorCoefficients { get; init; }
}

public sealed class FullAnalyticWeakCouplingReplayPackage
{
    [JsonPropertyName("packageId")]
    public required string PackageId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("bosonModeSourceKind")]
    public required string BosonModeSourceKind { get; init; }

    [JsonPropertyName("bosonPerturbationVector")]
    public required IReadOnlyList<double> BosonPerturbationVector { get; init; }

    [JsonPropertyName("analyticVariationMatrixRe")]
    public required IReadOnlyList<IReadOnlyList<double>> AnalyticVariationMatrixRe { get; init; }

    [JsonPropertyName("analyticVariationMatrixIm")]
    public IReadOnlyList<IReadOnlyList<double>>? AnalyticVariationMatrixIm { get; init; }

    [JsonPropertyName("fermionModeI")]
    public required ReplayFermionModeSnapshot FermionModeI { get; init; }

    [JsonPropertyName("fermionModeJ")]
    public required ReplayFermionModeSnapshot FermionModeJ { get; init; }

    [JsonPropertyName("couplingRecord")]
    public required BosonFermionCouplingRecord CouplingRecord { get; init; }

    [JsonPropertyName("evidenceBuild")]
    public required ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult EvidenceBuild { get; init; }

    [JsonPropertyName("materializationAudit")]
    public required ProductionAnalyticReplayInputMaterializationAuditResult MaterializationAudit { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class FullAnalyticWeakCouplingReplayPackageBuilder
{
    public const string AlgorithmId = "phase81-full-analytic-weak-coupling-replay-package-builder-v1";

    public static FullAnalyticWeakCouplingReplayPackage Build(
        string packageId,
        string bosonModeSourceKind,
        IReadOnlyList<double> bosonPerturbationVector,
        double[,] analyticVariationMatrixRe,
        double[,]? analyticVariationMatrixIm,
        FermionModeRecord modeI,
        FermionModeRecord modeJ,
        AnalyticWeakCouplingReplayResult replayResult,
        string provenanceId)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        ArgumentNullException.ThrowIfNull(bosonModeSourceKind);
        ArgumentNullException.ThrowIfNull(bosonPerturbationVector);
        ArgumentNullException.ThrowIfNull(analyticVariationMatrixRe);
        ArgumentNullException.ThrowIfNull(modeI);
        ArgumentNullException.ThrowIfNull(modeJ);
        ArgumentNullException.ThrowIfNull(replayResult);
        ArgumentNullException.ThrowIfNull(provenanceId);

        var closure = new List<string>();
        if (!string.Equals(replayResult.TerminalStatus, "analytic-weak-coupling-replay-validated", StringComparison.Ordinal))
            closure.Add("analytic weak-coupling replay has not validated");
        if (replayResult.CouplingRecord is null)
            closure.Add("full coupling record is missing");
        if (replayResult.EvidenceBuild is null)
            closure.Add("replayed evidence build is missing");
        if (bosonPerturbationVector.Count == 0 || bosonPerturbationVector.Any(v => !double.IsFinite(v)))
            closure.Add("boson perturbation vector must be non-empty and finite");
        if (modeI.EigenvectorCoefficients is null || modeI.EigenvectorCoefficients.Length == 0)
            closure.Add("fermion mode I eigenvector coefficients are missing");
        if (modeJ.EigenvectorCoefficients is null || modeJ.EigenvectorCoefficients.Length == 0)
            closure.Add("fermion mode J eigenvector coefficients are missing");

        var audit = ProductionAnalyticReplayInputMaterializationAuditor.Audit(
            new ProductionAnalyticReplayInputMaterializationRecord
            {
                ArtifactId = packageId,
                BosonModeSourceKind = bosonModeSourceKind,
                HasBosonPerturbationVector = bosonPerturbationVector.Count > 0,
                HasAnalyticVariationMatrix = analyticVariationMatrixRe.Length > 0,
                HasFermionModeEigenvectors = modeI.EigenvectorCoefficients is { Length: > 0 } &&
                    modeJ.EigenvectorCoefficients is { Length: > 0 },
                HasFullCouplingRecord = replayResult.CouplingRecord is not null,
                IsTopCouplingSummaryOnly = false,
                VariationMethod = replayResult.CouplingRecord?.VariationMethod,
                NormalizationConvention = replayResult.CouplingRecord?.NormalizationConvention,
                VariationEvidenceId = replayResult.CouplingRecord?.VariationEvidenceId,
                ProvenanceId = provenanceId,
            });
        closure.AddRange(audit.ClosureRequirements);

        return new FullAnalyticWeakCouplingReplayPackage
        {
            PackageId = packageId,
            BosonModeId = replayResult.CouplingRecord?.BosonModeId ?? string.Empty,
            BosonModeSourceKind = bosonModeSourceKind,
            BosonPerturbationVector = bosonPerturbationVector,
            AnalyticVariationMatrixRe = ToRows(analyticVariationMatrixRe),
            AnalyticVariationMatrixIm = analyticVariationMatrixIm is null ? null : ToRows(analyticVariationMatrixIm),
            FermionModeI = new ReplayFermionModeSnapshot
            {
                ModeId = modeI.ModeId,
                EigenvectorCoefficients = modeI.EigenvectorCoefficients ?? [],
            },
            FermionModeJ = new ReplayFermionModeSnapshot
            {
                ModeId = modeJ.ModeId,
                EigenvectorCoefficients = modeJ.EigenvectorCoefficients ?? [],
            },
            CouplingRecord = replayResult.CouplingRecord ?? EmptyCouplingRecord(),
            EvidenceBuild = replayResult.EvidenceBuild ?? EmptyEvidenceBuild(),
            MaterializationAudit = audit,
            ClosureRequirements = closure,
        };
    }

    private static IReadOnlyList<IReadOnlyList<double>> ToRows(double[,] matrix)
    {
        var rows = new List<IReadOnlyList<double>>();
        for (var r = 0; r < matrix.GetLength(0); r++)
        {
            var row = new double[matrix.GetLength(1)];
            for (var c = 0; c < matrix.GetLength(1); c++)
                row[c] = matrix[r, c];
            rows.Add(row);
        }

        return rows;
    }

    private static BosonFermionCouplingRecord EmptyCouplingRecord() => new()
    {
        CouplingId = "missing-coupling-record",
        BosonModeId = "missing-boson-mode",
        FermionModeIdI = "missing-fermion-i",
        FermionModeIdJ = "missing-fermion-j",
        CouplingProxyReal = 0.0,
        CouplingProxyImaginary = 0.0,
        CouplingProxyMagnitude = 0.0,
        NormalizationConvention = "missing",
        SelectionRuleNotes = ["missing coupling record"],
        Provenance = new Gu.Core.ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UnixEpoch,
            CodeRevision = "missing",
            Branch = new Gu.Core.BranchRef { BranchId = "missing", SchemaVersion = "1.0" },
            Backend = "missing",
        },
    };

    private static ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult EmptyEvidenceBuild() => new()
    {
        AlgorithmId = ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.AlgorithmId,
        TerminalStatus = "replayed-raw-weak-coupling-matrix-element-evidence-blocked",
        SourceCouplingId = "missing-coupling-record",
        EvidenceValidation = RawWeakCouplingMatrixElementEvidenceValidator.Validate(new RawWeakCouplingMatrixElementEvidenceRecord
        {
            EvidenceId = "missing-evidence",
            SourceKind = "missing",
            VariationMethod = "missing",
            NormalizationConvention = "missing",
            RawMatrixElementMagnitude = double.NaN,
            UsesFiniteDifferenceProxy = true,
            UsesCouplingProxyMagnitude = true,
            ReplayedFromCouplingRecordId = null,
            VariationEvidenceId = null,
            ProvenanceId = null,
        }),
        ClosureRequirements = ["missing replayed evidence build"],
    };
}
