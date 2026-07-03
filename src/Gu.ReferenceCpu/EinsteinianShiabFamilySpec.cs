using System.Globalization;

namespace Gu.ReferenceCpu;

/// <summary>
/// The rule that maps a face's incident vertices to the single per-face theta used by the
/// independent-theta epsilon dressing (design §3.5). Both rules were run in Phase442/443 and
/// the verdicts were rule-robust; the physicist pinned lowest-index as default and mandated
/// incident-average as the robustness variant.
/// <list type="bullet">
///   <item><see cref="LowestIndex"/> (DEFAULT): theta_face = theta at the face's lowest-index
///         incident vertex (mesh.Faces[f][0]). Cheapest; byte-identical to all prior results.</item>
///   <item><see cref="IncidentAverage"/>: theta_face = mean of theta over ALL incident vertices.
///         Manifestly translation-covariant (no arbitrary global-index choice), so on a
///         translation-invariant background the joint Hessian commutes with lattice translations —
///         the property the momentum block-diagonalization on a periodic mesh needs.</item>
/// </list>
/// </summary>
public enum VertexFaceRule
{
    /// <summary>theta_face = theta at the lowest-index incident vertex (default; byte-identical to prior results).</summary>
    LowestIndex = 0,

    /// <summary>theta_face = mean of theta over all incident vertices (translation-covariant).</summary>
    IncidentAverage = 1,
}

/// <summary>
/// One invariant element Phi drawn from the reduced Spin(4) slice menu
/// (physicist-4d decision 3 / design §3.4). It names a form degree and the
/// concrete Lambda^2(T*X^4) endomorphism it realizes:
/// <list type="bullet">
///   <item>"id0"  (degree 0): identity 1_{End(S)} — scalar/trace element.</item>
///   <item>"sd2"  (degree 2): self-dual projector P_+ on the 6-dim Lambda^2.</item>
///   <item>"asd2" (degree 2): anti-self-dual projector P_-.</item>
///   <item>"vol4" (degree 4): the Hodge ∗ / parity element.</item>
///   <item>"none" (degree -1): sentinel meaning "no term" — only valid for Phi2,
///         producing the one-term (Ricci-like only) contraction. {Phi1=id0,
///         Phi2=none} is the exact identity-Shiab control anchor.</item>
/// </list>
/// This is a sealed class with init properties (repo convention: no records).
/// </summary>
public sealed class InvariantElementSpec
{
    /// <summary>Form degree of the invariant element (0, 2, or 4; -1 for "none").</summary>
    public int FormDegree { get; init; }

    /// <summary>Invariant element identifier: "id0" | "sd2" | "asd2" | "vol4" | "none".</summary>
    public string InvariantElement { get; init; } = "id0";

    /// <summary>The four admissible menu elements plus the Phi2-only "none" sentinel.</summary>
    public static readonly IReadOnlyList<string> Admissible =
        new[] { "id0", "sd2", "asd2", "vol4", "none" };

    /// <summary>id0 element (scalar/identity on Lambda^2, form degree 0).</summary>
    public static InvariantElementSpec Id0 => new() { FormDegree = 0, InvariantElement = "id0" };

    /// <summary>Self-dual projector P_+ (form degree 2).</summary>
    public static InvariantElementSpec Sd2 => new() { FormDegree = 2, InvariantElement = "sd2" };

    /// <summary>Anti-self-dual projector P_- (form degree 2).</summary>
    public static InvariantElementSpec Asd2 => new() { FormDegree = 2, InvariantElement = "asd2" };

    /// <summary>Volume / Hodge-parity element (form degree 4).</summary>
    public static InvariantElementSpec Vol4 => new() { FormDegree = 4, InvariantElement = "vol4" };

    /// <summary>"none" sentinel: second term absent (one-term contraction). Phi2 only.</summary>
    public static InvariantElementSpec None => new() { FormDegree = -1, InvariantElement = "none" };
}

