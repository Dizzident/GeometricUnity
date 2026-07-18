using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

const string Root = "studies/phase520_a5_action_subject_lineage_parity_audit_001";
const string ContractPath = Root + "/preregistration/phase520_a5_action_subject_lineage_parity_contract_v1.json";
const string OutputPath = Root + "/output/a5_action_subject_lineage_parity_audit.json";
const string SummaryPath = Root + "/output/a5_action_subject_lineage_parity_audit_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");

var specs = new[]
{
    Spec("phase456-generator", "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs", "phase456GeneratorSha256"),
    Spec("phase456-serialized-a5", "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json", "phase456SerializedA5Sha256"),
    Spec("phase452-subject-record", "studies/phase452_scalar_channel_spectroscopy_probe_001/output/scalar_channel_spectroscopy_probe_summary.json", "phase452SubjectRecordSha256"),
    Spec("phase452-program", "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs", "phase452ProgramSha256"),
    Spec("shiab-operator", "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs", "shiabOperatorSha256"),
    Spec("shiab-family-spec", "src/Gu.ReferenceCpu/EinsteinianShiabFamilySpec.cs", "shiabFamilySpecSha256"),
    Spec("curvature-assembler", "src/Gu.ReferenceCpu/CurvatureAssembler.cs", "curvatureAssemblerSha256"),
    Spec("cpu-mass-matrix", "src/Gu.ReferenceCpu/CpuMassMatrix.cs", "cpuMassMatrixSha256"),
    Spec("identity-shiab", "src/Gu.ReferenceCpu/IdentityShiabCpu.cs", "identityShiabSha256"),
    Spec("lie-algebra-factory", "src/Gu.Math/LieAlgebraFactory.cs", "lieAlgebraFactorySha256"),
    Spec("phase517-summary", "studies/phase517_a5_dual_reflection_candidate_foundation_001/output/a5_dual_reflection_candidate_foundation_summary.json", "phase517SummarySha256"),
    Spec("phase517-contract", "studies/phase517_a5_dual_reflection_candidate_foundation_001/preregistration/phase517_dual_reflection_candidate_foundation_contract_v1.json", "phase517ContractSha256"),
    Spec("phase517-program", "studies/phase517_a5_dual_reflection_candidate_foundation_001/Program.cs", "phase517ProgramSha256"),
    Spec("phase519-summary", "studies/phase519_a5_candidate_foundation_readiness_001/output/a5_candidate_foundation_readiness_summary.json", "phase519SummarySha256"),
};

var bindings = specs.Select(x => Bind(x.Id, x.Path, expected.GetProperty(x.HashKey).GetString()!)).ToArray();
bool exactBindingsValid = bindings.All(x => x.Matches);

string generator = File.ReadAllText(specs.Single(x => x.Id == "phase456-generator").Path);
string serializedA5Text = File.ReadAllText(specs.Single(x => x.Id == "phase456-serialized-a5").Path);
string phase452Program = File.ReadAllText(specs.Single(x => x.Id == "phase452-program").Path);
string familySpec = File.ReadAllText(specs.Single(x => x.Id == "shiab-family-spec").Path);
string curvatureSource = File.ReadAllText(specs.Single(x => x.Id == "curvature-assembler").Path);
string operatorSource = File.ReadAllText(specs.Single(x => x.Id == "shiab-operator").Path);

using var serializedA5Doc = JsonDocument.Parse(serializedA5Text);
using var phase452SubjectDoc = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase452-subject-record").Path));
using var phase517Doc = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase517-summary").Path));
using var phase519Doc = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase519-summary").Path));

bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase520-a18-action-subject-lineage-parity-v1"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && contract.GetProperty("expectedCurrentVerdict").GetString() == "action-member-unresolved"
    && contract.GetProperty("expectedCurrentTerminalStatus").GetString() == "a5-action-subject-lineage-parity-audit-action-member-unresolved"
    && !contract.GetProperty("phase515Or516Unlocked").GetBoolean()
    && !contract.GetProperty("phase458G1Satisfied").GetBoolean()
    && !contract.GetProperty("theoremOrCounterexampleAllowed").GetBoolean()
    && !contract.GetProperty("samplingOrProductionAllowed").GetBoolean()
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && expected.EnumerateObject().Count() == specs.Length
    && PropertyNamesExact(contract, new[]
    {
        "schemaVersion", "contractId", "planSection", "frozenBeforePrecursorConsumption", "expectedBindings",
        "lineageQuestions", "expectedA5ResultFields", "registeredExecutableActionMemberInventory",
        "identityProofPolicy", "registeredOmegaParityEvidenceInventory", "inventoryScope", "terminalPrecedence",
        "auditMode", "futureSelectedMemberArtifactRequiresNewContractAndImplementation",
        "expectedCurrentVerdict", "expectedCurrentTerminalStatus", "phase515Or516Unlocked", "phase458G1Satisfied",
        "theoremOrCounterexampleAllowed", "samplingOrProductionAllowed", "externalReviewPending",
        "promotedPhysicalMassClaimCount",
    })
    && contract.GetProperty("lineageQuestions").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "does-the-frozen-a5-result-have-a-dedicated-qualifier-or-member-field-and-do-its-serialized-values-preserve-the-toy-control-qualifier",
        "does-committed-evidence-uniquely-select-one-executable-a5-action-member",
        "does-an-exact-bound-identity-cancel-every-omega-sign-odd-action-term",
    }, StringComparer.Ordinal)
    && contract.GetProperty("expectedA5ResultFields").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "packItem", "sector", "target", "sbStructure", "provable", "verdict", "obstructions",
        "whatItWouldHaveCancelled", "consumes", "mandatoryFraming",
    }, StringComparer.Ordinal)
    && MemberInventorySchemaValid(contract.GetProperty("registeredExecutableActionMemberInventory"))
    && IdentityProofPolicyValid(contract.GetProperty("identityProofPolicy"))
    && ParityInventorySchemaValid(contract.GetProperty("registeredOmegaParityEvidenceInventory"))
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "invalid-or-drifted-input", "action-member-unresolved",
    }, StringComparer.Ordinal)
    && contract.GetProperty("auditMode").GetString() == "frozen-negative-current-evidence-census"
    && contract.GetProperty("futureSelectedMemberArtifactRequiresNewContractAndImplementation").GetBoolean();

bool precursorSemanticsValid =
    serializedA5Doc.RootElement.GetProperty("verdict").GetString() == "a5-stage-a-gaussian-domination-not-provable-obstruction-recorded"
    && !serializedA5Doc.RootElement.GetProperty("provable").GetBoolean()
    && phase452SubjectDoc.RootElement.GetProperty("applicationSubjectKind").GetString() == "scalar-channel-spectroscopy-probe"
    && phase452SubjectDoc.RootElement.GetProperty("scalarChannelSpectroscopyProbePassed").GetBoolean()
    && phase452SubjectDoc.RootElement.GetProperty("targetBlindConstruction").GetBoolean()
    && !phase452SubjectDoc.RootElement.GetProperty("physicalTargetsConsultedForConstruction").GetBoolean()
    && phase452SubjectDoc.RootElement.GetProperty("tori")[0].GetProperty("memberBatteries").EnumerateArray()
        .Select(x => x.GetProperty("member").GetString()).SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal)
    && phase517Doc.RootElement.GetProperty("inputsValid").GetBoolean()
    && phase517Doc.RootElement.GetProperty("contractValid").GetBoolean()
    && phase517Doc.RootElement.GetProperty("verdictKind").GetString() == "action-member-or-omega-parity-ambiguous"
    && phase517Doc.RootElement.GetProperty("actionSubjectAudit").GetProperty("status").GetString() == "action-member-or-omega-parity-ambiguous"
    && phase519Doc.RootElement.GetProperty("inputsValid").GetBoolean()
    && phase519Doc.RootElement.GetProperty("contractValid").GetBoolean()
    && phase519Doc.RootElement.GetProperty("verdictKind").GetString() == "action-or-observable-subject-ambiguous";

// This establishes only that there is no dedicated qualifier/control/member
// field and that the committed serialized values omit the qualifier. It does
// not establish why the schema was designed that way or that the prose governs
// any particular executable member.
string a5ResultBody = ExtractTypeBody(generator, "sealed class A5Result");
string[] actualA5ResultFields = Regex.Matches(a5ResultBody, @"public\s+[A-Za-z0-9_\[\]]+\s+([A-Za-z0-9_]+)\s*\{")
    .Select(match => match.Groups[1].Value).ToArray();
string[] expectedA5ResultFields = contract.GetProperty("expectedA5ResultFields").EnumerateArray()
    .Select(x => x.GetString()!).ToArray();
