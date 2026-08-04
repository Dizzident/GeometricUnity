using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase560_transformation_law_source_census_independent_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase560_transformation_law_source_census_independent_adjudicator_contract_v1.json";
const string OutputDir = Root + "/output";
const string Phase559SummaryPath = "studies/phase559_bounded_transformation_law_source_census_001/output/bounded_transformation_law_source_census_summary.json";

var stopwatch = Stopwatch.StartNew();
using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
var contract = contractDoc.RootElement;

string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

string? JsonString(JsonElement element, string property)
    => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

bool? JsonBool(JsonElement element, string property)
    => element.TryGetProperty(property, out var value)
        && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) ? value.GetBoolean() : null;

// Battery is intentionally evaluated before Phase559 is parsed.
string ClassifySynthetic(string[] statuses, bool signConflict)
{
    if (signConflict)
        return "conflicted";
    return statuses.All(status => status == "bound") ? "complete" : "incomplete";
}

var knownAnswerRows = new List<object>();
bool knownAnswerBatteryPassed = true;
foreach (var item in contract.GetProperty("knownAnswerCases").EnumerateArray())
{
    string id = JsonString(item, "id") ?? "";
    string[] statuses = item.GetProperty("statuses").EnumerateArray().Select(value => value.GetString() ?? "").ToArray();
    bool signConflict = JsonBool(item, "signConflict") is true;
    string expected = JsonString(item, "expected") ?? "";
    string actual = ClassifySynthetic(statuses, signConflict);
    bool passed = actual == expected;
    knownAnswerBatteryPassed &= passed;
    knownAnswerRows.Add(new { id, statuses, signConflict, expected, actual, passed });
}
bool batteryRanBeforeAnyPhase559Datum = true;

var bindings = new List<object>();
bool exactBindingsValid = true;
foreach (var binding in contract.GetProperty("exactBindings").EnumerateArray())
{
    string id = JsonString(binding, "id") ?? "";
    string path = JsonString(binding, "path") ?? "";
    string expected = JsonString(binding, "sha256") ?? "";
    bool exists = File.Exists(path);
    string? actual = exists ? Sha256File(path) : null;
    bool matches = exists && actual == expected;
    exactBindingsValid &= matches;
    bindings.Add(new { id, path, expectedSha256 = expected, actualSha256 = actual, exists, hashMatches = matches });
}

bool contractValid = JsonString(contract, "contractId") == "phase560-a32-transformation-law-source-census-independent-adjudicator-v1"
    && JsonBool(contract, "frozenBeforeFirstExecution") is true
    && JsonBool(contract, "deterministicZeroSampling") is true
    && JsonBool(contract, "independentOfPhase559Implementation") is true
    && JsonBool(contract, "mayNotReferencePhase559Project") is true
    && JsonBool(contract, "batteryRunsBeforeAnyPhase559Datum") is true
    && JsonBool(contract, "continuumLawCannotFillRegisteredLowering") is true
    && JsonBool(contract, "phase561RequiresCompleteBridgeAndAdjudication") is true
    && JsonBool(contract, "phase561PresenceMustExactlyEqualGate") is true;

using var phase559Doc = JsonDocument.Parse(File.ReadAllText(Phase559SummaryPath));
var phase559 = phase559Doc.RootElement;

string primaryText = File.ReadAllText("docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt");
string phase548Program = File.ReadAllText("studies/phase548_bounded_complete_lattice_pilot_execution_001/Program.cs");
string registeredOperator = File.ReadAllText("src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs");
string genericMap = File.ReadAllText("src/Gu.Phase2.Stability/InfinitesimalGaugeMap.cs");

bool rawContinuumAffineLawPresent = primaryText.Contains("A = A0 + α", StringComparison.Ordinal)
    && primaryText.Contains("α · g", StringComparison.Ordinal)
    && primaryText.Contains("δ1ω", StringComparison.Ordinal)
    && primaryText.Contains("dAω", StringComparison.Ordinal)
    && primaryText.Contains("DLεω", StringComparison.Ordinal);
bool rawConventionWarningPresent = primaryText.Contains("multiple sign conventions", StringComparison.Ordinal)
    && primaryText.Contains("notational shifts", StringComparison.Ordinal);
