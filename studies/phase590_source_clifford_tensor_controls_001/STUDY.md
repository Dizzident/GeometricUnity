# Phase590: mixed-signature Clifford and invariant-tensor controls

Prospective Amendment A48 exact deterministic control. The existing
GammaMatrixBuilder is compared with independent signed Clifford arithmetic;
no core gamma or chirality convention is changed. Source Eqs8.5-8.7 motivate
the real-form and tensor questions. A valid candidate is not the author's
selected operator, a complete invariant-space classification, a registered
field identification, or a physical prediction. All coefficients below remain
formal real parameters. The source's word 'normed' supplies no chosen values.

## Independent matrix and blade conventions

Use Cl(7,7), eta_ab=sigma_a delta_ab, with sigma_0..6=+1 and
sigma_7..13=-1. The generators obey gamma_a gamma_b+gamma_b gamma_a=2eta_ab.
Spinors have complex dimension128. The independent representation acts on
seven-bit column labels. For mu=2k or2k+1, flip bit6-k, multiply by the
parity of all lower bits, and for odd mu additionally multiply by
i*(-1)^(the flipped input bit). Negative-signature generators receive another
factor i. This directly evaluates Pauli action on basis vectors, rather than
calling the core's recursive Kronecker-product implementation.

Compare every entry of all14 actual128x128 matrices with the independently
predicted signed Gaussian-unit permutation matrix, exactly. Only after this
comparison convert the actual matrices to sparse monomial maps. All16384
increasing-order blades are products of these validated actual generators.
Independent blade multiplication has mask xor and sign

    (-1)^(number of pairs i in I,j in J with i>j)
    times product_(k in I intersect J) sigma_k.

Check all229376 blade/right-generator products, all blade squares, traces,
and adjoints. All arithmetic after the existing builder uses checked integer
Gaussian coefficients or permutation/phase operations; no tolerance, matrix
diagonalization or nullspace search is needed. The dense core builder uses
Complex float64 operations on integers and Gaussian units; compare with zero
tolerance because this fixed construction has no irrational matrix entries.

## Hermitian form and grade rule

Let H=gamma_7...gamma_13, a product of seven anti-Hermitian negative generators.
Reversing seven factors contributes (-1)^21 and conjugating their signs
contributes (-1)^7, so H^dagger=H. Squaring gives the same total sign,
H^2=I. H anticommutes with every positive generator and commutes with every
negative generator. Since positive gammas are Hermitian and negative gammas
anti-Hermitian, gamma_a^dagger H=-H gamma_a for all14 directions.
Conjugation by an invertible positive gamma changes H to -H, giving TrH=0.
Hermiticity, involution and trace zero imply64 positive and64 negative
eigenvalues without computing eigenvectors. Thus this H defines a balanced
Hermitian form of signature(64,64), not a source-selected normalization.

Moving H through the adjoint of an r-blade gives

    B_r^dagger H=(-1)^(r(r+1)/2) H B_r.

Real grades1,2 modulo4 are H-anti-Hermitian; multiplying grades0,3 modulo4
by i makes them so. The all-blade battery has8128 real and8256 imaginary
H-anti-Hermitian basis directions,16384 total. Non-scalar blade traces vanish;
together with the signed multiplication law this also supplies trace
orthogonality and independence of the complex Clifford blade basis. The
phase-adjusted directions form a real basis of u(H). It does not follow that
any particular invariant tensor or source contraction has been selected.

Let Omega=gamma_0...gamma_13. Omega^2=(-1)^91*(-1)^7=+I and it
anticommutes with vectors while commuting with bivectors. The core builder's
raw chirality is i^7 Omega=-i Omega, whose square is -I. The diagnostic
rephasing i times raw chirality equals Omega and squares to I. Check all
these signs against the actual returned matrix. This is an expected negative
control on a convention, not permission to alter core code or historical
validation. Do not conflate the raw volume with the builder's phased chirality.

## Four explicit tensor directions and simultaneous Spin invariance

With theta^a dual to the vector representation carried by gamma_a, declare

    C1 = sum_a theta^a tensor gamma_a,
    C2 = sum_(a<b) theta^a wedge theta^b tensor gamma_a gamma_b,
    D1 = sum_a theta^a tensor Omega gamma_a,
    D2 = i sum_(a<b) theta^a wedge theta^b tensor Omega gamma_a gamma_b.

No additional sigma_a multiplies C1: theta^a is already a dual basis, and
the Clifford index is lower. All terms of C1,C2,D1,D2 are H-anti-Hermitian,
with Clifford grades1,2,13 and imaginary grade12 respectively. Test each term
through the actual sparse matrix representation and the all-grade identity.

