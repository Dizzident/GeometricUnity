"use strict";

// Manufactured 1/2/3-dimensional rational arithmetic only. These tests import
// the bounded algebra, never source geometry, tensor coefficients, or a study
// evaluator. Counter assertions concern logical admission charges, not RSS.
const test = require("node:test");
const assert = require("node:assert/strict");
const { GeometryAlgebra } = require("../a68-geometry-algebra");

const limits = overrides => ({ maxBits: 256, scalarOperations: 100000,
  rationalObjects: 100000, matrixObjects: 1000, matrixCells: 10000,
  slotVisits: 100000, ...overrides });
const create = overrides => new GeometryAlgebra(limits(overrides));
const poisoned = a => {
  assert.equal(a.snapshot().failed, true);
  assert.throws(() => a.sum(), /poisoned/);
};

test("geometry algebra requires explicit complete finite admission limits", () => {
  for (const value of [undefined, {}, { ...limits(), extra: 1 },
    limits({ maxBits: 15 }), limits({ maxBits: 1048577 }),
    limits({ matrixCells: 0 }), limits({ slotVisits: Infinity }),
    limits({ scalarOperations: 1.5 }), limits({ rationalObjects: Number.MAX_SAFE_INTEGER + 1 })]) {
    assert.throws(() => new GeometryAlgebra(value), /limits|bit limit/);
  }
  const incomplete = limits(); delete incomplete.matrixObjects;
  assert.throws(() => new GeometryAlgebra(incomplete), /limits/);
  assert.throws(() => create({ rationalObjects: 1 }), /rationalObjects ceiling/);
  const a = create();
  assert.deepEqual(a.snapshot(), { scalarOperations: 0, rationalObjects: 2,
    matrixObjects: 0, matrixCells: 0, slotVisits: 0, failed: false,
    processMemoryProved: false });
  assert.ok(Object.isFrozen(a)); assert.ok(Object.isFrozen(a.snapshot()));
});

test("geometry algebra performs exact reduced signed fraction operations", () => {
  const a = create(), half = a.parse("1/2"), negative = a.parse("-2/3");
  assert.equal(a.text(a.add(half, negative)), "-1/6");
  assert.equal(a.text(a.subtract(half, negative)), "7/6");
  assert.equal(a.text(a.multiply(half, negative)), "-1/3");
  assert.equal(a.text(a.reciprocal(negative)), "-3/2");
  assert.equal(a.text(a.negate(negative)), "2/3");
  assert.equal(a.text(a.sum(half, negative, a.parse("1/6"))), "0");
  assert.equal(a.text(a.sum()), "0");
  assert.ok(a.isZero(a.multiply(a.zero, negative)));
  assert.ok(a.equal(a.number(-7), a.parse("-7")));
  assert.ok(Object.isFrozen(half));
  assert.throws(() => { half[0] = 9n; }, TypeError);
});

test("geometry algebra rejects noncanonical fractions and unsafe integer inputs", () => {
  for (const text of ["-0", "01", "+1", "1.0", "1e2", " 1", "1\n", "1/0",
    "1/-2", "2/4", "0/2", "1/1", "-0/1", "1/02", 1, null]) {
    const a = create();
    assert.throws(() => a.parse(text), /canonical/); poisoned(a);
  }
  for (const number of [NaN, Infinity, 1.5, Number.MAX_SAFE_INTEGER + 1]) {
    const a = create(); assert.throws(() => a.number(number), /safe exact integer/); poisoned(a);
  }
  const a = create(); assert.throws(() => a.reciprocal(a.zero), /nonzero denominator/); poisoned(a);
});

test("geometry algebra checks parsed bit height before GCD and prospective product height", () => {
  for (const text of ["65536", "65536/2"]) {
    const a = create({ maxBits: 16 });
    assert.throws(() => a.parse(text), /parsed rational bit ceiling before GCD/);
    assert.equal(a.snapshot().rationalObjects, 2, "rejected parse allocates no rational token"); poisoned(a);
  }
  const a = create({ maxBits: 16 }), x = a.parse("256"), before = a.snapshot();
  assert.throws(() => a.multiply(x, x), /pre-product rational bit ceiling/);
  assert.equal(a.snapshot().rationalObjects, before.rationalObjects); poisoned(a);
  const b = create({ maxBits: 16 }), large = b.parse("32767"), inverse = b.parse("1/32767");
  assert.equal(b.text(b.multiply(large, inverse)), "1", "cross cancellation precedes bounded products");
  assert.equal(b.text(b.parse("65535")), "65535", "16-bit canonical input is admitted");
});

