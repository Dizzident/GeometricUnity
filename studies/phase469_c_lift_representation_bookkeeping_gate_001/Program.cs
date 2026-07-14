using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase469: C-LIFT Stages -1..2 representation-bookkeeping gate.
//
// The binding plan permits WS5 only if the Phase441 Cl(7,7)/128 object is
// source-pinned with its 64+ + 64- grading, isotypics, Casimirs, Y weights,
// embedding, and epsilon-conjugation operator.  Phase441 names that object as
// a frontier requirement but also records spinorRealizedInvariantBasis=false.
// This phase distinguishes an abstract Clifford algebra control (which can be
// built exactly) from the missing source-defined representation object (which
// must not be invented).  It also computes the exact Phase465/467 ray-menu
// intersection so the remaining obstruction is localized unambiguously.

var stopwatch = Stopwatch.StartNew();

const string OutputDir = "studies/phase469_c_lift_representation_bookkeeping_gate_001/output";
const string Phase441Path = "studies/phase441_toy_branch_family_universality_sweep_001/output/toy_branch_family_universality_sweep_summary.json";
const string Phase465Path = "studies/phase465_anomaly_consistency_variety_kernel_001/output/anomaly_consistency_variety_kernel_summary.json";
const string Phase467Path = "studies/phase467_derived_operator_stabilizer_ray_census_001/output/derived_operator_stabilizer_ray_census_summary.json";
const string Phase441ConstructionHashPin = "b1cfbc3b5546d05297cd2c1ba3e97e9298bd74ab20f4f344fa885a4bf5e98daf";
const string Phase465ConstructionHashPin = "51ee4558001a50c42af7c003d92d2c4b4c83b8608a019b215816240e13ee8915";
const string Phase467ConstructionHashPin = "b4e6525dd0509b161b1aa941c2c0c547b0a41ebb51ee654a3cd5fbc18db8cf06";
const string ApplicationSubjectKind = "c-lift-representation-bookkeeping-gate";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 13";

Directory.CreateDirectory(OutputDir);

string[] requiredObjectFields =
{
    "canonicalShiabObject.objectId",
    "canonicalShiabObject.sourceArtifactId",
    "canonicalShiabObject.ambientCliffordSignature",
    "canonicalShiabObject.fullSpinorDimension",
    "canonicalShiabObject.positiveChiralityDimension",
    "canonicalShiabObject.negativeChiralityDimension",
    "canonicalShiabObject.gradingOperatorDefinition",
    "canonicalShiabObject.spin10Spin4Embedding",
    "canonicalShiabObject.positiveIsotypics",
    "canonicalShiabObject.negativeIsotypics",
    "canonicalShiabObject.casimirEigenvalues",
    "canonicalShiabObject.yWeightMap",
    "canonicalShiabObject.epsilonConjugationOperator",
};

bool inputsPresent = File.Exists(Phase441Path) && File.Exists(Phase465Path) && File.Exists(Phase467Path);
using var phase441 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(Phase441Path)) : null;
using var phase465 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(Phase465Path)) : null;
using var phase467 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(Phase467Path)) : null;

JsonElement p441 = phase441?.RootElement ?? default;
JsonElement p465 = phase465?.RootElement ?? default;
JsonElement p467 = phase467?.RootElement ?? default;

bool phase441HashMatches = inputsPresent && StringValue(p441, "targetBlindConstructionHash") == Phase441ConstructionHashPin;
bool phase465HashMatches = inputsPresent && StringValue(p465, "kernelConstructionHash") == Phase465ConstructionHashPin;
bool phase467HashMatches = inputsPresent && StringValue(p467, "constructionHash") == Phase467ConstructionHashPin;
bool phase441TerminalMatches = inputsPresent && StringValue(p441, "terminalStatus") ==
    "no-saturation-theorem-extends-to-entire-toy-family-scale-gap-requires-dim-four-spinor-shiab-or-source-anchor";
bool phase465TerminalMatches = inputsPresent && StringValue(p465, "verdictKind") ==
    "anomaly-variety-positive-dimensional-route-closed";
bool phase467TerminalMatches = inputsPresent && StringValue(p467, "verdictKind") ==
    "all-symmetric-non-y-route-closed";
