"use strict";
// Computational context evidence is producer-complete, never independently
// accepted just because it can be read. The source adapter must authenticate
// geometry/recipes/leaves, then require full numerical and retained-file replay.
const { readPinnedArtifactPair } = require("./a68-pinned-artifacts");
const identities = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 context checkpoint: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function readContextCheckpoint(outputRoot, declaration, observed, limits) {
  const context = Object.getOwnPropertyDescriptor(declaration ?? {}, "contextId")?.value;
  need(typeof context === "string" && (/^point[01]\/m[0-9]_j(?:[0-9]|[12][0-9]|3[0-4])$/.test(context) ||
    ["diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"].includes(context)), "exact nonpoint computational context");
  const pinned = readPinnedArtifactPair(outputRoot, declaration, observed, limits), envelope = pinned.metadata;
  need(pinned.declaration.contextId === context, "stable captured context identity");
  need(envelope && !Array.isArray(envelope) && same(Object.keys(envelope), ["schema", "context", "status", "independentValidationComplete", "graph", "metadata"]), "complete producer-only computational envelope");
  need(envelope.schema === "phase627-context-computational-evidence-v1" && envelope.context === context &&
    envelope.status === "producer-complete" && envelope.independentValidationComplete === false, "producer completion cannot claim independent acceptance");
  need(same(envelope.graph, pinned.pins.graph), "metadata links the exact same read-pinned graph");
  need(envelope.metadata && typeof envelope.metadata === "object" && !Array.isArray(envelope.metadata), "recorded context metadata object");
  const token = Object.freeze({ schemaVersion: "phase627-pinned-context-checkpoint-v1", contextId: context });
  identities.set(token, Object.freeze({ ...pinned, envelope,
    scope: Object.freeze({ sameBufferHashesChecked: true, numericalReplayComplete: false, sourceAuthenticityEstablished: false,
      recordedGeometryCompared: false, expandedTensorArtifactsVerified: false, independentContextAccepted: false,
      totalProcessMemoryProved: false, atomicParentPathProtectionProved: false, scientificExecutionAuthorized: false }) }));
  return token;
}
function contextCheckpointIdentity(token) { const identity = identities.get(token); need(identity, "private read-pinned context identity required"); return identity; }
module.exports = { readContextCheckpoint, contextCheckpointIdentity };
