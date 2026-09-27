"use strict";

// Source binding for the three fixed diagnostic RECIPES. Genuine geometry
// paths remain UNCALLED pending FIRST. A caller cannot substitute a copied
// source hash or a test geometry validator for the private source bindings.
// The combined checkpoint path requires pinned geometry, numerical metadata
// and retained files; genuine source-positive execution remains unauthorized.
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const { sourceGeometryIdentity } = require("./a68-source-geometry");
const { geometryMatrixLayout, exportRecipeGeometry } = require("./a68-geometry-recipe-binding");
const { compareRecordedGeometry } = require("./a68-geometry-metadata");
const { createCanonicalLeafClosure, snapshotCanonicalMetadata } = require("./a68-source-orchestration");
const { MixedRecipe } = require("./a68-mixed-recipe");
const { buildGradeTenDiagnosticRecipe, buildAccelerationDiagnosticRecipe } = require("./a68-diagnostics-recipe");
const { buildSecondJetDiagnosticRecipe } = require("./a68-second-jet-recipe");
const { contextMetadataPlan } = require("./a68-context-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 diagnostic source: " + why); };
const integer = (n, min = 0) => Number.isSafeInteger(n) && !Object.is(n, -0) && n >= min;
const freeze = x => { if (x && typeof x === "object" && !Object.isFrozen(x)) { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const constants = Object.freeze(["0", "1", "-1", "-1/2", "1/2", "1/3", "907712", "-2", "-24", "-3/32"]);
const geometryRoot = "studies/phase621_induced_metric_full_variation_scope_audit_001/output";
const adapterIdentities = new WeakMap();
function diagnosticAdapterIdentity(adapter) {
  const read = adapterIdentities.get(adapter); need(read, "private diagnostic adapter identity required"); return read();
}
// Keep retained health/helper state outside the diagnostic constructor's
// environment. The helper clears bindings/commitments after finish/abort;
// its bounded counters, slot paths, limits and detached source-algebra health
// dependency are still retained health data requiring explicit admission.
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
function diagnosticGuard(onFailure = () => {}, healthy = () => true) {
  let failed = false, busy = false;
  const check = () => { if (!healthy()) { failed = true; onFailure(); } return !failed; };
  return Object.freeze({
    run(action) {
      if (failed || busy) { failed = true; onFailure(); throw new Error("A68 diagnostic source: failed or reentrant adapter"); } busy = true;
      try { need(check(), "healthy diagnostic dependencies"); const result = action(); need(check(), "swallowed reentry"); return result; }
      catch (error) { failed = true; onFailure(); throw error; } finally { busy = false; }
    },
    get failed() { check(); return failed; },
    snapshot: () => { check(); return Object.freeze({ failed }); }
  });
}
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && JSON.stringify(Reflect.ownKeys(value)) === JSON.stringify(names), "closed ordered fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
function caps(value, names) { value = fields(value, names); need(Object.values(value).every(n => integer(n, 1)), "explicit positive resource quotas"); return Object.freeze(value); }
function ownArray(value, count) {
  need(Array.isArray(value) && Object.getOwnPropertyDescriptor(value, "length")?.value === count && Reflect.ownKeys(value).length === count + 1, "complete fixed binding array");
  return Array.from({ length: count }, (_, i) => { const d = Object.getOwnPropertyDescriptor(value, String(i)); need(d && Object.hasOwn(d, "value"), "own binding array index"); return d.value; });
}
function diagnosticSourceLayout(diagnostic) {
  need(["grade10", "acceleration", "secondJets"].includes(diagnostic), "exact diagnostic name");
  const contextId = "diagnostic/" + diagnostic, point = diagnostic === "grade10" ? 0 : diagnostic === "acceleration" ? 1 : null;
  const geometries = [], leaves = [];
  function geometry(kind, metadataPath, metricBasis = null, jetIndex = null) {
    const sourcePath = kind === "baseline" ? geometryRoot + "/induced_metric_full_variation_scope_audit_summary.json" :
      `${geometryRoot}/shards/jet_p${point}_m${String(metricBasis).padStart(2, "0")}_j${String(jetIndex).padStart(2, "0")}.json`;
    const prefix = metadataPath.slice(0, -"geometry".length), suffix = kind === "baseline" ? "#independent-source-spin-curvature" : "#independent-source-spin-curvature-variation";
    const leafIds = (kind === "baseline" ? ["curvature"] : ["deltaCurvatureAdapted", "deltaCurvatureOracle"]).map(name => {
      const id = contextId + "/geometry/" + prefix + name; leaves.push({ id, degree: 2, source: sourcePath + suffix }); return id;
    });
    geometries.push({ kind, point, metricBasis, jetIndex, metadataPath, sourcePath, leafIds });
  }
  if (point !== null) {
    geometry("baseline", "background/geometry");
    if (diagnostic === "grade10") for (let m = 0; m < 4; m++) geometry("germ", `basis${m}/geometry`, m, 4);
    else geometry("germ", "germ/geometry", 0, 10);
  }
  const matrixSlots = geometries.reduce((n, g) => n + geometryMatrixLayout(g.kind).length, 0);
  return freeze({ diagnostic, contextId, point, geometries, leaves, constants: [...constants],
    census: { geometries: geometries.length, leaves: leaves.length, matrixSlots, exportCoordinateVisits: matrixSlots * 196,
      comparisonCoordinateVisits: geometries.reduce((n, g) => n + (g.kind === "baseline" ? 129556 : 330260), 0) } });
}
function createDiagnosticSourceAdapter(a, diagnostic, bindings, limits) {
  const layout = diagnosticSourceLayout(diagnostic);
  limits = fields(limits, ["leaf", "export", "metadata", "comparison"]);
  const cap = Object.freeze({
    leaf: caps(limits.leaf, ["leaves", "records", "stringCharacters", "wireBytes", "rationalCharacters", "resolutions"]),
    export: caps(limits.export, ["matrixSlots", "coordinateVisits", "entryRecords"]),
    metadata: caps(limits.metadata, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]),
    comparison: caps(limits.comparison, ["coordinateVisits", "sparseRecords", "tensorRecords", "rationalCharacters"])
  });
  need(cap.metadata.maxDepth <= 64 && cap.leaf.rationalCharacters <= 16384, "bounded snapshot and rational domains");
  need(layout.census.leaves <= cap.leaf.leaves && layout.census.matrixSlots <= cap.export.matrixSlots &&
    layout.census.exportCoordinateVisits <= cap.export.coordinateVisits && layout.census.comparisonCoordinateVisits <= cap.comparison.coordinateVisits, "complete diagnostic prospective resource census");
  bindings = fields(bindings, ["baseline", "germs"]); const germs = ownArray(bindings.germs, Math.max(0, layout.geometries.length - 1));
  const bound = [], rebuilt = [];
  if (layout.point === null) need(bindings.baseline === null && a === null, "secondJets has no geometry or coefficient-algebra input");
  else {
    bound.push(bindings.baseline, ...germs);
    // Authenticate ALL fixed identities and their exact common baseline before
    // exporting any matrices or copying any source tensor coefficients.
    for (let i = 0; i < bound.length; i++) {
      const identity = boundGeometryIdentity(a, bound[i]), source = sourceGeometryIdentity(a, identity.rebuilt), expected = layout.geometries[i];
      need(source.kind === expected.kind && source.point === expected.point && bound[i].source.path === expected.sourcePath, "exact diagnostic source kind/point/retained path");
      if (i > 0) need(source.metricBasis === expected.metricBasis && source.jetIndex === expected.jetIndex && source.baseline === rebuilt[0], "exact ordered diagnostic germ and common baseline");
      rebuilt.push(identity.rebuilt);
    }
  }
  const exportUsage = { matrixSlots: 0, coordinateVisits: 0, entryRecords: 0 }, comparisonUsage = { coordinateVisits: 0, sparseRecords: 0, tensorRecords: 0, rationalCharacters: 0 };
  const metadataUsage = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  function capture(value) {
    const remaining = { nodes: cap.metadata.nodes - metadataUsage.nodes, arraySlots: cap.metadata.arraySlots - metadataUsage.arraySlots,
      stringCharacters: cap.metadata.stringCharacters - metadataUsage.stringCharacters, maxDepth: cap.metadata.maxDepth };
    need(remaining.nodes > 0 && remaining.arraySlots > 0 && remaining.stringCharacters > 0, "cumulative metadata snapshot quota");
    const snapshot = snapshotCanonicalMetadata(value, remaining);
    for (const name of Object.keys(metadataUsage)) metadataUsage[name] += snapshot.usage[name]; return snapshot.value;
  }
  const exports = bound.map((binding, i) => {
    const count = geometryMatrixLayout(layout.geometries[i].kind).length;
    const value = exportRecipeGeometry(a, binding, { matrixSlots: count, coordinateVisits: count * 196, entryRecords: cap.export.entryRecords - exportUsage.entryRecords }, i === 0 ? null : bound[0]);
    for (const key of Object.keys(exportUsage)) exportUsage[key] += value.usage[key]; return value;
  });
  const entries = layout.geometries.flatMap((g, i) => g.leafIds.map(id => {
    const declared = layout.leaves.find(leaf => leaf.id === id);
    return { ...declared, tensor: i === 0 ? rebuilt[i].spinCurvature : rebuilt[i].spinCurvatureVariation };
  }));
  const closure = createCanonicalLeafClosure(entries, cap.leaf), recipeMatrices = Object.freeze(exports.flatMap(value => value.matrices));
  let built = false, compared = false, replayed = false, builtPlan = null;
  const live = liveGeometryState();
  const guardState = diagnosticGuard(live.abort, live.healthy), guard = guardState.run, identityToken = Object.freeze({});
  function compareGeometry(observed) {
    need(built && !compared, "independent recipe before one complete diagnostic geometry comparison");
    observed = fields(capture(observed), layout.geometries.map(g => g.metadataPath)); need(!guardState.failed, "snapshot reentry before geometry comparison");
    for (let i = 0; i < bound.length; i++) {
      live.current?.bindRecorded(layout.geometries[i].metadataPath, observed[layout.geometries[i].metadataPath]);
      const remaining = Object.fromEntries(Object.keys(comparisonUsage).map(key => [key, cap.comparison[key] - comparisonUsage[key]]));
      const usage = compareRecordedGeometry(a, bound[i], observed[layout.geometries[i].metadataPath], remaining);
      for (const key of Object.keys(comparisonUsage)) comparisonUsage[key] += usage[key];
    }
    need(comparisonUsage.coordinateVisits === layout.census.comparisonCoordinateVisits, "complete diagnostic metadata coordinate census"); compared = true;
    return freeze({ geometries: bound.length, usage: { ...comparisonUsage }, numericalReplayComplete: false, scientificExecutionAuthorized: false });
  }
  const adapter = Object.freeze({ contextId: layout.contextId, leaves: closure.leaves,
    buildRecipe: options => guard(() => {
      need(!built, "one diagnostic recipe"); options = fields(capture(options), ["retention", "nodeLimit", "markLimit"]); need(!guardState.failed, "snapshot reentry before recipe construction");
      need(integer(options.nodeLimit, 1) && integer(options.markLimit, 1), "explicit recipe limits");
      const recipe = new MixedRecipe({ leaves: closure.leaves, constants: layout.constants, matrices: recipeMatrices, nodeLimit: options.nodeLimit, markLimit: options.markLimit });
      let context;
      if (diagnostic === "secondJets") context = buildSecondJetDiagnosticRecipe(recipe, { retention: options.retention });
      else {
        const background = { geometry: exports[0].descriptor, validateGeometry: exports[0].validateGeometry, curvatureLeaf: layout.geometries[0].leafIds[0] };
        const germs = exports.slice(1).map((value, i) => ({ geometry: value.descriptor, validateGeometry: value.validateGeometry,
          adaptedLeaf: layout.geometries[i + 1].leafIds[0], oracleLeaf: layout.geometries[i + 1].leafIds[1] }));
        context = diagnostic === "grade10" ? buildGradeTenDiagnosticRecipe(recipe, { background, germs, retention: options.retention }) :
          buildAccelerationDiagnosticRecipe(recipe, { background, germ: germs[0], retention: options.retention });
      }
      const tensorPlan = recipe.Finish(); built = true;
      builtPlan = freeze({ context, tensorPlan, recipeMatrices, metadataPlan: contextMetadataPlan(context, tensorPlan),
        replayIdentity: { contextId: layout.contextId, tensorPlan, namedRoots: context.scalarRoots, geometry: context.geometry,
          checks: context.checks, domainChecks: context.domainChecks, error: null, requests: [] },
        scope: { numericalReplayComplete: false, recordedGeometryCompared: compared, wholeContextMemoryProved: false, scientificExecutionAuthorized: false } });
      return builtPlan;
    }),
    configureLiveGeometry: limits => guard(() => {
      need(built && !compared && !replayed && live.current === null, "one live geometry configuration after diagnostic recipe before observations");
      live.configure(a, layout.geometries.map((g, i) => ({ metadataPath: g.metadataPath, binding: bound[i] })), limits);
    }),
    precheckGeometry: (metadataPath, observed) => guard(() => {
      need(live.current && built && !compared && !replayed, "configured pending diagnostic live geometry");
      return live.current.precheck(metadataPath, observed);
    }),
    compareGeometryMetadata: observed => guard(() => {
      need(live.current === null, "configured live geometry requires pinned checkpoint comparison"); return compareGeometry(observed);
    }),
    replayContextCheckpoint: (checkpoint, replayLimits) => guard(() => {
      need(builtPlan && !compared && !replayed, "fresh independently planned diagnostic before pinned replay");
      live.current?.requireComplete();
      const { replayContextCheckpoint } = require("./a68-context-replay");
      let geometryComparison;
      const replay = replayContextCheckpoint(checkpoint, builtPlan.metadataPlan, replayLimits, {
        compareGeometryMetadata: observed => { need(!guardState.failed, "healthy diagnostic geometry callback"); geometryComparison = compareGeometry(observed); },
        resolveLeaf: specification => { need(!guardState.failed && compared, "healthy geometry comparison before diagnostic leaf"); return closure.resolveLeaf(specification); },
        resolveGeometryField: () => { throw new Error("diagnostic recipes declare no external geometry-field predicates"); },
        resolveSourceScalar: () => { throw new Error("diagnostic recipes declare no external source scalars"); }
      });
      need(!guardState.failed && compared, "healthy complete diagnostic checkpoint"); live.current?.finish(); replayed = true;
      return freeze({ geometryComparison, replay, scope: { checkpointNumericallyReplayed: true, recordedGeometryCompared: true,
        expandedTensorArtifactsVerified: true, arbitraryEtaEstablishedByFiniteControls: false, upstreamCertificateProofEstablished: false,
        independentContextAccepted: false, scientificExecutionAuthorized: false } });
    }),
    resolveLeaf: specification => guard(() => { need(built && compared, "complete diagnostic plan and geometry before leaf replay"); return closure.resolveLeaf(specification); }),
    snapshot: () => freeze({ failed: guardState.failed || live.current?.snapshot().failed === true, recipeBuilt: built, recordedGeometryCompared: compared, checkpointNumericallyReplayed: replayed, leaves: closure.snapshot(),
      liveGeometry: live.current?.snapshot() ?? null,
      exportUsage: { ...exportUsage }, comparisonUsage: { ...comparisonUsage }, metadataUsage: { ...metadataUsage } }),
    scope: Object.freeze({ sourceGeometryRequired: layout.point !== null, privateGeometryBrandsRequired: layout.point !== null,
      diagnosticXConstructedByRecipe: layout.point !== null, numericalReplayComplete: false, upstreamCertificateProofEstablished: false,
      arbitraryEtaEstablishedByFiniteControls: false, liveParentImmutabilityProved: false, full705ContextDriver: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false })
  });
  adapterIdentities.set(adapter, () => Object.freeze({ kind: "diagnostic", contextId: layout.contextId, parentIdentity: null, identity: identityToken,
    prepared: built && !compared, replayed, completed: false, health: guardState.snapshot }));
  return adapter;
}
module.exports = { diagnosticSourceLayout, createDiagnosticSourceAdapter, diagnosticAdapterIdentity };
