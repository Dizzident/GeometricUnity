"use strict";

// Prospective ANALYTIC metadata only: no Fourier/Clifford tensors or scientific
// coefficients. Bounds are conservative all-grade carrier envelopes, never
// observed support or inherited Phase626 ceilings. This is NOT an RSS proof.
// S bounds every partial accumulation's support in the full typed carrier;
// D is a common MULTIPLE of reduced coefficient denominators; M bounds the
// sum of absolute real/imaginary coefficients. Thus |numerator|<=ceil(M*D),
// and printed heights include denominator, slash and a possible minus sign.
// Pair/slot visits are logical counts, not uniform CPU instructions. Matrix,
// Clifford-bit, sorting, serializer, scalar/GCD and caller-copy costs remain
// separate obligations; callbacks may not retain unbudgeted tensors.
const { PLAN_SCHEMA } = require("./a68-mixed-recipe");
const need = (condition, message) => { if (!condition) throw new Error("A68 majorants: " + message); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const shape = (x, keys) => need(x !== null && typeof x === "object" && !Array.isArray(x) && same(Object.keys(x), keys), "closed ordered descriptor");
const integer = (x, lo, hi) => Number.isSafeInteger(x) && x >= lo && x <= hi;
const choose = (n, k) => { if (k < 0 || k > n) return 0n; let value = 1n; for (let i = 1; i <= k; i++) value = value * BigInt(n - k + i) / BigInt(i); return value; };
const ambient = d => d < 0 ? 0n : 16384n * choose(14, d);
const minimum = (...xs) => xs.reduce((a, b) => a < b ? a : b);
const maximum = (...xs) => xs.reduce((a, b) => a > b ? a : b);
const degree = mask => { let d = 0; while (mask) { d += mask & 1; mask >>>= 1; } return d; };
const gcd = (a, b) => { a = a < 0n ? -a : a; while (b) [a, b] = [b, a % b]; return a; };

function deriveMixedResourceMajorants(plan, leafBounds, validateLeafBound, { maxBits = 262144 } = {}) {
  need(integer(maxBits, 64, 1048576), "finite analytic BigInt bit ceiling");
  const bits = value => (value < 0n ? -value : value).toString(2).length;
  const check = value => { need(bits(value) <= maxBits, "analytic bound exceeded maxBits; no smaller bound substituted"); return value; };
  const mul = (a, b) => { if (a === 0n || b === 0n) return 0n; if (a === 1n) return check(b); if (b === 1n) return check(a); need(bits(a) + bits(b) <= maxBits, "pre-product analytic maxBits guard"); return check(a * b); };
  const add = (a, b) => { need(maximum(BigInt(bits(a)), BigInt(bits(b))) + 1n <= BigInt(maxBits), "pre-addition analytic maxBits guard"); return check(a + b); };
  const pow = (a, n) => { let r = 1n; for (let i = 0; i < n; i++) r = mul(r, a); return r; };
  const lcm = (a, b) => mul(a / gcd(a, b), b);
  const natural = (text, positive = false) => { need(typeof text === "string" && text.length <= Math.ceil(maxBits / 3) && /^(0|[1-9][0-9]*)$/.test(text), "bounded canonical natural string"); const x = check(BigInt(text)); need(!positive || x > 0n, "positive denominator"); return x; };
  const rational = text => {
    need(typeof text === "string" && text.length <= Math.ceil(maxBits / 3) && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded canonical rational string");
    const [ns, ds] = text.split("/"), n = check(BigInt(ns)), d = ds === undefined ? 1n : check(BigInt(ds));
    need(text !== "-0" && gcd(n, d) === 1n && (ds === undefined || d !== 1n), "reduced rational"); return { n, d };
  };
  const rat = (n, d) => { const g = gcd(n, d); return { n: check(n / g), d: check(d / g) }; };
  const text = x => x.n.toString() + (x.d === 1n ? "" : "/" + x.d.toString());
  const absolute = x => ({ n: x.n < 0n ? -x.n : x.n, d: x.d });
  const plus = (a, b) => { const d = lcm(a.d, b.d); return rat(add(mul(a.n, d / a.d), mul(b.n, d / b.d)), d); };
  const times = (a, b) => { const g = gcd(a.n, b.d), h = gcd(b.n, a.d); return rat(mul(a.n / g, b.n / h), mul(a.d / h, b.d / g)); };
  const power = (a, n) => ({ n: pow(a.n, n), d: pow(a.d, n) });
  const zero = { n: 0n, d: 1n }, one = { n: 1n, d: 1n }, two = { n: 2n, d: 1n };
  const normalized = (S, D, M, d) => d < 0 || S === 0n || M.n === 0n ? { S: 0n, D: 1n, M: zero } : { S: minimum(S, ambient(d)), D, M };
  const printedBound = b => {
    if (b.S === 0n || b.M.n === 0n) return 1n;
    const product = mul(b.M.n, b.D), numerator = product / b.M.d + (product % b.M.d === 0n ? 0n : 1n);
    return BigInt(numerator.toString().length) + (b.D === 1n ? 1n : BigInt(b.D.toString().length) + 2n);
  };
  const matrix = parameters => {
    shape(parameters, ["matrix"]); need(Array.isArray(parameters.matrix) && parameters.matrix.length <= 196, "matrix shape");
    let D = 1n, M = zero, lower = zero, lowerSupport = 0n, previous = -1;
    const rows = Array.from({ length: 14 }, () => zero), rowSupport = Array(14).fill(0n);
    for (const e of parameters.matrix) {
      shape(e, ["row", "column", "value"]); need(integer(e.row, 0, 13) && integer(e.column, 0, 13) && e.row * 14 + e.column > previous, "ordered matrix indices"); previous = e.row * 14 + e.column;
      const value = rational(e.value); need(value.n !== 0n, "sparse matrix omits explicit zeros"); const a = absolute(value);
      D = lcm(D, value.d); M = plus(M, a); rows[e.row] = plus(rows[e.row], a); rowSupport[e.row]++;
      if (e.row > e.column) { lower = plus(lower, a); lowerSupport++; }
    }
    const rowNorm = rows.reduce((a, b) => mul(a.n, b.d) >= mul(b.n, a.d) ? a : b, zero);
    return { D, M, lower, lowerSupport, rowNorm, rowSupport: maximum(...rowSupport) };
  };
  shape(plan, ["schemaVersion", "leaves", "nodes", "marks"]);
  need(plan.schemaVersion === PLAN_SCHEMA && Array.isArray(plan.leaves) && Array.isArray(plan.nodes) && Array.isArray(plan.marks), "independent symbolic plan required");
  need(Array.isArray(leafBounds) && leafBounds.length === plan.leaves.length && typeof validateLeafBound === "function", "independent source-bound leaf majorants required");
  const leaves = new Map();
  for (let i = 0; i < leafBounds.length; i++) {
    const b = leafBounds[i], source = plan.leaves[i]; shape(b, ["id", "source", "sha256", "support", "denominator", "norm"]);
    shape(source, ["id", "degree", "source", "sha256"]);
    need(integer(source.degree, 0, 14) && b.id === source.id && b.source === source.source && b.sha256 === source.sha256 && !leaves.has(b.id), "exact leaf source order and identity");
    const snapshot = Object.freeze({ ...b }), declared = Object.freeze({ ...source });
    need(validateLeafBound(snapshot, declared) === true, "independent source proof rejected leaf bound");
    const S = natural(b.support), D = natural(b.denominator, true), M = rational(b.norm);
    need(S <= ambient(source.degree) && M.n >= 0n, "nonnegative full-carrier leaf bounds"); leaves.set(b.id, normalized(S, D, M, source.degree));
  }
  const bounds = [], rows = [], last = plan.nodes.map((_, i) => i), seenLeaves = new Set();
  for (let i = 0; i < plan.nodes.length; i++) {
    const n = plan.nodes[i]; shape(n, ["id", "op", "degree", "inputs", "parameters"]);
    need(n.id === i && integer(n.degree, -1, 14) && Array.isArray(n.inputs) && n.inputs.every(x => integer(x, 0, i - 1)), "typed DAG topology");
    for (const input of n.inputs) last[input] = i;
  }
  const markNames = new Set();
  for (const mark of plan.marks) {
    shape(mark, ["name", "degree", "node", "expanded"]);
    need(typeof mark.name === "string" && /^[\x20-\x7e]+$/.test(mark.name) && mark.name.trim() && !markNames.has(mark.name) && integer(mark.degree, 0, 14) && integer(mark.node, 0, plan.nodes.length - 1) && typeof mark.expanded === "boolean", "exact typed mark identity");
    need(plan.nodes[mark.node].degree < 0 || plan.nodes[mark.node].degree === mark.degree, "mark degree"); markNames.add(mark.name);
  }
  const live = new Map(); let liveRecords = 0n, peakRecords = 0n, peakTensors = 0n, peakRational = 1n, totalPairs = 0n, totalSlots = 0n;
  for (let i = 0; i < plan.nodes.length; i++) {
    const n = plan.nodes[i], p = n.parameters, inputs = n.inputs.map(j => bounds[j]), ds = n.inputs.map(j => plan.nodes[j].degree);
    let S, D = 1n, M = zero, inferred = n.degree, pairVisits = 0n, slotVisits = 0n, extraRecords = null, scratchTensors = 1n, scratchRational = 1n;
    const unary = () => need(inputs.length === 1, "unary primitive arity"), binary = () => need(inputs.length === 2, "binary primitive arity");
    switch (n.op) {
      case "leaf": {
        shape(p, ["id"]); need(!inputs.length && leaves.has(p.id) && !seenLeaves.has(p.id), "declared leaf registration"); seenLeaves.add(p.id);
        ({ S, D, M } = leaves.get(p.id)); inferred = plan.leaves.find(l => l.id === p.id).degree;
        // Independent retained replay scans a canonical snapshot before
        // hashing, then imports that SAME snapshot. Neither scan is free.
        slotVisits = mul(2n, S); break;
      }
      case "zero": shape(p, []); need(!inputs.length, "zero arity"); S = 0n; inferred = -1; break;
      case "unit": {
        shape(p, ["form", "blade", "real", "imaginary"]); need(!inputs.length && integer(p.form, 0, 16383) && integer(p.blade, 0, 16383), "unit masks/arity");
        const real = rational(p.real), imaginary = rational(p.imaginary); S = 1n; D = lcm(real.d, imaginary.d); M = plus(absolute(real), absolute(imaginary)); inferred = degree(p.form); break;
      }
      case "sum": {
        shape(p, []); need(inputs.length > 0, "sum arity"); const known = [...new Set(ds.filter(d => d >= 0))]; need(known.length <= 1, "sum degree consistency"); inferred = known.length ? known[0] : -1;
        S = 0n; for (const a of inputs) { S = add(S, a.S); D = lcm(D, a.D); M = plus(M, a.M); } slotVisits = S; break;
      }
      case "scale": {
        unary(); shape(p, ["real", "imaginary"]); const real = rational(p.real), imaginary = rational(p.imaginary), a = inputs[0];
        S = a.S; D = mul(a.D, lcm(real.d, imaginary.d)); M = times(a.M, plus(absolute(real), absolute(imaginary))); inferred = ds[0]; slotVisits = a.S; break;
      }
      case "product": case "transpose": {
        binary(); shape(p, ["kind"]); need((n.op === "product" ? ["W", "C", "A"] : ["C", "A"]).includes(p.kind), "product kind");
        const [a, b] = inputs; pairVisits = mul(a.S, b.S); S = pairVisits; D = mul(a.D, b.D); M = times(times(a.M, b.M), p.kind === "W" ? one : two);
        const d = n.op === "product" ? ds[0] + ds[1] : ds[1] - ds[0]; inferred = ds.some(d => d < 0) || d < 0 || d > 14 ? -1 : d; break;
      }
      case "motion": case "motionAdjoint": {
        unary(); const a = inputs[0], m = matrix(p); inferred = ds[0]; slotVisits = mul(a.S, BigInt(Math.max(0, inferred) * 14)); S = slotVisits; D = mul(a.D, m.D); M = times(m.M, a.M); break;
      }
      case "pullback": {
        unary(); const a = inputs[0], m = matrix(p); inferred = ds[0]; const r = Math.max(0, inferred), forms = minimum(a.S, choose(14, r));
        const supports = Array.from({ length: r + 1 }, (_, k) => minimum(choose(14, k), pow(m.rowSupport, k)));
        const finalSupport = supports[r]; S = mul(a.S, finalSupport); D = mul(a.D, pow(m.D, r)); M = times(a.M, power(m.rowNorm, r));
        if (forms > 0n) {
          scratchRational = printedBound({ S: m.rowSupport, D: m.D, M: m.rowNorm });
          for (let k = 0; k <= r; k++) scratchRational = maximum(scratchRational, printedBound({ S: supports[k], D: pow(m.D, k), M: power(m.rowNorm, k) }));
        }
        let stagePairs = 0n, activeFormPeak = 1n;
        for (let k = 0; k < r; k++) { stagePairs = add(stagePairs, mul(supports[k], m.rowSupport)); activeFormPeak = maximum(activeFormPeak, add(add(supports[k], m.rowSupport), supports[k + 1])); }
        pairVisits = mul(forms, stagePairs); slotVisits = add(mul(forms, BigInt(14 * r)), mul(a.S, finalSupport));
        const cacheAndWork = forms === 0n ? 0n : maximum(mul(forms, finalSupport), add(mul(forms - 1n, finalSupport), activeFormPeak));
        // During Product(form,row), old form, row and next form coexist; all
        // earlier cached forms and the accumulated output also remain live.
        extraRecords = add(minimum(S, ambient(inferred)), cacheAndWork); scratchTensors = forms === 0n ? 1n : add(forms, r === 0 ? 1n : 3n); break;
      }
      case "spin": {
        need(!inputs.length, "spin arity"); const m = matrix(p); S = m.lowerSupport; D = mul(2n, m.D); M = times(m.lower, { n: 1n, d: 2n }); inferred = 0; slotVisits = 91n; break;
      }
      case "star": case "raise": case "contract": case "component": case "starAdjoint": {
        unary(); const a = inputs[0]; ({ S, D, M } = a); slotVisits = a.S;
        if (n.op === "contract") { shape(p, ["axis"]); need(integer(p.axis, 0, 13), "contraction axis"); inferred = ds[0] <= 0 ? -1 : ds[0] - 1; }
        else if (n.op === "component") { shape(p, ["form"]); need(integer(p.form, 0, 16383), "component mask"); inferred = 0; }
        else if (n.op === "starAdjoint") { shape(p, ["degree"]); need(integer(p.degree, 0, 14) && (ds[0] < 0 || ds[0] === 14 - p.degree), "Hodge-adjoint input degree"); inferred = p.degree; }
        else { shape(p, []); inferred = n.op === "star" ? ds[0] < 0 ? -1 : 14 - ds[0] : ds[0]; } break;
      }
      default: need(false, "unknown primitive " + n.op);
    }
    need(inferred === n.degree, "independent primitive degree inference");
    const b = normalized(S, D, M, inferred); bounds.push(b);
    const scratchRecords = extraRecords === null ? b.S : extraRecords;
    peakRecords = maximum(peakRecords, add(liveRecords, scratchRecords)); peakTensors = maximum(peakTensors, add(BigInt(live.size), scratchTensors));
    live.set(i, b); liveRecords = add(liveRecords, b.S);
    totalPairs = add(totalPairs, pairVisits); totalSlots = add(totalSlots, slotVisits);
    // Parameter rationals are parsed even when an empty operand or exact zero
    // result makes every tensor height trivial. Include all matrix entries,
    // not only the triangle/rows that happen to contribute to this operation.
    const parameterRational = n.op === "unit" || n.op === "scale"
      ? maximum(BigInt(p.real.length), BigInt(p.imaginary.length))
      : ["motion", "motionAdjoint", "pullback", "spin"].includes(n.op)
        ? maximum(0n, ...p.matrix.map(entry => BigInt(entry.value.length))) : 0n;
    const printed = printedBound(b), primitivePrinted = maximum(printed, scratchRational, parameterRational); peakRational = maximum(peakRational, primitivePrinted);
    const emptyRecordBytes = BigInt(Buffer.byteLength('{"form":16383,"blade":16383,"k0":0,"k1":0,"real":"","imaginary":""}'));
    const tensorBytes = b.S === 0n ? 2n : add(1n, mul(b.S, add(add(emptyRecordBytes, mul(2n, printed)), 1n)));
    rows.push({ id: i, degree: inferred, support: b.S.toString(), denominator: b.D.toString(), norm: text(b.M), rationalCharacters: printed.toString(), parameterRationalCharacters: parameterRational.toString(), primitiveRationalCharacters: primitivePrinted.toString(), tensorWireBytes: tensorBytes.toString(), pairVisits: pairVisits.toString(), slotVisits: slotVisits.toString(), primitiveScratchRecords: scratchRecords.toString(), primitiveScratchTensors: scratchTensors.toString() });
    for (const id of new Set(n.inputs)) if (last[id] === i) { liveRecords -= live.get(id).S; live.delete(id); }
    if (last[i] === i) { liveRecords -= b.S; live.delete(i); }
  }
  need(seenLeaves.size === leaves.size && live.size === 0 && liveRecords === 0n, "complete leaf/liveness census");
  // Recorder workload is separate from primitive/replay work. Append hashes
  // every tensor once; RegisterLeaf hashes it once more before Append; each
  // Mark hashes again even when another mark names the identical node. These
  // hashes serialize the tensor array without LF. Expanded marks serialize
  // that array again plus LF. Only tensor bodies are counted here, not graph
  // metadata, filenames, envelopes, SHA CPU instructions or sorting storage.
  let fingerprintRecordVisits = 0n, fingerprintWireBytes = 0n, expandedRecordVisits = 0n, expandedWireBytes = 0n, sortingRecords = 0n;
  for (let i = 0; i < plan.nodes.length; i++) {
    const repetitions = plan.nodes[i].op === "leaf" ? 2n : 1n, wireBytes = natural(rows[i].tensorWireBytes);
    fingerprintRecordVisits = add(fingerprintRecordVisits, mul(repetitions, bounds[i].S));
    fingerprintWireBytes = add(fingerprintWireBytes, mul(repetitions, wireBytes));
    sortingRecords = maximum(sortingRecords, bounds[i].S);
  }
  for (const mark of plan.marks) {
    const records = bounds[mark.node].S, wireBytes = natural(rows[mark.node].tensorWireBytes);
    fingerprintRecordVisits = add(fingerprintRecordVisits, records); fingerprintWireBytes = add(fingerprintWireBytes, wireBytes);
    if (mark.expanded) { expandedRecordVisits = add(expandedRecordVisits, records); expandedWireBytes = add(expandedWireBytes, add(wireBytes, 1n)); }
  }
  // In a successful exact-name plan, marks.All(Name != name) examines every
  // earlier mark. This is a comparison count, not a string-character or time
  // bound; failed/duplicate-name paths are outside the successful-run census.
  const markCount = BigInt(plan.marks.length), duplicateMarkComparisons = markCount < 2n ? 0n : mul(markCount, markCount - 1n) / 2n;
  return {
    schemaVersion: "phase627-analytic-resource-majorants-v1", nodes: rows,
    pairVisits: totalPairs.toString(), slotVisits: totalSlots.toString(), replayTensorRecordPeak: peakRecords.toString(), replayTensorDictionaryPeak: peakTensors.toString(), reducedRationalCharacterPeak: peakRational.toString(),
    recorderWork: { fingerprintRecordVisits: fingerprintRecordVisits.toString(), fingerprintWireBytes: fingerprintWireBytes.toString(), expandedRecordVisits: expandedRecordVisits.toString(), expandedWireBytes: expandedWireBytes.toString(), maximumSortingTensorRecords: sortingRecords.toString(), duplicateMarkNameComparisons: duplicateMarkComparisons.toString() },
    scope: { scientificCoefficientsEvaluated: false, observedNewResultsUsed: false, fullAmbientCarrier: true, beforePruningWorkBounds: true, pairAndSlotCountersOnly: true, otherScanWorkProved: false, pullbackScratchIncluded: true,
      graphMetadataMemoryProved: false, serializationSortingMemoryProved: false, callerLeafCopiesMemoryProved: false, unreducedScalarTemporaryMemoryProved: false, processRssProved: false, completeRuntimeResourceProof: false }
  };
}

module.exports = { deriveMixedResourceMajorants };
