"use strict";

// ENGINEERING-ONLY initialization envelope, not FIRST/scientific admission.
// Worker limits constrain the JS engine, NOT Buffer/native/RSS/global memory.
// Source buffers have independent exact byte caps. Node/V8/builtins/worker
// bootstrap and termination remain explicit trusted runtime assumptions.
const { Worker, isMainThread, workerData, parentPort, resourceLimits } = require("node:worker_threads");
const { finished } = require("node:stream/promises");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 initialization probe: " + why); };
const fields = (value, keys) => {
  need(value && typeof value === "object" && !Array.isArray(value), "closed data object");
  const actual = Reflect.ownKeys(value); need(actual.length === keys.length && actual.every((k, i) => k === keys[i]), "closed ordered fields");
  return Object.fromEntries(keys.map(k => { const d = Object.getOwnPropertyDescriptor(value, k); need(d && Object.hasOwn(d, "value"), "own data field"); return [k, d.value]; }));
};
const engineKeys = ["maxOldGenerationSizeMb", "maxYoungGenerationSizeMb", "codeRangeSizeMb", "stackSizeMb"];
function checkTrustedBootstrap() {
  const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto");
  const { expectedRuntime, sourcePins } = require("./a68-preflight-bootstrap-policy");
  need(process.version === expectedRuntime.node && process.versions.v8 === expectedRuntime.v8 &&
    process.platform === expectedRuntime.platform && process.arch === expectedRuntime.arch, "reviewed runtime identifiers before loading");
  for (const pin of sourcePins) {
    const file = path.join(__dirname, pin.id), stat = fs.lstatSync(file);
    need(stat.isFile() && !stat.isSymbolicLink() && stat.size === pin.bytes, "reviewed bootstrap file size/type");
    const bytes = fs.readFileSync(file);
    need(bytes.length === pin.bytes && crypto.createHash("sha256").update(bytes).digest("hex") === pin.sha256, "reviewed bootstrap source commitment");
  }
  // These ambient bootstrap sources/runtime are TRUSTED, not same-byte-loaded
  // by the loader they implement. Current-file hashes/identifiers do not prove
  // prior compiler consumption, atomic loading or complete native identity.
}
function captureOptions(options) {
  options = fields(options, ["repositoryRoot", "profile", "snapshotLimits", "workerLimits"]);
  const copy = snapshotCanonicalMetadata(options, options.snapshotLimits).value;
  const limits = fields(copy.workerLimits, [...engineKeys, "timeoutMs", "replyBytes", "stdioBytes"]);
  need(Object.values(limits).every(n => Number.isSafeInteger(n) && n > 0), "positive safe limits");
  need(engineKeys.every(k => limits[k] <= 256) && limits.timeoutMs <= 60000 && limits.replyBytes <= 65536 && limits.stdioBytes <= 65536, "bounded engineering probe limits");
  need(typeof copy.repositoryRoot === "string", "repository root"); return copy;
}
async function runPreflightInitializationProbe(options) {
  need(isMainThread, "parent-thread supervisor required"); const config = captureOptions(options), cap = config.workerLimits;
  checkTrustedBootstrap();
  const limits = Object.fromEntries(engineKeys.map(k => [k, cap[k]]));
  // No inherited Node flags/preloads or environment. This is not OS isolation:
  // inherited runtime state/native libraries remain part of the trust boundary.
  const worker = new Worker(__filename, { workerData: { operation: "a68-metadata-initialization-v1", config },
    execArgv: [], env: {}, resourceLimits: limits, stdout: true, stderr: true });
  return await new Promise((resolve, reject) => {
    let report = null, failure = null, messages = 0, stdout = 0, stderr = 0;
    const fail = error => { if (!failure) { failure = error; void worker.terminate().catch(() => {}); } };
    const timer = setTimeout(() => fail(new Error("A68 initialization probe: parent deadline")), cap.timeoutMs);
    worker.on("error", fail); worker.on("messageerror", fail);
    for (const [stream, name] of [[worker.stdout, "stdout"], [worker.stderr, "stderr"]]) {
      stream.on("error", fail); stream.on("data", bytes => {
        if (name === "stdout") stdout += bytes.length; else stderr += bytes.length;
        if (stdout > cap.stdioBytes || stderr > cap.stdioBytes) fail(new Error("A68 initialization probe: diagnostic quota"));
      });
    }
    const streamsDone = Promise.all([finished(worker.stdout), finished(worker.stderr)]).catch(fail);
    worker.on("message", message => {
      try {
        need(++messages === 1 && typeof message === "string" && Buffer.byteLength(message, "utf8") <= cap.replyBytes, "one bounded result");
        report = snapshotCanonicalMetadata(JSON.parse(message), config.snapshotLimits).value;
      } catch (error) { fail(error); }
    });
    worker.on("exit", async code => {
      try {
        await streamsDone;
        if (failure) throw failure;
        need(code === 0 && messages === 1 && report && stdout === 0 && stderr === 0, "clean worker exit and exact report without undeclared output");
        need(report.schemaVersion === "phase627-metadata-initialization-probe-v1" && report.initializedModules === 23 &&
          report.verifiedModules === 23 && report.requireCalls === 66 && report.ambientModuleAttempts === 0 &&
          report.scope.scientificExecutionAuthorized === false && report.scope.totalProcessMemoryProved === false, "closed engineering success scope");
        resolve(Object.freeze({ report, worker: Object.freeze({ exitCode: code, terminationObserved: true, stdoutBytes: stdout, stderrBytes: stderr }) }));
      } catch (error) {
        error.evidence = Object.freeze({ exitCode: code, terminationObserved: true, messages, stdoutBytes: stdout, stderrBytes: stderr }); reject(error);
      } finally { clearTimeout(timer); }
    });
  });
}

