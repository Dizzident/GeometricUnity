# Phase593: complete-branch first variation

Prospective A49 extension. No scientific execution precedes frozen code,
project, proof, fixture menu and exact bindings, independent review and explicit
coordinator approval. An expected negative variational finding is a successful
audit only if every required positive and negative control passes.

## Typed action and conventions

Use constant forms on the oriented flat unit-volume fourteen-torus of signature
(7,7), with positive axes0 through6 and negative axes7 through13. This removes
boundary and derivative terms; it is a conditional local algebraic test, not a
physical spacetime construction. Fix the metric, epsilon=I and background B=0.
The connection T takes values in the source's admitted u(64,64). Gamma blades
of grades1,2,14 used here are real H-anti-Hermitian, with H=gamma7...gamma13.
No physical projection or restriction to the Levi-Civita curvature subspace is
made. Omega=gamma0...gamma13 has Omega squared I.

Freeze the Phase592 conventions

    Phi1=(a+b Omega) sum_j theta^j gamma_j,
    Phi2=(c+i d Omega) sum_(j<k) theta^jk gamma_j gamma_k,
    C(X,Y)=XY-YX, A(X,Y)=i(XY+YX).

The complete CCA operator K maps two-forms to thirteen-forms:

    K(F)=[Phi1 wedge star(F)]_C
         -(1/2) star([Phi1 wedge star([Phi2 wedge star(F)]_A)]_C).

Its lowered output L=star1_inverse K is a one-form. The intermediate degrees
are 2->12->13 and 2->12->14->0->1->13. The source's outer star and coefficient
one-half are retained. All calculations apply this literal chain, including
the second term; the predicted component formulas are independent checks.

The declared real bilinear trace pairing is B=-ReTr/128 with the signed form
metric. Equivalently pair a one-form U with K(F) by the top-form coefficient
of -ReTr(U wedge K(F))/128. This is a diagnostic convention, not a source
normalization or positive kinetic metric. Multiplying this common pairing by
a nonzero scalar does not repair any mismatch below.

Compute Q(T)=T wedge T using the associative Clifford/exterior product. Then
F(T)=Q(T) on the constant flat fixture. Two readings of the source's cubic
symbol are frozen: [T,T]_gamma=gamma Q(T), gamma=1 or2. Gamma2 is the usual
graded-bracket reading, for which F=(1/2)[T,T]_2. The cubic action tested is

    I_gamma(T)=(gamma/3) B(T,L(Q(T))),
    candidate force in V = B(V,L(Q(T))).

This isolates Eq9.4's cubic coefficient against Eq9.7's curvature force. It
does not silently change the geometric curvature when changing the printed
bracket reading. B=0, dT=0 and fixed epsilon make the lower-degree curvature
and derivative terms vanish. A separately tested quadratic mass term has zero
directional derivative at z=0 for both witnesses, and cannot cancel their
homogeneous cubic discrepancy.

## Two independently derived witnesses

All axes mentioned here are positive. Write Gamma_ab=gamma_a gamma_b.

First witness:

    T= x theta^0 Gamma01 + y theta^1 Gamma12 + z theta^1 gamma2,
    V=partial_z T=theta^1 gamma2.

Because [Gamma01,Gamma12]=2Gamma02 and [Gamma01,gamma2]=0,
Q(T)=2xy theta^01 Gamma02 is independent of z. The inner A contraction
vanishes coefficientwise for both c Gamma01 and i d Omega Gamma01, since
{Gamma01,Gamma02}=0 and Omega commutes with bivectors. The complete lowered
operator is L1=-4xy(a+b Omega)gamma2, with all other components zero.
Thus the normalized force pairing is 4a xy, while the literal cubic action
is (4a gamma/3)xyz and its derivative is (4a gamma/3)xy.

Test the canonical row (a,b,d)=(1,0,0), and both matching chiral rows
(1,h,-h), h=-1,+1. Retain c formally by separate c^0 and c^1 coefficient
slots; no finite c scan is substituted for arbitrary c. This first witness
fails the asserted equality for both gamma choices on all three rows.

Second witness, only on the two matching chiral rows:

    T=x theta^0 Gamma02 + y theta^1 gamma1 + z theta^2 Omega,
    V=theta^2 Omega.

