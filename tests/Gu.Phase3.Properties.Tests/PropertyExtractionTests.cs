using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.ModeTracking;
using Gu.Phase3.Properties;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties.Tests;

public class PropertyExtractionTests
{
    // --- Test helpers ---

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static LieAlgebra Su2() =>
        LieAlgebraFactory.CreateSu2WithTracePairing();

    private static ModeRecord MakeMode(
        string modeId, double eigenvalue, double[] modeVector,
        string backgroundId = "bg-1", int modeIndex = 0,
        double gaugeLeakScore = 0.01, string? clusterId = null)
    {
        return new ModeRecord
        {
            ModeId = modeId,
            BackgroundId = backgroundId,
            OperatorType = SpectralOperatorType.FullHessian,
            Eigenvalue = eigenvalue,
            ResidualNorm = 1e-10,
            NormalizationConvention = "unit-L2-norm",
            MultiplicityClusterId = clusterId,
            GaugeLeakScore = gaugeLeakScore,
            ModeVector = modeVector,
            ModeIndex = modeIndex,
        };
    }

    private static ModeCluster MakeCluster(string clusterId, int[] indices, double meanEv, double spread)
    {
        return new ModeCluster
        {
            ClusterId = clusterId,
            ModeIndices = indices,
            MeanEigenvalue = meanEv,
            EigenvalueSpread = spread,
        };
    }

    private static ModeFamilyRecord MakeFamily(string familyId, IReadOnlyList<string> modeIds)
    {
        return new ModeFamilyRecord
        {
            FamilyId = familyId,
            MemberModeIds = modeIds,
            ContextIds = new[] { "ctx-1" },
            MeanEigenvalue = 1.0,
            EigenvalueSpread = 0.01,
            IsStable = true,
            Alignments = Array.Empty<ModeAlignmentRecord>(),
        };
    }

    // --- MassLikeScaleRecord tests ---

    [Fact]
    public void MassLikeScaleRecord_RoundTripsJson()
    {
        var record = new MassLikeScaleRecord
        {
            ModeId = "mode-0",
            Eigenvalue = 4.0,
            MassLikeScale = 2.0,
            ExtractionMethod = "eigenvalue",
            OperatorType = "FullHessian",
            BackgroundId = "bg-1",
            BranchManifestId = "branch-1",
        };

        string json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MassLikeScaleRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("mode-0", deserialized.ModeId);
        Assert.Equal(4.0, deserialized.Eigenvalue);
        Assert.Equal(2.0, deserialized.MassLikeScale);
        Assert.Equal("eigenvalue", deserialized.ExtractionMethod);
        Assert.Equal("FullHessian", deserialized.OperatorType);
        Assert.Equal("bg-1", deserialized.BackgroundId);
        Assert.Equal("branch-1", deserialized.BranchManifestId);
    }

    [Fact]
    public void MassLikeScaleExtractor_PositiveEigenvalue_ReturnsSquareRoot()
    {
        var mode = MakeMode("mode-0", eigenvalue: 9.0, modeVector: new double[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });
        var scale = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal("mode-0", scale.ModeId);
        Assert.Equal(9.0, scale.Eigenvalue);
        Assert.Equal(3.0, scale.MassLikeScale, 1e-12);
        Assert.Equal("eigenvalue", scale.ExtractionMethod);
    }

    [Fact]
    public void MassLikeScaleExtractor_NegativeEigenvalue_ReturnsNegativeRoot()
    {
        var mode = MakeMode("mode-neg", eigenvalue: -4.0, modeVector: new double[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });
        var scale = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal(-4.0, scale.Eigenvalue);
        Assert.Equal(-2.0, scale.MassLikeScale, 1e-12);
    }

    [Fact]
    public void MassLikeScaleExtractor_ZeroEigenvalue_ReturnsZero()
    {
        var mode = MakeMode("mode-zero", eigenvalue: 0.0, modeVector: new double[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });
        var scale = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal(0.0, scale.MassLikeScale);
    }

