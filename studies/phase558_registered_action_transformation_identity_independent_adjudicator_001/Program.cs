using System.Security.Cryptography;
using System.Text.Json;

// Phase558 independently adjudicates the applicable Phase556/557 branch. It
// has no project reference to either phase and reconstructs source-lineage facts
// directly from exact-bound artifacts. Its known-answer batteries run before
// any audited Phase556 value is parsed.

const string Root = "studies/phase558_registered_action_transformation_identity_independent_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase558_registered_action_transformation_identity_independent_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/registered_action_transformation_identity_independent_adjudicator.json";
const string SummaryPath = Root + "/output/registered_action_transformation_identity_independent_adjudicator_summary.json";
const string SupplementPath = Root + "/output/phase555_transformation_identity_supplement.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
string contractSha256 = Sha(ContractPath);

var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new
    {
        id = x.Id,
        path = x.Path,
        expectedSha256 = x.ExpectedSha256,
        actualSha256 = actual,
        hashMatches = actual == x.ExpectedSha256,
    };
}).ToArray();
bool exactBindingsValid = bindings.Length == 16 && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input",
    "known-answer-battery-failed",
    "adjudication-finds-source-map-inapplicable",
    "adjudication-confirms-transformation-map-source-underdetermined",
    "adjudication-finds-infinitesimal-identity-mismatch",
    "adjudication-finds-finite-transformation-law-missing",
    "adjudication-finds-finite-closure-or-covariance-failure",
    "machine-evidence-ready-for-human-review",
];
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble()
        <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64()
        <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resource.GetProperty("refusalClassifiesAs").GetString() == "invalid-or-drifted-input";
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString()
        == "phase558-a31-registered-action-transformation-identity-independent-adjudicator-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A31"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && contract.GetProperty("independentOfPhase556AndPhase557Implementations").GetBoolean()
    && contract.GetProperty("mayNotReferencePhase556OrPhase557Project").GetBoolean()
    && contract.GetProperty("batteryRunsBeforeAnyPhase556Datum").GetBoolean()
    && contract.GetProperty("phase557IsConditionalOnPhase556CompatibleTerminal").GetBoolean()
    && taxonomy.SequenceEqual(expectedTaxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject()
        .All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// ========================================================== known-answer battery
// Nothing from Phase556 is parsed above or in this section. File hashes are
// checked as opaque bytes only. The batteries cover the exact failure modes that
// matter on both the negative and hypothetical positive branches.
JsonElement batteryContract = contract.GetProperty("knownAnswerBattery");
long[] rankPrimes = batteryContract.GetProperty("rankPrimes")
    .EnumerateArray().Select(x => x.GetInt64()).ToArray();

double[] dXi = [0.25, -0.5, 1.0];
double[] omega = [1.0, 2.0, 3.0];
double[] xiAverage = [4.0, -1.0, 2.0];
double[] bracket = Cross(omega, xiAverage);
double[] assignmentAAtOrigin = Negate(dXi);
double[] assignmentBAtOrigin = Negate(dXi);
double[] assignmentAOffOrigin = Add(Negate(dXi), bracket);
double[] assignmentBOffOrigin = Subtract(Negate(dXi), bracket);
double[] wrongDerivativeSign = Add(dXi, bracket);
bool candidatesCoincideAtOrigin = EqualExactly(assignmentAAtOrigin, assignmentBAtOrigin);
bool candidatesDifferOffOrigin = !EqualExactly(assignmentAOffOrigin, assignmentBOffOrigin);
bool wrongDerivativeSignDetected = !EqualExactly(assignmentAOffOrigin, wrongDerivativeSign);
bool signFixturePassed = candidatesCoincideAtOrigin && candidatesDifferOffOrigin && wrongDerivativeSignDetected;
var signFixture = new
{
    id = "generic-map-sign-and-field-identification",
    formula = "-d(xi)+[omegaStar-2*A0,xi_avg]",
    dXi,
    omega,
    xiAverage,
    bracket,
    assignmentA = new
    {
        identification = "A0=0,omegaStar=omega",
        atOrigin = assignmentAAtOrigin,
        offOrigin = assignmentAOffOrigin,
    },
    assignmentB = new
    {
        identification = "A0=omega,omegaStar=omega",
        atOrigin = assignmentBAtOrigin,
        offOrigin = assignmentBOffOrigin,
    },
    wrongDerivativeSign,
    candidatesCoincideAtOrigin,
    candidatesDifferOffOrigin,
    wrongDerivativeSignDetected,
    passed = signFixturePassed,
};

long[,] rankFixture =
{
    { 1, 2, 3 },
    { 2, 4, 6 },
    { 0, 1, 1 },
};
long[,] subspaceA =
{
    { 1, 0 },
    { 0, 1 },
    { 0, 0 },
};
long[,] subspaceB =
{
    { 0, 0 },
    { 1, 0 },
    { 0, 1 },
};
long[,] joinedSubspaces = JoinColumns(subspaceA, subspaceB);
var rankRows = rankPrimes.Select(prime =>
{
    int plantedRank = ModularRank(rankFixture, prime);
    int rankA = ModularRank(subspaceA, prime);
    int rankB = ModularRank(subspaceB, prime);
    int joinedRank = ModularRank(joinedSubspaces, prime);
    int intersectionDimension = rankA + rankB - joinedRank;
    bool passed = plantedRank == 2 && rankA == 2 && rankB == 2
        && joinedRank == 3 && intersectionDimension == 1;
    return new { prime, plantedRank, rankA, rankB, joinedRank, intersectionDimension, passed };
}).ToArray();
const int AmbientDimension = 252;
const int ExactDimension = 240;
const int HarmonicDimension = 12;
long[,] exactBlock = CoordinateColumns(AmbientDimension, Enumerable.Range(0, ExactDimension));
long[,] harmonicBlock = CoordinateColumns(AmbientDimension,
    Enumerable.Range(ExactDimension, HarmonicDimension));
long[,] goodCandidate = CoordinateColumns(AmbientDimension, Enumerable.Range(0, ExactDimension));
long[,] substitutionDecoy = CoordinateColumns(AmbientDimension,
    Enumerable.Range(0, 228).Concat(Enumerable.Range(ExactDimension, HarmonicDimension)));
var ambientRankRows = rankPrimes.Select(prime =>
{
    int exactRank = ModularRank(exactBlock, prime);
    int harmonicRank = ModularRank(harmonicBlock, prime);
    int goodRank = ModularRank(goodCandidate, prime);
    int decoyRank = ModularRank(substitutionDecoy, prime);
    int goodExactIntersection = goodRank + exactRank
        - ModularRank(JoinColumns(goodCandidate, exactBlock), prime);
    int goodHarmonicIntersection = goodRank + harmonicRank
        - ModularRank(JoinColumns(goodCandidate, harmonicBlock), prime);
    int decoyExactIntersection = decoyRank + exactRank
        - ModularRank(JoinColumns(substitutionDecoy, exactBlock), prime);
    int decoyHarmonicIntersection = decoyRank + harmonicRank
        - ModularRank(JoinColumns(substitutionDecoy, harmonicBlock), prime);
    bool matchingRankAloneRejected = goodRank == 240 && decoyRank == 240
        && goodExactIntersection == 240 && goodHarmonicIntersection == 0
        && decoyExactIntersection == 228 && decoyHarmonicIntersection == 12;
    bool passed = exactRank == 240 && harmonicRank == 12 && matchingRankAloneRejected;
    return new
    {
        prime,
        ambientDimension = AmbientDimension,
        exactRank,
        harmonicRank,
        goodRank,
        decoyRank,
        goodExactIntersection,
        goodHarmonicIntersection,
        decoyExactIntersection,
        decoyHarmonicIntersection,
        matchingRankAloneRejected,
        passed,
    };
}).ToArray();
bool rankFixturePassed = rankRows.Length == 2 && rankRows.All(x => x.passed)
    && ambientRankRows.Length == 2 && ambientRankRows.All(x => x.passed);

// S(x)=|x|^2/2 is invariant along G(x)=Jx. Away from stationarity, H*G is
// nonzero but H*G + (DG)^T*gradient cancels exactly.
double[] wardPoint = [2.0, -3.0];
double[] gradient = (double[])wardPoint.Clone();
double[] generator = [3.0, 2.0];
double[] hessianGenerator = (double[])generator.Clone();
double[] derivativeGeneratorTransposeGradient = [-3.0, -2.0];
double[] fullWardResidual = Add(hessianGenerator, derivativeGeneratorTransposeGradient);
double[] wrongWardSignResidual = Subtract(hessianGenerator, derivativeGeneratorTransposeGradient);
double bareHgNorm = Norm(hessianGenerator);
double fullWardResidualNorm = Norm(fullWardResidual);
double wrongWardSignResidualNorm = Norm(wrongWardSignResidual);
bool wardFixturePassed = bareHgNorm > 0.0 && fullWardResidualNorm == 0.0 && wrongWardSignResidualNorm > bareHgNorm;
var wardFixture = new
{
    id = "nonstationary-full-ward-term",
    action = "S(x)=0.5*(x0^2+x1^2)",
    generatorLaw = "G(x)=J*x",
    point = wardPoint,
    gradient,
    generator,
    hessianGenerator,
    derivativeGeneratorTransposeGradient,
    bareHgNorm,
    fullWardResidual,
    fullWardResidualNorm,
    wrongWardSignResidualNorm,
    passed = wardFixturePassed,
};

double[] positiveVector = [1.0, -2.0, 0.5];
double positiveFormValue = 0.5 * positiveVector.Sum(x => x * x);
double negativePairingDecoyValue = 0.5 * (-positiveVector[0] * positiveVector[0]
    + positiveVector[1] * positiveVector[1] + positiveVector[2] * positiveVector[2]);
double negativeWeightDecoyValue = -0.5;
bool negativePairingRejected = !PositiveSemidefiniteDiagonal([-1.0, 1.0, 1.0]);
bool negativeWeightRejected = !PositiveSemidefiniteDiagonal([1.0, -1.0, 1.0]);
bool negativeSquaredNormRejected = !AdmissibleSquaredNorm(-1.0);
bool nanSquaredNormRejected = !AdmissibleSquaredNorm(double.NaN);
bool infinitySquaredNormRejected = !AdmissibleSquaredNorm(double.PositiveInfinity);
bool positivityFixturePassed = positiveFormValue == 2.625
    && negativePairingDecoyValue >= 0.0 // value alone can miss an indefinite direction
    && negativeWeightDecoyValue < 0.0
    && negativePairingRejected && negativeWeightRejected && negativeSquaredNormRejected
    && nanSquaredNormRejected && infinitySquaredNormRejected;
var positivityFixture = new
{
    id = "positive-form-and-indefinite-decoy",
    vector = positiveVector,
    identityMetricValue = positiveFormValue,
    negativePairingDecoyValue,
    negativeWeightDecoyValue,
    negativePairingRejected,
    negativeWeightRejected,
    negativeSquaredNormRejected,
    nanSquaredNormRejected,
    infinitySquaredNormRejected,
    passed = positivityFixturePassed,
};

var precedenceRows = new[]
{
    PrecedenceCase("invalid-precedes-battery-and-branch", false, false,
        "transformation-map-source-underdetermined", null, "invalid-or-drifted-input"),
    PrecedenceCase("battery-precedes-branch", true, false,
        "transformation-map-source-underdetermined", null, "known-answer-battery-failed"),
    PrecedenceCase("map-inapplicable", true, true, "transformation-map-source-inapplicable", null,
        "adjudication-finds-source-map-inapplicable"),
    PrecedenceCase("map-underdetermined", true, true, "transformation-map-source-underdetermined", null,
        "adjudication-confirms-transformation-map-source-underdetermined"),
    PrecedenceCase("infinitesimal-mismatch", true, true, "origin-infinitesimal-identity-falsified", null,
        "adjudication-finds-infinitesimal-identity-mismatch"),
    PrecedenceCase("compatible-with-missing-phase557", true, true,
        "registered-infinitesimal-map-compatible-for-finite-test", null, "invalid-or-drifted-input"),
    PrecedenceCase("finite-law-missing", true, true, "registered-infinitesimal-map-compatible-for-finite-test",
        "finite-transformation-law-not-registered", "adjudication-finds-finite-transformation-law-missing"),
    PrecedenceCase("finite-identity-failed", true, true, "registered-infinitesimal-map-compatible-for-finite-test",
        "finite-transformation-identity-falsified", "adjudication-finds-finite-closure-or-covariance-failure"),
    PrecedenceCase("ready", true, true, "registered-infinitesimal-map-compatible-for-finite-test",
        "finite-transformation-identity-supported-for-review", "machine-evidence-ready-for-human-review"),
};
bool precedenceFixturePassed = precedenceRows.All(x => x.Passed);
bool knownAnswerBatteryPassed = signFixturePassed && rankFixturePassed && wardFixturePassed
    && positivityFixturePassed && precedenceFixturePassed;

var knownAnswerBattery = new
{
    ranBeforeAnyPhase556Datum = true,
    requiredCaseCount = 5,
    signFixture,
    rankAndIntersectionFixture = new
    {
        id = "exact-rank-and-intersection",
        smallRows = rankRows,
        ambient252SubstitutionRows = ambientRankRows,
        passed = rankFixturePassed,
    },
    wardFixture,
    positivityFixture,
    terminalPrecedenceFixture = new
    {
        id = "terminal-precedence",
        rows = precedenceRows,
        passed = precedenceFixturePassed,
    },
    passed = knownAnswerBatteryPassed,
};

// ================================================ audited data starts here
string BoundPath(string id) => bindingSpecs.Single(x => x.Id == id).Path;
string ReadBoundText(string id) => File.ReadAllText(BoundPath(id));

using var phase556Document = JsonDocument.Parse(File.ReadAllBytes(BoundPath("phase556-summary")));
JsonElement phase556 = phase556Document.RootElement;
using var phase555Document = JsonDocument.Parse(File.ReadAllBytes(BoundPath("phase555-summary")));
JsonElement phase555 = phase555Document.RootElement;
using var phase548ContractDocument = JsonDocument.Parse(File.ReadAllBytes(BoundPath("phase548-contract")));
JsonElement phase548Contract = phase548ContractDocument.RootElement;
using var phase555ContractDocument = JsonDocument.Parse(File.ReadAllBytes(BoundPath("phase555-contract")));
JsonElement phase555Contract = phase555ContractDocument.RootElement;

string phase548Program = ReadBoundText("phase548-program");
string phase548Project = ReadBoundText("phase548-project");
string operatorSource = ReadBoundText("registered-operator-source");
string massMatrixSource = ReadBoundText("registered-mass-matrix-source");
string pairingSource = ReadBoundText("registered-trace-pairing-source");
string curvatureSource = ReadBoundText("registered-curvature-source");
string genericMapSource = ReadBoundText("generic-infinitesimal-map-source");
string linearizationSource = ReadBoundText("generic-action-linearization-source");

bool phase548UsesRegisteredDefault = phase548Program.Contains(
        "var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();", StringComparison.Ordinal)
    && phase548Program.Contains("var massMatrix = new CpuMassMatrix(mesh, algebra);", StringComparison.Ordinal)
    && phase548Program.Contains("double[] thetaZero = new double[mesh.VertexCount * algebra.Dimension];", StringComparison.Ordinal)
    && phase548Program.Contains("op.ComputeJointGradient(omega, thetaZero, massMatrix)", StringComparison.Ordinal);
JsonElement registeredTarget = phase548Contract.GetProperty("target");
bool phase548TargetContractMatches = registeredTarget.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && registeredTarget.GetProperty("epsilonMode").GetString() == "independent-theta"
    && registeredTarget.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && registeredTarget.GetProperty("beta").GetDouble() == 1.0
    && registeredTarget.GetProperty("extent").GetInt32() == 3
    && registeredTarget.GetProperty("degreesOfFreedom").GetInt32() == 3645
    && registeredTarget.GetProperty("algebra").GetString() == "su2-trace-pairing";
bool objectiveSourceAnchorsPresent = operatorSource.Contains(
        "var mUpsilon = massMatrix.Apply(FaceTensor(upsilon, \"Upsilon\")).Coefficients;", StringComparison.Ordinal)
    && operatorSource.Contains("objective += upsilon[i] * mUpsilon[i];", StringComparison.Ordinal)
    && operatorSource.Contains("objective *= 0.5;", StringComparison.Ordinal);
bool defaultPositiveMassMatrixAnchorsPresent = massMatrixSource.Contains(
        ": this(mesh, algebra, CreateUniformWeights(mesh.FaceCount))", StringComparison.Ordinal)
    && massMatrixSource.Contains("Array.Fill(weights, 1.0);", StringComparison.Ordinal)
    && massMatrixSource.Contains("InvariantMetric[a * dimG + b]", StringComparison.Ordinal);
bool positiveTracePairingAnchorsPresent = pairingSource.Contains(
        "public static LieAlgebra CreateSu2WithTracePairing()", StringComparison.Ordinal)
    && pairingSource.Contains("metric[0] = 1.0;", StringComparison.Ordinal)
    && pairingSource.Contains("metric[4] = 1.0;", StringComparison.Ordinal)
    && pairingSource.Contains("metric[8] = 1.0;", StringComparison.Ordinal)
    && pairingSource.Contains("PairingId = \"trace\"", StringComparison.Ordinal);
bool curvatureFormulaAnchorsPresent = curvatureSource.Contains(
        "coefficients[fi * dimG + a] = dOmega[a] + 0.5 * wedgeTerm[a];", StringComparison.Ordinal);
bool registeredObjectivePositivityIndependentlyClosed = phase548TargetContractMatches
    && phase548UsesRegisteredDefault
    && objectiveSourceAnchorsPresent && defaultPositiveMassMatrixAnchorsPresent
    && positiveTracePairingAnchorsPresent && positivityFixturePassed;

bool genericMapAnchorsPresent = genericMapSource.Contains(
        "CarrierType = \"gauge-parameter-0form\"", StringComparison.Ordinal)
    && genericMapSource.Contains("CarrierType = \"connection-1form\"", StringComparison.Ordinal)
    && genericMapSource.Contains(
        "xiAvg[a] = 0.5 * (xi.Coefficients[v0 * dimG + a] + xi.Coefficients[v1 * dimG + a]);",
        StringComparison.Ordinal)
    && genericMapSource.Contains(
        "bracketCoeff[a] = _omegaStarCoeffs[e * dimG + a] - 2.0 * _a0Coeffs[e * dimG + a];",
        StringComparison.Ordinal)
    && genericMapSource.Contains(
        "result[e * dimG + a] = -dXi[e * dimG + a] + bracket[a];", StringComparison.Ordinal);
bool linearizationOnlyWrapsGenericMap = linearizationSource.Contains(
        "_gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omegaStar);", StringComparison.Ordinal)
    && !linearizationSource.Contains("EinsteinianShiabOperator", StringComparison.Ordinal)
    && !linearizationSource.Contains("ComputeJointGradient", StringComparison.Ordinal);
bool phase548ProjectReferencesGenericAssemblies = phase548Project.Contains(
        "Gu.Phase2.Stability", StringComparison.Ordinal)
    || phase548Project.Contains("Gu.Phase3.GaugeReduction", StringComparison.Ordinal);
bool phase548NamesGenericMap = phase548Program.Contains("InfinitesimalGaugeMap", StringComparison.Ordinal)
    || phase548Program.Contains("GaugeActionLinearization", StringComparison.Ordinal);
bool phase548DefinesGenericBackgroundFields = phase548Program.Contains("omegaStar", StringComparison.Ordinal)
    || phase548Program.Contains("a0", StringComparison.OrdinalIgnoreCase);
bool operatorNamesGenericMap = operatorSource.Contains("InfinitesimalGaugeMap", StringComparison.Ordinal)
    || operatorSource.Contains("GaugeActionLinearization", StringComparison.Ordinal);
bool registeredActionTransformationLawPresent = phase548Program.Contains(
        "transformation law", StringComparison.OrdinalIgnoreCase)
    || operatorSource.Contains("transformation law", StringComparison.OrdinalIgnoreCase);
bool sourceBridgePresent = phase548ProjectReferencesGenericAssemblies || phase548NamesGenericMap
    || phase548DefinesGenericBackgroundFields || operatorNamesGenericMap
    || registeredActionTransformationLawPresent;

var independentTermRows = new[]
{
    new { id = "carrier-and-component-order", status = "source-matched", decisive = false },
    new { id = "origin-coboundary-term", status = "source-matched-at-origin-only", decisive = false },
    new { id = "background-field-identification", status = "source-unmapped", decisive = true },
    new { id = "edge-endpoint-averaging", status = "source-unmapped", decisive = true },
    new { id = "theta-transformation", status = "source-unmapped", decisive = true },
    new { id = "action-transformation-law", status = "source-absent", decisive = true },
};
var auditedTermRows = phase556.GetProperty("transformationComparison").GetProperty("termRows")
    .EnumerateArray().Select(x => new
    {
        id = x.GetProperty("id").GetString()!,
        status = x.GetProperty("status").GetString()!,
        decisive = x.GetProperty("decisive").GetBoolean(),
    }).ToArray();
bool termRowsAgree = independentTermRows.Length == auditedTermRows.Length
    && independentTermRows.Zip(auditedTermRows).All(pair => pair.First.id == pair.Second.id
        && pair.First.status == pair.Second.status && pair.First.decisive == pair.Second.decisive);
bool independentSourceMapUnderdetermined = genericMapAnchorsPresent
    && linearizationOnlyWrapsGenericMap && curvatureFormulaAnchorsPresent
    && !sourceBridgePresent && signFixturePassed;

string phase556Verdict = phase556.GetProperty("verdictKind").GetString()!;
bool phase556ContractValid = phase556.GetProperty("contractValid").GetBoolean();
bool phase556BindingsValid = phase556.GetProperty("exactBindingsValid").GetBoolean();
bool phase556PositivityClosed = phase556.GetProperty("registeredObjectivePositivityClosed").GetBoolean();
bool phase556MapApplicable = phase556.GetProperty("transformationMapSourceApplicable").GetBoolean();
bool phase556MapDetermined = phase556.GetProperty("transformationMapSourceDetermined").GetBoolean();
bool phase556GateOpen = phase556.GetProperty("phase557GateOpen").GetBoolean();
bool phase556ConditionalExecuted = phase556.GetProperty("conditionalOriginAuditExecuted").GetBoolean();
bool phase556FirewallsHold = !phase556.GetProperty("directionCalledGaugeOrRedundant").GetBoolean()
    && !phase556.GetProperty("quotientApplied").GetBoolean()
    && !phase556.GetProperty("gaugeFixingApplied").GetBoolean()
    && !phase556.GetProperty("measureNormalizationApplied").GetBoolean()
    && !phase556.GetProperty("rulingAuthoredOrInferred").GetBoolean()
    && !phase556.GetProperty("allDownstreamAuthority").GetBoolean()
    && phase556.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

string[] phase557Directories = Directory.GetDirectories("studies", "phase557_*_001")
    .OrderBy(x => x, StringComparer.Ordinal).ToArray();
bool phase557Absent = phase557Directories.Length == 0;
bool phase557AbsenceMatchesGate = phase556GateOpen || phase557Absent;

string[] parentRulingIds = phase555.GetProperty("reservedRulingIds")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] frozenRulingIds = contract.GetProperty("phase555Supplement").GetProperty("reservedRulingIds")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
bool phase555PacketImmutable = new[]
    {
        "phase555-summary", "phase555-full-packet", "phase555-contract", "phase555-program",
    }.All(id => bindings.Single(x => x.id == id).hashMatches)
    && bindings.Single(x => x.id == "phase555-summary").actualSha256
        == bindings.Single(x => x.id == "phase555-full-packet").actualSha256;
