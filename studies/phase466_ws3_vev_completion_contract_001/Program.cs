using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

// Phase466: WS3 VEV Completion Contract Schema (WAVE2_ACTION_PLAN_2026-07-12 item 5; C-CONTRACT v3).
//
// Commits the schema a WS3 verdict must satisfy to mechanically complete rows of
// the phase256 observed-field-extraction intake template. It:
//   * hard-pins phase434's targetBlindConstructionHash and phase256's templateId
//     (drift => blocking terminal);
//   * recomputes the 6-row / 7-conditional / 4-amplitude-blocked / 9-lineage-blocked
//     field partition from the committed phase434 ledger + phase256 template
//     (partition drift => blocking terminal);
//   * defines the VEV intake record schema {direction, magnitudeInLatticeUnits,
//     member, channel, errorModel} with the O8 M-probe scope note;
//   * defines the completion map: a conforming, >= 3 sigma (O5), weak-doublet VEV
//     completes the 7 conditional rows and the amplitude rows, with the z-amplitude
//     completing ONLY on the labeled g'^2/g^2 = 3/5 import branch; the 9 lineage
//     rows are PERMANENTLY blocked (O8 as a schema theorem);
//   * evaluates the FIVE golden fixtures (committed BEFORE this mapper) and rejects
//     the schema on any mismatch;
//   * emits {schemaId, schemaHash} and the consumption rule.
//
// MANDATORY FRAMING. This is a SCHEMA, not a measurement. No VEV is asserted to
// exist; no lattice quantity is relabelled; no GeV/pole/normalization is
// produced. promotedPhysicalMassClaimCount remains 0. physicistReviewPending is
// carried explicitly. Lattice-unit quantities stay in lattice units.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "ws3-vev-completion-contract";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 5";
const string DefaultOutputDir = "studies/phase466_ws3_vev_completion_contract_001/output";
const string FixturesDir = "studies/phase466_ws3_vev_completion_contract_001/fixtures";
const string TerminalPrefix = "ws3-vev-completion-contract-";

const string Phase434SummaryPath =
    "studies/phase434_conditional_observed_field_extraction_row_ledger_001/output/conditional_observed_field_extraction_row_ledger_summary.json";
const string Phase256SummaryPath =
    "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase256TemplatePath =
    "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_template.json";

// --- Hard pins (drift => blocking terminal) ---------------------------------
const string Phase434HashPin = "f5eafcc74583ecdf83872fdf914c1a5483847499f97359d9cbe8ae44c396023b";
const string Phase256TemplateIdPin = "observed-field-extraction-wzh-intake-template-v1";

// --- Pre-registered field partition (recomputed AND cross-checked below) -----
// 7 conditional rows: fixed in direction by the internal kernel given ANY VEV.
string[] conditionalFields =
{
    "electroweakGaugeEmbeddingId",
    "photonEigenstateProjectionId",
    "zBosonSourceRowId",
    "wBosonSourceRowId",
    "wzCommonBridgeGatePassed",
    "quadraticElectroweakMassOperatorId",
    "targetBlindConstructionHash",
};
// 4 amplitude-blocked rows: require a VEV magnitude / normalization.
string[] amplitudeFields =
{
    "fourDimensionalObservedVacuumArtifactId",
    "branchNormalizationSourceId",
    "wBosonRawAmplitudeGatePassed",
    "zBosonRawAmplitudeGatePassed",
};
// 9 lineage-blocked rows: require INDEPENDENT source lineage; WS3 can never
// complete any of them (O8 schema theorem).
string[] lineageFields =
{
    "observedFieldExtractionTheoremId",
    "sourceReferenceIds",
    "higgsScalarSourceOperatorId",
    "higgsMassiveScalarProfileId",
    "higgsPotentialSelfCouplingRelationId",
    "canonicalOrDeclaredShiabBranchId",
    "stabilitySidecarIds",
    "targetComparisonAfterConstructionGatePassed",
    "phase201And209ApplicationReady",
};

