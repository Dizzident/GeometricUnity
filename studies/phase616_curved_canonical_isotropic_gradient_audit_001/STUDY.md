# Phase616: curved canonical isotropic gradient

Prospective A61 audit on main. Complete code, compiled helpers, proof,
project, fixtures, rational coefficients, counts, resources and lineage freeze
before FIRST science. Build-only verification is allowed. Complete independent
and MAIN review and explicit MAIN execution approval are mandatory. Success:
`curved-canonical-isotropic-controls-pass-no-isotropic-branch`.

## Conditional model and scope

Use the canonical UNTIED CAA law of611: first C=commutator, inner and
outer A=i anticommutator; Phi1=Gamma1, Phi2=Gamma2. This is not610's
tied chiral CCA law and is not selected as the author's final operator.
Use only the two passed608 alpha1,beta-1/2,sigma-1 rows, at
y=diag(-1,1,1,1) and diag(-1,4,9,16), with the full recovered coordinate
curvature. No Einstein oracle replaces this literal curvature input.
The declaration has seven positive and seven negative frame directions.

Primary3.7-3.17 supplies the conditional metric-bundle geometry, musical
identification and downstairs Levi-Civita splitting, not a unique value of
all normalization constants. Source9.1 restricts the metric variable to
MET(X). The bracket/Hodge contraction9.3 and action9.4 motivate the local
diagnostic. The original action's actual variational derivative is used;
this does not repair its independently identified mismatch with the printed
claimed force. The declared real bilinear trace/form pairing is not a
positive norm and is not a choice of the still-missing source norm9.11.

This audit concerns ONLY constant scalar S=s Gamma1 on this declared curved
background. It does not reject anisotropic or nonconstant fields, another
operator convention, general coupled vacua or the source theory. No metric
Euler equation, observer-section pullback, fibre integral/boundary condition,
physical pole, particle extraction, unit scale or mass is computed.

## Full curvature reconstruction and independent controls

Read608's exact sparse coordinate curvature, rejecting repeated, zero,
out-of-range coefficients; all absent entries mean exact zero. The full
14^4 array enters two independent routes from immutable610 SpinGeometry.
Route one mixes both input slots then conjugates each curvature endomorphism.
Route two first lowers the output with coordinate G, then transforms all
four covariant slots independently. Every entry agrees after the remaining
frame metric sign; all skew, pair exchange and Bianchi identities are checked.

Use610's positively oriented rational frame
H0,A1,A2,A3,p1,p2,p3;H1,H2,H3,T,n1,n2,n3. The complete frame and hand
inverse formulas are bound in610's proof/helper; here Gaussian inverse,
hand inverse and eta E^T G agree entrywise, with both inverse products.
Frame determinants are1/8 and41472. Reversing the trace column retains
orthonormality but reverses orientation, an explicit planted error.
The transport is diag(L^-T,Sym L), L=diag(1,2,3,4). Both framed curvature
arrays must agree on every one of38416 entries.

With L_abcd=G(R(a,b)c,d), the spin curvature is
F_ab=-(1/2)sum_(c<d) L_abcd sigma_c sigma_d gamma_c gamma_d.
Its full commutator with all14 Clifford vectors equals gamma(R(a,b)e)
for each of91 exterior pairs at both points. The grouped Clifford kernel
also agrees with independent word reduction. All91 unit internal planes
are separate known answers:49 mixed-sign planes detect omission of metric
signs;42 same-sign controls remain unchanged; all91 detect removal of the
half factor or reversal of curvature sign. The actual induced(0,7),gamma7
anchor is-gamma0/4; the three errors yield+gamma0/4,-gamma0/2,+gamma0/4.

Ricci is contracted from the actual framed curvature, separately from
lowered-curvature contraction and transported608 Ricci. Raised Ricci is
-5/4 on traceless-vertical axes1,2,3,4,5,6,11,12,13, and+1/4 on the
other five. Scalar=-10. Complete raised Einstein agrees with its transported
608 matrix and has eigenvalues15/4 and21/4. These are oracle checks,
not the construction of F_B.

## Full canonical contraction and adjoint

The literal CAA stages are
sf=star F, one=C(Gamma1,sf), inner=A(Gamma2,sf), zero=star inner,
outer=A(Gamma1,zero), upper=one-star outer/2, K(F)=star upper.
All eight stages are checked against independent word-kernel multiplication,
including the input and all zero entries in tensor equality. Each has exact
form type2,12,13,14,0,1,13,1 and H-antiHermitian membership.

