using System.Text.Json;

const string DefaultOutputDir = "studies/phase350_spin_charge_family_boson_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";
const string Phase349Path = "studies/phase349_spin_exchange_preon_boson_mass_source_audit_001/output/spin_exchange_preon_boson_mass_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE350_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));
using var phase349 = JsonDocument.Parse(File.ReadAllText(Phase349Path));

const string spinChargeFamilyScalarWeakBosonArxiv = "https://arxiv.org/abs/1207.6233";
const string spinChargeFamilyHiggsYukawaArxiv = "https://arxiv.org/abs/1212.3184";
const string spinChargeFamilyTwiceFourFamiliesArxiv = "https://arxiv.org/abs/1307.2365";
const string spinChargeFamilyPredictionsArxiv = "https://arxiv.org/abs/1312.1542";
const string spinChargeFamilyExpandingUniverseArxiv = "https://arxiv.org/abs/1804.03513";

const bool spinChargeFamilyBosonSourceAuditPassedExpected = true;
const bool spinChargeFamilyBosonLeadPresent = true;
const bool spinChargeFamilyPrimarySourcesReviewed = true;
const bool spinChargeFamilyRouteExternalToGu = true;
const bool spinChargeFamilyRouteKaluzaKleinLike = true;
const bool spinChargeFamilyRouteUsesThirteenPlusOneDimensions = true;
const bool spinChargeFamilyRouteUsesTwoCliffordSpinConnections = true;
const bool spinChargeFamilyRouteExplainsSmGaugeVectorFields = true;
const bool spinChargeFamilyRouteExplainsScalarFieldsObservedAsHiggsAndYukawas = true;
const bool spinChargeFamilyRoutePredictsSeveralScalarFields = true;
const bool scalarFieldsDetermineFermionAndWeakBosonMassMixing = true;
const bool scalarFieldsAreWeakDoublets = true;
const bool scalarFieldsAreFamilyTriplets = true;
const bool scalarMassEigenstatesDifferFromWzCouplingFields = true;
const bool scalarMeasurementsDoNotCoincideWithSingleSmHiggs = true;
const bool twiceFourFermionFamilyPredictionPresent = true;
const bool fourthFamilyPredictionPresent = true;
const bool fifthFamilyDarkMatterCandidatePresent = true;
const bool routeProvidesFixedObservedWMass = false;
const bool routeProvidesFixedObservedZMass = false;
const bool routeProvidesFixedObservedHiggsMass = false;
const bool routeProvidesSingleObservedHiggsIdentification = false;

