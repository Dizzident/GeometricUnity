"use strict";
const test = require("node:test"), assert = require("node:assert/strict");
const { sourceContextIds, sourceContextMenu } = require("../a68-source-context-menu");
const { createSourceTemplateValidator } = require("../a68-source-template-validator");
const { clone, limits, profile, template } = require("./a68-source-template-fixture");
const make = (p, cap, admit, topology = () => {}) => createSourceTemplateValidator(p, cap, admit, topology);
const validator = (p = profile(), admit = () => {}, cap = limits) => {
  const v = make(p, cap, admit); v.validateCaptureDeclaration(p.capture); return v;
};
function rejectCandidate(change, id = "point0/m0_j4") {
  const p = profile(), candidate = template(id, p), v = validator(p); change(candidate);
  assert.throws(() => v.validateTemplate(candidate)); assert.equal(v.snapshot().failed, true);
  assert.throws(() => v.validateTemplate(template("point1", p)), /failed/); assert.throws(() => v.finish(), /failed/);
}
test("capture profile requires every lane dimension and ordered705 inspection policy", () => {
  for (const mutate of [p => delete p.capture, p => delete p.capture.limits.failure, p => delete p.capture.limits.normal.slots,
    p => p.capture.limits.inspection.graphs = -1, p => p.capture.limits.failure.arrays = Number.MAX_SAFE_INTEGER + 1,
    p => p.capture.inspectionPolicy.pop(), p => p.capture.inspectionPolicy.reverse(), p => p.capture.inspectionPolicy[0].copies = -1,
    p => p.capture.inspectionPolicy[0].extra = 0]) {
    const p = profile(); mutate(p); assert.throws(() => make(p, limits, () => {}));
  }
});
test("templates require exactly one accepted actual capture declaration", () => {
  const p = profile(); let calls = 0;
  const unbound = make(p, limits, () => calls++);
  assert.throws(() => unbound.validateTemplate(template("point0", p)), /must be bound/); assert.equal(calls, 0);
  for (const candidate of [null, {}, { limits: p.capture.limits }]) {
    const v = make(p, limits, () => calls++); assert.throws(() => v.validateCaptureDeclaration(candidate));
    assert.throws(() => v.validateCaptureDeclaration(p.capture), /failed/);
  }
  const v = validator(p); assert.equal(v.snapshot().captureBound, true);
  assert.throws(() => v.validateCaptureDeclaration(p.capture), /one capture/); assert.equal(v.snapshot().failed, true);
});
test("capture binding compares all actual values rather than trusting the profile commitment", () => {
  for (const lane of ["normal", "inspection", "failure"]) for (const field of ["graphs", "nodeObjects", "arrays", "slots", "elementCopies"]) {
    const p = profile(), candidate = clone(p.capture); candidate.limits[lane][field]++;
    const v = make(p, limits, () => {}); assert.throws(() => v.validateCaptureDeclaration(candidate), /capture declaration/);
    assert.equal(v.snapshot().captureBound, false); assert.equal(v.snapshot().contexts, 0);
  }
  const p = profile(), v = make(p, limits, () => {}), candidate = clone(p.capture); candidate.inspectionPolicy[0].copies++;
  assert.throws(() => v.validateCaptureDeclaration(candidate), /capture declaration/);
});
test("capture declarations snapshot before mutation and reject getters or swallowed reentry", () => {
  const p = profile(), original = clone(p.capture), v = make(p, limits, () => {});
  p.capture.limits.normal.graphs++; p.capture.inspectionPolicy[0].copies++;
  v.validateCaptureDeclaration(original); original.limits.normal.graphs = 0; v.validateTemplate(template("point0", p));
  let calls = 0; const q = profile(), bad = clone(q.capture), w = make(q, limits, () => {});
  Object.defineProperty(bad, "limits", { enumerable: true, get() { calls++; return q.capture.limits; } });
  assert.throws(() => w.validateCaptureDeclaration(bad)); assert.equal(calls, 0);
  const r = profile(), z = make(r, limits, () => {});
  const proxy = new Proxy(r.capture, { ownKeys(target) { assert.throws(() => z.validateCaptureDeclaration(r.capture), /reentrant/); return Reflect.ownKeys(target); } });
  assert.throws(() => z.validateCaptureDeclaration(proxy), /swallowed/); assert.equal(z.snapshot().failed, true);
});
test("source menu rejects malformed IDs and exposes fresh frozen complete declarations", () => {
  for (const id of [null, 0, "point2", "point0\n", "point0/m0_j00", "point0/m0_j35", "point0/m10_j0", "diagnostic/secondjets", "../point0"])
    assert.throws(() => sourceContextMenu(id), /context ID/);
  const ids = sourceContextIds(), first = sourceContextMenu("point0/m0_j4");
  assert.equal(ids.length, 705); assert.ok(Object.isFrozen(ids)); assert.ok(Object.isFrozen(first.callbacks[0]));
  assert.throws(() => first.marks.push({})); assert.throws(() => first.leaves[0].source = "changed");
  assert.notEqual(first, sourceContextMenu(first.id)); assert.deepEqual(first, sourceContextMenu(first.id));
});
test("complete705 metadata validation retains every291199 mark and never claims scientific proof", () => {
  const p = profile(), admitted = [], v = validator(p, id => { admitted.push(id); });
  for (const id of sourceContextIds()) v.validateTemplate(template(id, p));
  const result = v.finish(); assert.equal(result.contexts, 705); assert.equal(result.marks, 291199); assert.equal(result.leaves, 20316); assert.equal(result.callbacks, 940365);
  assert.deepEqual(admitted, sourceContextIds()); assert.equal(v.snapshot().finished, true); assert.equal(v.snapshot().constructions, 705);
  assert.equal(result.scope.structureOnly, true);
  for (const name of ["numericalReplayComplete", "sourceAuthenticityEstablished", "resourceSufficiencyProved", "totalProcessMemoryProved", "scientificExecutionAuthorized"]) assert.equal(result.scope[name], false);
  assert.throws(() => v.finish(), /completed/); assert.equal(v.snapshot().failed, true);
});
test("complete explicit budget profile rejects omissions duplicates unknown rows and unsafe caps", () => {
  for (const change of [p => p.budgets.pop(), p => p.budgets[1].id = p.budgets[0].id, p => p.budgets[0].id = "point2",
    p => p.budgets[1].checkpointBytes = 1, p => p.budgets[0].checkpointBytes = null, p => p.budgets[0].checkpointBytes = 100000001,
    p => p.budgets[0].resources.trace.marks = 0, p => p.budgets[0].resources.trace.nodes = 2147483648,
    p => p.budgets[0].resources.trace.rationalCharacters = 16385, p => p.budgets[0].resources.contextBytes = Number.MAX_SAFE_INTEGER + 1,
    p => delete p.retention.structured, p => p.retention.geometry = 0, p => p.retention.extra = false]) {
    const p = profile(); change(p); assert.throws(() => validator(p));
  }
  assert.throws(() => createSourceTemplateValidator(profile(), limits), /mandatory/);
});
test("all16 explicit retention combinations preserve mandatory expansion and path indices", () => {
  for (let bits = 0; bits < 16; bits++) {
    const p = profile(); Object.keys(p.retention).forEach((name, i) => p.retention[name] = !!(bits & (1 << i)));
    const v = validator(p);
    for (const id of ["point0", "point0/m0_j4", "diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"]) v.validateTemplate(template(id, p));
    assert.equal(v.snapshot().contexts, 5); assert.equal(v.snapshot().finished, false);
  }
});
test("source identities aliases leaf order and no-placeholder-hash rule cannot be weakened", () => {
  for (const change of [x => x.leaves[0].id += "/other", x => x.leaves[0].source = "observed-producer-export", x => x.leaves[0].degree = 2,
    x => x.leaves[0].sha256 = "0".repeat(64), x => x.leaves.reverse(), x => x.leaves.pop(),
    x => x.leafRoles["baseline/GradientPieces[0]"] = x.leafRoles["baseline/GradientPieces[1]"], x => x.leafRoles.extra = x.leaves[0].id]) rejectCandidate(change);
  rejectCandidate(x => x.leaves[0].source = x.leaves[0].source.replace("p0_", "p1_"), "point0");
  rejectCandidate(x => x.leaves.push({ id: "diagnostic/secondJets/input/X", degree: 1, source: "invented" }), "diagnostic/secondJets");
});
test("mark omission duplicate wrong degree weakened expansion and path remapping fail closed", () => {
  for (const change of [x => x.marks.pop(), x => x.marks[1] = clone(x.marks[0]), x => x.marks[0].degree = 14,
    x => x.marks.find(m => m.expanded).expanded = false, x => x.marks.find(m => !m.expanded).expanded = true,
    x => x.marks.find(m => m.expanded).relativePath = null, x => x.marks.find(m => m.expanded).relativePath = "point0/tensors/other.json",
    x => x.marks.find(m => !m.expanded).relativePath = "unexpected.json", x => x.marks.reverse()]) rejectCandidate(change);
});
test("callback omission replacement type drift and lifecycle or Ward weakening fail closed", () => {
  for (const change of [x => x.callbacks.callbacks.pop(), x => x.callbacks.callbacks[1] = clone(x.callbacks.callbacks[0]),
    x => x.callbacks.callbacks.find(c => c.category === "Tensor").expanded = false,
    x => x.callbacks.callbacks.find(c => c.category === "Check").degree = 0,
    x => x.callbacks.callbacks.find(c => c.category === "Ward").name += "_omitted", x => x.callbacks.id = "point1/m0_j4",
    x => x.callbacks.callbacks.find(c => c.category === "EndGerm").category = "EndPoint"]) rejectCandidate(change);
  rejectCandidate(x => x.callbacks.callbacks.find(c => c.category === "SealPointBackground").name = "fake", "point0");
  rejectCandidate(x => x.callbacks.callbacks.find(c => c.category === "ScalarArray").length = 14, "diagnostic/grade10");
});
test("callback census ordering is not an execution-order claim", () => {
  const p = profile(), v = validator(p), candidate = template("point1/m0_j10", p); candidate.callbacks.callbacks.reverse();
  v.validateTemplate(candidate); assert.equal(v.snapshot().contexts, 1);
});
test("paths checkpoint resource profile and schema must match separately frozen expectations", () => {
  for (const change of [x => x.schema = "other", x => x.graphPath = "../graph.json", x => x.metadataPath = x.graphPath,
    x => x.resources.trace.pairVisits++, x => x.resources.contextBytes++, x => x.pointCheckpoint.bytes--,
    x => x.pointCheckpoint.relativePath += ".other", x => x.pointCheckpoint = null, x => x.authorized = true]) rejectCandidate(change, "point0");
});
test("caller profile and candidate mutation cannot change snapshots during admission", () => {
  const p = profile(), candidate = template("point0", p), v = validator(p, () => {
    candidate.graphPath = "tampered-after-capture"; candidate.marks.length = 0; p.retention.background = true;
  });
  p.budgets[0].resources.fileBytes = 1;
  v.validateTemplate(candidate); assert.equal(candidate.marks.length, 0); assert.equal(v.snapshot().contexts, 1);
});
test("own-data profile and candidate boundaries reject accessors before invoking them", () => {
  let invoked = 0;
  const p = profile(); Object.defineProperty(p.retention, "geometry", { enumerable: true, get() { invoked++; return false; } });
  assert.throws(() => validator(p)); assert.equal(invoked, 0);
  const q = profile(), candidate = template("point0", q), v = validator(q);
  Object.defineProperty(candidate.marks[0], "expanded", { enumerable: true, get() { invoked++; return false; } });
  assert.throws(() => v.validateTemplate(candidate)); assert.equal(invoked, 0); assert.equal(v.snapshot().failed, true);
});
test("holes symbols nondata values cycles and noncanonical metadata cannot enter templates", () => {
  for (const change of [x => delete x.marks[0], x => x.marks.extra = true, x => x[Symbol("extra")] = 1,
    x => x.leaves[0].source = "non-ASCII-λ", x => x.resources.trace.nodes = -0, x => x.loop = x,
    x => x.toJSON = () => ({})]) rejectCandidate(change);
});
test("failed construction admission precedes expected-menu construction and cannot be retried", () => {
  const p = profile(), v = validator(p, () => { throw new Error("manufactured construction cap"); });
  assert.throws(() => v.validateTemplate(template("point0", p)), /construction cap/);
  assert.equal(v.snapshot().contexts, 0); assert.equal(v.snapshot().constructions, 0); assert.equal(v.snapshot().failed, true);
  assert.throws(() => v.validateTemplate(template("point1", p)), /failed/);
});
test("swallowed reentrant validation or finish poisons the outer admission transaction", () => {
  for (const finish of [false, true]) {
    const p = profile(); let v; v = validator(p, () => assert.throws(() => finish ? v.finish() : v.validateTemplate(template("point1", p)), /reentrant/));
    assert.throws(() => v.validateTemplate(template("point0", p)), /reentry/);
    assert.equal(v.snapshot().contexts, 0); assert.equal(v.snapshot().constructions, 0); assert.equal(v.snapshot().failed, true);
  }
});
test("snapshot ceilings are cumulative across profile and contexts with no admission after exhaustion", () => {
  const p = profile(), probe = validator(p); probe.validateTemplate(template("point0", p)); const consumed = probe.snapshot().metadata;
  for (const name of ["nodes", "arraySlots", "stringCharacters"]) {
    let admissions = 0; const v = validator(p, () => { admissions++; }, { ...limits, [name]: consumed[name] });
    v.validateTemplate(template("point0", p)); assert.equal(admissions, 1);
    assert.throws(() => v.validateTemplate(template("point1", p)), /quota|ceiling/);
    assert.equal(admissions, 1); assert.equal(v.snapshot().contexts, 1); assert.equal(v.snapshot().failed, true);
  }
  assert.throws(() => validator(p, () => {}, { ...limits, maxDepth: 2 }), /depth/);
});
test("duplicate context and premature full-run completion poison without accepting repairs", () => {
  const p = profile(); let calls = 0; const v = validator(p, () => { calls++; }); v.validateTemplate(template("point0", p));
  assert.throws(() => v.validateTemplate(template("point0", p)), /one declared/); assert.equal(calls, 1); assert.equal(v.snapshot().failed, true);
  const incomplete = validator(p); incomplete.validateTemplate(template("point0", p)); assert.throws(() => incomplete.finish(), /all705/);
  assert.throws(() => incomplete.validateTemplate(template("point1", p)), /failed/);
});
test("apparently positive caps still reject a mark census larger than the frozen declaration budget", () => {
  const p = profile(); p.budgets[0].resources.trace.marks = 1; const v = make(p, limits, () => {});
  assert.throws(() => v.validateCaptureDeclaration(p.capture), /topology nodes\/marks/); assert.equal(v.snapshot().failed, true);
});
test("an async or value-returning admission hook cannot be mistaken for completed synchronous admission", () => {
  const p = profile();
  for (const admit of [async () => {}, () => true, () => ({ then() {} })]) {
    const v = validator(p, admit); assert.throws(() => v.validateTemplate(template("point0", p)), /synchronous void/);
    assert.equal(v.snapshot().constructions, 0); assert.equal(v.snapshot().contexts, 0); assert.equal(v.snapshot().failed, true);
  }
});

