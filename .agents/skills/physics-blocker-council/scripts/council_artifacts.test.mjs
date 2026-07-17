import assert from "node:assert/strict";
import { mkdtemp, readFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { anonymizeMemo, initializeMatrix, initializeRun, validateMemo } from "./council_artifacts.mjs";

function memo() {
  return {
    schemaVersion: 1,
    blockerId: "test-blocker",
    role: "adversarial-reviewer",
    currentTerminal: "blocked",
    evidenceBindings: [{ path: "artifact.json", sha256: "a".repeat(64) }],
    proposedMechanism: "Run a decisive control.",
    derivationOutline: ["Derive the discriminating observable."],
    assumptions: [],
    conventionDependencies: [],
    decisiveTest: "Compare the frozen alternatives.",
    successOutcome: "One alternative survives.",
    failureOutcome: "Both alternatives remain live.",
    counterexamples: ["A degenerate control could erase the distinction."],
    runtimeAndStorageEstimate: "Reduced deterministic workload.",
    gateImpact: "No gate is lifted by this memo.",
    claimBoundary: "No physical-unit claim.",
    promotedPhysicalMassClaimCount: 0,
    humanRulingAuthoredOrInferred: false,
    repositoryWritePerformed: false,
    confidence: 0.5,
    provenance: { provider: "test", model: "test-model", team: "test-team", sessionId: "test-session" },
  };
}

test("validates the frozen memo boundary", () => assert.equal(validateMemo(memo()), true));

test("rejects claim and human-ruling leakage", () => {
  const invalid = memo();
  invalid.promotedPhysicalMassClaimCount = 1;
  invalid.humanRulingAuthoredOrInferred = true;
  assert.throws(() => validateMemo(invalid), /promotedPhysicalMassClaimCount/);
});

test("rejects undeclared memo fields", () => {
  const invalid = memo();
  invalid.unreviewedShortcut = true;
  assert.throws(() => validateMemo(invalid), /unexpected field/);
});

test("anonymization is deterministic and strips provenance", () => {
  const first = anonymizeMemo(memo());
  const second = anonymizeMemo(memo());
  assert.equal(first.proposalId, second.proposalId);
  assert.equal(first.provenance, undefined);
  assert.equal(first.authorLabel, "anonymous-participant");
});

test("initializes a fail-closed adjudication matrix", () => {
  const proposal = anonymizeMemo(memo());
  const matrix = initializeMatrix([proposal]);
  assert.equal(matrix.proposals[0].gateCompliance, "pending");
  assert.equal(matrix.proposals[0].scores.exactEvidenceSupport, null);
});

test("initializes only under tmp with closed boundaries", async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), "physics-council-test-"));
  const runDir = await initializeRun({ blocker: "blocker-1", conductor: "codex", root });
  const manifest = JSON.parse(await readFile(path.join(runDir, "manifest.json"), "utf8"));
  assert.equal(manifest.repositoryWritesAllowedForParticipants, false);
  assert.equal(manifest.recursiveDelegationAllowed, false);
  assert.equal(manifest.promotedPhysicalMassClaimCount, 0);
});

test("refuses a run root outside tmp", async () => {
  await assert.rejects(() => initializeRun({ blocker: "blocker-1", conductor: "codex", root: "/var/lib/council" }), /under \/tmp/);
});
