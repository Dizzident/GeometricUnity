using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase578_consolidated_conditional_electroweak_sector_ledger_001";
const string ContractPath = Root + "/preregistration/phase578_consolidated_conditional_electroweak_sector_ledger_contract_v5.json";
const string OutputPath = Root + "/output/consolidated_conditional_electroweak_sector_ledger.json";
const string SummaryPath = Root + "/output/consolidated_conditional_electroweak_sector_ledger_summary.json";

using JsonDocument contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
string contractSha256 = Sha(ContractPath);
Binding[] specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input",
    "known-answer-battery-failed",
    "source-values-or-statuses-inconsistent",
    "ledger-shape-or-assumption-coverage-invalid",
    "conditional-electroweak-sector-ledger-complete-absolute-masses-blocked-source-content-absent",
];
bool exactBindingsValid = bindings.Length == 9
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 9
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 9
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 9
    && bindings.All(x => x.hashMatches);
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 578
    && contract.GetProperty("contractId").GetString() == "phase578-a42-consolidated-conditional-electroweak-sector-ledger-v5"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("zeroSampling").GetBoolean()
    && !contract.GetProperty("rngUsed").GetBoolean()
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && contract.GetProperty("comparisonPolicy").GetProperty("knownElectroweakMissCount").GetInt32() == 2
    && contract.GetProperty("comparisonPolicy").GetProperty("anchorReconstructionIsSeparateNonPromotionalEvidence").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

static double DerivedSinSquared(double tanSquared) => tanSquared / (1.0 + tanSquared);
static double DerivedTreeMassRatio(double sinSquared) => Math.Sqrt(1.0 - sinSquared);
static double ScaledMiss(double derived, double measured) => Math.Abs(derived - measured) / Math.Abs(measured);
string SelectTerminal(bool invalid, bool batteryFailed, bool sourceInconsistent, bool shapeInvalid)
{
    if (invalid) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (sourceInconsistent) return taxonomy[2];
    if (shapeInvalid) return taxonomy[3];
    return taxonomy[4];
}

double fixtureTan = 3.0 / 5.0;
double fixtureSin = DerivedSinSquared(fixtureTan);
double fixtureTreeRatio = DerivedTreeMassRatio(fixtureSin);
double fixtureObservedRatio = 80.377 / 91.1876;
var truthTable = new[]
{
    new { id = "invalid", actual = SelectTerminal(true, false, false, false), expected = taxonomy[0] },
    new { id = "battery", actual = SelectTerminal(false, true, false, false), expected = taxonomy[1] },
    new { id = "source", actual = SelectTerminal(false, false, true, false), expected = taxonomy[2] },
    new { id = "shape", actual = SelectTerminal(false, false, false, true), expected = taxonomy[3] },
    new { id = "complete", actual = SelectTerminal(false, false, false, false), expected = taxonomy[4] },
    new { id = "precedence", actual = SelectTerminal(true, true, true, true), expected = taxonomy[0] },
};
bool arithmeticFixturePassed = fixtureTan == 0.6
    && fixtureSin == 0.37499999999999994
    && fixtureTreeRatio == 0.7905694150420949
    && fixtureObservedRatio == 0.8814466001956406
    && ScaledMiss(fixtureTreeRatio, fixtureObservedRatio) == 0.10310004614389022;
bool truthTablePassed = truthTable.All(x => x.actual == x.expected)
    && taxonomy.All(t => truthTable.Any(x => x.actual == t));
byte[] checksumFixture = System.Text.Encoding.UTF8.GetBytes("{\"phase\":578,\"fixture\":\"checksum\"}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = Convert.ToHexString(SHA256.HashData(checksumFixture))
    != Convert.ToHexString(SHA256.HashData(tamperedFixture));
bool knownAnswerPassed = arithmeticFixturePassed && truthTablePassed && checksumTamperDetected;
var battery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    exactRatioArithmetic = new
    {
        tanSquared = fixtureTan,
        sinSquared = fixtureSin,
        treeMassRatio = fixtureTreeRatio,
        declaredComparisonRatio = fixtureObservedRatio,
        scaledMiss = ScaledMiss(fixtureTreeRatio, fixtureObservedRatio),
        passed = arithmeticFixturePassed,
    },
    classificationTruthTable = new { rows = truthTable, everyTerminalReached = taxonomy.All(t => truthTable.Any(x => x.actual == t)), passed = truthTablePassed },
    checksumTamperDetected,
    passed = knownAnswerPassed,
};
if (!contractValid || !knownAnswerPassed)
{
    string earlyTerminal = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(earlyTerminal, contractValid, exactBindingsValid, bindings, battery));
    Console.WriteLine($"Phase578 verdict: {earlyTerminal}");
    return;
}

