# Phase404: GU Embedding-Chain Coupling-Ratio Enumeration (Brute Force #1)

## Question

USER DIRECTIVE (2026-06-11): run the brute-forceable computations. First:
exhaustively enumerate the GU-named embedding chain and compute its
embedding-derived coupling ratio and doublet inventories - the values the
Phase403 mechanism calls for on GU's actual chain.

## Construction

so(10) compact-level arithmetic (the D5 complexification of spin(6,4);
real-form caveat recorded): Pati-Salam scaffolding with su(2)_L/R from the
so(4) duality split (sign convention auto-resolved), color su(3) computed
as the traceless centralizer of the so(6) complex structure J, hypercharge
family Y(alpha, beta) = alpha R3 + beta J/2 scanned over a bounded rational
lattice (224 directions; B-L = (2/3) J, so the standard direction is
(1, 2/3)). Branching computed for BOTH the adjoint 45 (where the draft's
connection-valued scalar lives) and the spinor 16 (one family) via the
explicit Cl(10) Clifford construction on 32 complex dimensions. The Y
normalization is anchored INTERNALLY (the 16's color-singlet doublet
carries |Y| = 1/2 - lepton-doublet bookkeeping, not an experimental
input). CPU only: the objects are 10x10 / 32x32; the full enumeration
takes seconds (GPU is reserved for Phases 405/406 per the directive).

## Results (the computed values)

- **tan^2(theta_emb) = 3/5 exactly; sin^2(theta_emb) = 3/8** for the
  standard hypercharge direction - the embedding-derived coupling ratio of
  the GU chain at tree/algebraic level, derived blind and matching the
  classic unified-chain constant (asserted as a mathematical cross-check,
  like j(j+1)). The scan menu spans tan^2 in [0.027, 1.5] over admissible
  directions.
- **The complete SM family hypercharge pattern is DERIVED from the 16**:
  quark doublet |Y| = 1/6, lepton doublet |Y| = 1/2, up/down/electron/
  neutrino singlets at 2/3, 1/3, 1, 0 (lepton-normalized units), with the
  16-dimensionality and color/isospin tags machine-verified.
- **DECISIVE NEGATIVE for the adjoint scalar route: the 45 contains NO
  color-singlet charged doublet block for ANY of the 224 scanned
  hypercharge directions** - every doublet in the adjoint is colored
  (X/Y-boson type). The draft's scalar candidate is the pulled-back
  connection component; on this chain its gauge-algebra-adjoint part
  CANNOT supply the SM-Higgs-pattern doublet. The doublet must come from
  the NON-ADJOINT components of the GU connection (the vertical
  symmetric-2-tensor part named by the Iceberg analysis) or not at all -
  scalar-sector sub-gap (a) is now sharply localized.

## Status

Fail-closed. Charges and ratios only - no VEV, no scale, no GeV; the
compact form stands in for spin(6,4) (recorded); nothing promoted; zero
contract fields.

## Reproduce

```bash
dotnet run --project studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/Phase404GuEmbeddingChainCouplingRatioEnumeration.csproj
```
