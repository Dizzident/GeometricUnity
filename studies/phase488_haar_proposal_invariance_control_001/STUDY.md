# Phase488: Haar proposal invariance control

This exploration-lane phase prospectively freezes deterministic controls for
four independently constructed SO(3) Haar proposal/sampling families:
Shoemake quaternion independence, normalized-Gaussian left composition,
radial-axis-angle right composition, and direct Rodrigues axis-angle sampling.

The zero-action Haar target makes the exact acceptance probability one. The
control checks analytic character and matrix-entry moments, inversion symmetry,
fixed left/right composition invariance, equal-Haar-probability angle-bin
occupancy, and binned detailed balance. Seeds, sample counts, binning, and
tolerances are constants declared before sampling. Every failed family emits a
named negative category and the process then fails closed.

The independent Phase487 measure control is a mandatory precursor. A missing
or inadmissible precursor reaches its own fail-closed terminal.

This is reduced deterministic exploration only. It authors no human ruling,
does not discharge O4, does not evaluate Phase458, does not authorize
production, and supports no physical-unit promotion.
