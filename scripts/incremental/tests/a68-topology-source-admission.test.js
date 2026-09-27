"use strict";
// Real reviewed source bytes + MANUFACTURED compiler/launch declarations.
// Fake executable/assembly files are hashed only: NEVER executed or loaded.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { createTopologySourceAdmission } = require("../a68-topology-source-admission");
const { REVIEWED_SOURCE_SHA256 } = require("../a68-context-topology-envelope");
const { profileCommitment } = require("../a68-template-preflight-service");
const { createSourceTemplateValidator } = require("../a68-source-template-validator");
const fixtureProfile = require("./a68-source-template-fixture");
const root = path.resolve(__dirname, "../../.."), project = path.join(root, "studies/phase627_full_mixed_metric_native_field_variation_audit_001");
const clone = x => JSON.parse(JSON.stringify(x)), hash = x => crypto.createHash("sha256").update(x).digest("hex");
function fixture(t, planner = () => {}) {
  const temp = fs.mkdtempSync(path.join(os.tmpdir(), "a68-source-binding-"));
  t.after(() => fs.rmSync(temp, { recursive: true, force: true }));
  const files = Object.fromEntries(["runtime", "generated.cs", "core.dll", "analyzer.dll", "input.json", "config.editorconfig", "source.json", "extra-replay.js", "a.dll", "ref-a.dll", "target"].map(name => {
    const file = path.join(temp, name === "target" ? "target-a.dll" : name);
    fs.writeFileSync(file, "manufactured bytes only: " + name + "\n"); return [name, file];
  }));
  // Csc target basename must match the output basename; use one pinned output
  // as the declared post-copy target in this intentionally synthetic report.
  const compile = [...Object.keys(REVIEWED_SOURCE_SHA256).filter(f => f.endsWith(".cs")).map(f => path.join(root, f)), files["generated.cs"]];
  const replay = [...Object.keys(REVIEWED_SOURCE_SHA256).filter(f => f.endsWith(".js")).map(f => path.join(root, f)), files["extra-replay.js"]].sort();
  const report = { properties: { Configuration: "Release", TargetFramework: "net10.0", MSBuildVersion: "18.0.2", NETCoreSdkVersion: "10.0.100",
    UseSharedCompilation: "false", Deterministic: "true", Optimize: "true", DebugType: "portable", DefineConstants: "TRACE;RELEASE;NET10_0", TargetPath: files["a.dll"] },
    items: { Compile: compile, ReferencePath: [files["core.dll"]], Analyzer: [files["analyzer.dll"]], AdditionalFiles: [files["input.json"]],
      EditorConfigFiles: [files["config.editorconfig"]], CscCommandLineArgs: ["/noconfig", "/nostdlib+", "/define:TRACE;RELEASE;NET10_0", "/optimize+", "/deterministic+", "/debug-", "/debug:portable", "/target:exe",
        "/out:" + files["a.dll"], "/refout:" + files["ref-a.dll"], "/reference:" + files["core.dll"], "/analyzer:" + files["analyzer.dll"],
        "/additionalfile:" + files["input.json"], "/analyzerconfig:" + files["config.editorconfig"], "/sourcelink:" + files["source.json"], ...compile] } };
  const all = [...new Set([...compile, ...replay, ...Object.values(files)])].sort();
  const pins = all.map(file => { const bytes = fs.readFileSync(file); return { path: file, bytes: bytes.length, sha256: hash(bytes) }; });
  const profile = fixtureProfile.profile();
  const expected = { launch: { executable: files.runtime, arguments: [files["a.dll"]], workingDirectory: root, environment: {} },
    profileSha256: profileCommitment(profile, fixtureProfile.limits), processLimits: { timeoutMs: 1000 }, transportLimits: { timeoutMs: 900 } };
  const snapshotLimits = { nodes: 1000000, arraySlots: 1000000, stringCharacters: 10000000, maxDepth: 16 };
  const options = { repositoryRoot: root, projectDirectory: project, compilerReport: report,
    compilerLimits: { arguments: 1000, inputFiles: 1000, pathCharacters: 1000000, snapshot: { ...snapshotLimits } }, replayInputs: replay,
    launchAdmission: { expected, pins, argumentFiles: [0], limits: { files: pins.length, fileBytes: 2000000,
      totalReadBytes: pins.reduce((s, p) => s + p.bytes + 1, 0), chunkBytes: 4096, snapshot: { ...snapshotLimits } } },
    snapshotLimits, admitPlanning: planner };
  const descriptor = { operation: "prospective-tensor-topology-v1", contexts: 705, profile, sourceRevisions: clone(REVIEWED_SOURCE_SHA256) };
  return { options, descriptor, expected, files, temp };
}

