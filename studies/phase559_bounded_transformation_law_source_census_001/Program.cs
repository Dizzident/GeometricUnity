using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase559_bounded_transformation_law_source_census_001";
const string ContractPath = Root + "/preregistration/phase559_bounded_transformation_law_source_census_contract_v1.json";
const string OutputDir = Root + "/output";

var stopwatch = Stopwatch.StartNew();
using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
var contract = contractDoc.RootElement;

string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

string? JsonString(JsonElement element, string property)
    => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

bool? JsonBool(JsonElement element, string property)
    => element.TryGetProperty(property, out var value)
        && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : null;

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

bool contractValid = JsonString(contract, "contractId") == "phase559-a32-bounded-transformation-law-source-census-v1"
    && JsonBool(contract, "frozenBeforeFirstExecution") is true
    && JsonBool(contract, "candidateLocatedBeforeDesign") is true
    && JsonBool(contract, "deterministicZeroSampling") is true
    && JsonBool(contract, "completeBridgeRequiresEveryRegisteredRow") is true
    && JsonBool(contract, "continuumFormulaIsNotDiscreteLowering") is true
    && JsonBool(contract, "fixedSecondFieldVariationMayNotBeAssumed") is true
    && JsonBool(contract, "codeCommentsMayNotFillPrimarySourceRows") is true
    && JsonBool(contract, "signOrEndpointTuningProhibited") is true;

var corpus = contract.GetProperty("boundedCorpus");
string corpusRoot = JsonString(corpus, "root") ?? "";
string[] actualCorpusFiles = Directory.Exists(corpusRoot)
    ? Directory.GetFiles(corpusRoot, "*", SearchOption.TopDirectoryOnly)
        .Select(Path.GetFileName).Where(name => name is not null).Cast<string>().Order().ToArray()
    : [];
string[] expectedCorpusFiles = corpus.GetProperty("requiredRelativeFiles").EnumerateArray()
    .Select(value => value.GetString() ?? "").Order().ToArray();
bool boundedCorpusValid = actualCorpusFiles.SequenceEqual(expectedCorpusFiles)
    && actualCorpusFiles.Length == corpus.GetProperty("requiredFileCount").GetInt32();

string primaryPath = Path.Combine(corpusRoot, expectedCorpusFiles.Single());
string primaryText = File.ReadAllText(primaryPath);
string[] primaryLines = File.ReadAllLines(primaryPath);
bool primaryPdfHashRecorded = primaryText.Contains(
    JsonString(corpus, "primaryPdfSha256") ?? "missing", StringComparison.OrdinalIgnoreCase);

var anchorRows = new List<object>();
bool allPrimaryAnchorsPresent = true;
foreach (var anchor in contract.GetProperty("requiredPrimaryAnchors").EnumerateArray())
{
    string id = JsonString(anchor, "id") ?? "";
    string[] tokens = anchor.GetProperty("tokens").EnumerateArray().Select(v => v.GetString() ?? "").ToArray();
    var hitLines = new SortedSet<int>();
    foreach (string token in tokens)
        for (int index = 0; index < primaryLines.Length; index++)
            if (primaryLines[index].Contains(token, StringComparison.Ordinal))
                hitLines.Add(index + 1);
    bool present = tokens.All(token => primaryText.Contains(token, StringComparison.Ordinal));
    allPrimaryAnchorsPresent &= present;
    anchorRows.Add(new { id, tokens, present, lineNumbers = hitLines.Take(12).ToArray() });
}

string phase548Program = File.ReadAllText("studies/phase548_bounded_complete_lattice_pilot_execution_001/Program.cs");
string registeredOperator = File.ReadAllText("src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs");
string genericMap = File.ReadAllText("src/Gu.Phase2.Stability/InfinitesimalGaugeMap.cs");

bool primaryDefinesAffineConnectionCoordinates = primaryText.Contains("A = A0 + α", StringComparison.Ordinal)
    && primaryText.Contains("α · g", StringComparison.Ordinal);
bool primaryDefinesTwoComponentInfinitesimalLaw = primaryText.Contains("δ1ω", StringComparison.Ordinal)
    && primaryText.Contains("dAω", StringComparison.Ordinal)
    && primaryText.Contains("DLεω", StringComparison.Ordinal);
bool primaryWarnsOfConventionConflict = primaryText.Contains("multiple sign conventions", StringComparison.Ordinal)
    && primaryText.Contains("notational shifts", StringComparison.Ordinal);
bool primaryDefinesSquaredResidualAction = primaryText.Contains("I2B", StringComparison.Ordinal)
    && primaryText.Contains("||Υ", StringComparison.Ordinal);
bool registeredRunFixesSecondField = phase548Program.Contains("double[] thetaZero", StringComparison.Ordinal)
    && phase548Program.Contains("op.ComputeJointGradient(omega, thetaZero, massMatrix)", StringComparison.Ordinal);
bool registeredOperatorIdentifiesThetaWithGroupField = registeredOperator.Contains("eps_v = exp(theta_v)", StringComparison.Ordinal)
    && registeredOperator.Contains("theta is a genuine INDEPENDENT H-valued fluctuation field", StringComparison.Ordinal);
bool registeredSourcesIdentifyOmegaAsPrimaryAffineTranslation = phase548Program.Contains("omega is $", StringComparison.Ordinal)
    || registeredOperator.Contains("omega is $", StringComparison.Ordinal)
    || registeredOperator.Contains("omega = $", StringComparison.Ordinal);
bool primaryDefinesDiscreteEndpointAverage = primaryText.Contains("xi_avg", StringComparison.Ordinal)
    || primaryText.Contains("edge endpoint", StringComparison.OrdinalIgnoreCase);
