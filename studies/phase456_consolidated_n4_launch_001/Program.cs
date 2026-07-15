using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase456: Consolidated n=4 Launch — PACK-GATED (WAVE2_ACTION_PLAN_2026-07-12 item 9).
//
// The A4 + A5 Stage-A PRE-REGISTRATION PACK now lives, hash-pinned, in this phase's
// preregistration/ directory. This program is the pack REFUSE-TO-RUN GATE and the standing
// claim boundary and deterministic consumer of the separately launched, environment-clean
// production HMC. The generator path NEVER launches the long sampler. What this program does:
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
// MANDATORY FRAMING. Production measurements remain workbench-relative lattice quantities;
// nothing is filled or promoted;
// promotedPhysicalMassClaimCount remains 0. physicistReviewPending is carried explicitly.
// The pack is a pre-registration artifact only — no verdict is claimed by this program.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "consolidated-n4-launch";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 9";
const string DefaultOutputDir = "studies/phase456_consolidated_n4_launch_001/output";
const string PackDir = "studies/phase456_consolidated_n4_launch_001/preregistration";
const string AuthorizationPath = "studies/phase456_consolidated_n4_launch_001/production_authorization.json";
const string DefaultProductionSummaryPath = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4/scalar_channel_spectroscopy_probe_summary.json";
const string TerminalPrefix = "consolidated-n4-launch-";
const string PinnedAuthorizationSha256 = "b097b0504c6eafdf79523ef7913c69ab37fe086411334fea7e91c9fc9be70642";

bool analysisInputOverride = args.Length == 2 && args[0] == "--analysis-input";
if (args.Length != 0 && !analysisInputOverride)
    throw new InvalidOperationException("usage: Phase456ConsolidatedN4Launch [--analysis-input <test-artifact-path>]");
string productionSummaryPath = analysisInputOverride ? args[1] : DefaultProductionSummaryPath;

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
const bool productionSamplingImplemented = true;
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
const int promotedPhysicalMassClaimCount = 0;

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

// --- written-risk-renewal gate --------------------------------------------------------
bool authorizationPresent = File.Exists(AuthorizationPath);
string? computedAuthorizationSha256 = authorizationPresent
    ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AuthorizationPath))).ToLowerInvariant()
    : null;
bool authorizationHashMatches = computedAuthorizationSha256 == PinnedAuthorizationSha256;
bool explicitUserRiskRenewalValid = false;
if (authorizationPresent && authorizationHashMatches)
{
    using var authorization = JsonDocument.Parse(File.ReadAllText(AuthorizationPath));
    var a = authorization.RootElement;
    explicitUserRiskRenewalValid =
        a.GetProperty("authorizationKind").GetString() == "explicit-user-renewed-risk-acceptance" &&
        a.GetProperty("authority").GetString() == "user" &&
        a.GetProperty("authorizesPhase456ProductionHmc").GetBoolean() &&
        a.GetProperty("committedPreregistrationPackRequired").GetBoolean() &&
        a.GetProperty("committedDefaultConfigurationRequired").GetBoolean() &&
        !a.GetProperty("o4PhysicistRuling").GetBoolean() &&
        a.GetProperty("physicistReviewPending").GetBoolean() &&
        a.GetProperty("scaleIsWorkbenchRelativeCandidateOnly").GetBoolean() &&
        a.GetProperty("latticeUnitsOnly").GetBoolean() &&
        a.GetProperty("noGevPromotion").GetBoolean() &&
        a.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
}

