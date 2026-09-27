"use strict";
// Independent manufactured combinatorics; never evaluate production menus.
const test = require("node:test"), assert = require("node:assert/strict"), fs = require("node:fs"), path = require("node:path");
const { derivePreflightInitializationCensus } = require("../a68-preflight-initialization-census");
const { PREFLIGHT_SOURCE_PINS } = require("../a68-preflight-module-manifest");
function tuples() {
  const result = [];
  for (let a = 0; a < 4; a++) for (let b = 0; b < 4; b++) for (let c = 0; c < 4; c++) for (let d = 0; d < 4; d++)
    if (a + b + c + d <= 3) result.push([a, b, c, d]);
  return result.sort((a, b) => a.reduce((x, y) => x + y, 0) - b.reduce((x, y) => x + y, 0) || a[0] - b[0] || a[1] - b[1] || a[2] - b[2]);
}

test("initialization content census is source-bound data and explicitly not resource admission", () => {
  const before = Object.keys(require.cache).sort(), c = derivePreflightInitializationCensus();
  assert.deepEqual(Object.keys(require.cache).sort(), before);
  assert.equal(c.entry, "a68-preflight-entry.js"); assert.equal(c.moduleInitializations, 23); assert.equal(c.importRequests, 66);
  assert.equal(c.sourceBytes, 261049);
  assert.deepEqual(c.sourceCommitments, PREFLIGHT_SOURCE_PINS.map(({ id, bytes, sha256 }) => ({ id, bytes, sha256 })));
  assert.equal(c.arrays, 1138); assert.equal(c.arraySlots, 6365); assert.equal(c.sets, 5); assert.equal(c.setEntries, 2127);
  assert.equal(c.records, 87); assert.equal(c.propertySlots, 402);
  assert.equal(c.weakMaps, 3); assert.equal(c.weakMapEntries, 0);
  assert.deepEqual(c.calls, { multiindices: 5, wardControlsMenu: 2, sourceContextIds: 2, prospectiveBranchTopologyEnvelope: 1, sourceContextMenu: 0, diagnosticMenu: 0, coefficientEvaluation: 0 });
  for (const [name, value] of Object.entries(c.scope)) assert.equal(value, name === "sourceSemanticContentCensus");
  assert.ok(c.omitted.includes("functionsClosuresClassesPrototypes")); assert.ok(c.omitted.includes("parserRegexCompilerJitCodeAndStack"));
  assert.ok(Object.isFrozen(c)); assert.ok(Object.isFrozen(c.ledger)); assert.ok(Object.isFrozen(c.ledger[0]));
});

test("independent bounded tuple enumeration verifies multiindex and predicate/string recurrences", () => {
  const values = tuples(), c = derivePreflightInitializationCensus(); assert.equal(values.length, 35);
  const wanted = [[1, 0, 0, 0], [0, 2, 0, 0], [0, 3, 0, 0]];
  assert.deepEqual(wanted.map(t => values.findIndex(v => v.every((x, i) => x === t[i]))), [4, 10, 24]);
  const loops = { order: 0, a: 0, b: 0, c: 0 };
  for (let n = 0; n < 4; n++) { loops.order++; for (let a = 0; a <= n; a++) { loops.a++; for (let b = 0; b <= n - a; b++) {
    loops.b++; for (let z = 0; z <= n - a - b; z++) loops.c++;
  } } }
  assert.deepEqual(loops, c.multiindexLoopBodiesPerCall);
  let comparisons = 0, strings = 0, characters = 0;
  for (let menu = 0; menu < 2; menu++) for (let point = 0; point < 2; point++) for (const v of values) {
    for (const target of wanted) {
      const a = JSON.stringify(v), b = JSON.stringify(target); comparisons++; strings += 2; characters += a.length + b.length;
      if (a === b) break;
    }
  }
  assert.equal(comparisons, c.wardPredicateComparisons);
  assert.deepEqual({ strings, characters }, c.selectedStringEvents.comparisonJson);
  assert.deepEqual(c.ledger.find(r => r.component === "sourceMenuSeparateMultiindices"),
    { component: "sourceMenuSeparateMultiindices", arrays: 1 + values.length, arraySlots: values.length + values.flat().length });
});

test("manufactured Ward result checks recursive freeze reflection before arithmetic aggregation", () => {
  const contexts = [];
  for (const point of [0, 1]) for (const jetIndex of [4, 10, 24]) contexts.push({ point, metricBasis: 0, jetIndex,
    multiindex: tuples()[jetIndex], parameters: [0, 1].map(parameter => ({ parameter, name: `p${point}_j${jetIndex}_eta${parameter}`, routes: [0, 1] })) });
  const menu = { contexts, germContexts: 6, parameterContexts: 12, routeCombinations: 24, wardEvaluations: 48, accelerationEvaluations: 24, originalEvaluations: 24 };
  let aggregates = 0, slots = 0, visits = 0, records = 0, propertySlots = 0;
  function inspect(x) { visits++; if (x && typeof x === "object") { aggregates++; const v = Object.values(x); slots += v.length;
    if (!Array.isArray(x)) { records++; propertySlots += v.length; } v.forEach(inspect); } }
  inspect(menu); assert.equal(aggregates, 44); assert.equal(slots, 139); assert.equal(visits, 140);
  assert.deepEqual(derivePreflightInitializationCensus().recordLedger[0], { component: "twoWardMenus", records: 2 * records, propertySlots: 2 * propertySlots });
  const arrays = 2 * 36 + 70 * 4 + 1 + 6 + 6 + 12 + aggregates;
  const arraySlots = 2 * 175 + 70 * 15 + 6 + 12 + 12 + 24 + slots;
  assert.deepEqual(derivePreflightInitializationCensus().ledger[0], { component: "twoWardMenusIncludingFourMultiindices", arrays: 2 * arrays, arraySlots: 2 * arraySlots });
});

