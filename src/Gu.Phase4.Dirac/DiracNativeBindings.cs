using System.Runtime.InteropServices;

namespace Gu.Phase4.Dirac;

internal static partial class DiracNativeBindings
{
    private const string LibName = "gu_cuda_core";

    [LibraryImport(LibName, EntryPoint = "gu_dirac_upload_gammas")]
    internal static partial int UploadGammas(
        nint gammaRe,
        nint gammaIm,
        nint chiralityRe,
        nint chiralityIm,
        int spacetimeDim,
        int spinorDim);

    [LibraryImport(LibName, EntryPoint = "gu_dirac_gamma_action_gpu")]
    internal static partial int ApplyGamma(
        nint spinorIn,
        nint resultOut,
        int mu,
        int cellCount,
        int spinorDim,
        int gaugeDim);

    [LibraryImport(LibName, EntryPoint = "gu_dirac_apply_gpu")]
    internal static partial int ApplyDirac(
        nint spinorIn,
        nint resultOut,
        nint edgeDirectionCoeff,
        nint vertexEdgeIncidence,
        nint vertexEdgeOrient,
        nint edgeVertices,
        int vertexCount,
        int edgeCount,
        int cellCount,
        int spinorDim,
        int gaugeDim,
        int maxEdgesPerVertex,
        int spacetimeDim);

    [LibraryImport(LibName, EntryPoint = "gu_dirac_mass_apply_gpu")]
    internal static partial int ApplyMass(
        nint spinorIn,
        nint resultOut,
        nint cellVolumes,
        int spinorDof,
        int cellCount,
        int spinorDim,
        int gaugeDim);

    [LibraryImport(LibName, EntryPoint = "gu_dirac_chirality_project_gpu")]
    internal static partial int ApplyChiralityProjector(
        nint spinorIn,
        nint resultOut,
        int left,
        int cellCount,
        int spinorDim,
        int gaugeDim);

    [LibraryImport(LibName, EntryPoint = "gu_dirac_coupling_proxy_gpu")]
    internal static partial int ComputeCouplingProxy(
        nint spinorI,
        nint spinorJ,
        nint bosonK,
        out double resultRe,
        out double resultIm,
        int spinorDof,
        int edgeCount,
        int cellCount,
        int spinorDim,
        int gaugeDim,
        int spacetimeDim);
}
