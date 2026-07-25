# Phase551 v1 lineage - preserved and NON-CITABLE

These bytes are the first registered Phase551 attempt. They are preserved
because this program does not silently replace failed runs, and they are
non-citable: no downstream phase, document, or claim may cite any number in
them.

Two defects were found in this phase's own code and specification, both caught
by the adjudication itself rather than by inspection.

1. **Implementation defect in the homogeneous solve.** The independent
   decomposition solved `S(t) = t^2 S2 + t^3 S3 + t^4 S4` from `t = 1, 2, 3`
   with an incorrect elimination for the degree-four coefficient. The solve
   still reproduced `S(1)` exactly, so the error was invisible in the value and
   showed up only as disagreement with the audited split. The corrected
   elimination is `S4 = (2 s3 - 9 s2 + 18 s1) / 36`, `S3 = (s2 - 4 s1 - 12 S4)
   / 4`, `S2 = s1 - S3 - S4`.
2. **Specification defect in the smallest-eigenvalue comparison, present in
   both contracts.** The v1 rule required the independent and audited
   a-posteriori intervals for "the smallest eigenvalue on the orthogonal
   complement of the measured null basis" to overlap. That quantity is not
   well posed: deflating against the constructed null basis does not remove the
   assembled form's roundoff-level near-null eigenvectors, so the deflated
   operator genuinely retains an eigenvalue near zero alongside the intended
   gap. The audited phase's Lanczos converged to the residue near zero and this
   phase's shifted block iteration converged to the gap at `0.186888`; both are
   valid a-posteriori intervals around genuine eigenvalues of the deflated
   operator, so their non-overlap is not a disagreement about the operator.

Neither defect touched an integer comparison, an inertia count, a rung, or any
firewall. Every load-bearing agreement in the v1 run - the threshold-free
integer flat-sector lower bound, the inertia counts at all eight rungs, the
negative inertia, the largest eigenvalue, the null-basis residual, and the
flatness ladder - already held.