test("reviewed bytes and all declared inputs gate real validator topology admission without provenance promotion", t => {
  let planned = 0;
  const f = fixture(t, d => { planned++; assert.ok(Object.isFrozen(d.profile.capture)); });
  const a = createTopologySourceAdmission(f.options);
  assert.equal(a.snapshot().files.checkedFiles, 0); assert.equal(planned, 0);
  a.admitLaunch(f.expected);
  const v = createSourceTemplateValidator(f.descriptor.profile, fixtureProfile.limits, () => {}, a.admitTopology);
  v.validateCaptureDeclaration(f.descriptor.profile.capture);
  assert.equal(planned, 1); assert.equal(v.snapshot().topologyChecked, true);
  const r = a.snapshot(); assert.equal(r.launchVerified, true); assert.equal(r.topologyAdmitted, true);
  assert.equal(r.reviewedCompile, 11); assert.equal(r.reviewedReplay, 12);
  assert.equal(r.reportedCompilerInputs, 17); assert.equal(r.declaredReplayInputs, 13);
  assert.equal(r.files.checkedFiles, f.options.launchAdmission.pins.length);
  assert.equal(r.scope.declaredInputBytesVerifiedAtLaunch, true);
  for (const key of ["compilerConsumptionProved", "loadedReplayModuleIdentityProved", "sourceToBinaryCorrespondenceProved", "dependencyClosureComplete", "immutableBuildLoadBoundaryProved", "totalResourceProof", "scientificExecutionAuthorized"])
    assert.equal(r.scope[key], false);
});

test("reviewed files need their exact compiler/replay membership even if already pinned", t => {
  for (const kind of ["compile", "replay"]) {
    const f = fixture(t);
    if (kind === "compile") {
      const file = f.options.compilerReport.items.Compile.shift();
      f.options.compilerReport.items.CscCommandLineArgs = f.options.compilerReport.items.CscCommandLineArgs.filter(x => x !== file);
      // Membership in another input role is not Compile membership.
      f.options.compilerReport.items.AdditionalFiles.push(file);
      f.options.compilerReport.items.CscCommandLineArgs.push("/additionalfile:" + file);
    } else f.options.replayInputs.splice(f.options.replayInputs.findIndex(p => p.startsWith(root)), 1);
    assert.throws(() => createTopologySourceAdmission(f.options), /must be in Compile|must be in replay/);
  }
});

test("reviewed hashes cannot be caller-replaced and every extra reported input/output needs a pin", t => {
  const wrong = fixture(t); wrong.options.launchAdmission.pins.find(p => p.path.startsWith(root)).sha256 = "0".repeat(64);
  assert.throws(() => createTopologySourceAdmission(wrong.options), /reviewed source byte commitment/);
  for (const name of ["generated.cs", "core.dll", "analyzer.dll", "input.json", "config.editorconfig", "source.json", "extra-replay.js", "ref-a.dll"]) {
    const f = fixture(t); f.options.launchAdmission.pins = f.options.launchAdmission.pins.filter(p => p.path !== f.files[name]);
    assert.throws(() => createTopologySourceAdmission(f.options), /must be pinned/);
  }
});

test("remapped project duplicate replay paths and malformed compiler captures fail before reads", t => {
  for (const change of [f => f.options.projectDirectory = f.temp,
    f => f.options.replayInputs.push(f.options.replayInputs[0]),
    f => f.options.compilerReport.items.CscCommandLineArgs = [],
    f => f.options.compilerReport.properties.Configuration = "Debug"]) {
    const f = fixture(t); change(f); fs.unlinkSync(f.files.runtime);
    assert.throws(() => createTopologySourceAdmission(f.options), /project identity|ordered unique|incremental skipped|Release/);
  }
});

test("topology-before-launch and repeat admissions permanently poison the adapter", t => {
  for (const mode of ["early", "repeatLaunch", "repeatTopology"]) {
    const f = fixture(t), a = createTopologySourceAdmission(f.options);
    if (mode !== "early") a.admitLaunch(f.expected);
    if (mode === "repeatTopology") a.admitTopology(f.descriptor);
    assert.throws(() => mode === "repeatLaunch" ? a.admitLaunch(f.expected) : a.admitTopology(f.descriptor), /one launch|before one topology/);
    assert.equal(a.snapshot().failed, true); assert.equal(a.snapshot().topologyAdmitted, false);
    assert.throws(() => a.admitLaunch(f.expected), /failed/);
  }
});

