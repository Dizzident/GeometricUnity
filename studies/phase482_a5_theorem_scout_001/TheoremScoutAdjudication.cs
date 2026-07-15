using System.Text.Json.Serialization;

namespace Phase482A5TheoremScout;

/// <summary>
/// Fail-closed adjudication for the Phase482 theorem scout.  The analysis
/// modules provide facts; this class alone maps those facts to the frozen
/// Phase482 verdict taxonomy.
/// </summary>
public static class TheoremScoutAdjudication
{
    public const string InvalidProofPackage = "invalid-proof-package";
    public const string NoGoTheoremClosed = "no-go-theorem-closed";
    public const string ObstructionsSurviveNoTheorem = "obstructions-survive-no-theorem";
    public const string CounterexampleRefutesProposedNoGo = "counterexample-refutes-proposed-no-go";

    public const string CompositeCorrelationTransport = "composite-correlation-transport";
    public const string ReflectionPositivity = "reflection-positivity-committed-simplicial-action";
    public const string FaceLocalInteraction = "face-local-interaction-hypothesis";

    private static readonly string[] RequiredFamilyIds =
    {
        CompositeCorrelationTransport,
        ReflectionPositivity,
        FaceLocalInteraction,
    };

    public static TheoremScoutDecision Evaluate(TheoremScoutInputs input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var families = input.Families ?? Array.Empty<TheoremScoutFamilyResult>();
        bool familySetExact =
            families.Length == RequiredFamilyIds.Length &&
            families.Select(x => x.FamilyId).Order(StringComparer.Ordinal)
                .SequenceEqual(RequiredFamilyIds.Order(StringComparer.Ordinal), StringComparer.Ordinal);
        bool familyInputsValid = familySetExact && families.All(x =>
            x.InputsValid &&
            x.AnalysisImplemented &&
            x.ProofGateImplemented &&
            x.CounterexampleGateImplemented &&
            !(x.RequiredClaimProved && x.RefutingCounterexampleEstablished));

        bool inputsValid =
            input.A5ObstructionRecordExact &&
            input.Phase478ContractExact &&
            input.DecisionTaxonomyFrozenBeforeInspection &&
            input.CommittedActionIdentifiedExactly &&
            input.CommittedTriangulationIdentifiedExactly &&
            familyInputsValid;

        var evaluatedFamilies = families.Select(EvaluateFamily).ToArray();
        bool proofGateComplete = inputsValid && evaluatedFamilies.All(x => x.ProofAndCounterexampleGatesImplemented);
        bool validRefutingCounterexample = inputsValid && evaluatedFamilies.Any(x => x.ValidRefutingCounterexample);
        bool allThreeProofGatesGreen =
            proofGateComplete &&
            evaluatedFamilies.Length == RequiredFamilyIds.Length &&
            evaluatedFamilies.All(x => x.TheoremGateGreen);

        // Precedence is deliberate: malformed/drifted inputs cannot yield a
        // scientific terminal; an exact counterexample defeats a simultaneous
        // closure attempt; theorem closure requires every gate; all other
        // fully implemented outcomes preserve the obstruction.
        string verdictKind = !inputsValid
            ? InvalidProofPackage
            : validRefutingCounterexample
                ? CounterexampleRefutesProposedNoGo
                : allThreeProofGatesGreen
                    ? NoGoTheoremClosed
                    : ObstructionsSurviveNoTheorem;

        bool theoremClaimed = verdictKind == NoGoTheoremClosed;
        bool closesLimbL8 = theoremClaimed;

        return new TheoremScoutDecision
        {
            VerdictKind = verdictKind,
            InputsValid = inputsValid,
            FamilySetExact = familySetExact,
            ProofGateComplete = proofGateComplete,
            AllThreeObstructionFamilyProofGatesGreen = allThreeProofGatesGreen,
            ValidRefutingCounterexampleEstablished = validRefutingCounterexample,
            TheoremClaimed = theoremClaimed,
            ClosesLimbL8 = closesLimbL8,
            FamilyResults = evaluatedFamilies,
            DecisionPrecedence = new[]
            {
                InvalidProofPackage,
                CounterexampleRefutesProposedNoGo,
                NoGoTheoremClosed,
                ObstructionsSurviveNoTheorem,
            },
            PartialLemmaCanCloseL8 = false,
            ChangedTriangulationCanCloseL8 = false,
            ChangedActionCanCloseL8 = false,
            FiniteVolumeOnlyResultCanCloseL8 = false,
            SingleCouplingRegimeCanCloseL8 = false,
            Phase458Evaluated = false,
            Phase458EvaluationAuthorized = false,
            Phase458G1Satisfied = theoremClaimed,
            Phase458G3Satisfied = false,
            Phase458G4Satisfied = false,
            Phase458G5Satisfied = false,
            SamplingAuthorized = false,
            BinderLaunchAuthorized = false,
            AccelerationAuthorized = false,
            ProductionAuthorized = false,
            HumanRulingAuthored = false,
            O4Discharged = false,
            Phase481PackConstructed = false,
            TargetBlindConstruction = true,
            PhysicalTargetsConsultedForConstruction = false,
            PhysicistReviewPending = true,
            ScaleIsWorkbenchRelativeCandidateOnly = true,
            PhysicalCouplingProvided = false,
            RouteProvidesPhysicalEffectiveActionHessian = false,
            RouteProvidesVevOrSourceScaleLineage = false,
            RouteProvidesPoleExtractionAndGevNormalization = false,
            SourceContractApplicationAllowed = false,
            Phase201TemplateMutated = false,
            FieldsAppliedToPhase201TemplateCount = 0,
            AcceptedContractFieldCount = 0,
            CanFillPhase201WzContract = false,
            CanFillPhase201HiggsContract = false,
            CanFillPhase256Contract = false,
            CanFillPhase256ObservedFieldExtractionContract = false,
            RoutePromotesWzMasses = false,
            RoutePromotesHiggsMass = false,
            RouteCompletesBosonPredictions = false,
            NoGevPromotion = true,
            PromotedPhysicalMassClaimCount = 0,
        };
    }

