"use strict";

// Manufactured coefficients ONLY. These fixtures do not reconstruct/evaluate
// any source geometry, production tensor graph, or scientific study.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { TOP_SCHEMA } = require("../a68-original-action-recipe");
const { createScalarReplay } = require("../a68-scalar-replay");
const freezeRows = rows => Object.freeze(rows.map(row => Object.freeze(row)));
const row = (form, blade, real, imaginary = "0") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const constant = value => ({ op: "constant", value });
const pair = (left, right) => ({ op: "pair", left, right });
const add = (left, right) => ({ op: "add", left, right });
const multiply = (left, right) => ({ op: "multiply", left, right });
const root = (name, expression) => ({ name, expression });
const top = (node, weight = "1/2") => ({ schemaVersion: TOP_SCHEMA, tensor: {}, node, degree: 14, form: 16383,
  blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight });
const plan = degrees => {
  const leaves = degrees.map((degree, i) => ({ id: "t" + i, degree, source: "manufactured-only", sha256: "0".repeat(64) }));
  const r = new MixedRecipe({ leaves }); leaves.forEach(leaf => r.RegisterLeaf(leaf.id)); return r.Finish();
};
const scheduleLimits = overrides => ({ tensorNodes: 100, tensorEdges: 1000, tensorMarks: 100,
  scalarNodes: 1000, scalarEdges: 1000, roots: 100, geometryMatrices: 100,
  geometryEntries: 1000, maxDepth: 64, stringCharacters: 100000, rationalCharacters: 100, ...overrides });
const arithmeticLimits = overrides => ({ maxBits: 128, scalarOperations: 100000, rationalObjects: 100000,
  matrixObjects: 1, matrixCells: 1, slotVisits: 10000, ...overrides });
const replayLimits = overrides => ({ tensorRecords: 100, tensorResolutions: 1000, recordVisits: 10000,
  coefficientReads: 10000, readCharacters: 100000, outputCharacters: 100000,
  rationalCharacters: 100, liveScalars: 1000, ...overrides });
function setup(degrees, roots, rows = [], overrides = {}) {
  const results = [], calls = [];
  const options = { tensorPlan: plan(degrees), namedRoots: roots, geometry: [],
    scheduleLimits: scheduleLimits(), arithmeticLimits: arithmeticLimits(), replayLimits: replayLimits(),
    resolveTensor: id => { calls.push(id); return rows[id]; },
    compareRoot: (name, value) => { results.push([name, value]); return true; }, ...overrides };
  const replay = createScalarReplay(options);
  return { replay, results, calls, run() { for (let i = -1; i < degrees.length; i++) replay.advance(i); return replay.finish(); } };
}
const assertPoison = replay => {
  assert.equal(replay.snapshot().failed, true); assert.equal(replay.snapshot().liveScalars, 0);
  assert.throws(() => replay.advance(replay.snapshot().nextStep), /poisoned/);
  assert.throws(() => replay.finish(), /poisoned/);
};

test("scalar replay computes exact constant/matrix/add/multiply expressions before tensors", () => {
  const entry = { op: "matrixEntry", matrix: "manufactured", row: 0, column: 1 };
  const expression = multiply(add(constant("1/2"), entry), constant("-3/5"));
  const f = setup([], [root("value", expression)], [], { geometry: [{ id: "manufactured", entries: [{ row: 0, column: 1, value: "2/3" }] }] });
  const result = f.run(); assert.deepEqual(f.results, [["value", "-7/10"]]); assert.deepEqual(f.calls, []);
  assert.equal(result.complete, true); assert.equal(result.liveScalars, 0); assert.equal(result.scalarOperationsExecuted, 5);
  assert.equal(result.tensorSourceAuthenticationProved, false); assert.equal(result.outerTensorConversionStorageProved, false);
  assert.equal(result.totalProcessMemoryProved, false); assert.ok(Object.isFrozen(f.replay.schedule));
});

