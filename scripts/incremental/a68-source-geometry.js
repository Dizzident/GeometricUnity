"use strict";

// UNCALLED source-geometry reconstruction. No retained metric derivative is
// an input. Production evaluation still requires the complete FIRST/replay
// contract and resource proof. This module alone cannot authorize a pilot.
// Native coordinates are H0..3,V4..13; frame-signature indices are different.
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const { lowerCurvature, transformCovariant4, variationLoweredCurvature, spinConnectionTensor, spinCurvatureTensor } = require("./a68-curvature-geometry");
const need = (ok, why) => { if (!ok) throw new Error("A68 source geometry: " + why); };
const PAIRS = Object.freeze([[0, 0], [1, 1], [2, 2], [3, 3], [0, 1], [0, 2], [0, 3], [1, 2], [1, 3], [2, 3]].map(Object.freeze));
const sources = new WeakMap();
const sourceConnections = new WeakMap(), sourceGerms = new WeakMap(), sourceFrames = new WeakMap(), sourceDeltas = new WeakMap();
const baselineBundles = new WeakMap(), germBundles = new WeakMap();
function sourceGeometryIdentity(a, bundle) {
  need(a instanceof GeometryAlgebra, "bounded exact algebra required for source identity");
  if (baselineBundles.get(bundle) === a) return Object.freeze({ kind: "baseline", point: bundle.source.point });
  const baseline = germBundles.get(bundle);
  need(baseline && baselineBundles.get(baseline) === a, "closed complete source geometry bundle");
  return Object.freeze({ kind: "germ", point: baseline.source.point, metricBasis: bundle.germ.metricBasis, jetIndex: bundle.germ.jetIndex, baseline });
}
const list = (n, f) => Object.freeze(Array.from({ length: n }, (_, i) => f(i)));
const table = (n, f) => list(n, i => list(n, j => f(i, j)));
const matrixJet = (Value, D, DD) => Object.freeze({ Value, D, DD });
function validateMetricJet(a, jet, n) {
  need(jet && a.size(jet.Value) === n && Array.isArray(jet.D) && jet.D.length === n && Array.isArray(jet.DD) && jet.DD.length === n, "complete metric jet");
  const symmetric = m => {
    need(a.size(m) === n, "metric jet dimension");
    for (let i = 0; i < n; i++) for (let j = i + 1; j < n; j++) need(a.equal(a.get(m, i, j), a.get(m, j, i)), "symmetric metric jet");
  };
  symmetric(jet.Value); jet.D.forEach(symmetric);
  jet.DD.forEach(row => { need(Array.isArray(row) && row.length === n, "complete metric second jet"); row.forEach(symmetric); });
  for (let i = 0; i < n; i++) for (let j = i + 1; j < n; j++) need(a.matrixEqual(jet.DD[i][j], jet.DD[j][i]), "commuting ordinary metric second derivatives");
}
function multiindices() {
  const result = []; for (let d = 0; d <= 3; d++) for (let a = 0; a <= d; a++) for (let b = 0; b <= d - a; b++) for (let c = 0; c <= d - a - b; c++) result.push(Object.freeze([a, b, c, d - a - b - c]));
  return Object.freeze(result);
}
function symmetricBasis(a) {
  return list(10, k => a.matrix(4, (i, j) => i === PAIRS[k][0] && j === PAIRS[k][1] || i === PAIRS[k][1] && j === PAIRS[k][0] ? a.one : a.zero));
}
function buildSourceMetric(a, point) {
  need(a instanceof GeometryAlgebra && (point === 0 || point === 1), "bounded algebra and fixed point0/1");
  const minus = a.number(-1), half = a.parse("1/2"), yDiagonal = point === 0 ? [-1, 1, 1, 1] : [-1, 4, 9, 16];
  const y = a.matrix(4, (i, j) => i === j ? a.number(yDiagonal[i]) : a.zero), p = a.inverse(y), basis = symmetricBasis(a), zero4 = a.matrix(4);
  const pd = list(14, z => z < 4 ? zero4 : a.matScale(a.product(p, basis[z - 4], p), minus));
  const pdd = table(14, (z, w) => z < 4 || w < 4 ? zero4 : a.matAdd(a.product(p, basis[z - 4], p, basis[w - 4], p), a.product(p, basis[w - 4], p, basis[z - 4], p)));
  // Differentiate the SOURCE bilinear trace formula directly, not the C#
  // cached cyclic trace recurrence or a supplied first/second metric jet.
  const pa = list(10, i => a.matMultiply(p, basis[i]));
  const pda = list(14, z => list(10, i => a.matMultiply(pd[z], basis[i])));
  const pdda = table(14, (z, w) => list(10, i => a.matMultiply(pdd[z][w], basis[i])));
  const bilinear = (x, y) => a.subtract(a.traceProduct(x, y), a.multiply(half, a.multiply(a.trace(x), a.trace(y))));
  const value = a.matrix(14, (i, j) => i < 4 && j < 4 ? a.negate(a.get(y, i, j)) : i >= 4 && j >= 4 ? bilinear(pa[i - 4], pa[j - 4]) : a.zero);
  const d = list(14, z => a.matrix(14, (i, j) => z < 4 ? a.zero : i < 4 && j < 4 ? a.negate(a.get(basis[z - 4], i, j)) : i >= 4 && j >= 4 ? a.add(bilinear(pda[z][i - 4], pa[j - 4]), bilinear(pa[i - 4], pda[z][j - 4])) : a.zero));
  const dd = table(14, (z, w) => a.matrix(14, (i, j) => z < 4 || w < 4 || i < 4 || j < 4 ? a.zero : a.sum(
    bilinear(pdda[z][w][i - 4], pa[j - 4]), bilinear(pa[i - 4], pdda[z][w][j - 4]),
    bilinear(pda[z][i - 4], pda[w][j - 4]), bilinear(pda[w][i - 4], pda[z][j - 4]))));
  const result = Object.freeze({ point, y, inverseY: p, basis, inverseYPartial: pd, inverseYSecond: pdd, metric: matrixJet(value, d, dd) });
  sources.set(result, a); return result;
}
function germMatches(multi, derivatives) {
  if (!derivatives.every(i => Number.isInteger(i) && i >= 0 && i < 4)) return false;
  const counts = [0, 0, 0, 0]; derivatives.forEach(i => counts[i]++); return counts.every((v, i) => v === multi[i]);
}
function buildSourceGermMetric(a, source, metricBasis, jetIndex) {
  need(sources.get(source) === a && Number.isInteger(metricBasis) && metricBasis >= 0 && metricBasis < 10 && Number.isInteger(jetIndex) && jetIndex >= 0 && jetIndex < 35, "source identity and complete germ domain");
  const multi = multiindices()[jetIndex], m = source.basis[metricBasis], half = a.parse("1/2"), minus = a.number(-1);
  // h0 inverse is diag(-1,1,1,1) AT BOTH fibre points. No derivative of
  // h0^-1 contributes: it multiplies the zero baseline downstairs first jet.
  function christoffel(extra) {
    const hDerivative = (i, j, derivatives) => germMatches(multi, [...extra, ...derivatives]) ? a.get(m, i, j) : a.zero;
    return list(4, i => a.matrix(4, (k, j) => a.multiply(k === 0 ? a.negate(half) : half,
      a.subtract(a.add(hDerivative(j, k, [i]), hDerivative(i, k, [j])), hDerivative(i, j, [k])))));
  }
  const c = christoffel([]), cd = list(4, z => christoffel([z])), cdd = table(4, (z, w) => christoffel([z, w]));
  function shear(y, gamma) {
    const columns = gamma.map(g => a.matAdd(a.matMultiply(a.transpose(g), y), a.matMultiply(y, g)));
    return a.matrix(14, (row, column) => row < 4 || column >= 4 ? a.zero : a.get(columns[column], ...PAIRS[row - 4]));
  }
  const n = shear(source.y, c);
  const nd = list(14, z => z < 4 ? shear(source.y, cd[z]) : shear(source.basis[z - 4], c));
  const ndd = table(14, (z, w) => z < 4 && w < 4 ? shear(source.y, cdd[z][w]) : z < 4 ? shear(source.basis[w - 4], cd[z]) : w < 4 ? shear(source.basis[z - 4], cd[w]) : a.matrix(14));
  const shearJet = matrixJet(n, nd, ndd), g = source.metric;
  const sym = (n, g) => a.matScale(a.matAdd(a.matMultiply(a.transpose(n), g), a.matMultiply(g, n)), minus);
  const k = sym(n, g.Value), kd = list(14, z => a.matAdd(sym(nd[z], g.Value), sym(n, g.D[z])));
  const kdd = table(14, (z, w) => a.matAdd(sym(ndd[z][w], g.Value), sym(nd[z], g.D[w]), sym(nd[w], g.D[z]), sym(n, g.DD[z][w])));
  const result = Object.freeze({ point: source.point, metricBasis, jetIndex, multiindex: multi, order: multi.reduce((x, y) => x + y, 0), shear: shearJet, metricVariation: matrixJet(k, kd, kdd) });
  sourceGerms.set(result, source); return result;
}

