"use strict";

const fs = require("fs");
const path = require("path");

const MANDATORY_RULING_IDS = Object.freeze([
  "O4-F1-INVARIANT-RAYS",
  "O4-F1-COLLECTIVE-COORDINATE",
  "O4-F1-FP-NORMALIZATION",
  "O4-F2-POSITIVE-MODE-IR",
  "O4-F3-THETA-HAAR",
  "O4-F4-SADDLE-BACKGROUNDS",
  "O4-E1-P447-SOFT-FLOOR",
  "O4-E2-P453-UNIFORM-LADDER",
  "O4-P455-ZERO-MODE",
  "O4-P455-SB-MODEL",
  "O4-C1-COMPACT-REAL-FORM",
  "O4-C2-YHALF-BOOKKEEPING",
  "O4-C3-WS3-MPROBE-SCOPE",
]);

function pointerEscape(value) {
  return String(value).replace(/~/g, "~0").replace(/\//g, "~1");
}

function pointerUnescape(value) {
  return String(value).replace(/~1/g, "/").replace(/~0/g, "~");
}

function getPointer(root, pointer) {
  if (pointer === "") return { found: true, value: root };
  if (typeof pointer !== "string" || !pointer.startsWith("/"))
    throw new Error(`invalid JSON pointer: ${pointer}`);
  let current = root;
  for (const raw of pointer.slice(1).split("/")) {
    const key = pointerUnescape(raw);
    if (current === null || typeof current !== "object" || !(key in current))
      return { found: false, value: undefined };
    current = current[key];
  }
  return { found: true, value: current };
}

function findNamedPointers(root, propertyName) {
  const found = [];
  function visit(value, parts) {
    if (value === null || typeof value !== "object") return;
    if (Array.isArray(value)) {
      value.forEach((item, index) => visit(item, parts.concat(String(index))));
      return;
    }
    for (const [key, child] of Object.entries(value)) {
      const next = parts.concat(key);
      if (key === propertyName)
        found.push({ pointer: "/" + next.map(pointerEscape).join("/"), value: child });
      visit(child, next);
    }
  }
  visit(root, []);
  return found.sort((a, b) => a.pointer.localeCompare(b.pointer));
}

function deepEqual(left, right) {
  return JSON.stringify(left) === JSON.stringify(right);
}

function evaluatePredicate(document, predicate) {
  const actual = getPointer(document, predicate.pointer);
  switch (predicate.op) {
    case "equals":
      return actual.found && deepEqual(actual.value, predicate.value);
    case "exists":
      return actual.found === predicate.value;
    case "array-exact-set": {
      if (!actual.found || !Array.isArray(actual.value) || !Array.isArray(predicate.value)) return false;
      const a = [...actual.value].sort();
      const b = [...predicate.value].sort();
      return deepEqual(a, b) && new Set(actual.value).size === actual.value.length;
    }
    case "array-all-equal":
      return actual.found && Array.isArray(actual.value) && actual.value.length === predicate.count
        && actual.value.every((item) => deepEqual(item, predicate.value));
    default:
      throw new Error(`unknown predicate op: ${predicate.op}`);
  }
}

function loadJsonStrict(file) {
  let text;
  try {
    text = fs.readFileSync(file, "utf8");
  } catch (error) {
    throw new Error(`cannot read ${file}: ${error.message}`);
  }
  try {
    return JSON.parse(text);
  } catch (error) {
    throw new Error(`malformed JSON ${file}: ${error.message}`);
  }
}

function listSummaryFiles(repoRoot) {
  const studiesDir = path.join(repoRoot, "studies");
  const files = [];
  if (!fs.existsSync(studiesDir)) throw new Error(`missing studies directory: ${studiesDir}`);
  for (const phase of fs.readdirSync(studiesDir).sort()) {
    const outputDir = path.join(studiesDir, phase, "output");
    if (!fs.existsSync(outputDir)) continue;
    for (const name of fs.readdirSync(outputDir).sort()) {
      if (name.endsWith("_summary.json")) files.push(path.join(outputDir, name));
    }
  }
  return files;
}

function validateContract(contract) {
  if (contract.schemaVersion !== 1) throw new Error("coverage contract schemaVersion must be 1");
  if (!Array.isArray(contract.reviewItems) || !Array.isArray(contract.entries))
    throw new Error("coverage contract must contain reviewItems and entries arrays");
  const reviewIds = new Set();
  for (const item of contract.reviewItems) {
    if (!item.id || reviewIds.has(item.id)) throw new Error(`duplicate/empty review item: ${item.id}`);
    reviewIds.add(item.id);
  }
  if (!deepEqual([...reviewIds].sort(), [...MANDATORY_RULING_IDS].sort()))
    throw new Error("coverage review item IDs must exactly equal the 13 mandatory O4 ruling IDs");
  const phaseIds = new Set();
  const paths = new Set();
  for (const entry of contract.entries) {
    if (!entry.phaseId || phaseIds.has(entry.phaseId)) throw new Error(`duplicate/empty phaseId: ${entry.phaseId}`);
    if (!entry.summaryPath || paths.has(entry.summaryPath)) throw new Error(`duplicate/empty summaryPath: ${entry.summaryPath}`);
    phaseIds.add(entry.phaseId);
    paths.add(entry.summaryPath);
    if (!Array.isArray(entry.pendingPointers) || entry.pendingPointers.length === 0)
      throw new Error(`${entry.phaseId}: pendingPointers must be nonempty`);
    if (new Set(entry.pendingPointers).size !== entry.pendingPointers.length)
      throw new Error(`${entry.phaseId}: duplicate pending pointer`);
    if (!Array.isArray(entry.predicates)) throw new Error(`${entry.phaseId}: predicates must be an array`);
    if (!Array.isArray(entry.reviewItems)) throw new Error(`${entry.phaseId}: reviewItems must be an array`);
    if (new Set(entry.reviewItems).size !== entry.reviewItems.length)
      throw new Error(`${entry.phaseId}: duplicate review item`);
    for (const id of entry.reviewItems)
      if (!reviewIds.has(id)) throw new Error(`${entry.phaseId}: unknown review item ${id}`);
    if (!["direct-review", "dependent-output", "pending-marker-no-direct-review", "administrative-zero-compute"].includes(entry.disposition))
      throw new Error(`${entry.phaseId}: invalid disposition ${entry.disposition}`);
    if (entry.disposition === "direct-review" && entry.reviewItems.length === 0)
      throw new Error(`${entry.phaseId}: direct-review requires a review item`);
  }
  return contract;
}

function collectCoverage(repoRoot, contract) {
  validateContract(contract);
  const contractByPhase = new Map(contract.entries.map((entry) => [entry.phaseId, entry]));
  const contractByPath = new Map(contract.entries.map((entry) => [entry.summaryPath, entry]));
  const seenIds = new Map();
  const pendingDocuments = [];

  for (const file of listSummaryFiles(repoRoot)) {
    const relPath = path.relative(repoRoot, file).replace(/\\/g, "/");
    const document = loadJsonStrict(file);
    const occurrences = findNamedPointers(document, "physicistReviewPending");
    if (occurrences.length === 0) continue;
    for (const occurrence of occurrences) {
      if (typeof occurrence.value !== "boolean")
        throw new Error(`${relPath}: ${occurrence.pointer} must be boolean`);
    }
    if (!occurrences.some((occurrence) => occurrence.value === true)) continue;
    const phaseId = document.phaseId;
    if (typeof phaseId !== "string" || phaseId.length === 0)
      throw new Error(`${relPath}: pending artifact missing phaseId`);
    if (seenIds.has(phaseId))
      throw new Error(`duplicate pending phaseId ${phaseId}: ${seenIds.get(phaseId)} and ${relPath}`);
    seenIds.set(phaseId, relPath);
    const entry = contractByPhase.get(phaseId);
    if (!entry) throw new Error(`${relPath}: unmapped pending phase ${phaseId}`);
    if (entry.summaryPath !== relPath)
      throw new Error(`${phaseId}: summary path drift; expected ${entry.summaryPath}, observed ${relPath}`);
    const observedPointers = occurrences.map((occurrence) => occurrence.pointer).sort();
    const expectedPointers = [...entry.pendingPointers].sort();
    if (!deepEqual(observedPointers, expectedPointers))
      throw new Error(`${phaseId}: pending pointer drift; expected ${JSON.stringify(expectedPointers)}, observed ${JSON.stringify(observedPointers)}`);
    if (occurrences.some((occurrence) => occurrence.value !== true))
      throw new Error(`${phaseId}: contradictory pending values across authoritative pointers`);
    const failed = entry.predicates.filter((predicate) => !evaluatePredicate(document, predicate));
    if (failed.length > 0)
      throw new Error(`${phaseId}: coverage predicate drift at ${failed.map((p) => `${p.pointer}:${p.op}`).join(", ")}`);
    pendingDocuments.push({
      phaseId,
      subjectKind: String(document.applicationSubjectKind || ""),
      terminalStatus: String(document.terminalStatus || document.verdictKind || ""),
      summaryPath: relPath,
      pendingPointers: observedPointers,
      disposition: entry.disposition,
      reviewItems: [...entry.reviewItems],
      evidencePointers: entry.predicates.map((predicate) => predicate.pointer),
      note: entry.note,
    });
  }

  for (const [summaryPath, entry] of contractByPath) {
    if (!seenIds.has(entry.phaseId))
      throw new Error(`${entry.phaseId}: expected pending artifact absent or no longer pending at ${summaryPath}`);
  }

  return pendingDocuments.sort((a, b) => a.phaseId.localeCompare(b.phaseId));
}

function renderRegister(rows, contract) {
  const itemById = new Map(contract.reviewItems.map((item) => [item.id, item]));
  const lines = [
    "# O4 Unified Conventions Coverage Register (generated)",
    "",
    "Machine-generated from committed summary artifacts and the exact coverage contract",
    "`scripts/o4_register/coverage_contract.json`. Classification uses exact JSON",
    "pointers and typed predicates only; phase-name and prose keyword heuristics are forbidden.",
    "Committed phase artifacts remain byte-identical.",
    "",
    `Total recursively review-pending phase outputs: **${rows.length}**.`,
    "",
    "## Review items",
    "",
  ];
  for (const item of contract.reviewItems) {
    const direct = rows.filter((row) => row.disposition === "direct-review" && row.reviewItems.includes(item.id));
    const dependent = rows.filter((row) => row.disposition === "dependent-output" && row.reviewItems.includes(item.id));
    lines.push(`### ${item.title} (${direct.length} direct / ${dependent.length} dependent)`);
    lines.push("");
    if (direct.length === 0 && dependent.length === 0) {
      lines.push("_none_");
    } else {
      lines.push("| role | phaseId | terminalStatus | artifact | evidence |" );
      lines.push("|------|---------|----------------|----------|----------|" );
      for (const row of direct.concat(dependent)) {
        const role = row.disposition === "direct-review" ? "direct" : "dependent";
        lines.push(`| ${role} | ${row.phaseId} | ${row.terminalStatus} | \`${row.summaryPath}\` | \`${row.evidencePointers.join("`, `")}\` |`);
      }
    }
    lines.push("");
  }
  lines.push("## Other pending dispositions", "");
  lines.push("| disposition | phaseId | terminalStatus | artifact | pending evidence | note |" );
  lines.push("|-------------|---------|----------------|----------|------------------|------|" );
  for (const row of rows.filter((row) => row.disposition !== "direct-review" && row.disposition !== "dependent-output"))
    lines.push(`| ${row.disposition} | ${row.phaseId} | ${row.terminalStatus} | \`${row.summaryPath}\` | \`${row.pendingPointers.join("`, `")}\` | ${row.note} |`);
  lines.push("", "---", "", "This register records coverage only. It contains no physicist ruling, does not", "change any pending flag, and cannot authorize sampling or a claim.", "");
  return lines.join("\n");
}

module.exports = {
  MANDATORY_RULING_IDS,
  collectCoverage,
  deepEqual,
  evaluatePredicate,
  findNamedPointers,
  getPointer,
  listSummaryFiles,
  loadJsonStrict,
  renderRegister,
  validateContract,
};
