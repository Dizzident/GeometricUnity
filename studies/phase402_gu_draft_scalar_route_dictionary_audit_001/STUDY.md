# Phase402: GU-Draft Scalar-Route Dictionary Audit

## Question

The TOE-GU-ICEBERG-20250423 cataloguing produced a GU-native structural
ansatz for the scalar/Yukawa gap rows with three check items: the
representation assignment of the claimed Higgs component
(adjoint-vs-doublet), the "normalization fixed by contraction rules"
claim, and the negative-mass-squared origin. What does the PRIMARY 2021
draft actually say, and what does it pin down for the repo?

## Construction

The primary draft text is now stored
(`docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`,
PDF SHA256 pinned). Three machine-checked components:

1. **Primary text evidence** - line-level pattern verification of the
   draft's own location dictionary (eq. 12.28) and Lagrangian
   (eq. 9.11/12.2/12.3), the section 2.3 "valued in a Lie Algebra"
   statement, and absence counts (GeV, 246, doublet-applied-to-Higgs).
2. **Repo correspondence** - numerical verification at the persisted
   backgrounds that the draft-claimed Higgs potential and Higgs
   Klein-Gordon equation are exactly the repo's production machinery.
3. **Representation discriminator** - closed-form su(2)xu(1) gauge-boson
   mass patterns for doublet vs real adjoint-triplet scalar VEVs with
   non-physical study couplings (g = 1.3, g' = 0.7, v = 2.0).

## Results

- **The draft's own dictionary places the Higgs potential at
  <Upsilon_omega, Upsilon_omega> and the Higgs Klein-Gordon equation at
  D_omega^* Upsilon_omega = 0 - exactly the repo's Mode-B objective and
  stationarity condition** (verified: production objective =
  (1/2)<U, M U> to machine precision; backgrounds on the Upsilon ~ 0
  vacuum manifold at 5e-10/1.8e-9; discrete D^* Upsilon at the persisted
  solve scale 8.5e-10/3.1e-9, polished to 2.7e-17 by Phase401's
  baseline). The Phase393-401 toy-branch program has been characterizing
  the draft-claimed Higgs potential's vacuum manifold all along.
- **The potential <U, U> is non-negative with no free quartic coupling**:
  the "contraction rules" normalization claim is primary-faithful, but
  symmetry breaking must come from the geometry of the Upsilon = 0 locus
  - the explainer's negative-mass-squared phi-A mechanism is
  reconstruction, NOT the primary's mechanism.
- **Representation discriminator**: the draft assigns the scalar to a
  Lie-algebra-valued (adjoint) object (section 2.3) and never applies
  "doublet" to the Higgs (machine-checked: all doublet mentions are
  fermionic). The closed-form mass patterns show the adjoint-triplet VEV
  CANNOT produce the SM neutral sector (two massless neutrals, no W3-B
  mixing, custodial sentinel) while the doublet does (one massless
  neutral, W3-B mixing present, custodial identity to 1e-16). The
  binding scalar-sector gap is SHARPENED to: exhibit a DOUBLET-EQUIVALENT
  substructure inside the pulled-back connection component.
- **Zero dimensionful anchors in the primary**: the draft contains no
  "GeV" and no "246" anywhere; the VEV rows of the dictionary
  (cosmological constant, Yukawa couplings) carry no values.

## Status

Fail-closed. The audit verifies primary-source LOCATIONS, not
derivations; the doublet-equivalent substructure is a NAMED GAP, not a
finding; nothing is promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase402_gu_draft_scalar_route_dictionary_audit_001/Phase402GuDraftScalarRouteDictionaryAudit.csproj
```