    [Fact]
    public void MassLikeScaleExtractor_ExtractAll_ReturnsCorrectCount()
    {
        var modes = new[]
        {
            MakeMode("m0", 1.0, new double[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 }),
            MakeMode("m1", 4.0, new double[] { 0, 1, 0, 0, 0, 0, 0, 0, 0 }),
            MakeMode("m2", 16.0, new double[] { 0, 0, 1, 0, 0, 0, 0, 0, 0 }),
        };

        var scales = MassLikeScaleExtractor.ExtractAll(modes);

        Assert.Equal(3, scales.Count);
        Assert.Equal(1.0, scales[0].MassLikeScale, 1e-12);
        Assert.Equal(2.0, scales[1].MassLikeScale, 1e-12);
        Assert.Equal(4.0, scales[2].MassLikeScale, 1e-12);
    }

    // --- PolarizationDescriptor tests ---

    [Fact]
    public void PolarizationDescriptor_RoundTripsJson()
    {
        var desc = new PolarizationDescriptor
        {
            ModeId = "mode-0",
            BlockEnergyFractions = new Dictionary<string, double>
            {
                ["connection"] = 1.0,
                ["algebra-component-0"] = 0.8,
                ["algebra-component-1"] = 0.1,
                ["algebra-component-2"] = 0.1,
            },
            DominantClass = "connection-dominant",
            DominanceFraction = 0.8,
            BackgroundId = "bg-1",
        };

        string json = JsonSerializer.Serialize(desc);
        var deserialized = JsonSerializer.Deserialize<PolarizationDescriptor>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("mode-0", deserialized.ModeId);
        Assert.Equal(0.8, deserialized.DominanceFraction);
        Assert.Equal("connection-dominant", deserialized.DominantClass);
    }

    [Fact]
    public void PolarizationExtractor_ConcentratedMode_ConnectionDominant()
    {
        var mesh = SingleTriangle(); // 3 edges
        var algebra = Su2(); // dim 3
        var extractor = new PolarizationExtractor(mesh, algebra);

        // Mode concentrated in algebra component 0
        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        vector[0] = 1.0; // edge 0, component 0
        vector[3] = 0.5; // edge 1, component 0
        vector[6] = 0.3; // edge 2, component 0
        // All other components zero

        var mode = MakeMode("mode-concentrated", 1.0, vector);
        var pol = extractor.Extract(mode);

        Assert.Equal("connection-dominant", pol.DominantClass);
        Assert.True(pol.DominanceFraction > 0.8);
        Assert.True(pol.BlockEnergyFractions["algebra-component-0"] > 0.9);
    }

