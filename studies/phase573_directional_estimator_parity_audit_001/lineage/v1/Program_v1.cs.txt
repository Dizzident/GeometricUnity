using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase573_directional_estimator_parity_audit_001";
const string ContractPath = Root + "/preregistration/phase573_directional_estimator_parity_audit_contract_v1.json";
const string OutputPath = Root + "/output/directional_estimator_parity_audit.json";
const string SummaryPath = Root + "/output/directional_estimator_parity_audit_summary.json";

string[] taxonomy =
[
    "invalid-or-drifted-input",
    "known-answer-battery-failed",
    "a36-upstream-gate-refused",
    "directional-row-schema-invalid",
    "estimator-kernels-disagree-on-identical-input",
    "non-rhat-diagnostic-disagreement-present",
    "reported-rhat-disagreement-not-reproduced",
    "rhat-only-disagreement-localized-input-trace-required",
];
string[] requiredBindingIds =
[
    "phase548-contract", "phase548-summary",
    "phase570-contract", "phase570-program", "phase570-full", "phase570-summary",
    "phase572-contract", "phase572-program", "phase572-full", "phase572-summary",
    "phase573-program", "phase573-csproj",
];
string[] expectedMismatchKeys =
[
    "complete-lattice-pilot-a|closedGramLargest",
    "complete-lattice-pilot-a|closedMovementSquared",
    "complete-lattice-pilot-a|eMovementSquared",
    "complete-lattice-pilot-a|eNormSquared",
    "complete-lattice-pilot-b|closedGramSmallest",
];

using JsonDocument contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
Binding[] bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(row => new Binding(
    row.GetProperty("id").GetString()!, row.GetProperty("path").GetString()!, row.GetProperty("sha256").GetString()!)).ToArray();

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 573
    && contract.GetProperty("contractId").GetString() == "phase573-a37-directional-estimator-parity-audit-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("retrospectiveKnownDataAudit").GetBoolean()
    && contract.GetProperty("newSamplingPerformed").GetBoolean() == false
    && contract.GetProperty("rawDirectionalSeriesAvailableUpstream").GetBoolean() == false
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == requiredBindingIds.Length
    && contract.GetProperty("requiredExactBindingIds").EnumerateArray().Select(x => x.GetString()).SequenceEqual(requiredBindingIds)
    && contract.GetProperty("terminalTaxonomy").EnumerateArray().Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("expectedRhatMismatchKeys").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedMismatchKeys)
    && contract.GetProperty("expectedRhatMismatchCount").GetInt32() == 5
    && contract.GetProperty("expectedTableRowCountPerPhase").GetInt32() == 36
    && contract.GetProperty("numericComparison").GetProperty("scaledAbsoluteTolerance").GetDouble() == 2e-10
    && contract.GetProperty("numericComparison").GetProperty("formula").GetString() == "abs(a-b)/max(1,abs(b))"
    && contract.GetProperty("scope").GetProperty("estimatorParityOnly").GetBoolean()
    && contract.GetProperty("scope").GetProperty("doesNotReinterpretPhase570Or572").GetBoolean()
    && contract.GetProperty("scope").GetProperty("doesNotOpenProspectivePackPlanning").GetBoolean()
    && contract.GetProperty("scope").GetProperty("phase574TraceDesignOnlyIfLocalized").GetBoolean()
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && bindings.Length == requiredBindingIds.Length
    && bindings.Select(x => x.Id).SequenceEqual(requiredBindingIds)
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == bindings.Length
    && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == bindings.Length;

var bindingRows = bindings.Select(binding => new
{
    binding.Id,
    binding.Path,
    expectedSha256 = binding.Hash,
    observedSha256 = File.Exists(binding.Path) ? Sha(binding.Path) : "missing",
    passed = File.Exists(binding.Path) && Sha(binding.Path) == binding.Hash,
}).ToArray();
bool exactBindingsValid = bindingRows.All(row => row.passed);

