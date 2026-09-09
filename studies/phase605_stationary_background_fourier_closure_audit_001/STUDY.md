# Phase605: stationary-background Fourier closure

Prospective Amendment A56 exact audit. Freeze this proof, all program/helper
sources, project, complete fixtures/counts/resources/precedence and exact
input closure before the FIRST scientific execution. Release build checks
are allowed beforehand. Independent reviewer and MAIN must both read the
complete pack, and only MAIN may authorize its first scientific run.

## Fixed domain and actual first-action Hessian

Retain the declared primary-source CCA chain with BOTH Hodge legs, inner
i-anticommutator, second coefficient -1/2, fixed flat metric/reference,
epsilon identity, fixed invariant tensors and normalized periodic density.
The trace/form pairing B=-ReTr/128 is real, nondegenerate and indefinite.
It is not a selected source9.11 positive norm or a physical time prescription.
Use signature(7,7), Omega squared=1, h=-1,+1, P=1+hOmega,
Pbar=1-hOmega, Phi1=P Gamma1 and Phi2=(c-ihOmega)Gamma2.

The first action has local form

    I(T)=B(T,K dT)/2+gamma B(T,K(T wedge T))/3+kappa B(T,T)/2.

Phase603's full gradient establishes the constant stationary branch
S*=t Pbar Gamma1, t=-kappa/(416gamma), for gamma nonzero. This includes
the origin when kappa=0. It is a fixed-geometry connection stationary point,
not a solution of the metric equations or a chosen physical vacuum. At this
background the FULL connection Hessian is

    H V=H0 V+(gamma/3)(A(V)+Bleg(V)+C(V))+kappa V,
    H0=(K d+d-adjoint K-adjoint)/2,
    A=K(DQ_S* V), Bleg=DQ_V-adjoint K-adjoint S*,
    C=DQ_S*-adjoint K-adjoint V,
    DQ_S V=S wedge V+V wedge S.

Every potential leg is linear in S*, hence gamma*t=-kappa/416 eliminates
gamma algebraically. The executable constructs all three full legs at
Sunit=Pbar Gamma1 and scales their sum by -kappa/1248, then adds H0 and
kappa V. This is valid for EVERY nonzero gamma, not a fit to coupling rows.
The independent original-action oracle uses the computational normalization
gamma=1, S*=-kappa Sunit/416, with the same written cancellation proof.
No gamma=0 branch is inferred by division;603 treats that degeneracy.

The immutable600 literal source and two reverse-chain helpers are compiled
directly and exact-bound. Both reverse chains are compared on every queried
input. No source method knows the expected seven-by-seven matrix or projects
onto a carrier. All actual outputs are checked as complete Clifford/form
tensors before matrix extraction. The immutable601 exact rational matrix
helper supplies Gaussian elimination, inverse, characteristic polynomial
and determinant without numerical tolerances or eigensolvers.

## Seven nonzero-frequency directions and complete potential legs

For nonzero periodic integer n define the basis WITHOUT absorbing n into
its amplitude:

    u=theta0 gamma2 cos(nx0), b=theta2 gamma0 cos(nx0),
    e=sum_(j!=0,2)theta_j gamma2 gamma_j sin(nx0),
    e0=theta0 Gamma20 sin(nx0), q=theta2 Omega sin(nx0).

Order the real H-anti carrier as (u,b,Omega u,Omega b,e,e0,q). In particular
Omega e is H-Hermitian and is NOT an admissible replacement for q. Direct
Clifford traces and the normalized sine/cosine squares give diagonal Gram
(-1/2,-1/2,+1/2,+1/2,6,+1/2,-1/2), determinant -3/32. Cross terms vanish
by Clifford/form orthogonality. The carrier is nondegenerate but indefinite.

At Sunit the three potential legs on any of the four odd directions v are

    A=-48P v, Bleg=-96 swapped(v), C=-48Pbar v,

