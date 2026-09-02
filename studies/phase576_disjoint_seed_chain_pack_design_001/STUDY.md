# Phase 576: disjoint-seed chain-pack design

Phase576 exercises the single authority Phase575's planning gate granted:
registering and prospectively freezing a disjoint-seed chain-pack design.
It is deterministic and zero-sampling. Every pack quantity is derived from
committed bytes - the Phase571-supported long arm verbatim (step 0.06, 32
leapfrog steps, trajectory length 1.92), a retained trajectory count of
`ceil(1.25 * slowestModeTrajectoryLengthEstimate / 1.92)` against Phase548's
committed spectral bound, warmup `ceil(0.15 * retained)` - so the pack is
seed-blind by construction (`pristineSeedBlindPreregistration=true`),
repairing the disclosed Phase548 tune-then-confirm weakness. Eight chains in
two tables use a fresh 900xxx seed namespace machine-verified disjoint from
every committed seed; protected Phase554 seeds remain unread. The pack
mandates raw directional series retention (the Phase574 lesson), periodic
and final checkpoints, the hardened R-hat/ESS gates, and names the A39
registered fold convention for any future cross-implementation adjudication.

## Executed result

The v1 terminal is `chain-pack-design-frozen-execution-unauthorized` on the
first frozen execution. The derived pack: 8 chains x 2,125 trajectories
each (278 warmup + 1,847 retained), retained integration time 3,546.24 per
chain against the 2,835.77 slowest-mode estimate at coverage 1.25, and
561,000 total force evaluations within the frozen 620,000 ceiling.

The pack targets improved resolution of the diagnosed under-resolution; it
cannot promise convergence, and a future execution failing its ESS floors
would be a preserved first-class negative. Chain-pack EXECUTION is not
authorized: it requires a separately registered, prospectively frozen phase
in a future registry extension plus explicit written user sampling
authorization. No sampling, replay, RNG, Markov advance, configuration
retention, protected-seed access, production, launch, physical-unit, or GeV
authority follows. External review remains pending and
`promotedPhysicalMassClaimCount=0`.