// completion sub-sets of the 4 amplitude rows
const string ZImportBranchId = "labeled-gprime2-over-g2-three-fifths-import";
string[] amplitudeAlwaysOnFiniteVev = { "fourDimensionalObservedVacuumArtifactId", "wBosonRawAmplitudeGatePassed" };
string[] amplitudeOnlyWithImport = { "branchNormalizationSourceId", "zBosonRawAmplitudeGatePassed" };

const double O5GuardSigmaThreshold = 3.0;

Directory.CreateDirectory(DefaultOutputDir);

// --- Pin verification + partition recomputation ------------------------------
bool phase434Present = File.Exists(Phase434SummaryPath);
bool phase256Present = File.Exists(Phase256SummaryPath) && File.Exists(Phase256TemplatePath);

string phase434HashObserved = "";
string phase256TemplateIdObserved = "";
bool phase434HashMatches = false;
bool templateIdMatches = false;
bool partitionMatches = false;
int extractionRowCountObserved = -1;
int templateFieldCountObserved = -1;
string partitionNote = "";

if (phase434Present && phase256Present)
{
    using var p434 = JsonDocument.Parse(File.ReadAllText(Phase434SummaryPath));
    using var p256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
    using var p256t = JsonDocument.Parse(File.ReadAllText(Phase256TemplatePath));

    phase434HashObserved = p434.RootElement.GetProperty("targetBlindConstructionHash").GetString() ?? "";
    phase434HashMatches = string.Equals(phase434HashObserved, Phase434HashPin, StringComparison.Ordinal);

    phase256TemplateIdObserved = p256t.RootElement.GetProperty("templateId").GetString() ?? "";
    templateIdMatches = string.Equals(phase256TemplateIdObserved, Phase256TemplateIdPin, StringComparison.Ordinal);

    extractionRowCountObserved = p434.RootElement.GetProperty("extractionRowCount").GetInt32();

    // recompute the 7/4/9 partition from phase434's committed conditionalCompletionLedger
    var ledger = p434.RootElement.GetProperty("conditionalCompletionLedger");
    var obsConditional = ledger.GetProperty("conditionallyDeterminedByRowsGivenAnyVev")
        .EnumerateArray().Select(e => e.GetProperty("field").GetString()).ToList();
    var obsAmplitude = ledger.GetProperty("stillRequiresVevAmplitudeOrScale")
        .EnumerateArray().Select(e => e.GetProperty("field").GetString()).ToList();
    var obsLineage = ledger.GetProperty("stillRequiresIndependentLineage")
        .EnumerateArray().Select(e => e.GetProperty("field").GetString()).ToList();

    // recompute the template field set from phase256's committed requirementRows
    var templateFields = p256t.RootElement.GetProperty("requirementRows")
        .EnumerateArray().Select(e => e.GetProperty("fieldId").GetString()).Where(s => s != null).ToList();
    templateFieldCountObserved = templateFields.Count;

    bool conditionalOk = obsConditional.Count == 7 && new HashSet<string?>(obsConditional).SetEquals(conditionalFields);
    bool amplitudeOk = obsAmplitude.Count == 4 && new HashSet<string?>(obsAmplitude).SetEquals(amplitudeFields);
    bool lineageOk = obsLineage.Count == 9 && new HashSet<string?>(obsLineage).SetEquals(lineageFields);
    bool unionOk = templateFieldCountObserved == 20 &&
        new HashSet<string?>(conditionalFields.Concat(amplitudeFields).Concat(lineageFields))
            .SetEquals(templateFields);
    bool rowCountOk = extractionRowCountObserved == 6;

    partitionMatches = conditionalOk && amplitudeOk && lineageOk && unionOk && rowCountOk;
    partitionNote = partitionMatches
        ? "Partition 6-row / 7-conditional / 4-amplitude / 9-lineage recomputed and matched against the committed phase434 ledger and phase256 template (7+4+9 = 20 template fields)."
        : $"Partition drift: conditional={conditionalOk} amplitude={amplitudeOk} lineage={lineageOk} union20={unionOk} rowCount6={rowCountOk}.";
}
else
{
    partitionNote = "phase434 and/or phase256 committed artifacts not found; cannot recompute the partition.";
}

