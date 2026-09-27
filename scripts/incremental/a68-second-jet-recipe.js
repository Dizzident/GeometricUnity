"use strict";

// Metadata-only transcription of MixedWardControls.SymmetricSecondJets and
// MixedWard.SymmetricSecondJetImage. Never evaluate a scientific coefficient,
// infer a zero result, or replace a full tensor check by a sampled coefficient.
// The fixed controls are distinct from the conditional arbitrary-eta argument
// below. Complete enclosing lifecycle/source/resource closure remains external.
const { MixedRecipe } = require("./a68-mixed-recipe");
const SCHEMA = "phase627-symmetric-second-jet-recipe-v1";
const need = (ok, why) => { if (!ok) throw new Error("A68 second-jet recipe: " + why); };
const integer = (value, lo, hi) => Number.isSafeInteger(value) && !Object.is(value, -0) && value >= lo && value <= hi;
const freeze = value => {
  if (value && typeof value === "object" && !Object.isFrozen(value)) {
    Object.values(value).forEach(freeze); Object.freeze(value);
  }
  return value;
};

function secondJetMenu() {
  const out = [];
  for (let coefficient = 0; coefficient < 2; coefficient++)
    for (let first = 0; first < 14; first++) for (let second = first; second < 14; second++)
      for (let route = 0; route < 2; route++) out.push({
        name: `eta${coefficient}_second_${first}_${second}_route${route}`,
        coefficient, first, second, route, diagonal: first === second
      });
  return freeze(out);
}
function secondJetMarkMenu() {
  return freeze(secondJetMenu().map(item => ({ name: "secondJet/" + item.name, degree: 2, requiredExpanded: null })));
}
function secondJetDiagnosticMenu() {
  const names = secondJetMenu().map(item => item.name);
  return freeze({ id: "secondJets", tensors: [], scalarArrays: [], originalActions: [], wardActions: [], accelerations: [],
    secondJets: names, checks: [...names], backgrounds: 0, germs: 0, variationRows: 0 });
}

function buildSymmetricSecondJetImage(recipe, first, second, coefficient, oracle = false) {
  need(recipe instanceof MixedRecipe, "MixedRecipe session required");
  need(integer(first, 0, 13) && integer(second, first, 13), "ordered coordinate pair 0 <= first <= second < 14");
  need(typeof oracle === "boolean", "explicit boolean route");
  const degree = recipe.TensorDegree(coefficient);
  need(degree === 0 || degree === -1, "coefficient must be a local zero-form");
  // Local(coefficient,0) in C# also requires canonical zero-frequency and
  // H-anti-Hermitian coefficients. Those numerical domain checks are external,
  // never asserted by this typed metadata macro. In the fixed controls below
  // they are retained for EVERY source invocation as independent obligations.
  // Both literal and word-product routes have the SAME symbolic W operation;
  // oracle changes the numerical implementation, not a frozen primitive field.
  const left = recipe.P(recipe.P(recipe.Unit(1 << first, 0, "1"), recipe.Unit(1 << second, 0, "1"), "W"), coefficient, "W");
  // Do not double a diagonal coefficient, cache coordinate units, simplify the
  // wedge to zero, or merge equal-valued literal/word route objects.
  return first === second ? left : recipe.Sum(left,
    recipe.P(recipe.P(recipe.Unit(1 << second, 0, "1"), recipe.Unit(1 << first, 0, "1"), "W"), coefficient, "W"));
}

function secondJetProofNotes() {
  return freeze({
    status: "conditional algebraic derivation; not inferred from the finite controls",
    assumptions: [
      "A fixed holonomic native coordinate chart and ordinary exterior derivative d with commuting mixed partials (C2 regularity, or formal polynomial jets).",
      "An associative coefficient algebra over characteristic zero; the coordinate one-forms carry Clifford identity and use the exterior wedge product.",
      "The full symmetric ordinary Hessian eta_mu,nu = eta_nu,mu is retained before passing to its alternating exterior image.",
      "For the source real-domain claim, eta0 and all jet coefficients are H-anti-Hermitian in one fixed real basis; this restriction is not needed for exterior cancellation itself."
    ],
    steps: [
      "Any prescribed finite eta0, eta_mu and symmetric eta_mu,nu jets are realized at x=0 by eta(x)=eta0+sum_mu eta_mu*x_mu+(1/2)*sum_mu,nu eta_mu,nu*x_mu*x_nu.",
      "For a diagonal Hessian slot, dx_mu wedge dx_mu=0; the diagonal slot occurs once, not twice its coefficient.",
      "For mu<nu, (dx_mu wedge dx_nu)*A+(dx_nu wedge dx_mu)*A=0 for every coefficient A, because the same right-hand coefficient multiplies opposite exterior signs.",
      "By linearity over every exterior slot and every Clifford coefficient, d(d eta)=sum_mu<nu(eta_mu,nu-eta_nu,mu)*dx_mu wedge dx_nu=0. This includes central imaginary coefficients and does not require coefficient commutativity.",
      "For D_B=d+[B,.] with the graded commutator, expanding D_B(D_B eta) cancels B wedge d eta and d eta wedge B cross terms, leaving d squared eta+(dB+B wedge B)*eta-eta*(dB+B wedge B). Thus D_B squared eta=[F,eta] for F=dB+B wedge B."
    ],
    notEstablished: [
      "The 420 fixed controls alone do not prove the identity for arbitrary coefficients, all gauge parameters, or the complete Ward identity.",
      "Vanishing ordinary d squared eta does not imply a zero ordinary Hessian, zero covariant Hessian, or zero covariant exterior square.",
      "This argument cannot omit frame-commutator terms in a nonholonomic moving frame; it is stated first in native coordinates, before the separately bound pullback.",
      "Zero Fourier frequency here encodes local coordinate-jet coefficient tensors, not a claim that a globally Fourier-expanded eta is constant.",
      "The recipe does not validate its source provenance, numerical primitive implementation, full scientific action or total resource bounds."
    ]
  });
}

