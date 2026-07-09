"use strict";
// Input fingerprint per phase: sha256 over a SORTED, LABELED list of
// components. Labels keep component kinds from colliding; sorting makes the
// fingerprint independent of extraction order.
//
// Components (binding design item 3):
//   scheme:  schema version constant
//   invoke:  the exact generator invocation line (incl. env prefix)
//   self:    tree hash of the phase project dir (Program.cs, csproj, any
//            extra sources/data; obj/, bin/, output/ excluded)
//   src:     tree hash per transitively referenced src/ project dir
//   dep:     for each phase-JSON input produced by another run-node:
//            producer phase id + producer's manifest output hash
//   frozen:  path + content hash for static inputs (docs, legacy dir-root
//            JSONs, non-run-node phase outputs); missing files hash as
//            "MISSING" (a deterministic state -- appearing later flips it)
//   scan:    tree hash of each statically resolved scanner root
//   native:  tree hash of native/build for LD_LIBRARY_PATH phases
//   readset: content hash of each recorded-read-set path not already
//            covered by another component (strictly tightens freshness)
//
// Any statically UNRESOLVABLE scanner root (and any interpolated repo-path
// string when no recorded read-set exists) is a forced-run reason: the
// fingerprint cannot vouch for those reads, so the phase always runs until
// the situation is fixed (fail-closed).

const fs = require("fs");
const path = require("path");
const { sha256Hex, hashFileCanonical } = require("./canonical");
const { dirTreeHash } = require("./treehash");
const { extractPhaseInputs } = require("./extract");
const { outputsMapHash } = require("./manifest");
const { SCHEMA_VERSION, NATIVE_BUILD_RELDIR } = require("./config");

const RE_PHASE_OUTPUT = /^studies\/phase(\d+)_[^/]+\/output(\/|$)/;

function hashPathAuto(repoRoot, rel) {
  const abs = path.join(repoRoot, rel);
  let st;
  try {
    st = fs.statSync(abs);
  } catch {
    return "MISSING";
  }
  if (st.isDirectory()) {
    const t = dirTreeHash(abs);
    return t ? "tree:" + t.hash : "MISSING";
  }
  if (st.isFile()) return "file:" + hashFileCanonical(abs);
  return "MISSING";
}

// step: registry phase step. registryPhaseSet: Set<number> of phases the
// generator script runs. manifest: current manifest object (for producer
// output hashes). entry: this phase's existing manifest entry or undefined
// (source of recordedInputs).
function computeFingerprint({ repoRoot, step, registryPhaseSet, manifest, entry }) {
  const extraction = extractPhaseInputs(repoRoot, step.project);
  const components = [];
  const forcedRunReasons = [];

  components.push(`scheme:${SCHEMA_VERSION}`);
  components.push(`invoke:${step.invocation}`);

  // self
  const selfTree = dirTreeHash(path.join(repoRoot, extraction.projectDirRel), {
    excludeDirNames: new Set(["obj", "bin", "output", ".git"]),
  });
  if (!selfTree) throw new Error(`fingerprint: phase project dir missing: ${extraction.projectDirRel}`);
  components.push(`self:${extraction.projectDirRel}:${selfTree.hash}`);

  // src project references (transitive)
  for (const srcDir of extraction.srcRefs) {
    const t = dirTreeHash(path.join(repoRoot, srcDir));
    components.push(`src:${srcDir}:${t ? t.hash : "MISSING"}`);
    if (!t) forcedRunReasons.push(`missing src project dir: ${srcDir}`);
  }

  // path literals -> dep or frozen
  const coveredPrefixes = [extraction.projectDirRel];
  for (const rel of extraction.pathLiterals) {
    const m = rel.match(RE_PHASE_OUTPUT);
    const producer = m ? parseInt(m[1], 10) : null;
    if (producer !== null && producer !== step.phase && registryPhaseSet.has(producer)) {
      const producerEntry = manifest && manifest.phases ? manifest.phases[String(producer)] : undefined;
      if (producerEntry && producerEntry.outputs && producerEntry.outputs[rel] !== undefined) {
        components.push(`dep:${rel}:phase${producer}:out:${producerEntry.outputs[rel]}`);
      } else if (producerEntry) {
        components.push(`dep:${rel}:phase${producer}:map:${outputsMapHash(producerEntry)}`);
      } else {
        components.push(`dep:${rel}:phase${producer}:NOENTRY`);
      }
    } else {
      components.push(`frozen:${rel}:${hashPathAuto(repoRoot, rel)}`);
    }
    coveredPrefixes.push(rel);
  }

  // scanner roots
  for (const rel of extraction.scanRoots) {
    components.push(`scan:${rel}:${hashPathAuto(repoRoot, rel)}`);
    coveredPrefixes.push(rel);
  }
  for (const expr of extraction.unresolvedScanRoots) {
    components.push(`scan-unresolved:${expr}`);
    forcedRunReasons.push(`unresolvable scanner root: ${expr}`);
  }

  // native library inputs
  if ((step.env && step.env.LD_LIBRARY_PATH) || /(^|\s)LD_LIBRARY_PATH=/.test(step.invocation)) {
    components.push(`native:${NATIVE_BUILD_RELDIR}:${hashPathAuto(repoRoot, NATIVE_BUILD_RELDIR)}`);
    coveredPrefixes.push(NATIVE_BUILD_RELDIR);
  }

  // recorded read-set (real reads captured under instrumentation)
  const recorded = (entry && entry.recordedInputs) || [];
  for (const srcDir of extraction.srcRefs) coveredPrefixes.push(srcDir);
  for (const rel of recorded) {
    if (isCovered(rel, coveredPrefixes)) continue;
    components.push(`readset:${rel}:${hashPathAuto(repoRoot, rel)}`);
  }

  // interpolated repo paths are statically invisible; only a recorded
  // read-set can vouch for them (with the residual new-file caveat noted in
  // the README).
  if (extraction.interpolatedPathHints.length > 0 && recorded.length === 0) {
    for (const hint of extraction.interpolatedPathHints) {
      forcedRunReasons.push(`interpolated repo path without recorded read-set: ${hint}`);
    }
  }

  components.sort();
  const fingerprint = sha256Hex(components.join("\n") + "\n");
  return { fingerprint, components, forcedRunReasons, extraction, selfTreeHash: selfTree.hash };
}

function isCovered(rel, prefixes) {
  for (const p of prefixes) {
    if (rel === p || rel.startsWith(p + "/")) return true;
  }
  return false;
}

module.exports = { computeFingerprint };
