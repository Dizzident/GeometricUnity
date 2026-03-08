using Gu.Branching;

namespace Gu.Core.Tests;

public class InsertedAssumptionLedgerTests
{
    [Fact]
    public void All_ContainsAllSixAssumptions()
    {
        // Per Section 5: IA-1 through IA-6
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-1"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-2"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-3"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-4"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-5"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-6"));
    }

    [Fact]
    public void All_ContainsAllFiveChoices()
    {
        // Per Section 5: IX-1 through IX-5
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-1"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-2"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-3"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-4"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-5"));
    }

    [Fact]
    public void Assumptions_HasCorrectCount()
    {
        Assert.Equal(6, InsertedAssumptionLedger.Assumptions.Count);
        Assert.All(InsertedAssumptionLedger.Assumptions, a => Assert.Equal("assumption", a.Category));
    }

    [Fact]
    public void Choices_HasCorrectCount()
    {
        Assert.Equal(5, InsertedAssumptionLedger.Choices.Count);
        Assert.All(InsertedAssumptionLedger.Choices, c => Assert.Equal("choice", c.Category));
    }

    [Fact]
    public void TotalCount_Is11()
    {
        Assert.Equal(11, InsertedAssumptionLedger.All.Count);
    }

    [Fact]
    public void IA5_CpuBeforeCuda()
    {
        // Per IA-5: CPU reference backend is required before CUDA trust
        var ia5 = InsertedAssumptionLedger.All["IA-5"];
        Assert.Contains("CPU", ia5.Description);
        Assert.Contains("CUDA", ia5.Description);
    }

    [Fact]
    public void IA6_ObservationPipeline()
    {
        // Per IA-6: Observation pipeline is typed
        var ia6 = InsertedAssumptionLedger.All["IA-6"];
        Assert.Contains("sigma_h", ia6.Description);
    }

    [Fact]
    public void AllEntries_HaveNonEmptyFields()
    {
        foreach (var entry in InsertedAssumptionLedger.All.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Id));
            Assert.False(string.IsNullOrWhiteSpace(entry.Title));
            Assert.False(string.IsNullOrWhiteSpace(entry.Description));
            Assert.False(string.IsNullOrWhiteSpace(entry.Category));
            Assert.False(string.IsNullOrWhiteSpace(entry.PlanSection));
        }
    }
}
