"use strict";

// Manufactured small matrices and Clifford records only. No GU point/germ
// factory is evaluated; no scientific comparison result is asserted here.
const test = require("node:test"), assert = require("node:assert/strict");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const { GeometryComparison } = require("../a68-geometry-comparison");
const { bindRetainedGeometry, boundGeometryIdentity } = require("../a68-geometry-binding");
const { sourceGeometryIdentity } = require("../a68-source-geometry");
const { retainedGeometryIdentity } = require("../a68-retained-inputs");
const { geometryMatrixLayout, exportRecipeGeometry } = require("../a68-geometry-recipe-binding");
const { compareRecordedGeometry } = require("../a68-geometry-metadata");
const algebra = () => new GeometryAlgebra({ maxBits: 256, scalarOperations: 10000, rationalObjects: 10000, matrixObjects: 1000, matrixCells: 10000, slotVisits: 10000 });
const limits = overrides => ({ coordinateVisits: 10000, sparseRecords: 1000, tensorRecords: 1000, rationalCharacters: 10000, ...overrides });
const comparator = (a, overrides) => new GeometryComparison(a, limits(overrides));
const tensor = (form, blade, real, imaginary = "0") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const blocks = a => [[a.fromText([["0", "2"], ["3", "0"]]), a.matrix(2)], [a.matrix(2), a.matrix(2)]];

test("dense comparison scans every exact rational coordinate", () => {
  const a = algebra(), c = comparator(a), m = a.fromText([["1/2", "-2"], ["0", "3/7"]]);
  assert.equal(c.dense(m, [["1/2", "-2"], ["0", "3/7"]]), true);
  assert.equal(c.snapshot().coordinateVisits, 4); assert.equal(c.snapshot().sourceAuthenticityEstablished, false);
  assert.throws(() => c.dense(m, [["1/2", "-2"], ["1", "3/7"]]), /mismatch/);
  assert.throws(() => c.dense(m, [["1/2", "-2"], ["0", "3/7"]]), /poisoned/);
});

test("Sparse3 compares missing coordinates to zero, including last ordinary derivative slot", () => {
  const a = algebra(), matrices = [a.matrix(2), a.fromText([["0", "0"], ["-5/3", "0"]])];
  const c = comparator(a); assert.equal(c.matrices(matrices, [{ z: 1, i: 1, j: 0, value: "-5/3" }]), true);
  assert.equal(c.snapshot().coordinateVisits, 8); assert.equal(c.snapshot().sparseRecords, 1);
  assert.throws(() => comparator(a).matrices(matrices, []), /complete sparse coordinate mismatch at 6/);
  assert.throws(() => comparator(a).matrices(matrices, [{ z: 0, i: 1, j: 0, value: "-5/3" }]), /mismatch/);
});

test("Sparse4 connection uses derivative first and curvature swaps output/input matrix indices", () => {
  const a = algebra(), b = blocks(a);
  const connection = [{ z: 0, w: 0, i: 0, j: 1, value: "2" }, { z: 0, w: 0, i: 1, j: 0, value: "3" }];
  const curvature = [{ a: 0, b: 0, c: 0, d: 1, value: "3" }, { a: 0, b: 0, c: 1, d: 0, value: "2" }];
  const c = comparator(a); assert.equal(c.blocks(b, connection), true); assert.equal(c.blocks(b, curvature, true), true);
  assert.equal(c.snapshot().coordinateVisits, 32);
  const wrong = curvature.map(row => ({ ...row, value: row.value === "2" ? "3" : "2" }));
  assert.throws(() => comparator(a).blocks(b, wrong, true), /mismatch/);
  const asymmetric = [[a.matrix(2), a.fromText([["0", "5"], ["0", "0"]])], [a.matrix(2), a.matrix(2)]];
  assert.equal(comparator(a).blocks(asymmetric, [{ z: 0, w: 1, i: 0, j: 1, value: "5" }]), true);
  assert.throws(() => comparator(a).blocks(asymmetric, [{ z: 1, w: 0, i: 0, j: 1, value: "5" }]), /mismatch/);
});

test("canonical sparse stream rejects duplicates, zeros, invalid coordinates and omitted/extra coefficients", () => {
  const row = { z: 0, i: 0, j: 0, value: "1" };
  for (const records of [[row, row], [{ ...row, value: "0" }], [{ ...row, value: "2/2" }], [{ ...row, z: 2 }],
    [{ ...row, extra: true }], [row, { z: 1, i: 1, j: 1, value: "1" }], []]) {
    const a = algebra(), matrices = [a.fromText([["1", "0"], ["0", "0"]]), a.matrix(2)];
    assert.throws(() => comparator(a).matrices(matrices, records), /order|zero|canonical|domain|record|mismatch/);
  }
  const a = algebra(), matrices = [a.matrix(2), a.matrix(2)];
  assert.throws(() => comparator(a).matrices(matrices, new Array(1)), /dense array/);
  assert.throws(() => comparator(a).matrices([matrices[0], ,], []), /dense array/);
});

test("complete local tensor equality retains imaginary and central directions", () => {
  const a = algebra(), c = comparator(a), rows = [tensor(1, 0, "0", "3/2"), tensor(2, 16383, "-7/3")];
  assert.equal(c.tensor(rows, structuredClone(rows), 1), true); assert.equal(c.snapshot().tensorRecords, 4);
  assert.equal(c.tensor([], [], 2), true);
  assert.throws(() => comparator(a).tensor(rows, rows.slice(0, 1), 1), /support census/);
  assert.throws(() => comparator(a).tensor(rows, [tensor(1, 0, "3/2"), rows[1]], 1), /coefficient mismatch/);
  assert.throws(() => comparator(a).tensor(rows, [rows[0], tensor(2, 16382, "-7/3")], 1), /coordinates/);
});