test("topology planning requires a separate mandatory hook and exact limits pass without mutation", () => {
  const { deriveProspectiveCaptureRequirements } = require("../a68-context-topology-envelope");
  const p = profile(), report = deriveProspectiveCaptureRequirements(p.capture.inspectionPolicy);
  p.capture.limits = clone(report.requirements);
  for (const b of p.budgets) {
    const shape = report.contexts.find(c => c.id === b.id).shape;
    b.resources.trace.nodes = shape.nodes; b.resources.trace.marks = shape.marks;
  }
  assert.throws(() => createSourceTemplateValidator(p, limits, () => {}), /mandatory.*topology/);
  let calls = 0;
  const v = make(p, limits, () => {}, descriptor => {
    calls++; assert.equal(descriptor.operation, "prospective-tensor-topology-v1");
    assert.equal(descriptor.contexts, 705); assert.equal(Object.keys(descriptor.sourceRevisions).length, 23);
    assert.ok(Object.isFrozen(descriptor) && Object.isFrozen(descriptor.profile.budgets[0].resources.trace));
    assert.equal(v.snapshot().topologyChecked, false); assert.equal(v.snapshot().captureBound, false);
  });
  const before = clone(p); v.validateCaptureDeclaration(p.capture);
  assert.deepEqual(p, before); assert.equal(calls, 1); assert.equal(v.snapshot().topologyChecked, true);
  assert.equal(v.snapshot().scope.sourceAuthenticityEstablished, false);
  assert.equal(v.snapshot().scope.resourceSufficiencyProved, false);
  assert.throws(() => v.validateCaptureDeclaration(p.capture), /one capture/); assert.equal(calls, 1);
});

