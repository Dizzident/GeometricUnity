using System.Text.Json;

const string DefaultOutputDir = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE227_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase225 = JsonDocument.Parse(File.ReadAllText(Phase225Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));

var officialDraftActionPresent = true;
var officialSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftAlreadyCompletion = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var phase190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var wzValidation)
    && JsonBool(wzValidation, "promotable") is true;
var higgsPromotable = phase201.RootElement.TryGetProperty("higgsValidation", out var higgsValidation)
    && JsonBool(higgsValidation, "promotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var wParameterClosure = phase224.RootElement.TryGetProperty("closure", out var phase224Closure)
    && JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zParameterClosure = phase224.RootElement.TryGetProperty("closure", out phase224Closure)
    && JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsParameterClosure = phase224.RootElement.TryGetProperty("closure", out phase224Closure)
    && JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var su2RepresentationObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
var higgsNotationObstructionCertified = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true;

var fixedShiabOperatorIdPresent = false;
var fixedKappaOrInnerProductNormalizationPresent = false;
var componentExtractionTheoremPresent = false;
var observerPullbackSectorProjectionPresent = false;
var particleSpecificMassRowsPresent = false;

var officialGuShiabUpsilonExtractionPromotable =
    officialDraftActionPresent
    && fixedShiabOperatorIdPresent
    && fixedKappaOrInnerProductNormalizationPresent
    && componentExtractionTheoremPresent
    && observerPullbackSectorProjectionPresent
    && particleSpecificMassRowsPresent
    && allRequiredLineagesPromotable
    && wParameterClosure
    && zParameterClosure
    && higgsParameterClosure
    && !officialDraftAlreadyCompletion;

var unresolvedExtractionBlockers = new[]
{
    new ExtractionBlocker("fixed-shiab-operator-id", fixedShiabOperatorIdPresent, "The official draft discusses candidate Shiab constructions and an example operator, but does not provide a repository-fixed operator identity suitable for particle mass extraction."),
    new ExtractionBlocker("fixed-kappa1-and-inner-product-normalization", fixedKappaOrInnerProductNormalizationPresent, "The action includes kappa1 and norm/inner-product choices, but the current repo has no target-independent normalization value that closes electroweak W/Z/H masses."),
    new ExtractionBlocker("upsilon-component-extraction-theorem", componentExtractionTheoremPresent, "No local theorem decomposes Upsilon into Standard Model W, Z, and Higgs source rows with coefficients."),
    new ExtractionBlocker("observer-pullback-sector-projection", observerPullbackSectorProjectionPresent, "The draft's observer pullback/sector-location map has not been turned into a checked projection from Y-field components to the specific physical W/Z/H rows."),
    new ExtractionBlocker("particle-specific-mass-prediction-rows", particleSpecificMassRowsPresent, "No source-lineage rows fill the raw-amplitude, common-bridge, target-comparison, and stability gates for W, Z, and Higgs absolute masses."),
};

var checks = new[]
{
    new Check("official-gu-shiab-upsilon-action-recorded", officialDraftActionPresent, "Draft section 8-9 presents Shiab/Upsilon/augmented-torsion action structure and a second-order Upsilon norm."),
    new Check("official-public-source-audit-still-noncompletion", officialSourceAuditMaterialized && !officialDraftAlreadyCompletion, $"officialPublicSourceAuditMaterialized={officialSourceAuditMaterialized}; officialDraftProvidesCompletionSource={officialDraftAlreadyCompletion}"),
    new Check("phase190-direct-law-still-not-theorem", !phase190TheoremClaimed, $"phase190TheoremClaimed={phase190TheoremClaimed}"),
    new Check("shiab-operator-not-fixed-for-extraction", !fixedShiabOperatorIdPresent, $"fixedShiabOperatorIdPresent={fixedShiabOperatorIdPresent}"),
    new Check("kappa1-normalization-not-fixed", !fixedKappaOrInnerProductNormalizationPresent, $"fixedKappaOrInnerProductNormalizationPresent={fixedKappaOrInnerProductNormalizationPresent}"),
    new Check("upsilon-component-extraction-missing", !componentExtractionTheoremPresent, $"componentExtractionTheoremPresent={componentExtractionTheoremPresent}"),
    new Check("observer-sector-projection-missing", !observerPullbackSectorProjectionPresent, $"observerPullbackSectorProjectionPresent={observerPullbackSectorProjectionPresent}"),
    new Check("phase201-lineages-not-promotable", !allRequiredLineagesPromotable && !wzPromotable && !higgsPromotable, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzPromotable={wzPromotable}; higgsPromotable={higgsPromotable}"),
    new Check("phase213-blockers-remain", wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("electroweak-parameter-closure-still-blocked", !wParameterClosure && !zParameterClosure && !higgsParameterClosure, $"wClosure={wParameterClosure}; zClosure={zParameterClosure}; higgsClosure={higgsParameterClosure}"),
    new Check("p225-p226-obstructions-still-certified", su2RepresentationObstructionCertified && higgsNotationObstructionCertified, $"su2RepresentationObstructionCertified={su2RepresentationObstructionCertified}; higgsNotationObstructionCertified={higgsNotationObstructionCertified}"),
};

var officialGuShiabUpsilonExtractionObstructionCertified = checks.All(check => check.Passed)
    && unresolvedExtractionBlockers.All(blocker => !blocker.Filled)
    && !officialGuShiabUpsilonExtractionPromotable;
var terminalStatus = officialGuShiabUpsilonExtractionObstructionCertified
    ? "official-gu-shiab-upsilon-extraction-blocked-free-operator-coefficient-and-projection"
    : "official-gu-shiab-upsilon-extraction-review-required";

var result = new
{
    phaseId = "phase227-official-gu-shiab-upsilon-extraction-obstruction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    officialGuShiabUpsilonExtractionPromotable,
    officialGuShiabUpsilonExtractionObstructionCertified,
    objective = "Audit whether the official GU Shiab/Upsilon/augmented-torsion action can be extracted into target-independent W/Z/H mass predictions.",
    officialGuSourceContext = new
    {
        officialDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        officialLectureTranscriptUrl = "https://geometricunity.org/2013-oxford-lecture/",
        sourcePointers = new[]
        {
            "Draft section 8 presents Shiab contraction operators and notes incomplete operator taxonomy/source reconstruction.",
            "Draft section 9 presents a first-order bosonic action using augmented torsion, a Shiab contraction, and a kappa1 torsion term.",
            "Draft section 9 varies the action into Upsilon equations and presents a second-order norm of Upsilon for Yang-Mills/Higgs-like equations.",
            "The appendix maps Higgs potential to an inner product of Upsilon terms.",
            "The Oxford transcript says the quartic Higgs piece comes from Dirac squaring of a quadratic in augmented torsion.",
        },
        extractionInterpretation = "These sources identify a plausible architecture and research program. They do not by themselves fix the operator, coefficient normalization, component projection, or particle rows required by the local prediction gates.",
    },
    unresolvedExtractionBlockers,
    currentRepoEvidence = new
    {
        phase190 = new
        {
            status = JsonString(phase190.RootElement, "terminalStatus"),
            theoremClaimed = phase190TheoremClaimed,
            candidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed"),
        },
        phase201 = new
        {
            status = JsonString(phase201.RootElement, "terminalStatus"),
            allRequiredLineagesPromotable,
            wzPromotable,
            higgsPromotable,
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
            higgsMissingFieldCount,
            wzMissingFields = JsonStringArray(phase213.RootElement, "wzMissingFields"),
            higgsMissingFields = JsonStringArray(phase213.RootElement, "higgsMissingFields"),
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wParameterClosure,
            zParameterClosure,
            higgsParameterClosure,
        },
        phase225 = new
        {
            status = JsonString(phase225.RootElement, "terminalStatus"),
            representationNormalizationObstructionCertified = su2RepresentationObstructionCertified,
        },
        phase226 = new
        {
            status = JsonString(phase226.RootElement, "terminalStatus"),
            officialGuHiggsPotentialNotationObstructionCertified = higgsNotationObstructionCertified,
        },
    },
    checks,
    decision = officialGuShiabUpsilonExtractionObstructionCertified
        ? "Do not promote W/Z/H absolute masses from the official GU Shiab/Upsilon action alone. The action is suggestive, but the operator identity, kappa1/inner-product normalization, observer-sector projection, and particle-specific source rows remain unfilled."
        : "Review the official GU Shiab/Upsilon extraction audit before relying on this obstruction.",
    nextRequiredArtifact = new[]
    {
        "A fixed Shiab operator identity, including the invariant forms and representation-theoretic selection rule used for the physical sector.",
        "A target-independent kappa1/inner-product normalization derivation that closes electroweak scale, weak coupling, and Higgs self-coupling parameters.",
        "A checked observer-pullback and sector-projection theorem mapping Upsilon components to W, Z, and Higgs source rows.",
        "Filled Phase201 source-lineage applications that pass Phase209/Phase210/Phase213 and the top-level Phase101/Phase202 gates.",
    },
    sourceEvidence = new
    {
        phase190Path = Phase190Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase218Path = Phase218Path,
        phase224Path = Phase224Path,
        phase225Path = Phase225Path,
        phase226Path = Phase226Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "official_gu_shiab_upsilon_extraction_obstruction_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.officialGuShiabUpsilonExtractionPromotable,
        result.officialGuShiabUpsilonExtractionObstructionCertified,
        result.officialGuSourceContext,
        result.unresolvedExtractionBlockers,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"officialGuShiabUpsilonExtractionPromotable={officialGuShiabUpsilonExtractionPromotable}");
Console.WriteLine($"officialGuShiabUpsilonExtractionObstructionCertified={officialGuShiabUpsilonExtractionObstructionCertified}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExtractionBlocker(string BlockerId, bool Filled, string Detail);
