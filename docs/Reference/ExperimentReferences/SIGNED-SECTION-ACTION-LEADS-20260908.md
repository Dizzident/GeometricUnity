# Signed curvature, quotient measure and action-pairing leads

2026-09-08, Amendment A46 / Phases584-586. This note promotes the previous
temporary lead inventory to a durable research record, not to physics evidence.
The user explicitly requested parallel implementation of concrete next tests.

Primary source: Eric Weinstein, *Geometric Unity: Author's Working Draft v1.0*,
2021-04-01, [author-hosted PDF](https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf).
Local text `texts/GU-DRAFT-2021-TEXT.txt` has SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Section6 supplies the compared sign/quotient premises; section9 supplies
curvature-plus-torsion residuals and variations; section12.4 explicitly discusses
removing the first-order quadratic potential. It does not select that subcase
as the unique theory or provide the registered lattice dictionary.

## Signed spatial consistency

For oriented edge integrals x=e01, y=-e02, z=e12, put S=x+y+z. The registered
quadratic sum Q_reg and composable-loop sum Q_loop satisfy
Q_reg+Q_loop=[x,S]. Consequently
K_loop^(2)(A)-[-K_reg(-A)]=[x,S]. For smooth fields on shrinking triangles,
x=O(a), S=O(a^2), giving O(a^3) unnormalized discrepancy. At fixed mesh under
field scaling t, the same defect is generically O(t^2). These different limits
must not be equated. Phase565's negative remains an exact statement about its
frozen same-sign, fixed-mesh comparison.

Phase584 tests both dictionaries through the actual assembler, using exact
smooth edge integrals, vertex permutations and amplitude-scaling controls.
Its classical limit is not convergence of a rough-field quantum measure.
The favorable interpretation fits Phase583's c=s=-1,r=+1 family on the
section epsilon=I and globally trivial A0=0: A=Phi=-p, with omega=p=-Phi.
Neither the global reference assumption nor this field identification is
source-selected. Nonzero A0 adds F0 and d0 terms that cannot be ignored.

## Quotient and measure

The compatible continuum identities give Phi=c Ad(epsilon)p-(d0 epsilon)epsilon^-1.
The right tilted action has a unique epsilon=I section if epsilon^-1 is allowed
by the gauge group and boundary conditions. A finite affine coefficient map
(p,epsilon)->(Phi,epsilon) has unit absolute Jacobian when local compact adjoint
conjugation preserves the retained vector space. This conditionally supports
Lebesgue coefficients; it does not make exp(omega) Haar-distributed.

The derivative cocycle, retained-space closure, cutoff domains and gauge volume
are separate conditions. First-jet gauge groups contain noncompact derivative
fibers. A normalized compact Haar argument cannot be silently applied to them.
Finite differences need not obey Leibniz. Phase585 explicitly distinguishes a
compact algebraic toy cocycle, local first-jet identities, and failures of a
naive graph derivative. A complete registered regulator remains a separate task.

The right tilted redundancy, background-frame covariance and left seed action
are distinct. A connection-like left action can survive taking the quotient,
but its being a dynamical symmetry is not automatic. Phase548/577 already freeze
theta=0; off-slice theta dependence of the broader operator alone does not
reject a conditional quotient interpretation of those sampled targets.

## Action, torsion and omitted maps

TrivialTorsionCpu removes a residual contribution and its derivative for all
inputs; it does not constrain A=B. In a compatible section, the schematic
source residual is K_I(F0+d0 Phi+Phi wedge Phi)+kappa Phi, with appropriate
carrier identifications still required. Phase586 uses declared positive compact
controls to distinguish kappa=0 from Phi=0, and tests reference-dependent
curvature and variations. Its toy carrier is not a newly selected source map.

For a field-independent omitted linear map L and y=K F, the source residual
pairing becomes W=L^T M_source L. Linearity eliminates derivatives of L, not
its effect on the action or Hessian. Spectral equivalence requires matching the
pulled-back quadratic forms and, under field redefinitions, the kinetic metric.
Even exact polynomial degree can drop if L annihilates the highest component.
The implementation documented its degree-raising omission as a limited-scope
realization, not a proof of spectral equivalence. Phase586 tests these limits
with known-answer isometric/nonisometric and annihilation controls.

