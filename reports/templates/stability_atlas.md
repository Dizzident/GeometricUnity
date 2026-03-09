# Stability Atlas: {{atlasId}}

**Branch Manifest ID:** {{branchManifestId}}
**Background Family:** {{familyDescription}}
**Generated:** {{timestamp}}

---

## Linearization Records

| BackgroundStateId | BranchManifestId | GaugeHandlingMode | GaugeLambda | Dimension | AssemblyMode |
|-------------------|------------------|-------------------|-------------|-----------|--------------|
{{#linearizationRecords}}
| {{backgroundStateId}} | {{branchManifestId}} | {{gaugeHandlingMode}} | {{gaugeLambda}} | {{dimension}} | {{assemblyMode}} |
{{/linearizationRecords}}

---

## Hessian Mode Classification

| BackgroundStateId | GaugeHandlingMode | GaugeLambda | CoerciveModeCount | SoftModeCount | NearKernelCount | NegativeModeCount | SymmetryVerified | SymmetryError |
|-------------------|-------------------|-------------|-------------------|---------------|-----------------|-------------------|------------------|---------------|
{{#hessianRecords}}
| {{backgroundStateId}} | {{gaugeHandlingMode}} | {{gaugeLambda}} | {{coerciveModeCount}} | {{softModeCount}} | {{nearKernelCount}} | {{negativeModeCount}} | {{symmetryVerified}} | {{symmetryError}} |
{{/hessianRecords}}

---

## Continuation Path Summary

{{#paths}}
### Path: {{pathId}}

- **Lambda Range:** {{lambdaStart}} to {{lambdaEnd}}
- **Steps:** {{stepCount}}
- **Events:** {{eventCount}}

{{#events}}
- [lambda={{lambda}}] {{kind}}: {{description}}
{{/events}}

{{/paths}}

---

## Bifurcation Indicators

| Lambda | Kind | Description |
|--------|------|-------------|
{{#bifurcationIndicators}}
| {{lambda}} | {{kind}} | {{description}} |
{{/bifurcationIndicators}}
{{^bifurcationIndicators}}
_No bifurcation indicators detected._
{{/bifurcationIndicators}}

---

## Discretization Notes

{{discretizationNotes}}

## Theorem Status Notes

{{theoremStatusNotes}}

---

_Generated from StabilityAtlas. All mode counts are numerical only unless backed by theorem-level proofs noted above._