bool inputPinsGreen = inputsPresent && phase441HashMatches && phase465HashMatches && phase467HashMatches
    && phase441TerminalMatches && phase465TerminalMatches && phase467TerminalMatches;

string[] phase441Evidence = inputsPresent
    ? p441.GetProperty("canonicalShiabImpossibility").GetProperty("sourceEvidence")
        .EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
    : Array.Empty<string>();
bool phase441FrontierMentionsCl77128 = phase441Evidence.Any(x => x.Contains("Cl(7,7)/128", StringComparison.Ordinal));
bool phase441ExplicitlySaysBasisAbsent = inputsPresent
    && !p441.GetProperty("canonicalShiabImpossibility").GetProperty("spinorRealizedInvariantBasis").GetBoolean();
bool phase441ExplicitlySaysCanonicalShiabUnrealized = inputsPresent
    && !p441.GetProperty("canonicalShiabImpossibility").GetProperty("canonicalPhysicalShiabRealizableOnToy").GetBoolean();

var presentObjectFields = requiredObjectFields.Where(path => HasPath(p441, path)).ToArray();
var missingObjectFields = requiredObjectFields.Where(path => !HasPath(p441, path)).ToArray();
bool sourceDefinedRepresentationObjectPinned = inputPinsGreen && missingObjectFields.Length == 0;

// Exact abstract Cl(7,7) Fock-space control.  Seven creation/annihilation
// modes give 2^7=128 basis states; even/odd degree gives 64+64.  The fourteen
// signed-permutation gamma actions are checked on every basis state.  This
// validates the dimension/grading arithmetic only and is deliberately not
// treated as the missing source-defined embedding or Shiab operator.
int fullSpinorDimension = 1 << 7;
int positiveChiralityDimension = Enumerable.Range(0, fullSpinorDimension).Count(x => BitOperations.PopCount((uint)x) % 2 == 0);
int negativeChiralityDimension = fullSpinorDimension - positiveChiralityDimension;
bool cliffordRelationsExact = CheckCliffordRelations();
bool abstractCliffordControlPassed = fullSpinorDimension == 128
    && positiveChiralityDimension == 64 && negativeChiralityDimension == 64
    && cliffordRelationsExact;

// Stage 2: intersect the Phase467 generic stabilizer menu with the exact
// anomaly equations used by Phase465.  The Phase404 chart is
// Y(alpha,beta)=alpha R3+beta J/2.  The six signed-slot charges below reproduce
// the Phase465 standard signature at (1,2/3), and all anomaly forms are tested
// exactly for every candidate ray.
var intersection = new List<RayIntersection>();
int phase467GenericRayCount = 0;
int anomalyConsistentGenericRayCount = 0;
if (inputPinsGreen)
{
    foreach (var row in p467.GetProperty("rayCensus").EnumerateArray())
    {
        if (row.GetProperty("stabilizerDimension").GetInt32() != 13) continue;
        phase467GenericRayCount++;
        Q alpha = Q.Parse(row.GetProperty("alpha").GetString()!);
        Q beta = Q.Parse(row.GetProperty("beta").GetString()!);
        Q[] y =
        {
            -beta / 4,                  // Q
            3 * beta / 4,               // L
            -alpha / 2 - 3 * beta / 4,  // e
            -alpha / 2 + beta / 4,      // d
            alpha / 2 - 3 * beta / 4,   // n
            alpha / 2 + beta / 4,       // u
        };
        bool anomalyConsistent =
            3 * y[0] + y[1] == Q.Zero &&
            2 * y[0] + y[3] + y[5] == Q.Zero &&
            6 * y[0] + 2 * y[1] + y[2] + 3 * y[3] + y[4] + 3 * y[5] == Q.Zero &&
            6 * Cube(y[0]) + 2 * Cube(y[1]) + Cube(y[2]) + 3 * Cube(y[3]) + Cube(y[4]) + 3 * Cube(y[5]) == Q.Zero;
        if (anomalyConsistent) anomalyConsistentGenericRayCount++;
        intersection.Add(new(alpha.ToString(), beta.ToString(), anomalyConsistent));
    }
}
bool phase465Phase467IntersectionExact = inputPinsGreen
    && phase467GenericRayCount == 176
    && anomalyConsistentGenericRayCount == 176
    && intersection.All(x => x.AnomalyConsistent);
