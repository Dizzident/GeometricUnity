using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase569 is an independent, no-sampling adjudicator. It intentionally has no
// project reference to Phase567 or Phase568 and reconstructs both curvature
// orders, the Shiab contraction, its transpose, and the full gradients here.

const string Root = "studies/phase569_independent_path_ordered_counterfactual_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase569_independent_path_ordered_counterfactual_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/independent_path_ordered_counterfactual_adjudicator.json";
const string SummaryPath = Root + "/output/independent_path_ordered_counterfactual_adjudicator_summary.json";
const string ProjectPath = Root + "/Phase569IndependentPathOrderedCounterfactualAdjudicator.csproj";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new BindingSpec(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { x.Id, x.Path, ExpectedSha256 = x.Sha256, ActualSha256 = actual, HashMatches = actual == x.Sha256 };
}).ToArray();
bool exactBindingsValid = bindings.All(x => x.HashMatches)
    && bindingSpecs.All(x => !x.Sha256.StartsWith("PENDING-", StringComparison.Ordinal));

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
bool firewallsValid = contract.GetProperty("authorityFirewalls").EnumerateObject()
    .All(x => x.Value.ValueKind == JsonValueKind.False);
string[] requiredFirewallNames =
[
    "mayAnswerO4", "mayApplyQuotient", "mayAuthorizeProduction", "mayCreateOrMutatePhase481Pack",
    "mayGaugeFix", "mayMakeGevClaim", "mayMakePhysicalUnitClaim", "mayMutateCheckpoint",
    "mayMutateFrozenOperator", "mayNormalizeMeasure", "mayOpenPhase561", "mayReplayTrajectory",
    "mayReinterpretPhase548Or549", "maySample", "maySatisfyPhase458", "mayTouchProtectedPhase554Seed",
    "mayUseCheckpointRngState",
];
bool firewallSchemaValid = contract.GetProperty("authorityFirewalls").EnumerateObject()
    .Select(x => x.Name).Order().SequenceEqual(requiredFirewallNames.Order());
bool projectIndependent = !File.ReadAllText(ProjectPath).Contains("phase567", StringComparison.OrdinalIgnoreCase)
    && !File.ReadAllText(ProjectPath).Contains("phase568", StringComparison.OrdinalIgnoreCase);
JsonElement comparisonRule = contract.GetProperty("comparisonRule");
JsonElement resourceRule = contract.GetProperty("resourceRule");
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase569-a35-independent-path-ordered-counterfactual-adjudicator-v1"
    && contract.GetProperty("contractStatus").GetString() == "frozen-before-first-execution"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("independentOfPhase567AndPhase568Implementation").GetBoolean()
    && contract.GetProperty("mayNotReferencePhase567OrPhase568Projects").GetBoolean()
    && contract.GetProperty("auditedValuesReadOnlyAfterBattery").GetBoolean()
    && contract.GetProperty("phase568ResultKnownBeforeContractFreeze").GetBoolean()
    && contract.GetProperty("analysisIsIndependentAdjudicationOfKnownResult").GetBoolean()
    && taxonomy.Length == 10
    && firewallsValid && firewallSchemaValid
    && comparisonRule.GetProperty("everyCheckpointRequired").GetBoolean()
    && comparisonRule.GetProperty("spectraAreAdjudicatedFromPhase568NotRecomputed").GetBoolean()
    && comparisonRule.GetProperty("nonNullDoesNotEstablishSamplerCausality").GetBoolean()
    && resourceRule.GetProperty("maximumCheckpointCount").GetInt32() == 6
    && resourceRule.GetProperty("maximumDenseSpectrumRecomputations").GetInt32() == 0
    && resourceRule.GetProperty("maximumDirectionalFiniteDifferenceChecks").GetInt32() == 24
    && !resourceRule.GetProperty("newSamplingAllowed").GetBoolean()
    && !resourceRule.GetProperty("trajectoryReplayAllowed").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement batteryRule = contract.GetProperty("knownAnswerBattery");
double syntheticTolerance = batteryRule.GetProperty("syntheticTolerance").GetDouble();
double adjointTolerance = batteryRule.GetProperty("adjointTolerance").GetDouble();
double gradientTolerance = batteryRule.GetProperty("directionalGradientTolerance").GetDouble();
double polynomialTolerance = batteryRule.GetProperty("polynomialTolerance").GetDouble();

// ------------------------------------------------ batteries before any audited numeric data
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
double[] x0 = [0.13, -0.21, 0.34];
double[] x1 = [-0.19, 0.27, 0.08];
double[] x2 = [0.31, 0.06, -0.23];
double[] registeredFace = FaceCurvature(algebra, [x0, x1, x2], [0, 1, 2]);
double[] candidateFace = FaceCurvature(algebra, [x0, x1, x2], [0, 2, 1]);
double[] expectedDifference = algebra.Bracket(x2, x1);
double orderIdentityError = VectorScaledError(Subtract(candidateFace, registeredFace), expectedDifference);

