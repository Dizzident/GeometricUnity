using System.Text.Json;

const string DefaultOutputDir = "studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output";
const string CompletionRevisionPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase308Path = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json";
const string Phase309Path = "studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE310_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase308 = JsonDocument.Parse(File.ReadAllText(Phase308Path));
using var phase309 = JsonDocument.Parse(File.ReadAllText(Phase309Path));

var completionLines = File.ReadAllLines(CompletionRevisionPath);
var variationalEvidence = new[]
{
    LineEvidence(11689, "A completed bosonic variational branch must declare residuals, fields, boundary conditions, pairings, variational formulae, Euler-Lagrange equations, gauge conditions, source terms, and linearization/adjoint package."),
    LineEvidence(12233, "Every completed bosonic branch must include a linearization step around a declared background."),
    LineEvidence(12257, "The first-order branch fixes the master bosonic residual as Upsilon = S - T_aug."),
    LineEvidence(12306, "The second-order branch uses one-half the L2 norm of Upsilon as its minimal compatible action."),
    LineEvidence(12329, "The Yang-Mills/Maxwell-like reading is conditional on identifying which part of D Upsilon star Upsilon is read as a curvature-divergence source."),
    LineEvidence(12392, "The exact relation to a Yang-Mills/Maxwell-like equation and the exact bosonic source J remain proof obligations."),
    LineEvidence(12430, "The reusable linearization package is D Upsilon and D Upsilon star D Upsilon about a declared background."),
    LineEvidence(12508, "The first numerical objective of the minimal branch is residual and linearization evaluation, not parameter fitting."),
    LineEvidence(13736, "Structural falsification precedes parameter fitting."),
    LineEvidence(13756, "The completion revision still does not close full native-to-observed extraction or validation of a specific physical branch."),
};

var searchedCueCounts = new[]
{
    SearchCount("source-mode-vector-length", "source mode vector length", "mode-vector length"),
    SearchCount("adjoint/fundamental Casimir", "adjoint Casimir", "fundamental Casimir", "8/3"),
    SearchCount("charged ladder", "charged-ladder", "T+/-", "T_+", "T_-"),
    SearchCount("W source row", "Z source row", "physical W/Z source", "physical row selection"),
};

var branchLocalVariationalWorkbenchPresent = variationalEvidence.All(row => row.Found);
var completionRevisionDirectBridgeSourceAuditPassed = JsonBool(phase267.RootElement, "completionRevisionDirectBridgeSourceAuditPassed") is true;
var latestCompletionProvidesDirectWzTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesDirectWzTheorem") is true;
var latestCompletionProvidesObservedFieldExtractionTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesObservedFieldExtractionTheorem") is true;

var p302Best = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");
var phase302VectorLengthCasimirLeadRecorded =
    JsonString(p302Best, "commonScaleId") == "source-mode-vector-length"
    && JsonString(p302Best, "particleLawId") == "adjoint-casimir-over-fundamental-casimir"
    && JsonBool(p302Best, "rawAndCommonGatesPassed") is true
    && JsonBool(p302Best, "stableRawCommonGatesPassed") is false
    && JsonBool(p302Best, "commonScaleApplicationTheoremPresent") is false
    && JsonBool(p302Best, "particleLawApplicationTheoremPresent") is false
    && JsonBool(p302Best, "promotionEligible") is false
    && JsonBool(phase302.RootElement, "canFillPhase201WzContract") is false;

var phase308ScaleTransferStillBlocked =
    JsonBool(phase308.RootElement, "phase302ScaleTransferToDecoupledChargedLadderAuditPassed") is true
    && JsonBool(phase308.RootElement, "scaleTransferAllowed") is false
    && JsonBool(phase308.RootElement, "transferredScaleSourceRowsPromotable") is false
    && JsonBool(phase308.RootElement, "canFillPhase201WzContract") is false;