## Follow-up source correction and concrete next experiments

These are reviewed derivations/proposals, not additional executed phases.
Registry587+ remains free. Freeze a successor before testing the predictions;
do not modify Phase584-586 or their exact-bound core sources.

### Recover the literal operator before inventing a substitute

The coordinator and action worker visually inspected original PDF page43,
Eq.9.3. The PDF SHA256 is
`3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4`.
Its second term has an outer Hodge star in the numerator of `*/2`. The older
`docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md` section4 skeleton and its
"draft-unpinned" coefficient description are not a faithful literal record:
the displayed example does contain coefficient1/2 and that outer star.
Record the correction here; retain historical/frozen artifacts unchanged.

Let E_r be the displayed bracket/wedge map using epsilon^-1 Phi^r epsilon,
and star_k map k-forms to (d-k)-forms. Composition is right to left:

    S_e = E_1 star_2 - (1/2) star_1 E_1 star_d E_2 star_2.

The two degree chains are 2 -> d-2 -> d-1 and
2 -> d-2 -> d -> 0 -> 1 -> d-1. Thus the typed expression is recoverable;
the source's d=14 is not silently replaced by the reduced implementation's
d=4. Lowering with the actual inverse star_1 gives a common one-form carrier:

    K_e = star_1^-1 E_1 star_2 - (1/2) E_1 star_d E_2 star_2,
    U_lower = K_e F(A) + kappa T.

The source tensors Phi^r here are not the quotient coordinate Phi above.
Their invariant components, representation, normalization, reduced-field map
and source pairing are separate obligations. The printed example is also not
the author's lost final choice mentioned in the same page's footnote. This
correction makes a literal typed reconstruction possible; it does not establish
that the registered two-form residual is that reconstruction. At fixed metric,
retain (delta K_e)F as well as K_e d_A(delta A) and kappa delta T. Metric
variations additionally act on Hodge maps and the pairing.

### First priority: a parameter-free residual-factorization test

On one standard oriented4-simplex, the code's geometric reconstruction is
Q=(WW^T)^-1 W, P=W^T Q, and its theta=0 face contraction is
M=(I-P)+W^T R Q. There are ten faces but only six represented constant
two-form components. The candidate exact test must include the favorable
flat-tangent identity PD=D, where D is edge-to-face incidence: dimension
counting alone cannot reject that linearized sector.

For each face ijk, independently polarize the actual curvature with Jx on
edge ij and Jy on edge ik. The predicted mixed coefficient is -Jz/2 only
on that face. Thirty face/Lie fixtures should span the full face carrier;
same-generator pairs must vanish. Freeze the witness
n=e123-e023+e013-e012, for which Qn=0, Mn=n and ||n||^2=4 are predicted.
If confirmed, polynomial-coefficient equality rejects every field-independent
linear N satisfying M F(A)=N QF(A) for all retained A. The projected control
MP=W^T R Q must instead factor and annihilate n.

This would reject precisely one constant-form residual dictionary. It would
not exclude higher-order reconstructions, smooth limits, or all scalar-action
equivalences: polynomial Gram representations need not be unique. Use exact
rational controls, actual floating-point operator comparisons, fixed fixtures
and no chosen source Phi^r. No spectrum or production run is needed.

### Parallel priority: fixed-domain action and directional force

`CpuMassMatrix.cs` uses unit default face weights, not an assembled Whitney
mass. `CreateUniform4D(n)` covers [0,n]^4; a refinement of [0,1]^4 requires
rebuilding with coordinates divided by n. Homothety of one complex is only
a shrinking-domain scaling control. Proposed meshes n=1,2,4 test constant
abelian curvature using all six basis two-forms and their15 pair sums, with
both identity contraction and registered R=P_+/2. The proposed independent
open-grid face census gives50n^4+48n^3+12n^2 faces.

