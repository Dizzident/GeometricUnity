"use strict";
// Uncalled metadata-only duplex service. The reviewed host owns these streams,
// independently supplies the frozen profile/limits and MUST supply synchronous
// construction admission. There is no CLI default, no scientific lane and no
// authorization derived from a peer's profile or claimed hashes.
const crypto = require("node:crypto");
const { finished } = require("node:stream/promises");
const { parseCanonicalWire } = require("./a68-canonical-wire");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { sourceContextIds } = require("./a68-source-context-menu");
const { createSourceTemplateValidator } = require("./a68-source-template-validator");
const { createCheckpointCatalog, checkpointCatalogIdentity } = require("./a68-checkpoint-catalog");
const configurations = new WeakMap();
const REQUEST = "phase627-template-preflight-request-v2", REPLY = "phase627-template-preflight-reply-v2";
const need = (ok, why) => { if (!ok) throw new Error("A68 template transport: " + why); };
const digest = buffer => crypto.createHash("sha256").update(buffer).digest("hex");
function profileCommitment(profile, snapshotLimits) {
  const copy = snapshotCanonicalMetadata(profile, snapshotLimits).value;
  return digest(Buffer.from(JSON.stringify(copy) + "\n", "ascii"));
}
function exact(value, keys) {
  need(value && typeof value === "object" && !Array.isArray(value), "closed request object");
  const actual = Object.keys(value); need(actual.length === keys.length && keys.every(key => actual.includes(key)), "closed request fields");
}
function templatePreflightConfiguration(result) {
  const token = configurations.get(result); need(token, "private configured preflight result required");
  return checkpointCatalogIdentity(token);
}
async function serveTemplatePreflight({ input, output, profile, snapshotLimits, limits, admitConstruction, admitTopology, checkpointPolicy = null, checkpointLimits = null }) {
  // Both configuration snapshots precede external admission callbacks. Limits
  // are supplied by the trusted host, never overwritten by a request frame.
  const frozen = snapshotCanonicalMetadata({ profile, limits, snapshotLimits }, snapshotLimits).value;
  limits = frozen.limits; snapshotLimits = frozen.snapshotLimits; profile = frozen.profile;
  exact(limits, ["frameBytes", "totalInputBytes", "replyBytes", "totalOutputBytes", "timeoutMs"]);
  need(Object.values(limits).every(n => Number.isSafeInteger(n) && n > 0), "explicit positive transport quotas");
  need(limits.timeoutMs <= 2147483647, "nonoverflowing host timer");
  const expectedProfile = profileCommitment(profile, snapshotLimits), ids = sourceContextIds();
  need((checkpointPolicy === null) === (checkpointLimits === null), "policy and catalog limits must be supplied together");
  // The catalog is the ACTUAL validator wrapper, not a parallel second pass.
  const catalog = checkpointPolicy === null ? null : createCheckpointCatalog(profile, checkpointPolicy, checkpointLimits, admitConstruction, admitTopology);
  const validator = catalog ?? createSourceTemplateValidator(profile, snapshotLimits, admitConstruction, admitTopology);
  let pending = Buffer.alloc(0), received = 0, emitted = 0, sequence = 0, completion = null, streamFailure = null;
  const onError = error => { streamFailure ??= error; };
  input.on("error", onError); output.on("error", onError);
  const outputDone = finished(output).catch(onError);
  const timeout = setTimeout(() => {
    streamFailure ??= new Error("A68 template transport: whole-session deadline");
    input.destroy(streamFailure); output.destroy(streamFailure);
  }, limits.timeoutMs);
  async function reply(request, hash, report) {
    need(!streamFailure, "healthy reply stream");
    const text = JSON.stringify({ schema: REPLY, sequence: request.sequence, operation: request.operation, requestSha256: hash, status: "accepted", report }) + "\n";
    const bytes = Buffer.byteLength(text, "utf8");
    need(bytes <= limits.replyBytes && bytes <= limits.totalOutputBytes - emitted, "prewrite reply quotas"); emitted += bytes;
    await new Promise((resolve, reject) => output.write(text, "utf8", error => error ? reject(error) : resolve()));
    need(!streamFailure, "reply stream failure");
  }
  try {
    for await (const chunk of input) {
      need(!streamFailure && Buffer.isBuffer(chunk), "healthy binary request stream");
      need(chunk.length <= limits.totalInputBytes - received, "cumulative input quota"); received += chunk.length;
      // The host's already allocated chunk is not charged as proven RSS.
      // Split before concatenation so a coalesced group of valid frames is not
      // mistaken for a single oversized frame. Every partial frame is bounded.
      let offset = 0;
      while (offset < chunk.length) {
        const lf = chunk.indexOf(10, offset), end = lf < 0 ? chunk.length : lf + 1, slice = chunk.subarray(offset, end);
        need(slice.length <= limits.frameBytes - pending.length, "prospective frame quota"); pending = Buffer.concat([pending, slice]); offset = end;
        if (lf < 0) continue;
        need(completion === null, "no request after finish");
        const frame = pending; pending = Buffer.alloc(0); const request = parseCanonicalWire(frame, { graphBytes: limits.frameBytes });
        exact(request, ["schema", "sequence", "operation", "profileSha256", "template"]);
        need(request.schema === REQUEST && request.sequence === sequence, "exact schema and monotone sequence");
        const hash = digest(frame);
        if (sequence === 0) {
          need(request.operation === "begin" && request.profileSha256 === expectedProfile, "independently configured profile handshake");
          validator.validateCaptureDeclaration(request.template);
          await reply(request, hash, null);
        } else if (sequence <= ids.length) {
          need(request.operation === "validate" && request.profileSha256 === null && request.template?.id === ids[sequence - 1], "complete ordered705 template requests");
          const report = validator.validateTemplate(request.template); await reply(request, hash, report);
        } else {
          need(sequence === ids.length + 1 && request.operation === "finish" && request.profileSha256 === null && request.template === null, "one terminal full-set request");
          completion = { request, hash, report: validator.finish() };
        }
        sequence++;
      }
    }
    need(!streamFailure && pending.length === 0 && completion !== null && sequence === 707, "complete canonical requests and clean EOF before final acknowledgement");
    await reply(completion.request, completion.hash, completion.report);
    output.end(); await outputDone; need(!streamFailure, "clean terminal reply delivery");
    const result = Object.freeze({ requests: sequence, inputBytes: received, outputBytes: emitted, report: completion.report });
    // Not exposed before clean EOF, final ACK delivery and drained output.
    if (catalog) configurations.set(result, catalog.configurationToken());
    return result;
  } catch (error) {
    // No fabricated success reply or repair/retry lane. The owning host must
    // preserve/report the exception and nonzero process result to its caller.
    const failure = streamFailure ?? error;
    input.destroy(); output.destroy(); await outputDone; throw failure;
  } finally { clearTimeout(timeout); input.off("error", onError); output.off("error", onError); }
}
module.exports = { REQUEST, REPLY, profileCommitment, serveTemplatePreflight, templatePreflightConfiguration };
