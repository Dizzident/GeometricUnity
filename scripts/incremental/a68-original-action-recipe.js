"use strict";

// Metadata-only transcription of OriginalMixedAction.cs, including operation
// order and lazy empty FT identities. This does not evaluate tensor entries or
// scalar integrals. The caller supplies independently bound background, germ
// and native probe handles, and marks the returned stages only AFTER Evaluate,
// just as the C# structured evidence callback does. No marks are hidden here.
const { MixedRecipe } = require("./a68-mixed-recipe");
const need = (condition, message) => { if (!condition) throw new Error("A68 original action recipe: " + message); };
const FIELDS = Object.freeze(["Value", "H", "U", "HU"]);
const WEIGHTS = Object.freeze(["1", "1/2", "1/3", "453856"]);
const TOP_SCHEMA = "phase627-original-action-top-extraction-v1";
const typed = (recipe, tensor, degree) => {
  const actual = recipe.TensorDegree(tensor);
  need(actual === -1 || actual === degree, "boundary tensor degree " + degree);
  return tensor;
};
const flag = value => need(typeof value === "boolean", "explicit boolean route flag");

class BiTensorRecipe {
  #recipe;
  constructor(recipe) {
    need(recipe instanceof MixedRecipe, "MixedRecipe instance required");
    this.#recipe = recipe;
  }
  #check(x) {
    need(x !== null && typeof x === "object" && !Array.isArray(x) &&
      JSON.stringify(Object.keys(x)) === JSON.stringify(FIELDS), "closed ordered BiTensor descriptor");
    const degrees = FIELDS.map(field => this.#recipe.TensorDegree(x[field])).filter(d => d >= 0);
    need(new Set(degrees).size <= 1, "BiTensor component degree mismatch");
    return x;
  }
  Tensor(Value, H, U, HU) { return Object.freeze(this.#check({ Value, H, U, HU })); }
  Fixed(t) {
    this.#recipe.TensorDegree(t);
    return this.Tensor(t, this.#recipe.FreshZero(), this.#recipe.FreshZero(), this.#recipe.FreshZero());
  }
  Add(...values) {
    values.forEach(x => this.#check(x));
    const r = this.#recipe;
    return this.Tensor(r.Sum(...values.map(x => x.Value)), r.Sum(...values.map(x => x.H)),
      r.Sum(...values.map(x => x.U)), r.Sum(...values.map(x => x.HU)));
  }
  Scale(x, scalar) {
    this.#check(x); const r = this.#recipe;
    return this.Tensor(r.Times(x.Value, scalar), r.Times(x.H, scalar), r.Times(x.U, scalar), r.Times(x.HU, scalar));
  }
  Product(x, y, kind = "W", reverseOrder = false) {
    this.#check(x); this.#check(y); flag(reverseOrder);
    const r = this.#recipe;
    const value = r.P(x.Value, y.Value, kind);
    const h = r.Sum(r.P(x.H, y.Value, kind), r.P(x.Value, y.H, kind));
    const u = r.Sum(r.P(x.U, y.Value, kind), r.P(x.Value, y.U, kind));
    // The word-product route uses the same primitive descriptor, but its HU
    // contributions are deliberately constructed and accumulated in this
    // different order. There is no factorial 2 and no operand interchange.
    const hu = reverseOrder
      ? r.Sum(r.P(x.U, y.H, kind), r.P(x.Value, y.HU, kind), r.P(x.HU, y.Value, kind), r.P(x.H, y.U, kind))
      : r.Sum(r.P(x.HU, y.Value, kind), r.P(x.H, y.U, kind), r.P(x.U, y.H, kind), r.P(x.Value, y.HU, kind));
    return this.Tensor(value, h, u, hu);
  }
  Star(x, motion, oracle = false) {
    this.#check(x); flag(oracle); const r = this.#recipe;
    r.MatrixEntries(motion);
    return this.Tensor(r.Hodge(x.Value), r.Sum(r.Hodge(x.H), r.StarDelta(motion, x.Value)),
      r.Hodge(x.U), r.Sum(r.Hodge(x.HU), r.StarDelta(motion, x.U)));
  }
  Chain(input, motion, oracle = false) {
    this.#check(input); flag(oracle); const r = this.#recipe;
    FIELDS.forEach(field => typed(r, input[field], 2)); r.MatrixEntries(motion);
    // Do not hoist Phi1/Phi2 initialization: their first getters emit the
    // per-session solder units at precisely these expression boundaries.
    const p1 = this.Tensor(r.Phi1, r.Times(r.Motion(motion, r.Phi1), "-1"), r.FreshZero(), r.FreshZero());
    const p2 = this.Tensor(r.Phi2, r.Times(r.Motion(motion, r.Phi2), "-1"), r.FreshZero(), r.FreshZero());
    const sf = this.Star(input, motion, oracle);
    const first = this.Product(p1, sf, "C", oracle);
    const inner = this.Product(p2, sf, "A", oracle);
    const zero = this.Star(inner, motion, oracle);
    const outer = this.Product(p1, zero, "A", oracle);
    const upper = this.Add(first, this.Scale(this.Star(outer, motion, oracle), "-1/2"));
    const lower = this.Star(upper, motion, oracle);
    return Object.freeze([input, sf, first, inner, zero, outer, upper, lower]);
  }
}

function topExtraction(recipe, tensor, weight) {
  typed(recipe, tensor, 14);
  // Fourier.Top checks degree 14, takes the (Full,0,0,0) coefficient (zero
  // when absent), requires its imaginary part to vanish, and negates its real
  // part. Weighting is scalar postprocessing, NOT an extra DAG scale node.
  return Object.freeze({ schemaVersion: TOP_SCHEMA, tensor, node: recipe.RecordedNode(tensor), degree: 14,
    form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0",
    imaginaryRequired: "0", realFactor: "-1", weight });
}

function buildOriginalMixedAction(recipe, background, germ, nativeU, nativeDu, reverseOrder = false) {
  const bi = new BiTensorRecipe(recipe); flag(reverseOrder);
  need(background !== null && typeof background === "object" && germ !== null && typeof germ === "object", "background and germ handles required");
  recipe.MatrixEntries(background.Frame); recipe.MatrixEntries(germ.Motion);
  for (const [tensor, degree] of [[background.X, 1], [background.B, 1], [background.NativeExterior, 2],
    [background.F, 2], [germ.DeltaB, 1], [germ.DeltaFFixed, 2], [nativeU, 1], [nativeDu, 2]]) typed(recipe, tensor, degree);

  const u = recipe.Pullback(background.Frame, nativeU);
  const du = recipe.Pullback(background.Frame, nativeDu);
  const x = bi.Tensor(background.X, recipe.FreshZero(), u, recipe.FreshZero());
  const spin = bi.Tensor(background.B, germ.DeltaB, recipe.FreshZero(), recipe.FreshZero());
  const partial = bi.Tensor(recipe.Pullback(background.Frame, background.NativeExterior), recipe.FreshZero(), du, recipe.FreshZero());
  const covariant = bi.Add(partial, bi.Product(spin, x, "C", reverseOrder));
  const curvature = bi.Tensor(background.F, germ.DeltaFFixed, recipe.FreshZero(), recipe.FreshZero());
  const quadratic = bi.Product(x, x, "W", reverseOrder);
  const Stages = Object.freeze([bi.Chain(curvature, germ.Motion, reverseOrder),
    bi.Chain(covariant, germ.Motion, reverseOrder), bi.Chain(quadratic, germ.Motion, reverseOrder)]);
  const TopForms = [], Value = [], Metric = [], Field = [], Mixed = [];
  for (let p = 0; p < 4; p++) {
    const top = bi.Product(x, bi.Star(p < 3 ? Stages[p][7] : x, germ.Motion, reverseOrder), "W", reverseOrder);
    TopForms.push(top);
    Value.push(topExtraction(recipe, top.Value, WEIGHTS[p]));
    Metric.push(topExtraction(recipe, top.H, WEIGHTS[p]));
    Field.push(topExtraction(recipe, top.U, WEIGHTS[p]));
    Mixed.push(topExtraction(recipe, top.HU, WEIGHTS[p]));
  }
  // TopForms is a metadata retention aid: these complete tensors really are
  // computed by C#, although OriginalMixedResult exposes only their scalars.
  // It does not introduce an additional callback/mark into the frozen menu.
  return Object.freeze({ Stages, TopForms: Object.freeze(TopForms), Value: Object.freeze(Value),
    Metric: Object.freeze(Metric), Field: Object.freeze(Field), Mixed: Object.freeze(Mixed) });
}

module.exports = { BiTensorRecipe, buildOriginalMixedAction, FIELDS, WEIGHTS, TOP_SCHEMA };
