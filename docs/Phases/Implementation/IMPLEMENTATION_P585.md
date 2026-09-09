# Phase585 implementation: quotient section and measure controls

Prospective A46 deterministic study in
`studies/phase585_quotient_section_measure_controls_001/`.
The [study and proof](../../../studies/phase585_quotient_section_measure_controls_001/STUDY.md)
specify the conditional section h=epsilon^-1, the right/background/left
transformation laws, the finite triangular Jacobian and its domain premises.

The standalone executable freezes seven unique input bindings, all fixtures,
zero exact tolerance and a 1e-8 numerical determinant tolerance before its first
scientific run. It checks 3,456 exact first-jet cases, 576 compact-toy cocycle
cases and 48 full Jacobians. Independent determinant algorithms and central
differences challenge the analytic triangular formula. Retained-space projection,
stale cutoff domain and an independent-site finite-difference product rule are
explicit failing decoys; their prescribed failure is a passing control.

The compact coboundary toy contains no spatial derivative. Full first jets
have noncompact derivative fibers, so normalized compact Haar cannot be assigned
to them. The compact finite affine-cocycle limitation does not exclude link
regulators or discretization after selecting the section. Existing Phase548/577
targets already freeze theta=0; off-slice dependence does not reject that slice.

First frozen Release execution (2026-09-08), after coordinator review, returned:
`conditional-section-measure-controls-pass-source-regulator-unresolved`.
The standalone process completed successfully in 0.30 seconds. All seven unique
bindings, fixtures, known answers and mathematical controls passed. Section,
inverse derivative, right/left/background laws and cocycle residuals were zero
in all 3,456 jet cases. The 576 compact-toy cases also had zero residual.
All 48 Jacobian cases had exact determinant c in both signs; the largest
central-difference determinant error was 1.411e-11 and largest entry error was
4.697e-11, below the frozen 1e-8 tolerances. The projected Jacobian was zero,
the stale-domain point was (1,-2,0), and the graph product-rule/cocycle defects
were both exactly 2, as prescribed. The shifted product rule passed exactly.

Full and summary output SHA-256 are both
`0538c3e58949fd6d575b74505e198723e9d7ed092a238542f70790bd92a7ca08`.
The frozen contract SHA-256 is
`d8094acb1655fd268648d45092648c28ffdb3e21165ef94c1a0ec001d1a735c4`.
The registered measure remains unselected, action descent is unresolved,
all fourteen authority firewalls stay false, external review stays pending,
and promotedPhysicalMassClaimCount=0. No sampling or core edits.
