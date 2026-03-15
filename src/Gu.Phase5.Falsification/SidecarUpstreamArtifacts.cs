namespace Gu.Phase5.Falsification;

/// <summary>
/// Optional persisted artifact references that can upgrade sidecar generation from
/// heuristic derivation to upstream-sourced or bridge-derived evidence.
/// </summary>
public sealed class SidecarUpstreamArtifacts
{
    public string? RegistryPath { get; init; }

    public string? ObservablesPath { get; init; }

    public IReadOnlyList<string>? EnvironmentRecordPaths { get; init; }

    public string? FermionFamilyAtlasPath { get; init; }

    public string? CouplingAtlasPath { get; init; }

    public string? FermionSpectralResultPath { get; init; }

    public string? Phase4ReportPath { get; init; }
}