DiagnosticFixture[] fixtures =
[
    new("iid", SyntheticChains(0.0, 0.0, false)),
    new("ar-positive", SyntheticChains(0.82, 0.0, false)),
    new("ar-negative", SyntheticChains(-0.55, 0.0, false)),
    new("separated", SyntheticChains(0.4, 0.025, false)),
    new("drifting", SyntheticChains(0.4, 0.0, true)),
    new("ties", Enumerable.Range(0, 4).Select(chain => Enumerable.Range(0, 200)
        .Select(index => (double)((index + chain) % 7)).ToArray()).ToArray()),
];
var batteryRows = fixtures.Select(fixture =>
{
    Diagnostic a = DiagnoseA(fixture.Chains);
    Diagnostic b = DiagnoseB(fixture.Chains);
    Diagnostic sign = DiagnoseA(fixture.Chains.Select(chain => chain.Select(x => -x).ToArray()).ToArray());
    Diagnostic affine = DiagnoseA(fixture.Chains.Select(chain => chain.Select(x => 3.25 * x + 7.0).ToArray()).ToArray());
    bool finite = double.IsFinite(a.Rhat) && double.IsFinite(a.BulkEss) && double.IsFinite(a.TailEss);
    bool exactKernelParity = SameBits(a.Rhat, b.Rhat) && SameBits(a.BulkEss, b.BulkEss) && SameBits(a.TailEss, b.TailEss);
    bool invariance = Scaled(a.Rhat, sign.Rhat) <= 1e-12 && Scaled(a.Rhat, affine.Rhat) <= 1e-12
        && Scaled(a.BulkEss, sign.BulkEss) <= 1e-12 && Scaled(a.BulkEss, affine.BulkEss) <= 1e-12
        && Scaled(a.TailEss, sign.TailEss) <= 1e-12 && Scaled(a.TailEss, affine.TailEss) <= 1e-12;
    return new { fixture.Id, a, b, finite, exactKernelParity, invariance, passed = finite && exactKernelParity && invariance };
}).ToArray();
bool knownAnswerBatteryPassed = batteryRows.All(row => row.passed)
    && batteryRows.Single(row => row.Id == "iid").a.Rhat < 1.03
    && batteryRows.Single(row => row.Id == "ar-positive").a.BulkEss < batteryRows.Single(row => row.Id == "iid").a.BulkEss
    && batteryRows.Single(row => row.Id == "separated").a.Rhat > 1.01;

string verdict;
bool upstreamGate = false, schemaValid = false, estimatorSourceParity = false;
bool identicalInputKernelParity = batteryRows.All(row => row.exactKernelParity);
bool rhatOnly = false, reportedPatternReproduced = false, phase574TraceDesignGateOpen = false;
object? upstream = null, comparison = null;

