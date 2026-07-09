"use strict";
// Central configuration for fail-closed content-hash incremental validation.
// Binding design: docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md,
// "PERFORMANCE EXPLORATION OUTCOME (2026-07-09)".

const path = require("path");

// Bump whenever fingerprint composition, canonicalization, or manifest layout
// changes. A schema mismatch fails closed: every phase runs.
const SCHEMA_VERSION = 1;

// THE single declared volatile-key list. These keys are stripped at ANY
// nesting depth before hashing JSON outputs. Canonicalization is used only
// for hashing -- files on disk are never rewritten.
const VOLATILE_KEYS = Object.freeze([
  "generatedAt",
  "runtimeSeconds",
  "gpuScanSeconds",
  "slqHessianVectorSeconds",
  "fullJointGradientSeconds",
  "fullThetaGradientSeconds",
  "estimatedCpuFullScanSeconds",
  "cpuParitySeconds",
]);

// Phases that are NEVER skipped, kept as data (not scattered conditionals):
// - 204, 205, 253, 279, 281, 295, 296: the 7 whole-repo text scanners
// - 101, 202, 212, 216, 217, 158, 219: tail claim-boundary gates
// - 151, 156: they read the generator script itself
// The claim-integrity script always runs too (it is a structural step of the
// driver, not a phase, so it is not listed here).
const ALWAYS_RUN_PHASES = Object.freeze([
  204, 205, 253, 279, 281, 295, 296,
  101, 202, 212, 216, 217, 158, 219,
  151, 156,
]);

// Committed manifest location (repo-relative).
const MANIFEST_RELPATH = "scripts/boson_incremental_manifest.json";

// Per-pass skip reports (repo-relative directory, gitignored).
const SKIP_REPORT_DIR_RELPATH = "scripts/incremental/skip_reports";

// Directory names excluded from every tree hash and from read-set capture.
const TREE_EXCLUDE_DIR_NAMES = Object.freeze([
  "obj", "bin", ".git", "node_modules", ".vs", ".idea", ".vscode",
]);

// Read-set capture: keep only repo-relative paths; drop these prefixes.
const READSET_EXCLUDE_RELPREFIXES = Object.freeze([
  "obj/", "bin/", ".git/",
]);
// Any recorded path containing these segments is dropped as build noise.
const READSET_EXCLUDE_SEGMENTS = Object.freeze(["/obj/", "/bin/"]);

// Native library inputs for phases invoked with LD_LIBRARY_PATH=native/build.
const NATIVE_BUILD_RELDIR = "native/build";

// Repo-root build-configuration files read by EVERY `dotnet run` invocation
// (confirmed by read-set capture: dotnet run --no-build re-evaluates MSBuild
// and reads Directory.Build.props). A change to any of these can change what
// the phases execute, so they are frozen components of every fingerprint.
const GLOBAL_INPUT_RELPATHS = Object.freeze([
  "Directory.Build.props",
  "Directory.Build.targets",
  "Directory.Packages.props",
  "global.json",
  "nuget.config",
  "NuGet.config",
]);

function repoRootFromHere() {
  // scripts/incremental/config.js -> repo root is two levels up.
  return path.resolve(__dirname, "..", "..");
}

module.exports = {
  SCHEMA_VERSION,
  VOLATILE_KEYS,
  ALWAYS_RUN_PHASES,
  MANIFEST_RELPATH,
  SKIP_REPORT_DIR_RELPATH,
  TREE_EXCLUDE_DIR_NAMES,
  READSET_EXCLUDE_RELPREFIXES,
  READSET_EXCLUDE_SEGMENTS,
  NATIVE_BUILD_RELDIR,
  GLOBAL_INPUT_RELPATHS,
  repoRootFromHere,
};
