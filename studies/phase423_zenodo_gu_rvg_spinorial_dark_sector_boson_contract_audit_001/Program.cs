using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase423: source-level audit of the June 2026 Zenodo GU-RVG spinorial
// dark-sector article. This phase records reduced full-text and supplemental
// evidence. It does not import 95.4 GeV, Koide, collider, or cosmology claims
// as electroweak W/Z/H source-lineage rows.

const string DefaultOutputDir = "studies/phase423_zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_001/output";
const string Phase201SummaryPath = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213SummaryPath = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase312SummaryPath = "studies/phase312_current_public_gu_rvg_revision_delta_audit_001/output/current_public_gu_rvg_revision_delta_audit_summary.json";
const string Phase419SummaryPath = "studies/phase419_observed_field_symbolic_extraction_template_001/output/observed_field_symbolic_extraction_template_summary.json";
const string Phase420SummaryPath = "studies/phase420_naive_curvature_mass_scale_sanity_check_001/output/naive_curvature_mass_scale_sanity_check_summary.json";
const string Phase422SummaryPath = "studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/output/vector_spinor_144_bilinear_scalar_capacity_audit_summary.json";
const string ApplicationSubjectKind = "zenodo-gu-rvg-spinorial-dark-sector-boson-contract-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE423_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213SummaryPath));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase312 = JsonDocument.Parse(File.ReadAllText(Phase312SummaryPath));
using var phase419 = JsonDocument.Parse(File.ReadAllText(Phase419SummaryPath));
using var phase420 = JsonDocument.Parse(File.ReadAllText(Phase420SummaryPath));
using var phase422 = JsonDocument.Parse(File.ReadAllText(Phase422SummaryPath));

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool zenodoRecordReviewed = true;
const bool fullTextReviewed = true;
const bool supplementalManifestReviewed = true;
const bool supplementalTextCodeReviewed = true;
const bool currentJune2026SourceDeltaConfirmed = true;
const bool priorPhase312SourceSetDidNotIncludeThisRecord = true;
const bool guRvgSpinorialDarkSectorScopeConfirmed = true;
const bool sourceClaimsGu14dSpinorialDarkSector = true;
const bool sourceClaimsLookingGlassMatter = true;
const bool sourceClaimsDarkDecoupledLookingGlassMatter = true;
const bool sourceClaimsTwoFieldCosmologyCompletion = true;
const bool sourceMentions95GevDilaton = true;
const bool sourceMentionsKoideDilatonShift = true;
const bool sourceUsesExternalCollider95GevSignal = true;
const bool sourceUsesExternalElectroweakVev246Gev = true;
const bool sourceUsesExternalStandardModelInputs = true;
const bool sourceProvidesSpinorialDarkSectorContext = true;
const bool sourceProvidesVectorSpinor144ProjectionMap = false;
const bool sourceProvidesBosonContractEvidence = false;
const bool sourceProvidesObservedElectroweakNamespaceMap = false;
const bool sourceProvidesPhotonWzHProjectionRows = false;
const bool sourceProvidesWzSourceRows = false;
const bool sourceProvidesSeparateWzRows = false;
const bool sourceProvidesHiggsScalarSourceRow = false;
const bool sourceProvidesElectroweakVevMap = false;
const bool sourceProvidesTargetIndependentVevSource = false;
const bool sourceProvidesWeakAngleOrCouplingLineage = false;
const bool sourceProvidesCurvatureToElectroweakScaleLaw = false;
const bool sourceProvidesPoleExtraction = false;
const bool sourceProvidesGeVUnitNormalization = false;
const bool sourceProvidesPhase419TemplateFields = false;
const bool sourceSatisfiesPhase420ScaleFields = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

