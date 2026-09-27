"use strict";
// Release compile-only evidence. The CLOSED phase entry point is never run.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto"), { execFileSync } = require("node:child_process");
const { auditCompilerInputs } = require("../a68-compiler-inputs");
const repository = path.resolve(__dirname, "../../.."), project = path.join(repository, "studies/phase627_full_mixed_metric_native_field_variation_audit_001");
const name = "Phase627FullMixedMetricNativeFieldVariationAudit";
const keys = ["Configuration", "TargetFramework", "MSBuildVersion", "NETCoreSdkVersion", "UseSharedCompilation", "Deterministic", "Optimize", "DebugType", "DefineConstants", "TargetPath"];
const itemNames = ["Compile", "ReferencePath", "Analyzer", "AdditionalFiles", "EditorConfigFiles", "CscCommandLineArgs"];
const cap = { arguments: 1000, inputFiles: 1000, pathCharacters: 1000000,
  snapshot: { nodes: 100000, arraySlots: 100000, stringCharacters: 2000000, maxDepth: 16 } };
const hash = file => crypto.createHash("sha256").update(fs.readFileSync(file)).digest("hex");
let attempted = false, saved, failure;
function capture() {
  if (!attempted) {
    attempted = true;
    try {
      const artifacts = ["dll", "pdb"].map(ext => path.join(project, "bin/Release/net10.0", name + "." + ext));
      const before = artifacts.map(hash);
      const text = execFileSync("dotnet", ["msbuild", path.join(project, name + ".csproj"), "-target:Rebuild", "-p:Configuration=Release",
        "-p:UseSharedCompilation=false", "-p:ProvideCommandLineArgs=true", "-getItem:" + itemNames.join(","), "-getProperty:" + keys.join(",")],
      { cwd: repository, encoding: "utf8", maxBuffer: 8 * 1024 * 1024, timeout: 60000 });
      const start = text.indexOf('{\n  "Properties"'); assert.ok(start >= 0, "complete MSBuild query result");
      const report = JSON.parse(text.slice(start));
      saved = { before, after: artifacts.map(hash), report: { properties: Object.fromEntries(keys.map(k => [k, report.Properties[k]])),
        items: Object.fromEntries(itemNames.map(k => [k, report.Items[k].map(item => item.Identity)])) } };
    } catch (error) { failure = new Error("compile-only input capture failed without retry: " + (error.code ?? error.message)); }
  }
  if (failure) throw failure; return saved;
}
test("actual nonshared Release rebuild preserves DLL/PDB and matches all reported compiler input categories", () => {
  const evidence = capture(), inventory = auditCompilerInputs(project, evidence.report, cap);
  assert.deepEqual(evidence.after, evidence.before, "local byte reproducibility, not complete toolchain/source provenance");
  assert.equal(inventory.arguments.length, 251); assert.equal(inventory.inputs.length, 224);
  assert.deepEqual(Object.fromEntries(Object.entries(inventory.categories).map(([k, values]) => [k, values.length])),
    { Compile: 46, ReferencePath: 167, Analyzer: 8, AdditionalFiles: 0, EditorConfigFiles: 2, EmbeddedFiles: 3, SourceLink: 1 });
  assert.ok(inventory.categories.Compile.includes(path.join(project, "MixedTemplatePreflightCoordinator.cs")), "actual new metadata coordinator is compiled");
  assert.ok(inventory.categories.Compile.includes(path.join(project, "MixedPointEvidenceSeal.cs")), "actual point-final seal is compiled");
  assert.ok(inventory.categories.Compile.includes(path.join(project, "MixedSourcePreparationClient.cs")), "actual source protocol client is compiled");
  assert.ok(inventory.categories.Compile.includes(path.join(project, "MixedSourcePreparationCoordinator.cs")), "actual catalog source coordinator is compiled");
  assert.ok(inventory.categories.Compile.includes(path.join(project, "MixedGeometryWire.cs")), "actual bounded live geometry wire writer is compiled");
  assert.equal(inventory.categories.Compile.filter(file => file.startsWith(path.join(project, "obj") + path.sep)).length, 3);
  assert.ok(inventory.definitions.every(symbol => !symbol.startsWith("A68_GUARDED_")), "production guarded integration remains deliberately pending");
  assert.equal(inventory.scope.sourceToBinaryCorrespondenceProved, false); assert.equal(inventory.scope.scientificExecutionAuthorized, false);
  assert.equal(fs.existsSync(path.join(project, "output")), false);
});
test("actual capture cannot lose a generated source analyzer configuration or reference behind a passing build", () => {
  for (const category of ["Compile", "ReferencePath", "Analyzer", "EditorConfigFiles"]) {
    const report = JSON.parse(JSON.stringify(capture().report)); report.items[category].pop();
    assert.throws(() => auditCompilerInputs(project, report, cap), /census/);
  }
  const report = JSON.parse(JSON.stringify(capture().report)); report.items.CscCommandLineArgs = [];
  assert.throws(() => auditCompilerInputs(project, report, cap), /incremental skipped/);
});

