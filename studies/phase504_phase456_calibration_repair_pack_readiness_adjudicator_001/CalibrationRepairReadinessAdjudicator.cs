internal static class CalibrationRepairReadinessAdjudicator
{
    internal static readonly string[] Taxonomy =
    {
        "invalid-precursor",
        "protocol-validation-failure",
        "assumption-conditional-protocol",
        "adaptive-calibration-protocol-ready-for-independent-pack",
        "unresolved",
    };

    internal static ReadinessDecision Decide(ReadinessEvidence evidence)
    {
        bool contradictoryValidationState = new[]
        {
            evidence.ProtocolValidationFailed,
            evidence.AssumptionConditionalProtocol,
            evidence.ProtocolValidationPassed,
        }.Count(value => value) > 1;
        bool inputsValid = evidence.PrecursorsValid && !contradictoryValidationState;
        string classification = !inputsValid ? Taxonomy[0]
            : evidence.ProtocolValidationFailed ? Taxonomy[1]
            : evidence.AssumptionConditionalProtocol ? Taxonomy[2]
            : evidence.ProtocolSpecificationReady && evidence.ProtocolValidationPassed ? Taxonomy[3]
            : Taxonomy[4];
        return new ReadinessDecision(classification, inputsValid, contradictoryValidationState);
    }

    internal static PrecedenceBatteryResult RunPrecedenceBattery()
    {
        var cases = new[]
        {
            Case("invalid-dominates-ready", new(false, true, false, false, true), Taxonomy[0]),
            Case("contradictory-validation-fails-closed", new(true, true, true, false, true), Taxonomy[0]),
            Case("validation-failure", new(true, true, true, false, false), Taxonomy[1]),
            Case("conditional-dominates-ready", new(true, true, false, true, false), Taxonomy[2]),
            Case("ready", new(true, true, false, false, true), Taxonomy[3]),
            Case("valid-specification-not-ready", new(true, true, false, false, false), Taxonomy[4]),
            Case("validated-without-ready-specification", new(true, false, false, false, true), Taxonomy[4]),
        };
        return new PrecedenceBatteryResult
        {
            Cases = cases,
            CaseCount = cases.Length,
            AllCasesPassed = cases.All(item => item.Passed),
            InvalidPrecedencePassed = cases.Take(2).All(item => item.Passed),
            ValidationFailurePrecedencePassed = cases[2].Passed,
            ConditionalPrecedencePassed = cases[3].Passed,
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
    internal ReadinessEvidence(bool precursorsValid, bool protocolSpecificationReady,
        bool protocolValidationFailed, bool assumptionConditionalProtocol, bool protocolValidationPassed)
    {
        PrecursorsValid = precursorsValid;
        ProtocolSpecificationReady = protocolSpecificationReady;
        ProtocolValidationFailed = protocolValidationFailed;
        AssumptionConditionalProtocol = assumptionConditionalProtocol;
        ProtocolValidationPassed = protocolValidationPassed;
    }
    internal bool PrecursorsValid { get; }
    internal bool ProtocolSpecificationReady { get; }
    internal bool ProtocolValidationFailed { get; }
    internal bool AssumptionConditionalProtocol { get; }
    internal bool ProtocolValidationPassed { get; }
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
    public required bool InvalidPrecedencePassed { get; init; }
    public required bool ValidationFailurePrecedencePassed { get; init; }
    public required bool ConditionalPrecedencePassed { get; init; }
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