function buildSourceConnection(a, source) {
  need(sources.get(source) === a, "same-context rebuilt source metric");
  const { y, inverseY: p, basis, inverseYPartial: pd } = source, half = a.parse("1/2"), minusHalf = a.parse("-1/2"), zero = a.matrix(14);
  // Return the complete native output vector Gamma(axis,column), or its
  // ordinary derivative in vertical direction z. No C# Koszul loop is used.
  function column(axis, input, z = null) {
    if (z !== null && z < 4) return Array(14).fill(a.zero);
    const verticalA = axis >= 4, verticalB = input >= 4;
    if (verticalA && verticalB) {
      const inverse = z === null ? p : pd[z];
      const value = a.matScale(a.matAdd(a.product(basis[axis - 4], inverse, basis[input - 4]), a.product(basis[input - 4], inverse, basis[axis - 4])), minusHalf);
      return [...Array(4).fill(a.zero), ...PAIRS.map(([i, j]) => a.get(value, i, j))];
    }
    if (verticalA || verticalB) {
      const index = verticalA ? axis : input, horizontal = verticalA ? input : axis;
      const value = a.matScale(a.matMultiply(z === null ? p : pd[z], basis[index - 4]), half);
      return [...Array.from({ length: 4 }, (_, i) => a.get(value, i, horizontal)), ...Array(10).fill(a.zero)];
    }
    const sym = a.matrix(4, (i, j) => a.multiply(half, a.number(Number(i === axis && j === input) + Number(i === input && j === axis))));
    let value;
    if (z === null) value = a.matAdd(a.product(y, sym, y), a.matScale(y, a.multiply(minusHalf, a.get(y, axis, input))));
    else {
      const yz = basis[z - 4];
      value = a.matAdd(a.product(yz, sym, y), a.product(y, sym, yz), a.matScale(yz, a.multiply(minusHalf, a.get(y, axis, input))), a.matScale(y, a.multiply(minusHalf, a.get(yz, axis, input))));
    }
    value = a.matScale(value, half);
    return [...Array(4).fill(a.zero), ...PAIRS.map(([i, j]) => a.get(value, i, j))];
  }
  const gamma = list(14, mu => { const columns = list(14, input => column(mu, input)); return a.matrix(14, (row, col) => columns[col][row]); });
  const dgamma = table(14, (nu, mu) => {
    if (nu < 4) return zero;
    const columns = list(14, input => column(mu, input, nu)); return a.matrix(14, (row, col) => columns[col][row]);
  });
  const curvature = table(14, (mu, nu) => a.matAdd(dgamma[mu][nu], a.matScale(dgamma[nu][mu], a.number(-1)), a.commutator(gamma[mu], gamma[nu])));
  const result = Object.freeze({ Gamma: gamma, DGamma: dgamma, Curvature: curvature });
  verifyMetricConnection(a, source.metric, result); sourceConnections.set(result, source); return result;
}

