"use strict";

// Bind independently evaluated scalar roots/checks/domains/error formulas to
// the complete C# metadata menu. This is NOT a geometry or source certificate,
// nor a numerical evaluator: the actual replay must drive these callbacks.
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { compileAuditConsumerSchedule, ERROR_FIELDS } = require("./a68-audit-consumer-schedule");
const need = (ok, why) => { if (!ok) throw new Error("A68 context metadata: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const integer = (x, min = 0) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= min;
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered data fields");
  return Object.fromEntries(names.map(name => { const p = Object.getOwnPropertyDescriptor(value, name); need(p && Object.hasOwn(p, "value"), "own data field " + name); return [name, p.value]; }));
}
function keySet(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && Object.keys(value).length === names.length && names.every(k => Object.hasOwn(value, k)), "complete metadata field set");
}
function contextMetadataPlan(context, tensorPlan) {
  // Called on independently built recipe data, not on a producer graph.
  const germ = /^point[01]\/m[0-9]_j(?:[0-9]|[12][0-9]|3[0-4])$/.test(context.contextId);
  need(germ || ["diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"].includes(context.contextId), "germ or fixed diagnostic context");
  return { contextId: context.contextId, tensorPlan, namedRoots: context.scalarRoots, geometry: context.geometry,
    checks: context.checks, domainChecks: context.domainChecks, error: germ ? context.error : null,
    structured: context.structured ?? [], scalarArrays: context.scalarArrays ?? [],
    geometryPaths: germ ? ["geometry/geometry"] : (context.geometryArtifacts ?? []).map(g => g.metadataPath) };
}
function createContextMetadataComparator(expected, observed, limits) {
  limits = fields(limits, ["snapshot", "scalarSchedule", "consumerSchedule", "results"]);
  const snapshotCap = fields(limits.snapshot, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(snapshotCap).every(n => integer(n, 1)) && snapshotCap.maxDepth <= 64, "explicit cumulative snapshot limits");
  const used = { nodes: 0, arraySlots: 0, stringCharacters: 0 };
  function capture(value) {
    const remaining = { nodes: snapshotCap.nodes - used.nodes, arraySlots: snapshotCap.arraySlots - used.arraySlots,
      stringCharacters: snapshotCap.stringCharacters - used.stringCharacters, maxDepth: snapshotCap.maxDepth };
    need(remaining.nodes > 0 && remaining.arraySlots > 0 && remaining.stringCharacters > 0, "cumulative snapshot quota");
    const result = snapshotCanonicalMetadata(value, remaining); for (const key of Object.keys(used)) used[key] += result.usage[key]; return result.value;
  }
  const frozen = capture({ expected, observed, scalarSchedule: limits.scalarSchedule, consumerSchedule: limits.consumerSchedule, results: limits.results });
  expected = fields(frozen.expected, ["contextId", "tensorPlan", "namedRoots", "geometry", "checks", "domainChecks", "error", "structured", "scalarArrays", "geometryPaths"]); observed = frozen.observed;
  const resultCap = fields(frozen.results, ["rationalCharacters", "totalCharacters"]);
  need(Object.values(resultCap).every(n => integer(n, 1)) && resultCap.rationalCharacters <= 16384, "explicit result character ceilings");
  need(typeof expected.contextId === "string" && expected.contextId.trim() && ["structured", "scalarArrays", "geometryPaths"].every(k => Array.isArray(expected[k])), "complete expected context metadata declarations");
  const schedule = compileAuditConsumerSchedule({ tensorPlan: expected.tensorPlan, namedRoots: expected.namedRoots, geometry: expected.geometry,
    checks: expected.checks, domainChecks: expected.domainChecks, error: expected.error, scheduleLimits: frozen.scalarSchedule, limits: frozen.consumerSchedule });
  const roots = new Map(schedule.scalarSchedule.rootComparisons.map(root => [root.name, root]));
  const consumerRoots = new Set(schedule.consumers.flatMap(c => c.scalarRoots)), targets = new Map(), topKeys = new Set();
  let resultCharacters = 0;
  function rational(value) {
    need(typeof value === "string" && value.length > 0 && value.length <= resultCap.rationalCharacters && value.length <= resultCap.totalCharacters - resultCharacters, "prospective rational/result character quota");
    resultCharacters += value.length;
    need(/^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(value), "canonical rational syntax");
    const [ns, ds] = value.split("/"), n = BigInt(ns), d = ds === undefined ? 1n : BigInt(ds); let a = n < 0n ? -n : n, b = d;
    while (b) [a, b] = [b, a % b]; need(value !== "-0" && a === 1n && (ds === undefined || d !== 1n), "reduced canonical rational"); return value;
  }
  function path(name) { need(typeof name === "string" && name.trim() && !topKeys.has(name), "unique literal metadata path"); topKeys.add(name); need(Object.hasOwn(observed, name), "required metadata path " + name); return observed[name]; }
  function bind(name, value) {
    need(roots.has(name), "metadata targets a declared scalar root"); rational(value);
    if (!targets.has(name)) targets.set(name, []); targets.get(name).push(value);
  }
  need(observed && typeof observed === "object" && !Array.isArray(observed), "metadata object");
  for (const geometry of expected.geometryPaths) { const value = path(geometry); need(value && typeof value === "object" && !Array.isArray(value), "geometry artifact object (not authentication)"); }
  for (const check of expected.checks) need(path(check.metadataPath) === true, "complete true producer check declarations, not proof");
  for (const declaration of expected.scalarArrays) {
    const row = fields(declaration, ["name", "metadataPath", "roots"]); need(row.metadataPath === "scalars/" + row.name && Array.isArray(row.roots), "literal scalar array path");
    const values = path(row.metadataPath); need(Array.isArray(values) && values.length === row.roots.length, "complete scalar array length");
    row.roots.forEach((root, i) => bind(root, values[i]));
  }
  for (const declaration of expected.structured) {
    const row = fields(declaration, ["category", "name", "metadataPath", "fields", "scalarFields", "booleanFields"]);
    need(row.metadataPath === `structured/${row.category}/${row.name}` && Array.isArray(row.fields), "literal structured metadata path");
    const values = path(row.metadataPath), scalarKeys = [], booleanKeys = [], allKeys = [], seen = new Set();
    for (const raw of row.fields) {
      const field = fields(raw, ["category", "name", "degree", "expanded", "length"]);
      need(typeof field.name === "string" && !seen.has(field.name), "unique structured field"); seen.add(field.name);
      if (field.category === "Tensor") continue; // These are retained/DAG marks, not scalar metadata.
      need(["Scalar", "ScalarArray", "Boolean"].includes(field.category), "closed structured scalar category");
      allKeys.push(field.name); need(values && Object.hasOwn(values, field.name), "all structured scalar fields");
      if (field.category === "Boolean") {
        booleanKeys.push(field.name); need(typeof row.booleanFields[field.name] === "boolean" && values[field.name] === row.booleanFields[field.name], "exact independent route/control flag");
      } else {
        scalarKeys.push(field.name); const root = row.scalarFields[field.name], value = values[field.name];
        if (field.category === "Scalar") bind(root, value);
        else {
          need(integer(field.length, 1) && Array.isArray(root) && root.length === field.length && Array.isArray(value) && value.length === field.length, "complete structured scalar array");
          root.forEach((name, i) => bind(name, value[i]));
        }
      }
    }
    keySet(row.scalarFields, scalarKeys); keySet(row.booleanFields, booleanKeys); keySet(values, allKeys);
  }
  if (expected.error !== null) {
    const error = path("error"); keySet(error, ERROR_FIELDS);
    for (const name of ERROR_FIELDS) {
      if (name === "nativeFirstJetNorms") { need(Array.isArray(error[name]) && error[name].length === 14, "all14 recorded native jet norms"); error[name].forEach(rational); }
      else rational(error[name]);
    }
  }
  keySet(observed, [...topKeys]);
  for (const name of roots.keys()) need(targets.has(name) || consumerRoots.has(name), "every unretained root must feed an independent consumer");
  const seenRoots = new Set(), seenConsumers = new Set(); let failed = false, busy = false, finished = false;
  function guard(action) {
    if (failed || busy || finished) { failed = true; throw new Error("A68 context metadata: failed, reentrant or closed comparator"); } busy = true;
    try { const result = action(); need(!failed, "swallowed reentry"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  return Object.freeze({
    // The driver must authenticate these geometry artifacts separately; this
    // module deliberately cannot mint a geometry-accepted flag.
    geometryMetadata: Object.freeze(Object.fromEntries(expected.geometryPaths.map(name => [name, observed[name]]))),
    compareRoot: (name, value, descriptor) => guard(() => {
      const copy = capture({ name, value, descriptor }); need(!failed, "snapshot reentry before scalar comparison");
      need(roots.has(copy.name) && !seenRoots.has(copy.name) && same(copy.descriptor, roots.get(copy.name)), "exact unique independently scheduled scalar callback");
      rational(copy.value); need((targets.get(copy.name) ?? []).every(expectedValue => copy.value === expectedValue), "recomputed scalar disagrees with recorded metadata");
      seenRoots.add(copy.name); return true;
    }),
    compareConsumer: (consumer, value) => guard(() => {
      const copy = capture({ consumer, value }); need(!failed, "snapshot reentry before consumer comparison");
      const id = copy.consumer?.id, expectedConsumer = schedule.consumers[id];
      need(integer(id) && expectedConsumer && !seenConsumers.has(id) && same(copy.consumer, expectedConsumer), "exact unique independently scheduled consumer callback");
      if (expectedConsumer.kind === "error") {
        if (expectedConsumer.name === "nativeFirstJetNorms") { need(Array.isArray(copy.value) && copy.value.length === 14, "all14 recomputed native jet norms"); copy.value.forEach(rational); }
        else rational(copy.value);
        need(same(copy.value, observed.error[expectedConsumer.name]), "recomputed error formula disagrees with metadata");
      } else need(copy.value === true && (expectedConsumer.kind === "domain" || observed[expectedConsumer.descriptor.metadataPath] === copy.value), "recomputed predicate must pass and agree with metadata");
      seenConsumers.add(id); return true;
    }),
    finish: () => guard(() => {
      need(seenRoots.size === roots.size && seenConsumers.size === schedule.consumers.length, "all independent roots and consumers compared"); finished = true;
      return Object.freeze({ contextId: expected.contextId, roots: seenRoots.size, consumers: seenConsumers.size, recordedRootBindings: [...targets.values()].reduce((n, v) => n + v.length, 0),
        scope: Object.freeze({ geometryAuthenticatedHere: false, sourceAuthenticityEstablished: false, numericalEvaluationPerformedHere: false,
          totalProcessMemoryProved: false, scientificExecutionAuthorized: false }) });
    }),
    snapshot: () => Object.freeze({ failed, finished, roots: seenRoots.size, consumers: seenConsumers.size, resultCharacters, snapshotUsage: Object.freeze({ ...used }) })
  });
}
module.exports = { contextMetadataPlan, createContextMetadataComparator };
