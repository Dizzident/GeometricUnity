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

// 3. Parity benchmark (CPU vs CPU baseline)
Console.WriteLine();
Console.WriteLine("--- Parity Benchmark (CPU vs CPU) ---");
var (cpuReport, gpuReport, parityRecords) = runner.RunParityBenchmark("parity-100", 100);
foreach (var record in parityRecords)
{
    string status = record.Passed ? "PASS" : "FAIL";
    Console.WriteLine($"  {record.KernelName,-12}: {status} (maxRelErr={record.MaxRelativeError:E3})");
}

// 4. GPU parity benchmark (CPU vs real CUDA backend)
Console.WriteLine();
Console.WriteLine("--- GPU Parity Benchmark (CPU vs CUDA) ---");
var (gpuCpuReport, gpuGpuReport, gpuParityRecords) = runner.RunParityBenchmarkWithGpu("gpu-parity-100", 100);
Console.WriteLine($"  Target backend: {gpuGpuReport.BackendId}");
foreach (var record in gpuParityRecords)
{
    string status = record.Passed ? "PASS" : "FAIL";
    Console.WriteLine($"  {record.KernelName,-12}: {status} (maxRelErr={record.MaxRelativeError:E3})");
}
Console.WriteLine($"  Time: {gpuGpuReport.TotalTimeMs:F2}ms, Result: {gpuGpuReport.TerminationReason}");

// 5. GPU solve benchmark (Mode A with CUDA backend)
Console.WriteLine();
Console.WriteLine("--- GPU Solve Benchmark (Mode A with CUDA) ---");
try
{
    using var cudaBackend = new CudaNativeBackend();
    var gpuSolveReport = runner.RunSolveBenchmarkWithBackend(
        "gpu-solve-modeA-100", 100, new SolverOptions { Mode = SolveMode.ResidualOnly }, cudaBackend);
    Console.WriteLine($"  N={gpuSolveReport.ProblemSize}: {gpuSolveReport.Iterations} iters, {gpuSolveReport.TotalTimeMs:F2}ms, I2={gpuSolveReport.FinalObjective:E4}, backend={gpuSolveReport.BackendId}");
}
catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
{
    Console.WriteLine($"  Skipped: CUDA library not available ({ex.GetType().Name})");
}

// 6. Mesh-based scaling benchmark (real simplicial meshes)
Console.WriteLine();
Console.WriteLine("--- Mesh Scaling Benchmark (Real Simplicial Meshes) ---");
var meshScalingSizes = new[] { 100, 1_000, 10_000, 100_000 };
var meshScalingReports = runner.RunMeshScalingBenchmark("mesh-scaling", meshScalingSizes);
foreach (var report in meshScalingReports)
{
    Console.WriteLine($"  {report.Description}: {report.TotalTimeMs,8:F2}ms, I2={report.FinalObjective:E4}");
}

// Write all reports
string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "benchmark-results");
var allReports = new List<BenchmarkReport>(scalingReports) { solveReport, cpuReport, gpuReport, gpuCpuReport, gpuGpuReport };
allReports.AddRange(meshScalingReports);
BenchmarkRunner.WriteReports(outputDir, allReports);
Console.WriteLine();
Console.WriteLine($"Reports written to: {outputDir}");
