using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase491: Amendment A7 exploration. The Phase455 verdict is not consumed.
// Formulae, normalizations, menu, numerical resolution, and taxonomy are frozen here.

const string Phase428Source = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/Program.cs";
const string Phase430Source = "studies/phase430_net_one_loop_direction_selection_probe_001/Program.cs";
const string Phase455Source = "studies/phase455_exact_fermionic_backreaction_probe_001/Program.cs";
const string Phase428Artifact = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string Phase430Artifact = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase455Artifact = "studies/phase455_exact_fermionic_backreaction_probe_001/output/exact_fermionic_backreaction_probe_summary.json";
const string Phase428SourceSha256 = "9d273970eb10675db02d474f3ca62e36a07788ea6c665414f93bae1e3181e998";
const string Phase430SourceSha256 = "890c3748b0f1b5b389ed87ea0a1826e57020940176531293791943b4a27908f0";
const string Phase455SourceSha256 = "6278d7c502d613c3f5c87fe9db13c54bc0f740b00978d0c0688c23e401589994";
const string Phase428ArtifactSha256 = "0b3361ab34e925e75e3b722bc25293e8ed935a35697cb19c9a657c6db26596bb";
const string Phase430ArtifactSha256 = "b60b28eb46e55d5978c8cc372f3dc62a5e453dca3ab8243f25c1506bccc0e42e";
const string Phase455ArtifactSha256 = "021d3b92ff25cd86003c08590f8c7943ca74b070473f056bf47e43d1e749bfbc";
const int DerivedFundamentalCount = 4;
const int ScanIntervals = 40_000;
const int BisectionIterations = 70;
const double DomainLower = 1e-7;
const double ZeroTolerance = 1e-18;
const double DerivativeZeroTolerance = 1e-9;
const double RootMergeTolerance = 1e-6;
const double DepthTolerance = 1e-8;

var models = new[]
{
    new ModelSpec("phase430-transverse-one-loop-determinant", 1.0, true,
        "Phase430 Program.cs:228-244; +1/2 times two polarizations gives +1 per log factor; adjoint squared masses are explicit."),
    new ModelSpec("phase430-mixed-tree-quartic-rank-one-restriction", 0.0, true,
        "Phase430 Program.cs:423-478; kappa*(a*b)^2*||[U,V]||^2 with kappa=1 is identically zero on either single rank-one axis."),
};
var rejectedModels = new[]
{
    new RejectedModel("arbitrary-rescaling-of-phase430-determinant", "No alternate bosonic determinant prefactor is committed."),
    new RejectedModel("invented-positive-single-ray-quartic", "The committed mixed quartic is proportional to (a*b)^2 and vanishes on a single ray; no t^4 coefficient is supplied."),
    new RejectedModel("control-branch-hessian-determinant", "Phase430 explicitly records bosonicMassModelTiedToControlBranchHessian=false."),
    new RejectedModel("source-defined-bosonic-fluctuation-determinant", "Phase430 explicitly records sourceDefinesBosonicFluctuationDeterminant=false."),
};
var zeroModes = new[]
{
    new ZeroModeSpec("Za", false, "exclude every eps^2=0 momentum symmetrically from bosonic and fermionic sums", true),
    new ZeroModeSpec("Zc", true, "keep eps^2=0 and exclude only exact factors at the 1e-18 threshold", true),
};
var invalidFloorRows = new[]
{
    new InvalidFloorSpec("Zb", 1e-5),
    new InvalidFloorSpec("Zb", 1e-4),
    new InvalidFloorSpec("Zb", 1e-3),
};
var normalizations = new[]
{
    new NormalizationSpec("committed-minus-one-half", -0.5, "Phase428:327-344 and Phase430:219-225"),
    new NormalizationSpec("phase455-preregistration-literal-minus-one", -1.0, "Phase455:30-38 records this explicit normalization robustness arm"),
};
string[] axes = { "T", "D", "S" };

var inputChecks = ValidateInputs();
bool committedInputsValid = inputChecks.All(x => x.Passed);
var table = new List<SensitivityRow>();

