using System.Security.Cryptography;
using System.Text.Json;

const string Phase481PlanPath = "studies/phase481_phase456_prospective_repair_preregistration_001/planning/phase481_pack_construction_plan_v1.json";
const string Phase502ContractPath = "studies/phase502_phase456_adaptive_calibration_protocol_specification_001/preregistration/phase502_adaptive_calibration_protocol_v1.json";
const string Phase509Path = "studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001/output/phase481_anisotropic_cpu_reference_feasibility_summary.json";
const string Phase510Path = "studies/phase510_phase481_execution_readiness_adjudicator_001/output/phase481_execution_readiness_adjudicator_summary.json";
const string Phase510ContractPath = "studies/phase510_phase481_execution_readiness_adjudicator_001/preregistration/phase510_execution_readiness_contract_v1.json";
const string Phase452ProgramPath = "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs";
const string ContractPath = "studies/phase511_phase481_throughput_benchmark_eligibility_audit_001/preregistration/phase511_throughput_benchmark_eligibility_contract_v1.json";
const string OutputDir = "studies/phase511_phase481_throughput_benchmark_eligibility_audit_001/output";
const string PlannedDefaultConfigurationPath = "studies/phase481_phase456_prospective_repair_preregistration_001/preregistration/default_configuration.json";
const string PlannedPackManifestPath = "studies/phase481_phase456_prospective_repair_preregistration_001/preregistration/repair_pack_manifest.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");
var bindings = new[]
{
    Bind("phase481-plan", Phase481PlanPath, expected.GetProperty("phase481PlanSha256").GetString()!),
    Bind("phase502-contract", Phase502ContractPath, expected.GetProperty("phase502ContractSha256").GetString()!),
    Bind("phase509-summary", Phase509Path, expected.GetProperty("phase509SummarySha256").GetString()!),
    Bind("phase510-summary", Phase510Path, expected.GetProperty("phase510SummarySha256").GetString()!),
    Bind("phase510-contract", Phase510ContractPath, expected.GetProperty("phase510ContractSha256").GetString()!),
    Bind("phase452-program", Phase452ProgramPath, expected.GetProperty("phase452ProgramSha256").GetString()!),
};

string phase452Source = File.ReadAllText(Phase452ProgramPath);
string[] sequence = JsonStringArray(Phase481PlanPath, "constructionSequenceAfterAllBlockersClose");
string[] plannedPackFiles = JsonStringArray(Phase481PlanPath, "plannedPackFiles");
var requiredFields = contract.GetProperty("requiredWorkloadFields").EnumerateArray()
    .Select(x => new
    {
        id = x.GetProperty("id").GetString()!,
        category = x.GetProperty("category").GetString()!,
    })
    .ToArray();

bool currentPhase452FactsRecognized =
    phase452Source.Contains("int nLeap = phase456Mode ? 12", StringComparison.Ordinal)
    && phase452Source.Contains("const int ThetaMovesPerTraj = 2", StringComparison.Ordinal)
    && phase452Source.Contains("const int ThetaSweepEvery = 50", StringComparison.Ordinal)
    && phase452Source.Contains("Parallel.ForEach(specs", StringComparison.Ordinal)
    && phase452Source.Contains("swCol.Elapsed.TotalMilliseconds / nSamp", StringComparison.Ordinal)
    && phase452Source.Contains("if (lastDH < 0 || rng.NextDouble()", StringComparison.Ordinal);

bool precursorSchemasValid = bindings.All(x => x.HashMatches)
    && contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && requiredFields.Length == 22
    && requiredFields.Select(x => x.id).Distinct(StringComparer.Ordinal).Count() == requiredFields.Length
    && JsonBool(Phase481PlanPath, "planOnly")
    && !JsonBool(Phase481PlanPath, "phase481PackCreated")
    && sequence.Any(x => x.StartsWith("P1 freeze geometry RNG backend chain topology storage schema and default configuration", StringComparison.Ordinal))
    && plannedPackFiles.Contains("preregistration/default_configuration.json", StringComparer.Ordinal)
    && !JsonBool(Phase502ContractPath, "phase481PackCreationAllowed")
    && JsonString(Phase509Path, "verdictKind") == "anisotropic-cpu-reference-controls-ready"
    && JsonBool(Phase509Path, "allControlsPassed")
    && !JsonBool(Phase509Path, "fullT16OrT32MeshConstructed")
    && JsonString(Phase510Path, "verdictKind") == "cost-envelope-failure"
    && !JsonNestedBool(Phase510Path, "costRefusal", "admissibleThroughputEvidencePresent")
    && !JsonBool(Phase510Path, "benchmarkRun")
    && currentPhase452FactsRecognized;