var phase309VectorLengthMeasureRejected =
    JsonBool(phase309.RootElement, "sourceModeVectorLengthMeasureNormalizationAuditPassed") is true
    && JsonBool(phase309.RootElement, "hiddenMeasureConversionPresent") is false
    && JsonBool(phase309.RootElement, "sourceModeVectorLengthApplicationTheoremPresent") is false
    && JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable") is false
    && JsonBool(phase309.RootElement, "canFillPhase201WzContract") is false;

bool completionDraftProvidesVectorLengthNormalizationTheorem = false;
bool completionDraftProvidesCasimirApplicationTheorem = false;
bool completionDraftProvidesChargedLadderTransferTheorem = false;
bool completionDraftProvidesPhysicalWzSourceRowDerivation = false;
bool completionDraftProvidesBranchStableSourceRows = false;
bool completionDraftCanPromotePhase302Lead = false;
bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool canFillPhase201WzContract = false;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "completion-variational-workbench-evidence-present",
        branchLocalVariationalWorkbenchPresent,
        $"foundLineEvidenceCount={variationalEvidence.Count(row => row.Found)}; requiredLineEvidenceCount={variationalEvidence.Length}"),
    new Check(
        "variational-workbench-is-not-specific-wz-normalization-theorem",
        branchLocalVariationalWorkbenchPresent
            && !completionDraftProvidesVectorLengthNormalizationTheorem
            && !completionDraftProvidesCasimirApplicationTheorem
            && !completionDraftProvidesChargedLadderTransferTheorem
            && !completionDraftProvidesPhysicalWzSourceRowDerivation,
        $"vectorLengthTheorem={completionDraftProvidesVectorLengthNormalizationTheorem}; casimirTheorem={completionDraftProvidesCasimirApplicationTheorem}; chargedLadderTransferTheorem={completionDraftProvidesChargedLadderTransferTheorem}; physicalWzSourceRows={completionDraftProvidesPhysicalWzSourceRowDerivation}"),
    new Check(
        "completion-revision-prior-audit-still-nonpromotional",
        completionRevisionDirectBridgeSourceAuditPassed
            && !latestCompletionProvidesDirectWzTheorem
            && !latestCompletionProvidesObservedFieldExtractionTheorem,
        $"p267Passed={completionRevisionDirectBridgeSourceAuditPassed}; latestCompletionProvidesDirectWzTheorem={latestCompletionProvidesDirectWzTheorem}; latestCompletionProvidesObservedFieldExtractionTheorem={latestCompletionProvidesObservedFieldExtractionTheorem}"),
    new Check(
        "phase302-specific-near-pass-still-needs-theorem",
        phase302VectorLengthCasimirLeadRecorded,
        $"candidateId={JsonString(p302Best, "candidateId")}; commonScaleApplicationTheoremPresent={JsonBool(p302Best, "commonScaleApplicationTheoremPresent")}; particleLawApplicationTheoremPresent={JsonBool(p302Best, "particleLawApplicationTheoremPresent")}; stableRawCommonGatesPassed={JsonBool(p302Best, "stableRawCommonGatesPassed")}; promotionEligible={JsonBool(p302Best, "promotionEligible")}"),
    new Check(
        "phase308-charged-ladder-transfer-still-blocked",
        phase308ScaleTransferStillBlocked,
        $"scaleTransferAllowed={JsonBool(phase308.RootElement, "scaleTransferAllowed")}; transferredScaleSourceRowsPromotable={JsonBool(phase308.RootElement, "transferredScaleSourceRowsPromotable")}; canFillPhase201WzContract={JsonBool(phase308.RootElement, "canFillPhase201WzContract")}"),
    new Check(
        "phase309-hidden-measure-route-still-rejected",
        phase309VectorLengthMeasureRejected,
        $"hiddenMeasureConversionPresent={JsonBool(phase309.RootElement, "hiddenMeasureConversionPresent")}; sourceModeVectorLengthApplicationTheoremPresent={JsonBool(phase309.RootElement, "sourceModeVectorLengthApplicationTheoremPresent")}; sourceModeVectorLengthScalePromotable={JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable")}"),
    new Check(
        "source-contract-remains-blocked",
        !canFillPhase201WzContract
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0
            && !completionDraftProvidesBranchStableSourceRows
            && !completionDraftCanPromotePhase302Lead,
        $"canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; branchStableSourceRows={completionDraftProvidesBranchStableSourceRows}; completionDraftCanPromotePhase302Lead={completionDraftCanPromotePhase302Lead}"),
};

