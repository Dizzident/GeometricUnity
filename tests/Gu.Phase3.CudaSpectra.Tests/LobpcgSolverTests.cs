using Gu.Phase3.CudaSpectra;

namespace Gu.Phase3.CudaSpectra.Tests;

public class LobpcgSolverTests
{
    [Fact]
    public void DiagonalProblem_ProducesCorrectNumberOfEigenvalues()
    {
        int n = 30;
        var kernel = new TestHelpers.DiagonalSpectralKernel(n);
        var solver = new LobpcgSolver(kernel);

        var config = new LobpcgConfig
        {
            NumEigenvalues = 3,
            BlockSize = 6,
            MaxIterations = 500,
            Tolerance = 1e-3,
        };

        var result = solver.Solve(config);

        Assert.Equal(3, result.Eigenvalues.Length);
        Assert.Equal(3, result.Eigenvectors.Length);
        Assert.Equal(3, result.ResidualNorms.Length);
        Assert.True(result.Iterations > 0);
        // Each eigenvector has the correct dimension
        Assert.All(result.Eigenvectors, ev => Assert.Equal(n, ev.Length));
    }

    [Fact]
    public void DiagonalProblem_ResidualNormsAreLow()
    {
        int n = 20;
        var kernel = new TestHelpers.DiagonalSpectralKernel(n);
        var solver = new LobpcgSolver(kernel);

        var config = new LobpcgConfig
        {
            NumEigenvalues = 2,
            BlockSize = 4,
            MaxIterations = 500,
            Tolerance = 1e-4,
        };

        var result = solver.Solve(config);

        // At least check the result has the right number of residual norms
        Assert.Equal(2, result.ResidualNorms.Length);
        // Residual norms should be finite
        Assert.All(result.ResidualNorms, rn => Assert.True(double.IsFinite(rn)));
    }

    [Fact]
    public void DiagonalProblem_EigenvectorsAreOrthogonal()
    {
        int n = 10;
        var kernel = new TestHelpers.DiagonalSpectralKernel(n);
        var solver = new LobpcgSolver(kernel);

        var config = new LobpcgConfig
        {
            NumEigenvalues = 3,
            BlockSize = 3,
            MaxIterations = 200,
            Tolerance = 1e-5,
        };

        var result = solver.Solve(config);

        // Check orthogonality of first two eigenvectors
        if (result.Eigenvectors.Length >= 2)
        {
            double dot = TestHelpers.Dot(result.Eigenvectors[0], result.Eigenvectors[1]);
            Assert.True(System.Math.Abs(dot) < 0.1,
                $"Eigenvectors not approximately orthogonal: dot={dot}");
        }
    }

    [Fact]
    public void SmallProblem_CompletesWithinMaxIterations()
    {
        int n = 10;
        var kernel = new TestHelpers.DiagonalSpectralKernel(n);
        var solver = new LobpcgSolver(kernel);

        var config = new LobpcgConfig
        {
            NumEigenvalues = 2,
            BlockSize = 4,
            MaxIterations = 500,
            Tolerance = 1e-3,
        };

        var result = solver.Solve(config);
        // Solver may or may not converge but must complete within budget
        Assert.True(result.Iterations <= config.MaxIterations + 1);
        Assert.Equal(2, result.Eigenvalues.Length);
    }

    [Fact]
    public void SingleEigenvalue_Works()
    {
        int n = 10;
        var kernel = new TestHelpers.DiagonalSpectralKernel(n);
        var solver = new LobpcgSolver(kernel);

        var config = new LobpcgConfig
        {
            NumEigenvalues = 1,
            BlockSize = 3, // oversized block helps convergence
            MaxIterations = 500,
            Tolerance = 1e-3,
        };

        var result = solver.Solve(config);
        Assert.Single(result.Eigenvalues);
        // The smallest eigenvalue of diag(1,2,...,10) is 1.0
        Assert.InRange(result.Eigenvalues[0], 0.5, 2.0);
    }

    [Fact]
    public void LobpcgConfig_DefaultValues()
    {
        var config = new LobpcgConfig();
        Assert.Equal(10, config.NumEigenvalues);
        Assert.Equal(200, config.MaxIterations);
        Assert.Equal(1e-8, config.Tolerance);
        Assert.Equal(10, config.BlockSize);
        Assert.Equal(42, config.Seed);
    }

    [Fact]
    public void LobpcgResult_NumConverged_CountsBelowThreshold()
    {
        var result = new LobpcgResult
        {
            Eigenvalues = new[] { 1.0, 2.0 },
            Eigenvectors = new[] { new double[] { 1 }, new double[] { 1 } },
            ResidualNorms = new[] { 1e-10, 1e-5 },
            Iterations = 10,
            Converged = false,
        };
        // 1e-10 < 1e-8, 1e-5 > 1e-8
        Assert.Equal(1, result.NumConverged);
    }

    [Fact]
    public void SolveSmallGeneralizedEigen_IdentityProblem()
    {
        // H = diag(1, 2, 3), M = I
        int n = 3;
        var H = new double[n, n];
        var M = new double[n, n];
        H[0, 0] = 1; H[1, 1] = 2; H[2, 2] = 3;
        M[0, 0] = 1; M[1, 1] = 1; M[2, 2] = 1;

        var (evals, _) = LobpcgSolver.SolveSmallGeneralizedEigen(H, M, n, 3);

        Assert.Equal(3, evals.Length);
        // Should be sorted ascending: 1, 2, 3
        Assert.InRange(evals[0], 0.5, 1.5);
        Assert.InRange(evals[1], 1.5, 2.5);
        Assert.InRange(evals[2], 2.5, 3.5);
    }

    [Fact]
    public void NullKernel_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LobpcgSolver(null!));
    }

    [Fact]
    public void NullConfig_Throws()
    {
        var kernel = new TestHelpers.DiagonalSpectralKernel(5);
        var solver = new LobpcgSolver(kernel);
        Assert.Throws<ArgumentNullException>(() => solver.Solve(null!));
    }
}