    public static bool RunPrecedenceBattery()
    {
        var green = RequiredFamilyIds.Select(GreenFamily).ToArray();
        var baseInput = new TheoremScoutInputs
        {
            A5ObstructionRecordExact = true,
            Phase478ContractExact = true,
            DecisionTaxonomyFrozenBeforeInspection = true,
            CommittedActionIdentifiedExactly = true,
            CommittedTriangulationIdentifiedExactly = true,
            Families = green,
        };

        bool allGreenCloses = Evaluate(baseInput).VerdictKind == NoGoTheoremClosed;

        var partial = green.Select(Clone).ToArray();
        partial[0].RequiredClaimProved = false;
        bool partialDoesNotClose = Evaluate(CopyWithFamilies(baseInput, partial)).VerdictKind == ObstructionsSurviveNoTheorem;

        var finiteVolumeOnly = green.Select(Clone).ToArray();
        finiteVolumeOnly[1].UniformBeyondAuditedFiniteVolume = false;
        bool finiteVolumeDoesNotClose = Evaluate(CopyWithFamilies(baseInput, finiteVolumeOnly)).VerdictKind == ObstructionsSurviveNoTheorem;

        var oneRegime = green.Select(Clone).ToArray();
        oneRegime[2].AllRequiredCouplingRegimesCovered = false;
        bool oneRegimeDoesNotClose = Evaluate(CopyWithFamilies(baseInput, oneRegime)).VerdictKind == ObstructionsSurviveNoTheorem;

        var changedTriangulation = green.Select(Clone).ToArray();
        changedTriangulation[1].CommittedTriangulationUsed = false;
        bool changedTriangulationDoesNotClose = Evaluate(CopyWithFamilies(baseInput, changedTriangulation)).VerdictKind == ObstructionsSurviveNoTheorem;

        var counterexample = green.Select(Clone).ToArray();
        counterexample[0].RequiredClaimProved = false;
        counterexample[0].RefutingCounterexampleEstablished = true;
        counterexample[0].CounterexampleWithinProposedTheoremDomain = true;
        bool counterexamplePrecedes = Evaluate(CopyWithFamilies(baseInput, counterexample)).VerdictKind == CounterexampleRefutesProposedNoGo;

        var invalid = CopyWithFamilies(baseInput, green.Select(Clone).ToArray());
        invalid.Phase478ContractExact = false;
        bool invalidPrecedes = Evaluate(invalid).VerdictKind == InvalidProofPackage;

        return allGreenCloses && partialDoesNotClose && finiteVolumeDoesNotClose && oneRegimeDoesNotClose &&
            changedTriangulationDoesNotClose && counterexamplePrecedes && invalidPrecedes;
    }

