"use strict";
// Point-final byte linkage and a compact lifecycle primitive. Neither helper
// authenticates scientific sources. The source point adapter privately owns its
// ledger; only its authentic child adapters can register numerical completions.
const path = require("node:path");
const { readPinnedArtifactPair } = require("./a68-pinned-artifacts");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const identities = new WeakMap(), same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const need = (ok, why) => { if (!ok) throw new Error("A68 point completion: " + why); };
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(k => { const d = Object.getOwnPropertyDescriptor(value, k); need(d && Object.hasOwn(d, "value"), "own data field"); return [k, d.value]; }));
}
const point = id => id === "point0" || id === "point1";
function pin(value, expected) {
  value = fields(value, ["path", "bytes", "sha256"]);
  need(value.path === expected && Number.isSafeInteger(value.bytes) && value.bytes > 1 && typeof value.sha256 === "string" && /^[0-9a-f]{64}$/.test(value.sha256), "exact artifact descriptor");
}
function copier(limits) {
  const cap = fields(limits, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(cap).every(n => Number.isSafeInteger(n) && n > 0) && cap.maxDepth <= 16, "explicit bounded snapshot limits");
  const usage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  const remaining = () => ({ nodes: cap.nodes - usage.nodes, arraySlots: cap.arraySlots - usage.arraySlots,
    stringCharacters: cap.stringCharacters - usage.stringCharacters, maxDepth: cap.maxDepth });
  return { remaining, usage, copy(value) {
    const result = snapshotCanonicalMetadata(value, remaining());
    for (const key of Object.keys(usage)) usage[key] += result.usage[key]; return result.value;
  } };
}
function readPointCompletion(outputRoot, declaration, observed, limits) {
  limits = fields(limits, ["backgroundBytes", "metadataBytes", "totalReadBytes", "snapshot"]);
  const copy = copier(limits.snapshot), header = copy.copy({ declaration, observed });
  declaration = fields(header.declaration, ["contextId", "graphPath", "backgroundPath", "metadataPath"]);
  observed = fields(header.observed, ["context", "background", "metadata"]);
  const id = declaration.contextId, background = fields(observed.background, ["context", "graph", "metadata"]);
  need(point(id) && observed.context === id && background.context === id, "exact same-point final identity");
  need(declaration.graphPath === id + "/graph.json" && declaration.backgroundPath === id + "/background-checkpoint.json" &&
    declaration.metadataPath === id + "/metadata.json", "fixed source point artifact paths");
  pin(background.graph, declaration.graphPath); pin(background.metadata, declaration.backgroundPath); pin(observed.metadata, declaration.metadataPath);
  // The pair reader's graph slot here is the BACKGROUND METADATA FILE. Do not
  // reread/replay its numerical DAG merely to check point traversal completion.
  const pair = readPinnedArtifactPair(outputRoot,
    { contextId: id, graphPath: declaration.backgroundPath, metadataPath: declaration.metadataPath, marks: [] },
    { context: id, graph: background.metadata, metadata: observed.metadata },
    { graphBytes: limits.backgroundBytes, metadataBytes: limits.metadataBytes, totalReadBytes: limits.totalReadBytes, snapshot: copy.remaining() });
  const sealed = fields(pair.graph, ["schema", "context", "status", "pointTraversalComplete", "graph", "metadata"]);
  const final = fields(pair.metadata, ["schema", "context", "status", "independentValidationComplete", "backgroundCheckpoint", "metadata"]);
  need(sealed.schema === "phase627-point-background-checkpoint-v1" && sealed.context === id && sealed.status === "background-sealed" &&
    sealed.pointTraversalComplete === false && same(sealed.graph, background.graph), "exact earlier background checkpoint linkage");
  need(final.schema === "phase627-point-computational-completion-v1" && final.context === id && final.status === "producer-complete" &&
    final.independentValidationComplete === false && same(final.backgroundCheckpoint, background.metadata), "producer-only final envelope binds exact background");
  need(sealed.metadata && typeof sealed.metadata === "object" && !Array.isArray(sealed.metadata) && same(final.metadata, sealed.metadata), "unchanged complete background metadata");
  const token = Object.freeze({ schemaVersion: "phase627-pinned-point-completion-v1", contextId: id });
  identities.set(token, Object.freeze({ root: pair.root, declaration: header.declaration, observed: header.observed,
    background: pair.graph, completion: pair.metadata,
    snapshotUsage: Object.freeze(Object.fromEntries(Object.keys(copy.usage).map(k => [k, copy.usage[k] + pair.snapshotUsage[k]]))),
    scope: Object.freeze({ sameBufferHashesChecked: true, backgroundMetadataUnchanged: true, numericalReplayComplete: false,
      sourceAuthenticityEstablished: false, childReplaysAccepted: false, pointTraversalComplete: false,
      totalProcessMemoryProved: false, scientificExecutionAuthorized: false }) }));
  return token;
}
function pointCompletionIdentity(token) { const value = identities.get(token); need(value, "private read-pinned point completion required"); return value; }

