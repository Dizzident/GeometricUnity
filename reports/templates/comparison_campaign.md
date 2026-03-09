# Comparison Campaign: {{campaignId}}

**Mode:** {{mode}}
**Calibration Policy:** {{calibrationPolicy}}
**Completed:** {{completedAt}}

---

## Campaign Scope

- **Environments:** {{#environmentIds}}{{.}}, {{/environmentIds}}
- **Branch Subsets:** {{#branchSubsetIds}}{{.}}, {{/branchSubsetIds}}
- **Observed Output Classes:** {{#observedOutputClassIds}}{{.}}, {{/observedOutputClassIds}}
- **Comparison Assets:** {{#comparisonAssetIds}}{{.}}, {{/comparisonAssetIds}}

---

## Prediction Test Matrix

| TestId | ClaimClass | FormalSource | TheoremDependencyStatus | NumericalDependencyStatus | ApproximationStatus | Falsifier |
|--------|------------|-------------|-------------------------|---------------------------|---------------------|-----------|
{{#predictionTests}}
| {{testId}} | {{claimClass}} | {{formalSource}} | {{theoremDependencyStatus}} | {{numericalDependencyStatus}} | {{approximationStatus}} | {{falsifier}} |
{{/predictionTests}}

---

## Comparison Results

| TestId | Mode | Score | Passed | ResolvedClaimClass | Summary |
|--------|------|-------|--------|--------------------|---------|
{{#runRecords}}
| {{testId}} | {{mode}} | {{score}} | {{passed}} | {{resolvedClaimClass}} | {{summary}} |
{{/runRecords}}

---

## Uncertainty Decomposition

| TestId | Discretization | Solver | Branch | Extraction | Calibration | DataAsset |
|--------|----------------|--------|--------|------------|-------------|-----------|
{{#runRecords}}
| {{testId}} | {{uncertainty.discretization}} | {{uncertainty.solver}} | {{uncertainty.branch}} | {{uncertainty.extraction}} | {{uncertainty.calibration}} | {{uncertainty.dataAsset}} |
{{/runRecords}}

---

## Negative Results and Failures

{{#failures}}
### {{testId}}

- **Failure Reason:** {{reason}}
- **Details:** {{details}}
{{/failures}}
{{^failures}}
_No failures recorded._
{{/failures}}

---

## Calibration

{{#calibration}}
- **Calibration applied:** Yes
- **Details:** {{calibrationSummary}}
{{/calibration}}
{{^calibration}}
_No calibration applied._
{{/calibration}}

---

## Conclusion

**Claim Class Summary:** {{claimClassSummary}}

---

_Generated from ComparisonCampaignSpec and ComparisonCampaignResult. All claims are bounded by the weakest link in their provenance chain._
