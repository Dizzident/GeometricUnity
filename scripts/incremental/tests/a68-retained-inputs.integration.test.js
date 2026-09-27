"use strict";
// READ-ONLY integration against already committed upstream evidence. No
// polynomial reconstruction, new geometric response or study execution.
const test = require("node:test"), assert = require("node:assert/strict"), path = require("node:path");
const { RetainedMixedInputs, retainedGeometryIdentity, retainedStationaryIdentity, PINS, EPSILON } = require("../a68-retained-inputs");
const inputs = new RetainedMixedInputs(path.resolve(__dirname, "../../.."));

test("retained baseline API exports both pinned geometries and excludes old field responses", () => {
  const rows = [inputs.baseline(0), inputs.baseline(1)];
  for (const [point, row] of rows.entries()) {
    assert.deepEqual(Object.keys(row), ["point", "h0", "fibreMetricY", "frame", "spinReference", "spinCurvature", "source"]);
    assert.equal(row.point, point); assert.deepEqual(row.source, PINS.geometry);
    assert.equal(row.frame.length, 14); assert.ok(row.frame.every(r => r.length === 14));
    assert.ok(row.spinReference.length > 0); assert.ok(row.spinCurvature.length > 0);
    assert.ok(Object.isFrozen(row)); assert.ok(Object.isFrozen(row.frame[0]));
    assert.ok(!("fields" in row)); assert.ok(!("baselineForecasts" in row));
    assert.deepEqual(retainedGeometryIdentity(row), { kind: "baseline", point, source: PINS.geometry });
    assert.throws(() => retainedGeometryIdentity({ ...row }), /pinned geometry object identity/);
  }
  assert.deepEqual(rows[0].h0, rows[1].h0); assert.notDeepEqual(rows[0].fibreMetricY, rows[1].fibreMetricY);
});

test("retained geometry API covers all700 identities and never exports fieldRows", () => {
  const orders = [0, 0, 0, 0], paths = new Set(); let bytes = 0;
  const keys = ["point", "metricBasis", "jetIndex", "multiindex", "order", "source", "frameMotion", "shearVariation", "metricVariation", "inverseVariation", "metricFirstJets", "metricSecondJets", "connectionVariation", "connectionDerivative", "curvatureVariation", "spinConnectionVariation", "spinCurvatureVariation"];
  for (let point = 0; point < 2; point++) for (let metric = 0; metric < 10; metric++) for (let jet = 0; jet < 35; jet++) {
    const row = inputs.geometry(point, metric, jet);
    assert.deepEqual(Object.keys(row), keys); assert.deepEqual([row.point, row.metricBasis, row.jetIndex], [point, metric, jet]);
    assert.ok(Object.isFrozen(row)); assert.ok(Object.isFrozen(row.spinCurvatureVariation));
    assert.equal(row.order, row.multiindex.reduce((a, b) => a + b, 0)); orders[row.order]++;
    assert.ok(!paths.has(row.source.path)); paths.add(row.source.path); bytes += row.source.bytes;
    assert.ok(!("fieldRows" in row));
    assert.deepEqual(retainedGeometryIdentity(row), { kind: "germ", point, metricBasis: metric, jetIndex: jet, source: row.source });
    assert.throws(() => retainedGeometryIdentity({ ...row }), /pinned geometry object identity/);
  }
  assert.equal(paths.size, 700); assert.deepEqual(orders, [20, 80, 200, 400]);
  assert.equal(bytes, 39777820, "independently frozen prior shard byte census");
});

test("retained polynomial mappings bind all twelve complete canonical source arrays", () => {
  const paths = new Set();
  for (let point = 0; point < 2; point++) {
    const row = inputs.polynomialInputs(point); assert.deepEqual(Object.keys(row), ["S1", "S2", "S3", "S4", "S5", "X"]);
    assert.deepEqual(retainedStationaryIdentity(row), { kind: "polynomial", point, source: PINS.stationary });
    assert.ok(Object.isFrozen(retainedStationaryIdentity(row)));
    assert.throws(() => retainedStationaryIdentity({ ...row }), /pinned stationary object identity/);
    assert.throws(() => retainedGeometryIdentity(row), /pinned geometry object identity/);
    for (const [field, entry] of Object.entries(row)) {
      assert.match(entry.source.path, new RegExp(`/p${point}_kinetic_${field}_inputs_g000\\.json$`));
      assert.equal(entry.source.tensor, "input"); assert.ok(!paths.has(entry.source.path)); paths.add(entry.source.path);
      assert.ok(entry.tensor.length > 0); assert.ok(Object.isFrozen(entry.tensor)); assert.ok(Object.isFrozen(entry.tensor[0]));
      for (let i = 0; i < entry.tensor.length; i++) {
        const term = entry.tensor[i]; assert.equal(term.form & (term.form - 1), 0); assert.ok(term.form > 0);
        assert.equal(term.k0, 0); assert.equal(term.k1, 0);
        if (i) assert.ok(entry.tensor[i - 1].form * 16384 + entry.tensor[i - 1].blade < term.form * 16384 + term.blade);
      }
    }
  }
  assert.equal(paths.size, 12);
});

test("retained certificate API preserves conditional parameters and exact error at both points", () => {
  for (let point = 0; point < 2; point++) {
    const row = inputs.certificate(point);
    assert.deepEqual([row.point, row.gamma, row.kappa, row.lambda], [point, 1, 907712, "1/907712"]);
    assert.equal(row.certificate.certifiedError, EPSILON); assert.ok(Object.isFrozen(row.certificate));
    assert.match(row.source.path, new RegExp(`/point${point}_context\\.json$`));
    assert.deepEqual(retainedStationaryIdentity(row), { kind: "certificate", point, source: row.source });
    assert.ok(Object.isFrozen(retainedStationaryIdentity(row)));
    assert.throws(() => retainedStationaryIdentity({ ...row }), /pinned stationary object identity/);
    assert.throws(() => retainedGeometryIdentity(row), /pinned geometry object identity/);
  }
});

test("retained API rejects out-of-domain identities before shard access", () => {
  for (const point of [-1, 2, 0.5, "0", null]) for (const method of ["baseline", "polynomialInputs", "certificate"]) assert.throws(() => inputs[method](point), /point/);
  for (const indices of [[-1, 0, 0], [0, 10, 0], [0, 0, 35], [1, 0.5, 0], [0, 0, "0"]]) assert.throws(() => inputs.geometry(...indices), /geometry indices/);
});
