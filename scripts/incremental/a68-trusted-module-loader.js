"use strict";

// Same-byte input binding for REVIEWED TRUSTED CommonJS-subset code, NOT a
// security sandbox. Ambient globals, native builtins, async effects, monkey
// patches, VM/JIT/runtime/bootstrap and module behavior remain trust boundaries.
// No production module is loaded by importing this primitive. Metadata quotas
// are logical copies only; the external admission hook must separately justify
// initialization/compile/library/retained-lifetime costs. Void is not that proof.
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto");
const vm = require("node:vm"), { isBuiltin } = require("node:module");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const identities = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 trusted module loader: " + why); };
const natural = n => Number.isSafeInteger(n) && !Object.is(n, -0) && n >= 0;
const absolute = p => typeof p === "string" && path.isAbsolute(p) && path.resolve(p) === p && p !== path.parse(p).root;
const moduleId = s => typeof s === "string" && /^[A-Za-z0-9_-]+(?:\/[A-Za-z0-9_-]+)*\.js$/.test(s);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value), "closed data object");
  const keys = Reflect.ownKeys(value);
  need(keys.length === names.length && keys.every((k, i) => k === names[i]), "closed ordered fields");
  return Object.fromEntries(names.map(k => { const d = Object.getOwnPropertyDescriptor(value, k);
    need(d && Object.hasOwn(d, "value"), "own data field " + k); return [k, d.value]; }));
}
function createTrustedModuleLoader(options) {
  options = fields(options, ["manifest", "limits", "admitResources"]);
  const admit = options.admitResources; need(typeof admit === "function", "mandatory synchronous resource admission");
  const raw = fields(options.limits, ["modules", "sourceBytes", "fileBytes", "readBytes", "decodedCharacters", "requireCalls", "chunkBytes", "snapshot"]);
  const config = snapshotCanonicalMetadata({ manifest: options.manifest, limits: raw }, raw.snapshot).value;
  const cap = config.limits, manifest = fields(config.manifest, ["entry", "modules", "builtins"]);
  need([cap.modules, cap.sourceBytes, cap.fileBytes, cap.readBytes, cap.decodedCharacters, cap.requireCalls, cap.chunkBytes].every(n => natural(n) && n > 0), "positive safe quotas");
  need(cap.chunkBytes <= 1048576, "at most1MiB read requests");
  need(Array.isArray(manifest.modules) && manifest.modules.length > 0 && manifest.modules.length <= cap.modules, "bounded nonempty module census");
  need(Array.isArray(manifest.builtins), "explicit builtin census");
  let previous = "", bytes = 0n, edges = 0; const paths = new Set(), ids = new Set();
  for (const item of manifest.modules) {
    const m = fields(item, ["id", "path", "bytes", "sha256", "imports"]);
    need(moduleId(m.id) && m.id > previous && absolute(m.path) && !paths.has(m.path), "ordered unique module IDs and canonical distinct source paths");
    need(natural(m.bytes) && m.bytes <= cap.fileBytes && /^[0-9a-f]{64}$/.test(m.sha256), "bounded exact source size/hash");
    need(Array.isArray(m.imports), "explicit module import edges");
    previous = m.id; paths.add(m.path); ids.add(m.id); bytes += BigInt(m.bytes);
    need(natural(edges + m.imports.length), "safe edge census"); edges += m.imports.length;
  }
  need(ids.has(manifest.entry), "declared entry module"); previous = "";
  for (const name of manifest.builtins) {
    need(typeof name === "string" && name.startsWith("node:") && isBuiltin(name) && name > previous, "ordered unique explicit node builtins"); previous = name;
  }
  for (const m of manifest.modules) {
    previous = "";
    for (const item of m.imports) {
      const e = fields(item, ["request", "target"]);
      need(typeof e.request === "string" && e.request > previous, "ordered unique import requests"); previous = e.request;
      if (e.request.startsWith("node:")) need(e.target === e.request && manifest.builtins.includes(e.target), "explicit builtin edge");
      else {
        need(/^\.\.?\/[A-Za-z0-9_./-]+$/.test(e.request) && !e.request.includes("//"), "explicit relative JS import only");
        const normalized = path.posix.normalize(path.posix.join(path.posix.dirname(m.id), e.request));
        need(ids.has(e.target) && (normalized === e.target || normalized + ".js" === e.target), "exact declared relative target without search");
      }
    }
  }
  const reads = bytes + BigInt(manifest.modules.length);
  need(bytes <= BigInt(cap.sourceBytes) && bytes <= BigInt(cap.decodedCharacters) && reads <= BigInt(cap.readBytes), "prospective retained source/decode/read ceilings including EOF");
  // Descriptor construction and manifest validation above are bootstrap metadata,
  // not purportedly covered by this later external execution-stage admission.
  const reservation = Object.freeze({ operation: "trusted-commonjs-load-v1", manifest: config.manifest,
    sourceBytes: Number(bytes), decodedCharacters: Number(bytes), readBytes: Number(reads),
    eofScratchBytes: 1, moduleRecords: manifest.modules.length, importEdges: edges,
    compileCalls: manifest.modules.length, initializationCalls: manifest.modules.length, requireCalls: cap.requireCalls });
  let failed = false, busy = false, captured = false, completed = false;
  let verifiedModules = 0, initializedModules = 0, requireCalls = 0, readBytes = 0;
  let sources, cache;
  function guard(fn) {
    if (failed || busy) { failed = true; throw new Error("A68 trusted module loader: failed or reentrant public operation"); }
    busy = true;
    try { const result = fn(); need(!failed, "swallowed reentry/failure"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  const stable = (a, b) => ["dev", "ino", "size", "mode", "mtimeNs", "ctimeNs"].every(k => a[k] === b[k]);
  function readSource(m, probe) {
    const root = path.parse(m.path).root, parts = m.path.slice(root.length).split(path.sep); let current = root;
    for (let i = 0; i < parts.length; i++) {
      current = path.join(current, parts[i]); const s = fs.lstatSync(current);
      need(!failed && !s.isSymbolicLink() && (i === parts.length - 1 ? s.isFile() : s.isDirectory()), "nonsymlink regular source components");
    }
    const fd = fs.openSync(m.path, fs.constants.O_RDONLY | fs.constants.O_NOFOLLOW | fs.constants.O_NONBLOCK);
    try {
      const before = fs.fstatSync(fd, { bigint: true }); need(!failed && before.isFile() && before.size === BigInt(m.bytes), "exact source size before read");
      const buffer = Buffer.alloc(m.bytes); let offset = 0;
      while (offset < buffer.length) {
        need(!failed, "healthy loader before source read");
        const take = Math.min(cap.chunkBytes, buffer.length - offset), count = fs.readSync(fd, buffer, offset, take, null);
        need(!failed && natural(count) && count > 0 && count <= take, "complete read without swallowed reentry"); offset += count; readBytes += count;
      }
      readBytes++; need(fs.readSync(fd, probe, 0, 1, null) === 0 && !failed, "EOF without growth or reentry");
      const after = fs.fstatSync(fd, { bigint: true }), named = fs.lstatSync(m.path, { bigint: true });
      need(!failed && after.isFile() && named.isFile() && !named.isSymbolicLink() && stable(before, after) && stable(after, named), "stable source identity during capture");
      need(crypto.createHash("sha256").update(buffer).digest("hex") === m.sha256 && !failed, "exact retained source hash");
      // Explicit ASCII source subset: no silent replacement decoding, BOM or
      // shebang transform. UTF8/non-ASCII support requires a separate review.
      need(buffer.every(b => b === 9 || b === 10 || b === 13 || b >= 32 && b <= 126) && !(buffer[0] === 35 && buffer[1] === 33), "untransformed ASCII JS source");
      verifiedModules++; return buffer;
    } finally { fs.closeSync(fd); }
  }
  function initialize(id) {
    need(!failed && captured, "verified complete source capture before evaluation");
    if (cache.has(id)) return cache.get(id).module.exports; // CommonJS partial exports support cycles.
    const m = manifest.modules.find(x => x.id === id); need(m && sources.has(id), "declared retained module");
    const module = { exports: {} }, record = { module, initialized: false }; cache.set(id, record);
    const localRequire = request => {
      try {
        need(!failed && typeof request === "string" && requireCalls < cap.requireCalls, "healthy bounded require request"); requireCalls++;
        const edge = m.imports.find(e => e.request === request); need(edge, "undeclared import refused without ambient resolution");
        const value = edge.target.startsWith("node:") ? require(edge.target) : initialize(edge.target);
        need(!failed, "swallowed nested import failure"); return value;
      } catch (error) { failed = true; throw error; }
    };
    const text = sources.get(id).toString("ascii");
    const compiled = vm.compileFunction(text, ["exports", "require", "module", "__filename", "__dirname"], { filename: m.path });
    need(!failed, "healthy after compile");
    const returned = compiled.call(module.exports, module.exports, localRequire, module, m.path, path.dirname(m.path));
    need(!failed && returned === undefined, "synchronous void module initialization without swallowed failure");
    record.initialized = true; initializedModules++; return module.exports;
  }
  const scope = () => Object.freeze({ verifiedRetainedSourceInputs: captured && !failed,
    entryEvaluatedFromVerifiedBytes: completed && !failed, privateRequireResolution: true,
    completeDependencyClosureProved: false, moduleBehaviorImmutable: false, hostileCodeSandbox: false,
    runtimeAndNativeIdentityProved: false, sourceToBinaryCorrespondenceProved: false,
    totalResourceProof: false, scientificExecutionAuthorized: false });
  return Object.freeze({
    capture: () => guard(() => {
      need(!captured && !completed, "one complete capture");
      const result = admit(reservation); need(!failed && result === undefined, "synchronous void admission without swallowed reentry");
      sources = new Map(); cache = new Map(); const probe = Buffer.alloc(1);
      for (const m of manifest.modules) { sources.set(m.id, readSource(m, probe)); need(!failed, "healthy complete source capture"); }
      need(verifiedModules === manifest.modules.length && readBytes === Number(reads), "complete verified module census"); captured = true;
    }),
    load: () => guard(() => {
      need(captured && !completed, "one load after complete capture"); const exports = initialize(manifest.entry);
      need(!failed, "healthy entry initialization"); completed = true;
      const receipt = Object.freeze({ exports });
      identities.set(receipt, () => { need(completed && !failed, "live private module-load identity");
        return Object.freeze({ entry: manifest.entry, exports, manifest: config.manifest, initializedModules, scope: scope() }); });
      return receipt;
    }),
    snapshot: () => Object.freeze({ failed, captured: captured && !failed, completed: completed && !failed,
      verifiedModules, initializedModules, requireCalls, readBytes, reservation, scope: scope() })
  });
}
function trustedModuleLoadIdentity(receipt) {
  const identity = identities.get(receipt); need(identity, "private module-load receipt required"); return identity();
}
module.exports = { createTrustedModuleLoader, trustedModuleLoadIdentity };