test("scalar replay central iI pairs without complex conjugation", () => {
  const rows = freezeRows([row(0, 0, "0", "1")]);
  const f = setup([0], [root("iIsquare", pair(0, 0))], [rows]); f.run();
  assert.deepEqual(f.results, [["iIsquare", "1"]]); assert.deepEqual(f.calls, [0, 0], "one resolver call per operand");
});

test("scalar replay exterior signature and Clifford-square signs match hand cases", () => {
  for (const [form, blade, expected] of [[1, 0, "-6"], [128, 0, "6"], [1, 128, "6"],
    [128, 128, "-6"], [1, 3, "6"], [128, 3, "-6"], [1, 129, "-6"]]) {
    const f = setup([1, 1], [root("pair", pair(0, 1))], [freezeRows([row(form, blade, "2")]), freezeRows([row(form, blade, "3")])]);
    f.run(); assert.deepEqual(f.results, [["pair", expected]], "form=" + form + ", blade=" + blade);
  }
});

test("scalar replay permits opposite imaginary contributions only after complete trace cancellation", () => {
  const f = setup([1, 1], [root("cancel", pair(0, 1))], [
    freezeRows([row(1, 0, "2", "1"), row(1, 1, "2", "-1")]),
    freezeRows([row(1, 0, "1", "1"), row(1, 1, "1", "-1")])]);
  f.run(); assert.deepEqual(f.results, [["cancel", "-2"]]);
  const bad = setup([1, 1], [root("nonreal", pair(0, 1))], [freezeRows([row(1, 0, "2", "1")]), freezeRows([row(1, 0, "1", "1")])]);
  bad.replay.advance(-1); bad.replay.advance(0);
  assert.throws(() => bad.replay.advance(1), /complete Pair imaginary/); assert.deepEqual(bad.results, []); assertPoison(bad.replay);
});

test("scalar replay Pair merges full canonical supports and ignores unmatched trace terms", () => {
  const f = setup([1, 1], [root("pair", pair(0, 1))], [
    freezeRows([row(1, 0, "1/2"), row(1, 3, "9", "2"), row(2, 0, "5")]),
    freezeRows([row(1, 0, "2/3"), row(1, 2, "0", "3"), row(4, 0, "7")])]);
  f.run(); assert.deepEqual(f.results, [["pair", "-1/3"]]);
});

test("scalar replay TOP selects scalar blade, enforces selected reality, then negates and weights", () => {
  const f = setup([14], [root("density", top(0, "1/3"))], [freezeRows([row(16383, 0, "9/2"), row(16383, 1, "0", "7")])]);
  f.run(); assert.deepEqual(f.results, [["density", "-3/2"]]); assert.deepEqual(f.calls, [0]);
  const absent = setup([14], [root("absent", top(0))], [freezeRows([row(16383, 1, "0", "2")])]);
  absent.run(); assert.deepEqual(absent.results, [["absent", "0"]]);
  const bad = setup([14], [root("bad", top(0))], [freezeRows([row(16383, 0, "1", "1")])]);
  bad.replay.advance(-1); assert.throws(() => bad.replay.advance(0), /selected TOP/); assertPoison(bad.replay);
});

test("scalar replay handles empty typed and untyped zero tensors without numerical assumptions", () => {
  const r = new MixedRecipe(); r.Zero(); const tensorPlan = r.Finish();
  const f = setup([0], [root("pair", pair(0, 0)), root("top", top(0))], [freezeRows([])], { tensorPlan });
  f.run(); assert.deepEqual(f.results, [["pair", "0"], ["top", "0"]]);
  const bad = setup([0], [root("bad", pair(0, 0))], [freezeRows([row(0, 0, "1")])], { tensorPlan });
  bad.replay.advance(-1); assert.throws(() => bad.replay.advance(0), /untyped zero/); assertPoison(bad.replay);
});

