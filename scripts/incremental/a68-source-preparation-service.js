"use strict";
// Prospective second-producer transport, NOT a launcher or scientific grant.
// Own the genuine preparation driver; no caller-supplied adapters/driver seam.
// Synchronous source work needs separately reviewed external CPU/memory control:
// this event-loop deadline cannot interrupt an executing source calculation.
const crypto = require("node:crypto");
const { finished } = require("node:stream/promises");
const { parseCanonicalWire } = require("./a68-canonical-wire");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { validatedCheckpointConfiguration } = require("./a68-template-process-host");
const { sourceContextIds } = require("./a68-source-context-menu");
const { checkpointEvent } = require("./a68-source-checkpoint-dispatcher");
const { createSourcePreparationDriver } = require("./a68-source-preparation-driver");
const REQUEST = "phase627-source-preparation-request-v1", REPLY = "phase627-source-preparation-reply-v1";
const need = (ok, why) => { if (!ok) throw new Error("A68 source preparation transport: " + why); };
const digest = buffer => crypto.createHash("sha256").update(buffer).digest("hex");
function fields(value, keys) {
  need(value && typeof value === "object" && !Array.isArray(value) && JSON.stringify(Reflect.ownKeys(value)) === JSON.stringify(keys), "closed ordered fields");
  return Object.fromEntries(keys.map(key => { const d = Object.getOwnPropertyDescriptor(value, key);
    need(d && Object.hasOwn(d, "value"), "own data field " + key); return [key, d.value]; }));
}
async function serveSourcePreparation(options) {
  // Own cleanup BEFORE authentication, snapshots or driver construction. Keep
  // an error listener through asynchronous destroy/close on rejected setup;
  // cleanup errors must never replace the first setup/session failure.
  const streams = [], onError = error => { if (setupInProgress) firstError ??= error; };
  let firstError = null, succeeded = false, setupInProgress = true;
  try {
    for (const stream of new Set([options.input, options.output])) {
      if (stream && typeof stream.on === "function" && typeof stream.destroy === "function") {
        stream.on("error", onError); streams.push(stream);
      }
    }
    const result = await runSourcePreparation(options, () => { if (firstError) throw firstError; }, () => { setupInProgress = false; });
    succeeded = true; return result;
  } catch (error) {
    firstError ??= error;
    for (const stream of streams) {
      // Destroy can emit its error asynchronously. Release the temporary
      // listener on the close EVENT, not the earlier .closed state transition.
      // emitClose:false streams intentionally retain this harmless listener.
      const detach = () => stream.off("error", onError);
      try { stream.once("close", detach); stream.destroy(); }
      catch { /* Preserve firstError even if a supplied stream's cleanup throws. */ }
    }
    throw firstError;
  } finally {
    if (succeeded) for (const stream of streams) stream.off("error", onError);
  }
}
async function runSourcePreparation({ input, output, preflightResult, preparationPolicy, preparationLimits, snapshotLimits, limits, admitStage }, checkSetup, setupComplete) {
  checkSetup();
  const cap = fields(snapshotLimits, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(cap).every(n => Number.isSafeInteger(n) && n > 0) && cap.maxDepth <= 16, "finite cumulative service snapshot quotas and depth16");
  Object.freeze(cap); const usage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  function capture(value) {
    const remaining = Object.fromEntries(Object.keys(usage).map(k => [k, cap[k] - usage[k]])); remaining.maxDepth = cap.maxDepth;
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(usage)) usage[key] += copy.usage[key]; return copy.value;
  }
  const admitted = validatedCheckpointConfiguration(preflightResult);
  const frozen = capture({ preparationPolicy, preparationLimits, snapshotLimits, limits, producerCommitments: admitted.producerCommitments });
  preparationPolicy = frozen.preparationPolicy; preparationLimits = frozen.preparationLimits; limits = frozen.limits;
  fields(limits, ["frameBytes", "totalInputBytes", "replyBytes", "totalOutputBytes", "timeoutMs"]);
  need(Object.values(limits).every(n => Number.isSafeInteger(n) && n > 0) && limits.timeoutMs <= 2147483647, "positive finite transport quotas and timer");
  const producer = fields(frozen.producerCommitments, ["profileSha256", "captureSha256", "templates"]), ids = sourceContextIds();
  need(Array.isArray(producer.templates) && producer.templates.length === 705 && ids.length === 705, "complete authenticated producer commitments");
  for (let i = 0; i < ids.length; i++) {
    const row = fields(producer.templates[i], ["contextId", "templateSha256"]);
    need(row.contextId === ids[i] && /^[a-f0-9]{64}$/.test(row.templateSha256), "ordered authenticated producer template hashes");
  }
  // Construction is metadata-only. Actual source construction occurs only on
  // a valid prepare, after the whole second producer's705-template prepass.
  checkSetup();
  const driver = createSourcePreparationDriver(preflightResult, preparationPolicy, preparationLimits, admitStage);
  try { checkSetup(); } catch (error) { driver.abort(); throw error; }
  let pending = Buffer.alloc(0), received = 0, emitted = 0, sequence = 0, validated = 0, prepared = 0, eventIndex = 0;
  let begun = false, frozenCatalog = false, awaitingCheckpoint = false, completion = null, failure = null, menusMatched = 0, acceptedMenu = null;
  let signalFailure; const failureSignal = new Promise(resolve => { signalFailure = resolve; });
  function fail(error) { failure ??= error; driver.abort(); signalFailure(); }
  const onError = error => { fail(error); input.destroy(); output.destroy(); };
  input.on("error", onError); output.on("error", onError);
  const outputDone = finished(output).catch(onError);
  const timeout = setTimeout(() => {
    fail(new Error("A68 source preparation transport: whole-session deadline")); input.destroy(); output.destroy();
  }, limits.timeoutMs);
  function healthy() {
    need(!failure && admitted.health()?.failed === false, "healthy authenticated configuration and transport");
    const status = driver.snapshot();
    need(!status.failed && status.acceptedEvents === eventIndex && status.producerTemplatesMatched === prepared &&
      status.producerProfileMatched === begun && status.menusMatched === menusMatched, "healthy synchronized private preparation driver");
    return status;
  }
  function completed() {
    const status = healthy();
    need(begun && frozenCatalog && validated === 705 && prepared === 705 && menusMatched === 3 && acceptedMenu === null && eventIndex === 707 && !awaitingCheckpoint && status.completed &&
      status.pendingContextId === null && status.retainedActivePoints === 0 && status.retainedInputReaders === 0, "complete healthy source traversal before finish");
  }
  async function reply(request, requestSha256, report) {
    healthy();
    const text = JSON.stringify({ schema: REPLY, sequence: request.sequence, operation: request.operation, requestSha256, status: "accepted", report }) + "\n";
    const bytes = Buffer.byteLength(text, "utf8");
    need(bytes <= limits.replyBytes && bytes <= limits.totalOutputBytes - emitted, "prewrite reply quotas"); emitted += bytes;
    await Promise.race([new Promise((resolve, reject) => output.write(text, "utf8", error => error ? reject(error) : resolve())),
      failureSignal.then(() => { throw failure; })]);
    healthy();
  }
  try {
    checkSetup(); setupComplete();
    healthy();
    for await (const chunk of input) {
      healthy(); need(Buffer.isBuffer(chunk), "binary request stream");
      need(chunk.length <= limits.totalInputBytes - received, "cumulative input quota"); received += chunk.length;
      let offset = 0;
      while (offset < chunk.length) {
        const lf = chunk.indexOf(10, offset), end = lf < 0 ? chunk.length : lf + 1, slice = chunk.subarray(offset, end);
        need(slice.length <= limits.frameBytes - pending.length, "prospective frame quota"); pending = Buffer.concat([pending, slice]); offset = end;
        if (lf < 0) continue;
        need(completion === null, "no request after finish");
        const frame = pending; pending = Buffer.alloc(0); const request = parseCanonicalWire(frame, { graphBytes: limits.frameBytes });
        fields(request, ["schema", "sequence", "operation", "payload"]);
        need(request.schema === REQUEST && request.sequence === sequence, "exact schema and monotone sequence");
        const requestSha256 = digest(frame); let report = null;
        if (!begun) {
          need(sequence === 0 && request.operation === "begin", "one producer begin handshake");
          const payload = fields(request.payload, ["profile", "capture"]);
          driver.beginProducer(payload.profile, payload.capture); begun = true;
        } else if (!frozenCatalog) {
          if (validated < 705) {
            need(request.operation === "validate", "complete ordered705 second-producer prepass before freeze");
            const actual = capture(request.payload), commitment = producer.templates[validated];
            need(actual?.id === ids[validated] && digest(Buffer.from(JSON.stringify(actual) + "\n", "ascii")) === commitment.templateSha256,
              "exact full second-producer template commitment");
            validated++; report = { contextId: actual.id };
          } else {
            need(sequence === 706 && request.operation === "freeze" && request.payload === null, "one complete705 freeze acknowledgement");
            frozenCatalog = true; report = { contexts: 705, scientificExecutionAuthorized: false };
          }
        } else if (eventIndex < 707) {
          const event = checkpointEvent(eventIndex);
          if (event.kind === "final") {
            need(!awaitingCheckpoint && request.operation === "pointfinal", "ordered point completion after all child checkpoints");
            report = driver.completePoint({ contextId: event.contextId, graphPath: event.contextId + "/graph.json",
              backgroundPath: event.contextId + "/background-checkpoint.json", metadataPath: event.contextId + "/metadata.json" }, request.payload);
            eventIndex++;
          } else if (!awaitingCheckpoint && event.kind === "diagnostic" && acceptedMenu === null) {
            need(request.operation === "diagnostic-menu", "diagnostic menu required before preparation");
            const payload = fields(request.payload, ["diagnostic", "menu"]);
            need(typeof payload.diagnostic === "string" && event.contextId === "diagnostic/" + payload.diagnostic, "ordered diagnostic menu context");
            report = driver.validateDiagnosticMenu(payload.diagnostic, payload.menu);
            acceptedMenu = event.contextId; menusMatched++;
          } else if (!awaitingCheckpoint) {
            need(request.operation === "prepare" && request.payload?.id === event.contextId, "ordered full-template preparation");
            const template = request.payload;
            report = driver.prepareNext({ contextId: template.id, graphPath: template.graphPath,
              metadataPath: event.kind === "background" ? template.pointCheckpoint?.relativePath : template.metadataPath, marks: template.marks }, template);
            prepared++; awaitingCheckpoint = true; acceptedMenu = null;
          } else {
            need(request.operation === "checkpoint", "checkpoint required after prepared context");
            report = driver.acceptCheckpoint(request.payload); awaitingCheckpoint = false; eventIndex++;
          }
        } else {
          need(sequence === 2122 && request.operation === "finish" && request.payload === null, "one terminal source traversal request"); completed();
          completion = { request, requestSha256, report: { contexts: 705, checkpoints: 707, scientificExecutionAuthorized: false } };
        }
        healthy(); if (completion === null) await reply(request, requestSha256, report); sequence++;
      }
    }
    need(!failure && pending.length === 0 && completion !== null && sequence === 2123, "complete requests and clean EOF before final acknowledgement");
    completed(); await reply(completion.request, completion.requestSha256, completion.report);
    output.end(); await Promise.race([outputDone, failureSignal]); completed();
    return Object.freeze({ requests: sequence, inputBytes: received, outputBytes: emitted, snapshotUsage: Object.freeze({ ...usage }),
      report: Object.freeze(completion.report), scope: Object.freeze({ fullPrerequisitesImplemented: false, producerProcessIdentityEstablished: false,
        upstreamCertificateProofEstablished: false, totalProcessMemoryProved: false, hardCpuDeadlineProved: false, scientificExecutionAuthorized: false }) });
  } catch (error) {
    fail(error); input.destroy(); output.destroy(); await outputDone; throw failure;
  } finally { clearTimeout(timeout); input.off("error", onError); output.off("error", onError); }
}
module.exports = { REQUEST, REPLY, serveSourcePreparation };
