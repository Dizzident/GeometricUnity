using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase575_registered_fold_convention_readjudication_001";
const string ContractPath = Root + "/preregistration/phase575_registered_fold_convention_readjudication_contract_v1.json";
const string OutputPath = Root + "/output/registered_fold_convention_readjudication.json";
const string SummaryPath = Root + "/output/registered_fold_convention_readjudication_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
bool exactBindingsValid = bindings.Length == 25
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 25
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 25
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 25
    && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "known-answer-battery-failed", "a39-upstream-gate-refused",
    "side-consistency-not-reproduced", "classification-disagreement-present",
    "unexplained-rhat-difference-present", "threshold-margin-insufficient",
    "transition-or-replay-confirmation-missing",
    "evidence-independently-confirmed-under-registered-fold-convention",
];
string[] expectedFoldFlipKeys = contract.GetProperty("expectedFoldFlipKeys").EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement convention = contract.GetProperty("rowConvention");
double rhatThreshold = convention.GetProperty("rhatThreshold").GetDouble();
double minimumEss = convention.GetProperty("minimumEss").GetDouble();
double marginFactor = convention.GetProperty("marginSafetyFactor").GetDouble();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 575
    && contract.GetProperty("contractId").GetString() == "phase575-a39-registered-fold-convention-readjudication-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("newSamplingPerformed").GetBoolean() == false
    && contract.GetProperty("replayPerformed").GetBoolean() == false
    && contract.GetProperty("rngUsed").GetBoolean() == false
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && expectedFoldFlipKeys.Length == 5
    && rhatThreshold == 1.01 && minimumEss == 100.0 && marginFactor == 10.0
    && convention.GetProperty("essBitIdenticalRequired").GetBoolean()
    && convention.GetProperty("classificationAgreementRequired").GetBoolean()
    && convention.GetProperty("differenceMustBeInPhase574FoldFlipSet").GetBoolean()
    && convention.GetProperty("evaluatorSharedByBatteryAndProduction").GetBoolean()
    && contract.GetProperty("confirmationRule").GetProperty("expectedTransitionRowCount").GetInt32() == 96
    && contract.GetProperty("confirmationRule").GetProperty("expectedTableRowCount").GetInt32() == 36
    && contract.GetProperty("confirmationRule").GetProperty("expectedFoldFlipRowCount").GetInt32() == 5
    && contract.GetProperty("favorableTerminalAuthority").GetProperty("opensProspectiveChainPackPlanningGate").GetBoolean()
    && contract.GetProperty("favorableTerminalAuthority").GetProperty("samplingAuthorized").GetBoolean() == false
    && contract.GetProperty("favorableTerminalAuthority").GetProperty("executionAuthorized").GetBoolean() == false
    && contract.GetProperty("scope").GetProperty("phase572TerminalAndToleranceNeverRewritten").GetBoolean()
    && contract.GetProperty("scope").GetProperty("phase571ScopeUnchangedLocalMovementOnly").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

// The production row evaluator, shared verbatim with the battery below.
RowVerdict EvaluateRow(double rhatA, double rhatB, double bulkA, double bulkB, double tailA, double tailB, bool inFoldFlipSet)
{
    bool essBitIdentical = BitEqual(bulkA, bulkB) && BitEqual(tailA, tailB);
    bool classA = rhatA <= rhatThreshold && bulkA >= minimumEss && tailA >= minimumEss;
    bool classB = rhatB <= rhatThreshold && bulkB >= minimumEss && tailB >= minimumEss;
    bool classificationAgrees = classA == classB;
    double scaledDifference = ScaledAbsoluteDifference(rhatA, rhatB);
    bool explained = scaledDifference == 0.0 || inFoldFlipSet;
    bool marginSufficient = scaledDifference == 0.0
        || System.Math.Min(System.Math.Abs(rhatA - rhatThreshold), System.Math.Abs(rhatB - rhatThreshold))
            > marginFactor * scaledDifference;
    return new RowVerdict(essBitIdentical, classificationAgrees, explained, marginSufficient, scaledDifference,
        essBitIdentical && classificationAgrees && explained && marginSufficient);
}

