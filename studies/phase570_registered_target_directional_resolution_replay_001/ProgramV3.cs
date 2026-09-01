using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase570_registered_target_directional_resolution_replay_001";
const string ContractPath = Root + "/preregistration/phase570_registered_target_directional_resolution_replay_contract_v3.json";
const string OutputPath = Root + "/output/registered_target_directional_resolution_replay.json";
const string SummaryPath = Root + "/output/registered_target_directional_resolution_replay_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] requiredBindingIds =
[
    "phase570-v3-program", "phase570-v2-program-live", "phase570-v2-program-lineage", "phase570-v1-program-lineage",
    "phase570-v1-contract", "phase570-v2-contract", "phase570-lineage-record", "phase548-contract", "phase548-program", "phase548-summary",
    "phase548-telemetry-a-546101", "phase548-telemetry-a-546103", "phase548-telemetry-a-546107",
    "phase548-telemetry-b-546201", "phase548-telemetry-b-546203", "phase548-telemetry-b-546207",
    "phase548-checkpoint-a-546101", "phase548-checkpoint-a-546103", "phase548-checkpoint-a-546107",
    "phase548-checkpoint-b-546201", "phase548-checkpoint-b-546203", "phase548-checkpoint-b-546207",
    "phase549-summary", "phase550-summary", "phase551-summary", "phase552-summary", "phase569-summary",
    "shiab-operator", "mass-matrix", "curvature-assembler", "mesh-generator", "mesh-topology-builder", "lie-algebra-factory",
];
bool exactBindingIdsValid = specs.Select(x => x.Id).Order(StringComparer.Ordinal)
    .SequenceEqual(requiredBindingIds.Order(StringComparer.Ordinal), StringComparer.Ordinal);