test("scalar replay retains delayed tensor0 through unrelated tensor10 and releases shared scalars", () => {
  const current = multiply(pair(0, 10), constant("1/2")), live = new Map(), comparisons = [];
  const roots = [root("current", current), root("double", add(current, current)), root("currentAgain", current)];
  const f = setup(Array(11).fill(1), roots, [], { resolveTensor: id => { assert.ok(live.has(id)); return live.get(id); },
    compareRoot: (name, value, descriptor) => { comparisons.push([name, value, descriptor.readyAfter]); return true; } });
  f.replay.advance(-1);
  for (let step = 0; step < 11; step++) {
    live.set(step, freezeRows([row(1, 0, String(step + 1))]));
    const release = f.replay.advance(step);
    assert.ok(Object.isFrozen(release)); release.forEach(id => assert.ok(live.delete(id)));
    if (step < 10) assert.ok(live.has(0));
  }
  const done = f.replay.finish(); assert.equal(live.size, 0); assert.equal(done.liveScalars, 0);
  assert.deepEqual(comparisons, [["current", "-11/2", 10], ["currentAgain", "-11/2", 10], ["double", "-11", 10]]);
  assert.equal(done.rootsCompared, 3); assert.equal(done.scalarOperationsExecuted, 4);
});

test("scalar replay rejects mutable, duplicate, wrong-degree, nonlocal and malformed full rows", () => {
  const badRows = [ [row(1, 0, "1")], Object.freeze([row(1, 0, "1")]),
    freezeRows([row(1, 0, "1"), row(1, 0, "2")]), freezeRows([row(3, 0, "1")]),
    freezeRows([{ ...row(1, 0, "1"), k0: 1 }]), freezeRows([{ ...row(1, 0, "1"), k1: -0 }]),
    freezeRows([row(1, 0, "2/4")]), freezeRows([row(1, 0, "0")]),
    freezeRows([{ ...row(1, 0, "1"), extra: 0 }]), Object.freeze(new Array(1)),
    freezeRows([row(1, 0, "1"), row(1, 2, "01")]) ];
  for (const rows of badRows) {
    const f = setup([1], [root("bad", pair(0, 0))], [rows]); f.replay.advance(-1);
    assert.throws(() => f.replay.advance(0), /frozen|canonical|degree|frequency|zero|keys|dense/); assertPoison(f.replay);
  }
  assert.throws(() => setup([1, 2], [root("bad", pair(0, 1))]), /Pair degree/);
});

test("scalar replay frozen arrays cannot swap row identities through getters or hide extra keys", () => {
  const first = Object.freeze(row(1, 0, "1")), second = Object.freeze(row(1, 0, "99"));
  let reads = 0; const accessor = [];
  Object.defineProperty(accessor, "0", { enumerable: true, get() { return ++reads === 1 ? first : second; } });
  Object.freeze(accessor);
  const hidden = [first]; Object.defineProperty(hidden, "hidden", { value: "unexpected" }); Object.freeze(hidden);
  const symbol = [first]; symbol[Symbol("unexpected")] = true; Object.freeze(symbol);
  for (const rows of [accessor, hidden, symbol]) {
    const f = setup([1], [root("bad", pair(0, 0))], [rows]); f.replay.advance(-1);
    assert.throws(() => f.replay.advance(0), /own data properties|extra keys/); assertPoison(f.replay);
  }
  assert.equal(reads, 0, "getter rejected without invocation");
});

test("scalar replay resolver and root-comparison failures preserve exceptions and poison", () => {
  const failure = new Error("manufactured resolver failure");
  const f = setup([1], [root("bad", pair(0, 0))], [], { resolveTensor: () => { throw failure; } });
  f.replay.advance(-1); assert.throws(() => f.replay.advance(0), error => error === failure); assertPoison(f.replay);
  const reject = setup([], [root("bad", constant("1"))], [], { compareRoot: () => false });
  assert.throws(() => reject.replay.advance(-1), /comparison rejected/); assertPoison(reject.replay);
  const thrown = setup([], [root("bad", constant("1"))], [], { compareRoot: () => { throw failure; } });
  assert.throws(() => thrown.replay.advance(-1), error => error === failure); assertPoison(thrown.replay);
});

