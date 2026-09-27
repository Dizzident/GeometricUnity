"use strict";

// Independent diagnostic EXPRESSION/CALLBACK metadata, never diagnostic
// coefficient evaluation. The caller must reconstruct and source-bind every
// geometry descriptor/curvature leaf independently of an observed DAG. Numeric
// checks, geometry artifacts, resource admission and the enclosing run remain
// separate obligations. Fixed manufactured X/probes are constructed here.
const { MixedRecipe } = require("./a68-mixed-recipe");
const { buildMixedBackground, buildMixedMetricGerm, baselineRoles, CoordinateDual } = require("./a68-background-recipe");
const { buildMixedVariation } = require("./a68-variation-recipe");
const { buildOriginalMixedAction, FIELDS } = require("./a68-original-action-recipe");
const { buildMixedWardAction } = require("./a68-ward-recipe");
const { buildMixedWardAcceleration } = require("./a68-acceleration-recipe");
const { structuredFieldMenu, wardInputDomains, accelerationInputDomains } = require("./a68-ward-controls-recipe");
const { pointMarkMenu, intermediateMenu, exteriorComponent } = require("./a68-audit-context-recipe");
const SCHEMA = "phase627-diagnostics-recipe-v1";
const PIECES = ["source", "kinetic", "cubic", "mass"], DEGREES = [2, 12, 13, 14, 0, 1, 13, 1];
const FAMILIES = [["raw0", "Raw0", 1], ["raw2", "Raw2", 2], ["fieldFirst0", "FieldFirst0", 1], ["fieldFirst2", "FieldFirst2", 2],
  ["word0", "Oracle0", 1], ["word2", "Oracle2", 2], ["eulerCovariant", "EulerCovariant", 1], ["eulerMoving", "EulerMoving", 1], ["native0", "Native0", 1], ["nativeEuler", "NativeEuler", 1]];
const SINGLES = [["fixedAdjoint", "CAdjointFixed", 2], ["wordAdjoint", "CAdjointOracle", 2], ["movingAdjointDelta", "MovingAdjointDelta", 2],
  ["ordinaryAdjoint", "OrdinaryAdjoint", 1], ["native2", "Native2", 2], ["nativeDivergence", "NativeDivergence", 1], ["nativeAdjointAsFrame", "NativeAdjointAsFrame", 1]];
