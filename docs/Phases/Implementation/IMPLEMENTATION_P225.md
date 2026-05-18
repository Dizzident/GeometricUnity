# Implementation P225: SU(2) Normalization Representation Compatibility Audit

P225 explains why the numerically successful P221 W/Z factor cannot be promoted from the current Phase64 source operator.

P221 replaces the Phase63 trace-half single-generator scale with an adjoint/spin-1 SU(2) triplet RMS scale:

```text
sqrt(C2(adj)/dim su2) = sqrt(2/3)
```

That produces a strong numerical W/Z lead. The compatibility problem is representation lineage:

- P63 derives the canonical trace-half SU(2) convention `tr(T_a T_b)=1/2 delta_ab`.
- P64 is a fermion-current matrix element, `<phi_i, delta_D[b_k] phi_j>`, under that convention.
- The PDG electroweak charged-current convention uses weak-isospin generators acting on fermion doublets, `T+/-=(sigma1 +/- i sigma2)/2`.
- P221's successful factor is an adjoint/spin-1 triplet RMS hypothesis, not a replayed fermion-current source derivation.

Therefore the factor is a useful lead but not a prediction. Promotion requires a target-independent GU derivation that the W/Z source operator being evaluated is an isotropic SU(2) triplet RMS object, or a replayed analytic matrix element using the revised source operator, followed by the existing P201/P209/P210/P213 gates.
