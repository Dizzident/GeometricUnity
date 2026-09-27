"use strict";

// Generic complete numerical checkpoint bridge. Expected recipes and all
// source resolvers must come from independent source adapters in production.
// Manufactured fixtures may exercise this bridge but cannot mint source proof.
const { contextCheckpointIdentity } = require("./a68-context-checkpoint");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { createContextMetadataComparator } = require("./a68-context-metadata");
const { createExpandedTensorVerifier } = require("./a68-expanded-tensor-files");
const { verifyMixedDag } = require("./a68-tensor-replay");
const attempts = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 context replay: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
function freeze(value) {
  if (value && typeof value === "object" && !Object.isFrozen(value)) { Object.values(value).forEach(freeze); Object.freeze(value); } return value;
}
function replayContextCheckpoint(token, expected, limits, resolvers) {
  const pinned = contextCheckpointIdentity(token);
  let state = attempts.get(token);
  if (state) { state.failed = true; throw new Error("A68 context replay: failed, reentrant or completed checkpoint attempt"); }
  state = { failed: false }; attempts.set(token, state);
  try {
    limits = fields(limits, ["tensor", "scalarSchedule", "scalarArithmetic", "scalarReplay", "wire", "consumerSchedule", "consumerArithmetic", "consumerReplay", "metadata", "expanded", "snapshot"]);
    const copy = snapshotCanonicalMetadata({ expected, limits: {
      tensor: limits.tensor, scalarSchedule: limits.scalarSchedule, scalarArithmetic: limits.scalarArithmetic, scalarReplay: limits.scalarReplay,
      wire: limits.wire, consumerSchedule: limits.consumerSchedule, consumerArithmetic: limits.consumerArithmetic, consumerReplay: limits.consumerReplay,
      metadata: limits.metadata, expanded: limits.expanded
    } }, limits.snapshot).value;
    expected = fields(copy.expected, ["contextId", "tensorPlan", "namedRoots", "geometry", "checks", "domainChecks", "error", "structured", "scalarArrays", "geometryPaths"]);
    const cap = copy.limits;
    resolvers = fields(resolvers, ["compareGeometryMetadata", "resolveLeaf", "resolveGeometryField", "resolveSourceScalar"]);
    need(!state.failed && Object.values(resolvers).every(f => typeof f === "function"), "healthy snapshot and mandatory independent callbacks");
    need(expected.contextId === pinned.declaration.contextId, "same independent context identity");
    const marks = pinned.declaration.marks, plan = expected.tensorPlan;
    need(Array.isArray(marks) && marks.length === plan.marks.length && plan.marks.every(mark =>
      marks.some(m => m.name === mark.name && m.degree === mark.degree && m.expanded === mark.expanded)), "complete frozen retention menu agrees with independent recipe");
    const metadataCap = fields(cap.metadata, ["snapshot", "results"]);
    const metadata = createContextMetadataComparator(expected, pinned.envelope.metadata, {
      snapshot: metadataCap.snapshot, scalarSchedule: cap.scalarSchedule, consumerSchedule: cap.consumerSchedule, results: metadataCap.results
    });
    const expanded = createExpandedTensorVerifier(pinned.root, { contextId: expected.contextId, marks,
      reservedPaths: [pinned.declaration.graphPath, pinned.declaration.metadataPath] }, cap.expanded);
    function call(callback, ...args) {
      need(!state.failed, "poisoned checkpoint before callback");
      const value = callback(...args); need(!state.failed, "swallowed checkpoint reentry"); return value;
    }
    // Even an empty geometry menu is delivered and must be checked. A value,
    // Promise or thenable is not completed synchronous geometry validation.
    need(call(resolvers.compareGeometryMetadata, metadata.geometryMetadata) === undefined, "synchronous void geometry comparison required");
    const replay = verifyMixedDag(pinned.graph, plan.leaves, plan.marks, cap.tensor,
      specification => call(resolvers.resolveLeaf, specification),
      (node, i) => ["id", "op", "degree", "inputs", "parameters"].every(key => same(node[key], plan.nodes[i][key])),
      (mark, rows) => plan.marks.some(m => m.name === mark.name && m.degree === mark.degree && m.node === mark.node && m.expanded === mark.expanded) && expanded.compareMark(mark, rows),
      { tensorPlan: plan, namedRoots: expected.namedRoots, geometry: expected.geometry, scheduleLimits: cap.scalarSchedule,
        arithmeticLimits: cap.scalarArithmetic, replayLimits: cap.scalarReplay, wireLimits: cap.wire, compareRoot: metadata.compareRoot },
      { checks: expected.checks, domainChecks: expected.domainChecks, error: expected.error, scheduleLimits: cap.consumerSchedule,
        arithmeticLimits: cap.consumerArithmetic, replayLimits: cap.consumerReplay,
        resolveGeometryField: name => call(resolvers.resolveGeometryField, name),
        resolveSourceScalar: name => call(resolvers.resolveSourceScalar, name), compareConsumer: metadata.compareConsumer });
    const metadataComparison = metadata.finish(), expandedArtifacts = expanded.finish();
    need(!state.failed, "healthy complete checkpoint replay");
    return freeze({ contextId: expected.contextId, replay, metadataComparison, expandedArtifacts,
      scope: { sameBufferHashesChecked: true, numericalReplayComplete: true, completeScalarCheckMetadataCompared: true,
        expandedTensorArtifactsVerified: true, geometryComparisonCallbackCompleted: true,
        sourceAuthenticityEstablished: false, independentContextAccepted: false, upstreamCertificateProofEstablished: false,
        totalProcessMemoryProved: false, scientificExecutionAuthorized: false } });
  } catch (error) { state.failed = true; throw error; }
}
module.exports = { replayContextCheckpoint };