const HAND = ["motionX", "crossQ", "motionCrossQ", "adjointMotionX", "wordAdjointMotionX", "fixedAdjoint", "movingAdjointDelta", "pairingMotionAdjoint", "adjointPhi1", "pairingMotionAdjointPhi1"];
const GEO = [["DeltaB", 1], ["DeltaFAdapted", 2], ["DeltaFOracle", 2], ["DeltaFFixed", 2], ["DeltaBExterior", 2], ["DeltaCurvatureFromConnection", 2]];
const SCALARS = ["Value", "Metric", "Field", "Mixed"];
const need = (ok, why) => { if (!ok) throw new Error("A68 diagnostics recipe: " + why); };
const freeze = x => { if (x && typeof x === "object" && !Object.isFrozen(x)) { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const constant = value => freeze({ op: "constant", value }), add = (left, right) => freeze({ op: "add", left, right }), multiply = (left, right) => freeze({ op: "multiply", left, right });
const all = predicates => ({ kind: "all", predicates });
function own(x, key) { const d = x && Object.getOwnPropertyDescriptor(x, key); need(d && Object.hasOwn(d, "value") && d.enumerable, "own data field " + key); return d.value; }
function snapshot(x, active = new WeakSet(), depth = 0) {
  need(depth <= 32, "metadata depth"); if (x === null || ["string", "number", "boolean"].includes(typeof x)) return x;
  need(x && typeof x === "object" && !active.has(x), "acyclic plain metadata"); active.add(x); let out;
  if (Array.isArray(x)) {
    need(Reflect.ownKeys(x).length === x.length + 1, "dense metadata array"); out = Array.from({ length: x.length }, (_, i) => snapshot(own(x, String(i)), active, depth + 1));
  } else out = Object.fromEntries(Reflect.ownKeys(x).map(k => { need(typeof k === "string", "no metadata symbols"); return [k, snapshot(own(x, k), active, depth + 1)]; }));
  active.delete(x); return freeze(out);
}
function inputs(options, diagnostic) {
  const source = own(options, "background"), background = { geometry: snapshot(own(source, "geometry")), validateGeometry: own(source, "validateGeometry"), curvatureLeaf: own(source, "curvatureLeaf") };
  const raw = diagnostic === "grade10" ? own(options, "germs") : [own(options, "germ")];
  need(Array.isArray(raw) && raw.length === (diagnostic === "grade10" ? 4 : 1) && Reflect.ownKeys(raw).length === raw.length + 1, "complete fixed diagnostic germs");
  const germs = Array.from({ length: raw.length }, (_, i) => {
    const source = own(raw, String(i)); return { geometry: snapshot(own(source, "geometry")), validateGeometry: own(source, "validateGeometry"), adaptedLeaf: own(source, "adaptedLeaf"), oracleLeaf: own(source, "oracleLeaf") };
  });
  const point = diagnostic === "grade10" ? 0 : 1, jet = diagnostic === "grade10" ? 4 : 10;
  need(background.geometry.point === point && typeof background.validateGeometry === "function", "fixed diagnostic point and source validator");
  germs.forEach((g, i) => need(g.geometry.point === point && g.geometry.metricBasis === (diagnostic === "grade10" ? i : 0) && g.geometry.jetIndex === jet && typeof g.validateGeometry === "function", "fixed diagnostic germ/source menu"));
  need(typeof background.curvatureLeaf === "string" && germs.every(g => typeof g.adaptedLeaf === "string" && typeof g.oracleLeaf === "string"), "explicit curvature leaf IDs");
  need(new Set([background.curvatureLeaf, ...germs.flatMap(g => [g.adaptedLeaf, g.oracleLeaf])]).size === 1 + 2 * germs.length, "distinct independently bound geometry leaves");
  return { background: Object.freeze(background), germs: Object.freeze(germs.map(Object.freeze)), retention: snapshot(own(options, "retention")) };
}
function diagnosticMarkMenu(diagnostic) {
  need(diagnostic === "grade10" || diagnostic === "acceleration", "closed diagnostic name"); const out = [];
  const mark = (name, degree, requiredExpanded = null) => out.push({ name, degree, requiredExpanded });
  const tensor = (name, degree, expanded = true) => mark("tensor/" + name, degree, expanded);
  const background = () => pointMarkMenu().forEach(m => mark(m.name, m.degree));
  const geometry = prefix => GEO.forEach(([name, degree]) => mark(prefix + "/" + name, degree));
  const structured = (category, name) => structuredFieldMenu(category).filter(f => f.category === "Tensor").forEach(f => mark(`structured/${category}/${name}/${f.name}`, f.degree));
  tensor("input/X", 1);
  if (diagnostic === "grade10") {
    tensor("input/UFrame", 1); background(); tensor("input/UNative", 1); tensor("input/dUNative", 2);
    for (let basis = 0; basis < 4; basis++) {
      const prefix = "basis" + basis; geometry(prefix);
      intermediateMenu().forEach(m => tensor(prefix + "/intermediate/" + m.name, m.degree, false));
      for (const [name, , degree] of FAMILIES) for (const piece of [...PIECES, "total"]) tensor(`${prefix}/${name}/${piece}`, degree);
      SINGLES.forEach(([name, , degree]) => tensor(prefix + "/" + name, degree));
      for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) for (const kind of ["current", "currentPartial", "green"]) tensor(`${prefix}/${kind}/${mu}/${nu}`, 0, kind !== "currentPartial");
      structured("Original", prefix + "/original0"); structured("Original", prefix + "/original1");
    }
    for (const [name, , degree] of FAMILIES.slice(0, 8)) for (const piece of [...PIECES, "total"]) tensor(`combined/${name}/${piece}`, degree);
    HAND.forEach((name, i) => tensor("combined/" + name, i === 0 ? 1 : 2));
    for (let s = 0; s < 8; s++) for (const route of ["literal", "word"]) tensor(`forwardHand/${route}/${s}`, DEGREES[s]);
  } else {
    tensor("input/eta", 0); tensor("input/dEta", 1); background(); geometry("germ");
    for (let route = 0; route < 2; route++) {
      const p = "route" + route; structured("Ward", p + "/epsilon"); structured("Ward", p + "/compensated");
      structured("Acceleration", p + "/acceleration"); structured("Original", p + "/fixedNativeTangent");
    }
  }
  need(out.length === (diagnostic === "grade10" ? 3948 : 1883), "fixed full diagnostic mark census"); return freeze(out);
}
// Independently transcribed NAME SETS from MixedDiagnostics' frozen menus.
// In particular rawOriginal is interleaved here but executed after Original;
// the returned callbacks below, not this set menu, define execution order.
function diagnosticCheckMenu(diagnostic) {
  need(diagnostic === "grade10" || diagnostic === "acceleration", "closed diagnostic name"); const names = [];
  for (let mu = 0; mu < 14; mu++) names.push("background/nativeJet/" + mu); names.push("background/covariantExterior");
  const original = prefix => { for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) names.push(`${prefix}/stage/${p}/${s}`); names.push(prefix + "/scalars"); };
  const ward = prefix => {
    names.push(prefix + "/primitiveMenu"); for (let p = 0; p < 33; p++) names.push(`${prefix}/primitive/${p}`);
    for (let r = 0; r < 2; r++) { for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) names.push(`${prefix}/density/${r}/stage/${p}/${s}`);
      for (let p = 0; p < 4; p++) names.push(`${prefix}/density/${r}/top/${p}`); names.push(`${prefix}/density/${r}/scalars`); }
  };
  if (diagnostic === "grade10") {
    names.push("background/gradePreservingJets");
    for (let basis = 0; basis < 4; basis++) {
      const prefix = "basis" + basis; for (const kind of ["covariantDerivative", "ordinaryAdjoint", "fixedAdjoint"]) names.push(prefix + "/" + kind);
      for (let p = 0; p < 4; p++) for (const kind of ["raw0", "raw2", "euler", "nativeEuler", "rawOriginal"]) names.push(`${prefix}/${kind}/${p}`);
      for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) names.push(`${prefix}/current/${mu}/${nu}`);
      original(prefix + "/originalEquality");
    }
    names.push("combined/realizableMotion"); for (let route = 0; route < 2; route++) names.push("combined/originalForecast" + route);
    for (const f of [0, 2, 4, 6, 7]) names.push("combined/forecast/" + FAMILIES[f][0]);
    for (let s = 0; s < 8; s++) names.push("forwardHand/stage/" + s);
    for (const name of ["probeNorm", "quadraticZero", "motionX", "crossQ", "motionCrossZero", "reverseWord", "reverseCoefficient", "forwardCoefficient", "movingAdjointPairing", "adjointPhi", "nonzeroV2"]) names.push("hand/" + name);
  } else {
    for (let route = 0; route < 2; route++) {
      const prefix = "route" + route; for (const name of ["forecast", "accelerationCoefficient", "accelerationJets"]) names.push(prefix + "/" + name);
      for (const variant of ["epsilon", "compensated"]) for (const name of ["descent", "derivative", "curvature", "phiFirst", "phiOuter", "phiInner"]) names.push(`${prefix}/${variant}/${name}`);
      for (let p = 0; p < 4; p++) for (const name of ["ward", "epsilonWard", "green"]) names.push(`${prefix}/${name}/${p}`);
    }
    ward("word/epsilon"); ward("word/compensated"); original("word/original");
    names.push("word/acceleration/value", "word/acceleration/scalars");
    for (let mu = 0; mu < 14; mu++) names.push("word/acceleration/jet/" + mu);
    for (let p = 0; p < 2; p++) for (let s = 0; s < 8; s++) names.push(`word/acceleration/stage/${p}/${s}`);
  }
  need(names.length === (diagnostic === "grade10" ? 1019 : 310) && new Set(names).size === names.length, "independent exact check menu"); return freeze(names);
}
function collector(recipe, diagnostic, retention) {
  need(recipe instanceof MixedRecipe, "MixedRecipe session"); const menu = diagnosticMarkMenu(diagnostic), seen = new Set();
  need(retention && Reflect.ownKeys(retention).length === menu.length && menu.every(m => typeof retention[m.name] === "boolean" && (m.requiredExpanded === null || retention[m.name] === m.requiredExpanded)), "exact retention and mandatory expansion");
  const callbacks = [], marks = [], checks = [], scalarRoots = [], structured = [], scalarArrays = [], geometry = [], domainChecks = [], geometryArtifacts = [], roots = new WeakMap();
  const rootNames = new Set();
  const callback = (category, name, degree = null, expanded = null, length = null) => {
    const id = category + "/" + name; need(!seen.has(id), "unique diagnostic callback"); seen.add(id); callbacks.push({ category, name, degree, expanded, length });
  };
  function mark(name, degree, value) {
    const expected = menu[marks.length]; need(expected && expected.name === name && expected.degree === degree, "exact diagnostic mark order");
    recipe.Mark(name, degree, value, retention[name]); marks.push({ name, degree, node: recipe.RecordedNode(value), expanded: retention[name] });
  }
  const tensor = (name, degree, value, expanded = true) => { callback("Tensor", name, degree, expanded); mark("tensor/" + name, degree, value); };
  const ref = value => {
    const degree = recipe.TensorDegree(value);
    try { return { kind: "tensorNode", node: recipe.RecordedNode(value) }; }
    catch (e) { if (degree === -1 && e.message === "A68 recipe: tensor must already have a recorded node") return { kind: "literalZero" }; throw e; }
  };
  const equal = (a, b) => ({ kind: "tensorEqual", left: ref(a), right: ref(b) });
  const biEqual = (a, b) => all(FIELDS.map(field => equal(a[field], b[field])));
  const zero = value => ({ kind: "tensorZero", tensor: ref(value) });
  const check = (name, predicate) => { callback("Check", name); checks.push({ name, metadataPath: "check/" + name, predicate }); };
  const root = (name, expression) => { need(!rootNames.has(name), "unique scalar root name"); rootNames.add(name); scalarRoots.push({ name, expression }); if (!roots.has(expression)) roots.set(expression, name); return name; };
  const scalarRef = value => { need(roots.has(value), "scalar has a retained root"); return roots.get(value); };
  const scalarEqual = (a, b) => ({ kind: "scalarEqual", left: scalarRef(a), right: scalarRef(b) });
  const arrayEqual = (a, b) => { need(a.length === b.length, "scalar array equality length"); return { kind: "scalarArrayEqual", left: a.map(scalarRef), right: b.map(scalarRef) }; };
  const densityEqual = (a, b) => all(SCALARS.map(key => arrayEqual(a[key], b[key])));
  const pair = (a, b) => ({ op: "pair", left: recipe.RecordedNode(a), right: recipe.RecordedNode(b) });
  const arithmetic = (name, left, right) => check(name, { kind: "scalarEqual", left: root("check/" + name + "/left", left), right: root("check/" + name + "/right", right) });
  const scalarArray = (name, values) => { need(values.length === 4, "four diagnostic scalars"); callback("ScalarArray", name, null, null, 4); scalarArrays.push({ name, metadataPath: "scalars/" + name, roots: values.map((v, i) => root("scalars/" + name + "/" + i, v)) }); };
  function record(category, name, result) {
    const prefix = `structured/${category}/${name}`, fields = structuredFieldMenu(category), scalarFields = {}, booleanFields = {};
    const pathValue = path => {
      let value = result; const parts = path.split("/");
      for (let i = 0; i < parts.length; i++) if (parts[i] === "Primitives") {
        const name = parts[++i], p = result.Primitives.filter(p => p.Name === name); need(p.length === 1, "unique named Ward primitive"); value = p[0].Tensor;
      } else value = value[parts[i]];
      return value;
    };
    for (const field of fields) {
      const value = pathValue(field.name), path = prefix + "/" + field.name;
      if (field.category === "Tensor") mark(path, field.degree, value);
      else if (field.category === "ScalarArray") { need(value.length === field.length, "structured array length"); scalarFields[field.name] = value.map((v, i) => root(path + "/" + i, v)); }
      else if (field.category === "Scalar") scalarFields[field.name] = root(path, value);
      else { need(typeof value === "boolean", "structured boolean"); booleanFields[field.name] = value; }
    }
    structured.push({ category, name, metadataPath: prefix, fields, scalarFields, booleanFields }); callback(category, name);
  }
  function background(b, descriptor) {
    callback("GeometryLeaf", "background/curvature", 2); callback("Background", "background");
    geometryArtifacts.push({ metadataPath: "background/geometry", kind: "background", descriptor, numericalValidationEstablishedHere: false });
    for (const role of baselineRoles()) { const m = /^(\w+)\[(\d+)\]$/.exec(role.role); mark("background/" + role.role, role.degree, m ? b[m[1]][Number(m[2])] : b[role.role]); }
    for (const [name, degree] of [["NativeX", 1], ["DXOracle", 2], ["ReverseX", 1], ["Gradient", 1]]) mark("background/" + name, degree, b[name]);
    for (let mu = 0; mu < 14; mu++) for (const name of ["NativePartial", "NativePartialOracle", "CovariantCoordinate"]) mark(`background/${name}[${mu}]`, 1, b[name][mu]);
    for (let mu = 0; mu < 14; mu++) check("background/nativeJet/" + mu, equal(b.NativePartial[mu], b.NativePartialOracle[mu]));
    check("background/covariantExterior", equal(b.DX, b.DXOracle));
  }
  function germ(prefix, g, descriptor) {
    callback("GeometryLeaf", prefix + "/deltaCurvatureAdapted", 2); callback("GeometryLeaf", prefix + "/deltaCurvatureOracle", 2); callback("Geometry", prefix);
    geometryArtifacts.push({ metadataPath: prefix + "/geometry", kind: "germ", descriptor, numericalValidationEstablishedHere: false });
    GEO.forEach(([name, degree]) => mark(prefix + "/" + name, degree, g[name]));
  }
  function bindGeometry(id, entries) {
    const prior = geometry.find(g => g.id === id); if (prior) need(JSON.stringify(prior.entries) === JSON.stringify(entries), "consistent scalar geometry binding");
    else geometry.push({ id, entries });
  }
  const coefficient = (t, form, blade) => ({ tensor: ref(t), form, blade, k0: 0, k1: 0, absent: "0" });
  function finish(values) {
    need(marks.length === menu.length, "all diagnostic marks retained");
    need(checks.length === (diagnostic === "grade10" ? 1019 : 310), "complete source check menu");
    need(diagnosticCheckMenu(diagnostic).every(name => seen.has("Check/" + name)), "every independent named check required");
    need(scalarArrays.length === (diagnostic === "grade10" ? 10 : 3), "complete source scalar array menu");
    callback("End", "$");
    return freeze({ schemaVersion: SCHEMA, contextId: "diagnostic/" + diagnostic, callbacks, marks, checks, scalarRoots, scalarArrays, structured, geometry, domainChecks, geometryArtifacts, ...values,
      census: { tensorCallbacks: callbacks.filter(c => c.category === "Tensor").length, marks: marks.length, checks: checks.length,
        originalActions: structured.filter(s => s.category === "Original").length, wardActions: structured.filter(s => s.category === "Ward").length,
        accelerations: structured.filter(s => s.category === "Acceleration").length, scalarArrays: scalarArrays.length },
      scope: { completeDiagnosticExpressionAndCallbackRecipe: true, numericChecksEvaluated: false, geometryArtifactsValidatedHere: false,
        scientificSourceClosureEstablished: false, fullAuditRunClosed: false, totalResourceAdmissionProved: false },
      unresolved: ["independent geometry/curvature provenance and full retained geometry-artifact replay", "tensor/scalar/domain consumer replay",
        "enclosing run census and complete scientific validation", "whole-context and whole-run arithmetic/storage admission"] });
  }
  callback("Begin", "$");
  return { callback, tensor, ref, equal, biEqual, zero, check, root, scalarRef, scalarEqual, arrayEqual, densityEqual, pair, arithmetic,
    scalarArray, record, background, germ, bindGeometry, coefficient, finish, domainChecks };
}