// Complete uniqueness identities, not selected component anchors. This
// verifier is also useful on manufactured lower-dimensional test metrics.
function verifyMetricConnection(a, metric, connection) {
  const n = a.size(metric.Value);
  validateMetricJet(a, metric, n); a.inverse(metric.Value); // Nondegeneracy is essential to uniqueness.
  need(metric.D.length === n && metric.DD.length === n && metric.DD.every(row => row.length === n) && connection.Gamma.length === n && connection.DGamma.length === n && connection.DGamma.every(row => row.length === n), "full connection identity menus");
  for (let mu = 0; mu < n; mu++) {
    need(a.matrixEqual(metric.D[mu], a.matAdd(a.matMultiply(a.transpose(connection.Gamma[mu]), metric.Value), a.matMultiply(metric.Value, connection.Gamma[mu]))), "complete metric compatibility");
    for (let row = 0; row < n; row++) for (let col = 0; col < n; col++) need(a.equal(a.get(connection.Gamma[mu], row, col), a.get(connection.Gamma[col], row, mu)), "complete torsion zero");
    for (let nu = 0; nu < n; nu++) {
      const right = a.matAdd(a.matMultiply(a.transpose(connection.DGamma[nu][mu]), metric.Value), a.matMultiply(a.transpose(connection.Gamma[mu]), metric.D[nu]), a.matMultiply(metric.D[nu], connection.Gamma[mu]), a.matMultiply(metric.Value, connection.DGamma[nu][mu]));
      need(a.matrixEqual(metric.DD[nu][mu], right), "complete differentiated metric compatibility");
      for (let row = 0; row < n; row++) for (let col = 0; col < n; col++) need(a.equal(a.get(connection.DGamma[nu][mu], row, col), a.get(connection.DGamma[nu][col], row, mu)), "complete differentiated torsion zero");
    }
  }
  return true;
}

