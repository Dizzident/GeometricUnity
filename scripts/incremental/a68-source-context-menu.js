"use strict";
// Independent source STRUCTURE only. Mechanically promoted from the all705
// cross-language checks; no scientific coefficients, geometry or I/O.
const { pointMarkMenu, germMarkMenu } = require("./a68-audit-context-recipe");
const { baselineRoles, multiindices } = require("./a68-background-recipe");
const { wardControlsMenu, wardMarkMenu } = require("./a68-ward-controls-recipe");
const { diagnosticMarkMenu, diagnosticCheckMenu } = require("./a68-diagnostics-recipe");
const { secondJetMenu, secondJetMarkMenu } = require("./a68-second-jet-recipe");
const ordinal = (a, b) => a < b ? -1 : a > b ? 1 : 0;
const geoRoot = "studies/phase621_induced_metric_full_variation_scope_audit_001/output";
const polyRoot = "studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output/chunks";
const pieces = ["source", "kinetic", "cubic", "mass"], multis = multiindices();
const ward = new Set(wardControlsMenu().contexts.map(c => `point${c.point}/m${c.metricBasis}_j${c.jetIndex}`));
const ids = [];
for (let p = 0; p < 2; p++) { ids.push(`point${p}`); for (let m = 0; m < 10; m++) for (let j = 0; j < 35; j++) ids.push(`point${p}/m${m}_j${j}`); }
ids.push("diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets");
function expectedMarks(id) {
  if (/^point[01]$/.test(id)) return pointMarkMenu();
  const germ = /^point([01])\/m([0-9])_j([0-9]+)$/.exec(id);
  if (germ) return [...germMarkMenu(), ...(ward.has(id) ? wardMarkMenu(Number(germ[1]), Number(germ[3])) : [])];
  return id === "diagnostic/secondJets" ? secondJetMarkMenu() : diagnosticMarkMenu(id.slice(11));
}
function expectedLeaves(id) {
  const leaves = [], leafRoles = {};
  const leaf = (role, degree, source) => { const key = id + "/" + role; leaves.push({ id: key, degree, source }); leafRoles[role] = key; };
  const curvature = prefix => leaf("geometry/" + prefix + "curvature", 2, geoRoot + "/induced_metric_full_variation_scope_audit_summary.json#independent-source-spin-curvature");
  const variations = (prefix, p, m, j) => {
    const source = `${geoRoot}/shards/jet_p${p}_m${String(m).padStart(2, "0")}_j${String(j).padStart(2, "0")}.json#independent-source-spin-curvature-variation`;
    for (const name of ["deltaCurvatureAdapted", "deltaCurvatureOracle"]) leaf("geometry/" + prefix + name, 2, source);
  };
  const point = /^point([01])$/.exec(id), germ = /^point([01])\/m([0-9])_j([0-9]+)$/.exec(id);
  if (point) { leaf("input/X", 1, `${polyRoot}/p${point[1]}_kinetic_X_inputs_g000.json#independent-five-order-reconstruction`); curvature(""); }
  else if (germ) {
    for (const role of baselineRoles()) {
      if (role.role === role.canonicalId) leaf("baseline/" + role.role, role.degree, `verified-point-replay/point${germ[1]}/${role.canonicalId}`);
      else leafRoles["baseline/" + role.role] = id + "/baseline/" + role.canonicalId;
    }
    variations("", Number(germ[1]), Number(germ[2]), Number(germ[3]));
  } else if (id === "diagnostic/grade10") { curvature("background/"); for (let m = 0; m < 4; m++) variations(`basis${m}/`, 0, m, multis.findIndex(v => JSON.stringify(v) === "[1,0,0,0]")); }
  else if (id === "diagnostic/acceleration") { curvature("background/"); variations("germ/", 1, 0, multis.findIndex(v => JSON.stringify(v) === "[0,2,0,0]")); }
  return { leaves, leafRoles };
}
function expectedCallbacks(id) {
  const result = [], c = (category, name, degree = null, expanded = null, length = null) => result.push({ category, name, degree, expanded, length });
  const check = name => c("Check", name);
  const point = /^point([01])$/.exec(id), germ = /^point([01])\/m([0-9])_j([0-9]+)$/.exec(id);
  if (point) {
    c("BeginPoint", "$"); c("GeometryLeaf", "curvature", 2); c("Background", "$");
    for (let mu = 0; mu < 14; mu++) check(`nativeFirstJet_${mu}`); check("covariantExterior");
    if (point[1] === "0") ["nativeFirstJetHandAnchor", "nativeExteriorHandAnchor", "nativeVectorAnchorPositive"].forEach(check);
    c("SealPointBackground", "$"); for (let m = 0; m < 10; m++) for (let j = 0; j < 35; j++) c("Germ", `m${m}_j${j}`); c("EndPoint", "$");
  } else if (germ) {
    const [p, m, j] = germ.slice(1).map(Number), multi = multis[j], order = multi.reduce((a, b) => a + b, 0);
    c("BeginGerm", "$"); for (const name of ["deltaCurvatureAdapted", "deltaCurvatureOracle"]) c("GeometryLeaf", name, 2); c("Geometry", "$");
    check("traceFreeMotion"); check("metricValueDualBlocks");
    for (let mu = 0; mu < 14; mu++) {
      check(`metricFirstDualBlocks_${mu}`); check(`connectionDualPalatini_${mu}`);
      for (let nu = 0; nu < 14; nu++) { check(`metricSecondDualBlocks_${mu}_${nu}`); check(`connectionFirstDualPalatini_${mu}_${nu}`); }
    }
    ["curvatureDualPalatini", "spinCurvatureLowering", "referenceCurvatureDerivative", "nonInvariantCovariantDerivative", "fixedAdjointIndependent", "movingAdjointPairingMotion", "ordinaryAdjointNativeDivergence"].forEach(check);
    for (const mark of germMarkMenu()) if (mark.name.startsWith("tensor/")) c("Tensor", mark.name.slice(7), mark.degree, mark.requiredExpanded);
    for (const piece of pieces) {
      for (const kind of ["rawFieldFirst0", "rawFieldFirst2", "rawWord0", "rawWord2", "eulerMoving", "nativeEuler"]) check(kind + "_" + piece);
      if (order === 3 && piece !== "source") check("thirdGermLowerPiece_" + piece);
    }
    for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) {
      check(`nativeJetSlot_${mu}_${nu}`); check(`greenCurrent_${mu}_${nu}`);
      if (mu === nu) check(`diagonalNull_${mu}`); if (mu < nu) check(`symmetricNull_${mu}_${nu}`);
    }
    check("completeCurrent"); c("Error", "$"); if (order === 3) check("thirdGermZeroBackgroundError");
    if (p === 1 && m === 0 && JSON.stringify(multi) === "[0,3,0,0]") check("thirdSourceHandAnchor");
    if (p === 0 && m === 0 && JSON.stringify(multi) === "[1,0,0,0]") check("nonzeroMassHandAnchor");
    const pairs = [[0, 0], [1, 1], [2, 2], [3, 3], [0, 1], [0, 2], [0, 3], [1, 2], [1, 3], [2, 3]], axis = multi.indexOf(3);
    if (axis >= 0 && pairs[m].includes(axis)) check("pureBaseDiffeomorphismPrincipalZero");
    if (ward.has(id)) for (let eta = 0; eta < 2; eta++) for (let route = 0; route < 2; route++) {
      const name = `p${p}_j${j}_eta${eta}_route${route}`;
      c("Ward", name + "_epsilon"); c("Ward", name + "_compensated"); c("Acceleration", name); c("Original", name + "_fixedNativeTangent");
      for (const variant of ["compensated", "epsilon"]) for (const field of ["densityValue", "densityMetric", "densityField", "densityMixed", "descendedDerivative", "curvatureSquare", "curvatureVariation", "solderFirst", "solderOuter", "solderInner"]) check(`${name}_${variant}_${field}`);
      check(name + "_accelerationExteriorJet"); for (let piece = 0; piece < 4; piece++) for (const kind of ["compensatedWard", "epsilonMixedWard", "accelerationGreen"]) check(`${name}_${kind}_${piece}`);
      if (route === 0) continue;
      for (const variant of ["E", "C"]) {
        check(name + "_primitiveMenu" + variant); for (let primitive = 0; primitive < 33; primitive++) check(`${name}_wordPrimitive_${variant}${primitive}`);
        for (let density = 0; density < 2; density++) {
          for (let piece = 0; piece < 3; piece++) for (let stage = 0; stage < 8; stage++) check(`${name}_wordDensity_${variant}_${density}_p${piece}_s${stage}`);
          for (let piece = 0; piece < 4; piece++) check(`${name}_wordDensity_${variant}_${density}_top${piece}`);
          check(`${name}_wordDensity_${variant}_${density}_scalars`);
        }
      }
      for (let piece = 0; piece < 3; piece++) for (let stage = 0; stage < 8; stage++) check(`${name}_wordField_p${piece}_s${stage}`);
      for (const kind of ["wordFieldScalars", "wordAcceleration", "wordAccelerationTensors"]) check(name + "_" + kind);
      for (let mu = 0; mu < 14; mu++) check(`${name}_wordAccelerationJet${mu}`);
      for (let piece = 0; piece < 2; piece++) for (let stage = 0; stage < 8; stage++) check(`${name}_wordAcceleration_p${piece}_s${stage}`);
    }
    c("EndGerm", "$");
  } else {
    c("Begin", "$");
    if (id === "diagnostic/secondJets") for (const item of secondJetMenu()) { c("SecondJet", item.name, 2); check(item.name); }
    else {
      const grade = id === "diagnostic/grade10";
      c("Background", "background"); c("GeometryLeaf", "background/curvature", 2);
      for (const prefix of grade ? ["basis0", "basis1", "basis2", "basis3"] : ["germ"]) {
        c("Geometry", prefix); for (const route of ["deltaCurvatureAdapted", "deltaCurvatureOracle"]) c("GeometryLeaf", prefix + "/" + route, 2);
      }
      for (const mark of expectedMarks(id)) if (mark.name.startsWith("tensor/")) c("Tensor", mark.name.slice(7), mark.degree, mark.requiredExpanded);
      if (grade) {
        for (let route = 0; route < 2; route++) c("ScalarArray", "combined/originalRoute" + route, null, null, 4);
        for (const family of ["raw0", "raw2", "fieldFirst0", "fieldFirst2", "word0", "word2", "eulerCovariant", "eulerMoving"]) c("ScalarArray", "combined/projection/" + family, null, null, 4);
        for (let basis = 0; basis < 4; basis++) for (let route = 0; route < 2; route++) c("Original", `basis${basis}/original${route}`);
      } else {
        for (const name of ["forecast/original", "route0/nativeTangent", "route1/nativeTangent"]) c("ScalarArray", name, null, null, 4);
        for (let route = 0; route < 2; route++) { c("Original", `route${route}/fixedNativeTangent`); c("Acceleration", `route${route}/acceleration`); for (const name of ["epsilon", "compensated"]) c("Ward", `route${route}/${name}`); }
      }
      diagnosticCheckMenu(id.slice(11)).forEach(check);
    }
    c("End", "$");
  }
  return result.sort((a, b) => ordinal(a.category, b.category) || ordinal(a.name, b.name));
}

