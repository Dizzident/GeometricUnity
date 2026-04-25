# Phase XIX Dimensionless W/Z Candidate

## Purpose

This study creates the first narrow scaffold for a dimensionless physical
comparison without activating a physical prediction claim.

The chosen target is the PDG W/Z mass ratio because it avoids GeV scale-setting.
It still requires a validated W/Z mode-identification derivation before it can
be treated as a physical observable.

## Artifacts

- `candidate_observables.json`: an inactive candidate observable record for a
  W/Z vector-mode ratio.
- `candidate_modes.json`: provisional W and Z identified-mode input records
  documenting the input shape required before a ratio can be computed.
- `mode_identification_evidence.json`: provisional evidence records documenting
  the missing W and Z mode-identification derivations.
- `observable_classifications.json`: classifies the candidate as
  `physical-candidate`, not `physical-observable`.
- `physical_observable_mappings.json`: provisional mapping from the candidate
  record to `physical-w-z-mass-ratio`.
- `physical_calibrations.json`: provisional dimensionless identity
  normalization.
- `physical_targets.json`: PDG W/Z mass-ratio target copied into this isolated
  study.
- `target_coverage_blockers.json`: explicit blocker allowing a physical
  comparison campaign to run and report `blocked` without pretending the target
  is currently computable.

## Status

This study is not referenced by the active reference campaign.

The candidate is blocked from physical prediction because:

- the observable classification is `physical-candidate`;
- physical claims are not allowed for the classification;
- the mapping is `provisional`;
- the calibration is `provisional`;
- the extraction method is a placeholder contract, not a validated W/Z
  mode-identification calculation.

## Extractor Support

The quantitative validation library now has a reusable positive-mode ratio
extractor. It computes `numerator / denominator` for two positive mode values
and propagates independent numerator and denominator uncertainty into the
dimensionless ratio.

The extractor also accepts two quantitative mode records directly. That path
requires shared environment, branch, and refinement selectors, and it rejects
mode records with unestimated total uncertainty. This is the path a future
validated W/Z comparison should use after W and Z modes are identified.

The quantitative validation library also defines `IdentifiedPhysicalModeRecord`
for physics-facing mode inputs. Ratio extraction from these records requires
both inputs to be `validated`, to share units, and to pass the same selector and
uncertainty checks as generic quantitative mode records.

Phase XVIII adds mode-identification evidence records. Prediction extraction
can require validated evidence, in which case provisional placeholder modes are
rejected before any ratio is produced.

The checked-in candidate artifact names this extraction contract, but still
uses placeholder inputs. That keeps the code path ready while preserving the
scientific block on W/Z mode identification.

## Completion Requirements

To promote this scaffold into an active physical campaign:

- derive the vector-mode identification rule;
- compute the candidate from validated W and Z mode observables;
- replace provisional mapping and calibration records with validated records;
- activate the PDG target table in a separate campaign;
- resolve or explicitly carry severe falsifiers as physical-claim blockers.
