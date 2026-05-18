using System.Text.Json;

const string DefaultOutputDir = "studies/phase291_koide_charged_lepton_threshold_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase290Path = "studies/phase290_charged_lepton_threshold_source_replacement_audit_001/output/charged_lepton_threshold_source_replacement_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE291_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase290 = JsonDocument.Parse(File.ReadAllText(Phase290Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var thresholds = phase286.RootElement.GetProperty("chargedLeptonThresholds")
    .EnumerateArray()
    .Select(row => new ChargedLeptonThreshold(
        RequiredString(row, "leptonId"),
        RequiredDouble(row, "massGeV"),
        RequiredString(row, "sourceClass")))
    .OrderBy(row => row.MassGeV)
    .ToArray();

var electron = thresholds.Single(row => row.LeptonId == "electron");
var muon = thresholds.Single(row => row.LeptonId == "muon");
var tau = thresholds.Single(row => row.LeptonId == "tau");
var koideQFromExternalThresholds = KoideQ(electron.MassGeV, muon.MassGeV, tau.MassGeV);
var koideDeviationFromTwoThirds = koideQFromExternalThresholds - 2.0 / 3.0;
var tauSolutions = SolveTauMassesFromKoide(electron.MassGeV, muon.MassGeV);
var tauLikeSolution = tauSolutions.OrderByDescending(value => value).First();
var alternateSolution = tauSolutions.OrderBy(value => value).First();
var tauResidualGeV = tauLikeSolution - tau.MassGeV;
var tauRelativeResidual = tauResidualGeV / tau.MassGeV;

var koideThresholds = new[]
{
    new ChargedLeptonThreshold("electron", electron.MassGeV, "external charged-lepton mass retained by Koide diagnostic"),
    new ChargedLeptonThreshold("muon", muon.MassGeV, "external charged-lepton mass retained by Koide diagnostic"),
    new ChargedLeptonThreshold("tau", tauLikeSolution, "Koide-derived from external electron and muon masses plus empirical Q=2/3 relation"),
};
var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wTargetRow = FindRow(comparisonRows, "w-boson");
var zTargetRow = FindRow(comparisonRows, "z-boson");
var targetWGeV = RequiredDouble(wTargetRow, "targetValue");
var targetWUncertaintyGeV = RequiredDouble(wTargetRow, "targetUncertainty");
var targetZGeV = RequiredDouble(zTargetRow, "targetValue");
var targetZUncertaintyGeV = RequiredDouble(zTargetRow, "targetUncertainty");
var koideRunning = BuildSelfConsistentLeptonicRun(
    phase286.RootElement,
    koideThresholds,
    targetWGeV,
    targetWUncertaintyGeV,
    targetZGeV,
    targetZUncertaintyGeV);
var phase286BestRow = phase286.RootElement.GetProperty("bestRowByMaxSigmaResidual");
var phase286BestW = RequiredDouble(phase286BestRow, "predictedWGeV");
var phase286BestZ = RequiredDouble(phase286BestRow, "predictedZGeV");

var koideRelationLeadPresent = true;
var koideNumericallyMatchesChargedLeptonPoleMasses = Math.Abs(koideDeviationFromTwoThirds) < 1.0e-5
    && Math.Abs(tauResidualGeV) < 0.0002;
var koideCanReconstructTauFromExternalElectronMuon = Math.Abs(tauResidualGeV) < 0.0002;
var koideLeptonicRunningNumericallyClosesWz = koideRunning.MaxPullOrSigmaResidual < 3.0;
var koideUsesExternalElectronMuonMasses = true;
var koideUsesEmpiricalMassRelation = true;
var koideProvidesAllThreeThresholdsTargetIndependently = false;
var koideProvidesGuLocalSourceLineage = false;
var koideProvidesIndependentAlphaSource = false;
var koideProvidesIndependentRunningOperator = false;
var koideProvidesIndependentVevSource = false;
var koideThresholdRoutePromotesWzMasses = false;
var koidePromotesBosonPredictions = false;

var phase286LeptonicRunningNumericallyClosesWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz") is true;
var phase286AlphaRunningSourcePromotable = JsonBool(phase286.RootElement, "alphaRunningSourcePromotable") is true;
var phase286Boundary = phase286.RootElement.GetProperty("sourceLineageBoundary");
var phase286ExternalLeptonMassesUsed = JsonBool(phase286Boundary, "externalLeptonMassesUsed") is true;
var phase286GuChargedLeptonThresholdSourceFound = JsonBool(phase286Boundary, "guChargedLeptonThresholdSourceFound") is true;
var officialDraftProvidesChargedLeptonThresholdSource = JsonBool(phase287.RootElement, "officialDraftProvidesChargedLeptonThresholdSource") is true;
var phase290Passed = JsonBool(phase290.RootElement, "chargedLeptonThresholdSourceReplacementAuditPassed") is true;
var phase290IntakeReadyThresholdSourceCandidateCount = JsonInt(phase290.RootElement, "intakeReadyThresholdSourceCandidateCount") ?? -1;
var phase290AnyThresholdSourceCandidateFillsContract = JsonBool(phase290.RootElement, "anyThresholdSourceCandidateFillsContract") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "koide-lead-materialized",
        koideRelationLeadPresent && koideNumericallyMatchesChargedLeptonPoleMasses && koideCanReconstructTauFromExternalElectronMuon,
        $"koideQ={koideQFromExternalThresholds}; deviationFromTwoThirds={koideDeviationFromTwoThirds}; tauLikeSolutionGeV={tauLikeSolution}; tauResidualGeV={tauResidualGeV}"),
    new Check(
        "koide-alpha-running-diagnostic-preserves-numerical-wz-closure",
        koideLeptonicRunningNumericallyClosesWz,
        $"predictedWGeV={koideRunning.PredictedWGeV}; predictedZGeV={koideRunning.PredictedZGeV}; maxPull={koideRunning.MaxPullOrSigmaResidual}; phase286BestWGeV={phase286BestW}; phase286BestZGeV={phase286BestZ}"),
    new Check(
        "koide-route-remains-external-and-empirical",
        koideUsesExternalElectronMuonMasses
            && koideUsesEmpiricalMassRelation
            && !koideProvidesAllThreeThresholdsTargetIndependently
            && !koideProvidesGuLocalSourceLineage,
        $"usesExternalElectronMuonMasses={koideUsesExternalElectronMuonMasses}; usesEmpiricalMassRelation={koideUsesEmpiricalMassRelation}; providesAllThreeThresholdsTargetIndependently={koideProvidesAllThreeThresholdsTargetIndependently}; providesGuLocalSourceLineage={koideProvidesGuLocalSourceLineage}"),
    new Check(
        "koide-does-not-fill-alpha-running-vev-or-threshold-contract",
        !koideProvidesIndependentAlphaSource
            && !koideProvidesIndependentRunningOperator
            && !koideProvidesIndependentVevSource
            && !koideThresholdRoutePromotesWzMasses
            && !koidePromotesBosonPredictions,
        $"providesAlphaSource={koideProvidesIndependentAlphaSource}; providesRunningOperator={koideProvidesIndependentRunningOperator}; providesVevSource={koideProvidesIndependentVevSource}; thresholdRoutePromotesWzMasses={koideThresholdRoutePromotesWzMasses}; promotesBosonPredictions={koidePromotesBosonPredictions}"),
    new Check(
        "phase286-phase287-phase290-blockers-remain-active",
        phase286LeptonicRunningNumericallyClosesWz
            && !phase286AlphaRunningSourcePromotable
            && phase286ExternalLeptonMassesUsed
            && !phase286GuChargedLeptonThresholdSourceFound
            && !officialDraftProvidesChargedLeptonThresholdSource
            && phase290Passed
            && phase290IntakeReadyThresholdSourceCandidateCount == 0
            && !phase290AnyThresholdSourceCandidateFillsContract,
        $"phase286LeptonicRunningNumericallyClosesWz={phase286LeptonicRunningNumericallyClosesWz}; phase286AlphaRunningSourcePromotable={phase286AlphaRunningSourcePromotable}; phase286ExternalLeptonMassesUsed={phase286ExternalLeptonMassesUsed}; phase286GuChargedLeptonThresholdSourceFound={phase286GuChargedLeptonThresholdSourceFound}; officialDraftProvidesChargedLeptonThresholdSource={officialDraftProvidesChargedLeptonThresholdSource}; phase290IntakeReadyThresholdSourceCandidateCount={phase290IntakeReadyThresholdSourceCandidateCount}; phase290AnyThresholdSourceCandidateFillsContract={phase290AnyThresholdSourceCandidateFillsContract}"),
    new Check(
        "source-contracts-remain-unfilled",
        !unlockContractFilled && newSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var koideChargedLeptonThresholdSourceAuditPassed = checks.All(check => check.Passed)
    && !koideProvidesAllThreeThresholdsTargetIndependently
    && !koideProvidesGuLocalSourceLineage
    && !koidePromotesBosonPredictions;
var terminalStatus = koideChargedLeptonThresholdSourceAuditPassed
    ? "koide-charged-lepton-threshold-source-audit-empirical-not-gu-source"
    : "koide-charged-lepton-threshold-source-audit-review-required";

var result = new
{
    phaseId = "phase291-koide-charged-lepton-threshold-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    koideChargedLeptonThresholdSourceAuditPassed,
    koideRelationLeadPresent,
    koideNumericallyMatchesChargedLeptonPoleMasses,
    koideCanReconstructTauFromExternalElectronMuon,
    koideLeptonicRunningNumericallyClosesWz,
    koideUsesExternalElectronMuonMasses,
    koideUsesEmpiricalMassRelation,
    koideProvidesAllThreeThresholdsTargetIndependently,
    koideProvidesGuLocalSourceLineage,
    koideProvidesIndependentAlphaSource,
    koideProvidesIndependentRunningOperator,
    koideProvidesIndependentVevSource,
    koideThresholdRoutePromotesWzMasses,
    koidePromotesBosonPredictions,
    koideMassRelationDiagnostic = new
    {
        formula = "Q = (m_e + m_mu + m_tau) / (sqrt(m_e) + sqrt(m_mu) + sqrt(m_tau))^2",
        targetValue = 2.0 / 3.0,
        qFromExternalThresholds = koideQFromExternalThresholds,
        deviationFromTwoThirds = koideDeviationFromTwoThirds,
        externalElectronGeV = electron.MassGeV,
        externalMuonGeV = muon.MassGeV,
        externalTauGeV = tau.MassGeV,
        tauLikeSolutionFromExternalElectronMuonGeV = tauLikeSolution,
        alternateSolutionFromExternalElectronMuonGeV = alternateSolution,
        tauResidualGeV,
        tauRelativeResidual,
        constructionBoundary = "Solves the empirical Q=2/3 relation for tau after importing electron and muon masses. This is diagnostic and cannot supply all three charged thresholds target-independently.",
    },
    koideAlphaRunningDiagnostic = koideRunning,
    inheritedEvidence = new
    {
        phase286 = new
        {
            phase286LeptonicRunningNumericallyClosesWz,
            phase286AlphaRunningSourcePromotable,
            phase286ExternalLeptonMassesUsed,
            phase286GuChargedLeptonThresholdSourceFound,
        },
        phase287 = new
        {
            officialDraftProvidesChargedLeptonThresholdSource,
        },
        phase290 = new
        {
            phase290Passed,
            phase290IntakeReadyThresholdSourceCandidateCount,
            phase290AnyThresholdSourceCandidateFillsContract,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "koide-1993-renewed-tau-mass-values",
            "https://www.jstage.jst.go.jp/article/soken/87/3/87_KJ00004708094/_article/-char/en",
            "Koide reviews a charged-lepton mass formula that predicts a tau mass near 1777 MeV from the electron/muon inputs and the empirical relation."),
        new ExternalSource(
            "xing-zhang-2006-running-koide-relations",
            "https://arxiv.org/abs/hep-ph/0602134",
            "The charged-lepton Koide relation is accurate for pole masses, while running masses depart from exact 2/3 at the electroweak scale."),
        new ExternalSource(
            "sumino-2009-family-gauge-symmetry",
            "https://arxiv.org/abs/0812.2103",
            "Sumino supplies an external U(3) family-gauge effective-field-theory mechanism for the charged-lepton spectrum, not a GU-local source row in this repository."),
        new ExternalSource(
            "hofseth-weinstein-2026-gu-rvg-koide-lead",
            "https://zenodo.org/records/19465254",
            "The GU/RVG v8 public source explicitly links Koide deviation to a dilaton/trace-anomaly synthesis, but Phase281 classifies that route as external EFT/device-oriented and non-promotional for W/Z/H source-lineage contracts."),
        new ExternalSource(
            "pdg-2024-particle-properties",
            "https://pdg.lbl.gov/2024/listings/particle_properties.html",
            "The electron, muon, and tau masses used for this diagnostic are external particle-property inputs."),
    },
    checks,
    decision = koideChargedLeptonThresholdSourceAuditPassed
        ? "Do not promote Phase286's W/Z masses through a Koide charged-lepton threshold replacement. Koide is a strong empirical charged-lepton relation and can reconstruct the tau threshold from external electron and muon masses, preserving the lepton-running W/Z numerical closure. It still imports two thresholds, supplies no GU-local electron/muon/tau identity or scale source rows, and does not fill alpha, running-operator, VEV, W/Z source-lineage, or Higgs scalar-source contracts."
        : "Review the Koide charged-lepton threshold source audit before relying on its non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving the Koide relation or a stronger charged-lepton spectrum rule from native GU structures.",
        "A target-independent GU identity and threshold table for electron, muon, and tau, not just tau from external electron and muon.",
        "A GU alpha/charge source, running/threshold transport operator, and VEV/direct W/Z scale source if the alpha-running route remains in use.",
        "A separate solved Higgs scalar-source/self-coupling lineage; charged-lepton Koide structure does not address the Higgs blocker.",
    },
    sourceEvidence = new
    {
        phase286Path = Phase286Path,
        phase287Path = Phase287Path,
        phase290Path = Phase290Path,
        phase245Path = Phase245Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "koide_charged_lepton_threshold_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "koide_charged_lepton_threshold_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.koideChargedLeptonThresholdSourceAuditPassed,
        result.koideRelationLeadPresent,
        result.koideNumericallyMatchesChargedLeptonPoleMasses,
        result.koideCanReconstructTauFromExternalElectronMuon,
        result.koideLeptonicRunningNumericallyClosesWz,
        result.koideUsesExternalElectronMuonMasses,
        result.koideUsesEmpiricalMassRelation,
        result.koideProvidesAllThreeThresholdsTargetIndependently,
        result.koideProvidesGuLocalSourceLineage,
        result.koideProvidesIndependentAlphaSource,
        result.koideProvidesIndependentRunningOperator,
        result.koideProvidesIndependentVevSource,
        result.koideThresholdRoutePromotesWzMasses,
        result.koidePromotesBosonPredictions,
        result.koideMassRelationDiagnostic,
        result.koideAlphaRunningDiagnostic,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"koideChargedLeptonThresholdSourceAuditPassed={koideChargedLeptonThresholdSourceAuditPassed}");
Console.WriteLine($"koideQFromExternalThresholds={koideQFromExternalThresholds}");
Console.WriteLine($"tauLikeSolutionFromExternalElectronMuonGeV={tauLikeSolution}");
Console.WriteLine($"koideLeptonicRunningNumericallyClosesWz={koideLeptonicRunningNumericallyClosesWz}");
Console.WriteLine($"koideProvidesGuLocalSourceLineage={koideProvidesGuLocalSourceLineage}");
Console.WriteLine($"koidePromotesBosonPredictions={koidePromotesBosonPredictions}");

static double KoideQ(double electronGeV, double muonGeV, double tauGeV)
{
    var numerator = electronGeV + muonGeV + tauGeV;
    var rootSum = Math.Sqrt(electronGeV) + Math.Sqrt(muonGeV) + Math.Sqrt(tauGeV);
    return numerator / (rootSum * rootSum);
}

static double[] SolveTauMassesFromKoide(double electronGeV, double muonGeV)
{
    var electronRoot = Math.Sqrt(electronGeV);
    var muonRoot = Math.Sqrt(muonGeV);
    var rootSumKnown = electronRoot + muonRoot;
    var massSumKnown = electronGeV + muonGeV;
    var rootProductKnown = electronRoot * muonRoot;
    var constant = massSumKnown - 4.0 * rootProductKnown;
    var discriminant = 16.0 * rootSumKnown * rootSumKnown - 4.0 * constant;
    if (discriminant < 0.0)
    {
        throw new InvalidOperationException("Koide tau quadratic has no real roots for the supplied electron and muon masses.");
    }

    var sqrtDiscriminant = Math.Sqrt(discriminant);
    var rootA = (4.0 * rootSumKnown + sqrtDiscriminant) / 2.0;
    var rootB = (4.0 * rootSumKnown - sqrtDiscriminant) / 2.0;
    return new[] { rootA * rootA, rootB * rootB }
        .Where(value => double.IsFinite(value) && value > 0.0)
        .OrderBy(value => value)
        .ToArray();
}

static AlphaRunningRow BuildSelfConsistentLeptonicRun(
    JsonElement phase286Root,
    IReadOnlyList<ChargedLeptonThreshold> thresholds,
    double targetWGeV,
    double targetWUncertaintyGeV,
    double targetZGeV,
    double targetZUncertaintyGeV)
{
    var inputScalars = phase286Root.GetProperty("inputScalars");
    var guWzRatio = RequiredDouble(inputScalars, "guWzRatio");
    var guWzRatioUncertainty = RequiredDouble(inputScalars, "guWzRatioUncertainty");
    var guSin2Theta = RequiredDouble(inputScalars, "guSin2Theta");
    var externalVevGeV = RequiredDouble(inputScalars, "externalVevGeV");
    var externalVevUncertaintyGeV = RequiredDouble(inputScalars, "externalVevUncertaintyGeV");
    var alphaZeroInverse = RequiredDouble(inputScalars, "alphaZeroInverse");
    var alphaZero = 1.0 / alphaZeroInverse;

    var scaleGeV = externalVevGeV;
    var iterations = new List<AlphaRunningIteration>();
    for (var iteration = 0; iteration < 12; iteration++)
    {
        var deltaAlpha = DeltaAlphaLeptonic(alphaZero, scaleGeV, thresholds);
        var alpha = alphaZero / (1.0 - deltaAlpha);
        var electricCharge = Math.Sqrt(4.0 * Math.PI * alpha);
        var weakCoupling = electricCharge / Math.Sqrt(guSin2Theta);
        var predictedWGeV = 0.5 * weakCoupling * externalVevGeV;
        var predictedZGeV = predictedWGeV / guWzRatio;
        iterations.Add(new AlphaRunningIteration(iteration, scaleGeV, deltaAlpha, 1.0 / alpha, predictedWGeV, predictedZGeV));
        if (Math.Abs(predictedZGeV - scaleGeV) < 1.0e-10)
        {
            scaleGeV = predictedZGeV;
            break;
        }

        scaleGeV = predictedZGeV;
    }

    var last = iterations[^1];
    var wRelativeUncertainty = Math.Sqrt(
        Math.Pow(guWzRatio * guWzRatioUncertainty / guSin2Theta, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var zRatioDerivative = -1.0 / guWzRatio + guWzRatio / guSin2Theta;
    var zRelativeUncertainty = Math.Sqrt(
        Math.Pow(Math.Abs(zRatioDerivative) * guWzRatioUncertainty, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var wUncertaintyGeV = last.PredictedWGeV * wRelativeUncertainty;
    var zUncertaintyGeV = last.PredictedZGeV * zRelativeUncertainty;
    var wResidual = last.PredictedWGeV - targetWGeV;
    var zResidual = last.PredictedZGeV - targetZGeV;
    var wPull = Math.Abs(wResidual) / Math.Sqrt(
        wUncertaintyGeV * wUncertaintyGeV
        + targetWUncertaintyGeV * targetWUncertaintyGeV);
    var zPull = Math.Abs(zResidual) / Math.Sqrt(
        zUncertaintyGeV * zUncertaintyGeV
        + targetZUncertaintyGeV * targetZUncertaintyGeV);

    return new AlphaRunningRow(
        "alpha0-koide-tau-running-self-consistent-z-scale",
        last.PredictedWGeV,
        wUncertaintyGeV,
        last.PredictedZGeV,
        zUncertaintyGeV,
        wResidual,
        zResidual,
        wPull,
        zPull,
        Math.Max(wPull, zPull),
        last.ScaleGeV,
        last.DeltaAlphaLeptonic,
        last.AlphaInverse,
        iterations.Count,
        thresholds,
        iterations,
        ExternalInputsUsed: true,
        TargetMassesUsedForConstruction: false,
        PromotesBosonMasses: false);
}

static double DeltaAlphaLeptonic(double alphaZero, double scaleGeV, IEnumerable<ChargedLeptonThreshold> thresholds)
{
    return thresholds.Sum(threshold =>
    {
        var ratio = scaleGeV * scaleGeV / (threshold.MassGeV * threshold.MassGeV);
        return alphaZero / (3.0 * Math.PI) * (Math.Log(ratio) - 5.0 / 3.0);
    });
}

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId)
{
    foreach (var row in rows)
    {
        if (string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal))
        {
            return row;
        }
    }

    throw new InvalidOperationException($"Missing comparison row for {particleId}.");
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidOperationException($"Missing required numeric property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
    {
        return null;
    }

    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

sealed record ChargedLeptonThreshold(string LeptonId, double MassGeV, string SourceClass);

sealed record AlphaRunningRow(
    string RowId,
    double PredictedWGeV,
    double PredictedWUncertaintyGeV,
    double PredictedZGeV,
    double PredictedZUncertaintyGeV,
    double WResidualGeV,
    double ZResidualGeV,
    double WPullOrSigmaResidual,
    double ZPullOrSigmaResidual,
    double MaxPullOrSigmaResidual,
    double FixedPointScaleGeV,
    double DeltaAlphaLeptonic,
    double AlphaInverse,
    int IterationCount,
    IReadOnlyList<ChargedLeptonThreshold> ChargedLeptonThresholds,
    IReadOnlyList<AlphaRunningIteration> Iterations,
    bool ExternalInputsUsed,
    bool TargetMassesUsedForConstruction,
    bool PromotesBosonMasses);

sealed record AlphaRunningIteration(
    int Iteration,
    double ScaleGeV,
    double DeltaAlphaLeptonic,
    double AlphaInverse,
    double PredictedWGeV,
    double PredictedZGeV);

sealed record ExternalSource(string SourceId, string Url, string Finding);

sealed record Check(string CheckId, bool Passed, string Detail);
