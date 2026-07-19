using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;

const string Root = "studies/phase524_a5_exact_omega_parity_decomposition_001";
const string ContractPath = Root + "/preregistration/phase524_a5_exact_omega_parity_decomposition_contract_v1.json";
const string OutputPath = Root + "/output/a5_exact_omega_parity_decomposition.json";
const string SummaryPath = Root + "/output/a5_exact_omega_parity_decomposition_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");

var specs = new[]
{
    Spec("phase452-program", "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs", "phase452ProgramSha256"),
    Spec("phase452-summary", "studies/phase452_scalar_channel_spectroscopy_probe_001/output/scalar_channel_spectroscopy_probe_summary.json", "phase452SummarySha256"),
    Spec("curvature-assembler", "src/Gu.ReferenceCpu/CurvatureAssembler.cs", "curvatureAssemblerSha256"),
    Spec("shiab-operator", "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs", "shiabOperatorSha256"),
    Spec("shiab-family-spec", "src/Gu.ReferenceCpu/EinsteinianShiabFamilySpec.cs", "shiabFamilySpecSha256"),
    Spec("lambda2-algebra", "src/Gu.ReferenceCpu/Lambda2Algebra.cs", "lambda2AlgebraSha256"),
    Spec("cpu-mass-matrix", "src/Gu.ReferenceCpu/CpuMassMatrix.cs", "cpuMassMatrixSha256"),
    Spec("lie-algebra-factory", "src/Gu.Math/LieAlgebraFactory.cs", "lieAlgebraFactorySha256"),
    Spec("mesh-generator", "src/Gu.Geometry/SimplicialMeshGenerator.cs", "meshGeneratorSha256"),
    Spec("mesh-topology-builder", "src/Gu.Geometry/MeshTopologyBuilder.cs", "meshTopologyBuilderSha256"),
    Spec("a5-generator", "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs", "a5GeneratorSha256"),
    Spec("serialized-a5", "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json", "serializedA5Sha256"),
    Spec("phase517-program", "studies/phase517_a5_dual_reflection_candidate_foundation_001/Program.cs", "phase517ProgramSha256"),
    Spec("phase517-contract", "studies/phase517_a5_dual_reflection_candidate_foundation_001/preregistration/phase517_dual_reflection_candidate_foundation_contract_v1.json", "phase517ContractSha256"),
    Spec("phase517-summary", "studies/phase517_a5_dual_reflection_candidate_foundation_001/output/a5_dual_reflection_candidate_foundation_summary.json", "phase517SummarySha256"),
    Spec("phase520-program", "studies/phase520_a5_action_subject_lineage_parity_audit_001/Program.cs", "phase520ProgramSha256"),
    Spec("phase520-contract", "studies/phase520_a5_action_subject_lineage_parity_audit_001/preregistration/phase520_a5_action_subject_lineage_parity_contract_v1.json", "phase520ContractSha256"),
    Spec("phase520-summary", "studies/phase520_a5_action_subject_lineage_parity_audit_001/output/a5_action_subject_lineage_parity_audit_summary.json", "phase520SummarySha256"),
    Spec("phase522-program", "studies/phase522_a5_foundation_candidate_reduction_001/Program.cs", "phase522ProgramSha256"),
    Spec("phase522-contract", "studies/phase522_a5_foundation_candidate_reduction_001/preregistration/phase522_a5_foundation_candidate_reduction_contract_v1.json", "phase522ContractSha256"),
    Spec("phase522-summary", "studies/phase522_a5_foundation_candidate_reduction_001/output/a5_foundation_candidate_reduction_summary.json", "phase522SummarySha256"),
};

var bindings = specs.Select(x => Bind(x.Id, x.Path, expected.GetProperty(x.HashKey).GetString()!)).ToArray();
bool exactBindingsValid = bindings.All(x => x.HashMatches);
var sources = specs.ToDictionary(x => x.Id, x => File.ReadAllText(x.Path), StringComparer.Ordinal);

bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase524-a19-exact-omega-parity-decomposition-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A19"
    && contract.GetProperty("frozenBeforeEvaluation").GetBoolean()
    && expected.EnumerateObject().Count() == specs.Length
    && contract.GetProperty("expectedCurrentVerdict").GetString() == "exact-omega-parity-refuted-identity-member"
    && contract.GetProperty("expectedCurrentTerminalStatus").GetString() == "a5-exact-omega-parity-decomposition-exact-omega-parity-refuted-identity-member"
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "invalid-or-drifted-input",
        "exact-omega-parity-refuted-identity-member",
        "executable-formula-coverage-incomplete",
        "non-exact-coefficients-block-exact-parity",
        "odd-cancellation-unresolved",
        "both-members-exactly-even",
    }, StringComparer.Ordinal)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && !contract.GetProperty("samplingOrProductionAllowed").GetBoolean()
    && !contract.GetProperty("theoremOrCounterexampleAllowed").GetBoolean()
    && AuthorityFirewallsValid(contract.GetProperty("authorityFirewalls"));

var coverage = contract.GetProperty("sourceFormulaCoverageLedger").EnumerateArray().Select(row =>
{
    string id = row.GetProperty("formulaId").GetString()!;
    string bindingId = row.GetProperty("bindingId").GetString()!;
    string[] anchors = row.GetProperty("requiredAnchors").EnumerateArray().Select(x => x.GetString()!).ToArray();
    bool allAnchorsPresent = anchors.All(anchor => sources[bindingId].Contains(anchor, StringComparison.Ordinal));
    return new { formulaId = id, bindingId, requiredAnchors = anchors, allAnchorsPresent };
}).ToArray();
bool sourceFormulaCoverageLedgerComplete = coverage.Length == 12 && coverage.All(x => x.allAnchorsPresent);

var parityAssertions = contract.GetProperty("a5ParityAssertionLedger").EnumerateArray().Select(row =>
{
    string evidenceId = row.GetProperty("evidenceId").GetString()!;
    string bindingId = row.GetProperty("bindingId").GetString()!;
    string requiredAnchor = row.GetProperty("requiredAnchor").GetString()!;
    bool assertionPresent = sources[bindingId].Contains(requiredAnchor, StringComparison.Ordinal);
    return new
    {
        evidenceId,
        bindingId,
        evidenceKind = row.GetProperty("evidenceKind").GetString(),
        requiredAnchor,
        assertionPresent,
        acceptedAsIdentityProof = false,
    };
}).ToArray();
bool a5ParityAssertionsExactBound = parityAssertions.Length == 2
    && parityAssertions.Select(x => x.evidenceId).SequenceEqual(
        new[] { "generator-z2-comment", "serialized-a5-even-prose" }, StringComparer.Ordinal)
    && parityAssertions.All(x => x.assertionPresent && !x.acceptedAsIdentityProof);

using var phase452Doc = JsonDocument.Parse(sources["phase452-summary"]);
using var phase517Doc = JsonDocument.Parse(sources["phase517-summary"]);
using var phase520Doc = JsonDocument.Parse(sources["phase520-summary"]);
using var phase522Doc = JsonDocument.Parse(sources["phase522-summary"]);
bool precursorFindingsValid =
    phase452Doc.RootElement.GetProperty("applicationSubjectKind").GetString() == "scalar-channel-spectroscopy-probe"
    && phase452Doc.RootElement.GetProperty("scalarChannelSpectroscopyProbePassed").GetBoolean()
    && phase452Doc.RootElement.GetProperty("targetBlindConstruction").GetBoolean()
    && !phase452Doc.RootElement.GetProperty("physicalTargetsConsultedForConstruction").GetBoolean()
    && phase452Doc.RootElement.GetProperty("tori")[0].GetProperty("memberBatteries").EnumerateArray()
        .Select(x => x.GetProperty("member").GetString()).SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal)
    && phase517Doc.RootElement.GetProperty("verdictKind").GetString() == "action-member-or-omega-parity-ambiguous"
    && phase520Doc.RootElement.GetProperty("verdictKind").GetString() == "action-member-unresolved"
    && phase520Doc.RootElement.GetProperty("omegaParityAudit").GetProperty("linearAndQuadraticCurvaturePresent").GetBoolean()
    && phase520Doc.RootElement.GetProperty("omegaParityAudit").GetProperty("quadraticActionPresent").GetBoolean()
    && !phase520Doc.RootElement.GetProperty("omegaParityAudit").GetProperty("exactIdentityCancelsEveryOmegaOddContribution").GetBoolean()
    && phase522Doc.RootElement.GetProperty("verdictKind").GetString() == "action-member-or-omega-parity-unresolved";