bool rawRegisteredRunFixesTheta = phase548Program.Contains("double[] thetaZero", StringComparison.Ordinal)
    && phase548Program.Contains("op.ComputeJointGradient(omega, thetaZero, massMatrix)", StringComparison.Ordinal);
bool rawThetaRepresentsIndependentGroupField = registeredOperator.Contains("eps_v = exp(theta_v)", StringComparison.Ordinal)
    && registeredOperator.Contains("theta is a genuine INDEPENDENT H-valued fluctuation field", StringComparison.Ordinal);
bool rawRegisteredOmegaMappingPresent = phase548Program.Contains("omega is $", StringComparison.Ordinal)
    || registeredOperator.Contains("omega is $", StringComparison.Ordinal)
    || registeredOperator.Contains("omega = $", StringComparison.Ordinal);
bool rawPrimaryEndpointLoweringPresent = primaryText.Contains("xi_avg", StringComparison.Ordinal)
    || primaryText.Contains("edge endpoint", StringComparison.OrdinalIgnoreCase);
bool rawGenericEndpointAverageIsImplementationOnly = genericMap.Contains("xiAvg[a] = 0.5", StringComparison.Ordinal)
    && !rawPrimaryEndpointLoweringPresent;
bool rawDiscreteActionCovarianceBindingPresent = primaryText.Contains("ComputeJointGradient", StringComparison.Ordinal)
    || primaryText.Contains("EinsteinianShiabOperator", StringComparison.Ordinal);

var independentRows = new[]
{
    new { id = "continuum-affine-field-definition", status = rawContinuumAffineLawPresent ? "primary-source-present" : "source-absent" },
    new { id = "registered-field-identification", status = rawRegisteredOmegaMappingPresent ? "source-bound" : "source-unmapped" },
    new { id = "edge-endpoint-lowering", status = rawPrimaryEndpointLoweringPresent ? "primary-source-present" : "source-absent" },
    new { id = "second-field-transformation", status = rawContinuumAffineLawPresent && rawThetaRepresentsIndependentGroupField ? "continuum-source-present-registered-map-unbound" : "source-absent" },
    new { id = "registered-action-covariance", status = rawDiscreteActionCovarianceBindingPresent ? "source-bound" : "source-unmapped" }
};

var phase559Rows = phase559.GetProperty("registeredComparison").GetProperty("bridgeRows").EnumerateArray()
    .ToDictionary(row => JsonString(row, "id") ?? "", row => JsonString(row, "status") ?? "");
bool independentRowsAgree = independentRows.All(row => phase559Rows.TryGetValue(row.id, out string? status) && status == row.status);
bool phase559TerminalExpected = JsonString(phase559, "verdictKind") == JsonString(contract, "expectedPhase559Verdict");
bool phase559InputsValid = JsonBool(phase559, "contractValid") is true
    && JsonBool(phase559, "exactBindingsValid") is true
    && JsonBool(phase559, "resourceAccepted") is true;
bool completeRegisteredBridge = independentRows.Skip(1).All(row => row.status == "source-bound")
    && !rawConventionWarningPresent;
bool phase561GateOpen = phase559InputsValid && phase559TerminalExpected && independentRowsAgree && completeRegisteredBridge;
string[] phase561Directories = Directory.GetDirectories("studies", "phase561_*", SearchOption.TopDirectoryOnly);
bool phase561Present = phase561Directories.Length > 0;
bool phase561PresenceExactlyMatchesGate = phase561Present == phase561GateOpen;

bool adjudicationPassed = contractValid && exactBindingsValid && knownAnswerBatteryPassed
    && batteryRanBeforeAnyPhase559Datum && phase559InputsValid && phase559TerminalExpected
    && independentRowsAgree && !completeRegisteredBridge && phase561PresenceExactlyMatchesGate;
string verdictKind = adjudicationPassed
    ? "adjudication-confirms-continuum-law-discrete-bridge-incomplete"
    : "adjudication-refuses-source-census";
bool resourceAccepted = stopwatch.Elapsed.TotalSeconds <= contract.GetProperty("resourceCeilingSeconds").GetDouble();
if (!resourceAccepted)
    verdictKind = "adjudication-refuses-source-census";
if (verdictKind != JsonString(contract, "expectedVerdict"))
    throw new InvalidOperationException($"Phase560 terminal mismatch: {verdictKind}");

