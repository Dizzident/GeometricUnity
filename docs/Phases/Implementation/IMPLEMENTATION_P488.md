# Implementation P488 — Haar proposal invariance control

Phase488 implements Amendment A6's prospectively frozen exploration of four
independent SO(3) Haar proposal/sampling constructions. The executable is
`studies/phase488_haar_proposal_invariance_control_001/Program.cs` and targets
`.NET 10` with warnings treated as errors.

The four paths are Shoemake quaternion independence, normalized four-Gaussian
increments composed on the left, independently generated radial-axis-angle
increments composed on the right, and a direct Rodrigues matrix path using the
normalized radial CDF `F(theta) = (theta - sin(theta)) / pi`. The direct path
does not obtain its matrix through the quaternion conversion.

The frozen contract uses 200,000 samples per family, fixed seeds, eight
equal-Haar-probability angle bins, and ten component bins for proposal inversion
symmetry. It checks the exact Haar moments of the rotation character, angle,
and all matrix entries; repeats the entry checks after fixed left and right
composition; requires exact unit acceptance under the zero-action Haar target;
and checks stationary occupancy and pairwise binned probability-flux symmetry.

All statistics and their frozen tolerances are serialized. Any violation writes
the explicit negative taxonomy and then throws, so a negative result cannot be
silently promoted to a pass. The outputs are
`output/haar_proposal_invariance_control.json` and its byte-identical summary.
The exact passing Phase487 summary is a mandatory precursor; missing, negative,
or promotion-drifted precursor evidence reaches a distinct fail-closed
terminal before Phase488 can be consumed.

The result is exploration-only. `humanRulingAuthored=false`,
`o4Discharged=false`, `phase458EvaluationAuthorized=false`,
`productionAuthorized=false`, `noGevPromotion=true`, and
`promotedPhysicalMassClaimCount=0` on every branch.
