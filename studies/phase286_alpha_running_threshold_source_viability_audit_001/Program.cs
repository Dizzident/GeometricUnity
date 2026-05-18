using System.Text.Json;

const string DefaultOutputDir = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase284Path = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output/predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE286_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase284 = JsonDocument.Parse(File.ReadAllText(Phase284Path));

var ratioRow = phase203.RootElement.GetProperty("defensibleValues")
    .EnumerateArray()
    .First(row => string.Equals(JsonString(row, "particleId"), "electroweak-sector", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "observableId"), "physical-w-z-mass-ratio", StringComparison.Ordinal));
var guWzRatio = RequiredDouble(ratioRow, "predictedValue");
var guWzRatioUncertainty = RequiredDouble(ratioRow, "predictedUncertainty");
var guSin2Theta = 1.0 - guWzRatio * guWzRatio;

var vevRow = phase54.RootElement.GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => string.Equals(JsonString(row, "scaleId"), "phase54-fermi-derived-electroweak-vacuum-scale", StringComparison.Ordinal));
var externalVevGeV = RequiredDouble(vevRow, "value");
var externalVevUncertaintyGeV = RequiredDouble(vevRow, "standardUncertainty");
var phase54InternalBridgeBlocked = phase54.RootElement.TryGetProperty("internalBridgeStatus", out var p54Bridge)
    && string.Equals(JsonString(p54Bridge, "status"), "blocked", StringComparison.Ordinal);

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wTargetRow = FindRow(comparisonRows, "w-boson");
var zTargetRow = FindRow(comparisonRows, "z-boson");
var targetWGeV = RequiredDouble(wTargetRow, "targetValue");
var targetWUncertaintyGeV = RequiredDouble(wTargetRow, "targetUncertainty");
var targetZGeV = RequiredDouble(zTargetRow, "targetValue");
var targetZUncertaintyGeV = RequiredDouble(zTargetRow, "targetUncertainty");

var phase261Inputs = phase261.RootElement.GetProperty("schemeInputs");
var alphaZeroInverse = RequiredDouble(phase261Inputs, "alphaZeroInverse");
var alphaMzInverse = RequiredDouble(phase261Inputs, "alphaMzInverse");
var alphaZero = 1.0 / alphaZeroInverse;
const double TargetComparisonSigmaGate = 3.0;

var chargedLeptonThresholds = new[]
{
    new ChargedLeptonThreshold("electron", 0.00051099895, "PDG/CODATA external charged-lepton mass"),
    new ChargedLeptonThreshold("muon", 0.1056583755, "PDG/CODATA external charged-lepton mass"),
    new ChargedLeptonThreshold("tau", 1.77686, "PDG external charged-lepton mass"),
};
var selfConsistentLeptonicRun = SolveLeptonicFixedPoint(alphaZero, externalVevGeV, guWzRatio, guSin2Theta, chargedLeptonThresholds);
var alphaZeroMassRow = BuildMassRow(
    "alpha0-no-running-gu-ratio-gf-vev",
    alphaZero,
    "external alpha(0) without running",
    externalInputsUsed: true,
    targetMassesUsedForConstruction: false,
    promotesBosonMasses: false);
var selfConsistentLeptonicRunningRow = BuildMassRow(
    "alpha0-leptonic-running-self-consistent-z-scale",
    selfConsistentLeptonicRun.AlphaAtFixedPoint,
    "external alpha(0) plus one-loop charged-lepton vacuum-polarization running to self-consistent predicted Z scale",
    externalInputsUsed: true,
    targetMassesUsedForConstruction: false,
    promotesBosonMasses: false);
var externalAlphaMzMassRow = BuildMassRow(
    "alphaMz-imported-gu-ratio-gf-vev",
    1.0 / alphaMzInverse,
    "imported external alpha(MZ) from Phase261/Phase284",
    externalInputsUsed: true,
    targetMassesUsedForConstruction: false,
    promotesBosonMasses: false);

