# Phase496: Phase456 retained-data information census

## Purpose

Phase496 is the deterministic, read-only A10 census of the committed Phase456
production artifact and the exact Phase493--495 diagnostic outputs. It asks
what information was actually preserved and whether that information can
identify the admitted two-pole correlator class. It does not reinterpret any
invalid Phase456 row.

## Frozen adjudication

The precedence is:

1. `invalid-precursor`;
2. `retained-data-insufficient-for-identifiable-repair`;
3. `retained-data-sufficient-for-prospective-estimator-design`.

Sufficiency requires exact inputs, at least four independent temporal values,
configuration-level samples, and a temporal Jacobian-rank bound of at least
four for two amplitudes plus two masses. Spatial momenta cannot silently count
as extra time separations without a separately frozen dispersion or
shared-spectrum assumption.

## Result boundary

The committed `T=4` artifact contains all four stored separations but only
three independent periodic temporal values, of which two are at nonzero
separation. It preserves all 64 spatial-momentum aggregate correlators and 50
aligned delete-block summaries for each recorded family, but not raw
configuration-level measurements. The maximum temporal rank is therefore
three for the admitted four-parameter two-pole class.

The expected conservative terminal is
`retained-data-insufficient-for-identifiable-repair`. This negative result
does not erase the useful retained aggregates, diagnose the underlying
observable as invalid, authorize a repair pack or sampling, satisfy Phase458,
discharge O4, or support any physical-unit claim.
