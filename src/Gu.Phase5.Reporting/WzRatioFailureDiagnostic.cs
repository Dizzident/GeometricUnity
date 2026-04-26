using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzRatioFailureDiagnosticResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("targetObservableId")]
    public required string TargetObservableId { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }

    [JsonPropertyName("selectedPair")]
    public WzRatioPairDiagnosticRecord? SelectedPair { get; init; }

    [JsonPropertyName("bestDiagnosticPair")]
    public WzRatioPairDiagnosticRecord? BestDiagnosticPair { get; init; }

    [JsonPropertyName("pairDiagnostics")]
    public required IReadOnlyList<WzRatioPairDiagnosticRecord> PairDiagnostics { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzRatioPairDiagnosticRecord
{
    [JsonPropertyName("pairId")]
    public required string PairId { get; init; }

    [JsonPropertyName("wSourceId")]
    public required string WSourceId { get; init; }

    [JsonPropertyName("zSourceId")]
    public required string ZSourceId { get; init; }

    [JsonPropertyName("wChargeSector")]
    public required string WChargeSector { get; init; }

    [JsonPropertyName("zChargeSector")]
    public required string ZChargeSector { get; init; }

    [JsonPropertyName("ratio")]
    public required double Ratio { get; init; }

    [JsonPropertyName("uncertainty")]
    public required QuantitativeUncertainty Uncertainty { get; init; }

    [JsonPropertyName("targetDelta")]
    public required double TargetDelta { get; init; }

    [JsonPropertyName("combinedSigma")]
    public required double CombinedSigma { get; init; }

    [JsonPropertyName("pull")]
    public required double Pull { get; init; }

    [JsonPropertyName("passesSigma5")]
    public required bool PassesSigma5 { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public required double RequiredScaleToTarget { get; init; }

    [JsonPropertyName("requiredTotalUncertaintyForSigma5")]
    public required double RequiredTotalUncertaintyForSigma5 { get; init; }

    [JsonPropertyName("uncertaintyInflationFactorForSigma5")]
    public required double UncertaintyInflationFactorForSigma5 { get; init; }

    [JsonPropertyName("selectionRole")]
    public required string SelectionRole { get; init; }
}

public static class WzRatioFailureDiagnostic
{
    public const string AlgorithmId = "p29-wz-ratio-failure-diagnostic:v1";

    public static WzRatioFailureDiagnosticResult Evaluate(
        string identityRuleReadinessJson,
        string mixingReadinessJson,
        string candidateModeSourcesJson,
        string modeFamiliesJson,
        string targetTableJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityRuleReadinessJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(mixingReadinessJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateModeSourcesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetTableJson);

        var readiness = GuJsonDefaults.Deserialize<VectorBosonIdentityRuleReadinessResult>(identityRuleReadinessJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z identity-rule readiness.");
        var mixing = GuJsonDefaults.Deserialize<ElectroweakMixingConventionReadinessResult>(mixingReadinessJson)
            ?? throw new InvalidDataException("Failed to deserialize electroweak mixing readiness.");
        var sources = GuJsonDefaults.Deserialize<CandidateModeSourceBridgeTable>(candidateModeSourcesJson)
            ?? throw new InvalidDataException("Failed to deserialize candidate-mode sources.");
        var targetTable = GuJsonDefaults.Deserialize<ExternalTargetTable>(targetTableJson)
            ?? throw new InvalidDataException("Failed to deserialize external target table.");
        var target = targetTable.Targets.FirstOrDefault(t => t.ObservableId == "physical-w-z-mass-ratio")
            ?? throw new InvalidDataException("External target table must contain physical-w-z-mass-ratio.");
        var uncertainties = LoadFamilyUncertainties(modeFamiliesJson);

        var closure = new List<string>();
        if (!string.Equals(readiness.TerminalStatus, "identity-rule-ready", StringComparison.Ordinal))
            closure.Add("identity-rule readiness is not ready");
        if (!string.Equals(mixing.TerminalStatus, "mixing-convention-ready", StringComparison.Ordinal))
            closure.Add("mixing-convention readiness is not ready");

        var sourceByCandidateId = sources.CandidateModeSources.ToDictionary(
            SourceCandidateId,
            source => source,
            StringComparer.Ordinal);
        var assignmentByCandidateId = mixing.ChargeSectorAssignments.ToDictionary(
            a => a.SourceCandidateId,
            a => a,
            StringComparer.Ordinal);

        var charged = assignmentByCandidateId.Values
            .Where(a => a.ChargeSector == "charged" && sourceByCandidateId.ContainsKey(a.SourceCandidateId))
            .OrderBy(a => a.SourceCandidateId, StringComparer.Ordinal)
            .ToList();
        var neutral = assignmentByCandidateId.Values
            .Where(a => a.ChargeSector == "neutral" && sourceByCandidateId.ContainsKey(a.SourceCandidateId))
            .OrderBy(a => a.SourceCandidateId, StringComparer.Ordinal)
            .ToList();

        if (charged.Count == 0)
            closure.Add("no charged candidate sources are available");
        if (neutral.Count == 0)
            closure.Add("no neutral candidate sources are available");

        var selectedW = readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "w-boson")?.SourceId;
        var selectedZ = readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "z-boson")?.SourceId;

        var pairDiagnostics = new List<WzRatioPairDiagnosticRecord>();
        if (closure.Count == 0)
        {
            foreach (var w in charged)
            foreach (var z in neutral)
            {
                var wSource = sourceByCandidateId[w.SourceCandidateId];
                var zSource = sourceByCandidateId[z.SourceCandidateId];
                if (!uncertainties.TryGetValue(w.SourceCandidateId, out var wUncertainty) ||
                    !uncertainties.TryGetValue(z.SourceCandidateId, out var zUncertainty))
                {
                    continue;
                }

                var role = wSource.SourceId == selectedW && zSource.SourceId == selectedZ
                    ? "selected-by-identity-rule"
                    : "diagnostic-only";
                pairDiagnostics.Add(CreatePair(wSource, zSource, w.ChargeSector, z.ChargeSector, wUncertainty, zUncertainty, target, role));
            }
        }

        var selectedPair = pairDiagnostics.FirstOrDefault(p => p.SelectionRole == "selected-by-identity-rule");
        var bestPair = pairDiagnostics.OrderBy(p => System.Math.Abs(p.Pull)).FirstOrDefault();
        if (closure.Count == 0 && selectedPair is null)
            closure.Add("identity-rule selected W/Z pair is absent from the diagnostic pair set");

        var diagnosis = BuildDiagnosis(selectedPair, bestPair, pairDiagnostics);
        return new WzRatioFailureDiagnosticResult
        {
            ResultId = "phase29-wz-ratio-failure-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0 ? "wz-ratio-diagnostic-complete" : "wz-ratio-diagnostic-blocked",
            AlgorithmId = AlgorithmId,
            TargetObservableId = target.ObservableId,
            TargetValue = target.Value,
            TargetUncertainty = target.Uncertainty,
            SelectedPair = selectedPair,
            BestDiagnosticPair = bestPair,
            PairDiagnostics = pairDiagnostics,
            Diagnosis = diagnosis,
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static WzRatioPairDiagnosticRecord CreatePair(
        CandidateModeSourceRecord wSource,
        CandidateModeSourceRecord zSource,
        string wChargeSector,
        string zChargeSector,
        QuantitativeUncertainty wUncertainty,
        QuantitativeUncertainty zUncertainty,
        ExternalTarget target,
        string role)
    {
        var ratio = wSource.Value / zSource.Value;
        var branch = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.BranchVariation, zSource.Value, zUncertainty.BranchVariation);
        var refinement = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.RefinementError, zSource.Value, zUncertainty.RefinementError);
        var extraction = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.ExtractionError, zSource.Value, zUncertainty.ExtractionError);
        var environment = PropagateRatioComponent(ratio, wSource.Value, wUncertainty.EnvironmentSensitivity, zSource.Value, zUncertainty.EnvironmentSensitivity);
        var total = System.Math.Sqrt(branch * branch + refinement * refinement + extraction * extraction + environment * environment);
        var combined = System.Math.Sqrt(total * total + target.Uncertainty * target.Uncertainty);
        var delta = ratio - target.Value;
        var pull = combined > 0 ? delta / combined : double.NaN;
        var requiredTotal = RequiredComputedUncertaintyForSigma5(System.Math.Abs(delta), target.Uncertainty);

        return new WzRatioPairDiagnosticRecord
        {
            PairId = $"{wSource.SourceId}/{zSource.SourceId}",
            WSourceId = wSource.SourceId,
            ZSourceId = zSource.SourceId,
            WChargeSector = wChargeSector,
            ZChargeSector = zChargeSector,
            Ratio = ratio,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = branch,
                RefinementError = refinement,
                ExtractionError = extraction,
                EnvironmentSensitivity = environment,
                TotalUncertainty = total,
            },
            TargetDelta = delta,
            CombinedSigma = combined,
            Pull = pull,
            PassesSigma5 = System.Math.Abs(pull) <= 5.0,
            RequiredScaleToTarget = target.Value / ratio,
            RequiredTotalUncertaintyForSigma5 = requiredTotal,
            UncertaintyInflationFactorForSigma5 = total > 0 ? requiredTotal / total : double.PositiveInfinity,
            SelectionRole = role,
        };
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        WzRatioPairDiagnosticRecord? selected,
        WzRatioPairDiagnosticRecord? best,
        IReadOnlyList<WzRatioPairDiagnosticRecord> pairs)
    {
        var diagnosis = new List<string>();
        if (selected is null)
        {
            diagnosis.Add("selected identity-rule pair is unavailable, so no physical-ratio failure can be diagnosed");
            return diagnosis;
        }

        diagnosis.Add(selected.PassesSigma5
            ? "selected identity-rule pair is within the sigma-5 physical target gate"
            : $"selected identity-rule pair fails the sigma-5 physical target gate with pull {selected.Pull}");
        diagnosis.Add($"selected pair would require scale factor {selected.RequiredScaleToTarget} to land on the target");
        diagnosis.Add($"selected pair would require total computed uncertainty {selected.RequiredTotalUncertaintyForSigma5} for sigma-5 compatibility, an inflation factor {selected.UncertaintyInflationFactorForSigma5}");

        if (best is not null)
        {
            diagnosis.Add(best.PairId == selected.PairId
                ? "selected identity-rule pair is also the closest charged/neutral diagnostic pair to the target"
                : $"closest charged/neutral diagnostic pair is {best.PairId} with pull {best.Pull}; this is diagnostic-only and must not retune identity selection");
        }

        if (pairs.Count > 0 && pairs.All(p => !p.PassesSigma5))
            diagnosis.Add("no charged/neutral pair in the current internal source set passes the sigma-5 target gate");
        return diagnosis;
    }

    private static double RequiredComputedUncertaintyForSigma5(double absoluteDelta, double targetUncertainty)
    {
        var requiredCombined = absoluteDelta / 5.0;
        var squared = requiredCombined * requiredCombined - targetUncertainty * targetUncertainty;
        return squared > 0 ? System.Math.Sqrt(squared) : 0.0;
    }

    private static double PropagateRatioComponent(
        double ratio,
        double numerator,
        double numeratorUncertainty,
        double denominator,
        double denominatorUncertainty)
        => ratio * System.Math.Sqrt(
            System.Math.Pow(numeratorUncertainty / numerator, 2) +
            System.Math.Pow(denominatorUncertainty / denominator, 2));

    private static string SourceCandidateId(CandidateModeSourceRecord source)
        => source.SourceId.StartsWith("phase22-", StringComparison.Ordinal)
            ? source.SourceId["phase22-".Length..]
            : source.SourceId;

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
