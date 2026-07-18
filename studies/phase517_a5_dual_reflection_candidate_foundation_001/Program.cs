using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase517_a5_dual_reflection_candidate_foundation_001";
const string ContractPath = Root + "/preregistration/phase517_dual_reflection_candidate_foundation_contract_v1.json";
const string OutputDir = Root + "/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");
string[] sourceAuditAssertions = contract.GetProperty("sourceAuditAssertions").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] requiredPerCandidateFields = contract.GetProperty("requiredPerCandidateFields").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] precedence = contract.GetProperty("terminalPrecedence").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] expectedRequiredPerCandidateFields =
[
    "action-member-status-and-functional", "configuration-space-and-gauge-status",
    "measure-and-domain", "coupling-and-volume-domain",
    "reflection-plane-and-vertex-map", "oriented-edge-pullback", "boundary-rule",
    "conjugation-rule", "positive-time-algebra",
    "unchanged-observable-membership-conditional", "connected-subtraction",
    "proposed-os-bilinear",
];
string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "action-member-or-omega-parity-ambiguous",
    "measure-domain-underdefined", "candidate-specification-invalid",
    "dual-candidates-frozen-unregistered",
];
JsonElement candidateStatuses = contract.GetProperty("candidateStatuses");
bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase517-a17-dual-reflection-candidate-foundation-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A17"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && precedence.SequenceEqual(expectedPrecedence, StringComparer.Ordinal)
    && contract.GetProperty("expectedCurrentVerdict").GetString() == "action-member-or-omega-parity-ambiguous"
    && requiredPerCandidateFields.SequenceEqual(expectedRequiredPerCandidateFields, StringComparer.Ordinal)
    && !contract.GetProperty("phase515Or516Unlocked").GetBoolean()
    && !contract.GetProperty("phase518ExecutedByThisPhase").GetBoolean()
    && !contract.GetProperty("phase458G1Satisfied").GetBoolean()
    && !contract.GetProperty("theoremOrCounterexampleAllowed").GetBoolean()
    && !contract.GetProperty("physicalClaimAllowed").GetBoolean()
    && !contract.GetProperty("samplingOrProductionAllowed").GetBoolean()
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && candidateStatuses.GetProperty("registration").GetString() == "audit-authored-candidate-not-registered-theorem-reflection"
    && candidateStatuses.GetProperty("edgeClosure").GetString() == "not-checked-phase518-only"
    && candidateStatuses.GetProperty("measureNormalization").GetString() == "not-proved"
    && candidateStatuses.GetProperty("observableMembership").GetString() == "not-checked-phase518-only"
    && candidateStatuses.GetProperty("hermiticity").GetString() == "not-proved"
    && candidateStatuses.GetProperty("reflectionPositivity").GetString() == "not-established-or-refuted"
    && candidateStatuses.GetProperty("targetEquality").GetString() == "not-proved"
    && candidateStatuses.GetProperty("necessityAndEmbedding").GetString() == "not-proved"
    && candidateStatuses.GetProperty("allCouplingAllVolumeScope").GetString() == "not-proved";

