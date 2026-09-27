"use strict";
// Concrete routing of authentic source adapters and pinned readers. This does
// NOT build adapters, authorize execution, authenticate upstream certificates,
// or prove whole-process resources. Genuine source-positive use awaits FIRST.
const path = require("node:path");
const crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { sourceAdapterIdentity } = require("./a68-source-orchestration");
const { diagnosticAdapterIdentity } = require("./a68-diagnostic-source-adapter");
const { readPointCheckpoint } = require("./a68-point-checkpoint");
const { readContextCheckpoint } = require("./a68-context-checkpoint");
const { readPointCompletion } = require("./a68-point-completion");
const { validatedCheckpointConfiguration } = require("./a68-template-process-host");
const need = (ok, why) => { if (!ok) throw new Error("A68 source dispatcher: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered fields");
  return Object.fromEntries(names.map(name => {
    const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value];
  }));
}
function checkpointEvent(index) {
  need(Number.isSafeInteger(index) && index >= 0 && index < 707, "fixed707 event index");
  if (index >= 704) return Object.freeze({ kind: "diagnostic", contextId: "diagnostic/" + ["grade10", "acceleration", "secondJets"][index - 704] });
  const point = Math.floor(index / 352), offset = index % 352, id = "point" + point;
  return Object.freeze(offset === 0 ? { kind: "background", contextId: id } : offset === 351 ? { kind: "final", contextId: id } :
    { kind: "germ", contextId: id + "/m" + Math.floor((offset - 1) / 35) + "_j" + (offset - 1) % 35 });
}
const digest = declaration => crypto.createHash("sha256").update(JSON.stringify(declaration) + "\n").digest("hex");
function createSourceCheckpointDispatcher(preflightResult, limits) {
  const admitted = validatedCheckpointConfiguration(preflightResult), configuration = admitted.configuration;
  limits = fields(limits, ["configuration", "event", "total"]);
  // Detach limits before any adapter/reader callback, including nested quotas.
  const cap = {};
  for (const key of ["configuration", "event", "total"]) {
    const value = fields(limits[key], ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
    need(Object.values(value).every(n => Number.isSafeInteger(n) && n > 0) && value.maxDepth <= 64, "finite snapshot limits"); cap[key] = Object.freeze(value);
  }
  Object.freeze(cap);
  const initialCap = Object.fromEntries(Object.keys(cap.configuration).map(k => [k, Math.min(cap.configuration[k], cap.total[k])]));
  const copied = snapshotCanonicalMetadata(configuration, initialCap), config = fields(copied.value, ["outputRoot", "events", "profiles"]);
  need(typeof config.outputRoot === "string" && path.isAbsolute(config.outputRoot) && path.resolve(config.outputRoot) === config.outputRoot &&
    config.outputRoot !== path.parse(config.outputRoot).root, "canonical nonroot output directory");
  need(Array.isArray(config.events) && config.events.length === 707 && Array.isArray(config.profiles) && config.profiles.length > 0 && config.profiles.length <= 707, "complete frozen event and quota menus");
  config.profiles.forEach(profile => fields(profile, ["read", "replay"]));
  for (let i = 0; i < 707; i++) {
    const row = fields(config.events[i], ["kind", "contextId", "declarationSha256", "profile"]), expected = checkpointEvent(i);
    need(row.kind === expected.kind && row.contextId === expected.contextId && typeof row.declarationSha256 === "string" && /^[a-f0-9]{64}$/.test(row.declarationSha256) &&
      Number.isSafeInteger(row.profile) && row.profile >= 0 && row.profile < config.profiles.length, "exact frozen event commitment");
    const profile = config.profiles[row.profile];
    need(profile.read && typeof profile.read === "object" && (row.kind === "final" ? profile.replay === null : profile.replay && typeof profile.replay === "object"), "event-specific read/replay quotas");
  }
  // Only compact commitments and quotas survive construction, never705 mark
  // menus. Quotas count logical snapshots; stringify/native/GC/RSS are not proved.
  const used = { nodes: copied.usage.nodes, arraySlots: copied.usage.arraySlots, stringCharacters: copied.usage.stringCharacters };
  for (const key of Object.keys(used)) need(used[key] <= cap.total[key], "configuration within cumulative snapshot quota");
  let failed = false, busy = false, next = 0, active = null;
  const health = [];
  function healthy() {
    need(!failed, "sticky dispatcher failure");
    need(admitted.health()?.failed === false, "healthy validated checkpoint configuration");
    for (const check of health) need(check()?.failed === false, "healthy accepted source adapters");
    need(!failed, "health callback reentry");
  }
  function capture(declaration, observed) {
    const remaining = Object.fromEntries(Object.keys(used).map(k => [k, Math.min(cap.event[k], cap.total[k] - used[k])]));
    remaining.maxDepth = Math.min(cap.event.maxDepth, cap.total.maxDepth);
    const copy = snapshotCanonicalMetadata({ declaration, observed }, remaining);
    for (const key of Object.keys(used)) used[key] += copy.usage[key];
    need(!failed, "snapshot reentry"); return copy.value;
  }
  const identify = (kind, adapter) => kind === "diagnostic" ? diagnosticAdapterIdentity(adapter) : sourceAdapterIdentity(adapter);
  return Object.freeze({
    // Owning preparation/producer failure must also release this active point.
    // Cancellation only poisons; it cannot reset, retry or grant acceptance.
    abort: () => { failed = true; active = null; },
    dispatch: (adapter, declaration, observed) => {
      if (busy || failed || next === 707) { failed = true; need(false, "failed reentrant or exhausted dispatcher"); }
      busy = true;
      try {
        healthy();
        const event = config.events[next], profile = config.profiles[event.profile], before = identify(event.kind, adapter);
        need(before.contextId === event.contextId && before.kind === (event.kind === "background" || event.kind === "final" ? "point" : event.kind) &&
          before.health()?.failed === false, "authentic healthy next source identity");
        if (event.kind === "final") need(active && active.adapter === adapter && active.identity === before.identity && before.replayed && !before.completed, "same active point before final");
        else {
          need(before.prepared && !before.replayed && !before.completed, "fresh independently prepared source adapter");
          if (event.kind === "germ") need(active && before.parentIdentity === active.identity, "exact active parent source identity");
          else need(active === null && before.parentIdentity === null, "no overlapping point or foreign parent");
        }
        const copy = capture(declaration, observed), d = copy.declaration;
        need(digest(d) === event.declarationSha256 && d.contextId === event.contextId && d.graphPath === event.contextId + "/graph.json" &&
          d.metadataPath === event.contextId + (event.kind === "background" ? "/background-checkpoint.json" : "/metadata.json"), "frozen declaration commitment and exact context paths");
        if (event.kind === "final") {
          fields(d, ["contextId", "graphPath", "backgroundPath", "metadataPath"]);
          need(d.backgroundPath === event.contextId + "/background-checkpoint.json", "fixed point background path");
          adapter.acceptPointCompletion(readPointCompletion(config.outputRoot, d, copy.observed, profile.read));
        } else {
          fields(d, ["contextId", "graphPath", "metadataPath", "marks"]);
          if (event.kind === "background") adapter.replayPointCheckpoint(readPointCheckpoint(config.outputRoot, d, copy.observed, profile.read), profile.replay);
          else adapter.replayContextCheckpoint(readContextCheckpoint(config.outputRoot, d, copy.observed, profile.read), profile.replay);
        }
        // Plain return values and caller success flags confer no authority.
        const after = identify(event.kind, adapter);
        need(after.identity === before.identity && after.health === before.health && after.replayed && after.health()?.failed === false &&
          (event.kind === "final" ? after.completed : !after.completed), "live authentic replay postcondition");
        healthy();
        if (event.kind === "background") active = { adapter, identity: after.identity };
        if (event.kind === "final") active = null;
        else health.push(after.health);
        next++;
        return Object.freeze({ event: next - 1, kind: event.kind, contextId: event.contextId, scientificExecutionAuthorized: false });
      } catch (error) { failed = true; active = null; throw error; }
      finally { busy = false; }
    },
    snapshot: () => {
      if (busy) failed = true;
      try { healthy(); } catch { failed = true; }
      if (failed) active = null;
      return Object.freeze({ failed, acceptedEvents: next, completed: !failed && next === 707 && active === null,
        retainedHealthChecks: health.length, retainedActivePointAdapters: active ? 1 : 0,
        snapshotUsage: Object.freeze({ ...used }), scope: Object.freeze({ upstreamCertificateProofEstablished: false,
          totalProcessMemoryProved: false, fullPrerequisitesImplemented: false, scientificExecutionAuthorized: false }) });
    }
  });
}
module.exports = { checkpointEvent, createSourceCheckpointDispatcher };
