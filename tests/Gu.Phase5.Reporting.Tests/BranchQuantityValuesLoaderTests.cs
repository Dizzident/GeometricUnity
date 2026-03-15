using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class BranchQuantityValuesLoaderTests
{
    [Fact]
    public void LoadFromJson_AcceptsLegacyQuantityMap()
    {
        const string json = """
{
  "residual-norm": [1.0, 2.0],
  "objective-value": [3.0, 4.0]
}
""";

        var loaded = BranchQuantityValuesLoader.LoadFromJson(json, ["variant-a", "variant-b"]);

        Assert.Equal(new[] { 1.0, 2.0 }, loaded["residual-norm"]);
        Assert.Equal(new[] { 3.0, 4.0 }, loaded["objective-value"]);
    }

    [Fact]
    public void LoadFromJson_AcceptsBridgeTableAndReordersLevelsToStudyVariantOrder()
    {
        const string json = """
{
  "studyId": "branch-values",
  "levels": [
    {
      "levelId": "variant-b",
      "solverConverged": true,
      "quantities": {
        "residual-norm": 2.0,
        "objective-value": 4.0
      }
    },
    {
      "levelId": "variant-a",
      "solverConverged": true,
      "quantities": {
        "residual-norm": 1.0,
        "objective-value": 3.0
      }
    }
  ]
}
""";

        var loaded = BranchQuantityValuesLoader.LoadFromJson(json, ["variant-a", "variant-b"]);

        Assert.Equal(new[] { 1.0, 2.0 }, loaded["residual-norm"]);
        Assert.Equal(new[] { 3.0, 4.0 }, loaded["objective-value"]);
    }

    [Fact]
    public void LoadFromJson_RejectsBridgeTableMissingExpectedVariant()
    {
        const string json = """
{
  "studyId": "branch-values",
  "levels": [
    {
      "levelId": "variant-a",
      "solverConverged": true,
      "quantities": {
        "residual-norm": 1.0
      }
    },
    {
      "levelId": "variant-c",
      "solverConverged": true,
      "quantities": {
        "residual-norm": 2.0
      }
    }
  ]
}
""";

        var ex = Assert.Throws<InvalidDataException>(() =>
            BranchQuantityValuesLoader.LoadFromJson(json, ["variant-a", "variant-b"]));

        Assert.Contains("does not match any branch variant", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