where swapped exchanges u with b and Omega u with Omega b. For example,
DQ_Sunit u has F0j=2Gamma2j for j!=0,2 and F02=2hOmega. The full K kills
the Omega component by first/second-leg cancellation; each of the twelve
already-scaled F0j=2Gamma2j components supplies -4P u. Also
K-adjoint Sunit=-48Gamma2 and DQ_u-adjoint Gamma2=2b. The part of
K-adjoint u with a0 form index is2 sum theta0j Gamma2j; its remaining
components on pairs ij disjoint from{0,2} are -2hOmega Gammaij Gamma02.
These additional full-adjoint terms commute with the contracted Sunit
coefficient because its index repeats in the grade-four blade, and hence
vanish under DQ_Sunit-adjoint. Contracting the a0-index part against Sunit
supplies -4Pbar u per remaining index. This gives
all three u coefficients separately. Simultaneous positive-axis rotation
gives b; direct Omega algebra gives the two companions. These arguments are
pointwise tensor algebra, not assumptions about global torus rotations.

For E_i=theta_i Gamma2i sin(nx0), i!=2, direct full adjoints give
K-adjoint E_i=-2P sum_(j!=2,i)theta2j gamma_j sin(nx0). The three potential
legs are respectively8 sum E_j,96 sum E_j, and8 sum E_j+96h q.
Contraction signs from negative axes cancel against their Clifford metric
signs, so the index sums include all thirteen axes other than2. Summing
twelve such E_i, or taking i=0, gives

| Input | A | Bleg | C |
| --- | --- | --- | --- |
| e | 8(12e0+11e) | 96(12e0+11e) | 96e0+88e+1152h q |
| e0 | 8e | 96e | 8e+96h q |
| q | -96h(e+e0) | 0 | 0 |

For the last row K-adjoint q=0 by its two-leg cancellation, while
DQ_Sunit q=-2h sum_(i!=2)theta2i Pbar gamma_i sin(nx0). Its forward K
gives -96h(e+e0). These full leg equalities are separate executable controls,
not merely checks of a possibly cancellation-prone final sum.

The full origin differential columns are

    H0u=H0b=n e, H0Omega u=H0Omega b=-hn e,
    H0e=-12n P(u+b), H0e0=H0q=0.

For b, forward K db produces2n e even though outputs0 and2 cancel;
d-adjoint K-adjoint b=0. For e0 the exterior derivative vanishes and its
adjoint has no0-form index; for q, K on the relevant Omega curvature and
K-adjoint q both vanish. The actual executable differentiates the frequency
in each Fourier coefficient, rather than inserting n into an asserted
matrix. Combining the checked origin and potential legs yields

| Input | Full H output |
| --- | --- |
| u | n e+kappa(14u+b)/13 |
| b | n e+kappa(u+14b)/13 |
| Omega u | -hn e+kappa(14Omega u+Omega b)/13 |
| Omega b | -hn e+kappa(Omega u+14Omega b)/13 |
| e | -12n P(u+b)+kappa(e/78-14e0/13-12h q/13) |
| e0 | kappa(e0-7e/78-h q/13) |
| q | kappa(q+h(e+e0)/13) |

Every right side lies in the same nondegenerate carrier. The implementation
recovers its raised matrix by the actual signed Gram inverse and reconstructs
every FULL output tensor. This is closure, not a compression spectrum.

## Independent original-action reciprocity

For each pair V,W independently expand I(S*+sV+tW) and take the scalar
coefficient of st. The quadratic term contributes
[B(V,KdW)+B(W,KdV)]/2. The cubic term has six ordered placements of S,V,W
in B(T,K(T wedge T)); the implementation forms them using ONLY forward K
and ordered wedge products. Each exchanged pair is grouped before the real
H-anti trace pairing, avoiding imaginary individual words that only cancel
in their properly paired sum. Its background-unit coefficient is multiplied
by -kappa/1248. The mass term contributes kappa B(V,W).

