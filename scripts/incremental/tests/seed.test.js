"use strict";
const test = require("node:test");
const assert = require("node:assert");
const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");
const { loadManifest } = require("../manifest");
const { decide } = require("../should_run");
const { parseGeneratorScriptFile } = require("../registry");
const { SCHEMA_VERSION } = require("../config");
const { makeFixtureRepo, rmrf } = require("./fixture");

const SEED = path.join(__dirname, "..", "seed.js");

function runSeed(root, extra = []) {
  return spawnSync(
    "node",
    [
      SEED,
      "--repo-root",
      root,
      "--script",
      path.join(root, "scripts/generate_validated_boson_predictions.sh"),
      "--manifest",
      path.join(root, "scripts/boson_incremental_manifest.json"),
      ...extra,
    ],
    { encoding: "utf8" }
  );
}

test("seed writes entries for phases with outputs; skip-then-invalidate round trip", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));

  const res = runSeed(root);
  assert.strictEqual(res.status, 0, res.stderr);

  const manifestPath = path.join(root, "scripts/boson_incremental_manifest.json");
  const manifest = loadManifest(manifestPath);
  assert.strictEqual(manifest.schemaVersion, SCHEMA_VERSION);
  assert.deepStrictEqual(Object.keys(manifest.phases).sort(), ["1", "2", "3"]);

  const e2 = manifest.phases["2"];
  assert.strictEqual(e2.schemaVersion, SCHEMA_VERSION);
  assert.match(e2.inputFingerprint, /^[0-9a-f]{64}$/);
  assert.deepStrictEqual(Object.keys(e2.outputs), [
    "studies/phase2_beta_001/output/beta_summary.json",
  ]);

  // seeded manifest lets an unchanged phase skip...
  const parsed = parseGeneratorScriptFile(
    path.join(root, "scripts/generate_validated_boson_predictions.sh")
  );
  const steps = parsed.steps.filter((s) => s.kind === "phase");
  const registryPhaseSet = new Set(steps.map((s) => s.phase));
  const d1 = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d1.run, false, d1.reason);

  // ...and a subsequently edited input forces a run
  fs.writeFileSync(path.join(root, "docs/NOTES.md"), "notes v2\n");
  const d2 = decide({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(d2.run, true);
});

test("seed refuses to seed phases with no outputs on disk (fail-closed)", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  fs.rmSync(path.join(root, "studies/phase2_beta_001/output"), { recursive: true });

  const res = runSeed(root);
  assert.strictEqual(res.status, 0, res.stderr);
  assert.match(res.stdout, /no outputs on disk for phases: 2/);

  const manifest = loadManifest(path.join(root, "scripts/boson_incremental_manifest.json"));
  assert.strictEqual(manifest.phases["2"], undefined, "phase without outputs must not be seeded");
  assert.ok(manifest.phases["1"], "phases with outputs are still seeded");
});

test("seed --dry-run writes nothing", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  const res = runSeed(root, ["--dry-run"]);
  assert.strictEqual(res.status, 0, res.stderr);
  assert.ok(!fs.existsSync(path.join(root, "scripts/boson_incremental_manifest.json")));
});

test("seed is idempotent (same fingerprints on a second run)", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  runSeed(root);
  const m1 = loadManifest(path.join(root, "scripts/boson_incremental_manifest.json"));
  runSeed(root);
  const m2 = loadManifest(path.join(root, "scripts/boson_incremental_manifest.json"));
  assert.deepStrictEqual(m1, m2);
});

test("seed fails loudly on an unparseable generator script", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  fs.appendFileSync(
    path.join(root, "scripts/generate_validated_boson_predictions.sh"),
    "rm -rf something_unexpected\n"
  );
  const res = runSeed(root);
  assert.notStrictEqual(res.status, 0);
  assert.match(res.stderr, /unparseable/);
});