bool a5ResultFieldInventoryExact = actualA5ResultFields.SequenceEqual(expectedA5ResultFields, StringComparer.Ordinal)
    && serializedA5Doc.RootElement.EnumerateObject().Select(x => x.Name)
        .SequenceEqual(expectedA5ResultFields, StringComparer.Ordinal);
bool generatorContainsQualifier = generator.Contains("committed toy control branch", StringComparison.Ordinal);
bool dedicatedQualifierOrMemberFieldPresent = actualA5ResultFields.Any(name =>
    name.Contains("qualifier", StringComparison.OrdinalIgnoreCase)
        || name.Contains("control", StringComparison.OrdinalIgnoreCase)
        || name.Contains("member", StringComparison.OrdinalIgnoreCase));
string[] committedSerializedStringValues = JsonStrings(serializedA5Doc.RootElement).ToArray();
string[] committedSerializedToyControlQualifierHits = committedSerializedStringValues.Where(value =>
    value.Contains("toy control", StringComparison.OrdinalIgnoreCase)
    || value.Contains("toy-control", StringComparison.OrdinalIgnoreCase)).ToArray();
bool committedSerializedValuesPreserveToyControlQualifier = committedSerializedToyControlQualifierHits.Length != 0;
bool generatorSerializesA5ResultDirectly = generator.Contains("JsonSerializer.Serialize(a5, jsonOpts)", StringComparison.Ordinal);
bool frozenQualifierFieldAndValueAuditValid = generatorContainsQualifier
    && a5ResultFieldInventoryExact
    && !dedicatedQualifierOrMemberFieldPresent
    && committedSerializedStringValues.Length == 11
    && !committedSerializedValuesPreserveToyControlQualifier
    && generatorSerializesA5ResultDirectly;

// The measurement subject contains two executable family members, while the A5
// function has no member parameter or exact link to either construction.
JsonElement[] registeredMembers = contract.GetProperty("registeredExecutableActionMemberInventory").EnumerateArray().ToArray();
var executableActionMemberCensus = registeredMembers.Select(row =>
{
    string memberId = row.GetProperty("memberId").GetString()!;
    string declarationAnchor = row.GetProperty("declarationAnchor").GetString()!;
    string useAnchor = row.GetProperty("useAnchor").GetString()!;
    bool declarationPresent = phase452Program.Contains(declarationAnchor, StringComparison.Ordinal);
    bool executionUsePresent = phase452Program.Contains(useAnchor, StringComparison.Ordinal);
    bool memberSpecificDefinitionPresent = memberId switch
    {
        "identity" => phase452Program.Contains("Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None", StringComparison.Ordinal)
            && phase452Program.Contains("EpsilonMode = \"trivial\"", StringComparison.Ordinal),
        "sd2-id0/c0.5" => phase452Program.Contains("Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0", StringComparison.Ordinal)
            && phase452Program.Contains("EpsilonMode = \"independent-theta\"", StringComparison.Ordinal),
        _ => false,
    };
    return new { memberId, declarationAnchor, useAnchor, declarationPresent, executionUsePresent, memberSpecificDefinitionPresent };
}).ToArray();
bool actionMemberInventoryComplete = executableActionMemberCensus.Length == 2
    && executableActionMemberCensus.Select(x => x.memberId).SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal)
    && executableActionMemberCensus.All(x => x.declarationPresent && x.executionUsePresent && x.memberSpecificDefinitionPresent)
    && phase452Program.Contains("var memberNames = new[] { \"identity\", \"sd2-id0/c0.5\" }", StringComparison.Ordinal);
bool familyExplicitlyHasNoCanonicalMember = familySpec.Contains("no canonical member", StringComparison.Ordinal);
bool a5FunctionParameterizedByMember = generator.Contains("AttemptA5GaussianDomination(EinsteinianShiabFamilyMember", StringComparison.Ordinal);
string a5FunctionBody = ExtractFunctionBody(generator, "A5Result AttemptA5GaussianDomination()");
bool a5FunctionReferencesExecutableMember = a5FunctionBody.Contains("controlMember", StringComparison.Ordinal)
    || a5FunctionBody.Contains("controlSpec", StringComparison.Ordinal)
    || a5FunctionBody.Contains("sd2Spec", StringComparison.Ordinal)
    || a5FunctionBody.Contains("EinsteinianShiabFamilyMember", StringComparison.Ordinal);
bool uniqueExecutableActionMemberSelected =
    actionMemberInventoryComplete && executableActionMemberCensus.Length == 1
    && !familyExplicitlyHasNoCanonicalMember && a5FunctionParameterizedByMember && a5FunctionReferencesExecutableMember;

