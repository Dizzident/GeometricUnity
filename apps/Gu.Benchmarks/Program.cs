using System.Text.Json;
using Gu.Benchmarks;
using Gu.Interop;
using Gu.Solvers;

// Scaling benchmark: increasing problem sizes with Mode A (residual only)
Console.WriteLine("=== GeometricUnity Benchmark Suite ===");
Console.WriteLine();

var runner = new BenchmarkRunner();

// 1. Scaling benchmark (Mode A: residual-only)
Console.WriteLine("--- Scaling Benchmark (Mode A: Residual Only) ---");
var scalingSizes = new[] { 10, 50, 100, 500, 1000 };
var scalingOptions = new SolverOptions { Mode = SolveMode.ResidualOnly };
var scalingReports = runner.RunScalingBenchmark("scaling-modeA", scalingSizes, scalingOptions);

foreach (var report in scalingReports)
{
    Console.WriteLine($"  N={report.ProblemSize,5}: {report.TotalTimeMs,8:F2}ms, I2={report.FinalObjective:E4}");
}

// 2. Solve benchmark (Mode B: gradient descent, small problem)
Console.WriteLine();
Console.WriteLine("--- Solve Benchmark (Mode B: Gradient Descent) ---");
var solveOptions = new SolverOptions
{
    Mode = SolveMode.ObjectiveMinimization,
    MaxIterations = 20,
    InitialStepSize = 0.01,
    GaugePenaltyLambda = 0.0,
};
var solveReport = runner.RunSolveBenchmark("solve-modeB-100", 100, solveOptions);
Console.WriteLine($"  N={solveReport.ProblemSize}: {solveReport.Iterations} iters, {solveReport.TotalTimeMs:F2}ms, I2={solveReport.FinalObjective:E4}, {solveReport.TerminationReason}");

// 3. Parity benchmark
Console.WriteLine();
Console.WriteLine("--- Parity Benchmark ---");
var (cpuReport, gpuReport, parityRecords) = runner.RunParityBenchmark("parity-100", 100);
foreach (var record in parityRecords)
{
    string status = record.Passed ? "PASS" : "FAIL";
    Console.WriteLine($"  {record.KernelName,-12}: {status} (maxRelErr={record.MaxRelativeError:E3})");
}

// Write all reports
string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "benchmark-results");
var allReports = new List<BenchmarkReport>(scalingReports) { solveReport, cpuReport, gpuReport };
BenchmarkRunner.WriteReports(outputDir, allReports);
Console.WriteLine();
Console.WriteLine($"Reports written to: {outputDir}");