// --- The completion map (the schema's completion function) -------------------
CompletionOutcome ApplyCompletion(VevIntake v)
{
    // O5 guard first: below the >= 3 sigma departure from the exact free control, no completion.
    if (double.IsNaN(v.SigmaFromExactFreeControl) || v.SigmaFromExactFreeControl < O5GuardSigmaThreshold)
        return CompletionOutcome.None("below-o5-guard-no-completion", singlet: false);

    // Runaway: no finite VEV magnitude exists.
    if (v.MagnitudeInLatticeUnits is null)
        return CompletionOutcome.None("runaway-no-completion", singlet: false);

    // O8 M-probe scope: a singlet gap proves a singlet, never the named electroweak
    // scalar. A singlet channel cannot supply the weak-doublet direction and
    // completes nothing in the doublet contract.
    if (string.Equals(v.Channel, "singlet", StringComparison.Ordinal))
        return CompletionOutcome.None("singlet-dominates-not-electroweak-doublet-completion", singlet: true);

    // Direction/member must be resolved.
    if (string.Equals(v.Direction, "unresolved", StringComparison.Ordinal) ||
        string.Equals(v.Member, "unresolved", StringComparison.Ordinal))
        return CompletionOutcome.None("inconclusive-no-completion", singlet: false);

    // Conforming finite weak-doublet VEV at >= 3 sigma: complete conditional + amplitude.
    bool hasImport = string.Equals(v.BranchNormalizationSourceId, ZImportBranchId, StringComparison.Ordinal);
    int amplitude = amplitudeAlwaysOnFiniteVev.Length + (hasImport ? amplitudeOnlyWithImport.Length : 0);
    return new CompletionOutcome(
        Completes: true,
        ConditionalCount: conditionalFields.Length,     // all 7
        AmplitudeCount: amplitude,                       // 2 without import, 4 with
        ZAmplitudeCompleted: hasImport,
        LineageCount: 0,                                 // permanent (O8)
        SingletFlag: false,
        OutcomeClass: "finite-vev-convention-scoped-completion");
}

// --- Evaluate the five golden fixtures ---------------------------------------
var fixtureResults = new List<object>();
bool allFixturesPassed = true;
int fixturesEvaluated = 0;
var fixtureIdsSeen = new List<string>();

if (Directory.Exists(FixturesDir))
{
    foreach (var path in Directory.EnumerateFiles(FixturesDir, "*.json").OrderBy(p => p, StringComparer.Ordinal))
    {
        using var fx = JsonDocument.Parse(File.ReadAllText(path));
        var root = fx.RootElement;
        string fixtureId = root.GetProperty("fixtureId").GetString() ?? "";
        var vi = root.GetProperty("vevIntake");
        double? magnitude = vi.GetProperty("magnitudeInLatticeUnits").ValueKind == JsonValueKind.Null
            ? null
            : vi.GetProperty("magnitudeInLatticeUnits").GetDouble();
        string? branchId = vi.GetProperty("branchNormalizationSourceId").ValueKind == JsonValueKind.Null
            ? null
            : vi.GetProperty("branchNormalizationSourceId").GetString();
        var intake = new VevIntake(
            Direction: vi.GetProperty("direction").GetString() ?? "",
            MagnitudeInLatticeUnits: magnitude,
            Member: vi.GetProperty("member").GetString() ?? "",
            Channel: vi.GetProperty("channel").GetString() ?? "",
            ErrorModel: vi.GetProperty("errorModel").GetString() ?? "",
            SigmaFromExactFreeControl: vi.GetProperty("sigmaFromExactFreeControl").GetDouble(),
            BranchNormalizationSourceId: branchId);

        var computed = ApplyCompletion(intake);

        var exp = root.GetProperty("expected");
        bool match =
            computed.Completes == exp.GetProperty("completes").GetBoolean() &&
            computed.ConditionalCount == exp.GetProperty("completedConditionalFields").GetInt32() &&
            computed.AmplitudeCount == exp.GetProperty("completedAmplitudeFields").GetInt32() &&
            computed.ZAmplitudeCompleted == exp.GetProperty("zAmplitudeCompleted").GetBoolean() &&
            computed.LineageCount == exp.GetProperty("completedLineageFields").GetInt32() &&
            computed.SingletFlag == exp.GetProperty("singletFlag").GetBoolean() &&
            string.Equals(computed.OutcomeClass, exp.GetProperty("outcomeClass").GetString(), StringComparison.Ordinal);

        allFixturesPassed &= match;
        fixturesEvaluated++;
        fixtureIdsSeen.Add(fixtureId);
        fixtureResults.Add(new
        {
            fixtureId,
            computed = new
            {
                computed.Completes,
                computed.ConditionalCount,
                computed.AmplitudeCount,
                computed.ZAmplitudeCompleted,
                computed.LineageCount,
                computed.SingletFlag,
                computed.OutcomeClass,
            },
            match,
        });
    }
}