function createPointCompletionLedger(background, limits, parentHealth) {
  const copy = copier(limits), expected = copy.copy(background);
  fields(expected, ["root", "contextId", "graph", "backgroundMetadata", "metadataPath"]);
  const id = expected.contextId;
  need(point(id) && typeof expected.root === "string" && path.isAbsolute(expected.root) && path.resolve(expected.root) === expected.root &&
    expected.root !== path.parse(expected.root).root && expected.metadataPath === id + "/metadata.json", "fixed accepted point declaration");
  pin(expected.graph, id + "/graph.json"); pin(expected.backgroundMetadata, id + "/background-checkpoint.json");
  need(typeof parentHealth === "function", "private parent health capability");
  const children = []; let failed = false, busy = false, completed = false;
  const healthy = all => {
    need(parentHealth()?.failed === false, "healthy accepted parent");
    if (all) for (const child of children) need(child.health()?.failed === false, "healthy accepted child");
    need(!failed, "sticky lifecycle failure");
  };
  const guard = action => {
    if (failed || busy || completed) { failed = true; need(false, "failed reentrant or completed ledger"); }
    busy = true; try { healthy(false); const result = action(); healthy(false); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  };
  return Object.freeze({
    // This generic lifecycle helper is not a source brand. Its registration
    // capability MUST remain private inside the authentic source point adapter.
    registerChild: (child, health) => guard(() => {
      need(children.length < 350 && typeof health === "function", "bounded private child registration");
      const record = copy.copy(child); fields(record, ["root", "contextId", "graph", "metadata"]);
      const next = id + "/m" + Math.floor(children.length / 35) + "_j" + children.length % 35;
      need(record.root === expected.root && record.contextId === next, "exact ordered same-root same-point child");
      pin(record.graph, next + "/graph.json"); pin(record.metadata, next + "/metadata.json");
      need(health()?.failed === false, "healthy child after successful replay");
      children.push(Object.freeze({ record, health }));
    }),
    complete: token => guard(() => {
      need(children.length === 350, "all350 independently registered child replays"); healthy(true);
      const final = pointCompletionIdentity(token), observed = final.observed;
      need(final.root === expected.root && final.declaration.contextId === id && observed.context === id &&
        final.declaration.metadataPath === expected.metadataPath && same(observed.background.graph, expected.graph) &&
        same(observed.background.metadata, expected.backgroundMetadata), "final file binds this accepted background and root");
      completed = true;
      return Object.freeze({ contextId: id, children: 350, scope: Object.freeze({ artifactLinkageChecked: true,
        orderedChildRecordsComplete: true, sourceAuthenticityEstablished: false, scientificExecutionAuthorized: false }) });
    }),
    snapshot: () => {
      try { healthy(completed); } catch { failed = true; }
      return Object.freeze({ failed, children: children.length, completed: completed && !failed,
        snapshotUsage: Object.freeze({ ...copy.usage }), retainedFullGraphs: 0, retainedSourceAdapters: 0 });
    }
  });
}
module.exports = { readPointCompletion, pointCompletionIdentity, createPointCompletionLedger };