// Audited numeric/status JSON is parsed only after contract, binding, and battery checks pass.
JsonElement p404 = ReadBinding("phase404-full");
JsonElement p429 = ReadBinding("phase429-full");
JsonElement p433 = ReadBinding("phase433-full");
JsonElement p434 = ReadBinding("phase434-full");
JsonElement p451 = ReadBinding("phase451-full");
JsonElement p461 = ReadBinding("phase461-stable-electroweak-anchor-projection");
JsonElement p464 = ReadBinding("phase464-full");
JsonElement p201Wz = ReadBinding("phase201-wz-intake-template");
JsonElement p201Higgs = ReadBinding("phase201-higgs-intake-template");

JsonElement LedgerRow(JsonElement source, string id)
    => source.GetProperty("ledger").EnumerateArray().Single(x => x.GetProperty("rowId").GetString() == id);
JsonElement ExtractionRow(string id)
    => p434.GetProperty("extractionRows").EnumerateArray().Single(x => x.GetProperty("rowId").GetString() == id);
string[] Assumptions(string rowId)
    => contract.GetProperty("requiredAssumptionIdsByRow").GetProperty(rowId).EnumerateArray().Select(x => x.GetString()!).ToArray();

JsonElement p429Tan = LedgerRow(p429, "tan-squared-theta-w-at-unification");
JsonElement p429Sin = LedgerRow(p429, "sin-squared-theta-w-at-unification");
JsonElement p429Ratio = LedgerRow(p429, "tree-level-w-z-mass-ratio-at-unification");
JsonElement p433Content = p433.GetProperty("betaLedger").EnumerateArray().Single(x =>
    x.GetProperty("familyCount").GetInt32() == 3 && x.GetProperty("higgsIncluded").GetBoolean());
JsonElement p451Predictions = p451.GetProperty("predictions");
JsonElement p451Comparison = p451.GetProperty("falsificationComparison");
JsonElement p451Imports = p451.GetProperty("comparisonImports");
double massWImport = p461.GetProperty("comparisonWindowsGeV").GetProperty("massW").GetDouble();
double massZImport = p461.GetProperty("comparisonWindowsGeV").GetProperty("massZ").GetDouble();
double measuredMassRatio = massWImport / massZImport;
double treeMassRatio = p429Ratio.GetProperty("value").GetDouble();
double treeSignedMiss = treeMassRatio - measuredMassRatio;
double treeScaledMiss = ScaledMiss(treeMassRatio, measuredMassRatio);
JsonElement reconstructedAnchor = p461.GetProperty("reconstructedRefereeReading");
string[] wzMissingFields = MissingWzFields(p201Wz);
string[] higgsMissingFields = MissingHiggsFields(p201Higgs);
int wzListCount = wzMissingFields.Length;
int higgsListCount = higgsMissingFields.Length;