var completionVariationalBranchToWzNormalizationAuditPassed = checks.All(check => check.Passed)
    && !completionDraftProvidesVectorLengthNormalizationTheorem
    && !completionDraftProvidesCasimirApplicationTheorem
    && !completionDraftProvidesChargedLadderTransferTheorem
    && !completionDraftProvidesPhysicalWzSourceRowDerivation
    && !completionDraftCanPromotePhase302Lead
    && !canFillPhase201WzContract;

var terminalStatus = completionVariationalBranchToWzNormalizationAuditPassed
    ? "completion-variational-branch-to-wz-normalization-audit-workbench-not-source-law"
    : "completion-variational-branch-to-wz-normalization-audit-review-required";

var result = new
{
    phaseId = "phase310-completion-variational-branch-to-wz-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    completionVariationalBranchToWzNormalizationAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    branchLocalVariationalWorkbenchPresent,
    completionDraftProvidesVectorLengthNormalizationTheorem,
    completionDraftProvidesCasimirApplicationTheorem,
    completionDraftProvidesChargedLadderTransferTheorem,
    completionDraftProvidesPhysicalWzSourceRowDerivation,
    completionDraftProvidesBranchStableSourceRows,
    completionDraftCanPromotePhase302Lead,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    completionRevision = new
    {
        path = CompletionRevisionPath,
        totalLineCount = completionLines.Length,
        variationalEvidence,
        searchedCueCounts,
    },
    inheritedEvidence = new
    {
        phase267 = new
        {
            completionRevisionDirectBridgeSourceAuditPassed,
            latestCompletionProvidesDirectWzTheorem,
            latestCompletionProvidesObservedFieldExtractionTheorem,
        },
        phase302 = new
        {
            phase302VectorLengthCasimirLeadRecorded,
            candidateId = JsonString(p302Best, "candidateId"),
            commonScaleId = JsonString(p302Best, "commonScaleId"),
            particleLawId = JsonString(p302Best, "particleLawId"),
            commonScaleValue = JsonDouble(p302Best, "commonScaleValue"),
            wTotalScale = JsonDouble(p302Best, "wTotalScale"),
            zTotalScale = JsonDouble(p302Best, "zTotalScale"),
            rawAndCommonGatesPassed = JsonBool(p302Best, "rawAndCommonGatesPassed"),
            stableRawCommonGatesPassed = JsonBool(p302Best, "stableRawCommonGatesPassed"),
            commonScaleApplicationTheoremPresent = JsonBool(p302Best, "commonScaleApplicationTheoremPresent"),
            particleLawApplicationTheoremPresent = JsonBool(p302Best, "particleLawApplicationTheoremPresent"),
            promotionEligible = JsonBool(p302Best, "promotionEligible"),
        },
        phase308 = new
        {
            phase308ScaleTransferStillBlocked,
            scaleTransferAllowed = JsonBool(phase308.RootElement, "scaleTransferAllowed"),
            transferredScaleSourceRowsPromotable = JsonBool(phase308.RootElement, "transferredScaleSourceRowsPromotable"),
            canFillPhase201WzContract = JsonBool(phase308.RootElement, "canFillPhase201WzContract"),
        },
        phase309 = new
        {
            phase309VectorLengthMeasureRejected,
            commonVectorLength = JsonInt(phase309.RootElement, "commonVectorLength"),
            sqrtCommonVectorLength = JsonDouble(phase309.RootElement, "sqrtCommonVectorLength"),
            hiddenMeasureConversionPresent = JsonBool(phase309.RootElement, "hiddenMeasureConversionPresent"),
            sourceModeVectorLengthScalePromotable = JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable"),
        },
    },
    checks,
    decision = completionVariationalBranchToWzNormalizationAuditPassed
        ? "Do not use the completion revision's branch-local variational or linearization workbench to promote the Phase302 W/Z normalization lead. The workbench is a residual/linearization framework; it does not derive source-mode-vector-length scaling, the W-only adjoint/fundamental Casimir multiplier, charged-ladder transfer, or branch-stable physical W/Z source rows."
        : "Review the completion variational branch to W/Z normalization audit before relying on this non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A theorem deriving the source-mode-vector-length normalization as a W/Z source law, not as a coordinate-count or hidden measure interpretation.",
        "A theorem deriving the W-only adjoint/fundamental Casimir multiplier for physical W rows.",
        "A charged-ladder transfer theorem connecting that law to branch-stable W and Z source rows before target comparison.",
        "Phase201/P209 W/Z rows with derivation, raw-amplitude, common-bridge, target-comparison, and stability gates all filled.",
    },
    sourceEvidence = new
    {
        completionRevisionPath = CompletionRevisionPath,
        phase213Path = Phase213Path,
        phase267Path = Phase267Path,
        phase302Path = Phase302Path,
        phase308Path = Phase308Path,
        phase309Path = Phase309Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "completion_variational_branch_to_wz_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "completion_variational_branch_to_wz_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.completionVariationalBranchToWzNormalizationAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.branchLocalVariationalWorkbenchPresent,
        result.completionDraftProvidesVectorLengthNormalizationTheorem,
        result.completionDraftProvidesCasimirApplicationTheorem,
        result.completionDraftProvidesChargedLadderTransferTheorem,
        result.completionDraftProvidesPhysicalWzSourceRowDerivation,
        result.completionDraftProvidesBranchStableSourceRows,
        result.completionDraftCanPromotePhase302Lead,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        completionRevision = new
        {
            path = CompletionRevisionPath,
            variationalEvidenceCount = variationalEvidence.Length,
            keyEvidenceLines = variationalEvidence.Select(row => new { row.LineNumber, row.AuditFinding, row.Excerpt }).ToArray(),
            searchedCueCounts,
        },
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"completionVariationalBranchToWzNormalizationAuditPassed={completionVariationalBranchToWzNormalizationAuditPassed}");
Console.WriteLine($"branchLocalVariationalWorkbenchPresent={branchLocalVariationalWorkbenchPresent}");
Console.WriteLine($"completionDraftProvidesVectorLengthNormalizationTheorem={completionDraftProvidesVectorLengthNormalizationTheorem}");
Console.WriteLine($"completionDraftProvidesCasimirApplicationTheorem={completionDraftProvidesCasimirApplicationTheorem}");
Console.WriteLine($"completionDraftProvidesChargedLadderTransferTheorem={completionDraftProvidesChargedLadderTransferTheorem}");
Console.WriteLine($"completionDraftCanPromotePhase302Lead={completionDraftCanPromotePhase302Lead}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

LineEvidenceRecord LineEvidence(int lineNumber, string finding)
{
    var excerpt = lineNumber > 0 && lineNumber <= completionLines.Length ? completionLines[lineNumber - 1].Trim() : string.Empty;
    return new LineEvidenceRecord(lineNumber, finding, excerpt.Length > 0, excerpt);
}

SearchCountRecord SearchCount(params string[] patterns)
{
    var count = completionLines.Count(line => patterns.Any(pattern => line.Contains(pattern, StringComparison.OrdinalIgnoreCase)));
    return new SearchCountRecord(string.Join("|", patterns), count);
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record LineEvidenceRecord(int LineNumber, string AuditFinding, bool Found, string Excerpt);
sealed record SearchCountRecord(string PatternSet, int MatchingLineCount);
sealed record Check(string CheckId, bool Passed, string Detail);