// The long sampler is launched manually and tracked outside the generator. This consumer
// reads an artifact only after the atomic final write exists; a partial run has no summary.
bool productionArtifactPresent = File.Exists(productionSummaryPath);
Phase456ProductionAnalysis? productionAnalysis = null;
if (productionArtifactPresent && packHashMatches && explicitUserRiskRenewalValid)
{
    using var production = JsonDocument.Parse(File.ReadAllText(productionSummaryPath));
    productionAnalysis = Phase456ProductionAnalyzer.Evaluate(production.RootElement);
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
else if (packHashMatches && productionAnalysis is not null)
{
    interimTerminal = productionAnalysis.VerdictKind;
    verdictKind = productionAnalysis.VerdictKind;
    refuseToRun = false;
}
else if (packHashMatches && productionArtifactPresent && !explicitUserRiskRenewalValid)
{
    interimTerminal = "production-artifact-refused-authorization-invalid";
    verdictKind = "production-artifact-refused-authorization-invalid";
    refuseToRun = true;
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

// The phase integrity gate is green when the pack/authorization/claim firewall either records
// an input-valid analysis or records the exact fail-closed invalid-analysis terminal. A red
// scientific/control outcome never turns this infrastructure gate into a promotion path.
bool productionArtifactConsumed = productionAnalysis is not null;
bool awaitingProductionRun = packHashMatches && explicitUserRiskRenewalValid && !productionArtifactPresent;
bool productionAnalysisRecordedFailClosed = productionAnalysis is null || productionAnalysis.InputShapeValid ||
    (productionAnalysis.VerdictKind == "production-analysis-invalid" &&
     productionAnalysis.ProductionDefaultsVerified && productionAnalysis.PackHashVerified &&
     productionAnalysis.PerSiteStorageVerified && productionAnalysis.ExactGaussianControlsVerified &&
     productionAnalysis.RowCount == 5 && productionAnalysis.Rows.Length == 5 &&
     productionAnalysis.Rows.Any(row => row.Terminal == "invalid") &&
     productionAnalysis.InputErrors.Length > 0 && !productionAnalysis.MandatoryN5Escalation &&
     !productionAnalysis.G3Motivated && productionAnalysis.FamilyWiseSigmaThreshold is null &&
     productionAnalysis.AppliedSigmaThreshold is null);
bool phase456GateGreen = claimBoundaryHeld && !refuseToRun && productionAnalysisRecordedFailClosed;
string terminalStatus = TerminalPrefix + interimTerminal;

string decision = interimTerminal switch
{
    "awaiting-pack" =>
        "Consolidated n=4 launch (plan item 9): the pre-registration pack is not present; emitting the reachable interim terminal awaiting-pack. No sampling is run and nothing is promoted.",
    "pack-committed-awaiting-gates" =>
        explicitUserRiskRenewalValid
            ? "Consolidated n=4 launch (plan item 9): the pack hash and explicit user-renewed risk acceptance are valid. The separately tracked environment-clean production HMC is authorized and either running or awaiting its atomic result artifact. The generator never launches it. Nothing is promoted; promotedPhysicalMassClaimCount remains 0."
            : "Consolidated n=4 launch (plan item 9): the A4+A5 Stage-A pack hash is valid, but neither an O4 memo nor a valid explicit user risk renewal is available. Production remains forbidden. Nothing is promoted.",
    _ =>
        productionAnalysis is not null ? productionAnalysis.Decision
        : "Consolidated n=4 launch (plan item 9): the committed pre-registration pack hash does NOT match the pinned constant. This is the mandatory refuse-to-run condition — production must never sample against an unpinned or altered pack. Fail-closed BLOCK; regenerate/repin the pack before proceeding.",
};

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, interimTerminal, PlanSection,
        "pack refuse-to-run gate; environment-clean production consumer; standing claim boundary; lattice units only; zero physical-mass claims")))).ToLowerInvariant();

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
    zeroPhysicsCompute = !productionArtifactConsumed,
    productionSamplingImplemented,
    awaitingProductionImplementation = false,
    awaitingProductionRun,
    productionArtifactPresent,
    productionArtifactConsumed,
    productionAnalysisRecordedFailClosed,
    productionSummaryPath,
    analysisInputOverride,
    productionAnalysis,
    productionAuthorization = new
    {
        authorizationPath = AuthorizationPath,
        authorizationPresent,
        pinnedAuthorizationSha256 = PinnedAuthorizationSha256,
        computedAuthorizationSha256,
        authorizationHashMatches,
        explicitUserRiskRenewalValid,
        o4PhysicistRuling = false,
        physicistReviewPending,
    },
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
    promotedPhysicalMassClaimCount,
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
        awaitingProductionImplementation = false,
        explicitUserRiskRenewalValid,
        o4PhysicistRuling = false,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The pre-registration pack fixed the A4 kernels, A5 verdict, thresholds, power rule, and mechanical window aggregation before the production measurements; the consumer never changes that pack.",
        "Every reported a*m, dispersion residual, Binder cumulant, and susceptibility is workbench-relative lattice structure data only; none is a physical mass or GeV quantity.",
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
Console.WriteLine($"authorizationValid={explicitUserRiskRenewalValid} productionArtifactPresent={productionArtifactPresent} consumed={productionArtifactConsumed}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");