double[] c0 = [0.17, 0.0, 0.0];
double[] c1 = [-0.22, 0.0, 0.0];
double[] c2 = [0.09, 0.0, 0.0];
double commutingError = VectorScaledError(
    FaceCurvature(algebra, [c0, c1, c2], [0, 1, 2]),
    FaceCurvature(algebra, [c0, c1, c2], [0, 2, 1]));
double wrongOrderError = VectorScaledError(
    Subtract(FaceCurvature(algebra, [x0, x1, x2], [2, 0, 1]), registeredFace), expectedDifference);

byte[] checksumFixture = Encoding.UTF8.GetBytes("{\"fixture\":\"phase569-checksum\",\"value\":17}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = !SHA256.HashData(checksumFixture).AsSpan().SequenceEqual(SHA256.HashData(tamperedFixture));

var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);
var independent = new IndependentCounterfactual(mesh, algebra, latticePeriod: 3);
int dof = mesh.EdgeCount * algebra.Dimension;
double[] probe = Golden(dof, 0.021, cosine: false);
double[] direction = Normalize(Golden(dof, 1.0, cosine: true));
Evaluation candidateProbe = independent.Evaluate(probe, candidate: true);
double step = 2e-6;
double directionalFiniteDifference = (
    independent.Evaluate(Add(probe, direction, step), candidate: true).Objective
    - independent.Evaluate(Add(probe, direction, -step), candidate: true).Objective) / (2.0 * step);
double directionalAnalytic = Dot(candidateProbe.Gradient, direction);
double directionalGradientError = ScaledError(directionalFiniteDifference, directionalAnalytic);
double wrongGradientSignError = ScaledError(directionalFiniteDifference, -directionalAnalytic);

double[] faceCovector = Golden(mesh.FaceCount * algebra.Dimension, 0.7, cosine: true);
double[] omegaDirection = Golden(dof, 0.4, cosine: false);
double[] jv = independent.CurvatureLinearization(probe, omegaDirection, candidate: true);
double[] jTw = independent.CurvatureTranspose(probe, faceCovector, candidate: true);
double adjointError = ScaledError(Dot(jv, faceCovector), Dot(omegaDirection, jTw));

var components = independent.CurvatureComponents(probe, candidate: true);
double[] uLinear = independent.ApplyContraction(components.Linear);
double[] uQuadratic = independent.ApplyContraction(components.Quadratic);
double polynomialA2 = 0.5 * independent.MetricInnerProduct(uLinear, uLinear);
double polynomialA3 = independent.MetricInnerProduct(uLinear, uQuadratic);
double polynomialA4 = 0.5 * independent.MetricInnerProduct(uQuadratic, uQuadratic);
double polynomialError = 0.0;
foreach (double t in new[] { -0.7, 0.23, 0.61, 1.37 })
{
    double observed = independent.Evaluate(Scale(probe, t), candidate: true).Objective;
    double predicted = polynomialA2 * t * t + polynomialA3 * t * t * t + polynomialA4 * t * t * t * t;
    polynomialError = System.Math.Max(polynomialError, ScaledError(observed, predicted));
}

bool batteryPassed = orderIdentityError <= syntheticTolerance
    && commutingError <= syntheticTolerance
    && wrongOrderError > 1e-4
    && checksumTamperDetected
    && adjointError <= adjointTolerance
    && directionalGradientError <= gradientTolerance
    && wrongGradientSignError > 1e-4
    && polynomialError <= polynomialTolerance;

var battery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    checksumControl = new { tamperDetected = checksumTamperDetected, passed = checksumTamperDetected },
    orderControl = new
    {
        orderIdentityError,
        commutingError,
        wrongOrderError,
        predictedDifference = "candidate-minus-registered-equals-bracket-x2-x1",
        passed = orderIdentityError <= syntheticTolerance && commutingError <= syntheticTolerance && wrongOrderError > 1e-4,
    },
    derivativeControl = new
    {
        adjointError,
        directionalFiniteDifference,
        directionalAnalytic,
        directionalGradientError,
        wrongGradientSignError,
        passed = adjointError <= adjointTolerance && directionalGradientError <= gradientTolerance && wrongGradientSignError > 1e-4,
    },
    polynomialControl = new { polynomialA2, polynomialA3, polynomialA4, polynomialError, passed = polynomialError <= polynomialTolerance },
    passed = batteryPassed,
};

