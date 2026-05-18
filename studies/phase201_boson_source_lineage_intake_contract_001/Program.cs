using System.Text.Json;

const string DefaultOutputDir = "studies/phase201_boson_source_lineage_intake_contract_001/output";
const string Phase200Path = "studies/phase200_boson_prediction_root_cause_closure_001/output/boson_prediction_root_cause_closure_summary.json";
const string WzTemplateName = "wz_absolute_source_lineage_intake_template.json";
const string HiggsTemplateName = "higgs_scalar_source_lineage_intake_template.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE201_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase200 = JsonDocument.Parse(File.ReadAllText(Phase200Path));
var wzTemplatePath = Path.Combine(outputDir, WzTemplateName);
var higgsTemplatePath = Path.Combine(outputDir, HiggsTemplateName);

var wzTemplate = new
{
    artifactId = "phase201-wz-absolute-source-lineage-intake-template",
    schemaVersion = "1.0.0",
    artifactKind = "wz-absolute-source-lineage",
    status = "template-unfilled",
    externalTargetValuesUsed = (bool?)null,
    theoremOrDerivationId = (string?)null,
    sourceLineageId = (string?)null,
    particleRows = new[]
    {
        new WzParticleRow(
            "w-boson",
            "physical-w-boson-mass-gev",
            SourceRowId: null,
            RawAmplitudeGatePassed: null,
            CommonBridgeGatePassed: null,
            TargetComparisonGatePassed: null,
            StabilitySidecarsPresent: null,
            DerivationId: null),
        new WzParticleRow(
            "z-boson",
            "physical-z-boson-mass-gev",
            SourceRowId: null,
            RawAmplitudeGatePassed: null,
            CommonBridgeGatePassed: null,
            TargetComparisonGatePassed: null,
            StabilitySidecarsPresent: null,
            DerivationId: null),
    },
    requiredEvidence = new[]
    {
        "target-independent bridge/source theorem or derivation",
        "separate W and Z source rows",
        "raw-amplitude gate passed for both rows",
        "common W/Z bridge-scale gate passed",
        "physical target-comparison gate passed after construction, not during source selection",
        "branch/refinement/environment/representation/coupling stability sidecars",
    },
};

var higgsTemplate = new
{
    artifactId = "phase201-higgs-scalar-source-lineage-intake-template",
    schemaVersion = "1.0.0",
    artifactKind = "higgs-scalar-source-lineage",
    status = "template-unfilled",
    externalTargetValuesUsed = (bool?)null,
    sourceLineageId = (string?)null,
    scalarSourceOperatorId = (string?)null,
    higgsIdentityEnvelopeId = (string?)null,
    massiveScalarProfileId = (string?)null,
    potentialOrSelfCouplingSourceId = (string?)null,
    excitationRelationId = (string?)null,
    stabilitySidecars = new
    {
        branch = (bool?)null,
        refinement = (bool?)null,
        environment = (bool?)null,
        representation = (bool?)null,
        coupling = (bool?)null,
    },
    predictionRow = new
    {
        particleId = "higgs",
        observableId = "physical-higgs-mass-gev",
        sourceRowId = (string?)null,
        targetComparisonGatePassed = (bool?)null,
        derivationId = (string?)null,
    },
    requiredEvidence = new[]
    {
        "solved target-independent scalar-sector source/operator",
        "Higgs identity envelopes independent of physical Higgs mass",
        "massive scalar profile",
        "potential/self-coupling or scalar excitation relation",
        "branch/refinement/environment/representation/coupling stability sidecars",
        "physical target-comparison gate passed after construction, not during source selection",
    },
};

if (!File.Exists(wzTemplatePath))
    File.WriteAllText(wzTemplatePath, JsonSerializer.Serialize(wzTemplate, options));
if (!File.Exists(higgsTemplatePath))
    File.WriteAllText(higgsTemplatePath, JsonSerializer.Serialize(higgsTemplate, options));

using var wzArtifact = JsonDocument.Parse(File.ReadAllText(wzTemplatePath));
using var higgsArtifact = JsonDocument.Parse(File.ReadAllText(higgsTemplatePath));

var wzValidation = ValidateWz(wzArtifact.RootElement);
var higgsValidation = ValidateHiggs(higgsArtifact.RootElement);
var anySourceLineagePromotable = wzValidation.Promotable || higgsValidation.Promotable;
var allRequiredLineagesPromotable = wzValidation.Promotable && higgsValidation.Promotable;
var terminalStatus = allRequiredLineagesPromotable
    ? "boson-source-lineage-intake-contract-complete"
    : "boson-source-lineage-intake-contract-awaiting-artifacts";

var result = new
{
    phaseId = "phase201-boson-source-lineage-intake-contract",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    intakeContractMaterialized = true,
    anySourceLineagePromotable,
    allRequiredLineagesPromotable,
    wzTemplatePath,
    higgsTemplatePath,
    wzValidation,
    higgsValidation,
    blockers = wzValidation.Blockers.Concat(higgsValidation.Blockers).ToArray(),
    rootCauseClosure = phase200.RootElement.Clone(),
    closureRequirements = new[]
    {
        "fill the W/Z intake template with a derivation-backed target-independent source lineage and separate W/Z rows that pass all gates",
        "fill the Higgs intake template with a solved scalar-sector source/operator lineage and all identity/profile/stability gates",
        "externalTargetValuesUsed must be false for source construction in both artifacts",
        "after valid intake, rerun the boson generator so P192/P193 can decide whether all known boson values become defensible",
    },
    sourceEvidence = new
    {
        phase200Path = Phase200Path,
    },
};

