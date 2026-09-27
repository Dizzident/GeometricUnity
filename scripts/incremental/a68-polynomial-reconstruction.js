"use strict";

// Complete exact local one-form polynomial assembly, not a stationary-root
// solver, new recurrence proof or physical prediction. Generic manufactured
// tests may call reconstructPolynomial; the source-branded wrapper remains
// UNCALLED on the GU inputs until the complete frozen scientific gate.
const crypto = require("node:crypto");
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const { retainedStationaryIdentity, EPSILON } = require("./a68-retained-inputs");
const need = (ok, why) => { if (!ok) throw new Error("A68 polynomial reconstruction: " + why); };
const integer = (x, lo, hi) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= lo && x <= hi;
const shape = (x, names) => {
  need(x && typeof x === "object" && !Array.isArray(x) && Reflect.ownKeys(x).length === names.length && names.every((k, i) => Reflect.ownKeys(x)[i] === k), "closed ordered data object");
  names.forEach(k => need(Object.hasOwn(Object.getOwnPropertyDescriptor(x, k), "value"), "own data fields"));
};
const degree = mask => { let n = 0; while (mask) { n += mask & 1; mask >>>= 1; } return n; };
const proofIdentities = new WeakMap();

function reconstructPolynomial(coefficients, lambda, expected, arithmeticLimits, limits) {
  const names = ["inputRecords", "outputRecords", "recordVisits", "readCharacters", "outputCharacters", "rationalCharacters"];
  shape(limits, names); need(names.every(k => integer(limits[k], 1, Number.MAX_SAFE_INTEGER)) && limits.rationalCharacters <= 16384, "explicit finite reconstruction limits");
  const caps = Object.freeze({ ...limits }), used = Object.fromEntries(names.map(k => [k, 0]));
  const charge = (kind, count = 1) => { need(integer(count, 0, Number.MAX_SAFE_INTEGER) && count <= caps[kind] - used[kind], "pre-operation " + kind + " ceiling"); used[kind] += count; };
  const a = new GeometryAlgebra(arithmeticLimits), maxBits = arithmeticLimits.maxBits;
  function array(value, count) {
    need(Array.isArray(value) && value.length === count && Reflect.ownKeys(value).length === count + 1, "complete dense array");
    for (let i = 0; i < count; i++) need(Object.hasOwn(Object.getOwnPropertyDescriptor(value, String(i)) ?? {}, "value"), "array own data indices");
  }
  array(coefficients, 5);
  const read = text => {
    need(typeof text === "string" && text.length > 0 && text.length <= caps.rationalCharacters, "bounded rational string"); charge("readCharacters", text.length); return a.parse(text);
  };
  const write = q => { charge("outputCharacters", 2 * maxBits + 2); const text = a.text(q); need(text.length <= caps.rationalCharacters, "output rational height"); return text; };
  function rows(value, visit) {
    need(Array.isArray(value), "tensor array"); const count = value.length;
    need(integer(count, 0, caps.inputRecords), "bounded input tensor length"); charge("inputRecords", count); charge("recordVisits", count); array(value, count);
    let previous = -1;
    for (let i = 0; i < count; i++) {
      const row = value[i]; shape(row, ["form", "blade", "k0", "k1", "real", "imaginary"]);
      need(integer(row.form, 1, 16383) && degree(row.form) === 1 && integer(row.blade, 0, 16383) && row.k0 === 0 && row.k1 === 0 && !Object.is(row.k0, -0) && !Object.is(row.k1, -0), "complete local one-form domain");
      const key = row.form * 16384 + row.blade; need(key > previous, "strict unique tensor order"); previous = key;
      const real = read(row.real), imaginary = read(row.imaginary);
      need(!a.isZero(real) || !a.isZero(imaginary), "sparse zero omitted");
      const grade = degree(row.blade), antiSign = (grade * (grade + 1) / 2) % 2 === 1;
      need(a.isZero(antiSign ? imaginary : real), "full H-anti real Clifford domain");
      visit({ key, form: row.form, blade: row.blade, real, imaginary });
    }
  }
  const l = read(lambda), values = new Map(); let power = a.one, peakRecords = 0;
  for (let n = 0; n < 5; n++) {
    power = a.multiply(power, l);
    rows(coefficients[n], row => {
      const scaledReal = a.multiply(power, row.real), scaledImaginary = a.multiply(power, row.imaginary), old = values.get(row.key);
      const real = old ? a.add(old.real, scaledReal) : scaledReal, imaginary = old ? a.add(old.imaginary, scaledImaginary) : scaledImaginary;
      if (a.isZero(real) && a.isZero(imaginary)) { values.delete(row.key); return; }
      if (!old) need(values.size < caps.outputRecords, "prospective live polynomial support ceiling");
      values.set(row.key, { form: row.form, blade: row.blade, real, imaginary }); peakRecords = Math.max(peakRecords, values.size);
    });
  }
  let compared = 0;
  rows(expected, row => {
    const actual = values.get(row.key);
    need(actual && a.equal(actual.real, row.real) && a.equal(actual.imaginary, row.imaginary), "complete expected polynomial coefficient mismatch"); compared++;
  });
  need(compared === values.size, "complete polynomial support equality including absent coefficients");
  charge("recordVisits", values.size); charge("outputRecords", values.size);
  let norm = a.zero;
  const tensor = Object.freeze([...values.entries()].sort((x, y) => x[0] - y[0]).map(([, row]) => {
    norm = a.sum(norm, row.real[0] < 0n ? a.negate(row.real) : row.real, row.imaginary[0] < 0n ? a.negate(row.imaginary) : row.imaginary);
    return Object.freeze({ form: row.form, blade: row.blade, k0: 0, k1: 0, real: write(row.real), imaginary: write(row.imaginary) });
  }));
  return Object.freeze({ tensor, norm: write(norm), peakRecords, usage: Object.freeze(used), arithmetic: a.snapshot(),
    scope: Object.freeze({ allFiveOrdersReconstructed: true, expectedTensorCompletelyCompared: true,
      fullRealCliffordDomainChecked: true, upstreamRecurrenceReplayEstablished: false, exactStationaryRoot: false,
      sortingAndMetadataMemoryProved: false, totalProcessMemoryProved: false, physicalMassPrediction: false }) });
}