var diagnosticRows = new[] { alphaZeroMassRow, selfConsistentLeptonicRunningRow, externalAlphaMzMassRow };
var bestRowByMaxSigmaResidual = diagnosticRows
    .OrderBy(row => Math.Max(row.WPullOrSigmaResidual, row.ZPullOrSigmaResidual))
    .First();
var alphaZeroRowFailsWzTargetComparison = !alphaZeroMassRow.TargetComparisonPassed;
var leptonicRunningNumericallyClosesWz = selfConsistentLeptonicRunningRow.TargetComparisonPassed;
var importedAlphaMzNumericallyClosesWz = externalAlphaMzMassRow.TargetComparisonPassed
    && JsonBool(phase284.RootElement, "anyRowPassesWzTargetComparison") is true;

var externalAlphaZeroUsed = true;
var externalLeptonMassesUsed = true;
var externalVevUsed = true;
var targetMassesUsedForLeptonicRunningConstruction = false;
var guAlphaZeroSourceFound = false;
var guChargedLeptonThresholdSourceFound = false;
var guRunningOperatorSourceFound = false;
var guHadronicVacuumPolarizationSourceFound = false;
var guRenormalizationSchemeSourceFound = false;
var alphaRunningSourcePromotable = false;
var alphaRunningThresholdRoutePromotesWzMasses = false;
var sourceContractsFilled = false;
var newSourceEvidenceStillRequired = true;

