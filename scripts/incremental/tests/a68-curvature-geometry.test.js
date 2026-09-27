"use strict";

// Manufactured 2D rank-four arrays and scalar factors ONLY. These tests do
// not import source geometry, construct a source point, or call the complete
// 14D spin coefficient constructors on admitted inputs. Production resource
// limits, provenance and all coefficient-level study checks remain separate.
const test = require("node:test");
const assert = require("node:assert/strict");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const { lowerCurvature, transformCovariant4, variationLoweredCurvature,
  validateMetricSkew, validateLoweredCurvature, spinConnectionCoefficient, spinCurvatureCoefficient, spinConnectionTensor, spinCurvatureTensor } = require("../a68-curvature-geometry");
const create = overrides => new GeometryAlgebra({ maxBits: 256, scalarOperations: 100000,
  rationalObjects: 100000, matrixObjects: 1000, matrixCells: 10000, slotVisits: 100000, ...overrides });
const blocks = (a, entry = () => a.zero) => Array.from({ length: 2 }, (_, i) =>
  Array.from({ length: 2 }, (_, j) => a.matrix(2, (c, d) => entry(i, j, c, d))));
const text = (a, tensor) => tensor.map(row => row.map(matrix => a.matrixText(matrix)));
const identityMaps = a => Array.from({ length: 4 }, () => a.identity(2));

test("lowering transposes G*R and retains all four blocks without curvature-symmetry assumptions", () => {
  const a = create(), metric = a.fromText([["2", "3"], ["3", "5"]]);
  const values = [["7", "11"], ["13", "17"]], curvature = blocks(a, (i, j, c, d) => a.multiply(a.number(1 + i * 2 + j), a.parse(values[c][d])));
  const lower = lowerCurvature(a, metric, curvature);
  assert.deepEqual(text(a, lower), [
    [[["53", "86"], ["73", "118"]], [["106", "172"], ["146", "236"]]],
    [[["159", "258"], ["219", "354"]], [["212", "344"], ["292", "472"]]]
  ]);
  assert.ok(Object.isFrozen(lower)); lower.forEach(row => assert.ok(Object.isFrozen(row)));
  assert.deepEqual(a.matrixText(curvature[0][0]), values, "input endomorphism is unchanged");
});

test("four distinct covariant maps use old/new orientation in every slot", () => {
  const a = create(), input = blocks(a, (i, j, c, d) => i === 0 && j === 1 && c === 1 && d === 0 ? a.parse("3/2") : a.zero);
  const maps = [a.fromText([["1", "2"], ["3", "4"]]), a.fromText([["5", "6"], ["7", "8"]]),
    a.fromText([["9", "10"], ["11", "12"]]), a.fromText([["13", "14"], ["15", "16"]])];
  // Output=(3/2)*(1,2)_a*(7,8)_b*(11,12)_c*(13,14)_d.
  assert.deepEqual(text(a, transformCovariant4(a, input, maps)), [
    [[["3003/2", "1617"], ["1638", "1764"]], [["1716", "1848"], ["1872", "2016"]]],
    [[["3003", "3234"], ["3276", "3528"]], [["3432", "3696"], ["3744", "4032"]]]
  ]);
});

test("covariant transformation sums all16 manufactured entries and retains exact cancellations", () => {
  const a = create(), dense = blocks(a, (i, j, c, d) => a.number(1 + 8 * i + 4 * j + 2 * c + d));
  const maps = Array.from({ length: 4 }, () => a.fromText([["1", "1"], ["1", "1"]]));
  const all = transformCovariant4(a, dense, maps);
  assert.deepEqual(text(a, all), Array.from({ length: 2 }, () => Array.from({ length: 2 }, () => [["136", "136"], ["136", "136"]])));
  assert.deepEqual(text(a, transformCovariant4(a, dense, identityMaps(a))), text(a, dense));
  const cancel = blocks(a, (i, j, c, d) => a.number((i + j + c + d) % 2 ? -1 : 1));
  const zero = transformCovariant4(a, cancel, maps);
  assert.deepEqual(text(a, zero), Array.from({ length: 2 }, () => Array.from({ length: 2 }, () => [["0", "0"], ["0", "0"]])));
});

test("lowered variation includes G*deltaR, deltaG*R and each of four distinct frame derivatives", () => {
  const a = create(), metric = a.fromText([["3", "0"], ["0", "5"]]), deltaMetric = a.fromText([["11", "0"], ["0", "13"]]);
  const curvature = blocks(a, (i, j, c, d) => i + j + c + d === 0 ? a.number(2) : a.zero);
  const deltaR = blocks(a, (i, j, c, d) => i + j + c + d === 0 ? a.number(7) : a.zero);
  const deltaMaps = [2, 3, 5, 7].map(value => a.fromText([["0", String(value)], ["0", "0"]]));
  const result = variationLoweredCurvature(a, metric, deltaMetric, curvature, deltaR, identityMaps(a), deltaMaps);
  // Low_0000=6; deltaLow_0000=3*7+11*2=43. The four independent
  // frame terms are12,18,30,42 at indices1000,0100,0010,0001.
  assert.deepEqual(text(a, result), [
    [[["43", "42"], ["30", "0"]], [["18", "0"], ["0", "0"]]],
    [[["12", "0"], ["0", "0"]], [["0", "0"], ["0", "0"]]]
  ]);
});