// Exact-bound executable sources expose F=L+Q and S=1/2<Upsilon,M Upsilon>.
// The source audit accepts only a registered algebraic identity covering every
// action contribution. A prose assertion or finite evaluation is not such a proof.
bool linearAndQuadraticCurvaturePresent =
    curvatureSource.Contains("F = d(omega) + (1/2)[omega, omega]", StringComparison.Ordinal)
    && curvatureSource.Contains("coefficients[fi * dimG + a] = dOmega[a] + 0.5 * wedgeTerm[a]", StringComparison.Ordinal);
bool quadraticActionPresent =
    operatorSource.Contains("S_B(omega, theta) = (1/2)&lt;Upsilon, M Upsilon&gt;", StringComparison.Ordinal)
    && operatorSource.Contains("objective *= 0.5", StringComparison.Ordinal);
JsonElement registeredInventory = contract.GetProperty("registeredOmegaParityEvidenceInventory");
var parityEvidenceCensus = registeredInventory.EnumerateArray().Select(item =>
{
    string evidenceId = item.GetProperty("evidenceId").GetString()!;
    string bindingId = item.GetProperty("bindingId").GetString()!;
    string requiredText = item.GetProperty("requiredText").GetString()!;
    string boundText = bindingId switch
    {
        "phase456-generator" => generator,
        "phase456-serialized-a5" => serializedA5Doc.RootElement.GetProperty("sbStructure").GetString()!,
        _ => throw new InvalidOperationException($"Unregistered Phase520 parity-evidence binding: {bindingId}"),
    };
    bool assertionPresent = boundText.Contains(requiredText, StringComparison.Ordinal);
    bool sourceLocationMatchesEvidenceKind = bindingId switch
    {
        "phase456-generator" => generator.Split('\n').Any(line => line.TrimStart().StartsWith("//", StringComparison.Ordinal)
            && line.Contains(requiredText, StringComparison.Ordinal)),
        "phase456-serialized-a5" => serializedA5Doc.RootElement.GetProperty("sbStructure").ValueKind == JsonValueKind.String,
        _ => false,
    };
    bool containsExecutableDerivation = sourceLocationMatchesEvidenceKind && item.GetProperty("evidenceKind").GetString() switch
    {
        "source-comment-assertion" => false,
        "serialized-prose-assertion" => false,
        _ => false,
    };
    bool bindsUniqueActionMember = uniqueExecutableActionMemberSelected;
    bool expandsAndCancelsEveryOddCrossContribution = false;
    bool acceptedAsExactIdentity = assertionPresent
        && containsExecutableDerivation
        && bindsUniqueActionMember
        && expandsAndCancelsEveryOddCrossContribution;
    return new
    {
        evidenceId,
        bindingId,
        evidenceKind = item.GetProperty("evidenceKind").GetString(),
        requiredText,
        assertionPresent,
        sourceLocationMatchesEvidenceKind,
        containsExecutableDerivation,
        bindsUniqueActionMember,
        expandsAndCancelsEveryOddCrossContribution,
        acceptedAsExactIdentity,
    };
}).ToArray();
bool frozenInventoryComplete = parityEvidenceCensus.Length == 2
    && parityEvidenceCensus.Select(x => x.evidenceId).SequenceEqual(
        new[] { "generator-z2-comment", "serialized-a5-even-assertion" }, StringComparer.Ordinal)
    && parityEvidenceCensus.All(x => x.assertionPresent && x.sourceLocationMatchesEvidenceKind)
    && registeredInventory.EnumerateArray().All(x => x.EnumerateObject().Select(p => p.Name).SequenceEqual(
        new[] { "evidenceId", "bindingId", "evidenceKind", "requiredText" }, StringComparer.Ordinal));
bool exactIdentityCancelsEveryOmegaOddContribution =
    frozenInventoryComplete && parityEvidenceCensus.Any(x => x.acceptedAsExactIdentity);

bool auditStructureValid = a5ResultFieldInventoryExact
    && frozenQualifierFieldAndValueAuditValid
    && actionMemberInventoryComplete
    && linearAndQuadraticCurvaturePresent
    && quadraticActionPresent
    && frozenInventoryComplete;
bool inputsValid = contractValid && exactBindingsValid && precursorSemanticsValid && auditStructureValid;
string verdict = !inputsValid ? "invalid-or-drifted-input" : "action-member-unresolved";

