internal static class RobustReadinessAdjudicator
{
    internal static readonly string[] Taxonomy =
    {
        "invalid-precursor",
        "insufficient-retained-calibration",
        "robust-specification-identified",
        "assumption-conditional-specification",
        "no-viable-specification-within-audited-budget",
        "unresolved",
    };

    internal static ReadinessDecision Decide(ReadinessEvidence evidence)
    {
        bool contradictoryStressState = new[]
        {
            evidence.RobustSpecification,
            evidence.AssumptionConditionalSpecification,
            evidence.NoViableSpecificationWithinAuditedBudget,
        }.Count(value => value) > 1;
        bool inputsValid = evidence.PrecursorsValid && !contradictoryStressState;
        string classification = !inputsValid ? Taxonomy[0]
            : !evidence.RetainedCalibrationSufficient ? Taxonomy[1]
            : evidence.RobustSpecification ? Taxonomy[2]
            : evidence.AssumptionConditionalSpecification ? Taxonomy[3]
            : evidence.NoViableSpecificationWithinAuditedBudget ? Taxonomy[4]
            : Taxonomy[5];
        return new ReadinessDecision(classification, inputsValid, contradictoryStressState);
    }

    internal static PrecedenceBatteryResult RunPrecedenceBattery()
    {
        var cases = new[]
        {
            Case("invalid-dominates-all", new(false, false, true, false, false), Taxonomy[0]),
            Case("contradictory-stress-fails-closed", new(true, true, true, true, false), Taxonomy[0]),
            Case("insufficient-calibration-dominates-robust", new(true, false, true, false, false), Taxonomy[1]),
            Case("insufficient-calibration-dominates-conditional", new(true, false, false, true, false), Taxonomy[1]),
            Case("robust", new(true, true, true, false, false), Taxonomy[2]),
            Case("conditional", new(true, true, false, true, false), Taxonomy[3]),
            Case("budget-exhausted", new(true, true, false, false, true), Taxonomy[4]),
            Case("unresolved", new(true, true, false, false, false), Taxonomy[5]),
        };
        return new PrecedenceBatteryResult
        {
            Cases = cases,
            CaseCount = cases.Length,
            AllCasesPassed = cases.All(item => item.Passed),
            InvalidPrecedencePassed = cases.Take(2).All(item => item.Passed),
            InsufficientCalibrationPrecedencePassed = cases.Skip(2).Take(2).All(item => item.Passed),
        };
    }

    private static PrecedenceCaseResult Case(string caseId, ReadinessEvidence evidence, string expected)
    {
        string actual = Decide(evidence).Classification;
        return new(caseId, expected, actual, actual == expected);
    }
}

internal sealed class ReadinessEvidence
{
    internal ReadinessEvidence(bool precursorsValid, bool retainedCalibrationSufficient,
        bool robustSpecification, bool assumptionConditionalSpecification,
        bool noViableSpecificationWithinAuditedBudget)
    {
        PrecursorsValid = precursorsValid;
        RetainedCalibrationSufficient = retainedCalibrationSufficient;
        RobustSpecification = robustSpecification;
        AssumptionConditionalSpecification = assumptionConditionalSpecification;
        NoViableSpecificationWithinAuditedBudget = noViableSpecificationWithinAuditedBudget;
    }
    internal bool PrecursorsValid { get; }
    internal bool RetainedCalibrationSufficient { get; }
    internal bool RobustSpecification { get; }
    internal bool AssumptionConditionalSpecification { get; }
    internal bool NoViableSpecificationWithinAuditedBudget { get; }
}

internal sealed class ReadinessDecision
{
    internal ReadinessDecision(string classification, bool inputsValid, bool contradictoryStressState)
    {
        Classification = classification;
        InputsValid = inputsValid;
        ContradictoryStressState = contradictoryStressState;
    }
    internal string Classification { get; }
    internal bool InputsValid { get; }
    internal bool ContradictoryStressState { get; }
}

internal sealed class PrecedenceBatteryResult
{
    public required PrecedenceCaseResult[] Cases { get; init; }
    public required int CaseCount { get; init; }
    public required bool AllCasesPassed { get; init; }
    public required bool InvalidPrecedencePassed { get; init; }
    public required bool InsufficientCalibrationPrecedencePassed { get; init; }
}

internal sealed class PrecedenceCaseResult
{
    internal PrecedenceCaseResult(string caseId, string expected, string actual, bool passed)
    {
        CaseId = caseId;
        Expected = expected;
        Actual = actual;
        Passed = passed;
    }
    public string CaseId { get; }
    public string Expected { get; }
    public string Actual { get; }
    public bool Passed { get; }
}
