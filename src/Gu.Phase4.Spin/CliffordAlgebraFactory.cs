using System.Numerics;
using Gu.Core;

namespace Gu.Phase4.Spin;

/// <summary>
/// Factory for the 4D Clifford / gamma bundle used by the platform's spinor layer.
///
/// The signature is a *parameter*, never a buried constant: the physicist selects
/// Cl(4,0) (Euclidean, PRIMARY per FOUR_D_PLATFORM_PHYSICS_DECISIONS.md §1) or
/// Cl(3,1) (Lorentzian branch variant) at the call site. Two explicitly-named
/// convenience methods exist so neither signature is privileged inside the core
/// method — the decision is a visible call, not a default.
///
/// PHYSICIST-GATED (open question #1): the physical selection of Cl(3,1) vs
/// Cl(4,0) as the default 4D base. The physics memo pins Cl(4,0) as the primary
/// Euclidean slice (*^2 = +1 on 2-forms gives the real self-dual/anti-self-dual
/// Λ² split the Einsteinian Shiab needs) and keeps Cl(3,1) as a supported branch
/// variant. This factory builds the *family*; it does not choose for the caller.
///
/// Built entirely on top of the existing production <see cref="GammaMatrixBuilder"/>
/// (recursive Pauli tensor-product construction); no relocation, no modification.
/// </summary>
public static class CliffordAlgebraFactory
{
    /// <summary>Default gamma convention id, matching <see cref="GammaConventionSpec"/>.</summary>
    public const string DefaultConventionId = "dirac-tensor-product-v1";

    /// <summary>
    /// Build the 4D gamma bundle for the given signature. The signature is
    /// supplied by the caller (the physicist selects Cl(3,1) or Cl(4,0)); this
    /// core method chooses nothing.
    ///
    /// Contract: <c>signature.Dimension == 4</c>; the result has
    /// <c>SpinorDimension == 4</c> and a non-null chirality matrix (even
    /// dimension), and it is validated in-place via
    /// <see cref="CliffordAlgebraValidator"/> — the anticommutator
    /// {γ^μ, γ^ν} = 2 η^{μν} I with η = diag(+,+,+,+) or diag(+,+,+,−). The
    /// <see cref="CliffordValidationResult"/> is stored on the returned bundle.
    /// </summary>
    /// <param name="signature">Cl(p,q) with p+q == 4. e.g. {Positive=4,Negative=0} or {Positive=3,Negative=1}.</param>
    /// <param name="convention">Gamma convention; defaults to <see cref="DefaultConventionId"/>.</param>
    /// <param name="provenance">Optional provenance; a minimal record is synthesized when null.</param>
    public static GammaOperatorBundle CreateClifford4D(
        CliffordSignature signature,
        GammaConventionSpec? convention = null,
        ProvenanceMeta? provenance = null)
    {
        ArgumentNullException.ThrowIfNull(signature);
        if (signature.Dimension != 4)
            throw new ArgumentException(
                $"CreateClifford4D requires a 4-dimensional signature; got Cl({signature.Positive},{signature.Negative}) = dimension {signature.Dimension}.",
                nameof(signature));

        int spinorDim = GammaMatrixBuilder.SpinorDimension(signature.Dimension);
        var conv = convention ?? new GammaConventionSpec
        {
            ConventionId = DefaultConventionId,
            Signature = signature,
            Representation = "standard",
            SpinorDimension = spinorDim,
            HasChirality = signature.Dimension % 2 == 0,
        };

        var prov = provenance ?? new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "unspecified",
            Branch = new BranchRef { BranchId = "clifford-4d", SchemaVersion = "1.0.0" },
            Notes = $"CliffordAlgebraFactory.CreateClifford4D Cl({signature.Positive},{signature.Negative})",
        };

        var builder = new GammaMatrixBuilder();
        var bundle = builder.Build(signature, conv, prov);

