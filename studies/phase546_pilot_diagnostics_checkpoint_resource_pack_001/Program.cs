using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase546_pilot_diagnostics_checkpoint_resource_pack_001";
const string ContractPath = Root + "/preregistration/phase546_pilot_diagnostics_checkpoint_resource_repair_contract_v3.json";
const string OutputPath = Root + "/output/pilot_diagnostics_checkpoint_resource_pack.json";
const string SummaryPath = Root + "/output/pilot_diagnostics_checkpoint_resource_pack_summary.json";

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
    return new
    {
        x.Id,
        x.Path,
        x.ExpectedSha256,
        ActualSha256 = actual,
        HashMatches = actual == x.ExpectedSha256,
    };
}).ToArray();
string[] bindingIds =
[
    "phase546-v1-contract", "phase546-v1-program", "phase546-v1-checkpoint-codec",
    "phase546-v1-output", "phase546-v1-summary", "phase546-v2-contract",
    "phase546-v2-program", "phase546-v2-output", "phase546-v2-summary",
];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(bindingIds)
    && bindings.All(x => x.HashMatches);

using var v1OutputDocument = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[3].Path));
using var v1SummaryDocument = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[4].Path));
using var v2ContractDocument = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[5].Path));
using var v2OutputDocument = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[7].Path));
JsonElement v1Output = v1OutputDocument.RootElement;
JsonElement v1Summary = v1SummaryDocument.RootElement;
JsonElement v2Contract = v2ContractDocument.RootElement;
JsonElement v2Output = v2OutputDocument.RootElement;
JsonElement lineage = contract.GetProperty("lineageAdjudication");
JsonElement resource = contract.GetProperty("resourceRefusal");
JsonElement accounting = contract.GetProperty("fixtureAccounting");
string[] taxonomy =
[
    "invalid-or-drifted-lineage",
    "lineage-adjudication-invalid",
    "dof-mismatch-fixture-failed",
    "pilot-support-pack-materialized-with-authentic-lineage-dof-repair",
];

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 3
    && contract.GetProperty("contractId").GetString()
        == "phase546-a28-pilot-diagnostics-checkpoint-resource-pack-repair-v3"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A28"
    && contract.GetProperty("frozenBeforeCorrectedExecution").GetBoolean()
    && contract.GetProperty("syntheticFixturesOnly").GetBoolean()
    && contract.GetProperty("repairScope").GetString()
        == "authentic-v1-provenance-plus-dedicated-dof-mismatch-evidence"
    && exactBindingsValid
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject()
        .All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool authenticV1Valid =
    v1Output.GetProperty("verdictKind").GetString() == "pilot-support-pack-materialized"
    && v1Summary.GetProperty("verdictKind").GetString() == "pilot-support-pack-materialized"
    && v1Output.GetProperty("passedFixtureCount").GetInt32() == 13
    && v1Output.GetProperty("allFixturesPassed").GetBoolean()
    && Sha(bindingSpecs[3].Path) == lineage.GetProperty("v1AuthenticOutputSha256").GetString();
bool preservedV2Valid =
    v2Contract.GetProperty("contractId").GetString()
        == "phase546-a28-pilot-diagnostics-checkpoint-resource-pack-repair-v2"
    && v2Output.GetProperty("verdictKind").GetString()
        == "pilot-support-pack-materialized-with-dof-repair"
    && v2Output.GetProperty("correctedPassedFixtureCount").GetInt32() == 14
    && v2Output.GetProperty("repairEvidence").GetProperty("dofMismatchFixtureTested").GetBoolean();
bool lineageAdjudicationValid =
    lineage.GetProperty("v1AuthenticOutputSha256").GetString()
        == "c9a031da68b8d336a158358a058a23c37dec6d1f2b2eaecd8f58c516721cba04"
    && lineage.GetProperty("v1PositiveResultIncomplete").GetBoolean()
    && !lineage.GetProperty("v1CitableForPhase547ResourceGate").GetBoolean()
    && lineage.GetProperty("v1SoleEvidenceDefect").GetString()
        == "dedicated-dof-mismatch-fixture-evidence-absent"
    && lineage.GetProperty("v2PositiveResultNonCitable").GetBoolean()
    && lineage.GetProperty("v2ProvenanceDefect").GetString()
        == "v2-contract-bound-newline-altered-v1-output-instead-of-authentic-v1-bytes"
    && lineage.GetProperty("newlineAlteredV1Sha256BoundByV2").GetString()
        == "e9d7e2ee2b6dfbce7bab3e980be0c5889534c1694bbe9d270546597e0edd9cf6"
    && !lineage.GetProperty("v2ScientificFixtureResultReinterpretedOrInvalidated").GetBoolean();

var dofRequest = new ResourceShape(
    resource.GetProperty("requiredTopologyId").GetString()!,
    resource.GetProperty("requiredDimensions").GetInt32(),
    resource.GetProperty("fixtureExtent").GetInt32(),
    resource.GetProperty("mismatchedDegreesOfFreedom").GetInt32());