function compareOriginal(c, prefix, a, b) {
  for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) c.check(`${prefix}/stage/${p}/${s}`, c.biEqual(a.Stages[p][s], b.Stages[p][s]));
  c.check(prefix + "/scalars", c.densityEqual(a, b));
}
function compareWard(c, prefix, a, b) {
  c.check(prefix + "/primitiveMenu", { kind: "orderedNamesEqual", left: a.Primitives.map(p => p.Name), right: b.Primitives.map(p => p.Name) });
  for (let p = 0; p < 33; p++) c.check(`${prefix}/primitive/${p}`, c.biEqual(a.Primitives[p].Tensor, b.Primitives[p].Tensor));
  for (let route = 0; route < 2; route++) {
    const key = route === 0 ? "Literal" : "Descended", first = a[key], second = b[key];
    for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) c.check(`${prefix}/density/${route}/stage/${p}/${s}`, c.biEqual(first.Stages[p][s], second.Stages[p][s]));
    for (let p = 0; p < 4; p++) c.check(`${prefix}/density/${route}/top/${p}`, c.biEqual(first.TopForms[p], second.TopForms[p]));
    c.check(`${prefix}/density/${route}/scalars`, c.densityEqual(first, second));
  }
}
function retainVariation(recipe, c, prefix, row, b) {
  for (const [name, field, degree] of FAMILIES) {
    for (let p = 0; p < 4; p++) c.tensor(`${prefix}/${name}/${PIECES[p]}`, degree, row[field][p]);
    c.tensor(`${prefix}/${name}/total`, degree, recipe.Sum(...row[field]));
  }
  SINGLES.forEach(([name, field, degree]) => c.tensor(prefix + "/" + name, degree, row[field]));
  c.check(prefix + "/covariantDerivative", all(row.DerivativeIdentityComparisons.map(([a, b]) => c.equal(a, b))));
  c.check(prefix + "/ordinaryAdjoint", c.equal(row.OrdinaryAdjoint, row.NativeAdjointAsFrame)); c.check(prefix + "/fixedAdjoint", c.equal(row.CAdjointFixed, row.CAdjointOracle));
  for (let p = 0; p < 4; p++) {
    c.check(`${prefix}/raw0/${p}`, all([c.equal(row.Raw0[p], row.FieldFirst0[p]), c.equal(row.Raw0[p], row.Oracle0[p])]));
    c.check(`${prefix}/raw2/${p}`, all([c.equal(row.Raw2[p], row.FieldFirst2[p]), c.equal(row.Raw2[p], row.Oracle2[p])]));
    c.check(`${prefix}/euler/${p}`, c.equal(row.EulerCovariant[p], row.EulerMoving[p]));
    c.check(`${prefix}/nativeEuler/${p}`, c.equal(row.NativeEuler[p], CoordinateDual(recipe, b.Frame, row.EulerCovariant[p])));
  }
  for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) {
    c.tensor(`${prefix}/current/${mu}/${nu}`, 0, row.CurrentCoefficients[mu][nu]);
    c.tensor(`${prefix}/currentPartial/${mu}/${nu}`, 0, row.CurrentPartialCoefficients[mu][nu], false);
    c.tensor(`${prefix}/green/${mu}/${nu}`, 0, row.GreenVariationCoefficients[mu][nu]);
    c.check(`${prefix}/current/${mu}/${nu}`, all([c.equal(row.CurrentCoefficients[mu][nu], row.GreenVariationCoefficients[mu][nu]),
      c.equal(row.CurrentCoefficients[mu][nu], exteriorComponent(recipe, row.Native2, mu, nu))]));
  }
}

