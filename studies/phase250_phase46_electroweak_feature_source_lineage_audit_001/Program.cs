using System.Text.Json;

const string DefaultOutputDir = "studies/phase250_phase46_electroweak_feature_source_lineage_audit_001/output";
const string Phase46SourceRoot = "studies/phase46_electroweak_term_wz_source_spectra_001";
const string Phase46PhysicalRoot = "studies/phase46_electroweak_term_wz_physical_prediction_001";
const string Phase46SpectraManifestPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/spectra_manifest.json";
const string Phase46SourceCandidatesPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/source_candidates.json";
const string Phase46CandidateModeSourcesPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/candidate_mode_sources.json";
const string Phase46PromotionResultPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/promotion_result.json";
const string Phase46WzRatioDiagnosticPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/wz_ratio_diagnostic.json";
const string Phase46SelectorAuditPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/selector_eigen_operator_term_audit.json";
const string Phase46PhysicalCalibrationsPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/physical_calibrations.json";
const string Phase46PhysicalMappingsPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/physical_observable_mappings.json";
const string Phase46ObservablesPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/observables.json";
const string Phase46PhysicalModeRecordsPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/physical_mode_records.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase249Path = "studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE250_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var spectraManifest = ParseJson(Phase46SpectraManifestPath);
using var sourceCandidates = ParseJson(Phase46SourceCandidatesPath);
using var candidateModeSources = ParseJson(Phase46CandidateModeSourcesPath);
using var promotionResult = ParseJson(Phase46PromotionResultPath);
using var wzRatioDiagnostic = ParseJson(Phase46WzRatioDiagnosticPath);
using var selectorAudit = ParseJson(Phase46SelectorAuditPath);
using var physicalCalibrations = ParseJson(Phase46PhysicalCalibrationsPath);
using var physicalMappings = ParseJson(Phase46PhysicalMappingsPath);
using var observables = ParseJson(Phase46ObservablesPath);
using var physicalModeRecords = ParseJson(Phase46PhysicalModeRecordsPath);
using var phase213 = ParseJson(Phase213Path);
using var phase221 = ParseJson(Phase221Path);
using var phase225 = ParseJson(Phase225Path);
using var phase245 = ParseJson(Phase245Path);
using var phase247 = ParseJson(Phase247Path);
using var phase249 = ParseJson(Phase249Path);

var phase46Files = EnumerateEvidenceFiles(Phase46SourceRoot)
    .Concat(EnumerateEvidenceFiles(Phase46PhysicalRoot))
    .Distinct(StringComparer.Ordinal)
    .ToArray();

var manifestEntryCount = JsonArrayLength(spectraManifest.RootElement, "entries");
var manifestMatrixCellCount = JsonInt(spectraManifest.RootElement, "matrixCellCount") ?? 0;
var sourceCandidateCount = JsonArrayLength(sourceCandidates.RootElement, "candidates");
var readySourceCount = JsonInt(candidateModeSources.RootElement, "readySourceCount") ?? 0;
var sourceCandidateNotPhysicalPredictionAssumptionCount = sourceCandidates.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Count(row => JsonStringArray(row, "assumptions").Any(text => text.Contains("not a W or Z physical prediction", StringComparison.OrdinalIgnoreCase)));

var tripletLabelOccurrenceCount = CountOccurrences(phase46Files, "su2-adjoint-triplet:canonical-basis");
var electroweakFeatureTermOccurrenceCount = CountOccurrences(phase46Files, "electroweak-feature-charge-anisotropy:v1");
var fullHessianOccurrenceCount = CountOccurrences(phase46Files, "\"operatorType\": \"FullHessian\"");
var phase64OccurrenceCount = CountOccurrences(phase46Files, "phase64");
var fermionCurrentOccurrenceCount = CountOccurrences(phase46Files, "fermion-current");
var traceHalfOccurrenceCount = CountOccurrences(phase46Files, "trace-half");
var casimirRmsOccurrenceCount = CountOccurrences(phase46Files, "casimir-rms");
var sourceLineageFieldCount = CountOccurrences(phase46Files, "sourceLineage");
var sourceRowIdFieldCount = CountOccurrences(phase46Files, "sourceRowId");
var theoremOrDerivationIdFieldCount = CountOccurrences(phase46Files, "theoremOrDerivationId");
var rawAmplitudeGatePassedFieldCount = CountOccurrences(phase46Files, "rawAmplitudeGatePassed");
var derivationIdFieldCount = CountOccurrences(phase46Files, "\"derivationId\"");

