# Implementation P519 — A5 Candidate-Foundation Readiness

## Status

Finalized and frozen before the first runnable invocation. The Phase519
preregistration contract exact-binds eight artifacts: the Phase517 summary,
contract, program, site candidate, and link candidate, plus the Phase518
summary, contract, and program. `Program.cs` reads only its own contract before
checking every binding for placeholders and throws before reading any precursor
if a placeholder is present.

The final Release build completed without warnings or errors. Two consecutive
standalone invocations emitted the expected subject-ambiguity terminal and
byte-identical full/summary JSON, each with SHA-256
`83649b2cecabacae57b95b03a5d75063f6130514cfeaa1767e332dc222aa33a2`.

## Fail-closed adjudication

After exact bindings are supplied, Phase519 validates the required precursor
terminals and applies the Amendment A17 precedence in this order:

1. invalid or drifted input;
2. action or observable ambiguity;
3. incomplete configuration or normalized measure;
4. incomplete reflection or pullback;
5. unproved candidate-target equality;
6. unproved bilinear finiteness or Hermiticity;
7. finite-only evidence;
8. unproved necessity;
9. unproved embedding or gluing;
10. unproved all-scope extension;
11. candidate-package review readiness.

The executed terminal is
`a5-candidate-foundation-readiness-action-or-observable-subject-ambiguous`, preserving
the earliest substantive Phase517 action-member/parity ambiguity. Later
Phase518 finite nonclosure cannot mask that earlier blocker.

## Boundaries

Even the strongest terminal, `candidate-package-review-ready`, means only that
the frozen package is ready for independent mathematical review. External
review remains pending. It is not a theorem, counterexample, positivity result,
gate closure, or execution authorization.

Phase515 and Phase516 remain locked. Every theorem, L8, Phase458, sampling,
HMC, benchmark, production, launch, Binder, acceleration, O4, human-ruling,
source-contract, route-promotion, and physical-claim firewall remains false,
with `promotedPhysicalMassClaimCount=0`.

No shared traversal, generator, verifier, manifest, registry, amendment, or
Phase101 wiring is changed by this standalone adjudicator.