function buildSecondJetControlsRecipe(recipe, options = {}) {
  need(recipe instanceof MixedRecipe, "MixedRecipe session required");
  const property = options && Object.getOwnPropertyDescriptor(options, "retention");
  need(property && Object.hasOwn(property, "value") && property.enumerable, "own-data retention option required");
  const retention = property.value;
  const menu = secondJetMenu(), markMenu = secondJetMarkMenu();
  need(retention && typeof retention === "object" && !Array.isArray(retention) && Reflect.ownKeys(retention).length === markMenu.length,
    "exact explicit retention census");
  // Read only own data properties; do not run getters/toJSON while admitting
  // a supposedly fixed policy. Snapshot BEFORE emitting either coefficient.
  const policy = Object.freeze(Object.fromEntries(markMenu.map(item => {
    const property = Object.getOwnPropertyDescriptor(retention, item.name);
    need(property && Object.hasOwn(property, "value") && property.enumerable && typeof property.value === "boolean",
      "own-data boolean retention for " + item.name);
    return [item.name, property.value];
  })));
  const coefficients = [recipe.Unit(0, 1 << 4, "1"), recipe.Unit(0, 3, "1")];
  const callbacks = [], marks = [], checks = [], domainChecks = [], images = [];
  for (const item of menu) {
    const coefficient = coefficients[item.coefficient];
    // Local is invoked once per image in C#. The common immutable inputs may
    // be checked as soon as ready, but no numerical domain test is skipped in
    // this declared consumer census and no extra evidence callback is invented.
    domainChecks.push({ name: "secondJetCoefficient/" + item.name, node: recipe.RecordedNode(coefficient), degree: 0,
      local: true, hAntiHermitian: true, canonicalNonzeroRecords: true });
    const value = buildSymmetricSecondJetImage(recipe, item.first, item.second, coefficient, item.route === 1);
    const path = "secondJet/" + item.name;
    // Sink SecondJet first accepts its parent callback, then records this mark;
    // the following Check tests Count==0 on this SAME complete tensor.
    callbacks.push({ category: "SecondJet", name: item.name, degree: 2, expanded: null, length: null });
    recipe.Mark(path, 2, value, policy[path]);
    const node = recipe.RecordedNode(value);
    marks.push({ name: path, degree: 2, node, expanded: policy[path] });
    checks.push({ name: item.name, metadataPath: "check/" + item.name, predicate: { kind: "tensorZero", tensor: { kind: "tensorNode", node } } });
    callbacks.push({ category: "Check", name: item.name, degree: null, expanded: null, length: null });
    images.push({ ...item, coefficientNode: recipe.RecordedNode(coefficient), node });
  }
  need(images.length === 420 && marks.length === 420 && checks.length === 420 && callbacks.length === 840 && domainChecks.length === 420,
    "complete second-jet census");
  return freeze({ schemaVersion: SCHEMA, coefficients: coefficients.map(value => ({ kind: "tensorNode", node: recipe.RecordedNode(value) })),
    callbacks, marks, checks, domainChecks, images, scalarRoots: [], geometry: [],
    census: { coefficientControls: 2, symmetricSlotsPerCoefficientAndRoute: 105, diagonalSlotsPerCoefficientAndRoute: 14,
      offDiagonalSlotsPerCoefficientAndRoute: 91, routes: 2, images: 420, tensorCallbacks: 420, checkCallbacks: 420, localDomainObligations: 420,
      emittedNodes: 3502, emittedUnitNodes: 1570, emittedProductNodes: 1568, emittedSumNodes: 364 },
    proofNotes: secondJetProofNotes(),
    scope: { completeFixedControlDag: true, completeFixedMarkAndCheckCensus: true, numericalTensorsEvaluated: false,
      numericalChecksEvaluated: false, coefficientDomainChecksEvaluated: false, arbitraryEtaEstablishedByFiniteControls: false,
      outerLifecycleClosed: false, sourceProvenanceValidatedHere: false, productionResourceAdmissionProved: false },
    unresolved: ["full numerical tensor and input-domain replay", "enclosing context and run lifecycle", "source binding of primitive definitions",
      "independent acceptance of the conditional arbitrary-eta algebraic argument", "whole-run resource admission"] });
}

// Corresponds to MixedDiagnostics.SecondJets: a dedicated705th context,
// never an unannounced tail of either existing diagnostic or scientific germ.
// The callbacks are a complete lifecycle RECIPE, not an executed/validated run.
function buildSecondJetDiagnosticRecipe(recipe, options) {
  const controls = buildSecondJetControlsRecipe(recipe, options);
  const lifecycle = category => ({ category, name: "$", degree: null, expanded: null, length: null });
  return freeze({ ...controls, contextId: "diagnostic/secondJets", diagnosticMenu: secondJetDiagnosticMenu(),
    callbacks: [lifecycle("Begin"), ...controls.callbacks, lifecycle("End")],
    scope: { ...controls.scope, completeDiagnosticLifecycleRecipe: true },
    unresolved: ["full numerical tensor and input-domain replay", "whole-run705-context census and source binding",
      "independent acceptance of the conditional arbitrary-eta algebraic argument", "whole-run resource admission"] });
}

module.exports = { SCHEMA, secondJetMenu, secondJetMarkMenu, secondJetDiagnosticMenu, buildSymmetricSecondJetImage,
  buildSecondJetControlsRecipe, buildSecondJetDiagnosticRecipe, secondJetProofNotes };
