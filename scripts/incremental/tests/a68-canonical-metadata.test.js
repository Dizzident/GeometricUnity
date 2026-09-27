"use strict";
const test = require("node:test"), assert = require("node:assert/strict");
const path = require("node:path"), { execFileSync } = require("node:child_process");
const { snapshotCanonicalMetadata: snapshot } = require("../a68-canonical-metadata");
const caps = extra => ({ nodes: 100, arraySlots: 100, stringCharacters: 100, maxDepth: 8, ...extra });
const root = path.resolve(__dirname, "../../..");
function fresh(source) {
  return JSON.parse(execFileSync(process.execPath, ["-e", source], { cwd: root, encoding: "utf8",
    env: { ...process.env, NODE_OPTIONS: "", NODE_PATH: "" }, timeout: 10000, maxBuffer: 65536 }));
}

test("standalone metadata module loads without any local dependency", () => {
  const result = fresh('const p=require.resolve("./scripts/incremental/a68-canonical-metadata");const x=require(p);process.stdout.write(JSON.stringify({exports:Object.keys(x),files:Object.keys(require.cache),children:require.cache[p].children.map(m=>m.filename)}));');
  assert.deepEqual(result.exports, ["snapshotCanonicalMetadata"]);
  assert.deepEqual(result.files, [path.join(root, "scripts/incremental/a68-canonical-metadata.js")]);
  assert.deepEqual(result.children, []);
});

test("source orchestration reexports the identical snapshot function", () => {
  assert.equal(require("../a68-source-orchestration").snapshotCanonicalMetadata, snapshot);
});

test("detached frozen snapshot retains exact cumulative counts and repeated aliases", () => {
  const shared = { z: "ab" }, input = { a: [shared, shared], n: -2 };
  const output = snapshot(input, caps({ nodes: 7, arraySlots: 6, stringCharacters: 8, maxDepth: 3 }));
  assert.deepEqual(output.usage, { nodes: 7, arraySlots: 6, stringCharacters: 8, maxDepth: 0 });
  assert.deepEqual(output.value, input); assert.notEqual(output.value.a[0], output.value.a[1]);
  shared.z = "changed"; assert.equal(output.value.a[0].z, "ab");
  for (const value of [output, output.usage, output.value, output.value.a, ...output.value.a]) assert.ok(Object.isFrozen(value));
  for (const [key, exact] of Object.entries({ nodes: 7, arraySlots: 6, stringCharacters: 8, maxDepth: 3 }))
    assert.throws(() => snapshot({ a: [{ z: "ab" }, { z: "ab" }], n: -2 }, caps({ [key]: exact - 1 })), /ceiling|length/);
});

test("own-data copying never invokes accessors or inherited serialization", () => {
  let calls = 0;
  const input = Object.assign(Object.create({ toJSON() { calls++; throw Error("not JSON input"); } }), { a: [true, null] });
  assert.equal(JSON.stringify(snapshot(input, caps()).value), '{"a":[true,null]}');
  for (const object of [{}, [0]]) {
    Object.defineProperty(object, Array.isArray(object) ? "0" : "x", { get() { calls++; return 0; }, enumerable: true });
    assert.throws(() => snapshot(object, caps()), /own data property/);
  }
  const limits = caps(); Object.defineProperty(limits, "nodes", { get() { calls++; return 100; } });
  assert.throws(() => snapshot({}, limits), /own data property/); assert.equal(calls, 0);
});

test("special and nonenumerable own keys preserve historical plain-output semantics", () => {
  const input = JSON.parse('{"__proto__":{"x":1},"constructor":2}');
  Object.defineProperty(input, "hidden", { value: 3 });
  const output = snapshot(input, caps()).value;
  assert.equal(Object.getPrototypeOf(output), Object.prototype);
  assert.equal(Object.hasOwn(output, "__proto__"), true); assert.equal(output.__proto__.x, 1);
  assert.deepEqual(Object.keys(output), ["__proto__", "constructor", "hidden"]);
  assert.equal(output.hidden, 3);
});

test("malformed JSON metadata arrays numbers and limits remain fail closed", () => {
  const cycle = {}; cycle.x = cycle;
  const extra = [0]; extra.extra = 1;
  const symbolic = [0]; symbolic[Symbol("x")] = 1;
  for (const value of [cycle, new Array(1), extra, symbolic, { [Symbol("x")]: 1 }, undefined, 1n, () => 0,
    NaN, Infinity, -Infinity, -0, 0.5, Number.MAX_SAFE_INTEGER + 1, "\n", "é"])
    assert.throws(() => snapshot(value, caps()), /^Error: A68 source orchestration:/);
  for (const limits of [caps({ nodes: 0 }), caps({ nodes: -0 }), caps({ maxDepth: 65 }),
    caps({ arraySlots: 0.5 }), caps({ stringCharacters: Infinity }), { ...caps(), extra: 1 },
    { arraySlots: 100, nodes: 100, stringCharacters: 100, maxDepth: 8 }])
    assert.throws(() => snapshot({}, limits));
});

test("snapshot-only preflight imports cannot load the excluded geometry closure", () => {
  // Fresh process instrumentation rejects excluded loads BEFORE evaluation.
  // This observed CommonJS path is not static closure, loader authenticity,
  // native/runtime coverage, or a sandbox against arbitrary hostile code.
  const observed = fresh(`const Module=require("node:module"),path=require("node:path");
    const excluded=/a68-(source-orchestration|geometry-algebra|geometry-binding|geometry-comparison|geometry-metadata|geometry-recipe-binding|source-geometry|curvature-geometry|polynomial-reconstruction|retained-inputs|source-field-resolver)\\.js$/;
    const original=Module._load;
    Module._load=function(request,parent,isMain){const file=Module._resolveFilename(request,parent,isMain);if(excluded.test(file))throw Error("excluded metadata dependency: "+file);return original.apply(this,arguments);};
    require("./scripts/incremental/a68-template-process-host");require("./scripts/incremental/a68-topology-source-admission");
    process.stdout.write(JSON.stringify(Object.keys(require.cache).map(p=>path.basename(p)).sort()));`);
  assert.deepEqual(observed, ["acceleration-recipe", "audit-context-recipe", "background-recipe", "branch-topology-envelope",
    "canonical-metadata", "canonical-wire", "checkpoint-catalog", "compiler-inputs", "context-topology-envelope", "diagnostics-recipe",
    "launch-file-admission", "mixed-recipe", "original-action-recipe", "second-jet-recipe", "source-context-menu",
    "source-template-validator", "template-preflight-service", "template-process-host", "topology-source-admission",
    "variation-recipe", "ward-controls-recipe", "ward-recipe"].map(n => "a68-" + n + ".js"));
});
