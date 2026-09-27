"use strict";
// Test-only conditional ledger check; fixture values NEVER become production
// resource caps or source geometry proof. Compare the full actual symbolic
// stream against static fixed costs plus its manufactured branch event counts.
const assert = require("node:assert/strict");
const { FIXED, deriveProspectiveContextTopology } = require("../a68-context-topology-envelope");
const { deriveBranchTopologyContributions } = require("../a68-branch-topology-envelope");
function checkFixtureTopology(plan, id, counts) {
  const bound = deriveProspectiveContextTopology(id), branches = deriveBranchTopologyContributions(counts).blocks;
  const inputReferences = plan.nodes.reduce((n, node) => n + node.inputs.length, 0);
  assert.equal(plan.leaves.length, bound.shape.leaves); assert.equal(plan.marks.length, bound.shape.marks);
  assert.ok(plan.nodes.length <= bound.shape.nodes); assert.ok(inputReferences <= bound.shape.inputReferences);
  let nodes = 0, edges = 0, zeros = 0;
  for (const part of bound.composition) {
    const [n, e, z] = FIXED[part.name];
    const b = part.name === "background" ? [branches.backgroundBAndDX]
      : part.name === "germ" ? [branches.germDeltaBAndExterior]
      : part.name === "variation" ? [branches.movingEuler, branches.nativeCurrent]
      : part.name === "acceleration" ? [branches.accelerationPartialB, branches.accelerationCovariantWY] : [];
    nodes += part.times * (n + b.reduce((s, x) => s + x.bodyNodes, 0));
    edges += part.times * (e + b.reduce((s, x) => s + x.inputReferences, 0)); zeros += part.times * z;
  }
  // The bound reserves the largest optional anchor for every germ. Actual
  // source selection chooses one of2 anchors or none, independent of geometry.
  if (/^point[01]\//.test(id)) {
    const selected = id === "point0/m0_j4" ? [4, 3] : id === "point1/m0_j24" ? [2, 1] : [0, 0];
    nodes -= 4 - selected[0]; edges -= 3 - selected[1];
  }
  assert.equal(inputReferences, edges, id + " exact conditional input ledger");
  assert.ok(plan.nodes.length >= nodes && plan.nodes.length <= nodes + zeros, id + " owned-zero interval");
  return bound;
}
module.exports = { checkFixtureTopology };