/// <summary>
/// A single member of the Einsteinian Shiab candidate family (design §3.4,
/// physics-decisions §4.4). Verbatim field set: Phi1/Phi2 invariant elements,
/// the Einstein coefficient c, the bracket type, and the epsilon mode (the
/// scale-breaking lever). The realized operator is the degree-preserving
/// Omega^2 -> Omega^2 contraction
///   R(xi) = A(Phi1) ( xi - c * A(Phi2)(xi) )
/// on the 6-dimensional Lambda^2(T*X^4), dressed on the ad-index by the
/// epsilon-conjugation Ad_eps (see <see cref="EinsteinianShiabOperator"/>).
/// This is a Phase441-style universality-sweep spec: no canonical member.
/// </summary>
public sealed class EinsteinianShiabFamilyMember
{
    /// <summary>Phi_1: appears in both terms.</summary>
    public InvariantElementSpec Phi1 { get; init; } = InvariantElementSpec.Id0;

    /// <summary>Phi_2: appears in the second (Ricci-scalar-like) term only.
    /// Set to <see cref="InvariantElementSpec.None"/> for the one-term operator.</summary>
    public InvariantElementSpec Phi2 { get; init; } = InvariantElementSpec.Id0;

    /// <summary>Einstein coefficient c. PINNED default 0.5 (Einstein-analogy).
    /// Sweep {0, 0.5, 1}; c=0 degenerates to the one-term "Ricci-like" contraction.</summary>
    public double EinsteinCoefficient { get; init; } = 0.5;

    /// <summary>Bracket type. PINNED default "commutator" (matches existing [omega,omega]).
    /// "i-anticommutator" is a recorded variant; see <see cref="EinsteinianShiabOperator"/>
    /// remarks for the recorded-boundary note on its numerical realization.</summary>
    public string BracketType { get; init; } = "commutator";

    /// <summary>
    /// THE SCALE-BREAKING LEVER (physics-decisions §0/§4.3). Three modes:
    /// <list type="bullet">
    ///   <item>"trivial": eps = 1 — CONTROL, linear in omega, degree-2 Hessian
    ///         (reproduces the Phase436/441 no-go on a bigger mesh).</item>
    ///   <item>"frozen-background": eps from a FIXED background field — still LINEAR in omega.</item>
    ///   <item>"slaved-wilson-smoketest": the slaved-Wilson SMOKE-TEST eps(omega)=exp(kappa*sum omega_e)
    ///         — NONLINEAR in omega, but NON-GATING and NOT the pinned physics treatment. Physics
    ///         memo §6e (resolved 2026-07-02) LOCKED the pinned omega-coupling as independent-theta
    ///         (a genuine independent field with a joint (omega,theta) Hessian), which arrives as a
    ///         separate mode when the co-signed design §3.5 lands (possibly M3b). This mode is a
    ///         labeled surrogate for wiring/degree-probe smoke-tests only.</item>
    /// </list>
    /// </summary>
    public string EpsilonMode { get; init; } = "trivial";

    /// <summary>
    /// Vertex -> face theta rule for the independent-theta epsilon dressing. PINNED default
    /// <see cref="Gu.ReferenceCpu.VertexFaceRule.LowestIndex"/> (byte-identical to all prior
    /// results). <see cref="Gu.ReferenceCpu.VertexFaceRule.IncidentAverage"/> is the
    /// translation-covariant robustness variant. Only affects the independent-theta mode; the
    /// theta=0 slice (hence the omega-sector and every non-theta member) is unchanged either way.
    /// </summary>
    public VertexFaceRule VertexFaceRule { get; init; } = VertexFaceRule.LowestIndex;

    /// <summary>
    /// Derived branch identifier, e.g.
    /// "einsteinian-shiab/sd2-id0/c0.5/comm/slaved-wilson-smoketest". The
    /// "/avg" suffix is appended only for the non-default incident-average vertex-face rule,
    /// so every prior BranchId is byte-identical.
    /// </summary>
    public string BranchId =>
        $"einsteinian-shiab/{Phi1.InvariantElement}-{Phi2.InvariantElement}" +
        $"/c{EinsteinCoefficient.ToString("0.###", CultureInfo.InvariantCulture)}" +
        $"/{Abbrev(BracketType)}/{EpsilonMode}" +
        (VertexFaceRule == VertexFaceRule.IncidentAverage ? "/avg" : string.Empty);

    private static string Abbrev(string bracketType) => bracketType switch
    {
        "commutator" => "comm",
        "i-anticommutator" => "ianti",
        _ => bracketType,
    };
}
