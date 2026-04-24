# Zenodo SU(2) Plaquette Chain Benchmark

Phase XIV replaces the placeholder external lattice-gauge target with a
DOI-backed external software/data source:

- record: `https://zenodo.org/records/16739090`;
- DOI: `10.5281/zenodo.16739090`;
- file: `su2lgt-main.zip`;
- md5: `7c09478c1d2b2816d416e564695d0bc0`;
- sha256:
  `f1d566b97ad3fee275afd36784edbf8f2067e4bed4d63bcc09abfedd8db0eb8c`;
- license: MIT / CC BY 4.0 as reported by Zenodo.

The external package contains Python code for pure SU(2) lattice gauge theory on
square plaquette chains, including Hilbert-space construction, Kogut-Susskind
Hamiltonian construction, and exact diagonalization.

The checked-in benchmark uses the small deterministic periodic-chain case:

- plaquettes: `P=4`;
- truncation: `jmax=0.5`;
- momentum sector: `k=0`;
- coupling: `g^2=1.5`;
- eigenvalues:
  `[-2.0762941435550895, 1.839998261954932, 3.3631147376181514, 4.230924828019929, 5.2359841347303195, 6.531272181231768]`;
- observable: adjacent gap ratio at index `0`;
- value:
  `min(E1-E0, E2-E1) / max(E1-E0, E2-E1) = 0.3889179657576827`.

Reproduction commands used in ignored `study-runs/`:

```bash
curl -L -o study-runs/phase14_zenodo_su2_source/su2lgt-main.zip \
  'https://zenodo.org/records/16739090/files/su2lgt-main.zip?download=1'

python3 -m pip install h5py --target study-runs/phase14_zenodo_su2_source/pydeps

cd study-runs/phase14_zenodo_su2_source/extracted/su2lgt-main
printf 'phase14_zenodo_su2.h5\n' > storage_path.txt
PYTHONPATH=../../pydeps python3 create_basics.py
PYTHONPATH=../../pydeps python3 periodic/states_periodic_k0_undocumented.py 4 0.5 0
PYTHONPATH=../../pydeps python3 periodic/plaquette_periodic_k0.py 4 0.5 0
PYTHONPATH=../../pydeps python3 periodic/eigensystem_periodic_k0.py 4 0.5 0 1.5
```

The repository CLI can convert the checked-in eigenvalue list into the matching
observable:

```bash
dotnet run --project apps/Gu.Cli -- extract-spectrum-observable \
  --eigenvalues studies/phase5_su2_branch_refinement_env_validation/config/zenodo_su2_plaquette_chain_eigenvalues.json \
  --observable-id bosonic-eigenvalue-ratio-1 \
  --environment-id env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1 \
  --branch-id zenodo-su2lgt-periodic-k0 \
  --refinement-level P4-j0.5-g1.5 \
  --gap-index 0 \
  --uncertainty 0.001
```
