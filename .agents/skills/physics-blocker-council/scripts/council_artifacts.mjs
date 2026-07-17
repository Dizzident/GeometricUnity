#!/usr/bin/env node

import { createHash, randomUUID } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { pathToFileURL } from "node:url";

const REQUIRED_STRING_FIELDS = [
  "blockerId", "role", "currentTerminal", "proposedMechanism",
  "decisiveTest", "successOutcome", "failureOutcome",
  "runtimeAndStorageEstimate", "gateImpact", "claimBoundary",
];
const REQUIRED_ARRAY_FIELDS = ["derivationOutline", "assumptions", "conventionDependencies", "counterexamples"];
const PROVENANCE_FIELDS = new Set(["provider", "model", "team", "sessionId"]);
const MEMO_FIELDS = new Set([
  "schemaVersion", ...REQUIRED_STRING_FIELDS, ...REQUIRED_ARRAY_FIELDS,
  "evidenceBindings", "promotedPhysicalMassClaimCount",
  "humanRulingAuthoredOrInferred", "repositoryWritePerformed", "confidence", "provenance",
]);

function canonical(value) {
  if (Array.isArray(value)) return `[${value.map(canonical).join(",")}]`;
  if (value && typeof value === "object") {
    return `{${Object.keys(value).sort().map(key => `${JSON.stringify(key)}:${canonical(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function fail(errors) {
  const error = new Error(errors.join("\n"));
  error.validationErrors = errors;
  throw error;
}

export function validateMemo(memo) {
  const errors = [];
  if (!memo || typeof memo !== "object" || Array.isArray(memo)) fail(["memo must be an object"]);
  if (memo.schemaVersion !== 1) errors.push("schemaVersion must equal 1");
  for (const field of Object.keys(memo)) if (!MEMO_FIELDS.has(field)) errors.push(`unexpected field: ${field}`);
  for (const field of REQUIRED_STRING_FIELDS) {
    if (typeof memo[field] !== "string" || memo[field].trim() === "") errors.push(`${field} must be a non-empty string`);
  }
  for (const field of REQUIRED_ARRAY_FIELDS) {
    if (!Array.isArray(memo[field])) errors.push(`${field} must be an array`);
    else if (memo[field].some(x => typeof x !== "string")) errors.push(`${field} must contain only strings`);
  }
  for (const field of ["derivationOutline", "counterexamples"]) {
    if (Array.isArray(memo[field]) && (memo[field].length === 0 || memo[field].some(x => typeof x !== "string" || x.trim() === ""))) {
      errors.push(`${field} must contain at least one non-empty string`);
    }
  }
  if (!Array.isArray(memo.evidenceBindings) || memo.evidenceBindings.length === 0) {
    errors.push("evidenceBindings must contain at least one exact binding");
  } else {
    memo.evidenceBindings.forEach((binding, index) => {
      if (binding && Object.keys(binding).some(key => !new Set(["path", "sha256"]).has(key))) errors.push(`evidenceBindings[${index}] has an unexpected field`);
      if (!binding || typeof binding.path !== "string" || binding.path.trim() === "") errors.push(`evidenceBindings[${index}].path is invalid`);
      if (!binding || typeof binding.sha256 !== "string" || !/^[a-f0-9]{64}$/.test(binding.sha256)) errors.push(`evidenceBindings[${index}].sha256 is invalid`);
    });
  }
  if (memo.promotedPhysicalMassClaimCount !== 0) errors.push("promotedPhysicalMassClaimCount must equal 0");
  if (memo.humanRulingAuthoredOrInferred !== false) errors.push("humanRulingAuthoredOrInferred must be false");
  if (memo.repositoryWritePerformed !== false) errors.push("repositoryWritePerformed must be false");
  if (typeof memo.confidence !== "number" || memo.confidence < 0 || memo.confidence > 1) errors.push("confidence must be between 0 and 1");
  if (!memo.provenance || typeof memo.provenance !== "object") {
    errors.push("provenance must be an object");
  } else {
    for (const field of Object.keys(memo.provenance)) if (!PROVENANCE_FIELDS.has(field)) errors.push(`unexpected provenance field: ${field}`);
    for (const field of PROVENANCE_FIELDS) {
      if (typeof memo.provenance[field] !== "string" || memo.provenance[field].trim() === "") errors.push(`provenance.${field} must be a non-empty string`);
    }
  }
  if (errors.length > 0) fail(errors);
  return true;
}

export function anonymizeMemo(memo) {
  validateMemo(memo);
  const stripped = structuredClone(memo);
  delete stripped.provenance;
  stripped.authorLabel = "anonymous-participant";
  stripped.proposalId = `proposal-${sha256(canonical(stripped)).slice(0, 16)}`;
  return stripped;
}

export function initializeMatrix(proposals) {
  if (!Array.isArray(proposals) || proposals.length === 0) fail(["at least one anonymized proposal is required"]);
  return {
    schemaVersion: 1,
    proposals: proposals.map((proposal, index) => {
      if (!proposal || typeof proposal.proposalId !== "string" || typeof proposal.blockerId !== "string" || proposal.provenance !== undefined) {
        fail([`proposal ${index} is not anonymized`]);
      }
      return {
        proposalId: proposal.proposalId,
        blockerId: proposal.blockerId,
        scores: {
          exactEvidenceSupport: null,
          mathematicalConsistency: null,
          falsifiability: null,
          expectedInformationGain: null,
          computationalFeasibility: null,
          conventionRobustness: null,
        },
        gateCompliance: "pending",
        classification: "pending",
        strongestObjection: "",
      };
    }),
  };
}

function parseArgs(argv) {
  const positional = [];
  const options = {};
  for (let i = 0; i < argv.length; i += 1) {
    const token = argv[i];
    if (!token.startsWith("--")) positional.push(token);
    else {
      const key = token.slice(2);
      const value = argv[i + 1];
      if (!value || value.startsWith("--")) fail([`missing value for --${key}`]);
      options[key] = value;
      i += 1;
    }
  }
  return { positional, options };
}

export async function initializeRun({ blocker, conductor, root = "/tmp/physics-blocker-council" }) {
  if (!blocker || !/^[A-Za-z0-9._-]+$/.test(blocker)) fail(["--blocker must use letters, digits, dot, underscore, or hyphen"]);
  if (!new Set(["codex", "claude"]).has(conductor)) fail(["--conductor must be codex or claude"]);
  const resolvedRoot = path.resolve(root);
  if (resolvedRoot !== "/tmp" && !resolvedRoot.startsWith("/tmp/")) fail(["run root must be under /tmp"]);
  const runId = `${new Date().toISOString().replace(/[:.]/g, "-")}-${randomUUID().slice(0, 8)}`;
  const runDir = path.join(resolvedRoot, runId);
  await Promise.all(["evidence", "memos", "exchange", "adjudication", "rejected"].map(dir => mkdir(path.join(runDir, dir), { recursive: true })));
  const manifest = {
    schemaVersion: 1,
    runId,
    blockerId: blocker,
    conductor,
    status: "evidence-freeze-pending",
    createdAt: new Date().toISOString(),
    repositoryWritesAllowedForParticipants: false,
    recursiveDelegationAllowed: false,
    promotedPhysicalMassClaimCount: 0,
    humanRulingMayBeAuthoredOrInferred: false,
  };
  await writeFile(path.join(runDir, "manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`, { flag: "wx" });
  return runDir;
}

async function main(argv) {
  const [command, ...rest] = argv;
  const { positional, options } = parseArgs(rest);
  if (command === "init") {
    const runDir = await initializeRun({ blocker: options.blocker, conductor: options.conductor, root: options.root });
    process.stdout.write(`${runDir}\n`);
    return;
  }
  if (command === "validate-memo") {
    if (positional.length !== 1) fail(["usage: validate-memo MEMO.json"]);
    validateMemo(JSON.parse(await readFile(positional[0], "utf8")));
    process.stdout.write("memo-valid\n");
    return;
  }
  if (command === "anonymize") {
    if (positional.length !== 1 || !options.output) fail(["usage: anonymize MEMO.json --output FILE.json"]);
    const anonymized = anonymizeMemo(JSON.parse(await readFile(positional[0], "utf8")));
    await writeFile(options.output, `${JSON.stringify(anonymized, null, 2)}\n`, { flag: "wx" });
    process.stdout.write(`${options.output}\n`);
    return;
  }
  if (command === "init-matrix") {
    if (positional.length === 0 || !options.output) fail(["usage: init-matrix MEMO.json... --output FILE.json"]);
    const proposals = await Promise.all(positional.map(async file => JSON.parse(await readFile(file, "utf8"))));
    await writeFile(options.output, `${JSON.stringify(initializeMatrix(proposals), null, 2)}\n`, { flag: "wx" });
    process.stdout.write(`${options.output}\n`);
    return;
  }
  fail(["commands: init, validate-memo, anonymize, init-matrix"]);
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  main(process.argv.slice(2)).catch(error => {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = 1;
  });
}
