import { spawnSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDir, "..");
const cliProject = path.join(repoRoot, "apps", "Gu.Cli", "Gu.Cli.csproj");
const defaultCampaignSpec = path.join(
  repoRoot,
  "studies",
  "phase5_su2_branch_refinement_env_validation",
  "config",
  "campaign.json",
);
const timestamp = new Date().toISOString().replace(/[-:]/g, "").replace(/\.\d{3}Z$/, "Z");

const campaignSpec = process.argv[2] ?? defaultCampaignSpec;
const outRoot = process.argv[3] ?? path.join(repoRoot, "reports", "phase5_gap_closure_batch", timestamp);
const artifactsDir = path.join(outRoot, "campaign_artifacts");
const logDir = path.join(outRoot, "logs");
const summaryMd = path.join(outRoot, "summary.md");

const testProjects = [
  path.join(repoRoot, "tests", "Gu.Artifacts.Tests", "Gu.Artifacts.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase3.Backgrounds.Tests", "Gu.Phase3.Backgrounds.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase3.Spectra.Tests", "Gu.Phase3.Spectra.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.BranchIndependence.Tests", "Gu.Phase5.BranchIndependence.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.Convergence.Tests", "Gu.Phase5.Convergence.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.Dossiers.Tests", "Gu.Phase5.Dossiers.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.Environments.Tests", "Gu.Phase5.Environments.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.Falsification.Tests", "Gu.Phase5.Falsification.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.QuantitativeValidation.Tests", "Gu.Phase5.QuantitativeValidation.Tests.csproj"),
  path.join(repoRoot, "tests", "Gu.Phase5.Reporting.Tests", "Gu.Phase5.Reporting.Tests.csproj"),
];

const requiredArtifacts = [
  "branch/branch_robustness_record.json",
  "convergence/refinement_study_result.json",
  "quantitative/consistency_scorecard.json",
  "falsification/falsifier_summary.json",
  "dossiers/phase5_validation_dossier.json",
  "dossiers/validation_dossier.json",
  "dossiers/study_manifest.json",
  "reports/phase5_report.json",
  "reports/phase5_report.md",
];

fs.mkdirSync(logDir, { recursive: true });
fs.mkdirSync(artifactsDir, { recursive: true });

if (!fs.existsSync(campaignSpec)) {
  console.error(`Campaign spec not found: ${campaignSpec}`);
  process.exit(1);
}

function combinedOutput(result) {
  return `${result.stdout ?? ""}${result.stderr ?? ""}`;
}

function runAndCapture(name, command, args) {
  const logFile = path.join(logDir, `${name}.log`);
  const tempLog = path.join(os.tmpdir(), `phase5-gap-closure-${name}-${process.pid}.log`);
  console.log("");
  console.log(`=== ${name} ===`);

  const result = spawnSync(command, args, {
    cwd: repoRoot,
    encoding: "utf8",
    stdio: ["ignore", "pipe", "pipe"],
  });

  const output = combinedOutput(result);
  fs.writeFileSync(tempLog, output);
  fs.copyFileSync(tempLog, logFile);
  fs.rmSync(tempLog, { force: true });

  if (result.status === 0) {
    process.stdout.write(output);
    return;
  }

  process.stderr.write(output);
  process.exit(result.status ?? 1);
}

runAndCapture("build", "dotnet", ["build", path.join(repoRoot, "GeometricUnity.slnx"), "-c", "Release", "-nologo"]);

for (const project of testProjects) {
  const projectName = path.basename(path.dirname(project));
  runAndCapture(`test_${projectName}`, "dotnet", ["test", project, "--no-build", "-c", "Release", "-nologo"]);
}

runAndCapture("run_phase5_campaign", "dotnet", [
  "run",
  "--project",
  cliProject,
  "-c",
  "Release",
  "--no-build",
  "--",
  "run-phase5-campaign",
  "--spec",
  campaignSpec,
  "--out-dir",
  artifactsDir,
]);

console.log("");
console.log("=== verify artifacts ===");
const verified = [];
for (const rel of requiredArtifacts) {
  const full = path.join(artifactsDir, rel);
  if (!fs.existsSync(full)) {
    console.error(`Missing required artifact: ${full}`);
    process.exit(1);
  }

  const message = `verified ${full}`;
  verified.push(message);
  console.log(message);
}
fs.writeFileSync(path.join(logDir, "verify_artifacts.log"), `${verified.join("\n")}\n`);

const summaryLines = [
  "# Phase V Gap Closure Batch Summary",
  "",
  `- Timestamp (UTC): ${timestamp}`,
  `- Repository: ${repoRoot}`,
  `- Campaign spec: ${campaignSpec}`,
  `- Output root: ${outRoot}`,
  `- CLI project: ${cliProject}`,
  `- Test projects run: ${testProjects.length}`,
  `- Required artifacts verified: ${requiredArtifacts.length}`,
  "",
  "## Logs",
  "",
  `- Build log: ${path.join(logDir, "build.log")}`,
  `- Campaign log: ${path.join(logDir, "run_phase5_campaign.log")}`,
  `- Test logs directory: ${logDir}`,
  "",
  "## Campaign Artifacts",
  "",
  `- Artifacts root: ${artifactsDir}`,
  "",
  ...requiredArtifacts.map((rel) => `- ${path.join(artifactsDir, rel)}`),
  "",
  "## Evaluation Prompt",
  "",
  "Use:",
  "",
  `\`${path.join(repoRoot, "PHASE_6_EVALUATION_PROMPT.md")}\``,
  "",
  "with these concrete values:",
  "",
  `- batch summary: \`${summaryMd}\``,
  `- artifacts root: \`${artifactsDir}\``,
  `- logs root: \`${logDir}\``,
  `- gap handoff: \`${path.join(repoRoot, "GAPS_P5_01.md")}\``,
  `- open issues: \`${path.join(repoRoot, "PHASE_5_OPEN_ISSUES.md")}\``,
  "",
];
fs.writeFileSync(summaryMd, summaryLines.join("\n"));

console.log("");
console.log("Batch complete.");
console.log(`Summary: ${summaryMd}`);
console.log(`Artifacts: ${artifactsDir}`);
console.log(`Logs: ${logDir}`);
