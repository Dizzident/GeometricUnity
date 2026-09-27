"use strict";

// Read-only comparison of retained files against independently replayed FULL
// tensors. No producer hash or filename supplies numerical/source authority.
// Generic manufactured callers cannot turn this into a source-replay receipt.
const fs = require("node:fs"), path = require("node:path");
const { createCanonicalLeafClosure, snapshotCanonicalMetadata } = require("./a68-source-orchestration");
const need = (ok, why) => { if (!ok) throw new Error("A68 expanded tensors: " + why); };
const integer = (n, min = 0) => Number.isSafeInteger(n) && !Object.is(n, -0) && n >= min;
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const safePath = p => typeof p === "string" && !path.isAbsolute(p) && p.split("/").every(s => /^[A-Za-z0-9_.-]+$/.test(s) && s !== "." && s !== "..");
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered data fields");
  return Object.fromEntries(names.map(name => {
    const property = Object.getOwnPropertyDescriptor(value, name); need(property && Object.hasOwn(property, "value"), "own data field " + name); return [name, property.value];
  }));
}
function compareFile(root, relative, expected) {
  const absolute = path.resolve(root, relative); need(absolute.startsWith(root + path.sep), "root-contained tensor file");
  let cursor = path.parse(absolute).root; const parts = absolute.slice(cursor.length).split(path.sep);
  for (let i = 0; i < parts.length; i++) {
    cursor = path.join(cursor, parts[i]); const stat = fs.lstatSync(cursor);
    need(!stat.isSymbolicLink() && (i === parts.length - 1 ? stat.isFile() : stat.isDirectory()), "no symlink or nonregular tensor path component");
  }
  const fd = fs.openSync(absolute, fs.constants.O_RDONLY | fs.constants.O_NOFOLLOW | fs.constants.O_NONBLOCK);
  try {
    const before = fs.fstatSync(fd, { bigint: true });
    need(before.isFile() && before.size === BigInt(expected.length), "exact independently reconstructed file size before allocation");
    const actual = Buffer.alloc(expected.length); let offset = 0;
    while (offset < actual.length) { const count = fs.readSync(fd, actual, offset, actual.length - offset, null); need(count > 0, "tensor file shortened"); offset += count; }
    need(fs.readSync(fd, Buffer.alloc(1), 0, 1, null) === 0, "tensor file grew");
    const after = fs.fstatSync(fd, { bigint: true });
    need(before.size === after.size && before.mtimeNs === after.mtimeNs && before.ctimeNs === after.ctimeNs, "tensor file changed during read");
    // Exact bytes include canonical order/encoding and ONE terminal LF. No
    // second read or supplied success flag can substitute for this comparison.
    need(actual.equals(expected), "full independently replayed canonical file bytes");
  } finally { fs.closeSync(fd); }
}
function createExpandedTensorVerifier(outputRoot, declaration, limits) {
  need(typeof outputRoot === "string" && path.isAbsolute(outputRoot), "explicit absolute output root");
  const root = path.resolve(outputRoot); need(root !== path.parse(root).root, "non-filesystem-root output directory");
  limits = fields(limits, ["marks", "fileBytes", "totalReadBytes", "totalRecords", "totalStringCharacters", "totalSerializedBytes", "rationalCharacters", "snapshot"]);
  const captured = snapshotCanonicalMetadata({ declaration, limits }, limits.snapshot).value;
  limits = captured.limits; declaration = fields(captured.declaration, ["contextId", "marks", "reservedPaths"]);
  need(Object.entries(limits).every(([k, v]) => k === "snapshot" || integer(v, 1)) && limits.fileBytes > 2 && limits.rationalCharacters <= 16384, "explicit positive bounded comparison quotas");
  need(typeof declaration.contextId === "string" && declaration.contextId.trim() && Array.isArray(declaration.marks) && declaration.marks.length <= limits.marks, "declared bounded complete mark menu");
  need(Array.isArray(declaration.reservedPaths) && declaration.reservedPaths.every(safePath), "frozen reserved artifact paths");
  const menu = new Map(), paths = new Set(declaration.reservedPaths);
  need(paths.size === declaration.reservedPaths.length, "distinct reserved paths");
  for (const value of declaration.marks) {
    const mark = fields(value, ["name", "degree", "expanded", "relativePath"]);
    need(typeof mark.name === "string" && mark.name.trim() && !menu.has(mark.name) && integer(mark.degree) && mark.degree <= 14 && typeof mark.expanded === "boolean", "unique typed declared mark");
    need(mark.expanded ? safePath(mark.relativePath) && !paths.has(mark.relativePath) : mark.relativePath === null, "distinct expanded path iff retained");
    if (mark.expanded) paths.add(mark.relativePath); menu.set(mark.name, Object.freeze(mark));
  }
  for (const file of paths) { const parts = file.split("/"); while (parts.length > 1) { parts.pop(); need(!paths.has(parts.join("/")), "file/directory path collision"); } }
  const seen = new Set(); let failed = false, finished = false, busy = false, files = 0, readBytes = 0, records = 0, characters = 0, serializedBytes = 0;
  function guard(action) {
    if (failed || finished || busy) { failed = true; throw new Error("A68 expanded tensors: failed, reentrant or closed verifier"); }
    busy = true;
    try { const result = action(); need(!failed, "swallowed reentrant failure"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  const scope = Object.freeze({ sourceAuthenticityEstablished: false, numericalReplayEstablishedHere: false,
    totalProcessMemoryProved: false, atomicParentPathProtectionProved: false, pointTraversalComplete: false, scientificExecutionAuthorized: false });
  return Object.freeze({
    compareMark: (observed, rows) => guard(() => {
      const mark = fields(observed, ["name", "degree", "node", "expanded", "sha256"]), expected = menu.get(mark.name);
      need(expected && !seen.has(mark.name) && expected.degree === mark.degree && expected.expanded === mark.expanded && integer(mark.node) && typeof mark.sha256 === "string" && /^[0-9a-f]{64}$/.test(mark.sha256), "one exact declared mark identity");
      // Own-data tensor copying, rational canonicalization and full support
      // validation reuse the source-closure primitive, without minting source
      // identity. Charge cumulative work before each allocation/serialization.
      const length = Object.getOwnPropertyDescriptor(rows ?? {}, "length")?.value;
      need(Array.isArray(rows) && integer(length) && length <= limits.totalRecords - records, "prospective cumulative record quota");
      const remaining = limits.totalSerializedBytes - serializedBytes;
      const wireCap = Math.min(limits.fileBytes - 1, expected.expanded ? Math.floor((remaining - 1) / 2) : remaining);
      need(wireCap >= 2 && characters < limits.totalStringCharacters, "prospective serialization/string quota");
      const closure = createCanonicalLeafClosure([{ id: mark.name, degree: mark.degree, source: "independent-mark-replay", tensor: rows }], {
        leaves: 1, records: Math.max(1, limits.totalRecords - records), stringCharacters: limits.totalStringCharacters - characters,
        wireBytes: wireCap, rationalCharacters: limits.rationalCharacters, resolutions: 1
      });
      const usage = closure.snapshot(), leaf = closure.leaves[0];
      records += usage.records; characters += usage.stringCharacters; serializedBytes += usage.wireBytes;
      need(leaf.sha256 === mark.sha256, "reconstructed full tensor hash agrees with graph mark");
      if (expected.expanded) {
        const bytes = usage.wireBytes + 1;
        need(bytes <= limits.fileBytes && bytes + 1 <= limits.totalReadBytes - readBytes, "prospective file and cumulative read quota including growth probe");
        readBytes += bytes + 1; serializedBytes += bytes;
        const canonical = Buffer.from(JSON.stringify(closure.resolveLeaf(leaf)) + "\n", "ascii");
        need(canonical.length === bytes, "exact canonical envelope length");
        compareFile(root, expected.relativePath, canonical); files++;
      }
      seen.add(mark.name); return true;
    }),
    finish: () => guard(() => {
      need(seen.size === menu.size, "complete declared mark census before file verification receipt"); finished = true;
      return Object.freeze({ contextId: declaration.contextId, marks: seen.size, expandedFiles: files, readBytes, records, serializedBytes,
        expandedTensorArtifactsVerified: true, scope });
    }),
    snapshot: () => Object.freeze({ marks: seen.size, files, readBytes, records, characters, serializedBytes, failed, finished })
  });
}
module.exports = { createExpandedTensorVerifier };
