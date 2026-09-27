"use strict";

// Dependency-free own-data JSON snapshot, extracted without semantic changes
// from source orchestration. Preserve the historical diagnostic prefix below.
// Quotas cover logical copied nodes/slots/characters, NOT reflection scratch,
// engine/module initialization, serialization, GC/RSS or hostile Proxy effects.
// Callers needing sticky failure/reentry rejection must supply their own guard.
const need = (ok, why) => { if (!ok) throw new Error("A68 source orchestration: " + why); };
const integer = (x, min = 0, max = Number.MAX_SAFE_INTEGER) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= min && x <= max;
const ascii = x => typeof x === "string" && /^[\x20-\x7e]*$/.test(x);
function own(x, key) { const d = Object.getOwnPropertyDescriptor(x, key); need(d && Object.hasOwn(d, "value"), "own data property " + key); return d.value; }
function shape(x, keys) {
  need(x && typeof x === "object" && !Array.isArray(x), "closed data object");
  const actual = Reflect.ownKeys(x); need(actual.length === keys.length && keys.every((key, i) => actual[i] === key), "closed ordered fields " + keys.join(","));
  return Object.fromEntries(keys.map(key => [key, own(x, key)]));
}
function array(x, cap) {
  need(Array.isArray(x), "dense array"); const n = own(x, "length");
  need(integer(n, 0, cap), "pre-copy array length ceiling");
  need(Reflect.ownKeys(x).length === n + 1, "array has no extra fields or symbols");
  for (let i = 0; i < n; i++) own(x, String(i)); return n;
}
function caps(x, keys) { x = shape(x, keys); need(keys.every(k => integer(x[k], 1)), "explicit finite positive limits"); return Object.freeze(x); }
function counter(limits) {
  const used = Object.fromEntries(Object.keys(limits).map(k => [k, 0]));
  return { used, charge: (key, count = 1) => { need(integer(count) && count <= limits[key] - used[key], "pre-copy " + key + " ceiling"); used[key] += count; } };
}

function snapshotCanonicalMetadata(value, limits) {
  const cap = caps(limits, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(cap.maxDepth <= 64, "bounded recursive metadata depth");
  const { used, charge } = counter(cap), ancestors = new WeakSet();
  function visit(x, depth) {
    need(depth <= cap.maxDepth, "metadata depth ceiling"); charge("nodes");
    if (x === null || typeof x === "boolean") return x;
    if (typeof x === "number") { need(integer(x, -Number.MAX_SAFE_INTEGER), "canonical safe metadata integer"); return x; }
    if (typeof x === "string") { need(ascii(x), "printable ASCII metadata"); charge("stringCharacters", x.length); return x; }
    need(x && typeof x === "object" && !ancestors.has(x), "acyclic JSON metadata"); ancestors.add(x);
    let result;
    if (Array.isArray(x)) {
      const n = array(x, cap.arraySlots - used.arraySlots); charge("arraySlots", n); result = [];
      for (let i = 0; i < n; i++) result.push(visit(own(x, String(i)), depth + 1));
    } else {
      const keys = Reflect.ownKeys(x); charge("arraySlots", keys.length); result = {};
      for (const key of keys) {
        need(ascii(key), "own string metadata keys"); charge("stringCharacters", key.length);
        Object.defineProperty(result, key, { value: visit(own(x, key), depth + 1), enumerable: true, configurable: false, writable: false });
      }
    }
    ancestors.delete(x); return Object.freeze(result);
  }
  return Object.freeze({ value: visit(value, 0), usage: Object.freeze(used) });
}

module.exports = { snapshotCanonicalMetadata };