if (committedInputsValid)
{
    foreach (var model in models.Where(x => x.Admitted))
        foreach (var zeroMode in zeroModes.Where(x => x.IndependentlyExpressible))
            foreach (var normalization in normalizations)
                foreach (string axis in axes)
                    table.Add(Evaluate(model, zeroMode, normalization, axis));

    // Preserve all advertised soft-floor branches as invalid: no determinant-specific
    // rule maps Phase447's Hessian-relative inclusion floor to Phase455's log factors.
    foreach (var model in models.Where(x => x.Admitted))
        foreach (var floor in invalidFloorRows)
            foreach (var normalization in normalizations)
                foreach (string axis in axes)
                    table.Add(SensitivityRow.Invalid(model.Id, floor.ZeroMode, floor.FloorRel,
                        normalization.Id, axis,
                        "non-reconstructible-floor-map: Phase455 declares the sweep but ValueV/SB/VF contain no floor parameter; Phase447's floor is relative to a different Hessian spectrum"));
}

var validRows = table.Where(x => x.Status != "invalid").ToArray();
var invalidRows = table.Where(x => x.Status == "invalid").ToArray();
bool anyNull = validRows.Any(x => !x.HasNegativeDepthWell);
bool anyCandidate = validRows.Any(x => x.HasNegativeDepthWell);
string[] namedOutcomePartitionAssumptions = validRows.Length == 0 ? Array.Empty<string>() : new[]
{
    "bosonic-model",
    "zero-mode-treatment",
    "fermionic-normalization",
    "axis-orbit",
};
string verdictKind = !committedInputsValid || validRows.Length == 0
    ? "invalid-inputs"
    : anyNull && anyCandidate
        ? "model-convention-fragile"
        : anyCandidate
            ? "stable-candidate-well"
            : "stable-null";
string terminalStatus = "committed-bosonic-model-family-sensitivity-" + verdictKind;

string contractMaterial = string.Join("|",
    DerivedFundamentalCount, ScanIntervals, BisectionIterations, DomainLower,
    ZeroTolerance, DerivativeZeroTolerance, RootMergeTolerance, DepthTolerance,
    string.Join(';', models.Select(x => $"{x.Id}:{x.BosonCoefficient:R}:{x.Admitted}")),
    string.Join(';', zeroModes.Select(x => $"{x.Id}:{x.IncludeZeroMomentum}:{x.IndependentlyExpressible}")),
    string.Join(';', invalidFloorRows.Select(x => $"{x.ZeroMode}:{x.FloorRel:R}")),
    string.Join(';', normalizations.Select(x => $"{x.Id}:{x.Prefactor:R}")),
    string.Join(';', axes), Phase428SourceSha256, Phase430SourceSha256, Phase455SourceSha256,
    Phase428ArtifactSha256, Phase430ArtifactSha256, Phase455ArtifactSha256);
string frozenSensitivityContractSha256 = Sha256(Encoding.UTF8.GetBytes(contractMaterial));

bool structuralChecksPassed = models.Count(x => x.Admitted) == 2 && rejectedModels.Length == 4 &&
    validRows.Length == 24 && invalidRows.Length == 36 &&
    table.All(x => x.Status is "null" or "candidate-well" or "invalid") &&
    new[] { "stable-null", "stable-candidate-well", "model-convention-fragile", "invalid-inputs" }.Contains(verdictKind);