const bool routeRequiresExternalSpinChargeFamilyModel = true;
const bool routeRequiresFamilySymmetryBreakingSequence = true;
const bool routeRequiresScalarPotentialAndMassMatrixParameters = true;
const bool routeRequiresScalarMixingDiagonalization = true;
const bool routeRequiresLoopCorrectionAndFermionFitInputs = true;
const bool routeRequiresGuLocalThirteenPlusOneEmbedding = true;
const bool routeRequiresGuTwoCliffordConnectionMap = true;
const bool routeRequiresGuScalarFieldSourceOperator = true;
const bool routeRequiresGuWeakBosonScalarCouplingMap = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuIndependentWzSourceRows = true;
const bool routeRequiresGuSingleObservedHiggsOrScalarEnvelope = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalThirteenPlusOneEmbedding = false;
const bool routeProvidesGuTwoCliffordConnectionMap = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesSeparateObservedWzRows = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtractionContract = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesObservedHiggsMassFromGu = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesObservedFieldExtraction = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;
var kkRouteCompletesBosonPredictions = JsonBool(phase333.RootElement, "routeCompletesBosonPredictions") is true;
var octonionRouteCompletesBosonPredictions = JsonBool(phase337.RootElement, "routeCompletesBosonPredictions") is true;
var spinExchangePreonRouteCompletesBosonPredictions = JsonBool(phase349.RootElement, "routeCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1207.6233-spin-charge-family-scalars-weak-bosons",
        spinChargeFamilyScalarWeakBosonArxiv,
        "Spin-charge-family scalar fields and weak-boson couplings",
        "Discusses several scalar fields replacing the Standard Model Higgs/Yukawa description, including scalar mass eigenstates that differ from fields coupling to Z_m and W_m.",
        "Relevant scalar/weak-boson source lead, but external to GU and not a fixed observed W/Z/H mass prediction."),
    new SourceRow(
        "arxiv-1212.3184-spin-charge-family-higgs-yukawa",
        spinChargeFamilyHiggsYukawaArxiv,
        "Spin-charge-family explanation for Higgs and Yukawa couplings",
        "States that several scalar fields determine fermion and weak-boson masses and mixing matrices, with weak-doublet and family-triplet transformation behavior.",
        "Useful for scalar-source diagnosis; still lacks GU-local observed-field extraction, scalar-potential parameters, and GeV normalization."),
    new SourceRow(
        "arxiv-1307.2365-spin-charge-family-scalars-gauge-fields",
        spinChargeFamilyTwiceFourFamiliesArxiv,
        "Spin-charge-family twice-four families, scalars, and gauge fields",
        "Presents gauge vector and scalar fields emerging from two kinds of fields and relates the scalar fields and mass matrices to Standard Model Yukawa couplings and Higgs.",
        "Broad model-source lead, but it predicts a multi-scalar external theory rather than GU-local W/Z/H source rows."),
    new SourceRow(
        "arxiv-1312.1542-spin-charge-family-predictions",
        spinChargeFamilyPredictionsArxiv,
        "Spin-charge-family predictions beyond the Standard Model",
        "Describes the theory as Kaluza-Klein-like with two spins and predicts several scalar fields with the weak and hypercharge of the Standard Model Higgs.",
        "Relevant to Higgs-sector alternatives; no direct target-independent observed W/Z/H mass rows are supplied."),
    new SourceRow(
        "arxiv-1804.03513-spin-charge-family-expanding-universe",
        spinChargeFamilyExpandingUniverseArxiv,
        "Spin-charge-family d=(13+1) Clifford spin-connection framework",
        "Summarizes the theory as a d=(13+1) Kaluza-Klein-like route with two Clifford-algebra spin-connection fields that explains charges, vector gauge fields, families, and scalar fields observed as Higgs/Yukawa couplings.",
        "Geometric high-dimensional context, but still not a GU source-lineage artifact for physical W/Z/H mass extraction.")
};