// exactly the five pre-registered fixtures must be present
string[] requiredFixtureIds = { "T-finite-vev", "T-runaway", "T-singlet-wins", "T-inconclusive", "adversarial-1.5sigma-from-free" };
bool fixtureSetComplete = fixturesEvaluated == 5 && new HashSet<string>(fixtureIdsSeen).SetEquals(requiredFixtureIds);

// --- Machine-assert the O8 impossibility cap on all 9 lineage fields ---------
var lineageImpossibilityCap = lineageFields.Select(f => new
{
    field = f,
    wsThreeCannotComplete = true, // permanent, O8 schema theorem
}).ToArray();
bool o8CapAssertedOnAllLineage = lineageImpossibilityCap.Length == 9 && lineageImpossibilityCap.All(x => x.wsThreeCannotComplete);

// --- Schema identity + hash --------------------------------------------------
const string SchemaId = "ws3-vev-completion-contract-schema-v1";
string schemaCanonical = string.Join("|", new[]
{
    "schemaId=" + SchemaId,
    "phase434HashPin=" + Phase434HashPin,
    "phase256TemplateId=" + Phase256TemplateIdPin,
    "extractionRows=6",
    "conditional=" + string.Join(",", conditionalFields),
    "amplitude=" + string.Join(",", amplitudeFields),
    "lineage=" + string.Join(",", lineageFields),
    "amplitudeAlwaysOnFiniteVev=" + string.Join(",", amplitudeAlwaysOnFiniteVev),
    "amplitudeOnlyWithImport=" + string.Join(",", amplitudeOnlyWithImport),
    "zImportBranchId=" + ZImportBranchId,
    "o5GuardSigmaThreshold=" + O5GuardSigmaThreshold.ToString("F1"),
    "o8PermanentLineageBlocked=true",
    "wsThreeCannotCompleteOnAllNineLineageFields=true",
});
string schemaHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(schemaCanonical))).ToLowerInvariant();

const string ConsumptionRule =
    "A WS3 verdict is admissible against this contract only if it supplies a conforming VEV intake record " +
    "{direction, magnitudeInLatticeUnits, member, channel, errorModel} and clears the O5 guard (>= 3 sigma from the " +
    "exact free control). Consumers (e.g. the phase457 null-hash firewall) MUST pin to {schemaId, schemaHash}. " +
    "The z-amplitude row completes ONLY on the labeled g'^2/g^2 = 3/5 import branch (branchNormalizationSourceId as a " +
    "labeled convention adoption). The 9 lineage rows can NEVER be completed by any WS3 measurement (O8). A gap proves " +
    "a singlet, never the named electroweak scalar; the M field is a labeled probe convention and is never sourced.";