File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_intake_contract.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_intake_contract_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.intakeContractMaterialized,
        result.anySourceLineagePromotable,
        result.allRequiredLineagesPromotable,
        result.wzTemplatePath,
        result.higgsTemplatePath,
        result.wzValidation,
        result.higgsValidation,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"wzPromotable={wzValidation.Promotable}");
Console.WriteLine($"higgsPromotable={higgsValidation.Promotable}");
Console.WriteLine($"allRequiredLineagesPromotable={allRequiredLineagesPromotable}");

static IntakeValidation ValidateWz(JsonElement artifact)
{
    var blockers = new List<string>();
    var externalTargets = JsonBool(artifact, "externalTargetValuesUsed");
    var theoremOrDerivationId = JsonString(artifact, "theoremOrDerivationId");
    var sourceLineageId = JsonString(artifact, "sourceLineageId");
    var rows = artifact.TryGetProperty("particleRows", out var rowElement) && rowElement.ValueKind == JsonValueKind.Array
        ? rowElement.EnumerateArray().ToArray()
        : [];

    if (externalTargets is not false)
        blockers.Add("W/Z source construction must declare externalTargetValuesUsed=false");
    if (string.IsNullOrWhiteSpace(theoremOrDerivationId))
        blockers.Add("W/Z theoremOrDerivationId is required");
    if (string.IsNullOrWhiteSpace(sourceLineageId))
        blockers.Add("W/Z sourceLineageId is required");
    if (rows.Length != 2)
        blockers.Add("W/Z artifact must provide exactly separate W and Z particle rows");

    var rowAudits = rows.Select(row =>
    {
        var particleId = JsonString(row, "particleId");
        var complete = !string.IsNullOrWhiteSpace(JsonString(row, "sourceRowId"))
            && JsonBool(row, "rawAmplitudeGatePassed") is true
            && JsonBool(row, "commonBridgeGatePassed") is true
            && JsonBool(row, "targetComparisonGatePassed") is true
            && JsonBool(row, "stabilitySidecarsPresent") is true
            && !string.IsNullOrWhiteSpace(JsonString(row, "derivationId"));
        if (!complete)
            blockers.Add($"W/Z row incomplete for {particleId ?? "unknown-particle"}");
        return new { particleId, complete };
    }).ToArray();

    var hasW = rows.Any(row => JsonString(row, "particleId") == "w-boson");
    var hasZ = rows.Any(row => JsonString(row, "particleId") == "z-boson");
    if (!hasW || !hasZ)
        blockers.Add("W/Z artifact must include w-boson and z-boson rows");

    return new IntakeValidation(blockers.Count == 0, rowAudits.Length, rowAudits.Count(row => row.complete), blockers.Distinct().ToArray());
}

static IntakeValidation ValidateHiggs(JsonElement artifact)
{
    var blockers = new List<string>();
    if (JsonBool(artifact, "externalTargetValuesUsed") is not false)
        blockers.Add("Higgs source construction must declare externalTargetValuesUsed=false");
    if (string.IsNullOrWhiteSpace(JsonString(artifact, "sourceLineageId")))
        blockers.Add("Higgs sourceLineageId is required");
    if (string.IsNullOrWhiteSpace(JsonString(artifact, "scalarSourceOperatorId")))
        blockers.Add("scalarSourceOperatorId is required");
    if (string.IsNullOrWhiteSpace(JsonString(artifact, "higgsIdentityEnvelopeId")))
        blockers.Add("higgsIdentityEnvelopeId is required");
    if (string.IsNullOrWhiteSpace(JsonString(artifact, "massiveScalarProfileId")))
        blockers.Add("massiveScalarProfileId is required");
    if (string.IsNullOrWhiteSpace(JsonString(artifact, "potentialOrSelfCouplingSourceId"))
        && string.IsNullOrWhiteSpace(JsonString(artifact, "excitationRelationId")))
        blockers.Add("potentialOrSelfCouplingSourceId or excitationRelationId is required");

    if (!artifact.TryGetProperty("stabilitySidecars", out var sidecars) || sidecars.ValueKind != JsonValueKind.Object)
        blockers.Add("stabilitySidecars object is required");
    else
    {
        foreach (var key in new[] { "branch", "refinement", "environment", "representation", "coupling" })
        {
            if (JsonBool(sidecars, key) is not true)
                blockers.Add($"Higgs stability sidecar '{key}' must be true");
        }
    }

    if (!artifact.TryGetProperty("predictionRow", out var predictionRow) || predictionRow.ValueKind != JsonValueKind.Object)
    {
        blockers.Add("Higgs predictionRow is required");
    }
    else
    {
        if (JsonString(predictionRow, "particleId") != "higgs")
            blockers.Add("Higgs predictionRow must use particleId=higgs");
        if (JsonString(predictionRow, "observableId") != "physical-higgs-mass-gev")
            blockers.Add("Higgs predictionRow must use observableId=physical-higgs-mass-gev");
        if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "sourceRowId")))
            blockers.Add("Higgs predictionRow sourceRowId is required");
        if (JsonBool(predictionRow, "targetComparisonGatePassed") is not true)
            blockers.Add("Higgs targetComparisonGatePassed must be true");
        if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "derivationId")))
            blockers.Add("Higgs predictionRow derivationId is required");
    }

    return new IntakeValidation(blockers.Count == 0, 1, blockers.Count == 0 ? 1 : 0, blockers.Distinct().ToArray());
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record WzParticleRow(
    string ParticleId,
    string ObservableId,
    string? SourceRowId,
    bool? RawAmplitudeGatePassed,
    bool? CommonBridgeGatePassed,
    bool? TargetComparisonGatePassed,
    bool? StabilitySidecarsPresent,
    string? DerivationId);

sealed record IntakeValidation(bool Promotable, int RowCount, int CompleteRowCount, IReadOnlyList<string> Blockers);
