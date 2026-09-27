"use strict";
// Manufactured small-dimensional MATRIX jets only. Do not call any of the
// full GU source/metric/germ factories before the frozen scientific gate.
const test = require("node:test"), assert = require("node:assert/strict");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const { PAIRS, multiindices, germMatches, connectionVariation, verifyMetricConnection, verifyLinearizedMetricConnection, buildSourceGermGeometry, buildSourceFrameGeometry } = require("../a68-source-geometry");
const algebra = () => new GeometryAlgebra({ maxBits: 256, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 10000, matrixCells: 100000, slotVisits: 100000 });
const jet = (Value, D, DD) => ({ Value, D, DD });

test("source germ menu has the full ordered700 identities without evaluating geometry", () => {
  assert.deepEqual(PAIRS, [[0, 0], [1, 1], [2, 2], [3, 3], [0, 1], [0, 2], [0, 3], [1, 2], [1, 3], [2, 3]]);
  const jets = multiindices(); assert.equal(jets.length, 35); assert.equal(new Set(jets.map(JSON.stringify)).size, 35);
  assert.deepEqual([0, 1, 2, 3].map(order => jets.filter(j => j.reduce((a, b) => a + b) === order).length * 20), [20, 80, 200, 400]);
  for (const multi of jets) {
    const derivative = multi.flatMap((count, axis) => Array(count).fill(axis));
    assert.equal(germMatches(multi, derivative), true); assert.equal(germMatches(multi, derivative.reverse()), true);
    assert.equal(germMatches(multi, [...derivative, 4]), false); assert.equal(germMatches(multi, [...derivative, 0]), false);
  }
  assert.ok(Object.isFrozen(jets[0])); assert.ok(Object.isFrozen(PAIRS[0]));
});

test("one-dimensional nonconstant metric checks ordinaryDQ and inverse-metric derivative terms", () => {
  const a = algebra(), m = n => a.fromText([[String(n)]]);
  // At t=0: g=4 exp(2t), Gamma=1, dGamma=0; k=3+5t+7t²/2.
  const metric = jet(m(4), [m(8)], [[m(16)]]), connection = { Gamma: [m(1)], DGamma: [[m(0)]] };
  assert.equal(verifyMetricConnection(a, metric, connection), true);
  const changed = jet(m(3), [m(5)], [[m(7)]]), result = connectionVariation(a, metric, connection, changed);
  assert.equal(verifyLinearizedMetricConnection(a, metric, connection, changed, result), true);
  assert.deepEqual(a.matrixText(result.Gamma[0]), [["-1/8"]]);
  assert.deepEqual(a.matrixText(result.DGamma[0][0]), [["-1/8"]]);
  assert.deepEqual(a.matrixText(result.Curvature[0][0]), [["0"]]);
});

test("ordinaryDQ retains baseline connection derivatives even when metric first jets vanish", () => {
  const a = algebra(), m = n => a.fromText([[String(n)]]);
  const metric = jet(m(1), [m(0)], [[m(2)]]), connection = { Gamma: [m(0)], DGamma: [[m(1)]] };
  assert.equal(verifyMetricConnection(a, metric, connection), true);
  const result = connectionVariation(a, metric, connection, jet(m(1), [m(0)], [[m(0)]]));
  assert.deepEqual(a.matrixText(result.Gamma[0]), [["0"]]);
  assert.deepEqual(a.matrixText(result.DGamma[0][0]), [["-1"]]);
});

