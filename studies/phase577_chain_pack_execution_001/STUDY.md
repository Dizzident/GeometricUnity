# Phase 577: chain-pack execution

Phase577 is the single authorized execution of the frozen pack
`a40-disjoint-seed-chain-pack-v1`, run under the explicit written user
sampling authorization of 2026-09-01 recorded verbatim in Amendment A41 and
in the frozen contract. The contract was frozen before the first registered
seed was drawn, and the pack itself was derived seed-blind from committed
bytes, so the execution is pristine seed-blind preregistration end to end.

## Executed result

The terminal is `pack-executed-all-frozen-resolution-gates-pass` on the
first frozen execution. All eight chains completed their 2,125 trajectories
at the confirmed long arm (step 0.06, 32 leapfrog steps) with zero
non-finite and zero divergent trajectories, acceptance rates 0.9327-0.9511,
and largest absolute energy error 0.7206. All 36 frozen resolution gates
pass decisively: worst split rank-normalized R-hat 1.0019 against the 1.01
threshold, minimum bulk ESS 1,624 and minimum tail ESS 2,714 against the
100 floors. Every mandated artifact is retained: per-trajectory telemetry,
checkpoints every 250 trajectories plus final, and all 18 raw directional
series per chain.

This is the first fully resolved directional diagnostic result on the
registered target: the Phase548 pilot's under-resolution is removed at
eighteen times the pilot's integration time, exactly as the independently
confirmed Phase571 longer-trajectory lever predicted.

## Claim boundary

The result is workbench-relative and in lattice units. Passing the frozen
gates establishes resolution of the audited directional series only: it is
not stationarity, not sampling correctness beyond the frozen gates, not a
spectral or physical quantity, and not a production benchmark. It does not
satisfy Phase458, O4, Phase481, or any source contract, and it supports no
physical-unit or GeV claim. Phase572's terminal and tolerance stand,
protected Phase554 seeds remain unread, external review remains pending,
and `promotedPhysicalMassClaimCount=0`.
