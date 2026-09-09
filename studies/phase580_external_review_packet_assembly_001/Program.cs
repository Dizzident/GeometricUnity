using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase580 assembles review materials only. It performs no physics compute,
// authors no ruling, verifies no signature, and changes no pending flag.

const string Root = "studies/phase580_external_review_packet_assembly_001";
const string ContractPath = Root + "/preregistration/phase580_external_review_packet_assembly_contract_v5.json";
const string PacketDirectory = Root + "/output/packet";
const string GuidePath = PacketDirectory + "/REVIEWER_GUIDE.md";
const string PacketPath = PacketDirectory + "/o4_external_review_packet.json";
const string SummaryPath = PacketDirectory + "/o4_external_review_packet_summary.json";

using JsonDocument contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
Binding[] specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!,
    x.GetProperty("path").GetString()!,
    x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new
    {
        id = x.Id,
        path = x.Path,
        expectedSha256 = x.Sha256,
        actualSha256 = actual,
        hashMatches = actual == x.Sha256,
    };
}).ToArray();

string[] judgmentCallIds = contract.GetProperty("judgmentCallIds").EnumerateArray()
    .Select(x => x.GetString()!).ToArray();
string[] evidenceBackedProposalIds = contract.GetProperty("evidenceBackedProposalIds").EnumerateArray()
    .Select(x => x.GetString()!).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray()
    .Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input",
    "packet-content-or-reviewer-request-invalid",
    "external-review-packet-assembled-readiness-only",
];
string[] allO4Ids =
[
    "O4-F1-INVARIANT-RAYS",
    "O4-F1-COLLECTIVE-COORDINATE",
    "O4-F1-FP-NORMALIZATION",
    "O4-F2-POSITIVE-MODE-IR",
    "O4-F3-THETA-HAAR",
    "O4-F4-SADDLE-BACKGROUNDS",
    "O4-E1-P447-SOFT-FLOOR",
    "O4-E2-P453-UNIFORM-LADDER",
    "O4-P455-ZERO-MODE",
    "O4-P455-SB-MODEL",
    "O4-C1-COMPACT-REAL-FORM",
    "O4-C2-YHALF-BOOKKEEPING",
    "O4-C3-WS3-MPROBE-SCOPE",
];

bool exactBindingsValid = bindings.Length == 12
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 12
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 12
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 12
    && bindings.All(x => x.hashMatches);
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 580
    && contract.GetProperty("phaseId").GetString() == "phase580-external-review-packet-assembly"
    && contract.GetProperty("contractId").GetString() == "phase580-a42-external-review-packet-assembly-v5"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A42"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("zeroCompute").GetBoolean()
    && contract.GetProperty("zeroSampling").GetBoolean()
    && !contract.GetProperty("rngUsed").GetBoolean()
    && contract.GetProperty("packetDirectory").GetString() == PacketDirectory
    && contract.GetProperty("reviewerGuidePath").GetString() == GuidePath
    && contract.GetProperty("packetJsonPath").GetString() == PacketPath
    && contract.GetProperty("packetSummaryPath").GetString() == SummaryPath
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && contract.GetProperty("authorityFirewalls").EnumerateObject()
        .All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

// Deterministic shape battery, before parsing any exact-bound review input.
bool partitionCountsCorrect = judgmentCallIds.Length == 2 && evidenceBackedProposalIds.Length == 11;
bool partitionUnique = judgmentCallIds.Distinct(StringComparer.Ordinal).Count() == 2
    && evidenceBackedProposalIds.Distinct(StringComparer.Ordinal).Count() == 11;
bool partitionDisjoint = !judgmentCallIds.Intersect(evidenceBackedProposalIds, StringComparer.Ordinal).Any();
bool partitionCoversExactO4Set = judgmentCallIds.Concat(evidenceBackedProposalIds)
    .ToHashSet(StringComparer.Ordinal).SetEquals(allO4Ids);
bool exactJudgmentCalls = judgmentCallIds.SequenceEqual(
    ["O4-P455-SB-MODEL", "O4-C3-WS3-MPROBE-SCOPE"], StringComparer.Ordinal);
