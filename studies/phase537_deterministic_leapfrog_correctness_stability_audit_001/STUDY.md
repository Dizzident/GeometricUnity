# Phase 537 — deterministic leapfrog correctness and stability audit

Phase537 implements Amendment A23's RNG-free audit on the exact binary64
polynomial frozen by Phase534. Its preregistration exact-binds the immutable
Phase533-535 evidence before execution.

The phase independently compares the analytic polynomial gradient with a
symmetric finite-difference oracle, integrates a frozen state/momentum grid
forward and backward, and checks the aggregate energy-error order over a
fixed-length step-halving ladder. The terminal taxonomy is fail-closed and was
frozen before the first run.

This audit cannot repair or relabel Phase534, reopen Phase535, diagnose a
stochastic trajectory failure, or establish acceptance, sampling, convergence,
or mixing validity. It uses no RNG and runs no HMC chain. It creates or mutates
no Phase481 pack, satisfies no Phase458 or O4 gate, fills no source contract,
and supports no physical-unit claim. `promotedPhysicalMassClaimCount=0`.

Run with:

```bash
dotnet run -c Release --project studies/phase537_deterministic_leapfrog_correctness_stability_audit_001/Phase537DeterministicLeapfrogCorrectnessStabilityAudit.csproj
```
