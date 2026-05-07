using System.Globalization;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase152_boson_blocker_resolution_plan_001/output";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE152_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));

var failedAttempts = ReadRows(phase151.RootElement, "failedAttempts");
var blockedRows = ReadRows(phase151.RootElement, "blockedRows");
var openRows = failedAttempts.Concat(blockedRows).ToArray();
var workstreams = BuildWorkstreams(openRows);
var rerunGates = new[]
{
    "source artifact exists and is target-independent",
    "physical observable mapping is explicit and validated",
    "scale or benchmark contract is not fitted to the target row being predicted",
    "branch, refinement, environment, representation, and coupling sidecars pass",
    "Phase151 row has promotionAllowed=true and passed=true before it is reported as validated",
};

string terminalStatus = openRows.Length == 0
    ? "boson-blocker-resolution-plan-no-open-rows"
    : "boson-blocker-resolution-plan-ready";

var result = new
{
    phaseId = "phase152-boson-blocker-resolution-plan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    phase151Path = Phase151Path,
    phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
    currentValidatedPredictionCount = JsonInt(phase151.RootElement, "validatedPredictionCount"),
    openPredictionRowCount = openRows.Length,
    failedAttemptCount = failedAttempts.Count,
    blockedRowCount = blockedRows.Count,
    recommendedExecutionOrder = workstreams.Select(workstream => workstream.WorkstreamId).ToArray(),
    workstreams,
    rerunGates,
    nextPhaseRecommendation = new
    {
        phaseId = "phase153-wz-absolute-scale-closure",
        reason = "W/Z absolute mass closure reuses the existing W/Z identity path and can unblock two failed rows before introducing new Higgs or color-sector source families.",
        expectedOutputs = new[]
        {
            "target-independent mass-energy scale derivation",
            "validated physical mappings for physical-w-boson-mass-gev and physical-z-boson-mass-gev",
            "rerun Phase151 with W and Z absolute mass rows eligible for promotion",
        },
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "boson_blocker_resolution_plan.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_blocker_resolution_plan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase151Status,
        result.currentValidatedPredictionCount,
        result.openPredictionRowCount,
        result.failedAttemptCount,
        result.blockedRowCount,
        result.recommendedExecutionOrder,
        result.nextPhaseRecommendation,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_blocker_resolution_plan.md"),
    BuildMarkdown(terminalStatus, workstreams, rerunGates));

Console.WriteLine(terminalStatus);
Console.WriteLine($"openPredictionRowCount={openRows.Length}");
Console.WriteLine($"workstreamCount={workstreams.Length}");
Console.WriteLine("nextPhase=phase153-wz-absolute-scale-closure");

static IReadOnlyList<PredictionRow> ReadRows(JsonElement root, string propertyName) =>
    root.TryGetProperty(propertyName, out var rows) && rows.ValueKind == JsonValueKind.Array
        ? rows.EnumerateArray()
            .Select(row => new PredictionRow(
                ParticleId: RequiredString(row, "particleId"),
                ObservableId: RequiredString(row, "observableId"),
                Status: RequiredString(row, "status"),
                PredictedValue: JsonDouble(row, "predictedValue"),
                PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
                TargetValue: JsonDouble(row, "targetValue"),
                TargetUncertainty: JsonDouble(row, "targetUncertainty"),
                Unit: JsonString(row, "unit"),
                PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
                ClosureRequirements: StringArray(row, "closureRequirements")))
            .ToArray()
        : Array.Empty<PredictionRow>();

static Workstream[] BuildWorkstreams(IReadOnlyList<PredictionRow> openRows)
{
    var rowsByParticle = openRows.ToDictionary(row => row.ParticleId, StringComparer.Ordinal);
    var workstreams = new List<Workstream>();

    var wzRows = openRows
        .Where(row => row.ParticleId is "w-boson" or "z-boson")
        .ToArray();
    if (wzRows.Length > 0)
    {
        workstreams.Add(new Workstream(
            WorkstreamId: "wz-absolute-scale-closure",
            Priority: 1,
            Objective: "Convert the existing W/Z ratio path into target-independent absolute W and Z mass predictions.",
            OpenRows: wzRows,
            BlockingEvidence: new[]
            {
                "mass-energy scale calibration derived from an independent internal observable or external unit convention, not from W or Z target masses",
                "validated W internal mode mapping to physical-w-boson-mass-gev",
                "validated Z internal mode mapping to physical-z-boson-mass-gev",
                "W/Z absolute-mass sidecars for branch, refinement, environment, representation, and coupling stability",
            },
            AcceptanceGates: new[]
            {
                "W and Z rows leave failed-comparison-attempt-not-promoted status",
                "pulls are computed against held-out targets after scale derivation is frozen",
                "Phase151 promotes W and Z only if promotionAllowed=true and passed=true",
            }));
    }

    AddSingleParticleWorkstream(
        workstreams,
        rowsByParticle,
        "higgs",
        2,
        "higgs-scalar-sector-source",
        "Create the scalar-sector source/operator and target-independent identity path needed for a Higgs mass prediction.",
        new[]
        {
            "scalar-sector source/operator producing a Higgs-like mode",
            "Higgs identity features independent of the 125 GeV target",
            "scalar branch, refinement, environment, representation, and coupling sidecars",
            "absolute mass mapping and scale contract compatible with the W/Z closure path",
        });

    AddSingleParticleWorkstream(
        workstreams,
        rowsByParticle,
        "photon",
        3,
        "photon-u1-masslessness-contract",
        "Define and validate the massless U(1) mode contract required for photon prediction.",
        new[]
        {
            "physical target record or upper-limit contract for photon masslessness",
            "target-independent U(1) gauge identity rule",
            "computed masslessness observable and physical mapping",
            "sidecars proving mode stability across branch, refinement, and environment choices",
        });

    AddSingleParticleWorkstream(
        workstreams,
        rowsByParticle,
        "gluon",
        4,
        "gluon-color-sector-contract",
        "Define the confinement-aware color-sector identity and benchmark contract needed for gluon prediction.",
        new[]
        {
            "physical target or benchmark contract appropriate for confined gluon masslessness",
            "target-independent color-octet identity rule",
            "color gauge-sector observables and validated physical mapping",
            "sidecars for color representation content, gauge leakage, and environment stability",
        });

    workstreams.Add(new Workstream(
        WorkstreamId: "validated-generator-rerun-and-promotion",
        Priority: 5,
        Objective: "Rerun the end-to-end generator only after the upstream evidence gates are satisfied.",
        OpenRows: openRows,
        BlockingEvidence: new[]
        {
            "updated Phase149 contracts with all relevant rows ready",
            "updated Phase150 prerequisite execution with no unsupported row promoted by assumption",
            "updated Phase151 validated prediction package",
        },
        AcceptanceGates: new[]
        {
            "all newly predicted rows pass validation with promotionAllowed=true",
            "failed attempts remain visible and unpromoted if any row misses validation",
            "Markdown and JSON prediction artifacts agree on row status, target, prediction, and residual",
        }));

    return workstreams.OrderBy(workstream => workstream.Priority).ToArray();
}

static void AddSingleParticleWorkstream(
    ICollection<Workstream> workstreams,
    IReadOnlyDictionary<string, PredictionRow> rowsByParticle,
    string particleId,
    int priority,
    string workstreamId,
    string objective,
    IReadOnlyList<string> blockingEvidence)
{
    if (!rowsByParticle.TryGetValue(particleId, out var row))
        return;

    workstreams.Add(new Workstream(
        WorkstreamId: workstreamId,
        Priority: priority,
        Objective: objective,
        OpenRows: new[] { row },
        BlockingEvidence: blockingEvidence,
        AcceptanceGates: row.ClosureRequirements.Count > 0
            ? row.ClosureRequirements
            : blockingEvidence));
}

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<Workstream> workstreams,
    IReadOnlyList<string> rerunGates)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Boson Blocker Resolution Plan");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine("| Priority | Workstream | Open rows | Objective |");
    builder.AppendLine("|---:|---|---|---|");
    foreach (var workstream in workstreams)
    {
        builder.Append("| ");
        builder.Append(workstream.Priority.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(Escape(workstream.WorkstreamId));
        builder.Append(" | ");
        builder.Append(Escape(string.Join(", ", workstream.OpenRows.Select(row => row.ObservableId))));
        builder.Append(" | ");
        builder.Append(Escape(workstream.Objective));
        builder.AppendLine(" |");
    }

    foreach (var workstream in workstreams)
    {
        builder.AppendLine();
        builder.AppendLine($"## {workstream.Priority}. {workstream.WorkstreamId}");
        builder.AppendLine();
        builder.AppendLine(workstream.Objective);
        builder.AppendLine();
        builder.AppendLine("Open rows:");
        foreach (var row in workstream.OpenRows)
            builder.AppendLine($"- `{row.ParticleId}` `{row.ObservableId}` status `{row.Status}`");
        builder.AppendLine();
        builder.AppendLine("Required evidence:");
        foreach (var evidence in workstream.BlockingEvidence)
            builder.AppendLine($"- {evidence}");
        builder.AppendLine();
        builder.AppendLine("Acceptance gates:");
        foreach (var gate in workstream.AcceptanceGates)
            builder.AppendLine($"- {gate}");
    }

    builder.AppendLine();
    builder.AppendLine("## Generator Rerun Gates");
    foreach (var gate in rerunGates)
        builder.AppendLine($"- {gate}");

    return builder.ToString();
}

static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

sealed record PredictionRow(
    string ParticleId,
    string ObservableId,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    IReadOnlyList<string> ClosureRequirements);

sealed record Workstream(
    string WorkstreamId,
    int Priority,
    string Objective,
    IReadOnlyList<PredictionRow> OpenRows,
    IReadOnlyList<string> BlockingEvidence,
    IReadOnlyList<string> AcceptanceGates);
