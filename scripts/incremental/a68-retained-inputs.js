"use strict";

// Read-only provenance boundary for the prospective audit. This does NOT
// reconstruct geometry, prove a stationary branch, or authorize execution.
// Full upstream integrity/replay and the frozen Phase627 binding contract
// remain prerequisites. In particular no Phase621 action-field response is
// exported: those responses used a different native-field first jet.
const fs = require("node:fs");
const path = require("node:path");
const crypto = require("node:crypto");
const need = (ok, message) => { if (!ok) throw new Error("A68 retained input: " + message); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const freeze = value => { if (value && typeof value === "object") { Object.values(value).forEach(freeze); Object.freeze(value); } return value; };
const geometryIdentities = new WeakMap();
const stationaryIdentities = new WeakMap();
function retainedGeometryIdentity(value) {
  const identity = geometryIdentities.get(value); need(identity, "pinned geometry object identity required"); return identity;
}
function retainedStationaryIdentity(value) {
  const identity = stationaryIdentities.get(value); need(identity, "pinned stationary object identity required"); return identity;
}
const root621 = "studies/phase621_induced_metric_full_variation_scope_audit_001";
const root626 = "studies/phase626_fixed_cubic_full_stationary_residual_certificate_001";
const PINS = freeze({
  geometry: { path: root621 + "/output/induced_metric_full_variation_scope_audit_summary.json", bytes: 325198, sha256: "18eda4a2b62af3d450982c0181cf513d9f87d7acf0e8a4a48e313c310b3674f9" },
  stationary: { path: root626 + "/output/fixed_cubic_full_stationary_residual_certificate_summary.json", bytes: 1543546, sha256: "b19b56392ce3cf7880aacecc7bd85ea75fc189d326ee0ca8a5cd87958ad95d09" }
});
const EPSILON = "187031043155169336808079187600453282921/10404029315299191495351705759193144686118652855489848424722539216896";
const safeRelative = file => typeof file === "string" && /^[\x20-\x7e]+$/.test(file) && !file.includes("\\") && !path.posix.isAbsolute(file) && file.split("/").every(part => part && part !== "." && part !== "..");
function readPinnedJson(repoRoot, pin, byteCeiling) {
  need(safeRelative(pin.path) && /^[0-9a-f]{64}$/.test(pin.sha256), "safe source path/hash");
  need(Number.isSafeInteger(byteCeiling) && byteCeiling > 0 && Number.isSafeInteger(pin.bytes) && pin.bytes > 0 && pin.bytes <= byteCeiling, "prospective source byte ceiling");
  const root = fs.realpathSync(repoRoot), file = path.join(root, pin.path);
  // Reject parent as well as final symlinks at inspection. Hashing the exact
  // read buffer remains decisive against later replacement races. The final
  // frozen driver must additionally control its root and input mutation.
  let current = root;
  for (const [i, part] of pin.path.split("/").entries()) {
    current = path.join(current, part); const stat = fs.lstatSync(current);
    need(!stat.isSymbolicLink() && (i === pin.path.split("/").length - 1 ? stat.isFile() : stat.isDirectory()), "regular source path components");
  }
  const fd = fs.openSync(file, fs.constants.O_RDONLY | fs.constants.O_NOFOLLOW | fs.constants.O_NONBLOCK);
  try {
    const stat = fs.fstatSync(fd); need(stat.isFile() && stat.size === pin.bytes, "source size before allocation");
    const bytes = Buffer.alloc(pin.bytes); let offset = 0;
    while (offset < bytes.length) { const read = fs.readSync(fd, bytes, offset, bytes.length - offset, null); need(read > 0, "source shortened during read"); offset += read; }
    need(fs.readSync(fd, Buffer.alloc(1), 0, 1, null) === 0, "source grew during read");
    need(hash(bytes) === pin.sha256, "exact parsed source buffer hash");
    // Immutable upstream hashes refer to already reviewed JSON. They—not
    // generic JSON.parse—exclude altered duplicate-key/encoding documents.
    return JSON.parse(bytes.toString("utf8"));
  } finally { fs.closeSync(fd); }
}
function multiindices() {
  const result = []; for (let d = 0; d <= 3; d++) for (let a = 0; a <= d; a++) for (let b = 0; b <= d - a; b++) for (let c = 0; c <= d - a - b; c++) result.push([a, b, c, d - a - b - c]); return result;
}
function canonicalRational(value) {
  if (typeof value !== "string" || value.length > 512 || !/^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(value) || value === "-0") return false;
  const parts = value.split("/"), numerator = BigInt(parts[0]), denominator = parts.length === 2 ? BigInt(parts[1]) : 1n;
  let a = numerator < 0n ? -numerator : numerator, b = denominator; while (b) [a, b] = [b, a % b];
  return a === 1n && (parts.length === 1 || denominator !== 1n);
}
function tensor(value, degree) {
  need(Array.isArray(value), "tensor array"); let previous = -1;
  for (const entry of value) {
    need(same(Object.keys(entry), ["form", "blade", "k0", "k1", "real", "imaginary"]), "closed upstream tensor record");
    need(Number.isInteger(entry.form) && entry.form >= 0 && entry.form < 16384 && Number.isInteger(entry.blade) && entry.blade >= 0 && entry.blade < 16384 && entry.k0 === 0 && entry.k1 === 0, "local tensor coordinate domain");
    let form = entry.form, count = 0; while (form) { count += form & 1; form >>>= 1; }
    const key = entry.form * 16384 + entry.blade;
    need(count === degree && key > previous && canonicalRational(entry.real) && canonicalRational(entry.imaginary) && (entry.real !== "0" || entry.imaginary !== "0"), "canonical typed ordered nonzero tensor"); previous = key;
  }
  return value;
}
function authority(summary, phase) {
  need(summary.phase === phase && summary.schemaVersion === 1 && summary.auditPassed === true && summary.contractValid === true && summary.exactBindingsValid === true && summary.coreSourceTreeValid === true && summary.deterministic === true, "upstream audit identity/status");
  need(summary.externalReviewPending === true && summary.promotedPhysicalMassClaimCount === 0 && Object.keys(summary.authorityFirewalls).length === 14 && Object.values(summary.authorityFirewalls).every(value => value === false), "upstream authority boundary");
}
class RetainedMixedInputs {
  #root; #geometry; #stationary; #geometryPins; #chunks;
  constructor(repoRoot) {
    this.#root = fs.realpathSync(repoRoot);
    this.#geometry = readPinnedJson(this.#root, PINS.geometry, PINS.geometry.bytes); authority(this.#geometry, 621);
    this.#stationary = readPinnedJson(this.#root, PINS.stationary, PINS.stationary.bytes); authority(this.#stationary, 626);
    this.#geometryPins = this.#geometry.evidence.rows; need(this.#geometryPins.length === 700, "complete geometry pin census");
    for (let index = 0; index < 700; index++) {
      const point = Math.floor(index / 350), metric = Math.floor(index % 350 / 35), jet = index % 35, pin = this.#geometryPins[index];
      need(pin.point === point && pin.metricBasis === metric && pin.jetIndex === jet && pin.path === `${root621}/output/shards/jet_p${point}_m${String(metric).padStart(2, "0")}_j${String(jet).padStart(2, "0")}.json`, "fixed complete geometry pin order/path");
    }
    const chunks = this.#stationary.evidence.shards; need(chunks.length === 3914 && new Set(chunks.map(x => x.path)).size === 3914, "complete stationary chunk manifest");
    this.#chunks = new Map(chunks.map(pin => [pin.path, pin]));
    need(this.#stationary.evidence.contexts.length === 2, "two stationary certificate contexts");
  }
  baseline(point) {
    need(point === 0 || point === 1, "baseline point"); const p = this.#geometry.evidence.pointRows[point]; need(p.point === point, "baseline identity");
    // Deliberately omit old fields and baselineForecasts from the API.
    const result = freeze({ point, h0: p.h0, fibreMetricY: p.fibreMetricY, frame: p.frame, spinReference: tensor(p.spinReference, 1), spinCurvature: tensor(p.spinCurvature, 2), source: PINS.geometry });
    geometryIdentities.set(result, freeze({ kind: "baseline", point, source: PINS.geometry })); return result;
  }
  geometry(point, metric, jet) {
    need(Number.isInteger(point) && point >= 0 && point < 2 && Number.isInteger(metric) && metric >= 0 && metric < 10 && Number.isInteger(jet) && jet >= 0 && jet < 35, "geometry indices");
    const manifest = this.#geometryPins[point * 350 + metric * 35 + jet];
    const pin = { path: manifest.path, bytes: manifest.byteCount, sha256: manifest.sha256 };
    const shard = readPinnedJson(this.#root, pin, 145452), q = shard.evidence, multi = multiindices()[jet];
    need(shard.schemaVersion === 1 && shard.phase === 621 && shard.evidenceKind === "full-induced-metric-jet-v1" && shard.point === point && shard.metricBasis === metric && shard.jetIndex === jet && q.baselinePointId === point && same(q.multiindex, multi) && q.order === multi.reduce((a, b) => a + b, 0), "bound geometry shard identity");
    const result = { point, metricBasis: metric, jetIndex: jet, multiindex: multi, order: q.order, source: pin };
    for (const field of ["frameMotion", "shearVariation", "metricVariation", "inverseVariation", "metricFirstJets", "metricSecondJets", "connectionVariation", "connectionDerivative", "curvatureVariation"]) result[field] = q[field];
    result.spinConnectionVariation = tensor(q.spinConnectionVariation, 1); result.spinCurvatureVariation = tensor(q.spinCurvatureVariation, 2);
    // fieldRows are intentionally inaccessible. Geometric shape/arithmetic
    // equality with independently reconstructed jets remains the next layer.
    freeze(result); geometryIdentities.set(result, freeze({ kind: "germ", point, metricBasis: metric, jetIndex: jet, source: pin })); return result;
  }
  polynomialInputs(point) {
    need(point === 0 || point === 1, "polynomial point"); const result = {};
    for (const field of ["S1", "S2", "S3", "S4", "S5", "X"]) {
      const collection = `p${point}_kinetic_${field}_inputs`, file = `${root626}/output/chunks/${collection}_g000.json`, pin = this.#chunks.get(file);
      need(pin && pin.degree === 1 && pin.collection === collection && same(pin.forms, Array.from({ length: 14 }, (_, a) => 1 << a)), "complete polynomial input chunk pin");
      const shard = readPinnedJson(this.#root, pin, 1097699);
      need(shard.schemaVersion === 1 && shard.phase === 626 && shard.encoding === "expanded-rational-tensor-chunks-v1" && shard.collection === collection && shard.degree === 1 && same(shard.forms, pin.forms) && shard.metadata.point === point && shard.metadata.field === field, "polynomial chunk identity");
      result[field] = { tensor: tensor(shard.tensors.input, 1), source: { path: file, bytes: pin.bytes, sha256: pin.sha256, tensor: "input" } };
    }
    // No polynomial evaluation occurs here. Before use, independently sum
    // the source-bound S_n*(1/907712)^n and compare the COMPLETE X tensor.
    freeze(result); stationaryIdentities.set(result, freeze({ kind: "polynomial", point, source: PINS.stationary })); return result;
  }
  certificate(point) {
    need(point === 0 || point === 1, "certificate point"); const pin = this.#stationary.evidence.contexts[point];
    need(pin.point === point && pin.path === `${root626}/output/point${point}_context.json`, "certificate path");
    const row = readPinnedJson(this.#root, pin, 19274);
    need(row.schemaVersion === 1 && row.phase === 626 && row.point === point && row.gamma === 1 && row.kappa === 907712 && row.lambda === "1/907712" && row.certificate.certifiedError === EPSILON, "conditional polynomial/certificate identity");
    const result = freeze({ point, gamma: row.gamma, kappa: row.kappa, lambda: row.lambda, certificate: row.certificate, source: { path: pin.path, bytes: pin.bytes, sha256: pin.sha256 } });
    stationaryIdentities.set(result, freeze({ kind: "certificate", point, source: result.source })); return result;
  }
}
module.exports = { RetainedMixedInputs, retainedGeometryIdentity, retainedStationaryIdentity, readPinnedJson, PINS, EPSILON };
