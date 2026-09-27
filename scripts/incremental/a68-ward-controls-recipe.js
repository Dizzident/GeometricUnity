"use strict";

// Exact metadata transcription of MixedWardControls.Evaluate and the three
// MixedStructuredFields traversals. This constructs expressions, NOT tensor
// coefficients, successful scientific checks or a source-geometry certificate.
// Inputs must be independently source-bound session handles, never observed
// graph parameters. The six selected germs contain twelve parameter contexts,
// twenty-four route combinations and forty-eight Ward evaluations; this finite
// diagnostic menu is NOT an enumeration/proof for arbitrary gauge parameters.
const { MixedRecipe } = require("./a68-mixed-recipe");
const { multiindices } = require("./a68-background-recipe");
const { FIELDS, buildOriginalMixedAction } = require("./a68-original-action-recipe");
const { buildMixedWardAction } = require("./a68-ward-recipe");
const { buildMixedWardAcceleration } = require("./a68-acceleration-recipe");
const SCHEMA = "phase627-ward-controls-recipe-v1";
const DEGREES = [2, 12, 13, 14, 0, 1, 13, 1];
const SCALAR_FIELDS = ["Value", "Metric", "Field", "Mixed"];
const PRIMITIVES = ["epsilon", "inverse", "epsilonInverse", "inverseEpsilon", "referenceCurvature",
  "DBepsilon", "DBinverse", "DBSquaredEpsilon", "DBSquaredEpsilonExpanded", "curvatureVariationCommutator",
  "gaugeDifference", "DBgaugeDifference", "varpi", "DBvarpi", "T", "DBT", "rotatedCurvature", "covariantT", "quadraticT",
  "unrotatedPhiFirst", "unrotatedPhiOuter", "unrotatedPhiInner", "phiFirst", "phiOuter", "phiInner", "descendedT",
  "descendedDerivative", "descendedDerivativeOracle", "descendedCurvature", "descendedQuadratic",
  "descendedPhiFirst", "descendedPhiOuter", "descendedPhiInner"];
const PRIMITIVE_DEGREES = [0, 0, 0, 0, 2, 1, 1, 2, 2, 2, 1, 2, 1, 2, 1, 2, 2, 2, 2, 1, 1, 2, 1, 1, 2, 1, 2, 2, 2, 2, 1, 1, 2];
const need = (condition, reason) => { if (!condition) throw new Error("A68 Ward controls recipe: " + reason); };
const freeze = value => {
  if (value && typeof value === "object" && !Object.isFrozen(value)) {
    Object.values(value).forEach(freeze); Object.freeze(value);
  }
  return value;
};
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const add = (left, right) => freeze({ op: "add", left, right });
const zero = () => Object.freeze({ op: "constant", value: "0" });
function data(object, key) {
  const property = object && Object.getOwnPropertyDescriptor(object, key);
  need(property && Object.hasOwn(property, "value") && property.enumerable, "own data field " + key);
  return property.value;
}
function array(value, size, label) {
  need(Array.isArray(value) && value.length === size && Reflect.ownKeys(value).length === size + 1, "exact array " + label);
  return Object.freeze(Array.from({ length: size }, (_, i) => data(value, String(i))));
}
function selection(point, jet) {
  need(point === 0 || point === 1, "point in fixed menu");
  const multis = multiindices();
  need(Number.isSafeInteger(jet) && jet >= 0 && jet < multis.length &&
    [[1, 0, 0, 0], [0, 2, 0, 0], [0, 3, 0, 0]].some(m => same(m, multis[jet])), "jet in fixed Ward menu");
  return multis[jet];
}
function wardControlsMenu() {
  const contexts = [];
  for (let point = 0; point < 2; point++) multiindices().forEach((multiindex, jetIndex) => {
    if (![[1, 0, 0, 0], [0, 2, 0, 0], [0, 3, 0, 0]].some(m => same(m, multiindex))) return;
    contexts.push({ point, metricBasis: 0, jetIndex, multiindex,
      parameters: [0, 1].map(parameter => ({ parameter, name: `p${point}_j${jetIndex}_eta${parameter}`, routes: [0, 1] })) });
  });
  return freeze({ contexts, germContexts: 6, parameterContexts: 12, routeCombinations: 24,
    wardEvaluations: 48, accelerationEvaluations: 24, originalEvaluations: 24 });
}

