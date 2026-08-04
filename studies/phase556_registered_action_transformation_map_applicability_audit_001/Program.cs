using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase556 is a deterministic applicability audit. It closes positivity only
// for the exact Phase548 default mass-matrix path, then asks whether the bound
// generic infinitesimal map is source-defined for that executed action. A
// shared origin coboundary or matching dimension does not open the conditional
// rank/Hessian audit.

const string Root = "studies/phase556_registered_action_transformation_map_applicability_audit_001";
const string ContractPath = Root + "/preregistration/phase556_registered_action_transformation_map_applicability_audit_contract_v1.json";
const string OutputPath = Root + "/output/registered_action_transformation_map_applicability_audit.json";
const string SummaryPath = Root + "/output/registered_action_transformation_map_applicability_audit_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { x.Id, x.Path, x.ExpectedSha256, ActualSha256 = actual, HashMatches = actual == x.ExpectedSha256 };
}).ToArray();
bool exactBindingsValid = bindings.Length == 13 && bindings.All(x => x.HashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement target = contract.GetProperty("registeredTarget");
JsonElement positivityContract = contract.GetProperty("objectivePositivityClosure");
JsonElement comparisonContract = contract.GetProperty("transformationMapComparison");
JsonElement conditionalContract = contract.GetProperty("conditionalOriginAudit");
JsonElement resource = contract.GetProperty("resourceRefusal");

bool schemaContractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase556-a31-registered-action-transformation-map-applicability-audit-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A31"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && taxonomy.SequenceEqual(new[]
    {
        "input-integrity-invalid",
        "registered-objective-positivity-closure-failed",
        "transformation-map-source-inapplicable",
        "transformation-map-source-underdetermined",
        "origin-infinitesimal-identity-falsified",
        "registered-infinitesimal-map-compatible-for-finite-test",
    }, StringComparer.Ordinal)
    && positivityContract.GetProperty("scope").GetString() == "phase548-default-cpu-mass-matrix-and-trace-pairing-only"
    && positivityContract.GetProperty("customWeightConstructorIsOutsideClosure").GetBoolean()
    && comparisonContract.GetProperty("compatibilityRequiresEveryTermSourceBound").GetBoolean()
    && comparisonContract.GetProperty("sharedCoboundaryOrMatchingDimensionIsNotCompatibility").GetBoolean()
    && comparisonContract.GetProperty("signOrEndpointTuningProhibited").GetBoolean()
    && conditionalContract.GetProperty("executeOnlyOnExactSourceCompatibility").GetBoolean()
    && conditionalContract.GetProperty("exactAndHarmonicMustRemainSeparate").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

double estimatedSeconds = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble();
double maximumSeconds = resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble();
long estimatedBytes = resource.GetProperty("estimatedPeakBytes").GetInt64();
long maximumBytes = resource.GetProperty("maximumEstimatedPeakBytes").GetInt64();
bool resourceAccepted = estimatedSeconds <= maximumSeconds && estimatedBytes <= maximumBytes
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resource.GetProperty("noDenseHessianAllocatedUnlessConditionalGateOpens").GetBoolean()
    && resource.GetProperty("phase546SamplerCeilingUntouched").GetBoolean();

string BoundPath(string id) => bindingSpecs.Single(x => x.Id == id).Path;
JsonElement BoundJson(string id) => JsonDocument.Parse(File.ReadAllBytes(BoundPath(id))).RootElement.Clone();
bool upstreamTerminalsValid = false;
object upstream = new { checkedAfterHashValidation = false };
if (exactBindingsValid)
{
    JsonElement phase550 = BoundJson("phase550-summary");
    JsonElement phase551 = BoundJson("phase551-summary");
    JsonElement phase552 = BoundJson("phase552-summary");
    JsonElement phase555 = BoundJson("phase555-summary");
    string terminal550 = phase550.GetProperty("verdictKind").GetString()!;
    string terminal551 = phase551.GetProperty("verdictKind").GetString()!;
    string terminal552 = phase552.GetProperty("verdictKind").GetString()!;
    string terminal555 = phase555.GetProperty("verdictKind").GetString()!;
    JsonElement admissible = contract.GetProperty("admissibleUpstreamTerminals");
    bool Allowed(string phase, string terminal) => admissible.GetProperty(phase).EnumerateArray()
        .Any(x => x.GetString() == terminal);
    upstreamTerminalsValid = Allowed("phase550", terminal550) && Allowed("phase551", terminal551)
        && Allowed("phase552", terminal552) && Allowed("phase555", terminal555);
    upstream = new
    {
        checkedAfterHashValidation = true,
        phase550 = terminal550,
        phase551 = terminal551,
        phase552 = terminal552,
        phase555 = terminal555,
        terminalsAdmissible = upstreamTerminalsValid,
    };
}