// Defaults used on an early fail-closed branch. Audited JSON is parsed only if
// the contract, bindings, independence check, and planted battery are valid.
string phase567Verdict = "not-read";
string phase568Verdict = "not-read";
bool phase567GateOpen = false;
bool upstreamCandidateGateOpen = false;
bool checkpointsValid = false;
bool upstreamAgreement = false;
bool classificationConclusive = false;
bool counterfactualNull = false;
var checkpointAudit = new List<object>();
var rows = new List<ComparisonRow>();
var spectraAdjudication = new List<object>();

bool preliminaryGate = contractValid && exactBindingsValid && batteryPassed && projectIndependent;
if (preliminaryGate)
{
    // ------------------------------------------------ first audited reads
    using var p567Document = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase567-summary")));
    using var p568Document = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase568-summary")));
    JsonElement p567 = p567Document.RootElement;
    JsonElement p568 = p568Document.RootElement;
    phase567Verdict = p567.GetProperty("verdictKind").GetString()!;
    phase568Verdict = p568.GetProperty("verdictKind").GetString()!;
    phase567GateOpen = p567.GetProperty("phase568EvaluationGateOpen").GetBoolean();
    string[] required567 = contract.GetProperty("requiredUpstreamVerdicts").GetProperty("phase567")
        .EnumerateArray().Select(x => x.GetString()!).ToArray();
    string[] required568 = contract.GetProperty("requiredUpstreamVerdicts").GetProperty("phase568")
        .EnumerateArray().Select(x => x.GetString()!).ToArray();
    upstreamCandidateGateOpen = phase567GateOpen
        && required567.Contains(phase567Verdict)
        && required568.Contains(phase568Verdict)
        && p567.GetProperty("candidateSpecification").GetProperty("candidateId").GetString() == "a35-path-ordered-bch2-v1"
        && p568.GetProperty("contractId").GetString() == "phase568-a35-path-ordered-curvature-downstream-counterfactual-audit-v3"
        && p568.GetProperty("contractSha256").GetString() == bindingSpecs.Single(x => x.Id == "phase568-contract-v3").Sha256
        && p568.GetProperty("contractValid").GetBoolean()
        && p568.GetProperty("exactBindingsValid").GetBoolean()
        && p568.GetProperty("candidateDefinitionValid").GetBoolean()
        && p568.GetProperty("resourceAccepted").GetBoolean();

    if (upstreamCandidateGateOpen)
    {
        JsonElement checkpointRule = contract.GetProperty("checkpointRule");
        string[] checkpointIds =
        [
            "checkpoint-a-546101", "checkpoint-a-546103", "checkpoint-a-546107",
            "checkpoint-b-546201", "checkpoint-b-546203", "checkpoint-b-546207",
        ];
        var expectedExecutionSeeds = new Dictionary<string, int>
        {
            ["complete-lattice-pilot-a-546101"] = 2546101,
            ["complete-lattice-pilot-a-546103"] = 2546103,
            ["complete-lattice-pilot-a-546107"] = 2546107,
            ["complete-lattice-pilot-b-546201"] = 2546201,
            ["complete-lattice-pilot-b-546203"] = 2546203,
            ["complete-lattice-pilot-b-546207"] = 2546207,
        };
        var positions = new List<(string Id, double[] Position)>();
        checkpointsValid = true;
        foreach (string bindingId in checkpointIds)
        {
            string path = PathFor(bindingId);
            using var checkpointDocument = JsonDocument.Parse(File.ReadAllBytes(path));
            JsonElement root = checkpointDocument.RootElement;
            JsonElement payload = root.GetProperty("payload");
            string payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload.GetRawText()))).ToLowerInvariant();
            bool checksumOk = root.GetProperty("checksumAlgorithm").GetString() == checkpointRule.GetProperty("checksumAlgorithm").GetString()
                && CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(payloadHash),
                    Encoding.ASCII.GetBytes(root.GetProperty("payloadSha256").GetString()!));
            string chainId = payload.GetProperty("chainId").GetString()!;
            double[] position = payload.GetProperty("position").EnumerateArray().Select(x => x.GetDouble()).ToArray();
            bool headerOk = payload.GetProperty("formatId").GetString() == checkpointRule.GetProperty("formatId").GetString()
                && payload.GetProperty("actionFingerprint").GetString() == checkpointRule.GetProperty("expectedActionFingerprint").GetString()
                && payload.GetProperty("packFingerprint").GetString() == checkpointRule.GetProperty("expectedPackFingerprint").GetString()
                && payload.GetProperty("topologyId").GetString() == checkpointRule.GetProperty("topologyId").GetString()
                && payload.GetProperty("extent").GetInt32() == checkpointRule.GetProperty("extent").GetInt32()
                && payload.GetProperty("dimensions").GetInt32() == checkpointRule.GetProperty("dimensions").GetInt32()
                && payload.GetProperty("degreesOfFreedom").GetInt32() == checkpointRule.GetProperty("degreesOfFreedom").GetInt32()
                && payload.GetProperty("trajectoryIndex").GetInt32() == checkpointRule.GetProperty("trajectoryIndex").GetInt32()
                && payload.GetProperty("warmupCompleted").GetInt32() == checkpointRule.GetProperty("warmupCompleted").GetInt32()
                && payload.GetProperty("retainedCompleted").GetInt32() == checkpointRule.GetProperty("retainedCompleted").GetInt32()
                && expectedExecutionSeeds.TryGetValue(chainId, out int expectedSeed)
                && payload.GetProperty("seed").GetInt32() == expectedSeed
                && payload.GetProperty("stepSize").GetDouble() == checkpointRule.GetProperty("stepSize").GetDouble()
                && payload.GetProperty("leapfrogSteps").GetInt32() == checkpointRule.GetProperty("leapfrogSteps").GetInt32()
                && payload.GetProperty("beta").GetDouble() == checkpointRule.GetProperty("beta").GetDouble()
                && payload.GetProperty("thetaRule").GetString() == checkpointRule.GetProperty("thetaRule").GetString()
                && position.Length == dof && position.All(double.IsFinite);
            bool expectedChain = bindingId.EndsWith(chainId.Replace("complete-lattice-pilot-", ""), StringComparison.Ordinal);
            bool passed = checksumOk && headerOk && expectedChain;
            checkpointsValid &= passed;
            checkpointAudit.Add(new { bindingId, chainId, checksumOk, headerOk, expectedChain, positionLength = position.Length, passed });
            if (passed) positions.Add((chainId, position));
        }

        if (checkpointsValid && positions.Count == 6)
        {
            rows.Add(Compare("origin", new double[dof], independent));
            rows.AddRange(positions.Select(x => Compare(x.Id, x.Position, independent)));

            JsonElement[] upstreamRows = p568.GetProperty("counterfactual").GetProperty("rows").EnumerateArray().ToArray();
            double agreementTolerance = contract.GetProperty("comparisonRule").GetProperty("upstreamAgreementTolerance").GetDouble();
            upstreamAgreement = true;
            foreach (ComparisonRow row in rows.Where(x => x.Id != "origin"))
            {
                JsonElement match = upstreamRows.FirstOrDefault(x => x.GetProperty("Id").GetString() == row.Id);
                bool found = match.ValueKind != JsonValueKind.Undefined;
                bool numericAgreement = found
                    && ScaledError(row.RegisteredAction, match.GetProperty("RegisteredAction").GetDouble()) <= agreementTolerance
                    && ScaledError(row.CandidateAction, match.GetProperty("CandidateAction").GetDouble()) <= agreementTolerance
                    && ScaledError(row.RegisteredGradientNormSquared, match.GetProperty("RegisteredGradientNormSquared").GetDouble()) <= agreementTolerance
                    && ScaledError(row.CandidateGradientNormSquared, match.GetProperty("CandidateGradientNormSquared").GetDouble()) <= agreementTolerance
                    && ScaledError(row.GradientCosine, match.GetProperty("GradientCosine").GetDouble()) <= agreementTolerance
                    && ScaledError(row.ActionScaledDifference, match.GetProperty("ActionScaledDifference").GetDouble()) <= agreementTolerance
                    && ScaledError(row.GradientScaledDifference, match.GetProperty("GradientScaledDifference").GetDouble()) <= agreementTolerance;
                upstreamAgreement &= numericAgreement;
                spectraAdjudication.Add(new
                {
                    row.Id,
                    upstreamRowFound = found,
                    registeredNegativeInertiaCount = found ? match.GetProperty("RegisteredNegativeInertiaCount").GetInt32() : (int?)null,
                    candidateNegativeInertiaCount = found ? match.GetProperty("CandidateNegativeInertiaCount").GetInt32() : (int?)null,
                    candidateHessianSymmetric = found && match.GetProperty("CandidateHessianSymmetric").GetBoolean(),
                    candidateSpectrumValidated = found && match.GetProperty("CandidateSpectrumValidated").GetBoolean(),
                    independentlyRecomputedDenseSpectrum = false,
                    numericActionAndGradientAgreement = numericAgreement,
                });
            }
            bool spectraValid = spectraAdjudication.Count == 6 && spectraAdjudication.All(x =>
            {
                JsonElement json = JsonSerializer.SerializeToElement(x);
                return json.GetProperty("upstreamRowFound").GetBoolean()
                    && json.GetProperty("candidateHessianSymmetric").GetBoolean()
                    && json.GetProperty("candidateSpectrumValidated").GetBoolean();
            });
            classificationConclusive = upstreamAgreement && spectraValid && rows.All(x => x.Finite);
            double equalityTolerance = contract.GetProperty("comparisonRule").GetProperty("numericalEqualityTolerance").GetDouble();
            counterfactualNull = classificationConclusive && rows.All(x =>
                x.CurvatureScaledDifference <= equalityTolerance
                && x.ActionScaledDifference <= equalityTolerance
                && x.GradientScaledDifference <= equalityTolerance);
        }
    }
}