const allowedIds = new Set(ids);
const freeze = x => { if (x && typeof x === "object" && !Object.isFrozen(x)) { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
function sourceContextIds() { return Object.freeze([...ids]); }
function sourceContextMenu(id) {
  if (typeof id !== "string" || !allowedIds.has(id)) throw new Error("A68 source menu: exact declared context ID");
  const { leaves, leafRoles } = expectedLeaves(id);
  return freeze({ id, leaves, leafRoles,
    marks: [...expectedMarks(id)].sort((a, b) => ordinal(a.name, b.name)), callbacks: expectedCallbacks(id) });
}
// Full DiagnosticMenu declaration, including its declared array order. This
// is deliberately NOT derived from sorted callbacks or execution-order marks:
// grade10 declares the combined hand tensors before its four variation rows.
// Build lazily on explicit metadata request; no GU source evaluation occurs.
function diagnosticMenu(diagnostic) {
  if (!["grade10", "acceleration", "secondJets"].includes(diagnostic))
    throw new Error("A68 source menu: exact declared diagnostic ID");
  const menu = { id: diagnostic, tensors: [], scalarArrays: [], originalActions: [], wardActions: [],
    accelerations: [], secondJets: [], checks: [], backgrounds: 0, germs: 0, variationRows: 0 };
  if (diagnostic === "secondJets") {
    menu.secondJets = secondJetMenu().map(item => item.name); menu.checks = [...menu.secondJets];
  } else {
    menu.checks = diagnosticCheckMenu(diagnostic); menu.backgrounds = 1;
    if (diagnostic === "acceleration") {
      menu.tensors = ["input/X", "input/eta", "input/dEta"];
      menu.scalarArrays = ["forecast/original", "route0/nativeTangent", "route1/nativeTangent"];
      menu.originalActions = ["route0/fixedNativeTangent", "route1/fixedNativeTangent"];
      menu.wardActions = ["route0/epsilon", "route0/compensated", "route1/epsilon", "route1/compensated"];
      menu.accelerations = ["route0/acceleration", "route1/acceleration"]; menu.germs = 1;
    } else {
      menu.germs = 4; menu.variationRows = 4;
      menu.tensors = ["input/X", "input/UFrame", "input/UNative", "input/dUNative", "combined/motionX", "combined/crossQ",
        "combined/motionCrossQ", "combined/adjointMotionX", "combined/wordAdjointMotionX", "combined/fixedAdjoint",
        "combined/movingAdjointDelta", "combined/pairingMotionAdjoint", "combined/adjointPhi1", "combined/pairingMotionAdjointPhi1"];
      const families = ["raw0", "raw2", "fieldFirst0", "fieldFirst2", "word0", "word2", "eulerCovariant", "eulerMoving", "native0", "nativeEuler"];
      for (let basis = 0; basis < 4; basis++) {
        const prefix = "basis" + basis, tensor = name => menu.tensors.push(prefix + "/" + name);
        for (let p = 0; p < 3; p++) for (let s = 0; s < 8; s++) for (const part of ["value", "delta"]) tensor(`intermediate/fixed_p${p}_s${s}_${part}`);
        for (let s = 0; s < 9; s++) for (const part of ["value", "delta"]) tensor(`intermediate/reverse_s${s}_${part}`);
        for (let z = 0; z < 14; z++) { tensor("intermediate/covariant_z" + z); tensor("intermediate/covariantOracle_z" + z); }
        for (let mu = 0; mu < 14; mu++) tensor("intermediate/partial_mu" + mu);
        for (const family of families) for (const piece of [...pieces, "total"]) tensor(family + "/" + piece);
        for (const single of ["fixedAdjoint", "wordAdjoint", "movingAdjointDelta", "ordinaryAdjoint", "native2", "nativeDivergence", "nativeAdjointAsFrame"]) tensor(single);
        for (let mu = 0; mu < 14; mu++) for (let nu = 0; nu < 14; nu++) for (const kind of ["current", "currentPartial", "green"]) tensor(`${kind}/${mu}/${nu}`);
      }
      for (const family of families.slice(0, 8)) for (const piece of [...pieces, "total"]) menu.tensors.push("combined/" + family + "/" + piece);
      for (let s = 0; s < 8; s++) { menu.tensors.push("forwardHand/literal/" + s); menu.tensors.push("forwardHand/word/" + s); }
      menu.scalarArrays = ["combined/originalRoute0", "combined/originalRoute1", ...families.slice(0, 8).map(f => "combined/projection/" + f)];
      for (let basis = 0; basis < 4; basis++) for (let route = 0; route < 2; route++) menu.originalActions.push(`basis${basis}/original${route}`);
    }
  }
  return freeze(menu);
}
module.exports = { sourceContextIds, sourceContextMenu, diagnosticMenu };