bool primaryBindsRegisteredObjectiveCovariance = primaryText.Contains("ComputeJointGradient", StringComparison.Ordinal)
    || primaryText.Contains("EinsteinianShiabOperator", StringComparison.Ordinal);
bool genericMapUsesEndpointAverage = genericMap.Contains("xiAvg[a] = 0.5", StringComparison.Ordinal);

var bridgeRows = new[]
{
    new { id = "continuum-affine-field-definition", status = primaryDefinesAffineConnectionCoordinates && primaryDefinesTwoComponentInfinitesimalLaw ? "primary-source-present" : "source-absent", decisive = false },
    new { id = "registered-field-identification", status = registeredSourcesIdentifyOmegaAsPrimaryAffineTranslation ? "source-bound" : "source-unmapped", decisive = true },
    new { id = "edge-endpoint-lowering", status = primaryDefinesDiscreteEndpointAverage ? "primary-source-present" : "source-absent", decisive = true },
    new { id = "second-field-transformation", status = primaryDefinesTwoComponentInfinitesimalLaw && registeredOperatorIdentifiesThetaWithGroupField ? "continuum-source-present-registered-map-unbound" : "source-absent", decisive = true },
    new { id = "registered-action-covariance", status = primaryBindsRegisteredObjectiveCovariance ? "source-bound" : "source-unmapped", decisive = true }
};

var expectedStatuses = contract.GetProperty("registeredBridgeRows").EnumerateArray()
    .ToDictionary(row => JsonString(row, "id") ?? "", row => JsonString(row, "expectedStatus") ?? "");
bool bridgeStatusesMatchFrozen = bridgeRows.All(row => expectedStatuses.TryGetValue(row.id, out string? expected) && expected == row.status);
bool everyRegisteredBridgeRowSourceBound = bridgeRows.Where(row => row.decisive).All(row => row.status == "source-bound");
bool fullRegisteredSourceBridge = allPrimaryAnchorsPresent && bridgeStatusesMatchFrozen && everyRegisteredBridgeRowSourceBound;
bool continuumLawLocated = primaryDefinesAffineConnectionCoordinates && primaryDefinesTwoComponentInfinitesimalLaw
    && primaryDefinesSquaredResidualAction;
bool fixedSecondComponentSilentlyPreserved = false;
bool signOrEndpointTuned = false;
bool phase561GateOpen = fullRegisteredSourceBridge && !primaryWarnsOfConventionConflict;

string verdictKind;
if (!contractValid || !exactBindingsValid)
    verdictKind = "input-integrity-invalid";
else if (!boundedCorpusValid || !primaryPdfHashRecorded)
    verdictKind = "bounded-corpus-invalid";
else if (!allPrimaryAnchorsPresent)
    verdictKind = "required-primary-anchor-missing";
else if (!bridgeStatusesMatchFrozen)
    verdictKind = "primary-source-law-contradictory";
else if (!fullRegisteredSourceBridge)
    verdictKind = "bounded-source-census-finds-continuum-law-discrete-bridge-incomplete";
else
    verdictKind = "registered-transformation-source-bridge-complete";

bool resourceAccepted = stopwatch.Elapsed.TotalSeconds <= contract.GetProperty("resourceCeilingSeconds").GetDouble();
if (!resourceAccepted)
    verdictKind = "input-integrity-invalid";
bool expectedTerminalMatched = verdictKind == JsonString(contract, "expectedTerminal");
if (!expectedTerminalMatched)
    throw new InvalidOperationException($"Phase559 terminal mismatch: {verdictKind}");

string decision = "The bounded primary source contains a continuum affine-space action and a two-component infinitesimal law, but the registered Phase548 lowering remains incomplete: omega is not source-identified with the draft affine translation, no edge-endpoint lowering is supplied, the second-field law is not incorporated into the generic registered map, and covariance of the discrete objective is not source-bound. Phase561 remains closed.";

var result = new
{
    schemaVersion = 1,
    phaseId = "phase559-bounded-transformation-law-source-census",
    contractId = JsonString(contract, "contractId"),
    contractValid,
    exactBindingsValid,
    bindings,
    resourceAccepted,
    boundedCorpus = new { root = corpusRoot, expectedFiles = expectedCorpusFiles, actualFiles = actualCorpusFiles, boundedCorpusValid, primaryPdfHashRecorded },
    primaryEvidence = new { anchorRows, allPrimaryAnchorsPresent, primaryDefinesAffineConnectionCoordinates, primaryDefinesTwoComponentInfinitesimalLaw, primaryWarnsOfConventionConflict, primaryDefinesSquaredResidualAction, continuumLawLocated },
    registeredComparison = new { registeredRunFixesSecondField, registeredOperatorIdentifiesThetaWithGroupField, registeredSourcesIdentifyOmegaAsPrimaryAffineTranslation, primaryDefinesDiscreteEndpointAverage, genericMapUsesEndpointAverage, primaryBindsRegisteredObjectiveCovariance, bridgeRows, bridgeStatusesMatchFrozen, everyRegisteredBridgeRowSourceBound, fullRegisteredSourceBridge, fixedSecondComponentSilentlyPreserved, signOrEndpointTuned },
    verdictKind,
    terminalStatus = "bounded-transformation-law-source-census-" + verdictKind,
    decision,
    nextTask = "independent-source-census-adjudication",
    phase561GateOpen,
    phase561RegisteredOrExecuted = false,
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

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "bounded_transformation_law_source_census.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "bounded_transformation_law_source_census_summary.json"), json);
Console.WriteLine($"verdictKind={verdictKind}");
Console.WriteLine($"continuumLawLocated={continuumLawLocated}");
Console.WriteLine($"fullRegisteredSourceBridge={fullRegisteredSourceBridge}");
Console.WriteLine($"phase561GateOpen={phase561GateOpen}");
