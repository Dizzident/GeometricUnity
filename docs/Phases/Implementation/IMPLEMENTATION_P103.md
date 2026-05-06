# Phase 103 - Target-Scoped Falsifier Policy

## Goal

Clear the falsifier-policy process blocker for target-scoped comparison while
continuing to block unrestricted global physical-boson language.

## Completed

- Added `studies/phase103_target_scoped_falsifier_policy_001`.
- Consumed the Phase47 falsifier relevance audit.
- Adopted a policy where target-relevant fatal/high falsifiers block the target
  claim, while global sidecar severe falsifiers remain disclosed and block
  unrestricted physical language.

## Result

The Phase100 `falsifier-policy` gate now passes for target-scoped review because
the Phase47 target-relevant severe falsifier count is zero.

Unrestricted physical boson language remains blocked because global sidecar
severe falsifiers still exist.

## Verification

- `dotnet build studies/phase103_target_scoped_falsifier_policy_001/Phase103TargetScopedFalsifierPolicy.csproj --verbosity minimal`
- `dotnet run --project studies/phase103_target_scoped_falsifier_policy_001/Phase103TargetScopedFalsifierPolicy.csproj --no-build`
