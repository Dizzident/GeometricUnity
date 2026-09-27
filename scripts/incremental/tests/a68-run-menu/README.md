# A68 run-menu metadata tests

Run with:

```sh
dotnet run --project scripts/incremental/tests/a68-run-menu/A68RunMenuMetadataTests.csproj -c Release
```

This standalone harness links only `MixedAuditPlan.cs`. It does not reference
the study project or include its scientific entry point, numerical arithmetic,
Fourier tensors, geometry, or coefficient evaluators. Synthetic boolean checks
exercise metadata census rejection, not scientific validity.

The suite covers the exact 705-context plan, its dedicated second-jet owner,
all 420 tensor/check pairs with Begin/End, fail-closed omissions/duplicates,
and the unchanged 700 scientific germ menus. Production activation, numerical
validation, retention/resources, and source provenance remain separate gates.
