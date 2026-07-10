"use strict";
const test = require("node:test");
const assert = require("node:assert");
const { extractPhaseInputs } = require("../extract");
const { makeFixtureRepo, rmrf } = require("./fixture");

test("extraction: literals, const scan roots, Path.Combine, src refs, own-output exclusion", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));

  // phase2: dep literal + docs literal + two scan roots
  const p2 = extractPhaseInputs(root, "studies/phase2_beta_001/Phase2Beta.csproj");
  assert.deepStrictEqual(p2.pathLiterals, [
    "docs/NOTES.md",
    "studies/phase0_matrices_001/output/matrices",
    "studies/phase1_alpha_001/output/alpha_summary.json",
  ]);
  assert.deepStrictEqual(p2.scanRoots, [
    "studies/phase0_matrices_001/output/matrices",
    "studies/phase0_matrices_001/output/matrices/sub",
  ]);
  assert.deepStrictEqual(p2.unresolvedScanRoots, []);
  assert.deepStrictEqual(p2.srcRefs, []);
  // own output dir excluded everywhere
  assert.ok(!p2.pathLiterals.some((p) => p.startsWith("studies/phase2_beta_001")));

  // phase1: transitive src refs Gu.Core -> Gu.Base
  const p1 = extractPhaseInputs(root, "studies/phase1_alpha_001/Phase1Alpha.csproj");
  assert.deepStrictEqual(p1.srcRefs, ["src/Gu.Base", "src/Gu.Core"]);

  // phase3: unresolved scanner root reported
  const p3 = extractPhaseInputs(root, "studies/phase3_gamma_001/Phase3Gamma.csproj");
  assert.deepStrictEqual(p3.unresolvedScanRoots, ["dir"]);
});

test("extraction: interpolated consts resolve via fixpoint (scan roots and paths)", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  const fs = require("fs");
  const path = require("path");
  fs.writeFileSync(
    path.join(root, "studies/phase3_gamma_001/Program.cs"),
    [
      'const string Phase12Root = "studies/phase0_matrices_001/output";',
      'const string FermionDir = $"{Phase12Root}/matrices";',
      'const string SpinorPath = $"{FermionDir}/spinor.json";',
      'var files = Directory.GetFiles(FermionDir, "*.json");',
      "",
    ].join("\n")
  );
  const p3 = extractPhaseInputs(root, "studies/phase3_gamma_001/Phase3Gamma.csproj");
  assert.deepStrictEqual(p3.unresolvedScanRoots, []);
  assert.deepStrictEqual(p3.scanRoots, ["studies/phase0_matrices_001/output/matrices"]);
  assert.ok(p3.pathLiterals.includes("studies/phase0_matrices_001/output/matrices/spinor.json"));
  assert.deepStrictEqual(p3.interpolatedPathHints, []);
});

test("extraction: interpolated repo paths are reported as hints", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  const fs = require("fs");
  const path = require("path");
  fs.writeFileSync(
    path.join(root, "studies/phase3_gamma_001/Program.cs"),
    'var p = $"studies/phase{n}_x_001/output/y.json";\n'
  );
  const p3 = extractPhaseInputs(root, "studies/phase3_gamma_001/Phase3Gamma.csproj");
  assert.strictEqual(p3.interpolatedPathHints.length, 1);
});

test("extraction: narrative program docs are excluded as provenance-only mentions", (t) => {
  const root = makeFixtureRepo();
  t.after(() => rmrf(root));
  const fs = require("fs");
  const path = require("path");
  const dir = path.join(root, "studies", "phase9_prov_001");
  fs.mkdirSync(dir, { recursive: true });
  fs.writeFileSync(path.join(dir, "Phase9Prov.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>\n");
  fs.writeFileSync(path.join(dir, "Program.cs"), [
    'const string ReviewBoardSourcePath = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md";',
    'const string RestartPromptSourcePath = "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md";',
    'const string RealInput = "docs/NOTES.md";',
    'System.Console.WriteLine(ReviewBoardSourcePath + RestartPromptSourcePath + RealInput);',
  ].join("\n"));
  const p9 = extractPhaseInputs(root, "studies/phase9_prov_001/Phase9Prov.csproj");
  assert.ok(!p9.pathLiterals.includes("docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md"));
  assert.ok(!p9.pathLiterals.includes("docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md"));
  assert.ok(p9.pathLiterals.includes("docs/NOTES.md"));
});