string verdict = !exactBindingsValid ? taxonomy[0]
    : !contractValid || !firewallsValid || !firewallSchemaValid ? taxonomy[1]
    : !batteryPassed ? taxonomy[2]
    : !projectIndependent ? taxonomy[3]
    : !upstreamCandidateGateOpen ? taxonomy[4]
    : !checkpointsValid ? taxonomy[5]
    : !upstreamAgreement ? taxonomy[6]
    : !classificationConclusive ? taxonomy[7]
    : counterfactualNull ? taxonomy[8]
    : taxonomy[9];

var output = new
{
    schemaVersion = 1,
    phase = 569,
    phaseId = "phase569-independent-path-ordered-counterfactual-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    bindings,
    independence = new
    {
        referencesPhase567Project = false,
        referencesPhase568Project = false,
        sharesPhase567Or568Implementation = false,
        projectIndependent,
        curvatureReconstructedFromIncidence = true,
        contractionAndTransposeReconstructed = true,
        fullGradientsReconstructed = true,
        denseSpectraRecomputed = false,
    },
    knownAnswerBattery = battery,
    upstreamGate = new { phase567Verdict, phase567GateOpen, phase568Verdict, upstreamCandidateGateOpen },
    checkpointAudit = new { rows = checkpointAudit, passed = checkpointsValid, rngStateUsed = false },
    counterfactual = new
    {
        rows,
        everyPreservedPositionCompared = rows.Count(x => x.Id != "origin") == 6,
        upstreamActionAndGradientAgreement = upstreamAgreement,
        counterfactualNull,
        nonNullEstablishesSamplerCausality = false,
    },
    spectraAdjudication = new
    {
        rows = spectraAdjudication,
        denseEigensolveRepeated = false,
        phase568SpectraAdjudicatedOnly = true,
        classificationConclusive,
    },
    resource = new
    {
        checkpointCount = checkpointAudit.Count,
        denseSpectrumRecomputationCount = 0,
        hmcTrajectoryCount = 0,
        samplingCount = 0,
        withinFrozenResourceRule = checkpointAudit.Count <= contract.GetProperty("resourceRule").GetProperty("maximumCheckpointCount").GetInt32(),
    },
    verdictKind = verdict,
    terminalStatus = "independent-path-ordered-counterfactual-adjudicator-" + verdict,
    decision = verdict == taxonomy[8]
        ? "The independent reconstruction finds no ordering-dependent curvature, action, or gradient difference above the frozen numerical equality bound on the origin and all six preserved positions. Boundary ordering is excluded only at this audited scope."
        : verdict == taxonomy[9]
            ? "The independent reconstruction confirms a non-null ordering-dependent curvature, action, or gradient counterfactual on the preserved positions and agrees with the downstream audit. Because those positions were generated under the frozen target, sampler causality remains unresolved and requires a separate prospective pack."
            : "The independent adjudication stopped at the earliest frozen fail-closed condition; no downstream interpretation is authorized.",
    scope = new
    {
        preservedPositionsWereGeneratedUnderRegisteredTarget = true,
        establishesCandidateConvergence = false,
        explainsPhase548DiagnosticFailure = false,
        changesPhase548Or549Terminal = false,
        workbenchModelCandidateOnly = true,
        workbenchRelativeLatticeUnitsOnly = true,
    },
    rngUsed = false,
    samplingPerformed = false,
    trajectoryReplayPerformed = false,
    registeredBlindSeedTouched = false,
    checkpointMutated = false,
    frozenOperatorMutated = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    phase548Or549Reinterpreted = false,
    phase561Opened = false,
    o4Discharged = false,
    phase458Satisfied = false,
    phase481PackCreatedOrMutated = false,
    productionAuthorized = false,
    launchAuthorized = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0,
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
byte[] encoded = JsonSerializer.SerializeToUtf8Bytes(output, jsonOptions);
File.WriteAllBytes(OutputPath, encoded);
File.WriteAllBytes(SummaryPath, encoded);
Console.WriteLine(JsonSerializer.Serialize(output, jsonOptions));

string PathFor(string id) => bindingSpecs.Single(x => x.Id == id).Path;

ComparisonRow Compare(string id, double[] position, IndependentCounterfactual evaluator)
{
    Evaluation registered = evaluator.Evaluate(position, candidate: false);
    Evaluation candidate = evaluator.Evaluate(position, candidate: true);
    double registeredGradientNormSquared = Dot(registered.Gradient, registered.Gradient);
    double candidateGradientNormSquared = Dot(candidate.Gradient, candidate.Gradient);
    double denominator = System.Math.Sqrt(registeredGradientNormSquared * candidateGradientNormSquared);
    double cosine = denominator > 0.0 ? Dot(registered.Gradient, candidate.Gradient) / denominator : 1.0;
    return new ComparisonRow(
        id,
        registered.Curvature.All(double.IsFinite) && candidate.Curvature.All(double.IsFinite)
            && double.IsFinite(registered.Objective) && double.IsFinite(candidate.Objective)
            && registered.Gradient.All(double.IsFinite) && candidate.Gradient.All(double.IsFinite),
        Dot(registered.Curvature, registered.Curvature),
        Dot(candidate.Curvature, candidate.Curvature),
        VectorScaledError(registered.Curvature, candidate.Curvature),
        registered.Objective,
        candidate.Objective,
        ScaledError(registered.Objective, candidate.Objective),
        registeredGradientNormSquared,
        candidateGradientNormSquared,
        cosine,
        VectorScaledError(registered.Gradient, candidate.Gradient));
}

static double[] FaceCurvature(LieAlgebra algebra, double[][] x, int[] pairOrder)
{
    int dim = algebra.Dimension;
    var result = new double[dim];
    foreach (double[] edge in x)
        for (int a = 0; a < dim; a++) result[a] += edge[a];
    for (int ii = 0; ii < pairOrder.Length; ii++)
        for (int jj = ii + 1; jj < pairOrder.Length; jj++)
        {
            double[] bracket = algebra.Bracket(x[pairOrder[ii]], x[pairOrder[jj]]);
            for (int a = 0; a < dim; a++) result[a] += 0.5 * bracket[a];
        }
    return result;
}

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static double Dot(double[] a, double[] b) => a.Zip(b).Sum(x => x.First * x.Second);
static double ScaledError(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static double VectorScaledError(double[] a, double[] b) => System.Math.Sqrt(a.Zip(b).Sum(x => (x.First - x.Second) * (x.First - x.Second)))
    / System.Math.Max(1.0, System.Math.Max(System.Math.Sqrt(Dot(a, a)), System.Math.Sqrt(Dot(b, b))));
static double[] Subtract(double[] a, double[] b) => a.Zip(b).Select(x => x.First - x.Second).ToArray();
static double[] Add(double[] a, double[] b, double scale) => a.Zip(b).Select(x => x.First + scale * x.Second).ToArray();
static double[] Scale(double[] a, double scale) => a.Select(x => scale * x).ToArray();
static double[] Normalize(double[] a)
{
    double norm = System.Math.Sqrt(Dot(a, a));
    return a.Select(x => x / norm).ToArray();
}
static double[] Golden(int n, double scale, bool cosine)
{
    const double phi = 1.6180339887498948482;
    var result = new double[n];
    for (int i = 0; i < n; i++)
        result[i] = scale * (cosine ? System.Math.Cos((i + 1) * phi) : System.Math.Sin((i + 1) * phi));
    return result;
}

sealed record BindingSpec(string Id, string Path, string Sha256);
sealed record Evaluation(double Objective, double[] Gradient, double[] Curvature);
sealed record CurvatureParts(double[] Linear, double[] Quadratic);
sealed record ComparisonRow(
    string Id,
    bool Finite,
    double RegisteredCurvatureNormSquared,
    double CandidateCurvatureNormSquared,
    double CurvatureScaledDifference,
    double RegisteredAction,
    double CandidateAction,
    double ActionScaledDifference,
    double RegisteredGradientNormSquared,
    double CandidateGradientNormSquared,
    double GradientCosine,
    double GradientScaledDifference);

sealed class IndependentCounterfactual
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly int _dim;
    private readonly int[][] _cellFaces;
    private readonly double[][,] _cellFaceMap;
    private readonly double[] _faceInvCount;

    public IndependentCounterfactual(SimplicialMesh mesh, LieAlgebra algebra, int latticePeriod)
    {
        _mesh = mesh;
        _algebra = algebra;
        _dim = algebra.Dimension;
        var member = new EinsteinianShiabFamilyMember
        {
            Phi1 = InvariantElementSpec.Sd2,
            Phi2 = InvariantElementSpec.Id0,
            EinsteinCoefficient = 0.5,
            EpsilonMode = "independent-theta",
        };
        double[,] r = Lambda2Algebra.MemberEndomorphism(member);
        double[,] rMinusI = Lambda2Algebra.ScaleAdd(r, 1.0, Lambda2Algebra.Identity(Lambda2Algebra.Dim), -1.0);
        (_cellFaces, _cellFaceMap, _faceInvCount) = BuildCellMaps(rMinusI, latticePeriod);
    }

    public Evaluation Evaluate(double[] omega, bool candidate)
    {
        CurvatureParts parts = CurvatureComponents(omega, candidate);
        double[] curvature = parts.Linear.Zip(parts.Quadratic).Select(x => x.First + x.Second).ToArray();
        double[] upsilon = ApplyContraction(curvature);
        double[] mUpsilon = ApplyMetric(upsilon);
        double objective = 0.5 * DotLocal(upsilon, mUpsilon);
        double[] curvatureCovector = ApplyContractionTranspose(mUpsilon);
        double[] gradient = CurvatureTranspose(omega, curvatureCovector, candidate);
        return new Evaluation(objective, gradient, curvature);
    }

    public CurvatureParts CurvatureComponents(double[] omega, bool candidate)
    {
        int faceCount = _mesh.FaceCount;
        var linear = new double[faceCount * _dim];
        var quadratic = new double[faceCount * _dim];
        int[] order = candidate ? [0, 2, 1] : [0, 1, 2];
        for (int f = 0; f < faceCount; f++)
        {
            int[] edges = _mesh.FaceBoundaryEdges[f];
            int[] signs = _mesh.FaceBoundaryOrientations[f];
            var x = new double[edges.Length][];
            for (int k = 0; k < edges.Length; k++)
            {
                x[k] = new double[_dim];
                for (int a = 0; a < _dim; a++)
                {
                    x[k][a] = signs[k] * omega[edges[k] * _dim + a];
                    linear[f * _dim + a] += x[k][a];
                }
            }
            for (int ii = 0; ii < order.Length; ii++)
                for (int jj = ii + 1; jj < order.Length; jj++)
                {
                    double[] bracket = _algebra.Bracket(x[order[ii]], x[order[jj]]);
                    for (int a = 0; a < _dim; a++) quadratic[f * _dim + a] += 0.5 * bracket[a];
                }
        }
        return new CurvatureParts(linear, quadratic);
    }

    public double[] CurvatureLinearization(double[] omega, double[] delta, bool candidate)
    {
        int[] order = candidate ? [0, 2, 1] : [0, 1, 2];
        var result = new double[_mesh.FaceCount * _dim];
        for (int f = 0; f < _mesh.FaceCount; f++)
        {
            int[] edges = _mesh.FaceBoundaryEdges[f];
            int[] signs = _mesh.FaceBoundaryOrientations[f];
            var x = new double[edges.Length][];
            var dx = new double[edges.Length][];
            for (int k = 0; k < edges.Length; k++)
            {
                x[k] = new double[_dim];
                dx[k] = new double[_dim];
                for (int a = 0; a < _dim; a++)
                {
                    x[k][a] = signs[k] * omega[edges[k] * _dim + a];
                    dx[k][a] = signs[k] * delta[edges[k] * _dim + a];
                    result[f * _dim + a] += dx[k][a];
                }
            }
            for (int ii = 0; ii < order.Length; ii++)
                for (int jj = ii + 1; jj < order.Length; jj++)
                {
                    int p = order[ii], q = order[jj];
                    double[] first = _algebra.Bracket(dx[p], x[q]);
                    double[] second = _algebra.Bracket(x[p], dx[q]);
                    for (int a = 0; a < _dim; a++) result[f * _dim + a] += 0.5 * (first[a] + second[a]);
                }
        }
        return result;
    }

    public double[] CurvatureTranspose(double[] omega, double[] faceCovector, bool candidate)
    {
        int[] order = candidate ? [0, 2, 1] : [0, 1, 2];
        var result = new double[omega.Length];
        for (int f = 0; f < _mesh.FaceCount; f++)
        {
            int[] edges = _mesh.FaceBoundaryEdges[f];
            int[] signs = _mesh.FaceBoundaryOrientations[f];
            var x = new double[edges.Length][];
            var gx = new double[edges.Length][];
            for (int k = 0; k < edges.Length; k++)
            {
                x[k] = new double[_dim];
                gx[k] = new double[_dim];
                for (int a = 0; a < _dim; a++)
                {
                    x[k][a] = signs[k] * omega[edges[k] * _dim + a];
                    gx[k][a] = faceCovector[f * _dim + a];
                }
            }
            for (int ii = 0; ii < order.Length; ii++)
                for (int jj = ii + 1; jj < order.Length; jj++)
                {
                    int p = order[ii], q = order[jj];
                    for (int a = 0; a < _dim; a++)
                        for (int b = 0; b < _dim; b++)
                            for (int c = 0; c < _dim; c++)
                            {
                                double structure = _algebra.GetStructureConstant(a, b, c);
                                double w = faceCovector[f * _dim + c];
                                gx[p][a] += 0.5 * structure * x[q][b] * w;
                                gx[q][b] += 0.5 * structure * x[p][a] * w;
                            }
                }
            for (int k = 0; k < edges.Length; k++)
                for (int a = 0; a < _dim; a++)
                    result[edges[k] * _dim + a] += signs[k] * gx[k][a];
        }
        return result;
    }

    public double[] ApplyContraction(double[] coefficients)
    {
        var acc = new double[_mesh.FaceCount * _dim];
        for (int c = 0; c < _cellFaces.Length; c++)
        {
            int[] faces = _cellFaces[c];
            double[,] map = _cellFaceMap[c];
            for (int j = 0; j < faces.Length; j++)
                for (int a = 0; a < _dim; a++)
                {
                    double sum = coefficients[faces[j] * _dim + a];
                    for (int k = 0; k < faces.Length; k++)
                        sum += map[j, k] * coefficients[faces[k] * _dim + a];
                    acc[faces[j] * _dim + a] += sum;
                }
        }
        for (int f = 0; f < _mesh.FaceCount; f++)
            for (int a = 0; a < _dim; a++) acc[f * _dim + a] *= _faceInvCount[f];
        return acc;
    }

    public double MetricInnerProduct(double[] left, double[] right) => DotLocal(left, ApplyMetric(right));

    private double[] ApplyMetric(double[] coefficients)
    {
        var result = new double[coefficients.Length];
        for (int f = 0; f < _mesh.FaceCount; f++)
            for (int a = 0; a < _dim; a++)
                for (int b = 0; b < _dim; b++)
                    result[f * _dim + a] += _algebra.InvariantMetric[a * _dim + b] * coefficients[f * _dim + b];
        return result;
    }

    private double[] ApplyContractionTranspose(double[] covector)
    {
        var averaged = new double[covector.Length];
        for (int f = 0; f < _mesh.FaceCount; f++)
            for (int a = 0; a < _dim; a++) averaged[f * _dim + a] = covector[f * _dim + a] * _faceInvCount[f];
        var acc = new double[covector.Length];
        for (int c = 0; c < _cellFaces.Length; c++)
        {
            int[] faces = _cellFaces[c];
            double[,] map = _cellFaceMap[c];
            for (int k = 0; k < faces.Length; k++)
                for (int a = 0; a < _dim; a++)
                {
                    double sum = averaged[faces[k] * _dim + a];
                    for (int j = 0; j < faces.Length; j++) sum += map[j, k] * averaged[faces[j] * _dim + a];
                    acc[faces[k] * _dim + a] += sum;
                }
        }
        return acc;
    }

    private (int[][] CellFaces, double[][,] CellMaps, double[] InvCounts) BuildCellMaps(double[,] rMinusI, int period)
    {
        var cellFaces = new int[_mesh.CellCount][];
        var maps = new double[_mesh.CellCount][,];
        var counts = new int[_mesh.FaceCount];
        for (int c = 0; c < _mesh.CellCount; c++)
        {
            int[] faces = _mesh.CellFaces[c];
            cellFaces[c] = faces;
            var w = new double[Lambda2Algebra.Dim, faces.Length];
            for (int j = 0; j < faces.Length; j++)
            {
                int[] vertices = _mesh.Faces[faces[j]];
                double[] pa = _mesh.GetVertexCoordinates(vertices[0]).ToArray();
                double[] pb = _mesh.GetVertexCoordinates(vertices[1]).ToArray();
                double[] pc = _mesh.GetVertexCoordinates(vertices[2]).ToArray();
                var u = new double[4];
                var v = new double[4];
                for (int d = 0; d < 4; d++)
                {
                    u[d] = pb[d] - pa[d];
                    v[d] = pc[d] - pa[d];
                    u[d] -= period * System.Math.Round(u[d] / period);
                    v[d] -= period * System.Math.Round(v[d] / period);
                }
                double[] bivector = Lambda2Algebra.Wedge(u, v);
                for (int k = 0; k < Lambda2Algebra.Dim; k++) w[k, j] = bivector[k];
                counts[faces[j]]++;
            }
            double[,] wt = Lambda2Algebra.Transpose(w);
            double[,] q = Lambda2Algebra.Multiply(Lambda2Algebra.Invert(Lambda2Algebra.Multiply(w, wt)), w);
            maps[c] = Lambda2Algebra.Multiply(wt, Lambda2Algebra.Multiply(rMinusI, q));
        }
        double[] inverse = counts.Select(x => x > 0 ? 1.0 / x : 0.0).ToArray();
        return (cellFaces, maps, inverse);
    }

    private static double DotLocal(double[] a, double[] b) => a.Zip(b).Sum(x => x.First * x.Second);
}