string SelectTerminal(bool invalid, bool batteryFailed, bool gateRefused, bool sideConsistencyFailed,
    bool classificationDisagreement, bool unexplainedDifference, bool marginInsufficient, bool confirmationMissing)
{
    if (invalid) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (gateRefused) return taxonomy[2];
    if (sideConsistencyFailed) return taxonomy[3];
    if (classificationDisagreement) return taxonomy[4];
    if (unexplainedDifference) return taxonomy[5];
    if (marginInsufficient) return taxonomy[6];
    if (confirmationMissing) return taxonomy[7];
    return taxonomy[8];
}
var truthTable = new[]
{
    new { id = "invalid", actual = SelectTerminal(true, false, false, false, false, false, false, false), expected = taxonomy[0] },
    new { id = "battery", actual = SelectTerminal(false, true, false, false, false, false, false, false), expected = taxonomy[1] },
    new { id = "gate", actual = SelectTerminal(false, false, true, false, false, false, false, false), expected = taxonomy[2] },
    new { id = "side-consistency", actual = SelectTerminal(false, false, false, true, false, false, false, false), expected = taxonomy[3] },
    new { id = "classification", actual = SelectTerminal(false, false, false, false, true, false, false, false), expected = taxonomy[4] },
    new { id = "unexplained", actual = SelectTerminal(false, false, false, false, false, true, false, false), expected = taxonomy[5] },
    new { id = "margin", actual = SelectTerminal(false, false, false, false, false, false, true, false), expected = taxonomy[6] },
    new { id = "confirmation", actual = SelectTerminal(false, false, false, false, false, false, false, true), expected = taxonomy[7] },
    new { id = "confirmed", actual = SelectTerminal(false, false, false, false, false, false, false, false), expected = taxonomy[8] },
    new { id = "side-precedes-classification", actual = SelectTerminal(false, false, false, true, true, false, false, false), expected = taxonomy[3] },
    new { id = "early-precedence", actual = SelectTerminal(true, true, true, true, true, true, true, true), expected = taxonomy[0] },
};
bool truthTablePassed = truthTable.All(x => x.actual == x.expected)
    && expectedTaxonomy.All(terminal => truthTable.Any(x => x.actual == terminal));

// Battery rows exercise the shared evaluator before any audited numeric read.
var batteryRows = new[]
{
    new { id = "healthy-zero-diff", verdict = EvaluateRow(1.0002, 1.0002, 900.0, 900.0, 880.0, 880.0, false), expectedOk = true },
    new { id = "healthy-fold-flip", verdict = EvaluateRow(1.01750005, 1.01754383, 80.56, 80.56, 101.35, 101.35, true), expectedOk = true },
    new { id = "threshold-straddle", verdict = EvaluateRow(1.0099, 1.0101, 900.0, 900.0, 880.0, 880.0, true), expectedOk = false },
    new { id = "unexplained-diff", verdict = EvaluateRow(1.0002, 1.0003, 900.0, 900.0, 880.0, 880.0, false), expectedOk = false },
    new { id = "tight-margin", verdict = EvaluateRow(1.00999, 1.00998, 900.0, 900.0, 880.0, 880.0, true), expectedOk = false },
    new { id = "ess-mismatch", verdict = EvaluateRow(1.0002, 1.0002, 900.0, 901.0, 880.0, 880.0, false), expectedOk = false },
};
bool batteryRowsPassed = batteryRows.All(x => x.verdict.Ok == x.expectedOk)
    && batteryRows.Single(x => x.id == "threshold-straddle").verdict.ClassificationAgrees == false
    && batteryRows.Single(x => x.id == "unexplained-diff").verdict.Explained == false
    && batteryRows.Single(x => x.id == "tight-margin").verdict.MarginSufficient == false
    && batteryRows.Single(x => x.id == "ess-mismatch").verdict.EssBitIdentical == false;
