using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer.Tests;

public class ViewPayloadBuilderTests
{
    private readonly ViewPayloadBuilder _builder = new();

    [Fact]
    public void BuildGeometryView_ProducesValidPayload()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var payload = _builder.BuildGeometryView(mesh);

        Assert.Equal("geometry", payload.ViewType);
        Assert.Equal(mesh.EmbeddingDimension, payload.EmbeddingDimension);
        Assert.Equal(mesh.CellCount, payload.CellCount);
        Assert.Equal(mesh.EdgeCount, payload.EdgeCount);
        Assert.True(payload.ShowWireframe);
        Assert.Equal(mesh.VertexCount, payload.MeshData.VertexCount);
    }

    [Fact]
    public void BuildGeometryView_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.BuildGeometryView(null!));
    }

    [Fact]
    public void BuildFieldView_ProducesValidPayload()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 1, 2, 3 }, "test_field");

        var payload = _builder.BuildFieldView(field, mesh, "omega");

        Assert.Equal("field", payload.ViewType);
        Assert.Equal("omega", payload.FieldMode);
        Assert.Equal("test_field", payload.FieldLabel);
        Assert.Equal(3, payload.CoefficientCount);
        Assert.True(payload.FieldNorm > 0);
        Assert.Equal(mesh.VertexCount, payload.FieldData.VertexCount);
    }

    [Fact]
    public void BuildDerivedFieldViews_ReturnsFourViews()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);

        var views = _builder.BuildDerivedFieldViews(derived, mesh);

        Assert.Equal(4, views.Count);
        Assert.Equal("curvature", views[0].FieldMode);
        Assert.Equal("torsion", views[1].FieldMode);
        Assert.Equal("shiab", views[2].FieldMode);
        Assert.Equal("residual", views[3].FieldMode);
    }

    [Fact]
    public void BuildResidualView_UsesDivergingColorMap()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { -1, 0, 1 }, "residual");

        var payload = _builder.BuildResidualView(field, mesh, 0.5);

        Assert.Equal("residual", payload.ViewType);
        Assert.Equal(0.5, payload.ObjectiveValue);
        Assert.True(payload.ResidualNorm > 0);
        Assert.True(payload.MaxAbsComponent > 0);
        Assert.Equal("diverging", payload.ResidualData.ColorMap.ColorSchemeName);
    }

    [Fact]
    public void BuildResidualView_FromBundle()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { -1, 0, 1 }, "Upsilon");
        var bundle = new ResidualBundle
        {
            Components = Array.Empty<ResidualComponent>(),
            ObjectiveValue = 1.5,
            TotalNorm = 1.414,
        };

        var payload = _builder.BuildResidualView(bundle, field, mesh);

        Assert.Equal(1.5, payload.ObjectiveValue);
    }

    [Fact]
    public void BuildConvergenceView_FromHistory()
    {
        var history = TestDataHelper.CreateConvergenceHistory(10);

        var payload = _builder.BuildConvergenceView(history, true, "tolerance");

        Assert.Equal("convergence", payload.ViewType);
        Assert.True(payload.Converged);
        Assert.Equal("tolerance", payload.TerminationReason);
        Assert.Equal(10, payload.PlotData.IterationCount);
        Assert.True(payload.FinalObjective > 0);
        Assert.True(payload.FinalResidualNorm > 0);
    }

    [Fact]
    public void BuildConvergenceView_FromSolverResult()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);

        var result = new SolverResult
        {
            Converged = true,
            TerminationReason = "converged",
            Iterations = 5,
            FinalObjective = 0.001,
            FinalResidualNorm = 0.01,
            FinalGradientNorm = 0.001,
            FinalGaugeViolation = 0.0001,
            FinalOmega = TestDataHelper.CreateEdgeScalarField(new double[mesh.EdgeCount], "omega"),
            FinalDerivedState = derived,
            History = TestDataHelper.CreateConvergenceHistory(5),
            Mode = SolveMode.ObjectiveMinimization,
        };

        var payload = _builder.BuildConvergenceView(result);

        Assert.Equal("convergence", payload.ViewType);
        Assert.True(payload.Converged);
    }

    [Fact]
    public void BuildConvergenceView_ThrowsOnEmptyHistory()
    {
        Assert.Throws<ArgumentException>(() =>
            _builder.BuildConvergenceView(Array.Empty<ConvergenceRecord>()));
    }

    [Fact]
    public void BuildSpectrumView_ProducesValidPayload()
    {
        var linearization = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "jacobian",
                RealizationType = "matrix-free",
                Rows = 10,
                Cols = 5,
            },
            GradientLikeResidual = TestDataHelper.CreateVertexScalarField(
                new double[] { 0.1, 0.2, 0.3 }, "grad_residual"),
            SpectralDiagnostics = new Dictionary<string, double>
            {
                ["conditionNumber"] = 42.0,
            },
        };

        var payload = _builder.BuildSpectrumView(linearization);

        Assert.Equal("spectrum", payload.ViewType);
        Assert.Equal(5, payload.JacobianInputDim);
        Assert.Equal(10, payload.JacobianOutputDim);
        Assert.True(payload.GradientResidualNorm > 0);
        Assert.Equal(42.0, payload.Diagnostics["conditionNumber"]);
    }

    [Fact]
    public void BuildSpectrumView_HandlesNullDiagnostics()
    {
        var linearization = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "jacobian",
                RealizationType = "matrix-free",
                Rows = 10,
                Cols = 5,
            },
            GradientLikeResidual = TestDataHelper.CreateVertexScalarField(
                new double[] { 0.1 }, "grad"),
            SpectralDiagnostics = null,
        };

        var payload = _builder.BuildSpectrumView(linearization);

        Assert.Empty(payload.Diagnostics);
    }

    [Fact]
    public void ViewPayloads_AreReadOnly_DataIsSnapshot()
    {
        // Verify that view payloads carry snapshot data and the original
        // artifact data is not modified (IX-5 read-only constraint).
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var originalValues = new double[] { 1.0, 2.0, 3.0 };
        var field = TestDataHelper.CreateVertexScalarField(
            (double[])originalValues.Clone(), "snapshot_test");

        var payload = _builder.BuildFieldView(field, mesh, "test");

        // The field data should be visualized (colors generated) without
        // modifying the original field coefficients.
        Assert.Equal(originalValues, field.Coefficients);
        Assert.Equal(3, payload.CoefficientCount);
    }

    [Fact]
    public void AllViewTypes_HaveDistinctViewTypeStrings()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 1, 2, 3 });
        var history = TestDataHelper.CreateConvergenceHistory(3);

        var geo = _builder.BuildGeometryView(mesh);
        var fieldView = _builder.BuildFieldView(field, mesh, "test");
        var residual = _builder.BuildResidualView(field, mesh, 0.5);
        var convergence = _builder.BuildConvergenceView(history);

        var types = new HashSet<string> { geo.ViewType, fieldView.ViewType, residual.ViewType, convergence.ViewType };
        Assert.Equal(4, types.Count);
    }

    [Fact]
    public void BuildFieldView_ComputesCorrectNorm()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 3.0, 4.0, 0.0 });

        var payload = _builder.BuildFieldView(field, mesh, "test");

        // sqrt(9 + 16 + 0) = 5
        Assert.Equal(5.0, payload.FieldNorm, precision: 10);
    }

    [Fact]
    public void BuildResidualView_ComputesCorrectMaxAbs()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { -5.0, 2.0, 3.0 });

        var payload = _builder.BuildResidualView(field, mesh, 0.0);

        Assert.Equal(5.0, payload.MaxAbsComponent, precision: 10);
    }
}