var sourceSpecs = new (string Id, string Path, string Key)[]
{
    ("phase514-summary", "studies/phase514_a5_registered_reflection_foundation_audit_001/output/a5_registered_reflection_foundation_audit_summary.json", "phase514SummarySha256"),
    ("phase514-contract", "studies/phase514_a5_registered_reflection_foundation_audit_001/preregistration/phase514_registered_reflection_foundation_contract_v1.json", "phase514ContractSha256"),
    ("phase514-program", "studies/phase514_a5_registered_reflection_foundation_audit_001/Program.cs", "phase514ProgramSha256"),
    ("phase452-program", "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs", "phase452ProgramSha256"),
    ("phase456-a5", "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json", "phase456A5Sha256"),
    ("phase456-a4", "studies/phase456_consolidated_n4_launch_001/preregistration/a4_symmetry_irrep_projectors.json", "phase456A4Sha256"),
    ("phase456-generator", "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs", "phase456GeneratorSha256"),
    ("phase456-manifest", "studies/phase456_consolidated_n4_launch_001/preregistration/pack_manifest.json", "phase456ManifestSha256"),
    ("phase478-contract", "studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json", "phase478ContractSha256"),
    ("phase482-summary", "studies/phase482_a5_theorem_scout_001/output/a5_theorem_scout_summary.json", "phase482SummarySha256"),
    ("phase482-program", "studies/phase482_a5_theorem_scout_001/Program.cs", "phase482ProgramSha256"),
    ("phase482-reflection", "studies/phase482_a5_theorem_scout_001/ReflectionLocalityAnalysis.cs", "phase482ReflectionSha256"),
    ("phase482-composite", "studies/phase482_a5_theorem_scout_001/CompositeTransportAnalysis.cs", "phase482CompositeSha256"),
    ("phase482-adjudication", "studies/phase482_a5_theorem_scout_001/TheoremScoutAdjudication.cs", "phase482AdjudicationSha256"),
    ("shiab-operator", "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs", "shiabOperatorSha256"),
    ("shiab-family-spec", "src/Gu.ReferenceCpu/EinsteinianShiabFamilySpec.cs", "shiabFamilySpecSha256"),
    ("curvature-assembler", "src/Gu.ReferenceCpu/CurvatureAssembler.cs", "curvatureAssemblerSha256"),
    ("cpu-mass-matrix", "src/Gu.ReferenceCpu/CpuMassMatrix.cs", "cpuMassMatrixSha256"),
    ("connection-field", "src/Gu.ReferenceCpu/ConnectionField.cs", "connectionFieldSha256"),
    ("mesh-generator", "src/Gu.Geometry/SimplicialMeshGenerator.cs", "meshGeneratorSha256"),
    ("lie-algebra-factory", "src/Gu.Math/LieAlgebraFactory.cs", "lieAlgebraFactorySha256"),
};

var bindings = sourceSpecs.Select(s => Bind(s.Id, s.Path, expected.GetProperty(s.Key).GetString()!)).ToArray();
bool exactBindingsValid = bindings.All(x => x.Matches);

bool precursorSemanticsValid =
    JsonString(sourceSpecs.Single(x => x.Id == "phase514-summary").Path, "verdictKind") == "registered-foundation-definition-missing"
    && !JsonBool(sourceSpecs.Single(x => x.Id == "phase514-summary").Path, "theoremClaimed")
    && JsonString(sourceSpecs.Single(x => x.Id == "phase456-a5").Path, "verdict") == "a5-stage-a-gaussian-domination-not-provable-obstruction-recorded"
    && !JsonBool(sourceSpecs.Single(x => x.Id == "phase456-a5").Path, "provable")
    && JsonString(sourceSpecs.Single(x => x.Id == "phase482-summary").Path, "verdictKind") == "obstructions-survive-no-theorem"
    && !JsonBool(sourceSpecs.Single(x => x.Id == "phase482-summary").Path, "theoremClaimed");

var candidates = new[]
{
    Candidate(
        "site-centered-axis0", "site-centered", "r0(t,x)=(-t mod 4,x)",
        new[] { 1 }, new[] { 0, 2 }, new[] { 3 },
        "vertices on slices 0 and 2; incident boundary-edge treatment remains candidate-only",
        "audit-authored-candidate-not-registered-theorem-reflection"),
    Candidate(
        "link-centered-axis0", "link-centered", "r1(t,x)=(1-t mod 4,x)",
        new[] { 1, 2 }, Array.Empty<int>(), new[] { 0, 3 },
        "no fixed vertices; setwise-fixed plane-crossing links are candidate boundary objects",
        "audit-authored-candidate-not-registered-theorem-reflection"),
};

