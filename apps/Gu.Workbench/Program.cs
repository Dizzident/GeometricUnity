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

            // Load and print Phase III diagnostic views if Phase III folder structure is present
            PrintPhase3Views(runFolderPath);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void PrintPhase3Views(string runFolderPath)
    {
        // Only attempt Phase III loading if at least one expected subdirectory exists
        bool hasBackgrounds = Directory.Exists(Path.Combine(runFolderPath, "backgrounds"));
        bool hasSpectra = Directory.Exists(Path.Combine(runFolderPath, "spectra"));
        bool hasModes = Directory.Exists(Path.Combine(runFolderPath, "modes"));
        bool hasBosons = Directory.Exists(Path.Combine(runFolderPath, "bosons"));

        if (!hasBackgrounds && !hasSpectra && !hasModes && !hasBosons)
            return;

        Console.WriteLine("\n--- Phase III Diagnostic Views ---");

        var loader = new Phase3ArtifactLoader();
        var snapshot = loader.LoadPhase3Folder(runFolderPath);

        Console.WriteLine($"  Background atlas loaded: {snapshot.BackgroundAtlas is not null}");
        Console.WriteLine($"  Spectra loaded:          {snapshot.Spectra.Count}");
        Console.WriteLine($"  Mode families loaded:    {snapshot.ModeFamilies.Count}");
        Console.WriteLine($"  Boson registry loaded:   {snapshot.BosonRegistry is not null}");

        var views = loader.PreparePhase3Views(snapshot);
        Console.WriteLine($"  Views prepared:          {views.Count}");
        Console.WriteLine();

        foreach (var view in views)
        {
            if (view is BackgroundAtlasBrowserView bab)
                Console.WriteLine(bab.Print());
            else if (view is SpectralLadderView sl)
                Console.WriteLine(sl.Print());
            else if (view is EigenModeAmplitudeView ema)
                Console.WriteLine(ema.Print());
            else if (view is GaugeLeakView gl)
                Console.WriteLine(gl.Print());
            else if (view is BranchModeTrackView bmt)
                Console.WriteLine(bmt.Print());
            else if (view is BosonFamilyCardView bfc)
                Console.WriteLine(bfc.Print());
            else if (view is ObservedSignatureView osv)
                Console.WriteLine(osv.Print());
            else if (view is AmbiguityHeatmapView ahv)
                Console.WriteLine(ahv.Print());
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
