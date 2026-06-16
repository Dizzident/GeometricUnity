# Phase413: Noncompact Real-Form Transfer Probe

## Question

Every composite no-go in Phases 408/409/411/412 was proved on the
COMPACT form (so(4) welded into so(10)); the draft's structures are
noncompact (Lorentzian so(1,3); Spin(6,4)). DEEP-RESEARCH-20260612 named
a noncompact-only invariant as the single most plausible evasion. Does
that loophole exist for the welded chain?

## Construction

The transfer principle, machine-verified rather than assumed: every
no-go in the series is a complex-linear kernel dimension of operator
families built functorially from the weld; if the noncompact weld
complexifies to the SAME so(10, C) embedding as the compact one, every
count transfers verbatim. Batteries: (N1) the Lorentzian weld pi_eta:
so(1,3) -> gl(10) on Sym^2(R^{1,3}) with eta = diag(-1,1,1,1) -
homomorphism + invariance of the induced trace form
B(S,T) = Tr(eta S eta T) + machine signature of B; (N2) the keystone:
T4 = diag(i,1,1,1) carries eta to delta - verify
T10 pi_eta(X) T10^{-1} = pi_compact(T4 X T4^{-1}) exactly for all six
generators; (N3) direct fail-closed recomputation of the headline counts
on the noncompact form (linear singlet content of 4 x 10; bilinear
spin-0 dimension of its square); (N4) named residuals (real/Majorana
structure bookkeeping; the unitary representation category).

## Results

- **N1**: pi_eta is an exact Lie homomorphism (residual 2.2e-16) and
  preserves the induced form (2.2e-16), whose machine signature is
  **(7,3)** - exactly the Cl(7,3) signature axis Phase406 verified
  independently as 16-dim chiral.
- **N2 (keystone)**: the complexified welds COINCIDE exactly
  (residual 2.2e-16). Every complex-linear kernel dimension transfers.
- **N3**: direct noncompact recomputation: linear singlet content 0
  (= compact), bilinear spin-0 dimension 7 (= compact). The transfer is
  confirmed by construction AND by recomputation.
- **Verdict**: `noncompact-real-form-transfer-established-no-gos-are-
  signature-independent`. The Phase408-412 no-gos are REAL-FORM
  INDEPENDENT on the chain's finite-dimensional carriers; no
  noncompact-only invariant can evade them. Named residuals: real
  structure bookkeeping (relabels real dimensions; cannot create complex
  invariants), and the unitary infinite-dimensional category (out of
  scope). The remaining named routes reduce to the draft's
  unobserved-phase fields or a new primary-source specification.

## Status

Fail-closed. Exact representation arithmetic only; no dynamics, no
scales; nothing promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase413_noncompact_real_form_transfer_probe_001/Phase413NoncompactRealFormTransferProbe.csproj
```
