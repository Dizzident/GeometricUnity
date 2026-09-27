"use strict";
// Manufactured JS only. Never load the scientific/replay entrypoints here.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const vm = require("node:vm");
const { createTrustedModuleLoader, trustedModuleLoadIdentity } = require("../a68-trusted-module-loader");
const hash = b => crypto.createHash("sha256").update(b).digest("hex");
function fixture(t, source = { "a.js": 'module.exports={value:7};' }, imports = {}, builtins = []) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), "a68-trusted-loader-"));
  t.after(() => fs.rmSync(dir, { recursive: true, force: true }));
  const modules = Object.keys(source).sort().map(id => {
    const file = path.join(dir, id), bytes = Buffer.from(source[id]); fs.mkdirSync(path.dirname(file), { recursive: true }); fs.writeFileSync(file, bytes);
    return { id, path: file, bytes: bytes.length, sha256: hash(bytes),
      imports: Object.entries(imports[id] || {}).sort(([a], [b]) => a < b ? -1 : 1).map(([request, target]) => ({ request, target })) };
  });
  const total = modules.reduce((n, m) => n + m.bytes, 0);
  return { manifest: { entry: modules[0].id, modules, builtins }, limits: {
    modules: modules.length, sourceBytes: Math.max(1, total), fileBytes: Math.max(1, ...modules.map(m => m.bytes)),
    readBytes: total + modules.length, decodedCharacters: Math.max(1, total), requireCalls: 20, chunkBytes: 7,
    snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } }, admitResources() {} };
}
function healthy(t, source, imports, builtins) {
  const options = fixture(t, source, imports, builtins), loader = createTrustedModuleLoader(options); loader.capture(); return { options, loader };
}

test("verified retained bytes produce private exports and independent load identity", t => {
  const options = fixture(t, { "a.js": 'module.exports={value:require("./b").value+1,base:require("node:path").basename(__filename)};', "b.js": 'exports.value=6;' },
    { "a.js": { "./b": "b.js", "node:path": "node:path" } }, ["node:path"]);
  let admitted = 0;
  options.admitResources = r => {
    admitted++; assert.ok(Object.isFrozen(r)); assert.ok(Object.isFrozen(r.manifest.modules[0]));
    assert.equal(r.sourceBytes, options.limits.sourceBytes); assert.equal(r.readBytes, options.limits.readBytes);
    assert.equal(r.decodedCharacters, r.sourceBytes); assert.equal(r.moduleRecords, 2); assert.equal(r.importEdges, 2);
    assert.equal(r.compileCalls, 2); assert.equal(r.initializationCalls, 2); assert.equal(r.eofScratchBytes, 1);
  };
  const loader = createTrustedModuleLoader(options); assert.equal(admitted, 0); loader.capture(); assert.equal(admitted, 1);
  assert.equal(loader.snapshot().initializedModules, 0);
  const receipt = loader.load(), identity = trustedModuleLoadIdentity(receipt);
  assert.deepEqual(receipt.exports, { value: 7, base: "a.js" }); assert.equal(identity.exports, receipt.exports);
  assert.equal(identity.initializedModules, 2); assert.equal(loader.snapshot().readBytes, options.limits.readBytes);
  assert.equal(identity.scope.entryEvaluatedFromVerifiedBytes, true);
  for (const key of ["completeDependencyClosureProved", "moduleBehaviorImmutable", "hostileCodeSandbox", "runtimeAndNativeIdentityProved", "sourceToBinaryCorrespondenceProved", "totalResourceProof", "scientificExecutionAuthorized"])
    assert.equal(identity.scope[key], false);
  for (const fake of [{ ...receipt }, {}, Object.freeze({ exports: receipt.exports })]) assert.throws(() => trustedModuleLoadIdentity(fake), /private/);
});

test("disk changes and stale ambient cache cannot replace captured module code", t => {
  const { options, loader } = healthy(t, { "a.js": 'module.exports=require("./b");', "b.js": 'module.exports={value:7};' }, { "a.js": { "./b": "b.js" } });
  const file = options.manifest.modules[1].path, previous = require.cache[file];
  t.after(() => { if (previous) require.cache[file] = previous; else delete require.cache[file]; });
  require.cache[file] = { exports: { value: 99 } }; fs.writeFileSync(file, 'module.exports={value:8};');
  assert.deepEqual(loader.load().exports, { value: 7 }); assert.equal(require.cache[file].exports.value, 99);
});

test("compiler receives exactly verified source text and execution never reopens source paths", t => {
  const { options, loader } = healthy(t, { "a.js": 'module.exports=require("./b");', "b.js": 'module.exports=7;' }, { "a.js": { "./b": "b.js" } });
  const compile = vm.compileFunction, open = fs.openSync, observed = [];
  try {
    fs.openSync = () => { throw Error("unexpected source reopen"); };
    vm.compileFunction = function(text, parameters, context) {
      observed.push({ hash: hash(Buffer.from(text, "ascii")), path: context.filename });
      assert.deepEqual(parameters, ["exports", "require", "module", "__filename", "__dirname"]);
      return compile.apply(this, arguments);
    };
    assert.equal(loader.load().exports, 7);
  } finally { vm.compileFunction = compile; fs.openSync = open; }
  assert.deepEqual(observed, options.manifest.modules.map(m => ({ hash: m.sha256, path: m.path })));
});