        // Chirality phase normalization for the validator's Gamma_chi^2 = I check.
        // The shared GammaMatrixBuilder uses the phase i^(n/2), which gives
        // Gamma_chi^2 = +I only for the Riemannian construction. For Cl(3,1) it
        // multiplies the negative-signature gamma by i but keeps the same phase,
        // so the built Gamma_chi squares to -I. We must not modify the builder
        // (it is consumed elsewhere); instead we re-phase the chirality matrix
        // here so Gamma_chi^2 = I holds for BOTH signatures, as the design's
        // §2.2 acceptance requires. Re-phasing by a scalar does not touch
        // {Gamma_chi, gamma_mu} = 0. Cl(4,0) is unchanged (its phase already
        // gives +I). This is additive and local to the 4D factory.
        var chirality = NormalizeChiralityPhase(bundle.ChiralityMatrix);

        var rephased = new GammaOperatorBundle
        {
            ConventionId = bundle.ConventionId,
            Signature = bundle.Signature,
            SpinorDimension = bundle.SpinorDimension,
            GammaMatrices = bundle.GammaMatrices,
            ChiralityMatrix = chirality,
            Provenance = bundle.Provenance,
        };

        // Validate and re-attach the result. GammaOperatorBundle is sealed with
        // init-only properties (not a record), so we rebuild it carrying the
        // validation result instead of using `with`.
        var validation = new CliffordAlgebraValidator().Validate(rephased);
        return new GammaOperatorBundle
        {
            ConventionId = rephased.ConventionId,
            Signature = rephased.Signature,
            SpinorDimension = rephased.SpinorDimension,
            GammaMatrices = rephased.GammaMatrices,
            ChiralityMatrix = rephased.ChiralityMatrix,
            ValidationResult = validation,
            Provenance = rephased.Provenance,
        };
    }

    /// <summary>
    /// Re-phase a chirality matrix so that Gamma_chi^2 = I. For these algebras
    /// Gamma_chi^2 is a scalar multiple c·I (c = ±1); the corrected matrix is
    /// Gamma_chi / sqrt(c), which squares to I. Returns null if input is null.
    /// </summary>
    private static Complex[,]? NormalizeChiralityPhase(Complex[,]? chirality)
    {
        if (chirality is null) return null;

        int s = chirality.GetLength(0);
        // c = (Gamma_chi^2)[0,0] (Gamma_chi^2 is c·I for these algebras).
        Complex c = Complex.Zero;
        for (int k = 0; k < s; k++)
            c += chirality[0, k] * chirality[k, 0];

        // Already squares to +I (within tolerance): leave untouched.
        if ((c - Complex.One).Magnitude < 1e-12)
            return chirality;

        Complex factor = Complex.One / Complex.Sqrt(c);
        var result = new Complex[s, s];
        for (int i = 0; i < s; i++)
            for (int j = 0; j < s; j++)
                result[i, j] = factor * chirality[i, j];
        return result;
    }

    /// <summary>
    /// Convenience: the PRIMARY Euclidean 4D bundle, Cl(4,0). Riemannian, all
    /// gammas Hermitian, *^2 = +1 on 2-forms (real self-dual/anti-self-dual Λ²
    /// split). This is the physics memo's primary realization (§1); it is exposed
    /// as an explicit named call so the choice is visible, not defaulted.
    /// </summary>
    public static GammaOperatorBundle CreateClifford4DRiemannian(
        GammaConventionSpec? convention = null,
        ProvenanceMeta? provenance = null)
        => CreateClifford4D(new CliffordSignature { Positive = 4, Negative = 0 }, convention, provenance);

    /// <summary>
    /// Convenience: the Lorentzian 4D bundle, Cl(3,1). Branch variant per the
    /// physics memo (§1) — the physical base spacetime is Lorentzian, but the
    /// clean real Ricci/Weyl projectors the Einsteinian Shiab needs are complex in
    /// Lorentzian signature, so this is a supported variant rather than the
    /// primary slice. Negative-signature gamma is multiplied by i (anti-Hermitian)
    /// by <see cref="GammaMatrixBuilder"/>.
    /// </summary>
    public static GammaOperatorBundle CreateClifford4DLorentzian(
        GammaConventionSpec? convention = null,
        ProvenanceMeta? provenance = null)
        => CreateClifford4D(new CliffordSignature { Positive = 3, Negative = 1 }, convention, provenance);
}
