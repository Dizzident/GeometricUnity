using System.Text.Json;

const string DefaultOutputDir = "studies/phase204_boson_source_lineage_candidate_scan_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE204_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));

var jsonFiles = Directory.EnumerateFiles("studies", "*.json", SearchOption.AllDirectories)
    .Where(path => !path.Contains("/bin/", StringComparison.Ordinal) && !path.Contains("/obj/", StringComparison.Ordinal))
    .Where(path => !IsGeneratedAuditOrImplementationJson(path))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();

var candidates = new List<ScanCandidate>();
var parseFailureCount = 0;

foreach (var path in jsonFiles)
{
    JsonDocument doc;
    try
    {
        doc = JsonDocument.Parse(File.ReadAllText(path));
    }
    catch (JsonException)
    {
        parseFailureCount++;
        continue;
    }

    using (doc)
    {
        var text = doc.RootElement.GetRawText();
        var lower = text.ToLowerInvariant();
        var kind = ClassifyCandidate(path, lower, doc.RootElement);
        if (kind is null)
            continue;

        var promotableSignals = CollectPromotableSignals(lower, doc.RootElement);
        var blockers = CollectBlockers(kind, lower, doc.RootElement);
        candidates.Add(new ScanCandidate(
            Path: path,
            CandidateKind: kind,
            PromotableSignalCount: promotableSignals.Length,
            PromotableSignals: promotableSignals,
            Blockers: blockers,
            IntakeReady: blockers.Length == 0));
    }
}

var wzCandidates = candidates.Where(c => c.CandidateKind == "wz-source-lineage").ToArray();
var higgsCandidates = candidates.Where(c => c.CandidateKind == "higgs-source-lineage").ToArray();
var intakeReadyCandidates = candidates.Where(c => c.IntakeReady).ToArray();

var terminalStatus = intakeReadyCandidates.Length > 0
    ? "boson-source-lineage-candidate-scan-found-intake-ready-candidate"
    : "boson-source-lineage-candidate-scan-no-intake-ready-candidate";

