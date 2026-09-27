"use strict";

// Exact metadata-expression order for MixedWard.Evaluate, NOT a numerical
// Ward test. The caller must independently validate source-bound coefficients,
// including Local's canonical rationals, zero frequencies and H-anti-Hermitian
// real domain. Degree/identity checks here do not prove those properties.
// All marks belong to the caller's subsequent structured evidence callback.
const { BiTensorRecipe, FIELDS, WEIGHTS, TOP_SCHEMA } = require("./a68-original-action-recipe");
const need = (condition, message) => { if (!condition) throw new Error("A68 Ward recipe: " + message); };
const flag = value => need(typeof value === "boolean", "explicit boolean route/compensation flag");
const typed = (recipe, tensor, degree) => {
  const actual = recipe.TensorDegree(tensor);
  need(actual === -1 || actual === degree, "boundary tensor degree " + degree);
};
const typedBi = (recipe, bi, tensor, degree) => {
  need(tensor !== null && typeof tensor === "object" && JSON.stringify(Object.keys(tensor)) === JSON.stringify(FIELDS), "closed ordered BiTensor descriptor");
  bi.Tensor(...FIELDS.map(field => tensor[field]));
  FIELDS.forEach(field => typed(recipe, tensor[field], degree));
};

function wardConjugate(recipe, left, tensor, right, oracle = false) {
  const bi = new BiTensorRecipe(recipe); flag(oracle);
  return bi.Product(bi.Product(left, tensor, "W", oracle), right, "W", oracle);
}

function buildWardChain(recipe, input, firstPhi, outerPhi, innerPhi, motion, oracle = false) {
  const bi = new BiTensorRecipe(recipe); flag(oracle); recipe.MatrixEntries(motion);
  typedBi(recipe, bi, input, 2); typedBi(recipe, bi, firstPhi, 1);
  typedBi(recipe, bi, outerPhi, 1); typedBi(recipe, bi, innerPhi, 2);
  const sf = bi.Star(input, motion, oracle);
  const first = bi.Product(firstPhi, sf, "C", oracle);
  const inner = bi.Product(innerPhi, sf, "A", oracle);
  const zero = bi.Star(inner, motion, oracle);
  const outer = bi.Product(outerPhi, zero, "A", oracle);
  const upper = bi.Add(first, bi.Scale(bi.Star(outer, motion, oracle), "-1/2"));
  return Object.freeze([input, sf, first, inner, zero, outer, upper, bi.Star(upper, motion, oracle)]);
}

function topExtraction(recipe, tensor, weight) {
  typed(recipe, tensor, 14);
  // Exactly Fourier.Top: absent coefficient is zero; imaginary coefficient
  // must vanish; negate the real scalar before applying this fixed weight.
  // Descriptor creation never emits a component, scale or mark node.
  return Object.freeze({ schemaVersion: TOP_SCHEMA, tensor, node: recipe.RecordedNode(tensor), degree: 14,
    form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight });
}

function buildWardDensity(recipe, t, f, derivative, q, firstPhi, outerPhi, innerPhi, motion, oracle = false) {
  const bi = new BiTensorRecipe(recipe); flag(oracle); recipe.MatrixEntries(motion);
  typedBi(recipe, bi, t, 1);
  const Stages = Object.freeze([buildWardChain(recipe, f, firstPhi, outerPhi, innerPhi, motion, oracle),
    buildWardChain(recipe, derivative, firstPhi, outerPhi, innerPhi, motion, oracle),
    buildWardChain(recipe, q, firstPhi, outerPhi, innerPhi, motion, oracle)]);
  const TopForms = [], Value = [], Metric = [], Field = [], Mixed = [];
  for (let piece = 0; piece < 4; piece++) {
    const top = bi.Product(t, bi.Star(piece < 3 ? Stages[piece][7] : t, motion, oracle), "W", oracle);
    TopForms.push(top);
    Value.push(topExtraction(recipe, top.Value, WEIGHTS[piece]));
    Metric.push(topExtraction(recipe, top.H, WEIGHTS[piece]));
    Field.push(topExtraction(recipe, top.U, WEIGHTS[piece]));
    Mixed.push(topExtraction(recipe, top.HU, WEIGHTS[piece]));
  }
  return Object.freeze({ Stages, TopForms: Object.freeze(TopForms), Value: Object.freeze(Value),
    Metric: Object.freeze(Metric), Field: Object.freeze(Field), Mixed: Object.freeze(Mixed) });
}