function buildGradeTenDiagnosticRecipe(recipe, options) {
  const input = inputs(options, "grade10"), c = collector(recipe, "grade10", input.retention);
  const x = recipe.Sum(recipe.Unit(2, 1026), recipe.Unit(8, 992)), u = recipe.Unit(16, 2043);
  c.tensor("input/X", 1, x); c.tensor("input/UFrame", 1, u);
  const b = buildMixedBackground(recipe, { x, ...input.background }); c.background(b, input.background.geometry);
  const nativeU = recipe.Pullback(b.InverseFrame, u), nativeDu = recipe.FreshZero(); c.tensor("input/UNative", 1, nativeU); c.tensor("input/dUNative", 2, nativeDu);
  c.check("background/gradePreservingJets", all(b.NativePartial.map(t => ({ kind: "tensorBladeGrades", tensor: c.ref(t), allowed: [2, 5] }))));
  const weights = ["-1/2", "1/2", "-1/2", "-1/2"], sums = Array.from({ length: 8 }, () => Array.from({ length: 4 }, () => recipe.FreshZero()));
  const scalarSums = Array.from({ length: 2 }, () => Array.from({ length: 4 }, () => constant("0")));
  const combine = (matrices, scalars) => recipe.CombineMatrices([...matrices, ...Array(14 - matrices.length).fill(b.Identity)], [...scalars, ...Array(14 - scalars.length).fill(recipe.ScalarConstant("0"))]);
  let motion = combine([], []), adjoint = recipe.FreshZero(), movingAdjoint = recipe.FreshZero(); const rows = [], germs = [], originals = [];
  for (let basis = 0; basis < 4; basis++) {
    const prefix = "basis" + basis, g = buildMixedMetricGerm(recipe, b, input.germs[basis]); c.germ(prefix, g, input.germs[basis].geometry); germs.push(g);
    motion = combine([motion, g.Motion], [recipe.ScalarConstant("1"), recipe.ScalarConstant(weights[basis])]);
    const im = intermediateMenu(); let index = 0;
    const row = buildMixedVariation(recipe, b, g, (name, tensor) => { const expected = im[index++]; need(expected && expected.name === name, "exact intermediate callback order"); c.tensor(prefix + "/intermediate/" + name, expected.degree, tensor, false); });
    need(index === 108, "all108 variation intermediates"); rows.push(row); retainVariation(recipe, c, prefix, row, b);
    for (let f = 0; f < 8; f++) for (let p = 0; p < 4; p++) sums[f][p] = recipe.Sum(sums[f][p], recipe.Times(row[FAMILIES[f][1]][p], weights[basis]));
    adjoint = recipe.Sum(adjoint, recipe.Times(row.CAdjointFixed, weights[basis])); movingAdjoint = recipe.Sum(movingAdjoint, recipe.Times(row.MovingAdjointDelta, weights[basis]));
    const literal = buildOriginalMixedAction(recipe, b, g, nativeU, nativeDu), word = buildOriginalMixedAction(recipe, b, g, nativeU, nativeDu, true);
    originals.push([literal, word]); c.record("Original", prefix + "/original0", literal); c.record("Original", prefix + "/original1", word); compareOriginal(c, prefix + "/originalEquality", literal, word);
    for (let p = 0; p < 4; p++) {
      scalarSums[0][p] = add(scalarSums[0][p], multiply(literal.Mixed[p], constant(weights[basis]))); scalarSums[1][p] = add(scalarSums[1][p], multiply(word.Mixed[p], constant(weights[basis])));
      c.arithmetic(`${prefix}/rawOriginal/${p}`, literal.Mixed[p], c.pair(u, row.Raw0[p]));
    }
  }
  const motionId = "diagnostic.grade10.combinedMotion"; c.bindGeometry(motionId, recipe.MatrixEntries(motion));
  c.check("combined/realizableMotion", all(Array.from({ length: 196 }, (_, i) => ({ kind: "scalarRootEqualsConstant",
    root: c.root(`check/combined/realizableMotion/${i}`, { op: "matrixEntry", matrix: motionId, row: Math.floor(i / 14), column: i % 14 }), expected: i === 14 ? "1" : "0" }))));
  const forecast = ["0", "0", "4/3", "0"];
  const forecastCheck = (name, values) => c.check(name, all(values.map((value, i) => ({ kind: "scalarRootEqualsConstant", root: c.scalarRef(value), expected: forecast[i] }))));
  for (let route = 0; route < 2; route++) { c.scalarArray("combined/originalRoute" + route, scalarSums[route]); forecastCheck("combined/originalForecast" + route, scalarSums[route]); }
  for (let f = 0; f < 8; f++) {
    const [name, , degree] = FAMILIES[f], projection = [];
    for (let p = 0; p < 4; p++) { c.tensor(`combined/${name}/${PIECES[p]}`, degree, sums[f][p]); projection.push(degree === 1 ? c.pair(u, sums[f][p]) : constant("0")); }
    c.tensor(`combined/${name}/total`, degree, recipe.Sum(...sums[f])); c.scalarArray("combined/projection/" + name, projection);
    if (degree === 1) forecastCheck("combined/forecast/" + name, projection);
  }
  const motionX = recipe.Motion(motion, x), cross = recipe.Sum(recipe.P(x, u), recipe.P(u, x)), motionCross = recipe.Motion(motion, cross);
  const reverse = recipe.FixedAdjoint(motionX), reverseWord = recipe.FixedAdjoint(motionX), pairingAdjoint = recipe.PairingMotion(motion, b.AdjointX);
  const adjointPhi = recipe.FixedAdjoint(recipe.Phi1), pairingAdjointPhi = recipe.PairingMotion(motion, adjointPhi);
  const hand = [motionX, cross, motionCross, reverse, reverseWord, adjoint, movingAdjoint, pairingAdjoint, adjointPhi, pairingAdjointPhi];
  hand.forEach((value, i) => c.tensor("combined/" + HAND[i], i === 0 ? 1 : 2, value));
  const fdiag = recipe.Unit(24, 1051), stages = recipe.FixedForward(fdiag), words = recipe.FixedForward(fdiag);
  for (let s = 0; s < 8; s++) { c.tensor("forwardHand/literal/" + s, DEGREES[s], stages[s]); c.tensor("forwardHand/word/" + s, DEGREES[s], words[s]); c.check("forwardHand/stage/" + s, c.equal(stages[s], words[s])); }
  c.arithmetic("hand/probeNorm", c.pair(u, u), constant("1")); c.check("hand/quadraticZero", c.zero(b.Q));
  c.check("hand/motionX", c.equal(motionX, recipe.Unit(1, 1026))); c.check("hand/crossQ", c.equal(cross, recipe.Times(fdiag, "-2")));
  c.check("hand/motionCrossZero", c.zero(motionCross)); c.check("hand/reverseWord", c.equal(reverse, reverseWord));
  const coefficientCheck = (name, tensor, form, blade, real) => c.check(name, { kind: "complexCoefficientEqualsConstant", coefficient: c.coefficient(tensor, form, blade), real, imaginary: "0" });
  coefficientCheck("hand/reverseCoefficient", reverse, 24, 1051, "-2"); coefficientCheck("hand/forwardCoefficient", stages[7], 1, 1026, "2");
  c.check("hand/movingAdjointPairing", c.equal(adjoint, recipe.Sum(movingAdjoint, pairingAdjoint)));
  c.check("hand/adjointPhi", c.equal(adjointPhi, recipe.Times(recipe.Phi2, "-24"))); coefficientCheck("hand/nonzeroV2", pairingAdjointPhi, 5, 6, "-24");
  return c.finish({ background: b, germs, variations: rows, originals, combinedMotion: { id: motionId, entries: recipe.MatrixEntries(motion) }, sums });
}