// Independently verify the full linearization of torsion and metric
// compatibility, including ordinary derivatives. The source wrapper has
// already established the baseline identities; these are NOT scalar probes.
function verifyLinearizedMetricConnection(a, metric, connection, changed, delta) {
  const n = a.size(metric.Value); validateMetricJet(a, metric, n); validateMetricJet(a, changed, n); a.inverse(metric.Value);
  for (const candidate of [connection, delta]) {
    need(candidate.Gamma.length === n && candidate.DGamma.length === n && candidate.DGamma.every(row => row.length === n), "complete linearized connection menus");
    candidate.Gamma.forEach(m => need(a.size(m) === n, "linearized connection dimension"));
    candidate.DGamma.forEach(row => row.forEach(m => need(a.size(m) === n, "linearized connection derivative dimension")));
  }
  for (let mu = 0; mu < n; mu++) {
    const gamma = connection.Gamma[mu], c = delta.Gamma[mu], g = metric.Value, k = changed.Value;
    const first = a.matAdd(a.matMultiply(a.transpose(c), g), a.matMultiply(a.transpose(gamma), k), a.matMultiply(k, gamma), a.matMultiply(g, c));
    need(a.matrixEqual(changed.D[mu], first), "complete linearized metric compatibility");
    for (let row = 0; row < n; row++) for (let col = 0; col < n; col++) need(a.equal(a.get(c, row, col), a.get(delta.Gamma[col], row, mu)), "complete linearized torsion zero");
    for (let nu = 0; nu < n; nu++) {
      const dg = connection.DGamma[nu][mu], dc = delta.DGamma[nu][mu], gn = metric.D[nu], kn = changed.D[nu];
      const second = a.matAdd(a.matMultiply(a.transpose(dc), g), a.matMultiply(a.transpose(c), gn),
        a.matMultiply(a.transpose(dg), k), a.matMultiply(a.transpose(gamma), kn),
        a.matMultiply(kn, gamma), a.matMultiply(k, dg), a.matMultiply(gn, c), a.matMultiply(g, dc));
      need(a.matrixEqual(changed.DD[nu][mu], second), "complete differentiated linearized metric compatibility");
      for (let row = 0; row < n; row++) for (let col = 0; col < n; col++) need(a.equal(a.get(dc, row, col), a.get(delta.DGamma[nu][col], row, mu)), "complete differentiated linearized torsion zero");
    }
  }
  return true;
}