byte[] checksumFixture = System.Text.Encoding.UTF8.GetBytes("{\"phase\":575,\"fixture\":\"checksum\"}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = Convert.ToHexString(SHA256.HashData(checksumFixture))
    != Convert.ToHexString(SHA256.HashData(tamperedFixture));
bool knownAnswerPassed = truthTablePassed && batteryRowsPassed && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    rehearsedOutsideRepositoryBeforeFreeze = true,
    evaluatorSharedWithProduction = true,
    rows = batteryRows.Select(x => new { x.id, x.expectedOk, observedOk = x.verdict.Ok, x.verdict.EssBitIdentical, x.verdict.ClassificationAgrees, x.verdict.Explained, x.verdict.MarginSufficient }),
    classificationTruthTable = new { rows = truthTable, everyTerminalReached = expectedTaxonomy.All(t => truthTable.Any(x => x.actual == t)), passed = truthTablePassed },
    checksumTamperDetected, passed = knownAnswerPassed,
};
if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase575 verdict: {early}");
    return;
}

// Only now parse the exact-bound upstream scientific records.
JsonElement p570 = ReadBinding("phase570-full");
JsonElement p571 = ReadBinding("phase571-full");
JsonElement p572 = ReadBinding("phase572-full");
JsonElement p573 = ReadBinding("phase573-full");
JsonElement p574 = ReadBinding("phase574-full");
JsonElement requiredVerdicts = contract.GetProperty("requiredUpstreamVerdicts");
bool upstreamGateOpen = p570.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase570").GetString()
    && p571.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase571").GetString()
    && p572.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase572").GetString()
    && p573.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase573").GetString()
    && p574.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase574").GetString()
    && p574.GetProperty("hFold").GetProperty("confirmed").GetBoolean()
    && p574.GetProperty("reproduction").GetProperty("passed").GetBoolean()
    && p574.GetProperty("attribution").GetProperty("rows").EnumerateArray()
        .Select(x => x.GetProperty("rowKey").GetString()!).Order(StringComparer.Ordinal)
        .SequenceEqual(expectedFoldFlipKeys.Order(StringComparer.Ordinal), StringComparer.Ordinal);
