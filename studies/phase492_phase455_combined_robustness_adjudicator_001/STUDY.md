# Phase492: Phase455 Combined Robustness Adjudicator

This Amendment A7 phase is a prospective, zero-physics adjudicator. It reads
only the exact Phase490 and Phase491 summary paths, freezes their byte hashes
before semantic parsing, and verifies that neither precursor changes during
consumption.

The decision precedence is fixed in code: an invalid precursor fails closed;
a unique admissible quotient plus uniformly null branches reaches
`stable-null`; the same quotient plus uniformly candidate-well branches reaches
`stable-candidate-well`; a named partition reaches
`localized-assumption-dependence`; everything else remains
`underdetermined-external-interpretation`.

These are exploration classifications only. They are never translated into a
Phase455 terminal, Phase458 gate evidence, an O4 ruling, sampling authority,
source-contract content, or a promotion claim.
