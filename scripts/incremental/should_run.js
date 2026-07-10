"use strict";
// The fail-closed skip rule (binding design item 4).
//
// Skip phase P IFF:
//   - P is not in the always-run set, AND
//   - the manifest entry exists with a matching schemaVersion, AND
//   - the freshly computed inputFingerprint matches the recorded one, AND
//   - nothing about the fingerprint computation raised doubt
//     (unresolvable scanner roots, uncovered interpolated paths), AND
//   - EVERY manifest-listed output exists with a matching canonical hash, AND
//   - EVERY recordedInput resolves (exists on disk).
//
// ANY mismatch, missing file, parse error, unknown phase, or exception
// results in RUN. This function never throws.

const fs = require("fs");
const path = require("path");
const { hashFileCanonical } = require("./canonical");
const { computeFingerprint } = require("./fingerprint");
const { SCHEMA_VERSION, ALWAYS_RUN_PHASES } = require("./config");

const ALWAYS_RUN_SET = new Set(ALWAYS_RUN_PHASES);

// Returns { run: boolean, reason: string }
function decide({ repoRoot, step, registryPhaseSet, manifest }) {
  try {
    if (step.kind !== "phase") return { run: true, reason: "non-phase step always runs" };
    if (ALWAYS_RUN_SET.has(step.phase)) return { run: true, reason: "always-run set" };

    const entry = manifest && manifest.phases ? manifest.phases[String(step.phase)] : undefined;
    if (!entry) return { run: true, reason: "no manifest entry" };
    if (entry.schemaVersion !== SCHEMA_VERSION) {
      return { run: true, reason: `schemaVersion mismatch (manifest ${entry.schemaVersion}, code ${SCHEMA_VERSION})` };
    }
    if (typeof entry.inputFingerprint !== "string" || !entry.outputs || typeof entry.outputs !== "object") {
      return { run: true, reason: "manifest entry malformed" };
    }

    let fp;
    try {
      fp = computeFingerprint({ repoRoot, step, registryPhaseSet, manifest, entry });
    } catch (err) {
      return { run: true, reason: `fingerprint error: ${err.message}` };
    }
    if (fp.forcedRunReasons.length > 0) {
      return { run: true, reason: `fail-closed: ${fp.forcedRunReasons[0]}` };
    }
    if (fp.fingerprint !== entry.inputFingerprint) {
      return { run: true, reason: "inputFingerprint mismatch" };
    }

    for (const [rel, expected] of Object.entries(entry.outputs)) {
      const abs = path.join(repoRoot, rel);
      if (!fs.existsSync(abs)) return { run: true, reason: `output missing: ${rel}` };
      let actual;
      try {
        actual = hashFileCanonical(abs);
      } catch (err) {
        return { run: true, reason: `output unreadable: ${rel}: ${err.message}` };
      }
      if (actual !== expected) return { run: true, reason: `output hash mismatch: ${rel}` };
    }

    for (const rel of entry.recordedInputs || []) {
      if (!fs.existsSync(path.join(repoRoot, rel))) {
        return { run: true, reason: `recorded input missing: ${rel}` };
      }
    }

    return { run: false, reason: "fingerprint, outputs, and recorded inputs all match" };
  } catch (err) {
    return { run: true, reason: `exception during skip decision: ${err.message}` };
  }
}

module.exports = { decide, ALWAYS_RUN_SET };
