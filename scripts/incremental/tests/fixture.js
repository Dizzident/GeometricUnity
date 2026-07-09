"use strict";
// Shared synthetic mini-repo fixture for unit tests. Builds a temp repo with
// three fake phases exercising dep / frozen / scan / src / unresolved-scan
// classification, plus a generator script in the real script's shape.

const fs = require("fs");
const os = require("os");
const path = require("path");

function write(abs, content) {
  fs.mkdirSync(path.dirname(abs), { recursive: true });
  fs.writeFileSync(abs, content);
}

function makeFixtureRepo() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "incr-fixture-"));

  // src projects (transitive: Gu.Core -> Gu.Base)
  write(path.join(root, "src/Gu.Base/Gu.Base.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />\n");
  write(path.join(root, "src/Gu.Base/Base.cs"), "// base v1\n");
  write(
    path.join(root, "src/Gu.Core/Gu.Core.csproj"),
    '<Project Sdk="Microsoft.NET.Sdk">\n  <ItemGroup>\n    <ProjectReference Include="../Gu.Base/Gu.Base.csproj" />\n  </ItemGroup>\n</Project>\n'
  );
  write(path.join(root, "src/Gu.Core/Core.cs"), "// core v1\n");

  // docs input
  write(path.join(root, "docs/NOTES.md"), "notes v1\n");

  // scanned dir
  write(path.join(root, "studies/phase0_matrices_001/output/matrices/m1.json"), '{"a":1}\n');

  // phase1: references src/Gu.Core, writes output
  write(
    path.join(root, "studies/phase1_alpha_001/Phase1Alpha.csproj"),
    '<Project Sdk="Microsoft.NET.Sdk">\n  <ItemGroup>\n    <ProjectReference Include="../../src/Gu.Core/Gu.Core.csproj" />\n  </ItemGroup>\n</Project>\n'
  );
  write(
    path.join(root, "studies/phase1_alpha_001/Program.cs"),
    'const string OutDir = "studies/phase1_alpha_001/output";\n// writes alpha_summary.json\n'
  );
  write(
    path.join(root, "studies/phase1_alpha_001/output/alpha_summary.json"),
    JSON.stringify({ generatedAt: "2026-01-01T00:00:00Z", value: 42 }, null, 2) + "\n"
  );

  // phase2: dep on phase1 output, frozen docs input, scan root via const,
  // Path.Combine scan root
  write(path.join(root, "studies/phase2_beta_001/Phase2Beta.csproj"), '<Project Sdk="Microsoft.NET.Sdk" />\n');
  write(
    path.join(root, "studies/phase2_beta_001/Program.cs"),
    [
      'const string Phase1Path = "studies/phase1_alpha_001/output/alpha_summary.json";',
      'const string DocsPath = "docs/NOTES.md";',
      'const string MatrixDir = "studies/phase0_matrices_001/output/matrices";',
      'const string OutDir = "studies/phase2_beta_001/output";',
      "var files = Directory.EnumerateFiles(MatrixDir);",
      'var more = Directory.GetFiles(Path.Combine(MatrixDir, "sub"), "*.json");',
      "",
    ].join("\n")
  );
  write(
    path.join(root, "studies/phase2_beta_001/output/beta_summary.json"),
    JSON.stringify({ generatedAt: "2026-01-01T00:00:01Z", runtimeSeconds: 1.5, derived: 84 }, null, 2) + "\n"
  );

  // phase3: unresolved scanner root (computed at runtime)
  write(path.join(root, "studies/phase3_gamma_001/Phase3Gamma.csproj"), '<Project Sdk="Microsoft.NET.Sdk" />\n');
  write(
    path.join(root, "studies/phase3_gamma_001/Program.cs"),
    [
      "var dir = FindRoot();",
      "var files = Directory.EnumerateFiles(dir);",
      'const string OutDir = "studies/phase3_gamma_001/output";',
      "",
    ].join("\n")
  );
  write(
    path.join(root, "studies/phase3_gamma_001/output/gamma_summary.json"),
    JSON.stringify({ ok: true }, null, 2) + "\n"
  );

  // generator script in the real shape
  write(
    path.join(root, "scripts/generate_validated_boson_predictions.sh"),
    [
      "#!/usr/bin/env bash",
      "set -euo pipefail",
      "",
      '# comment line',
      'ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"',
      'cd "$ROOT"',
      "",
      "dotnet msbuild scripts/BosonPhasesTraversal.proj -t:Build -m -v:m -nologo",
      "",
      "dotnet run --no-build -c Release --project studies/phase1_alpha_001/Phase1Alpha.csproj",
      "dotnet run --no-build -c Release --project studies/phase2_beta_001/Phase2Beta.csproj",
      "LD_LIBRARY_PATH=native/build dotnet run --no-build -c Release --project studies/phase3_gamma_001/Phase3Gamma.csproj",
      "./scripts/verify_boson_claim_integrity.sh",
      "",
    ].join("\n")
  );

  // native library dir for phase3's env prefix
  write(path.join(root, "native/build/libgu.so"), "fake-native-v1\n");

  return root;
}

function rmrf(p) {
  fs.rmSync(p, { recursive: true, force: true });
}

module.exports = { makeFixtureRepo, rmrf, write };
