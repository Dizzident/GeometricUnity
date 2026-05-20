using System.Text.Json;

const string DefaultOutputDir = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase293Path = "studies/phase293_fermi_vev_source_audit_001/output/fermi_vev_source_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase314Path = "studies/phase314_dimension_casimir_wz_source_law_audit_001/output/dimension_casimir_wz_source_law_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE317_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase293 = JsonDocument.Parse(File.ReadAllText(Phase293Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase314 = JsonDocument.Parse(File.ReadAllText(Phase314Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var electroweakSchemeRadiativeSourceAuditPassed = JsonBool(phase261.RootElement, "electroweakSchemeRadiativeSourceAuditPassed") is true;
var schemeChoicePromotesBosonMasses = JsonBool(phase261.RootElement, "schemeChoicePromotesBosonMasses") is true;
var schemeChoiceProvidesGuSourceLineage = JsonBool(phase261.RootElement, "schemeChoiceProvidesGuSourceLineage") is true;
var fermiVevSourceAuditPassed = JsonBool(phase293.RootElement, "fermiVevSourceAuditPassed") is true;
var fermiVevSourcePromotesBosonPredictions = JsonBool(phase293.RootElement, "fermiVevSourcePromotesBosonPredictions") is true;
var guVevSourceFound = JsonBool(phase293.RootElement, "guVevSourceFound") is true;
var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;
var officialDraftProjectionMapPromotesWzMasses = JsonBool(phase313.RootElement, "officialDraftProjectionMapPromotesWzMasses") is true;
var officialDraftCanFillPhase201WzContract = JsonBool(phase313.RootElement, "canFillPhase201WzContract") is true;
var dimensionCasimirWzSourceLawAuditPassed = JsonBool(phase314.RootElement, "dimensionCasimirWzSourceLawAuditPassed") is true;
var casimirRatioSourceBackedForBosonApplication = JsonBool(phase314.RootElement, "casimirRatioSourceBackedForBosonApplication") is true;
var wOnlyCasimirMultiplierJustified = JsonBool(phase314.RootElement, "wOnlyCasimirMultiplierJustified") is true;
var zUnitMultiplierJustified = JsonBool(phase314.RootElement, "zUnitMultiplierJustified") is true;
var dimensionCasimirCanFillPhase201WzContract = JsonBool(phase314.RootElement, "canFillPhase201WzContract") is true;

const bool pdg2025ElectroweakMassMatrixSourceAvailable = true;
const bool smGaugeGroupRequiresSu2AndU1 = true;
const bool smMassGenerationRequiresHiggsDoublet = true;
const bool smMassGenerationRequiresVev = true;
const bool smMassGenerationRequiresWeakCouplingG = true;
const bool smMassGenerationRequiresHyperchargeCouplingGPrime = true;
const bool smDefinesPhotonZWeinbergRotation = true;
const bool smDefinesChargedWCombination = true;
const bool smTreeLevelMwDependsOnGAndV = true;
const bool smTreeLevelMzDependsOnGAndGPrimeAndV = true;
const bool smTreeLevelPhotonMassZero = true;
const bool smTreeLevelHiggsMassDependsOnPotentialParameter = true;
const bool smMassMatrixProvidesExternalDependencyMap = true;
const bool smMassMatrixProvidesGuLocalWzTheorem = false;
const bool smMassMatrixProvidesGuObservedFieldExtraction = false;
const bool smMassMatrixProvidesGuVevSource = false;
const bool smMassMatrixProvidesGuWeakCouplingSource = false;
const bool smMassMatrixProvidesGuHyperchargeCouplingSource = false;
const bool smMassMatrixProvidesGuHiggsScalarSourceOperator = false;
const bool smMassMatrixProvidesGuHiggsSelfCouplingSource = false;
const bool smMassMatrixJustifiesWOnlyCasimirMultiplier = false;
const bool smMassMatrixJustifiesZUnitMultiplier = false;
const bool smMassMatrixPromotesWzMasses = false;
const bool smMassMatrixPromotesHiggsMass = false;
const bool smMassMatrixCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var externalSources = new[]
{
    new ExternalSource(
        "pdg-2025-electroweak-model",
        "Particle Data Group, Electroweak Model and Constraints on New Physics, 2025 update",
        "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        "Public PDG review records the SM SU(2)xU(1) gauge structure, Higgs doublet VEV, photon/Z Weinberg rotation, W charged combinations, and tree-level W/Z/H mass formulas."),
    new ExternalSource(
        "pdg-2025-mass-formula-dependencies",
        "PDG 2025 tree-level electroweak mass dependencies",
        "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        "The SM tree-level formulas require v, g, g', theta_W or e, and the Higgs potential parameter lambda. That is a dependency map unless GU supplies those quantities and the observed-field extraction."),
};

var requiredGuContractMapping = new[]
{
    new ContractMapping("observedFieldExtractionTheoremId", "SM defines observed electroweak fields after choosing SU(2)xU(1), Higgs doublet, and unitary gauge; GU still lacks a source-derived observed-field extraction theorem.", false),
    new ContractMapping("electroweakGaugeEmbeddingId", "SM assumes the low-energy SU(2)xU(1) embedding; Phase313 records symbolic GU placement but not a promotable observed embedding.", false),
    new ContractMapping("photonEigenstateProjectionId", "SM gives photon/Z rotation; GU contract still requires a source-derived photon projection row and massless gate.", false),
    new ContractMapping("wBosonSourceRowId", "SM W row requires g and v; GU does not supply the W source row from a target-independent source theorem.", false),
    new ContractMapping("zBosonSourceRowId", "SM Z row requires g, g', v, and neutral mixing; GU does not supply the Z source row from a target-independent source theorem.", false),
    new ContractMapping("wzCommonBridgeGatePassed", "SM formulas share v/2 but only after importing the electroweak VEV and couplings; this is not a GU common bridge source.", false),
    new ContractMapping("higgsScalarSourceOperatorId", "SM has a Higgs potential, but it is an external scalar sector with free mu/lambda parameters, not the GU Higgs source operator.", false),
    new ContractMapping("potentialOrSelfCouplingSourceId", "SM tree-level Higgs mass depends on lambda v; GU still lacks a solved self-coupling source.", false),
};

var checks = new[]
{
    new Check(
        "pdg-electroweak-mass-matrix-source-recorded",
        pdg2025ElectroweakMassMatrixSourceAvailable
            && smGaugeGroupRequiresSu2AndU1
            && smMassGenerationRequiresHiggsDoublet
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime,
        $"pdgSource={pdg2025ElectroweakMassMatrixSourceAvailable}; su2u1={smGaugeGroupRequiresSu2AndU1}; higgsDoublet={smMassGenerationRequiresHiggsDoublet}; vev={smMassGenerationRequiresVev}; g={smMassGenerationRequiresWeakCouplingG}; gPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}"),
    new Check(
        "standard-model-field-rotation-and-mass-dependencies-recorded",
        smDefinesPhotonZWeinbergRotation
            && smDefinesChargedWCombination
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelPhotonMassZero
            && smTreeLevelHiggsMassDependsOnPotentialParameter,
        $"photonZRotation={smDefinesPhotonZWeinbergRotation}; chargedW={smDefinesChargedWCombination}; mwGv={smTreeLevelMwDependsOnGAndV}; mzGGPrimeV={smTreeLevelMzDependsOnGAndGPrimeAndV}; photonMassZero={smTreeLevelPhotonMassZero}; higgsLambdaV={smTreeLevelHiggsMassDependsOnPotentialParameter}"),
    new Check(
        "sm-mass-matrix-is-dependency-map-not-gu-source-lineage",
        smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixProvidesGuLocalWzTheorem
            && !smMassMatrixProvidesGuObservedFieldExtraction
            && !smMassMatrixProvidesGuVevSource
            && !smMassMatrixProvidesGuWeakCouplingSource
            && !smMassMatrixProvidesGuHyperchargeCouplingSource,
        $"externalDependencyMap={smMassMatrixProvidesExternalDependencyMap}; guLocalWzTheorem={smMassMatrixProvidesGuLocalWzTheorem}; guObservedExtraction={smMassMatrixProvidesGuObservedFieldExtraction}; guVevSource={smMassMatrixProvidesGuVevSource}; guWeakCoupling={smMassMatrixProvidesGuWeakCouplingSource}; guHypercharge={smMassMatrixProvidesGuHyperchargeCouplingSource}"),
    new Check(
        "sm-higgs-potential-is-not-gu-higgs-source",
        !smMassMatrixProvidesGuHiggsScalarSourceOperator
            && !smMassMatrixProvidesGuHiggsSelfCouplingSource
            && !smMassMatrixPromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"guHiggsScalarSource={smMassMatrixProvidesGuHiggsScalarSourceOperator}; guHiggsSelfCoupling={smMassMatrixProvidesGuHiggsSelfCouplingSource}; promotesHiggs={smMassMatrixPromotesHiggsMass}; canFillHiggs={canFillPhase201HiggsContract}"),
    new Check(
        "standard-model-neutral-mixing-does-not-justify-casimir-shortcut",
        smDefinesPhotonZWeinbergRotation
            && !smMassMatrixJustifiesWOnlyCasimirMultiplier
            && !smMassMatrixJustifiesZUnitMultiplier
            && dimensionCasimirWzSourceLawAuditPassed
            && !casimirRatioSourceBackedForBosonApplication
            && !wOnlyCasimirMultiplierJustified
            && !zUnitMultiplierJustified
            && !dimensionCasimirCanFillPhase201WzContract,
        $"smPhotonZRotation={smDefinesPhotonZWeinbergRotation}; smWOnlyCasimir={smMassMatrixJustifiesWOnlyCasimirMultiplier}; smZUnit={smMassMatrixJustifiesZUnitMultiplier}; p314CasimirApplication={casimirRatioSourceBackedForBosonApplication}; p314CanFill={dimensionCasimirCanFillPhase201WzContract}"),
    new Check(
        "existing-electroweak-input-audits-remain-blocked",
        electroweakSchemeRadiativeSourceAuditPassed
            && !schemeChoicePromotesBosonMasses
            && !schemeChoiceProvidesGuSourceLineage
            && fermiVevSourceAuditPassed
            && !fermiVevSourcePromotesBosonPredictions
            && !guVevSourceFound,
        $"p261Passed={electroweakSchemeRadiativeSourceAuditPassed}; schemePromotes={schemeChoicePromotesBosonMasses}; schemeGuLineage={schemeChoiceProvidesGuSourceLineage}; p293Passed={fermiVevSourceAuditPassed}; fermiPromotes={fermiVevSourcePromotesBosonPredictions}; guVevSourceFound={guVevSourceFound}"),
    new Check(
        "official-draft-and-observed-field-contract-remain-blocked",
        officialDraftElectroweakProjectionMapAuditPassed
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && !officialDraftProjectionMapPromotesWzMasses
            && !officialDraftCanFillPhase201WzContract
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !canFillPhase256ObservedFieldExtractionContract,
        $"p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p313ObservedEmbedding={officialDraftProvidesObservedElectroweakGaugeEmbedding}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}; canFillPhase256={canFillPhase256ObservedFieldExtractionContract}"),
    new Check(
        "phase201-contracts-remain-unfilled",
        !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !smMassMatrixCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && requiredGuContractMapping.All(row => !row.FilledBySmMassMatrix),
        $"promotesWz={smMassMatrixPromotesWzMasses}; promotesHiggs={smMassMatrixPromotesHiggsMass}; completes={smMassMatrixCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; mappedFilled={requiredGuContractMapping.Count(row => row.FilledBySmMassMatrix)}"),
};

var electroweakMassMatrixBridgeSourceAuditPassed = checks.All(check => check.Passed)
    && !smMassMatrixPromotesWzMasses
    && !smMassMatrixPromotesHiggsMass
    && !smMassMatrixCompletesBosonPredictions;
var terminalStatus = electroweakMassMatrixBridgeSourceAuditPassed
    ? "electroweak-mass-matrix-bridge-source-audit-dependency-map-not-gu-source"
    : "electroweak-mass-matrix-bridge-source-audit-review-required";

var result = new
{
    phaseId = "phase317-electroweak-mass-matrix-bridge-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    researchPerformedOn = "2026-05-20",
    electroweakMassMatrixBridgeSourceAuditPassed,
    pdg2025ElectroweakMassMatrixSourceAvailable,
    smGaugeGroupRequiresSu2AndU1,
    smMassGenerationRequiresHiggsDoublet,
    smMassGenerationRequiresVev,
    smMassGenerationRequiresWeakCouplingG,
    smMassGenerationRequiresHyperchargeCouplingGPrime,
    smDefinesPhotonZWeinbergRotation,
    smDefinesChargedWCombination,
    smTreeLevelMwDependsOnGAndV,
    smTreeLevelMzDependsOnGAndGPrimeAndV,
    smTreeLevelPhotonMassZero,
    smTreeLevelHiggsMassDependsOnPotentialParameter,
    smMassMatrixProvidesExternalDependencyMap,
    smMassMatrixPromotesWzMasses,
    smMassMatrixPromotesHiggsMass,
    smMassMatrixCompletesBosonPredictions,
    smMassMatrixBoundary = new
    {
        smMassMatrixProvidesGuLocalWzTheorem,
        smMassMatrixProvidesGuObservedFieldExtraction,
        smMassMatrixProvidesGuVevSource,
        smMassMatrixProvidesGuWeakCouplingSource,
        smMassMatrixProvidesGuHyperchargeCouplingSource,
        smMassMatrixProvidesGuHiggsScalarSourceOperator,
        smMassMatrixProvidesGuHiggsSelfCouplingSource,
        smMassMatrixJustifiesWOnlyCasimirMultiplier,
        smMassMatrixJustifiesZUnitMultiplier,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    upstreamEvidence = new
    {
        phase261 = new
        {
            electroweakSchemeRadiativeSourceAuditPassed,
            schemeChoicePromotesBosonMasses,
            schemeChoiceProvidesGuSourceLineage,
        },
        phase293 = new
        {
            fermiVevSourceAuditPassed,
            fermiVevSourcePromotesBosonPredictions,
            guVevSourceFound,
        },
        phase313 = new
        {
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesObservedElectroweakGaugeEmbedding,
            officialDraftProjectionMapPromotesWzMasses,
            officialDraftCanFillPhase201WzContract,
        },
        phase314 = new
        {
            dimensionCasimirWzSourceLawAuditPassed,
            casimirRatioSourceBackedForBosonApplication,
            wOnlyCasimirMultiplierJustified,
            zUnitMultiplierJustified,
            dimensionCasimirCanFillPhase201WzContract,
        },
    },
    externalSources,
    requiredGuContractMapping,
    checks,
    decision = "Do not promote W/Z or Higgs mass predictions by importing the Standard Model electroweak mass matrix. The SM mass matrix is the correct low-energy dependency map, and it explains why a W-only Casimir multiplier plus Z unit multiplier is not a physical source law. It still requires GU-derived observed-field extraction, VEV, g, g', neutral mixing, W/Z source rows, and Higgs scalar-source/self-coupling lineage before Phase201/256 can be filled.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving the observed SU(2)xU(1) embedding, photon/Z rotation, W rows, and Z row before target comparison.",
        "A GU-derived electroweak VEV and weak/hypercharge coupling transport source, not external SM input parameters.",
        "A GU Higgs scalar-source and self-coupling derivation that fixes the physical Higgs mass independent of observed targets.",
    },
    sourceEvidence = new
    {
        pdg2025ElectroweakModelUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase261Path = Phase261Path,
        phase293Path = Phase293Path,
        phase313Path = Phase313Path,
        phase314Path = Phase314Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "electroweak_mass_matrix_bridge_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_mass_matrix_bridge_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electroweakMassMatrixBridgeSourceAuditPassed={electroweakMassMatrixBridgeSourceAuditPassed}");
Console.WriteLine($"smMassMatrixProvidesExternalDependencyMap={smMassMatrixProvidesExternalDependencyMap}");
Console.WriteLine($"smMassMatrixPromotesWzMasses={smMassMatrixPromotesWzMasses}");
Console.WriteLine($"smMassMatrixPromotesHiggsMass={smMassMatrixPromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

public sealed record Check(
    string CheckId,
    bool Passed,
    string Detail);

public sealed record ExternalSource(
    string SourceId,
    string Title,
    string Url,
    string Finding);

public sealed record ContractMapping(
    string FieldId,
    string Assessment,
    bool FilledBySmMassMatrix);
