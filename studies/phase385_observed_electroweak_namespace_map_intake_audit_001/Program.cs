using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase385_observed_electroweak_namespace_map_intake_audit_001/output";
const string Phase27Path = "studies/phase27_charge_sector_convention_001/electroweak_mixing_convention.json";
const string Phase27ReadinessPath = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase311Path = "studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output/completion_observed_sector_wz_row_selector_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase344Path = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit_summary.json";
const string Phase365Path = "studies/phase365_dressing_field_electroweak_observed_variables_audit_001/output/dressing_field_electroweak_observed_variables_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase382Path = "studies/phase382_response_image_observed_projection_requirement_audit_001/output/response_image_observed_projection_requirement_audit_summary.json";
const string Phase384Path = "studies/phase384_phase307_basis_energy_response_image_proxy_audit_001/output/phase307_basis_energy_response_image_proxy_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;
const int ExpectedHiggsMissingFieldCount = 14;

var outputDir = Environment.GetEnvironmentVariable("PHASE385_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27Path));
using var phase27Readiness = JsonDocument.Parse(File.ReadAllText(Phase27ReadinessPath));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase311 = JsonDocument.Parse(File.ReadAllText(Phase311Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase344 = JsonDocument.Parse(File.ReadAllText(Phase344Path));
using var phase365 = JsonDocument.Parse(File.ReadAllText(Phase365Path));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase382 = JsonDocument.Parse(File.ReadAllText(Phase382Path));
using var phase384 = JsonDocument.Parse(File.ReadAllText(Phase384Path));

var phase256TemplatePath = JsonString(phase256.RootElement, "templatePath")
    ?? "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_template.json";
using var phase256Template = JsonDocument.Parse(File.ReadAllText(phase256TemplatePath));

var phase256RequiredFieldIds = phase256Template.RootElement
    .GetProperty("requirementRows")
    .EnumerateArray()
    .Where(row => JsonBool(row, "required") is true)
    .Select(row => RequiredString(row, "fieldId"))
    .ToArray();
int phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? phase256RequiredFieldIds.Length;
int phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
bool phase256ContractMaterialized = JsonBool(phase256.RootElement, "contractMaterialized") is true
    && JsonBool(phase256.RootElement, "observedFieldExtractionIntakeContractPassed") is true
    && phase256RequiredFieldCount == 20
    && phase256FilledRequiredFieldCount == 0
    && JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is false;

int phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool phase201IntakeContractMaterialized = JsonBool(phase201.RootElement, "intakeContractMaterialized") is true
    && JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is false
    && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount
    && phase213HiggsMissingFieldCount == ExpectedHiggsMissingFieldCount;

bool phase27InternalConventionReady = JsonString(phase27Readiness.RootElement, "terminalStatus") == "mixing-convention-ready"
    && phase27Readiness.RootElement.TryGetProperty("convention", out var phase27Convention)
    && JsonString(phase27Convention, "status") == "validated"
    && JsonBool(phase27Convention, "externalTargetValuesUsed") is false
    && JsonBool(phase27.RootElement, "externalTargetValuesUsed") is false;
bool phase311ObservedMapAbsent = JsonBool(phase311.RootElement, "completionObservedSectorWzRowSelectorAuditPassed") is true
    && JsonBool(phase311.RootElement, "completionDraftProvidesPhotonWzEigenstateProjectionRows") is false
    && JsonBool(phase311.RootElement, "completionDraftProvidesPhysicalWzObservableMap") is false
    && JsonBool(phase311.RootElement, "canFillPhase201WzContract") is false;
bool phase313OfficialDraftMapAbsent = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true
    && JsonBool(phase313.RootElement, "officialDraftProvidesPhotonMasslessProjectionRow") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is false
    && JsonBool(phase313.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase313.RootElement, "canFillPhase201WzContract") is false;
bool phase321NeutralRouteUnpromotable = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesPhotonMasslessProjection") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesObservedEmbedding") is false
    && JsonBool(phase321.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase321.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;
bool fmsTemplateOnly = JsonBool(phase344.RootElement, "fmsGaugeInvariantSpectrumSourceAuditPassed") is true
    && JsonBool(phase344.RootElement, "fmsProvidesObservedFieldExtractionTemplate") is true
    && JsonBool(phase344.RootElement, "fmsRouteProvidesGuObservedFieldExtractionTheorem") is false
    && JsonBool(phase344.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;
bool dressingTemplateOnly = JsonBool(phase365.RootElement, "dressingFieldElectroweakObservedVariablesAuditPassed") is true
    && JsonBool(phase365.RootElement, "routeProvidesExternalObservedFieldExtractionTemplate") is true
    && JsonBool(phase365.RootElement, "routeProvidesGuNativeDressingField") is false
    && JsonBool(phase365.RootElement, "routeProvidesObservedPhotonWzHiggsProjectionRows") is false
    && JsonBool(phase365.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;
bool responseImageDiagnosticsOnly = JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true
    && JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap") is false
    && JsonBool(phase379.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase382.RootElement, "responseImageObservedProjectionRequirementAuditPassed") is true
    && JsonBool(phase382.RootElement, "observedCarrierAxisNamespaceSeparationMapPresent") is false
    && JsonBool(phase384.RootElement, "phase307BasisEnergyResponseImageProxyAuditPassed") is true
    && JsonBool(phase384.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase384.RootElement, "canFillPhase201WzContract") is false;

var candidateSeeds = new[]
{
    new CandidateSeed
    {
        CandidateId = "phase27-internal-cartan-mixing-convention",
        CandidateKind = "internal-convention",
        SourcePaths = new[] { Phase27Path, Phase27ReadinessPath },
        TargetBlindProvenancePresent = phase27InternalConventionReady,
        PhysicalTargetsConsultedForConstruction = false,
        DomainBasisSpecified = phase27InternalConventionReady,
        Evidence = new[] { "Phase27 validates charged and neutral internal axes without external target values." },
        Decision = "Internal axis convention only; it does not define observed photon/W/Z/H projection rows."
    },
    new CandidateSeed
    {
        CandidateId = "phase311-completion-observed-sector-program",
        CandidateKind = "local-completion-boundary",
        SourcePaths = new[] { Phase311Path },
        TargetBlindProvenancePresent = JsonBool(phase311.RootElement, "targetObservablesUsedForConstruction") is false,
        PhysicalTargetsConsultedForConstruction = false,
        Evidence = new[] { "Phase311 says the completion draft requires a typed observable map before comparison." },
        Decision = "Programmatic requirement only; no canonical W/Z row selector or physical observable map is supplied."
    },
    new CandidateSeed
    {
        CandidateId = "phase313-official-draft-symbolic-electroweak-placement",
        CandidateKind = "primary-gu-symbolic-placement",
        SourcePaths = new[] { Phase313Path },
        TargetBlindProvenancePresent = true,
        PhysicalTargetsConsultedForConstruction = false,
        DomainBasisSpecified = true,
        Evidence = new[] { "Phase313 records weak-isospin and weak-hypercharge location leads plus the Phase27 internal convention." },
        Decision = "Symbolic placement is not an observed electroweak projection map or source-row theorem."
    },
    new CandidateSeed
    {
        CandidateId = "phase321-neutral-electroweak-mixing-route",
        CandidateKind = "neutral-mixing-boundary",
        SourcePaths = new[] { Phase321Path },
        TargetBlindProvenancePresent = true,
        PhysicalTargetsConsultedForConstruction = false,
        DomainBasisSpecified = true,
        Evidence = new[] { "Phase321 inherits the Phase27 internal convention and audits neutral mixing requirements." },
        Decision = "Neutral mixing lacks a target-independent weak angle, photon projection, Z projection row, and observed embedding."
    },
    new CandidateSeed
    {
        CandidateId = "phase344-fms-observed-field-template",
        CandidateKind = "external-observed-field-template",
        SourcePaths = new[] { Phase344Path },
        ObservedFieldExtractionTemplatePresent = true,
        PhysicalTargetsConsultedForConstruction = false,
        Evidence = new[] { "Phase344 finds that FMS is an observed-field extraction template external to GU." },
        Decision = "Template only; no GU-local theorem, vacuum, composite operators, source scale, or projection rows are supplied."
    },
    new CandidateSeed
    {
        CandidateId = "phase365-dressing-field-template",
        CandidateKind = "external-observed-variable-template",
        SourcePaths = new[] { Phase365Path },
        ObservedFieldExtractionTemplatePresent = true,
        PhysicalTargetsConsultedForConstruction = false,
        Evidence = new[] { "Phase365 finds the dressing-field route constructs gauge-invariant electroweak variables externally." },
        Decision = "Template only; no GU-native dressing field, observed vacuum, projection rows, or mass-source lineage is supplied."
    },
    new CandidateSeed
    {
        CandidateId = "phase379-response-image-carrier-axis-diagnostic",
        CandidateKind = "carrier-response-diagnostic",
        SourcePaths = new[] { Phase379Path },
        TargetBlindProvenancePresent = JsonBool(phase379.RootElement, "targetBlindConstruction") is true,
        PhysicalTargetsConsultedForConstruction = JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is true,
        DomainBasisSpecified = true,
        Evidence = new[] { "Phase379 characterizes a response image and its carrier-axis suppression pattern." },
        Decision = "Carrier diagnostic only; it explicitly does not provide an observed electroweak field map or source-row law."
    },
    new CandidateSeed
    {
        CandidateId = "phase382-observed-projection-requirement",
        CandidateKind = "requirement-audit",
        SourcePaths = new[] { Phase382Path },
        TargetBlindProvenancePresent = JsonBool(phase382.RootElement, "targetBlindConstruction") is true,
        PhysicalTargetsConsultedForConstruction = JsonBool(phase382.RootElement, "physicalTargetsConsultedForConstruction") is true,
        Evidence = new[] { "Phase382 materializes the need for a carrier-axis to observed-electroweak namespace separation map." },
        Decision = "Requirement only; it records that no namespace-separation map is present."
    },
    new CandidateSeed
    {
        CandidateId = "phase384-phase27-basis-energy-proxy",
        CandidateKind = "basis-energy-proxy",
        SourcePaths = new[] { Phase384Path },
        TargetBlindProvenancePresent = JsonBool(phase384.RootElement, "targetBlindConstruction") is true,
        PhysicalTargetsConsultedForConstruction = JsonBool(phase384.RootElement, "physicalTargetsConsultedForConstruction") is true,
        DomainBasisSpecified = true,
        Evidence = new[] { "Phase384 checks Phase27 basis-energy metadata as a suppressed-axis proxy under the response-image sidecar." },
        Decision = "Proxy only; basis energy is not an observed electroweak projection or source-lineage theorem."
    },
};

var strictNamespaceFields = new (string FieldId, Func<CandidateSeed, bool> Filled)[]
{
    ("guNativeTheoremOrDerivationId", candidate => candidate.GuNativeTheoremOrDerivationId),
    ("sourceLineageId", candidate => candidate.SourceLineageId),
    ("targetBlindProvenancePresent", candidate => candidate.TargetBlindProvenancePresent),
    ("noPhysicalTargetsConsultedForConstruction", candidate => !candidate.PhysicalTargetsConsultedForConstruction),
    ("carrierAxisToObservedElectroweakMap", candidate => candidate.CarrierAxisToObservedElectroweakMap),
    ("domainBasisSpecified", candidate => candidate.DomainBasisSpecified),
    ("codomainBasisSpecified", candidate => candidate.CodomainBasisSpecified),
    ("branchNormalizationSource", candidate => candidate.BranchNormalizationSource),
    ("observedVacuumOrExpansion", candidate => candidate.ObservedVacuumOrExpansion),
    ("electroweakGaugeEmbedding", candidate => candidate.ElectroweakGaugeEmbedding),
    ("photonProjectionRow", candidate => candidate.PhotonProjectionRow),
    ("wProjectionOrSourceRows", candidate => candidate.WProjectionOrSourceRows),
    ("zProjectionOrSourceRow", candidate => candidate.ZProjectionOrSourceRow),
    ("higgsProjectionOrSourceRow", candidate => candidate.HiggsProjectionOrSourceRow),
    ("weakMixingAngleOrCouplingSource", candidate => candidate.WeakMixingAngleOrCouplingSource),
    ("quadraticMassOperator", candidate => candidate.QuadraticMassOperator),
    ("stabilitySidecars", candidate => candidate.StabilitySidecars),
    ("gevUnitNormalization", candidate => candidate.GevUnitNormalization),
};

var candidateAudits = candidateSeeds
    .Select(candidate =>
    {
        var namespaceFieldAudit = strictNamespaceFields
            .Select(field => new FieldAudit(field.FieldId, field.Filled(candidate)))
            .ToArray();
        var missingNamespaceFields = namespaceFieldAudit.Where(field => !field.Filled).Select(field => field.FieldId).ToArray();
        bool strictNamespaceMapPresent = missingNamespaceFields.Length == 0;
        bool candidatePhase256Ready = strictNamespaceMapPresent && candidate.CanFillPhase256ObservedFieldExtractionContract;
        bool candidatePhase201WzReady = strictNamespaceMapPresent && candidate.CanFillPhase201WzContract;
        bool candidatePhase201HiggsReady = strictNamespaceMapPresent && candidate.CanFillPhase201HiggsContract;
        return new CandidateAudit(
            candidate.CandidateId,
            candidate.CandidateKind,
            candidate.SourcePaths,
            SourcePathsExist: candidate.SourcePaths.All(File.Exists),
            candidate.ObservedFieldExtractionTemplatePresent,
            namespaceFieldAudit,
            namespaceFieldAudit.Count(field => field.Filled),
            missingNamespaceFields,
            strictNamespaceMapPresent,
            candidatePhase256Ready,
            candidatePhase201WzReady,
            candidatePhase201HiggsReady,
            AcceptedPhase256FieldIds: Array.Empty<string>(),
            AcceptedPhase201WzFieldIds: Array.Empty<string>(),
            AcceptedPhase201HiggsFieldIds: Array.Empty<string>(),
            candidate.RoutePromotesWzMasses,
            candidate.RoutePromotesHiggsMass,
            candidate.RouteCompletesBosonPredictions,
            candidate.Evidence,
            candidate.Decision);
    })
    .ToArray();

int candidateCount = candidateAudits.Length;
int intakeReadyCandidateCount = candidateAudits.Count(candidate => candidate.StrictNamespaceMapPresent);
int phase256ApplicationReadyCandidateCount = candidateAudits.Count(candidate => candidate.Phase256ApplicationReady);
int phase201WzApplicationReadyCandidateCount = candidateAudits.Count(candidate => candidate.Phase201WzApplicationReady);
int phase201HiggsApplicationReadyCandidateCount = candidateAudits.Count(candidate => candidate.Phase201HiggsApplicationReady);
int observedTemplateCandidateCount = candidateAudits.Count(candidate => candidate.ObservedFieldExtractionTemplatePresent);

bool noCandidateProvidesGuNativeObservedElectroweakNamespaceMap = intakeReadyCandidateCount == 0
    && candidateAudits.All(candidate => !candidate.StrictNamespaceMapPresent);
bool noCandidateCanFillPhase256ObservedFieldExtractionContract = phase256ApplicationReadyCandidateCount == 0
    && candidateAudits.All(candidate => candidate.AcceptedPhase256FieldIds.Length == 0);
bool noCandidateCanFillPhase201WzContract = phase201WzApplicationReadyCandidateCount == 0
    && candidateAudits.All(candidate => candidate.AcceptedPhase201WzFieldIds.Length == 0);
bool noCandidateCanFillPhase201HiggsContract = phase201HiggsApplicationReadyCandidateCount == 0
    && candidateAudits.All(candidate => candidate.AcceptedPhase201HiggsFieldIds.Length == 0);
bool fmsAndDressingRemainExternalTemplatesOnly = observedTemplateCandidateCount == 2 && fmsTemplateOnly && dressingTemplateOnly;
bool phase27RemainsInternalConventionOnly = phase27InternalConventionReady
    && candidateAudits.Single(candidate => candidate.CandidateId == "phase27-internal-cartan-mixing-convention").StrictNamespaceMapPresent is false;
bool phase379382384RemainDiagnosticsOrRequirementsOnly = responseImageDiagnosticsOnly
    && candidateAudits.Where(candidate => candidate.CandidateId.StartsWith("phase379-", StringComparison.Ordinal)
        || candidate.CandidateId.StartsWith("phase382-", StringComparison.Ordinal)
        || candidate.CandidateId.StartsWith("phase384-", StringComparison.Ordinal))
        .All(candidate => !candidate.StrictNamespaceMapPresent);

bool sourceContractApplicationAllowed = false;
bool canFillPhase201WzContract = false;
bool canFillPhase201HiggsContract = false;
bool canFillPhase256ObservedFieldExtractionContract = false;
bool routePromotesWzMasses = false;
bool routePromotesHiggsMass = false;
bool routeCompletesBosonPredictions = false;
bool phase201TemplateMutated = false;
int fieldsAppliedToPhase201TemplateCount = 0;
int acceptedContractFieldCount = 0;
int blockedContractFieldCount = ExpectedWzMissingFieldCount;

var checks = new[]
{
    Check(
        "phase256-and-phase201-contracts-materialized-unfilled",
        phase256ContractMaterialized && phase201IntakeContractMaterialized,
        $"phase256RequiredFieldCount={phase256RequiredFieldCount}; phase256FilledRequiredFieldCount={phase256FilledRequiredFieldCount}; phase201AllRequiredLineagesPromotable={JsonBool(phase201.RootElement, "allRequiredLineagesPromotable")}; wzMissingFieldCount={phase213WzMissingFieldCount}; higgsMissingFieldCount={phase213HiggsMissingFieldCount}"),
    Check(
        "namespace-map-candidate-set-materialized",
        candidateCount == 9 && candidateAudits.All(candidate => candidate.SourcePathsExist),
        $"candidateCount={candidateCount}; sourcePathsMissing={string.Join(",", candidateAudits.Where(candidate => !candidate.SourcePathsExist).Select(candidate => candidate.CandidateId))}"),
    Check(
        "no-candidate-provides-gu-native-observed-electroweak-namespace-map",
        noCandidateProvidesGuNativeObservedElectroweakNamespaceMap,
        $"intakeReadyCandidateCount={intakeReadyCandidateCount}; minimumMissingFields={candidateAudits.Min(candidate => candidate.MissingNamespaceFieldIds.Length)}"),
    Check(
        "no-candidate-can-fill-phase256-or-phase201-contracts",
        noCandidateCanFillPhase256ObservedFieldExtractionContract
            && noCandidateCanFillPhase201WzContract
            && noCandidateCanFillPhase201HiggsContract,
        $"phase256ApplicationReadyCandidateCount={phase256ApplicationReadyCandidateCount}; phase201WzApplicationReadyCandidateCount={phase201WzApplicationReadyCandidateCount}; phase201HiggsApplicationReadyCandidateCount={phase201HiggsApplicationReadyCandidateCount}"),
    Check(
        "template-and-proxy-boundaries-preserved",
        fmsAndDressingRemainExternalTemplatesOnly
            && phase27RemainsInternalConventionOnly
            && phase311ObservedMapAbsent
            && phase313OfficialDraftMapAbsent
            && phase321NeutralRouteUnpromotable
            && phase379382384RemainDiagnosticsOrRequirementsOnly,
        $"fmsAndDressingRemainExternalTemplatesOnly={fmsAndDressingRemainExternalTemplatesOnly}; phase27RemainsInternalConventionOnly={phase27RemainsInternalConventionOnly}; phase311ObservedMapAbsent={phase311ObservedMapAbsent}; phase313OfficialDraftMapAbsent={phase313OfficialDraftMapAbsent}; phase321NeutralRouteUnpromotable={phase321NeutralRouteUnpromotable}; phase379382384RemainDiagnosticsOrRequirementsOnly={phase379382384RemainDiagnosticsOrRequirementsOnly}"),
    Check(
        "source-contracts-not-mutated-or-promoted",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; acceptedContractFieldCount={acceptedContractFieldCount}; blockedContractFieldCount={blockedContractFieldCount}; phase201TemplateMutated={phase201TemplateMutated}"),
};

bool observedElectroweakNamespaceMapIntakeAuditPassed = checks.All(check => check.Passed);
string terminalStatus = observedElectroweakNamespaceMapIntakeAuditPassed
    ? "observed-electroweak-namespace-map-intake-audit-no-current-candidate-ready"
    : "observed-electroweak-namespace-map-intake-audit-review-required";
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase385-observed-electroweak-namespace-map-intake-audit-v1",
    $"phase256Path={Phase256Path}",
    $"phase201Path={Phase201Path}",
    $"candidateIds={string.Join(",", candidateAudits.Select(candidate => candidate.CandidateId))}",
    "physicalTargetsConsultedForConstruction=false"));
string decision = observedElectroweakNamespaceMapIntakeAuditPassed
    ? "No current local or external-template candidate supplies a GU-native observed electroweak namespace map with enough source lineage to fill Phase256 or Phase201. The next required artifact is a new theorem/source artifact, not another application of existing Phase27, Phase313, Phase321, FMS, dressing-field, or response-image proxy outputs."
    : "Review Phase385 before using the current candidate set to diagnose observed electroweak namespace-map readiness.";

var result = new
{
    phaseId = "phase385-observed-electroweak-namespace-map-intake-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    observedElectroweakNamespaceMapIntakeAuditPassed,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    targetBlindConstructionHash,
    applicationSubjectKind = "observed-electroweak-namespace-map-intake-candidates-for-phase256-phase201",
    phase256Contract = new
    {
        phase256Path = Phase256Path,
        phase256TemplatePath,
        phase256ContractMaterialized,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ObservedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable"),
        phase256RequiredFieldIds,
    },
    phase201Contract = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase201IntakeContractMaterialized,
        phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable"),
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
    },
    candidateAudits,
    candidateCount,
    intakeReadyCandidateCount,
    phase256ApplicationReadyCandidateCount,
    phase201WzApplicationReadyCandidateCount,
    phase201HiggsApplicationReadyCandidateCount,
    observedTemplateCandidateCount,
    noCandidateProvidesGuNativeObservedElectroweakNamespaceMap,
    noCandidateCanFillPhase256ObservedFieldExtractionContract,
    noCandidateCanFillPhase201WzContract,
    noCandidateCanFillPhase201HiggsContract,
    fmsAndDressingRemainExternalTemplatesOnly,
    phase27RemainsInternalConventionOnly,
    phase311ObservedMapAbsent,
    phase313OfficialDraftMapAbsent,
    phase321NeutralRouteUnpromotable,
    phase379382384RemainDiagnosticsOrRequirementsOnly,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    blockedContractFieldCount,
    phase213WzMissingFieldCount,
    phase213HiggsMissingFieldCount,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    checks,
    decision,
    sourceEvidence = new
    {
        phase27Path = Phase27Path,
        phase27ReadinessPath = Phase27ReadinessPath,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase311Path = Phase311Path,
        phase313Path = Phase313Path,
        phase321Path = Phase321Path,
        phase344Path = Phase344Path,
        phase365Path = Phase365Path,
        phase379Path = Phase379Path,
        phase382Path = Phase382Path,
        phase384Path = Phase384Path,
    },
    nextRequiredArtifact = new[]
    {
        "A GU-native observed electroweak projection theorem/source artifact.",
        "An explicit carrier-axis-to-observed photon/W/Z/H namespace map with domain and codomain bases.",
        "A target-independent branch normalization, observed vacuum/expansion, weak-mixing/coupling lineage, and quadratic mass operator.",
        "Separate W and Z Phase201 source rows plus Higgs scalar-source lineage, stability sidecars, pole extraction, and GeV normalization.",
    },
};

File.WriteAllText(
    Path.Combine(outputDir, "observed_electroweak_namespace_map_intake_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observed_electroweak_namespace_map_intake_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observedElectroweakNamespaceMapIntakeAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        phase256ContractMaterialized,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ObservedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable"),
        phase201IntakeContractMaterialized,
        phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable"),
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        candidateCount,
        intakeReadyCandidateCount,
        phase256ApplicationReadyCandidateCount,
        phase201WzApplicationReadyCandidateCount,
        phase201HiggsApplicationReadyCandidateCount,
        observedTemplateCandidateCount,
        noCandidateProvidesGuNativeObservedElectroweakNamespaceMap,
        noCandidateCanFillPhase256ObservedFieldExtractionContract,
        noCandidateCanFillPhase201WzContract,
        noCandidateCanFillPhase201HiggsContract,
        fmsAndDressingRemainExternalTemplatesOnly,
        phase27RemainsInternalConventionOnly,
        phase311ObservedMapAbsent,
        phase313OfficialDraftMapAbsent,
        phase321NeutralRouteUnpromotable,
        phase379382384RemainDiagnosticsOrRequirementsOnly,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.acceptedContractFieldCount,
        result.blockedContractFieldCount,
        result.phase201FieldsDefensiblyFilled,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observedElectroweakNamespaceMapIntakeAuditPassed={observedElectroweakNamespaceMapIntakeAuditPassed}");
Console.WriteLine($"candidateCount={candidateCount}");
Console.WriteLine($"intakeReadyCandidateCount={intakeReadyCandidateCount}");
Console.WriteLine($"phase256ApplicationReadyCandidateCount={phase256ApplicationReadyCandidateCount}");
Console.WriteLine($"phase201WzApplicationReadyCandidateCount={phase201WzApplicationReadyCandidateCount}");
Console.WriteLine($"phase201HiggsApplicationReadyCandidateCount={phase201HiggsApplicationReadyCandidateCount}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static Check Check(string checkId, bool passed, string detail) => new(checkId, passed, detail);

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static string HashText(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed class CandidateSeed
{
    public required string CandidateId { get; init; }
    public required string CandidateKind { get; init; }
    public required string[] SourcePaths { get; init; }
    public bool ObservedFieldExtractionTemplatePresent { get; init; }
    public bool TargetBlindProvenancePresent { get; init; }
    public bool PhysicalTargetsConsultedForConstruction { get; init; }
    public bool GuNativeTheoremOrDerivationId { get; init; }
    public bool SourceLineageId { get; init; }
    public bool CarrierAxisToObservedElectroweakMap { get; init; }
    public bool DomainBasisSpecified { get; init; }
    public bool CodomainBasisSpecified { get; init; }
    public bool BranchNormalizationSource { get; init; }
    public bool ObservedVacuumOrExpansion { get; init; }
    public bool ElectroweakGaugeEmbedding { get; init; }
    public bool PhotonProjectionRow { get; init; }
    public bool WProjectionOrSourceRows { get; init; }
    public bool ZProjectionOrSourceRow { get; init; }
    public bool HiggsProjectionOrSourceRow { get; init; }
    public bool WeakMixingAngleOrCouplingSource { get; init; }
    public bool QuadraticMassOperator { get; init; }
    public bool StabilitySidecars { get; init; }
    public bool GevUnitNormalization { get; init; }
    public bool CanFillPhase256ObservedFieldExtractionContract { get; init; }
    public bool CanFillPhase201WzContract { get; init; }
    public bool CanFillPhase201HiggsContract { get; init; }
    public bool RoutePromotesWzMasses { get; init; }
    public bool RoutePromotesHiggsMass { get; init; }
    public bool RouteCompletesBosonPredictions { get; init; }
    public required string[] Evidence { get; init; }
    public required string Decision { get; init; }
}

sealed record FieldAudit(string FieldId, bool Filled);

sealed record CandidateAudit(
    string CandidateId,
    string CandidateKind,
    string[] SourcePaths,
    bool SourcePathsExist,
    bool ObservedFieldExtractionTemplatePresent,
    FieldAudit[] NamespaceFieldAudit,
    int NamespaceFilledFieldCount,
    string[] MissingNamespaceFieldIds,
    bool StrictNamespaceMapPresent,
    bool Phase256ApplicationReady,
    bool Phase201WzApplicationReady,
    bool Phase201HiggsApplicationReady,
    string[] AcceptedPhase256FieldIds,
    string[] AcceptedPhase201WzFieldIds,
    string[] AcceptedPhase201HiggsFieldIds,
    bool RoutePromotesWzMasses,
    bool RoutePromotesHiggsMass,
    bool RouteCompletesBosonPredictions,
    string[] Evidence,
    string Decision);

sealed record Check(string CheckId, bool Passed, string Detail);