string rayIntersectionHash = Sha256(string.Join("\n", intersection.Select(x => $"{x.Alpha}|{x.Beta}|{x.AnomalyConsistent}")));

// Fail-closed taxonomy sensitivity.  A synthetic complete source object must
// fund WS5, while removing any one required field must re-scope it.
bool ClassifyWs5Funding(IReadOnlyCollection<string> presentFields, bool menuNonempty) =>
    requiredObjectFields.All(presentFields.Contains) && menuNonempty;
bool syntheticCompleteObjectFundsWs5 = ClassifyWs5Funding(requiredObjectFields, menuNonempty: true);
bool syntheticMissingOneFieldRescopesWs5 = !ClassifyWs5Funding(requiredObjectFields.Skip(1).ToArray(), menuNonempty: true);
bool syntheticEmptyMenuRescopesWs5 = !ClassifyWs5Funding(requiredObjectFields, menuNonempty: false);
bool sensitivityBatteriesPassed = syntheticCompleteObjectFundsWs5
    && syntheticMissingOneFieldRescopesWs5 && syntheticEmptyMenuRescopesWs5;

bool representationBookkeepingPinned = sourceDefinedRepresentationObjectPinned
    && abstractCliffordControlPassed;
bool isotypicGatePassed = sourceDefinedRepresentationObjectPinned
    && HasPath(p441, "canonicalShiabObject.positiveIsotypics")
    && HasPath(p441, "canonicalShiabObject.negativeIsotypics");
bool casimirGatePassed = sourceDefinedRepresentationObjectPinned
    && HasPath(p441, "canonicalShiabObject.casimirEigenvalues");
bool yWeightGatePassed = sourceDefinedRepresentationObjectPinned
    && HasPath(p441, "canonicalShiabObject.yWeightMap");
bool epsilonConjugationPreRegistered = sourceDefinedRepresentationObjectPinned
    && HasPath(p441, "canonicalShiabObject.epsilonConjugationOperator");
bool menuNonempty = phase465Phase467IntersectionExact && anomalyConsistentGenericRayCount > 0;
bool ws5Funded = inputPinsGreen && representationBookkeepingPinned && isotypicGatePassed
    && casimirGatePassed && yWeightGatePassed && epsilonConjugationPreRegistered
    && menuNonempty && sensitivityBatteriesPassed;
bool ws5CancelledOrRescoped = inputPinsGreen && menuNonempty && !ws5Funded
    && !sourceDefinedRepresentationObjectPinned;

string verdictKind;
if (!inputPinsGreen)
    verdictKind = "input-pin-or-terminal-drift";
else if (!abstractCliffordControlPassed || !phase465Phase467IntersectionExact || !sensitivityBatteriesPassed)
    verdictKind = "exact-control-or-ray-intersection-failed";
else if (!sourceDefinedRepresentationObjectPinned)
    verdictKind = "bookkeeping-fails-source-object-unpinned-ws5-rescoped";
else if (!menuNonempty)
    verdictKind = "ray-menu-empty-ws5-cancelled";
else
    verdictKind = "bookkeeping-and-menu-green-ws5-funded-conditional";

string terminalStatus = $"c-lift-stages-minus1-through2-{verdictKind}";
bool cLiftRepresentationBookkeepingGateExecuted = inputPinsGreen
    && abstractCliffordControlPassed && phase465Phase467IntersectionExact
    && sensitivityBatteriesPassed && (ws5Funded || ws5CancelledOrRescoped || !menuNonempty);
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
const bool sourceContractApplicationAllowed = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

string constructionHash = Sha256(string.Join("|",
    Phase441ConstructionHashPin, Phase465ConstructionHashPin, Phase467ConstructionHashPin,
    string.Join(",", requiredObjectFields), string.Join(",", missingObjectFields),
    fullSpinorDimension, positiveChiralityDimension, negativeChiralityDimension,
    rayIntersectionHash, verdictKind));

