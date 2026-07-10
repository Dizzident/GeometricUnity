#!/usr/bin/env node
"use strict";
// Per-phase, ON-DEMAND read-set capture (binding design item 6).
//
//   node scripts/incremental/capture_readset.js --phase <N>
//       [--script <generator.sh>] [--manifest <manifest.json>]
//       [--build] [--keep-log] [--list-stale]
//
// Runs ONE phase under instrumentation, filters the observed opens to
// repo-relative read paths (excluding obj/, bin/, .git/, the phase's own
// output dir, and everything outside the repo -- dotnet runtime, /usr,
// /tmp, ...), stores the read-set in the phase's manifest entry, and
// recomputes the entry's fingerprint so the newly recorded inputs are
// folded in. Invoke it when a phase is first hardened or when its code
// changes; it is NOT a required full pass.
//
// Instrumentation: strace -f -e trace=openat when strace is available;
// otherwise an LD_PRELOAD libc-interposition shim (readset_shim.c, built
// on demand with cc). This host has no syscall tracer, so the shim is the
// operative path.
//
// --list-stale: no run; lists phases whose recorded read-set was captured
// against a different phase-code hash than what is on disk now.

const fs = require("fs");
const path = require("path");
const os = require("os");
const { spawnSync } = require("child_process");
const {
  repoRootFromHere,
  MANIFEST_RELPATH,
  READSET_EXCLUDE_RELPREFIXES,
  READSET_EXCLUDE_SEGMENTS,
} = require("./config");
const { parseGeneratorScriptFile, requireCleanParse } = require("./registry");
const { loadManifest, saveManifest } = require("./manifest");
const { buildEntry } = require("./entry");
const { computeFingerprint } = require("./fingerprint");

function parseArgs(argv) {
  const args = { build: false, keepLog: false, listStale: false };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--phase") args.phase = parseInt(argv[++i], 10);
    else if (a === "--script") args.script = argv[++i];
    else if (a === "--manifest") args.manifest = argv[++i];
    else if (a === "--repo-root") args.repoRoot = argv[++i];
    else if (a === "--build") args.build = true;
    else if (a === "--keep-log") args.keepLog = true;
    else if (a === "--list-stale") args.listStale = true;
    else {
      console.error(`capture_readset: unknown argument: ${a}`);
      process.exit(2);
    }
  }
  return args;
}

function haveStrace() {
  const r = spawnSync("strace", ["-V"], { stdio: "ignore" });
  return r.status === 0;
}

function buildShim(repoRoot) {
  const cacheDir = path.join(repoRoot, "scripts/incremental/.cache");
  fs.mkdirSync(cacheDir, { recursive: true });
  const shimSrc = path.join(__dirname, "readset_shim.c");
  const shimSo = path.join(cacheDir, "readset_shim.so");
  if (fs.existsSync(shimSo) && fs.statSync(shimSo).mtimeMs >= fs.statSync(shimSrc).mtimeMs) {
    return shimSo;
  }
  const r = spawnSync("cc", ["-shared", "-fPIC", "-O2", "-o", shimSo, shimSrc, "-ldl"], {
    stdio: "inherit",
  });
  if (r.status !== 0) throw new Error("capture_readset: shim compilation failed");
  return shimSo;
}

// Filter raw absolute paths to a sorted unique repo-relative read-set.
function filterPaths(rawPaths, repoRoot, ownOutputPrefix, projectDirRel) {
  const keep = new Set();
  const rootPrefix = repoRoot.endsWith("/") ? repoRoot : repoRoot + "/";
  for (let p of rawPaths) {
    p = p.trim();
    if (p === "") continue;
    p = path.posix.normalize(p);
    if (!p.startsWith(rootPrefix)) continue;
    const rel = p.slice(rootPrefix.length);
    if (rel === "") continue;
    if (READSET_EXCLUDE_RELPREFIXES.some((x) => rel.startsWith(x))) continue;
    if (READSET_EXCLUDE_SEGMENTS.some((x) => ("/" + rel).includes(x))) continue;
    if (rel === ownOutputPrefix || rel.startsWith(ownOutputPrefix + "/")) continue; // self-writes
    if (rel.startsWith("scripts/incremental/")) continue; // our own tooling
    let st;
    try {
      st = fs.statSync(path.join(repoRoot, rel));
    } catch {
      continue; // opened-then-missing: transient, ignore
    }
    if (!st.isFile()) continue; // directories opened for enumeration are
    // covered by scan components; recording them as files would be wrong.
    keep.add(rel);
  }
  // Reads inside the project dir itself (e.g. runtimeconfig) are covered by
  // the self component; keep the set focused on external inputs.
  const out = [...keep].filter((rel) => !(rel === projectDirRel || rel.startsWith(projectDirRel + "/")));
  out.sort();
  return out;
}