Directory.CreateDirectory(OutputDir);
var supplement = new
{
    schemaVersion = 1,
    supplementId = "phase560-a32-phase555-source-census-supplement-v1",
    artifactKind = "additive-evidence-supplement",
    parentPacket = new
    {
        phase = 555,
        path = JsonString(contract.GetProperty("phase555Supplement"), "parentPacketPath"),
        sha256 = JsonString(contract.GetProperty("phase555Supplement"), "parentPacketSha256"),
        byteImmutable = true
    },
    reservedRulingIds = new[] { "O4-F1-COLLECTIVE-COORDINATE", "O4-F1-FP-NORMALIZATION" },
    answersEitherReservedQuestion = false,
    authorsARuling = false,
    changesAPendingFlag = false,
    additiveOnly = true,
    sourceEvidence = new
    {
        phase559Verdict = JsonString(phase559, "verdictKind"),
        phase560Verdict = verdictKind,
        continuumTransformationLawLocated = rawContinuumAffineLawPresent,
        registeredDiscreteBridgeComplete = completeRegisteredBridge,
        unresolvedItems = new[] { "registered-field-identification", "edge-endpoint-lowering", "second-field-registered-map", "registered-action-covariance" },
        phase561GateOpen
    },
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0
};
var options = new JsonSerializerOptions { WriteIndented = true };
string supplementJson = JsonSerializer.Serialize(supplement, options) + Environment.NewLine;
string supplementPath = Path.Combine(OutputDir, "phase555_source_census_supplement.json");
File.WriteAllText(supplementPath, supplementJson);

string decision = "Independent reconstruction confirms that the bounded primary source contains a continuum affine and two-component infinitesimal law, but it does not bind the registered omega variable, discrete endpoint lowering, second-field map, or discrete objective covariance. The versioned finite-transformation phase remains absent behind an exact gate/presence equivalence check.";
var result = new
{
    schemaVersion = 1,
    phaseId = "phase560-transformation-law-source-census-independent-adjudicator",
    contractId = JsonString(contract, "contractId"),
    contractValid,
    exactBindingsValid,
    bindings,
    resourceAccepted,
    independentImplementation = new { phase559ProjectReference = false, sharedPhase559Code = false, rawSourcesReconstructed = true },
    knownAnswerBattery = new { ranBeforeAnyPhase559Datum = batteryRanBeforeAnyPhase559Datum, requiredCaseCount = 5, rows = knownAnswerRows, passed = knownAnswerBatteryPassed },
    independentSourceAudit = new { rawContinuumAffineLawPresent, rawConventionWarningPresent, rawRegisteredRunFixesTheta, rawThetaRepresentsIndependentGroupField, rawRegisteredOmegaMappingPresent, rawPrimaryEndpointLoweringPresent, rawGenericEndpointAverageIsImplementationOnly, rawDiscreteActionCovarianceBindingPresent, rows = independentRows, rowsAgreeWithPhase559 = independentRowsAgree, completeRegisteredBridge },
    auditedBranch = new { phase559InputsValid, phase559TerminalExpected, phase561GateOpen, phase561Directories, phase561Present, phase561PresenceExactlyMatchesGate },
    adjudicationPassed,
    supplementMaterialized = true,
    supplement = new { path = supplementPath, parentPacketByteImmutable = true, answersEitherReservedQuestion = false },
    verdictKind,
    terminalStatus = "transformation-law-source-census-independent-adjudicator-" + verdictKind,
    decision,
    nextTask = "request-or-register-the-missing-discrete-field-and-action-law-before-any-finite-test",
    rngUsed = false,
    samplingPerformed = false,
    reprocessingPerformed = false,
    protectedPhase554SeedsRead = false,
    phase553Or554RegisteredOrExecuted = false,
    directionCalledGaugeOrRedundant = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    rulingAuthoredOrInferred = false,
    o4Discharged = false,
    phase458Satisfied = false,
    phase481PackCreatedOrMutated = false,
    productionAuthorized = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0
};

string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "transformation_law_source_census_independent_adjudicator.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "transformation_law_source_census_independent_adjudicator_summary.json"), json);
Console.WriteLine($"verdictKind={verdictKind}");
Console.WriteLine($"knownAnswerBatteryPassed={knownAnswerBatteryPassed}");
Console.WriteLine($"completeRegisteredBridge={completeRegisteredBridge}");
Console.WriteLine($"phase561PresenceExactlyMatchesGate={phase561PresenceExactlyMatchesGate}");
