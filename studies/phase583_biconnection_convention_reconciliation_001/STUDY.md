# Phase583: bi-connection convention reconciliation

Prospective deterministic A45 audit. This is an internal exact calculation,
not independent-model adjudication or selection of the author's intended signs.

## Frozen mathematics

Hold fixed d0 h=dh+A0 h-h A0, C_h=h^-1 d0 h, the group product
(e,p)(h,q)=(eh,h^-1 p h+q), the right connection action
A dot h=h^-1 A h+h^-1 dh, and B=A0 dot e. Let

    tau_s(h)=(h,s C_h),   A_c=A0+c p,
    pi_r(e,p)=e p e^-1+r(d0 e)e^-1,   c,s,r in {-1,+1}.

The ordinary derivative product rule gives
C_eh=h^-1 C_e h+C_h. Consequently both tau signs are homomorphisms.
Right multiplication gives p'=h^-1 p h+s C_h and e'=eh. Direct subtraction:

    A_c' - A_c dot h = (c*s-1) C_h,
    B' - B dot h = 0,
    pi_r(e',p') - pi_r(e,p) = (s+r) e h C_h h^-1 e^-1.

These are identities for arbitrary smooth matrix-group first jets. Since
C_h can be nonzero, simultaneous common-connection covariance and quotient
invariance require c*s=1 and s+r=0. Exactly two of the eight triples survive:

| c | s | r | Keeps original right-action stabilizer? |
|---|---|---|---|
| -1 | -1 | +1 | Yes |
| +1 | +1 | -1 | No; the stabilizer remains a separate tau_- |

For either survivor, T=A-B=c p-C_e and

    c*pi_r(e,p) = e T e^-1.

Thus the invariant quotient coordinate is the reference-frame representative
of the relative field, with a convention-dependent overall sign. Under
(p,c,s,r)->(-p,-c,-s,-r), A,B,T and c*pi are identical. The coefficient sign
change has |det|=1 in any finite coefficient dimension; this is not a statement
about gauge fixing, the full measure, an action, or a continuum functional
determinant.

The printed triple (+1,-1,+1) correctly preserves pi and stabilizes A0 under
the original inhomogeneous right action, but fails bi-connection covariance.
Take A0=p=0, e=I and h(x)=exp(x*C), at x=0. Then A=B=0 initially;
the printed rules give A'=-C, B'=C, T'=-2C. C with Lie vector (1,2,-1)
has |T'|^2=24. This is a decisive counterexample to simultaneous printed
definitions, not a falsification of every consistent version of GU. Constant
h would hide the defect; no noncommutative effects are needed for this witness.

## Independent execution controls

Before source/upstream reads, validate exact generator commutators, the 24
proper signed-permutation rotation matrices, inverses, uniqueness and closure.
Use a nonzero A0, 24 epsilon rotations, 24 h rotations and three nonzero Lie
derivative fixtures in every sign case: 13,824 cases in checked Int64 arithmetic.
Build ordinary derivatives from prescribed tangent jets and compose by the
product rule independently of the claimed cocycle. Check every residual above,
the compatible dictionary, and simultaneous sign relabeling. Zero tolerance;
no random seeds, retained chains, parameter fits, or hidden scan selection.

## Source and remaining bridge

Primary draft Eqs. 6.2, 6.4, 6.7-6.13, 6.18-6.22 and 12.6 supply the compared
premises. The draft's own footnote 7 warns of conflicting conventions. The
PDF signs were visually checked in Phase582; the text is exact-hash bound.
See the [source/proof note](../../docs/Reference/ExperimentReferences/BICONNECTION-CONVENTIONS-20260908.md).
The frozen menu does not audit every later formula (e.g. every line of 6.16),
choose a repaired full manuscript, or infer author intent from the word
"flipped" used for the stabilizer.

Next derive the action in (B,T): A=B+T, so
F(A)=F(B)+d_B T+T wedge T. Compare the source's curvature-plus-T expression
with the registered curvature-only trivial-torsion objective. Dropping a T
contribution is not the constraint T=0, and neither identifies omega with T.
Track reference/epsilon dependence, variations, and measure before proposing
correlators. Phase561 stays closed, external review remains pending, no old
terminal changes, and promotedPhysicalMassClaimCount=0.
