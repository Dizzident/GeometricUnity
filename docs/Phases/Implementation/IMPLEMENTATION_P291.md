# Phase 291 - Koide Charged-Lepton Threshold Source Audit

Audit whether the Koide charged-lepton mass relation can replace the external electron, muon, and tau threshold inputs used by Phase286's alpha-running W/Z diagnostic.

The audit intentionally fails closed. Koide is a strong empirical charged-lepton lead: using the external electron and muon masses with `Q=2/3` reconstructs a tau-like mass near the observed tau pole mass, and the resulting lepton-running diagnostic preserves the W/Z numerical closure. It is not a promotable GU source because it imports electron and muon masses, assumes an empirical relation, supplies no GU-local source-lineage row for the three thresholds, and does not fill the alpha, running-operator, VEV, W/Z source-lineage, or Higgs scalar-source contracts.

Artifacts:

- `studies/phase291_koide_charged_lepton_threshold_source_audit_001/output/koide_charged_lepton_threshold_source_audit.json`
- `studies/phase291_koide_charged_lepton_threshold_source_audit_001/output/koide_charged_lepton_threshold_source_audit_summary.json`
