"use strict";
// Capture/read only: never evaluate the aggregate or its scientific recipe modules.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto"), vm = require("node:vm");
const { PREFLIGHT_SOURCE_PINS: pins, PREFLIGHT_BUILTINS: builtins, ENTRY, createPreflightModuleManifest } = require("../a68-preflight-module-manifest");
const { createTrustedModuleLoader } = require("../a68-trusted-module-loader");
const root = path.resolve(__dirname, "../../..");
const digest = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const limits = () => ({ modules: 23, sourceBytes: 261049, fileBytes: 33732, readBytes: 261072,
  decodedCharacters: 261049, requireCalls: 66, chunkBytes: 65536,
  snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } });

test("static manifest binds every reviewed file size hash and ASCII source", () => {
  assert.equal(pins.length, 23); assert.equal(pins.reduce((n, m) => n + m.bytes, 0), 261049);
  assert.deepEqual(pins.map(m => m.id), pins.map(m => m.id).sort());
  assert.equal(new Set(pins.map(m => m.id)).size, 23);
  for (const m of pins) {
    const bytes = fs.readFileSync(path.join(root, "scripts/incremental", m.id));
    assert.equal(bytes.length, m.bytes, m.id); assert.equal(digest(bytes), m.sha256, m.id);
    assert.ok(bytes.every(b => b === 9 || b === 10 || b === 13 || b >= 32 && b <= 126), m.id);
  }
});

test("reviewed literal import census matches explicit edges with no missing or unreachable module", () => {
  // Regression aid for these hash-pinned reviewed sources, NOT a general JS
  // parser or proof that hostile code has no alternative loader mechanism.
  const ids = new Set(pins.map(m => m.id)), usedBuiltins = new Set(); let local = 0, native = 0;
  for (const m of pins) {
    const source = fs.readFileSync(path.join(root, "scripts/incremental", m.id), "utf8");
    const requests = [...source.matchAll(/require\("([^"\n]+)"\)/g)].map(match => match[1]).sort();
    assert.deepEqual(m.imports.map(e => e.request), requests, m.id);
    assert.equal((source.match(/\brequire\s*\(/g) || []).length, requests.length, m.id);
    for (const edge of m.imports) {
      if (edge.request.startsWith("node:")) { native++; usedBuiltins.add(edge.target); assert.equal(edge.request, edge.target); }
      else { local++; assert.equal(path.posix.normalize(path.posix.join(path.posix.dirname(m.id), edge.request)) + ".js", edge.target); assert.ok(ids.has(edge.target)); }
    }
  }
  assert.equal(local, 53); assert.equal(native, 13);
  assert.deepEqual([...usedBuiltins].sort(), builtins);
  assert.deepEqual(builtins, ["node:child_process", "node:crypto", "node:fs", "node:path", "node:perf_hooks", "node:stream/promises"]);
  const reached = new Set();
  function visit(id) { if (reached.has(id)) return; reached.add(id); for (const e of pins.find(m => m.id === id).imports) if (!e.target.startsWith("node:")) visit(e.target); }
  visit(ENTRY); assert.deepEqual([...reached].sort(), [...ids].sort());
});

test("manifest instantiation is immutable and rejects ambiguous repository roots", () => {
  const manifest = createPreflightModuleManifest(root);
  assert.equal(manifest.entry, "a68-preflight-entry.js"); assert.equal(manifest.modules.length, 23);
  for (const m of manifest.modules) {
    assert.equal(m.path, path.join(root, "scripts/incremental", m.id)); assert.ok(Object.isFrozen(m));
    assert.ok(Object.isFrozen(m.imports)); for (const edge of m.imports) assert.ok(Object.isFrozen(edge));
  }
  assert.ok(Object.isFrozen(manifest)); assert.ok(Object.isFrozen(manifest.modules)); assert.ok(Object.isFrozen(manifest.builtins));
  assert.throws(() => { manifest.modules[0].sha256 = "0".repeat(64); }, TypeError);
  for (const bad of [null, {}, ".", "/", root + "/", root + "/../GeometricUnity", "/é", "/" + "a".repeat(4096)])
    assert.throws(() => createPreflightModuleManifest(bad), /repository root/);
});

test("real reviewed module capture verifies bytes without compiling or ambient-loading production modules", () => {
  const before = Object.keys(require.cache).sort(), compile = vm.compileFunction;
  let admitted = 0;
  const loader = createTrustedModuleLoader({ manifest: createPreflightModuleManifest(root), limits: limits(), admitResources(reservation) {
    admitted++; assert.equal(reservation.sourceBytes, 261049); assert.equal(reservation.readBytes, 261072);
    assert.equal(reservation.moduleRecords, 23); assert.equal(reservation.importEdges, 66);
    assert.equal(reservation.compileCalls, 23); assert.equal(reservation.initializationCalls, 23);
  } });
  try { vm.compileFunction = () => { throw Error("production compilation forbidden in capture-only test"); }; loader.capture(); }
  finally { vm.compileFunction = compile; }
  assert.equal(admitted, 1); assert.deepEqual(Object.keys(require.cache).sort(), before);
  const result = loader.snapshot(); assert.equal(result.verifiedModules, 23); assert.equal(result.readBytes, 261072);
  assert.equal(result.initializedModules, 0); assert.equal(result.requireCalls, 0); assert.equal(result.completed, false);
  assert.equal(result.scope.verifiedRetainedSourceInputs, true); assert.equal(result.scope.entryEvaluatedFromVerifiedBytes, false);
  assert.equal(result.scope.totalResourceProof, false); assert.equal(result.scope.scientificExecutionAuthorized, false);
});

test("missing manifest dependency and every one-short capture quota fail before resource admission", () => {
  const manifest = createPreflightModuleManifest(root);
  for (const key of ["modules", "sourceBytes", "fileBytes", "readBytes", "decodedCharacters"]) {
    const cap = limits(); cap[key]--; let calls = 0;
    assert.throws(() => createTrustedModuleLoader({ manifest, limits: cap, admitResources() { calls++; } })); assert.equal(calls, 0);
  }
  const copy = JSON.parse(JSON.stringify(manifest)); copy.modules = copy.modules.filter(m => m.id !== "a68-canonical-wire.js");
  assert.throws(() => createTrustedModuleLoader({ manifest: copy, limits: limits(), admitResources() {} }), /target/);
});

test("fixed manifest never regenerates expected hashes from changed source files", t => {
  const temporary = fs.mkdtempSync(path.join(os.tmpdir(), "a68-manifest-capture-"));
  t.after(() => fs.rmSync(temporary, { recursive: true, force: true }));
  const target = path.join(temporary, "scripts/incremental"); fs.mkdirSync(target, { recursive: true });
  for (const m of pins) fs.copyFileSync(path.join(root, "scripts/incremental", m.id), path.join(target, m.id));
  const changed = path.join(target, pins[0].id), bytes = fs.readFileSync(changed); bytes[0] = bytes[0] === 32 ? 33 : 32; fs.writeFileSync(changed, bytes);
  const manifest = createPreflightModuleManifest(temporary); assert.equal(manifest.modules[0].sha256, pins[0].sha256);
  const loader = createTrustedModuleLoader({ manifest, limits: limits(), admitResources() {} });
  assert.throws(() => loader.capture(), /retained source hash/); assert.equal(loader.snapshot().initializedModules, 0);
  assert.equal(loader.snapshot().failed, true);
});