var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "leptonic-running-diagnostic-materialized",
        alphaZeroRowFailsWzTargetComparison && leptonicRunningNumericallyClosesWz && importedAlphaMzNumericallyClosesWz,
        $"alphaZeroRowFailsWzTargetComparison={alphaZeroRowFailsWzTargetComparison}; leptonicRunningNumericallyClosesWz={leptonicRunningNumericallyClosesWz}; importedAlphaMzNumericallyClosesWz={importedAlphaMzNumericallyClosesWz}; selfConsistentAlphaInverse={1.0 / selfConsistentLeptonicRun.AlphaAtFixedPoint:R}"),
    new Check(
        "leptonic-running-does-not-use-wz-targets-for-construction",
        !targetMassesUsedForLeptonicRunningConstruction,
        $"targetMassesUsedForLeptonicRunningConstruction={targetMassesUsedForLeptonicRunningConstruction}; fixedPointZScaleGeV={selfConsistentLeptonicRun.FixedPointScaleGeV:R}; targetZGeV={targetZGeV:R}"),
    new Check(
        "running-inputs-are-external-not-gu-source",
        externalAlphaZeroUsed
            && externalLeptonMassesUsed
            && externalVevUsed
            && !guAlphaZeroSourceFound
            && !guChargedLeptonThresholdSourceFound
            && !guRunningOperatorSourceFound
            && !guHadronicVacuumPolarizationSourceFound
            && !guRenormalizationSchemeSourceFound,
        $"externalAlphaZeroUsed={externalAlphaZeroUsed}; externalLeptonMassesUsed={externalLeptonMassesUsed}; externalVevUsed={externalVevUsed}; guAlphaZeroSourceFound={guAlphaZeroSourceFound}; guChargedLeptonThresholdSourceFound={guChargedLeptonThresholdSourceFound}; guRunningOperatorSourceFound={guRunningOperatorSourceFound}; guHadronicVacuumPolarizationSourceFound={guHadronicVacuumPolarizationSourceFound}; guRenormalizationSchemeSourceFound={guRenormalizationSchemeSourceFound}"),
    new Check(
        "phase236-rg-transport-remains-nonpromotable",
        lowEnergyRgTransportSourceAuditPassed && !lowEnergyRgTransportSourcePromotable,
        $"lowEnergyRgTransportSourceAuditPassed={lowEnergyRgTransportSourceAuditPassed}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "phase54-vev-bridge-remains-external",
        phase54InternalBridgeBlocked,
        $"phase54InternalBridgeBlocked={phase54InternalBridgeBlocked}; externalVevUsed={externalVevUsed}"),
    new Check(
        "source-lineage-contracts-remain-unfilled",
        !unlockContractFilled
            && phase245NewSourceEvidenceStillRequired
            && !alphaRunningSourcePromotable
            && !alphaRunningThresholdRoutePromotesWzMasses
            && !sourceContractsFilled
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; phase245NewSourceEvidenceStillRequired={phase245NewSourceEvidenceStillRequired}; alphaRunningSourcePromotable={alphaRunningSourcePromotable}; alphaRunningThresholdRoutePromotesWzMasses={alphaRunningThresholdRoutePromotesWzMasses}; sourceContractsFilled={sourceContractsFilled}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var alphaRunningThresholdSourceViabilityAuditPassed = checks.All(check => check.Passed)
    && leptonicRunningNumericallyClosesWz
    && !alphaRunningSourcePromotable
    && !alphaRunningThresholdRoutePromotesWzMasses
    && !sourceContractsFilled
    && newSourceEvidenceStillRequired;
var terminalStatus = alphaRunningThresholdSourceViabilityAuditPassed
    ? "alpha-running-threshold-source-viability-audit-external-leptonic-closure-not-promotion"
    : "alpha-running-threshold-source-viability-audit-review-required";

var result = new
{
    phaseId = "phase286-alpha-running-threshold-source-viability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    alphaRunningThresholdSourceViabilityAuditPassed,
    alphaZeroRowFailsWzTargetComparison,
    leptonicRunningNumericallyClosesWz,
    importedAlphaMzNumericallyClosesWz,
    alphaRunningSourcePromotable,
    alphaRunningThresholdRoutePromotesWzMasses,
    sourceContractsFilled,
    newSourceEvidenceStillRequired,
    inputScalars = new
    {
        guWzRatio,
        guWzRatioUncertainty,
        guSin2Theta,
        externalVevGeV,
        externalVevUncertaintyGeV,
        alphaZeroInverse,
        alphaMzInverse,
        targetComparisonSigmaGate = TargetComparisonSigmaGate,
    },
    chargedLeptonThresholds,
    selfConsistentLeptonicRun,
    diagnosticRows,
    bestRowByMaxSigmaResidual,
    sourceLineageBoundary = new
    {
        externalAlphaZeroUsed,
        externalLeptonMassesUsed,
        externalVevUsed,
        targetMassesUsedForLeptonicRunningConstruction,
        guAlphaZeroSourceFound,
        guChargedLeptonThresholdSourceFound,
        guRunningOperatorSourceFound,
        guHadronicVacuumPolarizationSourceFound,
        guRenormalizationSchemeSourceFound,
        phase54InternalBridgeBlocked,
        lowEnergyRgTransportSourceAuditPassed,
        lowEnergyRgTransportSourcePromotable,
        unlockContractFilled,
        phase245NewSourceEvidenceStillRequired,
        wzMissingFieldCount,
        higgsMissingFieldCount,
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2024-electromagnetic-coupling",
            "https://pdg.lbl.gov/2024/reviews/rpp2024-rev-standard-model.pdf",
            "PDG electroweak review treats alpha as a running coupling and separates precision electromagnetic coupling, Fermi constant, and electroweak renormalization inputs."),
        new ExternalSource(
            "hadronic-running-electroweak-couplings-lattice-qcd",
            "https://arxiv.org/abs/2211.11401",
            "Hadronic vacuum polarization contributes to the running electroweak couplings and requires nonperturbative/data-driven input beyond a lepton-only QED log."),
    },
    checks,
    decision = alphaRunningThresholdSourceViabilityAuditPassed
        ? "Do not promote W/Z masses from the alpha-running threshold route. A lepton-only external QED running diagnostic can numerically close W/Z when combined with the promoted ratio and external Fermi VEV, but it imports alpha(0), charged-lepton thresholds, a running prescription, and the external VEV. The GU source rows for alpha/charge, thresholds, RG transport, hadronic vacuum polarization or scheme closure, and VEV remain missing."
        : "Review the alpha-running threshold source audit before relying on its boundary classification.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived electromagnetic charge or alpha source at a declared reference scale.",
        "A GU-derived low-energy running/threshold transport operator, including charged thresholds and hadronic vacuum-polarization or scheme closure when required.",
        "A GU-derived VEV or direct W/Z absolute-scale source row that can fill the Phase201/245 W/Z source-lineage contract.",
        "A separate Higgs scalar-source/self-coupling lineage; alpha running does not address the Higgs blocker.",
    },
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase148Path = Phase148Path,
        phase203Path = Phase203Path,
        phase213Path = Phase213Path,
        phase236Path = Phase236Path,
        phase245Path = Phase245Path,
        phase261Path = Phase261Path,
        phase284Path = Phase284Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "alpha_running_threshold_source_viability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "alpha_running_threshold_source_viability_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.alphaRunningThresholdSourceViabilityAuditPassed,
        result.alphaZeroRowFailsWzTargetComparison,
        result.leptonicRunningNumericallyClosesWz,
        result.importedAlphaMzNumericallyClosesWz,
        result.alphaRunningSourcePromotable,
        result.alphaRunningThresholdRoutePromotesWzMasses,
        result.sourceContractsFilled,
        result.newSourceEvidenceStillRequired,
        result.inputScalars,
        result.chargedLeptonThresholds,
        result.selfConsistentLeptonicRun,
        result.diagnosticRows,
        result.bestRowByMaxSigmaResidual,
        result.sourceLineageBoundary,
        result.externalResearchSnapshot,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"alphaRunningThresholdSourceViabilityAuditPassed={alphaRunningThresholdSourceViabilityAuditPassed}");
