using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.ModeTracking;
using Gu.Phase3.Registry;
using Gu.Phase3.Spectra;
using Gu.VulkanViewer;

namespace Gu.Workbench;

/// <summary>
/// Loads Phase III artifacts from the canonical run folder layout and prepares
/// all 8 diagnostic view payloads. All operations are READ-ONLY (IX-5).
/// </summary>
public sealed class Phase3ArtifactLoader
{
    /// <summary>
    /// Load all available Phase III artifacts from a run folder and build
    /// a Phase3WorkbenchSnapshot.
    /// </summary>
    /// <param name="runFolderPath">Root of the run folder.</param>
    public Phase3WorkbenchSnapshot LoadPhase3Folder(string runFolderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runFolderPath);

        var atlas = TryLoadAtlas(runFolderPath);
        var spectra = LoadSpectra(runFolderPath);
        var families = TryLoadModeFamilies(runFolderPath);
        var registry = TryLoadBosonRegistry(runFolderPath);

        return new Phase3WorkbenchSnapshot
        {
            RunFolderPath = runFolderPath,
            BackgroundAtlas = atlas,
            Spectra = spectra,
            ModeFamilies = families,
            BosonRegistry = registry,
            LoadedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Build all 8 Phase III view payloads from a loaded snapshot.
    /// Returns only views for which data is available.
    /// </summary>
    public IReadOnlyList<ViewPayload> PreparePhase3Views(Phase3WorkbenchSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var views = new List<ViewPayload>();

        if (snapshot.BackgroundAtlas is not null)
            views.Add(BackgroundAtlasBrowserView.FromAtlas(snapshot.BackgroundAtlas));

        if (snapshot.Spectra.Count > 0)
        {
            views.Add(SpectralLadderView.FromBundles(snapshot.Spectra));
            views.Add(EigenModeAmplitudeView.FromBundles(snapshot.Spectra));
            views.Add(GaugeLeakView.FromBundles(snapshot.Spectra));
            views.Add(ObservedSignatureView.FromBundles(snapshot.Spectra));
        }

        if (snapshot.ModeFamilies.Count > 0)
        {
            views.Add(BranchModeTrackView.FromFamilies(snapshot.ModeFamilies));
            views.Add(AmbiguityHeatmapView.FromFamilies(snapshot.ModeFamilies));
        }

        if (snapshot.BosonRegistry is not null)
            views.Add(BosonFamilyCardView.FromRegistry(snapshot.BosonRegistry));

        return views;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private loaders — all return null / empty if files are absent.
    // ─────────────────────────────────────────────────────────────────────────

    private static BackgroundAtlas? TryLoadAtlas(string root)
    {
        var path = Path.Combine(root, "backgrounds", "atlas.json");
        if (!File.Exists(path))
            return null;
        var json = File.ReadAllText(path);
        return BackgroundAtlasSerializer.DeserializeAtlas(json);
    }

    private static IReadOnlyList<SpectrumBundle> LoadSpectra(string root)
    {
        var spectraDir = Path.Combine(root, "spectra");
        if (!Directory.Exists(spectraDir))
            return Array.Empty<SpectrumBundle>();

        var bundles = new List<SpectrumBundle>();
        foreach (var file in Directory.EnumerateFiles(spectraDir, "*_spectrum.json"))
        {
            var json = File.ReadAllText(file);
            var bundle = GuJsonDefaults.Deserialize<SpectrumBundle>(json);
            if (bundle is not null)
                bundles.Add(bundle);
        }
        // Also accept spectrum_*.json files
        foreach (var file in Directory.EnumerateFiles(spectraDir, "spectrum_*.json"))
        {
            var json = File.ReadAllText(file);
            var bundle = GuJsonDefaults.Deserialize<SpectrumBundle>(json);
            if (bundle is not null && !bundles.Any(b => b.SpectrumId == bundle.SpectrumId))
                bundles.Add(bundle);
        }
        return bundles;
    }

    private static IReadOnlyList<ModeFamilyRecord> TryLoadModeFamilies(string root)
    {
        var path = Path.Combine(root, "modes", "mode_families.json");
        if (!File.Exists(path))
            return Array.Empty<ModeFamilyRecord>();
        var json = File.ReadAllText(path);
        return GuJsonDefaults.Deserialize<ModeFamilyRecord[]>(json)
               ?? Array.Empty<ModeFamilyRecord>();
    }

    private static BosonRegistry? TryLoadBosonRegistry(string root)
    {
        var path = Path.Combine(root, "bosons", "registry.json");
        if (!File.Exists(path))
            return null;
        var json = File.ReadAllText(path);
        return BosonRegistry.FromJson(json);
    }
}

/// <summary>
/// Immutable snapshot of all Phase III artifact data loaded from a run folder.
/// </summary>
public sealed class Phase3WorkbenchSnapshot
{
    public required string RunFolderPath { get; init; }
    public BackgroundAtlas? BackgroundAtlas { get; init; }
    public IReadOnlyList<SpectrumBundle> Spectra { get; init; } = Array.Empty<SpectrumBundle>();
    public IReadOnlyList<ModeFamilyRecord> ModeFamilies { get; init; } = Array.Empty<ModeFamilyRecord>();
    public BosonRegistry? BosonRegistry { get; init; }
    public required DateTimeOffset LoadedAt { get; init; }
}