bool sourcesConsistent =
    p404.GetProperty("terminalStatus").GetString() == "gu-embedding-chain-enumerated-ratio-menu-derived-adjoint-higgs-doublet-absent"
    && p404.GetProperty("standardTanSquaredEmb").GetDouble() == 0.6
    && Math.Abs(p404.GetProperty("standardSinSquaredEmb").GetDouble() - 0.375) < 1e-15
    && p429.GetProperty("terminalStatus").GetString() == "dimensionless-ratio-ledger-materialized-comparison-lineage-still-missing"
    && p429Tan.GetProperty("exactForm").GetString() == "3/5"
    && p429Tan.GetProperty("value").GetDouble() == p404.GetProperty("standardTanSquaredEmb").GetDouble()
    && p429Sin.GetProperty("exactForm").GetString() == "3/8"
    && Math.Abs(p429Sin.GetProperty("value").GetDouble() - 0.375) < 1e-15
    && p429Ratio.GetProperty("exactForm").GetString() == "sqrt(5/8)"
    && !p429Ratio.GetProperty("fixedByInternalStructure").GetBoolean()
    && p433.GetProperty("terminalStatus").GetString() == "blind-beta-coefficient-running-ledger-materialized-comparison-lineage-still-missing"
    && p433Content.GetProperty("b3").GetString() == "7"
    && p433Content.GetProperty("b2").GetString() == "19/6"
    && p433Content.GetProperty("b1").GetString() == "-41/10"
    && p433.GetProperty("conditionalHiggsContribution").GetProperty("recordedAsConditional").GetBoolean()
    && !p433.GetProperty("conditionalHiggsContribution").GetProperty("breakingSectorEstablished").GetBoolean()
    && p434.GetProperty("terminalStatus").GetString() == "conditional-observed-field-extraction-row-ledger-materialized-vev-and-lineage-still-missing"
    && p434.GetProperty("conditionallyDeterminedRowCount").GetInt32() == 6
    && p434.GetProperty("sourceDefinedFieldCount").GetInt32() == 0
    && p451.GetProperty("verdictKind").GetString() == "tension-persists-quantified"
    && p451Predictions.GetProperty("sin2MzPredictedOneLoop").GetDouble() == 0.20758851977298845
    && p451Predictions.GetProperty("sin2MzPredictedTwoLoop").GetDouble() == 0.21063705618289308
    && p451Imports.GetProperty("sin2ThetaWObservedMz").GetDouble() == 0.23122
    && !p451Imports.GetProperty("sin2ObservedUsedInPredictionComputation").GetBoolean()
    && p451Comparison.GetProperty("gapOverBand").GetDouble() == 115.1250020940215
    && p461.GetProperty("importCleanTrialsSurvivingHitCount").GetInt32() == 0
    && p461.GetProperty("verdictKind").GetString() == "declared-comparison-consistency-only"
    && reconstructedAnchor.GetProperty("killFactor").GetDouble() == 451.10527021047886
    && reconstructedAnchor.GetProperty("windowRole").GetString() == "referee-reconstruction-only"
    && reconstructedAnchor.GetProperty("windowBannedFromLiveVerdicts").GetBoolean()
    && p464.GetProperty("verdictKind").GetString() == "blocked-upstream-ambiguous"
    && p464.GetProperty("emittedSentence").GetString() == p464.GetProperty("blockedUpstreamSentence").GetString()
    && p464.GetProperty("rulingsCount").GetInt32() == 0
    && wzListCount == 15 && higgsListCount == 14;
if (!sourcesConsistent)
{
    Emit(Early(taxonomy[2], true, true, bindings, battery));
    Console.WriteLine($"Phase578 verdict: {taxonomy[2]}");
    return;
}

