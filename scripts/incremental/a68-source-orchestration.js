"use strict";

// Source-bound orchestration SLICE, not the full705-context driver. Production
// constructors accept existing private source/comparison/polynomial brands;
// neither an observed DAG nor a caller's success flag can mint those brands.
// Tests exercise generic manufactured closure/snapshot/actual tiny-replay
// positives and production rejection paths. No GU source factory or polynomial
// is run here. Receipt binding does not admit all705 prospective context plans.
const crypto = require("node:crypto");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const { sourceGeometryIdentity } = require("./a68-source-geometry");
const { reconstructedPolynomialIdentity } = require("./a68-polynomial-reconstruction");
const { exportRecipeGeometry } = require("./a68-geometry-recipe-binding");
const { compareRecordedGeometry } = require("./a68-geometry-metadata");
const { createSourceGermFieldResolver } = require("./a68-source-field-resolver");
const { MixedRecipe } = require("./a68-mixed-recipe");
const { buildAuditPointContext, buildAuditGermContext } = require("./a68-audit-context-recipe");
const { baselineRoles } = require("./a68-background-recipe");
const { EPSILON } = require("./a68-retained-inputs");
const need = (ok, why) => { if (!ok) throw new Error("A68 source orchestration: " + why); };
const integer = (x, min = 0, max = Number.MAX_SAFE_INTEGER) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= min && x <= max;
const ascii = x => typeof x === "string" && /^[\x20-\x7e]*$/.test(x);
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
function own(x, key) { const d = Object.getOwnPropertyDescriptor(x, key); need(d && Object.hasOwn(d, "value"), "own data property " + key); return d.value; }
function shape(x, keys) {
  need(x && typeof x === "object" && !Array.isArray(x), "closed data object");
  const actual = Reflect.ownKeys(x); need(actual.length === keys.length && keys.every((key, i) => actual[i] === key), "closed ordered fields " + keys.join(","));
  return Object.fromEntries(keys.map(key => [key, own(x, key)]));
}
function array(x, cap) {
  need(Array.isArray(x), "dense array"); const n = own(x, "length");
  need(integer(n, 0, cap), "pre-copy array length ceiling");
  need(Reflect.ownKeys(x).length === n + 1, "array has no extra fields or symbols");
  for (let i = 0; i < n; i++) own(x, String(i)); return n;
}
function caps(x, keys) { x = shape(x, keys); need(keys.every(k => integer(x[k], 1)), "explicit finite positive limits"); return Object.freeze(x); }
function counter(limits) {
  const used = Object.fromEntries(Object.keys(limits).map(k => [k, 0]));
  return { used, charge: (key, count = 1) => { need(integer(count) && count <= limits[key] - used[key], "pre-copy " + key + " ceiling"); used[key] += count; } };
}
const popcount = x => { let n = 0; while (x) { n += x & 1; x >>>= 1; } return n; };
function rational(text, cap) {
  need(typeof text === "string" && text.length > 0 && text.length <= cap && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded canonical rational");
  const parts = text.split("/"), n = BigInt(parts[0]), d = parts.length === 1 ? 1n : BigInt(parts[1]);
  let a = n < 0n ? -n : n, b = d; while (b) [a, b] = [b, a % b];
  need(text !== "-0" && a === 1n && (parts.length === 1 || d !== 1n), "reduced canonical rational");
}

// Copies own data into fresh plain JSON shapes before hashing, preventing an
// inherited toJSON or an array getter from giving hashing and replay two views.
// Logical copied data/characters are capped. Input objects, Reflect.ownKeys
// scratch, GCD/hash internals, UTF16/GC/RSS are NOT a total memory proof.
function createCanonicalLeafClosure(entries, limits) {
  const cap = caps(limits, ["leaves", "records", "stringCharacters", "wireBytes", "rationalCharacters", "resolutions"]);
  need(cap.rationalCharacters <= 16384, "common rational character domain");
  const { used, charge } = counter(cap), count = array(entries, cap.leaves); charge("leaves", count);
  const values = new Map(), leaves = [];
  for (let e = 0; e < count; e++) {
    const entry = shape(own(entries, String(e)), ["id", "degree", "source", "tensor"]);
    need(ascii(entry.id) && entry.id.trim() && ascii(entry.source) && entry.source.trim() && integer(entry.degree, 0, 14), "typed named leaf");
    need(!values.has(entry.id), "distinct declared leaf identities"); charge("stringCharacters", entry.id.length + entry.source.length);
    const n = array(entry.tensor, cap.records - used.records); charge("records", n);
    const rows = []; let previous = -1, bytes = 2 + Math.max(0, n - 1);
    charge("wireBytes", bytes);
    for (let i = 0; i < n; i++) {
      const r = shape(own(entry.tensor, String(i)), ["form", "blade", "k0", "k1", "real", "imaginary"]);
      need(integer(r.form, 0, 16383) && popcount(r.form) === entry.degree && integer(r.blade, 0, 16383) && r.k0 === 0 && r.k1 === 0 && !Object.is(r.k0, -0) && !Object.is(r.k1, -0), "complete local typed tensor domain");
      const key = r.form * 16384 + r.blade; need(key > previous, "strict complete tensor support ordering"); previous = key;
      need(typeof r.real === "string" && typeof r.imaginary === "string", "rational strings");
      charge("stringCharacters", r.real.length + r.imaginary.length); rational(r.real, cap.rationalCharacters); rational(r.imaginary, cap.rationalCharacters);
      need(r.real !== "0" || r.imaginary !== "0", "explicit sparse zero forbidden");
      // All value fields are ASCII and require no escaping. Admit exact wire
      // characters before constructing the row/string; no grade truncation.
      const rowBytes = '{"form":,"blade":,"k0":0,"k1":0,"real":"","imaginary":""}'.length + String(r.form).length + String(r.blade).length + r.real.length + r.imaginary.length;
      charge("wireBytes", rowBytes); bytes += rowBytes;
      rows.push(Object.freeze({ form: r.form, blade: r.blade, k0: 0, k1: 0, real: r.real, imaginary: r.imaginary }));
    }
    Object.freeze(rows); const wire = JSON.stringify(rows); need(wire.length === bytes, "canonical exact wire envelope");
    const leaf = Object.freeze({ id: entry.id, degree: entry.degree, source: entry.source, sha256: crypto.createHash("sha256").update(wire).digest("hex") });
    leaves.push(leaf); values.set(leaf.id, { leaf, rows });
  }
  const guard = stateGuard();
  return Object.freeze({ leaves: Object.freeze(leaves), resolveLeaf: specification => guard.run(() => {
      charge("resolutions"); specification = shape(specification, ["id", "degree", "source", "sha256"]);
      const value = values.get(specification.id); need(value && ["id", "degree", "source", "sha256"].every(k => specification[k] === value.leaf[k]), "complete declared leaf specification"); return value.rows;
  }), snapshot: () => Object.freeze({ ...used, ...guard.snapshot() }), scope: Object.freeze({ fullSupportPreserved: true, sourceAuthenticityEstablished: false,
    logicalCanonicalWireBounded: true, totalProcessMemoryProved: false }) });
}


const pointIdentities = new WeakMap();
const adapterIdentities = new WeakMap();
function sourceAdapterIdentity(adapter) {
  const read = adapterIdentities.get(adapter); need(read, "private source adapter identity required"); return read();
}
// Construct this closure OUTSIDE the source adapter's lexical environment.
// Retaining health must retain only sticky flags and the compact point ledger,
// never the point's geometry, expected recipe or verified tensor exports.
function pointAdapterHealth(guardHealth) {
  let ledgerHealth = null;
  return Object.freeze({ health: () => Object.freeze({ failed: guardHealth().failed || ledgerHealth?.().failed === true }),
    bindLedger(ledger) { need(ledgerHealth === null, "one private point health ledger"); ledgerHealth = ledger.snapshot; } });
}
const verifiedExportBindings = new WeakMap();
const fixedConstants = Object.freeze(["0", "1", "-1", "-1/2", "1/2", "1/3", "907712"]);
const identityKeys = ["contextId", "tensorPlan", "namedRoots", "geometry", "checks", "domainChecks", "error", "requests"];

// Generic positive tests may bind an ACTUAL manufactured numerical receipt to
// an independent manufactured expectation. This does not mint a source brand.
// Source adapters below supply their own privately saved source-bound plan.
// Frozen tensor arrays belong to the replay receipt and are shared, not copied
// into each of350 germs. Receipt export admission covers their retained data;
// snapshot limits here cover compared metadata only, not total live RSS.
function bindVerifiedReplayExports(receipt, expected, metadataLimits) {
  const { readVerifiedReplayReceipt } = require("./a68-tensor-replay");
  const actual = readVerifiedReplayReceipt(receipt);
  expected = snapshotCanonicalMetadata(shape(expected, identityKeys), metadataLimits).value;
  for (const key of identityKeys) need(JSON.stringify(actual[key]) === JSON.stringify(expected[key]), "exact replay identity " + key);
  need(actual.completion && ["tensor", "scalar", "consumers", "released"].every(k => actual.completion[k] === true), "complete numerical replay lifecycle");
  need(Array.isArray(actual.exports) && actual.exports.length === expected.requests.length, "exact replay export census");
  const byId = new Map();
  for (let i = 0; i < expected.requests.length; i++) {
    const requested = expected.requests[i], exported = actual.exports[i];
    need(exported.id === requested.id && exported.degree === requested.degree && exported.node === requested.node && !byId.has(exported.id), "exact ordered replay exports");
    need(Object.isFrozen(exported.tensor) && /^[0-9a-f]{64}$/.test(exported.sha256), "immutable verified numerical exports");
    byId.set(exported.id, exported);
  }
  const guard = stateGuard();
  const result = Object.freeze({ contextId: expected.contextId, exports: actual.exports,
    resolveExport: id => guard.run(() => { need(typeof id === "string" && byId.has(id), "declared replay export ID"); return byId.get(id); }),
    snapshot: () => Object.freeze(guard.snapshot()),
    scope: Object.freeze({ fullNumericalReplayIdentityMatched: true, sourceAuthenticityEstablished: false, sharedImmutableTensorArrays: true, totalProcessMemoryProved: false }) });
  verifiedExportBindings.set(result, { byId, expected }); return result;
}

function createVerifiedBaselineLeafBindings(binding, contextId, limits) {
  const verified = verifiedExportBindings.get(binding); need(verified && !binding.snapshot().failed, "private verified-export binding required");
  const cap = caps(limits, ["leaves", "stringCharacters", "resolutions"]), roles = baselineRoles();
  const canonical = roles.filter((r, i) => roles.findIndex(s => s.canonicalId === r.canonicalId) === i);
  need(ascii(contextId) && contextId.trim() && canonical.length === 27 && cap.leaves >= 27, "named admitted27 baseline imports");
  need(verified.expected.requests.length === 27 && canonical.every((role, i) => {
    const r = verified.expected.requests[i]; return r.id === role.canonicalId && r.degree === role.degree;
  }), "complete ordered canonical baseline export menu");
  const leafIds = {}, leaves = [], values = new Map(); let characters = 0, resolutions = 0;
  for (const role of canonical) {
    const value = binding.resolveExport(role.canonicalId), id = contextId + "/baseline/" + role.canonicalId;
    const source = "verified-point-replay/" + binding.contextId + "/" + role.canonicalId;
    need(id.length + source.length <= cap.stringCharacters - characters, "baseline declaration character ceiling"); characters += id.length + source.length;
    const specification = Object.freeze({ id, degree: role.degree, source, sha256: value.sha256 });
    leafIds[role.canonicalId] = id; leaves.push(specification); values.set(id, { specification, tensor: value.tensor });
  }
  const guard = stateGuard();
  return Object.freeze({ leaves: Object.freeze(leaves), leafIds: Object.freeze(leafIds),
    resolveLeaf: specification => guard.run(() => {
      need(!binding.snapshot().failed && resolutions < cap.resolutions, "healthy export binding and baseline resolution quota"); resolutions++;
      specification = shape(specification, ["id", "degree", "source", "sha256"]); const value = values.get(specification.id);
      need(value && ["id", "degree", "source", "sha256"].every(k => specification[k] === value.specification[k]), "exact verified baseline leaf binding"); return value.tensor;
    }),
    snapshot: () => Object.freeze({ ...guard.snapshot(), leafSpecifications: leaves.length, stringCharacters: characters, resolutions }),
    scope: Object.freeze({ canonicalRoleCount: 28, distinctLeafCount: 27, onlyAlias: "GradientPieces[0]=KInputs[0]", sourceAuthenticityEstablished: false,
      sharedPointExportTensorArrays: true, retainedExportStorageChargedByReplay: true, totalProcessMemoryProved: false }) });
}
function adapterLimits(limits, germ) {
  const names = ["leaf", "export", "metadata", "comparison", ...(germ ? ["fields"] : [])]; limits = shape(limits, names);
  return Object.freeze({ leaf: caps(limits.leaf, ["leaves", "records", "stringCharacters", "wireBytes", "rationalCharacters", "resolutions"]),
    export: caps(limits.export, ["matrixSlots", "coordinateVisits", "entryRecords"]),
    metadata: caps(limits.metadata, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]),
    comparison: caps(limits.comparison, ["coordinateVisits", "sparseRecords", "tensorRecords", "rationalCharacters"]),
    ...(germ ? { fields: caps(limits.fields, ["fields", "coordinateVisits", "arrayObjects", "arraySlots", "formatCharacters", "rationalCharacters"]) } : {}) });
}
// Detached retained-health state: never close over a child adapter's source
// bindings, parent, replay plan or resolver. A completed helper clears its
// bindings/commitments; its bounded slot paths/counters/limits and detached
// source-algebra health dependency remain. This extra retained per-child
// metadata still needs admission in the parent's350-child health ledger.
function liveGeometryState() {
  let current = null;
  return Object.freeze({
    get current() { return current; },
    configure(a, slots, limits) {
      need(current === null, "one detached live geometry helper");
      const { createLiveGeometryValidation } = require("./a68-live-geometry");
      current = createLiveGeometryValidation(a, slots, limits);
    },
    abort: () => current?.abort(),
    healthy: () => current?.snapshot().failed !== true
  });
}
function stateGuard(onFailure = () => {}, healthy = () => true) {
  let failed = false, busy = false;
  const check = () => { if (!healthy()) { failed = true; onFailure(); } return !failed; };
  return { run: f => { if (failed || busy) { failed = true; onFailure(); need(false, "poisoned or reentrant source adapter"); } busy = true; try { need(check(), "healthy source adapter dependencies"); const result = f(); need(check(), "reentrant source adapter failure"); return result; } catch (error) { failed = true; onFailure(); throw error; } finally { busy = false; } }, snapshot: () => { check(); return Object.freeze({ failed }); } };
}
function createSourcePointAdapter(a, binding, polynomialProof, limits) {
  const identity = boundGeometryIdentity(a, binding), source = sourceGeometryIdentity(a, identity.rebuilt), polynomial = reconstructedPolynomialIdentity(polynomialProof);
  need(source.kind === "baseline" && polynomial.point === source.point && polynomialProof.point === source.point, "same-point authentic baseline/polynomial");
  need(polynomialProof.gamma === 1 && polynomialProof.kappa === 907712 && polynomialProof.lambda === "1/907712" && polynomialProof.epsilon === EPSILON, "fixed conditional branch profile");
  const cap = adapterLimits(limits, false), exported = exportRecipeGeometry(a, binding, cap.export), prefix = "point" + source.point;
  const xLeaf = prefix + "/input/X", curvatureLeaf = prefix + "/geometry/curvature";
  const closure = createCanonicalLeafClosure([
    { id: xLeaf, degree: 1, source: polynomial.polynomial.X.source.path + "#independent-five-order-reconstruction", tensor: polynomialProof.tensor },
    { id: curvatureLeaf, degree: 2, source: binding.source.path + "#independent-source-spin-curvature", tensor: identity.rebuilt.spinCurvature }
  ], cap.leaf);
  need(closure.leaves[0].sha256 === polynomialProof.tensorSha256, "newly reconstructed X fingerprint");
  const live = liveGeometryState();
  const guard = stateGuard(live.abort, live.healthy), state = { expectedReplay: null, verifiedExports: null, completion: null, backgroundRoot: null }; let compared = false, built = false, replayed = false;
  const identityToken = Object.freeze({}), health = pointAdapterHealth(guard.snapshot);
  const adapter = Object.freeze({ point: source.point, leaves: closure.leaves,
    configureLiveGeometry: limits => guard.run(() => {
      need(built && !compared && !replayed && live.current === null, "one live geometry configuration after point recipe before observations");
      live.configure(a, [{ metadataPath: "background/geometry", binding }], limits);
    }),
    precheckGeometry: (metadataPath, observed) => guard.run(() => {
      need(live.current && built && !compared && !replayed, "configured pending point live geometry");
      return live.current.precheck(metadataPath, observed);
    }),
    compareGeometryMetadata: observed => guard.run(() => {
      need(live.current === null, "configured live geometry requires pinned checkpoint comparison");
      need(!compared, "one complete baseline metadata comparison"); const copy = snapshotCanonicalMetadata(observed, cap.metadata);
      const comparison = compareRecordedGeometry(a, binding, copy.value, cap.comparison); compared = true;
      return Object.freeze({ comparison, snapshotUsage: copy.usage });
    }),
    buildPointRecipe: options => guard.run(() => {
      // Pure independent planning precedes recorded observations. Acceptance
      // of a numerical replay receipt still requires complete metadata checks.
      need(!built, "one independent point recipe"); options = shape(options, ["retention", "nodeLimit", "markLimit"]);
      need(integer(options.nodeLimit, 1) && integer(options.markLimit, 1), "explicit point recipe ceilings");
      const retention = snapshotCanonicalMetadata(options.retention, cap.metadata).value;
      const recipe = new MixedRecipe({ leaves: closure.leaves, constants: fixedConstants, matrices: exported.matrices, nodeLimit: options.nodeLimit, markLimit: options.markLimit });
      const context = buildAuditPointContext(recipe, { xLeaf, curvatureLeaf, geometry: exported.descriptor, validateGeometry: exported.validateGeometry, retention });
      const requests = Object.freeze(context.canonicalBaselineExports.map(e => Object.freeze({ id: e.id, degree: e.degree, node: recipe.RecordedNode(e.value) })));
      need(requests.length === 27 && new Set(requests.map(e => e.node)).size === 27, "27 distinct canonical point export identities");
      const tensorPlan = recipe.Finish(); built = true;
      const scalarGeometry = Object.freeze(exported.matrices.map(m => Object.freeze({ id: m.name, entries: m.matrix })));
      state.expectedReplay = freeze({ contextId: context.contextId, tensorPlan, namedRoots: context.scalarRoots, geometry: scalarGeometry,
        checks: context.checks, domainChecks: context.domainChecks, error: null, requests });
      // These are MixedRecipe {name,matrix} declarations, NOT the scalar
      // scheduler's {id,entries} geometry format. Driver conversion is explicit.
      return Object.freeze({ context, tensorPlan, recipeMatrices: exported.matrices, replayIdentity: state.expectedReplay });
    }),
    // There is deliberately no raw numerical-receipt acceptance method here:
    // it would bypass read-pinned checkpoint and retained tensor FILE checks.
    // The generic bindVerifiedReplayExports helper remains numerical-only;
    // only this combined source path can install exports in this adapter.
    replayPointCheckpoint: (checkpoint, replayLimits) => guard.run(() => {
      need(built && !compared && state.verifiedExports === null, "fresh point plan before checkpoint replay");
      live.current?.requireComplete();
      // Lazy require avoids a module initialization cycle; no source work is
      // run by module loading. Only the actual bounded file reader brands it.
      const { pointCheckpointIdentity, replayPointCheckpoint } = require("./a68-point-checkpoint");
      const pinned = pointCheckpointIdentity(checkpoint);
      need(pinned.declaration.contextId === state.expectedReplay.contextId, "same source point checkpoint");
      const observed = snapshotCanonicalMetadata(pinned.checkpoint.metadata["background/geometry"], cap.metadata);
      live.current?.bindRecorded("background/geometry", observed.value);
      const geometryComparison = compareRecordedGeometry(a, binding, observed.value, cap.comparison);
      const replay = replayPointCheckpoint(checkpoint, state.expectedReplay, replayLimits, specification => closure.resolveLeaf(specification));
      state.verifiedExports = bindVerifiedReplayExports(replay.exportReceipt, state.expectedReplay, cap.metadata); compared = true;
      const { createPointCompletionLedger } = require("./a68-point-completion");
      state.completion = createPointCompletionLedger({ root: pinned.root, contextId: prefix, graph: pinned.pins.graph,
        backgroundMetadata: pinned.pins.metadata, metadataPath: prefix + "/metadata.json" }, cap.metadata, guard.snapshot);
      health.bindLedger(state.completion); state.backgroundRoot = pinned.root; live.current?.finish(); replayed = true;
      return freeze({ geometryComparison, replay, scope: { canonicalExports: 27, checkpointNumericallyReplayed: true,
        expandedTensorArtifactsVerified: replay.expandedArtifacts.expandedTensorArtifactsVerified, pointTraversalComplete: false, upstreamCertificateProofEstablished: false, scientificExecutionAuthorized: false } });
    }),
    acceptPointCompletion: token => guard.run(() => {
      need(state.completion && state.verifiedExports, "accepted numerical background before point completion");
      const completion = state.completion.complete(token);
      return freeze({ completion, scope: { pointTraversalComplete: true, samePointChildReplays: 350,
        upstreamCertificateProofEstablished: false, full705ContextAcceptance: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false } });
    }),
    resolveLeaf: specification => guard.run(() => closure.resolveLeaf(specification)),
    resolveSourceScalar: name => guard.run(() => { need(name === "phase626/certifiedEpsilon", "closed source scalar binding"); return polynomialProof.epsilon; }),
    snapshot: () => {
      const completion = state.completion?.snapshot() ?? null;
      return freeze({ failed: guard.snapshot().failed || completion?.failed === true || live.current?.snapshot().failed === true, recordedGeometryCompared: compared,
        pointRecipeBuilt: built, numericalReplayAccepted: state.verifiedExports !== null, pointCompletion: completion,
        leaves: closure.snapshot(), geometry: exported.usage, liveGeometry: live.current?.snapshot() ?? null });
    },
    scope: Object.freeze({ privateSourceAndPolynomialBrandsRequired: true, completeBaselineMatrixRoles: 73, nativeFirstJetZeroAssumed: false,
      upstreamCertificateProofEstablished: false, numericalPointReplayEstablished: false, canonicalBaselineImportsEstablished: false,
      liveParentImmutabilityProved: false, full705ContextDriver: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false }) });
  pointIdentities.set(adapter, { a, binding, source, polynomialProof, exported, guard, state, identityToken });
  adapterIdentities.set(adapter, () => Object.freeze({ kind: "point", contextId: prefix, parentIdentity: null, identity: identityToken,
    prepared: built && !compared, replayed, completed: state.completion?.snapshot().completed === true && !guard.snapshot().failed,
    health: health.health }));
  return adapter;
}

