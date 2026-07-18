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
    || path.Contains("studies/phase342_", StringComparison.Ordinal)
    || path.Contains("studies/phase343_", StringComparison.Ordinal)
    || path.Contains("studies/phase344_", StringComparison.Ordinal)
    || path.Contains("studies/phase345_", StringComparison.Ordinal)
    || path.Contains("studies/phase346_", StringComparison.Ordinal)
    || path.Contains("studies/phase347_", StringComparison.Ordinal)
    || path.Contains("studies/phase348_", StringComparison.Ordinal)
    || path.Contains("studies/phase349_", StringComparison.Ordinal)
    || path.Contains("studies/phase350_", StringComparison.Ordinal)
    || path.Contains("studies/phase351_", StringComparison.Ordinal)
    || path.Contains("studies/phase352_", StringComparison.Ordinal)
    || path.Contains("studies/phase353_", StringComparison.Ordinal)
    || path.Contains("studies/phase354_", StringComparison.Ordinal)
    || path.Contains("studies/phase355_", StringComparison.Ordinal)
    || path.Contains("studies/phase356_", StringComparison.Ordinal)
    || path.Contains("studies/phase357_", StringComparison.Ordinal)
    || path.Contains("studies/phase358_", StringComparison.Ordinal)
    || path.Contains("studies/phase359_", StringComparison.Ordinal)
    || path.Contains("studies/phase360_", StringComparison.Ordinal)
    || path.Contains("studies/phase361_", StringComparison.Ordinal)
    || path.Contains("studies/phase362_", StringComparison.Ordinal)
    || path.Contains("studies/phase363_", StringComparison.Ordinal)
    || path.Contains("studies/phase364_", StringComparison.Ordinal)
    || path.Contains("studies/phase365_", StringComparison.Ordinal)
    || path.Contains("studies/phase366_", StringComparison.Ordinal)
    || path.Contains("studies/phase367_", StringComparison.Ordinal)
    || path.Contains("studies/phase368_", StringComparison.Ordinal)
    || path.Contains("studies/phase369_", StringComparison.Ordinal)
    || path.Contains("studies/phase370_", StringComparison.Ordinal)
    || path.Contains("studies/phase371_", StringComparison.Ordinal)
    || path.Contains("studies/phase372_", StringComparison.Ordinal)
    || path.Contains("studies/phase373_", StringComparison.Ordinal)
    || path.Contains("studies/phase374_", StringComparison.Ordinal)
    || path.Contains("studies/phase375_", StringComparison.Ordinal)
    || path.Contains("studies/phase376_", StringComparison.Ordinal)
    || path.Contains("studies/phase377_", StringComparison.Ordinal)
    || path.Contains("studies/phase378_", StringComparison.Ordinal)
    || path.Contains("studies/phase379_", StringComparison.Ordinal)
    || path.Contains("studies/phase380_", StringComparison.Ordinal)
    || path.Contains("studies/phase381_", StringComparison.Ordinal)
    || path.Contains("studies/phase382_", StringComparison.Ordinal)
    || path.Contains("studies/phase383_", StringComparison.Ordinal)
    || path.Contains("studies/phase384_", StringComparison.Ordinal)
    || path.Contains("studies/phase385_", StringComparison.Ordinal)
    || path.Contains("studies/phase386_", StringComparison.Ordinal)
    || path.Contains("studies/phase387_", StringComparison.Ordinal)
    || path.Contains("studies/phase388_", StringComparison.Ordinal)
    || path.Contains("studies/phase389_", StringComparison.Ordinal)
    || path.Contains("studies/phase390_", StringComparison.Ordinal)
    || path.Contains("studies/phase391_", StringComparison.Ordinal)
    || path.Contains("studies/phase392_", StringComparison.Ordinal)
    || path.Contains("studies/phase393_", StringComparison.Ordinal)
    || path.Contains("studies/phase394_", StringComparison.Ordinal)
    || path.Contains("studies/phase395_", StringComparison.Ordinal)
    || path.Contains("studies/phase396_", StringComparison.Ordinal)
    || path.Contains("studies/phase397_", StringComparison.Ordinal)
    || path.Contains("studies/phase398_", StringComparison.Ordinal)
    || path.Contains("studies/phase399_", StringComparison.Ordinal)
    || path.Contains("studies/phase400_", StringComparison.Ordinal)
    || path.Contains("studies/phase401_", StringComparison.Ordinal)
    || path.Contains("studies/phase402_", StringComparison.Ordinal)
    || path.Contains("studies/phase403_", StringComparison.Ordinal)
    || path.Contains("studies/phase404_", StringComparison.Ordinal)
    || path.Contains("studies/phase405_", StringComparison.Ordinal)
    || path.Contains("studies/phase406_", StringComparison.Ordinal)
    || path.Contains("studies/phase407_", StringComparison.Ordinal)
    || path.Contains("studies/phase408_", StringComparison.Ordinal)
    || path.Contains("studies/phase409_", StringComparison.Ordinal)
    || path.Contains("studies/phase410_", StringComparison.Ordinal)
    || path.Contains("studies/phase411_", StringComparison.Ordinal)
    || path.Contains("studies/phase412_", StringComparison.Ordinal)
    || path.Contains("studies/phase413_", StringComparison.Ordinal)
    || path.Contains("studies/phase414_", StringComparison.Ordinal)
    || path.Contains("studies/phase415_", StringComparison.Ordinal)
    || path.Contains("studies/phase416_", StringComparison.Ordinal)
    || path.Contains("studies/phase417_", StringComparison.Ordinal)
    || path.Contains("studies/phase418_", StringComparison.Ordinal)
    || path.Contains("studies/phase419_", StringComparison.Ordinal)
    || path.Contains("studies/phase420_", StringComparison.Ordinal)
    || path.Contains("studies/phase421_", StringComparison.Ordinal)
    || path.Contains("studies/phase422_", StringComparison.Ordinal)
    || path.Contains("studies/phase423_", StringComparison.Ordinal)
    || path.Contains("studies/phase424_", StringComparison.Ordinal)
    || path.Contains("studies/phase425_", StringComparison.Ordinal)
    || path.Contains("studies/phase426_", StringComparison.Ordinal)
    || path.Contains("studies/phase427_", StringComparison.Ordinal)
    || path.Contains("studies/phase428_", StringComparison.Ordinal)
    || path.Contains("studies/phase429_", StringComparison.Ordinal)
    || path.Contains("studies/phase430_", StringComparison.Ordinal)
    || path.Contains("studies/phase431_", StringComparison.Ordinal)
    || path.Contains("studies/phase432_", StringComparison.Ordinal)
    || path.Contains("studies/phase433_", StringComparison.Ordinal)
    || path.Contains("studies/phase434_", StringComparison.Ordinal)
    || path.Contains("studies/phase435_", StringComparison.Ordinal)
    || path.Contains("studies/phase436_", StringComparison.Ordinal)
    || path.Contains("studies/phase437_", StringComparison.Ordinal)
    || path.Contains("studies/phase438_", StringComparison.Ordinal)
    || path.Contains("studies/phase439_", StringComparison.Ordinal)
    || path.Contains("studies/phase440_", StringComparison.Ordinal)
    || path.Contains("studies/phase441_", StringComparison.Ordinal)
    || path.Contains("studies/phase442_", StringComparison.Ordinal)
    || path.Contains("studies/phase443_", StringComparison.Ordinal)
    || path.Contains("studies/phase444_", StringComparison.Ordinal)
    || path.Contains("studies/phase445_", StringComparison.Ordinal)
    || path.Contains("studies/phase446_", StringComparison.Ordinal)
    || path.Contains("studies/phase447_", StringComparison.Ordinal)
    || path.Contains("studies/phase448_", StringComparison.Ordinal)
    || path.Contains("studies/phase449_", StringComparison.Ordinal)
    || path.Contains("studies/phase450_", StringComparison.Ordinal)
    || path.Contains("studies/phase451_", StringComparison.Ordinal)
    || path.Contains("studies/phase452_", StringComparison.Ordinal)
    || path.Contains("studies/phase453_", StringComparison.Ordinal)
    || path.Contains("studies/phase454_", StringComparison.Ordinal)
    || path.Contains("studies/phase455_", StringComparison.Ordinal)
    || path.Contains("studies/phase456_", StringComparison.Ordinal)
    || path.Contains("studies/phase457_", StringComparison.Ordinal)
    || path.Contains("studies/phase458_", StringComparison.Ordinal)
    || path.Contains("studies/phase459_", StringComparison.Ordinal)
    || path.Contains("studies/phase460_", StringComparison.Ordinal)
    || path.Contains("studies/phase461_", StringComparison.Ordinal)
    || path.Contains("studies/phase462_", StringComparison.Ordinal)
    || path.Contains("studies/phase463_", StringComparison.Ordinal)
    || path.Contains("studies/phase464_", StringComparison.Ordinal)
    || path.Contains("studies/phase465_", StringComparison.Ordinal)
    || path.Contains("studies/phase466_", StringComparison.Ordinal)
    || path.Contains("studies/phase467_", StringComparison.Ordinal)
    || path.Contains("studies/phase468_", StringComparison.Ordinal)
    || path.Contains("studies/phase469_", StringComparison.Ordinal)
    || path.Contains("studies/phase470_", StringComparison.Ordinal)
    || path.Contains("studies/phase471_", StringComparison.Ordinal)
    || path.Contains("studies/phase477_o4_adjudication_infrastructure_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase478_phase458_gate_specification_closure_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase479_phase457_post_ruling_readiness_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase480_o4_physicist_adjudication_intake_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase481_phase456_prospective_repair_preregistration_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase482_a5_theorem_scout_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase483_source_defined_reopening_intake_001/", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P477.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P478.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P479.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P480.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P481.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P482.md", StringComparison.Ordinal)
    || path.Contains("studies/phase484_exploratory_lane_governance_firewall_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase485_o4_assumption_falsifier_census_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase486_committed_evidence_sensitivity_triage_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase487_independent_so3_haar_measure_control_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase488_haar_proposal_invariance_control_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase489_reduced_sampler_restart_equivalence_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase490_zero_mode_quotient_audit_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase491_committed_bosonic_model_family_sensitivity_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase492_phase455_combined_robustness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase493_phase456_stored_artifact_failure_decomposition_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase494_phase456_estimator_oracle_battery_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase496_phase456_retained_data_information_census_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase499_phase456_retained_empirical_noise_information_audit_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase502_phase456_adaptive_calibration_protocol_specification_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase503_phase456_adaptive_calibration_protocol_validation_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase504_phase456_calibration_repair_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase505_phase503_frozen_failure_localization_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase506_phase456_selective_inference_protocol_validation_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase507_phase456_selective_inference_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase508_phase481_acquisition_geometry_closure_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase510_phase481_execution_readiness_adjudicator_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase511_phase481_throughput_benchmark_eligibility_audit_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase514_a5_registered_reflection_foundation_audit_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase517_a5_dual_reflection_candidate_foundation_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase518_a5_dual_reflection_exact_consistency_001/", StringComparison.Ordinal)
    || path.Contains("studies/phase519_a5_candidate_foundation_readiness_001/", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P483.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P484.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P485.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P486.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P487.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P488.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P489.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P490.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P491.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P492.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P493.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P494.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P495.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P496.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P497.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P498.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P499.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P500.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P501.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P502.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P503.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P504.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P505.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P506.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P507.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P508.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P509.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P510.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P511.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P514.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P517.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P518.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/Implementation/IMPLEMENTATION_P519.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/EXPLORATORY_SELF_AUDIT_PLAN_2026-07-15.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15.md", StringComparison.Ordinal)
    || path.Contains("docs/Phases/PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15.md", StringComparison.Ordinal);

sealed record ScanCandidate(
    string Path,
    string CandidateKind,
    int PromotableSignalCount,
    IReadOnlyList<string> PromotableSignals,
    IReadOnlyList<string> Blockers,
    bool IntakeReady);
