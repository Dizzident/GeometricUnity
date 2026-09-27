"use strict";
// Symbolic expression metadata only: synthetic geometry and opaque leaves.
// These fixtures neither evaluate the action nor certify a physical result.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { buildMixedVariation } = require("../a68-variation-recipe");
const fourteen = x => Array.from({ length: 14 }, () => x);
const constants = ["0", "1", "-1", "-1/2", "1/2", "1/3", "907712"];
function fixture(asymmetric = false, deltaSigns = null, frameEntries = null) {
  const degrees = { X: 1, B: 1, F: 2, DX: 2, Q: 2, AdjointX: 2, DeltaB: 1, DeltaFFixed: 2, DeltaFAdapted: 2 };
  for (let p = 0; p < 3; p++) degrees["K" + p] = 1;
  for (let p = 1; p < 4; p++) degrees["G" + p] = 1;
  for (let z = 0; z < 14; z++) degrees["Cov" + z] = 1;
  const matrices = [
    { name: "identity", matrix: Array.from({ length: 14 }, (_, i) => ({ row: i, column: i, value: "1" })) },
    { name: "zero", matrix: [] },
    { name: "partial", matrix: [{ row: 0, column: 0, value: "2/3" }] },
    { name: "delta", matrix: [{ row: 0, column: 0, value: "3/5" }] },
    { name: "lift", matrix: [{ row: 0, column: 0, value: "5/7" }] }
  ];
  if (asymmetric) {
    matrices[0].matrix.push({ row: 0, column: 7, value: "2" }, { row: 7, column: 0, value: "3" });
    matrices[0].matrix.sort((a, b) => a.row * 14 + a.column - b.row * 14 - b.column);
    matrices[2].matrix.push({ row: 0, column: 7, value: "4/5" }, { row: 7, column: 0, value: "5/6" }, { row: 7, column: 7, value: "7/8" });
    matrices[3].matrix.push({ row: 0, column: 7, value: "4/7" }, { row: 7, column: 0, value: "5/9" }, { row: 7, column: 7, value: "7/11" });
  }
  if (deltaSigns) matrices[3].matrix = deltaSigns.map((value, i) => ({ row: i, column: i, value }));
  if (frameEntries) matrices[0].matrix = frameEntries;
  const recipe = new MixedRecipe({ constants, matrices, leaves: Object.entries(degrees).map(([id, degree]) => ({ id, degree, source: "synthetic/" + id, sha256: "0".repeat(64) })) });
  const handles = Object.fromEntries(Object.keys(degrees).map(id => [id, recipe.RegisterLeaf(id)]));
  const zero = recipe.Matrix("zero"), identity = recipe.Matrix("identity");
  const b = { ...handles, Frame: identity, InverseFrame: identity, FramePartial: fourteen(zero), FrameLift: fourteen(zero), Omega: fourteen(zero),
    KInputs: [handles.K0, handles.K1, handles.K2], GradientPieces: [handles.K0, handles.G1, handles.G2, handles.G3], CovariantFrame: Array.from({ length: 14 }, (_, z) => handles["Cov" + z]) };
  b.FramePartial[0] = recipe.Matrix("partial"); b.FrameLift[0] = recipe.Matrix("lift");
  const g = { DeltaB: handles.DeltaB, DeltaFFixed: handles.DeltaFFixed, DeltaFAdapted: handles.DeltaFAdapted,
    Motion: zero, DeltaFrame: recipe.Matrix("delta"), MotionPartial: fourteen(zero), MotionCovariant: fourteen(zero), DeltaOmega: fourteen(zero) };
  return { recipe, b, g };
}
function intermediateDegree(name) {
  const forward = /^fixed_p[0-2]_s([0-7])_/.exec(name), reverse = /^reverse_s([0-8])_/.exec(name);
  return forward ? [2, 12, 13, 14, 0, 1, 13, 1][Number(forward[1])] : reverse ? [1, 13, 2, 1, 0, 14, 12, 2, 2][Number(reverse[1])] : 2;
}
let built;
function complete() {
  if (built) return built;
  const { recipe, b, g } = fixture(), names = [];
  const result = buildMixedVariation(recipe, b, g, (name, tensor) => { names.push(name); recipe.Mark(name, intermediateDegree(name), tensor, true); });
  for (const name of ["Raw0", "Raw2", "FieldFirst0", "FieldFirst2", "Oracle0", "Oracle2", "EulerCovariant", "EulerMoving", "Native0", "NativeEuler"])
    result[name].forEach((value, p) => recipe.Mark(`${name}/${p}`, name.endsWith("2") ? 2 : 1, value, true));
  for (const name of ["CurrentCoefficients", "CurrentPartialCoefficients", "GreenVariationCoefficients"])
    for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) recipe.Mark(`${name}/${mu}/${nu}`, 0, result[name][mu][nu], true);
  for (const name of ["NativeDivergence", "NativeAdjointAsFrame"]) recipe.Mark(name, 1, result[name], true);
  built = { result, names, plan: recipe.Finish() }; return built;
}

