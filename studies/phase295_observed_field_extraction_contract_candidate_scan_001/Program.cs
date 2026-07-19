using System.Text.Json;

const string DefaultOutputDir = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase256TemplatePath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_template.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE295_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase256Template = JsonDocument.Parse(File.ReadAllText(Phase256TemplatePath));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));

var scanRoots = new[]
{
    "README.md",
    "OriginalPrompts",
    "TheoryCompletitionRevisions",
    "apps",
    "data",
    "docs/Architecture",
    "docs/Guides",
    "docs/Phases/Gaps",
    "docs/Phases/OpenIssues",
    "docs/Phases/Plans",
    "docs/Phases/Prompts",
    "docs/Phases/Summaries",
    "docs/Reference",
    "examples",
    "native",
    "phase4",
    "reports",
    "schemas",
    "scripts",
    "src",
    "studies",
};

var fieldSpecs = new[]
{
    new FieldSpec(
        "observedFieldExtractionTheoremId",
        "A theorem or source artifact mapping GU variables to observed electroweak fields.",
        new[] { "observedFieldExtractionTheoremId", "observed-field extraction", "observed field extraction", "physical electroweak observation transform", "observed electroweak fields", "observed field content" },
        new[] { "theorem", "derivation", "source artifact", "mapping", "target-independent", "pullback", "observation map" }),
    new FieldSpec(
        "sourceReferenceIds",
        "Primary source references and local artifact paths for the extraction theorem.",
        new[] { "sourceReferenceIds", "source references", "primary source", "local artifact paths", "sourceReference" },
        new[] { "theorem", "derivation", "source artifact", "provenance", "reference" }),
    new FieldSpec(
        "canonicalOrDeclaredShiabBranchId",
        "A canonical Shiab/Upsilon operator choice, or an explicitly declared branch with noncanonicity labelled.",
        new[] { "canonicalOrDeclaredShiabBranchId", "canonical shiab", "declared shiab", "shiab branch", "upsilon branch", "operator choice", "family of shiab" },
        new[] { "canonical", "declared", "branch", "operator", "target-independent", "normalization" }),
    new FieldSpec(
        "branchNormalizationSourceId",
        "A target-independent normalization for the selected branch/operator and inner product.",
        new[] { "branchNormalizationSourceId", "branch normalization", "inner-product normalization", "inner product normalization", "selected branch/operator", "branch/operator" },
        new[] { "normalization", "source", "derivation", "target-independent", "inner product" }),
    new FieldSpec(
        "fourDimensionalObservedVacuumArtifactId",
        "A source-derived four-dimensional observed-sector vacuum/background.",
        new[] { "fourDimensionalObservedVacuumArtifactId", "four-dimensional observed-sector", "4d observed-sector", "observed-sector vacuum", "observed sector vacuum", "source-derived vacuum", "vacuum/background" },
        new[] { "vacuum", "background", "source-derived", "target-independent", "artifact", "selection rule" }),
    new FieldSpec(
        "quadraticElectroweakMassOperatorId",
        "The quadratic physical electroweak mass operator obtained by expansion around the source vacuum.",
        new[] { "quadraticElectroweakMassOperatorId", "quadratic electroweak mass", "electroweak mass operator", "physical electroweak mass matrix", "mass matrix", "mass-matrix" },
        new[] { "quadratic", "operator", "mass matrix", "source vacuum", "eigenstate", "target-independent" }),
    new FieldSpec(
        "electroweakGaugeEmbeddingId",
        "The observed SU(2)L x U(1)Y embedding and weak-mixing convention used by the mass operator.",
        new[] { "electroweakGaugeEmbeddingId", "electroweak gauge embedding", "su(2)l", "u(1)y", "weak-mixing convention", "weak mixing convention", "hypercharge" },
        new[] { "embedding", "weak mixing", "source", "derivation", "target-independent", "mass operator" }),
    new FieldSpec(
        "photonEigenstateProjectionId",
        "A photon projection row with a massless gate before W/Z mass promotion.",
        new[] { "photonEigenstateProjectionId", "photon eigenstate", "photon projection", "massless photon", "a/z mixing", "photon row" },
        new[] { "projection", "eigenstate", "massless gate", "passed", "target-independent", "source row" }),
    new FieldSpec(
        "wBosonSourceRowId",
        "A W source row derived before target comparison.",
        new[] { "wBosonSourceRowId", "w boson source row", "w source row", "w-boson source", "w row" },
        new[] { "source row", "sourceRowId", "derivation", "target-independent", "raw amplitude" }),
    new FieldSpec(
        "zBosonSourceRowId",
        "A Z source row derived before target comparison.",
        new[] { "zBosonSourceRowId", "z boson source row", "z source row", "z-boson source", "z row" },
        new[] { "source row", "sourceRowId", "derivation", "target-independent", "raw amplitude" }),
    new FieldSpec(
        "wBosonRawAmplitudeGatePassed",
        "The W raw-amplitude gate must pass from source replay, not target fit.",
        new[] { "wBosonRawAmplitudeGatePassed", "w raw-amplitude gate", "w raw amplitude gate", "w-boson raw amplitude" },
        new[] { "passed", "source replay", "target-independent", "not target fit", "source row" }),
    new FieldSpec(
        "zBosonRawAmplitudeGatePassed",
        "The Z raw-amplitude gate must pass from source replay, not target fit.",
        new[] { "zBosonRawAmplitudeGatePassed", "z raw-amplitude gate", "z raw amplitude gate", "z-boson raw amplitude" },
        new[] { "passed", "source replay", "target-independent", "not target fit", "source row" }),
    new FieldSpec(
        "wzCommonBridgeGatePassed",
        "The W/Z common bridge must pass with a source-derived common normalization.",
        new[] { "wzCommonBridgeGatePassed", "w/z common bridge", "common wz bridge", "common w/z bridge", "common bridge gate" },
        new[] { "passed", "source-derived", "common normalization", "target-independent", "source row" }),
    new FieldSpec(
        "higgsScalarSourceOperatorId",
        "A scalar source/operator identifying the Higgs sector.",
        new[] { "higgsScalarSourceOperatorId", "higgs scalar source", "scalar source/operator", "scalar-source operator", "higgs sector" },
        new[] { "operator", "source", "derivation", "target-independent", "higgs" }),
    new FieldSpec(
        "higgsMassiveScalarProfileId",
        "A massive scalar profile/eigenmode for the physical Higgs.",
        new[] { "higgsMassiveScalarProfileId", "massive scalar profile", "scalar profile", "higgs profile", "physical higgs eigenmode" },
        new[] { "profile", "eigenmode", "source", "derivation", "target-independent" }),
    new FieldSpec(
        "higgsPotentialSelfCouplingRelationId",
        "A target-independent Higgs potential/self-coupling or excitation relation.",
        new[] { "higgsPotentialSelfCouplingRelationId", "higgs potential", "self-coupling", "self coupling", "quartic", "excitation relation" },
        new[] { "relation", "source", "derivation", "target-independent", "lambda", "operator" }),
    new FieldSpec(
        "targetBlindConstructionHash",
        "A hash or provenance record proving construction before physical target comparison.",
        new[] { "targetBlindConstructionHash", "target-blind", "target blind", "construction hash", "before physical target comparison", "construction before target" },
        new[] { "hash", "provenance", "target-independent", "not from target", "before comparison" }),
    new FieldSpec(
        "stabilitySidecarIds",
        "Branch/refinement/stability sidecars for W, Z, photon, and Higgs projections.",
        new[] { "stabilitySidecarIds", "stability sidecar", "branch stability", "refinement stability", "environment stability", "representation stability", "coupling stability" },
        new[] { "sidecar", "branch", "refinement", "environment", "representation", "coupling" }),
    new FieldSpec(
        "targetComparisonAfterConstructionGatePassed",
        "Target comparison is allowed only after all source construction gates are filled.",
        new[] { "targetComparisonAfterConstructionGatePassed", "target comparison after construction", "post-construction target comparison", "after construction gate" },
        new[] { "passed", "target-independent", "source construction", "gate", "not target" }),
    new FieldSpec(
        "phase201And209ApplicationReady",
        "The artifact must be ready to fill P201/P209/P210/P213 W/Z and Higgs fields.",
        new[] { "phase201And209ApplicationReady", "p201", "p209", "phase201", "phase209", "application ready" },
        new[] { "ready", "sourceLineageId", "source row", "target-independent", "theoremOrDerivationId" }),
};

