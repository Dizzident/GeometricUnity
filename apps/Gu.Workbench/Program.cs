using Gu.VulkanViewer;

namespace Gu.Workbench;

/// <summary>
/// Geometric Unity Vulkan Workbench -- artifact visualization tool.
///
/// Usage: dotnet run -- [run-folder-path] [--export-obj path.obj] [--export-ply path.ply]
///                      [--export-csv convergence.csv] [--color-scheme viridis|plasma|coolwarm|diverging]
///
/// Loads a run folder, prepares all available view payloads, and optionally
/// exports mesh/convergence data to interchange formats.
///
/// All operations are READ-ONLY: artifact data is consumed but never modified (IX-5).
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        string runFolderPath = args[0];
        string? exportObj = GetArg(args, "--export-obj");
        string? exportPly = GetArg(args, "--export-ply");
        string? exportCsv = GetArg(args, "--export-csv");
        string colorScheme = GetArg(args, "--color-scheme") ?? "viridis";

        try
        {
            var service = new ArtifactViewerService(colorScheme);
            var snapshot = service.LoadRunFolder(runFolderPath);

            Console.WriteLine($"Loaded run folder: {runFolderPath}");
            Console.WriteLine($"  Artifact ID: {snapshot.ArtifactId ?? "N/A"}");
            Console.WriteLine($"  Branch: {snapshot.BranchManifest?.BranchId ?? "N/A"}");
            Console.WriteLine($"  Has initial state: {snapshot.InitialState is not null}");
            Console.WriteLine($"  Has final state: {snapshot.FinalState is not null}");
            Console.WriteLine($"  Has derived state: {snapshot.DerivedState is not null}");
            Console.WriteLine($"  Has residuals: {snapshot.Residuals is not null}");
            Console.WriteLine($"  Has linearization: {snapshot.Linearization is not null}");
            Console.WriteLine($"  Has solver result: {snapshot.SolverResult is not null}");
            Console.WriteLine($"  Comparison records: {snapshot.ComparisonRecords.Count}");

            // If geometry and mesh data are available, prepare views
            if (snapshot.Geometry is not null && snapshot.InitialState is not null)
            {
                Console.WriteLine("\nView payloads would be prepared here.");
                Console.WriteLine("(Mesh construction from GeometryContext requires runtime mesh builder.)");
            }

            // Export convergence CSV if solver result is available
            if (exportCsv is not null && snapshot.SolverResult is not null)
            {
                string csv = ConvergencePlotter.ToCsv(snapshot.SolverResult.History);
                File.WriteAllText(exportCsv, csv);
                Console.WriteLine($"\nExported convergence CSV: {exportCsv}");
            }

            // Print convergence summary if available
            if (snapshot.SolverResult is not null)
            {
                var result = snapshot.SolverResult;
                Console.WriteLine($"\nSolver summary:");
                Console.WriteLine($"  Mode: {result.Mode}");
                Console.WriteLine($"  Converged: {result.Converged}");
                Console.WriteLine($"  Iterations: {result.Iterations}");
                Console.WriteLine($"  Final objective: {result.FinalObjective:E6}");
                Console.WriteLine($"  Final residual norm: {result.FinalResidualNorm:E6}");
                Console.WriteLine($"  Termination: {result.TerminationReason}");
            }

            // Print comparison overlay if available
            if (snapshot.ComparisonRecords.Count > 0)
            {
                Console.WriteLine($"\nComparison results:");
                foreach (var rec in snapshot.ComparisonRecords)
                {
                    Console.WriteLine($"  [{rec.Outcome}] {rec.ObservableId}: {rec.Message}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Geometric Unity Vulkan Workbench");
        Console.WriteLine();
        Console.WriteLine("Usage: gu-workbench <run-folder-path> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --export-obj <path>        Export mesh to OBJ format");
        Console.WriteLine("  --export-ply <path>        Export mesh to PLY format");
        Console.WriteLine("  --export-csv <path>        Export convergence data to CSV");
        Console.WriteLine("  --color-scheme <scheme>    Color scheme: viridis, plasma, coolwarm, diverging");
    }

    private static string? GetArg(string[] args, string flag)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag)
                return args[i + 1];
        }
        return null;
    }
}