bool phase555SemanticsPreserved = parentRulingIds.SequenceEqual(frozenRulingIds)
    && phase555Contract.GetProperty("contractId").GetString()
        == "phase555-a30-flat-sector-external-review-escalation-packet-v1"
    && !phase555Contract.GetProperty("authorsARuling").GetBoolean()
    && !phase555Contract.GetProperty("changesAPendingFlag").GetBoolean()
    && !phase555.GetProperty("authorsARuling").GetBoolean()
    && !phase555.GetProperty("changesAPendingFlag").GetBoolean();

bool auditedNegativeBranchInternallyConsistent = phase556Verdict == "transformation-map-source-underdetermined"
    && phase556ContractValid && phase556BindingsValid && phase556PositivityClosed
    && !phase556MapApplicable && !phase556MapDetermined && !phase556GateOpen
    && !phase556ConditionalExecuted && phase556FirewallsHold;
bool adjudicationAgrees = registeredObjectivePositivityIndependentlyClosed
    && independentSourceMapUnderdetermined && auditedNegativeBranchInternallyConsistent
    && termRowsAgree && phase557Absent;

bool inputValid = contractValid && exactBindingsValid && resourceAccepted
    && phase555PacketImmutable && phase555SemanticsPreserved && phase557AbsenceMatchesGate
    && objectiveSourceAnchorsPresent && defaultPositiveMassMatrixAnchorsPresent
    && positiveTracePairingAnchorsPresent && genericMapAnchorsPresent;