var result = new
{
    phaseId = "phase491-committed-bosonic-model-family-sensitivity",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    inputsValid = committedInputsValid,
    sensitivityClassification = verdictKind,
    admissibleBranchCount = validRows.Length,
    allAdmissibleBranchesNull = validRows.Length > 0 && validRows.All(x => !x.HasNegativeDepthWell),
    allAdmissibleBranchesCandidateWell = validRows.Length > 0 && validRows.All(x => x.HasNegativeDepthWell),
    namedAssumptionPartitionsOutcomes = anyNull && anyCandidate && namedOutcomePartitionAssumptions.Length > 0,
    namedOutcomePartitionAssumptions,
    decisionContractSha256 = frozenSensitivityContractSha256,
    taxonomy = new[] { "stable-null", "stable-candidate-well", "model-convention-fragile", "invalid-inputs" },
    committedBosonicModelFamilySensitivityPassed = committedInputsValid && structuralChecksPassed,
    frozenSensitivityContractSha256,
    frozenConfiguration = new
    {
        derivedFundamentalCount = DerivedFundamentalCount,
        scanIntervals = ScanIntervals,
        bisectionIterations = BisectionIterations,
        domainLower = DomainLower,
        zeroTolerance = ZeroTolerance,
        derivativeZeroTolerance = DerivativeZeroTolerance,
        rootMergeTolerance = RootMergeTolerance,
        depthTolerance = DepthTolerance,
        axes,
        targetBlindTerminalDefinition = "candidate iff an interior stationary point changes dV/dt from negative to positive and V(t*)-V(0)<-depthTolerance; otherwise null; nonfinite or unresolved rows remain invalid",
    },
    committedInputChecks = inputChecks,
    committedInputHashes = new
    {
        phase428SourceSha256 = Sha256File(Phase428Source),
        phase430SourceSha256 = Sha256File(Phase430Source),
        phase455SourceSha256 = Sha256File(Phase455Source),
        phase428ArtifactSha256 = Sha256File(Phase428Artifact),
        phase430ArtifactSha256 = Sha256File(Phase430Artifact),
        phase455ArtifactSha256 = Sha256File(Phase455Artifact),
    },
    modelAdmissibility = new
    {
        admitted = models,
        rejected = rejectedModels,
        rule = "admit only a bosonic formula and normalization reconstructible from committed Phase428/430/455 source or artifacts",
    },
    zeroModeAndIrMenu = new
    {
        independentlyExpressible = zeroModes,
        preservedInvalidFloorRows = invalidFloorRows.Select(x => new
        {
            x.ZeroMode,
            x.FloorRel,
            status = "invalid",
            reason = "the committed code does not define how this Hessian-relative floor acts on determinant log factors",
        }).ToArray(),
    },
    fermionicNormalizationTable = normalizations,
    sensitivityTable = table,
    summary = new
    {
        totalRows = table.Count,
        validRows = validRows.Length,
        invalidRows = invalidRows.Length,
        nullRows = validRows.Count(x => !x.HasNegativeDepthWell),
        candidateWellRows = validRows.Count(x => x.HasNegativeDepthWell),
        candidateModels = validRows.Where(x => x.HasNegativeDepthWell).Select(x => x.ModelId).Distinct().Order().ToArray(),
        candidateZeroModes = validRows.Where(x => x.HasNegativeDepthWell).Select(x => x.ZeroMode).Distinct().Order().ToArray(),
        candidateNormalizations = validRows.Where(x => x.HasNegativeDepthWell).Select(x => x.FermionicNormalization).Distinct().Order().ToArray(),
        candidateAxes = validRows.Where(x => x.HasNegativeDepthWell).Select(x => x.Axis).Distinct().Order().ToArray(),
    },
    phase455VerdictConsumedAsAnswer = false,
    independentlyRecomputedFromCommittedFormulae = true,
    explorationOnly = true,
    confirmationEvidence = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458EvaluationAuthorized = false,
    phase458GateSatisfied = false,
    binderLaunchAuthorized = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    productionAuthorized = false,
    samplingAuthorized = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = verdictKind switch
    {
        "stable-null" => "Every valid reconstructible branch is null, at exploration scope only; invalid floor branches remain preserved and no upstream terminal changes.",
        "stable-candidate-well" => "Every valid reconstructible branch contains a candidate well, at exploration scope only; invalid floor branches remain preserved and no upstream terminal changes.",
        "model-convention-fragile" => "The independently recomputed candidate-well classification changes across named reconstructible bosonic, zero-mode, IR, or normalization branches. Fragility is preserved; no upstream terminal changes.",
        _ => "Committed inputs or the reconstructible table are invalid. The result fails closed and no upstream terminal changes.",
    },
};

string outputDirectory = "studies/phase491_committed_bosonic_model_family_sensitivity_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "committed_bosonic_model_family_sensitivity.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "committed_bosonic_model_family_sensitivity_summary.json"), json);

