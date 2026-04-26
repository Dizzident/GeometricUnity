using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzPhysicalPredictionArtifactPromotionResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("physicalModeRecords")]
    public required IReadOnlyList<IdentifiedPhysicalModeRecord> PhysicalModeRecords { get; init; }

    [JsonPropertyName("modeIdentificationEvidence")]
    public required ModeIdentificationEvidenceTable ModeIdentificationEvidence { get; init; }

    [JsonPropertyName("observables")]
    public required IReadOnlyList<QuantitativeObservableRecord> Observables { get; init; }

    [JsonPropertyName("physicalObservableMappings")]
    public required PhysicalObservableMappingTable PhysicalObservableMappings { get; init; }

    [JsonPropertyName("observableClassifications")]
    public required ObservableClassificationTable ObservableClassifications { get; init; }

    [JsonPropertyName("physicalCalibrations")]
    public required PhysicalCalibrationTable PhysicalCalibrations { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public static class WzPhysicalPredictionArtifactPromoter
{
    public const string AlgorithmId = "p28-wz-identity-rule-physical-artifact-promoter:v1";

    public static WzPhysicalPredictionArtifactPromotionResult Promote(
        string identityRuleReadinessJson,
        string candidateModeSourcesJson,
        string modeFamiliesJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityRuleReadinessJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateModeSourcesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);

        var readiness = GuJsonDefaults.Deserialize<VectorBosonIdentityRuleReadinessResult>(identityRuleReadinessJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z identity-rule readiness.");
        var bridge = GuJsonDefaults.Deserialize<CandidateModeSourceBridgeTable>(candidateModeSourcesJson)
            ?? throw new InvalidDataException("Failed to deserialize candidate-mode sources.");
        var uncertaintyBySourceCandidateId = LoadFamilyUncertainties(modeFamiliesJson);

        var closure = new List<string>();
        if (!string.Equals(readiness.TerminalStatus, "identity-rule-ready", StringComparison.Ordinal))
            closure.Add("provide identity-rule-ready W/Z readiness before promoting physical prediction artifacts");

        var wRule = readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "w-boson");
        var zRule = readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "z-boson");
        if (wRule is null)
            closure.Add("identity readiness must provide a W-boson rule");
        if (zRule is null)
            closure.Add("identity readiness must provide a Z-boson rule");

        var wSource = wRule is null ? null : FindSource(bridge.CandidateModeSources, wRule);
        var zSource = zRule is null ? null : FindSource(bridge.CandidateModeSources, zRule);
        if (wSource is null)
            closure.Add("W identity rule source is absent from candidate-mode sources");
        if (zSource is null)
            closure.Add("Z identity rule source is absent from candidate-mode sources");

        if (wSource is not null && zSource is not null)
        {
            if (!string.Equals(wSource.EnvironmentId, zSource.EnvironmentId, StringComparison.Ordinal))
                closure.Add("W and Z source modes must share an environment before ratio extraction");
            if (!string.Equals(wSource.BranchId, zSource.BranchId, StringComparison.Ordinal))
                closure.Add("W and Z source modes must share a branch before ratio extraction");
            if (!string.Equals(wSource.RefinementLevel, zSource.RefinementLevel, StringComparison.Ordinal))
                closure.Add("W and Z source modes must share a refinement level before ratio extraction");
            if (!string.Equals(wSource.UnitFamily, zSource.UnitFamily, StringComparison.Ordinal) ||
                !string.Equals(wSource.Unit, zSource.Unit, StringComparison.Ordinal))
                closure.Add("W and Z source modes must share mass-like units before ratio extraction");
        }

        if (closure.Count > 0 || wRule is null || zRule is null || wSource is null || zSource is null)
            return Blocked(closure, provenance);

        var wUncertainty = GetUncertainty(uncertaintyBySourceCandidateId, wSource);
        var zUncertainty = GetUncertainty(uncertaintyBySourceCandidateId, zSource);
        if (wUncertainty is null)
            closure.Add($"missing decomposed uncertainty for W source '{wSource.SourceId}'");
        if (zUncertainty is null)
            closure.Add($"missing decomposed uncertainty for Z source '{zSource.SourceId}'");
        if (closure.Count > 0 || wUncertainty is null || zUncertainty is null)
            return Blocked(closure, provenance);

        var wMode = CreateMode(wSource, wRule, provenance);
        var zMode = CreateMode(zSource, zRule, provenance);
        var observable = CreateRatioObservable(wSource, zSource, wUncertainty, zUncertainty, provenance);
        var mapping = CreateMapping(observable);
        var classification = CreateClassification(observable);
        var calibration = CreateCalibration(observable, mapping);

        return new WzPhysicalPredictionArtifactPromotionResult
        {
            ResultId = "phase28-wz-physical-prediction-artifact-promotion-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "physical-prediction-artifacts-ready",
            AlgorithmId = AlgorithmId,
            PhysicalModeRecords = [wMode, zMode],
            ModeIdentificationEvidence = new ModeIdentificationEvidenceTable
            {
                TableId = "phase28-wz-mode-identification-evidence-v1",
                Evidence =
                [
                    CreateEvidence(wMode, wSource, wRule, provenance),
                    CreateEvidence(zMode, zSource, zRule, provenance),
                ],
            },
            Observables = [observable],
            PhysicalObservableMappings = new PhysicalObservableMappingTable
            {
                TableId = "phase28-wz-physical-observable-mappings-v1",
                Mappings = [mapping],
            },
            ObservableClassifications = new ObservableClassificationTable
            {
                TableId = "phase28-wz-observable-classifications-v1",
                Classifications = [classification],
            },
            PhysicalCalibrations = new PhysicalCalibrationTable
            {
                TableId = "phase28-wz-physical-calibrations-v1",
                Calibrations = [calibration],
            },
            ClosureRequirements = [],
            Provenance = provenance,
        };
    }

    private static WzPhysicalPredictionArtifactPromotionResult Blocked(IReadOnlyList<string> closure, ProvenanceMeta provenance)
        => new()
        {
            ResultId = "phase28-wz-physical-prediction-artifact-promotion-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "physical-prediction-artifact-promotion-blocked",
            AlgorithmId = AlgorithmId,
            PhysicalModeRecords = [],
            ModeIdentificationEvidence = new ModeIdentificationEvidenceTable
            {
                TableId = "phase28-wz-mode-identification-evidence-v1",
                Evidence = [],
            },
            Observables = [],
            PhysicalObservableMappings = new PhysicalObservableMappingTable
            {
                TableId = "phase28-wz-physical-observable-mappings-v1",
                Mappings = [],
            },
            ObservableClassifications = new ObservableClassificationTable
            {
                TableId = "phase28-wz-observable-classifications-v1",
                Classifications = [],
            },
            PhysicalCalibrations = new PhysicalCalibrationTable
            {
                TableId = "phase28-wz-physical-calibrations-v1",
                Calibrations = [],
            },
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };

    private static CandidateModeSourceRecord? FindSource(
        IReadOnlyList<CandidateModeSourceRecord> sources,
        VectorBosonIdentityRuleRecord rule)
        => sources.FirstOrDefault(source =>
            string.Equals(source.SourceId, rule.SourceId, StringComparison.Ordinal) ||
            string.Equals(source.SourceObservableId, rule.SourceObservableId, StringComparison.Ordinal));

    private static IdentifiedPhysicalModeRecord CreateMode(
        CandidateModeSourceRecord source,
        VectorBosonIdentityRuleRecord rule,
        ProvenanceMeta provenance)
        => new()
        {
            ModeId = source.SourceId,
            ParticleId = rule.ParticleId,
            ModeKind = "vector-boson-mass-mode",
            ObservableId = source.SourceObservableId,
            Value = source.Value,
            Uncertainty = source.Uncertainty,
            UnitFamily = source.UnitFamily,
            Unit = source.Unit,
            Status = "validated",
            EnvironmentId = source.EnvironmentId,
            BranchId = source.BranchId,
            RefinementLevel = source.RefinementLevel,
            ExtractionMethod = $"{AlgorithmId}:{rule.RuleId}:{source.SourceExtractionMethod}",
            Assumptions =
            [
                "mode identity follows the P24/P27 internal W/Z identity-rule readiness result",
                "external physical target values were not used to select this source mode",
            ],
            ClosureRequirements = [],
            Provenance = provenance,
        };

    private static ModeIdentificationEvidenceRecord CreateEvidence(
        IdentifiedPhysicalModeRecord mode,
        CandidateModeSourceRecord source,
        VectorBosonIdentityRuleRecord rule,
        ProvenanceMeta provenance)
        => new()
        {
            EvidenceId = $"phase28-{mode.ParticleId}-mode-identification-evidence",
            ModeId = mode.ModeId,
            ParticleId = mode.ParticleId,
            ModeKind = mode.ModeKind,
            SourceObservableIds = [source.SourceObservableId],
            EnvironmentId = mode.EnvironmentId,
            BranchId = mode.BranchId,
            RefinementLevel = mode.RefinementLevel,
            DerivationId = $"{AlgorithmId}:{rule.DerivationId}",
            Status = "validated",
            Assumptions =
            [
                "evidence is derived from internal electroweak identity features and charge-sector convention",
                "external physical target values were not used to validate this identity evidence",
            ],
            ClosureRequirements = [],
            Provenance = provenance,
        };

    private static QuantitativeObservableRecord CreateRatioObservable(
        CandidateModeSourceRecord wSource,
        CandidateModeSourceRecord zSource,
        QuantitativeUncertainty wUncertainty,
        QuantitativeUncertainty zUncertainty,
        ProvenanceMeta provenance)
    {
        var ratio = SpectrumObservableExtractor.ComputePositiveModeRatio(wSource.Value, zSource.Value);
        var branch = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.BranchVariation, zSource.Value, zUncertainty.BranchVariation);
        var refinement = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.RefinementError, zSource.Value, zUncertainty.RefinementError);
        var extraction = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.ExtractionError, zSource.Value, zUncertainty.ExtractionError);
        var environment = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.EnvironmentSensitivity, zSource.Value, zUncertainty.EnvironmentSensitivity);
        var total = System.Math.Sqrt(
            branch * branch +
            refinement * refinement +
            extraction * extraction +
            environment * environment);

        return new QuantitativeObservableRecord
        {
            ObservableId = "physical-w-z-mass-ratio",
            Value = ratio,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = branch,
                RefinementError = refinement,
                ExtractionError = extraction,
                EnvironmentSensitivity = environment,
                TotalUncertainty = total,
            },
            BranchId = wSource.BranchId,
            EnvironmentId = wSource.EnvironmentId,
            RefinementLevel = wSource.RefinementLevel,
            ExtractionMethod = $"positive-mode-ratio:{wSource.SourceObservableId}/{zSource.SourceObservableId}:{AlgorithmId}",
            Provenance = provenance,
        };
    }

    private static PhysicalObservableMapping CreateMapping(QuantitativeObservableRecord observable)
        => new()
        {
            MappingId = "phase28-w-z-vector-mode-ratio-to-physical-mass-ratio",
            ParticleId = "electroweak-sector",
            PhysicalObservableType = "mass-ratio",
            SourceComputedObservableId = observable.ObservableId,
            TargetPhysicalObservableId = "physical-w-z-mass-ratio",
            RequiredEnvironmentId = observable.EnvironmentId,
            RequiredBranchId = observable.BranchId,
            UnitFamily = "dimensionless",
            Status = "validated",
            Assumptions =
            [
                "source observable is a dimensionless W/Z mode ratio from internal identity-rule-ready modes",
                "external physical target values were not used to construct the mapping",
            ],
            ClosureRequirements = [],
        };

    private static ObservableClassification CreateClassification(QuantitativeObservableRecord observable)
        => new()
        {
            ObservableId = observable.ObservableId,
            Classification = "physical-observable",
            Rationale = "P27 identity rules validate the W/Z mode identities, and P28 computes the dimensionless W/Z ratio from those internal modes.",
            PhysicalClaimAllowed = true,
        };

    private static PhysicalCalibrationRecord CreateCalibration(
        QuantitativeObservableRecord observable,
        PhysicalObservableMapping mapping)
        => new()
        {
            CalibrationId = "phase28-dimensionless-wz-ratio-identity-normalization",
            MappingId = mapping.MappingId,
            SourceComputedObservableId = observable.ObservableId,
            SourceUnitFamily = "dimensionless",
            TargetUnitFamily = "dimensionless",
            TargetUnit = "dimensionless",
            ScaleFactor = 1.0,
            ScaleUncertainty = 0.0,
            Status = "validated",
            Method = "dimensionless-identity-normalization-from-validated-wz-ratio",
            Source = "phase28-wz-identity-rule-physical-artifact-promoter",
            Assumptions =
            [
                "dimensionless ratio comparison requires no physical unit scale conversion",
                "identity normalization applies only to the ratio, not to either absolute boson mass",
            ],
            ClosureRequirements = [],
        };

    private static double PropagateRatioComponent(
        double ratio,
        double numerator,
        double numeratorUncertainty,
        double denominator,
        double denominatorUncertainty)
    {
        if (numeratorUncertainty < 0 || denominatorUncertainty < 0)
            return -1;
        return ratio * System.Math.Sqrt(
            System.Math.Pow(numeratorUncertainty / numerator, 2) +
            System.Math.Pow(denominatorUncertainty / denominator, 2));
    }

    private static QuantitativeUncertainty? GetUncertainty(
        IReadOnlyDictionary<string, QuantitativeUncertainty> uncertainties,
        CandidateModeSourceRecord source)
    {
        var candidateId = source.SourceId.StartsWith("phase22-", StringComparison.Ordinal)
            ? source.SourceId["phase22-".Length..]
            : source.SourceId;
        return uncertainties.TryGetValue(candidateId, out var uncertainty)
            ? uncertainty
            : null;
    }

    private static IReadOnlyDictionary<string, QuantitativeUncertainty> LoadFamilyUncertainties(string modeFamiliesJson)
    {
        using var doc = JsonDocument.Parse(modeFamiliesJson);
        var families = doc.RootElement.TryGetProperty("families", out var familyArray) &&
            familyArray.ValueKind == JsonValueKind.Array
                ? familyArray.EnumerateArray()
                : throw new InvalidDataException("Mode-family JSON must contain a 'families' array.");

        var result = new Dictionary<string, QuantitativeUncertainty>(StringComparer.Ordinal);
        foreach (var family in families)
        {
            var sourceCandidateId = family.GetProperty("sourceCandidateId").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourceCandidateId) ||
                !family.TryGetProperty("uncertainty", out var uncertainty))
            {
                continue;
            }

            result[sourceCandidateId] = new QuantitativeUncertainty
            {
                BranchVariation = uncertainty.GetProperty("branchVariation").GetDouble(),
                RefinementError = uncertainty.GetProperty("refinementError").GetDouble(),
                ExtractionError = uncertainty.GetProperty("extractionError").GetDouble(),
                EnvironmentSensitivity = uncertainty.GetProperty("environmentSensitivity").GetDouble(),
                TotalUncertainty = uncertainty.GetProperty("totalUncertainty").GetDouble(),
            };
        }

        return result;
    }
}