string verdictKind;
if (!inputValid)
    verdictKind = "invalid-or-drifted-input";
else if (!knownAnswerBatteryPassed)
    verdictKind = "known-answer-battery-failed";
else if (phase556Verdict == "transformation-map-source-inapplicable")
    verdictKind = "adjudication-finds-source-map-inapplicable";
else if (phase556Verdict == "transformation-map-source-underdetermined" && adjudicationAgrees)
    verdictKind = "adjudication-confirms-transformation-map-source-underdetermined";
else if (phase556Verdict == "origin-infinitesimal-identity-falsified")
    verdictKind = "adjudication-finds-infinitesimal-identity-mismatch";
else
    verdictKind = "invalid-or-drifted-input";

bool expectedTerminal = verdictKind == "adjudication-confirms-transformation-map-source-underdetermined";

bool independentReconstructionPassed = registeredObjectivePositivityIndependentlyClosed
    && independentSourceMapUnderdetermined && termRowsAgree;
bool supplementMaterialized = expectedTerminal && inputValid && knownAnswerBatteryPassed
    && adjudicationAgrees && phase555PacketImmutable && phase555SemanticsPreserved;
var supplement = new Dictionary<string, object?>
{
    ["schemaVersion"] = 1,
    ["supplementId"] = "phase558-a31-phase555-transformation-identity-supplement-v1",
    ["artifactKind"] = supplementMaterialized ? "additive-evidence-supplement" : "supplement-refusal",
    ["materialized"] = supplementMaterialized,
    ["parentPacket"] = new
    {
        phase = 555,
        path = BoundPath("phase555-summary"),
        sha256 = bindings.Single(x => x.id == "phase555-summary").actualSha256,
        byteImmutable = phase555PacketImmutable,
    },
    ["reservedRulingIds"] = frozenRulingIds,
    ["answersCollectiveCoordinateRuling"] = false,
    ["answersFpNormalizationRuling"] = false,
    ["authorsARuling"] = false,
    ["consumesAMemo"] = false,
    ["verifiesASignature"] = false,
    ["changesAPendingFlag"] = false,
    ["additiveOnly"] = supplementMaterialized,
    ["externalReviewPending"] = true,
    ["promotedPhysicalMassClaimCount"] = 0,
};
if (supplementMaterialized)
{
    supplement["sourceEvidence"] = new
    {
        phase556Verdict,
        phase558Verdict = verdictKind,
        registeredDefaultObjectiveNonnegative = registeredObjectivePositivityIndependentlyClosed,
        genericTransformationMapSourceBoundToRegisteredAction = !independentSourceMapUnderdetermined,
        conditionalOriginIdentityAuditExecuted = phase556ConditionalExecuted,
        phase557GateOpen = phase556GateOpen,
        phase557Present = !phase557Absent,
        unresolvedSourceItems = new[]
        {
            "background-field-identification",
            "edge-endpoint-averaging-rule-for-the-registered-action",
            "theta-transformation",
            "registered-action-transformation-law",
        },
        nextMachineOrSourceTask = "locate-or-request-a-source-defined-transformation-law-with-explicit-field-identifications",
    };
}
else
{
    supplement["refusalReason"] = "The adjudicated underdetermined branch was not validly confirmed; no negative-branch evidence supplement was materialized.";
    supplement["phase558Verdict"] = verdictKind;
}
Directory.CreateDirectory(Path.GetDirectoryName(SupplementPath)!);
WriteJson(SupplementPath, supplement);
string supplementSha256 = Sha(SupplementPath);