test("nonidentity frame variation is first-order, without missing frame factors or second-order terms", () => {
  const a = create(), metric = a.fromText([["3", "0"], ["0", "5"]]), deltaMetric = a.fromText([["11", "0"], ["0", "13"]]);
  const curvature = blocks(a, (i, j, c, d) => i + j + c + d === 0 ? a.number(2) : a.zero);
  const deltaR = blocks(a, (i, j, c, d) => i + j + c + d === 0 ? a.number(7) : a.zero);
  const maps = Array.from({ length: 4 }, () => a.fromText([["2", "0"], ["0", "2"]]));
  const deltaMaps = Array.from({ length: 4 }, () => a.fromText([["3", "0"], ["0", "3"]]));
  const result = variationLoweredCurvature(a, metric, deltaMetric, curvature, deltaR, maps, deltaMaps);
  // 43*2^4 + 4*6*3*2^3 =1264.
  assert.deepEqual(text(a, result), [
    [[["1264", "0"], ["0", "0"]], [["0", "0"], ["0", "0"]]],
    [[["0", "0"], ["0", "0"]], [["0", "0"], ["0", "0"]]]
  ]);
});

test("spin connection factor uses the d,c matrix entry and only sigma_c", () => {
  const a = create(), manufactured = a.fromText([["0", "7/3"], ["-5/2", "0"]]);
  const omegaDC = a.get(manufactured, 1, 0);
  assert.equal(a.text(spinConnectionCoefficient(a, 1, omegaDC)), "5/4");
  assert.equal(a.text(spinConnectionCoefficient(a, -1, omegaDC)), "-5/4");
  assert.equal(a.text(spinConnectionCoefficient(a, 1, a.get(manufactured, 0, 1))), "-7/6", "opposite entry would give a different answer");
  assert.equal(a.text(spinConnectionCoefficient(a, -1, a.zero)), "0");
});

test("nonsymmetric metric and metric variation fail closed instead of silently changing the lowering convention", () => {
  const a = create(), metric = a.identity(2), nonsymmetric = a.fromText([["1", "2"], ["3", "4"]]), input = blocks(a), maps = identityMaps(a);
  assert.throws(() => lowerCurvature(a, nonsymmetric, input), /metric must be symmetric/);
  assert.throws(() => variationLoweredCurvature(a, nonsymmetric, metric, input, input, maps, maps), /metric must be symmetric/);
  assert.throws(() => variationLoweredCurvature(a, metric, nonsymmetric, input, input, maps, maps), /metric variation must be symmetric/);
});

test("spin curvature factor is minus one half times both signature signs, with no pair doubling", () => {
  const a = create(), coefficient = a.parse("7/3");
  assert.equal(a.text(spinCurvatureCoefficient(a, 1, 1, coefficient)), "-7/6");
  assert.equal(a.text(spinCurvatureCoefficient(a, -1, -1, coefficient)), "-7/6");
  assert.equal(a.text(spinCurvatureCoefficient(a, 1, -1, coefficient)), "7/6");
  assert.equal(a.text(spinCurvatureCoefficient(a, -1, 1, coefficient)), "7/6");
  assert.equal(a.text(spinCurvatureCoefficient(a, 1, -1, a.zero)), "0");
});

test("metric-skew admission checks the omitted upper triangle and diagonal with explicit signature", () => {
  const a = create();
  assert.equal(validateMetricSkew(a, a.fromText([["0", "3"], ["3", "0"]]), [1, -1]), true);
  assert.equal(validateMetricSkew(a, a.fromText([["0", "3"], ["-3", "0"]]), [1, 1]), true);
  assert.equal(validateMetricSkew(a, a.fromText([["0", "-2/3"], ["2/3", "0"]]), [-1, -1]), true);
  assert.throws(() => validateMetricSkew(a, a.fromText([["0", "4"], ["3", "0"]]), [1, -1]), /metric-skew/);
  assert.throws(() => validateMetricSkew(a, a.fromText([["1", "3"], ["3", "0"]]), [1, -1]), /zero diagonal/);
  assert.throws(() => validateMetricSkew(a, a.fromText([["0", "3"], ["3", "1"]]), [1, -1]), /zero diagonal/);
  assert.throws(() => validateMetricSkew(a, a.identity(2), [1]), /signature/);
  assert.throws(() => validateMetricSkew(a, a.identity(2), [1, 0]), /signature/);
});

