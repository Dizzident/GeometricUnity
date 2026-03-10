using Gu.Phase3.ModeTracking;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Registry;

/// <summary>
/// Builds CandidateBosonRecord instances from mode families and spectra.
///
/// Algorithm:
/// 1. Collect related mode families.
/// 2. Aggregate property envelopes (eigenvalue -> mass-like, gauge leak, multiplicity).
/// 3. Evaluate stability scores.
/// 4. Assign initial claim class based on evidence.
/// 5. Run demotion rules.
/// 6. Generate CandidateBosonRecord.
/// </summary>
public sealed class CandidateBosonBuilder
{
    private readonly DemotionEngine _demotionEngine;
    private readonly string _registryVersion;
    private int _nextId;

    public CandidateBosonBuilder(
        DemotionConfig? demotionConfig = null,
        string registryVersion = "1.0.0")
    {
        _demotionEngine = new DemotionEngine(demotionConfig);
        _registryVersion = registryVersion;
    }

    /// <summary>
    /// Build a candidate boson from a mode family and its associated spectra.
    /// </summary>
    public CandidateBosonRecord BuildFromFamily(
        ModeFamilyRecord family,
        IReadOnlyDictionary<string, SpectrumBundle> spectraByBackground)
    {
        ArgumentNullException.ThrowIfNull(family);
        ArgumentNullException.ThrowIfNull(spectraByBackground);

        string candidateId = $"candidate-{_nextId++}";

        // Collect eigenvalues, gauge leaks, and multiplicities across members
        var eigenvalues = new List<double>();
        var gaugeLeaks = new List<double>();
        var multiplicities = new List<int>();
        var backgroundIds = new List<string>();

        foreach (var modeId in family.MemberModeIds)
        {
            foreach (var (bgId, spectrum) in spectraByBackground)
            {
                var mode = spectrum.Modes.FirstOrDefault(m => m.ModeId == modeId);
                if (mode != null)
                {
                    eigenvalues.Add(mode.Eigenvalue);
                    gaugeLeaks.Add(mode.GaugeLeakScore);
                    if (!backgroundIds.Contains(bgId))
                        backgroundIds.Add(bgId);

                    // Find cluster multiplicity
                    if (mode.MultiplicityClusterId != null)
                    {
                        var cluster = spectrum.Clusters
                            .FirstOrDefault(c => c.ClusterId == mode.MultiplicityClusterId);
                        if (cluster != null)
                            multiplicities.Add(cluster.Multiplicity);
                        else
                            multiplicities.Add(1);
                    }
                    else
                    {
                        multiplicities.Add(1);
                    }
                }
            }
        }

        // Compute envelopes
        double[] massEnvelope = eigenvalues.Count > 0
            ? new[] { eigenvalues.Min(), eigenvalues.Average(), eigenvalues.Max() }
            : new[] { 0.0, 0.0, 0.0 };

        double[] leakEnvelope = gaugeLeaks.Count > 0
            ? new[] { gaugeLeaks.Min(), gaugeLeaks.Average(), gaugeLeaks.Max() }
            : new[] { 0.0, 0.0, 0.0 };

        int[] multEnvelope = multiplicities.Count > 0
            ? new[] { multiplicities.Min(), (int)System.Math.Round(multiplicities.Average()), multiplicities.Max() }
            : new[] { 1, 1, 1 };

        // Determine initial claim class from evidence
        var claimClass = DetermineInitialClaimClass(family);

        var candidate = new CandidateBosonRecord
        {
            CandidateId = candidateId,
            PrimaryFamilyId = family.FamilyId,
            ContributingModeIds = family.MemberModeIds,
            BackgroundSet = backgroundIds,
            MassLikeEnvelope = massEnvelope,
            MultiplicityEnvelope = multEnvelope,
            GaugeLeakEnvelope = leakEnvelope,
            BranchStabilityScore = family.IsStable ? 1.0 : 0.3,
            RefinementStabilityScore = 1.0, // Default until refinement data available
            BackendStabilityScore = 1.0,    // Default until backend comparison available
            ObservationStabilityScore = 1.0, // Default until observation data available
            ClaimClass = claimClass,
            AmbiguityNotes = family.AmbiguityCount > 0
                ? new[] { $"{family.AmbiguityCount} ambiguous matches in family history" }
                : Array.Empty<string>(),
            RegistryVersion = _registryVersion,
        };

        // Apply demotion rules
        return _demotionEngine.ApplyDemotions(candidate);
    }

    /// <summary>
    /// Build candidates from all families in a tracking result.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> BuildFromFamilies(
        IReadOnlyList<ModeFamilyRecord> families,
        IReadOnlyDictionary<string, SpectrumBundle> spectraByBackground)
    {
        return families.Select(f => BuildFromFamily(f, spectraByBackground)).ToList();
    }

    private static BosonClaimClass DetermineInitialClaimClass(ModeFamilyRecord family)
    {
        // C0: single context, no matching evidence
        if (family.ContextIds.Count <= 1)
            return BosonClaimClass.C0_NumericalMode;

        // C1: persistent under continuation/refinement (multiple contexts)
        if (family.ContextIds.Count >= 2 && family.IsStable)
            return BosonClaimClass.C1_LocalPersistentMode;

        // C1 even if not stable, if there are multiple contexts
        if (family.ContextIds.Count >= 2)
            return BosonClaimClass.C1_LocalPersistentMode;

        return BosonClaimClass.C0_NumericalMode;
    }
}