Console.WriteLine(terminalStatus);
Console.WriteLine($"rows={table.Count} valid={validRows.Length} invalid={invalidRows.Length} null={validRows.Count(x => !x.HasNegativeDepthWell)} candidate={validRows.Count(x => x.HasNegativeDepthWell)}");
foreach (var group in validRows.GroupBy(x => new { x.ModelId, x.ZeroMode, x.FermionicNormalization }))
    Console.WriteLine($"{group.Key.ModelId}/{group.Key.ZeroMode}/{group.Key.FermionicNormalization}: " + string.Join(',', group.Select(x => $"{x.Axis}:{x.Status}")));

if (!committedInputsValid || !structuralChecksPassed)
    throw new InvalidOperationException("Phase491 fail-closed: committed input or structural table check failed.");

SensitivityRow Evaluate(ModelSpec model, ZeroModeSpec zeroMode, NormalizationSpec normalization, string axis)
{
    double tMax = axis == "S" ? 1.73 : 1.99;
    double V(double t) => Potential(t, model.BosonCoefficient, zeroMode.IncludeZeroMomentum, normalization.Prefactor, axis);
    double D(double t) => PotentialDerivative(t, model.BosonCoefficient, zeroMode.IncludeZeroMomentum, normalization.Prefactor, axis);

    var roots = new List<double>();
    double previousT = DomainLower;
    double previousD = D(previousT);
    if (!double.IsFinite(previousD))
        return SensitivityRow.Invalid(model.Id, zeroMode.Id, null, normalization.Id, axis, "nonfinite derivative at lower domain bound");
    for (int interval = 1; interval <= ScanIntervals; interval++)
    {
        double t = DomainLower + (tMax - DomainLower) * interval / ScanIntervals;
        double derivative = D(t);
        if (!double.IsFinite(derivative))
            return SensitivityRow.Invalid(model.Id, zeroMode.Id, null, normalization.Id, axis, $"nonfinite derivative at t={t:R}");
        if (previousD * derivative < 0.0)
        {
            double lo = previousT, hi = t, dlo = previousD;
            for (int iteration = 0; iteration < BisectionIterations; iteration++)
            {
                double mid = 0.5 * (lo + hi);
                double dm = D(mid);
                if (!double.IsFinite(dm))
                    return SensitivityRow.Invalid(model.Id, zeroMode.Id, null, normalization.Id, axis, $"nonfinite derivative during bisection at t={mid:R}");
                if (dlo * dm <= 0.0) hi = mid;
                else { lo = mid; dlo = dm; }
            }
            double root = 0.5 * (lo + hi);
            if (roots.Count == 0 || Math.Abs(root - roots[^1]) > RootMergeTolerance) roots.Add(root);
        }
        previousT = t;
        previousD = derivative;
    }

    var stationary = new List<StationaryPoint>();
    foreach (double root in roots)
    {
        double delta = Math.Max(2.0 * (tMax - DomainLower) / ScanIntervals, 1e-6);
        double left = D(Math.Max(DomainLower, root - delta));
        double right = D(Math.Min(tMax, root + delta));
        string kind = left < -DerivativeZeroTolerance && right > DerivativeZeroTolerance ? "minimum"
            : left > DerivativeZeroTolerance && right < -DerivativeZeroTolerance ? "maximum"
            : "degenerate-or-unresolved";
        double value = V(root);
        if (!double.IsFinite(value))
            return SensitivityRow.Invalid(model.Id, zeroMode.Id, null, normalization.Id, axis, $"nonfinite potential at stationary point t={root:R}");
        stationary.Add(new StationaryPoint(root, kind, value, kind == "minimum" && value < -DepthTolerance));
    }
    bool candidate = stationary.Any(x => x.NegativeDepthMinimum);
    return new SensitivityRow(model.Id, zeroMode.Id, null, normalization.Id, axis,
        candidate ? "candidate-well" : "null", tMax, roots.Count, stationary.ToArray(), candidate, null);
}

