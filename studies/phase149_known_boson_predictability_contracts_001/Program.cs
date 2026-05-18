using System.Text.Json;

const string DefaultOutputDir = "studies/phase149_known_boson_predictability_contracts_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase52Path = "studies/phase52_electroweak_absolute_scale_audit_001/absolute_scale_audit.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase72Path = "studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";
const string Phase183Path = "studies/phase183_massless_sector_invariant_prediction_001/output/massless_sector_invariant_prediction.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE149_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase52 = JsonDocument.Parse(File.ReadAllText(Phase52Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase72 = JsonDocument.Parse(File.ReadAllText(Phase72Path));
using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));
using var phase177 = File.Exists(Phase177Path) ? JsonDocument.Parse(File.ReadAllText(Phase177Path)) : null;
using var phase183 = File.Exists(Phase183Path) ? JsonDocument.Parse(File.ReadAllText(Phase183Path)) : null;

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray()
    .Select(row => row.Clone())
    .ToArray();
bool absoluteScaleValidated = string.Equals(JsonString(phase72.RootElement, "status"), "validated", StringComparison.Ordinal);
bool scalarBridgeDerived = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
bool correctedOperatorProjectionCandidate = string.Equals(
    JsonString(phase122.RootElement, "terminalStatus"),
    "corrected-operator-selection-rule-sweep-found-projection-candidate",
    StringComparison.Ordinal);
bool fermionSectorEvidencePresent = JsonBool(phase146.RootElement, "currentEvidencePresent") is true;
bool masslessSectorInvariantPredictionReady = phase183 is not null
    && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true;
var masslessBenchmarkContracts = phase177 is null
    ? new Dictionary<string, JsonElement>(StringComparer.Ordinal)
    : phase177.RootElement.GetProperty("contracts").EnumerateArray()
        .Where(contract => JsonBool(contract, "benchmarkContractPresent") is true)
        .ToDictionary(contract => RequiredString(contract, "observableId"), contract => contract.Clone(), StringComparer.Ordinal);

var contracts = rows.Select(row =>
{
    string particleId = RequiredString(row, "particleId");
    string observableId = RequiredString(row, "observableId");
    string status = RequiredString(row, "status");
    return BuildContract(particleId, observableId, status);
}).ToArray();

bool allContractsReady = contracts.All(contract => contract.PredictabilityReady);
string terminalStatus = allContractsReady
    ? "known-boson-predictability-contracts-ready"
    : "known-boson-predictability-contracts-open";

var result = new
{
    phaseId = "phase149-known-boson-predictability-contracts",
    terminalStatus,
    allContractsReady,
    contractCount = contracts.Length,
    readyContractCount = contracts.Count(contract => contract.PredictabilityReady),
    openContractCount = contracts.Count(contract => !contract.PredictabilityReady),
    contracts,
    sharedReadiness = new
    {
        absoluteScaleValidated,
        scalarBridgeDerived,
        correctedOperatorProjectionCandidate,
        fermionSectorEvidencePresent,
        masslessBenchmarkContractCount = masslessBenchmarkContracts.Count,
        masslessSectorInvariantPredictionReady,
        phase52Status = JsonString(phase52.RootElement, "terminalStatus"),
        phase70Status = JsonString(phase70.RootElement, "terminalStatus"),
        phase72Status = JsonString(phase72.RootElement, "status"),
        phase122Status = JsonString(phase122.RootElement, "terminalStatus"),
        phase146Status = JsonString(phase146.RootElement, "terminalStatus"),
    },
    nextExecutionOrder = contracts
        .Where(contract => !contract.PredictabilityReady)
        .OrderBy(contract => contract.Priority)
        .Select(contract => new
        {
            contract.ParticleId,
            contract.ObservableId,
            contract.NextConcretePhase,
            contract.RequiredArtifacts,
        })
        .ToArray(),
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase52Path = Phase52Path,
        phase70Path = Phase70Path,
        phase72Path = Phase72Path,
        phase122Path = Phase122Path,
        phase146Path = Phase146Path,
        phase177Path = File.Exists(Phase177Path) ? Phase177Path : null,
        phase183Path = File.Exists(Phase183Path) ? Phase183Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "known_boson_predictability_contracts.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "known_boson_predictability_contracts_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.allContractsReady,
        result.contractCount,
        result.readyContractCount,
        result.openContractCount,
        result.sharedReadiness,
        result.nextExecutionOrder,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"readyContractCount={result.readyContractCount}");
