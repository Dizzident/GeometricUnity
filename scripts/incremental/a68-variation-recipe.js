"use strict";

// Complete MixedVariation algebra-expression recipe, not coefficient replay.
// The caller must supply independently source-validated background/germ
// geometry and handles, and must retain every intermediate callback. The
// outer 700-row/control/Ward recipe and numerical identities are separate.
const { Derivative, Divergence, CoordinateDual, FrameDual } = require("./a68-background-recipe");
const need = (ok, message) => { if (!ok) throw new Error("A68 variation recipe: " + message); };
function slices(recipe, tensor) {
  const table = Array.from({ length: 14 }, () => Array(14));
  for (let i = 0; i < 14; i++) {
    table[i][i] = recipe.FreshZero();
    for (let j = i + 1; j < 14; j++) table[i][j] = table[j][i] = recipe.Component(tensor, (1 << i) | (1 << j));
  }
  return table;
}
function movingEuler(recipe, b, g) {
  const dx = recipe.Motion(g.Motion, b.X), dy = recipe.FixedAdjoint(dx);
  const partialDy = g.MotionPartial.map(p => recipe.FixedAdjoint(recipe.Motion(p, b.X)));
  const deltaReverseFrame = [];
  for (let a = 0; a < 14; a++) {
    let derivative = recipe.FreshZero();
    for (let mu = 0; mu < 14; mu++) {
      if (recipe.MatrixValue(g.DeltaFrame, mu, a) !== "0") derivative = recipe.Sum(derivative, recipe.TimesMatrixEntry(Derivative(recipe, b.Omega[mu], b.AdjointX), g.DeltaFrame, mu, a));
      if (recipe.MatrixValue(b.Frame, mu, a) !== "0") derivative = recipe.Sum(derivative, recipe.TimesMatrixEntry(recipe.Sum(partialDy[mu], Derivative(recipe, b.Omega[mu], dy), Derivative(recipe, g.DeltaOmega[mu], b.AdjointX)), b.Frame, mu, a));
    }
    deltaReverseFrame[a] = derivative;
  }
  const reverse = Divergence(recipe, deltaReverseFrame);
  const deltaDX = recipe.Sum(recipe.Motion(g.Motion, b.DX), recipe.P(g.DeltaB, b.X, "C"));
  const deltaQ = recipe.Sum(recipe.P(dx, b.X), recipe.P(b.X, dx));
  const adapted = [recipe.FixedForward(g.DeltaFAdapted)[7], recipe.Times(recipe.Sum(recipe.FixedForward(deltaDX)[7], reverse), "1/2"),
    recipe.Times(recipe.Sum(recipe.FixedForward(deltaQ)[7], recipe.Transpose(dx, b.AdjointX), recipe.Transpose(b.X, dy)), "1/3"), recipe.Times(dx, "907712")];
  return adapted.map((t, piece) => recipe.Sum(t, recipe.MotionAdjoint(g.Motion, b.GradientPieces[piece])));
}
function buildMixedVariation(recipe, b, g, intermediate) {
  need(typeof intermediate === "function", "full intermediate retention callback required");
  for (const values of [b.CovariantFrame, b.Omega, b.FramePartial, b.FrameLift, g.MotionPartial, g.MotionCovariant, g.DeltaOmega]) need(Array.isArray(values) && values.length === 14, "all fourteen geometric slots");
  need(b.KInputs.length === 3 && b.GradientPieces.length === 4, "complete original-action pieces");
  const result = {}, a = g.Motion, vx = recipe.PairingMotion(a, b.X);
  // Present in the C# expression stream even though this local is not used
  // later. Do not silently optimize a frozen recipe while matching its nodes.
  recipe.PairingMotion(a, b.AdjointX);
  const deltaDX = recipe.P(g.DeltaB, b.X, "C");
  const inputs = [{ Value: b.F, Delta: g.DeltaFFixed }, { Value: b.DX, Delta: deltaDX }, recipe.Fixed(b.Q)], front = [];
  for (let piece = 0; piece < 3; piece++) {
    const stages = recipe.Forward(inputs[piece], a);
    for (let stage = 0; stage < 8; stage++) { intermediate(`fixed_p${piece}_s${stage}_value`, stages[stage].Value); intermediate(`fixed_p${piece}_s${stage}_delta`, stages[stage].Delta); }
    front[piece] = recipe.Sum(stages[7].Delta, recipe.PairingMotion(a, b.KInputs[piece]));
  }
  const fixedReverse = recipe.Reverse(recipe.Fixed(b.X), a);
  for (let stage = 0; stage < 9; stage++) { intermediate(`reverse_s${stage}_value`, fixedReverse[stage].Value); intermediate(`reverse_s${stage}_delta`, fixedReverse[stage].Delta); }
  result.CAdjointFixed = recipe.Sum(fixedReverse[8].Delta, recipe.FixedAdjoint(vx));
  result.CAdjointOracle = recipe.CAdjoint(b.X, a);
  const dx = recipe.Motion(a, b.X), dy = recipe.FixedAdjoint(dx);
  result.MovingAdjointDelta = recipe.Sum(dy, recipe.Times(recipe.Motion(a, b.AdjointX), "-1"));
  const deltaBBack = recipe.Transpose(g.DeltaB, b.AdjointX), bBack = recipe.Transpose(b.B, result.CAdjointFixed), qBack = recipe.Transpose(b.X, result.CAdjointFixed);
  result.Raw0 = [front[0], recipe.Times(recipe.Sum(front[1], bBack, deltaBBack), "1/2"), recipe.Times(recipe.Sum(front[2], qBack), "1/3"), recipe.Times(vx, "907712")];
  result.Raw2 = [recipe.FreshZero(), recipe.Times(result.CAdjointFixed, "1/2"), recipe.FreshZero(), recipe.FreshZero()];
  const oldBBack = recipe.Transpose(b.B, b.AdjointX), oldQBack = recipe.Transpose(b.X, b.AdjointX);
  const dbAdapted = recipe.Sum(g.DeltaB, recipe.Motion(a, b.B));
  const derivativeBBack = recipe.Sum(recipe.Transpose(dbAdapted, b.AdjointX), recipe.Transpose(b.B, dy));
  const derivativeQBack = recipe.Sum(recipe.Transpose(dx, b.AdjointX), recipe.Transpose(b.X, dy));
  const derivativeDX = recipe.Sum(recipe.Motion(a, b.DX), recipe.P(g.DeltaB, b.X, "C"));
  const derivativeQ = recipe.Sum(recipe.P(dx, b.X), recipe.P(b.X, dx));
  result.FieldFirst0 = [recipe.Sum(recipe.FixedForward(g.DeltaFAdapted)[7], recipe.MotionAdjoint(a, b.KInputs[0])),
    recipe.Times(recipe.Sum(recipe.FixedForward(derivativeDX)[7], derivativeBBack, recipe.MotionAdjoint(a, recipe.Sum(b.KInputs[1], oldBBack))), "1/2"),
    recipe.Times(recipe.Sum(recipe.FixedForward(derivativeQ)[7], derivativeQBack, recipe.MotionAdjoint(a, recipe.Sum(b.KInputs[2], oldQBack))), "1/3"),
    recipe.Times(recipe.Sum(dx, recipe.MotionAdjoint(a, b.X)), "907712")];
  result.FieldFirst2 = [recipe.FreshZero(), recipe.Times(recipe.Sum(dy, recipe.MotionAdjoint(a, b.AdjointX)), "1/2"), recipe.FreshZero(), recipe.FreshZero()];
  const sourceOther = recipe.Sum(recipe.C(b.F, a), recipe.FixedForward(g.DeltaFFixed)[7]);
  const kineticOther = recipe.Sum(recipe.C(b.DX, a), recipe.FixedForward(recipe.P(g.DeltaB, b.X, "C"))[7], recipe.Transpose(b.B, result.CAdjointOracle), recipe.Transpose(g.DeltaB, b.AdjointX));
  const cubicOther = recipe.Sum(recipe.C(b.Q, a), recipe.Transpose(b.X, result.CAdjointOracle));
  result.Oracle0 = [sourceOther, recipe.Times(kineticOther, "1/2"), recipe.Times(cubicOther, "1/3"), recipe.Times(recipe.PairingMotion(a, b.X), "907712")];
  result.Oracle2 = [recipe.FreshZero(), recipe.Times(result.CAdjointOracle, "1/2"), recipe.FreshZero(), recipe.FreshZero()];
  let divergence = recipe.FreshZero(); result.DerivativeIdentityComparisons = [];
  for (let z = 0; z < 14; z++) {
    const derivative = recipe.Times(recipe.Sum(recipe.CAdjoint(b.X, g.MotionCovariant[z]), recipe.CAdjoint(b.CovariantFrame[z], a)), "1/2");
    const weights = Array.from({ length: 14 }, (_, mu) => recipe.MatrixCoefficient(b.Frame, mu, z));
    const partial = recipe.Times(recipe.CAdjoint(b.X, recipe.CombineMatrices(g.MotionPartial, weights)), "1/2");
    const alternate = recipe.Sum(partial, Derivative(recipe, recipe.CombineMatrices(b.Omega, weights), result.Oracle2[1]));
    result.DerivativeIdentityComparisons.push([derivative, alternate]);
    intermediate(`covariant_z${z}`, derivative); intermediate(`covariantOracle_z${z}`, alternate);
    divergence = recipe.Sum(divergence, recipe.Times(recipe.Contract(derivative, z), z < 7 ? "-1" : "1"));
  }
  result.OrdinaryAdjoint = recipe.Sum(divergence, recipe.Times(recipe.Transpose(b.B, result.Raw2[1]), "-1"));
  result.EulerCovariant = [result.Raw0[0], recipe.Sum(result.Raw0[1], result.OrdinaryAdjoint), result.Raw0[2], result.Raw0[3]];
  result.EulerMoving = movingEuler(recipe, b, g);
  result.Native0 = result.Raw0.map(t => CoordinateDual(recipe, b.Frame, t)); result.Native2 = CoordinateDual(recipe, b.Frame, result.Raw2[1]);
  let nativeDivergence = recipe.FreshZero();
  const adaptedYDelta = recipe.FixedAdjoint(recipe.Motion(a, b.X)), dualSlices = slices(recipe, result.Raw2[1]), dySlices = slices(recipe, adaptedYDelta), ySlices = slices(recipe, b.AdjointX);
  result.CurrentCoefficients = []; result.CurrentPartialCoefficients = []; result.GreenVariationCoefficients = [];
  const oneHalf = recipe.ScalarConstant("1/2");
  for (let mu = 0; mu < 14; mu++) {
    const dualPartial = recipe.Times(recipe.CAdjoint(b.X, g.MotionPartial[mu]), "1/2"); intermediate(`partial_mu${mu}`, dualPartial);
    const partialSlices = slices(recipe, dualPartial);
    result.CurrentCoefficients[mu] = []; result.CurrentPartialCoefficients[mu] = []; result.GreenVariationCoefficients[mu] = [];
    for (let nu = 0; nu < 14; nu++) {
      let current = recipe.FreshZero(), derivative = recipe.FreshZero(), green = recipe.FreshZero();
      for (let i = 0; i < 14; i++) for (let j = 0; j < 14; j++) {
        const orientation = recipe.ScalarConstant(String((i === j ? 0 : i < j ? 1 : -1) * (i < 7 ? 1 : -1) * (j < 7 ? 1 : -1)));
        const entry = (matrix, row, column) => recipe.MatrixCoefficient(matrix, row, column), mul = (x, y) => recipe.ScalarMultiply(x, y), add = (x, y) => recipe.ScalarAdd(x, y);
        const coefficient = mul(mul(entry(b.Frame, mu, i), entry(b.Frame, nu, j)), orientation);
        const changed = mul(add(mul(entry(b.FramePartial[mu], mu, i), entry(b.Frame, nu, j)), mul(entry(b.Frame, mu, i), entry(b.FramePartial[mu], nu, j))), orientation);
        const metricChanged = mul(add(mul(entry(g.DeltaFrame, mu, i), entry(b.Frame, nu, j)), mul(entry(b.Frame, mu, i), entry(g.DeltaFrame, nu, j))), orientation);
        if (recipe.ScalarValue(coefficient) !== "0") {
          current = recipe.Sum(current, recipe.ScaleWithScalar(dualSlices[i][j], coefficient));
          derivative = recipe.Sum(derivative, recipe.ScaleWithScalar(partialSlices[i][j], coefficient));
          green = recipe.Sum(green, recipe.ScaleWithScalar(dySlices[i][j], mul(coefficient, oneHalf)));
        }
        if (recipe.ScalarValue(changed) !== "0") derivative = recipe.Sum(derivative, recipe.ScaleWithScalar(dualSlices[i][j], changed));
        if (recipe.ScalarValue(metricChanged) !== "0") green = recipe.Sum(green, recipe.ScaleWithScalar(ySlices[i][j], mul(metricChanged, oneHalf)));
      }
      result.CurrentCoefficients[mu][nu] = current; result.GreenVariationCoefficients[mu][nu] = green;
      derivative = recipe.Sum(derivative, recipe.ScaleWithScalar(current, recipe.NegativeMatrixTrace(b.FrameLift[mu]))); result.CurrentPartialCoefficients[mu][nu] = derivative;
      nativeDivergence = recipe.Sum(nativeDivergence, recipe.P(recipe.Unit(1 << nu, 0), derivative));
    }
  }
  result.NativeDivergence = nativeDivergence; result.NativeAdjointAsFrame = FrameDual(recipe, b.InverseFrame, recipe.Times(nativeDivergence, "-1"));
  result.NativeEuler = [result.Native0[0], recipe.Sum(result.Native0[1], recipe.Times(nativeDivergence, "-1")), result.Native0[2], result.Native0[3]];
  return result;
}
module.exports = { buildMixedVariation };
