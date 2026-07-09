"use strict";
// Canonical JSON hashing. Canonicalization = parse, strip the declared
// volatile keys at any depth, sort object keys, deterministic re-serialize.
// Used ONLY to compute hashes; files on disk are never rewritten.

const crypto = require("crypto");
const fs = require("fs");
const { VOLATILE_KEYS } = require("./config");

const VOLATILE_SET = new Set(VOLATILE_KEYS);

function sha256Hex(bufOrString) {
  return crypto.createHash("sha256").update(bufOrString).digest("hex");
}

// Recursively strip volatile keys and produce a deterministic string.
function canonicalStringify(value) {
  if (value === null) return "null";
  const t = typeof value;
  if (t === "number") {
    if (!Number.isFinite(value)) return "null"; // matches JSON.stringify
    return JSON.stringify(value);
  }
  if (t === "boolean" || t === "string") return JSON.stringify(value);
  if (Array.isArray(value)) {
    return "[" + value.map((v) => canonicalStringify(v === undefined ? null : v)).join(",") + "]";
  }
  if (t === "object") {
    const keys = Object.keys(value).filter((k) => !VOLATILE_SET.has(k)).sort();
    const parts = [];
    for (const k of keys) {
      const v = value[k];
      if (v === undefined) continue;
      parts.push(JSON.stringify(k) + ":" + canonicalStringify(v));
    }
    return "{" + parts.join(",") + "}";
  }
  // undefined / function / symbol at top level
  return "null";
}

function canonicalizeJsonText(text) {
  const parsed = JSON.parse(text); // throws on invalid JSON (caller decides)
  return canonicalStringify(parsed);
}

// Canonical hash of a JSON text: sha256 of the canonicalized form.
function canonicalHashOfJsonText(text) {
  return sha256Hex(canonicalizeJsonText(text));
}

// Hash a file: .json files are canonically hashed (volatile keys stripped);
// a .json file that fails to parse falls back to a raw hash (deterministic,
// and any change still forces a run). Non-JSON files: raw sha256.
function hashFileCanonical(absPath) {
  const buf = fs.readFileSync(absPath);
  if (absPath.toLowerCase().endsWith(".json")) {
    try {
      return canonicalHashOfJsonText(buf.toString("utf8"));
    } catch {
      return sha256Hex(buf);
    }
  }
  return sha256Hex(buf);
}

module.exports = {
  sha256Hex,
  canonicalStringify,
  canonicalizeJsonText,
  canonicalHashOfJsonText,
  hashFileCanonical,
};