var frozenIds = new HashSet<string>(StringComparer.Ordinal)
{
    "acquisition-geometry",
    "rng-stream-contract",
    "chain-retention-cadence",
    "cost-ceiling-arithmetic",
};
string MissingReason(string id) => id switch
{
    "production-default-artifact" => "Phase481 P1 has not emitted the planned default_configuration.json.",
    "implementation-fingerprint" => "No Phase481 executable pack, implementation hash, or binary fingerprint exists.",
    "time-axis-and-layout-map" => "The historical Phase452 axis-0 convention is not bound to the prospective anisotropic layout.",
    "action-member-beta-and-control-mix" => "No future default selects the production action/member/beta/control mix.",
    "integrator-algorithm-and-step-count" => "The historical twelve-step leapfrog is not frozen as the Phase481 default.",
    "step-size-and-warmup-adaptation" => "No Phase481 step size, adaptation rule, or post-warmup freeze is specified.",
    "force-call-and-rejection-branch-counts" => "Historical accepted/rejected branches differ, but no future conservative count is frozen.",
    "zero-mode-and-projector-policy" => "No Phase481 production default freezes projection behavior.",
    "theta-update-cadence" => "Historical theta moves and sweeps are not forwarded into a Phase481 default.",
    "measurement-and-correlator-work" => "Required retained observables are named, but their executable work ledger is absent.",
    "execution-thread-and-process-topology" => "Four chain identities are frozen, but worker/process execution topology is not.",
    "state-dependence-and-worst-case-policy" => "No state-independent proof or worst-case branch policy exists.",
    "allocation-cache-and-gc-policy" => "No cache state, allocation, GC, or memory-pressure timing policy exists.",
    "aggregate-process-cpu-accounting" => "The CPU-second unit is named, but clock/API, worker, child, user, and kernel accounting are not.",
    "t16-t32-ceiling-scope" => "The contract does not uniquely state whether T16 and conditional T32 consume one joint or separate envelopes.",
    "single-state-allocation-legality" => "No frozen boundary distinguishes a reduced fixture from forbidden production-sized allocation.",
    "fixed-overhead-io-and-checkpoint-cost" => "The production loader and checkpoint codec do not exist, so their cost cannot be ledgered.",
    "repeat-variance-and-one-sided-bound" => "No repeats, warmup, variance statistic, confidence level, or one-sided upper-bound rule is frozen.",
    _ => "Missing required benchmark-definition evidence.",
};
var fieldAudit = requiredFields.Select(x => new
{
    x.id,
    x.category,
    status = frozenIds.Contains(x.id) ? "frozen" : "missing",
    evidenceKind = frozenIds.Contains(x.id) ? "prospective-exact-bound-contract" : "none",
    reason = frozenIds.Contains(x.id) ? "The current exact-bound planning/contract artifacts uniquely fix this field." : MissingReason(x.id),
}).ToArray();

bool productionDefaultPresent = File.Exists(PlannedDefaultConfigurationPath);
bool implementationFingerprintPresent = File.Exists(PlannedPackManifestPath);
bool productionDefaultOrImplementationMissing = !productionDefaultPresent || !implementationFingerprintPresent;
bool operationLedgerComplete = fieldAudit.Where(x => x.category == "operation-ledger").All(x => x.status == "frozen");
bool cpuMeasurementContractComplete = fieldAudit.Where(x => x.category == "benchmark-contract").All(x => x.status == "frozen");
bool workloadDefinitionComplete = fieldAudit.All(x => x.status == "frozen");

string[] precedence = contract.GetProperty("precedence").EnumerateArray().Select(x => x.GetString()!).ToArray();
string Decide(bool invalid, bool defaultMissing, bool ledgerIncomplete, bool cpuContractIncomplete) =>
    invalid ? precedence[0] : defaultMissing ? precedence[1] : ledgerIncomplete ? precedence[2]
    : cpuContractIncomplete ? precedence[3] : precedence[4];
