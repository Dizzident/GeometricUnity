#!/usr/bin/env node
"use strict";
// Registry-driven wrapper around the boson-prediction generator.
//
//   node scripts/incremental/driver.js [--full | --incremental]
//       [--script <generator.sh>] [--manifest <manifest.json>]
//       [--dry-run] [--print-registry]
//
// Default (no flag / --full): runs the registry EXACTLY as the existing
// generator script does -- one central build, every phase in order, the
// integrity verifier at the end -- and refreshes the manifest entry after
// each successful phase (so a full pass doubles as a re-seed).
//
// --incremental: consults the fail-closed skip rule per phase. Every
// decision (run or skip, with reason) is logged to a per-pass skip report
// under scripts/incremental/skip_reports/. The always-run set, the build,
// and the integrity verifier are never skipped.
//
// The existing scripts/generate_validated_boson_predictions.sh is left
// untouched and remains the reference path.

const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");
const {
  repoRootFromHere,
  MANIFEST_RELPATH,
  SKIP_REPORT_DIR_RELPATH,
} = require("./config");
const { parseGeneratorScriptFile, requireCleanParse } = require("./registry");
const { loadManifest, saveManifest, emptyManifest } = require("./manifest");
const { decide } = require("./should_run");
const { buildEntry } = require("./entry");

function parseArgs(argv) {
  const args = { mode: "full", dryRun: false, printRegistry: false };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--incremental") args.mode = "incremental";
    else if (a === "--full") args.mode = "full";
    else if (a === "--dry-run") args.dryRun = true;
    else if (a === "--print-registry") args.printRegistry = true;
    else if (a === "--script") args.script = argv[++i];
    else if (a === "--manifest") args.manifest = argv[++i];
    else if (a === "--repo-root") args.repoRoot = argv[++i];
    else {
      console.error(`driver: unknown argument: ${a}`);
      process.exit(2);
    }
  }
  return args;
}

function execLine(line, repoRoot, dryRun) {
  if (dryRun) {
    console.log(`driver: [dry-run] ${line}`);
    return 0;
  }
  console.log(`driver: + ${line}`);
  const res = spawnSync("bash", ["-c", "set -euo pipefail\n" + line], {
    cwd: repoRoot,
    stdio: "inherit",
  });
  return res.status === null ? 1 : res.status;
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
  if (args.printRegistry) {
    console.log(JSON.stringify({ buildSteps: parsed.buildSteps, steps: parsed.steps }, null, 2));
    return;
  }

  const registryPhaseSet = new Set(parsed.steps.filter((s) => s.kind === "phase").map((s) => s.phase));

  let manifest;
  let manifestCorrupt = null;
  try {
    manifest = loadManifest(manifestPath);
  } catch (err) {
    manifestCorrupt = err.message;
    manifest = emptyManifest();
    console.error(`driver: MANIFEST CORRUPT (${err.message}) -- fail-closed, every phase runs`);
  }

  const report = {
    startedAt: new Date().toISOString(),
    mode: args.mode,
    dryRun: args.dryRun,
    script: path.relative(repoRoot, scriptPath),
    manifest: path.relative(repoRoot, manifestPath),
    manifestCorrupt,
    steps: [],
    finishedAt: null,
    result: null,
  };
  const reportDir = path.join(repoRoot, SKIP_REPORT_DIR_RELPATH);
  const reportPath = path.join(
    reportDir,
    `pass_${report.startedAt.replace(/[:.]/g, "-")}.json`
  );
  const writeReport = () => {
    fs.mkdirSync(reportDir, { recursive: true });
    fs.writeFileSync(reportPath, JSON.stringify(report, null, 2) + "\n");
  };

  try {
    // Central build: always runs (as in the generator script).
    for (const line of parsed.buildSteps) {
      const code = execLine(line, repoRoot, args.dryRun);
      report.steps.push({ kind: "build", invocation: line, decision: "run", reason: "build always runs", exitCode: code });
      if (code !== 0) {
        report.result = `build failed (exit ${code})`;
        writeReport();
        process.exit(code);
      }
    }

    let ran = 0;
    let skipped = 0;
    for (const step of parsed.steps) {
      let decision;
      if (args.mode === "full") {
        decision = { run: true, reason: "--full pass (skipping disabled)" };
      } else {
        decision = decide({ repoRoot, step, registryPhaseSet, manifest });
        if (manifestCorrupt) decision = { run: true, reason: `manifest corrupt: ${manifestCorrupt}` };
      }

      const rec = {
        kind: step.kind,
        phase: step.kind === "phase" ? step.phase : null,
        invocation: step.invocation,
        decision: decision.run ? "run" : "skip",
        reason: decision.reason,
      };
      report.steps.push(rec);

      if (!decision.run) {
        skipped += 1;
        console.log(`driver: SKIP phase${step.phase} (${decision.reason})`);
        continue;
      }

      const code = execLine(step.invocation, repoRoot, args.dryRun);
      rec.exitCode = code;
      if (code !== 0) {
        report.result = `${step.kind === "phase" ? "phase" + step.phase : "integrity"} failed (exit ${code})`;
        writeReport();
        console.error(`driver: ABORT: ${report.result}; skip report: ${reportPath}`);
        process.exit(code);
      }
      ran += 1;

      if (step.kind === "phase" && !args.dryRun) {
        // Refresh the manifest entry from post-run disk state. A failure
        // here must not abort the pass: drop the entry (fail-closed -- the
        // phase will simply run next time) and continue.
        const key = String(step.phase);
        try {
          const { entry } = buildEntry({
            repoRoot,
            step,
            registryPhaseSet,
            manifest,
            previousEntry: manifest.phases[key],
          });
          manifest.phases[key] = entry;
        } catch (err) {
          delete manifest.phases[key];
          console.error(`driver: WARNING: manifest update failed for phase${step.phase}: ${err.message}`);
        }
        // Save after every phase (crash-resilient; also rebuilds a corrupt
        // manifest from scratch as the pass progresses).
        try {
          saveManifest(manifestPath, manifest);
        } catch (err) {
          console.error(`driver: WARNING: manifest save failed: ${err.message}`);
        }
      }
    }

    report.result = `ok (${ran} step(s) ran, ${skipped} skipped)`;
    report.finishedAt = new Date().toISOString();
    writeReport();
    console.log(`driver: ${report.result}`);
    console.log(`driver: skip report: ${reportPath}`);
  } catch (err) {
    report.result = `driver error: ${err.message}`;
    report.finishedAt = new Date().toISOString();
    writeReport();
    console.error(`driver: FATAL: ${err.message}`);
    process.exit(1);
  }
}

main();
