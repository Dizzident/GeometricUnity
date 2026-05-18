using System.Text.Json;

const string DefaultOutputDir = "studies/phase162_phase_sensitive_wz_bridge_projection_attempt_001/output";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase160Path = "studies/phase160_phase_sensitive_transition_rule_materialization_001/output/phase_sensitive_transition_rule_materialization.json";
const string Phase159Path = "studies/phase159_mode_branch_projector_derivation_experiment_001/output/mode_branch_projector_derivation_experiment.json";
const string Candidate8MatrixPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations/variation-bg-phase12-bg-a-20260315212202-candidate-8.matrix.json";

const double RawGate = 0.95;

var outputDir = Environment.GetEnvironmentVariable("PHASE162_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var modesDoc = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase160 = JsonDocument.Parse(File.ReadAllText(Phase160Path));
using var phase159 = JsonDocument.Parse(File.ReadAllText(Phase159Path));
using var candidate8Matrix = JsonDocument.Parse(File.ReadAllText(Candidate8MatrixPath));

bool rulePromotable = JsonBool(phase160.RootElement, "transitionRulePromotable") is true;
var directedTransitions = phase160.RootElement
    .GetProperty("transitionRule")
    .GetProperty("directedTransitions")
    .EnumerateArray()
    .Select(transition => transition.Clone())
    .ToArray();
if (directedTransitions.Length == 0)
    throw new InvalidOperationException("P160 transition rule has no directed transitions.");

var sourceByModeId = phase159.RootElement.GetProperty("projectorRows")
    .EnumerateArray()
    .ToDictionary(
        row => RequiredString(row, "modeId"),
        row => RequiredString(row, "sourceCanonicalFermionModeId"),
        StringComparer.Ordinal);
var modes = modesDoc.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Select((mode, index) =>
    {
        string modeId = RequiredString(mode, "modeId");
        return new ModeRecord(
            index,
            modeId,
            sourceByModeId.GetValueOrDefault(modeId),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray());
    })
    .Where(mode => mode.SourceCanonicalFermionModeId is not null)
    .ToArray();
var modeBySource = modes.ToDictionary(mode => mode.SourceCanonicalFermionModeId!, StringComparer.Ordinal);
var matrixRe = LoadMatrix(candidate8Matrix.RootElement.GetProperty("real"));
var matrixIm = LoadMatrix(candidate8Matrix.RootElement.GetProperty("imag"));

double targetRaw = phase110.RootElement
    .GetProperty("repairTarget")
    .GetProperty("targetImpliedRawMatrixElementMagnitude")
    .GetDouble();

var transitionAttempts = directedTransitions.Select(transition =>
{
    string fromSource = RequiredString(transition, "fromSourceCanonicalFermionModeId");
    string toSource = RequiredString(transition, "toSourceCanonicalFermionModeId");
    var fromMode = modeBySource[fromSource];
    var toMode = modeBySource[toSource];
    var matrixElement = MatrixElement(matrixRe, matrixIm, fromMode.Coefficients, toMode.Coefficients);
    double magnitude = Magnitude(matrixElement.Real, matrixElement.Imaginary);
    double rawToTargetRatio = magnitude / targetRaw;
    return new
    {
        direction = RequiredString(transition, "direction"),
        fromFamilyId = RequiredString(transition, "fromFamilyId"),
        toFamilyId = RequiredString(transition, "toFamilyId"),
        fromSourceCanonicalFermionModeId = fromSource,
        toSourceCanonicalFermionModeId = toSource,
        fromRepairedModeId = fromMode.ModeId,
        toRepairedModeId = toMode.ModeId,
        real = matrixElement.Real,
        imaginary = matrixElement.Imaginary,
        magnitude,
        targetRaw,
        rawToTargetRatio,
        rawGatePassed = rawToTargetRatio >= RawGate,
    };
}).ToArray();

bool anyRawGatePassed = transitionAttempts.Any(attempt => attempt.rawGatePassed);
bool projectionCandidateReady = rulePromotable && anyRawGatePassed;
string terminalStatus = projectionCandidateReady
    ? "phase-sensitive-wz-bridge-projection-candidate-ready"
    : "phase-sensitive-wz-bridge-projection-absolute-scale-blocked";

var result = new
{
    phaseId = "phase162-phase-sensitive-wz-bridge-projection-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    projectionCandidateReady,
    gates = new
    {
        rulePromotable,
        rawGate = RawGate,
        anyRawGatePassed,
        targetRaw,
    },
    transitionAttempts,
    selectedProjectionCandidate = transitionAttempts
        .Where(attempt => attempt.rawGatePassed)
        .OrderByDescending(attempt => attempt.rawToTargetRatio)
        .FirstOrDefault(),
    diagnosis = projectionCandidateReady
        ? "The accepted phase-sensitive transition rule also reaches the W/Z raw-amplitude gate."
        : "The accepted phase-sensitive transition rule defines a direction, but candidate-8 does not reach the W/Z raw-amplitude gate on repaired modes; the remaining blocker is an absolute bridge/source mismatch, not intake evidence.",
    nextWork = projectionCandidateReady
        ? "feed this projection candidate into the W/Z absolute projection rerun"
        : "derive a target-independent W/Z bridge source or normalization that connects the candidate-8 transition rule to the phase69 W/Z mass-generation relation without fitting W/Z targets",
    sourceEvidence = new
    {
        phase95ModesPath = Phase95ModesPath,
        phase110Path = Phase110Path,
        phase160Path = Phase160Path,
        phase159Path = Phase159Path,
        candidate8MatrixPath = Candidate8MatrixPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase_sensitive_wz_bridge_projection_attempt.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase_sensitive_wz_bridge_projection_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.projectionCandidateReady,
        result.gates,
        result.transitionAttempts,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"projectionCandidateReady={projectionCandidateReady}");
Console.WriteLine($"bestRawToTargetRatio={transitionAttempts.Max(attempt => attempt.rawToTargetRatio):R}");

static double[,] LoadMatrix(JsonElement rows)
{
    int rowCount = rows.GetArrayLength();
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    int row = 0;
    foreach (var rowElement in rows.EnumerateArray())
    {
        int col = 0;
        foreach (var value in rowElement.EnumerateArray())
            matrix[row, col++] = value.GetDouble();
        row++;
    }

    return matrix;
}

static (double Real, double Imaginary) MatrixElement(double[,] matrixRe, double[,] matrixIm, double[] phiI, double[] phiJ)
{
    int n = matrixRe.GetLength(0);
    var iNorm = Normalize(phiI);
    var jNorm = Normalize(phiJ);
    var deltaJ = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrixRe[row, col];
            double aIm = matrixIm[row, col];
            double bRe = jNorm[col * 2];
            double bIm = jNorm[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }

        deltaJ[row * 2] = sumRe;
        deltaJ[row * 2 + 1] = sumIm;
    }

    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < n; k++)
    {
        double iRe = iNorm[k * 2];
        double iIm = iNorm[k * 2 + 1];
        double dRe = deltaJ[k * 2];
        double dIm = deltaJ[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }

    return (real, imaginary);
}

static double[] Normalize(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(v => v * v));
    return norm < 1e-30 ? vector : vector.Select(v => v / norm).ToArray();
}

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ModeRecord(
    int Index,
    string ModeId,
    string? SourceCanonicalFermionModeId,
    double[] Coefficients);