test("geometry algebra hand-derived rational 2D products preserve order", () => {
  const a = create(), x = a.fromText([["1/2", "-2/3"], ["3/5", "4"]]),
    y = a.fromText([["2", "3/7"], ["-1/4", "5/6"]]);
  assert.deepEqual(a.matrixText(a.matMultiply(x, y)), [["7/6", "-43/126"], ["1/5", "377/105"]]);
  assert.deepEqual(a.matrixText(a.product(y, x)), [["44/35", "8/21"], ["3/8", "7/2"]]);
  assert.deepEqual(a.matrixText(a.commutator(x, y)), [["-19/210", "-13/18"], ["-7/40", "19/210"]]);
  assert.equal(a.text(a.traceProduct(x, y)), "333/70");
  assert.equal(a.text(a.trace(x)), "9/2");
  assert.deepEqual(a.matrixText(a.transpose(x)), [["1/2", "3/5"], ["-2/3", "4"]]);
  assert.deepEqual(a.matrixText(a.matScale(x, a.parse("-3/2"))), [["-3/4", "1"], ["-9/10", "-6"]]);
  assert.deepEqual(a.matrixText(a.matAdd(x, y)), [["5/2", "-5/21"], ["7/20", "29/6"]]);
  assert.equal(a.product(x), x, "one-factor product retains its validated token");
  assert.ok(a.matrixEqual(a.product(x, a.identity(2), a.identity(2)), x));
});

test("geometry algebra exact 1D and row-pivoted 2D/3D inverses", () => {
  for (const [input, expected] of [
    [[["-2/3"]], [["-3/2"]]],
    [[["0", "2"], ["3", "4"]], [["-2/3", "1/3"], ["1/2", "0"]]],
    [[["0", "0", "2"], ["0", "3", "4"], ["5", "6", "7"]],
      [["1/10", "-2/5", "1/5"], ["-2/3", "1/3", "0"], ["1/2", "0", "0"]]]
  ]) {
    const a = create(), matrix = a.fromText(input), inverse = a.inverse(matrix), identity = a.identity(input.length);
    assert.deepEqual(a.matrixText(inverse), expected);
    assert.ok(a.matrixEqual(a.matMultiply(matrix, inverse), identity));
    assert.ok(a.matrixEqual(a.matMultiply(inverse, matrix), identity));
    assert.deepEqual(a.matrixText(matrix), input, "elimination scratch does not mutate input");
  }
  const a = create(), singular = a.fromText([["1", "2"], ["2", "4"]]);
  assert.throws(() => a.inverse(singular), /nonsingular/); poisoned(a);
});

test("geometry algebra rejects cross-session rationals and matrices including one-factor products", () => {
  const operations = [
    (a, b, x, y) => a.text(b.one), (a, b) => a.equal(a.one, b.one),
    (a, b) => a.add(a.one, b.one), (a, b) => a.multiply(b.one, a.one),
    (a, b) => a.matrix(1, () => b.zero), (a, b, x, y) => a.get(y, 0, 0),
    (a, b, x, y) => a.product(y), (a, b, x, y) => a.product(x, y),
    (a, b, x, y) => a.matAdd(x, y), (a, b, x, y) => a.matScale(x, b.one),
    (a, b, x, y) => a.inverse(y), (a, b, x, y) => a.traceProduct(x, y),
    a => a.text(Object.freeze([1n, 1n])), a => a.product(Object.freeze({ dimension: 1 }))
  ];
  for (const op of operations) {
    const a = create(), b = create(), x = a.identity(1), y = b.identity(1);
    assert.throws(() => op(a, b, x, y), /foreign/); poisoned(a);
    assert.equal(b.snapshot().failed, false, "foreign token owner remains usable");
  }
});

test("geometry algebra rejects malformed dimensions, indices, empty products and mismatches", () => {
  const operations = [a => a.matrix(0), a => a.matrix(15), a => a.matrix(1.5),
    a => a.fromText([]), a => a.fromText([["1", "2"]]), a => a.matrix(1, null),
    a => a.product(), a => a.matAdd(), a => a.get(a.identity(2), 2, 0),
    a => a.get(a.identity(1), 0.5, 0), a => a.matMultiply(a.identity(1), a.identity(2))];
  for (const op of operations) {
    const a = create(); assert.throws(() => op(a), /dimension|square|callback|empty|nonempty|indices/); poisoned(a);
  }
});

