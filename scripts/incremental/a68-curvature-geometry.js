"use strict";

// Source-independent exact geometry operations. No source point, metric
// fixture, curvature coefficient or production resource ceiling is supplied.
// All scalar/matrix arithmetic and matrix allocation use GeometryAlgebra's
// prospective admission guards. Outer array construction, FT record objects,
// string conversion and sorting still require a separate full resource proof;
// these logical guards are not an allocator/RSS or runtime certificate.
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const need = (condition, message) => { if (!condition) throw new Error("A68 curvature geometry: " + message); };
const context = a => need(a instanceof GeometryAlgebra, "GeometryAlgebra context required");
function blocksDimension(a, blocks, expected) {
  context(a);
  need(Array.isArray(blocks) && blocks.length >= 1 && blocks.length <= 14, "curvature dimension 1..14");
  const n = blocks.length;
  need(expected === undefined || n === expected, "matching curvature dimension");
  need(blocks.every(row => Array.isArray(row) && row.length === n), "complete square curvature block array");
  for (const row of blocks) for (const block of row) need(a.size(block) === n, "curvature block matrix dimension");
  return n;
}
function mapsDimension(a, maps, n) {
  need(Array.isArray(maps) && maps.length === 4, "exactly four covariant frame maps");
  maps.forEach(map => need(a.size(map) === n, "matching frame map dimension"));
}
function blocks(n, entry) {
  return Object.freeze(Array.from({ length: n }, (_, i) => Object.freeze(Array.from({ length: n }, (_, j) => entry(i, j)))));
}
function symmetric(a, metric, label) {
  need(a.matrixEqual(metric, a.transpose(metric)), label + " must be symmetric");
}
const lowerBlocks = (a, metric, curvature, n) => blocks(n, (i, j) => a.transpose(a.matMultiply(metric, curvature[i][j])));

// Packing a bivector uses only one triangle, so first establish that the
// omitted triangle AND diagonal are determined by the retained one. The
// full equality below includes diagonal equations q=-q, hence q=0 over Q.
// All scans/scratch matrices use admitted GeometryAlgebra operations.
function validateMetricSkew(a, matrix, signs) {
  context(a); const n = a.size(matrix);
  need(Array.isArray(signs) && signs.length === n && signs.every(s => s === 1 || s === -1), "complete explicit signature signs");
  const signature = a.matrix(n, (i, j) => i === j ? a.number(signs[i]) : a.zero);
  const lowered = a.matMultiply(signature, matrix);
  need(a.matrixEqual(lowered, a.matScale(a.transpose(lowered), a.number(-1))), "complete metric-skew connection, including zero diagonal");
  return true;
}

function validateLoweredCurvature(a, lowered) {
  const n = blocksDimension(a, lowered), negative = a.number(-1);
  for (let i = 0; i < n; i++) for (let j = 0; j < n; j++) {
    need(a.matrixEqual(lowered[i][j], a.matScale(lowered[j][i], negative)), "complete first-pair curvature antisymmetry, including zero diagonal blocks");
    need(a.matrixEqual(lowered[i][j], a.matScale(a.transpose(lowered[i][j]), negative)), "complete last-pair curvature antisymmetry, including zero diagonal entries");
  }
  return true;
}

// Input endomorphism block R[a][b] has matrix row=d,column=c, i.e.
// R(a,b)e_c=R^d_c e_d. Output block has row=c,column=d. With the symmetric
// metric used by geometry, this is R_abcd=sum_k R^k_c G_kd.
function lowerCurvature(a, metric, curvature) {
  context(a); const n = a.size(metric); blocksDimension(a, curvature, n);
  symmetric(a, metric, "metric");
  return lowerBlocks(a, metric, curvature, n);
}

// All four entries are COVARIANT slots, with map[old,new]. Independent maps
// make the four differentiated frame insertions explicit. Packing into 2D
// planes lets every contraction loop use budgeted matrix multiplication;
// no sparse-support assumption or curvature symmetry drops any component.
function transformCovariant4(a, input, maps) {
  const n = blocksDimension(a, input); mapsDimension(a, maps, n);
  const firstTranspose = a.transpose(maps[0]), thirdTranspose = a.transpose(maps[2]);
  const planes = blocks(n, (c, d) => {
    const plane = a.matrix(n, (i, j) => a.get(input[i][j], c, d));
    return a.product(firstTranspose, plane, maps[1]);
  });
  return blocks(n, (i, j) => {
    const plane = a.matrix(n, (c, d) => a.get(planes[c][d], i, j));
    return a.product(thirdTranspose, plane, maps[3]);
  });
}