bool exactBindingsValid = bindings.Length == 33 && contract.GetProperty("requiredExactBindingCount").GetInt32() == 33
    && exactBindingIdsValid && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "known-answer-battery-failed", "registered-target-lane-gate-refused",
    "resource-refusal", "committed-chain-replay-not-bit-identical", "incidence-projector-control-failed",
    "directional-diagnostics-inconclusive", "retrospective-drift-detected",
    "closed-sector-under-resolution-enriched", "rank-one-valley-under-resolution-enriched",
    "invariant-directional-under-resolution-not-localized",
];
string[] expectedAuthorityFirewallKeys =
[
    "phase548Or549TerminalChanged", "phase548GateRehabilitated", "registeredBlindSeedTouched",
    "markovChainAdvancedBeyondCommittedReplay", "protectedPhase554SeedsRead", "directionCalledGaugeOrRedundant",
    "quotientApplied", "gaugeFixingApplied", "measureNormalizationApplied", "sourceOrModelSelected",
    "phase561Opened", "o4Discharged", "phase458Satisfied", "phase481PackCreatedOrMutated",
    "allDownstreamAuthority", "sourceContractApplicationAllowed", "productionDefaultSelected",
    "productionAuthorized", "launchAuthorized", "physicalUnitClaimAllowed", "gevClaimAllowed",
];
string[] expectedDriftDecisionSeries =
[
    "closedNormSquared", "closedPerpNormSquared", "closedGramLargest",
    "withinClosedRankOneDistanceSquared", "withinClosedRankOneRelativeDistance",
];
JsonElement incidenceSpec = contract.GetProperty("incidenceDecomposition");
JsonElement rankOneGeometrySpec = contract.GetProperty("closedRankOneGeometry");
JsonElement replaySpec = contract.GetProperty("replay");
JsonElement diagnosticSpec = contract.GetProperty("diagnostics");
JsonElement knownAnswerBandSpec = diagnosticSpec.GetProperty("knownAnswerBands");
JsonElement classificationSpec = contract.GetProperty("classification");
JsonElement driftSpec = contract.GetProperty("drift");
JsonElement resourceSpec = contract.GetProperty("resourceRefusal");
JsonElement upstreamSpec = contract.GetProperty("requiredUpstreamVerdicts");
JsonElement transitionGateSpec = contract.GetProperty("phase571TransitionProbeGate");
JsonElement outputRngSpec = contract.GetProperty("outputRngSemantics");
JsonElement authorityFirewalls = contract.GetProperty("authorityFirewalls");
string[] actualAuthorityFirewallKeys = authorityFirewalls.EnumerateObject().Select(x => x.Name).Order(StringComparer.Ordinal).ToArray();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 3
    && contract.GetProperty("contractId").GetString() == "phase570-a36-registered-target-directional-resolution-replay-v3"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A36"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("analysisIsRetrospectiveOnKnownData").GetBoolean()
    && contract.GetProperty("statisticsAndThresholdsFrozenBeforeReplay").GetBoolean()
    && contract.GetProperty("newSamplingPerformed").GetBoolean() == false
    && contract.GetProperty("lineage").GetProperty("v1").GetProperty("contractSha256").GetString() == "11d69600ebf48bdf4506cdcda6f99593c73e23f78380872f193986786a0992a9"
    && contract.GetProperty("lineage").GetProperty("v1").GetProperty("programSha256").GetString() == "da43a95ea29ab4f9b9d45eaa024ae88c1c8fa74c4bad2ca9d2d76fc1bb9f85de"
    && contract.GetProperty("lineage").GetProperty("v1").GetProperty("programBytesExactlyRecovered").GetBoolean()
    && contract.GetProperty("lineage").GetProperty("v1").GetProperty("executed").GetBoolean() == false
    && contract.GetProperty("lineage").GetProperty("v2").GetProperty("contractSha256").GetString() == "48e08d8dd0f441e42be27bb56e192366f79afcf3428c5fcb356f013bb33bae43"
    && contract.GetProperty("lineage").GetProperty("v2").GetProperty("programSha256").GetString() == "1ba9651d1cd45cb4a80bd4b01cf2dd4255324ca882ed786dd92762f12afdad3f"
    && contract.GetProperty("lineage").GetProperty("v2").GetProperty("executed").GetBoolean() == false
    && exactBindingsValid && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && incidenceSpec.GetProperty("scalarDimensions").GetProperty("e").GetInt32() == 80
    && incidenceSpec.GetProperty("scalarDimensions").GetProperty("w").GetInt32() == 4
    && incidenceSpec.GetProperty("scalarDimensions").GetProperty("c").GetInt32() == 84
    && incidenceSpec.GetProperty("scalarDimensions").GetProperty("cPerp").GetInt32() == 1131
    && incidenceSpec.GetProperty("fullDimensions").GetProperty("e").GetInt32() == 240
    && incidenceSpec.GetProperty("fullDimensions").GetProperty("w").GetInt32() == 12
    && incidenceSpec.GetProperty("fullDimensions").GetProperty("c").GetInt32() == 252
    && incidenceSpec.GetProperty("fullDimensions").GetProperty("cPerp").GetInt32() == 3393
    && incidenceSpec.GetProperty("eDefinition").GetString() == "image of exact integer vertex-to-edge incidence d0"
    && incidenceSpec.GetProperty("wDefinition").GetString() == "exact integer closed winding representatives orthogonalized against E"
    && incidenceSpec.GetProperty("cDefinition").GetString() == "kernel of face incidence d1 equals E direct-sum W"
    && incidenceSpec.GetProperty("exactIntegerGeneratorClosureRequired").GetBoolean()
    && incidenceSpec.GetProperty("numericOrthonormalBasisTolerance").GetDouble() == 2e-13
    && incidenceSpec.GetProperty("phase550CertifiedNullBasisDimensionRequired").GetInt32() == 252
    && incidenceSpec.GetProperty("primaryObservablesAreProjectorInvariant").GetBoolean()
    && incidenceSpec.GetProperty("individualLocalHessianEigenvectorsArePrimaryObservables").GetBoolean() == false
    && rankOneGeometrySpec.GetProperty("coefficientShape").EnumerateArray().Select(x => x.GetInt32()).SequenceEqual(new[] { 84, 3 })
    && rankOneGeometrySpec.GetProperty("gramShape").EnumerateArray().Select(x => x.GetInt32()).SequenceEqual(new[] { 3, 3 })
    && rankOneGeometrySpec.GetProperty("rankOneAlignment").GetString() == "lambda-largest/trace"
    && rankOneGeometrySpec.GetProperty("withinClosedDistanceSquared").GetString() == "lambda-middle+lambda-smallest"
    && rankOneGeometrySpec.GetProperty("withinClosedRelativeDistance").GetString() == "within-C distance squared/closed trace"
    && rankOneGeometrySpec.GetProperty("fullDistanceSquared").GetString() == "C-perp norm squared + within-C rank-one distance squared"
    && replaySpec.GetProperty("samplerReimplementedFromPhase548Contract").GetBoolean()
    && replaySpec.GetProperty("deltaHRelativeTolerance").GetDouble() == 1e-12
    && replaySpec.GetProperty("requireEveryDecisionMatch").GetBoolean()
    && replaySpec.GetProperty("requireFinalPositionBitIdentical").GetBoolean()
    && replaySpec.GetProperty("requireRetainedDrawsPerChain").GetInt32() == 340
    && replaySpec.GetProperty("rngUsed").GetBoolean()
    && replaySpec.GetProperty("rngUseRestrictedToCommittedReplay").GetBoolean()
    && replaySpec.GetProperty("markovChainAdvancedBeyondCommittedReplay").GetBoolean() == false
    && replaySpec.GetProperty("configurationsRetained").GetBoolean() == false
    && diagnosticSpec.GetProperty("estimator").GetString() == "split rank-normalized and folded R-hat, Geyer bulk ESS, and pooled-rank 5/95-percent indicator tail ESS"
    && diagnosticSpec.GetProperty("maximumRhat").GetDouble() == 1.01
    && diagnosticSpec.GetProperty("minimumEss").GetDouble() == 100.0
    && diagnosticSpec.GetProperty("tailEssDefinition").GetString() == "minimum ESS of pooled-rank lower-5-percent and upper-5-percent indicator series"
    && diagnosticSpec.GetProperty("closedEnrichmentRule").GetString() == "closedNorm fails both tables, closedPerpNorm passes both, and closed movement per dimension is at most 0.75 of C-perp in both tables"
    && diagnosticSpec.GetProperty("rankOneEnrichmentRule").GetString() == "closedNorm and largest Gram eigenvalue pass both tables while absolute and relative within-C rank-one distances fail both tables"
    && diagnosticSpec.GetProperty("lagOneReported").GetBoolean()
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("rhat").GetProperty("minimumInclusive").GetDouble() == 0.99
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("rhat").GetProperty("maximumInclusive").GetDouble() == 1.03
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("bulkEss").GetProperty("minimumInclusive").GetDouble() == 800.0
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("bulkEss").GetProperty("maximumInclusive").GetDouble() == 2200.0
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("tailEss").GetProperty("minimumInclusive").GetDouble() == 500.0
    && knownAnswerBandSpec.GetProperty("iid").GetProperty("tailEss").GetProperty("maximumInclusive").GetDouble() == 2200.0
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("rhat").GetProperty("minimumInclusive").GetDouble() == 1.0
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("rhat").GetProperty("maximumInclusive").GetDouble() == 1.2
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("bulkEss").GetProperty("minimumInclusive").GetDouble() == 20.0
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("bulkEss").GetProperty("maximumInclusive").GetDouble() == 400.0
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("tailEss").GetProperty("minimumInclusive").GetDouble() == 20.0
    && knownAnswerBandSpec.GetProperty("ar1Phi09").GetProperty("tailEss").GetProperty("maximumInclusive").GetDouble() == 600.0
    && knownAnswerBandSpec.GetProperty("ar1ToIidBulkEssRatio").GetProperty("minimumInclusive").GetDouble() == 0.01
    && knownAnswerBandSpec.GetProperty("ar1ToIidBulkEssRatio").GetProperty("maximumInclusive").GetDouble() == 0.45
    && knownAnswerBandSpec.GetProperty("ar1ToIidTailEssRatio").GetProperty("minimumInclusive").GetDouble() == 0.01
    && knownAnswerBandSpec.GetProperty("ar1ToIidTailEssRatio").GetProperty("maximumInclusive").GetDouble() == 0.60
    && classificationSpec.GetProperty("combinedEnrichmentState").GetString() == "logically contradictory under frozen rules; fail invalid-or-drifted-input"
    && classificationSpec.GetProperty("truthTableMustReachEveryTerminal").GetBoolean()
    && classificationSpec.GetProperty("driftPrecedesEnrichment").GetBoolean()
    && driftSpec.GetProperty("batchCount").GetInt32() == 20
    && driftSpec.GetProperty("absoluteZThreshold").GetDouble() == 3.0
    && driftSpec.GetProperty("chainsRequiredToDeclareDrift").GetInt32() == 3
    && driftSpec.GetProperty("precedesEnrichmentClassification").GetBoolean()
    && driftSpec.GetProperty("halfWindowAndSlopeZReported").GetBoolean()
    && driftSpec.GetProperty("decisionSeries").EnumerateArray().Select(x => x.GetString()!).SequenceEqual(expectedDriftDecisionSeries, StringComparer.Ordinal)
    && resourceSpec.GetProperty("forceEvaluationFormula").GetString() == "6*400*(8+1)"
    && resourceSpec.GetProperty("estimatedForceEvaluations").GetInt64() == 21600
    && resourceSpec.GetProperty("maximumForceEvaluations").GetInt64() == 25000
    && resourceSpec.GetProperty("derivedPeakBytes").GetInt64() == 208786812
    && resourceSpec.GetProperty("maximumPeakBytes").GetInt64() == 536870912
    && resourceSpec.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resourceSpec.GetProperty("noDenseHessianAllocated").GetBoolean()
    && resourceSpec.GetProperty("refusalOccursBefore").GetString() == "mesh/replay/RNG allocation"
    && resourceSpec.GetProperty("bindingBytesAndBatteryArraysMayPrecedeRefusal").GetBoolean()
    && resourceSpec.GetProperty("forbiddenAllocationShape").GetString() == "dof*dof"
    && resourceSpec.GetProperty("allocationMenu").EnumerateArray().All(x => x.GetProperty("shape").GetString() != "dof*dof")
    && upstreamSpec.GetProperty("phase548").GetString() == "pilot-executed-diagnostics-invalid"
    && upstreamSpec.GetProperty("phase549").GetString() == "adjudication-confirms-reported-terminal"
    && upstreamSpec.GetProperty("phase550").GetString() == "origin-and-configuration-spectrum-characterized"
    && upstreamSpec.GetProperty("phase551").GetString() == "adjudication-confirms-reported-values"
    && upstreamSpec.GetProperty("phase552").GetString() == "stationary-under-resolved-consistent"
    && upstreamSpec.GetProperty("phase569").GetString() == "adjudication-confirms-boundary-ordering-effect-present-sampler-causality-unresolved"
    && transitionGateSpec.GetProperty("outputField").GetString() == "phase571TransitionProbeGateOpen"
    && transitionGateSpec.GetProperty("opensOnlyOnCleanScientificTerminal").GetBoolean()
    && transitionGateSpec.GetProperty("authorizesOnly").GetString() == "separate prospective transition-probe registration"
    && transitionGateSpec.GetProperty("samplingOrLaunchAuthorized").GetBoolean() == false
    && outputRngSpec.GetProperty("earlyOutputRngUsed").GetBoolean() == false
    && outputRngSpec.GetProperty("successfulReplayRngUsed").GetBoolean()
    && outputRngSpec.GetProperty("replayRngOnly").GetBoolean()
    && actualAuthorityFirewallKeys.SequenceEqual(expectedAuthorityFirewallKeys.Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && authorityFirewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Synthetic controls run before any audited JSON is parsed.
double[] iid = Enumerable.Range(0, 256).Select(i => System.Math.Sin((i + 1) * 1.731) + 0.3 * System.Math.Cos((i + 1) * 0.419)).ToArray();
double[] ar05 = ArSeries(iid, 0.5), ar09 = ArSeries(iid, 0.9), arNegative = ArSeries(iid, -0.5);
double iidLag = LagOne(iid), ar05Lag = LagOne(ar05), ar09Lag = LagOne(ar09), arNegativeLag = LagOne(arNegative);
double driftSlope = LinearSlope(Enumerable.Range(0, 256).Select(i => iid[i] + 0.02 * i).ToArray());
double stationarySlope = LinearSlope(iid);
double[][] iidChains = SyntheticChains(0.0, 0.0, false);
double[][] ar1Chains = SyntheticChains(0.9, 0.0, false);
double[][] separatedChains = SyntheticChains(0.0, 4.0, false);
Diagnostics iidDiagnostics = Diagnose(iidChains);
Diagnostics ar1Diagnostics = Diagnose(ar1Chains);
Diagnostics separatedDiagnostics = Diagnose(separatedChains);
DriftResult stationaryDrift = Drift(iidChains[0], 20, 3.0);
DriftResult plantedDrift = Drift(SyntheticChains(0.0, 0.0, true)[0], 20, 3.0);
JsonElement knownAnswerBands = diagnosticSpec.GetProperty("knownAnswerBands");
JsonElement iidBands = knownAnswerBands.GetProperty("iid");
JsonElement ar1Bands = knownAnswerBands.GetProperty("ar1Phi09");
bool iidAbsoluteBandsPassed = InBand(iidDiagnostics.Rhat, iidBands.GetProperty("rhat"))
    && InBand(iidDiagnostics.BulkEss, iidBands.GetProperty("bulkEss"))
    && InBand(iidDiagnostics.TailEss, iidBands.GetProperty("tailEss"));
bool ar1AbsoluteBandsPassed = InBand(ar1Diagnostics.Rhat, ar1Bands.GetProperty("rhat"))
    && InBand(ar1Diagnostics.BulkEss, ar1Bands.GetProperty("bulkEss"))
    && InBand(ar1Diagnostics.TailEss, ar1Bands.GetProperty("tailEss"));
double ar1BulkDegradationRatio = ar1Diagnostics.BulkEss / iidDiagnostics.BulkEss;
double ar1TailDegradationRatio = ar1Diagnostics.TailEss / iidDiagnostics.TailEss;
bool ar1DegradationBandsPassed = InBand(ar1BulkDegradationRatio, knownAnswerBands.GetProperty("ar1ToIidBulkEssRatio"))
    && InBand(ar1TailDegradationRatio, knownAnswerBands.GetProperty("ar1ToIidTailEssRatio"));

string SelectTerminal(bool invalid, bool batteryFailed, bool laneRefused, bool resourceRefused,
    bool replayFailed, bool incidenceFailed, bool diagnosticsFailed, bool drift, bool closed, bool rankOne)
{
    if (invalid || (closed && rankOne)) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (laneRefused) return taxonomy[2];
    if (resourceRefused) return taxonomy[3];
    if (replayFailed) return taxonomy[4];
    if (incidenceFailed) return taxonomy[5];
    if (diagnosticsFailed) return taxonomy[6];
    if (drift) return taxonomy[7];
    if (closed) return taxonomy[8];
    if (rankOne) return taxonomy[9];
    return taxonomy[10];
}
var classificationTruthTable = new[]
{
    new { id="invalid", actual=SelectTerminal(true,false,false,false,false,false,false,false,false,false), expected=taxonomy[0] },
    new { id="battery", actual=SelectTerminal(false,true,false,false,false,false,false,false,false,false), expected=taxonomy[1] },
    new { id="lane", actual=SelectTerminal(false,false,true,false,false,false,false,false,false,false), expected=taxonomy[2] },
    new { id="resource", actual=SelectTerminal(false,false,false,true,false,false,false,false,false,false), expected=taxonomy[3] },
    new { id="replay", actual=SelectTerminal(false,false,false,false,true,false,false,false,false,false), expected=taxonomy[4] },
    new { id="incidence", actual=SelectTerminal(false,false,false,false,false,true,false,false,false,false), expected=taxonomy[5] },
    new { id="diagnostics", actual=SelectTerminal(false,false,false,false,false,false,true,false,false,false), expected=taxonomy[6] },
    new { id="drift-precedes-enrichment", actual=SelectTerminal(false,false,false,false,false,false,false,true,true,false), expected=taxonomy[7] },
    new { id="closed", actual=SelectTerminal(false,false,false,false,false,false,false,false,true,false), expected=taxonomy[8] },
    new { id="rank-one", actual=SelectTerminal(false,false,false,false,false,false,false,false,false,true), expected=taxonomy[9] },
    new { id="not-localized", actual=SelectTerminal(false,false,false,false,false,false,false,false,false,false), expected=taxonomy[10] },
    new { id="contradictory-enrichment-fails-closed", actual=SelectTerminal(false,false,false,false,false,false,false,false,true,true), expected=taxonomy[0] },
    new { id="early-precedence", actual=SelectTerminal(true,true,true,true,true,true,true,true,true,true), expected=taxonomy[0] },
};
bool classificationTruthTablePassed = classificationTruthTable.All(x => x.actual == x.expected)
    && expectedTaxonomy.All(terminal => classificationTruthTable.Any(x => x.actual == terminal));

double[] projectorVector = [0.7, -0.2, 0.4, 0.9];
double invSqrt2 = 1.0 / System.Math.Sqrt(2.0);
double[][] projectorBasis = [[1, 0, 0, 0], [0, 1, 0, 0]];
double[][] rotatedBasis = [[invSqrt2, invSqrt2, 0, 0], [-invSqrt2, invSqrt2, 0, 0]];
double projectorRotationError = System.Math.Abs(ProjectedNorm(projectorVector, projectorBasis) - ProjectedNorm(projectorVector, rotatedBasis));
double projectorPermutationError = System.Math.Abs(ProjectedNorm(projectorVector, projectorBasis) - ProjectedNorm(projectorVector, [projectorBasis[1], projectorBasis[0]]));

double[,] rankOneGram = GramFromRows([[1.0, 2.0, -1.0], [0.5, 1.0, -0.5], [-0.3, -0.6, 0.3]]);
double[,] transverseGram = GramFromRows([[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 0.25]]);
double[] rankOneEigen = SymmetricEigenvalues3(rankOneGram);
double[] transverseEigen = SymmetricEigenvalues3(transverseGram);
double rankOneDistance = System.Math.Sqrt(System.Math.Max(0.0, rankOneEigen[1] + rankOneEigen[2]));
double transverseDistance = System.Math.Sqrt(System.Math.Max(0.0, transverseEigen[1] + transverseEigen[2]));

byte[] checksumFixture = Encoding.UTF8.GetBytes("{\"phase\":570,\"fixture\":\"checksum\"}");
string checksum = Convert.ToHexString(SHA256.HashData(checksumFixture)).ToLowerInvariant();
byte[] tampered = (byte[])checksumFixture.Clone(); tampered[^2] ^= 1;
bool checksumTamperDetected = checksum != Convert.ToHexString(SHA256.HashData(tampered)).ToLowerInvariant();
bool replayDecoyRejected = !new[] { true, false, true }.SequenceEqual(new[] { true, true, true });
bool knownAnswerPassed = System.Math.Abs(iidLag) < 0.25 && ar05Lag > 0.25 && ar09Lag > ar05Lag
    && arNegativeLag < -0.2 && driftSlope > System.Math.Abs(stationarySlope) + 0.01
    && double.IsFinite(iidDiagnostics.Rhat) && double.IsFinite(iidDiagnostics.BulkEss) && double.IsFinite(iidDiagnostics.TailEss)
    && iidAbsoluteBandsPassed && ar1AbsoluteBandsPassed && ar1DegradationBandsPassed
    && separatedDiagnostics.Rhat > 1.1 && stationaryDrift.Conclusive && !stationaryDrift.Drifts
    && plantedDrift.Conclusive && plantedDrift.Drifts
    && projectorRotationError < 1e-15 && projectorPermutationError == 0.0
    && rankOneDistance < 1e-7 && transverseDistance > 0.5
    && checksumTamperDetected && replayDecoyRejected && classificationTruthTablePassed;
var knownAnswerBattery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    autocorrelation = new { iidLag, ar05Lag, ar09Lag, arNegativeLag, stationarySlope, driftSlope, iidDiagnostics, ar1Diagnostics, separatedDiagnostics, stationaryDrift, plantedDrift, iidAbsoluteBandsPassed, ar1AbsoluteBandsPassed, ar1BulkDegradationRatio, ar1TailDegradationRatio, ar1DegradationBandsPassed, actualDiagnoseSplitRhatEssAndDriftExercised = true },
    projector = new { projectorRotationError, projectorPermutationError },
    rankOneValley = new { rankOneDistance, transverseDistance },
    classificationTruthTable = new { rows = classificationTruthTable, everyTerminalReached = expectedTaxonomy.All(terminal => classificationTruthTable.Any(x => x.actual == terminal)), passed = classificationTruthTablePassed },
    checksumTamperDetected, replayDecoyRejected, passed = knownAnswerPassed,
};

