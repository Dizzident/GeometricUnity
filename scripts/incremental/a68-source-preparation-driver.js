"use strict";

// Prospective genuine-source preparation, deliberately uncalled on GU inputs
// while FIRST is closed. Admission callbacks reserve stages; they do not prove
// resource sufficiency, upstream certificates or producer/compiler identity.
const path = require("node:path");
const crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { validatedCheckpointConfiguration } = require("./a68-template-process-host");
const { checkpointEvent, createSourceCheckpointDispatcher } = require("./a68-source-checkpoint-dispatcher");
const { sourceContextIds, diagnosticMenu } = require("./a68-source-context-menu");
const { germMarkMenu } = require("./a68-audit-context-recipe");
const { RetainedMixedInputs, PINS } = require("./a68-retained-inputs");
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const { buildSourceBaselineGeometry, buildSourceGermGeometry } = require("./a68-source-geometry");
const { bindRetainedGeometry } = require("./a68-geometry-binding");
const { reconstructRetainedPolynomial } = require("./a68-polynomial-reconstruction");
const { createSourcePointAdapter, createSourceGermAdapter, sourceAdapterIdentity } = require("./a68-source-orchestration");
const { createDiagnosticSourceAdapter, diagnosticAdapterIdentity } = require("./a68-diagnostic-source-adapter");
const need = (ok, why) => { if (!ok) throw new Error("A68 source preparation: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered fields " + names.join(","));
  return Object.fromEntries(names.map(name => {
    const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value];
  }));
}
function caps(value, names) {
  const result = fields(value, names);
  need(Object.values(result).every(n => Number.isSafeInteger(n) && n > 0), "explicit finite positive quotas");
  return Object.freeze(result);
}
function metadataCaps(value) {
  const result = caps(value, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(result.maxDepth <= 64, "bounded metadata depth"); return result;
}
function algebraCaps(value) {
  const result = caps(value, ["maxBits", "scalarOperations", "rationalObjects", "matrixObjects", "matrixCells", "slotVisits"]);
  need(result.maxBits >= 16 && result.maxBits <= 1048576, "bounded arithmetic bit limit"); return result;
}
function comparisonCaps(value) { return caps(value, ["coordinateVisits", "sparseRecords", "tensorRecords", "rationalCharacters"]); }
function adapterCaps(value, germ) {
  fields(value, ["leaf", "export", "metadata", "comparison", ...(germ ? ["fields"] : [])]);
  caps(value.leaf, ["leaves", "records", "stringCharacters", "wireBytes", "rationalCharacters", "resolutions"]);
  caps(value.export, ["matrixSlots", "coordinateVisits", "entryRecords"]);
  metadataCaps(value.metadata); comparisonCaps(value.comparison);
  need(value.leaf.rationalCharacters <= 16384, "bounded leaf rational strings");
  if (germ) caps(value.fields, ["fields", "coordinateVisits", "arrayObjects", "arraySlots", "formatCharacters", "rationalCharacters"]);
}
function freeze(value) {
  if (value && typeof value === "object" && !Object.isFrozen(value)) { Object.values(value).forEach(freeze); Object.freeze(value); }
  return value;
}
const digest = value => crypto.createHash("sha256").update(JSON.stringify(value) + "\n").digest("hex");

function createSourcePreparationDriver(preflightResult, policy, limits, admitStage) {
  limits = fields(limits, ["snapshot", "dispatcher"]);
  const cap = metadataCaps(limits.snapshot), used = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  let failed = false, busy = false, next = 0, pending = null, active = null, inputs = null, dispatcher = null;
  let producerBegun = false, matchedTemplates = 0, menusMatched = 0, acceptedMenu = null;
  function release() { dispatcher?.abort(); pending = null; active = null; inputs = null; acceptedMenu = null; }
  function capture(value) {
    const remaining = Object.fromEntries(Object.keys(used).map(k => [k, cap[k] - used[k]])); remaining.maxDepth = cap.maxDepth;
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(used)) used[key] += copy.usage[key];
    need(!failed, "snapshot reentry"); return copy.value;
  }
  // Freeze the complete policy and every quota before any external admission.
  const copied = capture({ policy, limits }); policy = copied.policy; limits = copied.limits;
  fields(policy, ["repositoryRoot", "profiles", "routes"]);
  need(typeof policy.repositoryRoot === "string" && path.isAbsolute(policy.repositoryRoot) && path.resolve(policy.repositoryRoot) === policy.repositoryRoot &&
    policy.repositoryRoot !== path.parse(policy.repositoryRoot).root, "canonical nonroot repository directory");
  need(typeof admitStage === "function", "mandatory synchronous stage admission");
  need(Array.isArray(policy.profiles) && policy.profiles.length > 0 && policy.profiles.length <= 705, "bounded preparation profiles");
  for (const profile of policy.profiles) {
    need(profile && ["point", "germ", "diagnostic"].includes(profile.kind), "closed preparation kind");
    fields(profile, profile.kind === "point" ? ["kind", "algebra", "binding", "polynomialArithmetic", "polynomial", "adapter", "recipe"] :
      profile.kind === "germ" ? ["kind", "binding", "adapter", "recipe"] : ["kind", "algebra", "binding", "adapter", "recipe"]);
    if (profile.kind !== "germ" && profile.algebra !== null) algebraCaps(profile.algebra);
    if (profile.binding !== null) comparisonCaps(profile.binding);
    if (profile.kind !== "diagnostic") need(profile.binding !== null && (profile.kind === "germ" || profile.algebra !== null), "ordinary geometry quotas required");
    if (profile.kind === "point") {
      algebraCaps(profile.polynomialArithmetic);
      caps(profile.polynomial, ["inputRecords", "outputRecords", "recordVisits", "readCharacters", "outputCharacters", "rationalCharacters"]);
      need(profile.polynomial.rationalCharacters <= 16384, "bounded polynomial rational strings");
    }
    adapterCaps(profile.adapter, profile.kind === "germ"); caps(profile.recipe, ["nodeLimit", "markLimit"]);
  }
  const ids = sourceContextIds();
  need(Array.isArray(policy.routes) && policy.routes.length === 705 && ids.length === 705, "complete fixed705 preparation routes");
  const routes = new Map();
  for (let i = 0; i < ids.length; i++) {
    const route = fields(policy.routes[i], ["contextId", "profile"]);
    need(route.contextId === ids[i] && Number.isSafeInteger(route.profile) && route.profile >= 0 && route.profile < policy.profiles.length, "ordered exact preparation route");
    const profile = policy.profiles[route.profile], kind = /^point[01]$/.test(route.contextId) ? "point" : route.contextId.startsWith("diagnostic/") ? "diagnostic" : "germ";
    need(profile.kind === kind, "route/profile kind agreement");
    if (kind === "diagnostic") need(route.contextId === "diagnostic/secondJets" ? profile.algebra === null && profile.binding === null :
      profile.algebra !== null && profile.binding !== null, "diagnostic-specific geometry quotas");
    routes.set(route.contextId, profile);
  }
  // Both calls authenticate the same private, completed clean-transport result.
  // Construction performs metadata checks only, never retained I/O or geometry.
  const admitted = validatedCheckpointConfiguration(preflightResult), config = admitted.configuration;
  // The host owns this frozen descriptor; it is not caller-supplied hash data.
  // Compact checkpoint declarations alone omit producer resources/callbacks.
  const producer = fields(admitted.producerCommitments, ["profileSha256", "captureSha256", "templates"]);
  const hash = value => typeof value === "string" && /^[a-f0-9]{64}$/.test(value);
  need(hash(producer.profileSha256) && hash(producer.captureSha256) && Array.isArray(producer.templates) && producer.templates.length === ids.length,
    "complete authenticated producer profile capture and template commitments");
  for (let i = 0; i < ids.length; i++) {
    const row = fields(producer.templates[i], ["contextId", "templateSha256"]);
    need(row.contextId === ids[i] && hash(row.templateSha256), "ordered full producer template commitments");
  }
  dispatcher = createSourceCheckpointDispatcher(preflightResult, limits.dispatcher);
  function healthy() {
    need(!failed && admitted.health()?.failed === false, "healthy authenticated configuration");
    const status = dispatcher.snapshot();
    need(status.failed === false && status.acceptedEvents === next, "healthy synchronized private dispatcher");
    if (active) need(active.a.snapshot().failed === false && sourceAdapterIdentity(active.adapter).health()?.failed === false, "healthy active point");
    if (pending) need((pending.kind === "diagnostic" ? diagnosticAdapterIdentity(pending.adapter) : sourceAdapterIdentity(pending.adapter)).health()?.failed === false, "healthy pending source adapter");
    need(!failed, "swallowed callback reentry");
  }
  function run(action) {
    if (busy || failed || next === 707) { failed = true; release(); need(false, "failed reentrant or exhausted preparation driver"); }
    busy = true;
    try { healthy(); const value = action(); healthy(); return value; }
    catch (error) { failed = true; release(); throw error; }
    finally { busy = false; }
  }
  function stage(operation, contextId, details, action) {
    healthy();
    need(admitStage(freeze({ operation, contextId, details })) === undefined, "synchronous void stage admission required");
    healthy(); const result = action(); healthy(); return result;
  }
  function declaration(value, final) {
    const event = checkpointEvent(next), committed = config.events[next], d = capture(value);
    need((event.kind === "final") === final, "correct preparation versus point-final phase");
    fields(d, final ? ["contextId", "graphPath", "backgroundPath", "metadataPath"] : ["contextId", "graphPath", "metadataPath", "marks"]);
    need(committed.kind === event.kind && committed.contextId === event.contextId && d.contextId === event.contextId && digest(d) === committed.declarationSha256 &&
      d.graphPath === event.contextId + "/graph.json" && d.metadataPath === event.contextId + (event.kind === "background" ? "/background-checkpoint.json" : "/metadata.json"),
    "authenticated committed declaration hash and exact paths before source work");
    if (final) need(d.backgroundPath === event.contextId + "/background-checkpoint.json", "exact final background path");
    else {
      need(Array.isArray(d.marks), "committed mark menu"); const names = new Set();
      for (const mark of d.marks) {
        fields(mark, ["name", "degree", "expanded", "relativePath"]);
        need(typeof mark.name === "string" && !names.has(mark.name) && typeof mark.expanded === "boolean", "unique committed mark retention"); names.add(mark.name);
      }
    }
    healthy(); return { event, d };
  }
  function retained(contextId) {
    if (!inputs) inputs = stage("retained-inputs", contextId, { repositoryRoot: policy.repositoryRoot, pins: PINS }, () => new RetainedMixedInputs(policy.repositoryRoot));
    return inputs;
  }
  function baseline(contextId, point, profile) {
    const a = stage("geometry-algebra", contextId, { point, caps: profile.algebra }, () => new GeometryAlgebra(profile.algebra));
    const rebuilt = stage("source-baseline", contextId, { point, caps: profile.algebra }, () => buildSourceBaselineGeometry(a, point));
    const target = stage("retained-baseline", contextId, { point }, () => inputs.baseline(point));
    const binding = stage("geometry-binding", contextId, { kind: "baseline", point, caps: profile.binding }, () => bindRetainedGeometry(a, rebuilt, target, profile.binding));
    need(a.snapshot().failed === false, "healthy newly constructed baseline arithmetic"); return { a, baseline: rebuilt, binding };
  }
  function germ(contextId, point, metricBasis, jetIndex, owner, profile) {
    const rebuilt = stage("source-germ", contextId, { point, metricBasis, jetIndex, caps: owner.algebra }, () => buildSourceGermGeometry(owner.a, owner.baseline, metricBasis, jetIndex));
    const target = stage("retained-germ", contextId, { point, metricBasis, jetIndex, files: 1, byteCeiling: 145452 }, () => inputs.geometry(point, metricBasis, jetIndex));
    const binding = stage("geometry-binding", contextId, { kind: "germ", point, metricBasis, jetIndex, caps: profile.binding }, () => bindRetainedGeometry(owner.a, rebuilt, target, profile.binding));
    need(owner.a.snapshot().failed === false, "healthy germ arithmetic"); return binding;
  }
  function retention(d, isGerm) {
    const ordinary = isGerm ? new Set(germMarkMenu().map(mark => mark.name)) : null;
    const selected = [], ward = [];
    for (const mark of d.marks) (ordinary && !ordinary.has(mark.name) ? ward : selected).push([mark.name, mark.expanded]);
    return { retention: Object.freeze(Object.fromEntries(selected)), wardRetention: ward.length ? Object.freeze(Object.fromEntries(ward)) : null };
  }
  return Object.freeze({
    // Producer termination must not leave a prepared graph or point reachable.
    // Cancellation is sticky even after terminal completion; never a reset.
    abort: () => { failed = true; release(); },
    // This binds declared metadata, NOT the identity of a live C# process or
    // proof that its actual arithmetic respects these declarations.
    beginProducer: (profile, captureDeclaration) => run(() => {
      need(!producerBegun && next === 0 && pending === null, "one producer profile handshake before preparation");
      const copied = capture({ profile, captureDeclaration });
      need(digest(copied.profile) === producer.profileSha256 && digest(copied.captureDeclaration) === producer.captureSha256 &&
        same(copied.profile.capture, copied.captureDeclaration), "authenticated full producer profile and capture declaration");
      producerBegun = true;
    }),
    validateDiagnosticMenu: (diagnostic, actual) => run(() => {
      const event = checkpointEvent(next);
      need(producerBegun && pending === null && active === null && acceptedMenu === null &&
        event.kind === "diagnostic" && next === 704 + menusMatched && typeof diagnostic === "string" &&
        event.contextId === "diagnostic/" + diagnostic, "one ordered diagnostic menu before preparation");
      return stage("diagnostic-menu", event.contextId, { caps: cap }, () => {
        const expected = capture(diagnosticMenu(diagnostic)), copied = capture(actual);
        need(same(copied, expected) && digest(copied) === digest(expected), "complete ordered independent diagnostic menu");
        acceptedMenu = event.contextId; menusMatched++;
        return freeze({ contextId: event.contextId, scientificExecutionAuthorized: false });
      });
    }),
    prepareNext: (value, fullTemplate) => run(() => {
      need(pending === null, "accept the pending checkpoint before preparing another");
      const { event, d } = declaration(value, false), profile = routes.get(event.contextId), contextId = event.contextId;
      need(producerBegun && matchedTemplates < ids.length, "producer profile handshake before source preparation");
      const template = capture(fullTemplate), committed = producer.templates[matchedTemplates];
      need(template && template.id === contextId && committed.contextId === contextId && digest(template) === committed.templateSha256,
        "authenticated full regenerated producer template before source work");
      need(same(d, { contextId: template.id, graphPath: template.graphPath,
        metadataPath: event.kind === "background" ? template.pointCheckpoint.relativePath : template.metadataPath, marks: template.marks }),
        "compact declaration derived from the same full producer template");
      if (event.kind === "diagnostic") {
        need(acceptedMenu === contextId && menusMatched === next - 703, "accepted matching diagnostic menu before source preparation");
        acceptedMenu = null;
      }
      retained(contextId); let adapter, plan;
      if (event.kind === "background") {
        need(active === null, "no overlapping point preparation"); const point = Number(contextId.slice(-1));
        const owner = baseline(contextId, point, profile);
        const polynomial = stage("retained-polynomial", contextId, { point, files: 6, byteCeiling: 1097699 }, () => inputs.polynomialInputs(point));
        const certificate = stage("retained-certificate", contextId, { point, files: 1, byteCeiling: 19274 }, () => inputs.certificate(point));
        const proof = stage("polynomial-reconstruction", contextId, { point, arithmetic: profile.polynomialArithmetic, caps: profile.polynomial },
          () => reconstructRetainedPolynomial(polynomial, certificate, profile.polynomialArithmetic, profile.polynomial));
        adapter = stage("source-adapter", contextId, { kind: "point", point, caps: profile.adapter }, () => createSourcePointAdapter(owner.a, owner.binding, proof, profile.adapter));
        active = { ...owner, point, adapter, algebra: profile.algebra };
        const policy = stage("retention-policy", contextId, { kind: "point", marks: d.marks.length, caps: cap }, () => retention(d, false));
        const options = freeze({ retention: policy.retention, ...profile.recipe });
        plan = stage("source-recipe", contextId, { kind: "point", caps: profile.recipe, retention: options.retention }, () => adapter.buildPointRecipe(options));
      } else if (event.kind === "germ") {
        const match = /^point([01])\/m([0-9]+)_j([0-9]+)$/.exec(contextId), point = Number(match[1]), metricBasis = Number(match[2]), jetIndex = Number(match[3]);
        need(active && active.point === point, "same active point for ordinary germ");
        const binding = germ(contextId, point, metricBasis, jetIndex, active, profile);
        adapter = stage("source-adapter", contextId, { kind: "germ", point, metricBasis, jetIndex, caps: profile.adapter }, () => createSourceGermAdapter(active.a, active.adapter, binding, profile.adapter));
        const policy = stage("retention-policy", contextId, { kind: "germ", marks: d.marks.length, caps: cap }, () => retention(d, true));
        const options = freeze({ ...policy, ...profile.recipe });
        plan = stage("source-recipe", contextId, { kind: "germ", caps: profile.recipe, retention: options.retention, wardRetention: options.wardRetention }, () => adapter.buildGermRecipe(options));
      } else {
        need(active === null, "diagnostics after completed ordinary points");
        const diagnostic = contextId.slice("diagnostic/".length), point = diagnostic === "grade10" ? 0 : diagnostic === "acceleration" ? 1 : null;
        let owner = null; const bindings = { baseline: null, germs: [] };
        if (point !== null) {
          owner = baseline(contextId, point, profile); owner.algebra = profile.algebra; bindings.baseline = owner.binding;
          for (let m = 0; m < (diagnostic === "grade10" ? 4 : 1); m++) bindings.germs.push(germ(contextId, point, m, diagnostic === "grade10" ? 4 : 10, owner, profile));
        }
        adapter = stage("source-adapter", contextId, { kind: "diagnostic", diagnostic, caps: profile.adapter }, () => createDiagnosticSourceAdapter(owner ? owner.a : null, diagnostic, bindings, profile.adapter));
        const policy = stage("retention-policy", contextId, { kind: "diagnostic", marks: d.marks.length, caps: cap }, () => retention(d, false));
        const options = freeze({ retention: policy.retention, ...profile.recipe });
        plan = stage("source-recipe", contextId, { kind: "diagnostic", diagnostic, caps: profile.recipe, retention: options.retention }, () => adapter.buildRecipe(options));
      }
      const identity = event.kind === "diagnostic" ? diagnosticAdapterIdentity(adapter) : sourceAdapterIdentity(adapter);
      need(identity.contextId === contextId && identity.prepared && !identity.replayed && !identity.completed && identity.health()?.failed === false, "fresh genuine prepared adapter");
      const report = stage("leaf-declaration-export", contextId, { leaves: plan.tensorPlan.leaves.length, caps: cap }, () => {
        const result = capture({ contextId, leaves: plan.tensorPlan.leaves });
        need(result.leaves.length === template.leaves.length, "complete ordered producer leaf bindings");
        for (let i = 0; i < result.leaves.length; i++) {
          const actual = fields(result.leaves[i], ["id", "degree", "source", "sha256"]), expected = template.leaves[i];
          need(actual.id === expected.id && actual.degree === expected.degree && actual.source === expected.source && hash(actual.sha256),
            "exact ordered producer leaf identity source degree and canonical hash");
        }
        return result;
      });
      matchedTemplates++; pending = { kind: event.kind, adapter, declaration: d }; return report;
    }),
    acceptCheckpoint: observed => run(() => {
      need(pending !== null, "one prepared pending checkpoint required");
      const pinned = capture(observed), profile = config.profiles[config.events[next].profile];
      return stage("checkpoint-read-replay", pending.declaration.contextId, { kind: pending.kind, caps: profile }, () => {
        const result = dispatcher.dispatch(pending.adapter, pending.declaration, pinned);
        next++; pending = null; if (next === 707) inputs = null; return result;
      });
    }),
    completePoint: (value, observed) => run(() => {
      need(pending === null && active !== null, "accepted point with no pending child before final");
      const { d } = declaration(value, true);
      const pinned = capture(observed), profile = config.profiles[config.events[next].profile];
      return stage("point-completion", d.contextId, { caps: profile }, () => {
        const result = dispatcher.dispatch(active.adapter, d, pinned); next++; active = null; return result;
      });
    }),
    snapshot: () => {
      if (busy) failed = true;
      try { healthy(); } catch { failed = true; }
      if (failed) release();
      return freeze({ failed, acceptedEvents: next, completed: !failed && next === 707, pendingContextId: pending?.declaration.contextId ?? null,
        producerProfileMatched: producerBegun, producerTemplatesMatched: matchedTemplates, menusMatched,
        retainedActivePoints: active ? 1 : 0, retainedInputReaders: inputs ? 1 : 0, snapshotUsage: { ...used },
        scope: { fullPrerequisitesImplemented: false, upstreamCertificateProofEstablished: false, producerIdentityEstablished: false,
          totalProcessMemoryProved: false, scientificExecutionAuthorized: false } });
    }
  });
}
module.exports = { createSourcePreparationDriver };