var result = new
{
    schemaVersion = 1,
    phase = 520,
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    exactBindingsValid,
    precursorSemanticsValid,
    auditStructureValid,
    verdictKind = verdict,
    terminalStatus = "a5-action-subject-lineage-parity-audit-" + verdict,
    decision = verdict == "action-member-unresolved"
        ? "The frozen A5Result schema has no dedicated qualifier, control, or member field, and none of the committed serialized string or string-array values preserves the generator's toy/control qualifier. This does not imply generic schema non-representability, a serialization cause, or semantic applicability. The committed record still does not uniquely link the A5 prose to either executable Phase452 member, and neither registered omega-parity assertion qualifies as an exact derivation."
        : "Fail-closed precedence selected a non-current branch; no authority follows.",
    serializationAudit = new
    {
        generatorContainsQualifier,
        expectedA5ResultFields,
        actualA5ResultFields,
        a5ResultFieldInventoryExact,
        dedicatedQualifierOrMemberFieldPresent,
        committedSerializedStringValueCount = committedSerializedStringValues.Length,
        committedSerializedToyControlQualifierHits,
        committedSerializedValuesPreserveToyControlQualifier,
        generatorSerializesA5ResultDirectly,
        frozenQualifierFieldAndValueAuditValid,
        genericSchemaNonRepresentabilityClaimed = false,
        serializationCauseBeyondSchemaRepresentabilityInferred = false,
        semanticApplicabilityResolved = false,
        serializationRepaired = false,
    },
    actionMemberAudit = new
    {
        registeredMemberCount = registeredMembers.Length,
        executableActionMemberCensus,
        actionMemberInventoryComplete,
        familyExplicitlyHasNoCanonicalMember,
        a5FunctionParameterizedByMember,
        a5FunctionReferencesExecutableMember,
        uniqueExecutableActionMemberSelected,
        status = "multiple-executable-members-without-exact-a5-link",
        actionChoicePerformed = false,
        auditMode = contract.GetProperty("auditMode").GetString(),
        futureSelectedMemberArtifactRequiresNewContractAndImplementation = contract.GetProperty("futureSelectedMemberArtifactRequiresNewContractAndImplementation").GetBoolean(),
    },
    omegaParityAudit = new
    {
        linearAndQuadraticCurvaturePresent,
        quadraticActionPresent,
        inventoryScope = contract.GetProperty("inventoryScope").GetString(),
        frozenInventoryComplete,
        registeredEvidenceCount = parityEvidenceCensus.Length,
        parityEvidenceCensus,
        exactIdentityCancelsEveryOmegaOddContribution,
        numericalParityTreatedAsProof = false,
        exhaustiveMathematicalAbsenceClaimed = false,
        status = exactIdentityCancelsEveryOmegaOddContribution
            ? "registered-exact-bound-identity-qualifies"
            : "no-qualifying-identity-in-frozen-registered-evidence-inventory",
    },
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    bindings,
    externalReviewPending = true,
    phase515Unlocked = false,
    phase516Unlocked = false,
    phase458G1Satisfied = false,
    phase458EvaluationPerformed = false,
    closesLimbL8 = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    phase480OrPhase481WorkPerformed = false,
    humanRulingAuthoredOrInferred = false,
    humanRulingPerformed = false,
    samplingAuthorized = false,
    hmcAuthorized = false,
    benchmarkAuthorized = false,
    productionAuthorized = false,
    launchAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    physicalUnitClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
};

Require(contractValid, "Phase520 frozen contract is invalid.");
Require(inputsValid, "Phase520 exact-bound inputs are invalid or drifted.");
Require(frozenQualifierFieldAndValueAuditValid, "Phase520 dedicated qualifier-field or committed serialized-value census drifted.");
Require(!uniqueExecutableActionMemberSelected, "Phase520 current action-member ambiguity unexpectedly changed.");
Require(frozenInventoryComplete, "Phase520 frozen omega-parity evidence inventory is incomplete or drifted.");
Require(!exactIdentityCancelsEveryOmegaOddContribution, "Phase520 unexpectedly found an exact-bound omega-odd cancellation identity.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase520 terminal drifted from the frozen contract.");
Require("a5-action-subject-lineage-parity-audit-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase520 terminalStatus drifted from the frozen contract.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase520: {verdict}");
Console.WriteLine($"dedicated qualifier/member field present: {dedicatedQualifierOrMemberFieldPresent}");
Console.WriteLine($"committed serialized values preserve toy/control qualifier: {committedSerializedValuesPreserveToyControlQualifier}");
Console.WriteLine($"unique executable member selected: {uniqueExecutableActionMemberSelected}");
Console.WriteLine($"qualifying identity in frozen registered inventory: {exactIdentityCancelsEveryOmegaOddContribution}");