test("independent ID and parameter formatting checks selected string events", () => {
  const c = derivePreflightInitializationCensus(), dynamic = [], ward = [], parameters = [];
  for (const point of [0, 1]) {
    dynamic.push(`point${point}`);
    for (let m = 0; m < 10; m++) for (let j = 0; j < 35; j++) dynamic.push(`point${point}/m${m}_j${j}`);
    for (const j of [4, 10, 24]) { ward.push(`point${point}/m0_j${j}`); for (const p of [0, 1]) parameters.push(`p${point}_j${j}_eta${p}`); }
  }
  const length = a => a.reduce((n, s) => n + s.length, 0);
  assert.equal(dynamic.length, c.selectedStringEvents.contextIds.strings); assert.equal(length(dynamic), c.selectedStringEvents.contextIds.characters);
  assert.equal(length([...dynamic, "diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"]), c.selectedStringEvents.contextIds.retainedWithDiagnosticLiterals);
  assert.deepEqual({ strings: 2 * ward.length, characters: 2 * length(ward) }, c.selectedStringEvents.wardIds);
  assert.deepEqual({ strings: 2 * parameters.length, characters: 2 * length(parameters) }, c.selectedStringEvents.parameterNames);
});

test("reviewed path string count is checked from source text without loading topology code", () => {
  const source = fs.readFileSync(path.join(__dirname, "../a68-context-topology-envelope.js"), "utf8");
  const names = [...source.matchAll(/"(a68-[a-z-]+\.js|[A-Za-z]+\.cs)": "[0-9a-f]{64}"/g)].map(m => m[1]);
  const paths = names.map(n => (n.endsWith(".js") ? "scripts/incremental/" : "studies/phase627_full_mixed_metric_native_field_variation_audit_001/") + n);
  assert.deepEqual({ strings: paths.length, characters: paths.reduce((n, s) => n + s.length, 0) }, derivePreflightInitializationCensus().selectedStringEvents.reviewedPaths);
});

test("record rows catch offsetting export/scope field-count errors from exact reviewed source", () => {
  const c = derivePreflightInitializationCensus();
  const counts = PREFLIGHT_SOURCE_PINS.map(m => {
    const source = fs.readFileSync(path.join(__dirname, "..", m.id), "utf8");
    const match = source.match(/module\.exports = (?:Object\.freeze\()?\{([\s\S]*?)\}\)?;/);
    assert.ok(match, m.id); return match[1].split(",").filter(s => s.trim()).length;
  });
  assert.deepEqual(counts, [2, 8, 13, 3, 1, 1, 2, 1, 5, 5, 1, 4, 5, 5, 8, 3, 1, 5, 3, 1, 1, 7, 4]);
  assert.deepEqual(c.recordLedger.find(r => r.component === "explicitModuleExportsIncludingAggregate"),
    { component: "explicitModuleExportsIncludingAggregate", records: counts.length, propertySlots: counts.reduce((a, b) => a + b, 0) });
  const source = fs.readFileSync(path.join(__dirname, "../a68-source-template-validator.js"), "utf8");
  const scope = source.match(/const scope = Object\.freeze\(\{([^}]+)\}\);/); assert.ok(scope);
  assert.deepEqual(c.recordLedger.find(r => r.component === "validatorScope"),
    { component: "validatorScope", records: 1, propertySlots: scope[1].split(",").length });
});

test("reviewed top-level weak maps are counted separately from deferred instance and factory maps", () => {
  // Source drift aid for the hash-pinned, reviewed declarations, not a general
  // JavaScript execution or allocation analyzer. No production module runs.
  const actual = PREFLIGHT_SOURCE_PINS.flatMap(m => {
    const source = fs.readFileSync(path.join(__dirname, "..", m.id), "utf8");
    const maps = [...source.matchAll(/^const [A-Za-z]+ = new WeakMap\(\);$/gm)].length;
    return maps ? [{ module: m.id, maps, entries: 0 }] : [];
  });
  assert.deepEqual(actual, [
    { module: "a68-checkpoint-catalog.js", maps: 1, entries: 0 },
    { module: "a68-template-preflight-service.js", maps: 1, entries: 0 },
    { module: "a68-template-process-host.js", maps: 1, entries: 0 }
  ]);
  const census = derivePreflightInitializationCensus();
  assert.deepEqual(census.weakMapLedger, actual);
  assert.equal(census.weakMaps, actual.reduce((n, r) => n + r.maps, 0));
  assert.ok(Object.isFrozen(census.weakMapLedger));
});