Console.WriteLine($"leptonicRunningNumericallyClosesWz={leptonicRunningNumericallyClosesWz}");
Console.WriteLine($"alphaRunningThresholdRoutePromotesWzMasses={alphaRunningThresholdRoutePromotesWzMasses}");
Console.WriteLine($"bestRow={bestRowByMaxSigmaResidual.RowId}");

LeptonicRunningResult SolveLeptonicFixedPoint(
    double alphaAtZero,
    double vevGeV,
    double ratio,
    double sin2Theta,
    IReadOnlyList<ChargedLeptonThreshold> thresholds)
{
    var scaleGeV = vevGeV;
    var iterations = new List<LeptonicRunningIteration>();
    for (var iteration = 0; iteration < 32; iteration++)
    {
        var deltaAlphaLeptonic = LeptonicDeltaAlpha(alphaAtZero, scaleGeV, thresholds);
        var alphaAtScale = alphaAtZero / (1.0 - deltaAlphaLeptonic);
        var electricCharge = Math.Sqrt(4.0 * Math.PI * alphaAtScale);
        var weakCoupling = electricCharge / Math.Sqrt(sin2Theta);
        var predictedWGeV = weakCoupling * vevGeV / 2.0;
        var predictedZGeV = predictedWGeV / ratio;
        iterations.Add(new LeptonicRunningIteration(
            iteration,
            scaleGeV,
            deltaAlphaLeptonic,
            1.0 / alphaAtScale,
            predictedWGeV,
            predictedZGeV));
        if (Math.Abs(predictedZGeV - scaleGeV) < 1e-12)
        {
            return new LeptonicRunningResult(
                predictedZGeV,
                deltaAlphaLeptonic,
                alphaAtScale,
                1.0 / alphaAtScale,
                iterations.Count,
                iterations);
        }

        scaleGeV = predictedZGeV;
    }

    var last = iterations[^1];
    return new LeptonicRunningResult(
        last.PredictedZGeV,
        last.DeltaAlphaLeptonic,
        1.0 / last.AlphaInverse,
        last.AlphaInverse,
        iterations.Count,
        iterations);
}

static double LeptonicDeltaAlpha(
    double alphaAtZero,
    double scaleGeV,
    IReadOnlyList<ChargedLeptonThreshold> thresholds)
{
    var sum = 0.0;
    foreach (var threshold in thresholds)
    {
        sum += Math.Log(scaleGeV * scaleGeV / (threshold.MassGeV * threshold.MassGeV)) - 5.0 / 3.0;
    }

    return alphaAtZero * sum / (3.0 * Math.PI);
}