if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {early}");
    return;
}

// Only now parse the exact-bound upstream scientific records.
JsonElement p548 = ReadBinding("phase548-summary");
JsonElement p549 = ReadBinding("phase549-summary");
JsonElement p550 = ReadBinding("phase550-summary");
JsonElement p551 = ReadBinding("phase551-summary");
JsonElement p552 = ReadBinding("phase552-summary");
JsonElement p569 = ReadBinding("phase569-summary");
JsonElement required = contract.GetProperty("requiredUpstreamVerdicts");
bool upstreamGateOpen = p548.GetProperty("verdictKind").GetString() == required.GetProperty("phase548").GetString()
    && p549.GetProperty("verdictKind").GetString() == required.GetProperty("phase549").GetString()
    && p550.GetProperty("verdictKind").GetString() == required.GetProperty("phase550").GetString()
    && p551.GetProperty("verdictKind").GetString() == required.GetProperty("phase551").GetString()
    && p552.GetProperty("verdictKind").GetString() == required.GetProperty("phase552").GetString()
    && p569.GetProperty("verdictKind").GetString() == required.GetProperty("phase569").GetString();
if (!upstreamGateOpen)
{
    Emit(Early(taxonomy[2], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {taxonomy[2]}");
    return;
}

JsonElement phase548Contract = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase548-contract"))).RootElement.Clone();
JsonElement target = phase548Contract.GetProperty("target");
JsonElement defaults = phase548Contract.GetProperty("defaultConfiguration");
int extent = target.GetProperty("extent").GetInt32();
double stepSize = defaults.GetProperty("stepSize").GetDouble();
int leapfrogSteps = defaults.GetProperty("leapfrogSteps").GetInt32();
int warmupPerChain = defaults.GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = defaults.GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = defaults.GetProperty("trajectoriesPerChain").GetInt32();
double divergenceThreshold = defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
var chainPlan = phase548Contract.GetProperty("seedTables").EnumerateArray().SelectMany(table =>
{
    string tableId = table.GetProperty("id").GetString()!;
    int offset = table.GetProperty("seedOffset").GetInt32();
    int[] seeds = table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    double[] scales = table.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    return seeds.Select((seed, i) => new ChainPlan(tableId, seed, seed + offset, scales[i]));
}).ToArray();

long estimatedForceEvaluations = checked((long)chainPlan.Length * trajectoriesPerChain * (leapfrogSteps + 1));
long maximumForceEvaluations = resourceSpec.GetProperty("maximumForceEvaluations").GetInt64();
long maximumPeakBytes = resourceSpec.GetProperty("maximumPeakBytes").GetInt64();
int declaredDof = target.GetProperty("degreesOfFreedom").GetInt32();
const int DeclaredAlgebraDimension = 3, DeclaredSeriesCount = 18, ScalarClosedDimension = 84;
int derivedVertexCount = checked(extent * extent * extent * extent);
bool dofDivisible = declaredDof % DeclaredAlgebraDimension == 0;
int derivedEdgeCount = dofDivisible ? declaredDof / DeclaredAlgebraDimension : 0;
var derivedAllocationMenu = new[]
{
    new AllocationRow("mesh-topology", "edge*256", checked((long)derivedEdgeCount * 256)),
    new AllocationRow("integer-and-numeric-projectors", "(vertex+4)*edge*4 + 84*edge*8", checked((long)(derivedVertexCount + 4) * derivedEdgeCount * sizeof(int) + (long)ScalarClosedDimension * derivedEdgeCount * sizeof(double))),
    new AllocationRow("replay-vectors", "16*dof*8", checked(16L * declaredDof * sizeof(double))),
    new AllocationRow("evaluator-working-reserve", "128*dof*8", checked(128L * declaredDof * sizeof(double))),
    new AllocationRow("retained-series", "6*18*340*8", checked((long)chainPlan.Length * DeclaredSeriesCount * retainedPerChain * sizeof(double))),
    new AllocationRow("telemetry-checkpoint", "6*(400*32+dof*8)", checked((long)chainPlan.Length * (trajectoriesPerChain * 32L + declaredDof * sizeof(double)))),
    new AllocationRow("diagnostics-scratch", "4*6*18*340*8", checked(4L * chainPlan.Length * DeclaredSeriesCount * retainedPerChain * sizeof(double))),
    new AllocationRow("object-overhead-reserve", "64*1024*1024", 64L * 1024 * 1024),
    new AllocationRow("runtime-reserve", "128*1024*1024", 128L * 1024 * 1024),
};
long derivedPeakBytes = checked(derivedAllocationMenu.Sum(x => x.Bytes));
var declaredAllocationMenu = resourceSpec.GetProperty("allocationMenu").EnumerateArray().Select(x => new AllocationRow(
    x.GetProperty("id").GetString()!, x.GetProperty("shape").GetString()!, x.GetProperty("bytes").GetInt64())).ToArray();
bool allocationMenuMatches = declaredAllocationMenu.SequenceEqual(derivedAllocationMenu)
    && declaredAllocationMenu.Length == 9 && declaredAllocationMenu.All(x => x.Shape != "dof*dof");
bool resourceAccepted = resourceSpec.GetProperty("refuseBeforeAllocation").GetBoolean()
    && chainPlan.Length == 6 && trajectoriesPerChain == 400 && leapfrogSteps == 8
    && extent == 3 && derivedVertexCount == 81 && dofDivisible && derivedEdgeCount == 1215 && declaredDof == 3645
    && estimatedForceEvaluations == 21600 && estimatedForceEvaluations <= maximumForceEvaluations
    && allocationMenuMatches && derivedPeakBytes == resourceSpec.GetProperty("derivedPeakBytes").GetInt64()
    && derivedPeakBytes == 208786812 && derivedPeakBytes <= maximumPeakBytes;
if (!resourceAccepted)
{
    Emit(Early(taxonomy[3], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {taxonomy[3]}");
    return;
}

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var mass = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension, edgeCount = mesh.EdgeCount, dof = edgeCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];
(double Action, double[] Gradient) Evaluate(double[] omega)
{
    var joint = op.ComputeJointGradient(omega, thetaZero, mass);
    return (joint.Objective, joint.GradOmega);
}

// Exact scalar incidence decomposition: E=im d0 and W closes the remaining
// four torus cycles. Tensoring with the three algebra axes gives 240+12=252.
var rawEGenerators = new List<int[]>();
for (int vertex = 0; vertex < mesh.VertexCount; vertex++)
{
    var candidate = new int[edgeCount];
    for (int edge = 0; edge < edgeCount; edge++)
        candidate[edge] = (mesh.Edges[edge][1] == vertex ? 1 : 0) - (mesh.Edges[edge][0] == vertex ? 1 : 0);
    rawEGenerators.Add(candidate);
}
var rawWGenerators = new List<int[]>();
for (int axis = 0; axis < 4; axis++)
{
    var candidate = new int[edgeCount];
    for (int edge = 0; edge < edgeCount; edge++)
    {
        var c0 = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
        var c1 = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
        int difference = (int)System.Math.Round(c1[axis] - c0[axis]);
        int wrapped = ((difference % extent) + extent) % extent;
        candidate[edge] = wrapped == extent - 1 ? -1 : wrapped;
    }
    rawWGenerators.Add(candidate);
}
int integerGeneratorsNotClosed = rawEGenerators.Concat(rawWGenerators).Count(x => IntegerClosureResidual(x) != 0);
var eBasis = new List<double[]>();
foreach (int[] raw in rawEGenerators) AddOrthonormal(raw.Select(x => (double)x).ToArray(), eBasis);
var wBasis = new List<double[]>();
foreach (int[] raw in rawWGenerators)
{
    double[] candidate = raw.Select(x => (double)x).ToArray();
    Orthogonalize(candidate, eBasis); AddOrthonormal(candidate, wBasis);
}
double[][] cBasis = [.. eBasis, .. wBasis];
double maximumClosureResidual = cBasis.Max(ClosureResidual);
double eOrthonormalityError = OrthonormalityError(eBasis);
double wOrthonormalityError = OrthonormalityError(wBasis);
double crossOrthogonalityError = CrossOrthogonalityError(eBasis, wBasis);
bool incidenceValid = eBasis.Count == 80 && wBasis.Count == 4 && cBasis.Length == 84
    && integerGeneratorsNotClosed == 0 && maximumClosureResidual <= 2e-13 && eOrthonormalityError <= 2e-13
    && wOrthonormalityError <= 2e-13 && crossOrthogonalityError <= 2e-13
    && dof - cBasis.Length * dimG == 3393
    && p550.GetProperty("structuralPrechecks").GetProperty("measuredNullBasisDimension").GetInt32() == 252;
if (!incidenceValid)
{
    Emit(Early(taxonomy[5], true, true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {taxonomy[5]}");
    return;
}

string[] seriesNames =
[
    "actionDensity", "forceNormSquared", "configurationNormSquared",
    "eNormSquared", "wNormSquared", "closedNormSquared", "closedPerpNormSquared",
    "closedGramLargest", "closedGramMiddle", "closedGramSmallest",
    "closedRankOneAlignment", "withinClosedRankOneDistanceSquared", "withinClosedRankOneRelativeDistance", "fullRankOneDistanceSquared",
    "eMovementSquared", "wMovementSquared", "closedMovementSquared", "closedPerpMovementSquared",
];
double replayTolerance = contract.GetProperty("replay").GetProperty("deltaHRelativeTolerance").GetDouble();
var seriesByChain = new Dictionary<string, Dictionary<string, double[]>>();
var replayRows = new List<object>();
bool replayBitIdentical = true;
foreach (ChainPlan plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    using var telemetry = JsonDocument.Parse(File.ReadAllBytes(PathFor($"phase548-telemetry-{Suffix(chainId)}")));
    var recorded = telemetry.RootElement.GetProperty("rows").EnumerateArray().Select(x => new
    {
        Accepted = x.GetProperty("accepted").GetBoolean(),
        DeltaH = x.GetProperty("deltaH").GetDouble(),
    }).ToArray();
    var rng = new Xoshiro(ExpandSeed((ulong)plan.ExecutionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = plan.InitialScale * Gauss(rng);
    (double Action, double[] Gradient) current = Evaluate(position);
    var series = seriesNames.ToDictionary(x => x, _ => new List<double>());
    double worstDelta = 0.0;
    bool decisionsMatch = true;
    int accepted = 0;
    for (int trajectory = 0; trajectory < trajectoriesPerChain; trajectory++)
    {
        double[] before = (double[])position.Clone();
        var momentum = new double[dof];
        for (int i = 0; i < dof; i++) momentum[i] = Gauss(rng);
        double logUniform = System.Math.Log(Uniform(rng));
        double initialHamiltonian = current.Action + 0.5 * Dot(momentum, momentum);
        var q = (double[])position.Clone();
        var p = (double[])momentum.Clone();
        double action = current.Action;
        double[] gradient = current.Gradient;
        bool finite = true;
        for (int i = 0; i < dof; i++) p[i] -= 0.5 * stepSize * gradient[i];
        for (int leap = 0; leap < leapfrogSteps; leap++)
        {
            for (int i = 0; i < dof; i++) q[i] += stepSize * p[i];
            (action, gradient) = Evaluate(q);
            if (!double.IsFinite(action) || !gradient.All(double.IsFinite) || !q.All(double.IsFinite)) { finite = false; break; }
            double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
            for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
        }
        finite &= p.All(double.IsFinite);
        double finalHamiltonian = finite ? action + 0.5 * Dot(p, p) : double.NaN;
        double deltaH = finalHamiltonian - initialHamiltonian;
        bool divergent = !finite || !double.IsFinite(deltaH) || System.Math.Abs(deltaH) > divergenceThreshold;
        bool accept = finite && !divergent && logUniform <= System.Math.Min(0.0, -deltaH);
        if (accept) { position = q; current = (action, gradient); accepted++; }
        double scale = System.Math.Max(1.0, System.Math.Abs(recorded[trajectory].DeltaH));
        worstDelta = System.Math.Max(worstDelta, System.Math.Abs(deltaH - recorded[trajectory].DeltaH) / scale);
        decisionsMatch &= accept == recorded[trajectory].Accepted;

        if (trajectory >= warmupPerChain)
        {
            InvariantMetrics state = Measure(position);
            double[] movement = Subtract(position, before);
            InvariantMetrics move = Measure(movement);
            series["actionDensity"].Add(current.Action / dof);
            series["forceNormSquared"].Add(Dot(current.Gradient, current.Gradient));
            series["configurationNormSquared"].Add(state.TotalNormSquared);
            series["eNormSquared"].Add(state.ENormSquared);
            series["wNormSquared"].Add(state.WNormSquared);
            series["closedNormSquared"].Add(state.ClosedNormSquared);
            series["closedPerpNormSquared"].Add(state.ClosedPerpNormSquared);
            series["closedGramLargest"].Add(state.GramEigenvalues[0]);
            series["closedGramMiddle"].Add(state.GramEigenvalues[1]);
            series["closedGramSmallest"].Add(state.GramEigenvalues[2]);
            series["closedRankOneAlignment"].Add(state.RankOneAlignment);
            series["withinClosedRankOneDistanceSquared"].Add(state.WithinClosedRankOneDistanceSquared);
            series["withinClosedRankOneRelativeDistance"].Add(state.WithinClosedRankOneRelativeDistance);
            series["fullRankOneDistanceSquared"].Add(state.FullRankOneDistanceSquared);
            series["eMovementSquared"].Add(move.ENormSquared);
            series["wMovementSquared"].Add(move.WNormSquared);
            series["closedMovementSquared"].Add(move.ClosedNormSquared);
            series["closedPerpMovementSquared"].Add(move.ClosedPerpNormSquared);
        }
    }
    using var checkpoint = JsonDocument.Parse(File.ReadAllBytes(PathFor($"phase548-checkpoint-{Suffix(chainId)}")));
    double[] stored = checkpoint.RootElement.GetProperty("payload").GetProperty("position").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    bool finalBitsMatch = stored.Length == position.Length && stored.Zip(position).All(x =>
        BitConverter.DoubleToInt64Bits(x.First) == BitConverter.DoubleToInt64Bits(x.Second));
    bool rowPassed = recorded.Length == trajectoriesPerChain && decisionsMatch && finalBitsMatch
        && worstDelta <= replayTolerance && series.All(x => x.Value.Count == retainedPerChain);
    replayBitIdentical &= rowPassed;
    seriesByChain[chainId] = series.ToDictionary(x => x.Key, x => x.Value.ToArray());
    replayRows.Add(new { chainId, decisionsMatch, finalPositionBitIdentical = finalBitsMatch, worstRelativeDeltaHDeviation = worstDelta, acceptanceRate = (double)accepted / trajectoriesPerChain, retainedDraws = retainedPerChain, passed = rowPassed });
}

if (!replayBitIdentical)
{
    Emit(Early(taxonomy[4], true, true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {taxonomy[4]}");
    return;
}

double maximumRhat = contract.GetProperty("diagnostics").GetProperty("maximumRhat").GetDouble();
double minimumEss = contract.GetProperty("diagnostics").GetProperty("minimumEss").GetDouble();
var tableRows = new List<TableDiagnosticRow>();
foreach (var table in chainPlan.GroupBy(x => x.TableId))
    foreach (string name in seriesNames)
    {
        double[][] chains = table.Select(x => seriesByChain[$"{x.TableId}-{x.RawSeed}"][name]).ToArray();
        Diagnostics d = Diagnose(chains);
        tableRows.Add(new TableDiagnosticRow(table.Key, name, d.Rhat, d.BulkEss, d.TailEss,
            d.Rhat <= maximumRhat && d.BulkEss >= minimumEss && d.TailEss >= minimumEss));
    }

int batchCount = contract.GetProperty("drift").GetProperty("batchCount").GetInt32();
double driftThreshold = contract.GetProperty("drift").GetProperty("absoluteZThreshold").GetDouble();
var lagRows = new List<LagRow>();
var driftRows = new List<DriftRow>();
foreach (var chain in seriesByChain)
    foreach (string name in seriesNames)
    {
        double lag = LagOne(chain.Value[name]);
        DriftResult drift = Drift(chain.Value[name], batchCount, driftThreshold);
        lagRows.Add(new LagRow(chain.Key, name, Reportable(lag), double.IsFinite(lag)));
        driftRows.Add(new DriftRow(chain.Key, name, drift.HalfWindowZ, drift.SlopeZ, drift.Conclusive, drift.Drifts));
    }

bool diagnosticsConclusive = tableRows.All(x => double.IsFinite(x.Rhat) && double.IsFinite(x.BulkEss) && double.IsFinite(x.TailEss))
    && lagRows.All(x => x.Conclusive) && driftRows.All(x => x.Conclusive)
    && driftRows.Count == chainPlan.Length * seriesNames.Length;
if (!diagnosticsConclusive)
{
    Emit(Early(taxonomy[6], true, true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase570 verdict: {taxonomy[6]}");
    return;
}

bool SeriesFailsBothTables(string name) => tableRows.Where(x => x.Series == name).Count() == 2
    && tableRows.Where(x => x.Series == name).All(x => !x.Passed);
bool SeriesPassesBothTables(string name) => tableRows.Where(x => x.Series == name).Count() == 2
    && tableRows.Where(x => x.Series == name).All(x => x.Passed);
double MovementPerDimension(string table, string name, int dimension)
{
    return chainPlan.Where(x => x.TableId == table)
        .Select(x => seriesByChain[$"{x.TableId}-{x.RawSeed}"][name].Average() / dimension).Average();
}
bool movementConcentrated = chainPlan.Select(x => x.TableId).Distinct().All(table =>
    MovementPerDimension(table, "closedMovementSquared", 252)
        <= 0.75 * MovementPerDimension(table, "closedPerpMovementSquared", 3393));
bool closedEnriched = SeriesFailsBothTables("closedNormSquared") && SeriesPassesBothTables("closedPerpNormSquared") && movementConcentrated;
string[] driftDecisionSeries = driftSpec.GetProperty("decisionSeries").EnumerateArray().Select(x => x.GetString()!).ToArray();
int chainsRequiredForDrift = driftSpec.GetProperty("chainsRequiredToDeclareDrift").GetInt32();
var driftCountBySeries = driftDecisionSeries.ToDictionary(name => name,
    name => driftRows.Count(row => row.Series == name && row.Drifts));
bool driftDetected = driftCountBySeries.Values.Any(count => count >= chainsRequiredForDrift);
bool closedScaleStable = SeriesPassesBothTables("closedNormSquared") && SeriesPassesBothTables("closedGramLargest");
bool rankOneEnriched = closedScaleStable
    && SeriesFailsBothTables("withinClosedRankOneDistanceSquared")
    && SeriesFailsBothTables("withinClosedRankOneRelativeDistance");
string verdict = driftDetected ? taxonomy[7]
    : closedEnriched && rankOneEnriched ? taxonomy[0]
    : closedEnriched ? taxonomy[8]
    : rankOneEnriched ? taxonomy[9] : taxonomy[10];
bool scientificTerminal = !driftDetected && taxonomy.Skip(8).Contains(verdict, StringComparer.Ordinal);

var result = new
{
    schemaVersion = 3, phase = 570, phaseId = "phase570-registered-target-directional-resolution-replay",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
    contractValid = true, exactBindingsValid = true, resourceAccepted = true, bindings, knownAnswerBattery,
    upstreamGate = new { open = true, phase548Verdict = p548.GetProperty("verdictKind").GetString(), phase552Verdict = p552.GetProperty("verdictKind").GetString(), phase569Verdict = p569.GetProperty("verdictKind").GetString() },
    incidenceDecomposition = new
    {
        scalarDimensions = new { e = eBasis.Count, w = wBasis.Count, c = cBasis.Length, cPerp = edgeCount - cBasis.Length },
        fullDimensions = new { e = eBasis.Count * dimG, w = wBasis.Count * dimG, c = cBasis.Length * dimG, cPerp = dof - cBasis.Length * dimG },
        integerGeneratorsNotClosed, exactIntegerGeneratorsClosed = integerGeneratorsNotClosed == 0,
        maximumNumericClosureResidual = maximumClosureResidual, numericClosureTolerance = 2e-13,
        eOrthonormalityError, wOrthonormalityError, crossOrthogonalityError,
        phase550CertifiedNullBasisDimension = p550.GetProperty("structuralPrechecks").GetProperty("measuredNullBasisDimension").GetInt32(), passed = incidenceValid,
    },
    replay = new { rows = replayRows, bitIdentical = true, newSamplingPerformed = false, reExecutesAlreadyCommittedTrajectories = true },
    directionalCatalog = new
    {
        series = seriesNames,
        projectorNormsAreBasisRotationAndPermutationInvariant = true,
        closedGramShape = "84x3-coefficients-to-3x3-gram",
        rankOneAlignment = "lambda-largest/trace",
        withinClosedDistanceToRankOneValleySquared = "lambda-middle+lambda-smallest",
        withinClosedRelativeDistanceToRankOneValley = "within-C distance squared/closed trace",
        fullDistanceToClosedRankOneValleySquared = "C-perp norm squared + within-C rank-one distance squared",
    },
    tableDiagnostics = new { thresholds = new { maximumRhat, minimumBulkEss = minimumEss, minimumTailEss = minimumEss }, rows = tableRows },
    lagDiagnostics = new { rows = lagRows },
    driftDiagnostics = new { batchCount, absoluteZThreshold = driftThreshold, chainsRequiredForDrift, decisionSeries = driftDecisionSeries, driftCountBySeries, driftDetected, enrichmentClassificationSuppressedWhenDriftDetected = true, rows = driftRows },
    movementDiagnostics = new
    {
        closedMovementPerDimensionByTable = chainPlan.Select(x => x.TableId).Distinct().ToDictionary(x => x, x => MovementPerDimension(x, "closedMovementSquared", 252)),
        closedPerpMovementPerDimensionByTable = chainPlan.Select(x => x.TableId).Distinct().ToDictionary(x => x, x => MovementPerDimension(x, "closedPerpMovementSquared", 3393)),
        movementConcentrated,
    },
    classification = new { driftDetected, closedEnriched, closedScaleStable, rankOneEnriched, contradictoryEnrichmentFailsClosed = true, rankOneRequiresAbsoluteAndRelativeWithinClosedFailure = true, dimensionShareNull = new { e = 240.0 / dof, w = 12.0 / dof, c = 252.0 / dof, cPerp = 3393.0 / dof }, establishesSamplerCausality = false },
    resource = new { forceEvaluationFormula = "6*400*(8+1)", estimatedForceEvaluations, maximumForceEvaluations, derivedPeakBytes, maximumPeakBytes, allocationMenu = derivedAllocationMenu, allocationMenuMatches, forbiddenAllocationShape = "dof*dof", refusalOccursBefore = "mesh/replay/RNG allocation", bindingBytesAndBatteryArraysMayPrecedeRefusal = true, noDenseHessianAllocated = true },
    verdictKind = verdict, terminalStatus = "registered-target-directional-resolution-replay-" + verdict,
    phase571DesignGateOpen = scientificTerminal, phase571TransitionProbeGateOpen = scientificTerminal, phase572AdjudicationGateOpen = scientificTerminal,
    analysisIsRetrospectiveOnKnownData = true, newSamplingPerformed = false, markovChainAdvancedBeyondCommittedReplay = false,
    rngUsed = true, replayRngAllocated = true, rngUseRestrictedToCommittedReplay = true, configurationsRetained = false,
    phase548Or549TerminalChanged = false, phase548GateRehabilitated = false, registeredBlindSeedTouched = false,
    protectedPhase554SeedsRead = false, directionCalledGaugeOrRedundant = false, quotientApplied = false,
    gaugeFixingApplied = false, measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    allDownstreamAuthority = false, sourceContractApplicationAllowed = false, productionDefaultSelected = false,
    productionAuthorized = false, launchAuthorized = false, physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};
Emit(result);
Console.WriteLine($"Phase570 verdict: {verdict}");

InvariantMetrics Measure(double[] position)
{
    double total = Dot(position, position), eNorm = 0.0, wNorm = 0.0;
    var coefficientRows = new double[cBasis.Length][];
    for (int r = 0; r < cBasis.Length; r++)
    {
        var row = new double[dimG];
        for (int a = 0; a < dimG; a++)
            for (int edge = 0; edge < edgeCount; edge++)
                row[a] += cBasis[r][edge] * position[edge * dimG + a];
        coefficientRows[r] = row;
        double norm = Dot(row, row);
        if (r < eBasis.Count) eNorm += norm; else wNorm += norm;
    }
    double closed = eNorm + wNorm;
    double[] eigen = SymmetricEigenvalues3(GramFromRows(coefficientRows));
    double trace = System.Math.Max(0.0, eigen.Sum());
    double distanceSquared = System.Math.Max(0.0, eigen[1] + eigen[2]);
    double closedPerp = System.Math.Max(0.0, total - closed);
    return new InvariantMetrics(total, eNorm, wNorm, closed, System.Math.Max(0.0, total - closed), eigen,
        trace > 0.0 ? eigen[0] / trace : 1.0, distanceSquared, trace > 0.0 ? distanceSquared / trace : 0.0,
        closedPerp + distanceSquared);
}

long IntegerClosureResidual(int[] scalar)
{
    long maximum = 0;
    for (int face = 0; face < mesh.FaceCount; face++)
    {
        long sum = 0;
        for (int i = 0; i < mesh.FaceBoundaryEdges[face].Length; i++)
            sum += (long)mesh.FaceBoundaryOrientations[face][i] * scalar[mesh.FaceBoundaryEdges[face][i]];
        maximum = System.Math.Max(maximum, System.Math.Abs(sum));
    }
    return maximum;
}

double ClosureResidual(double[] scalar)
{
    double maximum = 0.0;
    for (int face = 0; face < mesh.FaceCount; face++)
    {
        double sum = 0.0;
        for (int i = 0; i < mesh.FaceBoundaryEdges[face].Length; i++)
            sum += mesh.FaceBoundaryOrientations[face][i] * scalar[mesh.FaceBoundaryEdges[face][i]];
        maximum = System.Math.Max(maximum, System.Math.Abs(sum));
    }
    return maximum;
}
JsonElement ReadBinding(string id) => JsonDocument.Parse(File.ReadAllBytes(PathFor(id))).RootElement.Clone();
string PathFor(string id) => specs.Single(x => x.Id == id).Path;
static string Suffix(string chainId) => chainId.Replace("complete-lattice-pilot-", string.Empty, StringComparison.Ordinal);
void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}
object Early(string verdict, bool valid, bool bindingsValid, bool accepted, object bindingRows, object battery) => new
{
    schemaVersion = 3, phase = 570, phaseId = "phase570-registered-target-directional-resolution-replay",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = valid,
    exactBindingsValid = bindingsValid, resourceAccepted = accepted, bindings = bindingRows, knownAnswerBattery = battery,
    verdictKind = verdict, terminalStatus = "registered-target-directional-resolution-replay-" + verdict,
    phase571DesignGateOpen = false, phase571TransitionProbeGateOpen = false, phase572AdjudicationGateOpen = false,
    analysisIsRetrospectiveOnKnownData = true, newSamplingPerformed = false, markovChainAdvancedBeyondCommittedReplay = false,
    rngUsed = false, replayRngAllocated = false, rngUseRestrictedToCommittedReplay = true, configurationsRetained = false,
    phase548Or549TerminalChanged = false, phase548GateRehabilitated = false, registeredBlindSeedTouched = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false, measureNormalizationApplied = false,
    protectedPhase554SeedsRead = false, sourceOrModelSelected = false, phase561Opened = false, o4Discharged = false,
    phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    allDownstreamAuthority = false, sourceContractApplicationAllowed = false, productionDefaultSelected = false,
    productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

static void AddOrthonormal(double[] candidate, List<double[]> basis)
{
    Orthogonalize(candidate, basis); Orthogonalize(candidate, basis);
    double norm = System.Math.Sqrt(Dot(candidate, candidate));
    if (norm <= 1e-10) return;
    for (int i = 0; i < candidate.Length; i++) candidate[i] /= norm;
    basis.Add(candidate);
}
static void Orthogonalize(double[] candidate, IEnumerable<double[]> basis)
{
    foreach (double[] vector in basis)
    {
        double projection = Dot(candidate, vector);
        for (int i = 0; i < candidate.Length; i++) candidate[i] -= projection * vector[i];
    }
}
static double OrthonormalityError(IReadOnlyList<double[]> basis)
{
    double maximum = 0.0;
    for (int i = 0; i < basis.Count; i++) for (int j = 0; j < basis.Count; j++)
        maximum = System.Math.Max(maximum, System.Math.Abs(Dot(basis[i], basis[j]) - (i == j ? 1.0 : 0.0)));
    return maximum;
}
static double CrossOrthogonalityError(IEnumerable<double[]> left, IEnumerable<double[]> right)
{
    double maximum = 0.0;
    foreach (double[] a in left) foreach (double[] b in right) maximum = System.Math.Max(maximum, System.Math.Abs(Dot(a, b)));
    return maximum;
}
static double ProjectedNorm(double[] value, IEnumerable<double[]> basis) => basis.Sum(x => { double c = Dot(value, x); return c * c; });
static double[,] GramFromRows(IEnumerable<double[]> rows)
{
    var gram = new double[3, 3];
    foreach (double[] row in rows) for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++) gram[a, b] += row[a] * row[b];
    return gram;
}
static double[] SymmetricEigenvalues3(double[,] source)
{
    var a = (double[,])source.Clone();
    for (int sweep = 0; sweep < 40; sweep++)
    {
        int p = 0, q = 1;
        if (System.Math.Abs(a[0, 2]) > System.Math.Abs(a[p, q])) { p = 0; q = 2; }
        if (System.Math.Abs(a[1, 2]) > System.Math.Abs(a[p, q])) { p = 1; q = 2; }
        if (System.Math.Abs(a[p, q]) <= 1e-15 * System.Math.Max(1.0, System.Math.Abs(a[p, p]) + System.Math.Abs(a[q, q]))) break;
        double angle = 0.5 * System.Math.Atan2(2.0 * a[p, q], a[q, q] - a[p, p]);
        double c = System.Math.Cos(angle), s = System.Math.Sin(angle);
        for (int k = 0; k < 3; k++) if (k != p && k != q)
        {
            double apk = a[p, k], aqk = a[q, k];
            a[p, k] = a[k, p] = c * apk - s * aqk;
            a[q, k] = a[k, q] = s * apk + c * aqk;
        }
        double app = a[p, p], aqq = a[q, q], apq = a[p, q];
        a[p, p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
        a[q, q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
        a[p, q] = a[q, p] = 0.0;
    }
    double[] values = [a[0, 0], a[1, 1], a[2, 2]];
    Array.Sort(values); Array.Reverse(values); return values;
}
static double[] ArSeries(double[] innovations, double phi)
{
    var result = new double[innovations.Length];
    for (int i = 1; i < result.Length; i++) result[i] = phi * result[i - 1] + innovations[i];
    return result;
}
static double LagOne(double[] values)
{
    double mean = values.Average(), numerator = 0.0, denominator = 0.0;
    for (int i = 0; i < values.Length; i++) { double d = values[i] - mean; denominator += d * d; if (i > 0) numerator += d * (values[i - 1] - mean); }
    return denominator > 0.0 ? numerator / denominator : double.NaN;
}
static double LinearSlope(double[] values)
{
    double mx = (values.Length - 1) / 2.0, my = values.Average(), xy = 0.0, xx = 0.0;
    for (int i = 0; i < values.Length; i++) { xy += (i - mx) * (values[i] - my); xx += (i - mx) * (i - mx); }
    return xy / xx;
}
static DriftResult Drift(double[] values, int batchCount, double threshold)
{
    int batchSize = values.Length / batchCount;
    double[] means = Enumerable.Range(0, batchCount).Select(b => values.Skip(b * batchSize).Take(batchSize).Average()).ToArray();
    int half = batchCount / 2;
    double m0 = means.Take(half).Average(), m1 = means.Skip(half).Average();
    double se = System.Math.Sqrt(Variance(means.Take(half).ToArray()) / half + Variance(means.Skip(half).ToArray()) / half);
    double halfZ = se > 0.0 ? (m1 - m0) / se : double.NaN;
    double slope = LinearSlope(means), mx = (batchCount - 1) / 2.0, mean = means.Average(), residual = 0.0, xx = 0.0;
    for (int i = 0; i < batchCount; i++) { double predicted = mean + slope * (i - mx); residual += (means[i] - predicted) * (means[i] - predicted); xx += (i - mx) * (i - mx); }
    double slopeSe = System.Math.Sqrt(residual / (batchCount - 2) / xx);
    double slopeZ = slopeSe > 0.0 ? slope / slopeSe : double.NaN;
    bool conclusive = double.IsFinite(halfZ) && double.IsFinite(slopeZ);
    return new DriftResult(Reportable(halfZ), Reportable(slopeZ), conclusive, conclusive && (System.Math.Abs(halfZ) > threshold || System.Math.Abs(slopeZ) > threshold));
}
static double Variance(double[] values)
{
    if (values.Length < 2) return 0.0; double mean = values.Average(); return values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1);
}
static double? Reportable(double value) => double.IsFinite(value) ? value : null;
static double[] Subtract(double[] a, double[] b) => a.Zip(b, (x, y) => x - y).ToArray();
static double Dot(double[] a, double[] b) { double sum = 0.0; for (int i = 0; i < a.Length; i++) sum += a[i] * b[i]; return sum; }
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static Diagnostics Diagnose(double[][] chains)
{
    int n = chains.Min(x => x.Length); double[] pooled = chains.SelectMany(x => x.Take(n)).ToArray();
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
static double[][] SyntheticChains(double phi, double separation, bool drifting)
{
    const int chainCount = 4, length = 400;
    var result = new double[chainCount][];
    for (int chain = 0; chain < chainCount; chain++)
    {
        var values = new double[length];
        for (int i = 1; i < length; i++)
        {
            double innovation = StatelessNormal(chain, i);
            values[i] = phi * values[i - 1] + innovation + separation * (chain - 1.5) + (drifting ? 0.015 * i : 0.0);
        }
        result[chain] = values;
    }
    return result;
}
static double StatelessNormal(int chain, int index)
{
    static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
    ulong key = ((ulong)(chain + 1) << 32) | (uint)(index + 1);
    double u1 = ((Mix(key) >> 11) + 0.5) / 9007199254740992.0;
    double u2 = ((Mix(key ^ 0xD1B54A32D192ED03UL) >> 11) + 0.5) / 9007199254740992.0;
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}
static bool InBand(double value, JsonElement band) => double.IsFinite(value)
    && value >= band.GetProperty("minimumInclusive").GetDouble()
    && value <= band.GetProperty("maximumInclusive").GetDouble();
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
static double[] RankNormalize(double[] values)
{
    int n = values.Length; int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray(); var ranks = new double[n];
    for (int i = 0; i < n;) { int j = i; while (j + 1 < n && values[order[j + 1]] == values[order[i]]) j++; double rank = (i + j) / 2.0 + 1.0; for (int k = i; k <= j; k++) ranks[order[k]] = rank; i = j + 1; }
    return ranks.Select(x => InverseNormalCdf((x - 0.375) / (n + 0.25))).ToArray();
}
static double Median(double[] values) { double[] x = values.Order().ToArray(); return x.Length % 2 == 1 ? x[x.Length / 2] : 0.5 * (x[x.Length / 2 - 1] + x[x.Length / 2]); }
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
static double Uniform(Xoshiro rng) => ((rng.Next() >> 11) + 0.5) / 9007199254740992.0;
static double Gauss(Xoshiro rng) { double u1=Uniform(rng),u2=Uniform(rng);return System.Math.Sqrt(-2*System.Math.Log(u1))*System.Math.Cos(2*System.Math.PI*u2); }
static ulong[] ExpandSeed(ulong seed) { ulong state=seed; ulong Next(){state+=0x9E3779B97F4A7C15UL;ulong z=state;z=(z^(z>>30))*0xBF58476D1CE4E5B9UL;z=(z^(z>>27))*0x94D049BB133111EBUL;return z^(z>>31);} return [Next(),Next(),Next(),Next()]; }

sealed record Binding(string Id, string Path, string Hash);
sealed record AllocationRow(string Id, string Shape, long Bytes);
sealed record ChainPlan(string TableId, int RawSeed, int ExecutionSeed, double InitialScale);
sealed record Diagnostics(double Rhat, double BulkEss, double TailEss);
sealed record DriftResult(double? HalfWindowZ, double? SlopeZ, bool Conclusive, bool Drifts);
sealed record LagRow(string ChainId, string Series, double? LagOne, bool Conclusive);
sealed record DriftRow(string ChainId, string Series, double? HalfWindowZ, double? SlopeZ, bool Conclusive, bool Drifts);
sealed record TableDiagnosticRow(string Table, string Series, double Rhat, double BulkEss, double TailEss, bool Passed);
sealed record InvariantMetrics(double TotalNormSquared, double ENormSquared, double WNormSquared, double ClosedNormSquared, double ClosedPerpNormSquared, double[] GramEigenvalues, double RankOneAlignment, double WithinClosedRankOneDistanceSquared, double WithinClosedRankOneRelativeDistance, double FullRankOneDistanceSquared);
sealed class Xoshiro(ulong[] state)
{
    private ulong s0=state[0],s1=state[1],s2=state[2],s3=state[3];
    public ulong Next(){ulong result=RotateLeft(s1*5,7)*9,t=s1<<17;s2^=s0;s3^=s1;s1^=s2;s0^=s3;s2^=t;s3=RotateLeft(s3,45);return result;}
    private static ulong RotateLeft(ulong x,int k)=>(x<<k)|(x>>(64-k));
}