var templateFieldIds = ReadTemplateFieldIds(phase256Template.RootElement);
var scannedFiles = EnumerateScanFiles(scanRoots).Distinct(StringComparer.Ordinal).ToArray();
var lineHits = ScanFiles(scannedFiles, fieldSpecs);
var fieldResults = fieldSpecs
    .Select(spec =>
    {
        var hits = lineHits.Where(hit => hit.FieldId == spec.FieldId).ToArray();
        var intakeReadyHits = hits.Where(hit => hit.IntakeReadyCandidate).ToArray();
        var strongestHits = hits
            .OrderByDescending(hit => hit.ReadinessScore)
            .ThenBy(hit => hit.Path, StringComparer.Ordinal)
            .ThenBy(hit => hit.LineNumber)
            .Take(8)
            .ToArray();

        return new FieldScanResult(
            spec.FieldId,
            spec.Acceptance,
            CandidateLineCount: hits.Length,
            IntakeReadyCandidateCount: intakeReadyHits.Length,
            Filled: intakeReadyHits.Length > 0,
            StrongestHits: strongestHits);
    })
    .ToArray();

var scannedBuckets = scannedFiles
    .GroupBy(TopLevelBucket, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => new ScannedBucket(group.Key, group.Count()))
    .ToArray();

var totalCandidateLineCount = fieldResults.Sum(result => result.CandidateLineCount);
var fieldsWithCandidateLineCount = fieldResults.Count(result => result.CandidateLineCount > 0);
var fieldsWithIntakeReadyCandidateCount = fieldResults.Count(result => result.IntakeReadyCandidateCount > 0);
var intakeReadyObservedFieldExtractionCandidateCount = fieldResults.Sum(result => result.IntakeReadyCandidateCount);
var allContractFieldsHaveIntakeReadyCandidate = fieldResults.All(result => result.Filled);
var anyObservedFieldExtractionCandidateFillsContract = allContractFieldsHaveIntakeReadyCandidate && fieldResults.Length == templateFieldIds.Length;