var precedenceCases = new[]
{
    new { id = "invalid-dominates", actual = Decide(true, true, true, true), expected = precedence[0] },
    new { id = "default-dominates-lower", actual = Decide(false, true, true, true), expected = precedence[1] },
    new { id = "ledger-dominates-cpu", actual = Decide(false, false, true, true), expected = precedence[2] },
    new { id = "cpu-contract-incomplete", actual = Decide(false, false, false, true), expected = precedence[3] },
    new { id = "eligible-only-when-all-clear", actual = Decide(false, false, false, false), expected = precedence[4] },
}.Select(x => new { x.id, x.actual, x.expected, passed = x.actual == x.expected }).ToArray();

bool inputsValid = precursorSchemasValid && precedenceCases.All(x => x.passed);
string verdict = Decide(!inputsValid, productionDefaultOrImplementationMissing, !operationLedgerComplete, !cpuMeasurementContractComplete);
bool benchmarkEligible = verdict == "throughput-benchmark-eligible" && workloadDefinitionComplete;

var result = new
{
    phaseId = "phase511-phase481-throughput-benchmark-eligibility-audit",
    terminalStatus = $"phase481-throughput-benchmark-eligibility-{verdict}",
    verdictKind = verdict,
    inputsValid,
    workloadDefinitionComplete,
    throughputBenchmarkEligible = benchmarkEligible,
    phase510CostEnvelopeResolved = false,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A15",
    contract = new
    {
        path = ContractPath,
        sha256 = Sha(File.ReadAllBytes(ContractPath)),
        frozenBeforePrecursorConsumption = true,
    },
    exactInputBindings = bindings,
    fieldAudit = new
    {
        requiredFieldCount = fieldAudit.Length,
        frozenFieldCount = fieldAudit.Count(x => x.status == "frozen"),
        missingFieldCount = fieldAudit.Count(x => x.status == "missing"),
        productionDefaultPresent,
        implementationFingerprintPresent,
        operationLedgerComplete,
        cpuMeasurementContractComplete,
        fields = fieldAudit,
    },
    historicalPhase452Evidence = new
    {
        exactSourceBindingRecognized = currentPhase452FactsRecognized,
        currentImplementationOnly = true,
        adoptedAsPhase481Default = false,
        isotropicHistoricalPath = true,
        leapfrogSteps = 12,
        forceCallsAcceptedTrajectory = 14,
        forceCallsRejectedTrajectory = 15,
        thetaMovesPerTrajectory = 2,
        thetaSweepEveryTrajectories = 50,
        warmupAdaptsStepAndThetaProposalScales = true,
        fiveColumnsRunInParallel = true,
        timingClock = "Stopwatch-wall-time",
        wallTimingAdmissibleAsAggregateProcessCpuEvidence = false,
    },
    benchmarkBoundary = new
    {
        benchmarkRun = false,
        fullT16OrT32MeshConstructed = false,
        fullT16OrT32AllocationPerformed = false,
        smallerExtentThroughputExtrapolationUsed = false,
        phase456WallTimingReused = false,
        directT16OrT32TimingDeferredToSeparatePhase = true,
        directTimingMayProceedOnlyAfterThisAuditCloses = true,
    },
    precedenceBattery = new
    {
        cases = precedenceCases,
        caseCount = precedenceCases.Length,
        passed = precedenceCases.All(x => x.passed),
    },
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    phase480Satisfied = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a15BoundaryHeld = true,
    councilRecommendationImplemented = true,
    decision = verdict == "production-default-or-implementation-missing"
        ? "Phase481 P1 has not emitted a production default or executable implementation fingerprint. The operation and CPU-measurement ledgers are also incomplete, so throughput timing refuses before any benchmark or T16/T32 allocation."
        : benchmarkEligible
            ? "Every benchmark-defining field is frozen. A separate phase may consider direct non-sampling throughput evidence; this audit itself authorizes no timing or run."
            : "The audit refuses at the first invalid or incomplete benchmark-definition requirement. No timing, pack, or run is authorized.",
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "phase481_throughput_benchmark_eligibility_audit.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "phase481_throughput_benchmark_eligibility_audit_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"inputs={inputsValid} frozen={result.fieldAudit.frozenFieldCount}/{result.fieldAudit.requiredFieldCount} eligible={benchmarkEligible} benchmark=False");

static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(File.ReadAllBytes(path)) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}

static bool JsonBool(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(property).GetBoolean();
}

static string JsonString(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(property).GetString() ?? string.Empty;
}

static bool JsonNestedBool(string path, string parent, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(parent).GetProperty(property).GetBoolean();
}

static string[] JsonStringArray(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(property).EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
}

static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed class Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
    public string ActualSha256 { get; } = actualSha256;
    public bool HashMatches { get; } = hashMatches;
}
