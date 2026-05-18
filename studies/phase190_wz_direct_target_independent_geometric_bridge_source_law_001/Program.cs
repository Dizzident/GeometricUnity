using System.Text.Json;

const string DefaultOutputDir = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string SpinorPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string ModeFamiliesPath = "studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json";
const string ModeSignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";
const string BosonModeVectorDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const string PromotedFermionModeDir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string VariationMatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE190_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorPath));
using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var modeFamilies = JsonDocument.Parse(File.ReadAllText(ModeFamiliesPath));
using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));

var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
var pair = new PairKey(4, 6);
double stabilitySpreadTolerance = JsonDouble(phase172.RootElement, "stabilitySpreadTolerance") ?? 0.05;

var registryCandidates = bosonRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(candidate => new CandidateRegistryRecord(
        RequiredString(candidate, "candidateId"),
        RequiredString(candidate, "primaryFamilyId"),
        candidate.GetProperty("contributingModeIds").EnumerateArray().Select(mode => mode.GetString() ?? "").Where(mode => mode.Length > 0).ToArray(),
        JsonDouble(candidate, "branchStabilityScore"),
        JsonString(candidate, "claimClass")))
    .OrderBy(candidate => CandidateOrdinal(candidate.CandidateId))
    .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();

var familyStability = modeFamilies.RootElement
    .EnumerateArray()
    .ToDictionary(
        family => RequiredString(family, "familyId"),
        family => new FamilyRecord(
            RequiredString(family, "familyId"),
            JsonBool(family, "isStable") is true,
            JsonInt(family, "ambiguityCount") ?? 0,
            JsonDouble(family, "eigenvalueSpread")),
        StringComparer.Ordinal);

var fermionModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadFermionModes(PromotedFermionModePath(backgroundId)),
    StringComparer.Ordinal);

var candidateAssessments = registryCandidates
    .Select(candidate => AssessCandidate(candidate, backgrounds, fermionModes, pair, familyStability))
    .OrderBy(assessment => assessment.RelativeSpread)
    .ThenByDescending(assessment => assessment.MinMagnitude)
    .ThenBy(assessment => CandidateOrdinal(assessment.CandidateId))
    .ToArray();

var stableCandidates = candidateAssessments.Where(assessment => assessment.StabilityPassed).ToArray();
var bestCandidate = candidateAssessments.FirstOrDefault();
bool candidateLawConstructed = candidateAssessments.Length > 0 && candidateAssessments.All(assessment => assessment.BackgroundRecords.Count == backgrounds.Length);
bool targetObservablesUsed = false;
bool theoremClaimed = false;
string terminalStatus = stableCandidates.Length > 0
    ? "wz-direct-target-independent-bridge-source-law-candidate-stable-not-theorem"
    : "wz-direct-target-independent-bridge-source-law-candidate-not-sibling-stable";

