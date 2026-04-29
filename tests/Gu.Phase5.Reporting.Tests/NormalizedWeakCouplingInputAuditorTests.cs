using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class NormalizedWeakCouplingInputAuditorTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Audit_AcceptsNormalizedCandidateWithTargetExclusions()
    {
        var result = NormalizedWeakCouplingInputAuditor.Audit(
            [
                new NormalizedWeakCouplingCandidateRecord
                {
                    CandidateId = "weak-coupling-candidate",
                    SourceKind = NormalizedWeakCouplingInputAuditor.AcceptedSourceKind,
                    NormalizationConvention = "physical-weak-coupling-normalization:su2-canonical-v1",
                    CouplingValue = 0.653,
                    CouplingUncertainty = 0.001,
                    VariationMethod = "operator-derived",
                    BranchStabilityScore = 0.99,
                    ExcludedTargetObservableIds = TargetObservableIds,
                },
            ],
            TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-ready", result.TerminalStatus);
        Assert.Equal(1, result.AcceptedCandidateCount);
        Assert.Empty(result.ClosureRequirements);
        Assert.Equal("accepted", result.Records.Single().Status);
    }

    [Fact]
    public void Audit_RejectsFiniteDifferenceCouplingProxy()
    {
        var result = NormalizedWeakCouplingInputAuditor.Audit(
            [
                new NormalizedWeakCouplingCandidateRecord
                {
                    CandidateId = "phase12-coupling-proxy",
                    SourceKind = "finite-difference-coupling-proxy",
                    NormalizationConvention = "unit-modes",
                    CouplingValue = 0.057993823115516055,
                    CouplingUncertainty = null,
                    VariationMethod = "finite-difference",
                    BranchStabilityScore = 0.0,
                    ExcludedTargetObservableIds = TargetObservableIds,
                },
            ],
            TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", result.TerminalStatus);
        var record = result.Records.Single();
        Assert.Equal("rejected", record.Status);
        Assert.Contains("source kind must be normalized-internal-weak-coupling", record.BlockReasons);
        Assert.Contains("normalization convention must start with physical-weak-coupling-normalization:", record.BlockReasons);
        Assert.Contains("finite-difference coupling proxies are not normalized weak-coupling inputs", record.BlockReasons);
    }

    [Fact]
    public void Audit_RejectsCandidateWithoutUncertainty()
    {
        var result = NormalizedWeakCouplingInputAuditor.Audit(
            [
                MakeAcceptedCandidate() with
                {
                    CandidateId = "missing-uncertainty",
                    CouplingUncertainty = null,
                },
            ],
            TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", result.TerminalStatus);
        Assert.Contains("coupling uncertainty must be finite and non-negative", result.ClosureRequirements);
    }

    [Fact]
    public void Audit_RejectsCandidateWithoutTargetExclusions()
    {
        var result = NormalizedWeakCouplingInputAuditor.Audit(
            [
                MakeAcceptedCandidate() with
                {
                    CandidateId = "target-leaking-candidate",
                    ExcludedTargetObservableIds = ["physical-w-boson-mass-gev"],
                },
            ],
            TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", result.TerminalStatus);
        Assert.Contains(
            "candidate must exclude target observable ids: physical-z-boson-mass-gev",
            result.ClosureRequirements);
    }

    [Fact]
    public void Audit_RejectsLowBranchStabilityCandidate()
    {
        var result = NormalizedWeakCouplingInputAuditor.Audit(
            [
                MakeAcceptedCandidate() with
                {
                    CandidateId = "unstable-candidate",
                    BranchStabilityScore = 0.5,
                },
            ],
            TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", result.TerminalStatus);
        Assert.Contains("branch stability score must be finite and at least 0.95", result.ClosureRequirements);
    }

    private static NormalizedWeakCouplingCandidateRecord MakeAcceptedCandidate() => new()
    {
        CandidateId = "weak-coupling-candidate",
        SourceKind = NormalizedWeakCouplingInputAuditor.AcceptedSourceKind,
        NormalizationConvention = "physical-weak-coupling-normalization:su2-canonical-v1",
        CouplingValue = 0.653,
        CouplingUncertainty = 0.001,
        VariationMethod = "operator-derived",
        BranchStabilityScore = 0.99,
        ExcludedTargetObservableIds = TargetObservableIds,
    };
}
