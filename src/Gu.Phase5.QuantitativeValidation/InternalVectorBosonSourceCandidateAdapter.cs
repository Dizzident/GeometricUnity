using System.Text.Json;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public static class InternalVectorBosonSourceCandidateAdapter
{
    public const string SourceOrigin = "internal-computed-artifact";
    public const string ModeRole = "vector-boson-source-candidate";
    public const string AlgorithmId = "p20-phase12-internal-vector-boson-source-adapter:v1";

    public static InternalVectorBosonSourceCandidateTable GenerateFromPhase12(
        string registryPath,
        string familiesPath,
        string spectraRoot,
        ProvenanceMeta provenance)
    {
        ValidateInternalPath(registryPath, nameof(registryPath));
        ValidateInternalPath(familiesPath, nameof(familiesPath));
        ValidateInternalPath(spectraRoot, nameof(spectraRoot));

        using var registryDoc = JsonDocument.Parse(File.ReadAllText(registryPath));
        using var familiesDoc = JsonDocument.Parse(File.ReadAllText(familiesPath));
        var families = LoadFamilyIndex(familiesDoc.RootElement);
        var candidates = new List<InternalVectorBosonSourceCandidate>();

        foreach (var candidateElement in registryDoc.RootElement.GetProperty("candidates").EnumerateArray())
            candidates.Add(CreateCandidate(candidateElement, families, registryPath, familiesPath, spectraRoot, provenance));

        var readyCount = candidates.Count(c => string.Equals(c.Status, "candidate-source-ready", StringComparison.Ordinal));
        var summary = readyCount > 0
            ? [$"{readyCount} internal vector-boson source candidate(s) are ready for downstream identity testing."]
            : new[]
            {
                "No Phase12 candidate has complete branch selectors, refinement coverage, unambiguous stability, and full uncertainty.",
                "Phase XX source generation remains blocked before W/Z identity testing.",
            };

        return new InternalVectorBosonSourceCandidateTable
        {
            TableId = "phase20-internal-vector-boson-source-candidates-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = readyCount > 0 ? "candidate-source-ready" : "source-blocked",
            Candidates = candidates,
            SummaryBlockers = summary,
            Provenance = provenance,
        };
    }

    public static CandidateModeSourceRecord ToCandidateModeSourceRecord(
        InternalVectorBosonSourceCandidate candidate,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        var firstPath = candidate.SourceArtifactPaths.FirstOrDefault() ?? string.Empty;
        var firstEnvironment = candidate.EnvironmentSelectors.FirstOrDefault() ?? string.Empty;
        var firstBranch = candidate.BranchSelectors.FirstOrDefault() ?? string.Empty;
        var firstRefinement = candidate.RefinementLevels.FirstOrDefault();

        return new CandidateModeSourceRecord
        {
            SourceId = candidate.SourceCandidateId,
            SourceOrigin = candidate.SourceOrigin,
            SourceArtifactKind = CandidateModeExtractor.ComputedObservableArtifactKind,
            SourceArtifactPath = firstPath,
            SourceObservableId = candidate.SourceCandidateId,
            Value = System.Math.Abs(candidate.MassLikeValue),
            Uncertainty = string.Equals(candidate.Status, "candidate-source-ready", StringComparison.Ordinal)
                ? candidate.Uncertainty.TotalUncertainty
                : -1,
            UnitFamily = "mass-energy",
            Unit = "internal-mass-unit",
            EnvironmentId = firstEnvironment,
            BranchId = firstBranch,
            RefinementLevel = firstRefinement,
            SourceExtractionMethod = AlgorithmId,
            Provenance = provenance,
        };
    }

    private static InternalVectorBosonSourceCandidate CreateCandidate(
        JsonElement candidateElement,
        IReadOnlyDictionary<string, Phase12FamilyInfo> families,
        string registryPath,
        string familiesPath,
        string spectraRoot,
        ProvenanceMeta provenance)
    {
        var candidateId = RequiredString(candidateElement, "candidateId");
        var familyId = OptionalString(candidateElement, "primaryFamilyId");
        families.TryGetValue(familyId ?? string.Empty, out var family);

        var sourceModeIds = ReadStringArray(candidateElement, "contributingModeIds");
        var backgrounds = ReadStringArray(candidateElement, "backgroundSet");
        var branchSelectors = ReadStringArray(candidateElement, "branchVariantSet");
        var massEnvelope = ReadDoubleArray(candidateElement, "massLikeEnvelope");
        var gaugeLeakEnvelope = ReadDoubleArray(candidateElement, "gaugeLeakEnvelope");
        var ambiguityCount = OptionalInt(candidateElement, "ambiguityCount") ?? family?.AmbiguityCount;
        var branchStability = OptionalDouble(candidateElement, "branchStabilityScore");
        var refinementStability = OptionalDouble(candidateElement, "refinementStabilityScore");

        var massValue = massEnvelope.Count >= 2
            ? massEnvelope[1]
            : massEnvelope.Count == 1
                ? massEnvelope[0]
                : family?.MeanEigenvalue ?? 0;
        var extractionSpread = ComputeSpread(massEnvelope, massValue, family?.EigenvalueSpread);
        var uncertainty = new QuantitativeUncertainty
        {
            ExtractionError = extractionSpread,
            EnvironmentSensitivity = family?.EigenvalueSpread >= 0 ? family.EigenvalueSpread : -1,
            BranchVariation = -1,
            RefinementError = -1,
            TotalUncertainty = -1,
        };

        var artifactPaths = new List<string>
        {
            registryPath,
            familiesPath,
        };
        artifactPaths.AddRange(backgrounds.Select(b => Path.Combine(spectraRoot, $"{b}_spectrum.json")));
        artifactPaths.AddRange(sourceModeIds.Select(m => Path.Combine(spectraRoot, "modes", $"{m}.json")));

        var blockers = BuildBlockers(branchSelectors, refinementStability, ambiguityCount, uncertainty, candidateElement, family);
        var status = blockers.Count == 0 ? "candidate-source-ready" : "source-blocked";

        return new InternalVectorBosonSourceCandidate
        {
            SourceCandidateId = $"phase12-{candidateId}",
            SourceOrigin = SourceOrigin,
            ModeRole = ModeRole,
            SourceArtifactPaths = artifactPaths,
            SourceModeIds = sourceModeIds,
            SourceFamilyId = familyId,
            MassLikeValue = massValue,
            Uncertainty = uncertainty,
            BranchSelectors = branchSelectors,
            EnvironmentSelectors = backgrounds,
            RefinementLevels = [],
            BranchStabilityScore = branchStability,
            RefinementStabilityScore = refinementStability,
            BackendStabilityScore = OptionalDouble(candidateElement, "backendStabilityScore"),
            ObservationStabilityScore = OptionalDouble(candidateElement, "observationStabilityScore"),
            AmbiguityCount = ambiguityCount,
            GaugeLeakEnvelope = gaugeLeakEnvelope,
            ClaimClass = OptionalString(candidateElement, "claimClass"),
            Status = status,
            Assumptions =
            [
                "source candidate is internal and particle-identity-neutral",
                "mass-like value is not a W or Z physical prediction",
            ],
            ClosureRequirements = blockers,
            Provenance = provenance,
        };
    }

    private static List<string> BuildBlockers(
        IReadOnlyList<string> branchSelectors,
        double? refinementStability,
        int? ambiguityCount,
        QuantitativeUncertainty uncertainty,
        JsonElement candidateElement,
        Phase12FamilyInfo? family)
    {
        var blockers = new List<string>();
        if (branchSelectors.Count == 0)
            blockers.Add("branch selectors are missing from the Phase12 candidate.");
        if (refinementStability is null || refinementStability < 1)
            blockers.Add("refinement coverage is not backed by a checked refinement ladder for this source candidate.");
        if (ambiguityCount is > 0)
            blockers.Add("mode family matching is ambiguous.");
        if (family?.IsStable == false)
            blockers.Add("mode family is not marked stable.");
        if (OptionalString(candidateElement, "claimClass") is not "C1_LocalPersistentMode" and not "C2_BranchStableCandidate")
            blockers.Add("candidate claim class is not strong enough for a ready source candidate.");
        if (!uncertainty.IsFullyEstimated || uncertainty.TotalUncertainty < 0)
            blockers.Add("source uncertainty budget is incomplete.");

        return blockers;
    }

    private static Dictionary<string, Phase12FamilyInfo> LoadFamilyIndex(JsonElement root)
    {
        var result = new Dictionary<string, Phase12FamilyInfo>(StringComparer.Ordinal);
        foreach (var element in root.EnumerateArray())
        {
            var familyId = RequiredString(element, "familyId");
            result[familyId] = new Phase12FamilyInfo(
                familyId,
                OptionalDouble(element, "meanEigenvalue") ?? 0,
                OptionalDouble(element, "eigenvalueSpread") ?? -1,
                OptionalBool(element, "isStable") ?? false,
                OptionalInt(element, "ambiguityCount") ?? 0);
        }

        return result;
    }

    private static double ComputeSpread(IReadOnlyList<double> envelope, double center, double? fallback)
    {
        if (envelope.Count >= 3)
            return envelope.Max(v => System.Math.Abs(v - center));
        if (fallback is >= 0)
            return fallback.Value;
        return -1;
    }

    private static void ValidateInternalPath(string path, string paramName)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"{paramName} is required.", paramName);
        if (path.Contains("external_target", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("target_table", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("physical_targets", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{paramName} appears to reference an external target table.", paramName);
        }
    }

    private static string RequiredString(JsonElement element, string propertyName)
        => OptionalString(element, propertyName) ?? throw new InvalidDataException($"Missing required property '{propertyName}'.");

    private static string? OptionalString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static double? OptionalDouble(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : null;

    private static int? OptionalInt(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;

    private static bool? OptionalBool(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToList()
            : [];

    private static IReadOnlyList<double> ReadDoubleArray(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Number)
                .Select(item => item.GetDouble())
                .ToList()
            : [];

    private sealed record Phase12FamilyInfo(
        string FamilyId,
        double MeanEigenvalue,
        double EigenvalueSpread,
        bool IsStable,
        int AmbiguityCount);
}