For Sigma_ab=gamma_a gamma_b/2, a<b,

    [Sigma_ab,gamma_c]=eta_bc gamma_a-eta_ac gamma_b = X_c^d gamma_d,
    delta theta^c=-X_d^c theta^d.

The program uses twice Sigma and twice X to keep every coefficient integral.
For Phi1, the coefficient of theta^d cancels between the adjoint action and
the dual-form action. For Phi2, apply the same rule to both exterior factors
and the derivation rule to the Clifford product; exterior antisymmetry gives
the matching two-form representation. Because Omega commutes with every
Spin generator, both volume-dual companions are invariant as well. Check
all91 generators, including49 boosts and42 same-signature generators, for
all four tensors. There are364 tensor/generator rows and19110 component
matrix-commutator comparisons with independent Clifford products.

The dual-form action is implemented separately by replacing each covector
index and recording the exterior reordering sign. No invariance result is
inserted from the formula being tested. The exact boost example(0,7) has
gamma_0->-gamma_7, gamma_7->-gamma_0, theta^0->+theta^7 and
theta^7->+theta^0 under Sigma, displaying the required metric signs.

Two prospectively frozen decoys are checked. Reversing the dual-form action
sign must fail for each of91 generators on each tensor. The wrong-metric
decoy is applied ONLY to the49 mixed-signature pairs: replace both metric
entries by+1 in the dual-form action while keeping the actual adjoint action.
It must fail on every one of those49 pairs. Same-signature rows retain the
correct metric and are marked decoy-not-applied. This is not a claim that
an all-Euclidean replacement on all91 generators fails on only49; negative
same-signature rotations would also be affected by that different decoy.

## Pairing and explicit nonuniqueness lower bounds

Declare B(X,Y)=-ReTr(XY)/128. On increasing-index r-forms use metric
product sigma_i without an additional factorial (the all-index convention
would require1/r!). Direct matrix trace evaluation gives

    ||C1||^2=-14,       ||C2||^2=+91,
    ||D1||^2=+14,       ||D2||^2=-91,
    <C1,D1>=<C2,D2>=0.

Indeed gamma_a^2=sigma_a, (gamma_a gamma_b)^2=-sigma_a sigma_b,
(Omega gamma_a)^2=-sigma_a, and (i Omega gamma_a gamma_b)^2=+sigma_a sigma_b.
Multiplying by the corresponding form-metric signs and the minus trace gives
the stated norms. Cross terms have zero trace. Canonical and companion terms
occupy disjoint Clifford grades, hence are linearly independent; the all-blade
trace controls exclude accidental matrix dependencies. There are therefore
AT LEAST two admitted invariant directions at form degree1 and at degree2.
This is not a complete classification or proof that their dimensions equal2.

For formal real a,b,c,d the resulting planes have norms
14(-a^2+b^2) and91(c^2-d^2). Even a chosen norm condition would not generally
select a unique direction. No coefficient is fitted or assigned here. A later
canonical-only contraction calculation must explicitly remain a subfamily
because these additional directions are already available.

## Freeze, resource estimates and terminals

Bind own program, project and this study; primary source text; summaries and
contracts of Phases587-589; Directory.Build.props; and the full726-file
src .cs/.csproj manifest. Match the complete fixture object structurally,
validate binding identities and hashes, and verify live source-tree closure
before any scientific evaluation. A fixed Unix-epoch provenance metadata
value keeps outputs deterministic; it is not a claimed execution timestamp.

Stored permutation/phase arrays for all blades use about16 MiB of raw integer
payload. The14 actual dense128x128 complex gammas use3.5 MiB; transient dense
chirality multiplication and exact controls motivate a conservative256 MiB
peak estimate below512 MiB. Twenty CPU seconds are estimated below a60-second
estimate ceiling. No dense all-blade matrices, eigensolver, random sampling,
protected seeds or scientific fit are used. These are prospective estimates,
not an enforced process-memory limit or measured performance claim.

Terminal precedence: invalid/drifted input; known-answer failure;
mixed-signature matrix failure; tensor invariance/pairing failure; then
`mixed-signature-clifford-tensor-controls-pass-source-choice-open`.
Preserve any failed frozen run; scientific repair requires versioning. Every
output keeps all14 authority firewalls false, externalReviewPending=true and
promotedPhysicalMassClaimCount=0. Phase561 remains closed and O4 pending.
