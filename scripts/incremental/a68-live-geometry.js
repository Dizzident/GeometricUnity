"use strict";

// Immediate source comparison plus later recorded-metadata binding. This does
// NOT replace the adapter's full pinned source/numerical replay. Quotas cover
// logical snapshots, comparisons and canonical bytes, not native memory/RSS.
const crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const { compareRecordedGeometry } = require("./a68-geometry-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 live geometry: " + why); };
const integer = n => Number.isSafeInteger(n) && !Object.is(n, -0);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) &&
    JSON.stringify(Reflect.ownKeys(value)) === JSON.stringify(names), "closed ordered fields");
  return Object.fromEntries(names.map(name => {
    const d = Object.getOwnPropertyDescriptor(value, name);
    need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value];
  }));
}
function caps(value, names) {
  const result = fields(value, names);
  need(Object.values(result).every(n => integer(n) && n > 0), "positive explicit finite quotas"); return Object.freeze(result);
}
function createLiveGeometryValidation(a, slots, limits) {
  limits = fields(limits, ["metadata", "comparison", "wireBytes", "totalWireBytes"]);
  const metadata = caps(limits.metadata, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  const comparison = caps(limits.comparison, ["coordinateVisits", "sparseRecords", "tensorRecords", "rationalCharacters"]);
  need(metadata.maxDepth <= 64 && [limits.wireBytes, limits.totalWireBytes].every(n => integer(n) && n > 0), "bounded metadata depth and wire quotas");
  limits = Object.freeze({ metadata, comparison, wireBytes: limits.wireBytes, totalWireBytes: limits.totalWireBytes });
  need(Array.isArray(slots), "ordered slot array");
  const length = Object.getOwnPropertyDescriptor(slots, "length")?.value;
  need(integer(length) && length >= 0 && length <= 5 && Reflect.ownKeys(slots).length === length + 1, "at most five dense slots");
  const owned = [], paths = new Set();
  for (let i = 0; i < length; i++) {
    const d = Object.getOwnPropertyDescriptor(slots, String(i));need(d && Object.hasOwn(d, "value"), "own slot data");
    const slot = fields(d.value, ["metadataPath", "binding"]);
    need(typeof slot.metadataPath === "string" && /^[\x21-\x7e]+$/.test(slot.metadataPath) &&
      slot.metadataPath.length <= metadata.stringCharacters && !paths.has(slot.metadataPath), "bounded unique metadata path");
    boundGeometryIdentity(a, slot.binding);paths.add(slot.metadataPath);
    owned.push({ metadataPath: slot.metadataPath, binding: slot.binding, commitment: null });
  }
  need(length > 0 || a === null, "empty geometry has no algebra input");
  slots = null;
  let failed = false, completed = false, busy = false, prechecked = 0, bound = 0, wireBytes = 0;
  const snapshotUsage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  const comparisonUsage = Object.fromEntries(Object.keys(comparison).map(k => [k, 0]));
  // Keep the bounded source-algebra health dependency after completion. It is
  // not observed producer metadata; a later arithmetic failure revokes health.
  function clear() { for (const slot of owned) { slot.binding = null; slot.commitment = null; } }
  function poison() { failed = true; clear(); }
  function healthy() { need(!failed && (a === null || a.snapshot().failed === false), "healthy live geometry and source algebra");need(!failed, "swallowed health reentry"); }
  function run(action) {
    if (busy || failed || completed) { poison();need(false, "failed reentrant or finished live geometry"); }
    busy = true;
    try { healthy();const result = action();healthy();return result; }
    catch (error) { poison();throw error; }
    finally { busy = false; }
  }
  function capture(value) {
    const remaining = Object.fromEntries(Object.keys(snapshotUsage).map(k => [k, metadata[k] - snapshotUsage[k]]));remaining.maxDepth = metadata.maxDepth;
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const k of Object.keys(snapshotUsage)) snapshotUsage[k] += copy.usage[k];
    healthy();return copy.value;
  }
  function commitment(value) {
    // Count exact JSON+LF BEFORE stringify, hash allocation, or byte debit.
    const ceiling = Math.min(limits.wireBytes, limits.totalWireBytes - wireBytes);let bytes = 0;
    function charge(n) { need(integer(n) && n >= 0 && n <= ceiling - bytes, "prospective canonical wire byte ceiling");bytes += n; }
    function string(s) { charge(2);charge(s.length);for (const c of s) if (c === '"' || c === "\\") charge(1); }
    function visit(x) {
      if (x === null) charge(4);
      else if (typeof x === "boolean") charge(x ? 4 : 5);
      else if (typeof x === "number") charge(String(x).length);
      else if (typeof x === "string") string(x);
      else if (Array.isArray(x)) { charge(2);if (x.length) charge(x.length - 1);for (const v of x) visit(v); }
      else { const keys = Object.keys(x);charge(2);if (keys.length) charge(keys.length - 1);
        for (const key of keys) { string(key);charge(1);visit(x[key]); } }
    }
    visit(value);charge(1);healthy();wireBytes += bytes;
    const text = JSON.stringify(value) + "\n";need(text.length === bytes, "exact canonical ASCII serialization census");healthy();
    const sha256 = crypto.createHash("sha256").update(text, "ascii").digest("hex");healthy();
    return Object.freeze({ bytes, sha256 });
  }
  function report(slot, value) { return Object.freeze({ metadataPath: slot.metadataPath, ...value, scientificExecutionAuthorized: false }); }
  healthy();
  return Object.freeze({
    precheck: (metadataPath, observed) => run(() => {
      need(bound === 0 && prechecked < length && owned[prechecked].metadataPath === metadataPath, "one ordered live geometry precheck");
      const slot = owned[prechecked], copy = capture(observed), value = commitment(copy);
      const remaining = Object.fromEntries(Object.keys(comparisonUsage).map(k => [k, comparison[k] - comparisonUsage[k]]));
      const usage = compareRecordedGeometry(a, slot.binding, copy, remaining);healthy();
      for (const k of Object.keys(comparisonUsage)) {
        need(integer(usage[k]) && usage[k] >= 0 && usage[k] <= remaining[k], "bounded source comparison usage");comparisonUsage[k] += usage[k];
      }
      slot.commitment = value;slot.binding = null;prechecked++;return report(slot, value);
    }),
    bindRecorded: (metadataPath, observed) => run(() => {
      need(prechecked === length && bound < length && owned[bound].metadataPath === metadataPath, "complete live prechecks before ordered recorded binding");
      const slot = owned[bound], value = commitment(capture(observed));
      need(value.bytes === slot.commitment.bytes && value.sha256 === slot.commitment.sha256, "recorded metadata equals accepted live geometry");
      bound++;return report(slot, value);
    }),
    requireComplete: () => run(() => { need(prechecked === length, "complete immediate geometry census"); }),
    finish: () => run(() => { need(prechecked === length && bound === length, "complete recorded geometry census");healthy();completed = true;clear(); }),
    abort: () => { poison(); },
    snapshot: () => {
      if (busy) poison();
      else { busy = true;try { healthy(); } catch { poison(); } finally { busy = false; } }
      return Object.freeze({ failed, completed: completed && !failed, slots: length, prechecked, bound,
        retainedCommitments: owned.filter(slot => slot.commitment !== null).length, snapshotUsage: Object.freeze({ ...snapshotUsage }),
        comparisonUsage: Object.freeze({ ...comparisonUsage }), wireBytes,
        scope: Object.freeze({ laterFullSourceComparisonRequired: true, liveParentImmutabilityEstablished: false,
          totalProcessMemoryProved: false, scientificExecutionAuthorized: false }) });
    }
  });
}
module.exports = { createLiveGeometryValidation };