test("lowered curvature admission checks both ignored halves and both kinds of diagonal", () => {
  const a = create();
  const entry = (i, j, c, d) => i === j || c === d ? a.zero : a.number((i < j ? 1 : -1) * (c < d ? 3 : -3));
  assert.equal(validateLoweredCurvature(a, blocks(a, entry)), true);
  const changed = (at, value) => blocks(a, (i, j, c, d) => [i, j, c, d].every((x, k) => x === at[k]) ? a.number(value) : entry(i, j, c, d));
  assert.throws(() => validateLoweredCurvature(a, changed([1, 0, 0, 1], -4)), /first-pair/);
  const ignoredLastHalf = blocks(a, (i, j, c, d) => i !== j && c === 1 && d === 0 ? a.number(i < j ? -4 : 4) : entry(i, j, c, d));
  assert.throws(() => validateLoweredCurvature(a, ignoredLastHalf), /last-pair/);
  assert.throws(() => validateLoweredCurvature(a, changed([0, 0, 0, 1], 1)), /zero diagonal blocks/);
  // Keep first-pair antisymmetry intact while corrupting a last-pair diagonal.
  const diagonal = blocks(a, (i, j, c, d) => i !== j && c === 0 && d === 0 ? a.number(i < j ? 1 : -1) : entry(i, j, c, d));
  assert.throws(() => validateLoweredCurvature(a, diagonal), /zero diagonal entries/);
});

test("complete symmetry admission reserves its matrix work and never claims process-memory proof", () => {
  const a = create({ matrixObjects: 1 }), matrix = a.identity(2), before = a.snapshot();
  assert.throws(() => validateMetricSkew(a, matrix, [1, -1]), /matrixObjects ceiling/);
  assert.equal(a.snapshot().scalarOperations, before.scalarOperations); assert.equal(a.snapshot().failed, true);
  const b = create({ matrixObjects: 4 }), input = blocks(b);
  assert.throws(() => validateLoweredCurvature(b, input), /matrixObjects ceiling/);
  assert.equal(b.snapshot().failed, true); assert.equal(b.snapshot().processMemoryProved, false);
});

test("dimension, ownership and four-map metadata are validated before arithmetic", () => {
  const a = create(), metric = a.identity(2), input = blocks(a), maps = identityMaps(a), before = a.snapshot();
  assert.throws(() => lowerCurvature(a, metric, []), /dimension/);
  assert.throws(() => lowerCurvature(a, metric, [[input[0][0]]]), /dimension/);
  assert.throws(() => transformCovariant4(a, input, maps.slice(0, 3)), /four/);
  assert.throws(() => transformCovariant4(a, [input[0]], maps), /square/);
  assert.throws(() => variationLoweredCurvature(a, metric, metric, input, input, maps, []), /four/);
  assert.throws(() => spinConnectionCoefficient(a, 0, a.one), /signature/);
  assert.throws(() => spinCurvatureCoefficient(a, 1, 2, a.one), /signature/);
  // These are rejection-only checks: no complete 14D spin coefficients run.
  assert.throws(() => spinConnectionTensor(a, metric, [metric, metric]), /dimension 14/);
  assert.throws(() => spinCurvatureTensor(a, input), /dimension/);
  assert.deepEqual(a.snapshot(), before, "malformed outer metadata performs no scalar or matrix arithmetic");
  assert.throws(() => lowerCurvature({}, metric, input), /GeometryAlgebra/);
  const other = create();
  assert.throws(() => lowerCurvature(a, other.identity(2), input), /foreign/);
  assert.equal(a.snapshot().failed, true); assert.equal(other.snapshot().failed, false);
});

test("lowering and four-slot transforms admit matrices before output entry arithmetic", () => {
  const a = create({ matrixObjects: 5 }), metric = a.identity(2), input = blocks(a), before = a.snapshot();
  assert.throws(() => lowerCurvature(a, metric, input), /matrixObjects ceiling/);
  assert.equal(a.snapshot().scalarOperations, before.scalarOperations); assert.equal(a.snapshot().failed, true);
  const b = create({ matrixObjects: 10 }), tensor = blocks(b), maps = identityMaps(b), start = b.snapshot();
  // Eight inputs, two transposes admitted, then the first packed-plane
  // allocation fails before its entry callback or any scalar contraction.
  assert.throws(() => transformCovariant4(b, tensor, maps), /matrixObjects ceiling/);
  assert.equal(b.snapshot().matrixObjects, 10); assert.equal(b.snapshot().scalarOperations, start.scalarOperations);
  assert.equal(b.snapshot().failed, true); assert.equal(b.snapshot().processMemoryProved, false);
});
