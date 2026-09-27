"use strict";

// Bind reviewed expression-source bytes to DECLARED compiler/replay inputs and
// a declared launch. This is NOT proof of compiler consumption, loaded Node
// module identity, complete dependency closure, immutable build/exec or science.
// No process is launched here. Actual reads use the bounded launch-file reader.
const path = require("node:path"), crypto = require("node:crypto");
const { auditCompilerInputs } = require("./a68-compiler-inputs");
const { createLaunchFileAdmission } = require("./a68-launch-file-admission");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { REVIEWED_SOURCE_SHA256 } = require("./a68-context-topology-envelope");
const need = (ok, why) => { if (!ok) throw new Error("A68 topology source admission: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const absolute = p => typeof p === "string" && path.isAbsolute(p) && path.resolve(p) === p && p !== path.parse(p).root;
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name);
    need(d && Object.hasOwn(d, "value") && d.enumerable, "own data " + name); return [name, d.value]; }));
}
function createTopologySourceAdmission(options) {
  options = fields(options, ["repositoryRoot", "projectDirectory", "compilerReport", "compilerLimits", "replayInputs", "launchAdmission", "snapshotLimits", "admitPlanning"]);
  const admitPlanning = options.admitPlanning;
  need(typeof admitPlanning === "function", "mandatory external planner admission");
  const caps = Object.freeze(fields(options.snapshotLimits, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]));
  need(Object.values(caps).every(n => Number.isSafeInteger(n) && n > 0) && caps.maxDepth <= 16, "positive snapshot quotas/depth16");
  const usage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  let failed = false, busy = false, launchVerified = false, topologyAdmitted = false;
  function capture(value) {
    const remaining = { ...caps };
    for (const key of Object.keys(usage)) { remaining[key] -= usage[key]; need(remaining[key] > 0, "remaining cumulative snapshot quota"); }
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(usage)) usage[key] += copy.usage[key];
    need(!failed, "swallowed snapshot reentry"); return copy.value;
  }
  const config = capture({ repositoryRoot: options.repositoryRoot, projectDirectory: options.projectDirectory,
    compilerReport: options.compilerReport, compilerLimits: options.compilerLimits,
    replayInputs: options.replayInputs, launchAdmission: options.launchAdmission });
  need(absolute(config.repositoryRoot) && config.projectDirectory === path.join(config.repositoryRoot,
    "studies/phase627_full_mixed_metric_native_field_variation_audit_001"), "fixed repository/project identity");
  const inventory = auditCompilerInputs(config.projectDirectory, config.compilerReport, config.compilerLimits);
  // Validate the whole pin declaration before constructing lookup tables. This
  // checks quotas/executable/argument pins but performs NO file reads yet.
  const files = createLaunchFileAdmission(config.launchAdmission);
  const pins = new Map(config.launchAdmission.pins.map(p => [p.path, p]));
  const compile = new Set(inventory.categories.Compile), replay = new Set();
  need(Array.isArray(config.replayInputs), "explicit replay-input manifest");
  let previous = "";
  for (const file of config.replayInputs) {
    need(absolute(file) && file > previous, "ordered unique canonical replay input"); previous = file; replay.add(file);
    need(pins.has(file), "every declared replay input must be pinned");
  }
  // Membership of the reviewed subset must not hide unpinned generated sources,
  // references, analyzers, configs, SourceLink or other reported compiler inputs.
  for (const input of inventory.inputs) need(pins.has(input.path), "every reported compiler input must be pinned");
  for (const output of Object.values(inventory.outputs)) need(pins.has(output), "declared compiler outputs/target must be pinned");
  let reviewedCompile = 0, reviewedReplay = 0;
  for (const [relative, hash] of Object.entries(REVIEWED_SOURCE_SHA256)) {
    const file = path.join(config.repositoryRoot, relative), pin = pins.get(file);
    need(pin && pin.sha256 === hash, "reviewed source byte commitment " + relative);
    if (relative.endsWith(".cs")) { need(compile.has(file), "reviewed C# source must be in Compile: " + relative); reviewedCompile++; }
    else { need(replay.has(file), "reviewed JS source must be in replay inputs: " + relative); reviewedReplay++; }
  }
  need(reviewedCompile === 11 && reviewedReplay === 12, "complete reviewed source census");
  function guard(action) {
    if (failed || busy) { failed = true; need(false, "failed or reentrant admission"); }
    busy = true;
    try { const result = action(); need(!failed, "swallowed admission reentry"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  return Object.freeze({
    admitLaunch: actual => guard(() => {
      need(!launchVerified && !topologyAdmitted, "one launch admission");
      const launch = capture(actual);
      files.admitLaunch(launch);
      need(!failed && files.snapshot().completed, "complete healthy file admission"); launchVerified = true;
    }),
    admitTopology: descriptor => guard(() => {
      need(launchVerified && !topologyAdmitted, "verified launch before one topology admission");
      const actual = fields(capture(descriptor), ["operation", "contexts", "profile", "sourceRevisions"]);
      need(actual.operation === "prospective-tensor-topology-v1" && actual.contexts === 705 &&
        same(actual.sourceRevisions, REVIEWED_SOURCE_SHA256), "exact reviewed topology descriptor");
      // Same canonical ASCII profile commitment as the transport. Snapshot
      // quotas are cumulative; hash/serialization/runtime memory are not claimed
      // to be fully proved by those JSON-content counters.
      const digest = crypto.createHash("sha256").update(JSON.stringify(actual.profile) + "\n", "ascii").digest("hex");
      need(digest === config.launchAdmission.expected.profileSha256, "actual topology profile matches admitted launch");
      const admitted = admitPlanning(Object.freeze(actual));
      need(!failed, "planner callback reentry");
      need(admitted === undefined, "synchronous void planner admission required"); topologyAdmitted = true;
    }),
    snapshot: () => Object.freeze({ failed, launchVerified: launchVerified && !failed,
      topologyAdmitted: topologyAdmitted && !failed, reviewedCompile, reviewedReplay,
      reportedCompilerInputs: inventory.inputs.length, declaredReplayInputs: replay.size,
      metadataUsage: Object.freeze({ ...usage }), files: files.snapshot(),
      scope: Object.freeze({ reviewedInputMembershipChecked: true,
        declaredInputBytesVerifiedAtLaunch: launchVerified && !failed,
        compilerConsumptionProved: false, loadedReplayModuleIdentityProved: false,
        sourceToBinaryCorrespondenceProved: false, dependencyClosureComplete: false,
        immutableBuildLoadBoundaryProved: false, totalResourceProof: false, scientificExecutionAuthorized: false }) })
  });
}
module.exports = { createTopologySourceAdmission };