function main() {
  const args = parseArgs(process.argv.slice(2));
  const repoRoot = args.repoRoot ? path.resolve(args.repoRoot) : repoRootFromHere();
  const scriptPath = args.script
    ? path.resolve(args.script)
    : path.join(repoRoot, "scripts/generate_validated_boson_predictions.sh");
  const manifestPath = args.manifest
    ? path.resolve(args.manifest)
    : path.join(repoRoot, MANIFEST_RELPATH);

  const parsed = requireCleanParse(parseGeneratorScriptFile(scriptPath), scriptPath);
  const phaseSteps = parsed.steps.filter((s) => s.kind === "phase");
  const registryPhaseSet = new Set(phaseSteps.map((s) => s.phase));
  const manifest = loadManifest(manifestPath);

  if (args.listStale) {
    let stale = 0;
    for (const step of phaseSteps) {
      const entry = manifest.phases[String(step.phase)];
      if (!entry || !entry.recordedInputs) continue;
      try {
        const fp = computeFingerprint({ repoRoot, step, registryPhaseSet, manifest, entry });
        if (entry.readsetCapturedForSelfHash && entry.readsetCapturedForSelfHash !== fp.selfTreeHash) {
          console.log(`phase${step.phase}: read-set captured for a different code state; re-instrument`);
          stale += 1;
        }
      } catch (err) {
        console.log(`phase${step.phase}: cannot evaluate (${err.message})`);
      }
    }
    console.log(`capture_readset: ${stale} stale read-set(s)`);
    return;
  }

  if (!Number.isInteger(args.phase)) {
    console.error("capture_readset: --phase <N> is required");
    process.exit(2);
  }
  const step = phaseSteps.find((s) => s.phase === args.phase);
  if (!step) {
    console.error(`capture_readset: phase${args.phase} not found in ${scriptPath}`);
    process.exit(2);
  }

  if (args.build) {
    const r = spawnSync("bash", ["-c", `dotnet build -c Release ${step.project}`], {
      cwd: repoRoot,
      stdio: "inherit",
    });
    if (r.status !== 0) process.exit(r.status || 1);
  }

  const logFile = path.join(os.tmpdir(), `readset_phase${step.phase}_${process.pid}.log`);
  let cmd;
  let env = { ...process.env };
  if (haveStrace()) {
    cmd = `strace -f -e trace=open,openat -o ${JSON.stringify(logFile)} -qq ${step.invocation}`;
  } else {
    const shim = buildShim(repoRoot);
    env.LD_PRELOAD = shim;
    env.READSET_LOG = logFile;
    cmd = step.invocation;
    console.error("capture_readset: no strace on this host; using LD_PRELOAD shim");
  }

  console.error(`capture_readset: running phase${step.phase} under instrumentation`);
  const r = spawnSync("bash", ["-c", "set -euo pipefail\n" + cmd], {
    cwd: repoRoot,
    stdio: "inherit",
    env,
  });
  if (r.status !== 0) {
    console.error(`capture_readset: phase run failed (exit ${r.status}); read-set NOT recorded`);
    process.exit(r.status || 1);
  }

  let rawPaths;
  const logText = fs.existsSync(logFile) ? fs.readFileSync(logFile, "utf8") : "";
  if (env.READSET_LOG) {
    rawPaths = logText.split("\n");
  } else {
    // strace format: pid open(at)("path", O_RDONLY...) = fd
    rawPaths = [];
    for (const line of logText.split("\n")) {
      const m = line.match(/open(?:at)?\((?:AT_FDCWD, )?"([^"]+)"(?:, ([^)]*))?\)\s*=\s*\d+/);
      if (!m) continue;
      const flags = m[2] || "";
      if (/O_WRONLY/.test(flags)) continue;
      rawPaths.push(m[1].startsWith("/") ? m[1] : path.join(repoRoot, m[1]));
    }
  }
  if (!args.keepLog) {
    try {
      fs.unlinkSync(logFile);
    } catch {}
  }

  const projectDirRel = path.posix.dirname(step.project.replace(/\\/g, "/"));
  const ownOutputPrefix = projectDirRel + "/output";
  const recordedInputs = filterPaths(rawPaths, repoRoot, ownOutputPrefix, projectDirRel);

  // Rebuild the manifest entry with the new read-set folded into the
  // fingerprint (the entry's fingerprint must be recomputed or the phase
  // would mismatch forever).
  const key = String(step.phase);
  const previousEntry = { ...(manifest.phases[key] || {}), recordedInputs };
  const { entry, selfTreeHash } = buildEntry({
    repoRoot,
    step,
    registryPhaseSet,
    manifest,
    previousEntry,
  });
  entry.recordedInputs = recordedInputs;
  entry.readsetCapturedForSelfHash = selfTreeHash;
  manifest.phases[key] = entry;
  saveManifest(manifestPath, manifest);

  console.log(`capture_readset: phase${step.phase}: recorded ${recordedInputs.length} input path(s)`);
  for (const relPath of recordedInputs) console.log(`  ${relPath}`);
  console.log(`capture_readset: manifest updated: ${manifestPath}`);
}

main();