bool inputsValid = schemaContractValid && exactBindingsValid && upstreamTerminalsValid && resourceAccepted;
if (!inputsValid)
{
    Write(new
    {
        schemaVersion = 1,
        phase = 556,
        phaseId = "phase556-registered-action-transformation-map-applicability-audit",
        contractId = contract.GetProperty("contractId").GetString(),
        contractSha256 = Sha(ContractPath),
        contractValid = schemaContractValid,
        exactBindingsValid,
        resourceAccepted,
        upstream,
        bindings,
        registeredObjectivePositivityClosed = false,
        transformationMapSourceApplicable = false,
        transformationMapSourceDetermined = false,
        phase557GateOpen = false,
        verdictKind = taxonomy[0],
        terminalStatus = "registered-action-transformation-map-applicability-audit-" + taxonomy[0],
        decision = "The audit refused before allocating the registered lattice because its frozen contract, exact bindings, upstream terminals, or resource ceiling was invalid.",
        conditionalOriginAuditExecuted = false,
        rngUsed = false,
        samplingPerformed = false,
        directionCalledGaugeOrRedundant = false,
        quotientApplied = false,
        measureNormalizationApplied = false,
        o4Discharged = false,
        externalReviewPending = true,
        allDownstreamAuthority = false,
        promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase556 verdict: {taxonomy[0]}");
    return;
}

string operatorSource = File.ReadAllText(BoundPath("phase548-operator-source"));
string massSource = File.ReadAllText(BoundPath("phase548-mass-matrix-source"));
string phase548ProgramSource = File.ReadAllText(BoundPath("phase548-program"));
string phase548ProjectSource = File.ReadAllText(BoundPath("phase548-project"));
string pairingFactorySource = File.ReadAllText(BoundPath("trace-pairing-factory-source"));
string curvatureAssemblerSource = File.ReadAllText(BoundPath("curvature-assembler-source"));
string genericMapSource = File.ReadAllText(BoundPath("generic-infinitesimal-map-source"));
string actionLinearizationSource = File.ReadAllText(BoundPath("generic-action-linearization-source"));

bool AllAnchors(JsonElement anchors, string source) => anchors.EnumerateArray()
    .Select(x => x.GetString()!).All(x => source.Contains(x, StringComparison.Ordinal));
JsonElement positivityAnchors = positivityContract.GetProperty("sourceAnchors");
JsonElement comparisonAnchors = comparisonContract.GetProperty("sourceAnchors");
bool objectiveSourceAnchorsPresent = AllAnchors(positivityAnchors.GetProperty("operator"), operatorSource)
    && AllAnchors(positivityAnchors.GetProperty("massMatrix"), massSource)
    && AllAnchors(positivityAnchors.GetProperty("phase548Program"), phase548ProgramSource)
    && AllAnchors(positivityAnchors.GetProperty("pairingFactory"), pairingFactorySource);
bool comparisonSourceAnchorsPresent = AllAnchors(comparisonAnchors.GetProperty("operator"), operatorSource)
    && AllAnchors(comparisonAnchors.GetProperty("phase548Program"), phase548ProgramSource)
    && AllAnchors(comparisonAnchors.GetProperty("curvatureAssembler"), curvatureAssemblerSource)
    && AllAnchors(comparisonAnchors.GetProperty("genericMap"), genericMapSource)
    && AllAnchors(comparisonAnchors.GetProperty("actionLinearization"), actionLinearizationSource);

if (!objectiveSourceAnchorsPresent || !comparisonSourceAnchorsPresent)
{
    Write(new
    {
        schemaVersion = 1,
        phase = 556,
        phaseId = "phase556-registered-action-transformation-map-applicability-audit",
        contractId = contract.GetProperty("contractId").GetString(),
        contractSha256 = Sha(ContractPath),
        contractValid = schemaContractValid,
        exactBindingsValid,
        resourceAccepted,
        upstream,
        bindings,
        sourceExtraction = new { objectiveSourceAnchorsPresent, comparisonSourceAnchorsPresent },
        registeredObjectivePositivityClosed = false,
        transformationMapSourceApplicable = false,
        transformationMapSourceDetermined = false,
        phase557GateOpen = false,
        verdictKind = taxonomy[0],
        terminalStatus = "registered-action-transformation-map-applicability-audit-" + taxonomy[0],
        decision = "An exact-bound source did not contain the frozen semantic anchors, so the audit refused rather than inferring an implementation law.",
        conditionalOriginAuditExecuted = false,
        rngUsed = false,
        samplingPerformed = false,
        directionCalledGaugeOrRedundant = false,
        quotientApplied = false,
        measureNormalizationApplied = false,
        o4Discharged = false,
        externalReviewPending = true,
        allDownstreamAuthority = false,
        promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase556 verdict: {taxonomy[0]}");
    return;
}

// ---------------- registered objective positivity, scoped to Phase548 defaults
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int extent = target.GetProperty("extent").GetInt32();
JsonElement phase548ContractTarget = BoundJson("phase548-contract").GetProperty("target");
bool frozenPhase548TargetMatches = phase548ContractTarget.GetProperty("extent").GetInt32() == extent
    && phase548ContractTarget.GetProperty("member").GetString() == target.GetProperty("member").GetString()
    && phase548ContractTarget.GetProperty("epsilonMode").GetString() == target.GetProperty("epsilonMode").GetString()
    && phase548ContractTarget.GetProperty("thetaRule").GetString() == target.GetProperty("thetaRule").GetString()
    && phase548ContractTarget.GetProperty("beta").GetDouble() == target.GetProperty("beta").GetDouble()
    && phase548ContractTarget.GetProperty("beta").GetDouble() == 1.0
    && phase548ContractTarget.GetProperty("algebra").GetString() == "su2-trace-pairing";
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var defaultMass = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension;
int connectionDimension = mesh.EdgeCount * dimG;
int faceDimension = mesh.FaceCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];

double[] requiredMetric = positivityContract.GetProperty("requiredPairingMatrix")
    .EnumerateArray().Select(x => x.GetDouble()).ToArray();
bool geometryMatches = mesh.VertexCount == target.GetProperty("expectedVertexCount").GetInt32()
    && mesh.EdgeCount == target.GetProperty("expectedEdgeCount").GetInt32()
    && mesh.FaceCount == target.GetProperty("expectedFaceCount").GetInt32()
    && connectionDimension == target.GetProperty("expectedConnectionDegreesOfFreedom").GetInt32();
bool pairingMatches = algebra.PairingId == positivityContract.GetProperty("requiredPairingId").GetString()
    && algebra.InvariantMetric.SequenceEqual(requiredMetric);
bool pairingPositiveDefinite = IsPositiveDiagonalMetric(algebra.InvariantMetric, dimG);
bool formMetricMatches = defaultMass.FormMetricId == positivityContract.GetProperty("requiredFormMetricId").GetString();

var massProbe = new double[faceDimension];
for (int i = 0; i < massProbe.Length; i++) massProbe[i] = ((i % 17) - 8) * 0.125;
var massTensor = FaceTensor(massProbe, "mass-identity-probe");
double[] appliedMassProbe = defaultMass.Apply(massTensor).Coefficients;
double maximumDefaultMassApplyDeviation = 0.0;
for (int i = 0; i < massProbe.Length; i++)
    maximumDefaultMassApplyDeviation = System.Math.Max(maximumDefaultMassApplyDeviation,
        System.Math.Abs(appliedMassProbe[i] - massProbe[i]));

var probeRows = new List<object>();
double maximumIndependentObjectiveDeviation = 0.0;
double minimumObjective = double.PositiveInfinity;
double maximumUpsilonSquaredNorm = 0.0;
for (int probe = 0; probe < positivityContract.GetProperty("deterministicProbeCount").GetInt32(); probe++)
{
    var omega = new double[connectionDimension];
    if (probe > 0)
        for (int i = 0; i < omega.Length; i++)
            omega[i] = probe * 0.001 * System.Math.Sin((i + 1) * (probe == 1 ? 0.173 : 0.419));
    var joint = op.ComputeJointGradient(omega, thetaZero, defaultMass);
    double[] curvature = CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;
    double[] upsilon = op.ApplyContractionWithTheta(curvature, thetaZero);
    double independentObjective = defaultMass.EvaluateObjective(FaceTensor(upsilon, "Upsilon-independent"));
    double upsilonSquaredNorm = upsilon.Sum(x => x * x);
    maximumUpsilonSquaredNorm = System.Math.Max(maximumUpsilonSquaredNorm, upsilonSquaredNorm);
    double deviation = System.Math.Abs(joint.Objective - independentObjective);
    maximumIndependentObjectiveDeviation = System.Math.Max(maximumIndependentObjectiveDeviation, deviation);
    minimumObjective = System.Math.Min(minimumObjective, joint.Objective);
    probeRows.Add(new { probe, joint.Objective, independentObjective, upsilonSquaredNorm, absoluteDeviation = deviation, nonnegative = joint.Objective >= 0.0 });
}

// Scope decoy: the public custom-weight constructor accepts a negative weight.
// Demonstrating its negative quadratic value prevents this audit from silently
// inflating the exact Phase548 default-path closure into a class-wide theorem.
var customWeights = Enumerable.Repeat(1.0, mesh.FaceCount).ToArray();
customWeights[0] = -1.0;
var negativeWeightMass = new CpuMassMatrix(mesh, algebra, customWeights);
var firstBasis = new double[faceDimension];
firstBasis[0] = 1.0;
double negativeWeightDecoyObjective = negativeWeightMass.EvaluateObjective(FaceTensor(firstBasis, "negative-weight-decoy"));
bool negativeControlsPassed = !IsPositiveDiagonalMetric(new[] { -1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 }, 3)
    && !IsPositiveDiagonalMetric(LieAlgebraFactory.CreateSu2().InvariantMetric, 3)
    && negativeWeightDecoyObjective < 0.0
    && !IsNonnegativeSquaredNorm(-0.5)
    && !IsNonnegativeSquaredNorm(double.NaN);

bool positivityClosurePassed = frozenPhase548TargetMatches && geometryMatches && pairingMatches && pairingPositiveDefinite && formMetricMatches
    && maximumDefaultMassApplyDeviation == 0.0
    && maximumIndependentObjectiveDeviation <= positivityContract.GetProperty("requiredMaximumIndependentObjectiveDeviation").GetDouble()
    && minimumObjective >= 0.0 && maximumUpsilonSquaredNorm > 0.0 && negativeControlsPassed;

var positivityClosure = new
{
    scope = positivityContract.GetProperty("scope").GetString(),
    classWideClaimMade = false,
    frozenPhase548TargetMatches,
    registeredBeta = target.GetProperty("beta").GetDouble(),
    betaEntersObjectiveAsSeparateScale = false,
    betaOneMakesOmittedSeparateScaleNumericallyInert = target.GetProperty("beta").GetDouble() == 1.0,
    geometryMatches,
    formula = positivityContract.GetProperty("registeredFormula").GetString(),
    objectiveSourceAnchorsPresent,
    pairingId = algebra.PairingId,
    pairingMatches,
    pairingPositiveDefinite,
    pairingMatrix = algebra.InvariantMetric,
    formMetricId = defaultMass.FormMetricId,
    formMetricMatches,
    defaultFaceWeightEstablishedByExactBoundSource = true,
    maximumDefaultMassApplyDeviation,
    probeRows,
    maximumIndependentObjectiveDeviation,
    minimumObjective,
    maximumUpsilonSquaredNorm,
    nonzeroUpsilonSumOfSquaresWitnessPresent = maximumUpsilonSquaredNorm > 0.0,
    customWeightConstructorOutsideClosure = true,
    negativeWeightDecoyObjective,
    negativeControlsPassed,
    nonnegativityClosedForRegisteredDefaultPath = positivityClosurePassed,
};

// --------------------------- term-by-term source applicability comparison
bool genericMapMentionsRegisteredOperator = genericMapSource.Contains("EinsteinianShiabOperator", StringComparison.Ordinal);
bool registeredOperatorMentionsGenericMap = operatorSource.Contains("InfinitesimalGaugeMap", StringComparison.Ordinal)
    || operatorSource.Contains("GaugeActionLinearization", StringComparison.Ordinal);
bool registeredOperatorDefinesA0 = operatorSource.Contains(" A0", StringComparison.Ordinal)
    || operatorSource.Contains("a0", StringComparison.Ordinal);
bool genericMapDefinesThetaTransformation = genericMapSource.Contains("theta", StringComparison.OrdinalIgnoreCase);
bool actionLinearizationBindsRegisteredOperator = actionLinearizationSource.Contains("EinsteinianShiabOperator", StringComparison.Ordinal);
bool phase548ProjectReferencesGenericMapAssemblies = phase548ProjectSource.Contains("Gu.Phase2.Stability", StringComparison.Ordinal)
    || phase548ProjectSource.Contains("Gu.Phase3.GaugeReduction", StringComparison.Ordinal);
bool phase548ProgramMentionsGenericMap = phase548ProgramSource.Contains("InfinitesimalGaugeMap", StringComparison.Ordinal)
    || phase548ProgramSource.Contains("GaugeActionLinearization", StringComparison.Ordinal);
bool phase548ProgramOrContractDefinesA0 = phase548ProgramSource.Contains("A0", StringComparison.Ordinal)
    || File.ReadAllText(BoundPath("phase548-contract")).Contains("\"A0\"", StringComparison.Ordinal);
bool sourceBridgePresent = genericMapMentionsRegisteredOperator || registeredOperatorMentionsGenericMap
    || actionLinearizationBindsRegisteredOperator || phase548ProjectReferencesGenericMapAssemblies
    || phase548ProgramMentionsGenericMap;

var termRows = new[]
{
    new { id = "carrier-and-component-order", status = "source-matched", executed = "omega is an edge-major connection 1-form", generic = "the map output is an edge-major connection 1-form", decisive = false },
    new { id = "origin-coboundary-term", status = "source-matched-at-origin-only", executed = "dF_0(delta)=d(delta)", generic = "R_0(xi)=-d(xi) only after unregistered A0=omegaStar=0", decisive = false },
    new { id = "background-field-identification", status = "source-unmapped", executed = "the registered operator has omega and fixed theta but defines no A0", generic = "the map requires both A0 and omegaStar", decisive = true },
    new { id = "edge-endpoint-averaging", status = "source-unmapped", executed = "curvature linearization uses oriented face-boundary brackets", generic = "the bracket uses xi averaged at the edge endpoints", decisive = true },
    new { id = "theta-transformation", status = "source-unmapped", executed = "theta is fixed identically to zero", generic = "the map supplies no source-bound transformation for theta", decisive = true },
    new { id = "action-transformation-law", status = "source-absent", executed = "the objective source defines evaluation and derivatives only", generic = "the wrapper asserts a generic action but binds no registered finite or infinitesimal action law", decisive = true },
};
JsonElement expectedStatuses = comparisonContract.GetProperty("expectedTermStatusesFromBoundSources");
bool termStatusesMatchFrozen = termRows.All(x => expectedStatuses.GetProperty(x.id).GetString() == x.status);
bool everyTermSourceBound = termRows.All(x => x.status == "source-matched");
bool explicitSourceContradiction = false;
bool sourceUnderdetermined = !explicitSourceContradiction && !everyTermSourceBound;
bool sourceCompatibilityExact = comparisonSourceAnchorsPresent && sourceBridgePresent
    && termStatusesMatchFrozen && everyTermSourceBound;

var transformationComparison = new
{
    executedActionVariables = new { connection = "omega", fixedField = "theta=0", a0Registered = registeredOperatorDefinesA0 || phase548ProgramOrContractDefinesA0 },
    genericMapVariables = new { gaugeParameter = "xi", distinguishedConnection = "A0", backgroundConnection = "omegaStar" },
    genericFormula = comparisonContract.GetProperty("genericFormula").GetString(),
    executedInfinitesimalVariation = new
    {
        formula = comparisonContract.GetProperty("executedInfinitesimalVariationFormula").GetString(),
        derivation = "ComputeJointGradient evaluates Upsilon=C(F(omega)), applies the default M, and returns D_F(omega)^T*C^T*M*Upsilon; pairing that gradient with deltaOmega gives the frozen variation formula.",
        genericCandidateSubstitution = comparisonContract.GetProperty("genericCandidateSubstitution").GetString(),
        identityRequiredForApplicability = comparisonContract.GetProperty("requiredRegisteredIdentity").GetString(),
        identityRegisteredByBoundSources = false,
        identityEvaluatedAfterUnregisteredFieldIdentification = false,
    },
    comparisonSourceAnchorsPresent,
    sourceBridgePresent,
    genericMapMentionsRegisteredOperator,
    registeredOperatorMentionsGenericMap,
    actionLinearizationBindsRegisteredOperator,
    phase548ProjectReferencesGenericMapAssemblies,
    phase548ProgramMentionsGenericMap,
    phase548ProgramOrContractDefinesA0,
    genericMapDefinesThetaTransformation,
    termRows,
    termStatusesMatchFrozen,
    everyTermSourceBound,
    explicitSourceContradiction,
    sourceUnderdetermined,
    sourceCompatibilityExact,
    sharedCoboundaryOrMatchingDimensionTreatedAsCompatibility = false,
    signOrEndpointTuned = false,
};

bool conditionalOriginAuditExecuted = sourceCompatibilityExact;
bool conditionalOriginAuditPassed = false;
object conditionalOriginAudit;
if (conditionalOriginAuditExecuted)
{
    long[] rankPrimes = conditionalContract.GetProperty("rankPrimes").EnumerateArray().Select(x => x.GetInt64()).ToArray();
    int[][] exactGenerators = BuildExactGenerators(mesh, dimG);
    int[][] harmonicGenerators = BuildHarmonicGenerators(mesh, dimG, extent);
    int[][] combinedGenerators = [.. exactGenerators, .. harmonicGenerators];
    var ranks = rankPrimes.Select(prime => new
    {
        prime,
        exactRank = ModularRank(exactGenerators, connectionDimension, prime),
        harmonicRank = ModularRank(harmonicGenerators, connectionDimension, prime),
        combinedRank = ModularRank(combinedGenerators, connectionDimension, prime),
    }).ToArray();
    bool ranksAgree = ranks.All(x => x.exactRank == ranks[0].exactRank
        && x.harmonicRank == ranks[0].harmonicRank && x.combinedRank == ranks[0].combinedRank);
    int intersectionDimension = ranks[0].exactRank + ranks[0].harmonicRank - ranks[0].combinedRank;
    var originGradient = op.ComputeJointGradient(new double[connectionDimension], thetaZero, defaultMass);
    double originGradientMax = originGradient.GradOmega.Select(System.Math.Abs).DefaultIfEmpty().Max();
    double h0G0MaximumAbsoluteResidual = 0.0;
    var zeroOmega = new double[connectionDimension];
    foreach (int[] generator in exactGenerators)
    {
        double[] direction = generator.Select(x => (double)x).ToArray();
        double[] linearizedCurvature = op.LinearizeCurvature(zeroOmega, direction);
        double[] contracted = op.ApplyContractionWithTheta(linearizedCurvature, thetaZero);
        double[] weighted = defaultMass.Apply(FaceTensor(contracted, "conditional-H0G0")).Coefficients;
        double[] residual = op.LinearizeCurvatureTranspose(zeroOmega, op.ApplyContractionWithThetaTranspose(weighted, thetaZero));
        foreach (double value in residual)
            h0G0MaximumAbsoluteResidual = System.Math.Max(h0G0MaximumAbsoluteResidual, System.Math.Abs(value));
    }
    conditionalOriginAuditPassed = ranksAgree
        && ranks[0].exactRank == conditionalContract.GetProperty("expectedExactRank").GetInt32()
        && ranks[0].harmonicRank == conditionalContract.GetProperty("expectedHarmonicRank").GetInt32()
        && intersectionDimension == 0 && originGradientMax == 0.0 && h0G0MaximumAbsoluteResidual == 0.0;
    conditionalOriginAudit = new
    {
        executed = true,
        ranks,
        ranksAgree,
        intersectionDimension,
        originStationary = originGradientMax == 0.0,
        originGradientMaximumAbsoluteComponent = originGradientMax,
        h0G0MaximumAbsoluteResidual,
        passed = conditionalOriginAuditPassed,
    };
}
else
{
    conditionalOriginAudit = new
    {
        executed = false,
        refusalReason = "Every semantic term was not source-bound; conditional ranks, intersections, origin stationarity, and H0G0 were not evaluated.",
        exactRank = (int?)null,
        harmonicRank = (int?)null,
        intersectionDimension = (int?)null,
        h0G0MaximumAbsoluteResidual = (double?)null,
        passed = false,
    };
}

string verdict = !positivityClosurePassed ? taxonomy[1]
    : explicitSourceContradiction ? taxonomy[2]
    : sourceUnderdetermined ? taxonomy[3]
    : !conditionalOriginAuditPassed ? taxonomy[4]
    : taxonomy[5];

string decision = verdict switch
{
    "registered-objective-positivity-closure-failed" => "The exact-bound Phase548 default quadratic-form path did not satisfy every frozen positivity obligation, so no transformation comparison is promoted.",
    "transformation-map-source-inapplicable" => "The bound sources explicitly contradict a required semantic identification. The generic map is not applied to the registered action.",
    "transformation-map-source-underdetermined" => "The registered Phase548 default objective is nonnegative, but the bound sources do not identify its omega and fixed-theta variables with the generic map's A0 and omegaStar semantics or register the required endpoint, theta, and action transformation laws. The shared origin coboundary is insufficient, so the conditional origin audit did not run.",
    "origin-infinitesimal-identity-falsified" => "Every source term was bound, but at least one frozen origin stationarity, rank, intersection, or H0G0 identity failed.",
    _ => "Every frozen source identification and conditional origin identity passed; this algebraic terminal opens only a separately preregistered finite-transformation falsifier.",
};

Write(new
{
    schemaVersion = 1,
    phase = 556,
    phaseId = "phase556-registered-action-transformation-map-applicability-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid = schemaContractValid,
    exactBindingsValid,
    resourceAccepted,
    upstream,
    bindings,
    registeredObjectivePositivityClosed = positivityClosurePassed,
    transformationMapSourceApplicable = sourceCompatibilityExact,
    transformationMapSourceDetermined = sourceCompatibilityExact || explicitSourceContradiction,
    phase557GateOpen = verdict == taxonomy[5],
    registeredTarget = new
    {
        extent,
        member = target.GetProperty("member").GetString(),
        epsilonMode = target.GetProperty("epsilonMode").GetString(),
        thetaRule = target.GetProperty("thetaRule").GetString(),
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        faceCount = mesh.FaceCount,
        connectionDimension,
        geometryMatches,
    },
    positivityClosure,
    transformationComparison,
    conditionalOriginAudit,
    conditionalOriginAuditExecuted,
    verdictKind = verdict,
    terminalStatus = "registered-action-transformation-map-applicability-audit-" + verdict,
    decision,
    inferenceScope = "This phase establishes only the registered default objective's nonnegative quadratic-form structure and whether the existing generic infinitesimal map is source-applicable. It does not interpret a null direction.",
    rngUsed = false,
    samplingPerformed = false,
    configurationsRetained = false,
    phase546SamplerCeilingTouched = false,
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
});

Console.WriteLine($"Phase556 verdict: {verdict}");
Console.WriteLine($"positivityClosed={positivityClosurePassed}; sourceCompatibilityExact={sourceCompatibilityExact}; conditionalOriginAuditExecuted={conditionalOriginAuditExecuted}");

FieldTensor FaceTensor(double[] coefficients, string label) => new()
{
    Label = label,
    Signature = op.OutputSignature,
    Coefficients = coefficients,
    Shape = new[] { mesh.FaceCount, dimG },
};

void Write(object output)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(output,
        new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes);
    File.WriteAllBytes(SummaryPath, bytes);
}

static bool IsPositiveDiagonalMetric(double[] metric, int dimension)
{
    for (int row = 0; row < dimension; row++)
        for (int column = 0; column < dimension; column++)
        {
            double expected = row == column ? 1.0 : 0.0;
            if (metric[row * dimension + column] != expected) return false;
        }
    return true;
}

static bool IsNonnegativeSquaredNorm(double value) => value >= 0.0;

static int[][] BuildExactGenerators(SimplicialMesh mesh, int dimG)
{
    var result = new int[mesh.VertexCount * dimG][];
    for (int v = 0; v < mesh.VertexCount; v++)
        for (int a = 0; a < dimG; a++)
        {
            var generator = new int[mesh.EdgeCount * dimG];
            for (int edge = 0; edge < mesh.EdgeCount; edge++)
                generator[edge * dimG + a] = (mesh.Edges[edge][1] == v ? 1 : 0)
                    - (mesh.Edges[edge][0] == v ? 1 : 0);
            result[v * dimG + a] = generator;
        }
    return result;
}

static int[][] BuildHarmonicGenerators(SimplicialMesh mesh, int dimG, int extent)
{
    var result = new int[4 * dimG][];
    for (int axis = 0; axis < 4; axis++)
        for (int a = 0; a < dimG; a++)
        {
            var generator = new int[mesh.EdgeCount * dimG];
            for (int edge = 0; edge < mesh.EdgeCount; edge++)
            {
                ReadOnlySpan<double> c0 = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
                ReadOnlySpan<double> c1 = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
                int difference = (int)System.Math.Round(c1[axis] - c0[axis]);
                int wrapped = ((difference % extent) + extent) % extent;
                generator[edge * dimG + a] = wrapped == extent - 1 ? -1 : wrapped;
            }
            result[axis * dimG + a] = generator;
        }
    return result;
}

static int ModularRank(int[][] rows, int columns, long prime)
{
    long[][] matrix = rows.Select(row => row.Select(x => ((x % prime) + prime) % prime).ToArray()).ToArray();
    int rank = 0;
    for (int column = 0; column < columns && rank < matrix.Length; column++)
    {
        int pivot = rank;
        while (pivot < matrix.Length && matrix[pivot][column] == 0) pivot++;
        if (pivot == matrix.Length) continue;
        (matrix[rank], matrix[pivot]) = (matrix[pivot], matrix[rank]);
        long inverse = ModPow(matrix[rank][column], prime - 2, prime);
        for (int j = column; j < columns; j++) matrix[rank][j] = MulMod(matrix[rank][j], inverse, prime);
        for (int i = 0; i < matrix.Length; i++)
        {
            if (i == rank || matrix[i][column] == 0) continue;
            long factor = matrix[i][column];
            for (int j = column; j < columns; j++)
                matrix[i][j] = (matrix[i][j] - MulMod(factor, matrix[rank][j], prime) + prime) % prime;
        }
        rank++;
    }
    return rank;
}

static long ModPow(long value, long exponent, long prime)
{
    long result = 1;
    while (exponent > 0)
    {
        if ((exponent & 1) != 0) result = MulMod(result, value, prime);
        value = MulMod(value, value, prime);
        exponent >>= 1;
    }
    return result;
}

static long MulMod(long left, long right, long prime) => (long)((System.Numerics.BigInteger)left * right % prime);

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
