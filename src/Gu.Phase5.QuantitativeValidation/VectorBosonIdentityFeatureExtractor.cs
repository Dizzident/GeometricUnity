using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class VectorBosonIdentityFeatureExtractionResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("featureRecords")]
    public required IReadOnlyList<VectorBosonIdentityFeatureRecord> FeatureRecords { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    [JsonIgnore]
    public string EnrichedModeFamiliesJson { get; init; } = string.Empty;
}

public sealed class VectorBosonIdentityFeatureRecord
{
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("electroweakMultipletId")]
    public string? ElectroweakMultipletId { get; init; }

    [JsonPropertyName("algebraBasisSector")]
    public string? AlgebraBasisSector { get; init; }

    [JsonPropertyName("chargeSector")]
    public string? ChargeSector { get; init; }

    [JsonPropertyName("currentCouplingSignature")]
    public string? CurrentCouplingSignature { get; init; }

    [JsonPropertyName("basisEnergyFractions")]
    public required IReadOnlyList<double> BasisEnergyFractions { get; init; }

    [JsonPropertyName("dominantBasisIndex")]
    public int? DominantBasisIndex { get; init; }

    [JsonPropertyName("couplingProfile")]
    public required VectorBosonCouplingProfile? CouplingProfile { get; init; }

    [JsonPropertyName("featureStatus")]
    public required string FeatureStatus { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public sealed class VectorBosonCouplingProfile
{
    [JsonPropertyName("profileHash")]
    public required string ProfileHash { get; init; }

    [JsonPropertyName("couplingCount")]
    public required int CouplingCount { get; init; }

    [JsonPropertyName("nonzeroCount")]
    public required int NonzeroCount { get; init; }

    [JsonPropertyName("meanMagnitude")]
    public required double MeanMagnitude { get; init; }

    [JsonPropertyName("diagonalMeanMagnitude")]
    public required double DiagonalMeanMagnitude { get; init; }

    [JsonPropertyName("offDiagonalMeanMagnitude")]
    public required double OffDiagonalMeanMagnitude { get; init; }

    [JsonPropertyName("imaginaryMagnitudeFraction")]
    public required double ImaginaryMagnitudeFraction { get; init; }
}

public static class VectorBosonIdentityFeatureExtractor
{
    public const string AlgorithmId = "p25-internal-electroweak-feature-extractor:v1";

    public static VectorBosonIdentityFeatureExtractionResult Extract(
        string modeFamiliesJson,
        string phase12ModeFamiliesJson,
        string modeSignatureRoot,
        IReadOnlyList<string> couplingAtlasJsons,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(phase12ModeFamiliesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeSignatureRoot);
        ArgumentNullException.ThrowIfNull(couplingAtlasJsons);

        var p22Root = JsonNode.Parse(modeFamiliesJson)
            ?? throw new InvalidDataException("Mode-family JSON is empty.");
        var p22Families = p22Root["families"]?.AsArray()
            ?? throw new InvalidDataException("Mode-family JSON must contain a 'families' array.");
        var phase12Families = LoadPhase12Families(phase12ModeFamiliesJson);
        var couplings = LoadCouplings(couplingAtlasJsons);

        var records = new List<VectorBosonIdentityFeatureRecord>();
        foreach (var familyNode in p22Families.OfType<JsonObject>())
        {
            var familyId = familyNode["familyId"]?.GetValue<string>() ?? string.Empty;
            var sourceCandidateId = familyNode["sourceCandidateId"]?.GetValue<string>() ?? string.Empty;
            var phase12CandidateId = sourceCandidateId.StartsWith("phase12-", StringComparison.Ordinal)
                ? sourceCandidateId["phase12-".Length..]
                : sourceCandidateId;
            var phase12Family = phase12Families.FirstOrDefault(f =>
                string.Equals(f.FamilyId, $"family-{phase12CandidateId.Split('-').Last()}", StringComparison.Ordinal));

            var record = ExtractRecord(familyId, sourceCandidateId, phase12CandidateId, phase12Family, modeSignatureRoot, couplings);
            records.Add(record);
            familyNode["identityFeatures"] = ToIdentityFeatureNode(record);
        }

        var summaryBlockers = records
            .SelectMany(r => r.Blockers)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new VectorBosonIdentityFeatureExtractionResult
        {
            ResultId = "phase25-internal-electroweak-identity-features-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = records.Count > 0 && records.All(r => r.FeatureStatus == "complete")
                ? "identity-features-complete"
                : "identity-features-partial",
            AlgorithmId = AlgorithmId,
            FeatureRecords = records,
            SummaryBlockers = summaryBlockers,
            Provenance = provenance,
            EnrichedModeFamiliesJson = p22Root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
        };
    }

    private static VectorBosonIdentityFeatureRecord ExtractRecord(
        string familyId,
        string sourceCandidateId,
        string phase12CandidateId,
        Phase12FamilyInfo? phase12Family,
        string modeSignatureRoot,
        IReadOnlyDictionary<string, IReadOnlyList<CouplingInfo>> couplings)
    {
        var blockers = new List<string>();
        var fractions = phase12Family is null
            ? []
            : ComputeBasisEnergyFractions(phase12Family.MemberModeIds, modeSignatureRoot, blockers);
        var dominantBasisIndex = fractions.Count == 3 ? IndexOfMax(fractions) : (int?)null;
        var electroweakMultipletId = fractions.Count == 3 ? "su2-adjoint-triplet:canonical-basis" : null;
        var algebraBasisSector = dominantBasisIndex is null
            ? null
            : fractions[dominantBasisIndex.Value] >= 0.45
                ? $"canonical-su2-adjoint-axis-{dominantBasisIndex.Value}"
                : "mixed-su2-adjoint-sector";

        if (phase12Family is null)
            blockers.Add("missing Phase12 family membership for source candidate.");
        if (electroweakMultipletId is null)
            blockers.Add("unable to derive SU(2)-adjoint multiplet from mode signatures.");

        var couplingProfile = couplings.TryGetValue(phase12CandidateId, out var candidateCouplings)
            ? BuildCouplingProfile(candidateCouplings)
            : null;
        var currentCouplingSignature = couplingProfile is null
            ? null
            : $"finite-difference-current-profile:{couplingProfile.ProfileHash}";
        if (couplingProfile is null)
            blockers.Add("missing finite-difference coupling profile for source candidate.");

        blockers.Add("charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts.");

        return new VectorBosonIdentityFeatureRecord
        {
            FamilyId = familyId,
            SourceCandidateId = sourceCandidateId,
            ElectroweakMultipletId = electroweakMultipletId,
            AlgebraBasisSector = algebraBasisSector,
            ChargeSector = null,
            CurrentCouplingSignature = currentCouplingSignature,
            BasisEnergyFractions = fractions,
            DominantBasisIndex = dominantBasisIndex,
            CouplingProfile = couplingProfile,
            FeatureStatus = blockers.Count == 0 ? "complete" : "partial",
            Blockers = blockers,
        };
    }

    private static IReadOnlyList<double> ComputeBasisEnergyFractions(
        IReadOnlyList<string> memberModeIds,
        string modeSignatureRoot,
        List<string> blockers)
    {
        var energy = new[] { 0.0, 0.0, 0.0 };
        var found = 0;
        foreach (var modeId in memberModeIds)
        {
            var path = Path.Combine(modeSignatureRoot, $"{modeId}.json");
            if (!File.Exists(path))
            {
                blockers.Add($"missing mode-signature artifact '{path}'.");
                continue;
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            var shape = root.GetProperty("observedShape").EnumerateArray().Select(e => e.GetInt32()).ToArray();
            if (shape.Length != 2 || shape[1] != 3)
            {
                blockers.Add($"mode signature '{path}' is not a three-component Lie-algebra signature.");
                continue;
            }

            var coeffs = root.GetProperty("observedCoefficients").EnumerateArray().Select(e => e.GetDouble()).ToArray();
            for (var i = 0; i < coeffs.Length; i++)
                energy[i % 3] += coeffs[i] * coeffs[i];
            found++;
        }

        if (found == 0)
            return [];

        var total = energy.Sum();
        return total > 0 ? energy.Select(v => v / total).ToList() : [];
    }

    private static VectorBosonCouplingProfile BuildCouplingProfile(IReadOnlyList<CouplingInfo> couplings)
    {
        var diagonal = couplings.Where(c => string.Equals(c.FermionModeIdI, c.FermionModeIdJ, StringComparison.Ordinal)).ToList();
        var offDiagonal = couplings.Where(c => !string.Equals(c.FermionModeIdI, c.FermionModeIdJ, StringComparison.Ordinal)).ToList();
        var mean = couplings.Count > 0 ? couplings.Average(c => c.Magnitude) : 0;
        var diagMean = diagonal.Count > 0 ? diagonal.Average(c => c.Magnitude) : 0;
        var offDiagMean = offDiagonal.Count > 0 ? offDiagonal.Average(c => c.Magnitude) : 0;
        var imagFraction = couplings.Sum(c => System.Math.Abs(c.Imaginary)) / System.Math.Max(couplings.Sum(c => c.Magnitude), double.Epsilon);
        var hashPayload = string.Join(":",
            couplings.Count,
            Round(mean),
            Round(diagMean),
            Round(offDiagMean),
            Round(imagFraction));

        return new VectorBosonCouplingProfile
        {
            ProfileHash = ShortHash(hashPayload),
            CouplingCount = couplings.Count,
            NonzeroCount = couplings.Count(c => c.Magnitude > 0),
            MeanMagnitude = mean,
            DiagonalMeanMagnitude = diagMean,
            OffDiagonalMeanMagnitude = offDiagMean,
            ImaginaryMagnitudeFraction = imagFraction,
        };
    }

    private static JsonObject ToIdentityFeatureNode(VectorBosonIdentityFeatureRecord record)
    {
        var node = new JsonObject
        {
            ["featureStatus"] = record.FeatureStatus,
            ["electroweakMultipletId"] = record.ElectroweakMultipletId,
            ["algebraBasisSector"] = record.AlgebraBasisSector,
            ["chargeSector"] = record.ChargeSector,
            ["currentCouplingSignature"] = record.CurrentCouplingSignature,
            ["dominantBasisIndex"] = record.DominantBasisIndex,
        };
        var blockers = new JsonArray();
        foreach (var blocker in record.Blockers)
            blockers.Add(blocker);
        node["blockers"] = blockers;

        var fractions = new JsonArray();
        foreach (var fraction in record.BasisEnergyFractions)
            fractions.Add(fraction);
        node["basisEnergyFractions"] = fractions;
        if (record.CouplingProfile is not null)
        {
            node["couplingProfile"] = new JsonObject
            {
                ["profileHash"] = record.CouplingProfile.ProfileHash,
                ["couplingCount"] = record.CouplingProfile.CouplingCount,
                ["nonzeroCount"] = record.CouplingProfile.NonzeroCount,
                ["meanMagnitude"] = record.CouplingProfile.MeanMagnitude,
                ["diagonalMeanMagnitude"] = record.CouplingProfile.DiagonalMeanMagnitude,
                ["offDiagonalMeanMagnitude"] = record.CouplingProfile.OffDiagonalMeanMagnitude,
                ["imaginaryMagnitudeFraction"] = record.CouplingProfile.ImaginaryMagnitudeFraction,
            };
        }

        return node;
    }

    private static IReadOnlyList<Phase12FamilyInfo> LoadPhase12Families(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(e => new Phase12FamilyInfo(
                e.GetProperty("familyId").GetString() ?? string.Empty,
                e.GetProperty("memberModeIds").EnumerateArray().Select(v => v.GetString() ?? string.Empty).ToList()))
            .ToList();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<CouplingInfo>> LoadCouplings(IReadOnlyList<string> atlasJsons)
    {
        var result = new Dictionary<string, List<CouplingInfo>>(StringComparer.Ordinal);
        foreach (var json in atlasJsons)
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var c in doc.RootElement.GetProperty("couplings").EnumerateArray())
            {
                var bosonModeId = c.GetProperty("bosonModeId").GetString() ?? string.Empty;
                if (!result.TryGetValue(bosonModeId, out var list))
                    result[bosonModeId] = list = [];
                list.Add(new CouplingInfo(
                    c.GetProperty("fermionModeIdI").GetString() ?? string.Empty,
                    c.GetProperty("fermionModeIdJ").GetString() ?? string.Empty,
                    c.GetProperty("couplingProxyImaginary").GetDouble(),
                    c.GetProperty("couplingProxyMagnitude").GetDouble()));
            }
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<CouplingInfo>)kvp.Value, StringComparer.Ordinal);
    }

    private static int IndexOfMax(IReadOnlyList<double> values)
    {
        var index = 0;
        for (var i = 1; i < values.Count; i++)
        {
            if (values[i] > values[index])
                index = i;
        }

        return index;
    }

    private static string ShortHash(string payload)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)))[..16].ToLowerInvariant();

    private static string Round(double value)
        => value.ToString("G12", System.Globalization.CultureInfo.InvariantCulture);

    private sealed record Phase12FamilyInfo(string FamilyId, IReadOnlyList<string> MemberModeIds);

    private sealed record CouplingInfo(string FermionModeIdI, string FermionModeIdJ, double Imaginary, double Magnitude);
}
