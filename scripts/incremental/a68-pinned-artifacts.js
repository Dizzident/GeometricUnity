"use strict";
// Shared bounded same-buffer byte/hash reader. This authenticates observed
// artifact bytes only, NOT the source, numerical result or context acceptance.
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto");
const { parseCanonicalWire } = require("./a68-canonical-wire");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 pinned artifacts: " + why); };
const integer = n => Number.isSafeInteger(n) && n > 0;
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && JSON.stringify(Reflect.ownKeys(value)) === JSON.stringify(names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
const safePath = p => typeof p === "string" && !path.isAbsolute(p) && p.split("/").every(s => /^[A-Za-z0-9_.-]+$/.test(s) && s !== "." && s !== "..");
function artifact(value) {
  value = fields(value, ["path", "bytes", "sha256"]);
  need(safePath(value.path) && integer(value.bytes) && value.bytes > 1 && typeof value.sha256 === "string" && /^[0-9a-f]{64}$/.test(value.sha256), "bounded artifact descriptor");
  return Object.freeze(value);
}
function readArtifact(root, pin, ceiling) {
  need(pin.bytes <= ceiling, "artifact pre-read ceiling");
  const absolute = path.resolve(root, pin.path); need(absolute.startsWith(root + path.sep), "root-contained artifact");
  const parts = absolute.slice(path.parse(absolute).root.length).split(path.sep); let cursor = path.parse(absolute).root;
  for (let i = 0; i < parts.length; i++) {
    cursor = path.join(cursor, parts[i]); const stat = fs.lstatSync(cursor);
    need(!stat.isSymbolicLink() && (i === parts.length - 1 ? stat.isFile() : stat.isDirectory()), "no symlink or nonregular artifact component");
  }
  const fd = fs.openSync(absolute, fs.constants.O_RDONLY | fs.constants.O_NOFOLLOW | fs.constants.O_NONBLOCK);
  try {
    const before = fs.fstatSync(fd, { bigint: true }); need(before.isFile() && before.size === BigInt(pin.bytes), "exact artifact size before allocation");
    const bytes = Buffer.alloc(pin.bytes); let offset = 0;
    while (offset < bytes.length) { const count = fs.readSync(fd, bytes, offset, bytes.length - offset, null); need(count > 0, "artifact shortened"); offset += count; }
    need(fs.readSync(fd, Buffer.alloc(1), 0, 1, null) === 0, "artifact grew");
    const after = fs.fstatSync(fd, { bigint: true });
    need(after.size === before.size && after.mtimeNs === before.mtimeNs && after.ctimeNs === before.ctimeNs, "artifact changed during read");
    need(crypto.createHash("sha256").update(bytes).digest("hex") === pin.sha256, "same-buffer artifact hash");
    return parseCanonicalWire(bytes, { graphBytes: ceiling });
  } finally { fs.closeSync(fd); }
}
function readPinnedArtifactPair(outputRoot, declaration, observed, limits) {
  need(typeof outputRoot === "string" && path.isAbsolute(outputRoot), "explicit absolute output root");
  const root = path.resolve(outputRoot); need(root !== path.parse(root).root, "non-filesystem-root output directory");
  declaration = fields(declaration, ["contextId", "graphPath", "metadataPath", "marks"]);
  need(typeof declaration.contextId === "string" && declaration.contextId.trim() && safePath(declaration.graphPath) && safePath(declaration.metadataPath) && declaration.graphPath !== declaration.metadataPath, "frozen distinct artifact paths");
  observed = fields(observed, ["context", "graph", "metadata"]);
  const pins = Object.freeze({ context: observed.context, graph: artifact(observed.graph), metadata: artifact(observed.metadata) });
  need(pins.context === declaration.contextId && pins.graph.path === declaration.graphPath && pins.metadata.path === declaration.metadataPath, "observed artifact identity matches frozen declaration");
  limits = fields(limits, ["graphBytes", "metadataBytes", "totalReadBytes", "snapshot"]);
  need([limits.graphBytes, limits.metadataBytes, limits.totalReadBytes].every(integer), "explicit positive read quotas");
  need(pins.graph.bytes <= limits.graphBytes && pins.metadata.bytes <= limits.metadataBytes &&
    BigInt(pins.graph.bytes) + BigInt(pins.metadata.bytes) + 2n <= BigInt(limits.totalReadBytes), "prospective combined artifact read admission");
  const snapshot = fields(limits.snapshot, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(snapshot).every(integer) && snapshot.maxDepth <= 16, "explicit snapshot quotas and depth16");
  // Freeze declarations BEFORE file I/O. Declaration and parsed-artifact
  // snapshots consume ONE cumulative quota, not a fresh quota for each copy.
  const declared = snapshotCanonicalMetadata(declaration, snapshot);
  const remaining = { nodes: snapshot.nodes - declared.usage.nodes, arraySlots: snapshot.arraySlots - declared.usage.arraySlots,
    stringCharacters: snapshot.stringCharacters - declared.usage.stringCharacters, maxDepth: snapshot.maxDepth };
  need(remaining.nodes > 0 && remaining.arraySlots > 0 && remaining.stringCharacters > 0, "remaining artifact snapshot quota");
  const graph = readArtifact(root, pins.graph, limits.graphBytes), metadata = readArtifact(root, pins.metadata, limits.metadataBytes);
  const parsed = snapshotCanonicalMetadata({ graph, metadata }, remaining);
  const usage = Object.freeze(Object.fromEntries(["nodes", "arraySlots", "stringCharacters"].map(k => [k, declared.usage[k] + parsed.usage[k]])));
  return Object.freeze({ root, declaration: declared.value, pins, graph: parsed.value.graph, metadata: parsed.value.metadata, snapshotUsage: usage });
}
module.exports = { readPinnedArtifactPair };