Here Q(T)=-2yz theta^12 Omega gamma1, because the other two coefficient
commutators vanish. At z=0 the curvature and candidate force are exactly zero.
For Z=Omega gamma1 on plane12, the inner lowered A contraction is
2h gamma2; its c coefficient is zero. The full chain gives L0=-2h Gamma02
for this unit Z and therefore L0(Q)=4h yz Gamma02. More generally
L_j(Q)=4h yz gamma_j gamma2 for j not in {1,2}; components1,2 vanish.
The gamma_j gamma2 expression retains its Clifford order, including j>2.
Only the j=0 component pairs with T, giving I_gamma=(4h gamma/3)xyz.
Its z derivative is nonzero at x=y=1, despite the zero force at z=0.

These two witnesses rule out a single global cubic reweighting as a repair:
writing I_alpha=alpha B(T,L(Q)), the first requires alpha=1 and the second
requires alpha=0. This is scoped to the full tested chiral CCA family, its
allowed torsion directions and the declared shared pairing. It is not a
no-go for all source operators, constrained torsion spaces, additional terms
or different action families. The source's lost final operator is not selected.

## Independent polynomial construction and controls

Represent tensors with sparse exact complex rational coefficients carrying
three formal exponents x,y,z. Construct Q by associative wedge multiplication,
apply the literal Hodge chain to each monomial, multiply T wedge K(Q), take
its top trace, and differentiate that resulting polynomial coefficient by
coefficient. Neither the predicted polynomial nor the candidate force is used
to generate the action derivative. Separately obtain the candidate force
through signed one-form pairing with V, and verify equality with direct
top-wedge trace pairing. Check all predicted full tensor coefficients, not
only the final scalar discrepancy. Curvature and its z derivative receive
independent expected-tensor checks.

Five operator/witness combinations times two c slots times two gamma readings
give20 polynomial rows. Their ten c^0 rows have the predicted nonzero mismatch;
the ten c^1 rows vanish identically. Evaluate each row at the fixed amplitude
menu (x,y)=(1,1),(2,1),(-1,2),(0,1),(1,0), always z=0:100 exact amplitude
checks, including40 x/y zero controls. Full polynomial identities, not these
point evaluations, establish all-amplitude claims.

For each of the five combinations test the independent quadratic control
I_mass=(kappa/2)B(T,T) at kappa=-1,0,1. Its derivative equals kappa B(V,T)
=-kappa z, giving15 exact positive comparisons; at z=0 all are zero.

The genuine Chern-Simons control uses three active positive coordinates and
the compact bivectors Gamma01,Gamma12,Gamma02:

    A=x theta0 Gamma01+y theta1 Gamma12+z theta2 Gamma02,
    K_CS(F)=F wedge nu11, nu11=theta3 wedge ... wedge theta13.

It is the ordinary constant three-dimensional action embedded using a fixed
closed eleven-form, not an identification with the source K. Independent
trace cyclicity gives -Tr(A cubed)/128=6xyz; the correctly normalized action
has derivative2xy, equal to -Tr(V wedge Q(A) wedge nu11)/128. For each gamma
encoding use coefficient1/(3gamma) on [A,A]_gamma, producing the same action
and the same positive equality. Also record the literal1/3 coefficient:
its derivative is 2gamma xy, agreeing only at gamma1. This isolates the
ordinary bracket-normalization effect from the full-branch noncyclic defect.

Known answers compare independent ordered-word multiplication against blade
mask multiplication for the16 blades generated by axes0,1,2 and Omega:
256 pairs. Check all16384 Hodge squares, rational arithmetic, i squared -1,
Omega squared I and H-anti-Hermitian torsion coefficients. Tolerance is zero.

## Freeze, bindings and terminal precedence

The project compiles the exact upstream Phase592 ExactAlgebra.cs through an
explicit Compile Include. It is a complete, hash-bound build input, not an
untracked copied helper. Ten unique binding IDs/paths cover own program,
project and this proof; primary source; Phase592 summary, contract, program
and helper; its complete core source manifest; and Directory.Build.props.
The live726 src C#/project files must exactly match the bound manifest.
The upstream passed terminal and safety flags are checked before calculations.

Freeze the complete fixture JSON in the contract and compare it structurally
against the executable before any scientific calculation. Estimated resources:
5 CPU seconds and64MiB, prospective ceilings15 CPU seconds and128MiB.
These are estimates, not runtime measurements. There is no sampling or large
census. Full and summary outputs are deterministic and byte-identical.

Terminal precedence: invalid-or-drifted-input; known-answer-control-failed;
literal-chain-control-failed; independent-variation-control-failed;
positive-control-failed; then
`companion-action-variation-mismatch-certified-source-choice-open`.
Only the final terminal has auditPassed=true. Preserve every failed frozen
output; scientific repairs require a versioned successor.

All fourteen authority firewalls remain false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0. No core or historical artifact changes,
source-action selection, dimensional reduction, sampling or physical claim.
