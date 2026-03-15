# PHASE7_SUMMARY.md

## What The Campaign Tested

The campaign asked four practical questions.

1. If we export the main stability and convergence inputs from persisted upstream-style background artifacts instead of typing them in by hand, do the branch and refinement checks still come out clean?
2. If we make the observation, environment, representation, and coupling side channels explicit, do we actually evaluate them, or were they only empty placeholders?
3. If we compare the computed quantities against both easy control targets and a harder benchmark, do we learn anything more than “the toy setup still agrees with itself”?
4. If we run the results through the candidate-upgrade and candidate-demotion logic, what survives and what gets knocked back?

## What Passed

- The bridge-backed branch check passed cleanly: all 5 bridge-exported quantities were robust across the four persisted branch variants.
- The bridge-backed refinement check passed cleanly: all 5 quantities were classified as convergent.
- All four sidecar channels were actually evaluated, not skipped.
- The observation side records were complete and low-sensitivity for all three registry candidates.
- The environment-variance side records were evaluated across toy, structured, and imported tiers and none of the five tracked quantities was unstable.
- The coupling-consistency side records were evaluated and all three candidates were consistent under the current proxy.
- The control-study quantitative targets all passed.
- The derived-synthetic quantitative check also passed.
- Two candidates were promoted one claim level because every required gate passed for them.

## What Failed Or Still Does Not Count As Real-World Evidence

- One stronger benchmark failed: `bosonic-mode-2-imported-geometry-benchmark` missed by a pull of about `6.24`. In plain English, the structured-environment result is too far from that harder benchmark to count as agreement.
- One candidate was demoted all the way to `C0_NumericalMode` because the representation-content check found that it was missing required supporting structure.
- The imported environment is still a synthetic example with fake provenance fields. That means the campaign exercises the imported-data contract, but not real imported data.
- The bridge-backed atlas is still a checked-in synthetic reference atlas. That means the bridge path is real, but the source evidence is still not solver-generated scientific evidence.
- The stronger benchmark is still an internal benchmark, not a laboratory measurement or outside dataset. So even the “harder” target is not real-world validation.
- The quantitative matcher still picks the first record for each observable ID. Because the file now contains toy, structured, and imported versions of the same observable, that selection is still driven by file order instead of an explicit environment-aware rule.

## What Can Be Done Next

- Replace the synthetic reference atlas with a real persisted atlas from the upstream background solver.
- Replace the synthetic imported environment with a genuine imported geometry dataset and real provenance.
- Replace heuristic sidecar derivation with upstream observation and coupling artifacts.
- Make quantitative matching environment-aware so the report can say exactly which environment passed or failed each target.
- Extend the main evidence path beyond identity-only Shiab so the current “clean” results are not limited to that single branch choice.

## What A Project Lead Can Decide Now

- The evidence pipeline itself is now good enough to use as the standard reporting path. It is no longer just a smoke test.
- The current campaign is strong enough to separate “the controls still work” from “the harder benchmark does not match.”
- The project should not claim physical validation, real imported-data validation, or experiment-facing agreement from this run.
- Phase VII should focus on replacing synthetic inputs with real upstream artifacts, not on rebuilding the reporting shell again.