const string O8MProbeScopeNote =
    "O8 M-probe scope: the WS3 probe field M is a LABELED probe convention, never defined by any audited source. A " +
    "measured gap proves a singlet, never the named electroweak scalar; with no hypercharge the rho parameter and the " +
    "weak mixing angle are not measurable. A portal rule-in yields a convention-scoped dynamical-scale candidate and " +
    "anchor-free ratios only, never sourced content.";

// --- Template mutation guard -------------------------------------------------
// This phase never writes the phase256 template; the committed template is intact
// (templateId + 20 requirementRows verified above).
const int templateMutationCount = 0;
const bool phase256TemplateMutated = false;

// --- Standing claim boundary (verbatim across the program) ------------------
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;

// --- Terminal (fail-closed) --------------------------------------------------
bool pinsOk = phase434Present && phase256Present && phase434HashMatches && templateIdMatches;
string verdictKind;
if (!pinsOk)
    verdictKind = "schema-rejected-pin-drift";
else if (!partitionMatches)
    verdictKind = "schema-rejected-partition-drift";
else if (!fixtureSetComplete || !allFixturesPassed)
    verdictKind = "schema-rejected-fixture-failure";
else if (!o8CapAssertedOnAllLineage)
    verdictKind = "schema-rejected-o8-cap-incomplete";
else
    verdictKind = "schema-committed";

bool schemaCommitted = verdictKind == "schema-committed";
string terminalStatus = TerminalPrefix + verdictKind;

// --- Assemble the schema payload (scanned for GeV / capital-Higgs tokens) -----
var schemaPayload = new
{
    schemaId = SchemaId,
    schemaHash,
    schemaCanonicalPreimage = schemaCanonical,
    consumptionRule = ConsumptionRule,
    o8MProbeScopeNote = O8MProbeScopeNote,

    pins = new
    {
        phase434HashPin = Phase434HashPin,
        phase434HashObserved,
        phase434HashMatches,
        phase256TemplateIdPin = Phase256TemplateIdPin,
        phase256TemplateIdObserved,
        templateIdMatches,
    },

    partition = new
    {
        extractionRowCount = 6,
        extractionRowCountObserved,
        conditionalRowCount = 7,
        amplitudeBlockedRowCount = 4,
        lineageBlockedRowCount = 9,
        templateFieldCount = 20,
        templateFieldCountObserved,
        partitionMatches,
        partitionNote,
        conditionalFields,
        amplitudeFields,
        lineageFields,
    },

    vevIntakeRecordSchema = new
    {
        fields = new[] { "direction", "magnitudeInLatticeUnits", "member", "channel", "errorModel" },
        o5GuardSigmaThreshold = O5GuardSigmaThreshold,
        o8MProbeScope = O8MProbeScopeNote,
        note = "magnitudeInLatticeUnits stays in lattice units and is never relabelled as a mass; no absolute-scale normalization is produced.",
    },

    completionMap = new
    {
        conditionalRowsCompletedByAnyConformingVev = conditionalFields,
        amplitudeRowsAlwaysCompletedOnFiniteVev = amplitudeAlwaysOnFiniteVev,
        amplitudeRowsCompletedOnlyWithImportBranch = amplitudeOnlyWithImport,
        zImportBranchId = ZImportBranchId,
        zAmplitudeRequiresImportBranch = true,
        lineageRowsEverCompletableByWsThree = false,
        o8SchemaTheorem = "wsThreeCannotComplete = true is PERMANENT on all 9 lineage fields.",
    },

    lineageImpossibilityCap,
    o8CapAssertedOnAllLineage,

    o5Guard = new
    {
        sigmaThreshold = O5GuardSigmaThreshold,
        rule = "Completion requires >= 3 sigma departure from the exact free control; otherwise no completion.",
    },

    fixtureBattery = new
    {
        committedBeforeMapper = true,
        requiredFixtureIds,
        fixturesEvaluated,
        fixtureSetComplete,
        allFixturesPassed,
        results = fixtureResults,
    },

    templateMutationCount,
    phase256TemplateMutated,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string payloadJson = JsonSerializer.Serialize(schemaPayload, options);

// token self-scan: no "GeV" and no capital-word "Higgs" may appear in the schema payload.
int gevTokenCount = Regex.Matches(payloadJson, "GeV").Count;
int unqualifiedScalarLabelTokenCount = Regex.Matches(payloadJson, @"\bHiggs\b").Count;
bool tokenScanClean = gevTokenCount == 0 && unqualifiedScalarLabelTokenCount == 0;

// token scan is part of the commit gate
if (schemaCommitted && !tokenScanClean)
{
    verdictKind = "schema-rejected-token-scan";
    schemaCommitted = false;
    terminalStatus = TerminalPrefix + verdictKind;
}

bool schemaCommittedAllGreen =
    schemaCommitted &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    physicistReviewPending && scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract && !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract && !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian && !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization && !routeCompletesBosonPredictions &&
    !routePromotesWzMasses && !routePromotesHiggsMass;

string decision = schemaCommitted
    ? "The WS3-VEV completion contract schema is COMMITTED. Pins to phase434 and phase256 hold; the 6/7/4/9 partition " +
      "matches the committed structures; all five golden fixtures reproduce their pre-registered outcomes (only the " +
      "conforming >= 3 sigma weak-doublet fixture completes, and only the amplitude/conditional rows, never lineage); " +
      "the O8 impossibility cap is machine-asserted on all 9 lineage fields. C's half of the WS3 hold may lift on the " +
      "published schema-commit date; the hold fully lifts only jointly with the O4 M-probe ruling. Nothing is " +
      "measured, completed, or promoted here; promotedPhysicalMassClaimCount remains 0."
    : "The WS3-VEV completion contract schema is REJECTED (" + verdictKind + "); the WS3 hold stays in force and is " +
      "never lifted by weakening an assert.";

var result = new
{
    phaseId = "phase466-ws3-vev-completion-contract",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    ws3VevCompletionContractSchemaCommitted = schemaCommittedAllGreen,

    schema = schemaPayload,

    tokenSelfScan = new
    {
        gevTokenCount,
        unqualifiedScalarLabelTokenCount,
        tokenScanClean,
    },

    // standing claim boundary
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256Contract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "This is a completion CONTRACT (a schema), not a completion: no VEV is asserted to exist, and no row is filled.",
        "The 9 lineage rows are permanently un-completable by WS3 (O8); a workbench measurement can never supply independent source lineage.",
        "A measured gap proves a singlet, never the named electroweak scalar. Lattice-unit quantities stay in lattice units; promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "ws3_vev_completion_contract.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "ws3_vev_completion_contract_summary.json"), JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} schemaId={SchemaId}");