test("all modules verify before any entry evaluation including unused declared files", t => {
  const options = fixture(t, { "a.js": 'throw Error("entry must not run");', "z.js": 'module.exports=1;' });
  fs.writeFileSync(options.manifest.modules[1].path, 'module.exports=2;');
  const loader = createTrustedModuleLoader(options); assert.throws(() => loader.capture(), /hash/);
  assert.equal(loader.snapshot().initializedModules, 0); assert.equal(loader.snapshot().failed, true);
  fs.writeFileSync(options.manifest.modules[1].path, 'module.exports=1;'); assert.throws(() => loader.capture(), /failed/);
});

test("explicit relative edges handle nested paths without search and CommonJS cycles", t => {
  const { loader } = healthy(t, { "a.js": 'exports.name="a";exports.other=require("./sub/b.js").from;',
    "sub/b.js": 'exports.from=require("../a").name;' }, { "a.js": { "./sub/b.js": "sub/b.js" }, "sub/b.js": { "../a": "a.js" } });
  assert.deepEqual(loader.load().exports, { name: "a", other: "a" }); assert.equal(loader.snapshot().initializedModules, 2);
});

test("late requires use retained sources and quotas and revoke receipt health on failure", t => {
  const options = fixture(t, { "a.js": 'module.exports=()=>require("./b");', "b.js": 'module.exports=7;' }, { "a.js": { "./b": "b.js" } });
  options.limits.requireCalls = 1; const loader = createTrustedModuleLoader(options); loader.capture();
  const receipt = loader.load(); assert.equal(trustedModuleLoadIdentity(receipt).initializedModules, 1);
  fs.writeFileSync(options.manifest.modules[1].path, 'module.exports=8;'); assert.equal(receipt.exports(), 7);
  assert.equal(trustedModuleLoadIdentity(receipt).initializedModules, 2);
  assert.throws(() => receipt.exports(), /bounded require/); assert.throws(() => trustedModuleLoadIdentity(receipt), /live private/);
  assert.equal(loader.snapshot().completed, false);
});

test("undeclared imports and swallowed late import failures permanently poison identity", t => {
  for (const request of ["node:fs", "fs", "./b", "/tmp/no-module.js"]) {
    const { loader } = healthy(t, { "a.js": `module.exports=()=>{try{return require(${JSON.stringify(request)});}catch{return 7;}};` });
    const receipt = loader.load(); assert.equal(receipt.exports(), 7);
    assert.equal(loader.snapshot().failed, true); assert.throws(() => trustedModuleLoadIdentity(receipt), /live private/);
  }
});

test("init syntax errors nonvoid returns and swallowed nested failures fail the whole loader", t => {
  for (const code of ['throw Error("manufactured init failure");', 'this is not valid JS;', 'return true;', 'return Promise.resolve();',
    'try{require("./missing");}catch{}module.exports=7;']) {
    const { loader } = healthy(t, { "a.js": code }); assert.throws(() => loader.load());
    assert.equal(loader.snapshot().failed, true); assert.throws(() => loader.load(), /failed/);
  }
});

test("cyclic partial exports cannot conceal a caught dependency initialization failure", t => {
  const { loader } = healthy(t, { "a.js": 'exports.name="partial";try{require("./b");}catch{}',
    "b.js": 'exports.partial=require("./a");throw Error("cycle failure");' }, { "a.js": { "./b": "b.js" }, "b.js": { "./a": "a.js" } });
  assert.throws(() => loader.load(), /swallowed/); assert.equal(loader.snapshot().completed, false);
  assert.equal(loader.snapshot().initializedModules, 0); assert.throws(() => loader.load(), /failed/);
});

test("malformed manifests cannot reach resource admission or file reading", t => {
  const mutations = [o => o.manifest.entry = "missing.js", o => o.manifest.modules[0].id = "../a.js",
    o => o.manifest.modules.push({ ...o.manifest.modules[0] }), o => o.manifest.modules[0].path += "/../a.js",
    o => o.manifest.modules[0].sha256 = "0", o => o.manifest.builtins = ["fs"], o => o.manifest.builtins = ["node:fs", "node:fs"],
    o => o.manifest.modules[0].imports = [{ request: "./b", target: "missing.js" }],
    o => o.manifest.modules[0].imports = [{ request: "a", target: "a.js" }],
    o => o.manifest.modules[0].imports = [{ request: "node:fs", target: "node:fs" }],
    o => o.manifest.modules[0].imports = [{ request: "./b", target: "a.js" }],
    o => o.manifest.extra = true];
  for (const change of mutations) {
    const options = fixture(t); let admitted = 0; options.admitResources = () => { admitted++; }; change(options);
    assert.throws(() => createTrustedModuleLoader(options)); assert.equal(admitted, 0);
  }
});