JsonElement[] requirements = contract.GetProperty("memberTermRequirements").EnumerateArray().ToArray();
var expectedTermMappings = new (string MemberId, string TermId, int Degree, string Parity)[]
{
    ("identity", "upsilon-linear", 1, "odd"),
    ("identity", "upsilon-quadratic", 2, "even"),
    ("identity", "action-degree-2", 2, "even"),
    ("identity", "action-degree-3", 3, "odd"),
    ("identity", "action-degree-4", 4, "even"),
    ("sd2-id0/c0.5", "upsilon-linear", 1, "odd"),
    ("sd2-id0/c0.5", "upsilon-quadratic", 2, "even"),
    ("sd2-id0/c0.5", "action-degree-2", 2, "even"),
    ("sd2-id0/c0.5", "action-degree-3", 3, "odd"),
    ("sd2-id0/c0.5", "action-degree-4", 4, "even"),
};
bool termRequirementsFrozenAndComplete = requirements.Length == 10
    && requirements.Select((x, i) =>
    {
        var e = expectedTermMappings[i];
        return x.GetProperty("memberId").GetString() == e.MemberId
            && x.GetProperty("termId").GetString() == e.TermId
            && x.GetProperty("requiredOmegaDegree").GetInt32() == e.Degree
            && x.GetProperty("requiredParity").GetString() == e.Parity;
    }).All(x => x);

var identityActionTerms = new[]
{
    new ActionTerm("action-degree-2", 2, "even", "(1/2)t^2<L,L>", true),
    new ActionTerm("action-degree-3", 3, "odd", "(1/2)t^3<L,W>", true),
    new ActionTerm("action-degree-4", 4, "even", "(1/8)t^4<W,W>", true),
};
var sd2ActionTerms = new[]
{
    new ActionTerm("action-degree-2", 2, "even", "(1/2)t^2<C_theta L,M C_theta L>", false),
    new ActionTerm("action-degree-3", 3, "odd", "(1/2)t^3(<C_theta L,M C_theta W>+<C_theta W,M C_theta L>)/2", false),
    new ActionTerm("action-degree-4", 4, "even", "(1/8)t^4<C_theta W,M C_theta W>", false),
};
bool emittedTermMappingsMatchContract =
    EmittedTermsMatch(requirements, "identity", identityActionTerms)
    && EmittedTermsMatch(requirements, "sd2-id0/c0.5", sd2ActionTerms);

// Exact full-action witness. All arithmetic below is integer arithmetic. The
// selected edge values are oriented so the first face sees e3,e1,e2. Every
// face in the n=3 committed lattice-canonical mesh contributes to totalLdotW.
JsonElement witnessSpec = contract.GetProperty("identityExactWitness");
int extent = witnessSpec.GetProperty("extent").GetInt32();
int faceIndex = witnessSpec.GetProperty("seedFaceIndex").GetInt32();
int[][] orientedBasis = witnessSpec.GetProperty("orientedBoundaryValues").EnumerateArray()
    .Select(row => row.EnumerateArray().Select(x => x.GetInt32()).ToArray()).ToArray();
SimplicialMesh mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
int[] witnessEdges = mesh.FaceBoundaryEdges[faceIndex];
int[] witnessSigns = mesh.FaceBoundaryOrientations[faceIndex];
var omega = new long[mesh.EdgeCount, 3];
for (int i = 0; i < 3; i++)
    for (int a = 0; a < 3; a++)
        omega[witnessEdges[i], a] = witnessSigns[i] * orientedBasis[i][a];

long totalLdotW = 0;
long totalL2 = 0;
long totalW2 = 0;
int nonzeroFaceContributionCount = 0;
for (int f = 0; f < mesh.FaceCount; f++)
{
    var x = new long[3, 3];
    for (int i = 0; i < 3; i++)
        for (int a = 0; a < 3; a++)
            x[i, a] = mesh.FaceBoundaryOrientations[f][i] * omega[mesh.FaceBoundaryEdges[f][i], a];
    long[] linear =
    [
        x[0, 0] + x[1, 0] + x[2, 0],
        x[0, 1] + x[1, 1] + x[2, 1],
        x[0, 2] + x[1, 2] + x[2, 2],
    ];
    long[] wedge = [0, 0, 0];
    for (int i = 0; i < 3; i++)
        for (int j = i + 1; j < 3; j++)
        {
            wedge[0] += x[i, 1] * x[j, 2] - x[i, 2] * x[j, 1];
            wedge[1] += x[i, 2] * x[j, 0] - x[i, 0] * x[j, 2];
            wedge[2] += x[i, 0] * x[j, 1] - x[i, 1] * x[j, 0];
        }
    long faceLdotW = linear[0] * wedge[0] + linear[1] * wedge[1] + linear[2] * wedge[2];
    if (faceLdotW != 0) nonzeroFaceContributionCount++;
    totalLdotW += faceLdotW;
    totalL2 += linear[0] * linear[0] + linear[1] * linear[1] + linear[2] * linear[2];
    totalW2 += wedge[0] * wedge[0] + wedge[1] * wedge[1] + wedge[2] * wedge[2];
}

