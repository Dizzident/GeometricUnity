# Fail-closed content-hash incremental validation

Tooling for skipping provably-unchanged phases of the boson-prediction
generator (`scripts/generate_validated_boson_predictions.sh`, ~317 phase
invocations, ~6.5-7 h full pass) on checkpoint passes. Binding design:
`docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`, section "PERFORMANCE
EXPLORATION OUTCOME (2026-07-09)".

The existing generator script is UNTOUCHED and remains the reference path.
The claim-integrity verifier asserts values, not freshness; the manifest
written by this tooling (`scripts/boson_incremental_manifest.json`,
committed) is the load-bearing freshness guarantee.

## Usage

```bash
./scripts/run_boson_phases_incremental.sh                # full pass, same semantics as the generator
./scripts/run_boson_phases_incremental.sh --incremental  # skip provably unchanged phases
node scripts/incremental/driver.js --print-registry      # inspect the parsed registry
node scripts/incremental/driver.js --incremental --dry-run  # decisions only, nothing runs
node scripts/incremental/seed.js                         # seed manifest from a completed full pass
node scripts/incremental/capture_readset.js --phase N [--build]  # record one phase's real read-set
node scripts/incremental/capture_readset.js --list-stale # read-sets captured against old phase code
node --test scripts/incremental/tests/                   # unit tests
```

## The promotion-relevant-claims rule

**Any promotion-relevant claim requires a `--full` pass.** Incremental
passes are for iteration cadence only. Periodic `--full` passes re-validate
the manifest (a full pass refreshes every entry as it goes, so it doubles
as a re-seed).

## The fail-closed skip rule

Phase P is skipped IFF ALL of:

1. P is not in the always-run set;
2. its manifest entry exists with a matching `schemaVersion`;
3. the freshly recomputed `inputFingerprint` equals the recorded one;
4. the fingerprint computation raised no doubt (no statically unresolvable
   scanner root, no uncovered interpolated repo path);
5. every manifest-listed output exists on disk with a matching canonical
   hash;
6. every recorded read-set path resolves (exists).

ANY mismatch, missing file, parse error, unknown phase, corrupt manifest,
or exception means RUN. Every decision (run and skip, with reason) is
logged to a per-pass skip report in `scripts/incremental/skip_reports/`
(gitignored; part of the pass record).

## Always-run set (kept as data in `config.js`)

- 204, 205, 253, 279, 281, 295, 296 - the 7 whole-repo text scanners
- 101, 202, 212, 216, 217, 158, 219 - tail claim-boundary gates
- 151, 156 - they read the generator script itself
- the central `dotnet msbuild` build and
  `scripts/verify_boson_claim_integrity.sh` always run structurally.

## Volatile-key list (ONE place: `config.js` `VOLATILE_KEYS`)

`generatedAt`, `runtimeSeconds`, `gpuScanSeconds`,
`slqHessianVectorSeconds`, `fullJointGradientSeconds`,
`fullThetaGradientSeconds`, `estimatedCpuFullScanSeconds`,
`cpuParitySeconds` - stripped at ANY nesting depth before hashing JSON.
Canonicalization (sorted keys, deterministic serialization) is used only
for hashing; files on disk are never rewritten. Non-JSON outputs get raw
sha256; a `.json` that fails to parse falls back to raw sha256.

## Input fingerprint composition (per phase, sorted labeled components)

- `scheme:` schema version constant (`SCHEMA_VERSION`)
- `invoke:` the exact generator invocation line, including env prefixes
- `frozen:` repo-root build config read by every `dotnet run` invocation
  (`Directory.Build.props` etc. - confirmed real inputs by read-set
  capture; the props linter therefore correctly invalidates everything)
- `self:` content tree hash of the phase project dir (sources, csproj,
  auxiliary data; `obj/`, `bin/`, `output/` excluded)
- `src:` content tree hash per TRANSITIVELY referenced `src/` project dir
- `dep:` for each phase-JSON input produced by another run node: producer
  phase id + the producer's manifest canonical output hash (volatile-
  immune, so a producer re-run with identical content does not cascade)
- `frozen:` path + content hash for static inputs (legacy dir-root JSONs,
  the phase12 background_family subtree, committed docs); a missing file
  hashes as `MISSING` - a deterministic state whose later appearance flips
  the fingerprint
- `scan:` content tree hash of each statically resolved scanner root
  (catches NEW files appearing in scanned dirs)
- `native:` content tree hash of `native/build` for `LD_LIBRARY_PATH`
  phases (phase405)
- `readset:` content hash of each recorded read-set path not covered by
  another component

Tree hashes are deterministic content hashes over a sorted file walk (not
git plumbing - scanned roots contain gitignored outputs and working-tree
edits must be seen). JSON files inside trees are hashed canonically.

Static extraction resolves plain const strings, `Path.Combine` chains, and
interpolated consts whose placeholders are resolvable (fixpoint). Scanner
roots passed through method parameters (phases 371, 372, 373, 389, 390,
394 at time of writing) are UNRESOLVABLE and those phases always run until
their code exposes const roots or the list shrinks - fail-closed.

## Seeding

After a full generator pass completes on a checkout:

```bash
node scripts/incremental/seed.js          # writes scripts/boson_incremental_manifest.json
git add scripts/boson_incremental_manifest.json && git commit ...
```

Seeding computes fingerprints and canonical output hashes for every phase
in registry order (producers before consumers). A phase whose output dir
is missing or empty is NOT seeded (an empty-outputs entry could skip a
phase that never produced its artifacts). `--dry-run` reports without
writing; `--fresh` ignores the existing manifest (dropping recorded
read-sets - normally undesirable).

## Read-set capture (on-demand hardening, not a required pass)

Static const-path parsing is a lower bound on the true read-set. For any
phase you want hardened (or after its code changes - see `--list-stale`):

```bash
node scripts/incremental/capture_readset.js --phase 197 --build
```

runs that ONE phase instrumented, filters opens to repo-relative read
paths (excluding `obj/`, `bin/`, `.git/`, the phase's own output dir, and
everything outside the repo), stores them in the manifest entry, and
recomputes the entry fingerprint with the recorded paths folded in.

This host has no syscall tracer (no strace/ltrace/fatrace/bpftrace/perf),
so the tool uses an `LD_PRELOAD` libc-interposition shim
(`readset_shim.c`, compiled on demand with `cc` into
`scripts/incremental/.cache/`). If strace is installed later the tool
prefers it automatically. Residual caveat: a recorded read-set vouches for
the files read AT CAPTURE TIME; reads of files enumerated dynamically
outside resolved scanner roots can only be re-verified by re-capture -
hence the periodic `--full` passes.

## Files

- `config.js` - schema version, volatile keys, always-run set (all data here)
- `canonical.js` - canonical JSON hashing
- `treehash.js` - deterministic directory content hashes
- `extract.js` - static input extraction from phase sources
- `registry.js` - generator-script parser (fails loudly on unknown lines)
- `fingerprint.js` - fingerprint composition
- `should_run.js` - the skip rule
- `entry.js` - manifest entry construction from disk state
- `manifest.js` - manifest IO
- `seed.js`, `driver.js`, `capture_readset.js` - CLIs
- `../run_boson_phases_incremental.sh` - the wrapper entry point
- `../boson_incremental_manifest.json` - the committed manifest
- `tests/` - `node --test` suite