var checks = new[]
{
    new Check(
        "spin-charge-family-primary-sources-reviewed",
        spinChargeFamilyBosonLeadPresent
            && spinChargeFamilyPrimarySourcesReviewed
            && spinChargeFamilyRouteExternalToGu
            && sourceRows.Length == 5,
        $"lead={spinChargeFamilyBosonLeadPresent}; reviewed={spinChargeFamilyPrimarySourcesReviewed}; externalToGu={spinChargeFamilyRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "spin-charge-family-boson-and-scalar-claims-captured",
        spinChargeFamilyRouteKaluzaKleinLike
            && spinChargeFamilyRouteUsesThirteenPlusOneDimensions
            && spinChargeFamilyRouteUsesTwoCliffordSpinConnections
            && spinChargeFamilyRouteExplainsSmGaugeVectorFields
            && spinChargeFamilyRouteExplainsScalarFieldsObservedAsHiggsAndYukawas
            && spinChargeFamilyRoutePredictsSeveralScalarFields
            && scalarFieldsDetermineFermionAndWeakBosonMassMixing
            && scalarFieldsAreWeakDoublets
            && scalarFieldsAreFamilyTriplets
            && scalarMassEigenstatesDifferFromWzCouplingFields
            && scalarMeasurementsDoNotCoincideWithSingleSmHiggs
            && twiceFourFermionFamilyPredictionPresent
            && fourthFamilyPredictionPresent
            && fifthFamilyDarkMatterCandidatePresent
            && !routeProvidesFixedObservedWMass
            && !routeProvidesFixedObservedZMass
            && !routeProvidesFixedObservedHiggsMass
            && !routeProvidesSingleObservedHiggsIdentification,
        $"kkLike={spinChargeFamilyRouteKaluzaKleinLike}; d13p1={spinChargeFamilyRouteUsesThirteenPlusOneDimensions}; twoClifford={spinChargeFamilyRouteUsesTwoCliffordSpinConnections}; scalarFields={spinChargeFamilyRoutePredictsSeveralScalarFields}; weakBosonMassMixing={scalarFieldsDetermineFermionAndWeakBosonMassMixing}; fixedW={routeProvidesFixedObservedWMass}; fixedZ={routeProvidesFixedObservedZMass}; fixedH={routeProvidesFixedObservedHiggsMass}; singleHiggs={routeProvidesSingleObservedHiggsIdentification}"),
    new Check(
        "current-source-lineage-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-field-contract-and-adjacent-routes-remain-nonpromotional",
        observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !higgsUpsilonRouteCompletesBosonPredictions
            && !kkRouteCompletesBosonPredictions
            && !octonionRouteCompletesBosonPredictions
            && !spinExchangePreonRouteCompletesBosonPredictions,
        $"observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; smWz={smMassMatrixPromotesWzMasses}; smHiggs={smMassMatrixPromotesHiggsMass}; higgsUpsilonCompletes={higgsUpsilonRouteCompletesBosonPredictions}; kkCompletes={kkRouteCompletesBosonPredictions}; octonionCompletes={octonionRouteCompletesBosonPredictions}; preonCompletes={spinExchangePreonRouteCompletesBosonPredictions}"),
    new Check(
        "spin-charge-family-route-requires-missing-gu-source-data",
        routeRequiresExternalSpinChargeFamilyModel
            && routeRequiresFamilySymmetryBreakingSequence
            && routeRequiresScalarPotentialAndMassMatrixParameters
            && routeRequiresScalarMixingDiagonalization
            && routeRequiresLoopCorrectionAndFermionFitInputs
            && routeRequiresGuLocalThirteenPlusOneEmbedding
            && routeRequiresGuTwoCliffordConnectionMap
            && routeRequiresGuScalarFieldSourceOperator
            && routeRequiresGuWeakBosonScalarCouplingMap
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuIndependentWzSourceRows
            && routeRequiresGuSingleObservedHiggsOrScalarEnvelope
            && routeRequiresGeVUnitNormalization,
        $"externalModel={routeRequiresExternalSpinChargeFamilyModel}; breaking={routeRequiresFamilySymmetryBreakingSequence}; scalarPotential={routeRequiresScalarPotentialAndMassMatrixParameters}; mixing={routeRequiresScalarMixingDiagonalization}; loops={routeRequiresLoopCorrectionAndFermionFitInputs}; guD13p1={routeRequiresGuLocalThirteenPlusOneEmbedding}; guClifford={routeRequiresGuTwoCliffordConnectionMap}; scalarSource={routeRequiresGuScalarFieldSourceOperator}; weakBosonMap={routeRequiresGuWeakBosonScalarCouplingMap}; observed={routeRequiresGuObservedFieldExtraction}; wzRows={routeRequiresGuIndependentWzSourceRows}; higgsEnvelope={routeRequiresGuSingleObservedHiggsOrScalarEnvelope}; gev={routeRequiresGeVUnitNormalization}"),
    new Check(
        "spin-charge-family-route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalThirteenPlusOneEmbedding
            && !routeProvidesGuTwoCliffordConnectionMap
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesSeparateObservedWzRows
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesGuObservedFieldExtractionContract
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesObservedHiggsMassFromGu
            && !routeProvidesGeVUnitNormalization
            && !routePromotesObservedFieldExtraction
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guD13p1={routeProvidesGuLocalThirteenPlusOneEmbedding}; guClifford={routeProvidesGuTwoCliffordConnectionMap}; guWzTheorem={routeProvidesGuLocalWzTheorem}; observedWzRows={routeProvidesSeparateObservedWzRows}; scale={routeProvidesTargetIndependentVevOrMassScale}; weakMixing={routeProvidesGuWeakMixingAngleSource}; coupling={routeProvidesGuGaugeCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; observedContract={routeProvidesGuObservedFieldExtractionContract}; scalarSource={routeProvidesGuHiggsScalarSourceOperator}; higgsMass={routeProvidesObservedHiggsMassFromGu}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}")
};

var spinChargeFamilyBosonSourceAuditPassed = checks.All(check => check.Passed)
    && spinChargeFamilyBosonSourceAuditPassedExpected
    && !routePromotesObservedFieldExtraction
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = spinChargeFamilyBosonSourceAuditPassed
    ? "spin-charge-family-source-audit-external-multiscalar-model-not-gu-source"
    : "spin-charge-family-source-audit-review-required";

var result = new
{
    phaseId = "phase350-spin-charge-family-boson-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    spinChargeFamilyBosonSourceAuditPassed,
    spinChargeFamilyBosonLeadPresent,
    spinChargeFamilyPrimarySourcesReviewed,
    spinChargeFamilyRouteExternalToGu,
    spinChargeFamilyRouteKaluzaKleinLike,
    spinChargeFamilyRouteUsesThirteenPlusOneDimensions,
    spinChargeFamilyRouteUsesTwoCliffordSpinConnections,
    spinChargeFamilyRouteExplainsSmGaugeVectorFields,
    spinChargeFamilyRouteExplainsScalarFieldsObservedAsHiggsAndYukawas,
    spinChargeFamilyRoutePredictsSeveralScalarFields,
    scalarFieldsDetermineFermionAndWeakBosonMassMixing,
    scalarFieldsAreWeakDoublets,
    scalarFieldsAreFamilyTriplets,
    scalarMassEigenstatesDifferFromWzCouplingFields,
    scalarMeasurementsDoNotCoincideWithSingleSmHiggs,
    twiceFourFermionFamilyPredictionPresent,
    fourthFamilyPredictionPresent,
    fifthFamilyDarkMatterCandidatePresent,
    routeProvidesFixedObservedWMass,
    routeProvidesFixedObservedZMass,
    routeProvidesFixedObservedHiggsMass,
    routeProvidesSingleObservedHiggsIdentification,
    routeRequiresExternalSpinChargeFamilyModel,
    routeRequiresFamilySymmetryBreakingSequence,
    routeRequiresScalarPotentialAndMassMatrixParameters,
    routeRequiresScalarMixingDiagonalization,
    routeRequiresLoopCorrectionAndFermionFitInputs,
    routeRequiresGuLocalThirteenPlusOneEmbedding,
    routeRequiresGuTwoCliffordConnectionMap,
    routeRequiresGuScalarFieldSourceOperator,
    routeRequiresGuWeakBosonScalarCouplingMap,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuIndependentWzSourceRows,
    routeRequiresGuSingleObservedHiggsOrScalarEnvelope,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalThirteenPlusOneEmbedding,
    routeProvidesGuTwoCliffordConnectionMap,
    routeProvidesGuLocalWzTheorem,
    routeProvidesSeparateObservedWzRows,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtractionContract,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesObservedHiggsMassFromGu,
    routeProvidesGeVUnitNormalization,
    routePromotesObservedFieldExtraction,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    relatedBlockingEvidence = new
    {
        smMassMatrixPromotesWzMasses,
        smMassMatrixPromotesHiggsMass,
        higgsUpsilonRouteCompletesBosonPredictions,
        kkRouteCompletesBosonPredictions,
        octonionRouteCompletesBosonPredictions,
        spinExchangePreonRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the spin-charge-family route. It is a serious external high-dimensional Clifford/Kaluza-Klein scalar-sector lead, but it introduces a separate multi-scalar model and does not supply GU-local d=(13+1) embedding, two-Clifford connection map, observed photon/W/Z/H projection rows, independent W/Z source rows, a single observed-Higgs scalar-source envelope, scalar-potential parameters, absolute GeV scale, or unit normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-native derivation of the spin-charge-family d=(13+1) or two-Clifford connection structure, or a proof that it is the same local GU source data.",
        "A target-independent scalar potential and mass-matrix diagonalization that identifies observed W, Z, and Higgs states rather than a general multi-scalar sector.",
        "Observed photon/W/Z/H projection rows and physical pole extraction before comparison with measured boson masses.",
        "Independent W and Z source rows with weak-angle, gauge-coupling, and absolute-scale lineage.",
        "A GU Higgs scalar-source envelope and GeV unit normalization before any W/Z/H mass promotion."
    }
};

var fullPath = Path.Combine(outputDir, "spin_charge_family_boson_source_audit.json");
var summaryPath = Path.Combine(outputDir, "spin_charge_family_boson_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.spinChargeFamilyBosonSourceAuditPassed,
    result.spinChargeFamilyBosonLeadPresent,
    result.spinChargeFamilyPrimarySourcesReviewed,
    result.spinChargeFamilyRouteExternalToGu,
    result.spinChargeFamilyRouteKaluzaKleinLike,
    result.spinChargeFamilyRouteUsesThirteenPlusOneDimensions,
    result.spinChargeFamilyRouteUsesTwoCliffordSpinConnections,
    result.spinChargeFamilyRouteExplainsSmGaugeVectorFields,
    result.spinChargeFamilyRouteExplainsScalarFieldsObservedAsHiggsAndYukawas,
    result.spinChargeFamilyRoutePredictsSeveralScalarFields,
    result.scalarFieldsDetermineFermionAndWeakBosonMassMixing,
    result.scalarFieldsAreWeakDoublets,
    result.scalarFieldsAreFamilyTriplets,
    result.scalarMassEigenstatesDifferFromWzCouplingFields,
    result.scalarMeasurementsDoNotCoincideWithSingleSmHiggs,
    result.twiceFourFermionFamilyPredictionPresent,
    result.fourthFamilyPredictionPresent,
    result.fifthFamilyDarkMatterCandidatePresent,
    result.routeProvidesFixedObservedWMass,
    result.routeProvidesFixedObservedZMass,
    result.routeProvidesFixedObservedHiggsMass,
    result.routeProvidesSingleObservedHiggsIdentification,
    result.routeRequiresExternalSpinChargeFamilyModel,
    result.routeRequiresFamilySymmetryBreakingSequence,
    result.routeRequiresScalarPotentialAndMassMatrixParameters,
    result.routeRequiresScalarMixingDiagonalization,
    result.routeRequiresLoopCorrectionAndFermionFitInputs,
    result.routeRequiresGuLocalThirteenPlusOneEmbedding,
    result.routeRequiresGuTwoCliffordConnectionMap,
    result.routeRequiresGuScalarFieldSourceOperator,
    result.routeRequiresGuWeakBosonScalarCouplingMap,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuIndependentWzSourceRows,
    result.routeRequiresGuSingleObservedHiggsOrScalarEnvelope,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalThirteenPlusOneEmbedding,
    result.routeProvidesGuTwoCliffordConnectionMap,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesSeparateObservedWzRows,
    result.routeProvidesTargetIndependentVevOrMassScale,
    result.routeProvidesGuWeakMixingAngleSource,
    result.routeProvidesGuGaugeCouplingNormalization,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesGuObservedFieldExtractionContract,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesObservedHiggsMassFromGu,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesObservedFieldExtraction,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBlockingEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"spinChargeFamilyBosonSourceAuditPassed={spinChargeFamilyBosonSourceAuditPassed}");
Console.WriteLine($"spinChargeFamilyRouteKaluzaKleinLike={spinChargeFamilyRouteKaluzaKleinLike}");
Console.WriteLine($"spinChargeFamilyRouteUsesTwoCliffordSpinConnections={spinChargeFamilyRouteUsesTwoCliffordSpinConnections}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetInt32(out var parsed) ? parsed : null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
