"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const { DenseGeometryFields, germGeometryFieldTarget, createSourceGermFieldResolver } = require("../a68-source-field-resolver");
const arithmetic = () => new GeometryAlgebra({ maxBits: 64, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 1000, matrixCells: 100000, slotVisits: 100000 });
const limits = { fields: 1000, coordinateVisits: 1000000, arrayObjects: 1000000, arraySlots: 2000000, formatCharacters: 1000000000, rationalCharacters: 64 };

test("dense fields retain signs, fractions, implicit zeros and frozen full shape", () => {
  const a = arithmetic(), x = a.fromText([["1/2", "0"], ["-3/7", "2"]]), f = new DenseGeometryFields(a, limits);
  const rows = f.matrix(x); assert.deepEqual(rows, [["1/2", "0"], ["-3/7", "2"]]);
  assert.ok(Object.isFrozen(rows) && rows.every(Object.isFrozen)); assert.throws(() => { rows[0][0] = "0"; }, TypeError);
  assert.deepEqual(f.snapshot(), { fields: 1, coordinateVisits: 4, arrayObjects: 3, arraySlots: 6, formatCharacters: 4 * 8 * 66, failed: false, processMemoryProved: false });
});
test("rank-four curvature exports output/input indices in retained order", () => {
  const a = arithmetic(), blocks = Array.from({ length: 2 }, (_, i) => Array.from({ length: 2 }, (_, j) => a.matrix(2, (d, c) => a.number(1000 * i + 100 * j + 10 * d + c))));
  const f = new DenseGeometryFields(a, limits), r = f.curvature(blocks);
  for (let i = 0; i < 2; i++) for (let j = 0; j < 2; j++) for (let c = 0; c < 2; c++) for (let d = 0; d < 2; d++) assert.equal(r[i][j][c][d], String(1000 * i + 100 * j + 10 * d + c));
  assert.equal(r[0][0][0][0], "0"); assert.equal(r[0][0][1][0], "1"); assert.equal(r[0][0][0][1], "10");
  assert.equal(f.snapshot().arrayObjects, 15); assert.equal(f.snapshot().arraySlots, 30); assert.equal(f.snapshot().coordinateVisits, 16);
  const frozen = x => { assert.ok(Object.isFrozen(x)); x.forEach(v => { if (Array.isArray(v)) frozen(v); }); }; frozen(r);
});
test("each array and formatting ceiling rejects the whole export before formatting", () => {
  const a = arithmetic(), x = a.identity(2);
  for (const [key, cap] of [["coordinateVisits", 3], ["arrayObjects", 2], ["arraySlots", 5], ["formatCharacters", 4 * 8 * 66 - 1]]) {
    const f = new DenseGeometryFields(a, { ...limits, [key]: cap }); assert.throws(() => f.matrix(x), new RegExp(key + " ceiling"));
    assert.ok(f.snapshot().failed); assert.throws(() => f.matrix(x), /poisoned/);
  }
});
test("field and character reservations are cumulative", () => {
  const a = arithmetic(), x = a.identity(1), f = new DenseGeometryFields(a, { ...limits, fields: 1 });
  assert.deepEqual(f.matrix(x), [["1"]]); assert.throws(() => f.matrix(x), /fields ceiling/); assert.equal(f.snapshot().fields, 1);
});
test("foreign algebra tokens and non-data block elements cannot become evidence", () => {
  const a = arithmetic(), other = arithmetic(), f = new DenseGeometryFields(a, limits);
  assert.throws(() => f.matrix(other.identity(1)), /foreign matrix/);
  let called = false; const block = []; Object.defineProperty(block, "0", { get() { called = true; return []; }, enumerable: true });
  const b = arithmetic(), g = new DenseGeometryFields(b, limits); assert.throws(() => g.curvature(block), /own block data/); assert.equal(called, false);
});
test("limits require own data and rational output height fails closed", () => {
  let called = false; const bad = { ...limits }; Object.defineProperty(bad, "fields", { get() { called = true; return 1; } });
  assert.throws(() => new DenseGeometryFields(arithmetic(), bad), /own finite/); assert.equal(called, false);
  const a = arithmetic(), f = new DenseGeometryFields(a, { ...limits, rationalCharacters: 1 });
  assert.throws(() => f.matrix(a.fromText([["123"]])), /rational height/); assert.ok(f.snapshot().failed);
});
test("complete fixed germ menu has845 paths and242060 scalar coordinates", () => {
  const paths = ["geometry/geometry.motion"];
  for (const role of ["deltaMetric", "blockMetric"]) {
    paths.push(`geometry/geometry.${role}.g`);
    for (let i = 0; i < 14; i++) { paths.push(`geometry/geometry.${role}.d[${i}]`); for (let j = 0; j < 14; j++) paths.push(`geometry/geometry.${role}.dd[${i}][${j}]`); }
  }
  for (const role of ["deltaConnection", "palatini"]) {
    paths.push(`geometry/geometry.${role}.curvature`);
    for (let i = 0; i < 14; i++) { paths.push(`geometry/geometry.${role}.gamma[${i}]`); for (let j = 0; j < 14; j++) paths.push(`geometry/geometry.${role}.dGamma[${i}][${j}]`); }
  }
  assert.equal(new Set(paths).size, 845); assert.equal(paths.reduce((sum, p) => sum + 14 ** germGeometryFieldTarget(p).rank, 0), 242060);
  assert.deepEqual(germGeometryFieldTarget("geometry/geometry.palatini.dGamma[13][0]"), { family: "connection", field: "dGamma", first: 13, second: 0, rank: 2 });
});
test("closed field parser rejects unknown routes, trailing text and wrong index arity", () => {
  for (const p of ["geometry/geometry.deltaMetric.gamma[0]", "geometry/geometry.deltaMetric.g[0]", "geometry/geometry.deltaMetric.d", "geometry/geometry.deltaMetric.dd[0]", "geometry/geometry.palatini.curvature[0]", "geometry/geometry.motion[0]", "geometry/geometry.deltaMetric.d[14]", "geometry/geometry.deltaMetric.d[01]", "geometry/geometry.deltaMetric.d[-0]", "geometry/geometry.deltaMetric.d[0]\n", "geometry/geometry.__proto__.g", null]) assert.throws(() => germGeometryFieldTarget(p));
});
test("source wrapper rejects forged geometry before constructing an exporter", () => {
  assert.throws(() => createSourceGermFieldResolver(arithmetic(), { kind: "germ", point: 0, comparison: { passed: true } }, limits), /verified geometry binding identity/);
});
test("format bound is analytic and unavailable after arithmetic poisoning", () => {
  const a = arithmetic(); assert.equal(a.textCharacterBound(), 528);
  assert.throws(() => a.parse("01")); assert.throws(() => a.textCharacterBound(), /poisoned/);
});