// This is execution order, not MixedAuditPlan's set-only census ordering.
function structuredFieldMenu(category) {
  const out = [];
  const tensor = (name, degree) => out.push({ category: "Tensor", name, degree, expanded: null, length: null });
  const scalarArray = (name, length) => out.push({ category: "ScalarArray", name, degree: null, expanded: null, length });
  const scalar = name => out.push({ category: "Scalar", name, degree: null, expanded: null, length: null });
  const boolean = name => out.push({ category: "Boolean", name, degree: null, expanded: null, length: null });
  const bi = (name, degree) => FIELDS.forEach(field => tensor(name + "/" + field, degree));
  const stages = (name, count, isBi) => {
    for (let p = 0; p < count; p++) for (let s = 0; s < 8; s++)
      (isBi ? bi : tensor)(`${name}/${p}/${s}`, DEGREES[s]);
  };
  const scalars = prefix => SCALAR_FIELDS.forEach(name => scalarArray(prefix + name, 4));
  if (category === "Ward") {
    boolean("Compensated"); boolean("Oracle"); tensor("Eta", 0);
    ["NativeDeta", "Deta", "DBeta", "Tangent"].forEach(name => tensor(name, 1)); tensor("DBTangent", 2);
    PRIMITIVES.forEach((name, i) => bi("Primitives/" + name, PRIMITIVE_DEGREES[i]));
    for (const density of ["Literal", "Descended"]) {
      stages(density + "/Stages", 3, true);
      for (let p = 0; p < 4; p++) bi(`${density}/TopForms/${p}`, 14);
      scalars(density + "/");
    }
  } else if (category === "Acceleration") {
    boolean("Oracle"); tensor("Eta", 0); tensor("NativeDeta", 1); tensor("W", 1);
    ["Dw", "DBw", "DwFromJet", "DBwFromJet", "CrossQ"].forEach(name => tensor(name, 2));
    for (const family of ["PartialW", "CovariantW", "DeltaBPartialInFrame", "CovariantAdjointX"])
      for (let mu = 0; mu < 14; mu++) tensor(family + "/" + mu, family === "CovariantAdjointX" ? 2 : 1);
    stages("Stages", 2, false);
    for (const name of ["Original", "Euler"]) scalarArray(name, 4);
    for (const name of ["Current", "CoordinateCurrent"]) scalarArray(name, 14);
    for (let mu = 0; mu < 14; mu++) scalarArray("CurrentCovariantDerivative/" + mu, 14);
    scalar("Divergence");
  } else if (category === "Original") { stages("Stages", 3, true); scalars(""); }
  else need(false, "closed structured category");
  return freeze(out);
}
function wardMarkMenu(point, jetIndex) {
  selection(point, jetIndex); const out = [];
  for (let parameter = 0; parameter < 2; parameter++) for (let route = 0; route < 2; route++) {
    const name = `p${point}_j${jetIndex}_eta${parameter}_route${route}`;
    for (const [category, suffix] of [["Ward", "_epsilon"], ["Ward", "_compensated"], ["Acceleration", ""], ["Original", "_fixedNativeTangent"]])
      for (const field of structuredFieldMenu(category)) if (field.category === "Tensor")
        out.push({ name: `structured/${category}/${name}${suffix}/${field.name}`, degree: field.degree, requiredExpanded: null });
  }
  return freeze(out);
}
function retentionSnapshot(retention, menu) {
  need(retention && typeof retention === "object" && !Array.isArray(retention) && Reflect.ownKeys(retention).length === menu.length, "exact retention census");
  return Object.freeze(Object.fromEntries(menu.map(item => {
    const value = data(retention, item.name); need(typeof value === "boolean", "explicit boolean retention"); return [item.name, value];
  })));
}

