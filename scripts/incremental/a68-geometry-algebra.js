"use strict";

// Independent exact MATRIX arithmetic for source geometry, not Clifford
// evaluation. Every instance requires explicit prospective admission limits.
// Cumulative allocation/operation counters are conservative logical charges,
// NOT allocator/RSS or wall-time bounds. No production limits are supplied.
const need = (ok, why) => { if (!ok) throw new Error("A68 geometry algebra: " + why); };
const safe = n => Number.isSafeInteger(n) && n > 0;
const gcd = (a, b) => { a = a < 0n ? -a : a; while (b) [a, b] = [b, a % b]; return a; };
class GeometryAlgebra {
  #limits; #used; #rationals = new WeakSet(); #matrices = new WeakMap(); #failed = false;
  constructor(limits) {
    const keys = ["maxBits", "scalarOperations", "rationalObjects", "matrixObjects", "matrixCells", "slotVisits"];
    need(limits && Object.keys(limits).length === keys.length && keys.every(k => safe(limits[k])), "explicit complete finite limits");
    need(limits.maxBits >= 16 && limits.maxBits <= 1048576, "bounded bit limit");
    this.#limits = Object.freeze({ ...limits }); this.#used = Object.fromEntries(keys.filter(k => k !== "maxBits").map(k => [k, 0]));
    this.zero = this.#make(0n, 1n); this.one = this.#make(1n, 1n);
    Object.freeze(this);
  }
  #check(ok, why) { if (!ok) this.#failed = true; need(ok, why); }
  #charge(kind, n = 1) {
    this.#check(!this.#failed, "poisoned arithmetic context");
    this.#check(Number.isSafeInteger(n) && n >= 0 && n <= this.#limits[kind] - this.#used[kind], "pre-operation " + kind + " ceiling");
    this.#used[kind] += n;
  }
  #bits(n) { return (n < 0n ? -n : n).toString(2).length; }
  #product(a, b) {
    if (!a || !b) return 0n;
    this.#check(this.#bits(a) + this.#bits(b) <= this.#limits.maxBits, "pre-product rational bit ceiling"); return a * b;
  }
  #sum(a, b) {
    this.#check(Math.max(this.#bits(a), this.#bits(b)) + 1 <= this.#limits.maxBits, "pre-addition rational bit ceiling"); return a + b;
  }
  #make(n, d) {
    this.#charge("rationalObjects"); this.#check(d !== 0n, "nonzero denominator");
    if (d < 0n) { n = -n; d = -d; }
    this.#check(this.#bits(n) <= this.#limits.maxBits && this.#bits(d) <= this.#limits.maxBits, "rational bit ceiling");
    const g = gcd(n, d), value = Object.freeze([n / g, d / g]); this.#rationals.add(value); return value;
  }
  #q(q) { this.#check(!this.#failed && this.#rationals.has(q), "poisoned context or foreign rational"); return q; }
  parse(text) {
    this.#charge("scalarOperations");
    this.#check(typeof text === "string" && text.length <= Math.ceil(this.#limits.maxBits / 3) + 2 && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded canonical rational syntax");
    const parts = text.split("/"), n = BigInt(parts[0]), d = parts.length === 1 ? 1n : BigInt(parts[1]);
    this.#check(this.#bits(n) <= this.#limits.maxBits && this.#bits(d) <= this.#limits.maxBits, "parsed rational bit ceiling before GCD");
    this.#check(text !== "-0" && gcd(n, d) === 1n && (parts.length === 1 || d !== 1n), "reduced canonical rational"); return this.#make(n, d);
  }
  number(n) { this.#check(Number.isSafeInteger(n), "safe exact integer"); return this.parse(String(n)); }
  text(q) { const [n, d] = this.#q(q); return n.toString() + (d === 1n ? "" : "/" + d.toString()); }
  // Conservative named-string character envelope for text(), including
  // numerator/denominator strings, slash join and final join. No formatting
  // occurs to discover this bound; allocator/BigInt implementation is separate.
  textCharacterBound() { this.#check(!this.#failed, "poisoned arithmetic context"); return 8 * (this.#limits.maxBits + 2); }
  isZero(q) { return this.#q(q)[0] === 0n; }
  equal(a, b) { this.#q(a); this.#q(b); return a[0] === b[0] && a[1] === b[1]; }
  add(a, b) {
    this.#charge("scalarOperations"); this.#q(a); this.#q(b);
    const g = gcd(a[1], b[1]), ad = a[1] / g, bd = b[1] / g;
    return this.#make(this.#sum(this.#product(a[0], bd), this.#product(b[0], ad)), this.#product(ad, b[1]));
  }
  multiply(a, b) {
    this.#charge("scalarOperations"); this.#q(a); this.#q(b);
    const g = gcd(a[0], b[1]), h = gcd(b[0], a[1]);
    return this.#make(this.#product(a[0] / g, b[0] / h), this.#product(a[1] / h, b[1] / g));
  }
  negate(a) { this.#charge("scalarOperations"); this.#q(a); return this.#make(-a[0], a[1]); }
  reciprocal(a) { this.#charge("scalarOperations"); this.#q(a); return this.#make(a[1], a[0]); }
  subtract(a, b) { return this.add(a, this.negate(b)); }
  sum(...values) { this.#check(!this.#failed, "poisoned arithmetic context"); return values.reduce((a, b) => this.add(a, b), this.zero); }
  #reserveMatrix(n, copies = 1) {
    this.#check(Number.isSafeInteger(n) && n >= 1 && n <= 14, "matrix dimension1..14");
    this.#charge("matrixObjects", copies); this.#charge("matrixCells", n * n * copies); this.#charge("slotVisits", n * n * copies);
  }
  #wrap(rows) {
    const token = Object.freeze({ dimension: rows.length }); rows.forEach(Object.freeze); this.#matrices.set(token, Object.freeze(rows)); return token;
  }
  #rows(matrix) { this.#check(!this.#failed && this.#matrices.has(matrix), "poisoned context or foreign matrix"); return this.#matrices.get(matrix); }
  matrix(n, entry = () => this.zero) {
    this.#reserveMatrix(n); this.#check(typeof entry === "function", "matrix entry callback");
    try { return this.#wrap(Array.from({ length: n }, (_, i) => Array.from({ length: n }, (_, j) => this.#q(entry(i, j))))); }
    catch (error) { this.#failed = true; throw error; }
  }
  fromText(rows) {
    this.#check(Array.isArray(rows) && rows.length >= 1 && rows.length <= 14 && rows.every(r => Array.isArray(r) && r.length === rows.length), "square matrix text");
    return this.matrix(rows.length, (i, j) => this.parse(rows[i][j]));
  }
  size(a) { return this.#rows(a).length; }
  get(a, i, j) { const rows = this.#rows(a); this.#check(Number.isInteger(i) && Number.isInteger(j) && i >= 0 && j >= 0 && i < rows.length && j < rows.length, "matrix indices"); return rows[i][j]; }
  identity(n) { return this.matrix(n, (i, j) => i === j ? this.one : this.zero); }
  transpose(a) { const rows = this.#rows(a); return this.matrix(rows.length, (i, j) => rows[j][i]); }
  #same(a, b) { this.#check(this.size(a) === this.size(b), "matching matrix dimensions"); }
  matAdd(...matrices) {
    this.#check(matrices.length > 0, "nonempty matrix sum"); matrices.forEach(m => this.#same(matrices[0], m));
    return this.matrix(this.size(matrices[0]), (i, j) => this.sum(...matrices.map(m => this.get(m, i, j))));
  }
  matScale(a, q) { this.#q(q); return this.matrix(this.size(a), (i, j) => this.multiply(this.get(a, i, j), q)); }
  matMultiply(a, b) {
    this.#same(a, b); const n = this.size(a); this.#charge("slotVisits", n * n * n);
    return this.matrix(n, (i, j) => {
      let result = this.zero;
      for (let k = 0; k < n; k++) if (!this.isZero(this.get(a, i, k)) && !this.isZero(this.get(b, k, j))) result = this.add(result, this.multiply(this.get(a, i, k), this.get(b, k, j)));
      return result;
    });
  }
  product(...matrices) { this.#check(!this.#failed && matrices.length > 0, "poisoned context or empty matrix product"); matrices.forEach(m => this.#rows(m)); return matrices.reduce((a, b) => this.matMultiply(a, b)); }
  commutator(a, b) { return this.matAdd(this.matMultiply(a, b), this.matScale(this.matMultiply(b, a), this.number(-1))); }
  trace(a) { this.#charge("slotVisits", this.size(a)); return this.sum(...Array.from({ length: this.size(a) }, (_, i) => this.get(a, i, i))); }
  traceProduct(a, b) {
    this.#same(a, b); const n = this.size(a); this.#charge("slotVisits", n * n); let result = this.zero;
    for (let i = 0; i < n; i++) for (let j = 0; j < n; j++) if (!this.isZero(this.get(a, i, j)) && !this.isZero(this.get(b, j, i))) result = this.add(result, this.multiply(this.get(a, i, j), this.get(b, j, i)));
    return result;
  }
  inverse(matrix) {
    const n = this.size(matrix); this.#reserveMatrix(n, 2); this.#charge("slotVisits", 4 * n * n * n);
    const a = this.#rows(matrix).map(row => [...row]), b = Array.from({ length: n }, (_, i) => Array.from({ length: n }, (_, j) => i === j ? this.one : this.zero));
    for (let column = 0; column < n; column++) {
      let pivot = column; while (pivot < n && this.isZero(a[pivot][column])) pivot++;
      this.#check(pivot < n, "nonsingular source matrix");
      [a[column], a[pivot]] = [a[pivot], a[column]]; [b[column], b[pivot]] = [b[pivot], b[column]];
      const factor = this.reciprocal(a[column][column]);
      for (let j = 0; j < n; j++) { a[column][j] = this.multiply(a[column][j], factor); b[column][j] = this.multiply(b[column][j], factor); }
      for (let i = 0; i < n; i++) if (i !== column) {
        const f = a[i][column]; if (this.isZero(f)) continue;
        for (let j = 0; j < n; j++) { a[i][j] = this.subtract(a[i][j], this.multiply(f, a[column][j])); b[i][j] = this.subtract(b[i][j], this.multiply(f, b[column][j])); }
      }
    }
    return this.#wrap(b);
  }
  matrixEqual(a, b) { this.#same(a, b); this.#charge("slotVisits", this.size(a) ** 2); return this.#rows(a).every((row, i) => row.every((q, j) => this.equal(q, this.get(b, i, j)))); }
  matrixText(a) { this.#charge("slotVisits", this.size(a) ** 2); return this.#rows(a).map(row => row.map(q => this.text(q))); }
  snapshot() { return Object.freeze({ ...this.#used, failed: this.#failed, processMemoryProved: false }); }
}
module.exports = { GeometryAlgebra };
