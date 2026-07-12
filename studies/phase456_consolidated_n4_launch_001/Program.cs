using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase456: Consolidated n=4 Launch — PACK-GATED (WAVE2_ACTION_PLAN_2026-07-12 item 9).
//
// The A4 + A5 Stage-A PRE-REGISTRATION PACK now lives, hash-pinned, in this phase's
// preregistration/ directory. This program is the pack REFUSE-TO-RUN GATE and the standing
// claim boundary. It performs NO physics computation and launches NO sampling: production
// (the ~6-16 h HMC) is deliberately NOT implemented yet. What this program does:
//
//   * absent pack            -> emits the interim terminal "awaiting-pack" (still reachable);
//   * committed pack, hash OK -> advances to "pack-committed-awaiting-gates";
//   * committed pack, hash MISMATCH -> BLOCKED "pack-hash-mismatch-refuse-to-run"
//                               (the MANDATORY refuse-to-run condition: production must never
//                                sample against an unpinned/altered pack).
//
// The per-site (un-slice-summed) correlator storage flag is pinned in the pack manifest and
// echoed here as MANDATORY; the dispersion (k_min = 2*pi/4) and non-identity channel both
// require it.
//
// MANDATORY FRAMING. Zero physics compute; nothing measured, filled, or promoted;
// promotedPhysicalMassClaimCount remains 0. physicistReviewPending is carried explicitly.
// The pack is a pre-registration artifact only — no verdict is claimed by this program.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "consolidated-n4-launch";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 9";
const string DefaultOutputDir = "studies/phase456_consolidated_n4_launch_001/output";
const string PackDir = "studies/phase456_consolidated_n4_launch_001/preregistration";
const string TerminalPrefix = "consolidated-n4-launch-";

// --- MANDATORY hash-refuse-to-run gate: pinned committed pack hash --------------------
// SHA-256 over the concatenation (listed order, byte-exact contents) of the pinned pack
// files. Regenerated only by the pre-registration generator; a mismatch fail-closes.
const string PinnedPackSha256 = "40fd3c3488f94d18f50961e85d0bb3a3eabd1a31a071b61149875b8cf3d437aa";
string[] packFiles =
{
    "a4_symmetry_irrep_projectors.json",
    "a5_gaussian_domination_stage_a.json",
    "pack_manifest.json",
};

// --- standing claim boundary (verbatim across the program) ---
const bool awaitingProductionImplementation = true;      // the ~6-16 h HMC is not implemented yet
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;

bool claimBoundaryHeld =
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    physicistReviewPending && scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract && !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract && !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian && !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization && !routeCompletesBosonPredictions &&
    !routePromotesWzMasses && !routePromotesHiggsMass;

// --- evaluate the pack gate -----------------------------------------------------------
bool packDirPresent = Directory.Exists(PackDir);
var missingPackFiles = packFiles.Where(f => !File.Exists(System.IO.Path.Combine(PackDir, f))).ToArray();
bool packPresent = packDirPresent && missingPackFiles.Length == 0;

string? computedPackSha256 = null;
bool packHashMatches = false;
if (packPresent)
{
    var hashInput = new List<byte>();
    foreach (var f in packFiles) hashInput.AddRange(File.ReadAllBytes(System.IO.Path.Combine(PackDir, f)));
    computedPackSha256 = Convert.ToHexString(SHA256.HashData(hashInput.ToArray())).ToLowerInvariant();
    packHashMatches = string.Equals(computedPackSha256, PinnedPackSha256, StringComparison.Ordinal);
}

// terminal taxonomy (pre-registered):
//   awaiting-pack                     -- interim green; pack absent
//   pack-committed-awaiting-gates     -- pack present + hash matches; awaiting the hard gates + O4
//   pack-hash-mismatch-refuse-to-run  -- BLOCKED; the mandatory refuse-to-run condition
string interimTerminal;
string verdictKind;
bool refuseToRun;
if (!packPresent)
{
    interimTerminal = "awaiting-pack";
    verdictKind = "awaiting-pack";
    refuseToRun = false;
}
else if (packHashMatches)
{
    interimTerminal = "pack-committed-awaiting-gates";
    verdictKind = "pack-committed-awaiting-gates";
    refuseToRun = false;
}
else
{
    interimTerminal = "pack-hash-mismatch-refuse-to-run";
    verdictKind = "pack-hash-mismatch-refuse-to-run";
    refuseToRun = true;
}

// The phase validates green (no silent-promotion path) in the two non-refuse states; the
// mismatch state is a fail-closed BLOCK. Production sampling is not implemented in any state.
bool phase456GateGreen = claimBoundaryHeld && !refuseToRun;
string terminalStatus = TerminalPrefix + interimTerminal;