This scalar oracle uses no adjoint, expected column or Gram inverse. It
must equal B(V,H W) in every entry. The lowered Hessian is symmetric;
the raised matrix is not required to be ordinarily symmetric in an
indefinite Gram. Cached unit-background coefficients are scaled by exact
couplings only after construction; no action values are fitted.

## Characteristic, actual cyclic closure and nonzero Jordan control

Put E=e+e0, F=e-12e0, x=u+b, z=P x, w=Pbar x/2. On the even E,q block
the potential is kappa[[-1/13,h/13],[-h,1]], with eigenvalues0,12kappa/13.
F has eigen85kappa/78. The two odd antisymmetric directions have eigenkappa;
the two symmetric odd directions have scalar potential alpha=15kappa/13.
The derivative maps w to2n e and e to-12n z, with H0z=0. In the adapted
order(z,E,q,F,w,u-b,Omega(u-b)) these couplings are triangular. Thus

    characteristic=(lambda-kappa)^2(lambda-alpha)^2
                    lambda(lambda-12kappa/13)(lambda-85kappa/78).

This polynomial is independent of n; the matrix and its Jordan coupling
are not. For kappa*n nonzero, the repeated alpha has a size-two block.
Eliminating the even block at alpha uses the exact ee resolvent entry
26/(15kappa). Indeed e=(12E+F)/13, the E-block EE resolvent is26/(45kappa)
and the F resolvent is78/(5kappa), giving that coefficient. The effective
w-to-z Schur coefficient is -24n squared times26/(15kappa), namely

    -208n squared/(5kappa), nonzero.

The program obtains this Schur coefficient from the recovered FULL matrix,
not the target expression; alpha-shift power ranks must be6,5,5.
Full H power ranks are6,6,6. The minimal polynomial has degree6, obtained
by deleting one (lambda-kappa) factor from the characteristic polynomial.
All its matrix coefficients must annihilate the recovered full operator.

The seed u has nonzero components in u-b, w and z. Its w-to-even coupling
reaches all three distinct even roots, and the alpha Jordan coefficient is
nonzero. Hence its cyclic dimension is six. Its cyclic space equals the
span of(u-b,u+b,Omega(u+b),e,e0,q), whose diagonal Gram is
(-1,-1,+1,6,+1/2,-1/2), determinant -3/2. The independent missing direction
Omega(u-b) has eigenkappa and is not generated. The program applies the
FULL source H seven times successively to u, verifies complete output
reconstruction at each step, and checks exact prefix ranks1,2,3,4,5,6,6,6
and nondegeneracy of the first six actual Krylov vectors. It does not create
Krylov vectors by multiplying an expected Jordan matrix.

At kappa0 and nonzero n, the background is the origin: full power ranks
2,1,0, minimal lambda cubed, and seed ranks1,2,3,3,3,3,3,3. The existing
origin Jordan-three control is therefore recovered as an exact degeneracy.

## Actual zero-frequency carrier, not a formal seven-mode specialization

At n=0 all sine fields e,e0,q vanish identically, whereas cos=1 changes
the remaining norms. Use a SEPARATE actual constant basis
(theta0 gamma2,theta2 gamma0,theta0 Omega gamma2,theta2 Omega gamma0),
Gram diag(-1,-1,+1,+1), determinant1. The differential columns vanish and
the potential is two kappa/13[[14,1],[1,14]] blocks. Its characteristic is
(lambda-kappa)^2(lambda-15kappa/13)^2. For nonzero kappa its minimal
polynomial has the two distinct factors, full power ranks4,4,4 and seed
ranks1,2,2,2,2. At kappa0 the constant operator is zero: minimal lambda,
full ranks0,0,0 and seed ranks1,1,1,1,1. Four actual source applications
per row check these claims. No sine basis or nonzero-frequency Gram is
retained at zero frequency.

## Formal-c failure of closure, not general-c eigenvalues

Retain only a fixed complete seed-u coefficient audit beyond c0. Define
Z_n=sum_(j!=0,2)theta_j Gamma0j gamma2 cos(nx0). The three c-coefficient
potential legs at Sunit are

    A_c=4iP Z_n, B_c=-112ih Omega Z_n, C_c=-44iPbar Z_n.