test("complete mixed recipe retains108 intermediates in hand-enumerated action order", () => {
  const { names, plan } = complete(), expected = [];
  for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) expected.push(`fixed_p${p}_s${s}_value`, `fixed_p${p}_s${s}_delta`);
  for (let s = 0; s < 9; s++) expected.push(`reverse_s${s}_value`, `reverse_s${s}_delta`);
  for (let z = 0; z < 14; z++) expected.push(`covariant_z${z}`, `covariantOracle_z${z}`);
  for (let mu = 0; mu < 14; mu++) expected.push(`partial_mu${mu}`);
  assert.equal(expected.length, 108); assert.deepEqual(names, expected);
  assert.deepEqual(plan.marks.slice(0, 108).map(m => m.name), expected);
  // Input leaves remain distinct even in this metadata-only zero-motion case.
  assert.equal(plan.leaves.length, 29); assert.equal(plan.nodes.filter(n => n.op === "leaf").length, 29);
});

test("all196 current, differentiated-current and Green slots have distinct result identities", () => {
  const { result, plan } = complete();
  for (const name of ["CurrentCoefficients", "CurrentPartialCoefficients", "GreenVariationCoefficients"]) {
    assert.equal(result[name].length, 14); assert.ok(result[name].every(row => row.length === 14));
    assert.equal(new Set(result[name].flat()).size, 196);
    const marks = plan.marks.filter(m => m.name.startsWith(name + "/"));
    assert.equal(marks.length, 196); assert.equal(new Set(marks.map(m => m.node)).size, 196);
  }
  assert.equal(result.DerivativeIdentityComparisons.length, 14);
  assert.ok(result.DerivativeIdentityComparisons.every(pair => pair.length === 2 && pair[0] !== pair[1]));
});

test("native current signs, frame derivatives, metric derivatives and volume trace are explicit", () => {
  const { plan } = complete(), at = (name, mu, nu) => plan.nodes[plan.marks.find(m => m.name === `${name}/${mu}/${nu}`).node];
  const scale = node => { assert.equal(node.op, "scale"); return node.parameters.real; };
  // Identity frame, mu0/nu7: sigma0*sigma7=-1 and positive orientation.
  const current = at("CurrentCoefficients", 0, 7);
  assert.equal(current.op, "sum"); assert.equal(scale(plan.nodes[current.inputs[1]]), "-1");
  const reversed = at("CurrentCoefficients", 7, 0);
  assert.equal(scale(plan.nodes[reversed.inputs[1]]), "1");
  // Their component source is the SAME canonical 0,7 slice, not two copies.
  assert.equal(plan.nodes[current.inputs[1]].inputs[0], plan.nodes[reversed.inputs[1]].inputs[0]);
  const partial = at("CurrentPartialCoefficients", 0, 7);
  assert.equal(scale(plan.nodes[partial.inputs[1]]), "-5/7", "negative trace of FrameLift[0]");
  assert.equal(plan.nodes[partial.inputs[1]].inputs[0], current.id);
  const beforeVolume = plan.nodes[partial.inputs[0]];
  assert.equal(scale(plan.nodes[beforeVolume.inputs[1]]), "-2/3", "both frame slots differentiated with mu index");
  const green = at("GreenVariationCoefficients", 0, 7);
  assert.equal(scale(plan.nodes[green.inputs[1]]), "-3/10", "metric frame change with original half factor");
  assert.equal(scale(plan.nodes[plan.nodes[green.inputs[0]].inputs[1]]), "-1/2", "adapted adjoint derivative with half factor");
  for (let mu = 0; mu < 14; mu++) assert.equal(at("CurrentCoefficients", mu, mu).op, "zero");
});