    private static TheoremScoutFamilyDecision EvaluateFamily(TheoremScoutFamilyResult family)
    {
        bool proofAndCounterexampleGatesImplemented =
            family.AnalysisImplemented && family.ProofGateImplemented && family.CounterexampleGateImplemented;
        bool scopeExact =
            family.CommittedActionUsed &&
            family.CommittedTriangulationUsed &&
            family.WorkbenchObservableUnchanged &&
            family.UniformBeyondAuditedFiniteVolume &&
            family.AllRequiredCouplingRegimesCovered &&
            family.NoUnprovedPremiseImported;
        bool theoremGateGreen = family.InputsValid && proofAndCounterexampleGatesImplemented &&
            family.RequiredClaimProved && scopeExact && !family.RefutingCounterexampleEstablished;
        bool validRefutingCounterexample = family.InputsValid && proofAndCounterexampleGatesImplemented &&
            family.RefutingCounterexampleEstablished &&
            family.CounterexampleWithinProposedTheoremDomain &&
            family.CommittedActionUsed && family.CommittedTriangulationUsed && family.WorkbenchObservableUnchanged;

        return new TheoremScoutFamilyDecision
        {
            FamilyId = family.FamilyId,
            ProofAndCounterexampleGatesImplemented = proofAndCounterexampleGatesImplemented,
            ScopeExactForTheoremClosure = scopeExact,
            RequiredClaimProved = family.RequiredClaimProved,
            TheoremGateGreen = theoremGateGreen,
            ValidRefutingCounterexample = validRefutingCounterexample,
        };
    }

    private static TheoremScoutFamilyResult GreenFamily(string id) => new()
    {
        FamilyId = id,
        InputsValid = true,
        AnalysisImplemented = true,
        ProofGateImplemented = true,
        CounterexampleGateImplemented = true,
        RequiredClaimProved = true,
        RefutingCounterexampleEstablished = false,
        CounterexampleWithinProposedTheoremDomain = false,
        CommittedActionUsed = true,
        CommittedTriangulationUsed = true,
        WorkbenchObservableUnchanged = true,
        UniformBeyondAuditedFiniteVolume = true,
        AllRequiredCouplingRegimesCovered = true,
        NoUnprovedPremiseImported = true,
    };

    private static TheoremScoutFamilyResult Clone(TheoremScoutFamilyResult x) => new()
    {
        FamilyId = x.FamilyId,
        InputsValid = x.InputsValid,
        AnalysisImplemented = x.AnalysisImplemented,
        ProofGateImplemented = x.ProofGateImplemented,
        CounterexampleGateImplemented = x.CounterexampleGateImplemented,
        RequiredClaimProved = x.RequiredClaimProved,
        RefutingCounterexampleEstablished = x.RefutingCounterexampleEstablished,
        CounterexampleWithinProposedTheoremDomain = x.CounterexampleWithinProposedTheoremDomain,
        CommittedActionUsed = x.CommittedActionUsed,
        CommittedTriangulationUsed = x.CommittedTriangulationUsed,
        WorkbenchObservableUnchanged = x.WorkbenchObservableUnchanged,
        UniformBeyondAuditedFiniteVolume = x.UniformBeyondAuditedFiniteVolume,
        AllRequiredCouplingRegimesCovered = x.AllRequiredCouplingRegimesCovered,
        NoUnprovedPremiseImported = x.NoUnprovedPremiseImported,
    };

    private static TheoremScoutInputs CopyWithFamilies(TheoremScoutInputs x, TheoremScoutFamilyResult[] families) => new()
    {
        A5ObstructionRecordExact = x.A5ObstructionRecordExact,
        Phase478ContractExact = x.Phase478ContractExact,
        DecisionTaxonomyFrozenBeforeInspection = x.DecisionTaxonomyFrozenBeforeInspection,
        CommittedActionIdentifiedExactly = x.CommittedActionIdentifiedExactly,
        CommittedTriangulationIdentifiedExactly = x.CommittedTriangulationIdentifiedExactly,
        Families = families,
    };
}

public sealed class TheoremScoutInputs
{
    public bool A5ObstructionRecordExact { get; set; }
    public bool Phase478ContractExact { get; set; }
    public bool DecisionTaxonomyFrozenBeforeInspection { get; set; }
    public bool CommittedActionIdentifiedExactly { get; set; }
    public bool CommittedTriangulationIdentifiedExactly { get; set; }
    public TheoremScoutFamilyResult[]? Families { get; set; }
}

