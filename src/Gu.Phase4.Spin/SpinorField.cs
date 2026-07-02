using System.Numerics;
using Gu.Geometry;

namespace Gu.Phase4.Spin;

/// <summary>
/// Vertex-valued Dirac spinor field on a <see cref="SimplicialMesh"/>.
///
/// PHYSICS: fermions live in sections of S(Y) ⊗ V_ρ, where S(Y) is the Clifford
/// spinor bundle (dimension 2^floor(dim/2); 4 in 4D) and V_ρ is the gauge
/// representation bundle. <see cref="SpinorDimension"/> tracks the Clifford index
/// only; <see cref="GaugeComponents"/> reserves layout space for the ⊗ V_ρ
/// extension. Its default is 1 (pure Clifford spinor) so M2 stays minimal.
///
/// PHYSICIST-GATED: whether and how V_ρ is coupled (the gauge index dynamics) is
/// out of M2 scope. The multiplier only reserves storage; it commits to nothing.
///
/// Storage is a flat complex array in vertex-major, then gauge, then spinor
/// order, matching the [carrier * dim + component] layout used across the
/// codebase:
///   Values[(vertex * GaugeComponents + g) * SpinorDimension + s].
/// Length = VertexCount * GaugeComponents * SpinorDimension.
///
/// RELATION TO Gu.Phase4.Dirac (design §2.3): the discrete Dirac assembler
/// (CpuDiracOperatorAssembler / FermionSpectralSolver) does NOT use a typed
/// vertex-spinor object — it operates on a flat real double[] with the same DOF
/// ordering but complex numbers stored as interleaved (Re, Im) pairs, i.e. real
/// index 2*((vertex*GaugeComponents+g)*SpinorDimension+s) + {0,1}. There is no
/// existing typed vertex-spinor class to reuse, so SpinorField is added here as
/// the typed complex convenience representation for the Clifford layer; a caller
/// bridges to the Dirac solver by de-interleaving Values into that real array.
/// </summary>
public sealed class SpinorField
{
    /// <summary>The mesh this field is sampled on (spinors live on vertices).</summary>
    public SimplicialMesh Mesh { get; }

    /// <summary>Clifford spinor dimension = 2^floor(dim/2); 4 for the 4D algebras.</summary>
    public int SpinorDimension { get; }

    /// <summary>Gauge representation multiplicity (⊗ V_ρ). Default 1 (pure Clifford spinor).</summary>
    public int GaugeComponents { get; }

    /// <summary>
    /// Flat complex coefficients, vertex-major then gauge then spinor:
    /// Values[(vertex * GaugeComponents + g) * SpinorDimension + s].
    /// Length = VertexCount * GaugeComponents * SpinorDimension.
    /// </summary>
    public Complex[] Values { get; }

    /// <summary>Number of complex numbers stored per vertex (GaugeComponents * SpinorDimension).</summary>
    public int ComponentsPerVertex => GaugeComponents * SpinorDimension;

    /// <summary>
    /// Construct a spinor field. When <paramref name="values"/> is null a
    /// zero-initialized backing array of the correct length is allocated.
    /// </summary>
    public SpinorField(SimplicialMesh mesh, int spinorDimension, int gaugeComponents = 1, Complex[]? values = null)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (spinorDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(spinorDimension), spinorDimension, "SpinorDimension must be >= 1.");
        if (gaugeComponents < 1)
            throw new ArgumentOutOfRangeException(nameof(gaugeComponents), gaugeComponents, "GaugeComponents must be >= 1.");

        Mesh = mesh;
        SpinorDimension = spinorDimension;
        GaugeComponents = gaugeComponents;

