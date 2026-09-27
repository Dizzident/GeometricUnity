"use strict";

// Synthetic wire-format tests only. The extracted block declares helpers but
// never activates the integrity verifier, a study, or coefficient replay.
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const vm = require("node:vm");
const crypto = require("node:crypto");
const { TextDecoder } = require("node:util");
const { createRequire } = require("node:module");

const verifier = path.resolve(__dirname, "../../verify_boson_claim_integrity.sh");
const source = fs.readFileSync(verifier, "utf8");
const begin = source.indexOf("// Begin A68 prospective DAG replay core.");
const end = source.indexOf("// End A68 prospective DAG replay core.", begin);
assert.ok(begin >= 0 && end > begin, "unique bounded prospective helper block exists");
assert.equal(source.indexOf("// Begin A68 prospective DAG replay core.", begin + 1), -1);
assert.equal(source.indexOf("// End A68 prospective DAG replay core.", end + 1), -1);

function helpers(fileSystem = fs) {
  const context = vm.createContext({ fs: fileSystem, Buffer, TextDecoder, assert, crypto,
    require: createRequire(path.resolve(__dirname, "../../../package.json")) });
  vm.runInContext(
    source.slice(begin, end) + "\nthis.wireHelpers = { parseA68MixedDagWire, readA68MixedDag };",
    context,
    { filename: "a68-prospective-wire-helper-extraction.js", timeout: 1000 }
  );
  return context.wireHelpers;
}

const { parseA68MixedDagWire: parse, readA68MixedDag: read } = helpers();
const limits = Object.freeze({ graphBytes: 16384 });
const graph = { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: [], marks: [] };
const valid = Buffer.from(JSON.stringify(graph) + "\n");
const validHash = crypto.createHash("sha256").update(valid).digest("hex");
const wire = text => Buffer.from(text, "utf8");
const reject = bytes => assert.throws(() => parse(bytes, limits));

test("A68 wire: canonical minimal synthetic graph is accepted without replay", () => {
  assert.equal(JSON.stringify(parse(valid, limits)), JSON.stringify(graph));
});

test("A68 wire: byte ceiling is enforced before decoding or parsing", () => {
  assert.throws(() => parse(valid, { graphBytes: valid.length - 1 }));
  assert.equal(JSON.stringify(parse(valid, { graphBytes: valid.length })), JSON.stringify(graph));
  for (const graphBytes of [0, -1, 1.5, Infinity, NaN, Number.MAX_SAFE_INTEGER + 1]) {
    assert.throws(() => parse(valid, { graphBytes }));
  }
  assert.throws(() => parse(Buffer.alloc(100, 0xff), { graphBytes: 99 }), /pre-parse byte ceiling/);
});

test("A68 wire: duplicate keys are rejected at every nesting level", () => {
  for (const text of [
    '{"a":1,"a":2}\n',
    '{"x":{"a":1,"a":2}}\n',
    '{"x":[{"a":1,"a":2}]}\n',
    '{"a":1,"\\u0061":2}\n',
    '{"__proto__":1,"__proto__":2}\n'
  ]) assert.throws(() => parse(wire(text), limits), /duplicate property/);
});

test("A68 wire: only canonical safe integer tokens are accepted", () => {
  for (const number of ["-0", "1.0", "1e0", "1E+0", "01", "-01", "+1", "9007199254740992", "-9007199254740992", "NaN", "Infinity"]) {
    reject(wire('{"n":' + number + '}\n'));
  }
  for (const number of ["0", "1", "-1", "9007199254740991", "-9007199254740991"]) {
    assert.equal(parse(wire('{"n":' + number + '}\n'), limits).n, Number(number));
  }
});

test("A68 wire: compact serialization and exactly one terminal LF are mandatory", () => {
  for (const text of [
    '{}', '{}\n\n', '{}\r\n', ' {}\n', '{} \n', '{ }\n', '{"a": 1}\n',
    '{\n"a":1}\n', '{}\n{}\n', '{}\t\n', '\ufeff{}\n', '{"a":1,}\n'
  ]) reject(wire(text));
  assert.equal(parse(wire('{"text":"space inside a string"}\n'), limits).text, "space inside a string");
});

