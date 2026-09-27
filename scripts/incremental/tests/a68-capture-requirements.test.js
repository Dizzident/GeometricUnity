"use strict";
// Manufactured symbolic plans only. No source geometry, tensor coefficients or
// source-positive computational plan is constructed or authenticated here.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe, SCHEMA, PLAN_SCHEMA } = require("../a68-mixed-recipe");
const { sourceContextIds } = require("../a68-source-context-menu");
const { deriveCaptureShape, createCaptureRequirementCensus } = require("../a68-capture-requirements");
const ids = sourceContextIds(), clone = x => JSON.parse(JSON.stringify(x));
const metadata = { nodes: 1000000, arraySlots: 1000000, stringCharacters: 10000000, maxDepth: 16 };
const zero = { graphs: 0, nodeObjects: 0, arrays: 0, slots: 0, elementCopies: 0 };
const one = { graphs: 1, nodeObjects: 2, arrays: 5, slots: 6, elementCopies: 6 };
const scaled = n => Object.fromEntries(Object.entries(one).map(([k, v]) => [k, n * v]));
const caps = { normal: scaled(705), inspection: scaled(5), failure: scaled(2) };
const policy = () => ids.map((id, i) => ({ id, copies: i === 0 ? 3 : i === 704 ? 2 : 0 }));
function plan() {
  const r = new MixedRecipe({ leaves: [{ id: "leaf", degree: 1, source: "manufactured-only", sha256: "0".repeat(64) }] });
  const a = r.RegisterLeaf("leaf"), b = r.Sum(a, a); r.Mark("retained", 1, b, false); return r.Finish();
}
function complete(c = createCaptureRequirementCensus(policy(), metadata), p = plan()) { for (const id of ids) c.add(id, p); return c; }