function createSourceGermAdapter(a, pointAdapter, binding, limits) {
  const parent = pointIdentities.get(pointAdapter); need(parent && parent.a === a && !pointAdapter.snapshot().failed, "authentic same-session point adapter");
  const identity = boundGeometryIdentity(a, binding), source = sourceGeometryIdentity(a, identity.rebuilt);
  need(source.kind === "germ" && source.point === parent.source.point && source.baseline === boundGeometryIdentity(a, parent.binding).rebuilt, "exact independently compared baseline parent");
  const cap = adapterLimits(limits, true), exported = exportRecipeGeometry(a, binding, cap.export, parent.binding), fields = createSourceGermFieldResolver(a, binding, cap.fields);
  const prefix = `point${source.point}/m${source.metricBasis}_j${source.jetIndex}`, adaptedLeaf = prefix + "/geometry/deltaCurvatureAdapted", oracleLeaf = prefix + "/geometry/deltaCurvatureOracle";
  // Distinct route identities even though the independently expected tensors
  // coincide. No observed hash deduplication or fitted coefficient constant.
  const closure = createCanonicalLeafClosure([adaptedLeaf, oracleLeaf].map(id => ({ id, degree: 2,
    source: binding.source.path + "#independent-source-spin-curvature-variation", tensor: identity.rebuilt.spinCurvatureVariation })), cap.leaf);
  const live = liveGeometryState();
  const guard = stateGuard(live.abort, live.healthy); let compared = false, built = false, replayed = false, resolutions = 0;
  let baselineBinding = null, builtPlan = null;
  const metadataUsage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  function capture(value) {
    const remaining = { nodes: cap.metadata.nodes - metadataUsage.nodes, arraySlots: cap.metadata.arraySlots - metadataUsage.arraySlots,
      stringCharacters: cap.metadata.stringCharacters - metadataUsage.stringCharacters, maxDepth: cap.metadata.maxDepth };
    need(remaining.nodes > 0 && remaining.arraySlots > 0 && remaining.stringCharacters > 0, "cumulative germ metadata snapshot quota");
    const copy = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(metadataUsage)) metadataUsage[key] += copy.usage[key];
    need(!guard.snapshot().failed, "germ snapshot reentry"); return copy;
  }
  const parentHealthy = () => need(!pointAdapter.snapshot().failed, "point adapter failure propagates to germ");
  function compareGeometry(observed) {
    parentHealthy(); need(!compared, "one complete germ metadata comparison"); const copy = capture(observed);
    live.current?.bindRecorded("geometry/geometry", copy.value);
    const comparison = compareRecordedGeometry(a, binding, copy.value, cap.comparison); compared = true;
    return Object.freeze({ comparison, snapshotUsage: copy.usage });
  }
  function resolveLeaf(specification) {
    parentHealthy(); need(!guard.snapshot().failed && resolutions < cap.leaf.resolutions, "healthy germ leaf resolution ceiling"); resolutions++;
    specification = shape(specification, ["id", "degree", "source", "sha256"]);
    if (closure.leaves.some(leaf => leaf.id === specification.id)) return closure.resolveLeaf(specification);
    need(built && baselineBinding, "built germ baseline imports"); return baselineBinding.resolveLeaf(specification);
  }
  const identityToken = Object.freeze({});
  const adapter = Object.freeze({ contextId: prefix, geometryLeaves: closure.leaves,
    configureLiveGeometry: limits => guard.run(() => {
      parentHealthy(); need(built && !compared && !replayed && live.current === null, "one live geometry configuration after germ recipe before observations");
      live.configure(a, [{ metadataPath: "geometry/geometry", binding }], limits);
    }),
    precheckGeometry: (metadataPath, observed) => guard.run(() => {
      parentHealthy(); need(live.current && built && !compared && !replayed, "configured pending germ live geometry");
      return live.current.precheck(metadataPath, observed);
    }),
    compareGeometryMetadata: observed => guard.run(() => {
      need(live.current === null, "configured live geometry requires pinned checkpoint comparison"); return compareGeometry(observed);
    }),
    recipeInputs: () => guard.run(() => {
      parentHealthy(); need(compared && pointAdapter.snapshot().recordedGeometryCompared, "complete parent and germ metadata comparisons");
      return freeze({ background: { geometry: parent.exported.descriptor, validateGeometry: parent.exported.validateGeometry },
        germ: { geometry: exported.descriptor, validateGeometry: exported.validateGeometry, adaptedLeaf, oracleLeaf },
        recipeMatrices: [...parent.exported.matrices, ...exported.matrices], constants: fixedConstants,
        pending: { canonicalBaselineLeafCount: 27, reason: "Supplemental geometry only. buildGermRecipe requires the exact accepted numerical point-replay receipt; this object is NOT a complete executable germ recipe." } });
    }),
    buildGermRecipe: options => guard.run(() => {
      // Independent planning must not rely on an unpinned producer geometry
      // comparison. The combined replay below compares the actual pinned file.
      parentHealthy(); need(parent.state.verifiedExports && !built, "verified point exports before one independent germ recipe");
      options = shape(options, ["retention", "wardRetention", "nodeLimit", "markLimit"]);
      need(integer(options.nodeLimit, 1) && integer(options.markLimit, 1), "explicit germ recipe ceilings");
      const retention = capture(options.retention).value;
      const wardRetention = options.wardRetention === null ? null : capture(options.wardRetention).value;
      need(closure.leaves.length + 27 <= cap.leaf.leaves, "all29 germ leaf declarations admitted");
      baselineBinding = createVerifiedBaselineLeafBindings(parent.state.verifiedExports, prefix,
        { leaves: cap.leaf.leaves - closure.leaves.length, stringCharacters: cap.leaf.stringCharacters - closure.snapshot().stringCharacters, resolutions: cap.leaf.resolutions });
      const { leafIds, leaves } = baselineBinding;
      const recipeMatrices = [...parent.exported.matrices, ...exported.matrices];
      const recipe = new MixedRecipe({ leaves: [...leaves, ...closure.leaves], constants: fixedConstants, matrices: recipeMatrices, nodeLimit: options.nodeLimit, markLimit: options.markLimit });
      const context = buildAuditGermContext(recipe, { background: { geometry: parent.exported.descriptor, validateGeometry: parent.exported.validateGeometry, leafIds },
        germ: { geometry: exported.descriptor, validateGeometry: exported.validateGeometry, adaptedLeaf, oracleLeaf }, retention, wardRetention });
      const tensorPlan = recipe.Finish(); built = true;
      // Lazy import avoids initialization cycles with generic snapshot/closure
      // helpers. This binds metadata DECLARATIONS, not observed acceptance.
      const { contextMetadataPlan } = require("./a68-context-metadata");
      builtPlan = freeze({ context, tensorPlan, recipeMatrices, baselineLeafIds: leafIds, metadataPlan: contextMetadataPlan(context, tensorPlan),
        replayIdentity: { contextId: prefix, tensorPlan, namedRoots: context.scalarRoots, geometry: context.geometry,
          checks: context.checks, domainChecks: context.domainChecks, error: context.error, requests: [] },
        scope: { canonicalBaselineImportsFromVerifiedPoint: true, sharedPointExportTensorArrays: true, germNumericalReplayComplete: false,
          full705ProspectivePlansAdmitted: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false } });
      return builtPlan;
    }),
    replayContextCheckpoint: (checkpoint, replayLimits) => guard.run(() => {
      parentHealthy(); need(builtPlan && !compared && !replayed && parent.state.verifiedExports, "fresh independently planned germ before pinned replay");
      live.current?.requireComplete();
      const { contextCheckpointIdentity } = require("./a68-context-checkpoint"), pinned = contextCheckpointIdentity(checkpoint);
      need(parent.state.completion && pinned.root === parent.state.backgroundRoot, "same accepted point output root before child replay");
      const { replayContextCheckpoint } = require("./a68-context-replay"); let geometryComparison;
      const replay = replayContextCheckpoint(checkpoint, builtPlan.metadataPlan, replayLimits, {
        compareGeometryMetadata: observed => { const menu = shape(observed, ["geometry/geometry"]); geometryComparison = compareGeometry(menu["geometry/geometry"]); },
        resolveLeaf,
        resolveGeometryField: name => { parentHealthy(); need(!guard.snapshot().failed && compared, "healthy pinned geometry before field resolution"); return fields.resolveGeometryField(name); },
        resolveSourceScalar: name => { parentHealthy(); need(!guard.snapshot().failed, "healthy pinned germ scalar resolution"); return pointAdapter.resolveSourceScalar(name); }
      });
      parentHealthy(); need(!guard.snapshot().failed && compared, "healthy complete germ checkpoint");
      // Only this successful source-geometry/numerical/expanded-file path can
      // mutate the authentic parent's PRIVATE ledger. Retain compact pins and
      // standalone sticky guard state, never the child adapter or replay graph.
      parent.state.completion.registerChild({ root: pinned.root, contextId: prefix, graph: pinned.pins.graph,
        metadata: pinned.pins.metadata }, guard.snapshot);
      live.current?.finish(); replayed = true;
      return freeze({ geometryComparison, replay, scope: { checkpointNumericallyReplayed: true, recordedGeometryCompared: true,
        expandedTensorArtifactsVerified: true, upstreamCertificateProofEstablished: false, full705ProspectivePlansAdmitted: false,
        independentContextAccepted: false, scientificExecutionAuthorized: false } });
    }),
    resolveLeaf: specification => guard.run(() => resolveLeaf(specification)),
    resolveGeometryField: name => guard.run(() => { parentHealthy(); need(compared, "compare complete germ metadata first"); return fields.resolveGeometryField(name); }),
    resolveSourceScalar: name => guard.run(() => { parentHealthy(); return pointAdapter.resolveSourceScalar(name); }),
    snapshot: () => freeze({ failed: guard.snapshot().failed || live.current?.snapshot().failed === true, recordedGeometryCompared: compared, germRecipeBuilt: built, checkpointNumericallyReplayed: replayed,
      liveGeometry: live.current?.snapshot() ?? null,
      metadataUsage: { ...metadataUsage }, baselineImports: baselineBinding?.snapshot() ?? null, resolutions, leaves: closure.snapshot(), geometry: exported.usage, fields: fields.snapshot() }),
    scope: Object.freeze({ privateSourceAndParentBrandsRequired: true, completeGermMatrixRoles: 240, baselineReplayReceiptRequired: true,
      canonicalBaselineImportsEstablished: false, completeExecutableGermRecipe: false, upstreamCertificateProofEstablished: false,
      liveParentImmutabilityProved: false, full705ContextDriver: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false }) });
  adapterIdentities.set(adapter, () => Object.freeze({ kind: "germ", contextId: prefix, parentIdentity: parent.identityToken, identity: identityToken,
    prepared: built && !compared, replayed, completed: false, health: guard.snapshot }));
  return adapter;
}

module.exports = { createCanonicalLeafClosure, snapshotCanonicalMetadata, bindVerifiedReplayExports, createVerifiedBaselineLeafBindings, createSourcePointAdapter, createSourceGermAdapter, sourceAdapterIdentity };
