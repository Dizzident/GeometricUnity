using System.Diagnostics;
using System.Text.Json;

// Phase477 implements machine-only O4 readiness. It validates exact coverage,
// the inert human-memo contract, and synthetic dependency overturns. It never
// reads, writes, chooses, or infers a real physicist ruling.
var stopwatch = Stopwatch.StartNew();
const string slug = "o4_adjudication_infrastructure";
const string terminal = "infrastructure-ready-pending-human-ruling";
const string outputDir = "studies/phase477_o4_adjudication_infrastructure_001/output";

var start = new ProcessStartInfo
{
    FileName = "node",
    WorkingDirectory = Directory.GetCurrentDirectory(),
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
};
start.ArgumentList.Add("scripts/o4_register/infrastructure_audit.js");
using var process = Process.Start(start) ?? throw new InvalidOperationException("Failed to start O4 infrastructure audit.");
string stdout = process.StandardOutput.ReadToEnd();
string stderr = process.StandardError.ReadToEnd();
process.WaitForExit();
if (process.ExitCode != 0)
    throw new InvalidOperationException($"O4 infrastructure audit failed closed (exit {process.ExitCode}): {stderr}{stdout}");

using var auditDocument = JsonDocument.Parse(stdout);
JsonElement audit = auditDocument.RootElement.Clone();
bool infrastructureReady = audit.GetProperty("infrastructureReady").GetBoolean();
bool exactCoveragePassed = audit.GetProperty("exactCoveragePassed").GetBoolean();
int pendingArtifactCount = audit.GetProperty("recursivePendingArtifactCount").GetInt32();
int mandatoryRulingIdCount = audit.GetProperty("mandatoryRulingIdCount").GetInt32();
JsonElement overturn = audit.GetProperty("syntheticOverturn");
bool overturnPassed = overturn.GetProperty("syntheticOverturnBatteryPassed").GetBoolean();
if (!infrastructureReady || !exactCoveragePassed || !overturnPassed)
    throw new InvalidOperationException("Phase477 readiness conjunction is not green.");

var result = new
{
    phaseId = "phase477-o4-adjudication-infrastructure",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"o4-adjudication-infrastructure-{terminal}",
    interimTerminal = terminal,
    verdictKind = terminal,
    applicationSubjectKind = "o4-adjudication-infrastructure",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A4",
    waveOrder = 1,
    skeletonBuilt = true,
    zeroPhysicsCompute = true,
    awaitingImplementation = false,
    infrastructureReady,
    exactCoveragePassed,
    recursivePendingArtifactCount = pendingArtifactCount,
    coverageEntryCount = audit.GetProperty("coverageEntryCount").GetInt32(),
    unmappedPendingArtifactCount = audit.GetProperty("unmappedPendingArtifactCount").GetInt32(),
    ambiguousCoverageArtifactCount = audit.GetProperty("ambiguousCoverageArtifactCount").GetInt32(),
    dispositionCounts = audit.GetProperty("dispositionCounts"),
    mandatoryRulingIdCount,
    coverageSchemaTemplateRulingIdsEqual = audit.GetProperty("coverageSchemaTemplateRulingIdsEqual").GetBoolean(),
    memoTemplateInertAndProductionInvalid = audit.GetProperty("memoTemplateInertAndProductionInvalid").GetBoolean(),
    humanRulingReadOrAuthored = audit.GetProperty("humanRulingReadOrAuthored").GetBoolean(),
    coverageContractSha256 = audit.GetProperty("coverageContractSha256").GetString(),
    dependencyMapSha256 = audit.GetProperty("dependencyMapSha256").GetString(),
    memoSchemaSha256 = audit.GetProperty("memoSchemaSha256").GetString(),
    memoTemplateSha256 = audit.GetProperty("memoTemplateSha256").GetString(),
    syntheticOverturn = overturn,
    coveredArtifacts = audit.GetProperty("coveredArtifacts"),
    executionPriorityDependencies = Array.Empty<string>(),
    rulingAuthoredOrInferred = false,
    physicistReviewPending = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    scaleIsWorkbenchRelativeCandidateOnly = true,
    physicalCouplingProvided = false,
    routeProvidesPhysicalEffectiveActionHessian = false,
    routeProvidesVevOrSourceScaleLineage = false,
    routeProvidesPoleExtractionAndGeVNormalization = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = "Exact O4 coverage, an inert 13-item human-memo schema/template, and the 94-edge synthetic-overturn protocol are machine-green. This is infrastructure readiness only: no human ruling exists or is inferred, every review-pending flag remains pending, and no downstream scientific terminal changes.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, $"{slug}.json"), json);
File.WriteAllText(Path.Combine(outputDir, $"{slug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"coverage={pendingArtifactCount}/{pendingArtifactCount} rulings={mandatoryRulingIdCount} overturnEdges={overturn.GetProperty("exercisedEdgeCount").GetInt32()}/{overturn.GetProperty("declaredEdgeCount").GetInt32()}");
Console.WriteLine("physicistReviewPending=True promotedPhysicalMassClaimCount=0");
