using System.Text.Json;

const string DefaultOutputDir = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase112Path = "studies/phase112_scalar_sector_relation_revision_attempt_001/output/scalar_sector_relation_revision_attempt.json";
const string Phase69Path = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase71Path = "studies/phase71_shared_wz_scale_bridge_001/shared_wz_scale_bridge.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE168_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase167 = JsonDocument.Parse(File.ReadAllText(Phase167Path));
using var phase112 = JsonDocument.Parse(File.ReadAllText(Phase112Path));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase71 = JsonDocument.Parse(File.ReadAllText(Phase71Path));

var bestAttempt = phase167.RootElement.GetProperty("bestAttempt");
var rows = bestAttempt.GetProperty("predictionAttempts").EnumerateArray()
    .Select(row => new BasePrediction(
        RequiredString(row, "particleId"),
        RequiredString(row, "observableId"),
        JsonDouble(row, "predictedValue") ?? 0.0,
        JsonDouble(row, "targetValue") ?? 0.0,
        JsonDouble(row, "targetUncertainty") ?? 1.0,
        JsonDouble(row, "scaledRawToTargetRatio") ?? 0.0))
    .ToArray();

double diagnosticCommonFactor = rows
    .Select(row => row.TargetValue / row.PredictedValue)
    .Aggregate(1.0, (product, value) => product * value);
diagnosticCommonFactor = Math.Pow(diagnosticCommonFactor, 1.0 / rows.Length);

var scalarCandidates = new[]
{
    new ScalarCandidate(
        "current-phase69-phase70-scalar-relation",
        1.0,
        true,
        true,
        "Existing Phase69/70 scalar-sector relation; no revision factor is present."),
    new ScalarCandidate(
        "phase112-target-implied-diagnostic-factor",
        JsonDouble(phase112.RootElement.GetProperty("diagnosticOnlyTargetImpliedRevision"), "meanRequiredScaleFactor") ?? 1.0,
        false,
        false,
        "Phase112 records this as target-implied diagnostic-only, not calibration evidence."),
    new ScalarCandidate(
        "p167-target-implied-common-factor",
        diagnosticCommonFactor,
        false,
        false,
        "Computed from P167 W/Z target residuals; useful only as a diagnostic of the remaining scalar offset."),
};

var attempts = scalarCandidates
    .Select(candidate => Evaluate(candidate, rows))
    .OrderBy(attempt => attempt.Gates.PromotionAllowed ? 0 : 1)
    .ThenBy(attempt => attempt.Gates.MaxSigmaResidual)
    .ToArray();

var best = attempts.First();
bool anyPromotable = attempts.Any(attempt => attempt.Gates.PromotionAllowed);
bool independentRevisionEvidencePresent = JsonBool(phase112.RootElement, "independentRevisionEvidencePresent") is true;
string terminalStatus = anyPromotable
    ? "source-shape-scalar-relation-closure-promotable"
    : independentRevisionEvidencePresent
        ? "source-shape-scalar-relation-closure-review-required"
        : "source-shape-scalar-relation-closure-blocked-no-independent-revision";

var result = new
{
    phaseId = "phase168-source-shape-scalar-relation-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    p167BestCandidateId = RequiredString(bestAttempt, "candidateId"),
    diagnosticCommonFactor,
    independentRevisionEvidencePresent,
    scalarCandidates,
    bestAttempt = best,
    attempts,
    diagnosis = anyPromotable
        ? "A scalar relation closure candidate passes all gates."
        : "The only scalar factor that improves P167 is target-implied diagnostic evidence; existing scalar-sector artifacts do not provide an independent derivation-backed revision.",
    nextWork = anyPromotable
        ? "wire the promotable scalar-relation closure into Phase148-151 promotion flow"
        : "derive an independent scalar-sector relation revision or analytic source-shape normalization law; do not use target-implied scalar factors as predictions",
    sourceEvidence = new
    {
        phase167Path = Phase167Path,
        phase112Path = Phase112Path,
        phase69Path = Phase69Path,
        phase70Path = Phase70Path,
        phase71Path = Phase71Path,
        phase69Status = JsonString(phase69.RootElement, "terminalStatus"),
        phase70Status = JsonString(phase70.RootElement, "terminalStatus"),
        phase71Status = JsonString(phase71.RootElement, "terminalStatus"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_shape_scalar_relation_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_shape_scalar_relation_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.p167BestCandidateId,
        result.diagnosticCommonFactor,
        result.bestAttempt,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"diagnosticCommonFactor={diagnosticCommonFactor:R}");
Console.WriteLine($"bestCandidateId={best.CandidateId}");
Console.WriteLine($"bestMaxSigmaResidual={best.Gates.MaxSigmaResidual:R}");
Console.WriteLine($"promotionAllowed={best.Gates.PromotionAllowed}");

static ScalarAttempt Evaluate(ScalarCandidate candidate, IReadOnlyList<BasePrediction> rows)
{
    const double sigmaThreshold = 5.0;
    var attempts = rows
        .Select(row =>
        {
            double predicted = row.PredictedValue * candidate.Factor;
            double sigma = Math.Abs(predicted - row.TargetValue) / row.TargetUncertainty;
            return new ScalarPredictionAttempt(
                row.ParticleId,
                row.ObservableId,
                predicted,
                row.TargetValue,
                row.TargetUncertainty,
                sigma,
                sigma <= sigmaThreshold);
        })
        .ToArray();
    bool targetComparisonPassed = attempts.All(attempt => attempt.Passed);
    var gates = new ScalarGates(
        candidate.TargetIndependent,
        candidate.DerivationBacked,
        targetComparisonPassed,
        attempts.Max(attempt => attempt.SigmaResidual),
        candidate.TargetIndependent && candidate.DerivationBacked && targetComparisonPassed);
    return new ScalarAttempt(candidate.CandidateId, candidate.Factor, candidate.Rationale, attempts, gates);
}

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record BasePrediction(
    string ParticleId,
    string ObservableId,
    double PredictedValue,
    double TargetValue,
    double TargetUncertainty,
    double ScaledRawToTargetRatio);

sealed record ScalarCandidate(
    string CandidateId,
    double Factor,
    bool TargetIndependent,
    bool DerivationBacked,
    string Rationale);

sealed record ScalarAttempt(
    string CandidateId,
    double Factor,
    string Rationale,
    IReadOnlyList<ScalarPredictionAttempt> PredictionAttempts,
    ScalarGates Gates);

sealed record ScalarPredictionAttempt(
    string ParticleId,
    string ObservableId,
    double PredictedValue,
    double TargetValue,
    double TargetUncertainty,
    double SigmaResidual,
    bool Passed);

sealed record ScalarGates(
    bool TargetIndependent,
    bool DerivationBacked,
    bool TargetComparisonPassed,
    double MaxSigmaResidual,
    bool PromotionAllowed);
