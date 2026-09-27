"use strict";

// Bind ALL recipe matrix roles to authenticated rebuilt geometry. New627
// derivatives are not supplied by621; they come from the same source bundle.
// Whole-file code/contract closure and string/RSS budgets remain driver duties.
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const { sourceGeometryIdentity } = require("./a68-source-geometry");
const { BACKGROUND_SCHEMA, GERM_SCHEMA } = require("./a68-background-recipe");
const need = (ok, why) => { if (!ok) throw new Error("A68 geometry recipe binding: " + why); };
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
function geometryMatrixLayout(kind) {
  need(kind === "baseline" || kind === "germ", "closed geometry role menu");
  const single = kind === "baseline" ? ["Frame", "InverseFrame", "Identity"] : ["Motion", "DeltaFrame"];
  const arrays = kind === "baseline" ? ["FrameLift", "FramePartial", "Omega", "InversePartial", "ConnectionInFrame"] : ["MotionPartial", "MotionCovariant", "DeltaOmega"];
  const result = single.map(role => ({ role, indices: [] }));
  for (const role of arrays) for (let i = 0; i < 14; i++) result.push({ role, indices: [i] });
  if (kind === "germ") for (let nu = 0; nu < 14; nu++) for (let mu = 0; mu < 14; mu++) result.push({ role: "DeltaOmegaPartial", indices: [nu, mu] });
  return freeze(result);
}
function exportRecipeGeometry(a, binding, limits, baselineBinding = null) {
  const identity = boundGeometryIdentity(a, binding), source = sourceGeometryIdentity(a, identity.rebuilt), isBase = source.kind === "baseline";
  const names = ["matrixSlots", "coordinateVisits", "entryRecords"];
  need(limits && JSON.stringify(Object.keys(limits)) === JSON.stringify(names) && names.every(k => Number.isSafeInteger(limits[k]) && limits[k] > 0), "explicit finite export admission limits");
  const used = { matrixSlots: 0, coordinateVisits: 0, entryRecords: 0 };
  const charge = (kind, count) => { need(count <= limits[kind] - used[kind], "pre-export " + kind + " ceiling"); used[kind] += count; };
  const provenance = [], pin = (id, source) => ({ id, source: source.path, sha256: source.sha256 });
  if (isBase) {
    need(baselineBinding === null, "baseline export has no parent binding"); provenance.push(pin("retained621-baseline", binding.source));
  } else {
    const parent = boundGeometryIdentity(a, baselineBinding);
    need(parent.rebuilt === source.baseline && baselineBinding.kind === "baseline", "exact independently compared baseline parent");
    provenance.push(pin("retained621-baseline", baselineBinding.source), pin("retained621-germ", binding.source));
  }
  const layout = geometryMatrixLayout(source.kind), matrixSource = isBase ? identity.rebuilt.frame : identity.rebuilt.matrices;
  charge("matrixSlots", layout.length); charge("coordinateVisits", layout.length * 196);
  const matrices = [], roles = {}, resolved = {};
  const prefix = "geometry.p" + source.point + (isBase ? ".baseline" : ".m" + source.metricBasis + ".j" + source.jetIndex);
  for (const { role, indices } of layout) {
    const name = prefix + "." + role + indices.map(i => "[" + i + "]").join("");
    let matrix = matrixSource[role]; for (const index of indices) matrix = matrix[index];
    need(a.size(matrix) === 14, "complete source matrix dimension"); const entries = [];
    for (let row = 0; row < 14; row++) for (let column = 0; column < 14; column++) {
      const value = a.get(matrix, row, column); if (a.isZero(value)) continue;
      charge("entryRecords", 1); entries.push({ row, column, value: a.text(value) });
    }
    matrices.push({ name, matrix: entries }); resolved[name] = entries;
    if (!indices.length) roles[role] = name;
    else {
      if (!Object.hasOwn(roles, role)) roles[role] = Array.from({ length: 14 }, () => indices.length === 2 ? Array(14) : null);
      if (indices.length === 1) roles[role][indices[0]] = name; else roles[role][indices[0]][indices[1]] = name;
    }
  }
  const descriptor = isBase ? { schemaVersion: BACKGROUND_SCHEMA, point: source.point, provenance, matrices: roles } :
    { schemaVersion: GERM_SCHEMA, point: source.point, metricBasis: source.metricBasis, jetIndex: source.jetIndex, provenance, matrices: roles };
  // The recipe validator resolves names in sorted order, not construction order.
  const expected = Object.fromEntries(Object.keys(resolved).sort().map(name => [name, resolved[name]]));
  freeze(descriptor); freeze(matrices); freeze(expected);
  const descriptorText = JSON.stringify(descriptor), valuesText = JSON.stringify(expected);
  return Object.freeze({ descriptor, matrices, validateGeometry: (candidate, values) =>
    JSON.stringify(candidate) === descriptorText && JSON.stringify(values) === valuesText,
  usage: Object.freeze(used), scope: Object.freeze({ completeSourceMatrixRolesBound: true,
    frozenCodeClosureEstablished: false, processMemoryProved: false, scientificExecutionAuthorized: false }) });
}
module.exports = { geometryMatrixLayout, exportRecipeGeometry };