test("polar manufactured metric requires noncommuting curvature terms to cancel", () => {
  const a = algebra(), m = rows => a.fromText(rows.map(row => row.map(String))), zero = a.matrix(2);
  // g=diag(1,r²), evaluated at r=1. Change only the radial metric by a constant.
  const metric = jet(a.identity(2), [m([[0, 0], [0, 2]]), zero], [[m([[0, 0], [0, 2]]), zero], [zero, zero]]);
  const connection = { Gamma: [m([[0, 0], [0, 1]]), m([[0, -1], [1, 0]])], DGamma: [[m([[0, 0], [0, -1]]), m([[0, -1], [-1, 0]])], [zero, zero]] };
  assert.equal(verifyMetricConnection(a, metric, connection), true);
  const changed = jet(m([[1, 0], [0, 0]]), [zero, zero], [[zero, zero], [zero, zero]]), result = connectionVariation(a, metric, connection, changed);
  assert.equal(verifyLinearizedMetricConnection(a, metric, connection, changed, result), true);
  assert.ok(a.matrixEqual(result.Gamma[0], zero));
  assert.deepEqual(a.matrixText(result.Gamma[1]), [["0", "1"], ["0", "0"]]);
  assert.ok(a.matrixEqual(result.DGamma[0][1], result.Gamma[1]));
  for (const row of result.Curvature) for (const value of row) assert.ok(a.matrixEqual(value, zero));
});

test("full two-dimensional variation reconstructs all connections, derivative order and curvature signs", () => {
  const a = algebra(), zero = a.matrix(2), id = a.identity(2), dd = () => [[zero, zero], [zero, zero]];
  const metric = jet(id, [zero, zero], dd()), connection = { Gamma: [zero, zero], DGamma: dd() };
  assert.equal(verifyMetricConnection(a, metric, connection), true);
  // k00=2x0+x1²/2; k01=k10=3x1; k11=4x0.
  const second = dd(); second[1][1] = a.fromText([["1", "0"], ["0", "0"]]);
  const changed = jet(zero, [a.fromText([["2", "0"], ["0", "4"]]), a.fromText([["0", "3"], ["3", "0"]])], second);
  const result = connectionVariation(a, metric, connection, changed);
  assert.equal(verifyLinearizedMetricConnection(a, metric, connection, changed, result), true);
  assert.deepEqual(result.Gamma.map(m => a.matrixText(m)), [[["1", "0"], ["0", "2"]], [["0", "1"], ["2", "0"]]]);
  assert.deepEqual(a.matrixText(result.DGamma[1][0]), [["0", "1/2"], ["-1/2", "0"]]);
  assert.deepEqual(a.matrixText(result.DGamma[1][1]), [["1/2", "0"], ["0", "0"]]);
  assert.ok(a.matrixEqual(result.DGamma[0][0], zero)); assert.ok(a.matrixEqual(result.DGamma[0][1], zero));
  assert.deepEqual(a.matrixText(result.Curvature[0][1]), [["0", "-1/2"], ["1/2", "0"]]);
  assert.deepEqual(a.matrixText(result.Curvature[1][0]), [["0", "1/2"], ["-1/2", "0"]]);
  for (let i = 0; i < 2; i++) assert.ok(a.matrixEqual(result.Curvature[i][i], zero));
});

test("complete connection identity checks reject torsion even when metric compatibility passes", () => {
  const a = algebra(), zero = a.matrix(2), metric = jet(a.identity(2), [zero, zero], [[zero, zero], [zero, zero]]);
  const skew = a.fromText([["0", "1"], ["-1", "0"]]);
  assert.throws(() => verifyMetricConnection(a, metric, { Gamma: [skew, zero], DGamma: [[zero, zero], [zero, zero]] }), /torsion zero/);
});