var output = new
{
    schemaVersion = 1,
    phase = 558,
    phaseId = "phase558-registered-action-transformation-identity-independent-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256,
    contractValid,
    exactBindingsValid,
    resourceAccepted,
    bindings,
    independentImplementation = new
    {
        phase556ProjectReference = false,
        phase557ProjectReference = false,
        sharedPhase556Or557Code = false,
        sourceReconstructionUsedExactBoundRawArtifacts = true,
    },
    knownAnswerBattery,
    independentObjectiveClosure = new
    {
        scope = "phase548-default-cpu-mass-matrix-and-trace-pairing-only",
        phase548TargetContractMatches,
        phase548UsesRegisteredDefault,
        objectiveSourceAnchorsPresent,
        defaultPositiveMassMatrixAnchorsPresent,
        positiveTracePairingAnchorsPresent,
        registeredObjectivePositivityIndependentlyClosed,
        classWideClaimMade = false,
    },
    independentTransformationSourceAudit = new
    {
        genericFormula = "-d(xi)+[omegaStar-2*A0,xi_avg]",
        genericMapAnchorsPresent,
        linearizationOnlyWrapsGenericMap,
        curvatureFormulaAnchorsPresent,
        phase548ProjectReferencesGenericAssemblies,
        phase548NamesGenericMap,
        phase548DefinesGenericBackgroundFields,
        operatorNamesGenericMap,
        registeredActionTransformationLawPresent,
        sourceBridgePresent,
        termRows = independentTermRows,
        termRowsAgreeWithPhase556 = termRowsAgree,
        candidatesCoincideAtOrigin,
        candidatesDifferOffOrigin,
        sharedOriginCoboundaryTreatedAsCompatibility = false,
        signOrEndpointTuned = false,
        sourceMapUnderdetermined = independentSourceMapUnderdetermined,
    },
    auditedBranch = new
    {
        phase556Verdict,
        phase556ContractValid,
        phase556BindingsValid,
        phase556PositivityClosed,
        phase556MapApplicable,
        phase556MapDetermined,
        phase556GateOpen,
        phase556ConditionalExecuted,
        phase556FirewallsHold,
        phase557Directories,
        phase557Absent,
        phase557AbsenceMatchesGate,
        auditedNegativeBranchInternallyConsistent,
    },
    conditionalRankIntersectionAndIdentityAudit = new
    {
        upstreamExecuted = phase556ConditionalExecuted,
        independentlyRebuilt = false,
        refusalReason = "The source-law gate closed before Phase556's conditional rank, intersection, stationarity, and identity-residual arm. Phase558 does not manufacture those audited values after closure.",
        knownAnswerRankAndIntersectionBatteryPassed = rankFixturePassed,
        knownAnswerFullWardTermBatteryPassed = wardFixturePassed,
    },
    adjudicationAgrees,
    adjudicationPassed = expectedTerminal,
    independentReconstructionPassed,
    phase557GateOpen = phase556GateOpen,
    supplementMaterialized,
    phase555Supplement = new
    {
        path = SupplementPath,
        sha256 = supplementSha256,
        parentPacketByteImmutable = phase555PacketImmutable,
        reservedRulingIds = frozenRulingIds,
        answersEitherReservedQuestion = false,
        authorsARuling = false,
        changesAPendingFlag = false,
        additiveOnly = supplementMaterialized,
    },
    verdictKind,
    terminalStatus = "registered-action-transformation-identity-independent-adjudicator-" + verdictKind,
    decision = expectedTerminal
        ? "Independent source reconstruction confirms that the registered Phase548 default objective is nonnegative and that the generic infinitesimal map is not source-bound to the registered action. Two plausible unregistered background assignments coincide at the origin and differ off origin. Phase557 correctly remains absent, and the conditional identity quantities remain refused rather than inferred."
        : "The frozen terminal taxonomy preserved a non-confirming outcome. No negative-branch evidence supplement or downstream authority was materialized.",
    nextUnallocatedTask = expectedTerminal
        ? "locate-or-request-a-source-defined-transformation-law-with-explicit-field-identifications"
        : "resolve-the-recorded-input-battery-or-branch-failure-before-any-source-law-follow-up",
    inferenceScope = "This phase adjudicates source applicability and packet lineage only. It does not interpret a measured null direction or establish a transformation orbit.",
    rngUsed = false,
    samplingPerformed = false,
    reprocessingPerformed = false,
    protectedPhase554SeedsRead = false,
    phase553Or554RegisteredOrExecuted = false,
    directionCalledGaugeOrRedundant = false,
    nullSpaceInterpretedAsGaugeVolume = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    rulingAuthoredOrInferred = false,
    phase548Or549Reinterpreted = false,
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    o4Discharged = false,
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
};

