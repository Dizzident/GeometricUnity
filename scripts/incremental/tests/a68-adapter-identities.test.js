"use strict";
// Identity and symbolic metadata only. No source point/germ geometry or
// scientific diagnostic coefficients are constructed or numerically replayed.
const test = require("node:test"), assert = require("node:assert/strict");
const { sourceAdapterIdentity } = require("../a68-source-orchestration");
const { createDiagnosticSourceAdapter, diagnosticAdapterIdentity } = require("../a68-diagnostic-source-adapter");
const { secondJetMarkMenu } = require("../a68-second-jet-recipe");
function second() {
  return createDiagnosticSourceAdapter(null, "secondJets", { baseline: null, germs: [] }, {
    leaf: { leaves: 10, records: 10000, stringCharacters: 1000000, wireBytes: 10000000, rationalCharacters: 128, resolutions: 20 },
    export: { matrixSlots: 2000, coordinateVisits: 400000, entryRecords: 400000 },
    metadata: { nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 32 },
    comparison: { coordinateVisits: 2000000, sparseRecords: 1, tensorRecords: 1, rationalCharacters: 10000000 }
  });
}
const plan = () => ({ retention: Object.fromEntries(secondJetMarkMenu().map(m => [m.name, false])), nodeLimit: 4000, markLimit: 420 });
test("private identity accessors reject copies tokens primitives and unknown proxies without reading properties", () => {
  let reads = 0;
  const fake = new Proxy({ kind: "point", contextId: "point0", prepared: true }, { get() { reads++; throw Error("untrusted getter"); } });
  for (const read of [sourceAdapterIdentity, diagnosticAdapterIdentity])
    for (const value of [undefined, null, 1, "point0", {}, fake]) assert.throws(() => read(value), /private .* adapter identity required/);
  assert.equal(reads, 0);
  const adapter = second(), identity = diagnosticAdapterIdentity(adapter);
  for (const value of [identity, identity.identity, { ...adapter }]) assert.throws(() => diagnosticAdapterIdentity(value), /private diagnostic adapter identity/);
  assert.throws(() => sourceAdapterIdentity(adapter), /private source adapter identity/);
});
test("diagnostic identities are immutable opaque distinct tokens with no private mutation capability", () => {
  const adapter = second(), record = diagnosticAdapterIdentity(adapter), again = diagnosticAdapterIdentity(adapter);
  assert.deepEqual(Object.keys(record), ["kind", "contextId", "parentIdentity", "identity", "prepared", "replayed", "completed", "health"]);
  assert.ok(Object.isFrozen(record)); assert.ok(Object.isFrozen(record.identity)); assert.deepEqual(Object.keys(record.identity), []);
  assert.notEqual(record, again); assert.equal(record.identity, again.identity); assert.equal(record.health, again.health);
  assert.notEqual(record.identity, diagnosticAdapterIdentity(second()).identity);
  assert.equal(record.kind, "diagnostic"); assert.equal(record.contextId, "diagnostic/secondJets"); assert.equal(record.parentIdentity, null);
  assert.equal(record.prepared, false); assert.equal(record.replayed, false); assert.equal(record.completed, false);
  assert.deepEqual(record.health(), { failed: false }); assert.ok(Object.isFrozen(record.health()));
  assert.throws(() => { record.prepared = true; }, TypeError);
  assert.throws(() => { record.identity.mint = true; }, TypeError);
  assert.throws(() => { record.health().failed = true; }, TypeError);
  assert.deepEqual(record.health(), { failed: false });
});
test("prepared means built and not already compared and never implies numerical replay", () => {
  const adapter = second(), before = diagnosticAdapterIdentity(adapter);
  adapter.buildRecipe(plan());
  const prepared = diagnosticAdapterIdentity(adapter);
  assert.equal(before.prepared, false); assert.equal(prepared.prepared, true); assert.equal(prepared.replayed, false);
  assert.equal(prepared.completed, false); assert.equal(prepared.identity, before.identity);
  adapter.compareGeometryMetadata({});
  assert.equal(diagnosticAdapterIdentity(adapter).prepared, false);
  assert.equal(diagnosticAdapterIdentity(adapter).replayed, false);
  assert.deepEqual(prepared.health(), { failed: false });
});
test("previously retained health capability observes sticky later failure", () => {
  const adapter = second(), { health, identity } = diagnosticAdapterIdentity(adapter);
  adapter.buildRecipe(plan());
  assert.throws(() => adapter.replayContextCheckpoint({}, {}), /private read-pinned/);
  assert.deepEqual(health(), { failed: true });
  const record = diagnosticAdapterIdentity(adapter);
  assert.equal(record.identity, identity); assert.equal(record.replayed, false); assert.equal(record.completed, false);
  assert.throws(() => adapter.compareGeometryMetadata({}), /failed or reentrant/);
  assert.deepEqual(health(), { failed: true });
});