test("changed operation counts commitments or actual profile cannot reach planner", t => {
  for (const change of [d => d.operation += "x", d => d.contexts--, d => delete d.sourceRevisions[Object.keys(d.sourceRevisions)[0]],
    d => d.sourceRevisions[Object.keys(d.sourceRevisions)[0]] = "0".repeat(64),
    d => d.profile.budgets[0].resources.trace.nodes++, d => d.extra = true]) {
    let calls = 0; const f = fixture(t, () => { calls++; }), a = createTopologySourceAdmission(f.options);
    a.admitLaunch(f.expected); change(f.descriptor);
    assert.throws(() => a.admitTopology(f.descriptor), /descriptor|profile matches|closed ordered/);
    assert.equal(calls, 0); assert.equal(a.snapshot().failed, true);
  }
});

test("changed declared bytes fail delegated reads and cannot be repaired into acceptance", t => {
  const f = fixture(t), a = createTopologySourceAdmission(f.options), original = fs.readFileSync(f.files["extra-replay.js"]);
  fs.writeFileSync(f.files["extra-replay.js"], Buffer.alloc(original.length, 65));
  assert.throws(() => a.admitLaunch(f.expected), /expected file bytes/);
  fs.writeFileSync(f.files["extra-replay.js"], original);
  assert.throws(() => a.admitLaunch(f.expected), /failed/); assert.equal(a.snapshot().launchVerified, false);
});

test("planner errors Promise values and swallowed nested admissions cannot complete", t => {
  for (const planner of [() => { throw new Error("planner cap"); }, async () => {}, () => true]) {
    const f = fixture(t, planner), a = createTopologySourceAdmission(f.options); a.admitLaunch(f.expected);
    assert.throws(() => a.admitTopology(f.descriptor), /planner cap|synchronous void/); assert.equal(a.snapshot().failed, true);
  }
  for (const action of [a => a.admitLaunch({}), a => a.admitTopology({})]) {
    let a; const f = fixture(t, () => { assert.throws(() => action(a), /reentrant/); }); a = createTopologySourceAdmission(f.options);
    a.admitLaunch(f.expected); assert.throws(() => a.admitTopology(f.descriptor), /planner callback reentry/);
    assert.equal(a.snapshot().topologyAdmitted, false);
  }
});

test("snapshot/streaming reentry and getters fail closed without invoking getters", t => {
  const f = fixture(t); let calls = 0;
  Object.defineProperty(f.options, "compilerReport", { enumerable: true, get() { calls++; return {}; } });
  assert.throws(() => createTopologySourceAdmission(f.options), /own data/); assert.equal(calls, 0);
  const g = fixture(t), a = createTopologySourceAdmission(g.options);
  const proxy = new Proxy(g.expected, { ownKeys(target) { assert.throws(() => a.admitTopology(g.descriptor), /reentrant/); return Reflect.ownKeys(target); } });
  assert.throws(() => a.admitLaunch(proxy), /snapshot reentry/); assert.equal(a.snapshot().files.checkedFiles, 0);
  const h = fixture(t), b = createTopologySourceAdmission(h.options), read = fs.readSync; let once = false;
  fs.readSync = (...args) => { if (!once) { once = true; assert.throws(() => b.admitTopology(h.descriptor), /reentrant/); } return read(...args); };
  try { assert.throws(() => b.admitLaunch(h.expected), /healthy file admission/); assert.equal(b.snapshot().launchVerified, false); }
  finally { fs.readSync = read; }
});

test("caller mutation cannot replace private configuration and all snapshots share one quota", t => {
  const f = fixture(t), expected = clone(f.expected), descriptor = clone(f.descriptor), a = createTopologySourceAdmission(f.options);
  f.options.replayInputs.length = 0; f.options.launchAdmission.pins[0].sha256 = "0".repeat(64);
  f.options.snapshotLimits.nodes = 1; f.options.launchAdmission.expected.profileSha256 = "0".repeat(64);
  a.admitLaunch(expected); a.admitTopology(descriptor); assert.equal(a.snapshot().topologyAdmitted, true);
  const used = a.snapshot().metadataUsage;
  for (const key of ["nodes", "arraySlots", "stringCharacters"]) {
    const g = fixture(t); g.options.snapshotLimits[key] = used[key] - 1;
    const b = createTopologySourceAdmission(g.options); b.admitLaunch(g.expected);
    assert.throws(() => b.admitTopology(g.descriptor), /quota|ceiling/); assert.equal(b.snapshot().failed, true);
  }
});