byte[] checksumFixture = Encoding.UTF8.GetBytes("{\"phase\":580,\"fixture\":\"packet\"}");
byte[] checksumTamper = (byte[])checksumFixture.Clone();
checksumTamper[^2] ^= 1;
bool checksumTamperDetected = !SHA256.HashData(checksumFixture).SequenceEqual(SHA256.HashData(checksumTamper));
bool knownAnswerPassed = partitionCountsCorrect && partitionUnique && partitionDisjoint
    && partitionCoversExactO4Set && exactJudgmentCalls && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedReviewInputParsedBeforeBattery = false,
    partitionCountsCorrect,
    partitionUnique,
    partitionDisjoint,
    partitionCoversExactO4Set,
    exactJudgmentCalls,
    checksumTamperDetected,
    passed = knownAnswerPassed,
};

string BoundPath(string id) => specs.Single(x => x.Id == id).Path;
JsonElement BoundJson(string id) => JsonDocument.Parse(File.ReadAllBytes(BoundPath(id))).RootElement.Clone();
string BoundText(string id) => File.ReadAllText(BoundPath(id));

string assessment = BoundText("o4-internal-assessment");
string coverageRegister = BoundText("o4-coverage-register");
JsonElement coverageContract = BoundJson("o4-coverage-contract");
string memoSchemaText = BoundText("o4-human-memo-schema");
JsonElement memoTemplate = BoundJson("o4-human-memo-template");
string signingInstructions = BoundText("phase480-signing-instructions");
JsonElement intakeContract = BoundJson("phase480-intake-contract");
JsonElement reviewerRegistry = BoundJson("phase480-empty-reviewer-registry");
JsonElement phase555 = BoundJson("phase555-escalation-packet");
JsonElement phase485 = BoundJson("phase485-falsifier-census");
JsonElement phase578 = BoundJson("phase578-conditional-electroweak-ledger");
JsonElement phase579 = BoundJson("phase579-collective-coordinate-self-check");

bool assessmentValid = assessment.Contains("STATUS: INTERNAL, NON-AUTHORITATIVE, MACHINE-AUTHORED", StringComparison.Ordinal)
    && assessment.Contains("all eleven machine-proposed dispositions now carry", StringComparison.Ordinal)
    && assessment.Contains("one preserved", StringComparison.Ordinal)
    && assessment.Contains("implementation negative from Phase579", StringComparison.Ordinal)
    && assessment.Contains("Every Phase485 row", StringComparison.Ordinal)
    && assessment.Contains("mayAuthorRuling: false", StringComparison.Ordinal)
    && assessment.Contains("all 31", StringComparison.Ordinal)
    && assessment.Contains("promotedPhysicalMassClaimCount=0", StringComparison.Ordinal)
    && allO4Ids.All(id => assessment.Contains(id, StringComparison.Ordinal));

string[] coverageIds = coverageContract.GetProperty("reviewItems").EnumerateArray()
    .Select(x => x.GetProperty("id").GetString()!).ToArray();
string[] coverageTitles = coverageContract.GetProperty("reviewItems").EnumerateArray()
    .Select(x => x.GetProperty("title").GetString()!).ToArray();
bool coverageValid = coverageContract.GetProperty("schemaVersion").GetInt32() == 1
    && coverageIds.Length == 13
    && coverageIds.ToHashSet(StringComparer.Ordinal).SetEquals(allO4Ids)
    && coverageContract.GetProperty("entries").GetArrayLength() == 31
    && coverageTitles.Length == 13
    && coverageTitles.All(title => coverageRegister.Contains(title, StringComparison.Ordinal));

string[] templateIds = memoTemplate.GetProperty("rulings").EnumerateArray()
    .Select(x => x.GetProperty("rulingId").GetString()!).ToArray();
bool memoMaterialsValid = memoTemplate.GetProperty("schemaVersion").GetInt32() == 1
    && memoTemplate.GetProperty("templateOnly").GetBoolean()
    && templateIds.Length == 13
    && templateIds.ToHashSet(StringComparer.Ordinal).SetEquals(allO4Ids)
    && allO4Ids.All(id => memoSchemaText.Contains($"\"{id}\"", StringComparison.Ordinal));

string[] acceptedSignatureModes = intakeContract.GetProperty("acceptedSignatureModes").EnumerateArray()
    .Select(x => x.GetString()!).ToArray();