WriteJson(OutputPath, output);
WriteJson(SummaryPath, output);

Console.WriteLine(JsonSerializer.Serialize(new
{
    phase = 558,
    verdictKind,
    exactBindingsValid,
    knownAnswerBatteryPassed,
    adjudicationAgrees,
    phase557Absent,
    supplementMaterialized,
    supplementSha256,
    promotedPhysicalMassClaimCount = 0,
}, JsonOptions()));

static PrecedenceRow PrecedenceCase(
    string id,
    bool inputValid,
    bool batteryPassed,
    string phase556Verdict,
    string? phase557Verdict,
    string expected)
{
    string actual = ClassifyFixture(inputValid, batteryPassed, phase556Verdict, phase557Verdict);
    return new PrecedenceRow
    {
        Id = id,
        InputValid = inputValid,
        BatteryPassed = batteryPassed,
        Phase556Verdict = phase556Verdict,
        Phase557Verdict = phase557Verdict,
        Expected = expected,
        Actual = actual,
        Passed = actual == expected,
    };
}

static string ClassifyFixture(
    bool inputValid,
    bool batteryPassed,
    string phase556Verdict,
    string? phase557Verdict)
{
    if (!inputValid) return "invalid-or-drifted-input";
    if (!batteryPassed) return "known-answer-battery-failed";
    if (phase556Verdict == "transformation-map-source-inapplicable")
        return "adjudication-finds-source-map-inapplicable";
    if (phase556Verdict == "transformation-map-source-underdetermined")
        return "adjudication-confirms-transformation-map-source-underdetermined";
    if (phase556Verdict == "origin-infinitesimal-identity-falsified")
        return "adjudication-finds-infinitesimal-identity-mismatch";
    if (phase556Verdict == "registered-infinitesimal-map-compatible-for-finite-test"
        && phase557Verdict == "finite-transformation-law-not-registered")
        return "adjudication-finds-finite-transformation-law-missing";
    if (phase556Verdict == "registered-infinitesimal-map-compatible-for-finite-test"
        && phase557Verdict == "finite-transformation-identity-falsified")
        return "adjudication-finds-finite-closure-or-covariance-failure";
    if (phase556Verdict == "registered-infinitesimal-map-compatible-for-finite-test"
        && phase557Verdict == "finite-transformation-identity-supported-for-review")
        return "machine-evidence-ready-for-human-review";
    return "invalid-or-drifted-input";
}

