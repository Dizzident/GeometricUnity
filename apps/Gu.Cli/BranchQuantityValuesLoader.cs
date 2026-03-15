using System.Text.Json;
using Gu.Core.Serialization;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Reporting;

internal static class BranchQuantityValuesLoader
{
    public static IReadOnlyDictionary<string, double[]> Load(
        string valuesPath,
        IReadOnlyList<string> expectedBranchVariantIds)
    {
        var json = File.ReadAllText(valuesPath);
        return LoadFromJson(json, expectedBranchVariantIds);
    }

    internal static IReadOnlyDictionary<string, double[]> LoadFromJson(
        string json,
        IReadOnlyList<string> expectedBranchVariantIds)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Branch quantity values root must be a JSON object.");
        }

        if (document.RootElement.TryGetProperty("levels", out _))
        {
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json)
                ?? throw new InvalidDataException("Failed to deserialize branch quantity value table.");
            return ConvertTable(table, expectedBranchVariantIds);
        }

        var direct = JsonSerializer.Deserialize<Dictionary<string, double[]>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (direct is null || direct.Count == 0)
        {
            throw new InvalidDataException("Failed to deserialize branch quantity values.");
        }

        foreach (var (quantityId, values) in direct)
        {
            if (values.Length != expectedBranchVariantIds.Count)
            {
                throw new InvalidDataException(
                    $"Quantity '{quantityId}' has {values.Length} values, expected {expectedBranchVariantIds.Count}.");
            }
        }

        return direct;
    }

    private static IReadOnlyDictionary<string, double[]> ConvertTable(
        RefinementQuantityValueTable table,
        IReadOnlyList<string> expectedBranchVariantIds)
    {
        var expectedIndex = expectedBranchVariantIds
            .Select((variantId, index) => (variantId, index))
            .ToDictionary(x => x.variantId, x => x.index, StringComparer.Ordinal);

        if (table.Levels.Count != expectedBranchVariantIds.Count)
        {
            throw new InvalidDataException(
                $"Bridge table has {table.Levels.Count} level(s), expected {expectedBranchVariantIds.Count} branch variants.");
        }

        var quantityValues = new Dictionary<string, double[]>(StringComparer.Ordinal);
        var seenVariants = new HashSet<string>(StringComparer.Ordinal);

        foreach (var level in table.Levels)
        {
            if (!expectedIndex.TryGetValue(level.LevelId, out var variantIndex))
            {
                throw new InvalidDataException(
                    $"Bridge table level '{level.LevelId}' does not match any branch variant in the study.");
            }

            if (!seenVariants.Add(level.LevelId))
            {
                throw new InvalidDataException($"Bridge table level '{level.LevelId}' is duplicated.");
            }

            foreach (var (quantityId, value) in level.Quantities)
            {
                if (!quantityValues.TryGetValue(quantityId, out var values))
                {
                    values = Enumerable.Repeat(double.NaN, expectedBranchVariantIds.Count).ToArray();
                    quantityValues[quantityId] = values;
                }

                values[variantIndex] = value;
            }
        }

        if (seenVariants.Count != expectedBranchVariantIds.Count)
        {
            var missing = expectedBranchVariantIds.Where(variantId => !seenVariants.Contains(variantId));
            throw new InvalidDataException(
                $"Bridge table is missing branch variant level(s): {string.Join(", ", missing)}.");
        }

        foreach (var (quantityId, values) in quantityValues)
        {
            if (values.Any(double.IsNaN))
            {
                throw new InvalidDataException(
                    $"Bridge table quantity '{quantityId}' is missing one or more branch values.");
            }
        }

        return quantityValues;
    }
}