int reviewerRegistryRecordCount = reviewerRegistry.GetProperty("records").GetArrayLength();
bool signingMaterialsValid = signingInstructions.Contains("RFC8785-JCS", StringComparison.Ordinal)
    && signingInstructions.Contains("Ed25519", StringComparison.Ordinal)
    && intakeContract.GetProperty("contractId").GetString() == "phase480-o4-physicist-adjudication-intake-v1"
    && acceptedSignatureModes.SequenceEqual(["ed25519-detached"], StringComparer.Ordinal)
    && intakeContract.GetProperty("requiredReviewerRegistryState").GetString()
        == "pre-pinned-active-human-reviewer-with-ed25519-spki-pem"
    && !intakeContract.GetProperty("machineAuthoredOrInferredRulingAccepted").GetBoolean()
    && reviewerRegistry.GetProperty("registryId").GetString() == "phase480-o4-reviewer-registry-v1"
    && reviewerRegistryRecordCount == 0;

bool phase555Valid = phase555.GetProperty("schemaVersion").GetInt32() == 1
    && phase555.GetProperty("phase").GetInt32() == 555
    && phase555.GetProperty("phaseId").GetString() == "phase555-flat-sector-external-review-escalation-packet"
    && phase555.GetProperty("contractValid").GetBoolean()
    && phase555.GetProperty("exactBindingsValid").GetBoolean()
    && phase555.GetProperty("verdictKind").GetString() == "packet-assembled-awaiting-external-ruling"
    && phase555.GetProperty("questions").GetArrayLength() == 2
    && !phase555.GetProperty("authorsARuling").GetBoolean()
    && phase555.GetProperty("externalReviewPending").GetBoolean()
    && phase555.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement[] censusRows = phase485.GetProperty("rows").EnumerateArray().ToArray();
string[] censusIds = censusRows.Select(x => x.GetProperty("rulingId").GetString()!).ToArray();
bool phase485Valid = phase485.GetProperty("phaseId").GetString() == "phase485-o4-assumption-falsifier-census"
    && phase485.GetProperty("verdictKind").GetString() == "exact-thirteen-item-census-complete"
    && phase485.GetProperty("rulingIdCount").GetInt32() == 13
    && phase485.GetProperty("uniqueRulingIdCount").GetInt32() == 13
    && phase485.GetProperty("everyItemHasFalsifier").GetBoolean()
    && censusIds.ToHashSet(StringComparer.Ordinal).SetEquals(allO4Ids)
    && censusRows.Count(x => x.GetProperty("externalInterpretationStillRequired").GetBoolean()) == 2
    && censusRows.All(x => !x.GetProperty("mayAuthorRuling").GetBoolean())
    && phase485.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool phase578Valid = phase578.GetProperty("schemaVersion").GetInt32() == 1
    && phase578.GetProperty("phase").GetInt32() == 578
    && phase578.GetProperty("phaseId").GetString() == "phase578-consolidated-conditional-electroweak-sector-ledger"
    && phase578.GetProperty("contractId").GetString() == "phase578-a42-consolidated-conditional-electroweak-sector-ledger-v5"
    && phase578.GetProperty("contractValid").GetBoolean()
    && phase578.GetProperty("exactBindingsValid").GetBoolean()
    && phase578.GetProperty("verdictKind").GetString() == "conditional-ledger-complete-absolute-masses-blocked"
    && phase578.GetProperty("ledgerRowCount").GetInt32() == 14
    && phase578.GetProperty("comparisonAccounting").GetProperty("knownElectroweakMissCount").GetInt32() == 2
    && !phase578.GetProperty("absoluteMassComputed").GetBoolean()
    && phase578.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement phase579Invariance = phase579.GetProperty("exactTransformationInvariance");
bool phase579Valid = phase579.GetProperty("schemaVersion").GetInt32() == 1
    && phase579.GetProperty("phase").GetInt32() == 579
    && phase579.GetProperty("phaseId").GetString() == "phase579-collective-coordinate-jacobian-self-check"
    && phase579.GetProperty("contractId").GetString() == "phase579-a42-collective-coordinate-jacobian-self-check-v2"
    && phase579.GetProperty("contractValid").GetBoolean()
    && phase579.GetProperty("exactBindingsValid").GetBoolean()
    && phase579.GetProperty("verdictKind").GetString() == "phase450-lineage-convention-defective-preserved-negative"
    && !phase579.GetProperty("falsifierPassed").GetBoolean()
    && phase579.GetProperty("preservedFirstClassNegative").GetBoolean()
    && !phase579.GetProperty("evidenceBackedInternalProposalAllowed").GetBoolean()
    && !phase579Invariance.GetProperty("passed").GetBoolean()
    && phase579.GetProperty("jacobian").GetProperty("passed").GetBoolean()
    && phase579.GetProperty("exactlySolvableLimit").GetProperty("passed").GetBoolean()
    && !phase579.GetProperty("mayAuthorRuling").GetBoolean()
    && !phase579.GetProperty("humanRulingAuthored").GetBoolean()
    && !phase579.GetProperty("phase450RecordMutated").GetBoolean()
    && phase579.GetProperty("externalReviewPending").GetBoolean()
    && phase579.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool packetContentValid = knownAnswerPassed && assessmentValid && coverageValid && memoMaterialsValid
    && signingMaterialsValid && phase555Valid && phase485Valid && phase578Valid && phase579Valid;
