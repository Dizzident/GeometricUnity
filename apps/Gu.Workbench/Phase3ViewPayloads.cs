using Gu.Phase3.Backgrounds;
using Gu.Phase3.ModeTracking;
using Gu.Phase3.Registry;
using Gu.Phase3.Spectra;
using Gu.VulkanViewer;

namespace Gu.Workbench;

// ─────────────────────────────────────────────────────────────────────────────
// Phase III diagnostic view payload types (GAP-2).
// All payloads are read-only snapshots — never modify source artifacts (IX-5).
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// One row in the BackgroundAtlasBrowserView listing.
/// </summary>
public sealed class BackgroundBrowserEntry
{
    public required string BackgroundId { get; init; }
    public required string EnvironmentId { get; init; }
    public required string AdmissibilityLevel { get; init; }
    public required double ResidualNorm { get; init; }
    public required double StationarityNorm { get; init; }
    public IReadOnlyDictionary<string, double>? ContinuationCoordinates { get; init; }
}

/// <summary>
/// View 1 — Background Atlas Browser: lists all backgrounds with IDs and key parameters.
/// </summary>
public sealed class BackgroundAtlasBrowserView : ViewPayload
{
    public override string ViewType => "p3_background_atlas_browser";

    public required string AtlasId { get; init; }
    public required string StudyId { get; init; }
    public required IReadOnlyList<BackgroundBrowserEntry> Admitted { get; init; }
    public required IReadOnlyList<BackgroundBrowserEntry> Rejected { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Background Atlas: {AtlasId} (study={StudyId}) ===");
        sb.AppendLine($"Admitted: {Admitted.Count}  Rejected: {Rejected.Count}");
        sb.AppendLine();
        sb.AppendLine("ADMITTED:");
        foreach (var b in Admitted)
        {
            string coords = b.ContinuationCoordinates is { Count: > 0 }
                ? " [" + string.Join(", ", b.ContinuationCoordinates.Select(kv => $"{kv.Key}={kv.Value:G4}")) + "]"
                : "";
            sb.AppendLine($"  {b.BackgroundId} ({b.EnvironmentId}) [{b.AdmissibilityLevel}]  resid={b.ResidualNorm:E3}  stat={b.StationarityNorm:E3}{coords}");
        }
        if (Rejected.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("REJECTED:");
            foreach (var b in Rejected)
                sb.AppendLine($"  {b.BackgroundId} ({b.EnvironmentId})  resid={b.ResidualNorm:E3}");
        }
        return sb.ToString();
    }

    public static BackgroundAtlasBrowserView FromAtlas(BackgroundAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        return new BackgroundAtlasBrowserView
        {
            AtlasId = atlas.AtlasId,
            StudyId = atlas.StudyId,
            Admitted = atlas.Backgrounds.Select(b => new BackgroundBrowserEntry
            {
                BackgroundId = b.BackgroundId,
                EnvironmentId = b.EnvironmentId,
                AdmissibilityLevel = b.AdmissibilityLevel.ToString(),
                ResidualNorm = b.ResidualNorm,
                StationarityNorm = b.StationarityNorm,
                ContinuationCoordinates = b.ContinuationCoordinates,
            }).ToList(),
            Rejected = atlas.RejectedBackgrounds.Select(b => new BackgroundBrowserEntry
            {
                BackgroundId = b.BackgroundId,
                EnvironmentId = b.EnvironmentId,
                AdmissibilityLevel = b.AdmissibilityLevel.ToString(),
                ResidualNorm = b.ResidualNorm,
                StationarityNorm = b.StationarityNorm,
                ContinuationCoordinates = b.ContinuationCoordinates,
            }).ToList(),
        };
    }
}

/// <summary>
/// One rung in a spectral ladder (one eigenvalue for one background).
/// </summary>
public sealed class SpectralLadderRung
{
    public required string BackgroundId { get; init; }
    public required int ModeIndex { get; init; }
    public required string ModeId { get; init; }
    public required double Eigenvalue { get; init; }
    public required double GaugeLeakScore { get; init; }
    public required double ResidualNorm { get; init; }
}

/// <summary>
/// View 2 — Spectral Ladder: eigenvalues per background in ascending order.
/// </summary>
public sealed class SpectralLadderView : ViewPayload
{
    public override string ViewType => "p3_spectral_ladder";

