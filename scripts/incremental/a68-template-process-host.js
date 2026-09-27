"use strict";

// POSIX process ownership for the METADATA-ONLY template protocol. No default
// command, environment, profile, resource admission or scientific entry point.
// The reviewed caller must independently admit/pin the entire executable/source
// closure. This host does not establish that closure or authorize FIRST.
const path = require("node:path");
const { spawn } = require("node:child_process");
const { performance } = require("node:perf_hooks");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { createSourceTemplateValidator } = require("./a68-source-template-validator");
const { profileCommitment, serveTemplatePreflight, templatePreflightConfiguration } = require("./a68-template-preflight-service");
const { createCheckpointCatalog } = require("./a68-checkpoint-catalog");
const configurations = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 template process: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
function freeze(value) { if (value && typeof value === "object" && !Object.isFrozen(value)) { Object.values(value).forEach(freeze); Object.freeze(value); } return value; }
const positive = value => Number.isSafeInteger(value) && value > 0;
function validatedCheckpointConfiguration(result) {
  const identity = configurations.get(result); need(identity, "private clean configured process result required");
  need(identity.health()?.failed === false, "healthy configured process result"); return identity;
}
async function runTemplatePreflightProcess(options) { return runPreflightProcess(options, null, null); }
async function runConfiguredTemplatePreflightProcess(options, checkpointPolicy, checkpointLimits) {
  need(checkpointPolicy && checkpointLimits, "explicit checkpoint policy and catalog quotas");
  return runPreflightProcess(options, checkpointPolicy, checkpointLimits);
}
async function runPreflightProcess(options, checkpointPolicy, checkpointLimits) {
  const started = performance.now();
  options = fields(options, ["launch", "profile", "snapshotLimits", "transportLimits", "processLimits", "admitLaunch", "admitConstruction", "admitTopology"]);
  need(process.platform !== "win32", "POSIX dedicated process-group host required");
  const { admitLaunch, admitConstruction, admitTopology } = options;
  need(typeof admitLaunch === "function" && typeof admitConstruction === "function", "mandatory independent launch and construction admission");
  need(typeof admitTopology === "function", "mandatory independent topology planning admission");
  const captured = snapshotCanonicalMetadata({ launch: options.launch, profile: options.profile, snapshotLimits: options.snapshotLimits,
    transportLimits: options.transportLimits, processLimits: options.processLimits,
    checkpointPolicy, checkpointLimits }, options.snapshotLimits).value;
  checkpointPolicy = captured.checkpointPolicy; checkpointLimits = captured.checkpointLimits;
  const launch = fields(captured.launch, ["executable", "arguments", "workingDirectory", "environment"]);
  need(typeof launch.executable === "string" && path.isAbsolute(launch.executable) && typeof launch.workingDirectory === "string" && path.isAbsolute(launch.workingDirectory), "explicit absolute executable and working directory");
  need(Array.isArray(launch.arguments) && launch.arguments.every(arg => typeof arg === "string"), "explicit argument array without a shell");
  need(launch.environment && typeof launch.environment === "object" && !Array.isArray(launch.environment) &&
    Object.entries(launch.environment).every(([key, value]) => /^[A-Za-z_][A-Za-z0-9_]*$/.test(key) && typeof value === "string"), "explicit own environment; never inherit credentials");
  const cap = fields(captured.processLimits, ["timeoutMs", "killGraceMs", "stdoutBytes", "stderrBytes"]);
  need(Object.values(cap).every(positive) && cap.timeoutMs <= 2147483647 && cap.killGraceMs <= 2147483647, "positive bounded process quotas and timers");
  const transport = fields(captured.transportLimits, ["frameBytes", "totalInputBytes", "replyBytes", "totalOutputBytes", "timeoutMs"]);
  need(Object.values(transport).every(positive) && transport.timeoutMs <= cap.timeoutMs, "transport deadline within whole-process deadline");
  // Reject malformed independent profiles before launching any child. This
  // validates profile declarations only, not all705 peer templates or science.
  if (checkpointPolicy === null) createSourceTemplateValidator(captured.profile, captured.snapshotLimits, admitConstruction, admitTopology);
  else createCheckpointCatalog(captured.profile, checkpointPolicy, checkpointLimits, admitConstruction, admitTopology);
  const profileSha256 = profileCommitment(captured.profile, captured.snapshotLimits);
  need(admitLaunch(freeze({ launch, profileSha256, processLimits: cap, transportLimits: transport })) === undefined, "synchronous void launch admission required");
  need(performance.now() - started < cap.timeoutMs, "deadline before process launch");

  let child = null, failure = null, spawnError = null, code = null, signal = null, closed = false, protocolCompleted = false;
  let stdoutBytes = 0, stderrBytes = 0;
  const stdout = [], stderr = [];
  let failSignal;
  const failed = new Promise(resolve => { failSignal = resolve; });
  const fail = error => { if (!failure) { failure = error; failSignal(); } };
  const alive = () => {
    if (!child?.pid) return false;
    try { process.kill(-child.pid, 0); return true; }
    catch (error) { if (error.code === "ESRCH") return false; throw error; }
  };
  const kill = sig => {
    if (!child?.pid) return;
    try { process.kill(-child.pid, sig); }
    catch (error) { if (error.code !== "ESRCH") fail(error); }
  };
  const destroyStreams = () => { if (child) for (const stream of child.stdio) stream?.destroy(); };
  const timer = setTimeout(() => fail(new Error("A68 template process: whole-process deadline")), Math.max(1, cap.timeoutMs - (performance.now() - started)));
  let closePromise, servicePromise;
  async function boundedClose() {
    if (closed || !closePromise) return;
    let timeout;
    try { await Promise.race([closePromise, new Promise(resolve => { timeout = setTimeout(resolve, cap.killGraceMs); })]); }
    finally { clearTimeout(timeout); }
  }
  try {
    child = spawn(launch.executable, [...launch.arguments], { cwd: launch.workingDirectory, env: { ...launch.environment },
      shell: false, detached: true, stdio: ["ignore", "pipe", "pipe", "pipe", "pipe"] });
    closePromise = new Promise(resolve => {
      child.once("close", (exitCode, exitSignal) => { code = exitCode; signal = exitSignal; closed = true; resolve(); });
    });
    child.once("error", error => { spawnError = error.code ?? "unknown"; fail(new Error("A68 template process: spawn failure " + spawnError)); });
    for (const stream of child.stdio) stream?.on("error", fail);
    function collect(chunks, bytes, ceiling, label) {
      need(Buffer.isBuffer(bytes), "binary diagnostic stream");
      const used = label === "stdout" ? stdoutBytes : stderrBytes;
      if (bytes.length > ceiling - used) { fail(new Error("A68 template process: " + label + " quota")); return; }
      if (label === "stdout") stdoutBytes += bytes.length; else stderrBytes += bytes.length;
      chunks.push(Buffer.from(bytes));
    }
    child.stdout.on("data", bytes => collect(stdout, bytes, cap.stdoutBytes, "stdout"));
    child.stderr.on("data", bytes => collect(stderr, bytes, cap.stderrBytes, "stderr"));
    servicePromise = serveTemplatePreflight({ input: child.stdio[3], output: child.stdio[4], profile: captured.profile,
      snapshotLimits: captured.snapshotLimits, limits: transport, admitConstruction, admitTopology, checkpointPolicy, checkpointLimits }).then(result => { protocolCompleted = true; return { result }; }, error => { fail(error); return { error }; });
    const outcome = await Promise.race([servicePromise, failed.then(() => null)]);
    if (failure) throw failure;
    need(outcome?.result, "complete metadata protocol result");
    await Promise.race([closePromise, failed]);
    if (failure) throw failure;
    need(closed && code === 0 && signal === null && spawnError === null, "clean zero child exit after complete protocol");
    need(stdoutBytes === 0 && stderrBytes === 0, "no undeclared stdout or stderr on successful preflight");
    need(!alive(), "no surviving owned process group after child exit");
    const result = freeze({ profileSha256, preflight: outcome.result,
      process: { exitCode: code, signal, stdoutBytes, stderrBytes, ownedProcessGroupGone: true },
      scope: { structureOnly: true, completeProtocolAndProcessExitChecked: true, executableClosurePinnedHere: false,
        sourceAuthenticityEstablished: false, numericalReplayComplete: false, resourceSufficiencyProved: false,
        totalProcessMemoryProved: false, hardCpuDeadlineProved: false, allDescendantsContained: false, scientificExecutionAuthorized: false } });
    if (checkpointPolicy !== null) configurations.set(result, templatePreflightConfiguration(outcome.result));
    return result;
  } catch (error) {
    fail(error); destroyStreams(); kill("SIGTERM"); await boundedClose();
    // The group was created by THIS spawn, never selected from caller input.
    // Escalate even if the group leader exited but descendants kept pipes open.
    if (alive()) kill("SIGKILL"); await boundedClose();
    const evidence = freeze({ childPid: child?.pid ?? null, spawnError, closed, exitCode: code, signal,
      stdoutBytes, stderrBytes, stdoutBase64: Buffer.concat(stdout).toString("base64"), stderrBase64: Buffer.concat(stderr).toString("base64"),
      cleanupConfirmed: closed && !alive(), protocolCompleted, scientificExecutionAuthorized: false });
    const rejected = new Error(failure.message, { cause: failure }); rejected.evidence = evidence; throw rejected;
  } finally {
    clearTimeout(timer); destroyStreams();
    // The service has a rejection handler before any race. Do not await an
    // uncooperative peer forever or mistake unresolved cleanup for success.
  }
}
module.exports = { runTemplatePreflightProcess, runConfiguredTemplatePreflightProcess, validatedCheckpointConfiguration };