function sourceFrame(a, point) {
  need(point === 0 || point === 1, "fixed frame point");
  const rows = [[1, 1, -1, -1], [1, -1, 1, -1], [1, -1, -1, 1]];
  const e0 = a.matrix(14, (i, j) => {
    if (i === 0 && j === 0 || i >= 1 && i < 4 && j === i + 6) return a.one;
    if (i >= 4 && i < 8 && j >= 1 && j <= 3) return a.parse(String((i === 4 ? -1 : 1) * rows[j - 1][i - 4]) + "/2");
    if (i >= 4 && i < 8 && j === 10) return a.parse(i === 4 ? "1/2" : "-1/2");
    for (let k = 0; k < 3; k++) {
      if (i === k + 8 && j === k + 4 || i === k + 11 && j === k + 11) return a.parse("1/4");
      if (i === k + 11 && j === k + 4 || i === k + 8 && j === k + 11) return a.parse("3/4");
    }
    return a.zero;
  });
  const transport = a.matrix(14, (i, j) => i !== j ? a.zero : point === 0 ? a.one : i < 4 ? a.reciprocal(a.number(i + 1)) : a.number((PAIRS[i - 4][0] + 1) * (PAIRS[i - 4][1] + 1)));
  return a.matMultiply(transport, e0);
}
function rho(a, z, basis) {
  const columns = basis.map(b => a.matScale(a.matAdd(a.matMultiply(a.transpose(z), b), a.matMultiply(b, z)), a.number(-1)));
  return a.matrix(14, (i, j) => i < 4 && j < 4 ? a.get(z, i, j) : i >= 4 && j >= 4 ? a.get(columns[j - 4], ...PAIRS[i - 4]) : a.zero);
}
function buildSourceFrameGeometry(a, source, connection) {
  need(sources.get(source) === a && sourceConnections.get(connection) === source, "same-source reconstructed connection");
  const frame = sourceFrame(a, source.point), inverse = a.inverse(frame), identity = a.identity(14), zero4 = a.matrix(4);
  const lift = list(14, mu => rho(a, mu < 4 ? zero4 : a.matScale(a.matMultiply(source.inverseY, source.basis[mu - 4]), a.parse("-1/2")), source.basis));
  const partial = list(14, mu => a.matMultiply(lift[mu], frame)), inversePartial = list(14, mu => a.matScale(a.matMultiply(inverse, lift[mu]), a.number(-1)));
  const omega = list(14, mu => a.product(inverse, a.matAdd(connection.Gamma[mu], lift[mu]), frame));
  const inFrame = list(14, mu => a.product(inverse, connection.Gamma[mu], frame));
  const eta = a.matrix(14, (i, j) => i === j ? a.number(i < 7 ? 1 : -1) : a.zero), zero = a.matrix(14);
  need(a.matrixEqual(a.product(a.transpose(frame), source.metric.Value, frame), eta), "source frame Gram convention");
  need(a.matrixEqual(a.matMultiply(inverse, frame), identity), "complete independent frame inverse");
  for (let mu = 0; mu < 14; mu++) need(a.matrixEqual(a.matAdd(a.matMultiply(a.transpose(omega[mu]), eta), a.matMultiply(eta, omega[mu])), zero), "complete frame connection metric compatibility");
  const result = Object.freeze({ Frame: frame, InverseFrame: inverse, Identity: identity, FrameLift: lift, FramePartial: partial, InversePartial: inversePartial, Omega: omega, ConnectionInFrame: inFrame });
  sourceFrames.set(result, Object.freeze({ source, connection })); return result;
}
function buildSourceGermFrame(a, source, connection, frame, germ, delta) {
  const binding = sourceFrames.get(frame);
  need(sources.get(source) === a && sourceConnections.get(connection) === source && binding?.source === source && binding?.connection === connection && sourceGerms.get(germ) === source && sourceDeltas.get(delta) === germ, "closed same-source germ/frame/connection lineage");
  const n = germ.shear, e = frame.Frame, ei = frame.InverseFrame, conjugate = x => a.product(ei, x, e);
  const motion = conjugate(n.Value), deltaFrame = a.matMultiply(n.Value, e);
  const partial = list(14, mu => conjugate(a.matAdd(n.D[mu], a.commutator(n.Value, frame.FrameLift[mu]))));
  const covariantCoordinate = list(14, mu => conjugate(a.matAdd(n.D[mu], a.commutator(connection.Gamma[mu], n.Value))));
  const covariant = list(14, r => a.matAdd(...Array.from({ length: 14 }, (_, mu) => a.matScale(covariantCoordinate[mu], a.get(e, mu, r)))));
  const z = list(14, mu => a.matAdd(delta.Gamma[mu], n.D[mu], a.commutator(connection.Gamma[mu], n.Value)));
  const omega = list(14, mu => conjugate(z[mu]));
  const omegaPartial = table(14, (nu, mu) => conjugate(a.matAdd(delta.DGamma[nu][mu], n.DD[nu][mu], a.commutator(connection.DGamma[nu][mu], n.Value), a.commutator(connection.Gamma[mu], n.D[nu]), a.commutator(z[mu], frame.FrameLift[nu]))));
  return Object.freeze({ Motion: motion, DeltaFrame: deltaFrame, MotionPartial: partial, MotionCovariant: covariant, DeltaOmega: omega, DeltaOmegaPartial: omegaPartial });
}

