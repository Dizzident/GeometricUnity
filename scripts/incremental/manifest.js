"use strict";
// Manifest IO. The manifest is the load-bearing freshness guarantee:
// scripts/boson_incremental_manifest.json, committed.
//
// Shape:
// {
//   "schemaVersion": 1,
//   "phases": {
//     "330": {
//       "schemaVersion": 1,
//       "inputFingerprint": "<sha256>",
//       "recordedInputs": ["repo/relative/path", ...],        // real read-set (optional until captured)
//       "readsetCapturedForSelfHash": "<sha256>",             // self tree hash at capture time (optional)
//       "outputs": { "studies/.../output/x.json": "<canonical sha256>", ... }
//     }, ...
//   }
// }

const fs = require("fs");
const path = require("path");
const { sha256Hex } = require("./canonical");
const { SCHEMA_VERSION } = require("./config");

function emptyManifest() {
  return { schemaVersion: SCHEMA_VERSION, phases: {} };
}

// Load the manifest. A missing file yields an empty manifest (everything
// runs). A CORRUPT file throws -- callers treat that as fail-closed (the
// driver catches per-phase and runs everything, loudly).
function loadManifest(absPath) {
  if (!fs.existsSync(absPath)) return emptyManifest();
  const text = fs.readFileSync(absPath, "utf8");
  const parsed = JSON.parse(text); // throws on corruption
  if (typeof parsed !== "object" || parsed === null || typeof parsed.phases !== "object") {
    throw new Error(`manifest: ${absPath} does not have the expected shape`);
  }
  return parsed;
}

// Atomic save (tmp + rename), stable key order for reviewable diffs.
function saveManifest(absPath, manifest) {
  const ordered = {
    schemaVersion: manifest.schemaVersion,
    phases: {},
  };
  for (const key of Object.keys(manifest.phases).sort((a, b) => Number(a) - Number(b))) {
    const e = manifest.phases[key];
    ordered.phases[key] = {
      schemaVersion: e.schemaVersion,
      inputFingerprint: e.inputFingerprint,
      ...(e.recordedInputs ? { recordedInputs: [...e.recordedInputs].sort() } : {}),
      ...(e.readsetCapturedForSelfHash ? { readsetCapturedForSelfHash: e.readsetCapturedForSelfHash } : {}),
      outputs: sortObject(e.outputs || {}),
    };
  }
  const tmp = absPath + ".tmp";
  fs.mkdirSync(path.dirname(absPath), { recursive: true });
  fs.writeFileSync(tmp, JSON.stringify(ordered, null, 2) + "\n");
  fs.renameSync(tmp, absPath);
}

function sortObject(obj) {
  const out = {};
  for (const k of Object.keys(obj).sort()) out[k] = obj[k];
  return out;
}

// Deterministic hash of a phase entry's outputs map; used as the "dep"
// fingerprint component for consumers when the exact file is not listed.
function outputsMapHash(entry) {
  const outputs = (entry && entry.outputs) || {};
  const lines = Object.keys(outputs)
    .sort()
    .map((k) => k + "\n" + outputs[k] + "\n");
  return sha256Hex(lines.join(""));
}

module.exports = { emptyManifest, loadManifest, saveManifest, outputsMapHash };