var source = new
{
    refId = "ZENODO-20618066-GURVG-SPINORIAL-DARK-SECTOR",
    title = "Refractive Vacuum Gravity in a 14-Dimensional Holographic Framework: Galactic-Scale Vacuum Refraction and a Retained Spinorial Dark Sector",
    authors = new[] { "Jesse D. Hofseth", "Eric R. Weinstein" },
    doi = "10.5281/zenodo.20618066",
    zenodoRecordId = 20618066,
    zenodoRecord = "https://zenodo.org/records/20618066",
    zenodoApiRecord = "https://zenodo.org/api/records/20618066",
    publicationDate = "2026-06-09",
    version = "v1",
    reviewedOn = "2026-06-18",
    pdfArtifactName = "Refractive Vacuum Gravity in a 14-Dimensional Holographic Framework--Galactic-Scale Vacuum Refraction and a Retained Spinorial Dark Sector.pdf",
    pdfContentUrl = "https://zenodo.org/api/records/20618066/files/Refractive%20Vacuum%20Gravity%20in%20a%2014-Dimensional%20Holographic%20Framework%E2%80%94Galactic-Scale%20Vacuum%20Refraction%20and%20a%20Retained%20Spinorial%20Dark%20Sector.pdf/content",
    pdfSizeBytes = 1647698,
    pdfChecksum = "md5:da23008825cf90eb89138b5c560ef47f",
    extractionTool = "pdftotext",
    extractedLineCount = 4015,
    supplementalArtifactName = "Supplemental_Material.zip",
    supplementalContentUrl = "https://zenodo.org/api/records/20618066/files/Supplemental_Material.zip/content",
    supplementalSizeBytes = 1556360,
    supplementalChecksum = "md5:53b2461515af445e3a6a459ace3619bb",
    supplementalArchiveEntryCount = 58,
    supplementalTextCodeSearchFileCount = 32,
};

