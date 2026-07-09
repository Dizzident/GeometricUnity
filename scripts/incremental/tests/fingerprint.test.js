"use strict";
const test = require("node:test");
const assert = require("node:assert");
const fs = require("fs");
const path = require("path");
const { computeFingerprint } = require("../fingerprint");
const { parseGeneratorScriptFile } = require("../registry");
const { emptyManifest } = require("../manifest");
const { buildEntry } = require("../entry");
const { makeFixtureRepo, rmrf } = require("./fixture");

function setup() {
  const root = makeFixtureRepo();
  const parsed = parseGeneratorScriptFile(
    path.join(root, "scripts/generate_validated_boson_predictions.sh")
  );
  const steps = parsed.steps.filter((s) => s.kind === "phase");
  const registryPhaseSet = new Set(steps.map((s) => s.phase));
  return { root, steps, registryPhaseSet };
}

test("fingerprint is deterministic and labeled components are sorted", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const a = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  const b = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(a.fingerprint, b.fingerprint);
  assert.deepStrictEqual(a.components, [...a.components].sort(), "components are sorted");
  // component kinds present for phase2: scheme, invoke, self, dep, frozen, scan
  const kinds = new Set(a.components.map((c) => c.split(":")[0]));
  for (const k of ["scheme", "invoke", "self", "dep", "frozen", "scan"]) {
    assert.ok(kinds.has(k), `missing component kind ${k}`);
  }
});

test("dep vs frozen classification follows run-node membership", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const fp = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  // phase1 is a run node -> dep component; phase0 (matrices) is not -> scan/frozen
  assert.ok(fp.components.some((c) => c.startsWith("dep:studies/phase1_alpha_001/output/alpha_summary.json:phase1:")));
  assert.ok(fp.components.some((c) => c.startsWith("frozen:docs/NOTES.md:file:")));
  assert.ok(fp.components.some((c) => c.startsWith("scan:studies/phase0_matrices_001/output/matrices:tree:")));
});

test("dep component uses producer manifest output hash and enables early cutoff", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();

  // no producer entry -> NOENTRY marker
  const fp0 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.ok(fp0.components.some((c) => /^dep:.*:phase1:NOENTRY$/.test(c)));

  // seed producer -> dep component now carries its canonical output hash
  const { entry: e1 } = buildEntry({ repoRoot: root, step: steps[0], registryPhaseSet, manifest });
  manifest.phases["1"] = e1;
  const fp1 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.ok(fp1.components.some((c) => /^dep:.*:phase1:out:[0-9a-f]{64}$/.test(c)));
  assert.notStrictEqual(fp0.fingerprint, fp1.fingerprint);

  // producer rewrites output with SAME canonical content but a new volatile
  // timestamp -> consumer fingerprint UNCHANGED (early cutoff)
  const outFile = path.join(root, "studies/phase1_alpha_001/output/alpha_summary.json");
  fs.writeFileSync(outFile, JSON.stringify({ value: 42, generatedAt: "2027-12-31T23:59:59Z" }) + "\n");
  const { entry: e1b } = buildEntry({ repoRoot: root, step: steps[0], registryPhaseSet, manifest });
  manifest.phases["1"] = e1b;
  const fp2 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.strictEqual(fp1.fingerprint, fp2.fingerprint, "volatile-only change must not cascade");

  // producer output VALUE change -> consumer fingerprint changes
  fs.writeFileSync(outFile, JSON.stringify({ value: 43, generatedAt: "2027-12-31T23:59:59Z" }) + "\n");
  const { entry: e1c } = buildEntry({ repoRoot: root, step: steps[0], registryPhaseSet, manifest });
  manifest.phases["1"] = e1c;
  const fp3 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp3.fingerprint, "value change must cascade");
});

test("frozen input changes and appearances flip the fingerprint", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const fp1 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  fs.writeFileSync(path.join(root, "docs/NOTES.md"), "notes v2\n");
  const fp2 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint);

  // a missing frozen input hashes as MISSING; its appearance changes the print
  fs.rmSync(path.join(root, "docs/NOTES.md"));
  const fpMissing = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.ok(fpMissing.components.some((c) => c === "frozen:docs/NOTES.md:MISSING"));
  assert.notStrictEqual(fpMissing.fingerprint, fp2.fingerprint);
});

test("scan root content changes (including NEW files) flip the fingerprint", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const fp1 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  fs.writeFileSync(
    path.join(root, "studies/phase0_matrices_001/output/matrices/m2.json"),
    '{"b":2}\n'
  );
  const fp2 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint, "new file in scanned dir must be seen");
});

test("unresolvable scanner root is a forced-run reason (any doubt runs)", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const fp = computeFingerprint({
    repoRoot: root,
    step: steps.find((s) => s.phase === 3),
    registryPhaseSet,
    manifest: emptyManifest(),
  });
  assert.ok(fp.forcedRunReasons.length > 0);
  assert.match(fp.forcedRunReasons[0], /unresolvable scanner root/);
});

test("native env prefix folds native/build into the fingerprint", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const step3 = steps.find((s) => s.phase === 3);
  const manifest = emptyManifest();
  const fp1 = computeFingerprint({ repoRoot: root, step: step3, registryPhaseSet, manifest });
  assert.ok(fp1.components.some((c) => c.startsWith("native:native/build:tree:")));
  fs.writeFileSync(path.join(root, "native/build/libgu.so"), "fake-native-v2\n");
  const fp2 = computeFingerprint({ repoRoot: root, step: step3, registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint);
});

test("recorded read-set paths not covered statically are folded in", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  fs.writeFileSync(path.join(root, "docs/EXTRA.md"), "extra v1\n");
  const entry = { recordedInputs: ["docs/EXTRA.md", "docs/NOTES.md"] };
  const manifest = emptyManifest();
  const fp1 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest, entry });
  // NOTES.md already covered by a frozen literal -> only EXTRA.md gets a readset component
  assert.ok(fp1.components.some((c) => c.startsWith("readset:docs/EXTRA.md:file:")));
  assert.ok(!fp1.components.some((c) => c.startsWith("readset:docs/NOTES.md:")));
  fs.writeFileSync(path.join(root, "docs/EXTRA.md"), "extra v2\n");
  const fp2 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest, entry });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint);
});

test("invocation line changes flip the fingerprint", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const fp1 = computeFingerprint({ repoRoot: root, step: steps[1], registryPhaseSet, manifest });
  const altered = { ...steps[1], invocation: steps[1].invocation + " --extra-arg" };
  const fp2 = computeFingerprint({ repoRoot: root, step: altered, registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint);
});

test("src project edits (transitive) flip the fingerprint", (t) => {
  const { root, steps, registryPhaseSet } = setup();
  t.after(() => rmrf(root));
  const manifest = emptyManifest();
  const step1 = steps[0];
  const fp1 = computeFingerprint({ repoRoot: root, step: step1, registryPhaseSet, manifest });
  fs.writeFileSync(path.join(root, "src/Gu.Base/Base.cs"), "// base v2\n");
  const fp2 = computeFingerprint({ repoRoot: root, step: step1, registryPhaseSet, manifest });
  assert.notStrictEqual(fp1.fingerprint, fp2.fingerprint, "transitive src ref must be fingerprinted");
});
