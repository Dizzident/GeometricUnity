"use strict";
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { createLaunchFileAdmission } = require("../a68-launch-file-admission");
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
function fixture(t) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-launch-pins-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const files = ["executable", "input.dll", "source.cs", "zero"].map((name, i) => {
    const file = path.join(root, name), bytes = Buffer.from(i === 3 ? "" : "manufactured-only " + name + "\n");
    fs.writeFileSync(file, bytes); return { path: file, bytes: bytes.length, sha256: hash(bytes) };
  });
  const expected = { launch: { executable: files[0].path, arguments: ["exec", files[1].path], workingDirectory: root, environment: {} },
    profileSha256: "a".repeat(64), processLimits: { timeoutMs: 1000, killGraceMs: 100, stdoutBytes: 128, stderrBytes: 128 },
    transportLimits: { frameBytes: 1000, totalInputBytes: 10000, replyBytes: 1000, totalOutputBytes: 10000, timeoutMs: 900 } };
  const limits = { files: 4, fileBytes: 1000, totalReadBytes: files.reduce((n, pin) => n + pin.bytes + 1, 0), chunkBytes: 7,
    snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } };
  return { root, files, expected, limits, options: { expected, pins: files, argumentFiles: [1], limits } };
}
test("complete streaming file admission includes empty files and EOF probes but grants no build or scientific provenance", t => {
  const f = fixture(t), admission = createLaunchFileAdmission(f.options);
  assert.equal(admission.admitLaunch(f.expected), undefined); const result = admission.snapshot();
  assert.equal(result.completed, true); assert.equal(result.checkedFiles, 4); assert.equal(result.readBytes, f.limits.totalReadBytes);
  assert.equal(result.scope.declaredFileBytesVerified, true);
  for (const key of ["dependencyClosureComplete", "sourceToBinaryCorrespondenceProved", "atomicExecBindingProved", "atomicParentPathProtectionProved", "scientificExecutionAuthorized"]) assert.equal(result.scope[key], false);
});
test("launch executable arguments environment profile and limits must all equal the independent snapshot", t => {
  for (const change of [v => v.launch.arguments.push("unreviewed"), v => v.launch.environment.NEW = "value", v => v.launch.executable += "2",
    v => v.launch.workingDirectory += "/other", v => v.profileSha256 = "b".repeat(64), v => v.processLimits.timeoutMs++, v => v.transportLimits.replyBytes++]) {
    const f = fixture(t), admission = createLaunchFileAdmission(f.options), actual = JSON.parse(JSON.stringify(f.expected)); change(actual);
    fs.unlinkSync(f.files[0].path); assert.throws(() => admission.admitLaunch(actual), /exact reviewed launch/); assert.equal(admission.snapshot().checkedFiles, 0);
  }
});
test("same-size changed bytes missing files and changed sizes poison without repair or retry", t => {
  for (const mode of ["content", "size", "missing"]) {
    const f = fixture(t), admission = createLaunchFileAdmission(f.options);
    if (mode === "missing") fs.unlinkSync(f.files[2].path);
    else fs.writeFileSync(f.files[2].path, Buffer.alloc(f.files[2].bytes + (mode === "size" ? 1 : 0), 65));
    assert.throws(() => admission.admitLaunch(f.expected)); assert.equal(admission.snapshot().failed, true);
    assert.throws(() => admission.admitLaunch(f.expected), /failed/); assert.equal(admission.snapshot().completed, false);
  }
});
test("declared executable and explicitly identified file arguments cannot be omitted or aliased", t => {
  for (const change of [f => f.options.pins.shift(), f => f.options.pins.splice(1, 1), f => f.options.argumentFiles = [1, 1],
    f => f.options.argumentFiles = [0], f => f.options.argumentFiles = [2], f => f.options.pins.reverse(),
    f => f.options.pins[0].path += "/../executable", f => f.options.pins[0].sha256 = "F".repeat(64)]) {
    const f = fixture(t); change(f); assert.throws(() => createLaunchFileAdmission(f.options));
  }
});
test("prospective quotas reject the entire manifest before any file reads", t => {
  for (const change of [c => c.files--, c => c.fileBytes = 1, c => c.totalReadBytes--, c => c.chunkBytes = 1048577,
    c => c.snapshot.nodes = 1, c => c.snapshot.maxDepth = 17]) {
    const f = fixture(t); fs.unlinkSync(f.files[0].path); change(f.limits);
    assert.throws(() => createLaunchFileAdmission(f.options), /quota|ceiling|scratch/);
  }
});
test("configuration and live launch getters are never invoked", t => {
  const f = fixture(t); let calls = 0;
  Object.defineProperty(f.options.pins[0], "bytes", { enumerable: true, get() { calls++; return 10; } });
  assert.throws(() => createLaunchFileAdmission(f.options), /own data/); assert.equal(calls, 0);
  const g = fixture(t), admission = createLaunchFileAdmission(g.options);
  Object.defineProperty(g.expected.launch, "executable", { enumerable: true, get() { calls++; return "bad"; } });
  assert.throws(() => admission.admitLaunch(g.expected), /own data/); assert.equal(calls, 0); assert.equal(admission.snapshot().failed, true);
});
test("caller changes to pins quotas and expected launch cannot change the private frozen expectations", t => {
  const f = fixture(t), actual = JSON.parse(JSON.stringify(f.expected)), admission = createLaunchFileAdmission(f.options);
  f.files[0].sha256 = "0".repeat(64); f.limits.totalReadBytes = 1; f.expected.launch.arguments[0] = "changed";
  admission.admitLaunch(actual); assert.equal(admission.snapshot().completed, true);
  assert.throws(() => admission.admitLaunch(actual), /consumed/); assert.equal(admission.snapshot().completed, false);
});
test("symlink final and parent components cannot satisfy declared file pins", t => {
  const f = fixture(t), admission = createLaunchFileAdmission(f.options), saved = f.files[0].path + ".saved";
  fs.renameSync(f.files[0].path, saved); fs.symlinkSync(saved, f.files[0].path);
  assert.throws(() => admission.admitLaunch(f.expected), /nonsymlink/);
  const g = fixture(t), alias = g.root + "-link"; fs.symlinkSync(g.root, alias, "dir"); t.after(() => fs.unlinkSync(alias));
  g.options.pins.forEach(pin => pin.path = pin.path.replace(g.root, alias));
  g.expected.launch.executable = g.files[0].path; g.expected.launch.arguments[1] = g.files[1].path;
  assert.throws(() => createLaunchFileAdmission(g.options).admitLaunch(g.expected), /nonsymlink/);
});
test("short growing and changed file metadata during streaming are rejected", t => {
  for (const mode of ["short", "growth", "metadata"]) {
    const f = fixture(t), admission = createLaunchFileAdmission(f.options), original = fs.readSync; let once = false;
    fs.readSync = (...args) => {
      if (!once) { once = true; if (mode === "short") return 0;
        if (mode === "growth") fs.appendFileSync(f.files[0].path, "x");
        if (mode === "metadata") fs.utimesSync(f.files[0].path, 1, 1);
      }
      return original(...args);
    };
    try { assert.throws(() => admission.admitLaunch(f.expected)); assert.equal(admission.snapshot().failed, true); }
    finally { fs.readSync = original; }
  }
});
test("swallowed reentry during launch snapshot or streaming poisons the outer admission", t => {
  const f = fixture(t), admission = createLaunchFileAdmission(f.options); let once = false;
  const proxy = new Proxy(f.expected, { ownKeys(target) { if (!once) { once = true; assert.throws(() => admission.admitLaunch(f.expected), /reentrant/); } return Reflect.ownKeys(target); } });
  assert.throws(() => admission.admitLaunch(proxy), /exact reviewed launch/); assert.equal(admission.snapshot().checkedFiles, 0);
  const g = fixture(t), other = createLaunchFileAdmission(g.options), original = fs.readSync; once = false;
  fs.readSync = (...args) => { if (!once) { once = true; assert.throws(() => other.admitLaunch(g.expected), /reentrant/); } return original(...args); };
  try { assert.throws(() => other.admitLaunch(g.expected), /reentry/); assert.equal(other.snapshot().completed, false); }
  finally { fs.readSync = original; }
});