stopwatch.Stop();
var result = new
{
    phaseId = "phase469-c-lift-representation-bookkeeping-gate",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    cLiftRepresentationBookkeepingGateExecuted,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    constructionHash,
    inputs = new
    {
        phase441Path = Phase441Path,
        phase465Path = Phase465Path,
        phase467Path = Phase467Path,
        inputsPresent,
        phase441ConstructionHashPin = Phase441ConstructionHashPin,
        phase441HashMatches,
        phase465ConstructionHashPin = Phase465ConstructionHashPin,
        phase465HashMatches,
        phase467ConstructionHashPin = Phase467ConstructionHashPin,
        phase467HashMatches,
        phase441TerminalMatches,
        phase465TerminalMatches,
        phase467TerminalMatches,
        inputPinsGreen,
    },
    stageMinusOneSourcePin = new
    {
        phase441FrontierMentionsCl77128,
        phase441ExplicitlySaysBasisAbsent,
        phase441ExplicitlySaysCanonicalShiabUnrealized,
        sourceDefinedRepresentationObjectPinned,
        requiredObjectFieldCount = requiredObjectFields.Length,
        presentObjectFieldCount = presentObjectFields.Length,
        missingObjectFieldCount = missingObjectFields.Length,
        requiredObjectFields,
        presentObjectFields,
        missingObjectFields,
        interpretation = "Phase441 commits a frontier requirement for a Cl(7,7)/128 invariant basis and explicitly records that the basis is absent; it does not commit a representation object that Stage -1 can pin.",
    },
    stageZeroAbstractCliffordControl = new
    {
        construction = "Lambda*(Q^7) signed Fock basis; seven creation/annihilation pairs realize Cl(7,7) exactly",
        fullSpinorDimension,
        positiveChiralityDimension,
        negativeChiralityDimension,
        cliffordRelationsExact,
        abstractCliffordControlPassed,
        sourceDefinedEmbeddingClaimed = false,
        scope = "dimension, grading, and Clifford-relation control only; no source-defined Spin(10)xSpin(4) embedding, isotypics, or Shiab operator is inferred",
    },
    stageOneRepresentationGates = new
    {
        representationBookkeepingPinned,
        isotypicGatePassed,
        casimirGatePassed,
        yWeightGatePassed,
        epsilonConjugationPreRegistered,
        gateFailureReason = sourceDefinedRepresentationObjectPinned
            ? ""
            : "The required source-defined embedding/isotypic/Casimir/Y-weight/epsilon-conjugation fields are absent from the pinned Phase441 object.",
    },
    stageTwoRayIntersection = new
    {
        phase467GenericRayCount,
        anomalyConsistentGenericRayCount,
        phase465Phase467IntersectionExact,
        menuNonempty,
        rayIntersectionHash,
        rows = intersection,
        interpretation = "Every one of Phase467's 176 generic dimension-13 rays satisfies the exact Phase465 anomaly equations in the Phase404 alpha/beta chart; the ray menu is nonempty and is not the WS5 blocker.",
    },
    fundingDecision = new
    {
        ws5Funded,
        ws5CancelledOrRescoped,
        rankGateDecision = ws5Funded ? "fund-ws5-conditional" : "rescope-ws5-source-object-required",
        reopeningCondition = "Commit a source-defined Cl(7,7)/128 representation object with the 13 required Stage -1 fields, including the embedding, 64+/- isotypics, Casimirs, Y-weight map, and epsilon-conjugation operator; then rerun Phase469.",
    },
    batteries = new
    {
        abstractCliffordControlPassed,
        phase465Phase467IntersectionExact,
        syntheticCompleteObjectFundsWs5,
        syntheticMissingOneFieldRescopesWs5,
        syntheticEmptyMenuRescopesWs5,
        sensitivityBatteriesPassed,
        allPassed = abstractCliffordControlPassed && phase465Phase467IntersectionExact && sensitivityBatteriesPassed,
    },
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
    decision = ws5CancelledOrRescoped
        ? "C-LIFT Stages -1 through 2 are adjudicated fail-closed. The abstract Cl(7,7) Fock control verifies 128=64+64 exactly and the Phase465/467 intersection contains all 176 generic rays, but Phase441 commits only the requirement for a metric-spinor invariant basis and explicitly records that basis absent. It supplies none of the source-defined embedding, isotypic, Casimir, Y-weight, or epsilon-conjugation fields required to fund WS5. The weeks-scale build is therefore re-scoped/cancelled pending the named source-object reopening condition; no representation object is invented and nothing is promoted."
        : "Do not consume this phase unless all input pins, exact controls, ray intersections, taxonomy batteries, and fail-closed funding rules pass.",
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "c_lift_representation_bookkeeping_gate.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "c_lift_representation_bookkeeping_gate_summary.json"), json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"cLiftRepresentationBookkeepingGateExecuted={cLiftRepresentationBookkeepingGateExecuted}");
Console.WriteLine($"phase441MissingObjectFieldCount={missingObjectFields.Length}");
Console.WriteLine($"abstractCliffordDimensions={fullSpinorDimension}={positiveChiralityDimension}+{negativeChiralityDimension}");
Console.WriteLine($"rayIntersectionCount={anomalyConsistentGenericRayCount}");
Console.WriteLine($"ws5Funded={ws5Funded}");
Console.WriteLine($"ws5CancelledOrRescoped={ws5CancelledOrRescoped}");
Console.WriteLine($"promotedPhysicalMassClaimCount={promotedPhysicalMassClaimCount}");
Environment.ExitCode = cLiftRepresentationBookkeepingGateExecuted ? 0 : 2;

static bool HasPath(JsonElement root, string path)
{
    if (root.ValueKind != JsonValueKind.Object) return false;
    JsonElement current = root;
    foreach (string segment in path.Split('.'))
    {
        if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current)) return false;
    }
    return current.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
}