double Potential(double t, double bosonCoefficient, bool includeZeroMomentum, double fermionPrefactor, string axis)
{
    double sum = 0.0;
    foreach (var (s1, s2) in Momenta())
    {
        double eps2 = s1 * s1 + s2 * s2;
        if (!includeZeroMomentum && eps2 == 0.0) continue;
        foreach (double mass2 in BosonMassesSquared(axis))
        {
            if (mass2 == 0.0) continue;
            double arg = eps2 + t * t * mass2;
            if (arg > ZeroTolerance) sum += bosonCoefficient * Math.Log(arg);
            if (eps2 > ZeroTolerance) sum -= bosonCoefficient * Math.Log(eps2);
        }
        foreach (double u in FundamentalEigenvalues(axis))
        {
            if (u == 0.0) continue;
            double arg = Math.Pow(s1 + t * u, 2) + Math.Pow(s2 + t * u, 2);
            if (arg > ZeroTolerance) sum += DerivedFundamentalCount * fermionPrefactor * 4.0 * Math.Log(arg);
            if (eps2 > ZeroTolerance) sum -= DerivedFundamentalCount * fermionPrefactor * 4.0 * Math.Log(eps2);
        }
    }
    return sum;
}

double PotentialDerivative(double t, double bosonCoefficient, bool includeZeroMomentum, double fermionPrefactor, string axis)
{
    double sum = 0.0;
    foreach (var (s1, s2) in Momenta())
    {
        double eps2 = s1 * s1 + s2 * s2;
        if (!includeZeroMomentum && eps2 == 0.0) continue;
        foreach (double mass2 in BosonMassesSquared(axis))
        {
            if (mass2 == 0.0) continue;
            double arg = eps2 + t * t * mass2;
            if (arg > ZeroTolerance) sum += bosonCoefficient * 2.0 * t * mass2 / arg;
        }
        foreach (double u in FundamentalEigenvalues(axis))
        {
            if (u == 0.0) continue;
            double a = s1 + t * u, b = s2 + t * u;
            double arg = a * a + b * b;
            if (arg > ZeroTolerance)
                sum += DerivedFundamentalCount * fermionPrefactor * 4.0 * (2.0 * u * (a + b) / arg);
        }
    }
    return sum;
}

static IEnumerable<(int S1, int S2)> Momenta()
{
    int[] s = { 0, 1, 0, -1 };
    foreach (int a in s) foreach (int b in s) yield return (a, b);
}

static double[] FundamentalEigenvalues(string axis) => axis == "S"
    ? new[] { Math.Sqrt(3.0) / 6.0, Math.Sqrt(3.0) / 6.0, -Math.Sqrt(3.0) / 3.0 }
    : new[] { 0.5, -0.5, 0.0 };

static double[] BosonMassesSquared(string axis) => axis == "S"
    ? new[] { 0.75, 0.75, 0.75, 0.75, 0.0, 0.0, 0.0, 0.0 }
    : new[] { 1.0, 1.0, 0.25, 0.25, 0.25, 0.25, 0.0, 0.0 };

InputCheck[] ValidateInputs()
{
    var checks = new List<InputCheck>();
    foreach (string path in new[] { Phase428Source, Phase430Source, Phase455Source, Phase428Artifact, Phase430Artifact, Phase455Artifact })
        checks.Add(new InputCheck("file-present:" + path, File.Exists(path), File.Exists(path) ? "present" : "missing"));
    if (!checks.All(x => x.Passed)) return checks.ToArray();

    string p428 = File.ReadAllText(Phase428Source);
    string p430 = File.ReadAllText(Phase430Source);
    string p455 = File.ReadAllText(Phase455Source);
    checks.Add(new InputCheck("phase428-fermion-prefactor", p428.Contains("return -0.5 * sum - reference;", StringComparison.Ordinal), "committed -1/2 formula"));
    checks.Add(new InputCheck("phase430-boson-polarizations", p430.Contains("sum += 2.0 * Math.Log(arg); // 2 polarizations", StringComparison.Ordinal) && p430.Contains("return 0.5 * sum;", StringComparison.Ordinal), "committed +1/2 x 2 formula"));
    checks.Add(new InputCheck("phase430-rank-one-tree-restriction", p430.Contains("MixedKappa * (a * b) * (a * b) * commNormSquared", StringComparison.Ordinal), "committed mixed quartic formula"));
    checks.Add(new InputCheck("phase455-derived-count", p455.Contains("N_f = 4 fundamental", StringComparison.Ordinal), "derived active fundamental count"));
    int valueStart = p455.IndexOf("double ValueV", StringComparison.Ordinal);
    int valueEnd = valueStart < 0 ? -1 : p455.IndexOf("TMaxRational", valueStart, StringComparison.Ordinal);
    checks.Add(new InputCheck("phase455-floor-not-implemented", valueStart >= 0 && valueEnd > valueStart && !p455[valueStart..valueEnd].Contains("floor", StringComparison.OrdinalIgnoreCase), "floor declaration has no determinant-specific value implementation"));
    checks.Add(new InputCheck("artifact-phase-ids",
        ArtifactPhaseId(Phase428Artifact) == "phase428-fermion-loop-block-selection-no-go-probe" &&
        ArtifactPhaseId(Phase430Artifact) == "phase430-net-one-loop-direction-selection-probe" &&
        ArtifactPhaseId(Phase455Artifact) == "phase455-exact-fermionic-backreaction-probe",
        "exact phase identities; verdict fields not consumed"));
    checks.Add(new InputCheck("committed-byte-hashes",
        Sha256File(Phase428Source) == Phase428SourceSha256 &&
        Sha256File(Phase430Source) == Phase430SourceSha256 &&
        Sha256File(Phase455Source) == Phase455SourceSha256 &&
        Sha256File(Phase428Artifact) == Phase428ArtifactSha256 &&
        Sha256File(Phase430Artifact) == Phase430ArtifactSha256 &&
        Sha256File(Phase455Artifact) == Phase455ArtifactSha256,
        "all six prospectively pinned source/artifact hashes match"));
    return checks.ToArray();
}