test("geometry matrix admission reserves object/cell/slot budgets before entry callbacks", () => {
  for (const [override, prepare, dimension, ceiling] of [
    [{ matrixObjects: 1 }, a => a.identity(1), 1, "matrixObjects"],
    [{ matrixCells: 3 }, () => {}, 2, "matrixCells"],
    [{ slotVisits: 3 }, () => {}, 2, "slotVisits"]
  ]) {
    const a = create(override); prepare(a); let visits = 0;
    assert.throws(() => a.matrix(dimension, () => { visits++; return a.zero; }), new RegExp("pre-operation " + ceiling + " ceiling"));
    assert.equal(visits, 0, "entry callbacks cannot run before full matrix admission");
    for (const [key, cap] of Object.entries(override)) assert.ok(a.snapshot()[key] <= cap);
    poisoned(a);
  }
});

test("geometry multiply and inverse reserve full logical loop work before output arithmetic", () => {
  const a = create({ slotVisits: 15 }), x = a.identity(2), y = a.identity(2), before = a.snapshot();
  assert.throws(() => a.matMultiply(x, y), /slotVisits ceiling/);
  assert.equal(a.snapshot().matrixObjects, before.matrixObjects);
  assert.equal(a.snapshot().scalarOperations, before.scalarOperations); poisoned(a);
  const b = create({ slotVisits: 43 }), z = b.identity(2);
  assert.throws(() => b.inverse(z), /slotVisits ceiling/);
  assert.equal(b.snapshot().scalarOperations, 0, "inverse admission fails before elimination arithmetic");
  assert.equal(b.snapshot().matrixObjects, 3, "two scratch matrices receive conservative precharges"); poisoned(b);
  const c = create(), u = c.identity(2), v = c.identity(2), start = c.snapshot();
  c.matMultiply(u, v); const after = c.snapshot();
  assert.equal(after.slotVisits - start.slotVisits, 12, "eight inner visits plus four output cells");
  assert.equal(after.matrixObjects - start.matrixObjects, 1);
  assert.equal(after.matrixCells - start.matrixCells, 4);
});

test("geometry scalar operation and rational allocation budgets fail closed", () => {
  const a = create({ scalarOperations: 1 }); a.parse("1"); const before = a.snapshot();
  assert.throws(() => a.add(a.one, a.zero), /scalarOperations ceiling/);
  assert.equal(a.snapshot().rationalObjects, before.rationalObjects); poisoned(a);
  const b = create({ rationalObjects: 2 });
  assert.throws(() => b.negate(b.one), /rationalObjects ceiling/);
  assert.equal(b.snapshot().rationalObjects, 2); poisoned(b);
});

test("geometry trace and matrix inspection reserve their logical scan budgets", () => {
  const a = create(), matrix = a.identity(2), start = a.snapshot().slotVisits;
  assert.equal(a.text(a.trace(matrix)), "2");
  assert.equal(a.snapshot().slotVisits - start, 2);
  a.matrixText(matrix); assert.equal(a.snapshot().slotVisits - start, 6);
  assert.ok(a.matrixEqual(matrix, matrix)); assert.equal(a.snapshot().slotVisits - start, 10);
  for (const [cap, operation] of [[5, a => a.trace(a.identity(2))],
    [7, a => a.matrixText(a.identity(2))],
    [11, a => a.matrixEqual(a.identity(2), a.identity(2))]]) {
    const b = create({ slotVisits: cap });
    assert.throws(() => operation(b), /slotVisits ceiling/); poisoned(b);
    assert.equal(b.snapshot().scalarOperations, 0, "scan admission precedes arithmetic");
  }
});

test("geometry entry callback exceptions preserve the original failure and poison the context", () => {
  const a = create(), failure = new Error("manufactured callback failure"); let visits = 0;
  assert.throws(() => a.matrix(2, (i, j) => {
    visits++;
    if (i === 0 && j === 1) throw failure;
    return a.one;
  }), error => error === failure);
  assert.equal(visits, 2, "callback traversal stops immediately at the failed entry");
  assert.equal(a.snapshot().matrixCells, 4, "full matrix reservation remains charged");
  poisoned(a);
});

test("geometry poisoned contexts reject zero-argument and read-only escape paths", () => {
  const a = create(), matrix = a.identity(1), one = a.one;
  assert.throws(() => a.parse("2/4"), /canonical/);
  const before = a.snapshot();
  for (const operation of [() => a.sum(), () => a.product(matrix), () => a.text(one),
    () => a.isZero(one), () => a.equal(one, one), () => a.size(matrix),
    () => a.get(matrix, 0, 0), () => a.matrixText(matrix), () => a.parse("1"),
    () => a.identity(1), () => a.trace(matrix), () => a.transpose(matrix)]) {
    assert.throws(operation, /poisoned/);
  }
  assert.deepEqual(a.snapshot(), before, "no arithmetic or allocation charges after poison");
  assert.equal(before.processMemoryProved, false);
});