MassDiagnosticRow BuildMassRow(
    string rowId,
    double alpha,
    string alphaSource,
    bool externalInputsUsed,
    bool targetMassesUsedForConstruction,
    bool promotesBosonMasses)
{
    var electricCharge = Math.Sqrt(4.0 * Math.PI * alpha);
    var weakCoupling = electricCharge / Math.Sqrt(guSin2Theta);
    var predictedWGeV = weakCoupling * externalVevGeV / 2.0;
    var predictedZGeV = predictedWGeV / guWzRatio;
    var wRelativeUncertainty = Math.Sqrt(
        Math.Pow(guWzRatio * guWzRatioUncertainty / guSin2Theta, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var zRatioDerivative = -1.0 / guWzRatio + guWzRatio / guSin2Theta;
    var zRelativeUncertainty = Math.Sqrt(
        Math.Pow(Math.Abs(zRatioDerivative) * guWzRatioUncertainty, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var predictedWUncertaintyGeV = predictedWGeV * wRelativeUncertainty;
    var predictedZUncertaintyGeV = predictedZGeV * zRelativeUncertainty;
    var wResidualGeV = predictedWGeV - targetWGeV;
    var zResidualGeV = predictedZGeV - targetZGeV;
    var wPullOrSigmaResidual = Math.Abs(wResidualGeV) / Math.Sqrt(
        predictedWUncertaintyGeV * predictedWUncertaintyGeV
        + targetWUncertaintyGeV * targetWUncertaintyGeV);
    var zPullOrSigmaResidual = Math.Abs(zResidualGeV) / Math.Sqrt(
        predictedZUncertaintyGeV * predictedZUncertaintyGeV
        + targetZUncertaintyGeV * targetZUncertaintyGeV);
    var targetComparisonPassed = wPullOrSigmaResidual <= TargetComparisonSigmaGate
        && zPullOrSigmaResidual <= TargetComparisonSigmaGate;

    return new MassDiagnosticRow(
        rowId,
        alphaSource,
        alpha,
        1.0 / alpha,
        electricCharge,
        weakCoupling,
        predictedWGeV,
        predictedWUncertaintyGeV,
        predictedZGeV,
        predictedZUncertaintyGeV,
        wResidualGeV,
        zResidualGeV,
        wPullOrSigmaResidual,
        zPullOrSigmaResidual,
        TargetComparisonSigmaGate,
        targetComparisonPassed,
        externalInputsUsed,
        targetMassesUsedForConstruction,
        promotesBosonMasses);
}

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record ChargedLeptonThreshold(string LeptonId, double MassGeV, string SourceClass);

sealed record LeptonicRunningIteration(
    int Iteration,
    double ScaleGeV,
    double DeltaAlphaLeptonic,
    double AlphaInverse,
    double PredictedWGeV,
    double PredictedZGeV);

sealed record LeptonicRunningResult(
    double FixedPointScaleGeV,
    double DeltaAlphaLeptonic,
    double AlphaAtFixedPoint,
    double AlphaInverseAtFixedPoint,
    int IterationCount,
    IReadOnlyList<LeptonicRunningIteration> Iterations);

sealed record MassDiagnosticRow(
    string RowId,
    string AlphaSource,
    double Alpha,
    double AlphaInverse,
    double ElectricCharge,
    double WeakCoupling,
    double PredictedWGeV,
    double PredictedWUncertaintyGeV,
    double PredictedZGeV,
    double PredictedZUncertaintyGeV,
    double WResidualGeV,
    double ZResidualGeV,
    double WPullOrSigmaResidual,
    double ZPullOrSigmaResidual,
    double TargetComparisonSigmaGate,
    bool TargetComparisonPassed,
    bool ExternalInputsUsed,
    bool TargetMassesUsedForConstruction,
    bool PromotesBosonMasses);

sealed record ExternalSource(string SourceId, string Url, string Finding);
sealed record Check(string CheckId, bool Passed, string Detail);
