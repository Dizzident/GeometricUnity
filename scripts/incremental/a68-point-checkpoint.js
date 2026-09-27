"use strict";

// Read-only point computational checkpoint, NOT a completed point traversal.
// Observed hashes bind bytes; independent source reconstruction/replay still
// decides whether those bytes describe the intended scientific calculation.
const { readPinnedArtifactPair } = require("./a68-pinned-artifacts");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { validateGeometryWireIdentity } = require("./a68-geometry-metadata");
const { verifyMixedDag } = require("./a68-tensor-replay");
const { createExpandedTensorVerifier } = require("./a68-expanded-tensor-files");
const identities = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 point checkpoint: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => {
    const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value];
  }));
}
function pointCheckNames(point) {
  need(point === 0 || point === 1, "point0 or point1");
  return [...Array.from({ length: 14 }, (_, mu) => "nativeFirstJet_" + mu), "covariantExterior",
    ...(point === 0 ? ["nativeFirstJetHandAnchor", "nativeExteriorHandAnchor", "nativeVectorAnchorPositive"] : [])];
}
function readPointCheckpoint(outputRoot, declaration, observed, limits) {
  declaration = fields(declaration, ["contextId", "graphPath", "metadataPath", "marks"]);
  need(["point0", "point1"].includes(declaration.contextId), "point checkpoint context");
  const pair = readPinnedArtifactPair(outputRoot, declaration, observed, limits);
  const { root, pins, graph, metadata: checkpoint, snapshotUsage } = pair; declaration = pair.declaration;
  fields(checkpoint, ["schema", "context", "status", "pointTraversalComplete", "graph", "metadata"]);
  need(checkpoint.schema === "phase627-point-background-checkpoint-v1" && checkpoint.context === declaration.contextId &&
    checkpoint.status === "background-sealed" && checkpoint.pointTraversalComplete === false, "sealed computation but incomplete traversal");
  need(same(checkpoint.graph, pins.graph), "checkpoint names exactly the same graph artifact");
  const point = declaration.contextId === "point0" ? 0 : 1, names = pointCheckNames(point);
  fields(checkpoint.metadata, ["background/geometry", ...names.map(name => "check/" + name)]);
  validateGeometryWireIdentity("baseline", { point }, checkpoint.metadata["background/geometry"]);
  need(names.every(name => checkpoint.metadata["check/" + name] === true), "complete producer check declarations (NOT independent proof)");
  const token = Object.freeze({ schemaVersion: "phase627-pinned-point-checkpoint-v1", contextId: declaration.contextId });
  identities.set(token, Object.freeze({ root, declaration, pins, graph, checkpoint, snapshotUsage,
    scope: Object.freeze({ sameBufferHashesChecked: true, numericalReplayComplete: false, sourceAuthenticityEstablished: false,
      expandedTensorArtifactsVerified: false, pointTraversalComplete: false, totalProcessMemoryProved: false, atomicParentPathProtectionProved: false }) }));
  return token;
}
function pointCheckpointIdentity(token) {
  const identity = identities.get(token); need(identity, "private read-pinned checkpoint identity required"); return identity;
}

// Generic numerical bridge. Only the source adapter can authenticate expected
// recipes/leaves/geometry. Manufactured callers may test this without minting
// a scientific source brand. Expanded tensor files must match the independently
// replayed values before this bridge returns any numerical export receipt.
function replayPointCheckpoint(token, expected, limits, resolveLeaf) {
  const pinned = pointCheckpointIdentity(token);
  limits = fields(limits, ["tensor", "scalarSchedule", "scalarArithmetic", "scalarReplay", "wire", "consumerSchedule", "consumerArithmetic", "consumerReplay", "exports", "expanded", "snapshot"]);
  const copy = snapshotCanonicalMetadata({ expected, limits: {
    tensor: limits.tensor, scalarSchedule: limits.scalarSchedule, scalarArithmetic: limits.scalarArithmetic, scalarReplay: limits.scalarReplay,
    wire: limits.wire, consumerSchedule: limits.consumerSchedule, consumerArithmetic: limits.consumerArithmetic, consumerReplay: limits.consumerReplay, exports: limits.exports, expanded: limits.expanded
  } }, limits.snapshot).value;
  expected = fields(copy.expected, ["contextId", "tensorPlan", "namedRoots", "geometry", "checks", "domainChecks", "error", "requests"]); const cap = copy.limits;
  need(expected.contextId === pinned.declaration.contextId && expected.namedRoots.length === 0 && expected.error === null && typeof resolveLeaf === "function", "point-only independent recipe");
  need(same(expected.checks.map(c => c.name), pointCheckNames(expected.contextId === "point0" ? 0 : 1)), "all independent point checks");
  need(Array.isArray(pinned.declaration.marks) && pinned.declaration.marks.length === expected.tensorPlan.marks.length &&
    expected.tensorPlan.marks.every(mark => pinned.declaration.marks.some(m => m.name === mark.name && m.degree === mark.degree && m.expanded === mark.expanded)), "complete frozen retention menu agrees with independent recipe");
  const expanded = createExpandedTensorVerifier(pinned.root, { contextId: expected.contextId, marks: pinned.declaration.marks,
    reservedPaths: [pinned.declaration.graphPath, pinned.declaration.metadataPath] }, cap.expanded);
  const metadata = pinned.checkpoint.metadata;
  const replay = verifyMixedDag(pinned.graph, expected.tensorPlan.leaves, expected.tensorPlan.marks, cap.tensor, resolveLeaf,
    (node, i) => ["id", "op", "degree", "inputs", "parameters"].every(key => same(node[key], expected.tensorPlan.nodes[i][key])),
    (mark, rows) => expected.tensorPlan.marks.some(m => m.name === mark.name && m.degree === mark.degree && m.node === mark.node && m.expanded === mark.expanded) && expanded.compareMark(mark, rows),
    { tensorPlan: expected.tensorPlan, namedRoots: expected.namedRoots, geometry: expected.geometry, scheduleLimits: cap.scalarSchedule,
      arithmeticLimits: cap.scalarArithmetic, replayLimits: cap.scalarReplay, wireLimits: cap.wire,
      compareRoot: () => { throw new Error("point checkpoint has no scalar roots"); } },
    { checks: expected.checks, domainChecks: expected.domainChecks, error: null, scheduleLimits: cap.consumerSchedule,
      arithmeticLimits: cap.consumerArithmetic, replayLimits: cap.consumerReplay,
      resolveGeometryField: () => { throw new Error("point checkpoint has no external geometry predicates"); },
      resolveSourceScalar: () => { throw new Error("point checkpoint has no external scalar predicates"); },
      compareConsumer: (consumer, value) => value === true && (consumer.kind === "domain" || consumer.kind === "check" && metadata[consumer.descriptor.metadataPath] === value) },
    { contextId: expected.contextId, requests: expected.requests, limits: cap.exports });
  return Object.freeze({ ...replay, expandedArtifacts: expanded.finish() });
}
module.exports = { pointCheckNames, readPointCheckpoint, pointCheckpointIdentity, replayPointCheckpoint };