bool candidateSchemaValid = candidates.Select(x => x.CandidateId).SequenceEqual(
    contract.GetProperty("candidateIds").EnumerateArray().Select(x => x.GetString()), StringComparer.Ordinal)
    && candidates.All(x => x.Registered == false && x.A5Validated == false)
    && candidates.All(x => x.Status == candidateStatuses.GetProperty("registration").GetString())
    && candidates.All(x => x.ActionMember == contract.GetProperty("actionSubject").GetProperty("memberStatus").GetString())
    && candidates.All(RequiredCandidateFieldsPresent)
    && candidates.All(x => x.PositiveVertexSlices.Concat(x.BoundaryVertexSlices).Concat(x.NegativeVertexSlices).Order().SequenceEqual(Enumerable.Range(0, 4)));

Require(contractValid, "Phase517 frozen contract is invalid.");
Require(candidateSchemaValid, "Phase517 candidate schema is invalid.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var candidateArtifacts = candidates.Select(candidate =>
{
    string file = candidate.CandidateId.StartsWith("site", StringComparison.Ordinal)
        ? "site_centered_reflection_candidate.json" : "link_centered_reflection_candidate.json";
    string path = Path.Combine(OutputDir, file);
    File.WriteAllText(path, JsonSerializer.Serialize(candidate, options) + Environment.NewLine);
    return new { candidate.CandidateId, path, sha256 = Sha(path) };
}).ToArray();

bool invalidOrDrifted = !contractValid || !exactBindingsValid || !precursorSemanticsValid;
bool actionMemberOrOmegaParityAmbiguous = !ActionAndParityAreUnambiguous();
bool measureDomainUnderdefined = candidateStatuses.GetProperty("measureNormalization").GetString() != "proved";
bool candidateSpecificationInvalid = !candidateSchemaValid;
string verdict = Decide(
    [invalidOrDrifted, actionMemberOrOmegaParityAmbiguous, measureDomainUnderdefined, candidateSpecificationInvalid],
    precedence);

