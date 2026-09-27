"use strict";
// Pure analytic metadata fixtures. These are not observed tensor supports or
// numerical tests of the physical model.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { deriveMixedResourceMajorants } = require("../a68-resource-majorants");
const hash = "0".repeat(64), leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: hash });
const bound = (id, support, denominator, norm) => ({ id, source: "synthetic/" + id, sha256: hash, support, denominator, norm });
function derive(recipe, bounds = [], options) {
  const plan = recipe.Finish();
  return deriveMixedResourceMajorants(plan, bounds, (b, source) => {
    assert.ok(Object.isFrozen(b)); assert.equal(b.source, source.source);
    assert.deepEqual(b, bounds.find(expected => expected.id === b.id)); return true;
  }, options);
}
const geometry = { name: "m", matrix: [{ row: 0, column: 0, value: "1/2" }, { row: 0, column: 1, value: "-3/2" }, { row: 7, column: 0, value: "2" }] };

test("analytic zero bound has exact empty-array envelope and no numerical/RSS promotion", () => {
  const r = new MixedRecipe(); r.Zero(); const result = derive(r), n = result.nodes[0];
  assert.deepEqual([n.support, n.denominator, n.norm, n.rationalCharacters, n.tensorWireBytes], ["0", "1", "0", "1", "2"]);
  assert.equal(result.pairVisits, "0"); assert.equal(result.replayTensorRecordPeak, "0");
  assert.equal(result.scope.scientificCoefficientsEvaluated, false); assert.equal(result.scope.processRssProved, false);
  assert.equal(result.scope.completeRuntimeResourceProof, false);
});

test("sum/scale majorants use lcm common denominators and exact positive l1 arithmetic", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 1), leaf("b", 1)], constants: ["0", "1", "1/2"] });
  const a = r.RegisterLeaf("a"), b = r.RegisterLeaf("b"); r.Times(r.Sum(a, b), "1/2");
  const result = derive(r, [bound("a", "2", "6", "3/2"), bound("b", "3", "10", "5/3")]);
  assert.deepEqual([result.nodes[2].support, result.nodes[2].denominator, result.nodes[2].norm], ["5", "30", "19/6"]);
  assert.deepEqual([result.nodes[3].support, result.nodes[3].denominator, result.nodes[3].norm], ["5", "60", "19/12"]);
  assert.deepEqual(result.nodes.slice(0, 2).map(n => n.slotVisits), ["4", "6"], "both leaf snapshot and import scans counted");
  assert.equal(result.slotVisits, "20"); assert.equal(result.replayTensorRecordPeak, "10"); assert.equal(result.replayTensorDictionaryPeak, "3");
});

test("raw pair work is charged even when exterior degree proves the result zero", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 8), leaf("b", 8)] });
  r.P(r.RegisterLeaf("a"), r.RegisterLeaf("b"));
  const result = derive(r, [bound("a", "2", "2", "1"), bound("b", "3", "3", "1")]), n = result.nodes[2];
  assert.equal(n.degree, -1); assert.equal(n.support, "0"); assert.equal(n.pairVisits, "6"); assert.equal(result.pairVisits, "6");
});

test("C/A products and transposes carry the factor2 majorant", () => {
  for (const operation of ["P", "Transpose"]) for (const kind of ["C", "A"]) {
    const r = new MixedRecipe({ leaves: [leaf("a", 1), leaf("b", 2)] });
    r[operation](r.RegisterLeaf("a"), r.RegisterLeaf("b"), kind);
    const n = derive(r, [bound("a", "10", "2", "3/2"), bound("b", "20", "3", "4/3")]).nodes[2];
    assert.deepEqual([n.support, n.denominator, n.norm, n.pairVisits], ["200", "6", "4", "200"]);
  }
});

test("all-grade ambient envelope is not a reused600000 cap or a final-Q grade filter", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 3), leaf("b", 4)] }); r.P(r.RegisterLeaf("a"), r.RegisterLeaf("b"));
  const n = derive(r, [bound("a", "8000", "1", "1"), bound("b", "8000", "1", "1")]).nodes[2];
  assert.equal(n.pairVisits, "64000000"); assert.equal(n.support, "56229888"); assert.ok(BigInt(n.support) > 600000n);
});

test("motion counts all r*14 scans; spin uses lower-triangle norm and denominator2Dm", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 2)], matrices: [geometry] });
  r.Motion(r.Matrix("m"), r.RegisterLeaf("a")); r.Spin(r.Matrix("m"));
  const result = derive(r, [bound("a", "3", "6", "5")]);
  assert.deepEqual([result.nodes[1].support, result.nodes[1].denominator, result.nodes[1].norm, result.nodes[1].slotVisits], ["84", "12", "20", "84"]);
  assert.deepEqual([result.nodes[2].support, result.nodes[2].denominator, result.nodes[2].norm, result.nodes[2].slotVisits], ["1", "4", "1", "91"]);
});

