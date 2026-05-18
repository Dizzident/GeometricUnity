# Implementation P308: Phase302 Scale Transfer to Decoupled Charged-Ladder Audit

Phase308 tests whether the Phase302 source-mode-vector-length normalization
and W-specific adjoint/fundamental Casimir multiplier can be transferred to the
Phase306/Phase307 decoupled charged-ladder W/Z row family as a prediction law.

The audit is intentionally JSON-only. It does not recompute rows; it reads the
already materialized Phase302, Phase306, and Phase307 artifacts, then checks
whether the Phase302 scale lead has the theorem and source-lineage support
needed for promotion after transfer.

Targets are not used to construct the transferred scale. Target information
appears only in upstream post-candidate numerical gates.

## Result

Terminal status:

`phase302-scale-transfer-to-decoupled-charged-ladder-audit-transfer-not-promotable`

The audit preserves the numerical lead:

- `p302CommonScaleId=source-mode-vector-length`;
- `p302ParticleLawId=adjoint-casimir-over-fundamental-casimir`;
- `p302CommonScaleValue=156`;
- `p302WTotalScale=416`;
- `p302ZTotalScale=156`.

It also preserves the blocker:

- `p302CommonScaleApplicationTheoremPresent=false`;
- `p302ParticleLawApplicationTheoremPresent=false`;
- `p302PromotionEligible=false`;
- `scaleTransferTheoremClaimed=false`;
- `scaleTransferAllowed=false`;
- `canFillPhase201WzContract=false`.

Phase306 and Phase307 show that the transferred Phase302 scale can produce
downstream numerical near-passes, but both transfer applications still have
`unscaledRawPassingCount=0` and cannot fill the Phase201 W/Z source-lineage
contract.

## Outputs

- `studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit.json`
- `studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json`