var report = new
{
    schemaVersion = 1,
    phase = 517,
    terminalStatus = verdict,
    inputsValid = contractValid && exactBindingsValid && precursorSemanticsValid && candidateSchemaValid,
    planSection = contract.GetProperty("planSection").GetString(),
    contract = new
    {
        path = ContractPath,
        sha256 = Sha(ContractPath),
        frozenBeforePrecursorConsumption = contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean(),
    },
    contractId = contract.GetProperty("contractId").GetString(),
    contractValid,
    verdictKind = verdict,
    decision = "Both formal reflection rows are frozen coequally for diagnosis, but neither can be A5-validated because the executable action member and omega-parity are not established by the exact-bound sources.",
    exactInputBindings = bindings,
    inputAudit = new { contractValid, exactBindingsValid, precursorSemanticsValid, candidateSchemaValid },
    terminalPrecedence = precedence,
    blockerAudit = new { invalidOrDrifted, actionMemberOrOmegaParityAmbiguous, measureDomainUnderdefined, candidateSpecificationInvalid },
    actionSubjectAudit = new
    {
        uniqueExecutableA5MemberFrozen = false,
        generatorCallsSubjectToyControlBranch = true,
        serializedArtifactPreservesToyQualifier = false,
        executableCurvatureStructure = "F=L(omega)+Q(omega)",
        fixedThetaDegree = "at-most-quartic",
        cubicSignOddCrossTermExcludedByExactBoundIdentity = false,
        omegaParityEstablished = false,
        sd2ThetaDependence = "transcendental",
        status = "action-member-or-omega-parity-ambiguous",
        frozenSourceAssertions = sourceAuditAssertions,
    },
    formalCandidateFoundation = new
    {
        lattice = new { extent = 4, vertexCount = 256, edgeCount = 3840, faceCount = 12800, algebraDimension = 3, configurationDimension = 11520 },
        configurationSpace = "Omega_4=R^11520 before any gauge quotient",
        density = "exp(-beta*S_B(omega))*domega (unnormalized)",
        probabilityMeasure = "mu_beta=Z_beta^-1 exp(-beta*S_B)domega only if 0<Z_beta<infinity",
        orientedEdgePullback = "on a reflected stored edge, coefficient sign is + when stored orientation is preserved and - when reversed; edge closure is not checked here",
        conjugation = "Theta_c F(omega)=conj(F(R_c omega))",
        positiveTimeAlgebra = "candidate complex polynomial cylinder star-algebra generated by positive/boundary edge coordinates",
        observables = "unchanged O1=Tr(F^2), O2=Tr(Upsilon^2), with membership conditional on an exact dependency audit",
        connectedSubtraction = "O_tilde=O-mu(O), conditional on mu; C_ab(s)=L^-1 sum_tau mu(O_tilde_a(tau+s) O_tilde_b(tau))",
        osBilinear = "B_c(F,G)=mu((Theta_c F)G), conditional on all domains and finiteness",
        candidates,
        candidateArtifacts,
        candidateSelectionPerformed = false,
        candidateRankingPerformed = false,
        candidatesCombined = false,
        candidateRegistrationPerformed = false,
        phase518ChecksExecuted = false,
    },
    unresolved = new
    {
        edgeClosure = "not-checked-phase518-only",
        observableMembership = "not-checked-phase518-only",
        measureNormalization = "not-proved",
        hermiticity = "not-proved",
        reflectionPositivity = "not-established-or-refuted",
        targetEquality = "not-proved",
        necessityAndEmbedding = "not-proved",
    },
    firewalls = new
    {
        targetBlind = true,
        targetBlindConstruction = true,
        externalReviewPending = true,
        phase515Unlocked = false,
        phase516Unlocked = false,
        phase518Executed = false,
        theoremClaimed = false,
        targetCounterexampleClaimed = false,
        reflectionPositivityEstablished = false,
        reflectionPositivityRefuted = false,
        phase458G1Satisfied = false,
        closesLimbL8 = false,
        phase458EvaluationOrG3G4G5Performed = false,
        phase480OrPhase481WorkPerformed = false,
        precursorArtifactsMutated = false,
        samplingHmcBenchmarkOrPhysicsComputePerformed = false,
        productionAllocationLaunchBinderOrAccelerationAuthorized = false,
        o4Discharged = false,
        humanRulingPerformed = false,
        sourceContractApplicationAllowed = false,
        routePromotesWzMasses = false,
        routePromotesHiggsMass = false,
        routeCompletesBosonPredictions = false,
        sourceContractOrWzHiggsRouteApplied = false,
        physicalUnitOrGevClaimMade = false,
        promotedPhysicalMassClaimCount = 0,
    },
};

Require(report.firewalls.targetBlindConstruction && report.firewalls.externalReviewPending,
    "Phase517 target-blind or external-review firewall drifted.");
Require(!report.firewalls.routePromotesWzMasses && !report.firewalls.routePromotesHiggsMass
    && !report.firewalls.routeCompletesBosonPredictions && !report.firewalls.sourceContractApplicationAllowed
    && !report.firewalls.physicalUnitOrGevClaimMade && report.firewalls.promotedPhysicalMassClaimCount == 0,
    "Phase517 source-contract, route-promotion, or physical-claim firewall drifted.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Unexpected terminal verdict.");
string json = JsonSerializer.Serialize(report, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "a5_dual_reflection_candidate_foundation.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_dual_reflection_candidate_foundation_summary.json"), json);
Console.WriteLine(json);

static bool ActionAndParityAreUnambiguous() => false;

static string Decide(bool[] blockers, string[] precedence)
{
    for (int i = 0; i < blockers.Length; i++)
        if (blockers[i]) return precedence[i];
    return precedence[^1];
}

