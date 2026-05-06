# Phase130 Fermion Sector Label Table Gate

This study turns the P129 sector-label blocker into a concrete table and validation gate.

It materializes one row per quality repaired fermion family, carrying the known source mode, cluster, candidate, registry coverage, representation-content evidence, and required physical sector-label fields.

The gate is fail-closed. A row is promotable only when it has:

- candidate coverage for the repaired family
- complete representation-content evidence
- explicit target-blind `chargeSector`
- explicit `weakSector` or `quantumNumbers`
- a derivation source for the physical labels

The generated table intentionally leaves physical labels unassigned when no source artifact supports them.