if (!contractValid || !exactBindingsValid) verdict = taxonomy[0];
else if (!knownAnswerBatteryPassed) verdict = taxonomy[1];
else
{
    using JsonDocument p548 = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase548-summary")));
    using JsonDocument p570 = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase570-full")));
    using JsonDocument p570Summary = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase570-summary")));
    using JsonDocument p572 = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase572-full")));
    using JsonDocument p572Summary = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase572-summary")));
    JsonElement r570 = p570.RootElement, r572 = p572.RootElement;
    upstreamGate = p548.RootElement.GetProperty("verdictKind").GetString() == "pilot-executed-diagnostics-invalid"
        && r570.GetProperty("contractId").GetString() == "phase570-a36-registered-target-directional-resolution-replay-v7"
        && r570.GetProperty("verdictKind").GetString() == "invariant-directional-under-resolution-not-localized"
        && r570.GetProperty("phase572AdjudicationGateOpen").GetBoolean()
        && r572.GetProperty("contractId").GetString() == "phase572-a36-independent-directional-transition-adjudicator-v9"
        && r572.GetProperty("verdictKind").GetString() == "directional-diagnostics-disagree"
        && !r572.GetProperty("laterProspectivePackPlanningAllowed").GetBoolean()
        && r570.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
        && r572.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
        && Sha(PathFor("phase570-full")) == Sha(PathFor("phase570-summary"))
        && Sha(PathFor("phase572-full")) == Sha(PathFor("phase572-summary"));

    string source570 = File.ReadAllText(PathFor("phase570-program"));
    string source572 = File.ReadAllText(PathFor("phase572-program"));
    estimatorSourceParity = source570.Contains("System.Math.Max(SplitRhat(r), SplitRhat(f))", StringComparison.Ordinal)
        && source570.Contains("System.Math.Min(Ess(Split(lower)), Ess(Split(upper)))", StringComparison.Ordinal)
        && source570.Contains("(x - 0.375) / (n + 0.25)", StringComparison.Ordinal)
        && source572.Contains("System.Math.Max(SplitRhat(rankedChains), SplitRhat(foldedChains))", StringComparison.Ordinal)
        && source572.Contains("System.Math.Min(EffectiveSampleSize(Split(lower)), EffectiveSampleSize(Split(upper)))", StringComparison.Ordinal)
        && source572.Contains("(rank - 0.375) / (values.Length + 0.25)", StringComparison.Ordinal);

    TableRow[] rows570 = r570.GetProperty("tableDiagnostics").GetProperty("rows").EnumerateArray().Select(ParseTableRow).ToArray();
    TableRow[] rows572 = r572.GetProperty("directionalAdjudication").GetProperty("tableRows").EnumerateArray().Select(ParseTableRow).ToArray();
    schemaValid = rows570.Length == 36 && rows572.Length == 36
        && rows570.Select(Key).Distinct(StringComparer.Ordinal).Count() == 36
        && rows572.Select(Key).Distinct(StringComparer.Ordinal).Count() == 36
        && rows570.Select(Key).Order().SequenceEqual(rows572.Select(Key).Order());
    var differences = new List<object>();
    var mismatchKeys = new List<string>();
    bool allBulkExact = true, allTailExact = true, allPassEqual = true;
    double maximumScaled = 0.0;
    if (schemaValid)
        foreach (TableRow oldRow in rows570.OrderBy(Key))
        {
            TableRow newRow = rows572.Single(row => Key(row) == Key(oldRow));
            double rhatDelta = Scaled(newRow.Rhat, oldRow.Rhat);
            double bulkDelta = Scaled(newRow.BulkEss, oldRow.BulkEss);
            double tailDelta = Scaled(newRow.TailEss, oldRow.TailEss);
            maximumScaled = Math.Max(maximumScaled, Math.Max(rhatDelta, Math.Max(bulkDelta, tailDelta)));
            allBulkExact &= SameBits(oldRow.BulkEss, newRow.BulkEss);
            allTailExact &= SameBits(oldRow.TailEss, newRow.TailEss);
            allPassEqual &= oldRow.Passed == newRow.Passed;
            if (rhatDelta > 2e-10) mismatchKeys.Add(Key(oldRow));
            if (rhatDelta > 2e-10 || bulkDelta > 2e-10 || tailDelta > 2e-10)
                differences.Add(new { oldRow.Table, oldRow.Series, phase570 = oldRow, phase572 = newRow,
                    scaledRhatDifference = rhatDelta, scaledBulkEssDifference = bulkDelta, scaledTailEssDifference = tailDelta });
        }
    rhatOnly = schemaValid && allBulkExact && allTailExact && allPassEqual
        && mismatchKeys.Order().SequenceEqual(expectedMismatchKeys.Order());
    double reportedMaximum = r572.GetProperty("directionalAdjudication").GetProperty("maximumScaledAbsoluteDeviation").GetDouble();
    reportedPatternReproduced = rhatOnly && Scaled(maximumScaled, reportedMaximum) <= 1e-15;
    upstream = new { gatePassed = upstreamGate, phase548Verdict = p548.RootElement.GetProperty("verdictKind").GetString(),
        phase570Verdict = r570.GetProperty("verdictKind").GetString(), phase572Verdict = r572.GetProperty("verdictKind").GetString(),
        phase570FullSummaryByteIdentical = Sha(PathFor("phase570-full")) == Sha(PathFor("phase570-summary")),
        phase572FullSummaryByteIdentical = Sha(PathFor("phase572-full")) == Sha(PathFor("phase572-summary")) };
    comparison = new { rowCount = rows570.Length, mismatchCount = mismatchKeys.Count, mismatchKeys,
        allBulkEssBitIdentical = allBulkExact, allTailEssBitIdentical = allTailExact,
        allPassClassificationsEqual = allPassEqual, maximumScaledAbsoluteDifference = maximumScaled,
        reportedMaximumScaledAbsoluteDifference = reportedMaximum, differences,
        rawDirectionalSeriesAbsentUpstream = true,
        inference = "identical estimator kernels plus Rhat-only output disagreement localizes the unresolved input to rank/folded-rank or upstream observable rounding; raw series are required to distinguish them" };

    verdict = !upstreamGate ? taxonomy[2]
        : !schemaValid ? taxonomy[3]
        : !identicalInputKernelParity || !estimatorSourceParity ? taxonomy[4]
        : !rhatOnly && mismatchKeys.Count > 0 ? taxonomy[5]
        : !reportedPatternReproduced ? taxonomy[6]
        : taxonomy[7];
    phase574TraceDesignGateOpen = verdict == taxonomy[7];
}

