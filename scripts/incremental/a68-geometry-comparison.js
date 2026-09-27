"use strict";

// Exhaustive coordinate comparison, not source authentication. The closed
// source wrapper separately requires branded reconstructed and pinned inputs.
// Supports manufactured dimensions 1..14; never evaluates source geometry.
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const need = (ok, why) => { if (!ok) throw new Error("A68 geometry comparison: " + why); };
const keys = (x, expected) => x && typeof x === "object" && !Array.isArray(x) &&
  JSON.stringify(Object.keys(x)) === JSON.stringify(expected);
class GeometryComparison {
  #a; #limits; #used; #failed = false;
  constructor(a, limits) {
    need(a instanceof GeometryAlgebra, "bounded exact algebra required");
    const names = ["coordinateVisits", "sparseRecords", "tensorRecords", "rationalCharacters"];
    need(keys(limits, names) && names.every(k => Number.isSafeInteger(limits[k]) && limits[k] > 0), "explicit ordered finite limits");
    this.#a = a; this.#limits = Object.freeze({ ...limits }); this.#used = Object.fromEntries(names.map(k => [k, 0])); Object.freeze(this);
  }
  #run(action) {
    need(!this.#failed, "poisoned comparison");
    try { return action(); } catch (error) { this.#failed = true; throw error; }
  }
  #charge(kind, count) {
    need(Number.isSafeInteger(count) && count >= 0 && count <= this.#limits[kind] - this.#used[kind], "pre-comparison " + kind + " ceiling"); this.#used[kind] += count;
  }
  #rational(value) {
    need(typeof value === "string", "rational text"); this.#charge("rationalCharacters", value.length); return this.#a.parse(value);
  }
  #array(value, count) {
    need(Array.isArray(value) && value.length === count && Object.keys(value).length === count, "complete dense array");
    for (let i = 0; i < count; i++) need(Object.hasOwn(value, i), "no sparse array holes");
  }
  dense(matrix, expected, transposeIndices = false) {
    return this.#run(() => {
      need(typeof transposeIndices === "boolean", "explicit dense index convention");
      const a = this.#a, n = a.size(matrix); this.#array(expected, n); expected.forEach(row => this.#array(row, n));
      this.#charge("coordinateVisits", n * n);
      for (let i = 0; i < n; i++) for (let j = 0; j < n; j++)
        need(a.equal(a.get(matrix, transposeIndices ? j : i, transposeIndices ? i : j), this.#rational(expected[i][j])), "dense matrix mismatch at " + i + "," + j);
      return true;
    });
  }
  denseMatrices(matrices, expected) {
    return this.#run(() => {
      need(Array.isArray(matrices) && matrices.length > 0, "dense matrix jet");
      const n = this.#a.size(matrices[0]); this.#array(matrices, n); this.#array(expected, n);
      for (let i = 0; i < n; i++) { need(this.#a.size(matrices[i]) === n, "dense jet dimension"); this.dense(matrices[i], expected[i]); }
      return true;
    });
  }
  denseBlocks(blocks, expected, curvature = false) {
    return this.#run(() => {
      need(Array.isArray(blocks) && blocks.length > 0 && Array.isArray(blocks[0]) && typeof curvature === "boolean", "dense block jet");
      const n = this.#a.size(blocks[0][0]); this.#array(blocks, n); this.#array(expected, n);
      for (let i = 0; i < n; i++) {
        this.#array(blocks[i], n); this.#array(expected[i], n);
        for (let j = 0; j < n; j++) {
          need(this.#a.size(blocks[i][j]) === n, "dense block dimension");
          this.dense(blocks[i][j], expected[i][j], curvature);
        }
      }
      return true;
    });
  }
  metricJet(jet, expected) {
    return this.#run(() => {
      need(keys(expected, ["g", "d", "dd"]), "complete metric metadata shape");
      this.dense(jet.Value, expected.g); this.denseMatrices(jet.D, expected.d); this.denseBlocks(jet.DD, expected.dd); return true;
    });
  }
  connection(connection, expected) {
    return this.#run(() => {
      need(keys(expected, ["gamma", "dGamma", "curvature"]), "complete connection metadata shape");
      this.denseMatrices(connection.Gamma, expected.gamma); this.denseBlocks(connection.DGamma, expected.dGamma);
      this.denseBlocks(connection.Curvature, expected.curvature, true); return true;
    });
  }
  #sparse(records, names, n, valueAt) {
    need(Array.isArray(records), "sparse record array");
    this.#charge("sparseRecords", records.length); this.#array(records, records.length);
    const count = n ** names.length; need(records.length <= count, "sparse carrier bound");
    this.#charge("coordinateVisits", count);
    let at = 0, previous = -1, next = null;
    const read = () => {
      if (at === records.length) { next = null; return; }
      const row = records[at++]; need(keys(row, [...names, "value"]), "closed ordered sparse record");
      let index = 0;
      for (const name of names) { need(Number.isInteger(row[name]) && row[name] >= 0 && row[name] < n, "sparse coordinate domain"); index = index * n + row[name]; }
      need(index > previous, "strict unique sparse order"); previous = index;
      const value = this.#rational(row.value); need(!this.#a.isZero(value), "sparse zero must be omitted"); next = { index, value };
    };
    read();
    for (let index = 0; index < count; index++) {
      const value = next?.index === index ? next.value : this.#a.zero;
      need(this.#a.equal(valueAt(index), value), "complete sparse coordinate mismatch at " + index);
      if (next?.index === index) read();
    }
    need(next === null && at === records.length, "complete sparse stream consumed"); return true;
  }
  matrices(matrices, expected) {
    return this.#run(() => {
      need(Array.isArray(matrices) && matrices.length > 0, "matrix jet array");
      const n = this.#a.size(matrices[0]); this.#array(matrices, n);
      matrices.forEach(m => need(this.#a.size(m) === n, "matrix jet dimensions"));
      return this.#sparse(expected, ["z", "i", "j"], n, index => {
        const j = index % n, i = Math.floor(index / n) % n, z = Math.floor(index / (n * n));
        return this.#a.get(matrices[z], i, j);
      });
    });
  }
  blocks(blocks, expected, curvature = false) {
    return this.#run(() => {
      need(Array.isArray(blocks) && blocks.length > 0 && Array.isArray(blocks[0]) && typeof curvature === "boolean", "matrix block array");
      const n = this.#a.size(blocks[0][0]); this.#array(blocks, n);
      blocks.forEach(row => { this.#array(row, n); row.forEach(m => need(this.#a.size(m) === n, "matrix block dimensions")); });
      const names = curvature ? ["a", "b", "c", "d"] : ["z", "w", "i", "j"];
      return this.#sparse(expected, names, n, index => {
        const j = index % n, i = Math.floor(index / n) % n, w = Math.floor(index / n ** 2) % n, z = Math.floor(index / n ** 3);
        // Retained R[a,b,c,d] has output d, input c. Matrix is row=d,col=c.
        return this.#a.get(blocks[z][w], curvature ? j : i, curvature ? i : j);
      });
    });
  }
  tensor(actual, expected, degree) {
    return this.#run(() => {
      need(Array.isArray(actual) && Array.isArray(expected) && Number.isInteger(degree) && degree >= 0 && degree <= 14, "typed tensor arrays");
      this.#charge("tensorRecords", actual.length + expected.length);
      this.#array(actual, actual.length); this.#array(expected, expected.length);
      need(actual.length === expected.length, "complete tensor support census");
      let previous = -1;
      for (let i = 0; i < actual.length; i++) {
        let key;
        for (const record of [actual[i], expected[i]]) {
          need(keys(record, ["form", "blade", "k0", "k1", "real", "imaginary"]), "closed tensor record");
          need(Number.isInteger(record.form) && record.form >= 0 && record.form < 16384 && Number.isInteger(record.blade) && record.blade >= 0 && record.blade < 16384 && record.k0 === 0 && record.k1 === 0, "local tensor coordinates");
          let form = record.form, count = 0; while (form) { count += form & 1; form >>>= 1; }
          need(count === degree, "tensor form degree");
          const current = record.form * 16384 + record.blade;
          need(current > previous && (key === undefined || key === current), "complete ordered tensor coordinates"); key = current;
          const re = this.#rational(record.real), im = this.#rational(record.imaginary);
          need(!this.#a.isZero(re) || !this.#a.isZero(im), "tensor zero omitted");
        }
        need(actual[i].real === expected[i].real && actual[i].imaginary === expected[i].imaginary, "complete tensor coefficient mismatch"); previous = key;
      }
      return true;
    });
  }
  snapshot() { return Object.freeze({ ...this.#used, failed: this.#failed, sourceAuthenticityEstablished: false, processMemoryProved: false }); }
}
module.exports = { GeometryComparison };
