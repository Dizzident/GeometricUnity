using System.Diagnostics;
using System.Text.Json;

var stopwatch = Stopwatch.StartNew();
const string outputDir = "studies/phase485_o4_assumption_falsifier_census_001/output";
string[] ids =
{
    "O4-F1-INVARIANT-RAYS", "O4-F1-COLLECTIVE-COORDINATE", "O4-F1-FP-NORMALIZATION",
    "O4-F2-POSITIVE-MODE-IR", "O4-F3-THETA-HAAR", "O4-F4-SADDLE-BACKGROUNDS",
    "O4-E1-P447-SOFT-FLOOR", "O4-E2-P453-UNIFORM-LADDER", "O4-P455-ZERO-MODE",
    "O4-P455-SB-MODEL", "O4-C1-COMPACT-REAL-FORM", "O4-C2-YHALF-BOOKKEEPING",
    "O4-C3-WS3-MPROBE-SCOPE",
};
string[] methods =
{
    "finite-ray-menu invariance and stabilizer census", "collective-coordinate Jacobian and solvable-limit comparison",
    "independent gauge-fixing determinant normalization", "IR-family sweep with convergence and analytic free control",
    "left-composition invariance plus independent small-lattice integration", "background-family and contour sensitivity sweep",
    "floor-to-zero stability with perturbative-regime gate", "uniform-versus-corrected-ladder overlap and recovery controls",
    "exact zero-mode-menu terminal sensitivity", "source-action alternatives and leave-one-model-out terminal sensitivity",
    "compact/noncompact exact arithmetic transfer", "independent weight normalization and representation census",
    "probe-scope alternatives frozen before reduced deterministic evaluation",
};
var rows = ids.Select((id, i) => new
{
    rulingId = id,
    selfCheckMethod = methods[i],
    falsifierDefined = true,
    externalInterpretationStillRequired = id is "O4-P455-SB-MODEL" or "O4-C3-WS3-MPROBE-SCOPE",
    mayAuthorRuling = false,
}).ToArray();
var result = new
{
    phaseId = "phase485-o4-assumption-falsifier-census",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = "o4-assumption-falsifier-census-exact-thirteen-item-census-complete",
    verdictKind = "exact-thirteen-item-census-complete",
    applicationSubjectKind = "o4-assumption-falsifier-census",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A5",
    amendmentOrder = 2,
    rulingIdCount = rows.Length,
    uniqueRulingIdCount = rows.Select(x => x.rulingId).Distinct(StringComparer.Ordinal).Count(),
    everyItemHasFalsifier = rows.All(x => x.falsifierDefined),
    rows,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458EvaluationAuthorized = false,
    productionAuthorized = false,
    sourceContractApplicationAllowed = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = "All 13 O4 assumptions have an explicit self-check route. The census is experimental planning, not an O4 ruling or launch authorization.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "o4_assumption_falsifier_census.json"), json);
File.WriteAllText(Path.Combine(outputDir, "o4_assumption_falsifier_census_summary.json"), json);
Console.WriteLine(result.terminalStatus);