test("reviewed source bytes bind to actual captured Release input declarations before topology admission", () => {
  const { createTopologySourceAdmission } = require("../a68-topology-source-admission");
  const { REVIEWED_SOURCE_SHA256 } = require("../a68-context-topology-envelope");
  const { profileCommitment } = require("../a68-template-preflight-service");
  const { createSourceTemplateValidator } = require("../a68-source-template-validator");
  const fixture = require("./a68-source-template-fixture"), profile = fixture.profile();
  const report = capture().report, inventory = auditCompilerInputs(project, report, cap);
  // Explicit declared replay subset, not discovered executable dependency
  // closure. This test reads/hashes files; it does not launch a process.
  const replayInputs = [...new Set([...Object.keys(REVIEWED_SOURCE_SHA256).filter(f => f.endsWith(".js")),
    "scripts/incremental/a68-topology-source-admission.js", "scripts/incremental/a68-context-topology-envelope.js",
    "scripts/incremental/a68-source-template-validator.js", "scripts/incremental/a68-compiler-inputs.js",
    "scripts/incremental/a68-launch-file-admission.js", "scripts/incremental/a68-canonical-metadata.js",
    "scripts/incremental/a68-template-preflight-service.js"].map(f => path.join(repository, f)))].sort();
  const executable = fs.realpathSync(process.execPath), script = path.join(__dirname, "a68-template-process-child.js");
  const files = [...new Set([...inventory.inputs.map(i => i.path), ...Object.values(inventory.outputs), ...replayInputs, executable, script])].sort();
  const pins = files.map(file => ({ path: file, bytes: fs.statSync(file).size, sha256: hash(file) }));
  const expected = { launch: { executable, arguments: [script, "stall"], workingDirectory: repository, environment: {} },
    profileSha256: profileCommitment(profile, fixture.limits), processLimits: { timeoutMs: 1000 }, transportLimits: { timeoutMs: 900 } };
  let planned = 0;
  const admission = createTopologySourceAdmission({ repositoryRoot: repository, projectDirectory: project, compilerReport: report,
    compilerLimits: cap, replayInputs, launchAdmission: { expected, pins, argumentFiles: [0], limits: {
      files: pins.length, fileBytes: Math.max(...pins.map(p => p.bytes)), totalReadBytes: pins.reduce((sum, p) => sum + p.bytes + 1, 0),
      chunkBytes: 65536, snapshot: fixture.limits } }, snapshotLimits: fixture.limits,
    admitPlanning(descriptor) { assert.equal(descriptor.contexts, 705); planned++; } });
  admission.admitLaunch(expected);
  const validator = createSourceTemplateValidator(profile, fixture.limits, () => {}, admission.admitTopology);
  validator.validateCaptureDeclaration(profile.capture);
  const result = admission.snapshot();
  assert.equal(result.reportedCompilerInputs, 224); assert.equal(result.reviewedCompile, 11); assert.equal(result.reviewedReplay, 12);
  assert.equal(result.files.checkedFiles, pins.length); assert.equal(result.files.readBytes, pins.reduce((sum, p) => sum + p.bytes + 1, 0));
  assert.equal(result.topologyAdmitted, true); assert.equal(planned, 1); assert.equal(validator.snapshot().topologyChecked, true);
  assert.equal(result.scope.compilerConsumptionProved, false); assert.equal(result.scope.loadedReplayModuleIdentityProved, false);
  assert.equal(result.scope.sourceToBinaryCorrespondenceProved, false); assert.equal(result.scope.dependencyClosureComplete, false);
  assert.equal(result.scope.scientificExecutionAuthorized, false); assert.equal(fs.existsSync(path.join(project, "output")), false);
});