static string? StringValue(JsonElement root, string property) =>
    root.ValueKind == JsonValueKind.Object && root.TryGetProperty(property, out var value)
        ? value.GetString()
        : null;

static bool CheckCliffordRelations()
{
    for (int a = 0; a < 14; a++)
    for (int b = a; b < 14; b++)
    for (int state = 0; state < 128; state++)
    {
        var ab = ApplyGamma(a, ApplyGamma(b, (state, 1)));
        var ba = ApplyGamma(b, ApplyGamma(a, (state, 1)));
        if (ab.State != ba.State) return false;
        int actual = ab.Coefficient + ba.Coefficient;
        int expected = a == b ? (a < 7 ? 2 : -2) : 0;
        if ((a == b && ab.State != state) || actual != expected) return false;
    }
    return true;
}

static (int State, int Coefficient) ApplyGamma(int gamma, (int State, int Coefficient) input)
{
    int mode = gamma % 7;
    bool positive = gamma < 7;
    int bit = 1 << mode;
    bool occupied = (input.State & bit) != 0;
    int lowerMask = bit - 1;
    int wedgeSign = BitOperations.PopCount((uint)(input.State & lowerMask)) % 2 == 0 ? 1 : -1;
    int coefficient = input.Coefficient * wedgeSign * (positive || !occupied ? 1 : -1);
    return (input.State ^ bit, coefficient);
}

static Q Cube(Q x) => x * x * x;
static string Sha256(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

readonly record struct RayIntersection(string Alpha, string Beta, bool AnomalyConsistent);

readonly struct Q : IEquatable<Q>
{
    public static readonly Q Zero = new(0, 1);
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public Q(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero) throw new DivideByZeroException();
        if (denominator.Sign < 0) { numerator = -numerator; denominator = -denominator; }
        BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    public static Q Parse(string text)
    {
        string[] parts = text.Split('/');
        return parts.Length == 1
            ? new Q(BigInteger.Parse(parts[0]), 1)
            : new Q(BigInteger.Parse(parts[0]), BigInteger.Parse(parts[1]));
    }

    public static Q operator +(Q a, Q b) => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Q operator -(Q a, Q b) => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Q operator -(Q a) => new(-a.Numerator, a.Denominator);
    public static Q operator *(Q a, Q b) => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    public static Q operator *(int a, Q b) => new(a * b.Numerator, b.Denominator);
    public static Q operator /(Q a, int b) => new(a.Numerator, a.Denominator * b);
    public static bool operator ==(Q a, Q b) => a.Equals(b);
    public static bool operator !=(Q a, Q b) => !a.Equals(b);
    public bool Equals(Q other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object? obj) => obj is Q q && Equals(q);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";
}