var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase255NoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true;
var phase255BridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var phase255NewArtifactRequired = JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired") is true;
var phase257Passed = JsonBool(phase257.RootElement, "observationPipelinePhysicalBosonCapabilityAuditPassed") is true;
var phase257CurrentImplementationCanFill = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var officialDraftParameterSourceGapAuditPassed = JsonBool(phase287.RootElement, "officialDraftParameterSourceGapAuditPassed") is true;
var officialGuParameterLocationLeadPresent = JsonBool(phase287.RootElement, "officialGuParameterLocationLeadPresent") is true;
var officialDraftProvidesHiggsScalarExtraction = JsonBool(phase287.RootElement, "officialDraftProvidesHiggsScalarExtraction") is true;
var officialDraftProvidesTargetIndependentVevSource = JsonBool(phase287.RootElement, "officialDraftProvidesTargetIndependentVevSource") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "phase256-contract-fields-loaded",
        templateFieldIds.Length == fieldSpecs.Length && phase256RequiredFieldCount == fieldSpecs.Length,
        $"templateFieldCount={templateFieldIds.Length}; fieldSpecCount={fieldSpecs.Length}; phase256RequiredFieldCount={phase256RequiredFieldCount}"),
    new Check(
        "broad-contract-corpus-scanned",
        scannedFiles.Length > 0 && scannedBuckets.Length > 0,
        $"scannedFileCount={scannedFiles.Length}; bucketCount={scannedBuckets.Length}; scanRoots={string.Join(",", scanRoots)}"),
    new Check(
        "observed-field-extraction-candidate-lines-classified",
        totalCandidateLineCount >= 0 && fieldResults.Length == fieldSpecs.Length,
        $"totalCandidateLineCount={totalCandidateLineCount}; fieldsWithCandidateLineCount={fieldsWithCandidateLineCount}; fieldResultCount={fieldResults.Length}"),
    new Check(
        "no-intake-ready-observed-field-extraction-candidate-found",
        intakeReadyObservedFieldExtractionCandidateCount == 0
            && fieldsWithIntakeReadyCandidateCount == 0
            && !anyObservedFieldExtractionCandidateFillsContract,
        $"intakeReadyObservedFieldExtractionCandidateCount={intakeReadyObservedFieldExtractionCandidateCount}; fieldsWithIntakeReadyCandidateCount={fieldsWithIntakeReadyCandidateCount}; anyObservedFieldExtractionCandidateFillsContract={anyObservedFieldExtractionCandidateFillsContract}"),
    new Check(
        "phase255-256-257-blockers-preserved",
        phase255NoGoPassed
            && !phase255BridgePromotable
            && phase255NewArtifactRequired
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable
            && phase257Passed
            && !phase257CurrentImplementationCanFill,
        $"phase255NoGoPassed={phase255NoGoPassed}; phase255BridgePromotable={phase255BridgePromotable}; phase255NewArtifactRequired={phase255NewArtifactRequired}; phase256FilledRequiredFieldCount={phase256FilledRequiredFieldCount}; phase256ContractPromotable={phase256ContractPromotable}; phase257Passed={phase257Passed}; phase257CurrentImplementationCanFill={phase257CurrentImplementationCanFill}"),
    new Check(
        "official-draft-symbolic-leads-still-do-not-fill-extraction",
        officialDraftParameterSourceGapAuditPassed
            && officialGuParameterLocationLeadPresent
            && !officialDraftProvidesHiggsScalarExtraction
            && !officialDraftProvidesTargetIndependentVevSource
            && !officialDraftFillsPhase286Gaps,
        $"officialDraftParameterSourceGapAuditPassed={officialDraftParameterSourceGapAuditPassed}; officialGuParameterLocationLeadPresent={officialGuParameterLocationLeadPresent}; officialDraftProvidesHiggsScalarExtraction={officialDraftProvidesHiggsScalarExtraction}; officialDraftProvidesTargetIndependentVevSource={officialDraftProvidesTargetIndependentVevSource}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}"),
    new Check(
        "source-lineage-blockers-still-present",
        wzMissingFieldCount == 15 && higgsMissingFieldCount == 14,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var observedFieldExtractionContractCandidateScanPassed = checks.All(check => check.Passed)
    && !anyObservedFieldExtractionCandidateFillsContract
    && fieldsWithIntakeReadyCandidateCount == 0
    && !phase256ContractPromotable
    && !phase257CurrentImplementationCanFill;
var terminalStatus = observedFieldExtractionContractCandidateScanPassed
    ? "observed-field-extraction-contract-candidate-scan-no-intake-ready-artifact"
    : "observed-field-extraction-contract-candidate-scan-review-required";

var result = new
{
    phaseId = "phase295-observed-field-extraction-contract-candidate-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    observedFieldExtractionContractCandidateScanPassed,
    scannedFileCount = scannedFiles.Length,
    scannedBuckets,
    contractFieldCount = fieldSpecs.Length,
    templateFieldIds,
    totalCandidateLineCount,
    fieldsWithCandidateLineCount,
    fieldsWithIntakeReadyCandidateCount,
    intakeReadyObservedFieldExtractionCandidateCount,
    allContractFieldsHaveIntakeReadyCandidate,
    anyObservedFieldExtractionCandidateFillsContract,
    fieldResults,
    scanBoundary = new
    {
        scanRoots,
        excludedOutputBinObj = true,
        excludedImplementationDocsAndJournal = true,
        generatedStudiesAreCountedButNotIntakeReady = true,
        candidateMentionsAreNotContractFillers = true,
    },
    inheritedEvidence = new
    {
        phase255 = new
        {
            phase255NoGoPassed,
            phase255BridgePromotable,
            phase255NewArtifactRequired,
        },
        phase256 = new
        {
            phase256RequiredFieldCount,
            phase256FilledRequiredFieldCount,
            phase256ContractPromotable,
        },
        phase257 = new
        {
            phase257Passed,
            phase257CurrentImplementationCanFill,
        },
        phase287 = new
        {
            officialDraftParameterSourceGapAuditPassed,
            officialGuParameterLocationLeadPresent,
            officialDraftProvidesHiggsScalarExtraction,
            officialDraftProvidesTargetIndependentVevSource,
            officialDraftFillsPhase286Gaps,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks,
    decision = observedFieldExtractionContractCandidateScanPassed
        ? "Do not promote W/Z/H masses on observed-field extraction grounds. The corpus contains many symbolic, requirement, and diagnostic mentions, but no intake-ready artifact that fills the Phase256 theorem, branch, vacuum, mass-operator, photon/W/Z/H projection, target-blindness, stability, and application fields."
        : "Review observed-field extraction candidates before relying on the current blocker classification.",
    nextRequiredArtifact = new[]
    {
        "A target-independent observed-field extraction theorem that maps GU variables to physical electroweak fields.",
        "A declared Shiab/Upsilon branch with branch normalization, a source-derived four-dimensional observed-sector vacuum, and a quadratic physical electroweak mass operator.",
        "Photon, W, Z, and Higgs projection/source rows with source-replayed gates and stability sidecars before any physical target comparison.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase255Path = Phase255Path,
        phase256Path = Phase256Path,
        phase256TemplatePath = Phase256TemplatePath,
        phase257Path = Phase257Path,
        phase287Path = Phase287Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "observed_field_extraction_contract_candidate_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observed_field_extraction_contract_candidate_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observedFieldExtractionContractCandidateScanPassed,
        result.scannedFileCount,
        result.scannedBuckets,
        result.contractFieldCount,
        result.totalCandidateLineCount,
        result.fieldsWithCandidateLineCount,
        result.fieldsWithIntakeReadyCandidateCount,
        result.intakeReadyObservedFieldExtractionCandidateCount,
        result.allContractFieldsHaveIntakeReadyCandidate,
        result.anyObservedFieldExtractionCandidateFillsContract,
        result.fieldResults,
        result.scanBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observedFieldExtractionContractCandidateScanPassed={observedFieldExtractionContractCandidateScanPassed}");
Console.WriteLine($"scannedFileCount={scannedFiles.Length}");
Console.WriteLine($"contractFieldCount={fieldSpecs.Length}");
Console.WriteLine($"totalCandidateLineCount={totalCandidateLineCount}");
Console.WriteLine($"fieldsWithCandidateLineCount={fieldsWithCandidateLineCount}");
Console.WriteLine($"fieldsWithIntakeReadyCandidateCount={fieldsWithIntakeReadyCandidateCount}");
Console.WriteLine($"intakeReadyObservedFieldExtractionCandidateCount={intakeReadyObservedFieldExtractionCandidateCount}");
Console.WriteLine($"anyObservedFieldExtractionCandidateFillsContract={anyObservedFieldExtractionCandidateFillsContract}");

static string[] ReadTemplateFieldIds(JsonElement template)
{
    if (!template.TryGetProperty("requirementRows", out var rows) || rows.ValueKind != JsonValueKind.Array)
    {
        return Array.Empty<string>();
    }

    return rows.EnumerateArray()
        .Select(row => JsonString(row, "fieldId"))
        .Where(fieldId => !string.IsNullOrWhiteSpace(fieldId))
        .Select(fieldId => fieldId!)
        .ToArray();
}

static IEnumerable<string> EnumerateScanFiles(IEnumerable<string> roots)
{
    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".c",
        ".cpp",
        ".cs",
        ".csproj",
        ".cu",
        ".h",
        ".hpp",
        ".json",
        ".md",
        ".sh",
        ".txt",
        ".yaml",
        ".yml",
    };

    foreach (var root in roots)
    {
        if (File.Exists(root))
        {
            var normalizedFile = NormalizePath(root);
            if (extensions.Contains(Path.GetExtension(root)) && !IsExcluded(normalizedFile))
            {
                yield return normalizedFile;
            }

            continue;
        }

        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var normalized = NormalizePath(path);
            if (extensions.Contains(Path.GetExtension(path)) && !IsExcluded(normalized))
            {
                yield return normalized;
            }
        }
    }
}

static IReadOnlyList<LineHit> ScanFiles(IReadOnlyList<string> files, IReadOnlyList<FieldSpec> fieldSpecs)
{
    var hits = new List<LineHit>();
    foreach (var file in files)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(file);
        }
        catch
        {
            continue;
        }

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var context = BuildContext(lines, i, 2);
            foreach (var spec in fieldSpecs)
            {
                if (!ContainsAny(context, spec.TriggerTerms))
                {
                    continue;
                }

                var fieldSpecificTermCount = CountTerms(context, spec.TriggerTerms);
                var supportTermCount = CountTerms(context, spec.SupportTerms);
                var sourceProvenanceLike = supportTermCount > 0 && ContainsAny(context, new[]
                {
                    "sourceLineageId",
                    "source row",
                    "source-row",
                    "sourceRowId",
                    "source artifact",
                    "source-derived",
                    "theorem",
                    "theoremOrDerivationId",
                    "derivation",
                    "proof",
                });
                var targetBlindConstruction = ContainsAny(context, new[]
                {
                    "target-independent",
                    "target blind",
                    "target-blind",
                    "externalTargetValuesUsed=false",
                    "not from target",
                    "before physical target comparison",
                    "before target comparison",
                    "post-construction",
                });
                var gateOrIdentifierSatisfied = ContainsAny(context, new[]
                {
                    "passed=true",
                    "gatePassed=true",
                    "promotable=true",
                    "promotionAllowed=true",
                    "filled=true",
                    "TheoremId",
                    "ArtifactId",
                    "OperatorId",
                    "EmbeddingId",
                    "ProjectionId",
                    "SourceRowId",
                    "RelationId",
                    "ConstructionHash",
                    "SidecarIds",
                    "sourceLineageId",
                    "theoremOrDerivationId",
                });
                var negativeContext = HasNegativeOrRequirementContext(context);
                var generatedOrPromptContext = HasGeneratedOrPromptContext(file, context);
                var intakeReadyCandidate = fieldSpecificTermCount > 0
                    && sourceProvenanceLike
                    && targetBlindConstruction
                    && gateOrIdentifierSatisfied
                    && !negativeContext
                    && !generatedOrPromptContext;
                var readinessScore = fieldSpecificTermCount
                    + supportTermCount
                    + (sourceProvenanceLike ? 3 : 0)
                    + (targetBlindConstruction ? 3 : 0)
                    + (gateOrIdentifierSatisfied ? 2 : 0)
                    - (negativeContext ? 4 : 0)
                    - (generatedOrPromptContext ? 3 : 0);

                hits.Add(new LineHit(
                    spec.FieldId,
                    file,
                    i + 1,
                    line.Length > 280 ? line[..280] : line,
                    readinessScore,
                    fieldSpecificTermCount,
                    supportTermCount,
                    sourceProvenanceLike,
                    targetBlindConstruction,
                    gateOrIdentifierSatisfied,
                    negativeContext,
                    generatedOrPromptContext,
                    intakeReadyCandidate));
            }
        }
    }

    return hits;
}