test("scalar replay strict lifecycle and swallowed reentrant callback failures remain poisoned", () => {
  for (const action of [r => r.advance(0), r => r.finish()]) {
    const f = setup([1], []); assert.throws(() => action(f.replay), /sequence|census/); assertPoison(f.replay);
  }
  const repeat = setup([1], []); repeat.replay.advance(-1);
  assert.throws(() => repeat.replay.advance(-1), /sequence/); assertPoison(repeat.replay);
  let replay;
  const f = setup([], [root("reentrant", constant("1"))], [], { compareRoot: () => {
    assert.throws(() => replay.finish(), /reentrant/); return true;
  } }); replay = f.replay;
  assert.throws(() => replay.advance(-1), /poisoned/); assertPoison(replay);
  const finished = setup([], []); finished.run();
  assert.throws(() => finished.replay.finish(), /completed/); assertPoison(finished.replay);
});

test("scalar replay rejects swallowed resolver reentrancy and cannot accept an observed schedule", () => {
  let replay;
  const f = setup([1], [root("pair", pair(0, 0))], [], { resolveTensor: () => {
    assert.throws(() => replay.advance(0), /reentrant/); return freezeRows([row(1, 0, "1")]);
  } }); replay = f.replay; replay.advance(-1);
  assert.throws(() => replay.advance(0), /resolver poisoned/); assertPoison(replay);
  assert.throws(() => setup([], [], [], { schedule: { nodes: [] } }), /closed object keys/);
});

test("scalar replay guards tensor counts and loop/read budgets before corresponding work", () => {
  const rows = freezeRows([row(1, 0, "1"), row(1, 1, "2")]);
  for (const [overrides, pattern] of [[{ tensorRecords: 1 }, /bounded tensor/], [{ recordVisits: 1 }, /recordVisits ceiling/],
    [{ coefficientReads: 1 }, /coefficientReads ceiling/], [{ readCharacters: 1 }, /readCharacters ceiling/],
    [{ tensorResolutions: 1 }, /tensorResolutions ceiling/]]) {
    const f = setup([1], [root("bad", pair(0, 0))], [rows], { replayLimits: replayLimits(overrides) });
    f.replay.advance(-1); assert.throws(() => f.replay.advance(0), pattern); assertPoison(f.replay);
    if (overrides.recordVisits || overrides.coefficientReads || overrides.tensorRecords)
      assert.equal(f.replay.snapshot().arithmetic.scalarOperations, 0, "admission precedes coefficient parse");
    if (overrides.tensorResolutions) assert.equal(f.calls.length, 1, "second resolver not invoked beyond cap");
  }
});

test("scalar replay uses explicit bounded arithmetic, output and live-scalar admission", () => {
  const f = setup([], [root("overflow", multiply(constant("256"), constant("256")))], [], { arithmeticLimits: arithmeticLimits({ maxBits: 16 }) });
  assert.throws(() => f.replay.advance(-1), /pre-product rational bit ceiling/); assertPoison(f.replay);
  const output = setup([], [root("output", constant("1"))], [], { replayLimits: replayLimits({ outputCharacters: 1 }) });
  assert.throws(() => output.replay.advance(-1), /outputCharacters ceiling/); assert.deepEqual(output.results, []); assertPoison(output.replay);
  const height = setup([], [root("height", add(constant("9"), constant("9")))], [], { replayLimits: replayLimits({ rationalCharacters: 1 }) });
  assert.throws(() => height.replay.advance(-1), /serialized height/); assertPoison(height.replay);
  assert.throws(() => setup([], [root("live", add(constant("1"), constant("2")))], [], { replayLimits: replayLimits({ liveScalars: 2 }) }), /prospective live scalar/);
  assert.throws(() => setup([], [], [], { replayLimits: { ...replayLimits(), extra: 1 } }), /keys/);
  assert.throws(() => setup([], [], [], { replayLimits: replayLimits({ recordVisits: Infinity }) }), /finite/);
});
