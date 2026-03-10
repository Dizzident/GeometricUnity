using Gu.Phase3.ModeTracking;
using Gu.Phase3.Properties;
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
        return BuildFromFamily(family, spectraByBackground, null);
    }

    /// <summary>
    /// Build a candidate boson from a mode family, spectra, and property vectors.
    /// When property vectors are provided, the three envelope fields are populated.
    /// </summary>
    public CandidateBosonRecord BuildFromFamily(
        ModeFamilyRecord family,
        IReadOnlyDictionary<string, SpectrumBundle> spectraByBackground,
        IReadOnlyDictionary<string, BosonPropertyVector>? propertyVectorsByModeId)
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

        // Compute envelope fields from property vectors
        var contributingVectors = GetContributingVectors(family, propertyVectorsByModeId);
        var polarizationEnvelope = BuildPolarizationEnvelope(contributingVectors);
        var symmetryEnvelope = BuildSymmetryEnvelope(contributingVectors);
        var interactionProxyEnvelope = BuildInteractionProxyEnvelope(contributingVectors);

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
            PolarizationEnvelope = polarizationEnvelope,
            SymmetryEnvelope = symmetryEnvelope,
            InteractionProxyEnvelope = interactionProxyEnvelope,
            BranchStabilityScore = family.IsStable ? 1.0 : 0.3,
            RefinementStabilityScore = 1.0, // Default until refinement data available
            BackendStabilityScore = 1.0,    // Default until backend comparison available
            ObservationStabilityScore = 1.0, // Default until observation data available
            ClaimClass = claimClass,
            AmbiguityCount = family.AmbiguityCount,
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

    /// <summary>
    /// Build candidates from all families with property vectors.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> BuildFromFamilies(
        IReadOnlyList<ModeFamilyRecord> families,
        IReadOnlyDictionary<string, SpectrumBundle> spectraByBackground,
        IReadOnlyDictionary<string, BosonPropertyVector>? propertyVectorsByModeId)
    {
        return families.Select(f => BuildFromFamily(f, spectraByBackground, propertyVectorsByModeId)).ToList();
    }

    private static List<BosonPropertyVector> GetContributingVectors(
        ModeFamilyRecord family,
        IReadOnlyDictionary<string, BosonPropertyVector>? propertyVectorsByModeId)
    {
        var result = new List<BosonPropertyVector>();
        if (propertyVectorsByModeId == null)
            return result;

        foreach (var modeId in family.MemberModeIds)
        {
            if (propertyVectorsByModeId.TryGetValue(modeId, out var pv))
                result.Add(pv);
        }
        return result;
    }

    internal static PolarizationEnvelope? BuildPolarizationEnvelope(List<BosonPropertyVector> vectors)
    {
        if (vectors.Count == 0)
            return null;

        // Find most frequent dominant polarization class
        var classCounts = new Dictionary<string, int>();
        foreach (var v in vectors)
        {
            var cls = v.Polarization.DominantClass;
            classCounts.TryGetValue(cls, out int count);
            classCounts[cls] = count + 1;
        }
        string dominantClass = classCounts.OrderByDescending(kv => kv.Value).First().Key;

        double minFraction = vectors.Min(v => v.Polarization.DominanceFraction);
        double maxFraction = vectors.Max(v => v.Polarization.DominanceFraction);

        return new PolarizationEnvelope
        {
            DominantClass = dominantClass,
            MinFraction = minFraction,
            MaxFraction = maxFraction,
        };
    }

    internal static SymmetryEnvelope? BuildSymmetryEnvelope(List<BosonPropertyVector> vectors)
    {
        if (vectors.Count == 0)
            return null;

        // Union of all symmetry labels
        var unionLabels = new HashSet<string>();
        foreach (var v in vectors)
        {
            foreach (var label in v.Symmetry.SymmetryLabels)
                unionLabels.Add(label);
        }

        // Min/max parity: null if any mode has null parity
        int? minParity = null;
        int? maxParity = null;
        bool allHaveParity = vectors.All(v => v.Symmetry.ParityEigenvalue.HasValue);
        if (allHaveParity)
        {
            var parities = vectors.Select(v => (int)v.Symmetry.ParityEigenvalue!.Value).ToList();
            minParity = parities.Min();
            maxParity = parities.Max();
        }

        return new SymmetryEnvelope
        {
            MinParity = minParity,
            MaxParity = maxParity,
            UnionLabels = unionLabels.OrderBy(l => l).ToList(),
        };
    }

    internal static InteractionProxyEnvelope? BuildInteractionProxyEnvelope(List<BosonPropertyVector> vectors)
    {
        if (vectors.Count == 0)
            return null;

        var allProxies = vectors.SelectMany(v => v.InteractionProxies).ToList();
        if (allProxies.Count == 0)
            return null;

        var absResponses = allProxies.Select(p => System.Math.Abs(p.CubicResponse)).ToList();

        return new InteractionProxyEnvelope
        {
            MinCubicResponse = absResponses.Min(),
            MaxCubicResponse = absResponses.Max(),
            ProxyCount = allProxies.Count,
        };
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
