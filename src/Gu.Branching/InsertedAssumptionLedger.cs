using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Registry of all inserted assumptions (IA) and inserted choices (IX)
/// that define the Minimal GU v1 branch.
/// </summary>
public static class InsertedAssumptionLedger
{
    /// <summary>All registered inserted assumptions and choices.</summary>
    public static IReadOnlyDictionary<string, InsertedAssumption> All { get; } = BuildLedger();

    /// <summary>Inserted assumptions only (IA-*).</summary>
    public static IReadOnlyList<InsertedAssumption> Assumptions { get; } =
        All.Values.Where(a => a.Category == "assumption").ToList();

    /// <summary>Inserted choices only (IX-*).</summary>
    public static IReadOnlyList<InsertedAssumption> Choices { get; } =
        All.Values.Where(a => a.Category == "choice").ToList();

    private static Dictionary<string, InsertedAssumption> BuildLedger()
    {
        var entries = new InsertedAssumption[]
        {
            new()
            {
                Id = "IA-1",
                Title = "Dimension-generic engine, 4D target",
                Description = "The engine supports generic dimensions internally, but the active branch defaults to dim(X) = 4.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IA-2",
                Title = "Local trivialization implementation of P",
                Description = "P_h is represented through local frames/trivializations. omega_h, F_h, T_h, S_h, and Upsilon_h are stored as coefficient arrays with explicit basis metadata.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IA-3",
                Title = "Variational solve mode first",
                Description = "The first solver mode is residual evaluation, objective evaluation, gradient/Gauss-Newton/Newton-style solve, gauge-stabilized stationary solve. No real-time time evolution.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IA-4",
                Title = "Gauge stabilization is mandatory",
                Description = "The solver must include explicit gauge treatment: penalty term, reduced-coordinate nullspace elimination, or explicit constrained solve.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IA-5",
                Title = "CPU reference backend required before CUDA trust",
                Description = "No CUDA result is admissible until the matching CPU path exists and parity tests pass.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IA-6",
                Title = "Observation pipeline is typed",
                Description = "No native Y quantity may be exported to comparison code unless it passes through sigma_h*, optional transform, normalization, output typing, and comparison adapter.",
                Category = "assumption",
                PlanSection = "5",
            },
            new()
            {
                Id = "IX-1",
                Title = "Active torsion branch API",
                Description = "The first active torsion branch is implemented as a local or weakly local branch operator: T_h = TorsionBranchKernel(omega_h, A0_h, branchMetadata, geometryBuffers).",
                Category = "choice",
                PlanSection = "5",
            },
            new()
            {
                Id = "IX-2",
                Title = "Active Shiab branch API",
                Description = "The first active Shiab branch is implemented as a first-order curvature-derived operator: S_h = ShiabMcKernel(F_h, omega_h, branchMetadata, geometryBuffers).",
                Category = "choice",
                PlanSection = "5",
            },
            new()
            {
                Id = "IX-3",
                Title = "Representation backend",
                Description = "The first Lie backend uses finite-dimensional real Lie algebra basis, explicit structure constants, explicit invariant metric/pairing, and explicit basis order metadata.",
                Category = "choice",
                PlanSection = "5",
            },
            new()
            {
                Id = "IX-4",
                Title = "Reference discretization branch",
                Description = "Use a patch-based finite-element or finite-volume style discretization on Y_h, with a simpler compatible carrier on X_h.",
                Category = "choice",
                PlanSection = "5",
            },
            new()
            {
                Id = "IX-5",
                Title = "Visualization backend is read-only",
                Description = "Visualization may derive render buffers from artifacts, but must not become a hidden state authority.",
                Category = "choice",
                PlanSection = "5",
            },
        };

        return entries.ToDictionary(e => e.Id);
    }
}
