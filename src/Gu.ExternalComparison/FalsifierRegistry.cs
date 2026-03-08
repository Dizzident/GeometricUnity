namespace Gu.ExternalComparison;

/// <summary>
/// Static registry of all falsifier conditions. Hard structural falsifiers are
/// automatic and fatal; soft physics falsifiers are opt-in warnings.
/// </summary>
public static class FalsifierRegistry
{
    // --- Hard structural (Fatal, automatic) ---

    public static readonly FalsifierCheck BianchiViolation = new()
    {
        Id = "F-HARD-01",
        Category = "HardStructural",
        Severity = "Fatal",
        Description = "Bianchi identity d_omega(F) = 0 must hold to machine precision when using consistent discrete operators",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-12,
            BranchSensitivity = "Low",
            MeshDependence = "Stable",
            Notes = "Algebraic identity: dd=0 + Jacobi => exact. Check: ||d_h(F_h) + [omega_h, F_h]_h|| / ||F_h|| < 1e-12. Any violation indicates a bug in curvature assembly or Lie algebra.",
        },
        BranchDependence = false,
    };

    public static readonly FalsifierCheck NonIntegerTopologicalCharge = new()
    {
        Id = "F-HARD-02",
        Category = "HardStructural",
        Severity = "Fatal",
        Description = "Integrated topological charge tr(F wedge F) must be integer-valued",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-4,
            BranchSensitivity = "Low",
            MeshDependence = "Converging",
            Notes = "Integer quantization condition",
        },
        BranchDependence = false,
    };

    public static readonly FalsifierCheck WrongGaugeModeCount = new()
    {
        Id = "F-HARD-03",
        Category = "HardStructural",
        Severity = "Fatal",
        Description = "Number of gauge zero modes must equal dim(G) for connected gauge group G",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "FactorBound",
            BaseValue = 0.0,
            BranchSensitivity = "Low",
            MeshDependence = "Stable",
            Notes = "Exact count check",
        },
        BranchDependence = false,
    };

    public static readonly FalsifierCheck CarrierMismatch = new()
    {
        Id = "F-HARD-04",
        Category = "HardStructural",
        Severity = "Fatal",
        Description = "T_h and S_h must carry the same tensor signature (carrier type, degree, algebra)",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "FactorBound",
            BaseValue = 0.0,
            BranchSensitivity = "Low",
            MeshDependence = "Stable",
            Notes = "Exact structural match",
        },
        BranchDependence = true,
    };

    public static readonly FalsifierCheck GaugeCovarianceFailure = new()
    {
        Id = "F-HARD-05",
        Category = "HardStructural",
        Severity = "Fatal",
        Description = "Residual Upsilon must transform covariantly under exact finite gauge transforms (eps=0.1, dim(g)+1 directions)",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-10,
            BranchSensitivity = "Medium",
            MeshDependence = "Stable",
            Notes = "Exact covariance test: for g=exp(eps*xi) with eps=0.1, check ||Upsilon(g.omega) - g.Upsilon(omega)|| / ||Upsilon(omega)|| < 1e-10. Test dim(g)+1 random xi directions (e.g. 4 for su(2): 3 basis + 1 random). Convergence variant: eps pair (1e-4, 1e-2) should show O(eps^2) rate. Finite transform test preferred over infinitesimal.",
        },
        BranchDependence = false,
    };

    // --- Soft physics (Warning, opt-in) ---

    public static readonly FalsifierCheck SolverDivergence = new()
    {
        Id = "F-SOFT-01",
        Category = "SoftPhysics",
        Severity = "Warning",
        Description = "Solver objective I2_h should decrease monotonically after initial transient",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "FactorBound",
            BaseValue = 1.1,
            BranchSensitivity = "High",
            MeshDependence = "Unknown",
            Notes = "Factor > 1.1 between consecutive iterations after warmup",
        },
        BranchDependence = true,
    };

    public static readonly FalsifierCheck SaddlePoint = new()
    {
        Id = "F-SOFT-02",
        Category = "SoftPhysics",
        Severity = "Warning",
        Description = "Converged solution should not be a saddle point (negative eigenvalue in Hessian)",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 0.0,
            BranchSensitivity = "High",
            MeshDependence = "Stable",
            Notes = "All physical eigenvalues should be non-negative at minimum",
        },
        BranchDependence = true,
    };

    public static readonly FalsifierCheck ObservationInstability = new()
    {
        Id = "F-SOFT-03",
        Category = "SoftPhysics",
        Severity = "Warning",
        Description = "Observables should be stable under small perturbation of sigma_h section",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 0.1,
            BranchSensitivity = "High",
            MeshDependence = "Unknown",
            Notes = "10% relative change under epsilon perturbation of section",
        },
        BranchDependence = true,
    };

    public static readonly FalsifierCheck BranchSensitivityExplosion = new()
    {
        Id = "F-SOFT-04",
        Category = "SoftPhysics",
        Severity = "Warning",
        Description = "Observable should not change by orders of magnitude between nearby branches",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "OrderEstimate",
            BaseValue = 2.0,
            BranchSensitivity = "High",
            MeshDependence = "Unknown",
            Notes = "More than 2 orders of magnitude suggests pathology",
        },
        BranchDependence = true,
    };

    public static readonly FalsifierCheck UnexpectedSymmetryBreaking = new()
    {
        Id = "F-SOFT-05",
        Category = "SoftPhysics",
        Severity = "Warning",
        Description = "Solution should preserve symmetries present in the initial data and geometry",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-4,
            BranchSensitivity = "Medium",
            MeshDependence = "Converging",
            Notes = "Symmetry-breaking residual should converge to zero with mesh refinement",
        },
        BranchDependence = true,
    };

    /// <summary>
    /// All registered falsifier checks.
    /// </summary>
    public static IReadOnlyList<FalsifierCheck> All { get; } = new[]
    {
        BianchiViolation,
        NonIntegerTopologicalCharge,
        WrongGaugeModeCount,
        CarrierMismatch,
        GaugeCovarianceFailure,
        SolverDivergence,
        SaddlePoint,
        ObservationInstability,
        BranchSensitivityExplosion,
        UnexpectedSymmetryBreaking,
    };

    /// <summary>
    /// Hard structural falsifiers that run automatically on every validation-grade run.
    /// </summary>
    public static IReadOnlyList<FalsifierCheck> HardStructural { get; } = new[]
    {
        BianchiViolation,
        NonIntegerTopologicalCharge,
        WrongGaugeModeCount,
        CarrierMismatch,
        GaugeCovarianceFailure,
    };

    /// <summary>
    /// Soft physics falsifiers that are opt-in per EnvironmentSpec.
    /// </summary>
    public static IReadOnlyList<FalsifierCheck> SoftPhysics { get; } = new[]
    {
        SolverDivergence,
        SaddlePoint,
        ObservationInstability,
        BranchSensitivityExplosion,
        UnexpectedSymmetryBreaking,
    };

    /// <summary>
    /// Look up a falsifier by ID. Returns null if not found.
    /// </summary>
    public static FalsifierCheck? GetById(string id)
    {
        foreach (var check in All)
        {
            if (check.Id == id)
                return check;
        }
        return null;
    }
}
