"use strict";

// Metadata-only transcription of the independently reviewed original-action
// acceleration/Green calculation. No Fourier coefficients or Pair values are
// computed. Input geometry/source proof remains the caller's prerequisite.
// Scalar Pair references are NOT tensor-DAG edges: the eventual scalar replay
// must separately account for their dependencies and operand lifetime.
const { Derivative, WedgeCoordinate } = require("./a68-background-recipe");
const SCALAR_SCHEMA = "phase627-ward-acceleration-scalar-expressions-v1";
const need = (condition, message) => { if (!condition) throw new Error("A68 acceleration recipe: " + message); };
const freeze = x => { if (x !== null && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const fourteen = (values, label) => { need(Array.isArray(values) && values.length === 14, label + " requires all14 entries"); return values; };
function typed(recipe, t, degree) { const actual = recipe.TensorDegree(t); need(actual === degree || actual === -1, "input degree " + degree); }

function buildMixedWardAcceleration(recipe, b, g, { eta, nativeDeta, oracle = false }) {
  need(typeof oracle === "boolean", "explicit oracle flag");
  typed(recipe, eta, 0); typed(recipe, nativeDeta, 1); typed(recipe, g.DeltaB, 1); typed(recipe, g.DeltaBExterior, 2);
  for (const name of ["X", "B"]) typed(recipe, b[name], 1);
  for (const name of ["F", "DX", "Q", "AdjointX"]) typed(recipe, b[name], 2);
  need(Array.isArray(b.KInputs) && b.KInputs.length === 3 && Array.isArray(b.GradientPieces) && b.GradientPieces.length === 4, "complete original action pieces");
  for (const t of [...b.KInputs, ...b.GradientPieces]) typed(recipe, t, 1);
  fourteen(b.FramePartial, "FramePartial"); fourteen(b.Omega, "Omega"); fourteen(g.DeltaOmega, "DeltaOmega");
  fourteen(g.DeltaOmegaPartial, "DeltaOmegaPartial").forEach(row => fourteen(row, "DeltaOmegaPartial row"));
  // Validate named matrix identity without emitting graph nodes. The identity
  // is the same fixed14-coordinate identity used by the C# wedge helper.
  for (let row = 0; row < 14; row++) for (let column = 0; column < 14; column++)
    need(recipe.MatrixValue(b.Identity, row, column) === (row === column ? "1" : "0"), "fixed coordinate identity");

  const deta = recipe.Pullback(b.Frame, nativeDeta);
  const w = recipe.P(g.DeltaB, eta, "C");
  const dw = recipe.Sum(recipe.P(g.DeltaBExterior, eta, "C"), recipe.Times(recipe.P(g.DeltaB, deta, "C"), "-1"));
  const dbw = recipe.Sum(dw, recipe.P(b.B, w, "C"));
  const partialW = [], covariantW = [], covariantY = [], partialBJ = [];
  for (let nu = 0; nu < 14; nu++) {
    let partialB = recipe.FreshZero();
    for (let mu = 0; mu < 14; mu++) {
      const spin = recipe.Spin(g.DeltaOmega[mu]);
      const partialSpin = recipe.Spin(g.DeltaOmegaPartial[nu][mu]);
      for (let a = 0; a < 14; a++) {
        if (recipe.MatrixValue(b.FramePartial[nu], mu, a) !== "0")
          partialB = recipe.Sum(partialB, recipe.TimesMatrixEntry(recipe.P(recipe.Unit(1 << a, 0), spin), b.FramePartial[nu], mu, a));
        if (recipe.MatrixValue(b.Frame, mu, a) !== "0")
          partialB = recipe.Sum(partialB, recipe.TimesMatrixEntry(recipe.P(recipe.Unit(1 << a, 0), partialSpin), b.Frame, mu, a));
      }
    }
    partialBJ[nu] = partialB;
    partialW[nu] = recipe.Sum(recipe.P(partialB, eta, "C"), recipe.P(g.DeltaB, recipe.Component(nativeDeta, 1 << nu), "C"));
  }
  for (let a = 0; a < 14; a++) {
    covariantW[a] = recipe.FreshZero(); covariantY[a] = recipe.FreshZero();
    for (let mu = 0; mu < 14; mu++) if (recipe.MatrixValue(b.Frame, mu, a) !== "0") {
      covariantW[a] = recipe.Sum(covariantW[a], recipe.TimesMatrixEntry(recipe.Sum(partialW[mu], Derivative(recipe, b.Omega[mu], w)), b.Frame, mu, a));
      covariantY[a] = recipe.Sum(covariantY[a], recipe.TimesMatrixEntry(Derivative(recipe, b.Omega[mu], b.AdjointX), b.Frame, mu, a));
    }
  }
  const dbwFromJet = WedgeCoordinate(recipe, covariantW, b.Identity);
  const dwFromJet = recipe.Sum(dbwFromJet, recipe.Times(recipe.P(b.B, w, "C"), "-1"));
  const cross = recipe.Sum(recipe.P(b.X, w), recipe.P(w, b.X));
  const stages = [recipe.FixedForward(dbw), recipe.FixedForward(cross)];

  // Closed scalar descriptor language. Pair reads already recorded tensor IDs
  // and emits NO graph node; Contract in a Pair argument still emits its own
  // distinct graph node before that Pair is described, just as in C#.
  const pair = (left, right) => {
    const dl = recipe.TensorDegree(left), dr = recipe.TensorDegree(right);
    need(dl < 0 || dr < 0 || dl === dr, "scalar Pair form-degree agreement");
    return { op: "pair", left: recipe.RecordedNode(left), right: recipe.RecordedNode(right) };
  };
  const constant = value => ({ op: "constant", value });
  const add = (left, right) => ({ op: "add", left, right });
  const multiply = (left, right) => ({ op: "multiply", left, right });
  const matrixEntry = (row, column) => ({ op: "matrixEntry", matrix: "background.Frame", row, column });
  const original = [pair(w, b.KInputs[0]),
    multiply(add(pair(w, b.KInputs[1]), pair(b.X, stages[0][7])), constant("1/2")),
    multiply(add(pair(w, b.KInputs[2]), pair(b.X, stages[1][7])), constant("1/3")),
    multiply(pair(b.X, w), constant("907712"))];
  const euler = b.GradientPieces.map(t => pair(w, t));
  const current = [], coordinateCurrent = [], currentDerivative = [];
  for (let a = 0; a < 14; a++) current[a] = multiply(pair(recipe.Contract(b.AdjointX, a), w), constant(a < 7 ? "1/2" : "-1/2"));
  for (let mu = 0; mu < 14; mu++) {
    coordinateCurrent[mu] = constant("0");
    for (let a = 0; a < 14; a++) coordinateCurrent[mu] = add(coordinateCurrent[mu], multiply(matrixEntry(mu, a), current[a]));
  }
  let divergence = constant("0");
  for (let z = 0; z < 14; z++) {
    currentDerivative[z] = [];
    for (let a = 0; a < 14; a++)
      currentDerivative[z][a] = multiply(add(pair(recipe.Contract(covariantY[z], a), w), pair(recipe.Contract(b.AdjointX, a), covariantW[z])), constant(a < 7 ? "1/2" : "-1/2"));
    divergence = add(divergence, currentDerivative[z][z]);
  }
  const scalarPlan = freeze({ schemaVersion: SCALAR_SCHEMA,
    geometry: [{ id: "background.Frame", entries: recipe.MatrixEntries(b.Frame) }],
    Original: original, Euler: euler, Current: current, CoordinateCurrent: coordinateCurrent, CurrentCovariantDerivative: currentDerivative, Divergence: divergence,
    scope: { coefficientsEvaluated: false, geometrySourceClosureEstablishedHere: false, scalarReplayImplementedHere: false, scalarOperandLivenessProvedHere: false, totalResourceProof: false } });
  return freeze({ Oracle: oracle, Eta: eta, NativeDeta: nativeDeta, W: w, Dw: dw, DBw: dbw,
    PartialW: partialW, CovariantW: covariantW, DwFromJet: dwFromJet, DBwFromJet: dbwFromJet,
    DeltaBPartialInFrame: partialBJ, CovariantAdjointX: covariantY, CrossQ: cross, Stages: stages,
    Original: original, Euler: euler, Current: current, CoordinateCurrent: coordinateCurrent,
    CurrentCovariantDerivative: currentDerivative, Divergence: divergence, scalarPlan });
}

module.exports = { SCALAR_SCHEMA, buildMixedWardAcceleration };
