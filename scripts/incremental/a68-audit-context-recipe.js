"use strict";

// MixedAudit.Evaluate + MixedAuditSink complete point/germ expression recipes.
// Metadata only: geometry/source predicates, coefficient comparisons and error
// bounds are descriptors, NEVER asserted successful here. No scientific tensor
// is evaluated. Geometry validators must independently establish source closure.
// Point child traversal and source-bound numerical execution remain separate.
const { buildMixedBackground, importMixedBaseline, buildMixedMetricGerm,
  baselineRoles, canonicalBaselineImports, CoordinateDual, multiindices } = require("./a68-background-recipe");
const { buildMixedVariation } = require("./a68-variation-recipe");
const { wardControlsMenu, wardMarkMenu, buildWardControlsRecipe } = require("./a68-ward-controls-recipe");
const SCHEMA = "phase627-audit-context-recipe-v2";
const PIECES = Object.freeze(["source", "kinetic", "cubic", "mass"]);
const FAMILIES = Object.freeze([["raw0", "Raw0", 1], ["raw2", "Raw2", 2],
  ["fieldFirst0", "FieldFirst0", 1], ["fieldFirst2", "FieldFirst2", 2], ["word0", "Oracle0", 1], ["word2", "Oracle2", 2],
  ["eulerCovariant", "EulerCovariant", 1], ["eulerMoving", "EulerMoving", 1], ["native0", "Native0", 1], ["nativeEuler", "NativeEuler", 1]]);
