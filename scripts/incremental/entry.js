"use strict";
// Build (or rebuild) one phase's manifest entry from on-disk state.
// Used by the seeder (after a completed full pass) and by the driver
// (after each successful phase run).

const fs = require("fs");
const path = require("path");
const { hashFileCanonical } = require("./canonical");
const { computeFingerprint } = require("./fingerprint");
const { SCHEMA_VERSION } = require("./config");

// Enumerate every file under the phase's own output dir (recursive) and
// canonically hash it. A missing output dir yields an empty map.
function enumerateOutputs(repoRoot, projectDirRel) {
  const outDirRel = projectDirRel + "/output";
  const outDirAbs = path.join(repoRoot, outDirRel);
  const outputs = {};
  if (!fs.existsSync(outDirAbs)) return outputs;
  const stack = [outDirAbs];
  while (stack.length) {
    const d = stack.pop();
    for (const name of fs.readdirSync(d).sort()) {
      const abs = path.join(d, name);
      const st = fs.statSync(abs);
      if (st.isDirectory()) stack.push(abs);
      else if (st.isFile()) {
        const rel = path.relative(repoRoot, abs).replace(/\\/g, "/");
        outputs[rel] = hashFileCanonical(abs);
      }
    }
  }
  return outputs;
}

// Returns { entry, forcedRunReasons } -- forcedRunReasons non-empty means
// the phase can never skip on this entry (recorded for reporting; the entry
// is still written so the doubt is visible and stable).
function buildEntry({ repoRoot, step, registryPhaseSet, manifest, previousEntry }) {
  const fp = computeFingerprint({ repoRoot, step, registryPhaseSet, manifest, entry: previousEntry });
  const outputs = enumerateOutputs(repoRoot, fp.extraction.projectDirRel);
  const entry = {
    schemaVersion: SCHEMA_VERSION,
    inputFingerprint: fp.fingerprint,
    outputs,
  };
  if (previousEntry && previousEntry.recordedInputs) {
    entry.recordedInputs = previousEntry.recordedInputs;
    if (previousEntry.readsetCapturedForSelfHash) {
      entry.readsetCapturedForSelfHash = previousEntry.readsetCapturedForSelfHash;
    }
  }
  return { entry, forcedRunReasons: fp.forcedRunReasons, selfTreeHash: fp.selfTreeHash };
}

module.exports = { buildEntry, enumerateOutputs };
