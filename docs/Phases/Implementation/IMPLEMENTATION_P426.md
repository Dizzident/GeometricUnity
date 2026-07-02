# Implementation P426 - Cox GU Series (I-V) Boson Contract Audit

Phase426 adds `studies/phase426_cox_gu_series_boson_contract_audit_001`,
the source audit for the June 2026 Cox "Geometric Unity I-V" Zenodo series
that resolves the hinted "Geometric Unity V" monitor target.

## What It Checks

- Records the five-record series identity (record IDs 20550275, 20517363,
  20517502, 20518853, 20531776; DOIs; publication dates 2026-06-02..05;
  PDF sizes/md5 checksums; pdftotext line counts) and the "Cox, Joseph"
  author lineage shared with the Phase421 GU IV v2 audit.
- Records the contract-keyword evidence per record: GU I/III/IV/V have no
  electroweak contract content (GU I's "246" is an equation number; GU
  III's hypercharge mentions are read-only GU II imports; GU IV/V are
  cosmology export/audit rigs with zero contract-keyword hits).
- Classifies GU II ("The Matter Ledger") against its own scope
  boundaries: it names the minimal Pati-Salam bi-doublet scalar channel
  `(1,2,2)` (candidate seed only; not proven realized by any internal
  fluctuation), derives the hypercharge kernel `Y = T_R3 + (B-L)/2` with
  `g_BL = sqrt(3/2) g4`, and proves the tree-level unification-point
  relation `g_Y^2 = (3/5) g^2` - recorded strictly as CORROBORATION of
  the repository's blind Phase404 embedding result and the Phase403/409
  doublet-carrier requirement.
- Preserves every fail-closed boundary: no scalar potential, VEV, mass
  spectrum, unification scale, threshold model, measured-coupling fit,
  W/Z/H rows, pole extraction, or GeV normalization anywhere in the
  series.

## Scientific Boundary

- `guIiKernelRelationCorroboratesPhase404=True` (corroboration only)
- `guIiProvidesScalarPotential=False`, `guIiProvidesVevOrScale=False`,
  `guIiProvidesMassSpectrumOrPole=False`
- `sourceProvidesWzSourceRows=False`,
  `sourceProvidesHiggsScalarSourceRow=False`,
  `sourceProvidesWeakAngleOrCouplingLineage=False`,
  `sourceProvidesGeVUnitNormalization=False`
- `canFillPhase201WzContract=False`,
  `canFillPhase201HiggsContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`, `routePromotesHiggsMass=False`

The series is useful formal context, and GU II independently reproduces
the repository's embedding-lineage boundary - but it stops exactly where
the repository's own program stopped: no symmetry-breaking dynamics, no
scale, no observed-field extraction.