static string BuildContext(IReadOnlyList<string> lines, int center, int radius)
{
    var start = Math.Max(0, center - radius);
    var end = Math.Min(lines.Count - 1, center + radius);
    return string.Join('\n', Enumerable.Range(start, end - start + 1).Select(index => lines[index].Trim()));
}

static bool IsExcluded(string normalizedPath) =>
    normalizedPath.Contains("/bin/", StringComparison.Ordinal)
    || normalizedPath.Contains("/obj/", StringComparison.Ordinal)
    || normalizedPath.Contains("/output/", StringComparison.Ordinal)
    || normalizedPath.Contains("scripts/incremental/", StringComparison.Ordinal)
    || normalizedPath.Contains("scripts/o4_register/", StringComparison.Ordinal)
    || normalizedPath.Contains("scripts/boson_incremental_manifest.json", StringComparison.Ordinal)
    || normalizedPath.Contains("/.git/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.claude/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.agents/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/Phases/Implementation/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md", StringComparison.Ordinal)
    || normalizedPath == "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md"
    || normalizedPath == "ExperimentReferences.md"
    || normalizedPath.StartsWith("docs/Reference/ExperimentReferences/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase295_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase295_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase311_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase311_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase312_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase312_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase313_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase313_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase314_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase314_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase315_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase315_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase316_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase316_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase317_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase317_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase318_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase318_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase319_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase319_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase320_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase320_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase321_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase321_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase322_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase322_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase323_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase323_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase324_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase324_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase325_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase325_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase326_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase326_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase327_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase327_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase328_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase328_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase329_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase329_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase330_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase330_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase331_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase331_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase332_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase332_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase333_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase333_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase334_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase334_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase335_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase335_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase336_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase336_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase337_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase337_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase338_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase338_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase339_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase339_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase340_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase340_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase341_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase341_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase342_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase342_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase343_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase343_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase344_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase344_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase345_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase345_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase346_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase346_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase347_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase347_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase348_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase348_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase349_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase349_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase350_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase350_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase351_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase351_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase352_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase352_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase353_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase353_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase354_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase354_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase355_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase355_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase356_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase356_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase357_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase357_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase358_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase358_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase359_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase359_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase360_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase360_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase361_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase361_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase362_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase362_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase363_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase363_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase364_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase364_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase365_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase365_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase366_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase366_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase367_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase367_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase368_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase368_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase369_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase369_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase370_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase370_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase371_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase371_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase372_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase372_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase373_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase373_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase374_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase374_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase375_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase375_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase376_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase376_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase377_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase377_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase378_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase378_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase379_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase379_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase380_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase380_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase381_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase381_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase382_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase382_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase383_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase383_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase384_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase384_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase385_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase385_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase386_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase386_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase387_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase387_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase388_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase388_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase389_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase389_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase390_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase390_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase391_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase391_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase392_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase392_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase393_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase393_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase394_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase394_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase395_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase395_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase396_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase396_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase397_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase397_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase398_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase398_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase399_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase399_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase400_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase400_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase401_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase401_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase402_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase402_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase403_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase403_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase404_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase404_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase405_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase405_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase406_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase406_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase407_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase407_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase408_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase408_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase409_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase409_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase410_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase410_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase411_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase411_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase412_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase412_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase413_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase413_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase414_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase414_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase415_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase415_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase416_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase416_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase417_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase417_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase418_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase418_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase419_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase419_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase420_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase420_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase421_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase421_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase422_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase422_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase423_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase423_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase424_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase424_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase425_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase425_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase426_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase426_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase427_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase427_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase428_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase428_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase429_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase429_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase430_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase430_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase431_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase431_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase432_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase432_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase433_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase433_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase434_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase434_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase435_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase435_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase436_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase436_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase437_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase437_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase438_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase438_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase439_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase439_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase440_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase440_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase441_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase441_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase442_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase442_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase443_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase443_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase444_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase444_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase445_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase445_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase446_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase446_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase447_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase447_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase448_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase448_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase449_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase449_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase450_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase450_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase451_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase451_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase452_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase452_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase453_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase453_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase454_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase454_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase455_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase455_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase456_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase456_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase457_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase457_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase458_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase458_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase459_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase459_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase460_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase460_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase461_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase461_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase462_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase462_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase463_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase463_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase464_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase464_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase465_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase465_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase466_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase466_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase467_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase467_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase468_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase468_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase469_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase469_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase470_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase470_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase471_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase471_", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase477_o4_adjudication_infrastructure_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase477_o4_adjudication_infrastructure_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase478_phase458_gate_specification_closure_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase478_phase458_gate_specification_closure_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase479_phase457_post_ruling_readiness_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase479_phase457_post_ruling_readiness_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase480_o4_physicist_adjudication_intake_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase480_o4_physicist_adjudication_intake_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase481_phase456_prospective_repair_preregistration_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase481_phase456_prospective_repair_preregistration_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase482_a5_theorem_scout_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase482_a5_theorem_scout_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase483_source_defined_reopening_intake_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase483_source_defined_reopening_intake_001/", StringComparison.Ordinal)
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P477.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P478.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P479.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P480.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P481.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P482.md"
    || normalizedPath.StartsWith("studies/phase484_exploratory_lane_governance_firewall_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase485_o4_assumption_falsifier_census_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase486_committed_evidence_sensitivity_triage_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase487_independent_so3_haar_measure_control_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase488_haar_proposal_invariance_control_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase489_reduced_sampler_restart_equivalence_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase490_zero_mode_quotient_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase491_committed_bosonic_model_family_sensitivity_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase492_phase455_combined_robustness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase493_phase456_stored_artifact_failure_decomposition_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase494_phase456_estimator_oracle_battery_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase496_phase456_retained_data_information_census_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase499_phase456_retained_empirical_noise_information_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase502_phase456_adaptive_calibration_protocol_specification_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase503_phase456_adaptive_calibration_protocol_validation_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase504_phase456_calibration_repair_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase505_phase503_frozen_failure_localization_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase506_phase456_selective_inference_protocol_validation_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase507_phase456_selective_inference_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase508_phase481_acquisition_geometry_closure_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase510_phase481_execution_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase511_phase481_throughput_benchmark_eligibility_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase514_a5_registered_reflection_foundation_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase517_a5_dual_reflection_candidate_foundation_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase518_a5_dual_reflection_exact_consistency_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase519_a5_candidate_foundation_readiness_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase520_a5_action_subject_lineage_parity_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase522_a5_foundation_candidate_reduction_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase523_a5_action_member_universalization_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase524_a5_exact_omega_parity_decomposition_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase526_a5_certificate_reducer_001/", StringComparison.Ordinal)
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P483.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P484.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P485.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P486.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P487.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P488.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P489.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P490.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P491.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P492.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P493.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P494.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P495.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P496.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P497.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P498.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P499.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P500.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P501.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P502.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P503.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P504.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P505.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P506.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P507.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P508.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P509.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P510.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P511.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P514.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P517.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P518.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P519.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P520.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P521.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P522.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P523.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P524.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P525.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P526.md"
    || normalizedPath == "docs/Phases/EXPLORATORY_SELF_AUDIT_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15.md";

static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('.', '/');

static string TopLevelBucket(string normalizedPath)
{
    if (normalizedPath == "README.md")
    {
        return "root-readme";
    }

    var slash = normalizedPath.IndexOf('/', StringComparison.Ordinal);
    if (slash < 0)
    {
        return "root";
    }

    if (normalizedPath.StartsWith("docs/Phases/", StringComparison.Ordinal))
    {
        var parts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3 ? $"docs/Phases/{parts[2]}" : "docs/Phases";
    }

    if (normalizedPath.StartsWith("studies/", StringComparison.Ordinal))
    {
        return "studies";
    }

    return normalizedPath[..slash];
}

static bool ContainsAny(string text, IEnumerable<string> terms) =>
    terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static int CountTerms(string text, IEnumerable<string> terms) =>
    terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static bool HasNegativeOrRequirementContext(string text)
{
    var normalized = text
        .Replace("externalTargetValuesUsed=false", "externalTargetValuesUsed target-blind", StringComparison.OrdinalIgnoreCase)
        .Replace("not from target", "target-blind", StringComparison.OrdinalIgnoreCase)
        .Replace("not target fit", "source-replay", StringComparison.OrdinalIgnoreCase);

    return ContainsAny(normalized, new[]
    {
        "missing",
        "unfilled",
        "placeholder",
        "blocked",
        "required",
        "requires",
        "require ",
        "request",
        "gap",
        "future",
        "todo",
        "cannot",
        "can't",
        "not promotable",
        "non-promotable",
        "nonpromotion",
        "diagnostic",
        "audit",
        "obstruction",
        "candidate",
        "template",
        "false",
        "no ",
        "not ",
    });
}

static bool HasGeneratedOrPromptContext(string path, string text) =>
    path.StartsWith("OriginalPrompts/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/Gaps/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/OpenIssues/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/Plans/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/Prompts/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/Summaries/", StringComparison.Ordinal)
    || path.StartsWith("reports/", StringComparison.Ordinal)
    || path.StartsWith("scripts/", StringComparison.Ordinal)
    || path.StartsWith("studies/phase", StringComparison.Ordinal)
    || ContainsAny(text, new[]
    {
        "prompt",
        "open issue",
        "request package",
        "diagnosis",
        "generated",
        "audit",
        "scan",
        "contract",
    });

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
    {
        return null;
    }

    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

sealed record FieldSpec(string FieldId, string Acceptance, IReadOnlyList<string> TriggerTerms, IReadOnlyList<string> SupportTerms);

sealed record FieldScanResult(
    string FieldId,
    string Acceptance,
    int CandidateLineCount,
    int IntakeReadyCandidateCount,
    bool Filled,
    IReadOnlyList<LineHit> StrongestHits);

sealed record LineHit(
    string FieldId,
    string Path,
    int LineNumber,
    string Excerpt,
    int ReadinessScore,
    int FieldSpecificTermCount,
    int SupportTermCount,
    bool SourceProvenanceLike,
    bool TargetBlindConstruction,
    bool GateOrIdentifierSatisfied,
    bool NegativeContext,
    bool GeneratedOrPromptContext,
    bool IntakeReadyCandidate);

sealed record ScannedBucket(string Bucket, int FileCount);

sealed record Check(string CheckId, bool Passed, string Detail);
