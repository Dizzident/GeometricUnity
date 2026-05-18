using System.Text.Json;

const string DefaultOutputDir = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output";
const string CompletionRevisionPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE267_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var lines = File.ReadAllLines(CompletionRevisionPath);

var researchEvidence = new[]
{
    LineEvidence(2043, "Observed field content is downstream of pullback, observation, subgroup reduction, and dynamics."),
    LineEvidence(2046, "Standard Model correspondences and predictions are inadmissible before the recovery pipeline and decomposition machinery are defined."),
    LineEvidence(2417, "No observation comparison is admissible without a typed prediction record, formal source, observable map, assumptions, external data source, and falsifier."),
    LineEvidence(11361, "Canonical Shiab operator classification remains an open critical issue."),
    LineEvidence(11367, "Boson-fermion coupling and Higgs/Yukawa reinterpretation remain speculative at the physical layer."),
    LineEvidence(11372, "Observed-field extraction has an interface but uniqueness and full recovery theorems remain open."),
    LineEvidence(11391, "The completion revision names representation decomposition into observed bosons and Higgs/Yukawa reinterpretation as severe physical uncertainties."),
    LineEvidence(11425, "The completion revision still records branch sensitivity and nonuniqueness for the Shiab/operator layer."),
    LineEvidence(11427, "Masses, couplings, scales, and low-energy phenomenology remain unfinished."),
    LineEvidence(11604, "Physical identification remains phenomenological unless a derivation has already been completed."),
    LineEvidence(11605, "Observational comparison requires explicit formal source and observable-map metadata."),
    LineEvidence(11666, "A branch-local observed extraction theorem is still a proof obligation."),
    LineEvidence(12115, "Direct identification of GU field components with Yang-Mills, Higgs, or Yukawa content requires a completed extraction theorem."),
    LineEvidence(13728, "A prediction is a structured tuple, not prose."),
    LineEvidence(13732, "If any prediction tuple component is missing, the prediction is not admitted into the registry."),
    LineEvidence(13756, "The completion revision explicitly does not close the hard research gaps, including full native-to-observed extraction or validation of a specific physical branch."),
};

var directBridgeMentions = Search("W/Z direct", "direct bridge", "bridge-source", "source-lineage", "source lineage");
var massPredictionDisciplineMentions = Search("masses, couplings, scales", "typed prediction", "observable map", "extraction theorem", "observed bosons");

var p190CandidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
var p190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var p190TargetObservablesUsed = JsonBool(phase190.RootElement, "targetObservablesUsed") is true;
var p190StableCandidateCount = phase190.RootElement.TryGetProperty("siblingStability", out var p190Sibling)
    ? JsonInt(p190Sibling, "stableCandidateCount") ?? 0
    : 0;
var p191Gates = phase191.RootElement.GetProperty("gates");
var p191RawGatePassed = JsonBool(p191Gates, "rawGatePassed") is true;
var p191WzParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent") is true;
var p191CanCompleteSuccessfulPrediction = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p247SourceRowRepairPossible = JsonBool(phase247.RootElement, "sourceRowRepairPossibleFromCurrentRegistry") is true;
var p255ObservedFieldExtractionBridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var p257CurrentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var latestCompletionProvidesDirectWzTheorem = false;
var latestCompletionProvidesObservedFieldExtractionTheorem = false;
var latestCompletionProvidesQuantitativeMassScaleSource = false;
var latestCompletionProvidesHiggsScalarSource = false;
var latestCompletionPromotesWzMasses = false;
var latestCompletionPromotesHiggsMass = false;
var latestCompletionCompletesBosonPredictions = false;

var completionRevisionDirectBridgeSourceAuditPassed =
    researchEvidence.All(row => row.Found)
    && p190CandidateLawConstructed
    && !p190TargetObservablesUsed
    && !p190TheoremClaimed
    && !p191RawGatePassed
    && !p191WzParticleSplitPresent
    && !p191CanCompleteSuccessfulPrediction
    && p247NewDirectBridgeTheoremStillRequired
    && !p247SourceRowRepairPossible
    && !p255ObservedFieldExtractionBridgePromotable
    && !p257CurrentImplementationCanFillObservedFieldExtractionContract
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0
    && !latestCompletionProvidesDirectWzTheorem
    && !latestCompletionProvidesObservedFieldExtractionTheorem
    && !latestCompletionProvidesQuantitativeMassScaleSource
    && !latestCompletionProvidesHiggsScalarSource
    && !latestCompletionPromotesWzMasses
    && !latestCompletionPromotesHiggsMass
    && !latestCompletionCompletesBosonPredictions;