The independent stage predictions are
star(one)=-RicciGamma, inner=5i top, zero=-5i,
outer=10Gamma1 and K(F)=-EinsteinGamma.
Thus the second lower contribution is-5Gamma1 and is nonzero.
The final fourteen coefficients have raw square2115/8,
trace/form self-pair-2115/8, and Pair(Gamma1,KF)=60.
The raw square is merely a coordinate diagnostic, not a selected norm.
A false scalar average-(30/7)Gamma1 has the same last cross-pair but is
not the source tensor. Removing either the source or the second forward
leg is separately rejected. These controls expose scalar projection loss.

For every196 constant vector one-form unit theta^a gamma_b, explicitly
reverse both CAA legs with immutable600 bilinear transpose primitives and
immutable611 Caa. Compare the complete reversed tensor against the simplified
adjoint and check its pairing with full F. Check the first and second
leg pairings individually against their forward images:392 full adjoint
rows and784 individual-leg pairings over the two points. These probes are
controls on the full transpose construction, not a claim that196 probes
span all allowed Clifford-valued variations. Completeness comes from the
literal full-space reverse composition and coefficientwise support.

## Parallelism is covariant, not constancy of components

In an orthonormal frame each Levi-Civita connection value lies in so(7,7).
A basis generator labelled a<b acts on vectors by R^a_b=sigma_b,
R^b_a=-sigma_a and on Clifford coefficients by commutator with gamma_ab/2.
Its action on covectors is contragredient:
nabla theta^j=-sum_i R^j_i theta^i.
On an exterior blade the replacement of j by i has the two increasing-order
shuffle signs. ParallelControls implements these Clifford and exterior
actions separately, without anticipating cancellation.

For each of91 independent generators, Gamma1, Gamma2 and the literal
Kdag Gamma1=-24Gamma2 have exactly cancelling complete actions.
Both separate contributions are nonzero in all273 tensor rows, so omitting
either connection part is exposed by546 nonzero planted-error checks.
The canonical soldering tensor and its wedge construction have constant
orthonormal components. Therefore cancellation for a basis of the complete
connection algebra proves parallelism for arbitrary actual connection
coefficients. This argument does NOT set the connection to zero or transform
a varying frame while dropping dE. It needs no selected frame connection
coefficients and says nothing about PT being parallel.

For each generator placed in each of14 derivative directions, explicitly
exterior-contract the full covariant derivative of Gamma1 and apply K,
and independently contract the full derivative of Kdag Gamma1 with
-sigma_axis interior_axis. All1274 directional rows have both kinetic
legs zero,2548 checks. These checks instantiate parallelism; they are not
independent nonzero validation of a general covariant differential operator.
The prerequisite transpose/derivative signs are separately bound611 evidence.
Constant s preserves this result at every point; D_B S=0 and
D_Bdag Kdag S=0 individually.

## Full action gradient and all coupling degeneracies

For the declared local action
I=Pair(S,KF_B)+Pair(S,K D_B S)/2
  +gamma Pair(S,K(S wedge S))/3+kappa Pair(S,S)/2,
the actual full gradient is
G=KF_B+(K D_B+D_Bdag Kdag)S/2
  +gamma[KQ+DQ_Sdag Kdag S]/3+kappa S.

The canonical isotropic identities are
Q=2s^2 Gamma2, KQ=312s^2 Gamma1,
Kdag S=-24s Gamma2, DQ_Sdag Kdag S=624s^2 Gamma1.
All are computed with full products and full reversed maps. There is no
projection onto a scalar action or a vector-only variation space.
In particular the cubic adjoint term is twice the forward term, not zero.
The complete actual tensors therefore have no hidden higher-grade residual,
and the full gradient is

    G=[(312gamma s^2+kappa s-21/4)I+(3/2)PT]Gamma1.

The arbitrary-real-parameter identity is verified coefficientwise. Use all
20 monomials in(s,gamma,kappa) of total degree<=3. Actual coefficients are
KF_B at(0,0,0), the full literal(KQ_unit+DQdag_unit)/3 at(2,1,0),
and Gamma1 at(1,0,1), with all others zero by exact homogeneity.
Compare every full tensor coefficient with the independent expression,
40 comparisons at the two points. This is exact symbolic coefficient
construction, not polynomial interpolation from the finite evaluation menu.