var physicalRows = physicalModeRecords.RootElement.ValueKind == JsonValueKind.Array
    ? physicalModeRecords.RootElement.EnumerateArray().ToArray()
    : Array.Empty<JsonElement>();
var promotionModeRows = promotionResult.RootElement.TryGetProperty("physicalModeRecords", out var promotionModeElement)
    ? promotionModeElement.EnumerateArray().ToArray()
    : Array.Empty<JsonElement>();
var modeRows = physicalRows.Length > 0 ? physicalRows : promotionModeRows;
var phase46HasSeparateWzModeRecords = modeRows.Any(row => JsonString(row, "particleId") == "w-boson")
    && modeRows.Any(row => JsonString(row, "particleId") == "z-boson");
var physicalModeRecordUnits = modeRows
    .Select(row => JsonString(row, "unit"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .ToArray();
var phase46ModeRecordsUseInternalMassUnit = physicalModeRecordUnits.Length == 1
    && physicalModeRecordUnits[0] == "internal-mass-unit";

var observableIds = observables.RootElement.ValueKind == JsonValueKind.Array
    ? observables.RootElement.EnumerateArray()
        .Select(row => JsonString(row, "observableId"))
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Cast<string>()
        .ToArray()
    : Array.Empty<string>();
var absoluteMassObservableIds = observableIds
    .Where(id => id.Contains("w-boson-mass", StringComparison.Ordinal)
        || id.Contains("z-boson-mass", StringComparison.Ordinal)
        || id == "physical-w-boson-mass-gev"
        || id == "physical-z-boson-mass-gev")
    .ToArray();

var mappings = physicalMappings.RootElement.GetProperty("mappings").EnumerateArray().ToArray();
var mappingObservableTypes = mappings
    .Select(row => JsonString(row, "physicalObservableType"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .ToArray();
var mappingUnitFamilies = mappings
    .Select(row => JsonString(row, "unitFamily"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .ToArray();
var phase46HasOnlyDimensionlessRatioMapping = mappingObservableTypes.SequenceEqual(new[] { "mass-ratio" })
    && mappingUnitFamilies.SequenceEqual(new[] { "dimensionless" });

var calibrationRows = physicalCalibrations.RootElement.GetProperty("calibrations").EnumerateArray().ToArray();
var calibrationAssumptions = calibrationRows.SelectMany(row => JsonStringArray(row, "assumptions")).ToArray();
var phase46CalibrationExplicitlyExcludesAbsoluteMass = calibrationAssumptions.Any(text => text.Contains("not to either absolute boson mass", StringComparison.OrdinalIgnoreCase));
var phase46CalibrationExplicitlyDimensionlessRatio = calibrationAssumptions.Any(text => text.Contains("dimensionless ratio comparison", StringComparison.OrdinalIgnoreCase));
var calibrationScaleFactors = calibrationRows.Select(row => JsonDouble(row, "scaleFactor")).Where(value => value.HasValue).Select(value => value!.Value).ToArray();
var phase46CalibrationScaleFactorIsIdentity = calibrationScaleFactors.Length > 0
    && calibrationScaleFactors.All(value => Math.Abs(value - 1.0) < 1.0e-12);

var selectedPair = wzRatioDiagnostic.RootElement.GetProperty("selectedPair");
var selectedPairId = JsonString(selectedPair, "pairId") ?? "missing";
var selectedRatio = JsonDouble(selectedPair, "ratio");
var selectedPairPassesSigma5 = JsonBool(selectedPair, "passesSigma5") is true;
var targetValue = JsonDouble(wzRatioDiagnostic.RootElement, "targetValue");
var targetObservableId = JsonString(wzRatioDiagnostic.RootElement, "targetObservableId");
var selectedRequiredScaleToTarget = JsonDouble(selectedPair, "requiredScaleToTarget");
var selectorRequiredScaleToTarget = JsonDouble(selectorAudit.RootElement, "requiredScaleToTarget");
var targetComparisonGateIsRatioOnly = targetObservableId == "physical-w-z-mass-ratio";
var selectorObservedOperatorTypes = JsonStringArray(selectorAudit.RootElement, "observedOperatorTypes");
var selectorObservedModeBlocks = JsonStringArray(selectorAudit.RootElement, "observedModeBlocks");
var selectorOnlyFullHessian = selectorObservedOperatorTypes.Length == 1
    && selectorObservedOperatorTypes[0] == "FullHessian";

var p221NumericalTargetComparisonPassed = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var p221SourceLineagePromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var p225RepresentationObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var p247CurrentDirectBridgeCandidatePromotable = JsonBool(phase247.RootElement, "currentDirectBridgeCandidatePromotable") is true;
var p247SourceRowRepairPossibleFromCurrentRegistry = JsonBool(phase247.RootElement, "sourceRowRepairPossibleFromCurrentRegistry") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p249WzInvariantFormulaSourceBacked = JsonBool(phase249.RootElement, "wzInvariantFormulaSourceBacked") is true;
var p249NewSourceEvidenceStillRequired = JsonBool(phase249.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var phase46HasElectroweakFeatureTripletLabels = tripletLabelOccurrenceCount > 0
    && electroweakFeatureTermOccurrenceCount > 0;
var phase46SupportsRatioOnlyDiagnostic = phase46HasOnlyDimensionlessRatioMapping
    && phase46CalibrationExplicitlyExcludesAbsoluteMass
    && phase46CalibrationExplicitlyDimensionlessRatio
    && phase46CalibrationScaleFactorIsIdentity
    && absoluteMassObservableIds.Length == 0;
var phase46ReferencesPhase64FermionCurrentTraceHalf = phase64OccurrenceCount > 0
    || fermionCurrentOccurrenceCount > 0
    || traceHalfOccurrenceCount > 0;
var phase46ProvidesAdjointRmsApplicationTheorem = phase46ReferencesPhase64FermionCurrentTraceHalf
    && casimirRmsOccurrenceCount > 0
    && theoremOrDerivationIdFieldCount > 0
    && !p225RepresentationObstructionCertified
    && p249WzInvariantFormulaSourceBacked;
var phase46ProvidesSeparateWzSourceRows = phase46HasSeparateWzModeRecords
    && sourceRowIdFieldCount >= 2
    && sourceLineageFieldCount >= 2
    && theoremOrDerivationIdFieldCount > 0
    && rawAmplitudeGatePassedFieldCount >= 2;
var phase46FillsWzAbsoluteScaleUnlock = phase46ProvidesSeparateWzSourceRows
    && phase46ProvidesAdjointRmsApplicationTheorem
    && !phase46SupportsRatioOnlyDiagnostic
    && absoluteMassObservableIds.Length >= 2
    && !p245NewSourceEvidenceStillRequired;
var phase46AbsoluteMassClaimPromotable = phase46FillsWzAbsoluteScaleUnlock
    && !p247NewDirectBridgeTheoremStillRequired
    && !p247SourceRowRepairPossibleFromCurrentRegistry
    && !p247CurrentDirectBridgeCandidatePromotable;
var newSourceEvidenceStillRequired = p245NewSourceEvidenceStillRequired
    && p247NewDirectBridgeTheoremStillRequired
    && p249NewSourceEvidenceStillRequired
    && !phase46FillsWzAbsoluteScaleUnlock;

var evidenceRows = new[]
{
    new EvidenceRow(
        "phase46-source-spectra",
        "Phase46 source spectra",
        true,
        "internal-electroweak-feature-spectra",
        "The spectra include SU(2) adjoint-triplet electroweak-feature labels, but source candidates are internal mass-unit artifacts and source-candidate assumptions say they are not W or Z physical predictions.",
        false),
    new EvidenceRow(
        "phase46-physical-mode-records",
        "Phase46 promoted physical mode records",
        phase46HasSeparateWzModeRecords,
        "wz-internal-mode-identity",
        "The promoted records identify separate W and Z internal modes, but both use internal-mass-unit and do not supply GeV-scale absolute masses.",
        false),
    new EvidenceRow(
        "phase46-ratio-calibration",
        "Phase46 physical calibration",
        phase46SupportsRatioOnlyDiagnostic,
        "dimensionless-ratio-calibration",
        "The calibration is explicitly dimensionless identity normalization and says it applies only to the ratio, not either absolute boson mass.",
        false),
    new EvidenceRow(
        "phase46-wz-ratio-target-diagnostic",
        "Phase46 W/Z ratio diagnostic",
        targetComparisonGateIsRatioOnly && selectedPairPassesSigma5,
        "ratio-target-comparison",
        "The target comparison is for physical-w-z-mass-ratio and cannot fill W/Z absolute source-row or raw-amplitude gates.",
        false),
    new EvidenceRow(
        "phase46-selector-eigen-operator-term",
        "Phase46 selector eigen operator audit",
        selectorOnlyFullHessian,
        "full-hessian-selector-diagnostic",
        "The selector audit records FullHessian/electroweak-mixing operator evidence and a target ratio scale, not a Phase64 fermion-current adjoint-RMS application theorem.",
        false),
};

var checks = new[]
{
    new Check("phase46-artifacts-present", manifestEntryCount > 0 && sourceCandidateCount > 0 && readySourceCount > 0 && modeRows.Length > 0, $"manifestEntryCount={manifestEntryCount}; sourceCandidateCount={sourceCandidateCount}; readySourceCount={readySourceCount}; modeRecordCount={modeRows.Length}"),
    new Check("electroweak-feature-triplet-labels-present", phase46HasElectroweakFeatureTripletLabels, $"tripletLabelOccurrenceCount={tripletLabelOccurrenceCount}; electroweakFeatureTermOccurrenceCount={electroweakFeatureTermOccurrenceCount}; fullHessianOccurrenceCount={fullHessianOccurrenceCount}"),
    new Check("phase46-ratio-only-calibration-explicit", phase46SupportsRatioOnlyDiagnostic, $"phase46HasOnlyDimensionlessRatioMapping={phase46HasOnlyDimensionlessRatioMapping}; phase46CalibrationExplicitlyExcludesAbsoluteMass={phase46CalibrationExplicitlyExcludesAbsoluteMass}; absoluteMassObservableCount={absoluteMassObservableIds.Length}"),
    new Check("phase46-contract-fields-absent", sourceLineageFieldCount == 0 && sourceRowIdFieldCount == 0 && theoremOrDerivationIdFieldCount == 0 && rawAmplitudeGatePassedFieldCount == 0, $"sourceLineageFieldCount={sourceLineageFieldCount}; sourceRowIdFieldCount={sourceRowIdFieldCount}; theoremOrDerivationIdFieldCount={theoremOrDerivationIdFieldCount}; rawAmplitudeGatePassedFieldCount={rawAmplitudeGatePassedFieldCount}; derivationIdFieldCount={derivationIdFieldCount}"),
    new Check("phase46-adjoint-rms-application-theorem-not-provided", !phase46ProvidesAdjointRmsApplicationTheorem && p225RepresentationObstructionCertified && p221NumericalTargetComparisonPassed && !p221SourceLineagePromotable && !p249WzInvariantFormulaSourceBacked, $"phase46ReferencesPhase64FermionCurrentTraceHalf={phase46ReferencesPhase64FermionCurrentTraceHalf}; casimirRmsOccurrenceCount={casimirRmsOccurrenceCount}; p225RepresentationObstructionCertified={p225RepresentationObstructionCertified}; p249WzInvariantFormulaSourceBacked={p249WzInvariantFormulaSourceBacked}"),
    new Check("phase46-wz-absolute-scale-unlock-not-filled", !phase46FillsWzAbsoluteScaleUnlock && !p245UnlockContractFilled && p245NewSourceEvidenceStillRequired && p247NewDirectBridgeTheoremStillRequired, $"phase46FillsWzAbsoluteScaleUnlock={phase46FillsWzAbsoluteScaleUnlock}; p245UnlockContractFilled={p245UnlockContractFilled}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}"),
    new Check("phase46-no-absolute-mass-promotion", !phase46AbsoluteMassClaimPromotable && phase46ModeRecordsUseInternalMassUnit && absoluteMassObservableIds.Length == 0, $"phase46AbsoluteMassClaimPromotable={phase46AbsoluteMassClaimPromotable}; phase46ModeRecordsUseInternalMassUnit={phase46ModeRecordsUseInternalMassUnit}; absoluteMassObservableIds=[{string.Join(",", absoluteMassObservableIds)}]"),
    new Check("source-blocker-counts-preserved", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14 && newSourceEvidenceStillRequired, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var phase46ElectroweakFeatureAuditPassed = checks.All(check => check.Passed);
var terminalStatus = phase46ElectroweakFeatureAuditPassed
    ? "phase46-electroweak-feature-source-lineage-audit-complete-ratio-only-not-wz-absolute-source"
    : "phase46-electroweak-feature-source-lineage-audit-review-required";
var phase46BestPromotableScope = "wz-ratio-or-internal-electroweak-feature-diagnostic";

var result = new
{
    phaseId = "phase250-phase46-electroweak-feature-source-lineage-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    phase46ElectroweakFeatureAuditPassed,
    phase46HasElectroweakFeatureTripletLabels,
    phase46SupportsRatioOnlyDiagnostic,
    phase46HasSeparateWzModeRecords,
    phase46ProvidesSeparateWzSourceRows,
    phase46ProvidesAdjointRmsApplicationTheorem,
    phase46FillsWzAbsoluteScaleUnlock,
    phase46AbsoluteMassClaimPromotable,
    newSourceEvidenceStillRequired,
    phase46BestPromotableScope,
    artifactInventory = new
    {
        phase46FileCount = phase46Files.Length,
        manifestEntryCount,
        manifestMatrixCellCount,
        sourceCandidateCount,
        readySourceCount,
        sourceCandidateNotPhysicalPredictionAssumptionCount,
        physicalModeRecordCount = modeRows.Length,
        physicalModeRecordUnits,
        observableIds,
        absoluteMassObservableIds,
    },
    phase46Signals = new
    {
        tripletLabelOccurrenceCount,
        electroweakFeatureTermOccurrenceCount,
        fullHessianOccurrenceCount,
        derivationIdFieldCount,
        sourceLineageFieldCount,
        sourceRowIdFieldCount,
        theoremOrDerivationIdFieldCount,
        rawAmplitudeGatePassedFieldCount,
        phase64OccurrenceCount,
        fermionCurrentOccurrenceCount,
        traceHalfOccurrenceCount,
        casimirRmsOccurrenceCount,
    },
    ratioEvidence = new
    {
        targetObservableId,
        targetValue,
        selectedPairId,
        selectedRatio,
        selectedPairPassesSigma5,
        selectedRequiredScaleToTarget,
        selectorRequiredScaleToTarget,
        phase46HasOnlyDimensionlessRatioMapping,
        mappingObservableTypes,
        mappingUnitFamilies,
        phase46CalibrationExplicitlyDimensionlessRatio,
        phase46CalibrationExplicitlyExcludesAbsoluteMass,
        phase46CalibrationScaleFactorIsIdentity,
        selectorObservedOperatorTypes,
        selectorObservedModeBlocks,
    },
    currentBlockerState = new
    {
        wzMissingFieldCount,
        higgsMissingFieldCount,
        p221NumericalTargetComparisonPassed,
        p221SourceLineagePromotable,
        p225RepresentationObstructionCertified,
        p245UnlockContractFilled,
        p245NewSourceEvidenceStillRequired,
        p247CurrentDirectBridgeCandidatePromotable,
        p247SourceRowRepairPossibleFromCurrentRegistry,
        p247NewDirectBridgeTheoremStillRequired,
        p249WzInvariantFormulaSourceBacked,
        p249NewSourceEvidenceStillRequired,
    },
    evidenceRows,
    checks,
    decision = phase46ElectroweakFeatureAuditPassed
        ? "Phase46 is useful as W/Z ratio and internal electroweak-feature evidence, but it does not supply the missing W/Z absolute-scale source rows, raw-amplitude gates, or the theorem applying the P221/P249 SU(2) adjoint RMS factor to the Phase64 fermion-current trace-half source. It cannot repair the direct bridge from current artifacts."
        : "Review Phase46 source-lineage classification before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A target-independent W/Z absolute-scale source law with theoremOrDerivationId and sourceLineageId.",
        "Separate W and Z sourceRowId entries with rawAmplitudeGatePassed=true and common bridge/normalization sidecars.",
        "A theorem or replayed matrix element applying any SU(2) adjoint RMS normalization to the actual Phase64 fermion-current source, not only to Phase46 internal selector spectra.",
    },
    sourceEvidence = new
    {
        phase46SpectraManifestPath = Phase46SpectraManifestPath,
        phase46SourceCandidatesPath = Phase46SourceCandidatesPath,
        phase46CandidateModeSourcesPath = Phase46CandidateModeSourcesPath,
        phase46PromotionResultPath = Phase46PromotionResultPath,
        phase46WzRatioDiagnosticPath = Phase46WzRatioDiagnosticPath,
        phase46SelectorAuditPath = Phase46SelectorAuditPath,
        phase46PhysicalCalibrationsPath = Phase46PhysicalCalibrationsPath,
        phase46PhysicalMappingsPath = Phase46PhysicalMappingsPath,
        phase46ObservablesPath = Phase46ObservablesPath,
        phase46PhysicalModeRecordsPath = Phase46PhysicalModeRecordsPath,
        phase213Path = Phase213Path,
        phase221Path = Phase221Path,
        phase225Path = Phase225Path,
        phase245Path = Phase245Path,
        phase247Path = Phase247Path,
        phase249Path = Phase249Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase46_electroweak_feature_source_lineage_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase46_electroweak_feature_source_lineage_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase46ElectroweakFeatureAuditPassed,
        result.phase46HasElectroweakFeatureTripletLabels,
        result.phase46SupportsRatioOnlyDiagnostic,
        result.phase46HasSeparateWzModeRecords,
        result.phase46ProvidesSeparateWzSourceRows,
        result.phase46ProvidesAdjointRmsApplicationTheorem,
        result.phase46FillsWzAbsoluteScaleUnlock,
        result.phase46AbsoluteMassClaimPromotable,
        result.newSourceEvidenceStillRequired,
        result.phase46BestPromotableScope,
        result.artifactInventory,
        result.phase46Signals,
        result.ratioEvidence,
        result.currentBlockerState,
        result.evidenceRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase46ElectroweakFeatureAuditPassed={phase46ElectroweakFeatureAuditPassed}");
Console.WriteLine($"phase46HasElectroweakFeatureTripletLabels={phase46HasElectroweakFeatureTripletLabels}");
Console.WriteLine($"phase46SupportsRatioOnlyDiagnostic={phase46SupportsRatioOnlyDiagnostic}");
Console.WriteLine($"phase46FillsWzAbsoluteScaleUnlock={phase46FillsWzAbsoluteScaleUnlock}");
Console.WriteLine($"phase46ProvidesAdjointRmsApplicationTheorem={phase46ProvidesAdjointRmsApplicationTheorem}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static JsonDocument ParseJson(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Required artifact missing: {path}", path);
    }

    return JsonDocument.Parse(File.ReadAllText(path));
}

static string[] EnumerateEvidenceFiles(string root) =>
    Directory.Exists(root)
        ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray()
        : Array.Empty<string>();

static int CountOccurrences(IEnumerable<string> files, string pattern)
{
    var count = 0;
    foreach (var file in files)
    {
        var text = File.ReadAllText(file);
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
    }

    return count;
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static int JsonArrayLength(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array ? property.GetArrayLength() : 0;

sealed record EvidenceRow(
    string EvidenceId,
    string Label,
    bool Present,
    string BestSupportedScope,
    string Detail,
    bool FillsWzAbsoluteScaleUnlock);

sealed record Check(string CheckId, bool Passed, string Detail);