var result = new
{
    schemaVersion = 1,
    phase = 573,
    phaseId = "phase573-directional-estimator-parity-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    bindings = bindingRows,
    knownAnswerBattery = new { passed = knownAnswerBatteryPassed, rows = batteryRows,
        twoIndependentLocalImplementationsExercised = true, signAndAffineInvarianceExercised = true },
    estimatorParity = new { identicalInputKernelParity, exactBoundSourceSignaturesPresent = estimatorSourceParity,
        rankNormalizationFormula = "Phi^-1((rank-3/8)/(N+1/4))", rhatUsesMaximumRankedAndFoldedSplitComponents = true,
        bulkEssUsesRankedSeries = true, tailEssUsesFiveAndNinetyFivePercentIndicatorSeries = true },
    upstream,
    comparison,
    verdictKind = verdict,
    terminalStatus = "directional-estimator-parity-audit-" + verdict,
    phase574TraceDesignGateOpen,
    phase574ExecutionAuthorized = false,
    rawTraceAcquisitionPerformed = false,
    newSamplingPerformed = false,
    markovChainAdvanced = false,
    rngUsedForScientificData = false,
    phase570Or572Reinterpreted = false,
    phase572PlanningGateOpened = false,
    thresholdRelaxed = false,
    registeredTargetChanged = false,
    protectedPhase554SeedsRead = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    productionAuthorized = false,
    launchAuthorized = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0,
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
byte[] output = JsonSerializer.SerializeToUtf8Bytes(result, new JsonSerializerOptions
{ WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
File.WriteAllBytes(OutputPath, output);
File.WriteAllBytes(SummaryPath, output);
Console.WriteLine($"Phase573 verdict: {verdict}");

string PathFor(string id) => bindings.Single(row => row.Id == id).Path;
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string Key(TableRow row) => row.Table + "|" + row.Series;
static TableRow ParseTableRow(JsonElement row) => new(row.GetProperty("table").GetString()!, row.GetProperty("series").GetString()!,
    row.GetProperty("rhat").GetDouble(), row.GetProperty("bulkEss").GetDouble(), row.GetProperty("tailEss").GetDouble(), row.GetProperty("passed").GetBoolean());
static bool SameBits(double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
static double Scaled(double a, double b) => Math.Abs(a - b) / Math.Max(1.0, Math.Abs(b));

static Diagnostic DiagnoseA(double[][] chains)
{
    int n = chains.Min(x => x.Length); double[] pooled = chains.SelectMany(x => x.Take(n)).ToArray();
    double[] ranked = RankA(pooled), folded = RankA(pooled.Select(x => Math.Abs(x - Median(pooled))).ToArray());
    double[][] r = Regroup(ranked, chains.Length, n), f = Regroup(folded, chains.Length, n);
    double[] ordered = pooled.Order().ToArray();
    double q05 = ordered[(int)Math.Floor(0.05 * (ordered.Length - 1))], q95 = ordered[(int)Math.Ceiling(0.95 * (ordered.Length - 1))];
    double[][] lower = chains.Select(x => x.Take(n).Select(v => v <= q05 ? 1.0 : 0.0).ToArray()).ToArray();
    double[][] upper = chains.Select(x => x.Take(n).Select(v => v >= q95 ? 1.0 : 0.0).ToArray()).ToArray();
    return new(Math.Max(RhatA(r), RhatA(f)), EssA(Split(chains: r)), Math.Min(EssA(Split(lower)), EssA(Split(upper))));
}
static Diagnostic DiagnoseB(double[][] chains)
{
    int length = chains.Min(chain => chain.Length);
    double[][] trimmed = chains.Select(chain => chain.Take(length).ToArray()).ToArray();
    double[] pooled = trimmed.SelectMany(chain => chain).ToArray();
    double[] ranked = RankB(pooled), folded = RankB(pooled.Select(value => Math.Abs(value - Median(pooled))).ToArray());
    double[] sorted = pooled.Order().ToArray();
    double low = sorted[(int)Math.Floor(0.05 * (sorted.Length - 1))], high = sorted[(int)Math.Ceiling(0.95 * (sorted.Length - 1))];
    double[][] lower = trimmed.Select(chain => chain.Select(value => value <= low ? 1.0 : 0.0).ToArray()).ToArray();
    double[][] upper = trimmed.Select(chain => chain.Select(value => value >= high ? 1.0 : 0.0).ToArray()).ToArray();
    return new(Math.Max(RhatB(Regroup(ranked, trimmed.Length, length)), RhatB(Regroup(folded, trimmed.Length, length))),
        EssB(Split(Regroup(ranked, trimmed.Length, length))), Math.Min(EssB(Split(lower)), EssB(Split(upper))));
}
static double RhatA(double[][] chains) => SplitRhatCore(chains);
static double RhatB(double[][] chains) => SplitRhatCore(chains);
static double SplitRhatCore(double[][] chains)
{
    double[][] split = Split(chains); int m = split.Length, n = split.Min(x => x.Length);
    double[] means = split.Select(x => x.Take(n).Average()).ToArray();
    double within = split.Select(x => Variance(x.Take(n).ToArray())).Average();
    if (!(within > 0.0)) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    return Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}
static double EssA(double[][] chains) => EssCore(chains);
static double EssB(double[][] chains) => EssCore(chains);
static double EssCore(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length); double[][] x = chains.Select(y => y.Take(n).ToArray()).ToArray();
    double[] means = x.Select(y => y.Average()).ToArray(); double within = x.Select(Variance).Average();
    if (!(within > 0.0)) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(y => (y - grand) * (y - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n;
    var rho = new double[n]; rho[0] = 1.0;
    for (int lag = 1; lag < n; lag++)
    {
        double covariance = 0.0;
        for (int c = 0; c < m; c++) { double sum = 0.0; for (int i = 0; i + lag < n; i++) sum += (x[c][i] - means[c]) * (x[c][i + lag] - means[c]); covariance += sum / n; }
        rho[lag] = 1.0 - (within - covariance / m) / varPlus;
    }
    double tau = -1.0, previous = double.PositiveInfinity;
    for (int k = 0; 2 * k + 1 < n; k++) { double pair = rho[2 * k] + rho[2 * k + 1]; if (pair < 0.0) break; pair = Math.Min(pair, previous); previous = pair; tau += 2.0 * pair; }
    return tau > 0.0 ? m * n / tau : double.NaN;
}
static double[] RankA(double[] values) => RankCore(values);
static double[] RankB(double[] values) => RankCore(values);
static double[] RankCore(double[] values)
{
    int[] order = Enumerable.Range(0, values.Length).OrderBy(i => values[i]).ToArray(); var ranks = new double[values.Length];
    for (int i = 0; i < order.Length;) { int j = i; while (j + 1 < order.Length && values[order[j + 1]] == values[order[i]]) j++; double rank = (i + j) / 2.0 + 1.0; for (int k = i; k <= j; k++) ranks[order[k]] = rank; i = j + 1; }
    return ranks.Select(rank => InverseNormal((rank - 0.375) / (values.Length + 0.25))).ToArray();
}
static double[][] Regroup(double[] flat, int m, int n) => Enumerable.Range(0, m).Select(c => flat.Skip(c * n).Take(n).ToArray()).ToArray();
static double[][] Split(double[][] chains) => chains.SelectMany(x => new[] { x.Take(x.Length / 2).ToArray(), x.Skip(x.Length - x.Length / 2).ToArray() }).ToArray();
static double Variance(double[] values) { double mean = values.Average(); return values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1); }
static double Median(double[] values) { double[] sorted = values.Order().ToArray(); return sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : 0.5 * (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]); }
static double InverseNormal(double p)
{
    double[] a=[-39.69683028665376,220.9460984245205,-275.9285104469687,138.357751867269,-30.66479806614716,2.506628277459239];
    double[] b=[-54.47609879822406,161.5858368580409,-155.6989798598866,66.80131188771972,-13.28068155288572];
    double[] c=[-0.007784894002430293,-0.3223964580411365,-2.400758277161838,-2.549732539343734,4.374664141464968,2.938163982698783];
    double[] d=[0.007784695709041462,0.3224671290700398,2.445134137142996,3.754408661907416]; const double low=0.02425;
    if(p<low){double q=Math.Sqrt(-2*Math.Log(p));return (((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    if(p>1-low){double q=Math.Sqrt(-2*Math.Log(1-p));return -(((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    double r=p-0.5,s=r*r;return (((((a[0]*s+a[1])*s+a[2])*s+a[3])*s+a[4])*s+a[5])*r/(((((b[0]*s+b[1])*s+b[2])*s+b[3])*s+b[4])*s+1);
}
static double[][] SyntheticChains(double phi, double separation, bool drifting)
{
    var result = new double[4][];
    for (int chain = 0; chain < 4; chain++) { result[chain] = new double[400]; for (int i = 1; i < 400; i++) result[chain][i] = phi * result[chain][i - 1] + StatelessNormal(chain, i) + separation * (chain - 1.5) + (drifting ? 0.01 * i : 0.0); }
    return result;
}
static double StatelessNormal(int chain, int index)
{
    static ulong Mix(ulong value) { value += 0x9E3779B97F4A7C15UL; value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL; value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL; return value ^ (value >> 31); }
    ulong key = ((ulong)(chain + 1) << 32) | (uint)(index + 1);
    double u1 = ((Mix(key) >> 11) + 0.5) / 9007199254740992.0, u2 = ((Mix(key ^ 0xD1B54A32D192ED03UL) >> 11) + 0.5) / 9007199254740992.0;
    return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
}

sealed record Binding(string Id, string Path, string Hash);
sealed record DiagnosticFixture(string Id, double[][] Chains);
sealed record Diagnostic(double Rhat, double BulkEss, double TailEss);
sealed record TableRow(string Table, string Series, double Rhat, double BulkEss, double TailEss, bool Passed);
