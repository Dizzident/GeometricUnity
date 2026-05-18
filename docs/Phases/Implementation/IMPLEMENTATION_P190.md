# Implementation P190: W/Z Direct Target-Independent Geometric Bridge-Source Law

P190 implements a standalone branch-local candidate for the direct W/Z bridge-source law requested by P188:

`B_b,k(i,j) = <psi_b,i, delta D_omega[eta_b,k] psi_b,j>`.

The implementation reads Phase12 geometry, spinor representation, boson registry entries, mode-family metadata, boson mode signatures, boson mode vectors, Phase91 branch-stability-promoted fermion mode vectors, and Phase12 finite-difference variation matrices. It evaluates the P172 best W/Z-like fermion pair `4 -> 6` across all candidate boson modes on both sibling backgrounds, then ranks candidates by sibling-background relative spread. This makes P190 comparable with P172's promoted-mode pair rather than a raw Phase12 fermion-mode exploratory census. The construction is target-independent: it does not use W/Z target masses, target-implied raw amplitudes, or target residuals.

Research finding: the manuscript draft/completion does not provide a unique direct W/Z geometric bridge-source law. It supplies a typed fermionic action placeholder and the VO-7 mixed-linearization obligation, so this implementation is a branch-local candidate for evidence gathering, not a theorem or promoted derivation.

Artifacts:

- `wz_direct_target_independent_geometric_bridge_source_law.json`
- `wz_direct_target_independent_geometric_bridge_source_law_summary.json`

Current result should be interpreted as a stability census for the candidate law only. Promotion still requires a derivation-backed unique law satisfying the mixed-linearization obligation.