public sealed class TheoremScoutFamilyResult
{
    public string FamilyId { get; set; } = "";
    public bool InputsValid { get; set; }
    public bool AnalysisImplemented { get; set; }
    public bool ProofGateImplemented { get; set; }
    public bool CounterexampleGateImplemented { get; set; }
    public bool RequiredClaimProved { get; set; }
    public bool RefutingCounterexampleEstablished { get; set; }
    public bool CounterexampleWithinProposedTheoremDomain { get; set; }
    public bool CommittedActionUsed { get; set; }
    public bool CommittedTriangulationUsed { get; set; }
    public bool WorkbenchObservableUnchanged { get; set; }
    public bool UniformBeyondAuditedFiniteVolume { get; set; }
    public bool AllRequiredCouplingRegimesCovered { get; set; }
    public bool NoUnprovedPremiseImported { get; set; }
}

public sealed class TheoremScoutFamilyDecision
{
    public string FamilyId { get; init; } = "";
    public bool ProofAndCounterexampleGatesImplemented { get; init; }
    public bool ScopeExactForTheoremClosure { get; init; }
    public bool RequiredClaimProved { get; init; }
    public bool TheoremGateGreen { get; init; }
    public bool ValidRefutingCounterexample { get; init; }
}

public sealed class TheoremScoutDecision
{
    public string VerdictKind { get; init; } = "";
    public bool InputsValid { get; init; }
    public bool FamilySetExact { get; init; }
    public bool ProofGateComplete { get; init; }
    public bool AllThreeObstructionFamilyProofGatesGreen { get; init; }
    public bool ValidRefutingCounterexampleEstablished { get; init; }
    public bool TheoremClaimed { get; init; }
    public bool ClosesLimbL8 { get; init; }
    public TheoremScoutFamilyDecision[] FamilyResults { get; init; } = Array.Empty<TheoremScoutFamilyDecision>();
    public string[] DecisionPrecedence { get; init; } = Array.Empty<string>();
    public bool PartialLemmaCanCloseL8 { get; init; }
    public bool ChangedTriangulationCanCloseL8 { get; init; }
    public bool ChangedActionCanCloseL8 { get; init; }
    public bool FiniteVolumeOnlyResultCanCloseL8 { get; init; }
    public bool SingleCouplingRegimeCanCloseL8 { get; init; }
    public bool Phase458Evaluated { get; init; }
    public bool Phase458EvaluationAuthorized { get; init; }
    public bool Phase458G1Satisfied { get; init; }
    public bool Phase458G3Satisfied { get; init; }
    public bool Phase458G4Satisfied { get; init; }
    public bool Phase458G5Satisfied { get; init; }
    public bool SamplingAuthorized { get; init; }
    public bool BinderLaunchAuthorized { get; init; }
    public bool AccelerationAuthorized { get; init; }
    public bool ProductionAuthorized { get; init; }
    public bool HumanRulingAuthored { get; init; }
    public bool O4Discharged { get; init; }
    public bool Phase481PackConstructed { get; init; }
    public bool TargetBlindConstruction { get; init; }
    public bool PhysicalTargetsConsultedForConstruction { get; init; }
    // The phase artifact carries the single registered top-level pending
    // marker. Keep the adjudicator's internal invariant out of the serialized
    // nested object so the O4 coverage surface has exactly one pointer.
    [JsonIgnore]
    public bool PhysicistReviewPending { get; init; }
    public bool ScaleIsWorkbenchRelativeCandidateOnly { get; init; }
    public bool PhysicalCouplingProvided { get; init; }
    public bool RouteProvidesPhysicalEffectiveActionHessian { get; init; }
    public bool RouteProvidesVevOrSourceScaleLineage { get; init; }
    public bool RouteProvidesPoleExtractionAndGevNormalization { get; init; }
    public bool SourceContractApplicationAllowed { get; init; }
    public bool Phase201TemplateMutated { get; init; }
    public int FieldsAppliedToPhase201TemplateCount { get; init; }
    public int AcceptedContractFieldCount { get; init; }
    public bool CanFillPhase201WzContract { get; init; }
    public bool CanFillPhase201HiggsContract { get; init; }
    public bool CanFillPhase256Contract { get; init; }
    public bool CanFillPhase256ObservedFieldExtractionContract { get; init; }
    public bool RoutePromotesWzMasses { get; init; }
    public bool RoutePromotesHiggsMass { get; init; }
    public bool RouteCompletesBosonPredictions { get; init; }
    public bool NoGevPromotion { get; init; }
    public int PromotedPhysicalMassClaimCount { get; init; }
}