In basis(01,02,03,12,13,23), let B's ij column be e_j-e_i and h=1/n.
The analytic conjectures to verify against actual topology and operators are

    G_n = (3/2+2h+h^2/2)I + (3/2+h/2)B^T B,
    S_n = (1/2) sum_Lie F^T R^T G_n R F,
    P_+ G_n P_+ = (9/2+3h+h^2/2)P_+.

The full limiting metric is anisotropic, but its pure self-dual restriction
may be a scalar. Do not reject that sector from the full metric alone. For
omega=-integral A, compare -GradOmega dot integral V with the independently
differentiated action. A raw coefficient-gradient norm is not a continuum
force norm or physical kinetic metric. Add affine noncommuting fields with
exact triangle-polynomial integration and a proved remainder bound; a fitted
three-point slope is insufficient. Freeze resource bounds and avoid tiny
homotheties that hit Lambda2Algebra's absolute inversion pivot floor.

### Parallel priority: section density versus literal joint descent

Selecting the continuum section before discretizing need not leave an exact
finite right action on the retained section variables. Claiming that the
existing joint(omega,theta) model gauge-fixes to the sampled theta=0 model
does require that action and invariant measure. Test these as different claims.
For a declared map Phi=L omega+a, the actual finite-density requirement is

    S_reg(omega,0) = beta S_sec(Phi) - log rho_sec(Phi)
                    - log J_L - log D_FP + constant.

Unit coordinate Jacobian or a local FP determinant of one does not choose
rho_sec. A compact one-site control with S_sec=|Phi|^2/2 and invariant weights
1 versus exp(-|Phi|^2/2) has identical Jacobians but Gaussian radial moments
3 versus3/2. This gives a parameter-free measure-nonselection test. A separate
finite necessary Ward test can compare actual joint gradients against exact
smooth periodic edge-integrated tilted tangents, including nonconstant
noncommuting p and both sign families. Avoid only testing uniform fields
against a Fourier mode: symmetry can make that weak diagnostic vanish.
A failed literal joint lift does not reject section-before-discretization.

Boundary conditions, global group/cover and coordinate charts, reference
holonomy, residual left symmetries, normalizability and physical kinetic
pairing remain distinct premises. A zero-action ray alone does not prove a
divergent integral; transverse integration matters. None of these tasks needs
an external signature to proceed, and none allows source inputs to be fitted
after looking at the saved sampler output.

## Executed results and claim boundary

All three first frozen Release runs passed. Phase584's 19,683 exact bracket
identities and 168 actual-assembler spatial rows have zero identity/polynomial
residual. The affine normalized error decreases from 0.1767766952966369 to
0.0027621358640099515. Fixed-mesh same/paired amplitude slopes remain
2.000016126988648 / 2.000012839777975. No old obstruction was overwritten.

Phase585's 3,456 first-jet and 576 compact-toy cocycle cases have zero exact
residual. Forty-eight compact full Jacobians have determinant c, with numerical
determinant error below 1.411e-11. The projected determinant vanishes, and the
naive graph product/cocycle defects are 2. These prescribed failures delimit
the unit-Jacobian argument rather than rejecting all regulators.

Phase586's actual commuting flat-connection objective and both gradients
vanish even for nonzero coefficients. Its nonzero-reference identity is exact;
omitting that reference produces defect 4.58257569495584. The maximum variation
error is 4.66e-9. Declared nonisometric pairing controls change spectra (1,1)
to (1,4) and a nonzero-residual Hessian 7 to 25; consistent kinetic metrics
restore the generalized spectrum. None supplies the missing source map.

The studies and `IMPLEMENTATION_P584.md` through `IMPLEMENTATION_P586.md` record
the frozen menus, exact bindings, actual results and any preserved failed run.
Neither an external signature nor internal consistency supplies missing source
content. No old negative is overwritten, no source action/measure is selected,
Phase561 stays closed, O4 stays pending, and no mass, sampling or production
authority follows. Every phase retains promotedPhysicalMassClaimCount=0.
