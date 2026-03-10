using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Target profile for Mode BC1 (internal comparison).
///
/// Internal profiles describe abstract structural expectations derived
/// from the GU framework itself, not from external particle data.
/// </summary>
public sealed class InternalTargetProfile : TargetProfile
{
    /// <summary>
    /// Internal profile type identifier:
    /// "massless-vector", "massive-scalar", "branch-stable-mixed-tensor", "two-polarization-low-leak".
    /// </summary>
    [JsonPropertyName("internalType")]
    public required string InternalType { get; init; }

    /// <summary>Create a "massless-vector" internal target profile.</summary>
    public static InternalTargetProfile MasslessVector() => new()
    {
        ProfileId = "internal-massless-vector",
        ProfileName = "Massless Vector-Like",
        ExpectedMassRange = [0.0, 1e-6],
        ExpectedMultiplicity = 2,
        ExpectedPolarizationType = "transverse",
        ExpectedSymmetryGroup = "u(1)",
        InternalType = "massless-vector",
        Tolerances = new TargetTolerances
        {
            MassTolerance = 1e-4,
            MultiplicityTolerance = 0,
            MaxGaugeLeakForCompatibility = 0.05,
        },
    };

    /// <summary>Create a "massive-scalar" internal target profile.</summary>
    public static InternalTargetProfile MassiveScalar() => new()
    {
        ProfileId = "internal-massive-scalar",
        ProfileName = "Massive Scalar-Like",
        ExpectedMassRange = [0.01, 100.0],
        ExpectedMultiplicity = 1,
        ExpectedPolarizationType = "scalar",
        ExpectedSymmetryGroup = "trivial",
        InternalType = "massive-scalar",
        Tolerances = new TargetTolerances
        {
            MassTolerance = 0.2,
            MultiplicityTolerance = 0,
            MaxGaugeLeakForCompatibility = 0.10,
        },
    };

    /// <summary>Create a "branch-stable-mixed-tensor" internal target profile.</summary>
    public static InternalTargetProfile BranchStableMixedTensor() => new()
    {
        ProfileId = "internal-branch-stable-mixed-tensor",
        ProfileName = "Branch-Stable Mixed Tensor",
        ExpectedMassRange = [0.0, 1000.0],
        ExpectedMultiplicity = 3,
        ExpectedPolarizationType = "mixed",
        ExpectedSymmetryGroup = "su(2)",
        InternalType = "branch-stable-mixed-tensor",
        Tolerances = new TargetTolerances
        {
            MassTolerance = 0.3,
            MultiplicityTolerance = 1,
            MaxGaugeLeakForCompatibility = 0.15,
            MinBranchStability = 0.6,
        },
    };

    /// <summary>Create a "two-polarization-low-leak" internal target profile.</summary>
    public static InternalTargetProfile TwoPolarizationLowLeak() => new()
    {
        ProfileId = "internal-two-polarization-low-leak",
        ProfileName = "Two-Polarization Low Leak",
        ExpectedMassRange = [0.0, 50.0],
        ExpectedMultiplicity = 2,
        ExpectedPolarizationType = "transverse",
        ExpectedSymmetryGroup = "u(1)",
        InternalType = "two-polarization-low-leak",
        Tolerances = new TargetTolerances
        {
            MassTolerance = 0.15,
            MultiplicityTolerance = 0,
            MaxGaugeLeakForCompatibility = 0.05,
        },
    };
}