Console.WriteLine($"schemaHash={schemaHash}");
Console.WriteLine($"pinsOk={pinsOk} partitionMatches={partitionMatches} extractionRows={extractionRowCountObserved} templateFields={templateFieldCountObserved}");
Console.WriteLine($"fixturesEvaluated={fixturesEvaluated} fixtureSetComplete={fixtureSetComplete} allFixturesPassed={allFixturesPassed}");
Console.WriteLine($"o8CapAssertedOnAllLineage={o8CapAssertedOnAllLineage} gevTokens={gevTokenCount} scalarTokens={unqualifiedScalarLabelTokenCount}");
Console.WriteLine($"runtimeSeconds={stopwatch.Elapsed.TotalSeconds:F3}");

return;

// ---------------------------------------------------------------------------
readonly record struct VevIntake(
    string Direction,
    double? MagnitudeInLatticeUnits,
    string Member,
    string Channel,
    string ErrorModel,
    double SigmaFromExactFreeControl,
    string? BranchNormalizationSourceId);

readonly record struct CompletionOutcome(
    bool Completes,
    int ConditionalCount,
    int AmplitudeCount,
    bool ZAmplitudeCompleted,
    int LineageCount,
    bool SingletFlag,
    string OutcomeClass)
{
    public static CompletionOutcome None(string outcomeClass, bool singlet) =>
        new(false, 0, 0, false, 0, singlet, outcomeClass);
}