// deltaLower=transpose(G*deltaR+deltaG*R), followed by its four-frame
// transform PLUS four distinct transforms of Lower(R,G), each containing
// one differentiated frame map. In the source-frame convention callers
// supply deltaE=N*E, not N alone. No mixed second-order terms are inserted.
function variationLoweredCurvature(a, metric, deltaMetric, curvature, deltaCurvature, maps, deltaMaps) {
  context(a); const n = a.size(metric);
  need(a.size(deltaMetric) === n, "matching metric variation dimension");
  blocksDimension(a, curvature, n); blocksDimension(a, deltaCurvature, n);
  mapsDimension(a, maps, n); mapsDimension(a, deltaMaps, n);
  symmetric(a, metric, "metric"); symmetric(a, deltaMetric, "metric variation");
  const lowered = lowerBlocks(a, metric, curvature, n);
  const deltaLowered = blocks(n, (i, j) => a.transpose(a.matAdd(
    a.matMultiply(metric, deltaCurvature[i][j]), a.matMultiply(deltaMetric, curvature[i][j]))));
  const terms = [transformCovariant4(a, deltaLowered, maps)];
  for (let slot = 0; slot < 4; slot++) {
    const changed = [...maps]; changed[slot] = deltaMaps[slot];
    terms.push(transformCovariant4(a, lowered, changed));
  }
  return blocks(n, (i, j) => a.matAdd(...terms.map(term => term[i][j])));
}

const sigma = axis => axis < 7 ? 1 : -1;
function spinConnectionCoefficient(a, signatureC, omegaDC) {
  context(a); need(signatureC === 1 || signatureC === -1, "signature sign must be +1 or -1");
  return a.multiply(a.parse(signatureC === 1 ? "-1/2" : "1/2"), omegaDC);
}
function spinCurvatureCoefficient(a, signatureC, signatureD, loweredCD) {
  context(a); need((signatureC === 1 || signatureC === -1) && (signatureD === 1 || signatureD === -1), "signature signs must be +1 or -1");
  return a.multiply(a.parse(signatureC === signatureD ? "-1/2" : "1/2"), loweredCD);
}
function record(a, form, blade, coefficient) {
  return Object.freeze({ form, blade, k0: 0, k1: 0, real: a.text(coefficient), imaginary: "0" });
}
function sortedRecords(records) {
  // Mask order is NOT lexicographic (c,d) pair order. Each loop produces one
  // unique key, and zero filtering follows exact rational cancellation.
  records.sort((x, y) => x.form - y.form || x.blade - y.blade || x.k0 - y.k0 || x.k1 - y.k1);
  return Object.freeze(records);
}

// Complete 14D connection: sum_mu E_mu,a (-sigma_c Omega_mu[d,c]/2)
// theta^a Gamma_cd, c<d. Omega_mu is already the spin-frame endomorphism;
// this routine changes only its differential-form slot through E.
function spinConnectionTensor(a, frame, omega) {
  context(a); need(a.size(frame) === 14, "spin connection requires dimension 14");
  need(Array.isArray(omega) && omega.length === 14, "all 14 spin connection slots required");
  omega.forEach(matrix => need(a.size(matrix) === 14, "spin connection matrix dimension 14"));
  const signs = Array.from({ length: 14 }, (_, axis) => sigma(axis));
  omega.forEach(matrix => validateMetricSkew(a, matrix, signs));
  const records = [];
  for (let axis = 0; axis < 14; axis++) {
    const adapted = a.matAdd(...omega.map((matrix, mu) => a.matScale(matrix, a.get(frame, mu, axis))));
    for (let c = 0; c < 14; c++) for (let d = c + 1; d < 14; d++) {
      const coefficient = spinConnectionCoefficient(a, sigma(c), a.get(adapted, d, c));
      if (!a.isZero(coefficient)) records.push(record(a, 1 << axis, (1 << c) | (1 << d), coefficient));
    }
  }
  return sortedRecords(records);
}

// Complete 14D curvature, with no double-counting of antisymmetric pairs:
// -sigma_c sigma_d low_abcd/2 theta^ab Gamma_cd, a<b and c<d.
// The caller separately validates that supplied coefficients are the bound
// adapted lowered curvature; this function does not infer that provenance.
function spinCurvatureTensor(a, loweredAdapted) {
  blocksDimension(a, loweredAdapted, 14); validateLoweredCurvature(a, loweredAdapted); const records = [];
  for (let i = 0; i < 14; i++) for (let j = i + 1; j < 14; j++)
    for (let c = 0; c < 14; c++) for (let d = c + 1; d < 14; d++) {
      const coefficient = spinCurvatureCoefficient(a, sigma(c), sigma(d), a.get(loweredAdapted[i][j], c, d));
      if (!a.isZero(coefficient)) records.push(record(a, (1 << i) | (1 << j), (1 << c) | (1 << d), coefficient));
    }
  return sortedRecords(records);
}

module.exports = { lowerCurvature, transformCovariant4, variationLoweredCurvature,
  validateMetricSkew, validateLoweredCurvature, spinConnectionCoefficient, spinCurvatureCoefficient, spinConnectionTensor, spinCurvatureTensor };