For A, the contracted anticommutator in K_c kills the shared-index
bivectors of DQ_Sunit u but retains F02=2hOmega. For B use
K_c-adjoint Sunit=-56ih Omega Gamma2. For C, K_c-adjoint u has02
coefficient2iI, whose commutator contraction is zero, and disjoint-pair
coefficients -2iGamma_ab Gamma02. Each output j receives eleven
contractions -4iPbar Z_j. Thus their sum is -8i(5+8hOmega)Z_n.
The full c coefficient is

    n i theta2 I sin(nx0)+i kappa(5+8hOmega)Z_n/156.

The even central term cannot cancel the odd grade3/11 term. Both are in
the real H-anti carrier, but outside the c0 seven-direction space. For
every declared nonzero n, including kappa0, the coefficient is nonzero
and its B-orthogonal projection into that seven-space is zero. The full
literal c operator and all three c potential legs are checked independently
against these expressions. No general-c eigenvalues or closed carrier are
claimed. This is a prospective falsifier of unjustified c0 extrapolation.

## Frozen finite menu, counts, resources and authority

Use both h, n=1,2 and kappa=-1,0,1, plus the separate actual n=0 constant
menu at the same couplings. These are exact known-answer controls, not
sampled/fitted parameters or physical coupling choices. The written
source algebra proves the n/kappa dependence before the finite controls;
the second nonzero frequency explicitly exercises derivative and Jordan
scaling. This gives12 nonzero-Fourier rows and6 constant rows.

Frozen totals:44944 word cases;16384 Hodge cases;4 matrix and3 frequency
known answers;36 base columns;108 separate potential-leg checks;36 origin
checks;18 rows;108 full combined columns;684 Gram entries and684 independent
original-action Hessian bilinears;108 actual Krylov applications;126 prefix
rank checks;18 characteristic/minimal rows;8 nonzero Jordan/Schur rows;
12 c-leg comparisons and12 full formal-c leakage rows. No row is selected
after observing results. Every exact tolerance is zero.

Full Fourier outputs/iterates have at most36 tensor terms in the seven-space;
constant outputs have at most4; the full c leakage has at most50 (two central,
24 grade3 and24 grade11 terms). These support bounds are checked. All source
and reverse intermediates are finite exact Fourier/Clifford products, with
no frequency convolution growth beyond the input modes because the source
tensors and background are constant. The longest source Krylov loop is
seven applications. Exact characteristic determinants are at most7-by-7;
no large numerical eigensystem or unrestricted search is used. Prospective
estimate30 CPU seconds/128MiB, conservative estimate ceilings120 CPU
seconds/256MiB; these are estimates, not measured claims or runtime timeouts.

Eighteen unique exact bindings cover own program, closure helper, project,
proof; primary source;600 summary/contract/program and all three compiled
helpers;601 summary/contract and compiled matrix helper;603 summary/contract;
the complete core manifest and Directory.Build.props. Verify all726 live
src C#/project paths and hashes, excluding bin/obj, and the sorted tree hash.
Passed upstream terminals, complete fixture JSON and terminal precedence
are mandatory, as are all fourteen exact false authority names. Full and
summary outputs are deterministic and byte-identical.

Precedence: invalid-or-drifted-input; known-answer-control-failed;
full-source-column-control-failed; original-action-control-failed;
cyclic-jordan-control-failed; formal-c-leakage-control-failed;
`stationary-background-fourier-closure-controls-pass-no-physical-dispersion`.
Only the last terminal passes. Preserve any failed first output; scientific
repairs require a versioned pack and renewed complete review.

All fourteen authority flags remain false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0. No sampling, core/historical rewrite,
source norm, time split, field identification, physical dispersion/pole,
vacuum, scale or unit law is selected. The full closed fixed-geometry
connection carrier is not the full metric-plus-connection GU Hessian.
