"use strict";

// Source-semantic CONTENT census for the exact reviewed entry/manifest, not
// complete allocation, runtime, resource admission or permission to load it.
// No production module is imported. Counts below include explicit temporary
// arrays/reflection results, but not engine allocation behavior or live RSS.
const { PREFLIGHT_SOURCE_PINS, ENTRY } = require("./a68-preflight-module-manifest");
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
function derivePreflightInitializationCensus() {
  // A multiindices call owns one35-slot outer array and35 four-slot rows.
  const multi = { arrays: 1 + 35, arraySlots: 35 + 35 * 4 };
  // A Ward menu calls multiindices twice, constructs70 literal comparison
  // menus (outer3 plus3 rows of4),6 parameter menus and12 route arrays.
  // Recursive freeze visits44 aggregate values, producing139 values slots.
  const ward = { arrays: 2 * multi.arrays + 70 * 4 + 1 + 6 * 2 + 12 + 44,
    arraySlots: 2 * multi.arraySlots + 70 * 15 + 6 + 6 * 4 + 12 * 2 + 139 };
  const ledger = [
    { component: "twoWardMenusIncludingFourMultiindices", arrays: 2 * ward.arrays, arraySlots: 2 * ward.arraySlots },
    { component: "sourceMenuSeparateMultiindices", ...multi },
    { component: "sourceMenuPieces", arrays: 1, arraySlots: 4 },
    { component: "originalContextIds", arrays: 1, arraySlots: 2 + 2 * 10 * 35 + 3 },
    { component: "twoWardIdMaps", arrays: 2, arraySlots: 2 * 6 },
    { component: "topologyAndValidatorIdCopies", arrays: 2, arraySlots: 2 * 705 },
    { component: "branchEnvelopeReflection", arrays: 2 + 1 + 6, arraySlots: 6 + 6 + 6 + 6 * 2 },
    { component: "fixedTriplesAndFreezeValues", arrays: 15 + 16, arraySlots: 15 * 3 + 15 + 15 * 3 },
    { component: "fixedLedgersAndFreezeValues", arrays: 6 + 48 + 55, arraySlots: 48 + 48 * 2 + 6 + 48 + 48 * 2 },
    { component: "topologyScopeFreezeValues", arrays: 1, arraySlots: 7 },
    { component: "copyFieldNames", arrays: 1, arraySlots: 5 },
    // Literal outer/pairs3, entries25, mapped25, flatMap1, freeze values1.
    { component: "reviewedSourceHashConstructionAndFreeze", arrays: 3 + 25 + 25 + 1 + 1, arraySlots: 6 + 69 + 69 + 23 + 23 },
    { component: "otherRecipeLiteralArrays", arrays: 2 + 4 + 12 + 30, arraySlots: 8 + 78 + 44 + 112 }
  ];
  const recordLedger = [
    { component: "twoWardMenus", records: 2 * (6 + 12 + 1), propertySlots: 2 * (6 * 5 + 12 * 3 + 7) },
    { component: "branchDescriptorsAndSnapshot", records: 1 + 6 + 1, propertySlots: 6 + 6 * 4 + 6 },
    { component: "branchReturnedEnvelope", records: 1 + 1 + 7 + 1, propertySlots: 4 + 7 + 7 * 4 + 6 },
    { component: "fixedLedgerAndScopeRoots", records: 3, propertySlots: 15 + 6 + 7 },
    { component: "reviewedHashMaps", records: 3, propertySlots: 12 + 11 + 23 },
    { component: "branchEventLimits", records: 1, propertySlots: 6 },
    { component: "validatorScope", records: 1, propertySlots: 6 },
    { component: "explicitModuleExportsIncludingAggregate", records: 23,
      propertySlots: [2, 8, 13, 3, 1, 1, 2, 1, 5, 5, 1, 4, 5, 5, 8, 3, 1, 5, 3, 1, 1, 7, 4].reduce((a, b) => a + b, 0) }
  ];
  // These constructors run at module initialization. Their maps start empty;
  // later factory registrations and engine backing storage are NOT counted.
  // MixedRecipe instance fields and function-local maps remain later work.
  const weakMapLedger = [
    { module: "a68-checkpoint-catalog.js", maps: 1, entries: 0 },
    { module: "a68-template-preflight-service.js", maps: 1, entries: 0 },
    { module: "a68-template-process-host.js", maps: 1, entries: 0 }
  ];
  return freeze({ schemaVersion: "phase627-preflight-initialization-content-v1", entry: ENTRY,
    sourceCommitments: PREFLIGHT_SOURCE_PINS.map(m => ({ id: m.id, bytes: m.bytes, sha256: m.sha256 })),
    sourceBytes: PREFLIGHT_SOURCE_PINS.reduce((n, m) => n + m.bytes, 0),
    moduleInitializations: PREFLIGHT_SOURCE_PINS.length,
    importRequests: PREFLIGHT_SOURCE_PINS.reduce((n, m) => n + m.imports.length, 0),
    calls: { multiindices: 1 + 2 * 2, wardControlsMenu: 2, sourceContextIds: 2,
      prospectiveBranchTopologyEnvelope: 1, sourceContextMenu: 0, diagnosticMenu: 0, coefficientEvaluation: 0 },
    multiindexLoopBodiesPerCall: { order: 4, a: 10, b: 20, c: 35 },
    wardPredicateComparisons: 2 * 2 * (35 * 3 - 2 - 1),
    ledger, arrays: ledger.reduce((n, r) => n + r.arrays, 0), arraySlots: ledger.reduce((n, r) => n + r.arraySlots, 0),
    recordLedger, records: recordLedger.reduce((n, r) => n + r.records, 0), propertySlots: recordLedger.reduce((n, r) => n + r.propertySlots, 0),
    sets: 3 + 2, setEntries: 3 * 705 + 2 * 6,
    weakMapLedger, weakMaps: weakMapLedger.reduce((n, r) => n + r.maps, 0),
    weakMapEntries: weakMapLedger.reduce((n, r) => n + r.entries, 0),
    selectedStringEvents: {
      contextIds: { strings: 702, characters: 8912, retainedWithDiagnosticLiterals: 8974 },
      wardIds: { strings: 12, characters: 152 }, parameterNames: { strings: 24, characters: 256 },
      comparisonJson: { strings: 816, characters: 7344 }, reviewedPaths: { strings: 23, characters: 1478 }
    },
    omitted: ["functionsClosuresClassesPrototypes", "loaderModuleRecordsAndBootstrap",
      "iteratorsAndBuiltinScratch", "numericToStringAndInterpolationInternals", "literalStringStorage",
      "parserRegexCompilerJitCodeAndStack", "allocatorCapacityGcAndRss", "nodeNativeBuiltinInitialization",
      "laterFactoryPlannerAndTraversalExecution"],
    scope: { sourceSemanticContentCensus: true, completeInitializationAllocationProof: false,
      actualLoadedModuleIdentityProved: false, runtimeClosureComplete: false,
      resourceFeasibilityProved: false, scientificExecutionAuthorized: false }
  });
}
module.exports = { derivePreflightInitializationCensus };
