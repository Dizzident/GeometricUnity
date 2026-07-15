internal static class ReadinessAdjudicator
{
    internal static readonly string[] Taxonomy =
    {
        "invalid-precursor",
        "retained-data-sufficient",
        "new-acquisition-specification-ready",
        "no-viable-specification-within-audited-grid",
        "unresolved",
    };

    internal static ReadinessDecision Decide(ReadinessEvidence evidence)
    {
        bool contradictoryOracleState = evidence.NewAcquisitionSpecificationReady &&
            evidence.NoViableSpecificationWithinAuditedGrid;
        bool contradictoryCensusState = evidence.RetainedDataSufficient &&
            evidence.NewAcquisitionRequired;
        bool inputsValid = evidence.PrecursorsValid && !contradictoryOracleState &&
            !contradictoryCensusState;

        string classification = !inputsValid ? Taxonomy[0]
            : evidence.RetainedDataSufficient && evidence.RetainedDataRecoveryGatesPassed ? Taxonomy[1]
            : evidence.NewAcquisitionRequired && evidence.NewAcquisitionSpecificationReady &&
                evidence.WholeAdmittedOracleMenuPassed ? Taxonomy[2]
            : evidence.NewAcquisitionRequired && evidence.NoViableSpecificationWithinAuditedGrid ? Taxonomy[3]
            : Taxonomy[4];

        return new ReadinessDecision
        {
            Classification = classification,
            InputsValid = inputsValid,
            ContradictoryOracleState = contradictoryOracleState,
            ContradictoryCensusState = contradictoryCensusState,
            RetainedDataSufficientCondition = inputsValid && evidence.RetainedDataSufficient &&
                evidence.RetainedDataRecoveryGatesPassed,
            NewAcquisitionSpecificationReadyCondition = inputsValid && evidence.NewAcquisitionRequired &&
                evidence.NewAcquisitionSpecificationReady && evidence.WholeAdmittedOracleMenuPassed,
            NoViableSpecificationCondition = inputsValid && evidence.NewAcquisitionRequired &&
                evidence.NoViableSpecificationWithinAuditedGrid,
        };
    }

    internal static PrecedenceBatteryResult RunPrecedenceBattery()
    {
        var cases = new[]
        {
            Case("invalid-over-everything", new ReadinessEvidence(false, true, false, true, true, true, true), Taxonomy[0]),
            Case("contradictory-census-fails-closed", new ReadinessEvidence(true, true, true, true, false, false, false), Taxonomy[0]),
            Case("contradictory-oracle-fails-closed", new ReadinessEvidence(true, false, true, false, true, true, true), Taxonomy[0]),
            Case("retained-data-requires-recovery-gates", new ReadinessEvidence(true, true, false, false, false, false, false), Taxonomy[4]),
            Case("retained-data-sufficient", new ReadinessEvidence(true, true, false, true, false, false, false), Taxonomy[1]),
            Case("new-spec-requires-whole-menu", new ReadinessEvidence(true, false, true, false, true, false, false), Taxonomy[4]),
            Case("new-spec-ready", new ReadinessEvidence(true, false, true, false, true, true, false), Taxonomy[2]),
            Case("audited-grid-exhausted", new ReadinessEvidence(true, false, true, false, false, false, true), Taxonomy[3]),
            Case("valid-unresolved", new ReadinessEvidence(true, false, true, false, false, false, false), Taxonomy[4]),
        };
        return new PrecedenceBatteryResult
        {
            Cases = cases,
            CaseCount = cases.Length,
            AllCasesPassed = cases.All(item => item.Passed),
            InvalidPrecedencePassed = cases.Take(3).All(item => item.Passed && item.Actual == Taxonomy[0]),
            ReadinessRequiresWholeMenuPassed = cases.Single(item => item.CaseId == "new-spec-requires-whole-menu").Passed,
        };
    }

    private static PrecedenceCaseResult Case(string caseId, ReadinessEvidence evidence, string expected)
    {
        var decision = Decide(evidence);
        return new PrecedenceCaseResult
        {
            CaseId = caseId,
            Expected = expected,
            Actual = decision.Classification,
            Passed = decision.Classification == expected,
        };
    }
}

internal sealed class ReadinessEvidence
{
    internal ReadinessEvidence(
        bool precursorsValid,
        bool retainedDataSufficient,
        bool newAcquisitionRequired,
        bool retainedDataRecoveryGatesPassed,
        bool newAcquisitionSpecificationReady,
        bool wholeAdmittedOracleMenuPassed,
        bool noViableSpecificationWithinAuditedGrid)
    {
        PrecursorsValid = precursorsValid;
        RetainedDataSufficient = retainedDataSufficient;
        NewAcquisitionRequired = newAcquisitionRequired;
        RetainedDataRecoveryGatesPassed = retainedDataRecoveryGatesPassed;
        NewAcquisitionSpecificationReady = newAcquisitionSpecificationReady;
        WholeAdmittedOracleMenuPassed = wholeAdmittedOracleMenuPassed;
        NoViableSpecificationWithinAuditedGrid = noViableSpecificationWithinAuditedGrid;
    }

    internal bool PrecursorsValid { get; }
    internal bool RetainedDataSufficient { get; }
    internal bool NewAcquisitionRequired { get; }
    internal bool RetainedDataRecoveryGatesPassed { get; }
    internal bool NewAcquisitionSpecificationReady { get; }
    internal bool WholeAdmittedOracleMenuPassed { get; }
    internal bool NoViableSpecificationWithinAuditedGrid { get; }
}

internal sealed class ReadinessDecision
{
    internal required string Classification { get; init; }
    internal required bool InputsValid { get; init; }
    internal required bool ContradictoryOracleState { get; init; }
    internal required bool ContradictoryCensusState { get; init; }
    internal required bool RetainedDataSufficientCondition { get; init; }
    internal required bool NewAcquisitionSpecificationReadyCondition { get; init; }
    internal required bool NoViableSpecificationCondition { get; init; }
}

internal sealed class PrecedenceBatteryResult
{
    public required PrecedenceCaseResult[] Cases { get; init; }
    public required int CaseCount { get; init; }
    public required bool AllCasesPassed { get; init; }
    public required bool InvalidPrecedencePassed { get; init; }
    public required bool ReadinessRequiresWholeMenuPassed { get; init; }
}

internal sealed class PrecedenceCaseResult
{
    public required string CaseId { get; init; }
    public required string Expected { get; init; }
    public required string Actual { get; init; }
    public required bool Passed { get; init; }
}
