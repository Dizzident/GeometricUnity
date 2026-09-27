"use strict";

// Manufactured complete local tensors only; never call the source-bound
// positive wrapper or evaluate the actual retained S_n/X coefficients.
const test = require("node:test"), assert = require("node:assert/strict");
const { reconstructPolynomial, reconstructRetainedPolynomial, reconstructedPolynomialIdentity } = require("../a68-polynomial-reconstruction");
const row = (form, blade, real, imaginary = "0") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const arithmetic = overrides => ({ maxBits: 256, scalarOperations: 100000, rationalObjects: 100000,
  matrixObjects: 1, matrixCells: 1, slotVisits: 1, ...overrides });
const limits = overrides => ({ inputRecords: 1000, outputRecords: 1000, recordVisits: 10000,
  readCharacters: 100000, outputCharacters: 100000, rationalCharacters: 100, ...overrides });
const run = (coefficients, lambda, expected, caps, arithmeticCaps) => reconstructPolynomial(coefficients, lambda, expected, arithmetic(arithmeticCaps), limits(caps));
const ones = () => Array.from({ length: 5 }, () => [row(1, 1, "1")]);

test("complete polynomial uses powers1through5 without factorials or an order-zero term", () => {
  const result = run(ones(), "1/2", [row(1, 1, "31/32")]);
  assert.deepEqual(result.tensor, [row(1, 1, "31/32")]); assert.equal(result.norm, "31/32");
  assert.equal(result.usage.inputRecords, 6); assert.equal(result.usage.recordVisits, 7);
  assert.equal(result.peakRecords, 1); assert.ok(Object.isFrozen(result.tensor[0]));
  assert.equal(result.scope.upstreamRecurrenceReplayEstablished, false); assert.equal(result.scope.exactStationaryRoot, false);
  assert.equal(result.scope.physicalMassPrediction, false);
  assert.throws(() => run(ones(), "1/2", [row(1, 1, "31/16")]), /coefficient mismatch/);
});

test("full carrier retains grade-five and central imaginary terms with unsigned L1 norm", () => {
  const coefficients = ones(); coefficients[1].push(row(2, 0, "0", "8")); coefficients[4].push(row(1, 31, "3"));
  const expected = [row(1, 1, "31/32"), row(1, 31, "3/32"), row(2, 0, "0", "2")];
  const result = run(coefficients, "1/2", expected);
  assert.deepEqual(result.tensor, expected); assert.equal(result.norm, "49/16");
  assert.throws(() => run(coefficients, "1/2", [expected[0], expected[2]]), /complete polynomial support/);
  assert.throws(() => run(coefficients, "1/2", [...expected, row(4, 1, "1")]), /coefficient mismatch/);
});

test("negative powers, exact cancellation and absent coefficients preserve canonical sparse equality", () => {
  const coefficients = [[row(1, 1, "2")], [row(1, 1, "4"), row(2, 3, "-3")], [], [], []];
  const result = run(coefficients, "-1/2", [row(2, 3, "-3/4")]);
  assert.equal(result.norm, "3/4"); assert.equal(result.peakRecords, 1); assert.equal(result.tensor.length, 1);
  assert.deepEqual(run(ones(), "0", []).tensor, []);
  assert.deepEqual(run([[], [], [], [], []], "1/3", []).tensor, []);
  assert.throws(() => run(coefficients, "-1/2", [row(1, 1, "0"), row(2, 3, "-3/4")]), /zero omitted/);
});

test("H-anti check uses only blade grade and admits every allowed real/imaginary direction", () => {
  // One manufactured blade per grade checks both allowed and forbidden
  // real/imaginary directions; this does not enumerate all16384 blades.
  for (let grade = 0; grade <= 14; grade++) {
    const blade = (1 << grade) - 1, realType = (grade * (grade + 1) / 2) % 2 === 1;
    const value = row(128, blade, realType ? "-2" : "0", realType ? "0" : "-2");
    assert.equal(run([[value], [], [], [], []], "1", [value]).norm, "2");
    const wrong = row(128, blade, realType ? "0" : "1", realType ? "1" : "0");
    assert.throws(() => run([[wrong], [], [], [], []], "1", [wrong]), /H-anti/);
  }
});

test("typed inputs reject duplicates, wrong form degree, nonlocal modes and malformed rationals", () => {
  for (const invalid of [row(3, 1, "1"), row(0, 1, "1"), row(1, 16384, "1"),
    { ...row(1, 1, "1"), k0: 1 }, { ...row(1, 1, "1"), k1: -0 }, row(1, 1, "2/4"), row(1, 1, "-0"),
    { ...row(1, 1, "1"), extra: true }]) assert.throws(() => run([[invalid], [], [], [], []], "1", []), /domain|canonical|object/);
  assert.throws(() => run([[row(1, 1, "1"), row(1, 1, "2")], [], [], [], []], "1", []), /unique tensor order/);
  assert.throws(() => run([[row(2, 1, "1"), row(1, 1, "2")], [], [], [], []], "1", []), /unique tensor order/);
  assert.throws(() => run(ones().slice(1), "1", []), /dense array/);
  assert.throws(() => run([new Array(1), [], [], [], []], "1", []), /dense array/);
});

test("accessor fields and array indices are rejected before invocation", () => {
  let invoked = false; const record = { ...row(1, 1, "1") };
  Object.defineProperty(record, "real", { enumerable: true, get() { invoked = true; return "1"; } });
  assert.throws(() => run([[record], [], [], [], []], "1", []), /own data fields/);
  const array = []; Object.defineProperty(array, "0", { enumerable: true, get() { invoked = true; return row(1, 1, "1"); } });
  assert.throws(() => run([array, [], [], [], []], "1", []), /own data indices/); assert.equal(invoked, false);
});

test("prospective caps guard total input scans, intermediate support and rational work", () => {
  assert.throws(() => run(ones(), "1", [row(1, 1, "5")], { inputRecords: 5 }), /inputRecords ceiling/);
  assert.throws(() => run(ones(), "1", [row(1, 1, "5")], { recordVisits: 1 }), /recordVisits ceiling/);
  assert.throws(() => run(ones(), "1", [row(1, 1, "5")], { readCharacters: 1 }), /readCharacters ceiling/);
  assert.throws(() => run(ones(), "1", [row(1, 1, "5")], { outputCharacters: 1 }), /outputCharacters ceiling/);
  const coefficients = [[row(1, 1, "1"), row(2, 1, "1")], [row(2, 1, "-1")], [], [], []];
  assert.throws(() => run(coefficients, "1", [row(1, 1, "1")], { outputRecords: 1 }), /live polynomial support/);
  assert.throws(() => run(ones(), "256", [], undefined, { maxBits: 16 }), /pre-product rational bit ceiling/);
  assert.throws(() => run(ones(), "1", [], { outputRecords: Infinity }), /finite reconstruction/);
});

test("source wrapper and result capabilities reject copied hash-bearing lookalikes", () => {
  const fake = Object.freeze({ point: 0, gamma: 1, kappa: 907712, lambda: "1/907712", source: { sha256: "0".repeat(64) } });
  assert.throws(() => reconstructRetainedPolynomial(fake, fake, arithmetic(), limits()), /pinned stationary object identity/);
  assert.throws(() => reconstructedPolynomialIdentity(fake), /complete reconstructed polynomial identity/);
  assert.throws(() => reconstructedPolynomialIdentity(run(ones(), "1", [row(1, 1, "5")])), /complete reconstructed polynomial identity/);
});