// Mirror EVERY Local call, including repeated/aliased inputs. These are
// numerical obligations, not evidence callbacks or extra traced operations.
// Call after the corresponding builder has registered any lazy empty inputs;
// RecordedNode deliberately refuses to invent a node for an unrecorded zero.
function inputDomains(recipe, prefix, entries) {
  need(recipe instanceof MixedRecipe && typeof prefix === "string" && /^[\x20-\x7e]+$/.test(prefix) && prefix.trim().length > 0,
    "recorded input-domain session and prefix");
  return freeze(entries.map(([role, degree, tensor]) => {
    const actual = recipe.TensorDegree(tensor);
    need(actual === -1 || actual === degree, "Local input form degree " + role);
    return { name: prefix + "/" + role, node: recipe.RecordedNode(tensor), degree,
      local: true, hAntiHermitian: true, canonicalNonzeroRecords: true };
  }));
}
function wardInputDomains(recipe, background, germ, eta, nativeDeta, prefix) {
  return inputDomains(recipe, prefix, [
    ["eta", 0, eta], ["nativeDeta", 1, nativeDeta],
    ...[["X", 1], ["B", 1], ["F", 2], ["DX", 2]].map(([role, degree]) => [role, degree, data(background, role)]),
    ...[["DeltaB", 1], ["DeltaFFixed", 2], ["DeltaBExterior", 2], ["DeltaCurvatureFromConnection", 2]].map(([role, degree]) => [role, degree, data(germ, role)])
  ]);
}
function accelerationInputDomains(recipe, background, germ, eta, nativeDeta, prefix) {
  const inputs = array(data(background, "KInputs"), 3, "Local KInputs"), gradient = array(data(background, "GradientPieces"), 4, "Local GradientPieces");
  return inputDomains(recipe, prefix, [
    ["eta", 0, eta], ["nativeDeta", 1, nativeDeta], ["DeltaB", 1, data(germ, "DeltaB")], ["DeltaBExterior", 2, data(germ, "DeltaBExterior")],
    ...[["X", 1], ["B", 1], ["F", 2], ["DX", 2], ["Q", 2], ["AdjointX", 2]].map(([role, degree]) => [role, degree, data(background, role)]),
    ...inputs.map((tensor, i) => ["KInputs[" + i + "]", 1, tensor]),
    ...gradient.map((tensor, i) => ["GradientPieces[" + i + "]", 1, tensor])
  ]);
}

