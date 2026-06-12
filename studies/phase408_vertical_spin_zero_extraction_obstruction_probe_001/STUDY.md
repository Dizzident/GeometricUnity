# Phase408: Vertical Spin-Zero Extraction Obstruction Probe

## Question

Phase407 found the SM-Higgs quantum numbers in the chimeric
frame-cross-internal block, spacetime-vector-valued, and named the spin-0
extraction through the Y14 -> X4 vertical-form structure as the next
step. Does the naive vertical/symmetric-2-tensor trace extraction work?

## Construction

The chimeric weld identifies the internal vector 10 with the metrics
fiber Sym^2(R^4), so spacetime so(4) acts on the SM-charged 10 through
the Sym^2 embedding pi: so(4) -> so(10) (built exactly as the action
S -> XS - SX on symmetric matrices with the trace inner product;
antisymmetry 0, homomorphism residual 2.2e-16). Three exact checks
against the Phase404 SM scaffolding on the same 10.

## Results

- **V1 - the weld entangles spin and isospin**: every tested SM generator
  (su(2)_L x3, su(2)_R Cartan, hypercharge, color samples) has a NONZERO
  commutator with pi(so(4)) (max norm 2.83).
- **V2 - the centralizer is trivial**: the exact kernel of
  X -> ([X, pi(M_a)])_a over so(10) has dimension 0 (Sym^2 = 9 + 1 is
  multiplicity-free; Schur). After the chimeric weld NO internal symmetry
  commutes with spacetime rotations - the Coleman-Mandula-shaped tension
  the draft's observerse construction must evade by keeping the weld
  observational.
- **V3 - the dimension bound**: the so(4)-singlet subspace of the
  vertical 10 is EXACTLY the 1-dimensional trace direction
  (machine-verified, alignment 1.0), while the SM complex doublet needs
  the full 4-real-dimensional Pati-Salam block. 1 < 4: the naive
  vertical-trace spin-0 extraction CANNOT carry a full SM doublet, for
  ANY alignment of the weld.

## Interpretation

Scalar-sector sub-gap (a) reaches its final internal form: the Phase407
doublet cannot descend to an X4-scalar doublet through the trace slot.
The draft's ADDITIONAL machinery (epsilon-conjugation/Shiab structure,
unobserved-phase fields, or a different extraction map - none specified
quantitatively in the primary text) is now the precisely named
requirement. Combined with the choice-independent gaps (VEV selection,
quantitative chain), the internal program has characterized the scalar
route to its boundary.

## Status

Fail-closed. Exact arithmetic; compact-form caveat; the draft's epsilon/
Shiab route is NOT evaluated here (named open); nothing promoted; zero
contract fields.

## Reproduce

```bash
dotnet run --project studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/Phase408VerticalSpinZeroExtractionObstructionProbe.csproj
```