var result = new
{
    phaseId = "phase204-boson-source-lineage-candidate-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    scannedJsonFileCount = jsonFiles.Length,
    parseFailureCount,
    candidateCount = candidates.Count,
    wzCandidateCount = wzCandidates.Length,
    higgsCandidateCount = higgsCandidates.Length,
    intakeReadyCandidateCount = intakeReadyCandidates.Length,
    candidates = candidates
        .OrderByDescending(c => c.IntakeReady)
        .ThenByDescending(c => c.PromotableSignalCount)
        .ThenBy(c => c.Path, StringComparer.Ordinal)
        .ToArray(),
    decision = intakeReadyCandidates.Length > 0
        ? "Review intake-ready candidates and apply them through Phase201 before rerunning prediction gates."
        : "No existing repository JSON artifact appears sufficient to fill the Phase201 W/Z or Higgs source-lineage intake contracts.",
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        wzTemplatePath = JsonString(phase201.RootElement, "wzTemplatePath"),
        higgsTemplatePath = JsonString(phase201.RootElement, "higgsTemplatePath"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_candidate_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_candidate_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.scannedJsonFileCount,
        result.parseFailureCount,
        result.candidateCount,
        result.wzCandidateCount,
        result.higgsCandidateCount,
        result.intakeReadyCandidateCount,
        topCandidates = result.candidates.Take(20).ToArray(),
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scannedJsonFileCount={jsonFiles.Length}");
Console.WriteLine($"candidateCount={candidates.Count}");
Console.WriteLine($"intakeReadyCandidateCount={intakeReadyCandidates.Length}");

static string? ClassifyCandidate(string path, string lower, JsonElement root)
{
    var fileName = Path.GetFileName(path).ToLowerInvariant();
    var hasWz = lower.Contains("w-boson", StringComparison.Ordinal)
        || lower.Contains("z-boson", StringComparison.Ordinal)
        || lower.Contains("wz", StringComparison.Ordinal)
        || lower.Contains("w/z", StringComparison.Ordinal);
    var hasHiggs = lower.Contains("higgs", StringComparison.Ordinal)
        || lower.Contains("scalar-source", StringComparison.Ordinal)
        || lower.Contains("scalar source", StringComparison.Ordinal)
        || lower.Contains("massive scalar", StringComparison.Ordinal);
    var hasSourceLineageTerms = lower.Contains("sourcelineageid", StringComparison.Ordinal)
        || lower.Contains("source lineage", StringComparison.Ordinal)
        || lower.Contains("source-lineage", StringComparison.Ordinal)
        || lower.Contains("theoremorderivationid", StringComparison.Ordinal)
        || lower.Contains("scalarsourceoperatorid", StringComparison.Ordinal);

    if ((hasWz || fileName.Contains("wz", StringComparison.Ordinal)) && (hasSourceLineageTerms || lower.Contains("bridge", StringComparison.Ordinal)))
        return "wz-source-lineage";
    if ((hasHiggs || fileName.Contains("higgs", StringComparison.Ordinal)) && (hasSourceLineageTerms || lower.Contains("scalar", StringComparison.Ordinal)))
        return "higgs-source-lineage";

    return null;
}

static string[] CollectPromotableSignals(string lower, JsonElement root)
{
    var signals = new List<string>();
    AddSignalIf(signals, lower.Contains("\"externaltargetvaluesused\":false", StringComparison.Ordinal), "externalTargetValuesUsed=false");
    AddSignalIf(signals, lower.Contains("\"rawamplitudegatepassed\":true", StringComparison.Ordinal), "rawAmplitudeGatePassed=true");
    AddSignalIf(signals, lower.Contains("\"commonbridgegatepassed\":true", StringComparison.Ordinal), "commonBridgeGatePassed=true");
    AddSignalIf(signals, lower.Contains("\"targetcomparisongatepassed\":true", StringComparison.Ordinal), "targetComparisonGatePassed=true");
    AddSignalIf(signals, lower.Contains("\"stabilitysidecarspresent\":true", StringComparison.Ordinal), "stabilitySidecarsPresent=true");
    AddSignalIf(signals, lower.Contains("\"censuspromotable\":true", StringComparison.Ordinal), "censusPromotable=true");
    AddSignalIf(signals, lower.Contains("\"predictionattemptallowed\":true", StringComparison.Ordinal), "predictionAttemptAllowed=true");
    AddSignalIf(signals, lower.Contains("\"canpromote", StringComparison.Ordinal) && lower.Contains(":true", StringComparison.Ordinal), "canPromote*=true");
    return signals.ToArray();
}

static string[] CollectBlockers(string kind, string lower, JsonElement root)
{
    var blockers = new List<string>();
    if (!lower.Contains("\"externaltargetvaluesused\":false", StringComparison.Ordinal))
        blockers.Add("missing externalTargetValuesUsed=false");

    if (kind == "wz-source-lineage")
    {
        if (!lower.Contains("theoremorderivationid", StringComparison.Ordinal) && !lower.Contains("theorem", StringComparison.Ordinal))
            blockers.Add("missing W/Z theorem or derivation id");
        if (!lower.Contains("w-boson", StringComparison.Ordinal))
            blockers.Add("missing w-boson row");
        if (!lower.Contains("z-boson", StringComparison.Ordinal))
            blockers.Add("missing z-boson row");
        foreach (var gate in new[] { "rawamplitudegatepassed", "commonbridgegatepassed", "targetcomparisongatepassed", "stabilitysidecarspresent" })
        {
            if (!lower.Contains($"\"{gate}\":true", StringComparison.Ordinal))
                blockers.Add($"missing {gate}=true");
        }
    }
    else
    {
        foreach (var required in new[] { "scalarsourceoperatorid", "higgsidentityenvelopeid", "massivescalarprofileid" })
        {
            if (!lower.Contains(required, StringComparison.Ordinal))
                blockers.Add($"missing {required}");
        }
        if (!lower.Contains("potentialorselfcouplingsourceid", StringComparison.Ordinal)
            && !lower.Contains("excitationrelationid", StringComparison.Ordinal))
            blockers.Add("missing potential/self-coupling or excitation relation id");
        foreach (var sidecar in new[] { "\"branch\":true", "\"refinement\":true", "\"environment\":true", "\"representation\":true", "\"coupling\":true" })
        {
            if (!lower.Contains(sidecar, StringComparison.Ordinal))
                blockers.Add($"missing Higgs stability sidecar {sidecar}");
        }
        if (!lower.Contains("\"targetcomparisongatepassed\":true", StringComparison.Ordinal))
            blockers.Add("missing Higgs targetComparisonGatePassed=true");
    }

    return blockers.Distinct().ToArray();
}

static void AddSignalIf(List<string> signals, bool condition, string signal)
{
    if (condition)
        signals.Add(signal);
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool IsGeneratedAuditOrImplementationJson(string path) =>
    path.Contains("studies/phase200_", StringComparison.Ordinal)
    || path.Contains("studies/phase201_", StringComparison.Ordinal)
    || path.Contains("studies/phase202_", StringComparison.Ordinal)
    || path.Contains("studies/phase203_", StringComparison.Ordinal)
    || path.Contains("studies/phase204_", StringComparison.Ordinal)
    || path.Contains("studies/phase205_", StringComparison.Ordinal)
    || path.Contains("studies/phase206_", StringComparison.Ordinal)
    || path.Contains("studies/phase207_", StringComparison.Ordinal)
    || path.Contains("studies/phase208_", StringComparison.Ordinal)
    || path.Contains("studies/phase209_", StringComparison.Ordinal)
    || path.Contains("studies/phase210_", StringComparison.Ordinal)
    || path.Contains("studies/phase211_", StringComparison.Ordinal)
    || path.Contains("studies/phase212_", StringComparison.Ordinal)
    || path.Contains("studies/phase213_", StringComparison.Ordinal)
    || path.Contains("studies/phase214_", StringComparison.Ordinal)
    || path.Contains("studies/phase215_", StringComparison.Ordinal)
    || path.Contains("studies/phase216_", StringComparison.Ordinal)
    || path.Contains("studies/phase217_", StringComparison.Ordinal)
    || path.Contains("studies/phase218_", StringComparison.Ordinal)
    || path.Contains("studies/phase219_", StringComparison.Ordinal)
    || path.Contains("studies/phase220_", StringComparison.Ordinal)
    || path.Contains("studies/phase221_", StringComparison.Ordinal)
    || path.Contains("studies/phase222_", StringComparison.Ordinal)
    || path.Contains("studies/phase223_", StringComparison.Ordinal)
    || path.Contains("studies/phase224_", StringComparison.Ordinal)
    || path.Contains("studies/phase225_", StringComparison.Ordinal)
    || path.Contains("studies/phase226_", StringComparison.Ordinal)
    || path.Contains("studies/phase227_", StringComparison.Ordinal)
    || path.Contains("studies/phase228_", StringComparison.Ordinal)
    || path.Contains("studies/phase229_", StringComparison.Ordinal)
    || path.Contains("studies/phase230_", StringComparison.Ordinal)
    || path.Contains("studies/phase231_", StringComparison.Ordinal)
    || path.Contains("studies/phase232_", StringComparison.Ordinal)
    || path.Contains("studies/phase233_", StringComparison.Ordinal)
    || path.Contains("studies/phase234_", StringComparison.Ordinal)
    || path.Contains("studies/phase235_", StringComparison.Ordinal)
    || path.Contains("studies/phase236_", StringComparison.Ordinal)
    || path.Contains("studies/phase237_", StringComparison.Ordinal)
    || path.Contains("studies/phase238_", StringComparison.Ordinal)
    || path.Contains("studies/phase239_", StringComparison.Ordinal)
    || path.Contains("studies/phase240_", StringComparison.Ordinal)
    || path.Contains("studies/phase241_", StringComparison.Ordinal)
    || path.Contains("studies/phase242_", StringComparison.Ordinal)
    || path.Contains("studies/phase243_", StringComparison.Ordinal)
    || path.Contains("studies/phase244_", StringComparison.Ordinal)
    || path.Contains("studies/phase245_", StringComparison.Ordinal)
    || path.Contains("studies/phase246_", StringComparison.Ordinal)
    || path.Contains("studies/phase247_", StringComparison.Ordinal)
    || path.Contains("studies/phase248_", StringComparison.Ordinal)
    || path.Contains("studies/phase249_", StringComparison.Ordinal)
    || path.Contains("studies/phase250_", StringComparison.Ordinal)
    || path.Contains("studies/phase251_", StringComparison.Ordinal)
    || path.Contains("studies/phase252_", StringComparison.Ordinal)
    || path.Contains("studies/phase253_", StringComparison.Ordinal)
    || path.Contains("studies/phase254_", StringComparison.Ordinal)
    || path.Contains("studies/phase255_", StringComparison.Ordinal)
    || path.Contains("studies/phase256_", StringComparison.Ordinal)
    || path.Contains("studies/phase257_", StringComparison.Ordinal)
    || path.Contains("studies/phase258_", StringComparison.Ordinal)
    || path.Contains("studies/phase259_", StringComparison.Ordinal)
    || path.Contains("studies/phase260_", StringComparison.Ordinal)
    || path.Contains("studies/phase261_", StringComparison.Ordinal)
    || path.Contains("studies/phase262_", StringComparison.Ordinal)
    || path.Contains("studies/phase263_", StringComparison.Ordinal)
    || path.Contains("studies/phase264_", StringComparison.Ordinal)
    || path.Contains("studies/phase265_", StringComparison.Ordinal)
    || path.Contains("studies/phase266_", StringComparison.Ordinal)
    || path.Contains("studies/phase267_", StringComparison.Ordinal)
    || path.Contains("studies/phase268_", StringComparison.Ordinal)
    || path.Contains("studies/phase269_", StringComparison.Ordinal)
    || path.Contains("studies/phase270_", StringComparison.Ordinal)
    || path.Contains("studies/phase271_", StringComparison.Ordinal)
    || path.Contains("studies/phase272_", StringComparison.Ordinal)
    || path.Contains("studies/phase273_", StringComparison.Ordinal)
    || path.Contains("studies/phase274_", StringComparison.Ordinal)
    || path.Contains("studies/phase275_", StringComparison.Ordinal)
    || path.Contains("studies/phase276_", StringComparison.Ordinal)
    || path.Contains("studies/phase277_", StringComparison.Ordinal)
    || path.Contains("studies/phase278_", StringComparison.Ordinal)
    || path.Contains("studies/phase279_", StringComparison.Ordinal)
    || path.Contains("studies/phase280_", StringComparison.Ordinal)
    || path.Contains("studies/phase281_", StringComparison.Ordinal)
    || path.Contains("studies/phase282_", StringComparison.Ordinal)
    || path.Contains("studies/phase283_", StringComparison.Ordinal)
    || path.Contains("studies/phase284_", StringComparison.Ordinal)
    || path.Contains("studies/phase285_", StringComparison.Ordinal)
    || path.Contains("studies/phase286_", StringComparison.Ordinal)
    || path.Contains("studies/phase287_", StringComparison.Ordinal)
    || path.Contains("studies/phase288_", StringComparison.Ordinal)
    || path.Contains("studies/phase289_", StringComparison.Ordinal)
    || path.Contains("studies/phase290_", StringComparison.Ordinal)
    || path.Contains("studies/phase291_", StringComparison.Ordinal)
    || path.Contains("studies/phase292_", StringComparison.Ordinal)
    || path.Contains("studies/phase293_", StringComparison.Ordinal)
    || path.Contains("studies/phase294_", StringComparison.Ordinal)
    || path.Contains("studies/phase295_", StringComparison.Ordinal)
    || path.Contains("studies/phase296_", StringComparison.Ordinal)
    || path.Contains("studies/phase297_", StringComparison.Ordinal)
    || path.Contains("studies/phase298_", StringComparison.Ordinal)
    || path.Contains("studies/phase299_", StringComparison.Ordinal)
    || path.Contains("studies/phase300_", StringComparison.Ordinal)
    || path.Contains("studies/phase301_", StringComparison.Ordinal)
    || path.Contains("studies/phase302_", StringComparison.Ordinal)
    || path.Contains("studies/phase303_", StringComparison.Ordinal)
    || path.Contains("studies/phase304_", StringComparison.Ordinal)
    || path.Contains("studies/phase305_", StringComparison.Ordinal)
    || path.Contains("studies/phase306_", StringComparison.Ordinal)
    || path.Contains("studies/phase307_", StringComparison.Ordinal)
    || path.Contains("studies/phase308_", StringComparison.Ordinal)
    || path.Contains("studies/phase309_", StringComparison.Ordinal)
    || path.Contains("studies/phase310_", StringComparison.Ordinal)
    || path.Contains("studies/phase311_", StringComparison.Ordinal)
    || path.Contains("studies/phase312_", StringComparison.Ordinal)
    || path.Contains("studies/phase313_", StringComparison.Ordinal)
    || path.Contains("studies/phase314_", StringComparison.Ordinal)
    || path.Contains("studies/phase315_", StringComparison.Ordinal)
    || path.Contains("studies/phase316_", StringComparison.Ordinal)
    || path.Contains("studies/phase317_", StringComparison.Ordinal)
    || path.Contains("studies/phase318_", StringComparison.Ordinal)
    || path.Contains("studies/phase319_", StringComparison.Ordinal)
    || path.Contains("studies/phase320_", StringComparison.Ordinal)
    || path.Contains("studies/phase321_", StringComparison.Ordinal)
    || path.Contains("studies/phase322_", StringComparison.Ordinal)
    || path.Contains("studies/phase323_", StringComparison.Ordinal)
    || path.Contains("studies/phase324_", StringComparison.Ordinal)
    || path.Contains("studies/phase325_", StringComparison.Ordinal)
    || path.Contains("studies/phase326_", StringComparison.Ordinal)
    || path.Contains("studies/phase327_", StringComparison.Ordinal)
    || path.Contains("studies/phase328_", StringComparison.Ordinal)
    || path.Contains("studies/phase329_", StringComparison.Ordinal)
    || path.Contains("studies/phase330_", StringComparison.Ordinal)
    || path.Contains("studies/phase331_", StringComparison.Ordinal)
    || path.Contains("studies/phase332_", StringComparison.Ordinal)
    || path.Contains("studies/phase333_", StringComparison.Ordinal)
    || path.Contains("studies/phase334_", StringComparison.Ordinal)
    || path.Contains("studies/phase335_", StringComparison.Ordinal)
    || path.Contains("studies/phase336_", StringComparison.Ordinal)
    || path.Contains("studies/phase337_", StringComparison.Ordinal)
    || path.Contains("studies/phase338_", StringComparison.Ordinal)
    || path.Contains("studies/phase339_", StringComparison.Ordinal)
    || path.Contains("studies/phase340_", StringComparison.Ordinal)
    || path.Contains("studies/phase341_", StringComparison.Ordinal)
    || path.Contains("studies/phase342_", StringComparison.Ordinal);

sealed record ScanCandidate(
    string Path,
    string CandidateKind,
    int PromotableSignalCount,
    IReadOnlyList<string> PromotableSignals,
    IReadOnlyList<string> Blockers,
    bool IntakeReady);
