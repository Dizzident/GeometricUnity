# Finite NCG Discrete-Higgs Sources

Back to [ExperimentReferences.md](../../../ExperimentReferences.md).

## Sources

- Alain Connes and John Lott, "Particle Models and Noncommutative Geometry":
  `https://repo-archives.ihes.fr/FONDS_IHES/I_Prepublications/CONNES/1985-1993/M_90_23/M_90_23.pdf`.
- "Noncommutative geometry and Higgs mechanism in the Standard Model":
  `https://www.sciencedirect.com/science/article/pii/0370269391911804`.
- Andrzej Sitarz, "Higgs Mass and Noncommutative Geometry":
  `https://arxiv.org/abs/hep-th/9304005`.
- Yoshitaka Okumura, "Renormalization Group Analysis of the Higgs Boson
  Mass in a Noncommutative Geometry":
  `https://academic.oup.com/ptp/article/98/6/1333/1868457`.
- Ali H. Chamseddine, Alain Connes, and Matilde Marcolli, "Gravity and the
  Standard Model with neutrino mixing": `https://arxiv.org/abs/hep-th/0610241`.
- Ali H. Chamseddine and Alain Connes, "Resilience of the Spectral Standard
  Model": `https://arxiv.org/abs/1208.1030`.
- Agostino Devastato, Fedele Lizzi, and Pierre Martinetti, "Higgs mass in
  Noncommutative Geometry": `https://arxiv.org/abs/1403.7567`.
- Fabien Besnard and Christian Brouder, "Noncommutative geometry, the
  Lorentzian Standard Model and its B-L extension":
  `https://arxiv.org/abs/2010.04960`.

## Summary

Finite noncommutative geometry is a serious geometric source family because it
places the Standard Model on an almost-commutative space: ordinary spacetime
times a finite internal algebra. In this picture the Higgs is not introduced as
an unrelated scalar but appears through the discrete/internal part of the
connection or as an inner fluctuation of the Dirac operator.

This is close to the desired bridge-source law structurally: it geometrizes the
Higgs mechanism, produces Yang-Mills-Higgs action terms, and historically led
to boundary relations such as `m_H = sqrt(2) m_W` and high-scale spectral-action
constraints. Later RG analyses put the Higgs well above the observed 125 GeV
in the minimal route, while post-LHC repairs need an extra scalar, B-L
extension, threshold effects, or restricted parameter regions.

## How It Was Used

Phase359 audits this route as a finite-NCG/discrete-Higgs source candidate and
wires the result into the boson prediction package, completion audit, and
claim-integrity verifier.

## Prediction Relevance

Useful:

- Gives a concrete geometric template for Higgs-as-connection or Higgs-as-inner
  fluctuation.
- Shows how a finite internal algebra can encode Standard Model gauge and
  Higgs structures.
- Supplies historical Higgs/W and weak-angle relation leads that are directly
  relevant to W/Z/H mass-source diagnostics.
- Provides a nearby template for the kind of GU Dirac-operator/algebra source
  that would be needed if GU is to derive the Higgs sector geometrically.

Not enough:

- Sitarz shows that simple Higgs-mass relations are not forced once an allowed
  gauge-invariant discrete-curvature term is included.
- The spectral-action route is high-scale, cutoff/RG/Yukawa/neutrino dependent
  and originally favored a Higgs near 170 GeV rather than the observed value.
- Post-LHC compatibility requires additional scalar or B-L extension structure,
  not a GU-local target-independent W/Z/H source law.

## Limitation

This route cannot currently promote physical W/Z/H masses in the repository.
It does not provide a GU-local finite algebra from Shiab/observer geometry,
separate W/Z source rows, a weak-angle/coupling source, observed photon/W/Z/H
projection, target-independent VEV or scale, Higgs scalar-source/self-coupling
lineage, low-energy RG/threshold transport, physical pole extraction, or GeV
normalization.

## Follow-Up

Revisit only if a source supplies:

- a GU-local derivation of an almost-commutative or finite internal algebra;
- a GU Dirac operator and finite algebra source producing observed electroweak
  SU(2) x U(1), photon/W/Z projection, and separate W/Z source rows;
- a Higgs scalar-source and self-coupling law that survives the Sitarz
  obstruction without fitted extra-scalar/B-L parameters;
- low-energy RG/threshold transport, physical pole extraction, and GeV
  normalization.