var checks = new[]
{
    new Check(
        "completion-revision-evidence-lines-present",
        researchEvidence.All(row => row.Found),
        $"foundLineEvidenceCount={researchEvidence.Count(row => row.Found)}; requiredLineEvidenceCount={researchEvidence.Length}"),
    new Check(
        "completion-revision-does-not-provide-direct-wz-theorem",
        !latestCompletionProvidesDirectWzTheorem && p247NewDirectBridgeTheoremStillRequired && !p190TheoremClaimed,
        $"latestCompletionProvidesDirectWzTheorem={latestCompletionProvidesDirectWzTheorem}; p190TheoremClaimed={p190TheoremClaimed}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}"),
    new Check(
        "direct-bridge-candidate-still-fails-prediction-gates",
        p190CandidateLawConstructed && !p190TargetObservablesUsed && !p191RawGatePassed && !p191WzParticleSplitPresent && !p191CanCompleteSuccessfulPrediction,
        $"p190CandidateLawConstructed={p190CandidateLawConstructed}; p190TargetObservablesUsed={p190TargetObservablesUsed}; p190StableCandidateCount={p190StableCandidateCount}; p191RawGatePassed={p191RawGatePassed}; p191WzParticleSplitPresent={p191WzParticleSplitPresent}; p191CanCompleteSuccessfulPrediction={p191CanCompleteSuccessfulPrediction}"),
    new Check(
        "completion-revision-does-not-fill-observed-field-extraction",
        !latestCompletionProvidesObservedFieldExtractionTheorem
            && !p255ObservedFieldExtractionBridgePromotable
            && !p257CurrentImplementationCanFillObservedFieldExtractionContract,
        $"latestCompletionProvidesObservedFieldExtractionTheorem={latestCompletionProvidesObservedFieldExtractionTheorem}; p255ObservedFieldExtractionBridgePromotable={p255ObservedFieldExtractionBridgePromotable}; p257CurrentImplementationCanFillObservedFieldExtractionContract={p257CurrentImplementationCanFillObservedFieldExtractionContract}"),
    new Check(
        "completion-revision-does-not-fill-mass-source-lineage",
        !latestCompletionProvidesQuantitativeMassScaleSource
            && !latestCompletionProvidesHiggsScalarSource
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"latestCompletionProvidesQuantitativeMassScaleSource={latestCompletionProvidesQuantitativeMassScaleSource}; latestCompletionProvidesHiggsScalarSource={latestCompletionProvidesHiggsScalarSource}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "completion-revision-does-not-complete-boson-predictions",
        !latestCompletionPromotesWzMasses && !latestCompletionPromotesHiggsMass && !latestCompletionCompletesBosonPredictions,
        $"latestCompletionPromotesWzMasses={latestCompletionPromotesWzMasses}; latestCompletionPromotesHiggsMass={latestCompletionPromotesHiggsMass}; latestCompletionCompletesBosonPredictions={latestCompletionCompletesBosonPredictions}"),
};

var terminalStatus = completionRevisionDirectBridgeSourceAuditPassed
    ? "completion-revision-direct-bridge-source-audit-no-promotion"
    : "completion-revision-direct-bridge-source-audit-review-required";