function buildAccelerationDiagnosticRecipe(recipe, options) {
  const input = inputs(options, "acceleration"), c = collector(recipe, "acceleration", input.retention);
  const x = recipe.Unit(128, 1), eta = recipe.Unit(0, 16), deta = recipe.FreshZero();
  c.tensor("input/X", 1, x); c.tensor("input/eta", 0, eta); c.tensor("input/dEta", 1, deta);
  const b = buildMixedBackground(recipe, { x, ...input.background }); c.background(b, input.background.geometry);
  const g = buildMixedMetricGerm(recipe, b, input.germs[0]); c.germ("germ", g, input.germs[0].geometry);
  const forecast = ["0", "0", "0", "-85098"].map(constant); c.scalarArray("forecast/original", forecast); const evaluations = []; let first;
  need(typeof wardInputDomains === "function" && typeof accelerationInputDomains === "function", "reviewed Ward/acceleration domain helpers required");
  for (let route = 0; route < 2; route++) {
    const oracle = route === 1, prefix = "route" + route;
    const epsilon = buildMixedWardAction(recipe, b, g, eta, deta, false, oracle), compensated = buildMixedWardAction(recipe, b, g, eta, deta, true, oracle);
    const acceleration = buildMixedWardAcceleration(recipe, b, g, { eta, nativeDeta: deta, oracle });
    c.domainChecks.push(...wardInputDomains(recipe, b, g, eta, deta, prefix + "/epsilon"), ...wardInputDomains(recipe, b, g, eta, deta, prefix + "/compensated"), ...accelerationInputDomains(recipe, b, g, eta, deta, prefix + "/acceleration"));
    c.record("Ward", prefix + "/epsilon", epsilon); c.record("Ward", prefix + "/compensated", compensated); c.record("Acceleration", prefix + "/acceleration", acceleration);
    const nativeU = recipe.Pullback(b.InverseFrame, compensated.Tangent), nativeDu = recipe.Pullback(b.InverseFrame, recipe.Sum(compensated.DBTangent, recipe.Times(recipe.P(b.B, compensated.Tangent, "C"), "-1")));
    const original = buildOriginalMixedAction(recipe, b, g, nativeU, nativeDu, oracle); c.record("Original", prefix + "/fixedNativeTangent", original); c.scalarArray(prefix + "/nativeTangent", original.Mixed);
    acceleration.scalarPlan.geometry.forEach(v => c.bindGeometry(v.id, v.entries)); evaluations.push({ epsilon, compensated, acceleration, original, nativeU: c.ref(nativeU), nativeDu: c.ref(nativeDu) });
    c.check(prefix + "/forecast", c.arrayEqual(acceleration.Original, forecast));
    const expectedW = recipe.Times(recipe.Sum(recipe.Unit(1, 128), recipe.Unit(128, 1)), "-3/32");
    c.check(prefix + "/accelerationCoefficient", c.equal(acceleration.W, expectedW));
    c.check(prefix + "/accelerationJets", all([c.equal(acceleration.Dw, acceleration.DwFromJet), c.equal(acceleration.DBw, acceleration.DBwFromJet)]));
    for (const row of [epsilon, compensated]) {
      const tag = prefix + (row.Compensated ? "/compensated" : "/epsilon"), named = name => row.Primitives.find(p => p.Name === name).Tensor;
      c.check(tag + "/descent", c.densityEqual(row.Literal, row.Descended)); c.check(tag + "/derivative", c.biEqual(named("descendedDerivative"), named("descendedDerivativeOracle")));
      c.check(tag + "/curvature", all([c.biEqual(named("DBSquaredEpsilon"), named("DBSquaredEpsilonExpanded")), c.equal(named("DBSquaredEpsilon").HU, named("curvatureVariationCommutator").HU)]));
      for (const suffix of ["First", "Outer", "Inner"]) c.check(tag + "/phi" + suffix, c.biEqual(named("descendedPhi" + suffix), named("unrotatedPhi" + suffix)));
    }
    for (let p = 0; p < 4; p++) {
      c.arithmetic(`${prefix}/ward/${p}`, add(compensated.Literal.Mixed[p], acceleration.Original[p]), constant("0"));
      c.arithmetic(`${prefix}/epsilonWard/${p}`, add(add(epsilon.Literal.Mixed[p], original.Mixed[p]), acceleration.Original[p]), constant("0"));
      c.arithmetic(`${prefix}/green/${p}`, acceleration.Original[p], add(acceleration.Euler[p], p === 1 ? acceleration.Divergence : constant("0")));
    }
    if (route === 0) { first = { epsilon, compensated, acceleration, original }; continue; }
    compareWard(c, "word/epsilon", first.epsilon, epsilon); compareWard(c, "word/compensated", first.compensated, compensated); compareOriginal(c, "word/original", first.original, original);
    const a = first.acceleration;
    c.check("word/acceleration/value", all(["W", "Dw", "DBw", "CrossQ"].map(key => c.equal(a[key], acceleration[key]))));
    c.check("word/acceleration/scalars", all([... ["Original", "Euler", "Current", "CoordinateCurrent"].map(key => c.arrayEqual(a[key], acceleration[key])), c.scalarEqual(a.Divergence, acceleration.Divergence)]));
    for (let mu = 0; mu < 14; mu++) c.check("word/acceleration/jet/" + mu, all([
      ...["PartialW", "CovariantW", "DeltaBPartialInFrame", "CovariantAdjointX"].map(key => c.equal(a[key][mu], acceleration[key][mu])), c.arrayEqual(a.CurrentCovariantDerivative[mu], acceleration.CurrentCovariantDerivative[mu]) ]));
    for (let p = 0; p < 2; p++) for (let s = 0; s < 8; s++) c.check(`word/acceleration/stage/${p}/${s}`, c.equal(a.Stages[p][s], acceleration.Stages[p][s]));
  }
  return c.finish({ background: b, germ: g, evaluations });
}

module.exports = { SCHEMA, diagnosticMarkMenu, diagnosticCheckMenu, buildGradeTenDiagnosticRecipe, buildAccelerationDiagnosticRecipe };