var rows = new List<Dictionary<string, object?>>
{
    Row("derived-embedding-tan-squared", "derived", new { decimalValue = p429Tan.GetProperty("value").GetDouble(), exactForm = "3/5", quantity = "tan^2(theta_W) at the unification point" },
        Assumptions("derived-embedding-tan-squared"), ["phase404-full", "phase429-full"]),
    Row("derived-embedding-sin-squared", "derived", new { decimalValue = p429Sin.GetProperty("value").GetDouble(), exactForm = "3/8", quantity = "sin^2(theta_W) at the unification point" },
        Assumptions("derived-embedding-sin-squared"), ["phase404-full", "phase429-full"]),
    Row("conditional-tree-w-z-mass-ratio", "conditional", new { decimalValue = treeMassRatio, exactForm = "sqrt(5/8)", quantity = "m_W/m_Z in the tree-level custodial limit" },
        Assumptions("conditional-tree-w-z-mass-ratio"), ["phase429-full", "phase461-stable-electroweak-anchor-projection"], new
        {
            comparisonOnly = true,
            derivedValue = treeMassRatio,
            measuredValue = measuredMassRatio,
            measuredInputs = new { massW = massWImport, massZ = massZImport, sourceUnits = "declared comparison imports only; no unit anchor adopted" },
            signedMiss = treeSignedMiss,
            absoluteMiss = Math.Abs(treeSignedMiss),
            scaledAbsoluteMiss = treeScaledMiss,
            withinComparison = false,
        }, knownElectroweakMiss: true),
    Row("conditional-running-content-template", "conditional", new
        {
            familyCount = 3,
            higgsIncluded = true,
            betaCoefficientsAfPositive = new { b3 = "7", b2 = "19/6", b1 = "-41/10" },
            conditionalHiggsContribution = new { deltaB3 = "0", deltaB2 = "-1/6", deltaB1 = "-1/10" },
        }, Assumptions("conditional-running-content-template"), ["phase433-full", "phase451-full"]),
    Row("conditional-running-weak-angle", "conditional", new
        {
            quantity = "sin^2(theta_W) at the declared low-energy comparison point",
            oneLoop = p451Predictions.GetProperty("sin2MzPredictedOneLoop").GetDouble(),
            twoLoop = p451Predictions.GetProperty("sin2MzPredictedTwoLoop").GetDouble(),
            honestBandTwoLoop = p451Comparison.GetProperty("thresholdBandTwoLoop").GetDouble(),
        }, Assumptions("conditional-running-weak-angle"), ["phase404-full", "phase429-full", "phase433-full", "phase451-full"], new
        {
            comparisonOnly = true,
            observed = p451Imports.GetProperty("sin2ThetaWObservedMz").GetDouble(),
            oneLoopSignedMiss = p451Comparison.GetProperty("gapOneLoop").GetDouble(),
            twoLoopSignedMiss = p451Comparison.GetProperty("gapTwoLoop").GetDouble(),
            twoLoopAbsoluteMiss = p451Comparison.GetProperty("gapTwoLoopAbs").GetDouble(),
            twoLoopHonestBand = p451Comparison.GetProperty("thresholdBandTwoLoop").GetDouble(),
            missOverHonestBand = p451Comparison.GetProperty("gapOverBand").GetDouble(),
            withinHonestBand = p451Comparison.GetProperty("twoLoopClosesWithinThresholdBand").GetBoolean(),
        }, knownElectroweakMiss: true),
    ExtractionLedgerRow("conditional-photon-extraction-template", "photon-eigenstate", ["phase404-full", "phase429-full", "phase434-full"]),
    ExtractionLedgerRow("conditional-z-extraction-template", "z-eigenstate", ["phase404-full", "phase429-full", "phase434-full"]),
    ExtractionLedgerRow("conditional-w-plus-extraction-template", "w-plus-eigenstate", ["phase434-full"]),
    ExtractionLedgerRow("conditional-w-minus-extraction-template", "w-minus-eigenstate", ["phase434-full"]),
    ExtractionLedgerRow("conditional-electric-charge-template", "electric-charge-relation", ["phase404-full", "phase429-full", "phase434-full"]),
    ExtractionLedgerRow("conditional-mass-squared-ratio-template", "w-z-mass-ratio", ["phase429-full", "phase434-full"]),
    Row("derived-anchor-menu-status", "derived", new
        {
            importCleanTrialsSurvivingAnchorCount = p461.GetProperty("importCleanTrialsSurvivingHitCount").GetInt32(),
            verdictKind = p461.GetProperty("verdictKind").GetString(),
            refereeReconstruction = new
            {
                killFactor = reconstructedAnchor.GetProperty("killFactor").GetDouble(),
                preregisteredBand = new { lo = reconstructedAnchor.GetProperty("killBandLo").GetDouble(), hi = reconstructedAnchor.GetProperty("killBandHi").GetDouble() },
                role = reconstructedAnchor.GetProperty("windowRole").GetString(),
                bannedFromLiveVerdicts = reconstructedAnchor.GetProperty("windowBannedFromLiveVerdicts").GetBoolean(),
                classification = "separate-non-promotional-anchor-evidence-not-an-electroweak-comparison-miss",
            },
        }, Assumptions("derived-anchor-menu-status"), ["phase461-stable-electroweak-anchor-projection"]),
    Row("derived-anchor-adjudication-status", "derived", new
        {
            verdictKind = p464.GetProperty("verdictKind").GetString(),
            pendingProseStatements = p464.GetProperty("nPending").GetInt32(),
            physicistRulings = p464.GetProperty("rulingsCount").GetInt32(),
            emittedSentence = p464.GetProperty("emittedSentence").GetString(),
            precommittedNoClaimSentence = p464.GetProperty("noClaimSentence").GetString(),
        }, Assumptions("derived-anchor-adjudication-status"), ["phase461-stable-electroweak-anchor-projection", "phase464-full"]),
    Row("derived-source-lineage-missing-field-counts", "derived", new
        {
            wzAbsoluteSourceLineageMissingFieldCount = wzListCount,
            higgsScalarSourceLineageMissingFieldCount = higgsListCount,
            countsMatchEnumeratedMissingFields = true,
        }, Assumptions("derived-source-lineage-missing-field-counts"), ["phase201-wz-intake-template", "phase201-higgs-intake-template"]),
};