string verdict = !contractValid ? taxonomy[0] : !packetContentValid ? taxonomy[1] : taxonomy[2];
bool packetReady = verdict == taxonomy[2];

const string ReviewerGuide = """
# O4 Independent Reviewer Guide

## Status

This packet was assembled by a machine and is non-authoritative. The internal
assessment is not an O4 memo, carries no signature, authors no ruling, and may
not change any `physicistReviewPending` flag. The independent signer may accept,
reject, defer, or otherwise overturn every proposed disposition.

## What the signer is asked to do

Make exactly two genuine judgment calls:

1. `O4-P455-SB-MODEL` — adjudicate the Phase455 workbench-model choice.
2. `O4-C3-WS3-MPROBE-SCOPE` — adjudicate the WS3 M-probe methodology scope.

Then adjudicate the eleven evidence-backed proposals listed in the sibling JSON
manifest. Ten are supporting or not-applicable proposals. One is an adverse
result: Phase579 falsified the Phase450-lineage fixed-coordinate active
gauge-invariance convention. Evidence-backed does not mean favorable, and the
negative must not be relabeled as support.

## Signing and intake prerequisites

The committed Phase480 reviewer registry is empty. Before Phase480 can accept a
memo, an independent human reviewer must be pre-pinned as active with an
Ed25519 SPKI public key. The completed memo must satisfy the strict JSON schema,
bind an ancestor repository commit and every reviewed artifact, use the
RFC8785/JCS restricted canonical payload and SHA-256 definition in the intake
contract, and carry a genuine Ed25519 detached signature under the required
input path. Shape validation, this guide, the inert template, or machine-authored
text is not authentication.

## Authority boundary

Packet readiness satisfies nothing. It does not authenticate or consume a
memo, discharge O4, change an intake or pending flag, satisfy Phase458, alter
Phase481, fill a source contract, authorize sampling/production/launch, or
permit a physical-unit or GeV claim. `promotedPhysicalMassClaimCount=0`.
""" + "\n";

byte[] guideBytes = Encoding.UTF8.GetBytes(ReviewerGuide);
string guideSha256 = Convert.ToHexString(SHA256.HashData(guideBytes)).ToLowerInvariant();

