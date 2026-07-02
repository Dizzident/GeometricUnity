# Phase428: Fermion-Loop Block-Selection No-Go Probe

First experiment of the 2026-07-01 beyond-the-literature directive. Phases
405/410/418 proved the bare bosonic control-branch objective cannot select
a doublet VEV and named fermionic backreaction as the remaining internal
mechanism class. Phase428 decides that class for one-loop fermion
determinants along the constant rank-1 block rays of the Phase418 menu.

Result (theorem-grade, machine-verified):

- CLASS FUNCTION: on rays `omega = t*u` the Dirac spectrum has the exact
  closed form `lambda^2 = (s1 + t*u_c)^2 + (s2 + t*u_c)^2` over lattice
  momenta and gauge-generator eigenvalues `u_c` - it depends only on the
  adjoint ORBIT of `u` (verified against a dense solve, residual 1.6e-13).
- ONE ORBIT FOR T AND D: the color-swap conjugator maps `lambda_4` to
  `lambda_1` exactly, so the fermion-loop potential is EXACTLY degenerate
  between triplet and doublet directions (residual 1.1e-13) in both the
  fundamental-3 and adjoint-8 representations. Only the `lambda_8` orbit
  (singlet axis) differs.
- NO STABILIZER: along every ray the functional falls like `-N log t`
  with `N` matching the coupled-mode count exactly
  (fund T/D -128, S -192; adj T/D -384, S -256).

Conclusion: an su(3)-invariant fermionic sector CANNOT produce the
direction-dependent block mass law or quartic stabilizer Phase418 had to
import. Doublet selection via fermionic backreaction requires
su(3)-breaking fermionic structure that no reviewed source defines. The
mechanism class is CLOSED on the control branch. No Phase201 or Phase256
field is filled.

Run (Release, seconds):

```bash
dotnet run -c Release --project studies/phase428_fermion_loop_block_selection_no_go_probe_001/Phase428FermionLoopBlockSelectionNoGoProbe.csproj
```
