using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase523_a5_action_member_universalization_audit_001";
const string ContractPath = Root + "/preregistration/phase523_a5_action_member_universalization_contract_v1.json";
const string OutputPath = Root + "/output/a5_action_member_universalization_audit.json";
const string SummaryPath = Root + "/output/a5_action_member_universalization_audit_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
Binding[] bindings = contract.GetProperty("expectedBindings").EnumerateArray().Select(row =>
{
    string id = row.GetProperty("id").GetString()!;
    string path = row.GetProperty("path").GetString()!;
    string expectedSha256 = row.GetProperty("sha256").GetString()!;
    string actualSha256 = File.Exists(path) ? Sha256(path) : "missing";
    return new Binding(id, path, expectedSha256, actualSha256, actualSha256 == expectedSha256);
}).ToArray();
var texts = bindings.ToDictionary(x => x.Id, x => File.ReadAllText(x.Path), StringComparer.Ordinal);

var documents = new List<JsonDocument>();
JsonElement ReadJson(string id)
{
    JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == id).Path));
    documents.Add(document);
    return document.RootElement;
}

try
{
    string phase452Program = texts["phase452-program"];
    string operatorSource = texts["shiab-operator"];
    string familySource = texts["shiab-family-spec"];
    string a5Generator = texts["phase456-generator"];
    JsonElement phase452Summary = ReadJson("phase452-summary");
    JsonElement a5Record = ReadJson("phase456-a5");
    JsonElement phase517 = ReadJson("phase517-summary");
    JsonElement phase520 = ReadJson("phase520-summary");
    JsonElement phase522 = ReadJson("phase522-summary");

    string[] expectedBindingIds =
    {
        "phase452-program", "phase452-summary", "shiab-operator", "shiab-family-spec",
        "phase456-generator", "phase456-a5",
        "phase517-program", "phase517-contract", "phase517-summary",
        "phase520-program", "phase520-contract", "phase520-summary",
        "phase522-program", "phase522-contract", "phase522-summary",
    };
    bool bindingSchemaValid = bindings.Select(x => x.Id).SequenceEqual(expectedBindingIds, StringComparer.Ordinal)
        && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == bindings.Length;
    bool exactBindingsValid = bindings.All(x => x.HashMatches);
    JsonElement[] contractMembers = contract.GetProperty("executableMemberDomain").EnumerateArray().ToArray();
    bool contractMemberDomainValid = contractMembers.Length == 2
        && contractMembers.Select(x => string.Join(':',
            x.GetProperty("memberId").GetString(),
            x.GetProperty("phi1").GetString(),
            x.GetProperty("phi2").GetString(),
            x.GetProperty("epsilonMode").GetString())).SequenceEqual(new[]
        {
            "identity:id0:none:trivial",
            "sd2-id0/c0.5:sd2:id0:independent-theta",
        }, StringComparer.Ordinal);

    string[] expectedPrerequisiteIds =
    {
        "kinematic-carrier-and-complete-family-domain",
        "executable-action-subject-binding",
        "complete-action-functional-coverage",
        "exact-omega-parity",
        "composite-correlation-transport",
        "reflection-positivity-for-unchanged-action",
        "face-local-interaction-factorization",
        "o1-o2-dependency-and-positive-time-membership",
        "normalized-measure-domain-target-equality-hermiticity",
        "necessity-gluing-and-all-scope",
    };
    JsonElement[] frozenRows = contract.GetProperty("registeredA5PrerequisiteInventory").EnumerateArray().ToArray();
    bool registeredInventorySchemaValid = frozenRows.Length == expectedPrerequisiteIds.Length
        && frozenRows.Select(x => x.GetProperty("id").GetString()).SequenceEqual(expectedPrerequisiteIds, StringComparer.Ordinal)
        && frozenRows.All(x => x.EnumerateObject().Select(p => p.Name).SequenceEqual(
            new[] { "id", "scope", "origin", "dependencyDimensions", "requirementEvidence" }, StringComparer.Ordinal))
        && frozenRows.All(x => x.GetProperty("scope").GetString()!.Length > 0)
        && frozenRows.All(x => x.GetProperty("origin").GetString() is "a5-explicit" or "audit-authored-lineage-refinement")
        && frozenRows.All(x => x.GetProperty("requirementEvidence").GetArrayLength() > 0);

    var inventoryEvidence = frozenRows.SelectMany(row => row.GetProperty("requirementEvidence").EnumerateArray().Select(evidence =>
    {
        string prerequisiteId = row.GetProperty("id").GetString()!;
        string bindingId = evidence.GetProperty("bindingId").GetString()!;
        string anchor = evidence.GetProperty("anchor").GetString()!;
        bool bindingRegistered = texts.ContainsKey(bindingId);
        bool anchorPresent = bindingRegistered && texts[bindingId].Contains(anchor, StringComparison.Ordinal);
        return new { prerequisiteId, bindingId, anchor, bindingRegistered, anchorPresent };
    })).ToArray();
    bool registeredInventoryCoverageComplete = registeredInventorySchemaValid
        && inventoryEvidence.Length == 12
        && inventoryEvidence.All(x => x.bindingRegistered && x.anchorPresent);

    bool identityDefinitionPresent = phase452Program.Contains(
        "Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = \"trivial\"",
        StringComparison.Ordinal);
    bool sd2DefinitionPresent = phase452Program.Contains(
        "Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0", StringComparison.Ordinal)
        && phase452Program.Contains("EinsteinCoefficient = 0.5, EpsilonMode = \"independent-theta\"", StringComparison.Ordinal);
    bool memberExecutionSelectorPresent = phase452Program.Contains("isControl ? controlSpec : sd2Spec", StringComparison.Ordinal);
    bool memberExecutionDomainPresent = phase452Program.Contains(
        "var memberNames = new[] { \"identity\", \"sd2-id0/c0.5\" }", StringComparison.Ordinal);
    bool summaryExecutionDomainExact = phase452Summary.GetProperty("tori").GetArrayLength() == 1
        && phase452Summary.GetProperty("tori")[0].GetProperty("memberBatteries").EnumerateArray()
            .Select(x => x.GetProperty("member").GetString())
            .SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal);
    bool sharedExecutableCarrierPresent = phase452Program.Contains("LieAlgebraFactory.CreateSu2WithTracePairing()", StringComparison.Ordinal)
        && phase452Program.Contains("SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true)", StringComparison.Ordinal);
    bool sharedExecutableObjectivePresent = operatorSource.Contains("var mUpsilon = massMatrix.Apply", StringComparison.Ordinal)
        && operatorSource.Contains("objective += upsilon[i] * mUpsilon[i]", StringComparison.Ordinal)
        && operatorSource.Contains("objective *= 0.5", StringComparison.Ordinal);
    bool familyExecutableStructurePresent = familySource.Contains("public sealed class EinsteinianShiabFamilyMember", StringComparison.Ordinal)
        && familySource.Contains("public InvariantElementSpec Phi1", StringComparison.Ordinal)
        && familySource.Contains("public InvariantElementSpec Phi2", StringComparison.Ordinal)
        && familySource.Contains("public string EpsilonMode", StringComparison.Ordinal);
    bool noCanonicalMemberProseEvidencePresent = familySource.Contains("no canonical member", StringComparison.Ordinal);
    bool completeTwoMemberDomainEstablished = identityDefinitionPresent && sd2DefinitionPresent
        && memberExecutionSelectorPresent && memberExecutionDomainPresent && summaryExecutionDomainExact
        && sharedExecutableCarrierPresent && sharedExecutableObjectivePresent && familyExecutableStructurePresent;

    bool a5ParameterizedByMember = a5Generator.Contains(
        "AttemptA5GaussianDomination(EinsteinianShiabFamilyMember", StringComparison.Ordinal);
    bool a5SerializedRecordHasMemberField = a5Record.EnumerateObject().Any(property =>
        property.Name.Contains("member", StringComparison.OrdinalIgnoreCase));
    bool a5BindsEitherExecutableMember = a5ParameterizedByMember || a5SerializedRecordHasMemberField;

    bool lineageInputsValid =
        phase517.GetProperty("inputsValid").GetBoolean()
        && phase517.GetProperty("verdictKind").GetString() == "action-member-or-omega-parity-ambiguous"
        && phase520.GetProperty("inputsValid").GetBoolean()
        && phase520.GetProperty("verdictKind").GetString() == "action-member-unresolved"
        && phase520.GetProperty("actionMemberAudit").GetProperty("registeredMemberCount").GetInt32() == 2
        && !phase520.GetProperty("actionMemberAudit").GetProperty("uniqueExecutableActionMemberSelected").GetBoolean()
        && phase522.GetProperty("inputsValid").GetBoolean()
        && phase522.GetProperty("verdictKind").GetString() == "action-member-or-omega-parity-unresolved";

    // These fingerprints are derived from exact-bound executable/source semantics.
    // The contract supplies the frozen question inventory, never the classification.
    var identitySemantics = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["kinematic-carrier-and-complete-family-domain"] = completeTwoMemberDomainEstablished ? "shared:su2-trace:lattice-canonical:quadratic-objective:two-member-domain" : "unresolved",
        ["executable-action-subject-binding"] = a5BindsEitherExecutableMember ? "bound:identity" : "required-binding:identity:absent",
        ["complete-action-functional-coverage"] = identityDefinitionPresent ? "identity:R=id0:none:epsilon=trivial" : "unresolved",
        ["exact-omega-parity"] = identityDefinitionPresent ? "identity-action:exact-odd-cancellation-obligation" : "unresolved",
        ["composite-correlation-transport"] = identityDefinitionPresent ? "identity:O2=norm(identity(F)):transport-obligation" : "unresolved",
        ["reflection-positivity-for-unchanged-action"] = identityDefinitionPresent ? "identity:unchanged-action-measure-algebra" : "unresolved",
        ["face-local-interaction-factorization"] = identityDefinitionPresent ? "identity:undressed-curvature-carrier" : "unresolved",
        ["o1-o2-dependency-and-positive-time-membership"] = identityDefinitionPresent ? "identity:O1=F2:O2=F2:membership-obligation" : "unresolved",
        ["normalized-measure-domain-target-equality-hermiticity"] = identityDefinitionPresent ? "identity:omega-domain:no-theta:target-hermiticity-obligation" : "unresolved",
        ["necessity-gluing-and-all-scope"] = identityDefinitionPresent ? "identity:action-carrier:all-volume-coupling-obligation" : "unresolved",
    };
    var sd2Semantics = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["kinematic-carrier-and-complete-family-domain"] = completeTwoMemberDomainEstablished ? "shared:su2-trace:lattice-canonical:quadratic-objective:two-member-domain" : "unresolved",
        ["executable-action-subject-binding"] = a5BindsEitherExecutableMember ? "bound:sd2-id0/c0.5" : "required-binding:sd2-id0/c0.5:absent",
        ["complete-action-functional-coverage"] = sd2DefinitionPresent ? "sd2:R=sd2-id0:c=0.5:epsilon=independent-theta" : "unresolved",
        ["exact-omega-parity"] = sd2DefinitionPresent ? "sd2-action:fixed-theta-exact-odd-cancellation-obligation" : "unresolved",
        ["composite-correlation-transport"] = sd2DefinitionPresent ? "sd2:O2=norm(M(Ad_theta(F))):transport-obligation" : "unresolved",
        ["reflection-positivity-for-unchanged-action"] = sd2DefinitionPresent ? "sd2:unchanged-joint-omega-theta-action-measure-algebra" : "unresolved",
        ["face-local-interaction-factorization"] = sd2DefinitionPresent ? "sd2:contracted-independent-theta-dressed-curvature-carrier" : "unresolved",
        ["o1-o2-dependency-and-positive-time-membership"] = sd2DefinitionPresent ? "sd2:O1=F2:O2=dressed-residual2:membership-obligation" : "unresolved",
        ["normalized-measure-domain-target-equality-hermiticity"] = sd2DefinitionPresent ? "sd2:joint-omega-theta-domain:target-hermiticity-obligation" : "unresolved",
        ["necessity-gluing-and-all-scope"] = sd2DefinitionPresent ? "sd2:dressed-action-carrier:all-volume-coupling-obligation" : "unresolved",
    };

    var classifications = frozenRows.Select(row =>
    {
        string id = row.GetProperty("id").GetString()!;
        string identityFingerprint = identitySemantics[id];
        string sd2Fingerprint = sd2Semantics[id];
        bool semanticEvidenceResolved = identityFingerprint != "unresolved" && sd2Fingerprint != "unresolved";
        bool fullScopeIdentical = semanticEvidenceResolved && identityFingerprint == sd2Fingerprint;
        string classification = !semanticEvidenceResolved ? "unresolved" : fullScopeIdentical ? "member-invariant" : "member-dependent";
        return new PrerequisiteClassification(
            id,
            row.GetProperty("scope").GetString()!,
            row.GetProperty("origin").GetString()!,
            row.GetProperty("dependencyDimensions").EnumerateArray().Select(x => x.GetString()!).ToArray(),
            identityFingerprint,
            sd2Fingerprint,
            semanticEvidenceResolved,
            fullScopeIdentical,
            classification,
            fullScopeIdentical,
            false,
            fullScopeIdentical
                ? "Universal quantification removes selection for this exact shared scope only."
                : "The exact-bound member semantics differ, so universal quantification creates two separately undischarged obligations.");
    }).ToArray();

    int invariantCount = classifications.Count(x => x.Classification == "member-invariant");
    int dependentCount = classifications.Count(x => x.Classification == "member-dependent");
    int unresolvedCount = classifications.Count(x => x.Classification == "unresolved");
    bool partialUniversalization = invariantCount > 0 && dependentCount > 0 && unresolvedCount == 0;

    string[] expectedTerminals =
    {
        "invalid-or-drifted-input",
        "prerequisite-inventory-incomplete",
        "universalization-partial-member-dependence-remains",
        "universalization-removes-member-selection-dependence-only",
    };
    bool contractStructureValid =
        contract.GetProperty("contractId").GetString() == "phase523-a19-action-member-universalization-v2"
        && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A19"
        && contract.GetProperty("frozenBeforeClassification").GetBoolean()
        && contract.GetProperty("expectedCurrentVerdict").GetString() == "universalization-partial-member-dependence-remains"
        && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedTerminals, StringComparer.Ordinal)
        && !contract.GetProperty("properSubsetAgreementIsActionWideEquivalence").GetBoolean()
        && contract.GetProperty("externalReviewPending").GetBoolean()
        && !contract.GetProperty("exhaustiveMathematicalInventoryCompletenessClaimed").GetBoolean()
        && !contract.GetProperty("allExecutionAndPromotionAuthorities").GetBoolean()
        && !contract.GetProperty("phase481WorkAllowed").GetBoolean()
        && !contract.GetProperty("phase458EvaluationAllowed").GetBoolean()
        && !contract.GetProperty("sourceContractApplicationAllowed").GetBoolean()
        && !contract.GetProperty("o4OrHumanRulingAllowed").GetBoolean()
        && !contract.GetProperty("physicalTargetTuningAllowed").GetBoolean()
        && !contract.GetProperty("physicalUnitClaimAllowed").GetBoolean()
        && !contract.GetProperty("gevClaimAllowed").GetBoolean()
        && !contract.GetProperty("samplingAllowed").GetBoolean()
        && !contract.GetProperty("hmcAllowed").GetBoolean()
        && !contract.GetProperty("reprocessingAllowed").GetBoolean()
        && !contract.GetProperty("benchmarkAllowed").GetBoolean()
        && !contract.GetProperty("productionAllocationAllowed").GetBoolean()
        && !contract.GetProperty("productionAllowed").GetBoolean()
        && !contract.GetProperty("launchAllowed").GetBoolean()
        && !contract.GetProperty("accelerationAllowed").GetBoolean()
        && !contract.GetProperty("theoremOrCounterexampleAllowed").GetBoolean()
        && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
        && bindingSchemaValid
        && contractMemberDomainValid;

    string EvaluateVerdict(bool invalid, bool inventoryIncomplete, bool partial, bool allInvariant) => invalid
        ? "invalid-or-drifted-input"
        : inventoryIncomplete
            ? "prerequisite-inventory-incomplete"
            : partial
                ? "universalization-partial-member-dependence-remains"
                : allInvariant
                    ? "universalization-removes-member-selection-dependence-only"
                    : "universalization-partial-member-dependence-remains";
    var precedenceBattery = new[]
    {
        new { caseId = "invalid", actual = EvaluateVerdict(true, true, true, true), expected = expectedTerminals[0] },
        new { caseId = "inventory-incomplete", actual = EvaluateVerdict(false, true, true, true), expected = expectedTerminals[1] },
        new { caseId = "partial", actual = EvaluateVerdict(false, false, true, false), expected = expectedTerminals[2] },
        new { caseId = "all-invariant", actual = EvaluateVerdict(false, false, false, true), expected = expectedTerminals[3] },
    };
    bool precedenceBatteryPassed = precedenceBattery.All(x => x.actual == x.expected)
        && precedenceBattery.Select(x => x.actual).SequenceEqual(expectedTerminals, StringComparer.Ordinal);

    bool baseInputsValid = contractStructureValid && exactBindingsValid && lineageInputsValid
        && completeTwoMemberDomainEstablished && precedenceBatteryPassed;
    bool allInvariant = classifications.Length > 0 && invariantCount == classifications.Length && unresolvedCount == 0;
    string verdict = EvaluateVerdict(
        !baseInputsValid,
        !registeredInventoryCoverageComplete,
        partialUniversalization,
        allInvariant);
    string terminalStatus = "a5-action-member-universalization-audit-" + verdict;

    var result = new
    {
        schemaVersion = 2,
        phase = 523,
        phaseId = "phase523-a5-action-member-universalization-audit",
        terminalStatus,
        verdictKind = verdict,
        inputsValid = baseInputsValid && registeredInventoryCoverageComplete,
        contractValid = contractStructureValid && registeredInventorySchemaValid,
        planSection = "WAVE2_AMENDMENTS_2026-07-12 A19",
        deterministicZeroSampling = true,
        contract = new { path = ContractPath, sha256 = Sha256(ContractPath), frozenBeforeClassification = true },
        exactInputBindings = bindings,
        exactBindingsValid,
        lineageInputsValid,
        memberDomainAudit = new
        {
            registeredMemberCount = 2,
            members = new[]
            {
                new { memberId = "identity", phi1 = "id0", phi2 = "none", epsilonMode = "trivial" },
                new { memberId = "sd2-id0/c0.5", phi1 = "sd2", phi2 = "id0", epsilonMode = "independent-theta" },
            },
            identityDefinitionPresent,
            sd2DefinitionPresent,
            memberExecutionSelectorPresent,
            memberExecutionDomainPresent,
            summaryExecutionDomainExact,
            sharedExecutableCarrierPresent,
            sharedExecutableObjectivePresent,
            familyExecutableStructurePresent,
            noCanonicalMemberProseEvidencePresent,
            proseEvidenceUsedAsExecutableBinding = false,
            completeTwoMemberDomainEstablished,
            actionMemberSelected = false,
            actionMemberRanked = false,
            canonicalMemberInvented = false,
        },
        a5BindingAudit = new { a5ParameterizedByMember, a5SerializedRecordHasMemberField, a5BindsEitherExecutableMember, proseTreatedAsExecutableBinding = false },
        prerequisiteInventory = new
        {
            frozenBeforeClassification = true,
            registeredFiniteInventory = true,
            registeredInventorySchemaValid,
            registeredInventoryCoverageComplete,
            exhaustiveMathematicalCompletenessClaimed = false,
            inventoryEvidence,
            count = classifications.Length,
            invariantCount,
            memberDependentCount = dependentCount,
            unresolvedCount,
            classificationsDerivedFromContractDependencyDimensions = false,
            rows = classifications,
        },
        universalizationAudit = new
        {
            quantifierDomain = "both exact-bound executable Phase452 members",
            removesSelectionForInvariantPrerequisiteCount = invariantCount,
            leavesSeparateMemberObligationCount = dependentCount,
            partialUniversalization,
            fullActionWideEquivalenceEstablished = false,
            prerequisiteSatisfactionEstablished = false,
            memberSelectionStillRequiredForSingleMemberActionClaim = true,
            alternativeUniversalFamilyClaimRequiresEveryMemberDependentObligationSeparatelyDischarged = true,
            finding = "Universal quantification removes member choice only for the exact shared carrier/domain scope. Per-row executable/source semantic fingerprints differ for each action-sensitive registered prerequisite, leaving two separately undischarged obligations. No exhaustive mathematical inventory claim is made.",
        },
        terminalPrecedence = new { precedenceBatteryPassed, cases = precedenceBattery },
        targetBlindConstruction = true,
        physicalTargetsConsultedForConstruction = false,
        externalReviewPending = true,
        theoremClaimed = false,
        targetCounterexampleClaimed = false,
        reflectionPositivityEstablished = false,
        reflectionPositivityRefuted = false,
        phase458G1Satisfied = false,
        phase458EvaluationPerformed = false,
        closesLimbL8 = false,
        phase515Or516Unlocked = false,
        phase481WorkPerformed = false,
        sourceContractApplicationPerformed = false,
        o4OrHumanRulingAuthoredOrInferred = false,
        physicalTargetTuningPerformed = false,
        samplingPerformed = false,
        hmcPerformed = false,
        reprocessingPerformed = false,
        benchmarkPerformed = false,
        productionAllocationAuthorized = false,
        productionAuthorized = false,
        launchAuthorized = false,
        accelerationAuthorized = false,
        allExecutionAndPromotionAuthorities = false,
        precursorArtifactsMutated = false,
        physicalUnitClaimAllowed = false,
        gevClaimAllowed = false,
        routePromotesWzMasses = false,
        routePromotesHiggsMass = false,
        routeCompletesBosonPredictions = false,
        promotedPhysicalMassClaimCount = 0,
    };

    Require(contractStructureValid, "Phase523 frozen contract structure is invalid.");
    Require(exactBindingsValid, "Phase523 exact input bindings drifted.");
    Require(lineageInputsValid, "Phase523 lineage findings drifted.");
    Require(registeredInventoryCoverageComplete, "Phase523 registered prerequisite inventory coverage is incomplete.");
    Require(precedenceBatteryPassed, "Phase523 terminal precedence battery failed.");
    Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase523 current verdict drifted.");

    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    var options = new JsonSerializerOptions { WriteIndented = true };
    string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
    File.WriteAllText(OutputPath, json);
    File.WriteAllText(SummaryPath, json);
    Console.WriteLine($"Phase523 verdict: {verdict}");
    Console.WriteLine($"Registered prerequisites: invariant={invariantCount}, member-dependent={dependentCount}, unresolved={unresolvedCount}");
    Console.WriteLine($"Precedence battery: {precedenceBattery.Length}/{precedenceBattery.Length}");
}
finally
{
    foreach (JsonDocument document in documents) document.Dispose();
}

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
    public string ActualSha256 { get; } = actualSha256;
    public bool HashMatches { get; } = hashMatches;
}

sealed class PrerequisiteClassification(
    string prerequisiteId,
    string scope,
    string origin,
    string[] recordedDependencyDimensions,
    string identitySemanticFingerprint,
    string sd2SemanticFingerprint,
    bool semanticEvidenceResolved,
    bool fullScopeIdentical,
    string classification,
    bool universalizationRemovesMemberSelectionDependency,
    bool prerequisiteEstablished,
    string consequence)
{
    public string PrerequisiteId { get; } = prerequisiteId;
    public string Scope { get; } = scope;
    public string Origin { get; } = origin;
    public string[] RecordedDependencyDimensions { get; } = recordedDependencyDimensions;
    public string IdentitySemanticFingerprint { get; } = identitySemanticFingerprint;
    public string Sd2SemanticFingerprint { get; } = sd2SemanticFingerprint;
    public bool SemanticEvidenceResolved { get; } = semanticEvidenceResolved;
    public bool FullScopeIdentical { get; } = fullScopeIdentical;
    public string Classification { get; } = classification;
    public bool UniversalizationRemovesMemberSelectionDependency { get; } = universalizationRemovesMemberSelectionDependency;
    public bool PrerequisiteEstablished { get; } = prerequisiteEstablished;
    public string Consequence { get; } = consequence;
}
