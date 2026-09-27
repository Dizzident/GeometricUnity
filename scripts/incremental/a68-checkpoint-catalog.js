"use strict";
// Metadata provenance only: commitments come from the EXACT candidate accepted
// by the owned source-structure validator. No numerical/source authentication,
// resource sufficiency, transport acceptance or scientific permission follows.
const path = require("node:path"), crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { sourceContextIds } = require("./a68-source-context-menu");
const { createSourceTemplateValidator } = require("./a68-source-template-validator");
const identities = new WeakMap();
const need = (ok, why) => { if (!ok) throw new Error("A68 checkpoint catalog: " + why); };
function fields(value, keys) {
  need(value && typeof value === "object" && !Array.isArray(value), "closed data object");
  const actual = Reflect.ownKeys(value);
  need(actual.length === keys.length && actual.every((k, i) => k === keys[i]), "closed ordered fields");
  return Object.fromEntries(keys.map(k => { const d = Object.getOwnPropertyDescriptor(value, k);
    need(d && Object.hasOwn(d, "value"), "own data field"); return [k, d.value]; }));
}
function snapshotLimits(value) {
  value = fields(value, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(value).every(n => Number.isSafeInteger(n) && n > 0) && value.maxDepth <= 16, "positive finite snapshot quotas and depth16");
  return Object.freeze(value);
}
// A retained token health function captures ONLY these flags. It cannot keep
// the validator/profile, candidate templates, policy or events alive.
function lifecycle() {
  let failed = false, busy = false, finished = false;
  return Object.freeze({
    run(action, terminal = false, finishedOnly = false) {
      if (failed || busy || (finishedOnly ? !finished : finished)) { failed = true; need(false, "failed reentrant or invalid terminal operation"); }
      busy = true;
      try { const result = action(); need(!failed, "swallowed callback failure"); if (terminal) finished = true; return result; }
      catch (error) { failed = true; throw error; } finally { busy = false; }
    },
    health: () => Object.freeze({ failed, finished })
  });
}
function checkpointCatalogIdentity(token) {
  const value = identities.get(token); need(value, "private completed catalog token required");
  const health = value.health(); need(health.finished && !health.failed, "healthy completed catalog required"); return value;
}
const digest = value => crypto.createHash("sha256").update(JSON.stringify(value) + "\n").digest("hex");
function createCheckpointCatalog(profile, policy, limits, admitConstruction, admitTopology) {
  limits = fields(limits, ["validator", "snapshot"]);
  const validatorLimits = snapshotLimits(limits.validator), cap = snapshotLimits(limits.snapshot);
  const used = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  function capture(value) {
    const remaining = {};
    for (const k of Object.keys(used)) { remaining[k] = cap[k] - used[k]; need(remaining[k] > 0, "remaining cumulative snapshot quota " + k); }
    remaining.maxDepth = cap.maxDepth;
    const copied = snapshotCanonicalMetadata(value, remaining);
    for (const k of Object.keys(used)) used[k] += copied.usage[k];
    return copied.value;
  }
  // Freeze policy AND the independent full profile before any external
  // admission callback. Hash the same profile passed to the owned validator,
  // which also charges its own independent cumulative snapshot copy.
  policy = fields(capture(policy), ["outputRoot", "profiles", "routes"]);
  profile = capture(profile);
  const profileSha256 = capture(digest(profile));
  need(typeof policy.outputRoot === "string" && path.isAbsolute(policy.outputRoot) && path.resolve(policy.outputRoot) === policy.outputRoot &&
    policy.outputRoot !== path.parse(policy.outputRoot).root, "canonical nonroot output directory");
  const ids = sourceContextIds();
  need(Array.isArray(policy.profiles) && policy.profiles.length > 0 && policy.profiles.length <= 707, "bounded nonempty quota profiles");
  for (const p of policy.profiles) {
    fields(p, ["read", "replay"]);
    need(p.read && typeof p.read === "object" && !Array.isArray(p.read), "explicit read quotas");
    need(p.replay === null || typeof p.replay === "object" && !Array.isArray(p.replay), "explicit replay quotas or terminal null");
  }
  need(Array.isArray(policy.routes) && policy.routes.length === ids.length, "all705 ordered routes");
  const profileIndex = i => Number.isSafeInteger(i) && i >= 0 && i < policy.profiles.length;
  for (let i = 0; i < ids.length; i++) {
    const route = fields(policy.routes[i], ["contextId", "profile", "completionProfile"]), point = /^point[01]$/.test(ids[i]);
    need(route.contextId === ids[i] && profileIndex(route.profile) && policy.profiles[route.profile].replay !== null, "exact ordered context and replay profile");
    need(point ? profileIndex(route.completionProfile) && policy.profiles[route.completionProfile].replay === null : route.completionProfile === null,
      "point-only final profile with null replay");
  }
  const validator = createSourceTemplateValidator(profile, validatorLimits, admitConstruction, admitTopology), state = lifecycle();
  profile = null; // Only the owned validator retains its independent profile.
  const events = [], templates = []; let pendingFinal = null, accepted = 0, captureAccepted = false, captureSha256 = null, token = null;
  function healthy() { need(!state.health().failed && !validator.snapshot().failed, "healthy owned validator and catalog"); }
  function event(kind, contextId, declaration, profileIndex) {
    // These commitments preserve actual accepted mark/path ordering. Hashing
    // scratch/native work remains outside copied-JSON quotas and RSS claims.
    return { kind, contextId, declarationSha256: digest(declaration), profile: profileIndex };
  }
  function append(row) {
    need(events.length < 707, "fixed707 compact event slots");
    // A one-element copy charges the retained event slot before insertion.
    const copied = capture([row]); healthy(); events.push(copied[0]);
  }
  return Object.freeze({
    validateCaptureDeclaration: candidate => state.run(() => {
      need(!captureAccepted && accepted === 0, "one initial capture declaration");
      const actual = capture(candidate); healthy(); validator.validateCaptureDeclaration(actual); healthy();
      captureSha256 = capture(digest(actual)); healthy(); captureAccepted = true;
    }),
    validateTemplate: candidate => state.run(() => {
      need(captureAccepted && accepted < ids.length, "capture before all705 ordered templates");
      const actual = capture(candidate); healthy(); need(actual && actual.id === ids[accepted], "exact next template identity");
      const report = validator.validateTemplate(actual); healthy();
      // Exact FULL accepted template, not merely its compact checkpoint view.
      // One-element snapshot charges the retained row and array slot before
      // insertion. The candidate can be released after this single pass.
      const producerRow = capture([{ contextId: actual.id, templateSha256: digest(actual) }]); healthy(); templates.push(producerRow[0]);
      const route = policy.routes[accepted], point = /^point[01]$/.test(actual.id);
      if (point) {
        need(pendingFinal === null, "previous point final event already committed");
        append(event("background", actual.id, { contextId: actual.id, graphPath: actual.graphPath,
          metadataPath: actual.pointCheckpoint.relativePath, marks: actual.marks }, route.profile));
        pendingFinal = capture(event("final", actual.id, { contextId: actual.id, graphPath: actual.graphPath,
          backgroundPath: actual.pointCheckpoint.relativePath, metadataPath: actual.metadataPath }, route.completionProfile));
      } else {
        append(event(actual.id.startsWith("diagnostic/") ? "diagnostic" : "germ", actual.id,
          { contextId: actual.id, graphPath: actual.graphPath, metadataPath: actual.metadataPath, marks: actual.marks }, route.profile));
        if (/^point[01]\/m9_j34$/.test(actual.id)) { need(pendingFinal && actual.id.startsWith(pendingFinal.contextId + "/"), "matching point final event"); append(pendingFinal); pendingFinal = null; }
      }
      healthy(); accepted++; return report;
    }),
    finish: () => state.run(() => {
      need(captureAccepted && accepted === ids.length && templates.length === ids.length && events.length === 707 && pendingFinal === null, "complete source-bound707 event catalog");
      const report = validator.finish(); healthy();
      const configuration = Object.freeze({ outputRoot: policy.outputRoot, events: Object.freeze(events), profiles: policy.profiles });
      // Charge final detached descriptor/container storage too. Clear the
      // temporary compact row array; never retain705 full candidate graphs.
      const producerCommitments = capture({ profileSha256, captureSha256, templates }); healthy(); templates.length = 0;
      token = Object.freeze({ schemaVersion: "phase627-checkpoint-catalog-configuration-v1" });
      identities.set(token, Object.freeze({ configuration, producerCommitments, health: state.health })); return report;
    }, true),
    configurationToken: () => state.run(() => { healthy(); need(token !== null, "completed private token"); return token; }, false, true)
  });
}
module.exports = { createCheckpointCatalog, checkpointCatalogIdentity };
