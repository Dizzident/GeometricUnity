using Gu.Core;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality;

/// <summary>
/// M38 pipeline: takes a solved FermionSpectralResult and runs chirality + conjugation
/// analysis to produce a FermionSpectralBundle.
///
/// Usage:
///   var solver = new FermionSpectralSolver(assembler);
///   var result = solver.Solve(diracBundle, layout, config, provenance);
///   var builder = new FermionSpectralBundleBuilder();
///   var bundle = builder.Build(result, gammas, chiralityConv, conjConv, layout, cellCount, provenance);
///
/// The builder:
///   1. Runs ChiralityAnalyzer on every mode.
///   2. Runs ConjugationAnalyzer to find conjugation pairs.
///   3. Computes a GaugeLeakSummary (leak scores are already on the mode records).
///   4. Returns a FermionSpectralBundle aggregating all results.
///
/// Architecture note: this class lives in Gu.Phase4.Chirality (downstream of Gu.Phase4.Dirac)
/// to avoid a circular dependency.
/// </summary>
public sealed class FermionSpectralBundleBuilder
{
    private readonly ChiralityAnalyzer _chiralityAnalyzer;
    private readonly ConjugationAnalyzer _conjugationAnalyzer;

    public FermionSpectralBundleBuilder()
    {
        _chiralityAnalyzer = new ChiralityAnalyzer();
        _conjugationAnalyzer = new ConjugationAnalyzer();
    }

    /// <summary>
    /// Solve and build a FermionSpectralBundle in one step.
    ///
    /// This convenience method uses the chiralityPostProcessor delegate pattern
    /// supported by FermionSpectralSolver to inject real chirality and conjugation
    /// analysis directly into the mode records during the solve.
    ///
    /// The returned bundle contains the solved + analyzed result with real chirality
    /// decompositions on each mode record AND separate ChiralityDecomposition objects
    /// for detailed analysis.
    /// </summary>
    public FermionSpectralBundle SolveAndBuild(
        FermionSpectralSolver solver,
        DiracOperatorBundle diracBundle,
        FermionFieldLayout layout,
        FermionSpectralConfig config,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        ConjugationConventionSpec conjugationConvention,
        int cellCount,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(solver);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(chiralityConvention);
        ArgumentNullException.ThrowIfNull(conjugationConvention);

        // Build the chirality post-processor delegate that the solver will call
        // after computing eigenvectors. This wires M37 analysis into the Dirac solver
        // without creating a circular project dependency.
        Func<FermionModeRecord[], FermionModeRecord[]> postProc =
            rawModes => ApplyChiralityAndConjugation(
                rawModes, gammas, chiralityConvention, conjugationConvention, layout, cellCount);

        var spectralResult = solver.Solve(diracBundle, layout, config, provenance, postProc);

        return Build(spectralResult, gammas, chiralityConvention, conjugationConvention,
            layout, cellCount, provenance);
    }

