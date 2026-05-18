# Phase 296 - Source-Lineage Contract Field Candidate Scan

## Purpose

Phase296 audits the exact W/Z and Higgs source-lineage fields required by Phase201 and Phase209. Phase204 and Phase205 already say no current artifact is intake-ready in broad JSON/text terms; Phase296 adds a per-field census so the remaining source-lineage blocker is explicit at the contract-field level.

## Contract Scope

Phase296 covers 29 required fields:

- 15 W/Z fields:
  - `externalTargetValuesUsed=false`;
  - `theoremOrDerivationId`;
  - `sourceLineageId`;
  - W and Z `sourceRowId`;
  - W and Z `rawAmplitudeGatePassed=true`;
  - W and Z `commonBridgeGatePassed=true`;
  - W and Z `targetComparisonGatePassed=true`;
  - W and Z `stabilitySidecarsPresent=true`;
  - W and Z `derivationId`.
- 14 Higgs fields:
  - `externalTargetValuesUsed=false`;
  - `sourceLineageId`;
  - `scalarSourceOperatorId`;
  - `higgsIdentityEnvelopeId`;
  - `massiveScalarProfileId`;
  - `potentialOrSelfCouplingSourceId` or `excitationRelationId`;
  - branch/refinement/environment/representation/coupling stability sidecars;
  - prediction-row `sourceRowId`, `targetComparisonGatePassed=true`, and `derivationId`.

## Implementation

Added:

- `studies/phase296_source_lineage_contract_field_candidate_scan_001/Phase296SourceLineageContractFieldCandidateScan.csproj`
- `studies/phase296_source_lineage_contract_field_candidate_scan_001/Program.cs`

The scan:

- loads Phase201 templates and summaries;
- loads Phase204, Phase205, Phase207, Phase209, Phase213, Phase245, and Phase295;
- scans the same broad first-party corpus used by Phase295;
- classifies hits for each W/Z and Higgs contract field;
- treats generated studies, prompt/gap/report material, diagnostics, templates, and requirement prose as candidate mentions only;
- requires source/theorem provenance, target-blind construction, and positive gate/identifier evidence before any hit can be intake-ready.

Wired Phase296 into:

- `scripts/generate_validated_boson_predictions.sh`;
- `scripts/verify_boson_claim_integrity.sh`;
- P101 prediction package output;
- P202 objective checklist;
- P204/P205/P207 scanner guards.

## Result

Initial run:

- `terminalStatus=source-lineage-contract-field-candidate-scan-no-intake-ready-artifact`.
- `sourceLineageContractFieldCandidateScanPassed=true`.
- `scannedFileCount=7142`.
- `contractFieldCount=29`.
- `wzContractFieldCount=15`.
- `higgsContractFieldCount=14`.
- `totalCandidateLineCount=47286`.
- `fieldsWithCandidateLineCount=29`.
- `fieldsWithIntakeReadyCandidateCount=0`.
- `intakeReadySourceLineageFieldCandidateCount=0`.
- `anySourceLineageCandidateFillsContract=false`.

## Decision

Do not promote W/Z/H masses on source-lineage contract grounds. The corpus has mentions for every required P201/P209 field, but no intake-ready artifact fills the W/Z theorem/source rows/gates or the Higgs scalar-source/profile/coupling/stability/prediction-row requirements.

## Next Required Artifact

A successful prediction still requires:

- a W/Z target-independent source-lineage artifact with theorem, source lineage, separate W and Z source rows, source-replayed gates, stability sidecars, and derivation ids;
- a Higgs target-independent scalar-source artifact with scalar source operator, identity envelope, massive profile, potential/self-coupling or excitation source, stability sidecars, and a prediction row;
- rerun of P201/P209/P210/P213, P101, P192, P193, and P202 after those artifacts are filled.