        int expected = mesh.VertexCount * gaugeComponents * spinorDimension;
        if (values is null)
        {
            Values = new Complex[expected];
        }
        else
        {
            if (values.Length != expected)
                throw new ArgumentException(
                    $"Values length {values.Length} does not match VertexCount ({mesh.VertexCount}) * GaugeComponents ({gaugeComponents}) * SpinorDimension ({spinorDimension}) = {expected}.",
                    nameof(values));
            Values = values;
        }
    }

    /// <summary>Flat index of spinor component (vertex, gauge, spinor) in <see cref="Values"/>.</summary>
    public int Index(int vertex, int gauge, int spinor)
        => (vertex * GaugeComponents + gauge) * SpinorDimension + spinor;

    /// <summary>
    /// The contiguous block of one vertex's components as a read-only span.
    /// Length = ComponentsPerVertex (GaugeComponents * SpinorDimension); ordered
    /// gauge-major then spinor.
    /// </summary>
    public ReadOnlySpan<Complex> GetVertexSpinor(int vertex)
    {
        if ((uint)vertex >= (uint)Mesh.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(vertex), vertex, "Vertex index out of range.");
        return new ReadOnlySpan<Complex>(Values, vertex * ComponentsPerVertex, ComponentsPerVertex);
    }

    /// <summary>The writable block of one vertex's components.</summary>
    public Span<Complex> GetVertexSpinorMutable(int vertex)
    {
        if ((uint)vertex >= (uint)Mesh.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(vertex), vertex, "Vertex index out of range.");
        return new Span<Complex>(Values, vertex * ComponentsPerVertex, ComponentsPerVertex);
    }

    /// <summary>A deep copy sharing the same mesh reference.</summary>
    public SpinorField Clone()
        => new(Mesh, SpinorDimension, GaugeComponents, (Complex[])Values.Clone());

    /// <summary>
    /// Convert to the Gu.Phase4.Dirac assembler's flat real spinor vector: complex
    /// DOF k (= <see cref="Index"/>) maps to real entries (2k, 2k+1) = (Re, Im).
    /// This is the lossless bridge to CpuDiracOperatorAssembler / FermionSpectralSolver,
    /// whose complex DOF index equals <see cref="Index"/>. Length = 2 * Values.Length
    /// = 2 * VertexCount * GaugeComponents * SpinorDimension.
    /// </summary>
    public double[] ToFlat()
    {
        var flat = new double[2 * Values.Length];
        for (int k = 0; k < Values.Length; k++)
        {
            flat[2 * k] = Values[k].Real;
            flat[2 * k + 1] = Values[k].Imaginary;
        }
        return flat;
    }

    /// <summary>
    /// Reconstruct a <see cref="SpinorField"/> from the assembler's flat real
    /// spinor vector (inverse of <see cref="ToFlat"/>). <paramref name="flat"/>
    /// must have length 2 * VertexCount * GaugeComponents * SpinorDimension.
    /// </summary>
    public static SpinorField FromFlat(SimplicialMesh mesh, int spinorDimension, int gaugeComponents, double[] flat)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(flat);
        int expected = 2 * mesh.VertexCount * gaugeComponents * spinorDimension;
        if (flat.Length != expected)
            throw new ArgumentException(
                $"flat length {flat.Length} does not match 2 * VertexCount ({mesh.VertexCount}) * GaugeComponents ({gaugeComponents}) * SpinorDimension ({spinorDimension}) = {expected}.",
                nameof(flat));

        var values = new Complex[flat.Length / 2];
        for (int k = 0; k < values.Length; k++)
            values[k] = new Complex(flat[2 * k], flat[2 * k + 1]);
        return new SpinorField(mesh, spinorDimension, gaugeComponents, values);
    }

    /// <summary>Component-wise sum. Both fields must share layout (mesh, dims).</summary>
    public SpinorField Add(SpinorField other)
    {
        RequireSameLayout(other);
        var result = new Complex[Values.Length];
        for (int i = 0; i < Values.Length; i++)
            result[i] = Values[i] + other.Values[i];
        return new SpinorField(Mesh, SpinorDimension, GaugeComponents, result);
    }

    /// <summary>Component-wise difference. Both fields must share layout.</summary>
    public SpinorField Subtract(SpinorField other)
    {
        RequireSameLayout(other);
        var result = new Complex[Values.Length];
        for (int i = 0; i < Values.Length; i++)
            result[i] = Values[i] - other.Values[i];
        return new SpinorField(Mesh, SpinorDimension, GaugeComponents, result);
    }

    /// <summary>Scale every component by a complex scalar.</summary>
    public SpinorField Scale(Complex factor)
    {
        var result = new Complex[Values.Length];
        for (int i = 0; i < Values.Length; i++)
            result[i] = Values[i] * factor;
        return new SpinorField(Mesh, SpinorDimension, GaugeComponents, result);
    }

    /// <summary>
    /// Hermitian inner product ⟨this, other⟩ = Σ_i conj(this_i) * other_i over all
    /// components (the discrete L² spinor pairing with unit vertex weights).
    /// </summary>
    public Complex InnerProduct(SpinorField other)
    {
        RequireSameLayout(other);
        Complex sum = Complex.Zero;
        for (int i = 0; i < Values.Length; i++)
            sum += Complex.Conjugate(Values[i]) * other.Values[i];
        return sum;
    }

    private void RequireSameLayout(SpinorField other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (!ReferenceEquals(Mesh, other.Mesh)
            || SpinorDimension != other.SpinorDimension
            || GaugeComponents != other.GaugeComponents)
            throw new ArgumentException("SpinorField layout mismatch (mesh, spinor dimension, or gauge components differ).", nameof(other));
    }
}
