"use strict";

// PARTIAL metadata-only semantic-plan building block. No tensor coefficients,
// numerical replay or study execution occur here. The complete independent
// background/700-germ/Ward recipe and its review are STILL REQUIRED.
// Inputs are frozen constants and independently reconstructed geometry, NEVER
// parameter values, identities or order copied from an observed result graph.
const SCHEMA = "phase627-typed-mixed-dag-v1";
const PLAN_SCHEMA = "phase627-symbolic-mixed-recipe-v1";
const need = (condition, message) => { if (!condition) throw new Error("A68 recipe: " + message); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const ascii = value => typeof value === "string" && /^[\x20-\x7e]*$/.test(value);
const name = value => ascii(value) && value.trim().length > 0;
const integer = (value, min, max) => Number.isSafeInteger(value) && value >= min && value <= max;
const shape = (value, keys) => need(value !== null && typeof value === "object" && !Array.isArray(value) && same(Object.keys(value), keys), "closed ordered descriptor");
const freeze = value => {
  if (value !== null && typeof value === "object") { Object.values(value).forEach(freeze); Object.freeze(value); }
  return value;
};
const clone = value => JSON.parse(JSON.stringify(value));
const degree = mask => { let count = 0; while (mask) { count += mask & 1; mask >>>= 1; } return count; };
const sigma = axis => axis < 7 ? 1 : -1;
const gcd = (a, b) => { a = a < 0n ? -a : a; while (b) [a, b] = [b, a % b]; return a; };
function rational(text) {
  need(typeof text === "string" && text.length > 0 && text.length <= 16384 && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "canonical rational syntax");
  const parts = text.split("/"), n = BigInt(parts[0]), d = parts.length === 1 ? 1n : BigInt(parts[1]);
  need(text !== "-0" && gcd(n, d) === 1n && (parts.length === 1 || d !== 1n), "reduced canonical rational");
  return [n, d];
}
function ratioText(n, d) {
  const factor = gcd(n, d); n /= factor; d /= factor;
  const text = n.toString() + (d === 1n ? "" : "/" + d.toString()); rational(text); return text;
}
const negative = text => { const [n, d] = rational(text); return ratioText(-n, d); };
function matrixEntries(entries) {
  need(Array.isArray(entries) && entries.length <= 196, "matrix sparse array");
  let previous = -1;
  return entries.map(entry => {
    shape(entry, ["row", "column", "value"]);
    need(integer(entry.row, 0, 13) && integer(entry.column, 0, 13) && entry.row * 14 + entry.column > previous, "canonical matrix ordering");
    const [n] = rational(entry.value); need(n !== 0n, "explicit matrix zero"); previous = entry.row * 14 + entry.column;
    return { row: entry.row, column: entry.column, value: entry.value };
  });
}

class MixedRecipe {
  #nodes = []; #marks = []; #leaves = []; #declaredLeaves = new Map(); #leafHandles = new Map();
  #handles = new WeakMap(); #matrices = new WeakMap(); #matrixNames = new Map(); #geometryScalars = new WeakMap();
  #constants = new Set(); #phi = new Map(); #closed = false; #nodeLimit; #markLimit;
  constructor({ leaves = [], constants = ["0", "1", "-1", "-1/2"], matrices = [], nodeLimit = 1000000, markLimit = 100000 } = {}) {
    need(integer(nodeLimit, 1, Number.MAX_SAFE_INTEGER) && integer(markLimit, 1, Number.MAX_SAFE_INTEGER), "positive metadata limits");
    this.#nodeLimit = nodeLimit; this.#markLimit = markLimit;
    need(Array.isArray(constants) && Array.isArray(matrices) && Array.isArray(leaves), "explicit input menus");
    for (const constant of constants) { rational(constant); need(!this.#constants.has(constant), "duplicate scalar constant"); this.#constants.add(constant); }
    for (const descriptor of matrices) {
      shape(descriptor, ["name", "matrix"]); need(name(descriptor.name) && !this.#matrixNames.has(descriptor.name), "unique named geometry matrix");
      this.#matrixNames.set(descriptor.name, this.#matrix(matrixEntries(descriptor.matrix)));
    }
    for (const descriptor of leaves) {
      shape(descriptor, ["id", "degree", "source", "sha256"]);
      need(name(descriptor.id) && name(descriptor.source) && integer(descriptor.degree, 0, 14) && typeof descriptor.sha256 === "string" && /^[0-9a-f]{64}$/.test(descriptor.sha256) && !this.#declaredLeaves.has(descriptor.id), "unique typed provenance leaf");
      this.#leaves.push(freeze(clone(descriptor)));
      this.#declaredLeaves.set(descriptor.id, this.#leaves.at(-1));
    }
  }
  #open() { need(!this.#closed, "closed recipe"); }
  #handle(id, d) { const token = Object.freeze({}); this.#handles.set(token, { id, degree: d }); return token; }
  #append(op, d, inputs, parameters) {
    this.#open(); need(this.#nodes.length < this.#nodeLimit, "node ceiling");
    const id = this.#nodes.length; this.#nodes.push(freeze({ id, op, degree: d, inputs: [...inputs], parameters: clone(parameters) }));
    return this.#handle(id, d);
  }
  #operand(token) {
    this.#open(); const state = this.#handles.get(token); need(state !== undefined, "foreign/unregistered tensor identity");
    if (state.id === null) { const recorded = this.#append("zero", -1, [], {}); state.id = this.#handles.get(recorded).id; }
    return state;
  }
  #emit(op, tokens, parameters, infer) {
    const inputs = tokens.map(token => this.#operand(token));
    return this.#append(op, infer(inputs.map(input => input.degree)), inputs.map(input => input.id), parameters);
  }
  #matrix(entries) { const token = Object.freeze({}); this.#matrices.set(token, freeze(entries)); return token; }
  #entries(token) { const result = this.#matrices.get(token); need(result !== undefined, "undeclared geometry matrix identity"); return result; }
  #scalar(text) { rational(text); need(this.#constants.has(text), "undeclared fixed scalar constant"); return text; }
  #scale(t, real, imaginary = "0") { return this.#emit("scale", [t], { real, imaginary }, ds => ds[0]); }
  #trace(a) {
    let n = 0n, d = 1n;
    for (const entry of this.#entries(a)) if (entry.row === entry.column) { const [x, y] = rational(entry.value); const text = ratioText(n * y + x * d, d * y); [n, d] = rational(text); }
    return ratioText(n, d);
  }
  // Geometry is registered at its actual construction point, not eagerly
  // moved ahead of background/metric operations to make a plan convenient.
  RegisterLeaf(id) {
    this.#open(); const descriptor = this.#declaredLeaves.get(id);
    need(descriptor && !this.#leafHandles.has(id), "undeclared or repeated leaf registration");
    const handle = this.#append("leaf", descriptor.degree, [], { id }); this.#leafHandles.set(id, handle); return handle;
  }
  Leaf(id) { this.#open(); need(this.#leafHandles.has(id), "unregistered leaf lookup"); return this.#leafHandles.get(id); }
  TensorDegree(token) { this.#open(); const state = this.#handles.get(token); need(state !== undefined, "foreign tensor identity"); return state.degree; }
  // Bind scalar extraction/identity plans without emitting a hidden mark or
  // forcing an otherwise-unused lazy zero into the expression stream.
  RecordedNode(token) { this.#open(); const state = this.#handles.get(token); need(state !== undefined && state.id !== null, "tensor must already have a recorded node"); return state.id; }
  Matrix(id) { this.#open(); need(this.#matrixNames.has(id), "undeclared matrix lookup"); return this.#matrixNames.get(id); }
  MatrixEntries(matrix) { this.#open(); return this.#entries(matrix); }
  MatrixValue(matrix, row, column) {
    this.#open(); need(integer(row, 0, 13) && integer(column, 0, 13), "matrix coefficient indices");
    return this.#entries(matrix).find(e => e.row === row && e.column === column)?.value ?? "0";
  }
  TimesMatrixEntry(t, matrix, row, column) { return this.#scale(t, this.MatrixValue(matrix, row, column)); }
  TimesMatrixProduct(t, left, row, column, right, otherRow, otherColumn) {
    const [a, b] = rational(this.MatrixValue(left, row, column)), [c, d] = rational(this.MatrixValue(right, otherRow, otherColumn));
    return this.#scale(t, ratioText(a * c, b * d));
  }
  TransposeMatrix(matrix) {
    this.#open(); return this.#matrix(this.#entries(matrix).map(e => ({ row: e.column, column: e.row, value: e.value })).sort((a, b) => a.row * 14 + a.column - b.row * 14 - b.column));
  }
  #geometryScalar(value) { const token = Object.freeze({}); this.#geometryScalars.set(token, value); return token; }
  ScalarValue(token) { this.#open(); const value = this.#geometryScalars.get(token); need(value !== undefined, "foreign geometry scalar identity"); return value; }
  ScalarConstant(value) { this.#open(); return this.#geometryScalar(this.#scalar(value)); }
  MatrixCoefficient(matrix, row, column) { return this.#geometryScalar(this.MatrixValue(matrix, row, column)); }
  ScalarAdd(left, right) {
    const [a, b] = rational(this.ScalarValue(left)), [c, d] = rational(this.ScalarValue(right)); return this.#geometryScalar(ratioText(a * d + c * b, b * d));
  }
  ScalarMultiply(left, right) {
    const [a, b] = rational(this.ScalarValue(left)), [c, d] = rational(this.ScalarValue(right)); return this.#geometryScalar(ratioText(a * c, b * d));
  }
  NegativeMatrixTrace(matrix) { this.#open(); return this.#geometryScalar(negative(this.#trace(matrix))); }
  ScaleWithScalar(t, scalar) { return this.#scale(t, this.ScalarValue(scalar)); }
  CombineMatrices(matrices, weights) {
    this.#open(); need(Array.isArray(matrices) && matrices.length === 14 && Array.isArray(weights) && weights.length === 14, "complete fourteen-slot matrix combination");
    matrices.forEach(m => this.#entries(m)); weights.forEach(w => this.ScalarValue(w));
    const entries = [];
    for (let row = 0; row < 14; row++) for (let column = 0; column < 14; column++) {
      let value = this.ScalarConstant("0");
      for (let mu = 0; mu < 14; mu++) value = this.ScalarAdd(value, this.ScalarMultiply(weights[mu], this.MatrixCoefficient(matrices[mu], row, column)));
      if (this.ScalarValue(value) !== "0") entries.push({ row, column, value: this.ScalarValue(value) });
    }
    return this.#matrix(entries);
  }
  FreshZero() { this.#open(); return this.#handle(null, -1); }
  Zero() { return this.#append("zero", -1, [], {}); }
  Unit(form, blade, real = "1", imaginary = "0") {
    need(integer(form, 0, 16383) && integer(blade, 0, 16383), "unit masks");
    return this.#append("unit", degree(form), [], { form, blade, real: this.#scalar(real), imaginary: this.#scalar(imaginary) });
  }
  Sum(...terms) {
    if (!terms.length) return this.Zero();
    return this.#emit("sum", terms, {}, ds => { const known = [...new Set(ds.filter(d => d >= 0))]; need(known.length <= 1, "sum degree mismatch"); return known.length ? known[0] : -1; });
  }
  Times(t, real) { return this.#scale(t, this.#scalar(real)); }
  Scale(t, real, imaginary) { return this.#scale(t, this.#scalar(real), this.#scalar(imaginary)); }
  P(a, b, kind = "W") { need(["W", "C", "A"].includes(kind), "product kind"); return this.#emit("product", [a, b], { kind }, ds => ds.some(d => d < 0) || ds[0] + ds[1] > 14 ? -1 : ds[0] + ds[1]); }
  Transpose(a, b, kind = "C") { need(["C", "A"].includes(kind), "transpose kind"); return this.#emit("transpose", [a, b], { kind }, ds => ds.some(d => d < 0) || ds[1] < ds[0] ? -1 : ds[1] - ds[0]); }
  Hodge(t) { return this.#emit("star", [t], {}, ds => ds[0] < 0 ? -1 : 14 - ds[0]); }
  HodgeAdjoint(t, forward) { need(integer(forward, 0, 14), "adjoint degree"); return this.#emit("starAdjoint", [t], { degree: forward }, ds => { need(ds[0] < 0 || ds[0] === 14 - forward, "adjoint input degree"); return forward; }); }
  Motion(a, t) { return this.#emit("motion", [t], { matrix: this.#entries(a) }, ds => ds[0]); }
  MetricTranspose(a) {
    return this.#matrix(this.#entries(a).map(e => ({ row: e.column, column: e.row, value: sigma(e.row) === sigma(e.column) ? e.value : negative(e.value) })).sort((x, y) => x.row * 14 + x.column - y.row * 14 - y.column));
  }
  MotionAdjoint(a, t) { return this.Motion(this.MetricTranspose(a), t); }
  PairingMotion(a, t) { return this.Sum(this.Motion(a, t), this.MotionAdjoint(a, t), this.#scale(t, negative(this.#trace(a)))); }
  Pullback(a, t) { return this.#emit("pullback", [t], { matrix: this.#entries(a) }, ds => ds[0]); }
  Spin(a) { return this.#append("spin", 0, [], { matrix: this.#entries(a) }); }
  Raise(t) { return this.#emit("raise", [t], {}, ds => ds[0]); }
  Contract(t, axis) { need(integer(axis, 0, 13), "contraction axis"); return this.#emit("contract", [t], { axis }, ds => ds[0] <= 0 ? -1 : ds[0] - 1); }
  Component(t, form) { need(integer(form, 0, 16383), "component mask"); return this.#emit("component", [t], { form }, () => 0); }
  #solder(d) {
    this.#open(); if (this.#phi.has(d)) return this.#phi.get(d);
    const terms = []; for (let a = 0; a < 14; a++) if (d === 1) terms.push(this.Unit(1 << a, 1 << a)); else for (let b = a + 1; b < 14; b++) terms.push(this.Unit((1 << a) | (1 << b), (1 << a) | (1 << b)));
    const result = this.Sum(...terms); this.#phi.set(d, result); return result;
  }
  get Phi1() { return this.#solder(1); }
  get Phi2() { return this.#solder(2); }
  Fixed(t) { need(this.#handles.has(t), "foreign fixed-jet value"); return Object.freeze({ Value: t, Delta: this.FreshZero() }); }
  #jet(value) { shape(value, ["Value", "Delta"]); need(this.#handles.has(value.Value) && this.#handles.has(value.Delta), "foreign jet identities"); return value; }
  #addJ(...jets) { jets.forEach(j => this.#jet(j)); return { Value: this.Sum(...jets.map(j => j.Value)), Delta: this.Sum(...jets.map(j => j.Delta)) }; }
  #scaleJ(j, c) { return { Value: this.Times(j.Value, c), Delta: this.Times(j.Delta, c) }; }
  #productJ(a, b, kind) { return { Value: this.P(a.Value, b.Value, kind), Delta: this.Sum(this.P(a.Delta, b.Value, kind), this.P(a.Value, b.Delta, kind)) }; }
  StarDelta(a, t) { return this.Sum(this.Hodge(this.Motion(a, t)), this.Times(this.Motion(a, this.Hodge(t)), "-1")); }
  #starJ(t, a) { return { Value: this.Hodge(t.Value), Delta: this.Sum(this.Hodge(t.Delta), this.StarDelta(a, t.Value)) }; }
  #starAdjointJ(y, d, a) {
    const value = this.HodgeAdjoint(y.Value, d);
    return { Value: value, Delta: this.Sum(this.HodgeAdjoint(y.Delta, d), this.MotionAdjoint(a, value), this.Times(this.HodgeAdjoint(this.MotionAdjoint(a, y.Value), d), "-1")) };
  }
  #transposeJ(a, b, kind) { return { Value: this.Transpose(a.Value, b.Value, kind), Delta: this.Sum(this.Transpose(a.Delta, b.Value, kind), this.Transpose(a.Value, b.Delta, kind)) }; }
  FixedForward(t) {
    const sf = this.Hodge(t), first = this.P(this.Phi1, sf, "C"), inner = this.P(this.Phi2, sf, "A");
    const zero = this.Hodge(inner), outer = this.P(this.Phi1, zero, "A"), upper = this.Sum(first, this.Times(this.Hodge(outer), "-1/2"));
    return [t, sf, first, inner, zero, outer, upper, this.Hodge(upper)];
  }
  FixedAdjoint(y) {
    const upper = this.HodgeAdjoint(y, 13), first = this.HodgeAdjoint(this.Transpose(this.Phi1, upper, "C"), 2);
    const one = this.HodgeAdjoint(upper, 1), zero = this.Transpose(this.Phi1, one, "A"), top = this.HodgeAdjoint(zero, 14);
    const second = this.Times(this.HodgeAdjoint(this.Transpose(this.Phi2, top, "A"), 2), "-1/2"); return this.Sum(first, second);
  }
  Forward(input, a) {
    this.#jet(input);
    const p1 = { Value: this.Phi1, Delta: this.Times(this.Motion(a, this.Phi1), "-1") };
    const p2 = { Value: this.Phi2, Delta: this.Times(this.Motion(a, this.Phi2), "-1") };
    const sf = this.#starJ(input, a), first = this.#productJ(p1, sf, "C"), inner = this.#productJ(p2, sf, "A");
    const zero = this.#starJ(inner, a), outer = this.#productJ(p1, zero, "A");
    const upper = this.#addJ(first, this.#scaleJ(this.#starJ(outer, a), "-1/2")), lower = this.#starJ(upper, a);
    return [input, sf, first, inner, zero, outer, upper, lower];
  }
  Reverse(y, a) {
    this.#jet(y);
    const p1 = { Value: this.Phi1, Delta: this.Times(this.Motion(a, this.Phi1), "-1") };
    const p2 = { Value: this.Phi2, Delta: this.Times(this.Motion(a, this.Phi2), "-1") };
    const upper = this.#starAdjointJ(y, 13, a), first = this.#starAdjointJ(this.#transposeJ(p1, upper, "C"), 2, a);
    const outer = this.#starAdjointJ(upper, 1, a), zero = this.#transposeJ(p1, outer, "A"), top = this.#starAdjointJ(zero, 14, a);
    const inner = this.#transposeJ(p2, top, "A"), second = this.#scaleJ(this.#starAdjointJ(inner, 2, a), "-1/2");
    return [y, upper, first, outer, zero, top, inner, second, this.#addJ(first, second)];
  }
  C(y, a) { const k = this.FixedForward(y)[7]; return this.Sum(this.FixedForward(this.Motion(a, y))[7], this.MotionAdjoint(a, k), this.#scale(k, negative(this.#trace(a)))); }
  CAdjoint(x, a) { const k = this.FixedAdjoint(x); return this.Sum(this.MotionAdjoint(a, k), this.FixedAdjoint(this.Motion(a, x)), this.#scale(k, negative(this.#trace(a)))); }
  Mark(markName, d, t, expanded) {
    this.#open(); need(name(markName) && integer(d, 0, 14) && typeof expanded === "boolean" && this.#marks.length < this.#markLimit && !this.#marks.some(m => m.name === markName), "unique typed mark descriptor");
    const state = this.#operand(t); need(state.degree < 0 || state.degree === d, "mark degree mismatch");
    this.#marks.push(freeze({ name: markName, degree: d, node: state.id, expanded }));
  }
  Finish() { this.#open(); need(this.#leafHandles.size === this.#declaredLeaves.size, "all declared leaves must be registered"); this.#closed = true; return freeze({ schemaVersion: PLAN_SCHEMA, leaves: [...this.#leaves], nodes: [...this.#nodes], marks: [...this.#marks] }); }
}

function validateMixedRecipe(graph, plan) {
  shape(plan, ["schemaVersion", "leaves", "nodes", "marks"]); need(plan.schemaVersion === PLAN_SCHEMA, "plan schema");
  shape(graph, ["schemaVersion", "leaves", "nodes", "marks"]); need(graph.schemaVersion === SCHEMA && same(graph.leaves, plan.leaves), "graph schema/exact leaf plan");
  need(Array.isArray(graph.nodes) && graph.nodes.length === plan.nodes.length && Array.isArray(graph.marks) && graph.marks.length === plan.marks.length, "exact node/mark census");
  graph.nodes.forEach((node, i) => {
    shape(node, ["id", "op", "degree", "inputs", "parameters", "records", "bytes", "sha256"]);
    need(same({ id: node.id, op: node.op, degree: node.degree, inputs: node.inputs, parameters: node.parameters }, plan.nodes[i]), "independent primitive expression at node " + i);
  });
  graph.marks.forEach((mark, i) => {
    shape(mark, ["name", "degree", "node", "expanded", "sha256"]);
    need(same({ name: mark.name, degree: mark.degree, node: mark.node, expanded: mark.expanded }, plan.marks[i]), "independent mark identity at index " + i);
  });
  // Deliberately does not approve records/bytes/hashes or physical scope.
  // Full independent numerical replay and complete scientific plan follow.
  return true;
}

module.exports = { MixedRecipe, validateMixedRecipe, SCHEMA, PLAN_SCHEMA };
