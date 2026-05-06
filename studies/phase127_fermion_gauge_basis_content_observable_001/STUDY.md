# Phase127 Fermion Gauge-Basis Content Observable

This study materializes a target-blind diagnostic observable for the quality repaired Phase95 fermion modes after P124/P125 source-family enrichment.

It computes per-mode SU(2) gauge-basis energy fractions directly from the stored complex eigenvector coefficients using the Dirac assembler layout:

`((vertex * dimG + gauge) * spinorComponents + spinor)`

The output is diagnostic evidence only. A gauge-basis dominance label is promoted only if a quality mode has a dominant basis fraction of at least `0.80` and a gap of at least `0.20` over the next basis component.