test("A68 wire: malformed UTF-8 is rejected rather than replacement-decoded", () => {
  for (const invalid of [[0xc0, 0xaf], [0xe2, 0x82], [0x80], [0xed, 0xa0, 0x80], [0xf4, 0x90, 0x80, 0x80]]) {
    reject(Buffer.concat([wire('{"x":"'), Buffer.from(invalid), wire('"}\n')]));
  }
});

test("A68 wire: decoded strings are printable ASCII with canonical escaping", () => {
  for (const text of [
    '{"x":"\\u0061"}\n', '{"\\u0078":"a"}\n', '{"x":"\\/"}\n',
    '{"x":"\\t"}\n', '{"x":"\\n"}\n', '{"x":"\\u0000"}\n',
    '{"x":"\\u007f"}\n', '{"x":"é"}\n', '{"x":"\\ud800"}\n'
  ]) reject(wire(text));
  const text = { x: 'printable quote " and slash / and backslash \\ ~' };
  assert.equal(JSON.stringify(parse(wire(JSON.stringify(text) + "\n"), limits)), JSON.stringify(text));
});

test("A68 wire: nesting is bounded before unbounded recursive descent", () => {
  const nested = depth => wire('{"x":'.repeat(depth) + '0' + '}'.repeat(depth) + '\n');
  assert.doesNotThrow(() => parse(nested(15), limits));
  assert.throws(() => parse(nested(16), limits), /maximum depth16/);
  assert.throws(() => parse(nested(17), limits), /maximum depth16/);
  assert.throws(() => parse(nested(1000), limits), /maximum depth16/);
});

test("A68 file reader: bounded regular file accepted; oversized, directory and final symlink rejected", () => {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), "a68-wire-test-"));
  const file = path.join(directory, "synthetic.json");
  const link = path.join(directory, "synthetic-link.json");
  try {
    fs.writeFileSync(file, valid);
    fs.symlinkSync(file, link);
    assert.equal(JSON.stringify(read(file, limits, validHash)), JSON.stringify(graph));
    assert.throws(() => read(file, { graphBytes: valid.length - 1 }, validHash), /pre-read byte ceiling/);
    assert.throws(() => read(directory, limits, validHash), /regular file/);
    assert.throws(() => read(link, limits, validHash), /ELOOP/);
    assert.throws(() => read(file, limits), /required independently bound wire hash/);
    assert.throws(() => read(file, limits, "0".repeat(64)), /bound wire hash mismatch/);
  } finally {
    fs.rmSync(directory, { recursive: true, force: true });
  }
});

test("A68 file reader: synthetic growth, truncation and metadata races fail closed", () => {
  for (const scenario of ["growth", "truncation", "metadata"]) {
    const contents = scenario === "growth" ? Buffer.concat([valid, wire("x")]) :
      scenario === "truncation" ? valid.subarray(0, valid.length - 1) : valid;
    let position = 0, stats = 0, closed = 0;
    const fakeFs = {
      constants: fs.constants,
      openSync() { return 7; },
      fstatSync() {
        stats++;
        return { isFile: () => true, size: valid.length, mtimeMs: scenario === "metadata" && stats > 1 ? 2 : 1, ctimeMs: 1 };
      },
      readSync(descriptor, destination, offset, length, filePosition) {
        assert.equal(descriptor, 7);
        assert.equal(filePosition, null);
        assert.ok(length > 0 && length <= valid.length, "every read has an explicit bounded length");
        const count = Math.min(length, contents.length - position);
        contents.copy(destination, offset, position, position + count);
        position += count;
        return count;
      },
      closeSync(descriptor) { assert.equal(descriptor, 7); closed++; }
    };
    const failure = { growth: /file grew/, truncation: /file shortened/, metadata: /file changed/ }[scenario];
    assert.throws(() => helpers(fakeFs).readA68MixedDag("synthetic-unopened-path", limits, validHash), failure, scenario);
    assert.equal(closed, 1, scenario + " closes the descriptor exactly once");
  }
});
