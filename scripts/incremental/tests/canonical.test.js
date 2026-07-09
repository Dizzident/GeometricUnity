"use strict";
const test = require("node:test");
const assert = require("node:assert");
const fs = require("fs");
const os = require("os");
const path = require("path");
const {
  canonicalizeJsonText,
  canonicalHashOfJsonText,
  hashFileCanonical,
  sha256Hex,
} = require("../canonical");
const { VOLATILE_KEYS } = require("../config");

test("volatile keys are stripped at any nesting depth", () => {
  const a = JSON.stringify({
    generatedAt: "2026-01-01",
    value: 1,
    nested: { runtimeSeconds: 99.5, deep: [{ gpuScanSeconds: 3, x: 2 }] },
  });
  const b = JSON.stringify({
    value: 1,
    nested: { deep: [{ x: 2, slqHessianVectorSeconds: 1 }], cpuParitySeconds: 7 },
    estimatedCpuFullScanSeconds: 123,
    fullJointGradientSeconds: 1,
    fullThetaGradientSeconds: 2,
  });
  assert.strictEqual(canonicalHashOfJsonText(a), canonicalHashOfJsonText(b));
});

test("every declared volatile key is actually stripped", () => {
  for (const key of VOLATILE_KEYS) {
    const withKey = JSON.stringify({ x: 1, [key]: "volatile" });
    const withoutKey = JSON.stringify({ x: 1 });
    assert.strictEqual(
      canonicalHashOfJsonText(withKey),
      canonicalHashOfJsonText(withoutKey),
      `key not stripped: ${key}`
    );
  }
});

test("key order does not affect the canonical form", () => {
  const a = '{"b":2,"a":1,"c":{"z":1,"y":2}}';
  const b = '{"a":1,"c":{"y":2,"z":1},"b":2}';
  assert.strictEqual(canonicalizeJsonText(a), canonicalizeJsonText(b));
});

test("value changes change the hash", () => {
  assert.notStrictEqual(
    canonicalHashOfJsonText('{"a":1}'),
    canonicalHashOfJsonText('{"a":2}')
  );
});

test("array order is significant", () => {
  assert.notStrictEqual(
    canonicalHashOfJsonText('{"a":[1,2]}'),
    canonicalHashOfJsonText('{"a":[2,1]}')
  );
});

test("volatile key as a value-bearing name only strips keys, not values", () => {
  // a string VALUE equal to a volatile key name must survive
  const t = '{"a":"generatedAt"}';
  assert.match(canonicalizeJsonText(t), /generatedAt/);
});

test("whitespace and formatting are canonicalized away", () => {
  assert.strictEqual(
    canonicalHashOfJsonText('{ "a" : 1 ,\n "b": [ 1, 2 ] }'),
    canonicalHashOfJsonText('{"b":[1,2],"a":1}')
  );
});

test("hashFileCanonical: json canonical, non-json raw, invalid json raw fallback", () => {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), "canon-"));
  try {
    const j1 = path.join(dir, "a.json");
    const j2 = path.join(dir, "b.json");
    fs.writeFileSync(j1, '{"x":1,"generatedAt":"t1"}');
    fs.writeFileSync(j2, '{"generatedAt":"t2","x":1}');
    assert.strictEqual(hashFileCanonical(j1), hashFileCanonical(j2));

    const txt = path.join(dir, "c.txt");
    fs.writeFileSync(txt, "hello");
    assert.strictEqual(hashFileCanonical(txt), sha256Hex("hello"));

    const bad = path.join(dir, "bad.json");
    fs.writeFileSync(bad, "{not json");
    assert.strictEqual(hashFileCanonical(bad), sha256Hex("{not json"));
  } finally {
    fs.rmSync(dir, { recursive: true, force: true });
  }
});