if (!upstreamGateOpen)
{
    Emit(Early(taxonomy[2], true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase575 verdict: {taxonomy[2]}");
    return;
}

// Side consistency: the shared kernel over each side's Phase574-retained raw
// traces must reproduce that side's committed diagnostics bit-for-bit.
JsonElement rawTraces = p574.GetProperty("rawTraces");
JsonElement phase548Contract = ReadBinding("phase548-contract");
var chainPlan = phase548Contract.GetProperty("seedTables").EnumerateArray().SelectMany(table =>
{
    string tableId = table.GetProperty("id").GetString()!;
    return table.GetProperty("seeds").EnumerateArray().Select(seed => new { TableId = tableId, ChainId = $"{tableId}-{seed.GetInt32()}" });
}).ToArray();
string[] tableIds = chainPlan.Select(x => x.TableId).Distinct().ToArray();
string[] seriesNames =
[
    "actionDensity", "forceNormSquared", "configurationNormSquared",
    "eNormSquared", "wNormSquared", "closedNormSquared", "closedPerpNormSquared",
    "closedGramLargest", "closedGramMiddle", "closedGramSmallest",
    "closedRankOneAlignment", "withinClosedRankOneDistanceSquared", "withinClosedRankOneRelativeDistance", "fullRankOneDistanceSquared",
    "eMovementSquared", "wMovementSquared", "closedMovementSquared", "closedPerpMovementSquared",
];
double[] TraceFor(string side, string chainId, string series) => rawTraces.GetProperty(side).GetProperty(chainId)
    .GetProperty(series).EnumerateArray().Select(x => x.GetDouble()).ToArray();
var committedA = p570.GetProperty("tableDiagnostics").GetProperty("rows").EnumerateArray()
    .ToDictionary(x => $"{x.GetProperty("table").GetString()}|{x.GetProperty("series").GetString()}", x => x, StringComparer.Ordinal);
var committedB = p572.GetProperty("directionalAdjudication").GetProperty("tableRows").EnumerateArray()
    .ToDictionary(x => $"{x.GetProperty("table").GetString()}|{x.GetProperty("series").GetString()}", x => x, StringComparer.Ordinal);

var rows = new List<object>();
bool sideConsistencyPassed = committedA.Count == 36 && committedB.Count == 36 && chainPlan.Length == 6;
bool anyClassificationDisagreement = false, anyUnexplainedDifference = false, anyMarginInsufficient = false;
int differingRowCount = 0;
foreach (string tableId in tableIds)
{
    string[] chainIds = chainPlan.Where(x => x.TableId == tableId).Select(x => x.ChainId).ToArray();
    foreach (string name in seriesNames)
    {
        string rowKey = $"{tableId}|{name}";
        bool inFoldFlipSet = expectedFoldFlipKeys.Contains(rowKey, StringComparer.Ordinal);
        Diagnostics sideA = Diagnose(chainIds.Select(chainId => TraceFor("sideA", chainId, name)).ToArray());
        Diagnostics sideB = Diagnose(chainIds.Select(chainId => TraceFor("sideB", chainId, name)).ToArray());
        JsonElement rowA = committedA[rowKey];
        JsonElement rowB = committedB[rowKey];
        bool sideAReproduces = BitEqual(sideA.Rhat, rowA.GetProperty("rhat").GetDouble())
            && BitEqual(sideA.BulkEss, rowA.GetProperty("bulkEss").GetDouble())
            && BitEqual(sideA.TailEss, rowA.GetProperty("tailEss").GetDouble());
        bool sideBReproduces = BitEqual(sideB.Rhat, rowB.GetProperty("rhat").GetDouble())
            && BitEqual(sideB.BulkEss, rowB.GetProperty("bulkEss").GetDouble())
            && BitEqual(sideB.TailEss, rowB.GetProperty("tailEss").GetDouble());
        sideConsistencyPassed &= sideAReproduces && sideBReproduces;
        RowVerdict verdict = EvaluateRow(sideA.Rhat, sideB.Rhat, sideA.BulkEss, sideB.BulkEss, sideA.TailEss, sideB.TailEss, inFoldFlipSet);
        if (verdict.ScaledRhatDifference > 0.0) differingRowCount++;
        anyClassificationDisagreement |= !verdict.ClassificationAgrees;
        anyUnexplainedDifference |= !verdict.Explained;
        anyMarginInsufficient |= !verdict.MarginSufficient;
        rows.Add(new
        {
            table = tableId, series = name, inFoldFlipSet,
            sideA = new { rhat = sideA.Rhat, bulkEss = sideA.BulkEss, tailEss = sideA.TailEss },
            sideB = new { rhat = sideB.Rhat, bulkEss = sideB.BulkEss, tailEss = sideB.TailEss },
            sideAReproducesCommitted = sideAReproduces, sideBReproducesCommitted = sideBReproduces,
            essBitIdentical = verdict.EssBitIdentical, classificationAgrees = verdict.ClassificationAgrees,
            explained = verdict.Explained, marginSufficient = verdict.MarginSufficient,
            scaledRhatDifference = verdict.ScaledRhatDifference, conventionSatisfied = verdict.Ok,
        });
    }
}

// Confirmation inputs: Phase572's committed replay and transition adjudication.
JsonElement p572Replay = p572.GetProperty("committedReplay");
JsonElement p572Transition = p572.GetProperty("transitionAdjudication");
bool confirmationPresent = p572Replay.GetProperty("agrees").GetBoolean()
    && p572Replay.GetProperty("everyFinalPositionBitIdentical").GetBoolean()
    && p572Replay.GetProperty("rows").GetArrayLength() == 6
    && p572Transition.GetProperty("agrees").GetBoolean()
    && p572Transition.GetProperty("rows").GetArrayLength() == 96
    && p572Transition.GetProperty("movementClassificationAgrees").GetBoolean()
    && p572Transition.GetProperty("movementClassification").GetProperty("supported").GetBoolean()
    && p572Transition.GetProperty("crossProposalStateCarry").GetBoolean() == false
    && p570.GetProperty("replay").GetProperty("bitIdentical").GetBoolean()
    && differingRowCount == 5;
string finalVerdict = SelectTerminal(false, false, false, !sideConsistencyPassed,
    anyClassificationDisagreement, anyUnexplainedDifference, anyMarginInsufficient, !confirmationPresent);
bool confirmed = finalVerdict == taxonomy[8];

var result = new
{
    schemaVersion = 1, phase = 575, phaseId = "phase575-registered-fold-convention-readjudication",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
    contractValid = true, exactBindingsValid = true, bindings, knownAnswerBattery,
    upstream = new
    {
        gatePassed = true,
        phase570Verdict = p570.GetProperty("verdictKind").GetString(),
        phase571Verdict = p571.GetProperty("verdictKind").GetString(),
        phase572Verdict = p572.GetProperty("verdictKind").GetString(),
        phase573Verdict = p573.GetProperty("verdictKind").GetString(),
        phase574Verdict = p574.GetProperty("verdictKind").GetString(),
        phase574HFoldConfirmed = true,
    },
    convention = new
    {
        rhatThreshold, minimumEss, marginSafetyFactor = marginFactor,
        scaledAbsoluteFormula = "abs(a-b)/max(1,abs(b))",
        registeredBeforeAuditedReads = true,
        inputIsPhase574RetainedRawTraces = true,
        replayPerformed = false, rngUsed = false,
    },
    adjudication = new
    {
        rowCount = rows.Count, differingRowCount,
        sideConsistencyPassed,
        classificationDisagreementPresent = anyClassificationDisagreement,
        unexplainedDifferencePresent = anyUnexplainedDifference,
        marginInsufficientPresent = anyMarginInsufficient,
        rows,
    },
    confirmation = new
    {
        phase570ReplayBitIdentical = p570.GetProperty("replay").GetProperty("bitIdentical").GetBoolean(),
        phase572ReplayAgreed = p572Replay.GetProperty("agrees").GetBoolean(),
        phase572TransitionRowCount = p572Transition.GetProperty("rows").GetArrayLength(),
        phase572TransitionAgreed = p572Transition.GetProperty("agrees").GetBoolean(),
        phase572MovementClassificationAgreed = p572Transition.GetProperty("movementClassificationAgrees").GetBoolean(),
        present = confirmationPresent,
    },
    verdictKind = finalVerdict,
    terminalStatus = "registered-fold-convention-readjudication-" + finalVerdict,
    directionalEvidenceIndependentlyConfirmedUnderRegisteredFoldConvention = confirmed,
    phase571LeverIndependentlyConfirmedUnderRegisteredFoldConvention = confirmed,
    phase571LeverScopeUnchangedLocalMovementOnly = true,
    prospectiveChainPackPlanningGateOpen = confirmed,
    prospectiveChainPackPlanningGateAuthorizesOnly = "registration and prospective freezing of a separate disjoint-seed chain-pack design phase",
    phase572TerminalAndToleranceNeverRewritten = true,
    newSamplingPerformed = false, replayPerformed = false, rngUsed = false, markovChainAdvanced = false,
    configurationsRetained = false, phase548Or549TerminalChanged = false,
    phase570Or571Or572Reinterpreted = false, phase572ToleranceRelaxed = false,
    registeredBlindSeedTouched = false, protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false,
    measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};
Emit(result);
Console.WriteLine($"Phase575 verdict: {finalVerdict}");
Console.WriteLine($"sideConsistencyPassed={sideConsistencyPassed}");
Console.WriteLine($"differingRowCount={differingRowCount}");
Console.WriteLine($"prospectiveChainPackPlanningGateOpen={confirmed}");
Console.WriteLine($"promotedPhysicalMassClaimCount=0");

// --- Shared estimator kernel (phase570 v7 verbatim port) ---
static Diagnostics Diagnose(double[][] chains)
{
    int n = chains.Min(x => x.Length);
    double[] pooled = chains.SelectMany(x => x.Take(n)).ToArray();
    if (pooled.Distinct().Count() <= 1) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    double[] ranked = RankNormalize(pooled), folded = RankNormalize(pooled.Select(x => System.Math.Abs(x - Median(pooled))).ToArray());
    double[][] r = Regroup(ranked, chains.Length, n), f = Regroup(folded, chains.Length, n);
    double[] ordered = pooled.Order().ToArray();
    double q05 = ordered[(int)System.Math.Floor(0.05 * (ordered.Length - 1))];
    double q95 = ordered[(int)System.Math.Ceiling(0.95 * (ordered.Length - 1))];
    double[][] lower = chains.Select(x => x.Take(n).Select(value => value <= q05 ? 1.0 : 0.0).ToArray()).ToArray();
    double[][] upper = chains.Select(x => x.Take(n).Select(value => value >= q95 ? 1.0 : 0.0).ToArray()).ToArray();
    return new Diagnostics(System.Math.Max(SplitRhat(r), SplitRhat(f)), Ess(Split(r)), System.Math.Min(Ess(Split(lower)), Ess(Split(upper))));
}
static double[] RankNormalize(double[] values)
{
    int n = values.Length; int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray(); var ranks = new double[n];
    for (int i = 0; i < n;) { int j = i; while (j + 1 < n && values[order[j + 1]] == values[order[i]]) j++; double rank = (i + j) / 2.0 + 1.0; for (int k = i; k <= j; k++) ranks[order[k]] = rank; i = j + 1; }
    return ranks.Select(x => InverseNormalCdf((x - 0.375) / (n + 0.25))).ToArray();
}
static double Median(double[] values) { double[] x = values.Order().ToArray(); return x.Length % 2 == 1 ? x[x.Length / 2] : 0.5 * (x[x.Length / 2 - 1] + x[x.Length / 2]); }
static double[][] Regroup(double[] flat, int m, int n) => Enumerable.Range(0, m).Select(c => flat.Skip(c * n).Take(n).ToArray()).ToArray();
static double[][] Split(double[][] chains) => chains.SelectMany(x => new[] { x.Take(x.Length / 2).ToArray(), x.Skip(x.Length - x.Length / 2).ToArray() }).ToArray();
static double SplitRhat(double[][] chains)
{
    double[][] split = Split(chains); int m = split.Length, n = split.Min(x => x.Length);
    double[] means = split.Select(x => x.Take(n).Average()).ToArray();
    double within = split.Select(x => Variance(x.Take(n).ToArray())).Average(); if (within <= 0.0) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    return System.Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}
static double Ess(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length); double[][] x = chains.Select(y => y.Take(n).ToArray()).ToArray();
    double[] means = x.Select(y => y.Average()).ToArray(); double within = x.Select(Variance).Average(); if (within <= 0.0) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(y => (y - grand) * (y - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n; if (varPlus <= 0.0) return double.NaN;
    double[] rho = new double[n]; rho[0] = 1.0;
    for (int lag = 1; lag < n; lag++)
    {
        double covariance = 0.0;
        for (int c = 0; c < m; c++) { double sum = 0.0; for (int i = 0; i + lag < n; i++) sum += (x[c][i] - means[c]) * (x[c][i + lag] - means[c]); covariance += sum / n; }
        rho[lag] = 1.0 - (within - covariance / m) / varPlus;
    }
    double tau = -1.0, previous = double.PositiveInfinity;
    for (int k = 0; 2 * k + 1 < n; k++) { double pair = rho[2 * k] + rho[2 * k + 1]; if (pair < 0) break; pair = System.Math.Min(pair, previous); previous = pair; tau += 2.0 * pair; }
    return tau > 0.0 ? m * n / tau : double.NaN;
}
static double Variance(double[] values)
{
    if (values.Length < 2) return 0.0; double mean = values.Average(); return values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1);
}
static double InverseNormalCdf(double p)
{
    double[] a=[-39.69683028665376,220.9460984245205,-275.9285104469687,138.357751867269,-30.66479806614716,2.506628277459239];
    double[] b=[-54.47609879822406,161.5858368580409,-155.6989798598866,66.80131188771972,-13.28068155288572];
    double[] c=[-0.007784894002430293,-0.3223964580411365,-2.400758277161838,-2.549732539343734,4.374664141464968,2.938163982698783];
    double[] d=[0.007784695709041462,0.3224671290700398,2.445134137142996,3.754408661907416]; const double low=0.02425;
    if(p<low){double q=System.Math.Sqrt(-2*System.Math.Log(p));return (((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    if(p>1-low){double q=System.Math.Sqrt(-2*System.Math.Log(1-p));return -(((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    double r=p-0.5,s=r*r;return (((((a[0]*s+a[1])*s+a[2])*s+a[3])*s+a[4])*s+a[5])*r/(((((b[0]*s+b[1])*s+b[2])*s+b[3])*s+b[4])*s+1);
}
static bool BitEqual(double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
static double ScaledAbsoluteDifference(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Abs(b));
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
JsonElement ReadBinding(string id) => JsonDocument.Parse(File.ReadAllBytes(PathFor(id))).RootElement.Clone();
string PathFor(string id) => specs.Single(x => x.Id == id).Path;
void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}
object Early(string verdict, bool valid, bool bindingsValid, object bindingRows, object battery) => new
{
    schemaVersion = 1, phase = 575, phaseId = "phase575-registered-fold-convention-readjudication",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = valid,
    exactBindingsValid = bindingsValid, bindings = bindingRows, knownAnswerBattery = battery,
    upstream = (object?)null, convention = (object?)null, adjudication = (object?)null, confirmation = (object?)null,
    verdictKind = verdict, terminalStatus = "registered-fold-convention-readjudication-" + verdict,
    directionalEvidenceIndependentlyConfirmedUnderRegisteredFoldConvention = false,
    phase571LeverIndependentlyConfirmedUnderRegisteredFoldConvention = false,
    phase571LeverScopeUnchangedLocalMovementOnly = true,
    prospectiveChainPackPlanningGateOpen = false,
    prospectiveChainPackPlanningGateAuthorizesOnly = "registration and prospective freezing of a separate disjoint-seed chain-pack design phase",
    phase572TerminalAndToleranceNeverRewritten = true,
    newSamplingPerformed = false, replayPerformed = false, rngUsed = false, markovChainAdvanced = false,
    configurationsRetained = false, phase548Or549TerminalChanged = false,
    phase570Or571Or572Reinterpreted = false, phase572ToleranceRelaxed = false,
    registeredBlindSeedTouched = false, protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false,
    measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

sealed record Binding(string Id, string Path, string Hash);
sealed record Diagnostics(double Rhat, double BulkEss, double TailEss);
sealed record RowVerdict(bool EssBitIdentical, bool ClassificationAgrees, bool Explained, bool MarginSufficient, double ScaledRhatDifference, bool Ok);