var result = new
{
    phaseId = "phase190-wz-direct-target-independent-geometric-bridge-source-law",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    candidateLawConstructed,
    theoremClaimed,
    targetObservablesUsed,
    evaluatedPair = new
    {
        source = "P172 best W/Z-like fermion pair",
        fromModeIndex = pair.FromModeIndex,
        toModeIndex = pair.ToModeIndex,
    },
    branchLocalLaw = new
    {
        lawId = "branch-local-phase91-promoted-fermion-phase12-connection-mode-dirac-variation-matrix-element-v1",
        formula = "B_b,k(i,j) = <psi_b,i, delta D_omega[eta_b,k] psi_b,j>",
        etaSource = "Phase12 boson registry contributingModeIds plus branch-local spectra/mode-signature vectors",
        deltaDSource = "Phase12 finite-difference variation matrix for matching boson candidate on the same background",
        fermionSource = "Phase91 branch-stability-promoted fermion modes, matching the P172 promoted W/Z-like mode-index pair",
        normalization = "raw matrix element using Phase91 promoted fermion eigenvectors and Phase12 finite-difference variation matrices; no W/Z target mass, target-implied scalar, or target residual is used",
        branchLocal = true,
        targetIndependent = true,
        excludedTargetObservableIds = new[] { "physical-w-boson-mass-gev", "physical-z-boson-mass-gev", "phase110-target-implied-raw-matrix-element-magnitude" },
    },
    manuscriptResearchFinding = new
    {
        uniqueLawProvidedByDraftOrCompletion = false,
        finding = "The manuscript draft/completion supplies a typed fermionic action placeholder and a VO-7 mixed-linearization obligation, but not a unique direct W/Z bridge-source law.",
        implementationStatus = "This phase implements a branch-local target-independent candidate law for evidence gathering; it is not promoted as a theorem.",
    },
    siblingStability = new
    {
        backgroundIds = backgrounds,
        stabilitySpreadTolerance,
        stableCandidateCount = stableCandidates.Length,
        bestCandidate,
        topCandidates = candidateAssessments.Take(12).ToArray(),
        stableCandidates = stableCandidates.Take(12).ToArray(),
    },
    inputInventory = new
    {
        ambientSpace = geometry.RootElement.GetProperty("ambientSpace").Clone(),
        spinorComponents = JsonInt(spinor.RootElement, "spinorComponents"),
        candidateCount = registryCandidates.Length,
        modeFamilyCount = modeFamilies.RootElement.GetArrayLength(),
        fermionModeCounts = fermionModes.ToDictionary(pair => pair.Key, pair => pair.Value.Count, StringComparer.Ordinal),
        promotedFermionModePaths = backgrounds.ToDictionary(
            backgroundId => backgroundId,
            PromotedFermionModePath,
            StringComparer.Ordinal),
        variationCandidateCountPerBackground = backgrounds.ToDictionary(
            backgroundId => backgroundId,
            backgroundId => Directory.EnumerateFiles(VariationMatrixDir, $"variation-{backgroundId}-candidate-*.matrix.json").Count(),
            StringComparer.Ordinal),
    },
    decision = theoremClaimed
        ? "Direct bridge-source theorem promoted."
        : "No theorem is promoted. The artifact records a target-independent branch-local candidate law and its sibling-background stability evidence.",
    sourceEvidence = new
    {
        geometryPath = GeometryPath,
        spinorPath = SpinorPath,
        bosonRegistryPath = BosonRegistryPath,
        modeFamiliesPath = ModeFamiliesPath,
        modeSignatureDir = ModeSignatureDir,
        bosonModeVectorDir = BosonModeVectorDir,
        promotedFermionModeDir = PromotedFermionModeDir,
        promotedFermionModePaths = backgrounds.ToDictionary(
            backgroundId => backgroundId,
            PromotedFermionModePath,
            StringComparer.Ordinal),
        variationMatrixDir = VariationMatrixDir,
        phase172Path = Phase172Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_direct_target_independent_geometric_bridge_source_law.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_direct_target_independent_geometric_bridge_source_law_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.candidateLawConstructed,
        result.theoremClaimed,
        result.targetObservablesUsed,
        result.evaluatedPair,
        result.branchLocalLaw,
        result.manuscriptResearchFinding,
        result.siblingStability,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"candidateLawConstructed={candidateLawConstructed}");
Console.WriteLine($"targetObservablesUsed={targetObservablesUsed}");
Console.WriteLine($"theoremClaimed={theoremClaimed}");
Console.WriteLine($"evaluatedPair={pair.FromModeIndex}->{pair.ToModeIndex}");
Console.WriteLine($"candidateCount={candidateAssessments.Length}");
Console.WriteLine($"stableCandidateCount={stableCandidates.Length}");
Console.WriteLine($"bestCandidate={bestCandidate?.CandidateId}");
Console.WriteLine($"bestRelativeSpread={bestCandidate?.RelativeSpread:R}");

CandidateAssessment AssessCandidate(
    CandidateRegistryRecord candidate,
    IReadOnlyList<string> backgroundIds,
    IReadOnlyDictionary<string, IReadOnlyList<FermionModeRecord>> backgroundFermionModes,
    PairKey targetPair,
    IReadOnlyDictionary<string, FamilyRecord> families)
{
    var backgroundRecords = backgroundIds
        .Select(backgroundId =>
        {
            string modeId = candidate.ContributingModeIds.Single(mode => mode.StartsWith(backgroundId + "-mode-", StringComparison.Ordinal));
            string matrixPath = Path.Combine(VariationMatrixDir, $"variation-{backgroundId}-{candidate.CandidateId}.matrix.json");
            using var matrix = JsonDocument.Parse(File.ReadAllText(matrixPath));
            var matrixRe = LoadMatrix(matrix.RootElement.GetProperty("real"));
            var matrixIm = LoadMatrix(matrix.RootElement.GetProperty("imag"));
            var from = backgroundFermionModes[backgroundId].Single(mode => mode.ModeIndex == targetPair.FromModeIndex);
            var to = backgroundFermionModes[backgroundId].Single(mode => mode.ModeIndex == targetPair.ToModeIndex);
            var value = MatrixElement(matrixRe, matrixIm, from.Coefficients, to.Coefficients);
            double magnitude = Magnitude(value.Real, value.Imaginary);
            var signature = LoadBosonSignature(Path.Combine(ModeSignatureDir, modeId + ".json"));
            var vector = LoadBosonModeVector(Path.Combine(BosonModeVectorDir, modeId + ".json"));
            return new CandidateBackgroundRecord(
                backgroundId,
                modeId,
                from.ModeId,
                to.ModeId,
                signature.ObservedShape,
                signature.SignatureHash,
                Norm(signature.ObservedCoefficients),
                vector.Eigenvalue,
                vector.GaugeLeakScore,
                Norm(vector.ModeVector),
                value.Real,
                value.Imaginary,
                magnitude);
        })
        .ToArray();

    double minMagnitude = backgroundRecords.Min(record => record.Magnitude);
    double maxMagnitude = backgroundRecords.Max(record => record.Magnitude);
    double meanMagnitude = backgroundRecords.Average(record => record.Magnitude);
    double relativeSpread = (maxMagnitude - minMagnitude) / Math.Max(Math.Abs(meanMagnitude), 1e-300);
    bool stabilityPassed = relativeSpread <= stabilitySpreadTolerance;
    double complexAlignment = ComplexAlignment(backgroundRecords[0], backgroundRecords[1]);
    families.TryGetValue(candidate.PrimaryFamilyId, out var family);
    return new CandidateAssessment(
        candidate.CandidateId,
        candidate.PrimaryFamilyId,
        candidate.BranchStabilityScore,
        candidate.ClaimClass,
        family,
        backgroundRecords,
        minMagnitude,
        maxMagnitude,
        meanMagnitude,
        relativeSpread,
        complexAlignment,
        stabilityPassed);
}

static IReadOnlyList<FermionModeRecord> LoadFermionModes(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new FermionModeRecord(
            RequiredString(mode, "modeId"),
            RequiredString(mode, "backgroundId"),
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex"),
            JsonDouble(mode, "eigenvalueRe"),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
}

static string PromotedFermionModePath(string backgroundId) =>
    Path.Combine(PromotedFermionModeDir, backgroundId, "branch_stability_promoted_fermion_modes.json");

static BosonSignatureRecord LoadBosonSignature(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return new BosonSignatureRecord(
        RequiredString(doc.RootElement, "modeId"),
        doc.RootElement.GetProperty("observedCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray(),
        doc.RootElement.GetProperty("observedShape").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
        RequiredString(doc.RootElement, "signatureHash"));
}

static BosonModeVectorRecord LoadBosonModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return new BosonModeVectorRecord(
        RequiredString(doc.RootElement, "modeId"),
        RequiredDouble(doc.RootElement, "eigenvalue"),
        RequiredDouble(doc.RootElement, "gaugeLeakScore"),
        doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
}

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
    double norm = Norm(vector);
    return norm < 1e-30 ? vector : vector.Select(value => value / norm).ToArray();
}

static double Norm(IEnumerable<double> values) => Math.Sqrt(values.Sum(value => value * value));
static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);
static double ComplexAlignment(CandidateBackgroundRecord a, CandidateBackgroundRecord b)
{
    double denom = a.Magnitude * b.Magnitude;
    if (denom < 1e-300)
        return 0.0;
    return (a.MatrixElementReal * b.MatrixElementReal + a.MatrixElementImaginary * b.MatrixElementImaginary) / denom;
}

static int CandidateOrdinal(string candidateId)
{
    const string prefix = "candidate-";
    return candidateId.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(candidateId[prefix.Length..], out int value)
        ? value
        : int.MaxValue;
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record CandidateRegistryRecord(string CandidateId, string PrimaryFamilyId, IReadOnlyList<string> ContributingModeIds, double? BranchStabilityScore, string? ClaimClass);
sealed record FamilyRecord(string FamilyId, bool IsStable, int AmbiguityCount, double? EigenvalueSpread);
sealed record FermionModeRecord(string ModeId, string BackgroundId, int ModeIndex, double? EigenvalueRe, double[] Coefficients);
sealed record BosonSignatureRecord(string ModeId, double[] ObservedCoefficients, IReadOnlyList<int> ObservedShape, string SignatureHash);
sealed record BosonModeVectorRecord(string ModeId, double Eigenvalue, double GaugeLeakScore, double[] ModeVector);
sealed record CandidateBackgroundRecord(
    string BackgroundId,
    string EtaModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    IReadOnlyList<int> EtaObservedShape,
    string EtaSignatureHash,
    double EtaObservedCoefficientNorm,
    double EtaBosonEigenvalue,
    double EtaGaugeLeakScore,
    double EtaModeVectorNorm,
    double MatrixElementReal,
    double MatrixElementImaginary,
    double Magnitude);
sealed record CandidateAssessment(
    string CandidateId,
    string PrimaryFamilyId,
    double? RegistryBranchStabilityScore,
    string? RegistryClaimClass,
    FamilyRecord? ModeFamily,
    IReadOnlyList<CandidateBackgroundRecord> BackgroundRecords,
    double MinMagnitude,
    double MaxMagnitude,
    double MeanMagnitude,
    double RelativeSpread,
    double ComplexAlignment,
    bool StabilityPassed);