The difference between the theta1 gamma1 and theta0 gamma0 coefficients
is3/2 independently of every parameter. Thus no scalar s is stationary
for ANY real gamma,kappa, including gamma0,kappa!=0 and both0; gamma!=0
with arbitrary kappa is the remaining complete logical class. Six class
checks (three per point) confirm the same coefficient identity. These are
not a fitted root search or assertions about general field configurations.

A separate exact menu uses s=-2,-1,0,1,2; gamma0,1,2;kappa-1,0,1:
45 rows per point,90 full gradients. Each reconstructs Q,KQ,Kdag S,
DQdag Kdag S literally. Compare all196 vector entries in addition to full
tensor equality, and retain the nonzero anisotropic witness in every row.
Omitting curvature fails every row. Omitting the cubic adjoint fails exactly
the48 rows with s!=0 and gamma!=0 (2points*4s*2gamma*3kappa), with full
defect208gamma s^2 Gamma1. Cross-point equality covers all45 gradients.

## Fixed census, resources, exact closure and freeze

The FixtureJson inProgram and the contract have exact deep parity.
All50 expected count fields are frozen before execution. Dimension-derived
counts are: eight arithmetic controls;16384*31=507904 selected word products;
16384 Hodge squares;91 plane rows with1274 commutators,49 mixed metric
rejections,42 accepted same-sign controls,91 factor and91 sign rejections.
Two contexts yield392 frame metric entries,392 inverse entries,two each
orientation/control-decoy rows,76832 curvature and76832 symmetry entries,
392 Ricci and392 Einstein entries,38416 point-transport comparisons.
Actual spin rows give2548 commutators and2548 word checks; two lift type
rows and two each actual metric/factor/sign decoys.

Two source chains give16 stage comparisons,16 type,16 real-form checks,
10 oracle comparisons,two diagnostics,six source errors,392 full adjoints
and784 leg pairings. Parallelism gives91 generators,273 complete tensor
actions,546 errors,1274 direction placements,2548 separate kinetic legs.
The parameter menu gives90 rows,360 potential tensor identities,90 full
gradients,17640 individual vector entries,90 anisotropic witnesses,
90 missing-source errors and48 missing-cubic-adjoint errors.
There are40 polynomial coefficients,six coupling classes,one full-source
transport and45 full-gradient transport rows. No count is learned by a run.

Arbitrary precision rational and Gaussian-rational arithmetic has tolerance0.
Maximum matrix dimension14 and scalar curvature array38416 are structural.
The full spin curvature has at most91^2=8281 coefficients. In the canonical
chain a first-leg term can have at most two coframe choices per curvature
pair and yields grades1 or3 (14+364 per coframe); thus at most5292 first
leg entries. The inner top form has even Clifford grades0,2,4 at most1093
entries before cancellation; multiplying its actual scalar result byGamma1
has14 entries. For arbitrary unit-vector adjoint input the exterior2
support has at most91 positions with one Clifford mask each; unions of
the two legs remain below182. Canonical potential and connection actions
are smaller. A conservative16384 tracked-FT ceiling bounds all partial
accumulations as well as completed tensors. Product counters cover only
instrumented Matrix.Mul and grouped Fourier products, with ceilings100M
each; they do not count every rational or naive-word operation.

CPU90s/256MiB are estimates, with240s/768MiB planning allowances, not
measured outcomes or runtime rejection gates. Polynomial degree3 and20
coefficients are structural. There is no Fourier frequency, sampling,
fitting, iterative eigensolver, tolerance search or physical interpretation.

Thirty-seven unique exact bindings cover own four scientific files, every
compiled600/607/610/611 helper, the required passed provenance, full608
scientific pack,610 and611 programs/proofs/projects/contracts/summaries,
613 proof/contract/passed summary, primary text, complete live726 core
manifest and Directory.Build.props. The contract has a separate hash and
does not self-bind. Every ID/path/hash, full fixture, upstream pass with
known-answer/controls and false flags, and every live core path/hash/tree
must pass before science. Source anchors are required as well.

Terminal precedence: invalid-or-drifted-input,known-answer-control-failed,
frame-curvature-control-failed,typed-spin-control-failed,
literal-contraction-adjoint-control-failed,canonical-parallelism-control-failed,
full-isotropic-gradient-control-failed,resource-census-control-failed,success.
All controls and census/resource booleans must pass. Outputs are deterministic
and full/summary byte-identical. Every new file has exactly one final newline.
Failed first outputs are preserved; repairs require versioning and renewed
review. Only the unbound implementation note may receive approved results.
All14 authority flags remain false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0; O4 remains pending and Phase561 closed.
