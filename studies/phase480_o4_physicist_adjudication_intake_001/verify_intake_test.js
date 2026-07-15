#!/usr/bin/env node
"use strict";
const assert = require("assert");
const crypto = require("crypto");
const { canonicalize, parseStrict } = require("./verify_intake");

assert.deepStrictEqual(parseStrict('{"b":2,"a":1}'), { b: 2, a: 1 });
assert.throws(() => parseStrict('{"a":1,"a":2}'), /duplicate-key/);
assert.throws(() => parseStrict('{"a":NaN}'), /value-expected/);
assert.strictEqual(canonicalize({ z: "é", a: [true, -0, "\n"] }), '{"a":[true,0,"\\n"],"z":"é"}');
assert.throws(() => canonicalize("\ud800"), /unpaired-surrogate/);

const { publicKey, privateKey } = crypto.generateKeyPairSync("ed25519");
const payload = Buffer.from(canonicalize({ humanAuthored: true, ruling: "fixture-only" }));
const signature = crypto.sign(null, payload, privateKey);
assert.strictEqual(signature.length, 64);
assert.strictEqual(crypto.verify(null, payload, publicKey, signature), true);
assert.strictEqual(crypto.verify(null, Buffer.from(payload.toString() + "x"), publicKey, signature), false);

process.stdout.write("Phase480 strict JSON/JCS/Ed25519 tests passed.\n");
