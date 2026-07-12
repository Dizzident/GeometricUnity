# Implementation P457 - Upsilon Portal Stage A (Team B, Wave-2) (Team B)

Phase457 implements **Stage A** of the WS3 Upsilon-portal probe under the
standing hold (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md` item 10;
`TEAM_ELIMINATION_PROGRAM_2026-07-10.md` item 15). Registry number 457 per
`docs/Phases/PHASE_NUMBER_REGISTRY.md`. It supersedes the Step-0 skeleton
(interim terminal `awaiting-stage-a`); the design was pre-registered in
`studies/phase457_upsilon_portal_stage_a_001/STUDY.md` before any of the
Stage-A mathematics was written.

## What this phase commits

1. **Exhaustive-cell boundedness-below certificates** for the portal action
   class `S_portal(omega,theta,M) = S_B + kappa*sum||D_omega M||^2 +
   sum_x[lambda|M|^4 + (mu2 + g*Omega)|M|^2]`, `Omega in {||F||, ||F||^2}`, over
   a disjoint, exhaustive sign-cell menu (machine-checked, 24/24 over
   `sign(lambda) x sign(g) x sign(mu2) x convention`). Certificates are EXACT
   mathematics resting on committed structural facts (`S_B >= 0`;
   control-branch `Upsilon = F` so `||F||^2 = 2 S_B`; `S_B ~ t^4`, `||F|| ~ t^2`
   on non-flat rays; `F = 0` on the flat vacuum). Seven cells: 2 bounded,
   4 unbounded, 1 conditional-critical with exact threshold
   `g_crit = sqrt(2 lambda / rho)` (`rho = 1` control). The unbounded cells are
   named certificates that constrain the probe design, not physics claims.
   No sampling; no HMC.
2. **The null-hash firewall keyed to phase466**: no verdict-bearing summary
   field is emitted unless the on-disk phase466 `{schemaId, schemaHash}` matches
   the pinned pair AND an `o4MProbeRuling` record exists. The record does not
   exist, so the portal verdict is WITHHELD. A battery machine-checks it
   (synthetic wrong `schemaHash` / wrong `schemaId` block; a synthetic
   `o4MProbeRuling` opens the conjunction).
3. **Arm Q motivation-gate measurement code — implemented and GATED OFF**: the
   seed-regenerated-ensemble portal statistic plus the RNG-stream-neutrality
   battery, executable later via the committed default once the launch gates
   `G-Q1..G-Q4` hold. Here only a synthetic plumbing self-test runs (zero
   physics).

## Terminal

`stage-a-certificates-committed` (all cells certified). The portal verdict is
withheld; pre-unlock the only reachable summary terminal is
`measurement-recorded-verdict-withheld`. Closes ledger limb L7 only post-unlock,
with named reopening conditions.

## Shared-surface wiring deltas (for the next checkpoint; NOT edited here)

The advance from `awaiting-stage-a` to `stage-a-certificates-committed` requires
the following shared-surface asserts (owned outside this phase) to be updated in
the batched wave checkpoint. They still assert the old interim terminal:

- `scripts/verify_boson_claim_integrity.sh` ~L5542-5543:
  `phase457.interimTerminal === "awaiting-stage-a"` and
  `phase457.verdictKind === "awaiting-stage-a"` → `"stage-a-certificates-committed"`;
  add asserts for `firewall.firewallOpen === false`,
  `firewall.firewallMachineChecked === true`,
  `stageA.allCellsCertified === true`, `armQ.gatedOff === true`.
- `studies/phase202_boson_objective_completion_audit_001/Program.cs` ~L6491:
  `interimTerminal == "awaiting-stage-a"` → `"stage-a-certificates-committed"`.
- `studies/phase101_boson_prediction_package_001/Program.cs`: passthrough of
  `terminalStatus`/`interimTerminal`/`verdictKind` — no hard assert on the
  value, updates automatically.
- `scripts/boson_incremental_manifest.json` L3376-3377: phase457 output hashes
  change; re-seed via the `--full` pass at checkpoint.

The standing-boundary fields the shared surfaces assert
(`upsilonPortalStageASkeletonBuilt`, `targetBlindConstruction`,
`physicistReviewPending`, `noGevPromotion`, all contract/promotion falses) are
preserved unchanged, so those asserts continue to hold.

## Framing

Stage A is exact mathematics about the action class; it constrains the probe
design and is not a physics claim about any target. Nothing is measured, filled,
or promoted; `promotedPhysicalMassClaimCount = 0`; `physicistReviewPending`
carried explicitly. Lattice-unit quantities stay in lattice units and are never
relabelled.