test("pullback envelope includes all cached forms, old/product/row coexistence and bounded row scans", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 2)], matrices: [geometry] }); r.Pullback(r.Matrix("m"), r.RegisterLeaf("a"));
  const result = derive(r, [bound("a", "3", "6", "5")]), n = result.nodes[1];
  assert.deepEqual([n.support, n.denominator, n.norm, n.pairVisits, n.slotVisits], ["12", "24", "20", "18", "96"]);
  assert.equal(n.primitiveScratchRecords, "28"); assert.equal(n.primitiveScratchTensors, "6");
  assert.equal(result.replayTensorRecordPeak, "31"); assert.equal(result.replayTensorDictionaryPeak, "7");
});

test("generic pullback accounts for middle-degree3432 form carrier, not only one/two forms", () => {
  const matrix = []; for (let row = 0; row < 14; row++) for (let column = 0; column < 14; column++) matrix.push({ row, column, value: "1" });
  const r = new MixedRecipe({ leaves: [leaf("a", 7)], matrices: [{ name: "dense", matrix }] }); r.Pullback(r.Matrix("dense"), r.RegisterLeaf("a"));
  const result = derive(r, [bound("a", "3", "1", "1")]), n = result.nodes[1];
  assert.equal(n.support, "10296"); assert.equal(n.primitiveScratchRecords, "23609"); assert.equal(result.replayTensorRecordPeak, "23612");
  assert.equal(n.norm, "105413504");
});

test("zero-degree pullback retains a one-record form seed in addition to input/output", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 0)], matrices: [{ name: "zero", matrix: [] }] }); r.Pullback(r.Matrix("zero"), r.RegisterLeaf("a"));
  const result = derive(r, [bound("a", "2", "3", "1")]), n = result.nodes[1];
  assert.deepEqual([n.support, n.denominator, n.norm, n.primitiveScratchRecords], ["2", "3", "1", "3"]);
  assert.equal(result.replayTensorRecordPeak, "5"); assert.equal(n.pairVisits, "0"); assert.equal(n.slotVisits, "2");
});

test("printed rational bound covers both signed numerator and denominator plus separator", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 1)] }); r.RegisterLeaf("a");
  const n = derive(r, [bound("a", "1", "4", "3/2")]).nodes[0];
  assert.equal(n.rationalCharacters, "4");
  const longest = JSON.stringify([{ form: 16383, blade: 16383, k0: 0, k1: 0, real: "-5/4", imaginary: "-5/4" }]);
  assert.equal(n.tensorWireBytes, String(Buffer.byteLength(longest)));
});

test("missing source proof, changed closure, malformed ratios and excessive analytic bounds reject", () => {
  const make = () => { const r = new MixedRecipe({ leaves: [leaf("a", 1)] }); r.RegisterLeaf("a"); return r.Finish(); };
  assert.throws(() => deriveMixedResourceMajorants(make(), [bound("a", "1", "1", "1")], () => false), /source proof/);
  assert.throws(() => deriveMixedResourceMajorants(make(), [bound("wrong", "1", "1", "1")], () => true), /source order/);
  assert.throws(() => deriveMixedResourceMajorants(make(), [bound("a", "1", "1", "2\/4")], () => true), /reduced/);
  assert.throws(() => deriveMixedResourceMajorants(make(), [bound("a", "1", "18446744073709551616", "1")], () => true, { maxBits: 64 }), /maxBits/);
  assert.throws(() => deriveMixedResourceMajorants(make(), [bound("a", "1", "1", "-1")], () => true), /nonnegative/);
});

test("star, raise, contraction and component preserve conservative D/M envelopes", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 2)] }), a = r.RegisterLeaf("a");
  r.Hodge(a); r.Raise(a); r.Contract(a, 0); r.Component(a, 3);
  const result = derive(r, [bound("a", "3", "7", "5/2")]);
  assert.deepEqual(result.nodes.slice(1).map(n => [n.denominator, n.norm, n.slotVisits]), Array(4).fill(["7", "5/2", "3"]));
});