test("exact copy shape includes declarations marks and every repeated ordered input", () => {
  const result = deriveCaptureShape(plan(), metadata);
  assert.deepEqual(result.shape, { leaves: 1, nodes: 2, marks: 1, inputReferences: 2 }); assert.deepEqual(result.copies, one);
  assert.equal(result.scope.sourceAuthenticityEstablished, false); assert.equal(result.scope.productionResourceSufficiencyProved, false);
  assert.equal(result.scope.scientificExecutionAuthorized, false); assert.ok(Object.isFrozen(result.copies));
});
test("empty symbolic graph still needs one graph and three arrays", () => {
  assert.deepEqual(deriveCaptureShape(new MixedRecipe().Finish(), metadata).copies, { ...zero, graphs: 1, arrays: 3 });
});
test("all705 completion copies sum while terminal failure covers only a point plus child", () => {
  const c = complete(), result = c.finish(caps);
  assert.equal(result.contexts, 705); assert.deepEqual(result.requirements, caps);
  assert.equal(c.snapshot().finished, true); assert.equal(result.scope.totalProcessMemoryProved, false);
  assert.ok(Object.isFrozen(result.requirements.failure)); assert.throws(() => c.add(ids[0], plan()), /finished/);
});
test("terminal failure maximizes each dimension over diagnostics and both point families", () => {
  const c = createCaptureRequirementCensus(ids.map(id => ({ id, copies: 0 })), metadata);
  const small = plan(), r = new MixedRecipe(); let value = r.Unit(1, 1);
  for (let i = 0; i < 8; i++) value = r.Sum(value, value, value); r.Mark("large", 1, value, true);
  const large = r.Finish(), cost = deriveCaptureShape(large, metadata).copies;
  for (const id of ids) c.add(id, id === "diagnostic/acceleration" ? large : small);
  const generous = Object.fromEntries(["normal", "inspection", "failure"].map(k => [k, Object.fromEntries(Object.keys(one).map(f => [f, 1000000]))]));
  const result = c.finish(generous);
  assert.equal(result.requirements.failure.graphs, 2); assert.equal(result.requirements.failure.nodeObjects, cost.nodeObjects);
  assert.equal(result.requirements.failure.slots, cost.slots); assert.deepEqual(result.requirements.inspection, zero);
});
test("every one-short aggregate lane dimension is refused with no repair", () => {
  for (const lane of Object.keys(caps)) for (const dimension of Object.keys(one)) {
    const c = complete(), bad = clone(caps); bad[lane][dimension]--;
    assert.throws(() => c.finish(bad), new RegExp(lane + "/" + dimension)); assert.equal(c.snapshot().failed, true);
    assert.throws(() => c.finish(caps), /failed/);
  }
});
test("inspection policy is complete ordered finite and frozen before plan ingestion", () => {
  for (const mutate of [p => p.pop(), p => p.reverse(), p => p[0].copies = -1, p => p[0].copies = Number.MAX_SAFE_INTEGER + 1, p => p[0].extra = 0]) {
    const p = policy(); mutate(p); assert.throws(() => createCaptureRequirementCensus(p, metadata));
  }
  const p = policy(), c = createCaptureRequirementCensus(p, metadata); p[0].copies = 100;
  assert.deepEqual(complete(c).finish(caps).requirements.inspection, scaled(5));
});
test("missing duplicate reordered or out-of-menu contexts poison before counting", () => {
  const p = plan();
  for (const id of [ids[1], "point2", "diagnostic/unknown"]) {
    const c = createCaptureRequirementCensus(policy(), metadata); assert.throws(() => c.add(id, p), /sequence/);
    assert.equal(c.snapshot().contexts, 0); assert.throws(() => c.add(ids[0], p), /failed/);
  }
  const c = createCaptureRequirementCensus(policy(), metadata); c.add(ids[0], p); assert.throws(() => c.add(ids[0], p), /sequence/);
  const early = createCaptureRequirementCensus(policy(), metadata); assert.throws(() => early.finish(caps), /all705/);
});
test("observed producer schema and producer record fields cannot become symbolic counts", () => {
  const observed = clone(plan()); observed.schemaVersion = SCHEMA; assert.throws(() => deriveCaptureShape(observed, metadata), /symbolic plan/);
  const extra = clone(plan()); extra.nodes[0].records = 0; assert.throws(() => deriveCaptureShape(extra, metadata), /exact fields/);
  assert.throws(() => deriveCaptureShape({ schemaVersion: PLAN_SCHEMA, leaves: [], nodes: [], marks: [], counts: one }, metadata), /exact fields/);
});
test("invalid topology registration and marks are rejected instead of counting a partial plan", () => {
  for (const mutate of [p => p.nodes[1].inputs[0] = 1, p => p.nodes[1].id = 2, p => p.nodes.pop(),
    p => p.nodes[0].parameters.id = "other", p => p.nodes[0].degree = 2, p => p.leaves.push(p.leaves[0]),
    p => p.marks[0].node = 99, p => p.marks.push(p.marks[0]), p => p.marks[0].degree = 2]) {
    const p = clone(plan()); mutate(p); assert.throws(() => deriveCaptureShape(p, metadata));
  }
});
test("metadata accessors are rejected without calls and snapshots ignore inherited toJSON", () => {
  let calls = 0; const p = clone(plan()); Object.defineProperty(p.nodes[0], "inputs", { enumerable: true, get() { calls++; return []; } });
  assert.throws(() => deriveCaptureShape(p, metadata)); assert.equal(calls, 0);
  const inherited = Object.assign(Object.create({ toJSON() { throw Error("must not call"); } }), plan());
  assert.deepEqual(deriveCaptureShape(inherited, metadata).copies, one);
});
test("metadata admission is cumulative across all plans rather than reset by context", () => {
  const c = createCaptureRequirementCensus(policy(), { ...metadata, nodes: 3000 });
  let accepted = 0; try { for (const id of ids) { c.add(id, plan()); accepted++; } } catch (error) { assert.match(error.message, /ceiling|allowance/); }
  assert.ok(accepted > 0 && accepted < 705); assert.equal(c.snapshot().failed, true);
  assert.throws(() => deriveCaptureShape(plan(), { ...metadata, arraySlots: 1 }), /ceiling|slots|array/);
});
test("swallowed proxy reentry cannot append a context or release the outer guard", () => {
  const c = createCaptureRequirementCensus(policy(), metadata), p = plan(); let rejected = 0;
  const proxy = new Proxy(p, { ownKeys(target) { try { c.add(ids[0], p); } catch { rejected++; } return Reflect.ownKeys(target); } });
  assert.throws(() => c.add(ids[0], proxy), /reentry/); assert.ok(rejected > 0); assert.equal(c.snapshot().contexts, 0);
  assert.throws(() => c.add(ids[0], p), /failed/);
});
test("safe integer overflow in inspection multiplication is rejected before context commit", () => {
  const p = policy(); p[0].copies = Number.MAX_SAFE_INTEGER;
  const c = createCaptureRequirementCensus(p, metadata); assert.throws(() => c.add(ids[0], plan()), /safe multiplied/);
  assert.equal(c.snapshot().contexts, 0); assert.equal(c.snapshot().failed, true);
});
test("returned counts and final proposed limits are detached from caller mutation", () => {
  const c = createCaptureRequirementCensus(policy(), metadata), p = clone(plan()), first = c.add(ids[0], p);
  p.nodes[1].inputs.push(0); assert.equal(first.shape.inputReferences, 2); assert.throws(() => first.copies.slots = 0, TypeError);
  for (const id of ids.slice(1)) c.add(id, plan()); const supplied = clone(caps), result = c.finish(supplied); supplied.normal.slots = 0;
  assert.equal(result.limits.normal.slots, caps.normal.slots);
});