    public required IReadOnlyList<SpectralLadderRung> Rungs { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Spectral Ladder ===");
        var groups = Rungs.GroupBy(r => r.BackgroundId);
        foreach (var g in groups)
        {
            sb.AppendLine($"  Background: {g.Key}");
            foreach (var r in g.OrderBy(x => x.Eigenvalue))
                sb.AppendLine($"    [{r.ModeIndex}] lambda={r.Eigenvalue:G6}  leak={r.GaugeLeakScore:G3}  resid={r.ResidualNorm:E2}");
        }
        return sb.ToString();
    }

    public static SpectralLadderView FromBundles(IReadOnlyList<SpectrumBundle> bundles)
    {
        ArgumentNullException.ThrowIfNull(bundles);
        var rungs = new List<SpectralLadderRung>();
        foreach (var bundle in bundles)
        {
            foreach (var mode in bundle.Modes.OrderBy(m => m.Eigenvalue))
            {
                rungs.Add(new SpectralLadderRung
                {
                    BackgroundId = bundle.BackgroundId,
                    ModeIndex = mode.ModeIndex,
                    ModeId = mode.ModeId,
                    Eigenvalue = mode.Eigenvalue,
                    GaugeLeakScore = mode.GaugeLeakScore,
                    ResidualNorm = mode.ResidualNorm,
                });
            }
        }
        return new SpectralLadderView { Rungs = rungs };
    }
}

/// <summary>
/// Amplitude statistics for a single eigenmode.
/// </summary>
public sealed class EigenModeAmplitudeEntry
{
    public required string ModeId { get; init; }
    public required string BackgroundId { get; init; }
    public required int ModeIndex { get; init; }
    public required double Eigenvalue { get; init; }
    public required double MinAmplitude { get; init; }
    public required double MaxAmplitude { get; init; }
    public required double MeanAmplitude { get; init; }
    public required double L2Norm { get; init; }
    public required int VectorLength { get; init; }
}

/// <summary>
/// View 3 — EigenMode Amplitude: per-mode amplitude stats (min/max/mean of eigenvector components).
/// </summary>
public sealed class EigenModeAmplitudeView : ViewPayload
{
    public override string ViewType => "p3_eigenmode_amplitude";

    public required IReadOnlyList<EigenModeAmplitudeEntry> Entries { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== EigenMode Amplitude Statistics ===");
        sb.AppendLine($"{"ModeId",-30} {"lambda",12} {"min",10} {"max",10} {"mean",10} {"L2",10}");
        foreach (var e in Entries)
            sb.AppendLine($"{e.ModeId,-30} {e.Eigenvalue,12:G4} {e.MinAmplitude,10:G4} {e.MaxAmplitude,10:G4} {e.MeanAmplitude,10:G4} {e.L2Norm,10:G4}");
        return sb.ToString();
    }

    public static EigenModeAmplitudeView FromBundles(IReadOnlyList<SpectrumBundle> bundles)
    {
        ArgumentNullException.ThrowIfNull(bundles);
        var entries = new List<EigenModeAmplitudeEntry>();
        foreach (var bundle in bundles)
        {
            foreach (var mode in bundle.Modes)
            {
                var v = mode.ModeVector;
                double min = v.Length > 0 ? v.Min() : 0.0;
                double max = v.Length > 0 ? v.Max() : 0.0;
                double mean = v.Length > 0 ? v.Average() : 0.0;
                double l2 = v.Length > 0 ? System.Math.Sqrt(v.Sum(x => x * x)) : 0.0;
                entries.Add(new EigenModeAmplitudeEntry
                {
                    ModeId = mode.ModeId,
                    BackgroundId = bundle.BackgroundId,
                    ModeIndex = mode.ModeIndex,
                    Eigenvalue = mode.Eigenvalue,
                    MinAmplitude = min,
                    MaxAmplitude = max,
                    MeanAmplitude = mean,
                    L2Norm = l2,
                    VectorLength = v.Length,
                });
            }
        }
        return new EigenModeAmplitudeView { Entries = entries };
    }
}