test("factored Koszul numerators preserve dense inverse contractions and derivative orientation", () => {
  const a = algebra(), z = a.matrix(2), m = rows => a.fromText(rows.map(row => row.map(String))), dd = [[z, z], [z, z]];
  const metric = jet(m([[2, 1], [1, 3]]), [z, z], dd), connection = { Gamma: [z, z], DGamma: dd };
  const changed = jet(z, [m([[1, 2], [2, 3]]), m([[5, 7], [7, 11]])], [[z, z], [z, m([[13, 17], [17, 19]])]]);
  assert.equal(verifyMetricConnection(a, metric, connection), true);
  const result = connectionVariation(a, metric, connection, changed);
  assert.equal(verifyLinearizedMetricConnection(a, metric, connection, changed, result), true);
  // G^-1=(1/5)[[3,-1],[-1,2]]. Independent hand numerators are
  // S0=[[1,5],[-1,3]], S1=[[5,11],[3,11]], D1S0=[[0,13],[-13,0]],
  // D1S1=[[13,34],[0,19]]. None is supplied to the implementation.
  assert.deepEqual(result.Gamma.map(q => a.matrixText(q)), [
    [["2/5", "6/5"], ["-3/10", "1/10"]], [["6/5", "11/5"], ["1/10", "11/10"]]
  ]);
  assert.deepEqual(a.matrixText(result.DGamma[1][0]), [["13/10", "39/10"], ["-13/5", "-13/10"]]);
  assert.deepEqual(a.matrixText(result.DGamma[1][1]), [["39/10", "83/10"], ["-13/10", "2/5"]]);
  assert.ok(a.matrixEqual(result.DGamma[0][0], z)); assert.ok(a.matrixEqual(result.DGamma[0][1], z));
  assert.deepEqual(a.matrixText(result.Curvature[0][1]), [["-13/10", "-39/10"], ["13/5", "13/10"]]);
  assert.ok(a.matrixEqual(result.Curvature[1][0], a.matScale(result.Curvature[0][1], a.number(-1))));
});

test("differentiated compatibility is required even when all zeroth identities pass", () => {
  const a = algebra(), zero = a.matrix(2), id = a.identity(2), metric = jet(id, [zero, zero], [[id, zero], [zero, zero]]);
  assert.throws(() => verifyMetricConnection(a, metric, { Gamma: [zero, zero], DGamma: [[zero, zero], [zero, zero]] }), /differentiated metric compatibility/);
});

test("connection uniqueness rejects degenerate and nonsymmetric candidate metrics", () => {
  const a = algebra(), zero = a.matrix(1);
  assert.throws(() => verifyMetricConnection(a, jet(zero, [zero], [[zero]]), { Gamma: [zero], DGamma: [[zero]] }), /nonsingular/);
  const b = algebra(), z = b.matrix(2), bad = b.fromText([["1", "1"], ["0", "1"]]);
  assert.throws(() => verifyMetricConnection(b, jet(bad, [z, z], [[z, z], [z, z]]), { Gamma: [z, z], DGamma: [[z, z], [z, z]] }), /symmetric metric/);
});

test("covariant reconstruction rejects incomplete metric and connection jet menus", () => {
  const a = algebra(), zero = a.matrix(2), metric = jet(a.identity(2), [zero, zero], [[zero, zero], [zero, zero]]), connection = { Gamma: [zero, zero], DGamma: [[zero, zero], [zero, zero]] };
  assert.throws(() => connectionVariation(a, { ...metric, D: [zero] }, connection, metric), /complete metric jet/);
  assert.throws(() => connectionVariation(a, metric, { ...connection, DGamma: [[zero], [zero, zero]] }, metric), /derivative slots/);
  assert.throws(() => connectionVariation(a, metric, connection, { ...metric, DD: [[zero, zero], [zero]] }), /second jet/);
});

test("linearized identity checks reject a changed derivative and forged source-looking wrappers", () => {
  const a = algebra(), m = n => a.fromText([[String(n)]]);
  const metric = jet(m(1), [m(0)], [[m(2)]]), connection = { Gamma: [m(0)], DGamma: [[m(1)]] }, changed = jet(m(1), [m(0)], [[m(0)]]);
  const delta = connectionVariation(a, metric, connection, changed);
  assert.equal(verifyLinearizedMetricConnection(a, metric, connection, changed, delta), true);
  assert.throws(() => verifyLinearizedMetricConnection(a, metric, connection, changed, { ...delta, DGamma: [[m(0)]] }), /differentiated linearized metric/);
  assert.throws(() => buildSourceFrameGeometry(a, { point: 0 }, connection), /same-source/);
  assert.throws(() => buildSourceGermGeometry(a, { source: { point: 0 }, connection, frame: {} }, 0, 0), /lineage/);
});
