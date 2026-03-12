using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Builds a <see cref="Phase4Report"/> from a fermion family atlas, coupling atlas summary,
/// and a unified particle registry.
///
/// Follows the Phase III ReportBuilder fluent accumulation pattern.
/// Call AddNegativeResult / AddCouplingMatrix / AddFamilyCluster to accumulate entries,
/// then Build().
/// </summary>
public sealed class Phase4ReportBuilder
{
    private readonly string _studyId;
    private readonly List<CouplingMatrixSummary> _couplingMatrices = new();
    private readonly List<FamilyClusterRecord> _familyClusters = new();
    private readonly List<string> _negativeResults = new();

    /// <summary>Threshold above which a family is considered "stable".</summary>
    private const double StabilityThreshold = 0.5;

    /// <summary>Noise floor below which a coupling entry is considered zero.</summary>
    private const double CouplingNoiseFloor = 1e-10;

    /// <summary>Maximum number of top candidates to include in the registry summary.</summary>
    private const int TopCandidateCount = 20;

    // Thresholds for negative result classification
    private const double BrokenClusterPersistenceThreshold = 0.3;
    private const double BrokenClusterAmbiguityThreshold = 0.7;
    private const double UnstableGaugeLeakThreshold = 0.3;

    public Phase4ReportBuilder(string studyId)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        _studyId = studyId;
    }

    /// <summary>Add a coupling matrix summary for one bosonic mode.</summary>
    public Phase4ReportBuilder AddCouplingMatrix(CouplingMatrixSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        _couplingMatrices.Add(summary);
        return this;
    }

    /// <summary>Add a family cluster record (from M41) for negative result analysis.</summary>
    public Phase4ReportBuilder AddFamilyCluster(FamilyClusterRecord cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        _familyClusters.Add(cluster);
        return this;
    }

    /// <summary>Add a negative result description (demotion note, incompatible comparison, etc.).</summary>
    public Phase4ReportBuilder AddNegativeResult(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        _negativeResults.Add(description);
        return this;
    }

    /// <summary>
    /// Build the Phase4Report from a fermion family atlas and unified particle registry.
    /// </summary>
    /// <param name="atlas">Fermion family atlas (output of M39/M41 pipeline).</param>
    /// <param name="registry">Unified particle registry (output of M42).</param>
    /// <param name="provenance">Provenance metadata for this report.</param>
    public Phase4Report Build(
        FermionFamilyAtlas atlas,
        UnifiedParticleRegistry registry,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(provenance);

        var fermionSummary = BuildFermionAtlasSummary(atlas);
        var couplingSummary = BuildCouplingAtlasSummary();
        var registrySummary = BuildUnifiedRegistrySummary(registry);
        var dashboard = BuildNegativeResultDashboard(atlas, registry);

        return new Phase4Report
        {
            ReportId = $"phase4-report-{_studyId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StudyId = _studyId,
            FermionAtlas = fermionSummary,
            CouplingAtlas = couplingSummary,
            RegistrySummary = registrySummary,
            NegativeResultDashboard = dashboard,
            NegativeResults = _negativeResults.ToList(),
            Provenance = provenance,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    // ------------------------------------------------------------------
    // Private builders
    // ------------------------------------------------------------------

    private NegativeResultDashboard BuildNegativeResultDashboard(
        FermionFamilyAtlas atlas,
        UnifiedParticleRegistry registry)
    {
        var unstableChirality = new List<UnstableChiralityEntry>();
        var fragileCouplings = new List<FragileCouplingEntry>();
        var brokenClusters = new List<BrokenFamilyClusterEntry>();
        var additionalNotes = new List<string>(_negativeResults);

        // Unstable chirality: mixed/trivial chirality or high gauge leakage
        foreach (var family in atlas.Families)
        {
            bool isMixedOrTrivial = family.DominantChiralityProfile is "mixed" or "undetermined" or "trivial";
            bool isHighLeakage = family.AverageGaugeLeakScore > UnstableGaugeLeakThreshold;
            bool isAmbiguous = family.AmbiguityNotes.Count > 0;

            if (isMixedOrTrivial || isHighLeakage || isAmbiguous)
            {
                string status = isMixedOrTrivial ? family.DominantChiralityProfile : "ambiguous";
                (double left, double right) = family.DominantChiralityProfile switch
                {
                    "left"  => (0.9, 0.1),
                    "right" => (0.1, 0.9),
                    _       => (0.5, 0.5),
                };
                string reason = isMixedOrTrivial
                    ? $"Chirality profile '{family.DominantChiralityProfile}' is not definite-left or definite-right."
                    : isHighLeakage
                        ? $"Gauge leakage score {family.AverageGaugeLeakScore:F3} exceeds threshold {UnstableGaugeLeakThreshold}."
                        : $"Family '{family.FamilyId}' has {family.AmbiguityNotes.Count} ambiguity note(s).";
                unstableChirality.Add(new UnstableChiralityEntry
                {
                    FamilyId = family.FamilyId,
                    ChiralityStatus = status,
                    LeftProjection = left,
                    RightProjection = right,
                    LeakageNorm = family.AverageGaugeLeakScore,
                    Reason = reason,
                });
            }
        }

        // Fragile couplings: below noise floor
        foreach (var matrix in _couplingMatrices)
        {
            if (matrix.MaxEntry <= CouplingNoiseFloor)
            {
                fragileCouplings.Add(new FragileCouplingEntry
                {
                    BosonModeId = matrix.BosonModeId,
                    MaxMagnitude = matrix.MaxEntry,
                    FrobeniusNorm = matrix.FrobeniusNorm,
                    FermionPairCount = matrix.FermionPairCount,
                    FragilityReason = "below-noise-floor",
                });
            }
        }

        // Broken family clusters: low persistence, high ambiguity, or split/merge notes
        foreach (var cluster in _familyClusters)
        {
            bool isLowPersistence = cluster.MeanBranchPersistence < BrokenClusterPersistenceThreshold;
            bool isHighAmbiguity = cluster.AmbiguityScore > BrokenClusterAmbiguityThreshold;
            bool hasSplitNote = cluster.ClusteringNotes.Any(n =>
                n.IndexOf("split", StringComparison.OrdinalIgnoreCase) >= 0);
            bool hasMergeNote = cluster.ClusteringNotes.Any(n =>
                n.IndexOf("merge", StringComparison.OrdinalIgnoreCase) >= 0);

            string? failureMode = null;
            if (isLowPersistence) failureMode = "low-persistence";
            else if (isHighAmbiguity) failureMode = "high-ambiguity";
            else if (hasSplitNote) failureMode = "split-detected";
            else if (hasMergeNote) failureMode = "merge-detected";

            if (failureMode is not null)
            {
                brokenClusters.Add(new BrokenFamilyClusterEntry
                {
                    ClusterId = cluster.ClusterId,
                    MeanPersistence = cluster.MeanBranchPersistence,
                    AmbiguityScore = cluster.AmbiguityScore,
                    MemberCount = cluster.MemberFamilyIds.Count,
                    FailureMode = failureMode,
                    DemotionReason = $"Cluster '{cluster.ClusterId}' failed with mode '{failureMode}'.",
                });
            }
        }

        // Collect registry demotions into additional notes
        foreach (var candidate in registry.Candidates)
        {
            foreach (var demotion in candidate.Demotions)
            {
                additionalNotes.Add(
                    $"[demotion] {candidate.ParticleId}: {demotion.Reason} — {demotion.Details} " +
                    $"({demotion.FromClaimClass} → {demotion.ToClaimClass})");
            }
        }

        return new NegativeResultDashboard
        {
            DashboardId = $"neg-dashboard-{_studyId}",
            StudyId = _studyId,
            UnstableChiralityEntries = unstableChirality,
            FragileCouplingEntries = fragileCouplings,
            BrokenFamilyClusterEntries = brokenClusters,
            AdditionalNotes = additionalNotes,
        };
    }

    private FermionAtlasSummary BuildFermionAtlasSummary(FermionFamilyAtlas atlas)
    {
        var familySheets = new List<FermionFamilySheet>();
        var chiralitySummaries = new List<ChiralitySummaryEntry>();
        var conjugationSummaries = new List<ConjugationSummaryEntry>();

        int stableCount = 0;
        int ambiguousCount = 0;

        foreach (var family in atlas.Families)
        {
            bool isStable = family.BranchPersistenceScore > StabilityThreshold;
            if (isStable) stableCount++;
            if (family.AmbiguityNotes.Count > 0) ambiguousCount++;

            double meanEv = family.EigenvalueMagnitudeEnvelope.Length >= 2
                ? family.EigenvalueMagnitudeEnvelope[1] : 0.0;
            double evSpread = family.EigenvalueMagnitudeEnvelope.Length >= 3
                ? family.EigenvalueMagnitudeEnvelope[2] - family.EigenvalueMagnitudeEnvelope[0] : 0.0;

            familySheets.Add(new FermionFamilySheet
            {
                FamilyId = family.FamilyId,
                MeanEigenvalue = meanEv,
                EigenvalueSpread = evSpread,
                MemberCount = family.MemberModeIds.Count,
                IsStable = isStable,
                ClaimClass = isStable ? "C1_ObservedMode" : "C0_NumericalMode",
                MemberModeIds = family.MemberModeIds.ToList(),
            });

            chiralitySummaries.Add(BuildChiralitySummaryEntry(family));
            conjugationSummaries.Add(BuildConjugationSummaryEntry(family));
        }

        return new FermionAtlasSummary
        {
            SummaryId = $"fermion-atlas-summary-{atlas.AtlasId}",
            FamilySheets = familySheets,
            ChiralitySummaries = chiralitySummaries,
            ConjugationSummaries = conjugationSummaries,
            TotalFamilies = atlas.Families.Count,
            StableFamilies = stableCount,
            AmbiguousFamilies = ambiguousCount,
        };
    }

    private static ChiralitySummaryEntry BuildChiralitySummaryEntry(FermionModeFamily family)
    {
        // Derive chirality projections from dominant chirality tag.
        // These are conservative estimates from the spectral record alone.
        double leftProj, rightProj;
        string status;

        switch (family.DominantChiralityProfile)
        {
            case "left":
                leftProj = 1.0; rightProj = 0.0;
                status = "definite-left";
                break;
            case "right":
                leftProj = 0.0; rightProj = 1.0;
                status = "definite-right";
                break;
            case "trivial":
                leftProj = 0.5; rightProj = 0.5;
                status = "trivial";
                break;
            case "mixed":
            default:
                leftProj = 0.5; rightProj = 0.5;
                status = "mixed";
                break;
        }

        return new ChiralitySummaryEntry
        {
            FamilyId = family.FamilyId,
            LeftProjection = leftProj,
            RightProjection = rightProj,
            LeakageNorm = family.AverageGaugeLeakScore,
            ChiralityType = "F",  // fermion-sector chirality
            ChiralityStatus = status,
            DiagnosticNotes = family.AmbiguityNotes.Count > 0
                ? family.AmbiguityNotes.ToList()
                : null,
        };
    }

    private static ConjugationSummaryEntry BuildConjugationSummaryEntry(FermionModeFamily family)
    {
        bool hasPair = family.ConjugateFamilyId != null;
        return new ConjugationSummaryEntry
        {
            FamilyId = family.FamilyId,
            HasConjugatePair = hasPair,
            PairedFamilyId = family.ConjugateFamilyId,
            PairingScore = hasPair ? family.BranchPersistenceScore : 0.0,
        };
    }

    private CouplingAtlasSummary BuildCouplingAtlasSummary()
    {
        int totalCouplings = _couplingMatrices.Sum(m => m.FermionPairCount);
        int nonzero = _couplingMatrices.Sum(m =>
            m.MaxEntry > CouplingNoiseFloor ? m.FermionPairCount : 0);
        double maxMag = _couplingMatrices.Count > 0
            ? _couplingMatrices.Max(m => m.MaxEntry)
            : 0.0;

        return new CouplingAtlasSummary
        {
            SummaryId = $"coupling-atlas-summary-{_studyId}",
            CouplingMatrices = _couplingMatrices.ToList(),
            TotalCouplings = totalCouplings,
            NonzeroCouplings = nonzero,
            MaxCouplingMagnitude = maxMag,
        };
    }

    private static UnifiedRegistrySummary BuildUnifiedRegistrySummary(UnifiedParticleRegistry registry)
    {
        int bosons = registry.QueryByType(UnifiedParticleType.Boson).Count;
        int fermions = registry.QueryByType(UnifiedParticleType.Fermion).Count;
        int interactions = registry.QueryByType(UnifiedParticleType.Interaction).Count;

        // Build claim class counts C0-C5
        var claimClassCounts = new Dictionary<string, int>();
        for (int level = 0; level <= 5; level++)
        {
            string key = $"C{level}";
            int count = registry.CountAboveClass(key) - (level < 5 ? registry.CountAboveClass($"C{level + 1}") : 0);
            if (count > 0)
                claimClassCounts[key] = count;
        }

        // Top candidates: highest claim class, then highest mean mass-like value
        var topCandidates = registry.Candidates
            .OrderByDescending(c => UnifiedParticleRegistry.ParseClaimClassLevel(c.ClaimClass))
            .ThenByDescending(c => c.MassLikeEnvelope.Length >= 2 ? c.MassLikeEnvelope[1] : 0.0)
            .Take(TopCandidateCount)
            .Select(c => new CandidateParticleSummary
            {
                CandidateId = c.ParticleId,
                ParticleType = c.ParticleType.ToString(),
                ClaimClass = c.ClaimClass,
                MassLikeValue = c.MassLikeEnvelope.Length >= 2 ? c.MassLikeEnvelope[1] : 0.0,
                DemotionCount = c.Demotions.Count,
            })
            .ToList();

        return new UnifiedRegistrySummary
        {
            SummaryId = $"registry-summary-{registry.RegistryVersion}",
            TotalBosons = bosons,
            TotalFermions = fermions,
            TotalInteractions = interactions,
            ClaimClassCounts = claimClassCounts,
            TopCandidates = topCandidates,
        };
    }
}
