using System.Text.Json;

const string DefaultOutputDir = "studies/phase146_fermion_sector_evidence_census_001/output";
const string StudiesRoot = "studies";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string Phase145Path = "studies/phase145_fermion_sector_intake_unlock_fixture_001/output/fermion_sector_intake_unlock_fixture.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE146_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
using var phase145 = JsonDocument.Parse(File.ReadAllText(Phase145Path));
var targetRows = phase139.RootElement.GetProperty("targetRows").EnumerateArray()
    .Select(row => new TargetRow(
        RequiredString(row, "familyId"),
        RequiredString(row, "candidateId"),
        RequiredString(row, "sourceCanonicalFermionModeId")))
    .ToArray();
var targetTokens = targetRows
    .SelectMany(row => new[] { row.FamilyId, row.CandidateId, row.SourceCanonicalFermionModeId })
    .Distinct(StringComparer.Ordinal)
    .ToArray();

var jsonFiles = Directory.EnumerateFiles(StudiesRoot, "*.json", SearchOption.AllDirectories)
    .Where(path => path.Contains($"{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains("/output/", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();
var candidates = new List<CensusCandidate>();

foreach (var path in jsonFiles)
{
    string text;
    try
    {
        text = File.ReadAllText(path);
    }
    catch
    {
        continue;
    }

    using JsonDocument? document = TryParse(text);
    if (document is null)
        continue;

    var hits = new List<string>();
    foreach (string token in targetTokens)
    {
        if (text.Contains(token, StringComparison.Ordinal))
            hits.Add(token);
    }

    var fieldNames = new HashSet<string>(StringComparer.Ordinal);
    var stringValues = new HashSet<string>(StringComparer.Ordinal);
    Collect(document.RootElement, fieldNames, stringValues);

    bool hasTargetHit = hits.Count > 0;
    bool hasChargeSector = fieldNames.Contains("chargeSector") && HasNonNullStringField(document.RootElement, "chargeSector");
    bool hasWeakSector = fieldNames.Contains("weakSector") && HasNonNullStringField(document.RootElement, "weakSector");
    bool hasQuantumNumbers = fieldNames.Contains("quantumNumbers") && HasNonNullObjectOrArrayField(document.RootElement, "quantumNumbers");
    bool hasDerivationId = fieldNames.Contains("derivationId") && HasNonNullStringField(document.RootElement, "derivationId");
    bool hasTransitionRule = fieldNames.Contains("transitionRule");
    bool hasDirectedTransitions = fieldNames.Contains("directedTransitions") && HasNonEmptyArrayField(document.RootElement, "directedTransitions");
    bool marksSynthetic = ContainsAny(text, "synthetic-fixture", "syntheticFixtureOnly", "phase145-synthetic");
    bool marksRejectedShortcut = ContainsAny(text, "numeric-alias", "phase46", "base-chirality-alone");
    bool marksTest = ContainsAny(path, $"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", "/tests/")
        || ContainsAny(text, "\"derivationId\": \"test", "\"derivationId\":\"test");
    bool rowEvidenceShape = hasTargetHit && hasChargeSector && (hasWeakSector || hasQuantumNumbers) && hasDerivationId;
    bool transitionEvidenceShape = hasTransitionRule && hasDirectedTransitions && hasDerivationId;
    bool promotableShape = (rowEvidenceShape || transitionEvidenceShape) && !marksSynthetic && !marksRejectedShortcut && !marksTest;

    if (hasTargetHit || hasChargeSector || hasWeakSector || hasQuantumNumbers || hasDerivationId || hasTransitionRule || hasDirectedTransitions)
    {
        candidates.Add(new CensusCandidate(
            Path: path,
            TargetHitCount: hits.Count,
            TargetHits: hits,
            HasChargeSector: hasChargeSector,
            HasWeakSector: hasWeakSector,
            HasQuantumNumbers: hasQuantumNumbers,
            HasDerivationId: hasDerivationId,
            HasTransitionRule: hasTransitionRule,
            HasDirectedTransitions: hasDirectedTransitions,
            RowEvidenceShape: rowEvidenceShape,
            TransitionEvidenceShape: transitionEvidenceShape,
            PromotableShape: promotableShape,
            RejectedSyntheticFixture: marksSynthetic,
            RejectedShortcutMarker: marksRejectedShortcut,
            RejectedTestLike: marksTest));
    }
}

var promotableCandidates = candidates.Where(c => c.PromotableShape).ToArray();
var targetRowHits = candidates.Where(c => c.TargetHitCount > 0).ToArray();
var rowShapeRejected = candidates.Where(c => c.RowEvidenceShape && !c.PromotableShape).ToArray();
var transitionShapeRejected = candidates.Where(c => c.TransitionEvidenceShape && !c.PromotableShape).ToArray();
bool currentEvidencePresent = promotableCandidates.Length > 0;
bool syntheticFixtureValidated = string.Equals(
    JsonString(phase145.RootElement, "terminalStatus"),
    "fermion-sector-intake-unlock-fixture-validated-real-evidence-required",
    StringComparison.Ordinal);
string terminalStatus = currentEvidencePresent
    ? "fermion-sector-evidence-census-existing-candidate-found"
    : "fermion-sector-evidence-census-no-existing-candidate";

var result = new
{
    phaseId = "phase146-fermion-sector-evidence-census",
    terminalStatus,
    evidenceCensusMaterialized = true,
    currentEvidencePresent,
    syntheticFixtureValidated,
    scannedJsonFileCount = jsonFiles.Length,
    candidateFileCount = candidates.Count,
    targetRowHitCount = targetRowHits.Length,
    promotableCandidateCount = promotableCandidates.Length,
    rejectedRowShapeCount = rowShapeRejected.Length,
    rejectedTransitionShapeCount = transitionShapeRejected.Length,
    targetRows,
    promotableCandidates,
    targetRowHits = targetRowHits.Take(40).ToArray(),
    rejectedCandidateSummary = new
    {
        syntheticFixtureCount = candidates.Count(c => c.RejectedSyntheticFixture),
        rejectedShortcutCount = candidates.Count(c => c.RejectedShortcutMarker),
        testLikeCount = candidates.Count(c => c.RejectedTestLike),
        rowShapeRejected = rowShapeRejected.Take(20).ToArray(),
        transitionShapeRejected = transitionShapeRejected.Take(20).ToArray(),
    },
    blockers = currentEvidencePresent
        ? Array.Empty<string>()
        : new[]
        {
            "no existing local study output contains a non-synthetic, non-rejected row-label or transition-rule evidence shape for the P140 intake contract",
            "real target-blind fermion-sector evidence must still be supplied or derived",
        },
    nextWork = currentEvidencePresent
        ? "review promotableCandidates and apply the accepted artifact through P140"
        : "derive or supply a new target-blind fermion-sector label table or nontrivial transition rule, then rerun P140-P146",
    sourceEvidence = new
    {
        phase139Path = Phase139Path,
        phase145Path = Phase145Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_evidence_census.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_evidence_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.evidenceCensusMaterialized,
        result.currentEvidencePresent,
        result.syntheticFixtureValidated,
        result.scannedJsonFileCount,
        result.candidateFileCount,
        result.targetRowHitCount,
        result.promotableCandidateCount,
        result.rejectedRowShapeCount,
        result.rejectedTransitionShapeCount,
        result.targetRows,
        result.promotableCandidates,
        result.rejectedCandidateSummary,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scannedJsonFileCount={jsonFiles.Length}");
Console.WriteLine($"promotableCandidateCount={promotableCandidates.Length}");

static JsonDocument? TryParse(string text)
{
    try
    {
        return JsonDocument.Parse(text);
    }
    catch
    {
        return null;
    }
}

static void Collect(JsonElement element, ISet<string> fieldNames, ISet<string> stringValues)
{
    switch (element.ValueKind)
    {
        case JsonValueKind.Object:
            foreach (var property in element.EnumerateObject())
            {
                fieldNames.Add(property.Name);
                Collect(property.Value, fieldNames, stringValues);
            }
            break;
        case JsonValueKind.Array:
            foreach (var item in element.EnumerateArray())
                Collect(item, fieldNames, stringValues);
            break;
        case JsonValueKind.String:
            stringValues.Add(element.GetString() ?? "");
            break;
    }
}

static bool HasNonNullStringField(JsonElement element, string fieldName)
{
    bool found = false;
    Visit(element, property =>
    {
        if (property.NameEquals(fieldName) && property.Value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(property.Value.GetString()))
            found = true;
    });
    return found;
}

static bool HasNonNullObjectOrArrayField(JsonElement element, string fieldName)
{
    bool found = false;
    Visit(element, property =>
    {
        if (property.NameEquals(fieldName) && property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            found = true;
    });
    return found;
}

static bool HasNonEmptyArrayField(JsonElement element, string fieldName)
{
    bool found = false;
    Visit(element, property =>
    {
        if (property.NameEquals(fieldName) && property.Value.ValueKind == JsonValueKind.Array && property.Value.GetArrayLength() > 0)
            found = true;
    });
    return found;
}

static void Visit(JsonElement element, Action<JsonProperty> visitor)
{
    if (element.ValueKind == JsonValueKind.Object)
    {
        foreach (var property in element.EnumerateObject())
        {
            visitor(property);
            Visit(property.Value, visitor);
        }
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (var item in element.EnumerateArray())
            Visit(item, visitor);
    }
}

static bool ContainsAny(string text, params string[] needles) =>
    needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

sealed record TargetRow(string FamilyId, string CandidateId, string SourceCanonicalFermionModeId);
sealed record CensusCandidate(
    string Path,
    int TargetHitCount,
    IReadOnlyList<string> TargetHits,
    bool HasChargeSector,
    bool HasWeakSector,
    bool HasQuantumNumbers,
    bool HasDerivationId,
    bool HasTransitionRule,
    bool HasDirectedTransitions,
    bool RowEvidenceShape,
    bool TransitionEvidenceShape,
    bool PromotableShape,
    bool RejectedSyntheticFixture,
    bool RejectedShortcutMarker,
    bool RejectedTestLike);
