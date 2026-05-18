using System.Text.Json;

const string DefaultOutputDir = "studies/phase194_draft_boson_source_evidence_audit_001/output";
const string DraftPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase193Path = "studies/phase193_boson_prediction_completion_audit_001/output/boson_prediction_completion_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE194_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var lines = File.ReadAllLines(DraftPath);
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase193 = JsonDocument.Parse(File.ReadAllText(Phase193Path));

var findings = new[]
{
    EvidenceFinding.FromLine(
        "draft-mass-yukawa-open",
        DraftPath,
        4772,
        lines,
        "Draft says Yukawa-like mass-generation is a target and follow-on chapter, not a completed geometric derivation.",
        "Supports the Higgs/scalar-source blocker."),
    EvidenceFinding.FromLine(
        "draft-higgs-sector-conjectural",
        DraftPath,
        5287,
        lines,
        "Draft classifies the Higgs-like sector as conjectural/open rather than a uniquely isolated sector.",
        "Supports rejection of a direct Higgs mass prediction from current artifacts."),
    EvidenceFinding.FromLine(
        "draft-higgs-approximate-prediction",
        DraftPath,
        5719,
        lines,
        "Draft classifies Higgs-like reorganization as approximate and requires explicit scalar spectrum/potential/vacuum/coupling extraction.",
        "Matches the missing P189 scalar source/operator and identity sidecars."),
    EvidenceFinding.FromLine(
        "draft-fermionic-action-placeholder",
        DraftPath,
        12364,
        lines,
        "Draft supplies a typed fermionic action template, not a unique completed operator branch.",
        "Explains why a W/Z bridge-source theorem cannot be read directly from the draft."),
    EvidenceFinding.FromLine(
        "draft-vo7-mixed-linearization-obligation",
        DraftPath,
        12398,
        lines,
        "Draft records mixed boson-fermion linearization as a proof obligation.",
        "Directly matches the P191 missing theorem and particle-specific source split."),
};

var draftProvidesDirectWzLaw = false;
var draftProvidesSolvedHiggsSource = false;
var phase191CanComplete = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true;
var allSuccessCriteriaMet = JsonBool(phase193.RootElement, "allSuccessCriteriaMet") is true;

var terminalStatus = draftProvidesDirectWzLaw || draftProvidesSolvedHiggsSource
    ? "draft-boson-source-evidence-audit-actionable-source-found"
    : "draft-boson-source-evidence-audit-no-completion-source-found";

var result = new
{
    phaseId = "phase194-draft-boson-source-evidence-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    draftPath = DraftPath,
    draftProvidesDirectWzLaw,
    draftProvidesSolvedHiggsSource,
    phase191CanComplete,
    phase193AllSuccessCriteriaMet = allSuccessCriteriaMet,
    findings,
    conclusion = "The latest completion draft does not supply the missing W/Z direct bridge-source theorem or solved Higgs scalar source. It explicitly frames the relevant boson-fermion, Yukawa, Higgs, and mixed-linearization material as open, approximate, conjectural, or proof-obligation material.",
    implication = "Do not promote W/Z absolute masses or Higgs mass from draft text alone. Current defensible boson values remain limited to the W/Z mass ratio plus photon/gluon protected masslessness indicators.",
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "draft_boson_source_evidence_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "draft_boson_source_evidence_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.draftProvidesDirectWzLaw,
        result.draftProvidesSolvedHiggsSource,
        result.phase191CanComplete,
        result.phase193AllSuccessCriteriaMet,
        result.findings,
        result.conclusion,
        result.implication,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"draftProvidesDirectWzLaw={draftProvidesDirectWzLaw}");
Console.WriteLine($"draftProvidesSolvedHiggsSource={draftProvidesSolvedHiggsSource}");
Console.WriteLine($"findingCount={findings.Length}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record EvidenceFinding(
    string FindingId,
    string EvidencePath,
    int LineNumber,
    string EvidenceText,
    string Interpretation,
    string PredictionImpact)
{
    public static EvidenceFinding FromLine(
        string findingId,
        string path,
        int oneBasedLine,
        string[] lines,
        string interpretation,
        string predictionImpact)
    {
        var text = oneBasedLine > 0 && oneBasedLine <= lines.Length ? lines[oneBasedLine - 1].Trim() : "";
        return new EvidenceFinding(findingId, path, oneBasedLine, text, interpretation, predictionImpact);
    }
}