string decision = interimTerminal switch
{
    "awaiting-pack" =>
        "Consolidated n=4 launch (plan item 9): the pre-registration pack is not present; emitting the reachable interim terminal awaiting-pack. No sampling is run and nothing is promoted.",
    "pack-committed-awaiting-gates" =>
        "Consolidated n=4 launch (plan item 9): the A4+A5 Stage-A pre-registration pack is committed and its SHA-256 matches the pinned constant, so the refuse-to-run gate is satisfied. The phase now awaits the remaining hard gates (phase455 terminal, O4 memo or user-renewed risk acceptance) before the ~6-16 h production HMC, which is deliberately not yet implemented. Per-site correlator storage is MANDATORY (pinned in the pack). Nothing is measured or promoted; promotedPhysicalMassClaimCount remains 0.",
    _ =>
        "Consolidated n=4 launch (plan item 9): the committed pre-registration pack hash does NOT match the pinned constant. This is the mandatory refuse-to-run condition — production must never sample against an unpinned or altered pack. Fail-closed BLOCK; regenerate/repin the pack before proceeding.",
};

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, interimTerminal, PlanSection,
        "pack refuse-to-run gate; zero physics compute; standing claim boundary; production HMC not implemented")))).ToLowerInvariant();

Directory.CreateDirectory(DefaultOutputDir);
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase456-consolidated-n4-launch",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    interimTerminal,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    phase456GateGreen,
    claimBoundaryHeld,
    zeroPhysicsCompute = true,
    productionSamplingImplemented = false,
    awaitingProductionImplementation,
    packGate = new
    {
        packDir = PackDir,
        packFiles,
        packPresent,
        missingPackFiles,
        pinnedPackSha256 = PinnedPackSha256,
        computedPackSha256,
        packHashMatches,
        refuseToRun,
        refuseToRunGate = "MANDATORY",
        hashAlgorithm = "SHA-256 over concatenation of packFiles in listed order (byte-exact contents)",
    },
    mandatoryGates = new
    {
        hashRefuseToRunGate = "MANDATORY",
        perSiteCorrelatorStorageFlag = "MANDATORY",
        perSiteCorrelatorStorageEnvFlag = "PHASE456_STORE_PER_SITE_CORRELATORS",
    },
    packContents = new
    {
        a4 = "Stage-A automorphism/irrep enumeration (n=4): realized point group order 48; rest-frame little group H_s = S_3 (order 6, irreps {1,1,2}); parity inadmissible (no realized spatial inversion) => 0-+-like banned; exact-rational projector kernels for the identity-irrep 2x2 GEVP + the realized non-identity (sign) channel; k_min=2*pi/4 dispersion spec",
        a5 = "Stage-A Gaussian-domination attempt: NOT provable at Stage A; obstruction recorded (compositeness + reflection positivity of the Kuhn-simplicial action); verdict a5-stage-a-gaussian-domination-not-provable-obstruction-recorded",
        manifest = "threshold max(family-wise>=3sigma, per-row 3sigma) [bare 99th percentile forbidden]; power gate; mechanical AIC window aggregation (no analyst-chosen window); MV-456 fallback (scoping device, not a gate bypass)",
    },
    limbConsumed = "L6 (closes at probed-volume scope on T1 with a Gaussian-null Binder column); L8 at two-volume strength",
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256Contract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    recordedBoundary = new
    {
        physicistReviewPending,
        awaitingProductionImplementation,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The pre-registration pack fixes the A4 kernels and A5 verdict and the analysis thresholds BEFORE any physics number is computed; it measures nothing and promotes nothing.",
        "The interim terminal is a program-state marker, not a scientific verdict; no measurement, elimination, or promotion is claimed.",
        "physicistReviewPending = true is carried explicitly; no contract field is filled and nothing is promoted (promotedPhysicalMassClaimCount remains 0).",
        "A4 group-theory content (irreps, projector coefficients) is exact rational structure of the reduced Spin(4)-slice lattice symmetry; it is not a physical spectrum.",
    },
    decision,
    runtimeSeconds,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "consolidated_n4_launch.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "consolidated_n4_launch_summary.json"), JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"interimTerminal={interimTerminal} packPresent={packPresent} packHashMatches={packHashMatches} refuseToRun={refuseToRun}");
Console.WriteLine($"pinnedPackSha256={PinnedPackSha256}");
Console.WriteLine($"computedPackSha256={computedPackSha256 ?? "(pack absent)"}");
Console.WriteLine($"phase456GateGreen={phase456GateGreen} claimBoundaryHeld={claimBoundaryHeld}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");