/// <summary>
/// Gauge leak summary for a single spectrum.
/// </summary>
public sealed class GaugeLeakEntry
{
    public required string SpectrumId { get; init; }
    public required string BackgroundId { get; init; }
    public required int ModeCount { get; init; }
    public required double MaxGaugeLeak { get; init; }
    public required double MeanGaugeLeak { get; init; }
    public required int HighLeakCount { get; init; }
    public required double MaxOrthogonalityDefect { get; init; }
    public required string ConvergenceStatus { get; init; }
}

/// <summary>
/// View 4 — Gauge Leak: constraint defect summary per spectrum.
/// </summary>
public sealed class GaugeLeakView : ViewPayload
{
    public override string ViewType => "p3_gauge_leak";

    public required IReadOnlyList<GaugeLeakEntry> Entries { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Gauge Leak / Constraint Defect ===");
        sb.AppendLine($"{"SpectrumId",-28} {"bgId",-20} {"modes",6} {"maxLeak",10} {"meanLeak",10} {"highLeak",9} {"orthDef",10} {"conv",-15}");
        foreach (var e in Entries)
            sb.AppendLine($"{e.SpectrumId,-28} {e.BackgroundId,-20} {e.ModeCount,6} {e.MaxGaugeLeak,10:G4} {e.MeanGaugeLeak,10:G4} {e.HighLeakCount,9} {e.MaxOrthogonalityDefect,10:E2} {e.ConvergenceStatus,-15}");
        return sb.ToString();
    }

    public static GaugeLeakView FromBundles(IReadOnlyList<SpectrumBundle> bundles, double leakThreshold = 0.1)
    {
        ArgumentNullException.ThrowIfNull(bundles);
        var entries = new List<GaugeLeakEntry>();
        foreach (var bundle in bundles)
        {
            var leaks = bundle.Modes.Select(m => m.GaugeLeakScore).ToArray();
            double maxLeak = leaks.Length > 0 ? leaks.Max() : 0.0;
            double meanLeak = leaks.Length > 0 ? leaks.Average() : 0.0;
            int highLeak = leaks.Count(l => l > leakThreshold);
            entries.Add(new GaugeLeakEntry
            {
                SpectrumId = bundle.SpectrumId,
                BackgroundId = bundle.BackgroundId,
                ModeCount = bundle.Modes.Count,
                MaxGaugeLeak = maxLeak,
                MeanGaugeLeak = meanLeak,
                HighLeakCount = highLeak,
                MaxOrthogonalityDefect = bundle.MaxOrthogonalityDefect,
                ConvergenceStatus = bundle.ConvergenceStatus,
            });
        }
        return new GaugeLeakView { Entries = entries };
    }
}

/// <summary>
/// One track entry: a mode family across branches.
/// </summary>
public sealed class BranchModeTrackEntry
{
    public required string FamilyId { get; init; }
    public required int MemberCount { get; init; }
    public required int ContextCount { get; init; }
    public required double MeanEigenvalue { get; init; }
    public required double EigenvalueSpread { get; init; }
    public required bool IsStable { get; init; }
    public required int AmbiguityCount { get; init; }
    public required IReadOnlyList<string> ContextIds { get; init; }
}

/// <summary>
/// View 5 — Branch Mode Track: mode family tracks across branches.
/// </summary>
public sealed class BranchModeTrackView : ViewPayload
{
    public override string ViewType => "p3_branch_mode_track";

    public required IReadOnlyList<BranchModeTrackEntry> Tracks { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Branch Mode Tracking ===");
        sb.AppendLine($"{"FamilyId",-30} {"members",8} {"contexts",9} {"meanEV",12} {"spread",10} {"stable",7} {"ambig",6}");
        foreach (var t in Tracks)
        {
            sb.AppendLine($"{t.FamilyId,-30} {t.MemberCount,8} {t.ContextCount,9} {t.MeanEigenvalue,12:G5} {t.EigenvalueSpread,10:G4} {t.IsStable,7} {t.AmbiguityCount,6}");
        }
        return sb.ToString();
    }