var packet = new
{
    schemaVersion = 1,
    phase = 580,
    phaseId = "phase580-external-review-packet-assembly",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    deterministic = true,
    zeroCompute = true,
    zeroSampling = true,
    rngUsed = false,
    knownAnswerBattery,
    packetDirectory = new
    {
        path = PacketDirectory,
        reviewerGuidePath = GuidePath,
        reviewerGuideSha256 = guideSha256,
        packetJsonPath = PacketPath,
        packetSummaryPath = SummaryPath,
        fullAndSummaryRequiredByteIdentical = true,
    },
    hashManifest = new
    {
        artifactCount = bindings.Length,
        allHashesMatch = exactBindingsValid,
        artifacts = bindings,
    },
    inputValidation = new
    {
        assessmentValid,
        assessmentMachineAuthored = true,
        assessmentNonAuthoritative = true,
        assessmentPhase579NegativePreserved = phase579Valid,
        coverageValid,
        coverageReviewItemCount = coverageIds.Length,
        coverageEntryCount = coverageContract.GetProperty("entries").GetArrayLength(),
        memoMaterialsValid,
        memoTemplateRulingCount = templateIds.Length,
        signingMaterialsValid,
        phase555Valid,
        phase485Valid,
        phase578Valid,
        phase579Valid,
    },
    reviewerRequest = new
    {
        judgmentCallCount = judgmentCallIds.Length,
        judgmentCalls = new object[]
        {
            new { rulingId = judgmentCallIds[0], request = "Adjudicate the Phase455 S_B workbench-model choice; the machine assessment proposes insufficient-basis/defer." },
            new { rulingId = judgmentCallIds[1], request = "Adjudicate the WS3 M-probe methodology scope; the stricter gate remains in force." },
        },
        evidenceBackedProposalCount = evidenceBackedProposalIds.Length,
        evidenceBackedProposalIds,
        supportingOrNotApplicableProposalCount = 10,
        preservedImplementationNegativeProposalCount = 1,
        preservedImplementationNegativeRulingId = "O4-F1-COLLECTIVE-COORDINATE",
        phase579NegativeIsEvidenceNotSupport = true,
        allThirteenItemsPresented = partitionCoversExactO4Set,
        signerMayOverturnAnyProposal = true,
        machineProposalIsNotRuling = true,
    },
    signingAndIntake = new
    {
        acceptedSignatureModes,
        reviewerRegistryRecordCount,
        reviewerRegistryEmpty = reviewerRegistryRecordCount == 0,
        independentReviewerMustBePrePinned = true,
        requiredReviewerRegistryState = intakeContract.GetProperty("requiredReviewerRegistryState").GetString(),
        canonicalization = intakeContract.GetProperty("canonicalization").GetString(),
        signedPayloadDefinition = intakeContract.GetProperty("signedPayloadDefinition").GetString(),
        repositoryBindingCommitPolicy = intakeContract.GetProperty("repositoryBindingCommitPolicy").GetString(),
        signingPrerequisitesCurrentlySatisfied = false,
        phase480IntakeReady = false,
        productionMemoPresent = false,
        signatureVerified = false,
        shapeValidationIsAuthentication = false,
    },
    upstreamEvidence = new
    {
        phase555Verdict = phase555.GetProperty("verdictKind").GetString(),
        phase485Verdict = phase485.GetProperty("verdictKind").GetString(),
        phase578Verdict = phase578.GetProperty("verdictKind").GetString(),
        phase579Verdict = phase579.GetProperty("verdictKind").GetString(),
        phase579FalsifierPassed = phase579.GetProperty("falsifierPassed").GetBoolean(),
        phase579PreservedFirstClassNegative = phase579.GetProperty("preservedFirstClassNegative").GetBoolean(),
    },
    packetContentValid,
    packetReadyForIndependentReview = packetReady,
    readinessOnly = true,
    verdictKind = verdict,
    terminalStatus = "external-review-packet-assembly-" + verdict,
    decision = packetReady
        ? "The exact-bound O4 review packet is assembled for an independent reviewer. It presents exactly two genuine judgment calls and eleven evidence-backed proposals, including Phase579's preserved implementation negative, and discloses the empty reviewer registry and every signing prerequisite. This is packet readiness only; it is not intake readiness, authentication, a ruling, or authority."
        : "At least one frozen binding, packet-content invariant, or reviewer-request partition is invalid. The packet is incomplete and no readiness follows.",
    internalAssessmentMachineAuthored = true,
    internalAssessmentNonAuthoritative = true,
    independentSignerMayOverturnAnyProposal = true,
    authorsO4Ruling = false,
    consumesO4Memo = false,
    verifiesSignature = false,
    changesIntake = false,
    changesPendingFlag = false,
    o4Discharged = false,
    phase458Satisfied = false,
    phase481Changed = false,
    sourceContractApplicationAllowed = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    launchAuthorized = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    externalReviewPending = true,
    pendingArtifactCountChanged = false,
    promotedPhysicalMassClaimCount = 0,
};

Directory.CreateDirectory(PacketDirectory);
File.WriteAllBytes(GuidePath, guideBytes);
byte[] packetBytes = JsonSerializer.SerializeToUtf8Bytes(packet,
    new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
File.WriteAllBytes(PacketPath, packetBytes);
File.WriteAllBytes(SummaryPath, packetBytes);

Console.WriteLine($"Phase580 verdict: {verdict}");
Console.WriteLine($"bindings={bindings.Length}; judgmentCalls={judgmentCallIds.Length}; evidenceBackedProposals={evidenceBackedProposalIds.Length}; reviewerRegistryRecords={reviewerRegistryRecordCount}; promotedPhysicalMassClaimCount=0");

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed record Binding(string Id, string Path, string Sha256);
