# Phase584: signed spatial curvature consistency

Prospective deterministic Amendment A46 control. The program and contract are
frozen before execution. This study changes no registered operator or historical
result. It evaluates two declared dictionaries through the actual
`CurvatureAssembler`, using `MeshTopologyBuilder` and exact straight-edge
integrals of smooth affine connections. Each spatial mesh is one triangle in
two dimensions; it is not the full sampled four-dimensional action. The
separate fixed-mesh decoy uses the original four-dimensional Phase565 mesh.
Neither dictionary is author-selected.

## General bracket identity and the two limits

For an oriented triangle with stored face tuple (v0,v1,v2), write the registered
boundary array as x=e01, y=-e02, z=e12, and S=x+y+z. The composable path order
is x,z,y. Define the quadratic terms

    Qreg  = ([x,y]+[x,z]+[y,z])/2,
    Qloop = ([x,z]+[x,y]+[z,y])/2.

Antisymmetry cancels the last brackets in the sum, leaving

    Qreg+Qloop = [x,y]+[x,z] = [x,S],
    Kloop^(2)(A) - [-Kreg(-A)] = [x,S].

This holds in any Lie algebra by bilinearity and antisymmetry, without a
commutativity assumption or a special choice of triangle. The finite exact
integer-valued cross-product battery checks all 19,683 ordered triples of
vectors in {-1,0,1}^3; it supplements the proof rather than establishing its
generality. The core SU(2) bracket is independently compared with cross product
on all 729 pairs from the same menu.

For a fixed smooth connection on shrinking, shape-regular triangles, each
edge integral is O(a), while Stokes' theorem gives S=O(a^2). Consequently
[x,S]=O(a^3), or O(a) after dividing by signed area. Higher BCH terms and the
difference between exp(edge integral) and path-ordered smooth transport do not
affect this leading curvature statement; this study does not identify these
finite links with exact noncommuting parallel transport.

At fixed geometry under A->tA, however, x->tx and S->tS, so [x,S]->t^2[x,S].
The paired dictionary therefore need not repair the quadratic weak-field
obstruction. Independently sampled rough edge coefficients need not satisfy
smooth Stokes scaling. Neither a quantum limit nor pole convergence follows.

## Signed dictionaries and analytic affine oracle

The same dictionary inputs omega=A and reads F=Kreg(A). The paired dictionary
inputs omega=-A and reads F=-Kreg(-A). Both operations are required: negating
the input alone reverses the derivative term. Negation is not a Lie-algebra
automorphism, since [-x,-y]=[x,y], whereas -[x,y] has the opposite sign.
Under the paired dictionary, physical links would use exp(-omega), and the
inhomogeneous connection transformation must be translated consistently.
No gauge covariance claim for the finite registered operator is made here.

Let A1=X+xP+yQ and A2=Y+xR+yU. For any straight edge from u to v the affine
line integral equals the midpoint value contracted with v-u, exactly.
On (0,a e1,a(e1+e2)) this gives

    x = a X + a^2 P/2,
    y = -a(X+Y) - a^2(P+Q+R+U)/2,
    z = a Y + a^2 R + a^2 U/2,
    S = a^2(R-Q)/2.

The quadratic constant-field term is Qreg=-a^2[X,Y]/2. Therefore

    -Kreg(-A) = a^2(R-Q+[X,Y])/2 + O(a^3),
     Kreg( A) = a^2(R-Q-[X,Y])/2 + O(a^3).

The continuum target F12(0) is R-Q+[X,Y]. The same-sign leading value has the
wrong bracket sign for noncommuting fields. For arbitrary vertex permutations,
the area carries orientation. With oriented edges e(a)=a L+a^2 D and unit
signed area A0, the program independently expands Qreg into a^2 Q2+a^3 Q3+
a^4 Q4 using cross products. It checks both exact normalized polynomials:

    paired/area = F12(0) - a Q3/A0 - a^2 Q4/A0,
    same/area   = (R-Q-[X,Y]) + a Q3/A0 + a^2 Q4/A0.

It also checks the bound error <= a||Q3/A0||+a^2||Q4/A0||, Stokes' identity,
the assembler-level bracket identity, topology signs and the analytic leading
coefficient for every row. Permutations change finite errors and basepoints;
the claim is a shared signed leading limit, not exact finite-mesh permutation
invariance. Vertex permutations are implemented by assigning permuted physical
coordinates to global vertex IDs before building each actual mesh.

## Frozen fixtures and decisions

Four profiles: constant commuting, constant noncommuting, the affine witness
A1=Jx, A2=Jy+xJz, and a fully populated affine noncommuting profile. The exact
vectors are in the contract and structurally matched to a program literal.
For each, use all six vertex permutations and a=1/4 through 1/256 by dyadic
halving: 168 spatial rows and 336 assembly calls across both dictionaries.
The constant noncommuting paired result is exact, while the same-sign target
error is exactly 2. In the affine witness the target is 2Jz; the same-sign
error remains at least 1.9. Every nonzero refinement sequence must reduce its
endpoint error to at most 0.03 of its starting value. The exact polynomial
oracle and explicit remainder bound are the primary consistency controls.
Exact algebra tolerances are zero; floating control tolerance is 2e-12.

The separate weak-field control uses the original Phase565
CreateUniform4D(1) wave (scale 0.031, frequency 0.419, cosine weight 0.37 and
frequency multiplier 1.7) and all nine original amplitudes 1 through 1/256.
It compares the actual assembler with principal logarithms of quaternion
products in composable and registered-array order. The first six amplitudes
must give slopes within 0.2 of 2 for the same and paired defects, and within
0.2 of 3 for the original array-order remainder. At t=1/64 each quadratic
coefficient must agree within relative error 0.003 with its signed prediction:

    same:   Qloop-Qreg = [z,y],
    paired: Qloop+Qreg = [x,S].

Both coefficients must have norm greater than 1e-8. Phase565's exact-bound
negative terminal is required upstream. This is a reproduction and scoped
extension of its deterministic decoy, not a rerun or reinterpretation of old
sample outputs. On the smooth witness at fixed a=1/4, [x,S]=-a^3 Jy/2 is
also checked exactly, including t^2 scaling over the same amplitude ladder.

Terminal precedence is invalid/drifted contract or input, known-answer failure,
mathematical control failure, then
`paired-sign-smooth-spatial-consistency-fixed-mesh-obstruction-preserved`.
No failures are overwritten by a scientific repair; any changed scientific
menu or threshold requires a separately versioned successor. The estimated
10 CPU seconds / 80 MB fit the frozen 60-second / 400-MB estimate ceilings;
these are prospective estimates, not timing measurements or allocation limits.
No RNG, adaptive fitting, sampling or production is used.

## Conditional bridge and boundaries

Phase583 permits the c=s=-1, r=+1 family. On epsilon=I and a globally trivial
flat reference A0=0, the candidate omega=p=-A is compatible with the favorable
paired sign map. This is a conditional mathematical lead, not a derivation of
the complete registered action. A nonzero reference, the source's first-order
contribution, residual carrier maps, pairing and measure remain separate
requirements. An overall residual sign drops out of a squared norm, but the
entire variable/derivative dictionary must still be propagated. Phase585 and
Phase586 independently investigate adjacent requirements; they are not inputs
to this frozen study.

All 14 Phase583 authority firewalls stay false. Phase561 remains closed,
external review remains pending, no source-action identification or physical
particle interpretation follows, and promotedPhysicalMassClaimCount=0.
