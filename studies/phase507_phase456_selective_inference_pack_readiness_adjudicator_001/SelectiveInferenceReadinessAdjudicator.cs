internal static class SelectiveInferenceReadinessAdjudicator
{
    internal static readonly string[] Taxonomy =
    {
        "invalid-precursor",
        "protocol-validation-failure",
        "assumption-conditional-repair",
        "selective-inference-protocol-ready-for-independent-phase481-pack-construction",
        "unresolved",
    };

    internal static ReadinessDecision Decide(ReadinessEvidence evidence)
    {
        bool contradictoryValidationState = new[]
        {
            evidence.ProtocolValidationFailed,
            evidence.AssumptionConditionalRepair,
            evidence.CompleteMenuPassed,
        }.Count(value => value) > 1;
        bool inputsValid = evidence.PrecursorsValid && evidence.Phase503BaselinePreserved &&
            evidence.FailureLocalizationComplete && !contradictoryValidationState;
        string classification = !inputsValid ? Taxonomy[0]
            : evidence.ProtocolValidationFailed ? Taxonomy[1]
            : evidence.AssumptionConditionalRepair ? Taxonomy[2]
            : evidence.CompleteMenuPassed ? Taxonomy[3]
            : Taxonomy[4];
        return new ReadinessDecision(classification, inputsValid, contradictoryValidationState);
    }

    internal static PrecedenceBatteryResult RunPrecedenceBattery()
    {
        var cases = new[]
        {
            Case("invalid-dominates-ready", new(false, true, true, false, false, true), Taxonomy[0]),
            Case("phase503-baseline-drift-fails-closed", new(true, false, true, false, false, true), Taxonomy[0]),
            Case("incomplete-localization-fails-closed", new(true, true, false, false, false, true), Taxonomy[0]),
            Case("contradictory-failed-and-ready-fails-closed", new(true, true, true, true, false, true), Taxonomy[0]),
            Case("contradictory-conditional-and-ready-fails-closed", new(true, true, true, false, true, true), Taxonomy[0]),
            Case("protocol-validation-failure", new(true, true, true, true, false, false), Taxonomy[1]),
            Case("assumption-conditional-repair", new(true, true, true, false, true, false), Taxonomy[2]),
            Case("complete-menu-ready", new(true, true, true, false, false, true), Taxonomy[3]),
            Case("valid-unresolved", new(true, true, true, false, false, false), Taxonomy[4]),
        };
        return new PrecedenceBatteryResult
        {
            Cases = cases,
            CaseCount = cases.Length,
            AllCasesPassed = cases.All(item => item.Passed),
            InvalidCasesPassed = cases.Take(5).All(item => item.Passed),
            FailureCasePassed = cases[5].Passed,
            ConditionalCasePassed = cases[6].Passed,
            ReadyCasePassed = cases[7].Passed,
            UnresolvedCasePassed = cases[8].Passed,
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
    internal ReadinessEvidence(bool precursorsValid, bool phase503BaselinePreserved,
        bool failureLocalizationComplete, bool protocolValidationFailed,
        bool assumptionConditionalRepair, bool completeMenuPassed)
    {
        PrecursorsValid = precursorsValid;
        Phase503BaselinePreserved = phase503BaselinePreserved;
        FailureLocalizationComplete = failureLocalizationComplete;
        ProtocolValidationFailed = protocolValidationFailed;
        AssumptionConditionalRepair = assumptionConditionalRepair;
        CompleteMenuPassed = completeMenuPassed;
    }
    internal bool PrecursorsValid { get; }
    internal bool Phase503BaselinePreserved { get; }
    internal bool FailureLocalizationComplete { get; }
    internal bool ProtocolValidationFailed { get; }
    internal bool AssumptionConditionalRepair { get; }
    internal bool CompleteMenuPassed { get; }
}

internal sealed class ReadinessDecision
{
    internal ReadinessDecision(string classification, bool inputsValid, bool contradictoryValidationState)
    {
        Classification = classification;
        InputsValid = inputsValid;
        ContradictoryValidationState = contradictoryValidationState;
    }
    internal string Classification { get; }
    internal bool InputsValid { get; }
    internal bool ContradictoryValidationState { get; }
}

internal sealed class PrecedenceBatteryResult
{
    public required PrecedenceCaseResult[] Cases { get; init; }
    public required int CaseCount { get; init; }
    public required bool AllCasesPassed { get; init; }
    public required bool InvalidCasesPassed { get; init; }
    public required bool FailureCasePassed { get; init; }
    public required bool ConditionalCasePassed { get; init; }
    public required bool ReadyCasePassed { get; init; }
    public required bool UnresolvedCasePassed { get; init; }
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
