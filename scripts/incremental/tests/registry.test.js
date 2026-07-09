"use strict";
const test = require("node:test");
const assert = require("node:assert");
const fs = require("fs");
const path = require("path");
const { parseGeneratorScript, parseGeneratorScriptFile, requireCleanParse } = require("../registry");
const { repoRootFromHere } = require("../config");

const REAL_SCRIPT = path.join(repoRootFromHere(), "scripts/generate_validated_boson_predictions.sh");

test("round-trips the real generator script: every dotnet-run line captured", () => {
  const text = fs.readFileSync(REAL_SCRIPT, "utf8");
  const parsed = parseGeneratorScript(text);

  assert.deepStrictEqual(parsed.errors, [], "no unparseable lines expected");
  assert.strictEqual(parsed.buildSteps.length, 1, "one central msbuild step");

  const phaseSteps = parsed.steps.filter((s) => s.kind === "phase");
  const grepCount = text
    .split("\n")
    .filter((l) => /dotnet run/.test(l) && !l.trim().startsWith("#")).length;
  assert.strictEqual(phaseSteps.length, grepCount, "every dotnet run line becomes a phase step");

  const integritySteps = parsed.steps.filter((s) => s.kind === "integrity");
  assert.strictEqual(integritySteps.length, 1, "integrity verifier captured");
  assert.strictEqual(parsed.steps[parsed.steps.length - 1].kind, "integrity", "integrity is the last step");

  // run order preserved: first phase in the script is the first step
  assert.strictEqual(phaseSteps[0].phase, 138);

  // env-prefixed invocation (phase405 native library) captured with env
  const p405 = phaseSteps.find((s) => s.phase === 405);
  assert.ok(p405, "phase405 present");
  assert.strictEqual(p405.env.LD_LIBRARY_PATH, "native/build");
  assert.match(p405.invocation, /^LD_LIBRARY_PATH=native\/build dotnet run /);

  // duplicate invocations preserved as distinct steps (140 twice, 177 thrice)
  assert.strictEqual(phaseSteps.filter((s) => s.phase === 140).length, 2);
  assert.strictEqual(phaseSteps.filter((s) => s.phase === 177).length, 3);
});

test("real script step count matches the checked-out script", () => {
  // The worktree copy of the script runs 321 phase invocations over 314
  // unique phases (the main checkout adds 450-452 uncommitted -> 324/317;
  // this assertion is against whatever is checked out here).
  const parsed = parseGeneratorScriptFile(REAL_SCRIPT);
  const phases = parsed.steps.filter((s) => s.kind === "phase");
  const unique = new Set(phases.map((s) => s.phase));
  assert.ok(phases.length >= 321, `expected >= 321 phase invocations, got ${phases.length}`);
  assert.ok(unique.size >= 314, `expected >= 314 unique phases, got ${unique.size}`);
});

test("fails loudly on unrecognized lines", () => {
  const bad = [
    "#!/usr/bin/env bash",
    "set -euo pipefail",
    "dotnet msbuild scripts/BosonPhasesTraversal.proj -t:Build",
    "python3 do_something_sneaky.py",
    "dotnet run --no-build -c Release --project studies/phase1_alpha_001/P.csproj",
  ].join("\n");
  const parsed = parseGeneratorScript(bad);
  assert.strictEqual(parsed.errors.length, 1);
  assert.match(parsed.errors[0].line, /sneaky/);
  assert.throws(() => requireCleanParse(parsed, "test.sh"), /unparseable|refusing/);
});

test("fails loudly on a dotnet run line without a parseable project", () => {
  const bad = [
    "dotnet msbuild scripts/BosonPhasesTraversal.proj -t:Build",
    "dotnet run --no-build -c Release --project somewhere/else.csproj",
  ].join("\n");
  const parsed = parseGeneratorScript(bad);
  assert.strictEqual(parsed.errors.length, 1);
  assert.match(parsed.errors[0].reason, /without a parseable/);
});

test("missing build step is rejected", () => {
  const parsed = parseGeneratorScript(
    "dotnet run --no-build -c Release --project studies/phase1_a_001/P.csproj"
  );
  assert.throws(() => requireCleanParse(parsed, "test.sh"), /no 'dotnet msbuild'/);
});