function buildWardControlsRecipe(recipe, background, germ, { retention } = {}) {
  need(recipe instanceof MixedRecipe, "MixedRecipe session required");
  // Snapshot only the used own-data fields, preserving opaque session identity.
  // No user callback is invoked here; geometry/source validation is a prerequisite.
  const b = {}, g = {};
  for (const name of ["Point", "Frame", "InverseFrame", "Identity", "X", "B", "F", "DX", "Q", "AdjointX", "NativeExterior"]) b[name] = data(background, name);
  for (const [name, length] of [["FramePartial", 14], ["Omega", 14], ["KInputs", 3], ["GradientPieces", 4]]) b[name] = array(data(background, name), length, name);
  for (const name of ["MetricBasis", "JetIndex", "Motion", "DeltaB", "DeltaFFixed", "DeltaBExterior", "DeltaCurvatureFromConnection"]) g[name] = data(germ, name);
  g.Multiindex = array(data(germ, "Multiindex"), 4, "Multiindex");
  g.DeltaOmega = array(data(germ, "DeltaOmega"), 14, "DeltaOmega");
  g.DeltaOmegaPartial = Object.freeze(array(data(germ, "DeltaOmegaPartial"), 14, "DeltaOmegaPartial").map(row => array(row, 14, "DeltaOmegaPartial row")));
  need(g.MetricBasis === 0 && same(selection(b.Point, g.JetIndex), g.Multiindex), "fixed metric/jet multiindex binding");
  const menu = wardMarkMenu(b.Point, g.JetIndex), policy = retentionSnapshot(retention, menu);
  const marks = [], callbacks = [], structured = [], checks = [], domainChecks = [], scalarRoots = [], evaluations = [], geometry = [];
  const rootNames = new Set(), expressions = new WeakMap(), checkNames = new Set();
  const root = (name, expression) => {
    need(!rootNames.has(name), "unique scalar root"); rootNames.add(name); scalarRoots.push({ name, expression });
    if (!expressions.has(expression)) expressions.set(expression, name); return name;
  };
  const ref = expression => { const name = expressions.get(expression); need(name !== undefined, "check scalar was retained by structured traversal"); return name; };
  const tensorRef = tensor => ({ kind: "tensorNode", node: recipe.RecordedNode(tensor) });
  const tensorEqual = (a, z) => ({ kind: "tensorEqual", left: tensorRef(a), right: tensorRef(z) });
  const all = predicates => ({ kind: "all", predicates });
  const biEqual = (a, z) => all(FIELDS.map(field => tensorEqual(a[field], z[field])));
  const scalarEqual = (a, z) => ({ kind: "scalarEqual", left: ref(a), right: ref(z) });
  const arrayEqual = (a, z) => {
    need(a.length === z.length, "scalar array comparison length");
    return { kind: "scalarArrayEqual", left: a.map(ref), right: z.map(ref) };
  };
  const densitiesEqual = (a, z) => all(SCALAR_FIELDS.map(field => arrayEqual(a[field], z[field])));
  const check = (name, predicate) => {
    need(!checkNames.has(name), "unique check name"); checkNames.add(name);
    checks.push({ name, metadataPath: "check/" + name, predicate });
    callbacks.push({ category: "Check", name, degree: null, expanded: null, length: null });
  };
  const arithmeticCheck = (name, left, right) => {
    const leftRoot = root("check/" + name + "/left", left), rightRoot = root("check/" + name + "/right", right);
    check(name, { kind: "scalarEqual", left: leftRoot, right: rightRoot });
  };
  function record(category, name, result) {
    const prefix = `structured/${category}/${name}`, fields = structuredFieldMenu(category), scalarFields = {}, booleanFields = {};
    if (category === "Ward") need(same(result.Primitives.map(p => p.Name), PRIMITIVES), "exact ordered Ward primitive menu");
    const valueAt = path => {
      let current = result;
      const segments = path.split("/");
      for (let i = 0; i < segments.length; i++) {
        if (segments[i] === "Primitives") { current = result.Primitives[PRIMITIVES.indexOf(segments[++i])].Tensor; }
        else current = current[segments[i]];
      }
      return current;
    };
    for (const field of fields) {
      const value = valueAt(field.name), path = prefix + "/" + field.name;
      if (field.category === "Tensor") {
        const expected = menu[marks.length];
        need(expected && expected.name === path && expected.degree === field.degree, "exact structured mark order");
        recipe.Mark(path, field.degree, value, policy[path]);
        marks.push({ name: path, degree: field.degree, node: recipe.RecordedNode(value), expanded: policy[path] });
      } else if (field.category === "ScalarArray") {
        need(value.length === field.length, "exact structured scalar array");
        scalarFields[field.name] = value.map((expression, i) => root(path + "/" + i, expression));
      } else if (field.category === "Scalar") scalarFields[field.name] = root(path, value);
      else { need(typeof value === "boolean", "structured boolean"); booleanFields[field.name] = value; }
    }
    structured.push({ category, name, metadataPath: prefix, fields, scalarFields, booleanFields });
    callbacks.push({ category, name, degree: null, expanded: null, length: null });
  }
  const primitive = (row, name) => row.Primitives[PRIMITIVES.indexOf(name)].Tensor;
  // C# constructs both eta controls and the native first jet BEFORE the loops.
  const parameters = [recipe.Unit(0, 1 << 4, "1"), recipe.Unit(0, 3, "1")], nativeDeta = recipe.Unit(1, 1 << 1, "1");
  for (let parameter = 0; parameter < 2; parameter++) {
    const eta = parameters[parameter], prefix = `p${b.Point}_j${g.JetIndex}_eta${parameter}`; let literal;
    for (let route = 0; route < 2; route++) {
      const oracle = route === 1, name = prefix + "_route" + route;
      const epsilon = buildMixedWardAction(recipe, b, g, eta, nativeDeta, false, oracle);
      domainChecks.push(...wardInputDomains(recipe, b, g, eta, nativeDeta, "wardInput/" + name + "_epsilon"));
      const compensated = buildMixedWardAction(recipe, b, g, eta, nativeDeta, true, oracle);
      domainChecks.push(...wardInputDomains(recipe, b, g, eta, nativeDeta, "wardInput/" + name + "_compensated"));
      const acceleration = buildMixedWardAcceleration(recipe, b, g, { eta, nativeDeta, oracle });
      domainChecks.push(...accelerationInputDomains(recipe, b, g, eta, nativeDeta, "accelerationInput/" + name));
      record("Ward", name + "_epsilon", epsilon); record("Ward", name + "_compensated", compensated); record("Acceleration", name, acceleration);
      const nativeTangent = recipe.Pullback(b.InverseFrame, compensated.Tangent);
      const nativeDTangent = recipe.Pullback(b.InverseFrame, recipe.Sum(compensated.DBTangent, recipe.Times(recipe.P(b.B, compensated.Tangent, "C"), "-1")));
      const field = buildOriginalMixedAction(recipe, b, g, nativeTangent, nativeDTangent, oracle);
      record("Original", name + "_fixedNativeTangent", field);
      if (geometry.length === 0) geometry.push(...acceleration.scalarPlan.geometry);
      else need(same(geometry, acceleration.scalarPlan.geometry), "one independently bound scalar geometry");
      evaluations.push({ name, parameter, route, nativeTangent: tensorRef(nativeTangent), nativeDTangent: tensorRef(nativeDTangent), epsilon, compensated, acceleration, field });
      for (const row of [epsilon, compensated]) {
        const tag = name + (row.Compensated ? "_compensated" : "_epsilon");
        for (const part of SCALAR_FIELDS) check(tag + "_density" + part, arrayEqual(row.Literal[part], row.Descended[part]));
        check(tag + "_descendedDerivative", biEqual(primitive(row, "descendedDerivative"), primitive(row, "descendedDerivativeOracle")));
        check(tag + "_curvatureSquare", biEqual(primitive(row, "DBSquaredEpsilon"), primitive(row, "DBSquaredEpsilonExpanded")));
        check(tag + "_curvatureVariation", tensorEqual(primitive(row, "DBSquaredEpsilon").HU, primitive(row, "curvatureVariationCommutator").HU));
        for (const suffix of ["First", "Outer", "Inner"]) check(tag + "_solder" + suffix, biEqual(primitive(row, "descendedPhi" + suffix), primitive(row, "unrotatedPhi" + suffix)));
      }
      check(name + "_accelerationExteriorJet", all([tensorEqual(acceleration.Dw, acceleration.DwFromJet), tensorEqual(acceleration.DBw, acceleration.DBwFromJet)]));
      for (let piece = 0; piece < 4; piece++) {
        arithmeticCheck(name + "_compensatedWard_" + piece, add(compensated.Literal.Mixed[piece], acceleration.Original[piece]), zero());
        arithmeticCheck(name + "_epsilonMixedWard_" + piece, add(add(epsilon.Literal.Mixed[piece], field.Mixed[piece]), acceleration.Original[piece]), zero());
        arithmeticCheck(name + "_accelerationGreen_" + piece, acceleration.Original[piece], add(acceleration.Euler[piece], piece === 1 ? acceleration.Divergence : zero()));
      }
      if (route === 0) { literal = { epsilon, compensated, acceleration, field }; continue; }
      for (const [firstRow, secondRow] of [[literal.epsilon, epsilon], [literal.compensated, compensated]]) {
        const suffix = secondRow.Compensated ? "C" : "E";
        check(name + "_primitiveMenu" + suffix, { kind: "orderedNamesEqual", left: firstRow.Primitives.map(p => p.Name), right: secondRow.Primitives.map(p => p.Name) });
        for (let p = 0; p < PRIMITIVES.length; p++) check(name + "_wordPrimitive_" + suffix + p, biEqual(firstRow.Primitives[p].Tensor, secondRow.Primitives[p].Tensor));
        const tag = name + "_wordDensity_" + suffix;
        for (let density = 0; density < 2; density++) {
          const key = density === 0 ? "Literal" : "Descended", first = firstRow[key], second = secondRow[key];
          for (let piece = 0; piece < 3; piece++) for (let stage = 0; stage < 8; stage++) check(`${tag}_${density}_p${piece}_s${stage}`, biEqual(first.Stages[piece][stage], second.Stages[piece][stage]));
          for (let piece = 0; piece < 4; piece++) check(`${tag}_${density}_top${piece}`, biEqual(first.TopForms[piece], second.TopForms[piece]));
          check(`${tag}_${density}_scalars`, densitiesEqual(first, second));
        }
      }
      for (let piece = 0; piece < 3; piece++) for (let stage = 0; stage < 8; stage++) check(`${name}_wordField_p${piece}_s${stage}`, biEqual(literal.field.Stages[piece][stage], field.Stages[piece][stage]));
      check(name + "_wordFieldScalars", densitiesEqual(literal.field, field));
      const a = literal.acceleration;
      check(name + "_wordAcceleration", all([arrayEqual(a.Original, acceleration.Original), arrayEqual(a.Euler, acceleration.Euler), arrayEqual(a.Current, acceleration.Current), scalarEqual(a.Divergence, acceleration.Divergence)]));
      check(name + "_wordAccelerationTensors", all(["W", "Dw", "DBw", "CrossQ"].map(key => tensorEqual(a[key], acceleration[key]))));
      for (let mu = 0; mu < 14; mu++) check(`${name}_wordAccelerationJet${mu}`, all([
        ...["PartialW", "CovariantW", "DeltaBPartialInFrame", "CovariantAdjointX"].map(key => tensorEqual(a[key][mu], acceleration[key][mu])),
        arrayEqual(a.CurrentCovariantDerivative[mu], acceleration.CurrentCovariantDerivative[mu]) ]));
      for (let piece = 0; piece < 2; piece++) for (let stage = 0; stage < 8; stage++) check(`${name}_wordAcceleration_p${piece}_s${stage}`, tensorEqual(a.Stages[piece][stage], acceleration.Stages[piece][stage]));
    }
  }
  need(marks.length === 3600 && checks.length === 614 && domainChecks.length === 148 && structured.length === 16 && callbacks.length === 630 && scalarRoots.length === 1348, "complete fixed local census");
  return freeze({ schemaVersion: SCHEMA, contextId: `point${b.Point}/m0_j${g.JetIndex}`, callbacks, structured, marks, checks, domainChecks, scalarRoots, geometry, evaluations,
    census: { parameterContexts: 2, routeCombinations: 4, wardEvaluations: 8, accelerationEvaluations: 4, originalEvaluations: 4,
      tensorMarks: 3600, structuredScalarRoots: 1252, checkArithmeticRoots: 96, currentDerivativeScalarSlots: 784, checks: 614, localDomainObligations: 148 },
    scope: { completeSelectedGermWardDag: true, completeSelectedGermWardMarkMenu: true, numericChecksEvaluated: false,
      sourceGeometryValidatedHere: false, geometryErrorBoundsEvaluated: false, fullAuditLifecycle: false,
      symmetricSecondJetsIncluded: false, arbitraryEtaProofEstablished: false, totalMemoryOrWorkProof: false },
    unresolved: ["independent source binding of all input handles and geometry", "tensor/scalar predicates and retained-value replay",
      "geometry/error-bound checks in enclosing audit", "enclosing EndGerm/point/run census", "separate symmetric second-jet controls and arbitrary-eta proof",
      "outer metadata, scalar/tensor liveness and total resource admission"] });
}

module.exports = { SCHEMA, wardControlsMenu, structuredFieldMenu, wardMarkMenu, wardInputDomains, accelerationInputDomains, buildWardControlsRecipe };