Console.WriteLine($"openContractCount={result.openContractCount}");

PredictabilityContract BuildContract(string particleId, string observableId, string status)
{
    if (status == "predicted")
    {
        return new PredictabilityContract(
            ParticleId: particleId,
            ObservableId: observableId,
            CurrentStatus: status,
            PredictabilityReady: true,
            Priority: 0,
            NextConcretePhase: "none",
            RequiredArtifacts: Array.Empty<string>(),
            AcceptanceGates: new[] { "existing predicted comparison remains within promotion policy" },
            Blockers: Array.Empty<string>());
    }

    if (particleId is "w-boson" or "z-boson")
    {
        var blockers = new List<string>();
        if (!absoluteScaleValidated)
            blockers.Add("absolute mass-energy scale calibration is not validated");
        blockers.Add("existing absolute W/Z comparison fails and cannot be promoted");
        if (!correctedOperatorProjectionCandidate)
            blockers.Add("corrected-operator replay has no projection candidate");
        if (!fermionSectorEvidencePresent)
            blockers.Add("fermion-sector transition/label evidence is absent");

        return new PredictabilityContract(
            ParticleId: particleId,
            ObservableId: observableId,
            CurrentStatus: status,
            PredictabilityReady: false,
            Priority: 1,
            NextConcretePhase: "derive-target-independent-wz-bridge-revision-or-fermion-sector-transition-rule",
            RequiredArtifacts: new[]
            {
                "target-blind fermion-sector label table or nontrivial transition rule satisfying P140",
                "corrected W/Z operator sweep with projection candidate or documented bridge revision",
                "rerun absolute W/Z mass projection and target comparison with no W/Z target-fit calibration",
            },
            AcceptanceGates: new[]
            {
                "P140/P141/P142 ready",
                "P122 projection candidate found or W/Z bridge revision accepted",
                "absolute W/Z comparison passes sigma policy",
            },
            Blockers: blockers);
    }

    if (particleId == "higgs")
    {
        return new PredictabilityContract(
            ParticleId: particleId,
            ObservableId: observableId,
            CurrentStatus: status,
            PredictabilityReady: false,
            Priority: 2,
            NextConcretePhase: "derive-higgs-scalar-sector-source-and-identity",
            RequiredArtifacts: new[]
            {
                "scalar-sector source/operator producing a Higgs-like mode",
                "target-independent Higgs identity feature table",
                "scalar-sector branch/refinement/environment sidecars",
                "validated mapping and mass-energy calibration for physical-higgs-mass-gev",
            },
            AcceptanceGates: new[]
            {
                "Higgs mode residual and stability gates pass",
                "identity rule validates without Higgs target fit",
                "physical-higgs-mass-gev comparison is emitted with uncertainty",
            },
            Blockers: scalarBridgeDerived
                ? new[] { "scalar bridge exists, but no Higgs-like scalar source/operator or identity rule exists" }
                : new[] { "scalar bridge and Higgs-like scalar source/operator are missing" });
    }

    if (particleId == "photon")
    {
        bool benchmarkContractPresent = masslessBenchmarkContracts.ContainsKey(observableId);
        if (masslessSectorInvariantPredictionReady)
        {
            return new PredictabilityContract(
                ParticleId: particleId,
                ObservableId: observableId,
                CurrentStatus: status,
                PredictabilityReady: true,
                Priority: 0,
                NextConcretePhase: "none",
                RequiredArtifacts: new[] { "P183 protected-sector masslessness invariant prediction" },
                AcceptanceGates: new[] { "zero masslessness-indicator contract passes inside the protected massless gauge sector" },
                Blockers: Array.Empty<string>());
        }

        return new PredictabilityContract(
            ParticleId: particleId,
            ObservableId: observableId,
            CurrentStatus: status,
            PredictabilityReady: false,
            Priority: 3,
            NextConcretePhase: "derive-massless-u1-gauge-mode-contract",
            RequiredArtifacts: new[]
            {
                benchmarkContractPresent ? "photon masslessness benchmark contract present" : "photon masslessness target or upper-limit contract",
                "target-independent massless U(1) gauge identity rule",
                "computed masslessness observable",
                "branch/refinement/environment sidecars proving protected zero-mode stability",
            },
            AcceptanceGates: new[]
            {
                "massless U(1) identity validates without target fit",
                "computed masslessness observable satisfies target/upper-limit contract",
                "masslessness stability sidecars pass",
            },
            Blockers: benchmarkContractPresent
                ? new[] { "photon masslessness benchmark contract exists, but no U(1) massless-mode identity exists" }
                : new[] { "no active photon masslessness target contract or U(1) massless-mode identity exists" });
    }

    if (particleId == "gluon")
    {
        bool benchmarkContractPresent = masslessBenchmarkContracts.ContainsKey(observableId);
        if (masslessSectorInvariantPredictionReady)
        {
            return new PredictabilityContract(
                ParticleId: particleId,
                ObservableId: observableId,
                CurrentStatus: status,
                PredictabilityReady: true,
                Priority: 0,
                NextConcretePhase: "none",
                RequiredArtifacts: new[] { "P183 protected-sector masslessness invariant prediction" },
                AcceptanceGates: new[] { "zero masslessness-indicator contract passes inside the protected massless gauge sector" },
                Blockers: Array.Empty<string>());
        }

        return new PredictabilityContract(
            ParticleId: particleId,
            ObservableId: observableId,
            CurrentStatus: status,
            PredictabilityReady: false,
            Priority: 4,
            NextConcretePhase: "derive-color-octet-gauge-sector-contract",
            RequiredArtifacts: new[]
            {
                benchmarkContractPresent ? "gluon masslessness benchmark contract present" : "gluon masslessness or confinement-compatible benchmark contract",
                "target-independent color-octet gauge-sector identity rule",
                "computed color gauge-sector observables",
                "sidecars for color representation content, gauge leakage, and environment stability",
            },
            AcceptanceGates: new[]
            {
                "color-sector identity validates without target fit",
                "computed color observable satisfies the benchmark contract",
                "color representation and leakage sidecars pass",
            },
            Blockers: benchmarkContractPresent
                ? new[] { "gluon masslessness benchmark contract exists, but no color-octet gauge-sector identity exists" }
                : new[] { "no active gluon benchmark contract or color-octet gauge-sector identity exists" });
    }

    return new PredictabilityContract(
        ParticleId: particleId,
        ObservableId: observableId,
        CurrentStatus: status,
        PredictabilityReady: false,
        Priority: 99,
        NextConcretePhase: "define-predictability-contract",
        RequiredArtifacts: new[] { "unknown boson row requires a dedicated predictability contract" },
        AcceptanceGates: Array.Empty<string>(),
        Blockers: new[] { "no contract template matched this row" });
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record PredictabilityContract(
    string ParticleId,
    string ObservableId,
    string CurrentStatus,
    bool PredictabilityReady,
    int Priority,
    string NextConcretePhase,
    IReadOnlyList<string> RequiredArtifacts,
    IReadOnlyList<string> AcceptanceGates,
    IReadOnlyList<string> Blockers);