test("tensor parser rejects wrong degrees, nonlocal modes, explicit zeros and unordered records", () => {
  for (const row of [tensor(3, 1, "1"), tensor(1, 1, "0"), { ...tensor(1, 1, "1"), k0: 1 }, tensor(1, 1, "2/4")])
    assert.throws(() => comparator(algebra()).tensor([row], [row], 1), /degree|zero|coordinates|canonical/);
  const a = algebra();
  const rows = [tensor(2, 0, "1"), tensor(1, 0, "1")]; assert.throws(() => comparator(a).tensor(rows, rows, 1), /ordered/);
});

test("comparison admission precedes scans and failures poison the comparison", () => {
  const a = algebra(), m = a.identity(2), before = a.snapshot(), c = comparator(a, { coordinateVisits: 3 });
  assert.throws(() => c.dense(m, [["1", "0"], ["0", "1"]]), /coordinateVisits ceiling/);
  assert.deepEqual(a.snapshot(), before); assert.equal(c.snapshot().failed, true);
  assert.throws(() => comparator(a, { rationalCharacters: 1 }).dense(m, [["1", "0"], ["0", "1"]]), /rationalCharacters ceiling/);
  const t = [tensor(1, 0, "1")]; assert.throws(() => comparator(a, { tensorRecords: 1 }).tensor(t, t, 1), /tensorRecords ceiling/);
  assert.throws(() => new GeometryComparison(a, {}), /explicit/);
});

test("source and retained outer lookalikes cannot acquire authentic binding identities", () => {
  const a = algebra(), fake = Object.freeze({ source: { point: 0 }, point: 0 });
  assert.throws(() => sourceGeometryIdentity(a, fake), /closed complete/);
  assert.throws(() => sourceGeometryIdentity(undefined, fake), /bounded exact algebra/);
  assert.throws(() => retainedGeometryIdentity(fake), /pinned geometry object identity/);
  assert.throws(() => bindRetainedGeometry(a, fake, fake, limits()), /closed complete/);
  assert.throws(() => boundGeometryIdentity(a, fake), /verified geometry binding identity/);
  assert.throws(() => boundGeometryIdentity(undefined, fake), /verified geometry binding identity/);
  assert.throws(() => exportRecipeGeometry(a, fake, { matrixSlots: 1000, coordinateVisits: 100000, entryRecords: 100000 }), /verified geometry binding identity/);
  assert.throws(() => compareRecordedGeometry(a, fake, {}, limits()), /verified geometry binding identity/);
});

test("source recipe layout includes all73 background and240 germ matrix slots without zero-dependent deduplication", () => {
  const base = geometryMatrixLayout("baseline"), germ = geometryMatrixLayout("germ");
  assert.equal(base.length, 73); assert.equal(germ.length, 240);
  assert.deepEqual(base.slice(0, 3), ["Frame", "InverseFrame", "Identity"].map(role => ({ role, indices: [] })));
  assert.deepEqual(germ.slice(0, 2), ["Motion", "DeltaFrame"].map(role => ({ role, indices: [] })));
  const partials = germ.filter(x => x.role === "DeltaOmegaPartial"); assert.equal(partials.length, 196);
  assert.deepEqual(partials.map(x => x.indices), Array.from({ length: 196 }, (_, i) => [Math.floor(i / 14), i % 14]));
  assert.equal(new Set([...base, ...germ].map(x => x.role + JSON.stringify(x.indices))).size, 313);
  assert.ok(Object.isFrozen(base[0].indices)); assert.throws(() => geometryMatrixLayout("observed"), /closed geometry/);
});

test("complete dense sink metric and connection structures compare both all-zero and nonzero slots", () => {
  const a = algebra(), zero = a.matrix(2), b = blocks(a), g = a.identity(2);
  const text = m => a.matrixText(m), matrices = [zero, b[0][0]], dd = b.map(row => row.map(text));
  const c = comparator(a), jet = { Value: g, D: matrices, DD: b };
  assert.equal(c.metricJet(jet, { g: text(g), d: matrices.map(text), dd }), true);
  const curvature = b.map(row => row.map(m => text(a.transpose(m))));
  const connection = { Gamma: matrices, DGamma: b, Curvature: b };
  assert.equal(c.connection(connection, { gamma: matrices.map(text), dGamma: dd, curvature }), true);
  assert.equal(c.snapshot().coordinateVisits, (4 + 8 + 16) + (8 + 16 + 16));
  assert.throws(() => comparator(a).connection(connection, { gamma: matrices.map(text), dGamma: dd, curvature: dd }), /mismatch/);
  assert.throws(() => comparator(a).metricJet(jet, { g: text(g), d: matrices.map(text) }), /metadata shape/);
  const missing = structuredClone(dd); missing[1].pop();
  assert.throws(() => comparator(a).metricJet(jet, { g: text(g), d: matrices.map(text), dd: missing }), /dense array/);
  const before = a.snapshot();
  assert.throws(() => comparator(a, { coordinateVisits: 3 }).denseBlocks(b, curvature, true), /coordinateVisits ceiling/);
  assert.deepEqual(a.snapshot(), before, "curvature index swap must not allocate a transpose before admission");
});