(string Id, string Path, string HashKey) Spec(string id, string path, string hashKey) => (id, path, hashKey);

BindingResult Bind(string id, string path, string expectedSha256)
{
    string actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    return new BindingResult(id, path, expectedSha256, actual, actual == expectedSha256);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static string ExtractTypeBody(string source, string declaration) => ExtractBraceBody(source, source.IndexOf(declaration, StringComparison.Ordinal));
static string ExtractFunctionBody(string source, string declaration) => ExtractBraceBody(source, source.IndexOf(declaration, StringComparison.Ordinal));
static string ExtractBraceBody(string source, int declarationIndex)
{
    if (declarationIndex < 0) throw new InvalidOperationException("Phase520 required source declaration is missing.");
    int open = source.IndexOf('{', declarationIndex);
    if (open < 0) throw new InvalidOperationException("Phase520 required source declaration has no body.");
    int depth = 0;
    for (int index = open; index < source.Length; index++)
    {
        if (source[index] == '{') depth++;
        else if (source[index] == '}' && --depth == 0) return source[(open + 1)..index];
    }
    throw new InvalidOperationException("Phase520 required source declaration body is unterminated.");
}

static bool PropertyNamesExact(JsonElement element, string[] expected) =>
    element.EnumerateObject().Select(x => x.Name).Order(StringComparer.Ordinal)
        .SequenceEqual(expected.Order(StringComparer.Ordinal), StringComparer.Ordinal);

static bool MemberInventorySchemaValid(JsonElement inventory)
{
    JsonElement[] rows = inventory.EnumerateArray().ToArray();
    return rows.Length == 2
        && rows.All(row => PropertyNamesExact(row, new[] { "memberId", "declarationAnchor", "useAnchor" }))
        && rows.Select(row => row.GetProperty("memberId").GetString())
            .SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal);
}

static bool IdentityProofPolicyValid(JsonElement policy) =>
    PropertyNamesExact(policy, new[] { "accepted", "rejected" })
    && policy.GetProperty("accepted").GetString() == "an explicit exact-bound algebraic identity covering every action contribution"
    && policy.GetProperty("rejected").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "finite numerical parity samples",
        "comments or serialized assertions contradicted by executable structure",
        "choosing an action member for convenience",
        "assuming cross-face cancellation without an exact identity",
    }, StringComparer.Ordinal);

static bool ParityInventorySchemaValid(JsonElement inventory)
{
    JsonElement[] rows = inventory.EnumerateArray().ToArray();
    return rows.Length == 2
        && rows.All(row => PropertyNamesExact(row, new[] { "evidenceId", "bindingId", "evidenceKind", "requiredText" }))
        && rows.Select(row => row.GetProperty("evidenceId").GetString()).SequenceEqual(
            new[] { "generator-z2-comment", "serialized-a5-even-assertion" }, StringComparer.Ordinal)
        && rows.Select(row => row.GetProperty("bindingId").GetString()).SequenceEqual(
            new[] { "phase456-generator", "phase456-serialized-a5" }, StringComparer.Ordinal)
        && rows.Select(row => row.GetProperty("evidenceKind").GetString()).SequenceEqual(
            new[] { "source-comment-assertion", "serialized-prose-assertion" }, StringComparer.Ordinal);
}

static IEnumerable<string> JsonStrings(JsonElement element)
{
    if (element.ValueKind == JsonValueKind.String)
    {
        yield return element.GetString()!;
        yield break;
    }
    if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (JsonElement child in element.EnumerateArray())
            foreach (string value in JsonStrings(child)) yield return value;
        yield break;
    }
    if (element.ValueKind == JsonValueKind.Object)
        foreach (JsonProperty property in element.EnumerateObject())
            foreach (string value in JsonStrings(property.Value)) yield return value;
}

sealed class BindingResult
{
    public BindingResult(string id, string path, string expectedSha256, string actualSha256, bool matches)
    {
        Id = id;
        Path = path;
        ExpectedSha256 = expectedSha256;
        ActualSha256 = actualSha256;
        Matches = matches;
    }

    public string Id { get; }
    public string Path { get; }
    public string ExpectedSha256 { get; }
    public string ActualSha256 { get; }
    public bool Matches { get; }
}