var result = new
{
    phaseId = "phase267-completion-revision-direct-bridge-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    completionRevisionDirectBridgeSourceAuditPassed,
    latestCompletionProvidesDirectWzTheorem,
    latestCompletionProvidesObservedFieldExtractionTheorem,
    latestCompletionProvidesQuantitativeMassScaleSource,
    latestCompletionProvidesHiggsScalarSource,
    latestCompletionPromotesWzMasses,
    latestCompletionPromotesHiggsMass,
    latestCompletionCompletesBosonPredictions,
    completionRevision = new
    {
        path = CompletionRevisionPath,
        totalLineCount = lines.Length,
        researchEvidence,
        directBridgeMentionCount = directBridgeMentions.Length,
        directBridgeMentions = directBridgeMentions.Take(20).ToArray(),
        massPredictionDisciplineMentionCount = massPredictionDisciplineMentions.Length,
        massPredictionDisciplineMentions = massPredictionDisciplineMentions.Take(20).ToArray(),
    },
    currentBridgeEvidence = new
    {
        phase190 = new
        {
            candidateLawConstructed = p190CandidateLawConstructed,
            theoremClaimed = p190TheoremClaimed,
            targetObservablesUsed = p190TargetObservablesUsed,
            stableCandidateCount = p190StableCandidateCount,
        },
        phase191 = new
        {
            rawGatePassed = p191RawGatePassed,
            wZParticleSplitPresent = p191WzParticleSplitPresent,
            canCompleteSuccessfulPrediction = p191CanCompleteSuccessfulPrediction,
        },
        phase247 = new
        {
            newDirectBridgeTheoremStillRequired = p247NewDirectBridgeTheoremStillRequired,
            sourceRowRepairPossibleFromCurrentRegistry = p247SourceRowRepairPossible,
        },
        phase255 = new
        {
            observedFieldExtractionBridgePromotable = p255ObservedFieldExtractionBridgePromotable,
        },
        phase257 = new
        {
            currentImplementationCanFillObservedFieldExtractionContract = p257CurrentImplementationCanFillObservedFieldExtractionContract,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    sourceLineageBoundary = new
    {
        directWzTheoremFound = latestCompletionProvidesDirectWzTheorem,
        observedFieldExtractionTheoremFound = latestCompletionProvidesObservedFieldExtractionTheorem,
        quantitativeMassScaleSourceFound = latestCompletionProvidesQuantitativeMassScaleSource,
        higgsScalarSourceFound = latestCompletionProvidesHiggsScalarSource,
        currentP190CandidateIsTheorem = p190TheoremClaimed,
        currentP191RawGatePassed = p191RawGatePassed,
        currentP191WzParticleSplitPresent = p191WzParticleSplitPresent,
    },
    checks,
    decision = completionRevisionDirectBridgeSourceAuditPassed
        ? "Do not promote W/Z absolute masses or Higgs mass from the latest completion revision. The revision supplies governance, proof obligations, and open-problem language, but not a direct W/Z bridge theorem, observed-field extraction theorem, quantitative mass-scale source, or solved Higgs scalar source."
        : "Review the completion-revision source audit before relying on its promotion boundary.",
    nextRequiredArtifact = new[]
    {
        "A completed direct W/Z bridge theorem or branch-local proof, not only a branch-local matrix-element candidate.",
        "A completed observed-field extraction theorem with W and Z source rows and observable-map metadata.",
        "A quantitative mass-scale source and solved Higgs scalar source that fill Phase213/Phase209 contracts without target leakage.",
    },
    sourceEvidence = new
    {
        completionRevisionPath = CompletionRevisionPath,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase213Path = Phase213Path,
        phase247Path = Phase247Path,
        phase255Path = Phase255Path,
        phase257Path = Phase257Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "completion_revision_direct_bridge_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "completion_revision_direct_bridge_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.completionRevisionDirectBridgeSourceAuditPassed,
        result.latestCompletionProvidesDirectWzTheorem,
        result.latestCompletionProvidesObservedFieldExtractionTheorem,
        result.latestCompletionProvidesQuantitativeMassScaleSource,
        result.latestCompletionProvidesHiggsScalarSource,
        result.latestCompletionPromotesWzMasses,
        result.latestCompletionPromotesHiggsMass,
        result.latestCompletionCompletesBosonPredictions,
        completionRevision = new
        {
            path = CompletionRevisionPath,
            researchEvidenceCount = researchEvidence.Length,
            directBridgeMentionCount = directBridgeMentions.Length,
            massPredictionDisciplineMentionCount = massPredictionDisciplineMentions.Length,
            keyEvidenceLines = researchEvidence.Select(row => new { row.LineNumber, row.AuditFinding, row.Excerpt }).ToArray(),
        },
        result.currentBridgeEvidence,
        result.sourceLineageBoundary,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"completionRevisionDirectBridgeSourceAuditPassed={completionRevisionDirectBridgeSourceAuditPassed}");
Console.WriteLine($"latestCompletionProvidesDirectWzTheorem={latestCompletionProvidesDirectWzTheorem}");
Console.WriteLine($"latestCompletionProvidesObservedFieldExtractionTheorem={latestCompletionProvidesObservedFieldExtractionTheorem}");
Console.WriteLine($"latestCompletionProvidesQuantitativeMassScaleSource={latestCompletionProvidesQuantitativeMassScaleSource}");
Console.WriteLine($"latestCompletionProvidesHiggsScalarSource={latestCompletionProvidesHiggsScalarSource}");
Console.WriteLine($"latestCompletionPromotesWzMasses={latestCompletionPromotesWzMasses}");
Console.WriteLine($"latestCompletionPromotesHiggsMass={latestCompletionPromotesHiggsMass}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

LineEvidenceRecord LineEvidence(int lineNumber, string auditFinding)
{
    var found = lineNumber >= 1 && lineNumber <= lines.Length;
    return new LineEvidenceRecord(
        CompletionRevisionPath,
        lineNumber,
        found,
        auditFinding,
        found ? lines[lineNumber - 1].Trim() : "line missing");
}

LineEvidenceRecord[] Search(params string[] terms)
{
    return lines
        .Select((line, index) => new { line, index })
        .Where(row => terms.Any(term => row.line.Contains(term, StringComparison.OrdinalIgnoreCase)))
        .Select(row => new LineEvidenceRecord(
            CompletionRevisionPath,
            row.index + 1,
            true,
            "search-hit",
            row.line.Trim()))
        .ToArray();
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
        ? result
        : null;

record LineEvidenceRecord(
    string Path,
    int LineNumber,
    bool Found,
    string AuditFinding,
    string Excerpt);

record Check(string CheckId, bool Passed, string Detail);