    [Fact]
    public void PolarizationExtractor_EvenlyDistributed_MixedOrScalarLike()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        // Mode evenly distributed across all algebra components
        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < vector.Length; i++)
            vector[i] = 1.0;

        var mode = MakeMode("mode-even", 1.0, vector);
        var pol = extractor.Extract(mode);

        // Each component should have ~1/3 of energy
        foreach (int a in Enumerable.Range(0, algebra.Dimension))
        {
            double frac = pol.BlockEnergyFractions[$"algebra-component-{a}"];
            Assert.InRange(frac, 0.3, 0.4); // ~1/3
        }
    }

    // --- SymmetryDescriptor tests ---

    [Fact]
    public void SymmetryDescriptor_RoundTripsJson()
    {
        var desc = new SymmetryDescriptor
        {
            ModeId = "mode-0",
            ParityEigenvalue = 1.0,
            SymmetryLabels = new[] { "parity-even", "dominant-algebra-0" },
            SectorOverlaps = new Dictionary<string, double>
            {
                ["algebra-0"] = 0.6,
                ["algebra-1"] = 0.2,
                ["algebra-2"] = 0.2,
            },
            BackgroundId = "bg-1",
        };

        string json = JsonSerializer.Serialize(desc);
        var deserialized = JsonSerializer.Deserialize<SymmetryDescriptor>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(1.0, deserialized.ParityEigenvalue);
        Assert.Equal(2, deserialized.SymmetryLabels.Count);
    }

    [Fact]
    public void SymmetryExtractor_ExtractsSectorOverlaps()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new SymmetryExtractor(mesh.EdgeCount, algebra.Dimension);

        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        vector[0] = 1.0; // edge 0, component 0
        vector[1] = 0.5; // edge 0, component 1

        var mode = MakeMode("mode-sym", 1.0, vector);
        var sym = extractor.Extract(mode);

        Assert.NotNull(sym.SectorOverlaps);
        double sum = sym.SectorOverlaps.Values.Sum();
        Assert.Equal(1.0, sum, 1e-10);
        Assert.True(sym.SectorOverlaps["algebra-0"] > sym.SectorOverlaps["algebra-1"]);
    }

    [Fact]
    public void SymmetryExtractor_EvenMode_ParityEven()
    {
        // Build a palindromic vector to test parity
        int n = 6;
        var v = new double[] { 1.0, 0.5, 0.3, 0.3, 0.5, 1.0 };

        double? parity = SymmetryExtractor.ComputeParityEigenvalue(v, n);

        // Palindrome has positive overlap with its reverse -> parity even
        Assert.NotNull(parity);
        Assert.Equal(1.0, parity.Value);
    }

    [Fact]
    public void SymmetryExtractor_OddMode_ParityOdd()
    {
        int n = 6;
        var v = new double[] { 1.0, 0.5, 0.3, -0.3, -0.5, -1.0 };

        double? parity = SymmetryExtractor.ComputeParityEigenvalue(v, n);

        Assert.NotNull(parity);
        Assert.Equal(-1.0, parity.Value);
    }

    // --- InteractionProxy tests ---

    [Fact]
    public void InteractionProxyRecord_RoundTripsJson()
    {
        var record = new InteractionProxyRecord
        {
            ModeIds = new[] { "m0", "m1", "m2" },
            CubicResponse = 0.123,
            Epsilon = 1e-4,
            Method = "finite-difference-residual",
            BackgroundId = "bg-1",
            EstimatedError = 1e-6,
        };

        string json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<InteractionProxyRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.ModeIds.Count);
        Assert.Equal(0.123, deserialized.CubicResponse);
        Assert.Equal(1e-6, deserialized.EstimatedError);
    }

    [Fact]
    public void SimpleInteractionProxy_CubicPolynomial_RecoversCoefficent()
    {
        // F(x) = a * x[0] * x[1] * x[2]  =>  d^3F/dx0 dx1 dx2 = a
        double a = 6.0;
        Func<double[], double> objective = x => a * x[0] * x[1] * x[2];

        var computer = new SimpleInteractionProxyComputer(objective, epsilon: 1e-3);

        var bg = new double[] { 1.0, 1.0, 1.0 };
        var vi = new double[] { 1.0, 0.0, 0.0 };
        var vj = new double[] { 0.0, 1.0, 0.0 };
        var vk = new double[] { 0.0, 0.0, 1.0 };

        var result = computer.Compute(bg, vi, vj, vk, "bg-cubic", "m0", "m1", "m2");

        // The FD third derivative of a*x0*x1*x2 is exactly a
        Assert.Equal(a, result.CubicResponse, 0.1);
        Assert.Equal("finite-difference-residual", result.Method);
        Assert.Equal(3, result.ModeIds.Count);
    }

    [Fact]
    public void SimpleInteractionProxy_QuadraticObjective_CubicResponseNearZero()
    {
        // F(x) = x[0]^2 + x[1]^2 + x[2]^2  =>  all third derivatives are zero
        Func<double[], double> objective = x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2];

        var computer = new SimpleInteractionProxyComputer(objective, epsilon: 1e-3);

        var bg = new double[] { 1.0, 1.0, 1.0 };
        var vi = new double[] { 1.0, 0.0, 0.0 };
        var vj = new double[] { 0.0, 1.0, 0.0 };
        var vk = new double[] { 0.0, 0.0, 1.0 };

        var result = computer.Compute(bg, vi, vj, vk, "bg-quad", "m0", "m1", "m2");

        Assert.True(System.Math.Abs(result.CubicResponse) < 1e-3,
            $"Cubic response of quadratic should be ~0, got {result.CubicResponse}");
    }

    [Fact]
    public void SimpleInteractionProxy_ReportsEstimatedError()
    {
        Func<double[], double> objective = x => x[0] * x[1] * x[2];
        var computer = new SimpleInteractionProxyComputer(objective, epsilon: 1e-3);

        var bg = new double[] { 0.0, 0.0, 0.0 };
        var vi = new double[] { 1.0, 0.0, 0.0 };
        var vj = new double[] { 0.0, 1.0, 0.0 };
        var vk = new double[] { 0.0, 0.0, 1.0 };

        var result = computer.Compute(bg, vi, vj, vk, "bg-err", "m0", "m1", "m2");

        Assert.NotNull(result.EstimatedError);
    }

    [Fact]
    public void SimpleInteractionProxy_ThrowsOnMismatchedLengths()
    {
        Func<double[], double> objective = x => 0.0;
        var computer = new SimpleInteractionProxyComputer(objective);

        var bg = new double[] { 1.0, 1.0, 1.0 };
        var vi = new double[] { 1.0, 0.0 }; // Wrong length

        Assert.Throws<ArgumentException>(() =>
            computer.Compute(bg, vi, new double[] { 0, 1, 0 }, new double[] { 0, 0, 1 }, "bg", "m0", "m1", "m2"));
    }

    // --- StabilityScoreCard tests ---

    [Fact]
    public void StabilityScoreCard_RoundTripsJson()
    {
        var card = new StabilityScoreCard
        {
            EntityId = "family-0",
            BranchStability = 0.95,
            RefinementStability = 0.98,
            BackendStability = 1.0,
            BranchVariantCount = 5,
            RefinementLevelCount = 3,
            BackendCount = 2,
            MaxEigenvalueDrift = 0.02,
            MaxOverlapLoss = 0.01,
        };

        string json = JsonSerializer.Serialize(card);
        var deserialized = JsonSerializer.Deserialize<StabilityScoreCard>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("family-0", deserialized.EntityId);
        Assert.Equal(0.95, deserialized.BranchStability);
        Assert.Equal(0.02, deserialized.MaxEigenvalueDrift);
    }

    [Fact]
    public void StabilityScoreComputer_IdenticalEigenvalues_PerfectStability()
    {
        var eigenvalues = new double[] { 1.0, 1.0, 1.0, 1.0 };
        double stability = StabilityScoreComputer.ComputeStabilityFromEigenvalues(eigenvalues);

        Assert.Equal(1.0, stability);
    }

    [Fact]
    public void StabilityScoreComputer_WideSpread_LowStability()
    {
        var eigenvalues = new double[] { 1.0, 5.0 };
        double stability = StabilityScoreComputer.ComputeStabilityFromEigenvalues(eigenvalues);

        // Spread = 4, mean = 3, scale = 3, relative spread = 4/3 > 1 => clamped to 0
        Assert.Equal(0.0, stability);
    }

    [Fact]
    public void StabilityScoreComputer_SingleEigenvalue_ReturnsOne()
    {
        var eigenvalues = new double[] { 42.0 };
        double stability = StabilityScoreComputer.ComputeStabilityFromEigenvalues(eigenvalues);

        Assert.Equal(1.0, stability);
    }

    [Fact]
    public void StabilityScoreComputer_SmallDrift_HighStability()
    {
        var eigenvalues = new double[] { 10.0, 10.01, 9.99 };
        double stability = StabilityScoreComputer.ComputeStabilityFromEigenvalues(eigenvalues);

        Assert.True(stability > 0.99, $"Expected > 0.99, got {stability}");
    }

    [Fact]
    public void StabilityScoreComputer_Compute_CombinesAllAxes()
    {
        var family = MakeFamily("fam-1", new[] { "m0", "m1" });
        var branchEv = new double[] { 1.0, 1.01, 0.99 };
        var refinementEv = new double[] { 1.0, 1.0 };
        var backendEv = new double[] { 1.0, 1.001 };

        var card = StabilityScoreComputer.Compute(family, branchEv, refinementEv, backendEv);

        Assert.Equal("fam-1", card.EntityId);
        Assert.Equal(3, card.BranchVariantCount);
        Assert.Equal(2, card.RefinementLevelCount);
        Assert.Equal(2, card.BackendCount);
        Assert.True(card.BranchStability > 0.9);
        Assert.Equal(1.0, card.RefinementStability); // identical values
        Assert.True(card.BackendStability > 0.99);
    }

    [Fact]
    public void StabilityScoreComputer_FromSingleContext()
    {
        var card = StabilityScoreComputer.FromSingleContext("entity-1", new double[] { 5.0, 5.1, 4.9 });

        Assert.Equal("entity-1", card.EntityId);
        Assert.True(card.BranchStability > 0.9);
        Assert.Equal(1.0, card.RefinementStability);
        Assert.Equal(1.0, card.BackendStability);
        Assert.NotNull(card.MaxEigenvalueDrift);
    }

    [Fact]
    public void StabilityScoreComputer_MaxDrift_NullForSingleValue()
    {
        var drift = StabilityScoreComputer.ComputeMaxDrift(new double[] { 42.0 });
        Assert.Null(drift);
    }

    [Fact]
    public void StabilityScoreComputer_MaxDrift_ReturnsSpread()
    {
        var drift = StabilityScoreComputer.ComputeMaxDrift(new double[] { 1.0, 3.0, 2.0 });
        Assert.NotNull(drift);
        Assert.Equal(2.0, drift.Value, 1e-12);
    }

    // --- BosonPropertyVector tests ---

    [Fact]
    public void BosonPropertyVector_RoundTripsJson()
    {
        var bpv = new BosonPropertyVector
        {
            ModeId = "mode-0",
            BackgroundId = "bg-1",
            MassLikeScale = new MassLikeScaleRecord
            {
                ModeId = "mode-0",
                Eigenvalue = 4.0,
                MassLikeScale = 2.0,
                ExtractionMethod = "eigenvalue",
                OperatorType = "FullHessian",
                BackgroundId = "bg-1",
            },
            Polarization = new PolarizationDescriptor
            {
                ModeId = "mode-0",
                BlockEnergyFractions = new Dictionary<string, double> { ["connection"] = 1.0 },
                DominantClass = "connection-dominant",
                DominanceFraction = 1.0,
                BackgroundId = "bg-1",
            },
            Symmetry = new SymmetryDescriptor
            {
                ModeId = "mode-0",
                BackgroundId = "bg-1",
            },
            GaugeLeakScore = 0.01,
            Multiplicity = 3,
        };

        string json = JsonSerializer.Serialize(bpv);
        var deserialized = JsonSerializer.Deserialize<BosonPropertyVector>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("mode-0", deserialized.ModeId);
        Assert.Equal(3, deserialized.Multiplicity);
        Assert.Equal(0.01, deserialized.GaugeLeakScore);
    }

    [Fact]
    public void BosonPropertyVector_Distance_IdenticalVectors_ZeroDistance()
    {
        var bpv = CreateSampleBpv("m0", 2.0, 0.01);
        double dist = BosonPropertyVector.Distance(bpv, bpv);

        Assert.Equal(0.0, dist, 1e-12);
    }

    [Fact]
    public void BosonPropertyVector_Distance_DifferentMass_NonZero()
    {
        var a = CreateSampleBpv("m0", 2.0, 0.01);
        var b = CreateSampleBpv("m1", 4.0, 0.01);

        double dist = BosonPropertyVector.Distance(a, b);
        Assert.True(dist > 0);
    }

    [Fact]
    public void BosonPropertyVector_Distance_IsSymmetric()
    {
        var a = CreateSampleBpv("m0", 2.0, 0.01);
        var b = CreateSampleBpv("m1", 3.0, 0.05);

        double distAB = BosonPropertyVector.Distance(a, b);
        double distBA = BosonPropertyVector.Distance(b, a);
        Assert.Equal(distAB, distBA, 1e-12);
    }

    // --- PropertyExtractor end-to-end tests ---

    [Fact]
    public void PropertyExtractor_ExtractsAllFields()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PropertyExtractor(mesh, algebra);

        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        vector[0] = 1.0;
        vector[1] = 0.5;

        var mode = MakeMode("mode-e2e", eigenvalue: 4.0, modeVector: vector);
        var bpv = extractor.Extract(mode);

        Assert.Equal("mode-e2e", bpv.ModeId);
        Assert.Equal("bg-1", bpv.BackgroundId);
        Assert.Equal(2.0, bpv.MassLikeScale.MassLikeScale, 1e-12);
        Assert.NotNull(bpv.Polarization);
        Assert.NotNull(bpv.Symmetry);
        Assert.Equal(0.01, bpv.GaugeLeakScore);
        Assert.Equal(1, bpv.Multiplicity); // default
        Assert.Null(bpv.Stability);
        Assert.Empty(bpv.InteractionProxies);
    }

    [Fact]
    public void PropertyExtractor_WithCluster_SetsMultiplicity()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PropertyExtractor(mesh, algebra);

        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        vector[0] = 1.0;

        var cluster = MakeCluster("cluster-0", new[] { 0, 1, 2 }, 4.0, 0.01);
        var mode = MakeMode("mode-clust", eigenvalue: 4.0, modeVector: vector, clusterId: "cluster-0");
        var bpv = extractor.Extract(mode, cluster);

        Assert.Equal(3, bpv.Multiplicity);
    }

    [Fact]
    public void PropertyExtractor_WithStabilityAndProxies_AttachesThem()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PropertyExtractor(mesh, algebra);

        var vector = new double[mesh.EdgeCount * algebra.Dimension];
        vector[0] = 1.0;

        var mode = MakeMode("mode-full", eigenvalue: 1.0, modeVector: vector);
        var stability = new StabilityScoreCard
        {
            EntityId = "mode-full",
            BranchStability = 0.95,
            RefinementStability = 0.98,
            BackendStability = 1.0,
            BranchVariantCount = 3,
            RefinementLevelCount = 2,
            BackendCount = 2,
        };
        var proxies = new[]
        {
            new InteractionProxyRecord
            {
                ModeIds = new[] { "mode-full", "mode-full", "mode-full" },
                CubicResponse = 0.5,
                Epsilon = 1e-4,
                Method = "finite-difference-residual",
                BackgroundId = "bg-1",
            },
        };

        var bpv = extractor.Extract(mode, stability: stability, interactionProxies: proxies);

        Assert.NotNull(bpv.Stability);
        Assert.Equal(0.95, bpv.Stability.BranchStability);
        Assert.Single(bpv.InteractionProxies);
        Assert.Equal(0.5, bpv.InteractionProxies[0].CubicResponse);
    }

    [Fact]
    public void PropertyExtractor_ExtractAll_ProcessesMultipleModes()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PropertyExtractor(mesh, algebra);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var modes = new[]
        {
            MakeMode("m0", 1.0, Enumerable.Range(0, dim).Select(i => (double)(i == 0 ? 1 : 0)).ToArray(), modeIndex: 0),
            MakeMode("m1", 4.0, Enumerable.Range(0, dim).Select(i => (double)(i == 1 ? 1 : 0)).ToArray(), modeIndex: 1),
            MakeMode("m2", 9.0, Enumerable.Range(0, dim).Select(i => (double)(i == 2 ? 1 : 0)).ToArray(), modeIndex: 2),
        };

        var results = extractor.ExtractAll(modes);

        Assert.Equal(3, results.Count);
        Assert.Equal("m0", results[0].ModeId);
        Assert.Equal("m1", results[1].ModeId);
        Assert.Equal("m2", results[2].ModeId);
        Assert.Equal(1.0, results[0].MassLikeScale.MassLikeScale, 1e-12);
        Assert.Equal(2.0, results[1].MassLikeScale.MassLikeScale, 1e-12);
        Assert.Equal(3.0, results[2].MassLikeScale.MassLikeScale, 1e-12);
    }

    [Fact]
    public void PropertyExtractor_ExtractAll_UsesClusterMultiplicity()
    {
        var mesh = SingleTriangle();
        var algebra = Su2();
        var extractor = new PropertyExtractor(mesh, algebra);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var modes = new[]
        {
            MakeMode("m0", 1.0, new double[dim], clusterId: "c1", modeIndex: 0),
            MakeMode("m1", 1.01, new double[dim], clusterId: "c1", modeIndex: 1),
        };

        var clusters = new Dictionary<string, ModeCluster>
        {
            ["c1"] = MakeCluster("c1", new[] { 0, 1 }, 1.005, 0.01),
        };

        var results = extractor.ExtractAll(modes, clusters);

        Assert.Equal(2, results.Count);
        Assert.Equal(2, results[0].Multiplicity);
        Assert.Equal(2, results[1].Multiplicity);
    }

    // --- Helper for BosonPropertyVector distance tests ---

    private static BosonPropertyVector CreateSampleBpv(string modeId, double mass, double gaugeLeakScore)
    {
        return new BosonPropertyVector
        {
            ModeId = modeId,
            BackgroundId = "bg-1",
            MassLikeScale = new MassLikeScaleRecord
            {
                ModeId = modeId,
                Eigenvalue = mass * mass,
                MassLikeScale = mass,
                ExtractionMethod = "eigenvalue",
                OperatorType = "FullHessian",
                BackgroundId = "bg-1",
            },
            Polarization = new PolarizationDescriptor
            {
                ModeId = modeId,
                BlockEnergyFractions = new Dictionary<string, double> { ["connection"] = 1.0 },
                DominantClass = "connection-dominant",
                DominanceFraction = 1.0,
                BackgroundId = "bg-1",
            },
            Symmetry = new SymmetryDescriptor
            {
                ModeId = modeId,
                BackgroundId = "bg-1",
            },
            GaugeLeakScore = gaugeLeakScore,
            Multiplicity = 1,
        };
    }
}