    public static BranchModeTrackView FromFamilies(IReadOnlyList<ModeFamilyRecord> families)
    {
        ArgumentNullException.ThrowIfNull(families);
        var tracks = families.Select(f => new BranchModeTrackEntry
        {
            FamilyId = f.FamilyId,
            MemberCount = f.MemberModeIds.Count,
            ContextCount = f.ContextIds.Count,
            MeanEigenvalue = f.MeanEigenvalue,
            EigenvalueSpread = f.EigenvalueSpread,
            IsStable = f.IsStable,
            AmbiguityCount = f.AmbiguityCount,
            ContextIds = f.ContextIds,
        }).ToList();
        return new BranchModeTrackView { Tracks = tracks };
    }
}

/// <summary>
/// One card for a boson family candidate.
/// </summary>
public sealed class BosonFamilyCard
{
    public required string CandidateId { get; init; }
    public required string PrimaryFamilyId { get; init; }
    public required string ClaimClass { get; init; }
    public required double MassLikeScaleMean { get; init; }
    public required double MassLikeScaleMin { get; init; }
    public required double MassLikeScaleMax { get; init; }
    public required double GaugeLeakMean { get; init; }
    public required int BackgroundCount { get; init; }
    public required int DemotionCount { get; init; }
    public string? PolarizationClass { get; init; }
}

/// <summary>
/// View 6 — Boson Family Card: one card per boson family showing ClaimClass, mass-like scale, polarization.
/// </summary>
public sealed class BosonFamilyCardView : ViewPayload
{
    public override string ViewType => "p3_boson_family_card";

    public required IReadOnlyList<BosonFamilyCard> Cards { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Boson Family Cards ===");
        sb.AppendLine($"{"CandidateId",-28} {"ClaimClass",-32} {"massMean",10} {"massMin",10} {"massMax",10} {"leakMean",10} {"bgs",4} {"demot",6} {"polariz",-12}");
        foreach (var c in Cards)
        {
            sb.AppendLine($"{c.CandidateId,-28} {c.ClaimClass,-32} {c.MassLikeScaleMean,10:G4} {c.MassLikeScaleMin,10:G4} {c.MassLikeScaleMax,10:G4} {c.GaugeLeakMean,10:G4} {c.BackgroundCount,4} {c.DemotionCount,6} {c.PolarizationClass ?? "N/A",-12}");
        }
        return sb.ToString();
    }

    public static BosonFamilyCardView FromRegistry(BosonRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        var cards = registry.Candidates.Select(c => new BosonFamilyCard
        {
            CandidateId = c.CandidateId,
            PrimaryFamilyId = c.PrimaryFamilyId,
            ClaimClass = c.ClaimClass.ToString(),
            MassLikeScaleMean = c.MassLikeEnvelope.Length > 1 ? c.MassLikeEnvelope[1] : (c.MassLikeEnvelope.Length > 0 ? c.MassLikeEnvelope[0] : 0.0),
            MassLikeScaleMin = c.MassLikeEnvelope.Length > 0 ? c.MassLikeEnvelope[0] : 0.0,
            MassLikeScaleMax = c.MassLikeEnvelope.Length > 2 ? c.MassLikeEnvelope[2] : (c.MassLikeEnvelope.Length > 0 ? c.MassLikeEnvelope[0] : 0.0),
            GaugeLeakMean = c.GaugeLeakEnvelope.Length > 1 ? c.GaugeLeakEnvelope[1] : (c.GaugeLeakEnvelope.Length > 0 ? c.GaugeLeakEnvelope[0] : 0.0),
            BackgroundCount = c.BackgroundSet.Count,
            DemotionCount = c.Demotions.Count,
            PolarizationClass = c.PolarizationEnvelope?.DominantClass,
        }).ToList();
        return new BosonFamilyCardView { Cards = cards };
    }
}

/// <summary>
/// One row in the observed signature view.
/// </summary>
public sealed class ObservedSignatureEntry
{
    public required string ModeId { get; init; }
    public required string BackgroundId { get; init; }
    public required double Eigenvalue { get; init; }
    public required bool HasSignatureRef { get; init; }
    public required IReadOnlyDictionary<string, double> TensorEnergyFractions { get; init; }
    public required IReadOnlyDictionary<string, double> BlockEnergyFractions { get; init; }
}

/// <summary>
/// View 7 — Observed Signature: field component overlaps per mode.
/// </summary>
public sealed class ObservedSignatureView : ViewPayload
{
    public override string ViewType => "p3_observed_signature";