test("native divergence retains every mu,nu derivative including diagonal zeros", () => {
  const { plan } = complete();
  let node = plan.nodes[plan.marks.find(m => m.name === "NativeDivergence").node];
  for (let mu = 13; mu >= 0; mu--) for (let nu = 13; nu >= 0; nu--) {
    assert.equal(node.op, "sum"); const wedge = plan.nodes[node.inputs[1]];
    assert.equal(wedge.op, "product"); assert.deepEqual(wedge.parameters, { kind: "W" });
    assert.deepEqual(plan.nodes[wedge.inputs[0]].parameters, { form: 1 << nu, blade: 0, real: "1", imaginary: "0" });
    assert.equal(wedge.inputs[1], plan.marks.find(m => m.name === `CurrentPartialCoefficients/${mu}/${nu}`).node);
    node = plan.nodes[node.inputs[0]];
  }
  assert.equal(node.op, "zero");
});

test("asymmetric frame retains BOTH differentiated slots and signed half Green factors", () => {
  const { recipe, b, g } = fixture(true);
  const result = buildMixedVariation(recipe, b, g, (name, tensor) => recipe.Mark(name, intermediateDegree(name), tensor, true));
  for (const name of ["CurrentCoefficients", "CurrentPartialCoefficients", "GreenVariationCoefficients"]) recipe.Mark(name, 0, result[name][0][7], true);
  const plan = recipe.Finish();
  function orderedScales(name) {
    let node = plan.nodes[plan.marks.find(m => m.name === name).node]; const values = [];
    while (node.op === "sum") {
      const term = plan.nodes[node.inputs[1]]; assert.equal(term.op, "scale"); values.unshift(term.parameters.real);
      node = plan.nodes[node.inputs[0]];
    }
    assert.equal(node.op, "zero"); return values;
  }
  assert.deepEqual(orderedScales("CurrentCoefficients"), ["-1", "6"]);
  assert.deepEqual(orderedScales("CurrentPartialCoefficients"), ["-1", "-37/24", "6", "61/15", "-5/7"]);
  assert.deepEqual(orderedScales("GreenVariationCoefficients"), ["-1/2", "-34/55", "3", "89/63"]);
});

test("mixed construction requires retention and full geometric slot menus", () => {
  const { recipe, b, g } = fixture();
  assert.throws(() => buildMixedVariation(recipe, b, g), /retention callback/);
  assert.throws(() => buildMixedVariation(recipe, { ...b, CovariantFrame: b.CovariantFrame.slice(1) }, g, () => {}), /fourteen/);
  assert.throws(() => buildMixedVariation(recipe, { ...b, KInputs: [] }, g, () => {}), /action pieces/);
});

test("complete symbolic variation is admitted by the analytic resource interpreter", () => {
  const { deriveMixedResourceMajorants } = require("../a68-resource-majorants");
  const { plan } = complete();
  // Bounds on opaque SYNTHETIC leaves only; these are not bounds on P5.
  const leaves = plan.leaves.map(({ id, source, sha256 }) => ({ id, source, sha256, support: "1", denominator: "1", norm: "1" }));
  const report = deriveMixedResourceMajorants(plan, leaves, (actual, declared) => {
    assert.deepEqual(actual, leaves.find(l => l.id === declared.id)); return true;
  });
  assert.equal(report.nodes.length, plan.nodes.length);
  assert.ok(BigInt(report.pairVisits) > 0n); assert.ok(BigInt(report.replayTensorRecordPeak) > 0n);
  assert.equal(report.scope.scientificCoefficientsEvaluated, false);
  assert.equal(report.scope.completeRuntimeResourceProof, false);
});