bool witnessConstructionExact = extent == 3 && faceIndex == 0
    && orientedBasis.SelectMany(x => x).SequenceEqual(new[] { 0, 0, 1, 1, 0, 0, 0, 1, 0 })
    && mesh.VertexCount == 81 && mesh.EdgeCount == 1215 && mesh.FaceCount == 4050
    && witnessEdges.SequenceEqual(new[] { 0, 1, 4 }) && witnessSigns.SequenceEqual(new[] { 1, -1, 1 });
bool identityOddTermSurvives = witnessConstructionExact && totalLdotW == 1;
bool identityExactlyEven = !identityOddTermSurvives && false;
bool sd2CoefficientsExact = false; // executable W/Q and exp(ad_theta) paths use binary64 inverses/exponentials
bool sd2OddCancellationResolved = false;

int[] faceIncidenceCounts = new int[mesh.FaceCount];
foreach (int[] cellFaces in mesh.CellFaces)
    foreach (int f in cellFaces)
        faceIncidenceCounts[f]++;
bool incidenceAveragingIdentityCertified = faceIncidenceCounts.All(x => x > 0);
string[] reductionFormulaIds = contract.GetProperty("identityReductionCertificate")
    .GetProperty("requiredFormulaIds").EnumerateArray().Select(x => x.GetString()!).ToArray();
bool exactIdentityReductionCertified =
    reductionFormulaIds.SequenceEqual(new[]
    {
        "identity-member-definition", "curvature-linear", "curvature-quadratic",
        "fixed-theta-linear-contraction", "quadratic-action", "uniform-trace-pairing",
        "identity-endomorphism", "su2-exact-bracket-and-trace-metric",
    }, StringComparer.Ordinal)
    && reductionFormulaIds.All(id => coverage.Single(x => x.formulaId == id).allAnchorsPresent)
    && sources["shiab-operator"].Contains("var rMinusI = Lambda2Algebra.ScaleAdd(r, 1.0, Lambda2Algebra.Identity(Lambda2Algebra.Dim), -1.0)", StringComparison.Ordinal)
    && sources["shiab-operator"].Contains("var rmiQ = Lambda2Algebra.Multiply(rMinusI, q)", StringComparison.Ordinal)
    && sources["shiab-operator"].Contains("double s = fa[j][a]", StringComparison.Ordinal)
    && sources["shiab-operator"].Contains("acc[gf * _dimG + a] += s", StringComparison.Ordinal)
    && incidenceAveragingIdentityCertified;
bool sourceFormulaCoverageComplete = sourceFormulaCoverageLedgerComplete && exactIdentityReductionCertified;
bool exactIdentityRefutationEstablished = identityOddTermSurvives && sourceFormulaCoverageComplete;

bool inputsValid = contractValid && exactBindingsValid && a5ParityAssertionsExactBound
    && precursorFindingsValid && termRequirementsFrozenAndComplete
    && emittedTermMappingsMatchContract && witnessConstructionExact;
string verdict = EvaluateVerdict(
    invalidOrDriftedInput: !inputsValid,
    exactIdentityRefutationEstablished,
    sourceFormulaCoverageComplete,
    sd2CoefficientsExact,
    sd2OddCancellationResolved);