    public required IReadOnlyList<ObservedSignatureEntry> Entries { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Observed Mode Signatures ===");
        foreach (var e in Entries)
        {
            sb.AppendLine($"  Mode {e.ModeId} (bg={e.BackgroundId}, lambda={e.Eigenvalue:G4}, sigRef={e.HasSignatureRef})");
            if (e.TensorEnergyFractions.Count > 0)
            {
                sb.Append("    TensorEnergy: ");
                sb.AppendLine(string.Join(", ", e.TensorEnergyFractions.Select(kv => $"{kv.Key}={kv.Value:G3}")));
            }
            if (e.BlockEnergyFractions.Count > 0)
            {
                sb.Append("    BlockEnergy:  ");
                sb.AppendLine(string.Join(", ", e.BlockEnergyFractions.Select(kv => $"{kv.Key}={kv.Value:G3}")));
            }
        }
        return sb.ToString();
    }

    public static ObservedSignatureView FromBundles(IReadOnlyList<SpectrumBundle> bundles)
    {
        ArgumentNullException.ThrowIfNull(bundles);
        var entries = new List<ObservedSignatureEntry>();
        foreach (var bundle in bundles)
        {
            foreach (var mode in bundle.Modes)
            {
                entries.Add(new ObservedSignatureEntry
                {
                    ModeId = mode.ModeId,
                    BackgroundId = bundle.BackgroundId,
                    Eigenvalue = mode.Eigenvalue,
                    HasSignatureRef = mode.ObservedSignatureRef is not null,
                    TensorEnergyFractions = (IReadOnlyDictionary<string, double>?)mode.TensorEnergyFractions
                        ?? new Dictionary<string, double>(),
                    BlockEnergyFractions = (IReadOnlyDictionary<string, double>?)mode.BlockEnergyFractions
                        ?? new Dictionary<string, double>(),
                });
            }
        }
        return new ObservedSignatureView { Entries = entries };
    }
}

/// <summary>
/// One cell in the ambiguity heatmap.
/// </summary>
public sealed class AmbiguityCell
{
    public required string SourceModeId { get; init; }
    public required string TargetModeId { get; init; }
    public required double AggregateScore { get; init; }
    public required string AlignmentType { get; init; }
    public required double Confidence { get; init; }
}

/// <summary>
/// View 8 — Ambiguity Heatmap: mode matching scores in a matrix format (ambiguity > threshold).
/// </summary>
public sealed class AmbiguityHeatmapView : ViewPayload
{
    public override string ViewType => "p3_ambiguity_heatmap";

    public required IReadOnlyList<AmbiguityCell> Cells { get; init; }
    public required double AmbiguityThreshold { get; init; }

    public string Print()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== Ambiguity Heatmap (threshold={AmbiguityThreshold:G3}) ===");
        if (Cells.Count == 0)
        {
            sb.AppendLine("  (no cells above threshold)");
            return sb.ToString();
        }
        sb.AppendLine($"{"Source",-30} {"Target",-30} {"Score",8} {"Type",-20} {"Conf",7}");
        foreach (var c in Cells.OrderByDescending(x => x.AggregateScore))
            sb.AppendLine($"{c.SourceModeId,-30} {c.TargetModeId,-30} {c.AggregateScore,8:G4} {c.AlignmentType,-20} {c.Confidence,7:G3}");
        return sb.ToString();
    }

    public static AmbiguityHeatmapView FromFamilies(
        IReadOnlyList<ModeFamilyRecord> families,
        double ambiguityThreshold = 0.4)
    {
        ArgumentNullException.ThrowIfNull(families);
        var cells = new List<AmbiguityCell>();
        foreach (var family in families)
        {
            foreach (var alignment in family.Alignments)
            {
                if (alignment.Metrics.AggregateScore >= ambiguityThreshold)
                {
                    cells.Add(new AmbiguityCell
                    {
                        SourceModeId = alignment.SourceModeId,
                        TargetModeId = alignment.TargetModeId,
                        AggregateScore = alignment.Metrics.AggregateScore,
                        AlignmentType = alignment.AlignmentType,
                        Confidence = alignment.Confidence,
                    });
                }
            }
        }
        return new AmbiguityHeatmapView
        {
            Cells = cells,
            AmbiguityThreshold = ambiguityThreshold,
        };
    }
}
