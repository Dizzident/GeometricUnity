"use strict";
const test = require("node:test");
const assert = require("node:assert");
const fs = require("fs");
const path = require("path");
const { decide } = require("../should_run");
const { parseGeneratorScriptFile } = require("../registry");
const { emptyManifest } = require("../manifest");
const { buildEntry } = require("../entry");
const { SCHEMA_VERSION, ALWAYS_RUN_PHASES } = require("../config");
const { makeFixtureRepo, rmrf } = require("./fixture");

function setupSeeded() {
  const root = makeFixtureRepo();
  const parsed = parseGeneratorScriptFile(
    path.join(root, "scripts/generate_validated_boson_predictions.sh")
  );
  const steps = parsed.steps.filter((s) => s.kind === "phase");
  const registryPhaseSet = new Set(steps.map((s) => s.phase));
  const manifest = emptyManifest();
  for (const step of steps) {
    const { entry } = buildEntry({ repoRoot: root, step, registryPhaseSet, manifest });
    manifest.phases[String(step.phase)] = entry;
  }
  return { root, steps, registryPhaseSet, manifest };
}

test("happy path: seeded, unchanged phase SKIPS", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, false, d.reason);
});

test("failure mode: no manifest entry => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  delete manifest.phases["2"];
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /no manifest entry/);
});

test("failure mode: schemaVersion mismatch => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  manifest.phases["2"].schemaVersion = SCHEMA_VERSION + 1;
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /schemaVersion mismatch/);
});

test("failure mode: phase code change => fingerprint mismatch => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.appendFileSync(path.join(root, "studies/phase2_beta_001/Program.cs"), "// edited\n");
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /inputFingerprint mismatch/);
});

test("failure mode: upstream dep output value change => RUN (and cascade order)", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  // simulate phase1 having re-run with a different value: disk + manifest entry change
  fs.writeFileSync(
    path.join(root, "studies/phase1_alpha_001/output/alpha_summary.json"),
    JSON.stringify({ value: 99, generatedAt: "x" }) + "\n"
  );
  const { entry } = buildEntry({ repoRoot: root, step: steps[0], registryPhaseSet, manifest });
  manifest.phases["1"] = entry;
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /inputFingerprint mismatch/);
});

test("no cascade on volatile-only upstream rewrite", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.writeFileSync(
    path.join(root, "studies/phase1_alpha_001/output/alpha_summary.json"),
    JSON.stringify({ generatedAt: "2099-01-01T00:00:00Z", value: 42 }) + "\n"
  );
  const { entry } = buildEntry({ repoRoot: root, step: steps[0], registryPhaseSet, manifest });
  manifest.phases["1"] = entry;
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, false, d.reason);
});

test("failure mode: manifest-listed output missing => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.rmSync(path.join(root, "studies/phase2_beta_001/output/beta_summary.json"));
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /output missing/);
});

test("failure mode: output hash mismatch (tampered value) => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.writeFileSync(
    path.join(root, "studies/phase2_beta_001/output/beta_summary.json"),
    JSON.stringify({ derived: 9999 }) + "\n"
  );
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /output hash mismatch/);
});

test("volatile-only output rewrite still SKIPS (canonical hash)", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.writeFileSync(
    path.join(root, "studies/phase2_beta_001/output/beta_summary.json"),
    JSON.stringify({ runtimeSeconds: 777, derived: 84, generatedAt: "z" }) + "\n"
  );
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, false, d.reason);
});

test("failure mode: recorded input missing => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  // record a read-set including a file, reseed so the fingerprint includes it,
  // then delete the file
  fs.writeFileSync(path.join(root, "docs/EXTRA.md"), "x\n");
  manifest.phases["2"].recordedInputs = ["docs/EXTRA.md"];
  const { entry } = buildEntry({
    repoRoot: root,
    step: steps[1],
    registryPhaseSet,
    manifest,
    previousEntry: manifest.phases["2"],
  });
  manifest.phases["2"] = entry;
  assert.strictEqual(decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest }).run, false);
  fs.rmSync(path.join(root, "docs/EXTRA.md"));
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  // the missing file flips the readset component (fingerprint mismatch) --
  // either reason is a valid fail-closed detection
  assert.match(d.reason, /recorded input missing|inputFingerprint mismatch/);
});

test("failure mode: unresolvable scanner root => RUN even when seeded", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  const step3 = steps.find((s) => s.phase === 3);
  const d = decide({ repoRoot: root, step: step3, registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
  assert.match(d.reason, /fail-closed: unresolvable scanner root/);
});

test("failure mode: malformed manifest entry => RUN", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  manifest.phases["2"] = { schemaVersion: SCHEMA_VERSION, outputs: null, inputFingerprint: 5 };
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
});

test("failure mode: exception (project dir removed) => RUN, never throws", (t) => {
  const { root, steps, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  fs.rmSync(path.join(root, "studies/phase2_beta_001"), { recursive: true });
  const d = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d.run, true);
});

test("always-run set is never skipped, kept as data", (t) => {
  const { root, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  assert.deepStrictEqual(
    [...ALWAYS_RUN_PHASES].sort((a, b) => a - b),
    [101, 151, 156, 158, 202, 204, 205, 212, 216, 217, 219, 253, 279, 281, 295, 296]
  );
  for (const phase of ALWAYS_RUN_PHASES) {
    const step = {
      kind: "phase",
      phase,
      project: `studies/phase${phase}_x_001/X.csproj`,
      invocation: "dotnet run ...",
      env: {},
    };
    const d = decide({ repoRoot: root, step, registryPhaseSet, manifest });
    assert.strictEqual(d.run, true);
    assert.match(d.reason, /always-run/);
  }
});

test("integrity step always runs", (t) => {
  const { root, registryPhaseSet, manifest } = setupSeeded();
  t.after(() => rmrf(root));
  const d = decide({
    repoRoot: root,
    step: { kind: "integrity", invocation: "./scripts/verify_boson_claim_integrity.sh" },
    registryPhaseSet,
    manifest,
  });
  assert.strictEqual(d.run, true);
});