string[] requiredRows = contract.GetProperty("requiredRows").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] knownMissRows = contract.GetProperty("comparisonPolicy").GetProperty("knownElectroweakMissRowIds").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] rowIds = rows.Select(x => (string)x["rowId"]!).ToArray();
bool rowShapeValid = rowIds.SequenceEqual(requiredRows, StringComparer.Ordinal)
    && rowIds.Distinct(StringComparer.Ordinal).Count() == rows.Count
    && rows.All(x => (string)x["classification"]! is "derived" or "conditional")
    && rows.All(x => !(bool)x["predictional"]!)
    && rows.All(x => ((string[])x["assumptions"]!).Length > 0)
    && rows.All(x => ((string[])x["sourceBindingIds"]!).Length > 0)
    && rows.All(x => ((Array)x["sourceBindings"]!).Length > 0)
    && rows.Where(x => (bool)x["knownElectroweakMiss"]!).Select(x => (string)x["rowId"]!).SequenceEqual(knownMissRows, StringComparer.Ordinal)
    && rows.Count(x => (bool)x["knownElectroweakMiss"]!) == 2
    && rows.Single(x => (string)x["rowId"]! == "derived-anchor-menu-status")["knownElectroweakMiss"] is false
    && rows.All(x =>
    {
        string id = (string)x["rowId"]!;
        string[] actual = (string[])x["assumptions"]!;
        return actual.SequenceEqual(Assumptions(id), StringComparer.Ordinal);
    });
if (!rowShapeValid)
{
    Emit(Early(taxonomy[3], true, true, bindings, battery));
    Console.WriteLine($"Phase578 verdict: {taxonomy[3]}");
    return;
}