test("one-short node or mark ceilings fail during binding for every context family", () => {
  const { deriveProspectiveContextTopology } = require("../a68-context-topology-envelope");
  for (const id of ["point0", "point0/m5_j0", "point0/m0_j4", "diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"])
    for (const field of ["nodes", "marks"]) {
      const p = profile(); p.budgets.find(b => b.id === id).resources.trace[field] = deriveProspectiveContextTopology(id).shape[field] - 1;
      const v = make(p, limits, () => {});
      assert.throws(() => v.validateCaptureDeclaration(p.capture), /topology nodes\/marks/);
      assert.equal(v.snapshot().captureBound, false); assert.equal(v.snapshot().topologyChecked, false);
      assert.throws(() => v.validateCaptureDeclaration(profile().capture), /failed/);
    }
});

test("every one-short capture lane dimension rejects even when peer and independent profiles agree", () => {
  const { deriveProspectiveCaptureRequirements } = require("../a68-context-topology-envelope");
  const base = profile(); base.capture.inspectionPolicy[0].copies = 1;
  base.capture.limits = clone(deriveProspectiveCaptureRequirements(base.capture.inspectionPolicy).requirements);
  for (const lane of ["normal", "inspection", "failure"]) for (const field of ["graphs", "nodeObjects", "arrays", "slots", "elementCopies"]) {
    const p = clone(base); p.capture.limits[lane][field]--;
    const v = make(p, limits, () => {});
    assert.throws(() => v.validateCaptureDeclaration(p.capture), new RegExp("capture " + lane + "/" + field));
    assert.equal(v.snapshot().failed, true); assert.equal(v.snapshot().captureBound, false);
  }
});

