"use strict";

// Prospective TENSOR-node/input-edge contributions for the geometry-dependent
// branches only, in the fixed fourteen-dimensional recipes. This is NOT a
// complete context envelope or a way to authenticate caller-supplied counts.
// No geometry, scalar coefficients, producer DAG or scientific output is read.
// Unconditional work, imports, call multiplicities, marks, parameter storage,
// scalar expressions, serialization and retained lifetimes require separate proof.
const EVENT_LIMITS = Object.freeze({
  frameNonzeros: 196, deltaFrameNonzeros: 196, framePartialNonzeros: 2744,
  nativeCoefficientEvents: 35672, nativeChangedEvents: 35672, nativeMetricEvents: 35672
});
const need = (ok, message) => { if (!ok) throw new Error("A68 branch topology: " + message); };
function snapshotCounts(value) {
  need(value !== null && typeof value === "object" && !Array.isArray(value), "count record");
  const descriptors = Object.getOwnPropertyDescriptors(value), keys = Reflect.ownKeys(descriptors);
  need(keys.length === Object.keys(EVENT_LIMITS).length && keys.every(k => Object.hasOwn(EVENT_LIMITS, k)), "exact count fields");
  const result = {};
  for (const [key, limit] of Object.entries(EVENT_LIMITS)) {
    const d = descriptors[key];
    need(d && Object.hasOwn(d, "value"), "own data counts, not accessors");
    const n = d.value;
    need(Number.isSafeInteger(n) && n >= 0 && n <= limit, "bounded count " + key);
    result[key] = n;
  }
  return Object.freeze(result);
}
// Each block describes ONE invocation, not one whole context or run. Lazy-zero
// allowances cover accumulator seeds in that block, not arbitrary zero inputs.
const block = (bodyNodes, inputReferences, lazyZeroNodes) => Object.freeze({
  bodyNodes, inputReferences, lazyZeroNodes, nodeUpperBound: bodyNodes + lazyZeroNodes
});
function deriveBranchTopologyContributions(counts) {
  const events = snapshotCounts(counts);
  const { frameNonzeros: f, deltaFrameNonzeros: d, framePartialNonzeros: p,
    nativeCoefficientEvents: c, nativeChangedEvents: h, nativeMetricEvents: k } = events;
  return Object.freeze({
    schemaVersion: "phase627-conditional-branch-topology-v1", events,
    blocks: Object.freeze({
      // Unit + product + scale + sum: (4,5) per selected frame entry.
      wedgeCoordinate: block(4 * f, 5 * f, 1),
      backgroundBAndDX: block(8 * f, 10 * f, 2),
      // Exterior retains all ordered pairs, including equal indices. The
      // 14+196 unconditional Spin nodes are NOT part of this branch block.
      germDeltaBAndExterior: block(4 * f + 6 * f * f, 5 * f + 7 * f * f, 2),
      movingEuler: block(7 * d + 13 * f, 9 * d + 18 * f, 14),
      // Three independent predicates; do not infer changed/metricChanged
      // from coefficient support. Orientation excludes i==j from all three.
      nativeCurrent: block(6 * c + 2 * h + 2 * k, 9 * c + 3 * h + 3 * k, 588),
      accelerationPartialB: block(4 * p + 56 * f, 5 * p + 70 * f, 14),
      accelerationCovariantWY: block(15 * f, 20 * f, 28)
    }),
    scope: Object.freeze({
      conditionalBranchContributionsOnly: true, sourceCountsAuthenticated: false,
      completeContextEnvelope: false, productionCaptureSufficiency: false,
      fullRuntimeResourceProof: false, scientificExecutionAuthorized: false
    })
  });
}
// A conservative branch-only envelope follows from finite index menus, NOT
// from claiming that a single dense numerical fixture maximizes every branch.
function prospectiveBranchTopologyEnvelope() { return deriveBranchTopologyContributions(EVENT_LIMITS); }
module.exports = { EVENT_LIMITS, deriveBranchTopologyContributions, prospectiveBranchTopologyEnvelope };