static string? ArtifactPhaseId(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("phaseId").GetString();
}

static string Sha256File(string path) => Sha256(File.ReadAllBytes(path));
static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed class ModelSpec(string id, double bosonCoefficient, bool admitted, string provenance)
{
    public string Id { get; } = id;
    public double BosonCoefficient { get; } = bosonCoefficient;
    public bool Admitted { get; } = admitted;
    public string Provenance { get; } = provenance;
}
sealed class RejectedModel(string id, string reason) { public string Id { get; } = id; public string Reason { get; } = reason; }
sealed class ZeroModeSpec(string id, bool includeZeroMomentum, string definition, bool independentlyExpressible)
{
    public string Id { get; } = id;
    public bool IncludeZeroMomentum { get; } = includeZeroMomentum;
    public string Definition { get; } = definition;
    public bool IndependentlyExpressible { get; } = independentlyExpressible;
}
sealed class InvalidFloorSpec(string zeroMode, double floorRel) { public string ZeroMode { get; } = zeroMode; public double FloorRel { get; } = floorRel; }
sealed class NormalizationSpec(string id, double prefactor, string provenance) { public string Id { get; } = id; public double Prefactor { get; } = prefactor; public string Provenance { get; } = provenance; }
sealed class InputCheck(string id, bool passed, string detail) { public string Id { get; } = id; public bool Passed { get; } = passed; public string Detail { get; } = detail; }
sealed class StationaryPoint(double t, string kind, double relativePotential, bool negativeDepthMinimum)
{
    public double T { get; } = t;
    public string Kind { get; } = kind;
    public double RelativePotential { get; } = relativePotential;
    public bool NegativeDepthMinimum { get; } = negativeDepthMinimum;
}
sealed class SensitivityRow(string modelId, string zeroMode, double? floorRel, string fermionicNormalization, string axis,
    string status, double? tMax, int stationaryPointCount, StationaryPoint[] stationaryPoints, bool hasNegativeDepthWell, string? invalidReason)
{
    public string ModelId { get; } = modelId;
    public string ZeroMode { get; } = zeroMode;
    public double? FloorRel { get; } = floorRel;
    public string FermionicNormalization { get; } = fermionicNormalization;
    public string Axis { get; } = axis;
    public string Status { get; } = status;
    public double? TMax { get; } = tMax;
    public int StationaryPointCount { get; } = stationaryPointCount;
    public StationaryPoint[] StationaryPoints { get; } = stationaryPoints;
    public bool HasNegativeDepthWell { get; } = hasNegativeDepthWell;
    public string? InvalidReason { get; } = invalidReason;
    public static SensitivityRow Invalid(string model, string zeroMode, double? floor, string normalization, string axis, string reason) =>
        new(model, zeroMode, floor, normalization, axis, "invalid", null, 0, Array.Empty<StationaryPoint>(), false, reason);
}