var precedenceBattery = new[]
{
    PrecedenceCase("invalid", true, true, true, true, true, "invalid-or-drifted-input"),
    PrecedenceCase("identity-refuted", false, true, true, false, false, "exact-omega-parity-refuted-identity-member"),
    PrecedenceCase("coverage-incomplete", false, false, false, false, false, "executable-formula-coverage-incomplete"),
    PrecedenceCase("coefficients-nonexact", false, false, true, false, false, "non-exact-coefficients-block-exact-parity"),
    PrecedenceCase("cancellation-unresolved", false, false, true, true, false, "odd-cancellation-unresolved"),
    PrecedenceCase("both-even", false, false, true, true, true, "both-members-exactly-even"),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

var result = new
{
    schemaVersion = 1,
    phase = 524,
    phaseId = "phase524-a5-exact-omega-parity-decomposition",
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    exactBindingsValid,
    precursorFindingsValid,
    sourceFormulaCoverageComplete,
    sourceFormulaCoverageLedgerComplete,
    exactIdentityReductionCertified,
    incidenceAveragingIdentityCertified,
    a5ParityAssertionsExactBound,
    termRequirementsFrozenAndComplete,
    emittedTermMappingsMatchContract,
    precedenceBatteryPassed,
    precedenceBattery,
    verdictKind = verdict,
    terminalStatus = "a5-exact-omega-parity-decomposition-" + verdict,
    decision = identityOddTermSurvives
        ? "The exact rational action implied by the exact-bound identity-member formulas on the full n=3 lattice has a surviving degree-three omega term. For the frozen integer direction, sum_f L_f dot W_f = 1, hence the coefficient of t^3 in S_B(t omega) is 1/2 and S_B(omega)-S_B(-omega)=1. Omega-sign evenness is therefore refuted for the identity member in this finite formula-bound scope. The sd2 member remains unresolved and no action member is selected."
        : "Fail-closed precedence selected a non-current branch; no parity or downstream authority follows.",
    sourceFormulaCoverageLedger = coverage,
    a5ParityAssertionLedger = parityAssertions,
    memberDecompositions = new object[]
    {
        new
        {
            memberId = "identity",
            fixedParameters = "theta-absent; R=I; uniform positive trace pairing",
            residual = "Upsilon(t omega)=t L(omega)+(t^2/2) W(omega)",
            actionTerms = identityActionTerms,
            completeExecutableFormulaCoverage = sourceFormulaCoverageComplete,
            exactOddCancellationResolved = true,
            identityExactlyEven,
            omegaParityStatus = "refuted-by-full-finite-action-exact-witness",
        },
        new
        {
            memberId = "sd2-id0/c0.5",
            fixedParameters = "independent theta held fixed under omega sign flip",
            residual = "Upsilon(t omega,theta)=t C_theta L(omega)+(t^2/2) C_theta W(omega)",
            actionTerms = sd2ActionTerms,
            completeExecutableFormulaCoverage = sourceFormulaCoverageComplete,
            exactExecutableCoefficients = sd2CoefficientsExact,
            exactOddCancellationResolved = sd2OddCancellationResolved,
            omegaParityStatus = "unresolved-non-exact-coefficients-and-no-global-cancellation-identity",
        },
    },
    identityExactWitness = new
    {
        arithmetic = "signed-int64-only",
        extent,
        latticeCanonical = true,
        mesh.VertexCount,
        mesh.EdgeCount,
        mesh.FaceCount,
        seedFaceIndex = faceIndex,
        witnessEdges,
        witnessSigns,
        orientedBoundaryValues = orientedBasis,
        everyFaceIncluded = true,
        nonzeroFaceContributionCount,
        totalL2,
        totalLdotW,
        totalW2,
        cubicCoefficientNumerator = totalLdotW,
        cubicCoefficientDenominator = 2,
        actionAtPlusMinusDifferenceNumerator = totalLdotW,
        actionAtPlusMinusDifferenceDenominator = 1,
        identityOddTermSurvives,
        memberId = "identity",
        scope = "exact rational action implied by the exact-bound Phase452 identity-member formulas; lattice-canonical n=3, uniform trace pairing, all 4050 faces",
        semanticReductionDependency = contract.GetProperty("identityReductionCertificate").GetProperty("semanticDependency").GetString(),
    },
    proofPolicy = new
    {
        commentsOrProseUsedAsIdentityProof = false,
        contractTruthValuesUsedAsIdentityProof = false,
        randomProbeUsed = false,
        floatingPointProbeUsed = false,
        globalCancellationAssumed = false,
        everyFaceContributionIncluded = true,
    },
    bindings,
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    actionMemberSelected = false,
    productionActionOrGeometryRegistered = false,
    phase515Unlocked = false,
    phase516Unlocked = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    samplingAuthorized = false,
    hmcAuthorized = false,
    benchmarkAuthorized = false,
    productionAuthorized = false,
    launchAuthorized = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
};

Require(contractValid, "Phase524 frozen contract is invalid.");
Require(inputsValid, "Phase524 exact-bound inputs or frozen ledgers are invalid or drifted.");
Require(precedenceBatteryPassed, "Phase524 deterministic precedence evaluator battery failed.");
Require(sourceFormulaCoverageComplete, "Phase524 exact identity reduction or formula coverage is incomplete.");
Require(exactIdentityRefutationEstablished, "Phase524 frozen exact identity-member odd-term witness drifted.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase524 verdict drifted from preregistration.");
Require("a5-exact-omega-parity-decomposition-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase524 terminal status drifted from preregistration.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase524: {verdict}");
Console.WriteLine($"identity full-action exact sum L.W: {totalLdotW}");
Console.WriteLine($"identity cubic coefficient: {totalLdotW}/2");
Console.WriteLine($"sd2 parity: unresolved (non-exact coefficients and no global cancellation identity)");

static (string Id, string Path, string HashKey) Spec(string id, string path, string hashKey) => (id, path, hashKey);

static bool EmittedTermsMatch(JsonElement[] requirements, string memberId, ActionTerm[] terms)
{
    var actionRequirements = requirements
        .Where(x => x.GetProperty("memberId").GetString() == memberId
            && x.GetProperty("termId").GetString()!.StartsWith("action-degree-", StringComparison.Ordinal))
        .ToArray();
    return actionRequirements.Length == terms.Length
        && actionRequirements.Select((x, i) =>
            x.GetProperty("termId").GetString() == terms[i].TermId
            && x.GetProperty("requiredOmegaDegree").GetInt32() == terms[i].OmegaDegree
            && x.GetProperty("requiredParity").GetString() == terms[i].Parity).All(x => x);
}

static string EvaluateVerdict(bool invalidOrDriftedInput, bool exactIdentityRefutationEstablished,
    bool sourceFormulaCoverageComplete, bool sd2CoefficientsExact, bool sd2OddCancellationResolved) =>
    invalidOrDriftedInput ? "invalid-or-drifted-input"
    : exactIdentityRefutationEstablished ? "exact-omega-parity-refuted-identity-member"
    : !sourceFormulaCoverageComplete ? "executable-formula-coverage-incomplete"
    : !sd2CoefficientsExact ? "non-exact-coefficients-block-exact-parity"
    : !sd2OddCancellationResolved ? "odd-cancellation-unresolved"
    : "both-members-exactly-even";

static PrecedenceBatteryCase PrecedenceCase(string id, bool invalid, bool refuted, bool coverage,
    bool coefficientsExact, bool cancellationResolved, string expected)
{
    string actual = EvaluateVerdict(invalid, refuted, coverage, coefficientsExact, cancellationResolved);
    return new PrecedenceBatteryCase(id, expected, actual, actual == expected);
}

static bool AuthorityFirewallsValid(JsonElement firewalls)
{
    string[] expectedNames =
    [
        "actionMemberSelectionAllowed", "productionActionOrGeometryRegistrationAllowed",
        "phase515UnlockAllowed", "phase516UnlockAllowed", "phase458G1SatisfactionAllowed",
        "limbL8ClosureAllowed", "reflectionPositivityRulingAllowed", "samplingAllowed",
        "hmcAllowed", "benchmarkAllowed", "productionAllowed", "launchAllowed",
        "sourceContractApplicationAllowed", "physicalUnitClaimAllowed",
    ];
    return firewalls.EnumerateObject().Select(x => x.Name).SequenceEqual(expectedNames, StringComparer.Ordinal)
        && firewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False);
}

static BindingResult Bind(string id, string path, string expectedSha256)
{
    string actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    return new BindingResult
    {
        Id = id,
        Path = path,
        ExpectedSha256 = expectedSha256,
        ActualSha256 = actual,
        HashMatches = actual == expectedSha256,
    };
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class BindingResult
{
    public required string Id { get; init; }
    public required string Path { get; init; }
    public required string ExpectedSha256 { get; init; }
    public required string ActualSha256 { get; init; }
    public required bool HashMatches { get; init; }
}

sealed record ActionTerm(string TermId, int OmegaDegree, string Parity, string Formula, bool Exact);
sealed record PrecedenceBatteryCase(string CaseId, string ExpectedVerdict, string ActualVerdict, bool Passed);
