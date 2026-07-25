using System.Security.Cryptography;
using System.Text.Json;

// Phase555 assembles the two reserved measure-convention questions in a form a
// reviewer can answer, equipped with the measured inputs from Phases550-552. It
// authors no ruling, consumes no memo, verifies no signature, and changes no
// pending flag. Phase480 semantics are unchanged. Zero compute: no operator, no
// mesh, no linear algebra, no RNG.

const string Root = "studies/phase555_flat_sector_external_review_escalation_packet_001";
const string ContractPath = Root + "/preregistration/phase555_flat_sector_external_review_escalation_packet_contract_v1.json";
const string OutputPath = Root + "/output/flat_sector_external_review_escalation_packet.json";
const string SummaryPath = Root + "/output/flat_sector_external_review_escalation_packet_summary.json";

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
    return new { x.Id, x.Path, x.ExpectedSha256, ActualSha256 = actual, HashMatches = actual == x.ExpectedSha256 };
}).ToArray();
bool exactBindingsValid = bindings.Length == 7 && bindings.All(x => x.HashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase555-a30-flat-sector-external-review-escalation-packet-v1"
    && contract.GetProperty("planSection").GetString() == "COMPLETE_LATTICE_FLAT_SECTOR_PLAN_2026-07-25 A30"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("zeroCompute").GetBoolean()
    && contract.GetProperty("authorsARuling").GetBoolean() == false
    && contract.GetProperty("consumesAMemo").GetBoolean() == false
    && contract.GetProperty("verifiesASignature").GetBoolean() == false
    && contract.GetProperty("changesAPendingFlag").GetBoolean() == false
    && contract.GetProperty("phase480SemanticsChanged").GetBoolean() == false
    && exactBindingsValid
    && taxonomy.Length == 2
    && taxonomy[0] == "packet-incomplete-inputs-missing"
    && taxonomy[1] == "packet-assembled-awaiting-external-ruling"
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement Bound(string id)
{
    string path = bindingSpecs.First(x => x.Id == id).Path;
    return JsonDocument.Parse(File.ReadAllBytes(path)).RootElement.Clone();
}
JsonElement census = Bound("phase550-summary");
JsonElement adjudication = Bound("phase551-summary");
JsonElement reanalysis = Bound("phase552-summary");
JsonElement memoSchema = Bound("o4-memo-schema");

// The two reserved ruling identifiers must exist in the unchanged memo schema.
string[] reservedRulingIds = contract.GetProperty("reservedRulingIds")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string schemaText = memoSchema.GetRawText();
bool reservedRulingIdsPresentInSchema = reservedRulingIds.All(id => schemaText.Contains($"\"{id}\"", StringComparison.Ordinal));

// Upstream terminals must be the clean ones before a packet may claim to be
// equipped; a failed upstream terminal makes the packet incomplete, not wrong.
string censusTerminal = census.GetProperty("verdictKind").GetString()!;
string adjudicationTerminal = adjudication.GetProperty("verdictKind").GetString()!;
string reanalysisTerminal = reanalysis.GetProperty("verdictKind").GetString()!;
bool upstreamTerminalsAdmissible =
    censusTerminal == "origin-and-configuration-spectrum-characterized"
    && adjudicationTerminal == "adjudication-confirms-reported-values"
    && (reanalysisTerminal == "stationary-under-resolved-consistent"
        || reanalysisTerminal == "non-stationary-drift-detected");

// ------------------------------------------------------- measured packet inputs
JsonElement nullity = census.GetProperty("nullityTwoSided");
JsonElement incidence = census.GetProperty("structuralPrechecks").GetProperty("exactIntegerIncidence");
JsonElement lattice = census.GetProperty("completeLattice");
int flatSectorDimension = nullity.GetProperty("lowerBoundExactInteger").GetInt32();
int thresholdConditionalUpperBound = nullity.GetProperty("upperBoundThresholdConditional").GetInt32();
int vertexCount = lattice.GetProperty("vertexCount").GetInt32();
int algebraDimension = 3;
int exactPart = (vertexCount - 1) * algebraDimension;
int harmonicPart = flatSectorDimension - exactPart;
int operatorContributionBeyondKernelOfExteriorDerivative = thresholdConditionalUpperBound - flatSectorDimension;

var censusRows = census.GetProperty("spectralCensus").GetProperty("rows").EnumerateArray().ToArray();
var preservedRows = censusRows.Where(x => x.GetProperty("kind").GetString() == "preserved-checkpoint-position").ToArray();
var rayRows = censusRows.Where(x => x.GetProperty("kind").GetString() == "flat-ray-point").ToArray();

var homogeneousRows = census.GetProperty("homogeneousDecomposition").GetProperty("rows").EnumerateArray()
    .Select(x => new
    {
        configuration = x.GetProperty("id").GetString(),
        value = x.GetProperty("valueAtPosition").GetDouble(),
        degreeTwoFraction = x.GetProperty("degree2Fraction").GetDouble(),
        degreeThreeFraction = x.GetProperty("degree3Fraction").GetDouble(),
        degreeFourFraction = x.GetProperty("degree4Fraction").GetDouble(),
    }).ToArray();

var transverseRows = rayRows.Select(x => new
{
    rayPoint = x.GetProperty("id").GetString(),
    positionNormSquared = x.GetProperty("positionNormSquared").GetDouble(),
    remainingExactlyFlatCount = x.GetProperty("nullityUpperBoundThresholdConditional").GetInt32(),
    liftedFlatBlockLargestEigenvalue = x.GetProperty("flatBlockLargestEigenvalue").GetDouble(),
    fullLogDeterminantAboveRoundoffFloor = x.GetProperty("fullLogDeterminantAboveRoundoffFloor").GetDouble(),
    isModelBased = true,
}).ToArray();

var configurationRows = preservedRows.Select(x => new
{
    configuration = x.GetProperty("id").GetString(),
    positionNormSquared = x.GetProperty("positionNormSquared").GetDouble(),
    largestEigenvalue = x.GetProperty("largestEigenvalue").GetDouble(),
    smallestEigenvalue = x.GetProperty("smallestEigenvalue").GetDouble(),
    countBelowSmallestPositiveRung = x.GetProperty("nullityUpperBoundThresholdConditional").GetInt32(),
    negativeInertiaAtRoundoffFloor = x.GetProperty("negativeInertiaAtRoundoffFloor").GetInt32(),
    originFlatBlockSmallestEigenvalue = x.GetProperty("flatBlockSmallestEigenvalue").GetDouble(),
    originFlatBlockLargestEigenvalue = x.GetProperty("flatBlockLargestEigenvalue").GetDouble(),
}).ToArray();

var invarianceRows = census.GetProperty("measuredObservableInvariance").GetProperty("summary").EnumerateArray()
    .Select(x => new
    {
        observable = x.GetProperty("observable").GetString(),
        prospectivelyDeclaredClass = x.GetProperty("phase548DeclaredClass").GetString(),
        worstRelativeDeviationAlongTheMeasuredFlatSector = x.GetProperty("worstRelativeDeviationAcrossBasePoints").GetDouble(),
        measuredFlatSectorInvariant = x.GetProperty("measuredFlatSectorInvariant").GetBoolean(),
    }).ToArray();

var requiredInputs = new (string Id, bool Present)[]
{
    ("measured-flat-sector-dimension", flatSectorDimension > 0 && nullity.GetProperty("certified").GetBoolean()),
    ("topological-versus-operator-decomposition", exactPart > 0 && harmonicPart >= 0),
    ("exact-homogeneous-decomposition-at-real-configurations", homogeneousRows.Length == 6),
    ("transverse-scale-along-a-flat-ray", transverseRows.Length == 3),
    ("measured-observable-invariance", invarianceRows.Length == 3),
    ("independent-confirmation-of-the-measured-values", adjudicationTerminal == "adjudication-confirms-reported-values"),
    ("committed-chain-drift-reading", reanalysis.GetProperty("driftTest").GetProperty("driftInconclusive").GetBoolean() == false),
};
bool allRequiredInputsPresent = requiredInputs.All(x => x.Present);

bool packetComplete = contractValid && exactBindingsValid && reservedRulingIdsPresentInSchema
    && upstreamTerminalsAdmissible && allRequiredInputsPresent;
string verdict = packetComplete ? taxonomy[1] : taxonomy[0];

var questions = new object[]
{
    new
    {
        rulingId = reservedRulingIds[0],
        question = "May the measured exactly flat sector of the second-order form at the origin be classified as pure redundancy of the description, so that a quotient by it is the correct construction for this registered action on this complete lattice?",
        whyItIsReserved = "Classifying a measured null direction as redundancy is a convention about the measure, not a measurement. Phase490 already recorded quotient-underdetermined when this program attempted the analogous classification, and no phase in this lane applies a quotient.",
        measuredInputs = new
        {
            flatSectorDimension,
            thresholdConditionalUpperBound,
            boundsCoincide = nullity.GetProperty("boundsCoincide").GetBoolean(),
            lowerBoundIsThresholdFree = nullity.GetProperty("lowerBoundIsThresholdFree").GetBoolean(),
            exactPart,
            harmonicPart,
            operatorContributionBeyondKernelOfExteriorDerivative,
            scalarCoboundaryRank = incidence.GetProperty("scalarCoboundaryRank").GetInt32(),
            degreesOfFreedom = lattice.GetProperty("degreesOfFreedom").GetInt32(),
            exactlyFlatAlongTheWholeFrozenLadder = census.GetProperty("exactFlatness").GetProperty("exactFlatSectorObserved").GetBoolean(),
            negativeControlsValid = census.GetProperty("exactFlatness").GetProperty("negativeControlsValid").GetBoolean(),
            configurationRows,
        },
        whatEachAnswerWouldPermit = new
        {
            ifTheSectorIsRedundancy = "a separately preregistered construction could be motivated; it would still require its own frozen contract, its own controls, and its own independent adjudicator, and it would not by itself satisfy any Phase458 gate",
            ifTheSectorIsNotRedundancy = "the flat sector is a property of the registered action and the measure is not normalizable along it without a further physical input, which is itself a finding this lane may record but not act on",
            ifTheReviewerDefers = "the question remains unresolved and every downstream construction remains blocked, which is the current state",
        },
        rulingAuthoredHere = false,
    },
    new
    {
        rulingId = reservedRulingIds[1],
        question = "If the measured flat sector were classified as redundancy, is normalizing by the corresponding transverse factor the correct normalization for this registered action, given that the transverse curvature measured along a flat ray grows as the square of the ray parameter and 200 of the 252 directions remain exactly flat there?",
        whyItIsReserved = "The transverse normalization is the second half of the same measure convention. Measuring the transverse scale is permitted in this lane; choosing the normalization is not.",
        measuredInputs = new
        {
            transverseRows,
            remainingExactlyFlatAlongTheRay = transverseRows.Select(x => x.remainingExactlyFlatCount).Distinct().ToArray(),
            liftedDirectionCountAlongTheRay = transverseRows.Select(x => flatSectorDimension - x.remainingExactlyFlatCount).Distinct().ToArray(),
            transverseCurvatureScalesAsTheSquareOfTheRayParameter = true,
            quantityIsModelBasedNotCertified = true,
            homogeneousRows,
            invarianceRows,
            measuredInvarianceIsNotAGaugeOrbitStatement = true,
        },
        whatEachAnswerWouldPermit = new
        {
            ifTheNormalizationIsCorrect = "a separately preregistered normalized construction could be motivated under its own contract; it would not by itself satisfy any Phase458 gate",
            ifTheNormalizationIsNotCorrect = "no normalized construction follows and the lane's prerequisite question remains open",
            ifTheReviewerDefers = "the question remains unresolved, which is the current state",
        },
        rulingAuthoredHere = false,
    },
};

var output = new
{
    schemaVersion = 1,
    phase = 555,
    phaseId = "phase555-flat-sector-external-review-escalation-packet",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    bindings,
    zeroCompute = true,
    upstream = new
    {
        censusTerminal, adjudicationTerminal, reanalysisTerminal, upstreamTerminalsAdmissible,
    },
    requiredInputs = requiredInputs.Select(x => new { x.Id, x.Present }).ToArray(),
    allRequiredInputsPresent,
    reservedRulingIds,
    reservedRulingIdsPresentInSchema,
    questions,
    standingFirewallsRestatedForTheReviewer = new
    {
        measuringIsPermittedInterpretingIsNot = true,
        noQuotientApplied = true,
        noGaugeFixingApplied = true,
        noMeasureNormalizationApplied = true,
        noCeilingInflation = true,
        noReadjudicationOfPhase548Or549 = true,
        noGateProgress = "G3, G4 and G5 are keyed to the Phase455 and Phase456 artifacts and to the register, and nothing in this lane can supply them at any level of success",
        whatThisLaneEstablishes = "only whether the registered operator's complete-lattice target is samplable at all, which is a prerequisite for any future pack and is not a gate input",
        everyQuantityIsAPropertyOfADiscreteOperator = true,
    },
    verdictKind = verdict,
    terminalStatus = "flat-sector-external-review-escalation-packet-" + verdict,
    decision = verdict == taxonomy[1]
        ? "The two reserved measure-convention questions are assembled with the measured inputs a reviewer needs to answer them: a threshold-free integer flat-sector dimension with its topological and operator decomposition, the exact homogeneous decomposition of the value at six committed configurations, the transverse scale along a flat ray, and the measured rather than declared observable invariance. No ruling is authored, no memo is consumed, no signature is verified, and no pending flag changes."
        : "At least one required measured input or upstream terminal is missing, so the packet is incomplete and no escalation follows.",
    authorsARuling = false,
    consumesAMemo = false,
    verifiesASignature = false,
    changesAPendingFlag = false,
    phase480SemanticsChanged = false,
    o4Discharged = false,
    nullSpaceInterpretedAsGaugeVolume = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    phase548Or549Reinterpreted = false,
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
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
byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(output,
    new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
File.WriteAllBytes(OutputPath, bytes);
File.WriteAllBytes(SummaryPath, bytes);
Console.WriteLine($"Phase555 verdict: {verdict}");
Console.WriteLine($"reservedRulingIds={string.Join(", ", reservedRulingIds)}; inputsPresent={allRequiredInputsPresent}; rulingAuthored=False");

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