var pdfContractTermEvidence = new[]
{
    new TermEvidence("electroweak", 5, new[] { 1249, 2468, 2471, 3020, 3991 }, "external-sm-scale-or-precision-context"),
    new TermEvidence("weak-mixing", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weak-angle", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weinberg", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("hypercharge", 1, new[] { 3668 }, "standard-model-anomaly-cancellation-appendix"),
    new TermEvidence("higgs", 15, new[] { 184, 529, 539, 664, 666, 705, 721, 742 }, "95gev-extended-higgs-comparison-not-observed-higgs-source"),
    new TermEvidence("w-boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("z-boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("standard-model", 12, new[] { 93, 540, 809, 1037, 1059, 1291, 1292, 1298 }, "compatibility-or-reference-context"),
    new TermEvidence("gev", 76, new[] { 21, 45, 61, 69, 181, 183, 208, 229 }, "external-95gev-dilaton-and-sm-input-units"),
    new TermEvidence("vev", 1, new[] { 1249 }, "external-electroweak-vev-input-in-koide-dilaton-matching"),
    new TermEvidence("pole-substring", 2, new[] { 2069, 3059 }, "false-positive-dipole-or-multipole-not-pole-mass-extraction"),
    new TermEvidence("dirac", 5, new[] { 1021, 1025, 1027, 3355, 3644 }, "spinor-dark-sector-or-reference-context"),
    new TermEvidence("yang-mills", 0, Array.Empty<int>(), "absent-contract-keyword"),
};

var pdfScopeTermEvidence = new[]
{
    new TermEvidence("spinorial", 49, new[] { 2, 15, 35, 38, 55, 64, 65, 72 }, "positive-dark-sector-scope"),
    new TermEvidence("spinor", 65, new[] { 2, 15, 17, 35, 38, 55, 57, 61 }, "positive-dark-sector-scope"),
    new TermEvidence("looking-glass", 13, new[] { 16, 56, 139, 1056, 1070, 1093, 1270, 3029 }, "positive-dark-sector-scope"),
    new TermEvidence("dilaton", 78, new[] { 20, 44, 69, 185, 195, 229, 514, 521 }, "positive-95gev-rvg-scope"),
    new TermEvidence("koide", 38, new[] { 43, 71, 201, 209, 555, 686, 732, 1149 }, "positive-koide-dilaton-scope"),
    new TermEvidence("photon", 30, new[] { 21, 45, 204, 531, 536, 553, 556, 571 }, "trace-anomaly-diphoton-context"),
    new TermEvidence("projection", 3, new[] { 954, 1011, 1146 }, "holographic-dark-sector-projection-not-observed-wzh-projection"),
    new TermEvidence("source", 28, new[] { 20, 49, 118, 169, 384, 621, 768, 819 }, "generic-source-term-or-data-source-context"),
    new TermEvidence("field", 106, new[] { 20, 49, 53, 137, 138, 155, 176, 181 }, "rvg-scalar-and-dark-field-context"),
    new TermEvidence("scalar", 69, new[] { 20, 155, 169, 181, 182, 198, 201, 260 }, "95gev-dilaton-or-effective-scalar-context"),
};

var supplementalTermEvidence = new[]
{
    new SupplementalTermEvidence("electroweak", 1, new[] { "pipeline/verify_widths.py:42" }, "external-vev-input"),
    new SupplementalTermEvidence("weak-mixing", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("weak-angle", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("weinberg", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("hypercharge", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("higgs", 6, new[] { "pipeline/make_brmatrix.py:7", "pipeline/make_brmatrix.py:9", "pipeline/make_brmatrix.py:23", "pipeline/make_brmatrix.py:29" }, "95gev-branching-ratio-comparison"),
    new SupplementalTermEvidence("w-boson", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("z-boson", 0, Array.Empty<string>(), "absent-contract-keyword"),
    new SupplementalTermEvidence("standard-model", 1, new[] { "pipeline/bayes_plot_evidence.py:31" }, "plot-label-context"),
    new SupplementalTermEvidence("gev", 54, new[] { "pipeline/constraints.py:4", "pipeline/constraints.py:48", "pipeline/constraints.py:49", "pipeline/constraints.py:50" }, "external-95gev-koide-collider-units"),
    new SupplementalTermEvidence("vev", 1, new[] { "pipeline/verify_widths.py:42" }, "external-246gev-input"),
    new SupplementalTermEvidence("projection", 0, Array.Empty<string>(), "absent-observed-field-projection-keyword"),
    new SupplementalTermEvidence("spinorial", 19, new[] { "MANIFEST.txt:15", "README.txt:4", "pipeline/colab_for_hybrid-run.txt:2", "pipeline/colab_for_hybrid-run.txt:4" }, "positive-dark-sector-pipeline-scope"),
    new SupplementalTermEvidence("koide", 12, new[] { "MANIFEST.txt:27", "README.txt:38", "README.txt:91", "pipeline/make_exclusion.py:12" }, "positive-koide-check-scope"),
    new SupplementalTermEvidence("dilaton", 15, new[] { "MANIFEST.txt:28", "pipeline/constraints.py:4", "pipeline/constraints.py:62", "pipeline/constraints.py:91" }, "positive-95gev-dilaton-scope"),
};

var supplementPipelineRows = new[]
{
    new SupplementPipelineRow("verify.py", "Koide/f_phi, muon g-2, RVM/S8 checks.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: false),
    new SupplementPipelineRow("verify_widths.py", "Trace-anomaly partial-width and two-scale 95 GeV checks; uses v=246 GeV as an input.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: true),
    new SupplementPipelineRow("make_brmatrix.py", "95.4 GeV scalar branching-ratio comparison against extended-Higgs regions.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: false),
    new SupplementPipelineRow("full_sparc.py", "SPARC rotation-curve model comparison.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: false),
    new SupplementPipelineRow("bayes_full_sample.py", "Nested-sampling evidence run for RVG, NFW, MOND, and hybrid profiles.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: false),
    new SupplementPipelineRow("twofield_cmb.py", "Two-field cold/hot psi CMB comparison.", ElectroweakBosonProjectionRow: false, UsesExternalElectroweakInput: false),
};

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase312Passed = JsonBool(phase312.RootElement, "currentPublicGuRvgRevisionDeltaAuditPassed") is true;
var phase312PromotesWz = JsonBool(phase312.RootElement, "currentPublicGuRvgPromotesWzMasses") is true;
var phase312PromotesHiggs = JsonBool(phase312.RootElement, "currentPublicGuRvgPromotesHiggsMass") is true;
var phase419Passed = JsonBool(phase419.RootElement, "observedFieldSymbolicExtractionTemplatePassed") is true;
var phase419SourceDefinedPhase256FieldCount = JsonInt(phase419.RootElement, "sourceDefinedPhase256FieldCount") ?? -1;
var phase419CanFillObserved = JsonBool(phase419.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;
var phase420Passed = JsonBool(phase420.RootElement, "naiveCurvatureMassScaleSanityCheckPassed") is true;
var phase420MissingScaleSpecificationFieldCount = JsonInt(phase420.RootElement, "missingScaleSpecificationFieldCount") ?? -1;
var phase420SourceProvidesGeVUnitNormalization = JsonBool(phase420.RootElement, "sourceProvidesGeVUnitNormalization") is true;
var phase420CanFillWz = JsonBool(phase420.RootElement, "canFillPhase201WzContract") is true;
var phase422Passed = JsonBool(phase422.RootElement, "vectorSpinor144BilinearScalarCapacityAuditPassed") is true;
var phase422RequiresProjectionMap = JsonBool(phase422.RootElement, "vectorSpinor144BilinearStillRequiresSourceProjectionMap") is true;

var absentPdfContractKeywordCount = pdfContractTermEvidence.Count(term =>
    term.Count == 0
    && term.EvidenceClass == "absent-contract-keyword");
var pdfPositiveContractKeywordHitCount = pdfContractTermEvidence.Count(term => term.Count > 0);
var supplementalElectroweakProjectionRowCount = supplementPipelineRows.Count(row => row.ElectroweakBosonProjectionRow);
var supplementalExternalElectroweakInputRowCount = supplementPipelineRows.Count(row => row.UsesExternalElectroweakInput);
var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    ApplicationSubjectKind,
    source.refId,
    source.doi,
    source.pdfChecksum,
    source.supplementalChecksum,
    source.extractedLineCount,
    pdfContractTermEvidence = pdfContractTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    pdfScopeTermEvidence = pdfScopeTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    supplementalTermEvidence = supplementalTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    supplementPipelineRows = supplementPipelineRows.Select(row => new { row.FileId, row.ElectroweakBosonProjectionRow, row.UsesExternalElectroweakInput }),
}, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

var checks = new[]
{
    new Check(
        "zenodo-record-and-artifacts-recorded",
        zenodoRecordReviewed
            && source.zenodoRecordId == 20618066
            && source.publicationDate == "2026-06-09"
            && source.pdfSizeBytes == 1647698
            && source.pdfChecksum == "md5:da23008825cf90eb89138b5c560ef47f"
            && source.supplementalSizeBytes == 1556360
            && source.supplementalChecksum == "md5:53b2461515af445e3a6a459ace3619bb",
        $"recordId={source.zenodoRecordId}; publicationDate={source.publicationDate}; pdfChecksum={source.pdfChecksum}; supplementalChecksum={source.supplementalChecksum}"),
    new Check(
        "full-text-and-supplement-search-scope-recorded",
        fullTextReviewed
            && supplementalManifestReviewed
            && supplementalTextCodeReviewed
            && source.extractionTool == "pdftotext"
            && source.extractedLineCount == 4015
            && source.supplementalArchiveEntryCount == 58
            && source.supplementalTextCodeSearchFileCount == 32,
        $"extractionTool={source.extractionTool}; extractedLineCount={source.extractedLineCount}; supplementalArchiveEntryCount={source.supplementalArchiveEntryCount}; supplementalTextCodeSearchFileCount={source.supplementalTextCodeSearchFileCount}"),
    new Check(
        "current-june-2026-source-delta-is-new-relative-to-phase312",
        currentJune2026SourceDeltaConfirmed
            && priorPhase312SourceSetDidNotIncludeThisRecord
            && phase312Passed
            && !phase312PromotesWz
            && !phase312PromotesHiggs,
        $"currentJune2026SourceDeltaConfirmed={currentJune2026SourceDeltaConfirmed}; priorPhase312SourceSetDidNotIncludeThisRecord={priorPhase312SourceSetDidNotIncludeThisRecord}; phase312Passed={phase312Passed}; phase312PromotesWz={phase312PromotesWz}; phase312PromotesHiggs={phase312PromotesHiggs}"),
    new Check(
        "positive-source-scope-is-spinorial-dark-sector-and-rvg",
        guRvgSpinorialDarkSectorScopeConfirmed
            && sourceClaimsGu14dSpinorialDarkSector
            && sourceClaimsLookingGlassMatter
            && sourceClaimsDarkDecoupledLookingGlassMatter
            && sourceClaimsTwoFieldCosmologyCompletion
            && sourceProvidesSpinorialDarkSectorContext
            && pdfScopeTermEvidence.Single(term => term.TermId == "spinorial").Count == 49
            && pdfScopeTermEvidence.Single(term => term.TermId == "looking-glass").Count == 13,
        $"spinorialCount={pdfScopeTermEvidence.Single(term => term.TermId == "spinorial").Count}; lookingGlassCount={pdfScopeTermEvidence.Single(term => term.TermId == "looking-glass").Count}; providesSpinorialDarkSectorContext={sourceProvidesSpinorialDarkSectorContext}"),
    new Check(
        "positive-95gev-and-koide-material-uses-external-inputs",
        sourceMentions95GevDilaton
            && sourceMentionsKoideDilatonShift
            && sourceUsesExternalCollider95GevSignal
            && sourceUsesExternalElectroweakVev246Gev
            && sourceUsesExternalStandardModelInputs
            && pdfContractTermEvidence.Single(term => term.TermId == "gev").EvidenceClass == "external-95gev-dilaton-and-sm-input-units"
            && pdfContractTermEvidence.Single(term => term.TermId == "vev").EvidenceClass == "external-electroweak-vev-input-in-koide-dilaton-matching"
            && supplementalExternalElectroweakInputRowCount == 1,
        $"mentions95GevDilaton={sourceMentions95GevDilaton}; mentionsKoideDilatonShift={sourceMentionsKoideDilatonShift}; usesExternalElectroweakVev246Gev={sourceUsesExternalElectroweakVev246Gev}; supplementalExternalElectroweakInputRowCount={supplementalExternalElectroweakInputRowCount}"),
    new Check(
        "wz-contract-key-source-terms-absent-or-noncontract",
        absentPdfContractKeywordCount == 6
            && pdfContractTermEvidence.Single(term => term.TermId == "w-boson").Count == 0
            && pdfContractTermEvidence.Single(term => term.TermId == "z-boson").Count == 0
            && pdfContractTermEvidence.Single(term => term.TermId == "weak-mixing").Count == 0
            && pdfContractTermEvidence.Single(term => term.TermId == "weak-angle").Count == 0
            && pdfContractTermEvidence.Single(term => term.TermId == "weinberg").Count == 0
            && pdfContractTermEvidence.Single(term => term.TermId == "yang-mills").Count == 0
            && !sourceProvidesWzSourceRows
            && !sourceProvidesSeparateWzRows
            && !sourceProvidesWeakAngleOrCouplingLineage
            && !sourceProvidesTargetIndependentVevSource,
        $"absentPdfContractKeywordCount={absentPdfContractKeywordCount}; wBosonCount={pdfContractTermEvidence.Single(term => term.TermId == "w-boson").Count}; zBosonCount={pdfContractTermEvidence.Single(term => term.TermId == "z-boson").Count}; weakAngleCount={pdfContractTermEvidence.Single(term => term.TermId == "weak-angle").Count}"),
    new Check(
        "higgs-and-95gev-material-does-not-fill-observed-higgs-source",
        pdfContractTermEvidence.Single(term => term.TermId == "higgs").Count == 15
            && pdfContractTermEvidence.Single(term => term.TermId == "higgs").EvidenceClass == "95gev-extended-higgs-comparison-not-observed-higgs-source"
            && !sourceProvidesHiggsScalarSourceRow
            && !sourceProvidesPoleExtraction
            && !sourceProvidesGeVUnitNormalization,
        $"higgsCount={pdfContractTermEvidence.Single(term => term.TermId == "higgs").Count}; sourceProvidesHiggsScalarSourceRow={sourceProvidesHiggsScalarSourceRow}; sourceProvidesPoleExtraction={sourceProvidesPoleExtraction}; sourceProvidesGeVUnitNormalization={sourceProvidesGeVUnitNormalization}"),
    new Check(
        "supplement-is-reproducibility-pipeline-not-electroweak-projection-source",
        supplementalElectroweakProjectionRowCount == 0
            && supplementalTermEvidence.Single(term => term.TermId == "w-boson").Count == 0
            && supplementalTermEvidence.Single(term => term.TermId == "z-boson").Count == 0
            && supplementalTermEvidence.Single(term => term.TermId == "projection").Count == 0
            && !sourceProvidesPhotonWzHProjectionRows
            && !sourceProvidesObservedElectroweakNamespaceMap,
        $"supplementalElectroweakProjectionRowCount={supplementalElectroweakProjectionRowCount}; supplementalWCount={supplementalTermEvidence.Single(term => term.TermId == "w-boson").Count}; supplementalZCount={supplementalTermEvidence.Single(term => term.TermId == "z-boson").Count}; supplementalProjectionCount={supplementalTermEvidence.Single(term => term.TermId == "projection").Count}"),
    new Check(
        "phase419-phase420-and-phase422-boundaries-preserved",
        phase419Passed
            && phase419SourceDefinedPhase256FieldCount == 0
            && !phase419CanFillObserved
            && phase420Passed
            && phase420MissingScaleSpecificationFieldCount == 9
            && !phase420SourceProvidesGeVUnitNormalization
            && !phase420CanFillWz
            && phase422Passed
            && phase422RequiresProjectionMap
            && !sourceProvidesPhase419TemplateFields
            && !sourceSatisfiesPhase420ScaleFields
            && !sourceProvidesVectorSpinor144ProjectionMap,
        $"phase419SourceDefinedFields={phase419SourceDefinedPhase256FieldCount}; phase420MissingScaleFields={phase420MissingScaleSpecificationFieldCount}; phase422RequiresProjectionMap={phase422RequiresProjectionMap}; sourceProvidesVectorSpinor144ProjectionMap={sourceProvidesVectorSpinor144ProjectionMap}"),
    new Check(
        "phase201-and-phase256-contracts-remain-unfilled",
        !phase201AllRequiredLineagesPromotable
            && phase213WzMissingFieldCount == 15
            && phase213HiggsMissingFieldCount == 14
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable
            && !sourceProvidesBosonContractEvidence
            && !sourceProvidesObservedElectroweakNamespaceMap
            && !sourceProvidesPhotonWzHProjectionRows
            && !sourceProvidesWzSourceRows
            && !sourceProvidesHiggsScalarSourceRow,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; wzMissing={phase213WzMissingFieldCount}; higgsMissing={phase213HiggsMissingFieldCount}; phase256Filled={phase256FilledRequiredFieldCount}"),
    new Check(
        "source-contracts-not-mutated-or-promoted",
        targetBlindConstruction
            && !physicalTargetsConsultedForConstruction
            && targetBlindConstructionHash.Length == 64
            && !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; targetBlindHashLength={targetBlindConstructionHash.Length}; acceptedContractFieldCount={acceptedContractFieldCount}; routeCompletesBosonPredictions={routeCompletesBosonPredictions}"),
};

var zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed = checks.All(check => check.Passed)
    && currentJune2026SourceDeltaConfirmed
    && guRvgSpinorialDarkSectorScopeConfirmed
    && sourceProvidesSpinorialDarkSectorContext
    && !sourceProvidesBosonContractEvidence
    && !routeCompletesBosonPredictions;
var terminalStatus = zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed
    ? "zenodo-gu-rvg-spinorial-dark-sector-audit-no-electroweak-source"
    : "zenodo-gu-rvg-spinorial-dark-sector-audit-review-required";

var decision = "Treat Zenodo record 20618066 as current GU-RVG spinorial-dark-sector and 95.4 GeV dilaton/Koide/cosmology context, not as boson-prediction promotion evidence. The paper and supplement contain dark spinorial matter, Looking-Glass matter, two-field cosmology, 95.4 GeV dilaton, Koide, SPARC/CMB, and collider/detection checks, but they do not provide W/Z source rows, photon/W/Z/H observed projection rows, a GU-local electroweak VEV or weak-angle source, an observed-Higgs scalar source row, pole extraction, GeV/unit normalization, or the vector-spinor 144 projection map required after Phase422.";

var result = new
{
    phaseId = "phase423-zenodo-gu-rvg-spinorial-dark-sector-boson-contract-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind = ApplicationSubjectKind,
    zenodoRecordReviewed,
    fullTextReviewed,
    supplementalManifestReviewed,
    supplementalTextCodeReviewed,
    currentJune2026SourceDeltaConfirmed,
    priorPhase312SourceSetDidNotIncludeThisRecord,
    guRvgSpinorialDarkSectorScopeConfirmed,
    sourceClaimsGu14dSpinorialDarkSector,
    sourceClaimsLookingGlassMatter,
    sourceClaimsDarkDecoupledLookingGlassMatter,
    sourceClaimsTwoFieldCosmologyCompletion,
    sourceMentions95GevDilaton,
    sourceMentionsKoideDilatonShift,
    sourceUsesExternalCollider95GevSignal,
    sourceUsesExternalElectroweakVev246Gev,
    sourceUsesExternalStandardModelInputs,
    sourceProvidesSpinorialDarkSectorContext,
    sourceProvidesVectorSpinor144ProjectionMap,
    sourceProvidesBosonContractEvidence,
    sourceProvidesObservedElectroweakNamespaceMap,
    sourceProvidesPhotonWzHProjectionRows,
    sourceProvidesWzSourceRows,
    sourceProvidesSeparateWzRows,
    sourceProvidesHiggsScalarSourceRow,
    sourceProvidesElectroweakVevMap,
    sourceProvidesTargetIndependentVevSource,
    sourceProvidesWeakAngleOrCouplingLineage,
    sourceProvidesCurvatureToElectroweakScaleLaw,
    sourceProvidesPoleExtraction,
    sourceProvidesGeVUnitNormalization,
    sourceProvidesPhase419TemplateFields,
    sourceSatisfiesPhase420ScaleFields,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    source,
    pdfContractTermEvidence,
    pdfScopeTermEvidence,
    supplementalTermEvidence,
    supplementPipelineRows,
    counts = new
    {
        absentPdfContractKeywordCount,
        pdfPositiveContractKeywordHitCount,
        supplementalElectroweakProjectionRowCount,
        supplementalExternalElectroweakInputRowCount,
    },
    contractBoundary = new
    {
        phase201SummaryPath = Phase201SummaryPath,
        phase201AllRequiredLineagesPromotable,
        phase213SummaryPath = Phase213SummaryPath,
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        phase256SummaryPath = Phase256SummaryPath,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ContractPromotable,
        phase312SummaryPath = Phase312SummaryPath,
        phase312Passed,
        phase312PromotesWz,
        phase312PromotesHiggs,
        phase419SummaryPath = Phase419SummaryPath,
        phase419Passed,
        phase419SourceDefinedPhase256FieldCount,
        phase419CanFillObserved,
        phase420SummaryPath = Phase420SummaryPath,
        phase420Passed,
        phase420MissingScaleSpecificationFieldCount,
        phase420SourceProvidesGeVUnitNormalization,
        phase420CanFillWz,
        phase422SummaryPath = Phase422SummaryPath,
        phase422Passed,
        phase422RequiresProjectionMap,
    },
    checks,
    decision,
    nextRequiredArtifact = new[]
    {
        "A GU-local target-independent W/Z theorem with separate W and Z source rows.",
        "A source-defined observed-field extraction map with photon/W/Z/H projection rows and pole extraction.",
        "A GU-local scalar-sector source for the observed Higgs row, including scale, self-coupling, and GeV/unit normalization.",
        "If using Phase422, a source-defined vector-spinor 144 projection map from the same-chirality scalar capacity to observed electroweak fields.",
    },
};

var summary = new
{
    result.phaseId,
    result.terminalStatus,
    result.generatedAt,
    result.zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed,
    result.targetBlindConstruction,
    result.physicalTargetsConsultedForConstruction,
    result.targetBlindConstructionHash,
    result.applicationSubjectKind,
    result.zenodoRecordReviewed,
    result.fullTextReviewed,
    result.supplementalManifestReviewed,
    result.supplementalTextCodeReviewed,
    result.currentJune2026SourceDeltaConfirmed,
    result.priorPhase312SourceSetDidNotIncludeThisRecord,
    result.guRvgSpinorialDarkSectorScopeConfirmed,
    result.sourceMentions95GevDilaton,
    result.sourceMentionsKoideDilatonShift,
    result.sourceUsesExternalCollider95GevSignal,
    result.sourceUsesExternalElectroweakVev246Gev,
    result.sourceUsesExternalStandardModelInputs,
    result.sourceProvidesSpinorialDarkSectorContext,
    result.sourceProvidesVectorSpinor144ProjectionMap,
    result.sourceProvidesBosonContractEvidence,
    result.sourceProvidesObservedElectroweakNamespaceMap,
    result.sourceProvidesPhotonWzHProjectionRows,
    result.sourceProvidesWzSourceRows,
    result.sourceProvidesSeparateWzRows,
    result.sourceProvidesHiggsScalarSourceRow,
    result.sourceProvidesElectroweakVevMap,
    result.sourceProvidesTargetIndependentVevSource,
    result.sourceProvidesWeakAngleOrCouplingLineage,
    result.sourceProvidesCurvatureToElectroweakScaleLaw,
    result.sourceProvidesPoleExtraction,
    result.sourceProvidesGeVUnitNormalization,
    result.sourceProvidesPhase419TemplateFields,
    result.sourceSatisfiesPhase420ScaleFields,
    result.sourceContractApplicationAllowed,
    result.phase201TemplateMutated,
    result.fieldsAppliedToPhase201TemplateCount,
    result.acceptedContractFieldCount,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.source,
    result.counts,
    result.contractBoundary,
    result.checks,
    result.decision,
    result.nextRequiredArtifact,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit.json"), JsonSerializer.Serialize(result, options) + Environment.NewLine);
File.WriteAllText(Path.Combine(outputDir, "zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_summary.json"), JsonSerializer.Serialize(summary, options) + Environment.NewLine);

Console.WriteLine(terminalStatus);
Console.WriteLine($"zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed={zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed}");
Console.WriteLine($"currentJune2026SourceDeltaConfirmed={currentJune2026SourceDeltaConfirmed}");
Console.WriteLine($"sourceProvidesSpinorialDarkSectorContext={sourceProvidesSpinorialDarkSectorContext}");
Console.WriteLine($"sourceUsesExternalElectroweakVev246Gev={sourceUsesExternalElectroweakVev246Gev}");
Console.WriteLine($"sourceProvidesWzSourceRows={sourceProvidesWzSourceRows}");
Console.WriteLine($"sourceProvidesHiggsScalarSourceRow={sourceProvidesHiggsScalarSourceRow}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static bool? JsonBool(JsonElement element, string name)
{
    return element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string name)
{
    return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
        ? result
        : null;
}

static string Sha256Hex(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

public sealed record TermEvidence(string TermId, int Count, int[] SampleLineNumbers, string EvidenceClass);
public sealed record SupplementalTermEvidence(string TermId, int Count, string[] SampleLocations, string EvidenceClass);
public sealed record SupplementPipelineRow(string FileId, string Description, bool ElectroweakBosonProjectionRow, bool UsesExternalElectroweakInput);
public sealed record Check(string CheckId, bool Passed, string Evidence);