test("empty-tensor scale still requires full real and imaginary parameter heights", () => {
  const long = "-1" + "0".repeat(300) + "/3";
  for (const component of ["real", "imaginary"]) {
    const r = new MixedRecipe({ constants: ["0", long] });
    r.Scale(r.FreshZero(), component === "real" ? long : "0", component === "imaginary" ? long : "0");
    const result = derive(r), n = result.nodes.at(-1);
    assert.equal(n.support, "0"); assert.equal(n.rationalCharacters, "1");
    assert.equal(n.parameterRationalCharacters, String(long.length));
    assert.equal(n.primitiveRationalCharacters, String(long.length));
    assert.equal(result.reducedRationalCharacterPeak, String(long.length));
  }
});

test("empty motion/pullback and zero spin output retain heights of every matrix parameter", () => {
  const long = "1" + "0".repeat(300);
  const matrices = [{ name: "long-diagonal", matrix: [{ row: 13, column: 13, value: long }] }];
  for (const operation of ["Motion", "MotionAdjoint", "Pullback", "Spin"]) {
    const r = new MixedRecipe({ matrices }), matrix = r.Matrix("long-diagonal");
    if (operation === "Spin") r.Spin(matrix); else r[operation](matrix, r.FreshZero());
    const result = derive(r), n = result.nodes.at(-1);
    assert.equal(n.support, "0"); assert.equal(n.rationalCharacters, "1");
    assert.equal(n.parameterRationalCharacters, String(long.length));
    assert.equal(n.primitiveRationalCharacters, String(long.length));
    assert.equal(result.reducedRationalCharacterPeak, String(long.length));
  }
});

test("unit height explicitly includes both serialized scalar parameters", () => {
  const long = "-1/" + "9".repeat(200);
  for (const component of ["real", "imaginary"]) {
    const r = new MixedRecipe({ constants: ["0", long] });
    r.Unit(1, 1, component === "real" ? long : "0", component === "imaginary" ? long : "0");
    const result = derive(r), n = result.nodes[0];
    assert.equal(n.parameterRationalCharacters, String(long.length));
    assert.ok(BigInt(n.primitiveRationalCharacters) >= BigInt(long.length));
    assert.ok(BigInt(result.reducedRationalCharacterPeak) >= BigInt(long.length));
  }
});

test("recorder workload counts leaf double hashing, repeated marks and expanded tensor LF", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 1)] }), a = r.RegisterLeaf("a"), empty = r.Zero(), scaled = r.Times(a, "-1");
  r.Mark("leaf-first", 1, a, false); r.Mark("leaf-second", 1, a, true);
  r.Mark("zero-expanded", 2, empty, true); r.Mark("scaled-expanded", 1, scaled, true);
  const result = derive(r, [bound("a", "2", "1", "3")]);
  // Each of the two nonempty nodes has two-record support, D=1 and a printed
  // signed coefficient bound2. This independently constructs its maximal
  // tensor-array byte envelope, without consuming the helper's byte result.
  const record = { form: 16383, blade: 16383, k0: 0, k1: 0, real: "-3", imaginary: "-3" };
  const bodyBytes = BigInt(Buffer.byteLength(JSON.stringify([record, record])));
  // Append: leaf twice + scale once + zero once. Marks: same leaf twice,
  // scale once + zero once. Thus6 nonempty hashes and2 empty-array hashes.
  assert.deepEqual(result.recorderWork, {
    fingerprintRecordVisits: "12", fingerprintWireBytes: String(6n * bodyBytes + 4n),
    expandedRecordVisits: "4", expandedWireBytes: String(2n * bodyBytes + 5n),
    maximumSortingTensorRecords: "2", duplicateMarkNameComparisons: "6"
  });
  assert.equal(result.scope.serializationSortingMemoryProved, false);
  assert.equal(result.scope.processRssProved, false);
});

test("recorder workload charges [] bytes even with no records and every repeated mark", () => {
  const r = new MixedRecipe(), empty = r.Zero();
  r.Mark("zero-not-expanded", 0, empty, false); r.Mark("same-zero-expanded", 14, empty, true);
  const result = derive(r);
  assert.deepEqual(result.recorderWork, {
    fingerprintRecordVisits: "0", fingerprintWireBytes: "6", expandedRecordVisits: "0", expandedWireBytes: "3",
    maximumSortingTensorRecords: "0", duplicateMarkNameComparisons: "1"
  });
});

test("recorder workload on an empty metadata plan is identically zero", () => {
  const result = derive(new MixedRecipe());
  assert.deepEqual(result.recorderWork, {
    fingerprintRecordVisits: "0", fingerprintWireBytes: "0", expandedRecordVisits: "0", expandedWireBytes: "0",
    maximumSortingTensorRecords: "0", duplicateMarkNameComparisons: "0"
  });
});