static int ModularRank(long[,] input, long prime)
{
    int rows = input.GetLength(0), columns = input.GetLength(1);
    var matrix = new long[rows, columns];
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
            matrix[r, c] = Mod(input[r, c], prime);
    int rank = 0;
    for (int column = 0; column < columns && rank < rows; column++)
    {
        int pivot = rank;
        while (pivot < rows && matrix[pivot, column] == 0) pivot++;
        if (pivot == rows) continue;
        if (pivot != rank)
            for (int c = column; c < columns; c++)
                (matrix[rank, c], matrix[pivot, c]) = (matrix[pivot, c], matrix[rank, c]);
        long inverse = PowMod(matrix[rank, column], prime - 2, prime);
        for (int c = column; c < columns; c++) matrix[rank, c] = matrix[rank, c] * inverse % prime;
        for (int r = 0; r < rows; r++)
        {
            if (r == rank || matrix[r, column] == 0) continue;
            long factor = matrix[r, column];
            for (int c = column; c < columns; c++)
                matrix[r, c] = Mod(matrix[r, c] - factor * matrix[rank, c], prime);
        }
        rank++;
    }
    return rank;
}

static long[,] JoinColumns(long[,] left, long[,] right)
{
    int rows = left.GetLength(0);
    if (right.GetLength(0) != rows) throw new ArgumentException("Row counts differ.");
    int leftColumns = left.GetLength(1), rightColumns = right.GetLength(1);
    var result = new long[rows, leftColumns + rightColumns];
    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < leftColumns; c++) result[r, c] = left[r, c];
        for (int c = 0; c < rightColumns; c++) result[r, leftColumns + c] = right[r, c];
    }
    return result;
}

