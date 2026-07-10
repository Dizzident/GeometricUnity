# CLAUDE.md

Project instructions for Claude sessions working in this repository. Read this
first, then the key docs listed at the bottom.

## What this repo is

An executable research platform for Eric Weinstein's Geometric Unity framework:
a connection-centered, observer-based, explicitly discretized engine (CPU
reference backend, CUDA/Vulkan hooks) plus a large fail-closed validation
surface around a boson-prediction / source-law investigation. The platform
milestones are complete; the active work is the research program, which is
deliberately conservative about what may be claimed.

## Build and test

- .NET 10. Solution is `.slnx` format.
- Standard cycle: `dotnet build && dotnet test --no-build`.
- Stale-cache symptoms (spurious `Build FAILED` with no compiler errors):
  `dotnet clean && dotnet build`.
- 0-warnings policy. Keep it green; do not introduce warnings.
- A linter auto-modifies `Directory.Build.props` and `.csproj` files (e.g.
  normalizing target frameworks to `net10.0`). Expect these files to change
  under you; do not fight the linter. Note: `Directory.Build.props` is a real
  build input of every phase run — editing it invalidates all incremental
  fingerprints (see below).

## Code conventions

- Core types are **sealed classes, not records** — there is no `with` syntax.
- Nullable reference types are enabled.
- `Gu.Math` conflicts with `System.Math`. Where `Gu.Math` is imported, call
  `System.Math.Sqrt()` (and other `System.Math` members) explicitly.

## The boson-prediction study-phase workflow

Research steps live under `studies/phaseNNN_<name>_001/` as small standalone
programs. They are **fail-closed**: a phase asserts a precise verdict and the
surrounding scripts refuse to pass ("review-required" / blocked) on any
mismatch, missing file, or unexpected change. This is the system working as
designed — never weaken a gate to make a pass go green.

Phase outputs (`studies/phaseNNN_*_001/output/`) are gitignored and
force-added at checkpoint time.

**Every new phase must be wired across all validation surfaces in the same
checkpoint.** `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md` holds the
authoritative list; at time of writing it is:

- the generator script run line in
  `scripts/generate_validated_boson_predictions.sh`;
- an item in `scripts/BosonPhasesTraversal.proj` (parallel MSBuild traversal);
- the whole-repo scanner exclusion lists (see the hazard below);
- the phase101 prediction-package block
  (`studies/phase101_boson_prediction_package_001/Program.cs`);
- the phase202 checklist
  (`studies/phase202_boson_objective_completion_audit_001/Program.cs`);
- the asserts in `scripts/verify_boson_claim_integrity.sh`;
- an `IMPLEMENTATION_PNNN.md` under `docs/Phases/Implementation/`.

The next free phase number and the current wiring list are always in the
restart prompt — check it, do not guess.

## Incremental validation (iteration cadence)

The full generator pass runs ~317 phase invocations and takes hours. For
iteration, use the fail-closed content-hash incremental tooling. Read
`scripts/incremental/README.md` for exact semantics before relying on it.

- Iterate: `./scripts/run_boson_phases_incremental.sh --incremental`
  (~3 min on a typical no-change / small-change checkpoint).
- Promote: `./scripts/run_boson_phases_incremental.sh --full` (or the original
  `scripts/generate_validated_boson_predictions.sh`) is **required for any
  promotion-relevant claim**. Incremental passes are for cadence only.
- The committed manifest `scripts/boson_incremental_manifest.json` is the
  load-bearing freshness guarantee; a `--full` pass refreshes and re-seeds it.
- Skip rule is strictly fail-closed: a phase is skipped only when its
  fingerprint, outputs, and read-set all verify; ANY doubt means RUN. A
  fixed always-run set (the whole-repo scanners, the tail claim-boundary
  gates, the central build, and the integrity verifier) always executes.

## CRITICAL HAZARD: whole-repo text scanners

Several study phases are **whole-repo text scanners**. They walk the
filesystem directly (gitignore-blind) across `studies/`, `docs/`, `scripts/`,
and `src/`, auditing repo text for topic keywords outside their known-file
lists. This is a deliberate integrity mechanism.

Consequence (learned the hard way in the 2026-07-10 incident — see the last
entry of `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`): **any new file whose
PATH or TEXT contains a scanner topic keyword flips the scanners to
review-required and cascades blocked verdicts through the precursor gates**,
aborting the whole pass. In that incident the incremental tooling's own skip
reports and manifest embedded phase project paths containing topic keywords;
the result was a multi-hour cascade and a red integrity check. A full pass
would have failed the same way.

Rules that follow:

- Any **non-phase tooling that writes files inside the repo** must be added to
  the exclusion helpers of the six scripts-root scanners (phases
  278 / 279 / 281 / 289 / 295 / 296). The incremental tooling directory and
  its manifest are already excluded there — follow the same pattern.
- Any new doc that legitimately names scanner terms (including
  `IMPLEMENTATION_PNNN.md` files) must be registered in the relevant
  exclusion lists in the same checkpoint. Register deliberately; never weaken
  a scanner.
- **After any doc or tooling change, validate with a ~3-min `--incremental`
  pass before committing.** If your text tripped a scanner, the failing
  scanner's output JSON names the match — fix the wording and rerun. Do not
  commit red.
- When writing docs, avoid gratuitously naming beyond-Standard-Model model
  names or other scanner topic terms; refer to "the literature-audit phases"
  generically instead.

## Honesty discipline (non-negotiable)

- `promotedPhysicalMassClaimCount = 0`. **No absolute GeV mass predictions
  exist, and none may be claimed.** No computed observable is validated as a
  physical W/Z/Higgs/photon property; no unit calibration to GeV exists.
- The legal deliverable class is **anchor-free dimensionless ratios**, clearly
  separated from measured comparison values.
- Preserve scientific caution in all prose and commit messages. Never phrase a
  benchmark or workbench result in a way that could read as a physics
  prediction. Lattice-unit quantities stay in lattice units and are never
  relabeled as particle masses.
- Failed runs and negative results are first-class artifacts — preserve them,
  never silently replace them.

## Git

- Work on `main` only when explicitly asked; otherwise branch first.
- Commit or push only when the user asks. Commit messages end with the
  standard Claude co-author trailer used in recent commits (check
  `git log -1 --format=%B`).

## Key docs

- `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md` — program state, gate status,
  next free phase number, and the authoritative wiring/validation-surface list.
- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md` — running history and incident
  record (read the last entry for the current frontier).
- `docs/Phases/FOUR_D_PLATFORM_BUILD_PLAN.md` — the 4D platform build plan.
- `scripts/incremental/README.md` — exact incremental-validation semantics.
- `README.md` — project overview, layout, and CLI usage.