ResourceRefusal dofAssessment = AssessShape(dofRequest);
bool dofMismatchFixtureTested = true;
bool dofMismatchFixturePassed =
    resource.GetProperty("expectedDegreesOfFreedom").GetInt32() == 3645
    && dofRequest.DegreesOfFreedom == 3644
    && dofAssessment.ExpectedDegreesOfFreedom == 3645
    && dofAssessment.ObservedDegreesOfFreedom == 3644
    && !dofAssessment.Allowed
    && dofAssessment.RefusalReason == "invalid-shape"
    && dofAssessment.RefusalReason == resource.GetProperty("expectedRefusalReason").GetString();
int inheritedFixtureCount =
    accounting.GetProperty("exactBoundInheritedAuthenticV1FixtureCount").GetInt32();
int newFixtureCount = accounting.GetProperty("newDedicatedFixtureIds").GetArrayLength();
int correctedFixtureCount = checked(inheritedFixtureCount + newFixtureCount);
bool fixtureAccountingValid = inheritedFixtureCount == 13 && newFixtureCount == 1
    && accounting.GetProperty("newDedicatedFixtureIds").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(["resource-dof-mismatch-refused"])
    && accounting.GetProperty("correctedTotalFixtureCount").GetInt32() == 14
    && correctedFixtureCount == 14;

string verdict = !contractValid || !authenticV1Valid || !preservedV2Valid ? taxonomy[0]
    : !lineageAdjudicationValid || !fixtureAccountingValid ? taxonomy[1]
    : !dofMismatchFixtureTested || !dofMismatchFixturePassed ? taxonomy[2]
    : taxonomy[3];

var output = new
{
    schemaVersion = 3,
    phase = 546,
    phaseId = "phase546-pilot-diagnostics-checkpoint-resource-pack-repair",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    bindings,
    lineage = new
    {
        authenticV1Valid,
        authenticV1OutputSha256 = Sha(bindingSpecs[3].Path),
        v1PositiveResultIncomplete = true,
        v1CitableForPhase547ResourceGate = false,
        v1SoleEvidenceDefect = "dedicated-dof-mismatch-fixture-evidence-absent",
        preservedV2Valid,
        v2PositiveResultNonCitable = true,
        v2ProvenanceDefect =
            "v2-contract-bound-newline-altered-v1-output-instead-of-authentic-v1-bytes",
        v2ScientificFixtureResultReinterpretedOrInvalidated = false,
    },
    repairEvidence = new
    {
        fixtureId = "resource-dof-mismatch-refused",
        dofMismatchFixtureTested,
        dofMismatchFixturePassed,
        request = dofRequest,
        assessment = dofAssessment,
        expectedRefusalReason = "invalid-shape",
    },
    inheritedAuthenticV1FixtureCount = inheritedFixtureCount,
    newDedicatedFixtureCount = newFixtureCount,
    correctedPassedFixtureCount = verdict == taxonomy[3] ? correctedFixtureCount : inheritedFixtureCount,
    correctedTotalFixtureCount = correctedFixtureCount,
    allCorrectedFixturesPassed = verdict == taxonomy[3],
    verdictKind = verdict,
    terminalStatus = "pilot-diagnostics-checkpoint-resource-pack-" + verdict,
    decision = verdict == taxonomy[3]
        ? "Authentic v1 bytes and the non-citable v2 provenance record are exact-bound. Dedicated synthetic DOF-mismatch refusal evidence repairs the sole v1 resource-gate evidence gap for dependent Phase547 adjudication."
        : "The earliest frozen v3 lineage or fixture failure is preserved; no Phase547 evidence follows.",
    rngUsed = false,
    registeredOperatorProposalPerformed = false,
    markovChainAdvanced = false,
    warmupPerformed = false,
    adaptationPerformed = false,
    samplingPerformed = false,
    benchmarkPerformed = false,
    configurationsRetained = false,
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
byte[] outputBytes = JsonSerializer.SerializeToUtf8Bytes(output, options);
File.WriteAllBytes(OutputPath, outputBytes);
File.WriteAllBytes(SummaryPath, outputBytes);
Console.WriteLine($"Phase546 v3 verdict: {verdict}");
Console.WriteLine($"correctedFixtures={output.correctedPassedFixtureCount}/{correctedFixtureCount}, dofMismatchTested={dofMismatchFixtureTested}");
Console.WriteLine("rng=False, proposal=False, chain=False, sampling=False");

static ResourceRefusal AssessShape(ResourceShape request)
{
    try
    {
        int expected = checked(45 * request.Extent * request.Extent * request.Extent * request.Extent);
        string? refusal = request.TopologyId != "periodic-hypercubic-4d" ? "topology-mismatch"
            : request.Dimensions != 4 ? "dimension-mismatch"
            : request.Extent <= 0 || request.DegreesOfFreedom != expected ? "invalid-shape"
            : null;
        return new ResourceRefusal(refusal is null, refusal, expected, request.DegreesOfFreedom);
    }
    catch (OverflowException)
    {
        return new ResourceRefusal(false, "checked-arithmetic-overflow", -1, request.DegreesOfFreedom);
    }
}

static string Sha(string path) =>
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed record ResourceShape(string TopologyId, int Dimensions, int Extent, int DegreesOfFreedom);
sealed record ResourceRefusal(
    bool Allowed,
    string? RefusalReason,
    int ExpectedDegreesOfFreedom,
    int ObservedDegreesOfFreedom);