function reconstructRetainedPolynomial(polynomial, certificate, arithmeticLimits, limits) {
  const p = retainedStationaryIdentity(polynomial), c = retainedStationaryIdentity(certificate);
  need(p.kind === "polynomial" && c.kind === "certificate" && p.point === c.point && certificate.point === p.point, "same-point source-bound polynomial/certificate");
  need(certificate.gamma === 1 && certificate.kappa === 907712 && certificate.lambda === "1/907712" && certificate.certificate.certifiedError === EPSILON, "fixed conditional branch parameters");
  const fields = ["S1", "S2", "S3", "S4", "S5"];
  const result = reconstructPolynomial(fields.map(field => polynomial[field].tensor), certificate.lambda, polynomial.X.tensor, arithmeticLimits, limits);
  need(result.norm === certificate.certificate.candidateNorm, "independent candidate L1 norm agrees with certificate");
  // Fingerprint is of the newly reconstructed canonical tensor, not the input
  // file container or a serializable untrusted leaf object.
  const tensorSha256 = crypto.createHash("sha256").update(JSON.stringify(result.tensor)).digest("hex");
  const proof = Object.freeze({ point: p.point, gamma: 1, kappa: 907712, lambda: certificate.lambda, epsilon: EPSILON,
    tensor: result.tensor, tensorSha256, norm: result.norm, reconstruction: result,
    bindings: Object.freeze([p.source, ...[...fields, "X"].map(field => polynomial[field].source), c.source]),
    scientificExecutionAuthorized: false });
  proofIdentities.set(proof, Object.freeze({ point: p.point, polynomial, certificate })); return proof;
}
function reconstructedPolynomialIdentity(value) {
  const identity = proofIdentities.get(value); need(identity, "complete reconstructed polynomial identity"); return identity;
}
module.exports = { reconstructPolynomial, reconstructRetainedPolynomial, reconstructedPolynomialIdentity };