test("equal frame/delta supports can differ in native topology through exact two-term cancellation", () => {
  const { deriveBranchTopologyContributions } = require("../a68-branch-topology-envelope");
  function measured(deltaSigns) {
    const { recipe, b, g } = fixture(false, deltaSigns), events = [0, 0, 0]; let calls = 0;
    // Observe only the three DIRECT branch predicates. Bind all internal
    // scalar arithmetic to the real recipe so its private calls are excluded.
    const observed = new Proxy(recipe, { get(target, key) {
      const value = Reflect.get(target, key, target);
      if (key === "ScalarValue") return scalar => {
        const result = value.call(target, scalar), slot = calls++ % 3;
        if (result !== "0") events[slot]++; return result;
      };
      return typeof value === "function" ? value.bind(target) : value;
    } });
    const result = buildMixedVariation(observed, b, g, (name, tensor) => recipe.Mark(name, intermediateDegree(name), tensor, true));
    for (const name of ["CurrentCoefficients", "CurrentPartialCoefficients", "GreenVariationCoefficients"])
      for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) recipe.Mark(`${name}/${mu}/${nu}`, 0, result[name][mu][nu], true);
    const plan = recipe.Finish();
    assert.equal(calls, 3 * 14 ** 4);
    const contribution = deriveBranchTopologyContributions({ frameNonzeros: 14, deltaFrameNonzeros: 2, framePartialNonzeros: 1,
      nativeCoefficientEvents: events[0], nativeChangedEvents: events[1], nativeMetricEvents: events[2] });
    return { events, contribution, nodes: plan.nodes.length, edges: plan.nodes.reduce((s, n) => s + n.inputs.length, 0) };
  }
  const positive = measured(["1", "1"]), cancelling = measured(["1", "-1"]);
  assert.deepEqual(positive.events, [182, 13, 50]);
  assert.deepEqual(cancelling.events, [182, 13, 48]);
  assert.equal(positive.nodes - cancelling.nodes, 4);
  assert.equal(positive.edges - cancelling.edges, 6);
  for (const key of ["nodes", "edges"]) {
    const field = key === "nodes" ? "nodeUpperBound" : "inputReferences";
    assert.equal(positive[key] - positive.contribution.blocks.nativeCurrent[field],
      cancelling[key] - cancelling.contribution.blocks.nativeCurrent[field]);
  }
});

test("moving and native branch totals leave the same fixed work for distinct support distributions", () => {
  const { deriveBranchTopologyContributions } = require("../a68-branch-topology-envelope");
  const entry = (row, column) => ({ row, column, value: "1" });
  const supports = [[], [entry(0, 0)], Array.from({ length: 14 }, (_, i) => entry(i, 0)),
    Array.from({ length: 14 }, (_, i) => entry(0, i)), Array.from({ length: 14 }, (_, i) => entry(i, i))];
  let residual;
  for (const frameEntries of supports) for (const deltaSigns of [[], ["1", "-1"]]) {
    const { recipe, b, g } = fixture(false, deltaSigns, frameEntries), events = [0, 0, 0]; let calls = 0;
    const observed = new Proxy(recipe, { get(target, key) {
      const value = Reflect.get(target, key, target);
      if (key === "ScalarValue") return scalar => {
        const result = value.call(target, scalar), slot = calls++ % 3;
        if (result !== "0") events[slot]++; return result;
      };
      return typeof value === "function" ? value.bind(target) : value;
    } });
    const result = buildMixedVariation(observed, b, g, (name, tensor) => recipe.Mark(name, intermediateDegree(name), tensor, true));
    for (const name of ["CurrentCoefficients", "CurrentPartialCoefficients", "GreenVariationCoefficients"])
      for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) recipe.Mark(`${name}/${mu}/${nu}`, 0, result[name][mu][nu], true);
    assert.equal(calls, 3 * 14 ** 4);
    const plan = recipe.Finish(), blocks = deriveBranchTopologyContributions({ frameNonzeros: frameEntries.length,
      deltaFrameNonzeros: deltaSigns.length, framePartialNonzeros: 1,
      nativeCoefficientEvents: events[0], nativeChangedEvents: events[1], nativeMetricEvents: events[2] }).blocks;
    const actual = { nodes: plan.nodes.length - blocks.movingEuler.nodeUpperBound - blocks.nativeCurrent.nodeUpperBound,
      edges: plan.nodes.reduce((s, n) => s + n.inputs.length, 0) - blocks.movingEuler.inputReferences - blocks.nativeCurrent.inputReferences };
    if (!residual) residual = actual; else assert.deepEqual(actual, residual);
  }
  // Static fixed-body audit:4788 nodes/6117 edges +29 input leaves,
  // Phi cache107/105, and4 fixed internally consumed zeros. Nine returned
  // zero slots and238 unconsumed slice-diagonal handles are not marked here.
  assert.deepEqual(residual, { nodes: 4928, edges: 6222 });
});