function buildMixedWardAction(recipe, b, g, eta, nativeDeta, compensated, oracle = false) {
  const bi = new BiTensorRecipe(recipe); flag(compensated); flag(oracle);
  need(b !== null && typeof b === "object" && g !== null && typeof g === "object", "background and germ handles required");
  recipe.MatrixEntries(b.Frame); recipe.MatrixEntries(g.Motion);
  for (const [tensor, degree] of [[eta, 0], [nativeDeta, 1], [b.X, 1], [b.B, 1], [b.F, 2], [b.DX, 2],
    [g.DeltaB, 1], [g.DeltaFFixed, 2], [g.DeltaBExterior, 2], [g.DeltaCurvatureFromConnection, 2]]) typed(recipe, tensor, degree);
  const mul = (x, y) => bi.Product(x, y, "W", oracle), neg = x => bi.Scale(x, "-1");
  const conjugate = (left, x, right) => mul(mul(left, x), right);
  const identity = recipe.Unit(0, 0, "1"), empty = recipe.FreshZero();
  const deta = recipe.Pullback(b.Frame, nativeDeta);
  const dbeta = recipe.Sum(deta, recipe.P(b.B, eta, "C"));
  const acceleration = recipe.P(g.DeltaB, eta, "C");
  const tangent = compensated ? recipe.Sum(dbeta, recipe.P(b.X, eta, "C")) : recipe.FreshZero();
  const dbTangent = compensated ? recipe.Sum(recipe.P(b.F, eta, "C"), recipe.P(b.DX, eta, "C"),
    recipe.Times(recipe.P(b.X, dbeta, "C"), "-1")) : recipe.FreshZero();

  const epsilon = bi.Tensor(identity, empty, eta, empty);
  const inverse = bi.Tensor(identity, empty, recipe.Times(eta, "-1"), empty);
  const depsilon = bi.Tensor(empty, empty, dbeta, acceleration);
  const dinverse = bi.Tensor(empty, empty, recipe.Times(dbeta, "-1"), recipe.Times(acceleration, "-1"));
  const d2epsilon = bi.Tensor(empty, empty, recipe.P(b.F, eta, "C"), recipe.P(g.DeltaFFixed, eta, "C"));
  // This independent connection expansion must never be replaced by the
  // DeltaFFixed commutator, even though the full coefficients must agree.
  const d2Expanded = bi.Tensor(empty, empty, d2epsilon.U,
    recipe.Sum(recipe.P(g.DeltaBExterior, eta, "C"), recipe.Times(recipe.P(g.DeltaB, deta, "C"), "-1"),
      recipe.P(g.DeltaB, dbeta, "C"), recipe.P(b.B, acceleration, "C")));
  const curvatureVariationCommutator = bi.Tensor(empty, empty, empty, recipe.P(g.DeltaCurvatureFromConnection, eta, "C"));
  const referenceCurvature = bi.Tensor(b.F, g.DeltaFFixed, empty, empty);
  const difference = mul(inverse, depsilon);
  const differenceDerivative = bi.Add(mul(dinverse, depsilon), mul(inverse, d2epsilon));

  // Tangent is FIXED in native identification. Following D_A(h) eta here
  // would introduce acceleration and make the mixed cancellation circular.
  const varpi = bi.Tensor(b.X, empty, tangent, empty);
  const dbVarpi = bi.Tensor(b.DX, recipe.P(g.DeltaB, b.X, "C"), dbTangent, recipe.P(g.DeltaB, tangent, "C"));
  const t = bi.Add(varpi, neg(difference));
  const dt = bi.Add(dbVarpi, neg(differenceDerivative));
  const rotatedCurvature = bi.Add(referenceCurvature, differenceDerivative, mul(difference, difference));
  const covariant = bi.Add(dt, bi.Product(difference, t, "C", oracle));
  const quadratic = mul(t, t);

  const phi = gamma => bi.Tensor(gamma, recipe.Times(recipe.Motion(g.Motion, gamma), "-1"), empty, empty);
  const unrotatedFirst = phi(recipe.Phi1), unrotatedOuter = phi(recipe.Phi1), unrotatedInner = phi(recipe.Phi2);
  const first = conjugate(inverse, unrotatedFirst, epsilon);
  const outer = conjugate(inverse, unrotatedOuter, epsilon);
  const inner = conjugate(inverse, unrotatedInner, epsilon);
  const literal = buildWardDensity(recipe, t, rotatedCurvature, covariant, quadratic, first, outer, inner, g.Motion, oracle);

  const descended = conjugate(epsilon, t, inverse);
  const descendedDerivative = bi.Add(mul(mul(depsilon, t), inverse), conjugate(epsilon, dt, inverse), neg(mul(mul(epsilon, t), dinverse)));
  const derivativeOracle = conjugate(epsilon, covariant, inverse);
  const descendedCurvature = conjugate(epsilon, rotatedCurvature, inverse);
  const descendedQuadratic = mul(descended, descended);
  const baseDensity = buildWardDensity(recipe, descended, referenceCurvature, descendedDerivative, descendedQuadratic,
    unrotatedFirst, unrotatedOuter, unrotatedInner, g.Motion, oracle);
  const primitive = (Name, Tensor) => Object.freeze({ Name, Tensor });
  // Array construction order matters: inverse controls and the final three
  // conjugations run only AFTER both complete densities, as in the C# source.
  const Primitives = Object.freeze([
    primitive("epsilon", epsilon), primitive("inverse", inverse), primitive("epsilonInverse", mul(epsilon, inverse)),
    primitive("inverseEpsilon", mul(inverse, epsilon)), primitive("referenceCurvature", referenceCurvature),
    primitive("DBepsilon", depsilon), primitive("DBinverse", dinverse), primitive("DBSquaredEpsilon", d2epsilon),
    primitive("DBSquaredEpsilonExpanded", d2Expanded), primitive("curvatureVariationCommutator", curvatureVariationCommutator),
    primitive("gaugeDifference", difference), primitive("DBgaugeDifference", differenceDerivative),
    primitive("varpi", varpi), primitive("DBvarpi", dbVarpi), primitive("T", t), primitive("DBT", dt),
    primitive("rotatedCurvature", rotatedCurvature), primitive("covariantT", covariant), primitive("quadraticT", quadratic),
    primitive("unrotatedPhiFirst", unrotatedFirst), primitive("unrotatedPhiOuter", unrotatedOuter), primitive("unrotatedPhiInner", unrotatedInner),
    primitive("phiFirst", first), primitive("phiOuter", outer), primitive("phiInner", inner), primitive("descendedT", descended),
    primitive("descendedDerivative", descendedDerivative), primitive("descendedDerivativeOracle", derivativeOracle),
    primitive("descendedCurvature", descendedCurvature), primitive("descendedQuadratic", descendedQuadratic),
    primitive("descendedPhiFirst", conjugate(epsilon, first, inverse)),
    primitive("descendedPhiOuter", conjugate(epsilon, outer, inverse)),
    primitive("descendedPhiInner", conjugate(epsilon, inner, inverse))
  ]);
  return Object.freeze({ Compensated: compensated, Oracle: oracle, Eta: eta, NativeDeta: nativeDeta,
    Deta: deta, DBeta: dbeta, Tangent: tangent, DBTangent: dbTangent, Primitives, Literal: literal, Descended: baseDensity });
}

module.exports = { buildMixedWardAction, buildWardChain, buildWardDensity, wardConjugate };
