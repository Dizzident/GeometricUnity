"use strict";

// COMPLETE algebra-expression order for MixedBackground/MixedMetricGerm ONLY.
// This is metadata: no tensor coefficients, metric/connection/curvature
// arithmetic, or scientific evaluation. Geometry values must be independently
// reconstructed and source-bound BEFORE calling this module. A descriptor,
// matching hashes or a callback that merely echoes true is NOT that proof.
// Never populate this API from an observed DAG or its recorded parameters.
const BACKGROUND_SCHEMA = "phase627-independent-background-geometry-v1";
const GERM_SCHEMA = "phase627-independent-germ-geometry-v1";
const need = (condition, message) => { if (!condition) throw new Error("A68 background recipe: " + message); };
const equal = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const named = x => typeof x === "string" && /^[\x20-\x7e]+$/.test(x) && x.trim().length > 0;
const shape = (x, keys) => need(x !== null && typeof x === "object" && !Array.isArray(x) && equal(Object.keys(x), keys), "closed ordered descriptor");
const freeze = x => { if (x !== null && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const integer = (x, min, max) => Number.isSafeInteger(x) && x >= min && x <= max;
const typed = (recipe, value, d) => { need(recipe.TensorDegree(value) === d, "exact boundary tensor degree " + d); return value; };
function fourteen(values, label) { need(Array.isArray(values) && values.length === 14, label + " has all14 slots"); return values; }
function validateDescriptor(descriptor, keys, schema) {
  shape(descriptor, keys); need(descriptor.schemaVersion === schema && integer(descriptor.point, 0, 1), "geometry schema/point");
  need(Array.isArray(descriptor.provenance) && descriptor.provenance.length > 0, "explicit geometry source closure required");
  const seen = new Set();
  for (const entry of descriptor.provenance) {
    shape(entry, ["id", "source", "sha256"]);
    need(named(entry.id) && named(entry.source) && typeof entry.sha256 === "string" && /^[0-9a-f]{64}$/.test(entry.sha256) && !seen.has(entry.id), "unique geometry provenance descriptor"); seen.add(entry.id);
  }
  return freeze(JSON.parse(JSON.stringify(descriptor)));
}
function validateGeometryValues(recipe, descriptor, validator) {
  need(typeof validator === "function", "independent geometry/provenance validator required");
  const names = new Set();
  const collect = value => { if (Array.isArray(value)) value.forEach(collect); else { need(named(value), "named geometry binding"); names.add(value); } };
  Object.values(descriptor.matrices).forEach(collect);
  const values = freeze(Object.fromEntries([...names].sort().map(id => [id, recipe.MatrixEntries(recipe.Matrix(id))])));
  // Compare ACTUAL resolved values as well as role names/provenance. Otherwise
  // a recipe could bind the expected name to an unrelated matrix unnoticed.
  need(validator(descriptor, values) === true, "independent geometry/source closure rejected");
}
function resolveMatrices(recipe, names, single, arrays, squares = []) {
  shape(names, [...single, ...arrays, ...squares]); const result = {};
  const resolve = id => { need(named(id), "named predeclared geometry matrix"); return recipe.Matrix(id); };
  for (const field of single) result[field] = resolve(names[field]);
  for (const field of arrays) result[field] = fourteen(names[field], field).map(resolve);
  for (const field of squares) result[field] = fourteen(names[field], field).map(row => fourteen(row, field + " row").map(resolve));
  return result;
}
function backgroundGeometry(recipe, descriptor, validator) {
  const d = validateDescriptor(descriptor, ["schemaVersion", "point", "provenance", "matrices"], BACKGROUND_SCHEMA);
  const matrices = resolveMatrices(recipe, d.matrices, ["Frame", "InverseFrame", "Identity"], ["FrameLift", "FramePartial", "Omega", "InversePartial", "ConnectionInFrame"]);
  for (let i = 0; i < 14; i++) for (let j = 0; j < 14; j++) need(recipe.MatrixValue(matrices.Identity, i, j) === (i === j ? "1" : "0"), "fixed fourteen-slot identity matrix");
  validateGeometryValues(recipe, d, validator);
  return { Point: d.point, ...matrices };
}

function Derivative(recipe, omega, y) {
  return recipe.Sum(recipe.P(recipe.Spin(omega), y, "C"), recipe.Times(recipe.Motion(omega, y), "-1"));
}
function WedgeCoordinate(recipe, derivatives, frame) {
  fourteen(derivatives, "coordinate derivative"); let result = recipe.FreshZero();
  for (let mu = 0; mu < 14; mu++) for (let a = 0; a < 14; a++) if (recipe.MatrixValue(frame, mu, a) !== "0")
    result = recipe.Sum(result, recipe.TimesMatrixEntry(recipe.P(recipe.Unit(1 << a, 0), derivatives[mu]), frame, mu, a));
  return result;
}
function Divergence(recipe, derivatives) {
  return recipe.Sum(...fourteen(derivatives, "covariant derivative").map((t, a) => recipe.Times(recipe.Contract(t, a), a < 7 ? "-1" : "1")));
}
function CoordinateDual(recipe, frame, t) { return recipe.Pullback(recipe.TransposeMatrix(frame), recipe.Raise(t)); }
function FrameDual(recipe, inverseFrame, native) { return recipe.Raise(recipe.Pullback(recipe.TransposeMatrix(inverseFrame), native)); }

function buildMixedBackground(recipe, { x, curvatureLeaf, geometry, validateGeometry }) {
  const b = backgroundGeometry(recipe, geometry, validateGeometry);
  b.X = typed(recipe, x, 1); b.B = recipe.FreshZero(); b.NativeX = recipe.Pullback(b.InverseFrame, x);
  b.NativePartial = []; b.NativePartialOracle = []; b.CovariantCoordinate = []; b.CovariantFrame = [];
  for (let mu = 0; mu < 14; mu++) {
    const spin = recipe.Spin(b.Omega[mu]);
    for (let a = 0; a < 14; a++) if (recipe.MatrixValue(b.Frame, mu, a) !== "0")
      b.B = recipe.Sum(b.B, recipe.TimesMatrixEntry(recipe.P(recipe.Unit(1 << a, 0), spin), b.Frame, mu, a));
    b.NativePartial[mu] = recipe.Pullback(b.InversePartial[mu], x);
    b.CovariantCoordinate[mu] = Derivative(recipe, b.Omega[mu], x);
    const partialInFrame = recipe.Sum(b.CovariantCoordinate[mu], recipe.Motion(b.ConnectionInFrame[mu], x), recipe.Times(recipe.P(spin, x, "C"), "-1"));
    b.NativePartialOracle[mu] = recipe.Pullback(b.InverseFrame, partialInFrame);
  }
  for (let a = 0; a < 14; a++) b.CovariantFrame[a] = recipe.Sum(...b.CovariantCoordinate.map((t, mu) => recipe.TimesMatrixEntry(t, b.Frame, mu, a)));
  b.DX = WedgeCoordinate(recipe, b.CovariantCoordinate, b.Frame);
  b.NativeExterior = WedgeCoordinate(recipe, b.NativePartial, b.Identity);
  b.DXOracle = recipe.Sum(recipe.Pullback(b.Frame, b.NativeExterior), recipe.P(b.B, x, "C"));
  // Exact C# construction boundary: F follows all native/covariant jet work,
  // and precedes Q. It is NOT hoisted to the beginning of the point session.
  need(named(curvatureLeaf), "declared curvature leaf name"); b.F = typed(recipe, recipe.RegisterLeaf(curvatureLeaf), 2);
  b.Q = recipe.P(x, x); b.AdjointX = recipe.FixedAdjoint(x);
  b.KInputs = [recipe.FixedForward(b.F)[7], recipe.FixedForward(b.DX)[7], recipe.FixedForward(b.Q)[7]];
  const reverseCoordinate = b.Omega.map(omega => Derivative(recipe, omega, b.AdjointX));
  const reverseFrame = Array.from({ length: 14 }, (_, a) => recipe.Sum(...reverseCoordinate.map((t, mu) => recipe.TimesMatrixEntry(t, b.Frame, mu, a))));
  b.ReverseX = Divergence(recipe, reverseFrame);
  b.GradientPieces = [b.KInputs[0], recipe.Times(recipe.Sum(b.KInputs[1], b.ReverseX), "1/2"), recipe.Times(recipe.Sum(b.KInputs[2], recipe.Transpose(x, b.AdjointX)), "1/3"), recipe.Times(x, "907712")];
  b.Gradient = recipe.Sum(...b.GradientPieces);
  return freeze(b);
}

function baselineRoles() {
  const roles = [], add = (role, degree, canonicalId = role) => roles.push({ role, canonicalId, degree });
  for (const [role, degree] of [["X", 1], ["B", 1], ["F", 2], ["DX", 2], ["Q", 2], ["AdjointX", 2]]) add(role, degree);
  for (let p = 0; p < 3; p++) add("KInputs[" + p + "]", 1);
  for (let p = 0; p < 4; p++) add("GradientPieces[" + p + "]", 1, p === 0 ? "KInputs[0]" : undefined);
  add("NativeExterior", 2); for (let z = 0; z < 14; z++) add("CovariantFrame[" + z + "]", 1);
  need(roles.length === 28 && new Set(roles.map(r => r.canonicalId)).size === 27, "fixed baseline census"); return freeze(roles);
}
function roleValue(background, role) {
  const indexed = /^(KInputs|GradientPieces|CovariantFrame)\[(\d+)\]$/.exec(role);
  return indexed ? background[indexed[1]][Number(indexed[2])] : background[role];
}
function canonicalBaselineImports(recipe, background) {
  const roles = baselineRoles(), canonical = [], seen = new Set();
  for (const role of roles) {
    const value = typed(recipe, roleValue(background, role.role), role.degree);
    for (const other of roles) need((value === roleValue(background, other.role)) === (role.canonicalId === other.canonicalId), "only declared source-gradient alias permitted");
    if (!seen.has(role.canonicalId)) { seen.add(role.canonicalId); canonical.push({ id: role.canonicalId, degree: role.degree, value }); }
  }
  return freeze(canonical);
}
function importMixedBaseline(recipe, { geometry, validateGeometry, leafIds }) {
  const b = backgroundGeometry(recipe, geometry, validateGeometry), roles = baselineRoles();
  const ids = [...new Set(roles.map(r => r.canonicalId))]; shape(leafIds, ids);
  need(new Set(Object.values(leafIds)).size === 27 && Object.values(leafIds).every(named), "exact distinct canonical baseline leaf names");
  const tensors = new Map();
  for (const id of ids) {
    const d = roles.find(role => role.canonicalId === id).degree;
    tensors.set(id, typed(recipe, recipe.RegisterLeaf(leafIds[id]), d));
  }
  for (const name of ["X", "B", "F", "DX", "Q", "AdjointX", "NativeExterior"]) b[name] = tensors.get(name);
  b.KInputs = Array.from({ length: 3 }, (_, p) => tensors.get("KInputs[" + p + "]"));
  b.GradientPieces = [b.KInputs[0], ...Array.from({ length: 3 }, (_, p) => tensors.get("GradientPieces[" + (p + 1) + "]"))];
  b.CovariantFrame = Array.from({ length: 14 }, (_, z) => tensors.get("CovariantFrame[" + z + "]"));
  canonicalBaselineImports(recipe, b); return freeze(b);
}
function multiindices() {
  const result = []; for (let order = 0; order <= 3; order++) for (let a = 0; a <= order; a++) for (let b = 0; b <= order - a; b++) for (let c = 0; c <= order - a - b; c++) result.push([a, b, c, order - a - b - c]);
  return result;
}
function buildMixedMetricGerm(recipe, background, { geometry, validateGeometry, adaptedLeaf, oracleLeaf }) {
  const d = validateDescriptor(geometry, ["schemaVersion", "point", "metricBasis", "jetIndex", "provenance", "matrices"], GERM_SCHEMA);
  need(d.point === background.Point && integer(d.metricBasis, 0, 9) && integer(d.jetIndex, 0, 34), "complete fixed metric germ indices");
  const matrices = resolveMatrices(recipe, d.matrices, ["Motion", "DeltaFrame"], ["MotionPartial", "MotionCovariant", "DeltaOmega"], ["DeltaOmegaPartial"]);
  validateGeometryValues(recipe, d, validateGeometry);
  const g = { MetricBasis: d.metricBasis, JetIndex: d.jetIndex, Multiindex: multiindices()[d.jetIndex], ...matrices };
  g.Order = g.Multiindex.reduce((sum, value) => sum + value, 0);
  g.DeltaB = recipe.FreshZero();
  for (let mu = 0; mu < 14; mu++) {
    const spin = recipe.Spin(g.DeltaOmega[mu]);
    for (let a = 0; a < 14; a++) if (recipe.MatrixValue(background.Frame, mu, a) !== "0")
      g.DeltaB = recipe.Sum(g.DeltaB, recipe.TimesMatrixEntry(recipe.P(recipe.Unit(1 << a, 0), spin), background.Frame, mu, a));
  }
  g.DeltaBExterior = recipe.FreshZero();
  for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) {
    const spin = recipe.Spin(g.DeltaOmegaPartial[nu][mu]);
    for (let i = 0; i < 14; i++) if (recipe.MatrixValue(background.Frame, nu, i) !== "0")
      for (let j = 0; j < 14; j++) if (recipe.MatrixValue(background.Frame, mu, j) !== "0")
        g.DeltaBExterior = recipe.Sum(g.DeltaBExterior, recipe.TimesMatrixProduct(recipe.P(recipe.P(recipe.Unit(1 << i, 0), recipe.Unit(1 << j, 0)), spin), background.Frame, nu, i, background.Frame, mu, j));
  }
  g.DeltaCurvatureFromConnection = recipe.Sum(g.DeltaBExterior, recipe.P(background.B, g.DeltaB, "C"));
  // The two independent curvature lifts are registered together at the exact
  // post-exterior-derivative boundary, NOT inferred from DeltaFFixed.
  need(named(adaptedLeaf) && named(oracleLeaf) && adaptedLeaf !== oracleLeaf, "distinct independently bound curvature leaves");
  g.DeltaFAdapted = typed(recipe, recipe.RegisterLeaf(adaptedLeaf), 2);
  g.DeltaFOracle = typed(recipe, recipe.RegisterLeaf(oracleLeaf), 2);
  g.DeltaFFixed = recipe.Sum(g.DeltaFAdapted, recipe.Times(recipe.Motion(g.Motion, background.F), "-1"));
  return freeze(g);
}

module.exports = { BACKGROUND_SCHEMA, GERM_SCHEMA, Derivative, WedgeCoordinate, Divergence, CoordinateDual, FrameDual, buildMixedBackground, baselineRoles, canonicalBaselineImports, importMixedBaseline, buildMixedMetricGerm, multiindices };
