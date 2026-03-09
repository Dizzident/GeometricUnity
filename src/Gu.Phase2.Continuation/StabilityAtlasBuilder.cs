using Gu.Phase2.Stability;

namespace Gu.Phase2.Continuation;

/// <summary>
/// Builds a <see cref="StabilityAtlas"/> from continuation runs, Hessian records,
/// and principal-symbol samples.
/// </summary>
public sealed class StabilityAtlasBuilder
{
    private readonly string _atlasId;
    private readonly string _branchManifestId;
    private readonly string _familyDescription;
    private readonly List<ContinuationResult> _paths = new();
    private readonly List<HessianRecord> _hessianRecords = new();
    private readonly List<PrincipalSymbolRecord> _symbolSamples = new();
    private readonly List<LinearizationRecord> _linearizationRecords = new();
    private readonly List<GaugeFixedLinearizationRecord> _gaugeFixedLinearizationRecords = new();
    private string? _discretizationNotes;
    private string? _theoremStatusNotes;

    public StabilityAtlasBuilder(string atlasId, string branchManifestId, string familyDescription)
    {
        _atlasId = atlasId ?? throw new ArgumentNullException(nameof(atlasId));
        _branchManifestId = branchManifestId ?? throw new ArgumentNullException(nameof(branchManifestId));
        _familyDescription = familyDescription ?? throw new ArgumentNullException(nameof(familyDescription));
    }

    public StabilityAtlasBuilder AddPath(ContinuationResult path)
    {
        _paths.Add(path ?? throw new ArgumentNullException(nameof(path)));
        return this;
    }

    public StabilityAtlasBuilder AddHessianRecord(HessianRecord record)
    {
        _hessianRecords.Add(record ?? throw new ArgumentNullException(nameof(record)));
        return this;
    }

    public StabilityAtlasBuilder AddSymbolSample(PrincipalSymbolRecord record)
    {
        _symbolSamples.Add(record ?? throw new ArgumentNullException(nameof(record)));
        return this;
    }

    public StabilityAtlasBuilder AddLinearizationRecord(LinearizationRecord record)
    {
        _linearizationRecords.Add(record ?? throw new ArgumentNullException(nameof(record)));
        return this;
    }

    public StabilityAtlasBuilder AddGaugeFixedLinearizationRecord(GaugeFixedLinearizationRecord record)
    {
        _gaugeFixedLinearizationRecords.Add(record ?? throw new ArgumentNullException(nameof(record)));
        return this;
    }

    public StabilityAtlasBuilder WithDiscretizationNotes(string notes)
    {
        _discretizationNotes = notes;
        return this;
    }

    public StabilityAtlasBuilder WithTheoremStatusNotes(string notes)
    {
        _theoremStatusNotes = notes;
        return this;
    }

    /// <summary>
    /// Build the stability atlas. Extracts bifurcation indicators from continuation events
    /// and converts them to typed <see cref="BifurcationIndicatorRecord"/> objects.
    /// </summary>
    public StabilityAtlas Build()
    {
        // Extract bifurcation-relevant events from all paths
        var bifurcationKinds = new HashSet<ContinuationEventKind>
        {
            ContinuationEventKind.SingularValueCollapse,
            ContinuationEventKind.HessianSignChange,
            ContinuationEventKind.BranchMergeSplitCandidate,
        };

        var bifurcationIndicators = _paths
            .SelectMany(p => p.AllEvents)
            .Where(e => bifurcationKinds.Contains(e.Kind))
            .Select((e, i) => ConvertToIndicator(e, i))
            .ToList();

        return new StabilityAtlas
        {
            AtlasId = _atlasId,
            BranchManifestId = _branchManifestId,
            FamilyDescription = _familyDescription,
            Paths = _paths,
            HessianRecords = _hessianRecords,
            SymbolSamples = _symbolSamples,
            LinearizationRecords = _linearizationRecords,
            GaugeFixedLinearizationRecords = _gaugeFixedLinearizationRecords,
            BifurcationIndicators = bifurcationIndicators,
            DiscretizationNotes = _discretizationNotes,
            TheoremStatusNotes = _theoremStatusNotes,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Infer bifurcation kind from <see cref="ContinuationEventKind"/>.
    /// </summary>
    public static string InferKind(ContinuationEventKind eventKind) => eventKind switch
    {
        ContinuationEventKind.SingularValueCollapse => "fold",
        ContinuationEventKind.HessianSignChange => "sign-change",
        ContinuationEventKind.BranchMergeSplitCandidate => "branch-point",
        _ => "unknown",
    };

    private static BifurcationIndicatorRecord ConvertToIndicator(ContinuationEvent e, int index)
    {
        return new BifurcationIndicatorRecord
        {
            IndicatorId = $"bif-{index}",
            Lambda = e.Lambda,
            Kind = InferKind(e.Kind),
            TriggeringEvent = e.Kind,
            Confidence = "numerical-only",
            TheoremDependencyStatus = "unverified",
            Description = e.Description,
        };
    }
}