test("every source reservation quota rejects one-short before admission", t => {
  for (const key of ["modules", "sourceBytes", "fileBytes", "readBytes", "decodedCharacters"]) {
    const options = fixture(t); let admitted = false; options.admitResources = () => { admitted = true; }; options.limits[key]--;
    assert.throws(() => createTrustedModuleLoader(options), /quota|census|ceilings|source size/); assert.equal(admitted, false);
  }
  const options = fixture(t); options.limits.snapshot.nodes = 1; assert.throws(() => createTrustedModuleLoader(options), /nodes/);
});

test("resource callback errors values promises and swallowed public reentry fail before reads", t => {
  for (const kind of ["error", "value", "promise", "capture", "load"]) {
    const options = fixture(t); let loader;
    options.admitResources = () => {
      if (kind === "error") throw Error("admission refused"); if (kind === "value") return true; if (kind === "promise") return Promise.resolve();
      try { loader[kind](); } catch {} // Swallowing cannot grant admission.
    };
    loader = createTrustedModuleLoader(options); assert.throws(() => loader.capture());
    assert.equal(loader.snapshot().readBytes, 0); assert.equal(loader.snapshot().failed, true);
  }
});

test("capture/load ordering and repetition are terminal", t => {
  const early = createTrustedModuleLoader(fixture(t)); assert.throws(() => early.load(), /after complete/); assert.throws(() => early.capture(), /failed/);
  const twice = createTrustedModuleLoader(fixture(t)); twice.capture(); assert.throws(() => twice.capture(), /one complete/);
  const { loader } = healthy(t); const receipt = loader.load(); assert.throws(() => loader.load(), /one load/);
  assert.throws(() => trustedModuleLoadIdentity(receipt), /live private/);
});

test("snapshot rejects getters and caller mutation cannot change private pins or edges", t => {
  const options = fixture(t); let calls = 0; const original = options.manifest.modules[0].bytes;
  Object.defineProperty(options.manifest.modules[0], "bytes", { configurable: true, get() { calls++; return original; } });
  assert.throws(() => createTrustedModuleLoader(options), /own data/); assert.equal(calls, 0);
  Object.defineProperty(options.manifest.modules[0], "bytes", { value: original });
  const loader = createTrustedModuleLoader(options); options.manifest.modules[0].sha256 = "0".repeat(64); options.manifest.entry = "unknown.js";
  options.limits.requireCalls = 0; loader.capture(); assert.deepEqual(loader.load().exports, { value: 7 });
});

test("read-hook swallowed reentry and partial reads cannot complete capture", t => {
  const options = fixture(t), loader = createTrustedModuleLoader(options), original = fs.readSync;
  let attempted = false;
  try {
    fs.readSync = function(...args) { if (!attempted) { attempted = true; try { loader.load(); } catch {} } return original.apply(this, args); };
    assert.throws(() => loader.capture(), /reentry/);
  } finally { fs.readSync = original; }
  assert.equal(loader.snapshot().failed, true);
  const short = createTrustedModuleLoader(fixture(t));
  try { fs.readSync = () => 0; assert.throws(() => short.capture(), /complete read/); } finally { fs.readSync = original; }
});

test("swallowed public reentry during compilation prevents initialization and receipt creation", t => {
  const { loader } = healthy(t, { "a.js": 'throw Error("must not evaluate");' }), compile = vm.compileFunction;
  try {
    vm.compileFunction = function(...args) { try { loader.load(); } catch {} return compile.apply(this, args); };
    assert.throws(() => loader.load(), /healthy after compile/);
  } finally { vm.compileFunction = compile; }
  assert.equal(loader.snapshot().initializedModules, 0); assert.equal(loader.snapshot().failed, true);
});

test("source byte domain rejects Unicode malformed UTF8 BOM and shebang transformations", t => {
  for (const source of ['// é\nmodule.exports=1;', Buffer.from([0xff]), Buffer.from([0xef, 0xbb, 0xbf]), '#!/usr/bin/node\nmodule.exports=1;']) {
    const loader = createTrustedModuleLoader(fixture(t, { "a.js": source })); assert.throws(() => loader.capture(), /ASCII/);
    assert.equal(loader.snapshot().initializedModules, 0);
  }
});

test("symlinks truncation growth and wrong hashes cannot enter the source cache", t => {
  for (const mode of ["symlink", "truncate", "grow", "hash"]) {
    const options = fixture(t), file = options.manifest.modules[0].path;
    if (mode === "symlink") { const copy = file + ".copy"; fs.renameSync(file, copy); fs.symlinkSync(copy, file); }
    if (mode === "truncate") fs.writeFileSync(file, "");
    if (mode === "grow") fs.appendFileSync(file, " ");
    if (mode === "hash") options.manifest.modules[0].sha256 = "0".repeat(64);
    const loader = createTrustedModuleLoader(options); assert.throws(() => loader.capture()); assert.equal(loader.snapshot().failed, true);
  }
});
