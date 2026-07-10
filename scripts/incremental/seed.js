#!/usr/bin/env node
"use strict";
// Seed the incremental manifest from a COMPLETED full pass's outputs on disk.
//
//   node scripts/incremental/seed.js [--script <generator.sh>]
//       [--manifest <manifest.json>] [--dry-run] [--fresh] [--verbose]
//
// For every phase step in the generator registry (in order, so producer
// entries exist before consumers), computes the input fingerprint and the
// canonical output hashes and writes the manifest entry.
//
// FAIL-CLOSED GUARD: a phase whose output dir is missing or empty is NOT
// seeded (an entry with no outputs could let an incremental pass skip a
// phase that never produced its artifacts). Run this only after a full
// generator pass has completed on the same checkout.

const path = require("path");
const { repoRootFromHere, MANIFEST_RELPATH } = require("./config");
const { parseGeneratorScriptFile, requireCleanParse } = require("./registry");
const { loadManifest, saveManifest, emptyManifest } = require("./manifest");
const { buildEntry } = require("./entry");

function parseArgs(argv) {
  const args = { dryRun: false, fresh: false, verbose: false };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--dry-run") args.dryRun = true;
    else if (a === "--fresh") args.fresh = true;
    else if (a === "--verbose") args.verbose = true;
    else if (a === "--script") args.script = argv[++i];
    else if (a === "--manifest") args.manifest = argv[++i];
    else if (a === "--repo-root") args.repoRoot = argv[++i];
    else {
      console.error(`seed: unknown argument: ${a}`);
      process.exit(2);
    }
  }
  return args;
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

  let manifest;
  if (args.fresh) manifest = emptyManifest();
  else {
    try {
      manifest = loadManifest(manifestPath);
    } catch (err) {
      console.error(`seed: existing manifest unreadable (${err.message}); starting fresh`);
      manifest = emptyManifest();
    }
  }

  const stats = { seeded: 0, skippedEmptyOutputs: [], forcedRun: [], errors: [] };
  for (const step of phaseSteps) {
    const key = String(step.phase);
    try {
      const previousEntry = manifest.phases[key];
      const { entry, forcedRunReasons } = buildEntry({
        repoRoot,
        step,
        registryPhaseSet,
        manifest,
        previousEntry,
      });
      if (Object.keys(entry.outputs).length === 0) {
        if (!stats.skippedEmptyOutputs.includes(step.phase)) stats.skippedEmptyOutputs.push(step.phase);
        if (args.verbose) console.error(`seed: phase${step.phase}: NOT seeded (no outputs on disk)`);
        continue;
      }
      manifest.phases[key] = entry;
      stats.seeded += 1;
      if (forcedRunReasons.length > 0 && !stats.forcedRun.some(([p]) => p === step.phase)) {
        stats.forcedRun.push([step.phase, forcedRunReasons[0]]);
      }
      if (args.verbose) {
        console.error(
          `seed: phase${step.phase}: fingerprint ${entry.inputFingerprint.slice(0, 12)}..., ` +
            `${Object.keys(entry.outputs).length} output(s)` +
            (forcedRunReasons.length ? ` [will always run: ${forcedRunReasons[0]}]` : "")
        );
      }
    } catch (err) {
      stats.errors.push([step.phase, err.message]);
      console.error(`seed: phase${step.phase}: ERROR ${err.message}`);
    }
  }

  if (!args.dryRun) {
    saveManifest(manifestPath, manifest);
    console.log(`seed: wrote ${manifestPath}`);
  } else {
    console.log("seed: dry run, manifest not written");
  }
  console.log(
    `seed: ${stats.seeded} entr${stats.seeded === 1 ? "y" : "ies"} seeded; ` +
      `${stats.skippedEmptyOutputs.length} phase(s) not seeded (no outputs); ` +
      `${stats.forcedRun.length} seeded phase(s) will still always run (fail-closed doubt); ` +
      `${stats.errors.length} error(s)`
  );
  if (stats.skippedEmptyOutputs.length > 0) {
    console.log(`seed: no outputs on disk for phases: ${stats.skippedEmptyOutputs.join(", ")}`);
  }
  for (const [p, why] of stats.forcedRun) console.log(`seed: phase${p} always runs: ${why}`);
  process.exit(stats.errors.length > 0 ? 1 : 0);
}

main();