test("topology admission throw value Promise and swallowed reentry poison before binding", () => {
  for (const admit of [() => { throw new Error("planner quota"); }, () => true, async () => {}]) {
    const p = profile(), v = make(p, limits, () => {}, admit);
    assert.throws(() => v.validateCaptureDeclaration(p.capture), /planner quota|synchronous void/);
    assert.equal(v.snapshot().failed, true); assert.equal(v.snapshot().captureBound, false);
  }
  for (const action of [v => v.finish(), (v, p) => v.validateCaptureDeclaration(p.capture), (v, p) => v.validateTemplate(template("point0", p))]) {
    const p = profile(); let v;
    v = make(p, limits, () => {}, () => assert.throws(() => action(v, p), /reentrant/));
    assert.throws(() => v.validateCaptureDeclaration(p.capture), /topology admission reentry/);
    assert.equal(v.snapshot().failed, true); assert.equal(v.snapshot().topologyChecked, false);
  }
});

test("planning sees the frozen profile even if its callback mutates caller caps and policy", () => {
  const p = profile(), declaration = clone(p.capture), v = make(p, limits, () => {}, () => {
    p.capture.limits.normal.graphs = 0; p.capture.inspectionPolicy[0].copies = 100;
    p.budgets[0].resources.trace.nodes = 1;
  });
  v.validateCaptureDeclaration(declaration); assert.equal(v.snapshot().topologyChecked, true);
});

test("topology report cannot escape cumulative snapshot quotas after profile and declaration fit", () => {
  const { snapshotCanonicalMetadata } = require("../a68-source-orchestration");
  const p = profile(), initial = make(p, limits, () => {}).snapshot().metadata;
  const declaration = snapshotCanonicalMetadata(p.capture, limits).usage;
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    let admitted = 0;
    const cap = { ...limits, [field]: initial[field] + declaration[field] + 1 };
    const v = make(p, cap, () => {}, () => { admitted++; });
    assert.throws(() => v.validateCaptureDeclaration(p.capture), /quota|ceiling/);
    assert.equal(admitted, 1); assert.equal(v.snapshot().failed, true);
    assert.equal(v.snapshot().captureBound, false); assert.equal(v.snapshot().topologyChecked, false);
    assert.throws(() => v.validateCaptureDeclaration(p.capture), /failed/);
  }
});