var branches = new[]
{
    new { id = "derived-ratio-chain", promotedPhysicalMassClaimCount = 0 },
    new { id = "conditional-tree-ratio-and-comparison", promotedPhysicalMassClaimCount = 0 },
    new { id = "conditional-running-and-comparison", promotedPhysicalMassClaimCount = 0 },
    new { id = "conditional-extraction-templates", promotedPhysicalMassClaimCount = 0 },
    new { id = "anchor-menu-and-adjudication", promotedPhysicalMassClaimCount = 0 },
    new { id = "source-lineage-deficits", promotedPhysicalMassClaimCount = 0 },
};
string terminal = SelectTerminal(false, false, false, false);
var result = new
{
    schemaVersion = 1,
    phase = 578,
    phaseId = "phase578-consolidated-conditional-electroweak-sector-ledger",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256,
    contractValid,
    verdictKind = "conditional-ledger-complete-absolute-masses-blocked",
    terminalStatus = terminal,
    terminalStatement = contract.GetProperty("terminalStatement").GetString(),
    deterministic = true,
    zeroSampling = true,
    rngUsed = false,
    exactBindingCount = bindings.Length,
    exactBindingsValid,
    exactBindings = bindings,
    knownAnswerBattery = battery,
    sourceValuesAndStatusesConsistent = sourcesConsistent,
    ledgerRowCount = rows.Count,
    rowShapeAndAssumptionCoverageValid = rowShapeValid,
    ledger = rows,
    comparisonAccounting = new
    {
        knownElectroweakMissCount = rows.Count(x => (bool)x["knownElectroweakMiss"]!),
        knownElectroweakMissRowIds = knownMissRows,
        treeMassRatioMiss = new { derived = treeMassRatio, measured = measuredMassRatio, signed = treeSignedMiss, absolute = Math.Abs(treeSignedMiss), scaledAbsolute = treeScaledMiss },
        runningWeakAngleMiss = new { twoLoop = p451Predictions.GetProperty("sin2MzPredictedTwoLoop").GetDouble(), measured = p451Imports.GetProperty("sin2ThetaWObservedMz").GetDouble(), signed = p451Comparison.GetProperty("gapTwoLoop").GetDouble(), honestBandFactor = p451Comparison.GetProperty("gapOverBand").GetDouble() },
        anchorReconstruction = new { killFactor = reconstructedAnchor.GetProperty("killFactor").GetDouble(), classification = "separate-non-promotional-anchor-evidence", countedAsKnownElectroweakMiss = false },
    },
    missingSourceContent = new
    {
        unitAnchor = new { available = false, phase461ImportCleanTrialsSurvivingAnchorCount = 0, phase464Verdict = "blocked-upstream-ambiguous" },
        observedFieldExtractionMapOrTheorem = new { available = false, phase434SourceDefinedFieldCount = 0, conditionalTemplatesDoNotFillContract = true },
        quarticOrBreakingSectorContent = new { available = false, phase433BreakingSectorEstablished = false, phase434CandidateVevExistenceEstablished = false },
    },
    noUnitAnchorIntroduced = true,
    absoluteMassComputed = false,
    everyRowDerivedOrConditionalNeverPredictional = true,
    branches,
    everyBranchPromotedPhysicalMassClaimCountZero = branches.All(x => x.promotedPhysicalMassClaimCount == 0),
    sourceContractFieldFilledCount = 0,
    promotedPhysicalMassClaimCount = 0,
    externalReviewPending = true,
};
Emit(result);
Console.WriteLine($"Phase578 verdict: {terminal}");

Dictionary<string, object?> ExtractionLedgerRow(string ledgerId, string sourceRowId, string[] sourceBindings)
{
    JsonElement source = ExtractionRow(sourceRowId);
    object value = new
    {
        sourceRowId,
        field = source.GetProperty("field").GetString(),
        terms = source.GetProperty("terms").Clone(),
        category = source.GetProperty("category").GetString(),
        requiresVevAmplitude = source.GetProperty("requiresVevAmplitude").GetBoolean(),
    };
    return Row(ledgerId, "conditional", value, Assumptions(ledgerId), sourceBindings);
}

Dictionary<string, object?> Row(string rowId, string classification, object value, string[] assumptions,
    string[] sourceBindings, object? measuredComparison = null, bool knownElectroweakMiss = false)
    => new(StringComparer.Ordinal)
    {
        ["rowId"] = rowId,
        ["classification"] = classification,
        ["predictional"] = false,
        ["value"] = value,
        ["assumptions"] = assumptions,
        ["sourceBindingIds"] = sourceBindings,
        ["sourceBindings"] = sourceBindings.Select(id =>
        {
            Binding binding = specs.Single(x => x.Id == id);
            return new { id = binding.Id, path = binding.Path, sha256 = binding.Hash };
        }).ToArray(),
        ["measuredComparison"] = measuredComparison,
        ["knownElectroweakMiss"] = knownElectroweakMiss,
        ["promotedPhysicalMassClaimCount"] = 0,
    };

JsonElement ReadBinding(string id)
{
    Binding binding = specs.Single(x => x.Id == id);
    return JsonDocument.Parse(File.ReadAllBytes(binding.Path)).RootElement.Clone();
}

