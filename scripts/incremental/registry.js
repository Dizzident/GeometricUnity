"use strict";
// Registry generator: parses scripts/generate_validated_boson_predictions.sh
// into an ordered registry of steps. FAILS LOUDLY on unparseable lines --
// any non-comment, non-blank line that is not one of the known forms is an
// error, so a new construct in the generator script cannot be silently
// skipped by the incremental driver.

const fs = require("fs");

// Known non-step preamble lines (exact or prefix matched after trim).
const PREAMBLE_PATTERNS = [
  /^#!/,
  /^set -euo pipefail$/,
  /^ROOT="\$\(cd .*\)"$/,
  /^cd "\$ROOT"$/,
];

const RE_ENV_PREFIX = /^((?:[A-Za-z_][A-Za-z0-9_]*=[^\s]*\s+)+)/;
const RE_PHASE = /--project\s+(studies\/phase(\d+)_[^\s]+\.csproj)/;

// Parse the generator script text. Returns:
//   { buildSteps: [line...], steps: [step...], errors: [ {lineNo, line} ] }
// step = { kind: "phase", index, phase, project, invocation, env } or
//        { kind: "integrity", index, invocation }
function parseGeneratorScript(text) {
  const buildSteps = [];
  const steps = [];
  const errors = [];
  const lines = text.split("\n");
  for (let i = 0; i < lines.length; i++) {
    const raw = lines[i];
    const line = raw.trim();
    if (line === "" || line.startsWith("#")) continue;
    if (PREAMBLE_PATTERNS.some((re) => re.test(line))) continue;

    if (/^dotnet msbuild\s/.test(line)) {
      buildSteps.push(line);
      continue;
    }

    // Optional env prefix (e.g. LD_LIBRARY_PATH=native/build) then dotnet run.
    const envMatch = line.match(RE_ENV_PREFIX);
    const envPairs = {};
    let rest = line;
    if (envMatch && line.slice(envMatch[1].length).startsWith("dotnet ")) {
      rest = line.slice(envMatch[1].length);
      for (const kv of envMatch[1].trim().split(/\s+/)) {
        const eq = kv.indexOf("=");
        envPairs[kv.slice(0, eq)] = kv.slice(eq + 1);
      }
    }

    if (/^dotnet run\s/.test(rest)) {
      const m = rest.match(RE_PHASE);
      if (!m) {
        errors.push({ lineNo: i + 1, line, reason: "dotnet run line without a parseable studies/phaseNNN_ project" });
        continue;
      }
      steps.push({
        kind: "phase",
        index: steps.length,
        phase: parseInt(m[2], 10),
        project: m[1],
        invocation: line,
        env: envPairs,
      });
      continue;
    }

    if (/^\.\/scripts\/verify_boson_claim_integrity\.sh$/.test(line)) {
      steps.push({ kind: "integrity", index: steps.length, invocation: line });
      continue;
    }

    errors.push({ lineNo: i + 1, line, reason: "unrecognized generator script line" });
  }
  return { buildSteps, steps, errors };
}

function parseGeneratorScriptFile(absPath) {
  return parseGeneratorScript(fs.readFileSync(absPath, "utf8"));
}

// Convenience: throw with a clear message if any line failed to parse.
function requireCleanParse(parsed, scriptPath) {
  if (parsed.errors.length > 0) {
    const detail = parsed.errors
      .map((e) => `  line ${e.lineNo}: ${e.reason}: ${e.line}`)
      .join("\n");
    throw new Error(
      `registry: refusing to proceed -- ${parsed.errors.length} unparseable line(s) in ${scriptPath}:\n${detail}`
    );
  }
  if (parsed.buildSteps.length === 0) {
    throw new Error(`registry: no 'dotnet msbuild' build step found in ${scriptPath}`);
  }
  return parsed;
}

module.exports = { parseGeneratorScript, parseGeneratorScriptFile, requireCleanParse };
