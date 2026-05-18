using System.Text.Json;

const string DefaultOutputDir = "studies/phase244_electroweak_identifiability_rank_audit_001/output";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase243Path = "studies/phase243_public_web_source_delta_audit_001/output/public_web_source_delta_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE244_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase243 = JsonDocument.Parse(File.ReadAllText(Phase243Path));

var defensibleValueIds = phase203.RootElement.GetProperty("defensibleValues")
    .EnumerateArray()
    .Select(row => JsonString(row, "observableId"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Select(value => value!)
    .ToArray();

var promotedWzRatioPresent = defensibleValueIds.Contains("physical-w-z-mass-ratio", StringComparer.Ordinal);
var promotedPhotonMasslessnessPresent = defensibleValueIds.Contains("physical-photon-masslessness", StringComparer.Ordinal);
var promotedGluonMasslessnessPresent = defensibleValueIds.Contains("physical-gluon-masslessness", StringComparer.Ordinal);
var anyPromotedAbsoluteMassPresent = defensibleValueIds.Any(id =>
    id is "physical-w-boson-mass-gev" or "physical-z-boson-mass-gev" or "physical-higgs-mass-gev");

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var p224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(p224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(p224Closure, "higgsMassParameterClosure") is true;
var webDeltaPromotableForBosonMasses = JsonBool(phase243.RootElement, "webDeltaPromotableForBosonMasses") is true;

var coordinateRows = new[]
{
    new CoordinateRow(
        "wz-absolute-scale-log",
        "log(v g)",
        "Controls the common absolute W/Z mass scale through log(MW)=log(vg)-log(2).",
        PromotableSourcePresent: wAbsoluteMassParameterClosure),
    new CoordinateRow(
        "weak-mixing-log",
        "log(sqrt(1+(g'/g)^2))",
        "Controls the W/Z ratio through log(MZ/MW).",
        PromotableSourcePresent: promotedWzRatioPresent),
    new CoordinateRow(
        "higgs-absolute-scale-log",
        "log(v sqrt(lambda))",
        "Controls the Higgs mass scale through log(MH)=log(v sqrt(lambda))+log(sqrt(2)).",
        PromotableSourcePresent: higgsMassParameterClosure),
};

var massJacobian = new[]
{
    new[] { 1.0, 0.0, 0.0 },
    new[] { 1.0, 1.0, 0.0 },
    new[] { 0.0, 0.0, 1.0 },
};
var currentConstraintMatrix = new[]
{
    new[] { 0.0, 1.0, 0.0 },
};
var requiredFullPredictionRank = Rank(massJacobian);
var currentPromotedConstraintRank = Rank(currentConstraintMatrix);
var remainingNullity = coordinateRows.Length - currentPromotedConstraintRank;

var nullDirections = new[]
{
    new NullDirection(
        "common-wz-scale-direction",
        new[] { 1.0, 0.0, 0.0 },
        "Changes W and Z absolute masses together while preserving the promoted W/Z ratio and leaving Higgs unchanged.",
        "Must be fixed by a GU-derived W/Z absolute scale source, e.g. a promotable source for log(vg) or equivalent v and g rows."),
    new NullDirection(
        "higgs-scale-direction",
        new[] { 0.0, 0.0, 1.0 },
        "Changes the Higgs mass while preserving the promoted W/Z ratio and W/Z absolute-scale diagnostics.",
        "Must be fixed by a solved GU scalar source for v sqrt(lambda), or equivalent VEV plus scalar self-coupling rows."),
};

var sourceDeficitRows = new[]
{
    new SourceDeficitRow(
        "wz-absolute-scale-source",
        Filled: wAbsoluteMassParameterClosure,
        "The current promoted W/Z ratio fixes only weak mixing. P224 and P213 show no promotable source for the absolute W/Z scale coordinate."),
    new SourceDeficitRow(
        "higgs-absolute-scale-source",
        Filled: higgsMassParameterClosure,
        "P224 and P213 show no promotable scalar source for the Higgs absolute scale coordinate."),
};

var minimumAdditionalIndependentSourceConstraints = sourceDeficitRows.Count(row => !row.Filled);
var rankAuditPromotableForBosonMasses =
    currentPromotedConstraintRank == requiredFullPredictionRank
    && minimumAdditionalIndependentSourceConstraints == 0
    && !anyPromotedAbsoluteMassPresent;

var checks = new[]
{
    new Check("mass-map-rank-recorded", requiredFullPredictionRank == 3, $"requiredFullPredictionRank={requiredFullPredictionRank}"),
    new Check("current-promoted-constraint-rank-is-one", promotedWzRatioPresent && currentPromotedConstraintRank == 1, $"promotedWzRatioPresent={promotedWzRatioPresent}; currentPromotedConstraintRank={currentPromotedConstraintRank}"),
    new Check("massless-observables-do-not-fix-ew-scale", promotedPhotonMasslessnessPresent && promotedGluonMasslessnessPresent && !anyPromotedAbsoluteMassPresent, $"promotedPhotonMasslessnessPresent={promotedPhotonMasslessnessPresent}; promotedGluonMasslessnessPresent={promotedGluonMasslessnessPresent}; anyPromotedAbsoluteMassPresent={anyPromotedAbsoluteMassPresent}"),
    new Check("remaining-nullity-two", remainingNullity == 2 && nullDirections.Length == 2, $"remainingNullity={remainingNullity}; nullDirectionCount={nullDirections.Length}"),
    new Check("wz-and-higgs-absolute-closures-false", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}"),
    new Check("source-lineage-blockers-preserved", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("public-web-delta-not-promotable", !webDeltaPromotableForBosonMasses, $"webDeltaPromotableForBosonMasses={webDeltaPromotableForBosonMasses}"),
    new Check("two-additional-source-constraints-required", minimumAdditionalIndependentSourceConstraints == 2, $"minimumAdditionalIndependentSourceConstraints={minimumAdditionalIndependentSourceConstraints}"),
};

var electroweakIdentifiabilityRankAuditPassed = checks.All(check => check.Passed)
    && !rankAuditPromotableForBosonMasses;
var terminalStatus = electroweakIdentifiabilityRankAuditPassed
    ? "electroweak-identifiability-rank-audit-underconstrained"
    : "electroweak-identifiability-rank-audit-review-required";

var result = new
{
    phaseId = "phase244-electroweak-identifiability-rank-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    electroweakIdentifiabilityRankAuditPassed,
    rankAuditPromotableForBosonMasses,
    promotedWzRatioPresent,
    promotedPhotonMasslessnessPresent,
    promotedGluonMasslessnessPresent,
    anyPromotedAbsoluteMassPresent,
    requiredFullPredictionRank,
    currentPromotedConstraintRank,
    remainingNullity,
    minimumAdditionalIndependentSourceConstraints,
    objective = "Determine whether current promoted boson information mathematically identifies W/Z/H absolute masses, or whether the electroweak mass map remains underconstrained.",
    externalPhysicsContext = new
    {
        source = "Particle Data Group 2025 Review, Electroweak Model and Constraints on New Physics",
        url = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        massCoordinateModel = new[]
        {
            "log(MW)=log(v g)-log(2)",
            "log(MZ)=log(v g)+log(sqrt(1+(g'/g)^2))-log(2)",
            "log(MH)=log(v sqrt(lambda))+log(sqrt(2))",
        },
        interpretation = "The W/Z ratio fixes only the weak-mixing coordinate. Absolute W/Z and Higgs masses require independent source constraints on the W/Z absolute scale and Higgs scalar scale.",
    },
    coordinateRows,
    massJacobian,
    currentConstraintMatrix,
    nullDirections,
    sourceDeficitRows,
    checks,
    decision = electroweakIdentifiabilityRankAuditPassed
        ? "Do not promote W, Z, or Higgs absolute masses from the current promoted values. The promoted W/Z ratio has rank one in the electroweak mass-coordinate model and leaves two continuous null directions: common W/Z scale and Higgs scalar scale."
        : "Review rank audit inputs before relying on this identifiability result.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU source row fixing the W/Z absolute-scale coordinate log(vg), or equivalent independent GU rows for v and g.",
        "A target-independent GU scalar-source row fixing the Higgs absolute-scale coordinate log(v sqrt(lambda)), or equivalent independent GU rows for v and lambda.",
        "After those independent source constraints exist, rerun Phase201/P209/P210/P213 and the full boson generator.",
    },
    sourceEvidence = new
    {
        phase203Path = Phase203Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase243Path = Phase243Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_identifiability_rank_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_identifiability_rank_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.electroweakIdentifiabilityRankAuditPassed,
        result.rankAuditPromotableForBosonMasses,
        result.promotedWzRatioPresent,
        result.promotedPhotonMasslessnessPresent,
        result.promotedGluonMasslessnessPresent,
        result.anyPromotedAbsoluteMassPresent,
        result.requiredFullPredictionRank,
        result.currentPromotedConstraintRank,
        result.remainingNullity,
        result.minimumAdditionalIndependentSourceConstraints,
        result.externalPhysicsContext,
        result.coordinateRows,
        result.nullDirections,
        result.sourceDeficitRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electroweakIdentifiabilityRankAuditPassed={electroweakIdentifiabilityRankAuditPassed}");
Console.WriteLine($"currentPromotedConstraintRank={currentPromotedConstraintRank}");
Console.WriteLine($"remainingNullity={remainingNullity}");
Console.WriteLine($"minimumAdditionalIndependentSourceConstraints={minimumAdditionalIndependentSourceConstraints}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int Rank(double[][] matrix)
{
    var rows = matrix.Length;
    var cols = matrix.Max(row => row.Length);
    var work = matrix.Select(row => row.Concat(Enumerable.Repeat(0.0, cols - row.Length)).ToArray()).ToArray();
    var rank = 0;
    const double tolerance = 1e-12;

    for (var col = 0; col < cols && rank < rows; col++)
    {
        var pivot = rank;
        for (var row = rank + 1; row < rows; row++)
        {
            if (Math.Abs(work[row][col]) > Math.Abs(work[pivot][col]))
            {
                pivot = row;
            }
        }

        if (Math.Abs(work[pivot][col]) <= tolerance)
        {
            continue;
        }

        (work[rank], work[pivot]) = (work[pivot], work[rank]);
        var pivotValue = work[rank][col];
        for (var c = col; c < cols; c++)
        {
            work[rank][c] /= pivotValue;
        }

        for (var row = 0; row < rows; row++)
        {
            if (row == rank)
            {
                continue;
            }

            var factor = work[row][col];
            for (var c = col; c < cols; c++)
            {
                work[row][c] -= factor * work[rank][c];
            }
        }

        rank++;
    }

    return rank;
}

sealed record CoordinateRow(string CoordinateId, string CoordinateExpression, string Role, bool PromotableSourcePresent);
sealed record NullDirection(string DirectionId, double[] Vector, string Effect, string RequiredSource);
sealed record SourceDeficitRow(string SourceId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