object Early(string terminalStatus, bool contractOk, bool bindingOk, object bindingRows, object knownAnswerBattery) => new
{
    schemaVersion = 1,
    phase = 578,
    phaseId = "phase578-consolidated-conditional-electroweak-sector-ledger",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256,
    verdictKind = terminalStatus,
    terminalStatus,
    deterministic = true,
    zeroSampling = true,
    rngUsed = false,
    contractValid = contractOk,
    exactBindingsValid = bindingOk,
    exactBindings = bindingRows,
    knownAnswerBattery,
    noUnitAnchorIntroduced = true,
    absoluteMassComputed = false,
    branches = new[] { new { id = "fail-closed", promotedPhysicalMassClaimCount = 0 } },
    everyBranchPromotedPhysicalMassClaimCountZero = true,
    sourceContractFieldFilledCount = 0,
    promotedPhysicalMassClaimCount = 0,
    externalReviewPending = true,
};

void Emit(object payload)
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    string json = JsonSerializer.Serialize(payload, options) + "\n";
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    File.WriteAllText(OutputPath, json);
    File.WriteAllText(SummaryPath, json);
}

static string Sha(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string[] MissingWzFields(JsonElement template)
{
    var missing = new List<string>();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        missing.Add("externalTargetValuesUsed=false");
    if (string.IsNullOrWhiteSpace(JsonString(template, "theoremOrDerivationId")))
        missing.Add("theoremOrDerivationId");
    if (string.IsNullOrWhiteSpace(JsonString(template, "sourceLineageId")))
        missing.Add("sourceLineageId");
    foreach (var row in template.GetProperty("particleRows").EnumerateArray())
    {
        string particleId = JsonString(row, "particleId") ?? "unknown";
        if (string.IsNullOrWhiteSpace(JsonString(row, "sourceRowId")))
            missing.Add($"{particleId}.sourceRowId");
        if (JsonBool(row, "rawAmplitudeGatePassed") is not true)
            missing.Add($"{particleId}.rawAmplitudeGatePassed=true");
        if (JsonBool(row, "commonBridgeGatePassed") is not true)
            missing.Add($"{particleId}.commonBridgeGatePassed=true");
        if (JsonBool(row, "targetComparisonGatePassed") is not true)
            missing.Add($"{particleId}.targetComparisonGatePassed=true");
        if (JsonBool(row, "stabilitySidecarsPresent") is not true)
            missing.Add($"{particleId}.stabilitySidecarsPresent=true");
        if (string.IsNullOrWhiteSpace(JsonString(row, "derivationId")))
            missing.Add($"{particleId}.derivationId");
    }
    return missing.ToArray();
}

static string[] MissingHiggsFields(JsonElement template)
{
    var missing = new List<string>();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        missing.Add("externalTargetValuesUsed=false");
    foreach (string key in new[] { "sourceLineageId", "scalarSourceOperatorId", "higgsIdentityEnvelopeId", "massiveScalarProfileId" })
        if (string.IsNullOrWhiteSpace(JsonString(template, key)))
            missing.Add(key);
    if (string.IsNullOrWhiteSpace(JsonString(template, "potentialOrSelfCouplingSourceId"))
        && string.IsNullOrWhiteSpace(JsonString(template, "excitationRelationId")))
        missing.Add("potentialOrSelfCouplingSourceId-or-excitationRelationId");
    JsonElement sidecars = template.GetProperty("stabilitySidecars");
    foreach (string key in new[] { "branch", "refinement", "environment", "representation", "coupling" })
        if (JsonBool(sidecars, key) is not true)
            missing.Add($"stabilitySidecars.{key}=true");
    JsonElement predictionRow = template.GetProperty("predictionRow");
    if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "sourceRowId")))
        missing.Add("predictionRow.sourceRowId");
    if (JsonBool(predictionRow, "targetComparisonGatePassed") is not true)
        missing.Add("predictionRow.targetComparisonGatePassed=true");
    if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "derivationId")))
        missing.Add("predictionRow.derivationId");
    return missing.ToArray();
}

static string? JsonString(JsonElement element, string propertyName)
    => element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName)
    => element.TryGetProperty(propertyName, out JsonElement property) ? property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    } : null;

sealed record Binding(string Id, string Path, string Hash);
