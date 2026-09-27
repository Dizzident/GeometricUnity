"use strict";

// Complete dense values for the geometry predicates, not comparison flags.
// Scientific source wrapper is UNCALLED. Generic exporter accepts only this
// algebra's immutable matrix tokens and is tested with manufactured1D/2D data.
// Full recorded C# metadata comparison remains a SEPARATE prerequisite: both
// delta/Palatini roles here resolve to their independent common source value.
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const need = (ok, why) => { if (!ok) throw new Error("A68 source fields: " + why); };
const integer = n => Number.isSafeInteger(n) && !Object.is(n, -0) && n > 0;
const limitNames = Object.freeze(["fields", "coordinateVisits", "arrayObjects", "arraySlots", "formatCharacters", "rationalCharacters"]);
class DenseGeometryFields {
  #a; #caps; #used; #failed = false;
  constructor(a, limits) {
    need(a instanceof GeometryAlgebra, "bounded exact geometry algebra");
    need(limits && typeof limits === "object" && Reflect.ownKeys(limits).length === limitNames.length, "closed field export limits");
    const caps = {};
    for (const name of limitNames) {
      const d = Object.getOwnPropertyDescriptor(limits, name);
      need(d && Object.hasOwn(d, "value") && integer(d.value), "own finite field limit " + name); caps[name] = d.value;
    }
    need(caps.rationalCharacters <= 16384, "bounded rational character domain");
    this.#a = a; this.#caps = Object.freeze(caps);
    this.#used = Object.fromEntries(limitNames.filter(k => k !== "rationalCharacters").map(k => [k, 0])); Object.freeze(this);
  }
  #charge(key, count) {
    need(Number.isSafeInteger(count) && count >= 0 && count <= this.#caps[key] - this.#used[key], "pre-export " + key + " ceiling"); this.#used[key] += count;
  }
  #run(action) {
    need(!this.#failed, "poisoned field exporter");
    try { return action(); } catch (error) { this.#failed = true; throw error; }
  }
  #reserve(n, rank) {
    let arrays = 0, slots = 0;
    for (let i = 0; i < rank; i++) { arrays += n ** i; slots += n ** (i + 1); }
    const coordinates = n ** rank;
    // All charges precede every returned array and decimal string allocation.
    this.#charge("fields", 1); this.#charge("coordinateVisits", coordinates);
    this.#charge("arrayObjects", arrays); this.#charge("arraySlots", slots);
    this.#charge("formatCharacters", coordinates * this.#a.textCharacterBound());
  }
  #text(value) {
    const text = this.#a.text(value); need(text.length <= this.#caps.rationalCharacters, "canonical field rational height"); return text;
  }
  matrix(matrix) {
    return this.#run(() => {
      const a = this.#a, n = a.size(matrix); this.#reserve(n, 2);
      return Object.freeze(Array.from({ length: n }, (_, i) => Object.freeze(Array.from({ length: n }, (_, j) => this.#text(a.get(matrix, i, j))))));
    });
  }
  curvature(blocks) {
    return this.#run(() => {
      // Source bundles are immutable; generic input arrays must also have only
      // own data elements. Validate before touching a possible caller getter.
      const dense = (value, n) => {
        need(Array.isArray(value) && value.length === n && Reflect.ownKeys(value).length === n + 1, "complete block array");
        for (let i = 0; i < n; i++) need(Object.hasOwn(Object.getOwnPropertyDescriptor(value, String(i)) ?? {}, "value"), "own block data indices");
      };
      need(Array.isArray(blocks) && Number.isInteger(blocks.length) && blocks.length >= 1 && blocks.length <= 14, "curvature dimension1..14");
      const a = this.#a, n = blocks.length; this.#reserve(n, 4); dense(blocks, n);
      for (let i = 0; i < n; i++) { dense(blocks[i], n); for (let j = 0; j < n; j++) need(a.size(blocks[i][j]) === n, "matching curvature matrix dimensions"); }
      // R[a,b,c,d] has OUTPUT d and INPUT c. Do not export matrix[c,d].
      return Object.freeze(Array.from({ length: n }, (_, i) => Object.freeze(Array.from({ length: n }, (_, j) =>
        Object.freeze(Array.from({ length: n }, (_, c) => Object.freeze(Array.from({ length: n }, (_, d) => this.#text(a.get(blocks[i][j], d, c))))))))));
    });
  }
  snapshot() { return Object.freeze({ ...this.#used, failed: this.#failed, processMemoryProved: false }); }
}

function germGeometryFieldTarget(path) {
  need(typeof path === "string" && path.length <= 100, "bounded canonical geometry field path");
  if (path === "geometry/geometry.motion") return Object.freeze({ family: "motion", field: "value", first: null, second: null, rank: 2 });
  const m = /^geometry\/geometry\.(deltaMetric|blockMetric|deltaConnection|palatini)\.(g|d|dd|gamma|dGamma|curvature)(?:\[(0|[1-9][0-9]?)\])?(?:\[(0|[1-9][0-9]?)\])?$/.exec(path);
  need(m, "closed germ geometry field path");
  const metric = m[1] === "deltaMetric" || m[1] === "blockMetric", field = m[2];
  const rank = field === "curvature" ? 4 : 2;
  const indices = field === "d" || field === "gamma" ? 1 : field === "dd" || field === "dGamma" ? 2 : 0;
  need((metric ? ["g", "d", "dd"] : ["gamma", "dGamma", "curvature"]).includes(field) &&
    (indices >= 1 ? m[3] !== undefined && Number(m[3]) < 14 : m[3] === undefined) &&
    (indices === 2 ? m[4] !== undefined && Number(m[4]) < 14 : m[4] === undefined), "exact field index domain");
  return Object.freeze({ family: metric ? "metric" : "connection", field, first: m[3] === undefined ? null : Number(m[3]), second: m[4] === undefined ? null : Number(m[4]), rank });
}

function createSourceGermFieldResolver(a, binding, limits) {
  const { rebuilt } = boundGeometryIdentity(a, binding);
  need(binding.kind === "germ", "authenticated source germ required");
  const exporter = new DenseGeometryFields(a, limits), cache = new Map(); let failed = false;
  const resolveGeometryField = path => {
    need(!failed, "poisoned source field resolver");
    try {
      const target = germGeometryFieldTarget(path);
      if (cache.has(path)) return cache.get(path);
      let value;
      if (target.family === "motion") value = rebuilt.matrices.Motion;
      else {
        const source = target.family === "metric" ? rebuilt.germ.metricVariation : rebuilt.connectionVariation;
        value = source[{ g: "Value", d: "D", dd: "DD", gamma: "Gamma", dGamma: "DGamma", curvature: "Curvature" }[target.field]];
        if (target.first !== null) value = value[target.first];
        if (target.second !== null) value = value[target.second];
      }
      const result = target.rank === 4 ? exporter.curvature(value) : exporter.matrix(value);
      cache.set(path, result); return result;
    } catch (error) { failed = true; cache.clear(); throw error; }
  };
  return Object.freeze({ resolveGeometryField, snapshot: () => Object.freeze({ ...exporter.snapshot(), failed, cachedFields: cache.size }),
    scope: Object.freeze({ sourceGeometryValuesBound: true, recordedMetadataCompared: false, upstreamCertificateReplayed: false,
      wholeContextMemoryProved: false, scientificExecutionAuthorized: false }) });
}
module.exports = { DenseGeometryFields, germGeometryFieldTarget, createSourceGermFieldResolver };