function evaluateReviewedInitialization(config) {
  config = captureOptions(config); const cap = config.workerLimits;
  checkTrustedBootstrap();
  need(!isMainThread && engineKeys.every(k => resourceLimits[k] === cap[k]), "actual worker engine limits match admission");
  const { createPreflightModuleManifest } = require("./a68-preflight-module-manifest");
  const { derivePreflightInitializationCensus } = require("./a68-preflight-initialization-census");
  const { createTrustedModuleLoader, trustedModuleLoadIdentity } = require("./a68-trusted-module-loader");
  const manifest = createPreflightModuleManifest(config.repositoryRoot), census = derivePreflightInitializationCensus();
  const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
  need(same(census.sourceCommitments, manifest.modules.map(({ id, bytes, sha256 }) => ({ id, bytes, sha256 }))), "same reviewed census and source manifest");
  need(census.arrays === 1138 && census.arraySlots === 6365 && census.records === 87 && census.propertySlots === 402 &&
    census.sets === 5 && census.setEntries === 2127 && census.weakMaps === 3 && census.weakMapEntries === 0 &&
    census.calls.diagnosticMenu === 0 && census.calls.coefficientEvaluation === 0, "reviewed initialization-only logical envelope");
  const loader = createTrustedModuleLoader({ manifest, limits: { modules: 23, sourceBytes: 261049, fileBytes: 33732,
    readBytes: 261072, decodedCharacters: 261049, requireCalls: 66, chunkBytes: 65536, snapshot: config.snapshotLimits },
    admitResources(r) {
      need(same(r.manifest, manifest) && r.sourceBytes === 261049 && r.decodedCharacters === 261049 && r.readBytes === 261072 &&
        r.eofScratchBytes === 1 && r.moduleRecords === 23 && r.importEdges === 66 && r.compileCalls === 23 && r.initializationCalls === 23 &&
        r.requireCalls === 66 && engineKeys.every(k => resourceLimits[k] === cap[k]), "exact source/census/worker reservation before capture");
      // Trusted engine envelope covers callable/iterator/compile/GC work only
      // within Node's documented JS-engine scope. This does not close native,
      // external-buffer or whole-process memory, nor later factory execution.
    } });
  const Module = require("node:module"), original = Module._load, forbidden = new Set(manifest.modules.map(m => m.path));
  let ambientModuleAttempts = 0;
  Module._load = function(request, parent, isMain) {
    const resolved = Module._resolveFilename(request, parent, isMain);
    if (forbidden.has(resolved)) { ambientModuleAttempts++; throw new Error("A68 initialization probe: ambient production module forbidden"); }
    return original.apply(this, arguments);
  };
  try {
    loader.capture(); const receipt = loader.load(), identity = trustedModuleLoadIdentity(receipt);
    const names = ["runTemplatePreflightProcess", "runConfiguredTemplatePreflightProcess", "validatedCheckpointConfiguration", "createTopologySourceAdmission", "profileCommitment"];
    need(same(Object.keys(receipt.exports), names) && names.every(k => typeof receipt.exports[k] === "function") && Object.isFrozen(receipt.exports), "actual private aggregate factories");
    need(identity.exports === receipt.exports && identity.initializedModules === 23, "live exact private load identity");
    // Exercise one ACTUAL receipt-exported pure metadata factory. Host/spawn
    // and source-admission factories are intentionally NOT invoked in this probe.
    const profileSha256 = receipt.exports.profileCommitment(config.profile, config.snapshotLimits);
    need(trustedModuleLoadIdentity(receipt).exports === receipt.exports && ambientModuleAttempts === 0, "healthy identity after metadata factory");
    const state = loader.snapshot();
    return { schemaVersion: "phase627-metadata-initialization-probe-v1", nodeVersion: process.version, v8Version: process.versions.v8,
      engineLimits: Object.fromEntries(engineKeys.map(k => [k, resourceLimits[k]])), verifiedModules: state.verifiedModules,
      initializedModules: state.initializedModules, requireCalls: state.requireCalls, readBytes: state.readBytes,
      ambientModuleAttempts, profileSha256, factoryNames: names,
      scope: { sameByteMetadataEntryEvaluated: true, privateProfileFactoryUsed: true, actualHostOrAdmissionInvoked: false,
        engineEnvelopeObserved: true, parentDeadlineIsHardCpuProof: false, runtimeIdentityProved: false,
        totalProcessMemoryProved: false, numericalReplayComplete: false, scientificExecutionAuthorized: false } };
  } finally { Module._load = original; }
}
if (!isMainThread && workerData?.operation === "a68-metadata-initialization-v1") {
  const report = evaluateReviewedInitialization(workerData.config), text = JSON.stringify(report);
  need(Buffer.byteLength(text, "utf8") <= workerData.config.workerLimits.replyBytes, "pre-post reply quota"); parentPort.postMessage(text);
}
module.exports = { runPreflightInitializationProbe };