    private FermionModeRecord[] ApplyChiralityAndConjugation(
        FermionModeRecord[] modes,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        ConjugationConventionSpec conjugationConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        // Apply chirality
        var chiralityResults = _chiralityAnalyzer.AnalyzeAll(
            modes, gammas, chiralityConvention, layout, cellCount);

        var updated = new FermionModeRecord[modes.Length];
        for (int i = 0; i < modes.Length; i++)
        {
            var m = modes[i];
            var cd = chiralityResults[i];
            updated[i] = new FermionModeRecord
            {
                ModeId = m.ModeId,
                BackgroundId = m.BackgroundId,
                BranchVariantId = m.BranchVariantId,
                LayoutId = m.LayoutId,
                ModeIndex = m.ModeIndex,
                EigenvalueRe = m.EigenvalueRe,
                EigenvalueIm = m.EigenvalueIm,
                ResidualNorm = m.ResidualNorm,
                EigenvectorCoefficients = m.EigenvectorCoefficients,
                ChiralityDecomposition = new ChiralityDecompositionRecord
                {
                    LeftFraction = cd.LeftFraction,
                    RightFraction = cd.RightFraction,
                    MixedFraction = cd.MixedFraction,
                    SignConvention = cd.SignConvention,
                },
                ConjugationPairing = m.ConjugationPairing,
                GaugeLeakScore = m.GaugeLeakScore,
                GaugeReductionApplied = m.GaugeReductionApplied,
                Backend = m.Backend,
                ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
                BranchStabilityScore = m.BranchStabilityScore,
                RefinementStabilityScore = m.RefinementStabilityScore,
                ReplayTier = m.ReplayTier,
                AmbiguityNotes = m.AmbiguityNotes,
                Provenance = m.Provenance,
            };
        }

        // Apply conjugation pairing (operates on the chirality-updated modes)
        var pairs = _conjugationAnalyzer.FindPairs(updated, conjugationConvention, gammas);
        var pairingMap = new Dictionary<string, ConjugationPairRecord>();
        foreach (var pair in pairs)
        {
            pairingMap[pair.ModeIdA] = pair;
            pairingMap[pair.ModeIdB] = pair;
        }

        for (int i = 0; i < updated.Length; i++)
        {
            var m = updated[i];
            if (pairingMap.TryGetValue(m.ModeId, out var pair))
            {
                string partnerId = pair.ModeIdA == m.ModeId ? pair.ModeIdB : pair.ModeIdA;
                double partnerEv = pair.ModeIdA == m.ModeId ? pair.EigenvalueB : pair.EigenvalueA;
                updated[i] = new FermionModeRecord
                {
                    ModeId = m.ModeId,
                    BackgroundId = m.BackgroundId,
                    BranchVariantId = m.BranchVariantId,
                    LayoutId = m.LayoutId,
                    ModeIndex = m.ModeIndex,
                    EigenvalueRe = m.EigenvalueRe,
                    EigenvalueIm = m.EigenvalueIm,
                    ResidualNorm = m.ResidualNorm,
                    EigenvectorCoefficients = m.EigenvectorCoefficients,
                    ChiralityDecomposition = m.ChiralityDecomposition,
                    ConjugationPairing = new ConjugationPairingRecord
                    {
                        HasPair = true,
                        PartnerModeId = partnerId,
                        PartnerEigenvalue = partnerEv,
                        ConjugationType = conjugationConvention.ConjugationType,
                    },
                    GaugeLeakScore = m.GaugeLeakScore,
                    GaugeReductionApplied = m.GaugeReductionApplied,
                    Backend = m.Backend,
                    ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
                    BranchStabilityScore = m.BranchStabilityScore,
                    RefinementStabilityScore = m.RefinementStabilityScore,
                    ReplayTier = m.ReplayTier,
                    AmbiguityNotes = m.AmbiguityNotes,
                    Provenance = m.Provenance,
                };
            }
        }

        return updated;
    }

    /// <summary>
    /// Build a FermionSpectralBundle from a solved spectral result.
    ///
    /// If gammas.ChiralityMatrix is null (odd-dimensional Y), chirality is trivial.
    /// Conjugation analysis always runs regardless of chirality availability.
    /// </summary>
    public FermionSpectralBundle Build(
        FermionSpectralResult spectralResult,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        ConjugationConventionSpec conjugationConvention,
        FermionFieldLayout layout,
        int cellCount,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(spectralResult);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(chiralityConvention);
        ArgumentNullException.ThrowIfNull(conjugationConvention);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(provenance);

        var modes = spectralResult.Modes;

        // 1. Chirality analysis
        var chiralityDecomps = _chiralityAnalyzer.AnalyzeAll(
            modes, gammas, chiralityConvention, layout, cellCount);
        bool chiralityAvailable = chiralityConvention.HasChirality && gammas.ChiralityMatrix is not null;

        // 2. Conjugation analysis
        var conjPairs = _conjugationAnalyzer.FindPairs(
            modes, conjugationConvention, gammas);

        // 3. Gauge leak summary (using leak scores already stored on mode records)
        double leakThreshold = 0.5;
        var highLeakIds = modes
            .Where(m => m.GaugeLeakScore > leakThreshold)
            .Select(m => m.ModeId)
            .ToList();

        double meanLeak = modes.Count > 0 ? modes.Average(m => m.GaugeLeakScore) : 0.0;
        double maxLeak  = modes.Count > 0 ? modes.Max(m => m.GaugeLeakScore) : 0.0;

        var leakSummary = new GaugeLeakSummary
        {
            BackgroundId = spectralResult.FermionBackgroundId,
            TotalModes = modes.Count,
            HighLeakModeCount = highLeakIds.Count,
            LeakThreshold = leakThreshold,
            MeanLeakScore = meanLeak,
            MaxLeakScore = maxLeak,
            HighLeakModeIds = highLeakIds,
        };

        return new FermionSpectralBundle
        {
            BundleId = $"spectral-bundle-{spectralResult.FermionBackgroundId}",
            FermionBackgroundId = spectralResult.FermionBackgroundId,
            SpectralResult = spectralResult,
            ChiralityDecompositions = chiralityDecomps.ToList(),
            ConjugationPairs = conjPairs.ToList(),
            GaugeLeakSummary = leakSummary,
            ChiralityAnalysisAvailable = chiralityAvailable,
            ConjugationAnalysisApplied = true,
            Provenance = provenance,
        };
    }
}
