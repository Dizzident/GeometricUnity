"use strict";

// Exact declared FILE-byte admission for a separately reviewed launch view.
// This does not discover dependency closure, prove source-to-binary provenance,
// bind the kernel's later exec to these reads or authorize scientific execution.
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 launch file admission: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const integer = (n, min = 0) => Number.isSafeInteger(n) && !Object.is(n, -0) && n >= min;
const absolute = p => typeof p === "string" && path.isAbsolute(p) && path.resolve(p) === p && p !== path.parse(p).root;
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
function createLaunchFileAdmission(options) {
  options = fields(options, ["expected", "pins", "argumentFiles", "limits"]);
  const rawLimits = fields(options.limits, ["files", "fileBytes", "totalReadBytes", "chunkBytes", "snapshot"]);
  const snapshotCap = fields(rawLimits.snapshot, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(snapshotCap).every(n => integer(n, 1)) && snapshotCap.maxDepth <= 16, "explicit cumulative snapshot quotas");
  const usage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  let failed = false, busy = false, completed = false, checkedFiles = 0, readBytes = 0;
  function capture(value) {
    const remaining = { nodes: snapshotCap.nodes - usage.nodes, arraySlots: snapshotCap.arraySlots - usage.arraySlots,
      stringCharacters: snapshotCap.stringCharacters - usage.stringCharacters, maxDepth: snapshotCap.maxDepth };
    need(remaining.nodes > 0 && remaining.arraySlots > 0 && remaining.stringCharacters > 0, "remaining cumulative snapshot quota");
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(usage)) usage[key] += copy.usage[key]; return copy.value;
  }
  const config = capture({ expected: options.expected, pins: options.pins, argumentFiles: options.argumentFiles,
    limits: { files: rawLimits.files, fileBytes: rawLimits.fileBytes, totalReadBytes: rawLimits.totalReadBytes, chunkBytes: rawLimits.chunkBytes } });
  const expected = fields(config.expected, ["launch", "profileSha256", "processLimits", "transportLimits"]);
  const launch = fields(expected.launch, ["executable", "arguments", "workingDirectory", "environment"]), cap = config.limits;
  need(Object.values(cap).every(n => integer(n, 1)) && cap.chunkBytes <= 1048576, "positive read quotas and at most1MiB hash scratch");
  need(absolute(launch.executable) && absolute(launch.workingDirectory) && Array.isArray(launch.arguments) && launch.arguments.every(v => typeof v === "string"), "canonical reviewed launch paths and arguments");
  need(typeof expected.profileSha256 === "string" && /^[0-9a-f]{64}$/.test(expected.profileSha256), "exact independent profile commitment");
  need(Array.isArray(config.pins) && config.pins.length > 0 && config.pins.length <= cap.files, "explicit nonempty file census within quota");
  const byPath = new Map(); let previous = "", reservedReadBytes = 0n;
  for (const raw of config.pins) {
    const pin = fields(raw, ["path", "bytes", "sha256"]);
    need(absolute(pin.path) && pin.path > previous && integer(pin.bytes) && typeof pin.sha256 === "string" && /^[0-9a-f]{64}$/.test(pin.sha256), "ordered distinct absolute file pins and exact size/hash");
    need(pin.bytes <= cap.fileBytes, "per-file read quota");
    previous = pin.path; byPath.set(pin.path, pin); reservedReadBytes += BigInt(pin.bytes) + 1n;
  }
  need(reservedReadBytes <= BigInt(cap.totalReadBytes), "prospective complete file read quota plus EOF probes");
  need(byPath.has(launch.executable), "reviewed executable MUST be pinned");
  need(Array.isArray(config.argumentFiles), "explicit file argument indices"); let previousIndex = -1;
  for (const index of config.argumentFiles) {
    need(integer(index) && index > previousIndex && index < launch.arguments.length && byPath.has(launch.arguments[index]), "ordered distinct file arguments MUST be pinned"); previousIndex = index;
  }
  const stable = (a, b) => ["dev", "ino", "size", "mode", "mtimeNs", "ctimeNs"].every(key => a[key] === b[key]);
  function inspect(pin, buffer) {
    const root = path.parse(pin.path).root, parts = pin.path.slice(root.length).split(path.sep); let current = root;
    for (let i = 0; i < parts.length; i++) {
      current = path.join(current, parts[i]); const stat = fs.lstatSync(current);
      need(!stat.isSymbolicLink() && (i === parts.length - 1 ? stat.isFile() : stat.isDirectory()), "regular nonsymlink file components");
    }
    const fd = fs.openSync(pin.path, fs.constants.O_RDONLY | fs.constants.O_NOFOLLOW | fs.constants.O_NONBLOCK);
    try {
      const before = fs.fstatSync(fd, { bigint: true }); need(before.isFile() && before.size === BigInt(pin.bytes), "exact size before hashing");
      const hash = crypto.createHash("sha256"); let remaining = pin.bytes;
      while (remaining > 0) {
        need(!failed, "healthy file admission before read");
        const take = Math.min(remaining, buffer.length), count = fs.readSync(fd, buffer, 0, take, null);
        need(!failed && integer(count, 1) && count <= take, "short read or swallowed admission reentry");
        remaining -= count; readBytes += count; hash.update(buffer.subarray(0, count));
      }
      readBytes++; need(fs.readSync(fd, buffer, 0, 1, null) === 0 && !failed, "growing file or admission reentry");
      const after = fs.fstatSync(fd, { bigint: true }), named = fs.lstatSync(pin.path, { bigint: true });
      need(after.isFile() && named.isFile() && !named.isSymbolicLink() && stable(before, after) && stable(after, named), "file identity or metadata changed during read");
      need(hash.digest("hex") === pin.sha256, "exact independently expected file bytes"); checkedFiles++;
    } finally { fs.closeSync(fd); }
  }
  return Object.freeze({
    admitLaunch: actual => {
      if (failed || busy || completed) { failed = true; throw new Error("A68 launch file admission: failed, reentrant or consumed admission"); }
      busy = true;
      try {
        const observed = capture(actual); need(!failed && same(observed, config.expected), "exact reviewed launch profile and limits before I/O");
        const buffer = Buffer.alloc(cap.chunkBytes);
        for (const pin of byPath.values()) inspect(pin, buffer);
        need(!failed && checkedFiles === byPath.size && BigInt(readBytes) === reservedReadBytes, "complete declared file census and reads"); completed = true;
        // Deliberately void: compatible with the host's mandatory sync admission.
      } catch (error) { failed = true; throw error; } finally { busy = false; }
    },
    snapshot: () => Object.freeze({ failed, completed: completed && !failed, checkedFiles, readBytes, reservedReadBytes: Number(reservedReadBytes),
      metadataUsage: Object.freeze({ ...usage }), scope: Object.freeze({ declaredFileBytesVerified: completed && !failed,
        dependencyClosureComplete: false, sourceToBinaryCorrespondenceProved: false, atomicExecBindingProved: false,
        atomicParentPathProtectionProved: false, scientificExecutionAuthorized: false }) })
  });
}
module.exports = { createLaunchFileAdmission };
