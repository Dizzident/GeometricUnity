# Phase XXI Source Readiness Campaign

## Purpose

This study evaluates identity-neutral internal vector-boson source candidates
against explicit source-readiness rules. It is a readiness gate for upstream
source data only; it does not assign W or Z identity and does not open a
physical prediction claim.

## Inputs

- `studies/phase20_internal_vector_boson_sources_001/source_candidates.json`
- `studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json`
- `studies/phase21_source_readiness_campaign_001/spectra_manifest.json`

External physical target tables are not inputs to this study.

## Outputs

- `source_candidates.json`: Phase XXI source-readiness candidate table.
- `candidate_mode_sources.json`: blocked identity-neutral bridge summary for
  the P19 candidate-mode source contract.

Current terminal status:

- `source-blocked`

The Phase XX candidates remain internal computed artifacts, but no candidate is
ready for downstream identity testing. The checked-in Phase12 source records do
not include enough selector and uncertainty information to satisfy the Phase XXI
readiness policy.

## Current Blockers

- branch selectors are missing;
- refinement coverage is missing;
- mode-family matching remains ambiguous for the candidate set;
- branch stability is below the configured threshold;
- claim class is below `C2_BranchStableCandidate`;
- source uncertainty components and total uncertainty remain incomplete.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- evaluate-internal-vector-boson-source-readiness \
  --source-candidates studies/phase20_internal_vector_boson_sources_001/source_candidates.json \
  --spec studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json \
  --out studies/phase21_source_readiness_campaign_001/source_candidates.json
```

## Guardrails

- Source candidates remain particle-identity-neutral.
- No record assigns W or Z identity.
- No PDG or external physical target value is used as source data.
- Blocked source candidates are not promoted into physical candidate modes.