function buildSourceBaselineGeometry(a, point) {
  const source = buildSourceMetric(a, point), connection = buildSourceConnection(a, source), frame = buildSourceFrameGeometry(a, source, connection);
  const e = frame.Frame, loweredCurvature = transformCovariant4(a, lowerCurvature(a, source.metric.Value, connection.Curvature), [e, e, e, e]);
  const spinReference = spinConnectionTensor(a, e, frame.Omega), spinCurvature = spinCurvatureTensor(a, loweredCurvature);
  const result = Object.freeze({ source, connection, frame, loweredCurvature, spinReference, spinCurvature });
  baselineBundles.set(result, a); return result;
}
function buildSourceGermGeometry(a, baseline, metricBasis, jetIndex) {
  need(baselineBundles.get(baseline) === a, "closed source baseline bundle lineage");
  const { source, connection, frame } = baseline, binding = sourceFrames.get(frame);
  need(sources.get(source) === a && sourceConnections.get(connection) === source && binding?.source === source && binding?.connection === connection, "closed reconstructed baseline lineage");
  const germ = buildSourceGermMetric(a, source, metricBasis, jetIndex);
  const delta = connectionVariation(a, source.metric, connection, germ.metricVariation);
  verifyLinearizedMetricConnection(a, source.metric, connection, germ.metricVariation, delta); sourceDeltas.set(delta, germ);
  const matrices = buildSourceGermFrame(a, source, connection, frame, germ, delta);
  const e = frame.Frame, de = matrices.DeltaFrame;
  const loweredVariation = variationLoweredCurvature(a, source.metric.Value, germ.metricVariation.Value,
    connection.Curvature, delta.Curvature, [e, e, e, e], [de, de, de, de]);
  const spinConnectionVariation = spinConnectionTensor(a, e, matrices.DeltaOmega);
  // This is ADAPTED deltaF. Fixed-coordinate deltaF additionally subtracts
  // T2(Motion)F in the independent tensor expression recipe, not here.
  const spinCurvatureVariation = spinCurvatureTensor(a, loweredVariation);
  const inverse = a.inverse(source.metric.Value);
  const inverseVariation = a.matScale(a.product(inverse, germ.metricVariation.Value, inverse), a.number(-1));
  const result = Object.freeze({ germ, inverseVariation, connectionVariation: delta, matrices, loweredVariation, spinConnectionVariation, spinCurvatureVariation });
  germBundles.set(result, baseline); return result;
}

