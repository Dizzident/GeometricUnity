# Phase592: companion tensor chirality audit

Amendment A49. Prospective study at
`studies/phase592_companion_tensor_chirality_audit_001`.

The implementation compares the literal signed14D Hodge/Clifford chain on
the full3185 algebraic Riemann basis to independently derived formal
companion-parameter formulas in four tied branches. Six monomial slots give
76440 exact coefficient comparisons. Polynomial elimination tests universal
CCA matching, both chiral solutions, invertible and null-wrong-partner decoys.
Diagnostic norms and28 chirality/mixed-pairing controls preserve the distinction
between self-nullity and the source's unspecified mixed torsion pairing.

## Frozen execution

Coordinator and independent reviewer approved the full prospective pack
before its first scientific Release run. Build passed with zero warnings/errors.
The first approved run exited0 in1.04 seconds with terminal
`companion-tensor-chirality-controls-pass-source-choice-open`.

Contract SHA256:
`ffa3f692d45dd763ccecf7778d0e0f28164ec95209efd1b60258b9f67527caf6`.
Program SHA256:
`c2be95aafa4aeabb794615f4350539c1667783de00f54523a933fa30eea40987`.
ExactAlgebra helper SHA256:
`cf139f4cc6226ef5e2c7f830ee413a50c9e30d483411c77f87a631063ff3b8f5`.
STUDY SHA256:
`722dc3a89cd5331ed9005723beebae5705a55375406b473d8be408d76cd17e3c`.
Identical full/summary output SHA256:
`47fac92eaac8baccdc9e17c559bc5ddbfd97ed975f26844ed2a035e5f35e89be`.

All12 exact bindings have unique IDs/paths and match. The original/helper
structural parity check passes, as does the full live726-file core manifest.
The output has knownAnswerPassed=true and controlsPassed=true. All44944
independent Clifford-word checks and16384 Hodge-mask checks pass exactly;
Omega squared is+1 and top-star is-1. The3185 basis rows have distinct pivots,
pair symmetry and zero Bianchi residual. All12740 tied branch cases and76440
formal coefficient cases match the independently derived oracle. All five
geometric anchors pass, including nonzero Weyl curvature annihilation.

## Bounded findings

With P=a+b Omega, the executed formal identities on algebraic Riemann input
are CCC=-PJ, CCA=-PJ-(dR/2)(b+a Omega)G, AAC=0 and AAA=(cR/2)PG.
The polynomial residual and elimination/sufficiency certificates pass exactly.
Universal CCA matching to -P(J-RG/2) has nontrivial solutions precisely
a!=0,d=+/-1,b=-da,c arbitrary; a=b=0 is a separate trivial solution with
arbitrary c,d. Both chiral fixtures match; both invertible fixtures and the
null-but-wrong-d fixture fail matching as predicted. An invertible canonical
row does match the fixed scalar-flat, nonzero-Ricci input, confirming the
need for the universal-curvature quantifier in the parameter restriction.

The declared norm polynomial is14(-a^2+b^2), with basis norms-14,+14 and
zero cross terms. All28 chirality cases confirm input S_d to output S_-d;
all28 incorrect same-chirality input assertions are detected. Same-chiral
odd products vanish while opposite traces equal64 sigma_a and are nonzero.
Omega's negative H-adjoint sign and chiral H-isotropy also pass exactly.

These controls do not turn null self-pairing into a zero mixed source action,
source inadmissibility or a physical chirality projection. Eq9.4's torsion
sector and pairing remain unspecified here. The tested invariant subfamily
is not claimed exhaustive, and tying bracket occurrences is not attributed
to author intent. No source choice, action equivalence or mass claim follows.
All14 authority flags remain false, externalReviewPending=true and promoted
physical mass claim count0; O4 pending and Phase561 closed. Frozen scientific
files and outputs were not edited after execution; only this unbound result
record was updated.