const need = (ok, why) => { if (!ok) throw new Error("A68 audit context recipe: " + why); };
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const named = x => typeof x === "string" && /^[\x20-\x7e]+$/.test(x) && x.trim().length > 0;
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const own = (object, key) => {
  const descriptor = object && Object.getOwnPropertyDescriptor(object, key);
  need(descriptor && Object.hasOwn(descriptor, "value"), "own data option " + key); return descriptor.value;
};
// Capture caller-owned metadata before ANY validator/continuation callback.
// Never JSON-clone an object with a caller-controlled getter/toJSON. This
// closes descriptor identity drift; whole metadata/RSS admission is separate.
function snapshotData(value, active = new WeakSet(), depth = 0) {
  need(depth <= 32, "bounded metadata nesting");
  if (value === null || ["string", "boolean", "number"].includes(typeof value)) return value;
  need(value && typeof value === "object" && !active.has(value), "acyclic plain metadata");
  active.add(value);
  const isArray = Array.isArray(value), keys = Reflect.ownKeys(value);
  let result;
  if (isArray) {
    const count = Object.getOwnPropertyDescriptor(value, "length")?.value;
    need(Number.isSafeInteger(count) && count >= 0 && keys.length === count + 1, "dense metadata array");
    result = [];
    for (let i = 0; i < count; i++) {
      const entry = Object.getOwnPropertyDescriptor(value, String(i));
      need(entry && Object.hasOwn(entry, "value") && entry.enumerable, "own metadata data indices");
      result.push(snapshotData(entry.value, active, depth + 1));
    }
  } else {
    result = Object.fromEntries(keys.map(key => {
      const entry = Object.getOwnPropertyDescriptor(value, key);
      need(typeof key === "string" && entry && Object.hasOwn(entry, "value") && entry.enumerable, "own metadata data fields");
      return [key, snapshotData(entry.value, active, depth + 1)];
    }));
  }
  active.delete(value); return Object.freeze(result);
}
const roleValue = (b, role) => { const m = /^(KInputs|GradientPieces|CovariantFrame)\[(\d+)\]$/.exec(role); return m ? b[m[1]][Number(m[2])] : b[role]; };
function intermediateMenu() {
  const out = [];
  for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) for (const part of ["value", "delta"])
    out.push({ name: `fixed_p${p}_s${s}_${part}`, degree: [2, 12, 13, 14, 0, 1, 13, 1][s] });
  for (let s = 0; s < 9; s++) for (const part of ["value", "delta"])
    out.push({ name: `reverse_s${s}_${part}`, degree: [1, 13, 2, 1, 0, 14, 12, 2, 2][s] });
  for (let z = 0; z < 14; z++) for (const name of [`covariant_z${z}`, `covariantOracle_z${z}`]) out.push({ name, degree: 2 });
  for (let mu = 0; mu < 14; mu++) out.push({ name: `partial_mu${mu}`, degree: 2 });
  return out;
}
function pointMarkMenu() {
  const out = baselineRoles().map(role => ({ name: "background/" + role.role, degree: role.degree, requiredExpanded: null }));
  for (const [name, degree] of [["NativeX", 1], ["DXOracle", 2], ["ReverseX", 1], ["Gradient", 1]]) out.push({ name: "background/" + name, degree, requiredExpanded: null });
  for (let mu = 0; mu < 14; mu++) for (const name of ["NativePartial", "NativePartialOracle", "CovariantCoordinate"])
    out.push({ name: `background/${name}[${mu}]`, degree: 1, requiredExpanded: null });
  return freeze(out);
}
function germMarkMenu() {
  const out = [];
  for (const [name, degree] of [["DeltaB", 1], ["DeltaFAdapted", 2], ["DeltaFOracle", 2], ["DeltaFFixed", 2], ["DeltaBExterior", 2], ["DeltaCurvatureFromConnection", 2]])
    out.push({ name: "geometry/" + name, degree, requiredExpanded: null });
  for (const item of intermediateMenu()) out.push({ name: "tensor/intermediate_" + item.name, degree: item.degree, requiredExpanded: false });
  for (const [name, , degree] of [...FAMILIES, ["nativeFieldFirst0", null, 1], ["nativeMovingEuler", null, 1]])
    for (const piece of [...PIECES, "total"]) out.push({ name: `tensor/${name}_${piece}`, degree, requiredExpanded: true });
  for (const [name, degree] of [["native2", 2], ["nativeFieldFirst2", 2], ["nativeDivergence", 1], ["ordinaryAdjoint", 1]])
    out.push({ name: "tensor/" + name, degree, requiredExpanded: true });
  for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) out.push({ name: `tensor/currentPartial_${mu}_${nu}`, degree: 0, requiredExpanded: false });
  for (const name of ["current", "greenCurrent"]) out.push({ name: "tensor/" + name, degree: 2, requiredExpanded: true });
  return freeze(out);
}
function retentionPolicy(retention, menu) {
  retention = snapshotData(retention);
  need(retention && typeof retention === "object" && !Array.isArray(retention), "explicit retention map");
  need(Reflect.ownKeys(retention).length === menu.length && menu.every(m => Object.hasOwn(retention, m.name) && typeof retention[m.name] === "boolean" && (m.requiredExpanded === null || retention[m.name] === m.requiredExpanded)), "exact retention census and mandatory expansion");
  return retention;
}
function collector(recipe, retention, menu) {
  const policy = retentionPolicy(retention, menu), callbacks = [], checks = [], scalarRoots = [], marks = [], domainChecks = [];
  const callbackNames = new Set(), rootNames = new Set();
  function callback(category, name, degree = null, expanded = null) {
    const key = category + "/" + name; need(named(name) && !callbackNames.has(key), "unique callback name"); callbackNames.add(key);
    callbacks.push({ category, name, degree, expanded, length: null });
  }
  function tensorRef(t) {
    const degree = recipe.TensorDegree(t);
    try { return { kind: "tensorNode", node: recipe.RecordedNode(t) }; }
    catch (error) {
      // Equal/Count on C# new FT() does not touch the trace. Registering such a
      // diagonal zero merely to describe a check would add a nonexistent node.
      if (degree === -1 && error.message === "A68 recipe: tensor must already have a recorded node") return { kind: "literalZero" };
      throw error;
    }
  }
  function mark(name, degree, tensor, required = null, domain = false) {
    const expected = menu[marks.length];
    need(expected && expected.name === name && expected.degree === degree && (required === null || policy[name] === required), "exact sink mark execution order/type");
    recipe.Mark(name, degree, tensor, policy[name]); const node = recipe.RecordedNode(tensor);
    marks.push({ name, degree, node, expanded: policy[name] });
    if (domain) domainChecks.push({ name, node, degree, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true });
  }
  const check = (name, predicate) => { callback("Check", name); checks.push({ name, metadataPath: "check/" + name, predicate }); };
  const tensor = (name, degree, value, expanded, domain = false) => { callback("Tensor", name, degree, expanded); mark("tensor/" + name, degree, value, expanded, domain); };
  const root = (name, expression) => { need(named(name) && !rootNames.has(name), "unique scalar root"); rootNames.add(name); scalarRoots.push({ name, expression }); return name; };
  const finish = () => need(marks.length === menu.length, "complete exact non-Ward mark census");
  return { callback, tensorRef, mark, check, tensor, root, finish, callbacks, checks, scalarRoots, marks, domainChecks };
}
const equal = (c, a, b) => ({ kind: "tensorEqual", left: c.tensorRef(a), right: c.tensorRef(b) });
const zero = (c, value) => ({ kind: "tensorZero", tensor: c.tensorRef(value) });
const all = predicates => ({ kind: "all", predicates });
const coefficient = (c, tensor, form, blade) => ({ tensor: c.tensorRef(tensor), form, blade, k0: 0, k1: 0, absent: "0" });
const geometryEqual = (left, right) => ({ kind: "geometryEqual", left, right });
function exteriorComponent(recipe, tensor, mu, nu) {
  return mu === nu ? recipe.FreshZero() : recipe.Times(recipe.Component(tensor, (1 << mu) | (1 << nu)), mu < nu ? "1" : "-1");
}
function baselineMarks(c, b) {
  for (const role of baselineRoles()) c.mark("background/" + role.role, role.degree, roleValue(b, role.role));
  for (const [name, degree] of [["NativeX", 1], ["DXOracle", 2], ["ReverseX", 1], ["Gradient", 1]]) c.mark("background/" + name, degree, b[name]);
  for (let mu = 0; mu < 14; mu++) for (const name of ["NativePartial", "NativePartialOracle", "CovariantCoordinate"]) c.mark(`background/${name}[${mu}]`, 1, b[name][mu]);
}
function buildAuditPointContext(recipe, { xLeaf, curvatureLeaf, geometry, validateGeometry, retention }) {
  geometry = snapshotData(geometry);
  const c = collector(recipe, retention, pointMarkMenu()); need(named(xLeaf), "source-bound X leaf ID");
  c.callback("BeginPoint", "$"); const x = recipe.RegisterLeaf(xLeaf);
  const b = buildMixedBackground(recipe, { x, curvatureLeaf, geometry, validateGeometry });
  c.callback("GeometryLeaf", "curvature", 2); c.callback("Background", "$"); baselineMarks(c, b);
  for (let mu = 0; mu < 14; mu++) c.check(`nativeFirstJet_${mu}`, equal(c, b.NativePartial[mu], b.NativePartialOracle[mu]));
  c.check("covariantExterior", equal(c, b.DX, b.DXOracle));
  if (b.Point === 0) {
    const ax = coefficient(c, b.X, 1, 1);
    c.check("nativeFirstJetHandAnchor", { kind: "complexCoefficientScaledEqual", left: coefficient(c, b.NativePartial[4], 1, 1), right: ax, factor: "-1/2" });
    c.check("nativeExteriorHandAnchor", { kind: "complexCoefficientScaledEqual", left: coefficient(c, b.NativeExterior, 17, 1), right: ax, factor: "1/2" });
    c.check("nativeVectorAnchorPositive", { kind: "realCoefficientStrictLowerBound", coefficient: ax, imaginaryRequired: "0", lowerBound: "181/203327488" });
  }
  c.callback("SealPointBackground", "$"); c.finish();
  const childContexts = [];
  for (let metric = 0; metric < 10; metric++) for (let jet = 0; jet < 35; jet++) childContexts.push({ id: `point${b.Point}/m${metric}_j${jet}`, callback: { category: "Germ", name: `m${metric}_j${jet}` } });
  return freeze({ schemaVersion: SCHEMA, contextId: "point" + b.Point, background: b, callbacks: c.callbacks, checks: c.checks,
    scalarRoots: c.scalarRoots, marks: c.marks, domainChecks: [{ name: "input/X", node: recipe.RecordedNode(x), degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }],
    canonicalBaselineExports: canonicalBaselineImports(recipe, b), childContexts,
    continuation: { pending: true, reason: "all350 child germ contexts must finish before EndPoint", endCallback: { category: "EndPoint", name: "$" } },
    scope: { completeBackgroundDag: true, sourceClosureEstablishedHere: false, numericChecksReplayed: false, fullAuditRecipe: false, childTraversalExecuted: false } });
}
function geometryChecks(c, g) {
  c.check("traceFreeMotion", { kind: "geometryTraceZero", matrix: "geometry/geometry.motion" });
  c.check("metricValueDualBlocks", geometryEqual("geometry/geometry.deltaMetric.g", "geometry/geometry.blockMetric.g"));
  for (let mu = 0; mu < 14; mu++) {
    c.check(`metricFirstDualBlocks_${mu}`, geometryEqual(`geometry/geometry.deltaMetric.d[${mu}]`, `geometry/geometry.blockMetric.d[${mu}]`));
    c.check(`connectionDualPalatini_${mu}`, geometryEqual(`geometry/geometry.deltaConnection.gamma[${mu}]`, `geometry/geometry.palatini.gamma[${mu}]`));
    for (let nu = 0; nu < 14; nu++) {
      c.check(`metricSecondDualBlocks_${mu}_${nu}`, geometryEqual(`geometry/geometry.deltaMetric.dd[${mu}][${nu}]`, `geometry/geometry.blockMetric.dd[${mu}][${nu}]`));
      c.check(`connectionFirstDualPalatini_${mu}_${nu}`, geometryEqual(`geometry/geometry.deltaConnection.dGamma[${mu}][${nu}]`, `geometry/geometry.palatini.dGamma[${mu}][${nu}]`));
    }
  }
  c.check("curvatureDualPalatini", geometryEqual("geometry/geometry.deltaConnection.curvature", "geometry/geometry.palatini.curvature"));
  c.check("spinCurvatureLowering", equal(c, g.DeltaFAdapted, g.DeltaFOracle));
  c.check("referenceCurvatureDerivative", equal(c, g.DeltaFFixed, g.DeltaCurvatureFromConnection));
}
function errorDescriptor(c, b, g, backgroundGeometry, germGeometry) {
  // Explicit fixed formula obligations, not values asserted by this recipe.
  // This separate norm-expression language is NOT the scalar Pair/Top replay
  // AST; its source-bound epsilon and numerical evaluator remain prerequisites.
  const constant = value => ({ op: "constant", value }), add = (...values) => ({ op: "sum", values }), mul = (...values) => ({ op: "product", values });
  const matrix = name => ({ op: "geometryMatrix", name }), norm = value => ({ op: "entryL1", value });
  const radius = constant("15/113464"), fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true };
  const motionNorm = norm(matrix(germGeometry.matrices.Motion));
  const motionDerivativeSum = add(...germGeometry.matrices.MotionCovariant.map(id => norm(matrix(id))));
  const referenceNorm = { op: "tensorL1", tensor: c.tensorRef(b.B) }, referenceVariationNorm = { op: "tensorL1", tensor: c.tensorRef(g.DeltaB) };
  const k = constant("2576"), kappa = constant("907712"), two = constant("2"), four = constant("4");
  const raw0Majorant = add(mul(k, add(mul(add(constant("20"), mul(two, referenceNorm)), motionNorm), mul(two, referenceVariationNorm), mul(four, motionNorm, radius))), mul(two, kappa, motionNorm));
  const raw2Majorant = mul(k, motionNorm);
  const eulerMajorant = add(mul(k, add(motionDerivativeSum, mul(add(constant("40"), mul(four, referenceNorm)), motionNorm), mul(two, referenceVariationNorm), mul(four, motionNorm, radius))), mul(two, kappa, motionNorm));
  const nativeOneFormDualNorm = { op: "maximumColumnL1", matrix: matrix(backgroundGeometry.matrices.Frame) };
  const nativeTwoFormDualNorm = mul(nativeOneFormDualNorm, nativeOneFormDualNorm);
  const raw0Error = mul(raw0Majorant, fieldError), raw2Error = mul(raw2Majorant, fieldError), eulerError = mul(eulerMajorant, fieldError);
  return { schemaVersion: "phase627-mixed-error-formula-obligations-v1", numericalReplayImplemented: false,
    fields: { radius, fieldError, motionNorm, motionDerivativeSum, referenceNorm, referenceVariationNorm,
      raw0Majorant, raw2Majorant, eulerMajorant, raw0Error, raw2Error, eulerError, nativeOneFormDualNorm, nativeTwoFormDualNorm,
      nativeRaw0Error: mul(nativeOneFormDualNorm, raw0Error), nativeRaw2Error: mul(nativeTwoFormDualNorm, raw2Error), nativeEulerError: mul(nativeOneFormDualNorm, eulerError),
      nativeFieldNorm: { op: "maximumRowL1", matrix: matrix(backgroundGeometry.matrices.InverseFrame) },
      nativeFirstJetNorms: backgroundGeometry.matrices.FrameLift.map(id => ({ op: "maximumRowL1", matrix: { op: "matrixProduct", left: matrix(backgroundGeometry.matrices.InverseFrame), right: matrix(id) } })) } };
}
function buildAuditGermContext(recipe, options) {
  need(options && typeof options === "object" && !Array.isArray(options) &&
    Reflect.ownKeys(options).every(k => ["background", "germ", "retention", "wardRetention"].includes(k) &&
      Object.hasOwn(Object.getOwnPropertyDescriptor(options, k), "value")), "closed germ options; arbitrary Ward hooks are not permitted");
  let background = own(options, "background"), germ = own(options, "germ"), retention = own(options, "retention");
  let wardRetention = Object.hasOwn(options, "wardRetention") ? own(options, "wardRetention") : null;
  // In particular, the background validator must not be able to rewrite the
  // germ matrix roles, leaf declarations or later error-bound formulas.
  background = Object.freeze({ geometry: snapshotData(own(background, "geometry")), validateGeometry: own(background, "validateGeometry"),
    leafIds: snapshotData(own(background, "leafIds")) });
  germ = Object.freeze({ geometry: snapshotData(own(germ, "geometry")), validateGeometry: own(germ, "validateGeometry"),
    adaptedLeaf: own(germ, "adaptedLeaf"), oracleLeaf: own(germ, "oracleLeaf") });
  const selected = wardControlsMenu().contexts.some(item => item.point === background.geometry.point &&
    item.metricBasis === germ.geometry.metricBasis && item.jetIndex === germ.geometry.jetIndex);
  // Snapshot BOTH retention menus before a geometry validator can mutate them.
  // Missing/extra Ward policy is an error, never permission to omit a tail.
  if (selected) wardRetention = retentionPolicy(wardRetention, wardMarkMenu(background.geometry.point, germ.geometry.jetIndex));
  else need(wardRetention === null, "no Ward retention outside fixed selected menu");
  const c = collector(recipe, retention, germMarkMenu()); c.callback("BeginGerm", "$");
  const b = importMixedBaseline(recipe, background), g = buildMixedMetricGerm(recipe, b, germ), contextId = `point${b.Point}/m${g.MetricBasis}_j${g.JetIndex}`;
  c.callback("GeometryLeaf", "deltaCurvatureAdapted", 2); c.callback("GeometryLeaf", "deltaCurvatureOracle", 2); c.callback("Geometry", "$");
  for (const [name, degree] of [["DeltaB", 1], ["DeltaFAdapted", 2], ["DeltaFOracle", 2], ["DeltaFFixed", 2], ["DeltaBExterior", 2], ["DeltaCurvatureFromConnection", 2]]) c.mark("geometry/" + name, degree, g[name]);
  geometryChecks(c, g);
  const intermediate = intermediateMenu(); let at = 0;
  const row = buildMixedVariation(recipe, b, g, (name, tensor) => {
    const expected = intermediate[at++]; need(expected && expected.name === name, "actual intermediate callback order"); c.tensor("intermediate_" + name, expected.degree, tensor, false);
  });
  need(at === 108, "complete108 variation intermediate callbacks");
  c.check("nonInvariantCovariantDerivative", all(row.DerivativeIdentityComparisons.map(([a, b]) => equal(c, a, b))));
  c.check("fixedAdjointIndependent", equal(c, row.CAdjointFixed, row.CAdjointOracle));
  c.check("movingAdjointPairingMotion", equal(c, row.CAdjointFixed, recipe.Sum(row.MovingAdjointDelta, recipe.PairingMotion(g.Motion, b.AdjointX))));
  c.check("ordinaryAdjointNativeDivergence", equal(c, row.OrdinaryAdjoint, row.NativeAdjointAsFrame));
  const mark = (name, degree, value) => c.tensor(name, degree, value, true, true);
  const family = (name, degree, values) => {
    need(values.length === 4, "four original-action pieces"); values.forEach((t, p) => mark(name + "_" + PIECES[p], degree, t));
    mark(name + "_total", degree, recipe.Sum(...values));
  };
  for (const [name, field, degree] of FAMILIES) family(name, degree, row[field]);
  family("nativeFieldFirst0", 1, row.FieldFirst0.map(t => CoordinateDual(recipe, b.Frame, t)));
  family("nativeMovingEuler", 1, row.EulerMoving.map(t => CoordinateDual(recipe, b.Frame, t)));
  mark("native2", 2, row.Native2); mark("nativeFieldFirst2", 2, CoordinateDual(recipe, b.Frame, row.FieldFirst2[1]));
  mark("nativeDivergence", 1, row.NativeDivergence); mark("ordinaryAdjoint", 1, row.OrdinaryAdjoint);
  for (let p = 0; p < 4; p++) {
    for (const [name, left, right] of [["rawFieldFirst0", row.Raw0, row.FieldFirst0], ["rawFieldFirst2", row.Raw2, row.FieldFirst2], ["rawWord0", row.Raw0, row.Oracle0], ["rawWord2", row.Raw2, row.Oracle2], ["eulerMoving", row.EulerCovariant, row.EulerMoving]])
      c.check(name + "_" + PIECES[p], equal(c, left[p], right[p]));
    c.check("nativeEuler_" + PIECES[p], equal(c, row.NativeEuler[p], CoordinateDual(recipe, b.Frame, row.EulerCovariant[p])));
    if (g.Order === 3 && p > 0) c.check("thirdGermLowerPiece_" + PIECES[p], all([row.Raw0[p], row.Raw2[p], row.EulerCovariant[p]].map(t => zero(c, t))));
  }
  let current = recipe.FreshZero(), green = recipe.FreshZero();
  for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) {
    const part = exteriorComponent(recipe, row.Native2, mu, nu);
    c.check(`nativeJetSlot_${mu}_${nu}`, equal(c, part, row.CurrentCoefficients[mu][nu]));
    c.check(`greenCurrent_${mu}_${nu}`, equal(c, row.CurrentCoefficients[mu][nu], row.GreenVariationCoefficients[mu][nu]));
    c.tensor(`currentPartial_${mu}_${nu}`, 0, row.CurrentPartialCoefficients[mu][nu], false);
    if (mu === nu) c.check(`diagonalNull_${mu}`, zero(c, part));
    if (mu < nu) {
      c.check(`symmetricNull_${mu}_${nu}`, zero(c, recipe.Sum(row.CurrentCoefficients[mu][nu], row.CurrentCoefficients[nu][mu])));
      const form = recipe.Unit((1 << mu) | (1 << nu), 0);
      current = recipe.Sum(current, recipe.P(form, row.CurrentCoefficients[mu][nu]));
      green = recipe.Sum(green, recipe.P(form, row.GreenVariationCoefficients[mu][nu]));
    }
  }
  mark("current", 2, current); mark("greenCurrent", 2, green);
  c.check("completeCurrent", all([equal(c, current, row.Native2), equal(c, current, green)]));
  const error = errorDescriptor(c, b, g, background.geometry, germ.geometry); c.callback("Error", "$");
  if (g.Order === 3) c.check("thirdGermZeroBackgroundError", { kind: "metadataScalarsZero", fields: ["error/raw0Error", "error/raw2Error", "error/eulerError"] });
  if (b.Point === 1 && g.MetricBasis === 0 && same(g.Multiindex, [0, 3, 0, 0])) {
    const probe = recipe.Pullback(b.Frame, recipe.Unit(1 << 8, 1));
    const actual = c.root("check/thirdSourceHandAnchor/actual", pairExpression(recipe, probe, row.Raw0[0]));
    c.check("thirdSourceHandAnchor", { kind: "scalarRootEqualsConstant", root: actual, expected: "3/16" });
  }
  if (b.Point === 0 && g.MetricBasis === 0 && same(g.Multiindex, [1, 0, 0, 0])) {
    const nativeProbe = recipe.Sum(recipe.Unit(1, 1), recipe.Unit(1 << 4, 1));
    const probe = recipe.Pullback(b.Frame, nativeProbe);
    const actual = c.root("check/nonzeroMassHandAnchor/actual", pairExpression(recipe, probe, row.Raw0[3]));
    c.check("nonzeroMassHandAnchor", { kind: "scalarRootEqualsRealCoefficient", root: actual, coefficient: coefficient(c, b.X, 1, 1), factor: "-907712" });
  }
  const metricPairs = [...Array.from({ length: 4 }, (_, i) => [i, i])]; for (let i = 0; i < 4; i++) for (let j = i + 1; j < 4; j++) metricPairs.push([i, j]);
  const axis = g.Multiindex.findIndex(v => v === 3), pureDiffeomorphism = axis >= 0 && metricPairs[g.MetricBasis].includes(axis);
  if (pureDiffeomorphism) c.check("pureBaseDiffeomorphismPrincipalZero", all([{ kind: "geometryZero", field: "geometry/geometry.deltaConnection.curvature" }, ...[g.DeltaFFixed, row.Raw0[0], row.EulerCovariant[0]].map(t => zero(c, t))]));
  c.finish();
  const wardSelected = g.MetricBasis === 0 && [[1, 0, 0, 0], [0, 2, 0, 0], [0, 3, 0, 0]].some(m => same(m, g.Multiindex));
  need(selected === wardSelected, "validated fixed Ward selection matches pre-callback policy");
  const wardControls = wardSelected ? buildWardControlsRecipe(recipe, b, g, { retention: wardRetention }) : null;
  if (wardControls) need(wardControls.contextId === contextId && wardControls.domainChecks.length === 148, "same selected Ward context and complete input domains");
  const merge = (left, right, key, label) => {
    const merged = [...left, ...right], names = merged.map(key);
    need(new Set(names).size === names.length, "unique combined " + label); return merged;
  };
  const callbacks = merge(c.callbacks, wardControls?.callbacks ?? [], item => item.category + "/" + item.name, "callbacks");
  callbacks.push({ category: "EndGerm", name: "$", degree: null, expanded: null, length: null });
  const checks = merge(c.checks, wardControls?.checks ?? [], item => item.name, "checks");
  const scalarRoots = merge(c.scalarRoots, wardControls?.scalarRoots ?? [], item => item.name, "scalar roots");
  const marks = merge(c.marks, wardControls?.marks ?? [], item => item.name, "marks");
  const domainChecks = merge(c.domainChecks, wardControls?.domainChecks ?? [], item => item.name, "domain obligations");
  // Error formulas refer to the independently validated named matrices; Ward
  // scalar recipes also use their fixed background.Frame alias. Admit a name
  // collision only when its COMPLETE sparse matrix is identical.
  const geometry = [], matrixNames = new Map();
  const addGeometry = (id, entries) => {
    if (matrixNames.has(id)) need(same(matrixNames.get(id), entries), "same scalar geometry for aliased name");
    else { matrixNames.set(id, entries); geometry.push({ id, entries }); }
  };
  function geometryRoles(value) {
    if (typeof value === "string") addGeometry(value, recipe.MatrixEntries(recipe.Matrix(value)));
    else if (Array.isArray(value)) value.forEach(geometryRoles);
    else Object.values(value).forEach(geometryRoles);
  }
  geometryRoles(background.geometry.matrices); geometryRoles(germ.geometry.matrices);
  for (const item of wardControls?.geometry ?? []) addGeometry(item.id, item.entries);
  return freeze({ schemaVersion: SCHEMA, contextId, background: b, germ: g, variation: row, current, green,
    callbacks, checks, scalarRoots, marks, domainChecks, geometry, error, wardControls,
    structured: wardControls?.structured ?? [],
    nonWard: { callbacks: c.callbacks, checks: c.checks, scalarRoots: c.scalarRoots, marks: c.marks, domainChecks: c.domainChecks },
    census: { nativeJetSlots: 196, differentiatedCurrentMarks: 196, diagonalNullControls: 14, symmetricNullControls: 91,
      intermediateMarks: 108, nonWardMarks: 376, wardMarks: wardSelected ? 3600 : 0,
      wardChecks: wardSelected ? 614 : 0, wardScalarRoots: wardSelected ? 1348 : 0,
      thirdGermLowerPieceChecks: g.Order === 3 ? 3 : 0, pureDiffeomorphismControls: pureDiffeomorphism ? 1 : 0 },
    scope: { completeNonWardDag: true, nonWardMarkCensusComplete: true, completeContextLifecycle: true,
      sourceClosureEstablishedHere: false, numericChecksReplayed: false, geometryPredicatesReplayed: false, errorPredicatesReplayed: false, fullAuditRecipe: false } });
}
function pairExpression(recipe, left, right) { return { op: "pair", left: recipe.RecordedNode(left), right: recipe.RecordedNode(right) }; }

module.exports = { SCHEMA, PIECES, pointMarkMenu, germMarkMenu, intermediateMenu, exteriorComponent, buildAuditPointContext, buildAuditGermContext };