// General covariant metric-variation identity, independent of both existing
// C# dual-Koszul and linearized inverse implementations. Also supports small
// manufactured geometries for tests; the scientific driver must supply14.
function connectionVariation(a, metric, connection, changed) {
  need(a instanceof GeometryAlgebra, "bounded exact algebra"); const n = a.size(metric.Value), half = a.parse("1/2");
  validateMetricJet(a, metric, n); validateMetricJet(a, changed, n);
  need(Array.isArray(connection.Gamma) && connection.Gamma.length === n && Array.isArray(connection.DGamma) && connection.DGamma.length === n, "complete connection jet");
  connection.Gamma.forEach(m => need(a.size(m) === n, "connection dimension")); connection.DGamma.forEach(row => { need(Array.isArray(row) && row.length === n, "connection derivative slots"); row.forEach(m => need(a.size(m) === n, "connection derivative dimension")); });
  const minus = a.number(-1), inverse = a.inverse(metric.Value), inverseD = list(n, z => a.matScale(a.product(inverse, metric.D[z], inverse), minus));
  const q = list(n, mu => a.matAdd(changed.D[mu], a.matScale(a.matAdd(a.matMultiply(a.transpose(connection.Gamma[mu]), changed.Value), a.matMultiply(changed.Value, connection.Gamma[mu])), minus)));
  // DQ[nu][mu] is an ORDINARY derivative. No extra lower derivative-slot
  // connection term is subtracted here; all components use native coordinates.
  const dq = table(n, (nu, mu) => a.matAdd(changed.DD[nu][mu], a.matScale(a.matAdd(
    a.matMultiply(a.transpose(connection.DGamma[nu][mu]), changed.Value),
    a.matMultiply(a.transpose(connection.Gamma[mu]), changed.D[nu]),
    a.matMultiply(changed.D[nu], connection.Gamma[mu]),
    a.matMultiply(changed.Value, connection.DGamma[nu][mu])), minus)));
  const koszul = (q, mu, b, l) => a.subtract(a.add(a.get(q[mu], b, l), a.get(q[b], mu, l)), a.get(q[l], mu, b));
  // The Koszul numerator does not depend on the output row. Materialize it
  // ONCE per (mu,l,column), rather than rebuilding it inside every inverse
  // contraction. Matrix multiplication still admits all n^3 slot visits and
  // returns every coordinate; exact zero factors merely avoid BigInt work.
  // This is a fixed algebraic rewrite, not support/grade/fixture selection.
  const numerator = list(n, mu => a.matrix(n, (l, column) => koszul(q, mu, column, l)));
  const c = list(n, mu => a.matScale(a.matMultiply(inverse, numerator[mu]), half));
  const dc = table(n, (nu, mu) => {
    const differentiatedNumerator = a.matrix(n, (l, column) => koszul(dq[nu], mu, column, l));
    return a.matScale(a.matAdd(a.matMultiply(inverseD[nu], numerator[mu]),
      a.matMultiply(inverse, differentiatedNumerator)), half);
  });
  const curvature = table(n, (mu, nu) => a.matAdd(dc[mu][nu], a.matScale(dc[nu][mu], minus), a.commutator(connection.Gamma[mu], c[nu]), a.matScale(a.commutator(connection.Gamma[nu], c[mu]), minus)));
  return Object.freeze({ Gamma: c, DGamma: dc, Curvature: curvature });
}
module.exports = { PAIRS, multiindices, germMatches, symmetricBasis, buildSourceMetric, buildSourceGermMetric, buildSourceConnection, verifyMetricConnection, verifyLinearizedMetricConnection, sourceFrame, buildSourceFrameGeometry, buildSourceGermFrame, buildSourceBaselineGeometry, buildSourceGermGeometry, sourceGeometryIdentity, connectionVariation };