static bool RequiredCandidateFieldsPresent(ReflectionCandidate x) =>
    !string.IsNullOrWhiteSpace(x.ActionMember) && !string.IsNullOrWhiteSpace(x.ActionFunctional)
    && !string.IsNullOrWhiteSpace(x.ConfigurationSpace) && !string.IsNullOrWhiteSpace(x.GaugeStatus)
    && !string.IsNullOrWhiteSpace(x.Measure) && !string.IsNullOrWhiteSpace(x.MeasureDomain)
    && !string.IsNullOrWhiteSpace(x.CouplingDomain) && !string.IsNullOrWhiteSpace(x.VolumeDomain)
    && !string.IsNullOrWhiteSpace(x.ReflectionPlane) && !string.IsNullOrWhiteSpace(x.VertexMap)
    && !string.IsNullOrWhiteSpace(x.OrientedEdgePullback) && !string.IsNullOrWhiteSpace(x.BoundaryRule)
    && !string.IsNullOrWhiteSpace(x.ConjugationRule) && !string.IsNullOrWhiteSpace(x.PositiveTimeAlgebra)
    && !string.IsNullOrWhiteSpace(x.UnchangedObservableMembership) && !string.IsNullOrWhiteSpace(x.ConnectedSubtraction)
    && !string.IsNullOrWhiteSpace(x.ProposedOsBilinear);

static Binding Bind(string id, string path, string expected)
{
    string actual = Sha(path);
    return new Binding(id, path, expected, actual, string.Equals(expected, actual, StringComparison.Ordinal));
}

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string JsonString(string path, string name)
{
    using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
    return doc.RootElement.GetProperty(name).GetString() ?? string.Empty;
}

static bool JsonBool(string path, string name)
{
    using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
    return doc.RootElement.GetProperty(name).GetBoolean();
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static ReflectionCandidate Candidate(
    string id, string centering, string vertexMap, int[] positive, int[] boundary,
    int[] negative, string boundaryRule, string status) => new(
        id, centering, vertexMap, positive, boundary, negative, boundaryRule, status,
        "ambiguous-not-frozen-by-a5",
        "family S_B=0.5*<Upsilon,M*Upsilon>; exact executable member unresolved",
        "Omega_4=R^11520 before quotient",
        "gauge quotient not defined or proved",
        "mu_beta=Z_beta^-1 exp(-beta*S_B)domega, conditional",
        "only if 0<Z_beta<infinity; normalization not proved",
        "beta domain not frozen for an A5 theorem subject",
        "formal L=4 row only; allowed-volume theorem domain not proved",
        centering == "site-centered" ? "fixed vertex planes t=0,2" : "link-centered planes between t=0/1 and t=2/3",
        "conditional on reflected stored-edge closure: + for preserved stored orientation, - for reversal",
        "Theta_c F(omega)=conj(F(R_c omega))",
        "candidate complex polynomial cylinder star-algebra on positive/boundary edge coordinates",
        "O1=Tr(F^2), O2=Tr(Upsilon^2); membership conditional on exact dependency audit",
        "O_tilde=O-mu(O); C_ab(s)=L^-1 sum_tau mu(O_tilde_a(tau+s)O_tilde_b(tau)), conditional on mu",
        "B_c(F,G)=mu((Theta_c F)G), conditional on domains, membership, and finiteness");

internal sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool Matches);

internal sealed record ReflectionCandidate(
    string CandidateId,
    string Centering,
    string VertexMap,
    int[] PositiveVertexSlices,
    int[] BoundaryVertexSlices,
    int[] NegativeVertexSlices,
    string BoundaryRule,
    string Status,
    string ActionMember,
    string ActionFunctional,
    string ConfigurationSpace,
    string GaugeStatus,
    string Measure,
    string MeasureDomain,
    string CouplingDomain,
    string VolumeDomain,
    string ReflectionPlane,
    string OrientedEdgePullback,
    string ConjugationRule,
    string PositiveTimeAlgebra,
    string UnchangedObservableMembership,
    string ConnectedSubtraction,
    string ProposedOsBilinear)
{
    public bool Registered => false;
    public bool A5Validated => false;
    public string CandidateRole => "formal-diagnostic-row-only";
}