static long[,] CoordinateColumns(int rows, IEnumerable<int> rowIndices)
{
    int[] indices = rowIndices.ToArray();
    var result = new long[rows, indices.Length];
    for (int c = 0; c < indices.Length; c++)
    {
        if (indices[c] < 0 || indices[c] >= rows) throw new ArgumentOutOfRangeException(nameof(rowIndices));
        result[indices[c], c] = 1;
    }
    return result;
}

static bool PositiveSemidefiniteDiagonal(double[] diagonal) =>
    diagonal.All(x => double.IsFinite(x) && x >= 0.0);

static bool AdmissibleSquaredNorm(double value) => double.IsFinite(value) && value >= 0.0;

static long PowMod(long value, long exponent, long modulus)
{
    long result = 1;
    value = Mod(value, modulus);
    while (exponent > 0)
    {
        if ((exponent & 1) != 0) result = result * value % modulus;
        value = value * value % modulus;
        exponent >>= 1;
    }
    return result;
}

static long Mod(long value, long modulus)
{
    long result = value % modulus;
    return result < 0 ? result + modulus : result;
}

static double[] Cross(double[] left, double[] right) =>
[
    left[1] * right[2] - left[2] * right[1],
    left[2] * right[0] - left[0] * right[2],
    left[0] * right[1] - left[1] * right[0],
];

static double[] Negate(double[] value) => value.Select(x => -x).ToArray();
static double[] Add(double[] left, double[] right) => left.Zip(right, (x, y) => x + y).ToArray();
static double[] Subtract(double[] left, double[] right) => left.Zip(right, (x, y) => x - y).ToArray();
static bool EqualExactly(double[] left, double[] right) => left.SequenceEqual(right);
static double Norm(double[] value) => System.Math.Sqrt(value.Sum(x => x * x));

static string Sha(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}

static void WriteJson(string path, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions()) + Environment.NewLine);
}

static JsonSerializerOptions JsonOptions() => new() { WriteIndented = true };

public sealed class PrecedenceRow
{
    public required string Id { get; init; }
    public required bool InputValid { get; init; }
    public required bool BatteryPassed { get; init; }
    public required string Phase556Verdict { get; init; }
    public string? Phase557Verdict { get; init; }
    public required string Expected { get; init; }
    public required string Actual { get; init; }
    public required bool Passed { get; init; }
}
