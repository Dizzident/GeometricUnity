using System.Text.Json;

const string DefaultOutputDir = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string WzTemplatePath = "studies/phase201_boson_source_lineage_intake_contract_001/output/wz_absolute_source_lineage_intake_template.json";
const string HiggsTemplatePath = "studies/phase201_boson_source_lineage_intake_contract_001/output/higgs_scalar_source_lineage_intake_template.json";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase209SummaryPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE296_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var wzTemplate = JsonDocument.Parse(File.ReadAllText(WzTemplatePath));
using var higgsTemplate = JsonDocument.Parse(File.ReadAllText(HiggsTemplatePath));
using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));

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

var wzSpecs = new[]
{
    new FieldSpec("wz.externalTargetValuesUsed=false", "W/Z source construction must be target-independent.", new[] { "externalTargetValuesUsed=false", "external target values", "target-independent", "target blind", "target-blind" }, new[] { "w", "z", "source", "lineage", "construction" }),
    new FieldSpec("wz.theoremOrDerivationId", "A W/Z direct bridge-source theorem or derivation id.", new[] { "theoremOrDerivationId", "theorem", "derivation", "direct bridge", "bridge-source law", "source theorem" }, new[] { "w", "z", "mass", "sourceLineageId", "target-independent" }),
    new FieldSpec("wz.sourceLineageId", "A source lineage id for the W/Z absolute source.", new[] { "sourceLineageId", "source lineage", "source-lineage" }, new[] { "w", "z", "absolute", "mass", "target-independent" }),
    new FieldSpec("w-boson.sourceRowId", "A W source row derived before target comparison.", new[] { "w-boson", "w boson", "physical-w-boson-mass-gev", "w source row", "wBosonSourceRowId" }, new[] { "sourceRowId", "source row", "derivation", "target-independent" }),
    new FieldSpec("w-boson.rawAmplitudeGatePassed=true", "A W raw-amplitude gate passed from source replay.", new[] { "w raw-amplitude", "w raw amplitude", "wBosonRawAmplitudeGatePassed", "rawAmplitudeGatePassed" }, new[] { "w-boson", "passed", "source replay", "target-independent" }),
    new FieldSpec("w-boson.commonBridgeGatePassed=true", "A W common W/Z bridge gate passed from source normalization.", new[] { "w common bridge", "commonBridgeGatePassed", "wzCommonBridgeGatePassed", "common bridge gate" }, new[] { "w-boson", "passed", "source-derived", "target-independent" }),
    new FieldSpec("w-boson.targetComparisonGatePassed=true", "A W target comparison gate passed after source construction.", new[] { "w target comparison", "targetComparisonGatePassed", "post-construction target comparison" }, new[] { "w-boson", "passed", "after source construction", "target-independent" }),
    new FieldSpec("w-boson.stabilitySidecarsPresent=true", "W branch/refinement/environment/representation/coupling stability sidecars.", new[] { "w stability", "stabilitySidecarsPresent", "stability sidecar" }, new[] { "w-boson", "branch", "refinement", "environment", "representation", "coupling" }),
    new FieldSpec("w-boson.derivationId", "A W row derivation id.", new[] { "w derivation", "derivationId", "w-boson" }, new[] { "source row", "sourceRowId", "target-independent" }),
    new FieldSpec("z-boson.sourceRowId", "A Z source row derived before target comparison.", new[] { "z-boson", "z boson", "physical-z-boson-mass-gev", "z source row", "zBosonSourceRowId" }, new[] { "sourceRowId", "source row", "derivation", "target-independent" }),
    new FieldSpec("z-boson.rawAmplitudeGatePassed=true", "A Z raw-amplitude gate passed from source replay.", new[] { "z raw-amplitude", "z raw amplitude", "zBosonRawAmplitudeGatePassed", "rawAmplitudeGatePassed" }, new[] { "z-boson", "passed", "source replay", "target-independent" }),
    new FieldSpec("z-boson.commonBridgeGatePassed=true", "A Z common W/Z bridge gate passed from source normalization.", new[] { "z common bridge", "commonBridgeGatePassed", "wzCommonBridgeGatePassed", "common bridge gate" }, new[] { "z-boson", "passed", "source-derived", "target-independent" }),
    new FieldSpec("z-boson.targetComparisonGatePassed=true", "A Z target comparison gate passed after source construction.", new[] { "z target comparison", "targetComparisonGatePassed", "post-construction target comparison" }, new[] { "z-boson", "passed", "after source construction", "target-independent" }),
    new FieldSpec("z-boson.stabilitySidecarsPresent=true", "Z branch/refinement/environment/representation/coupling stability sidecars.", new[] { "z stability", "stabilitySidecarsPresent", "stability sidecar" }, new[] { "z-boson", "branch", "refinement", "environment", "representation", "coupling" }),
    new FieldSpec("z-boson.derivationId", "A Z row derivation id.", new[] { "z derivation", "derivationId", "z-boson" }, new[] { "source row", "sourceRowId", "target-independent" }),
};

var higgsSpecs = new[]
{
    new FieldSpec("higgs.externalTargetValuesUsed=false", "Higgs source construction must be target-independent.", new[] { "externalTargetValuesUsed=false", "external target values", "target-independent", "target blind", "target-blind" }, new[] { "higgs", "source", "lineage", "construction" }),
    new FieldSpec("higgs.sourceLineageId", "A source lineage id for the Higgs scalar source.", new[] { "sourceLineageId", "source lineage", "source-lineage" }, new[] { "higgs", "scalar", "target-independent" }),
    new FieldSpec("higgs.scalarSourceOperatorId", "A solved scalar source/operator id.", new[] { "scalarSourceOperatorId", "scalar source operator", "scalar-source operator", "scalar source" }, new[] { "higgs", "operator", "solved", "target-independent" }),
    new FieldSpec("higgs.higgsIdentityEnvelopeId", "A Higgs identity envelope independent of the physical target mass.", new[] { "higgsIdentityEnvelopeId", "higgs identity envelope", "identity envelope" }, new[] { "higgs", "target-independent", "source" }),
    new FieldSpec("higgs.massiveScalarProfileId", "A massive scalar profile or eigenmode for the Higgs.", new[] { "massiveScalarProfileId", "massive scalar profile", "higgs profile", "physical higgs eigenmode" }, new[] { "higgs", "profile", "eigenmode", "target-independent" }),
    new FieldSpec("higgs.potentialOrSelfCouplingSourceId-or-excitationRelationId", "A Higgs potential/self-coupling source id or scalar excitation relation id.", new[] { "potentialOrSelfCouplingSourceId", "excitationRelationId", "higgs potential", "self-coupling", "self coupling", "quartic" }, new[] { "higgs", "source", "relation", "target-independent", "lambda" }),
    new FieldSpec("higgs.stabilitySidecars.branch=true", "A Higgs branch stability sidecar.", new[] { "branch stability", "stabilitySidecars", "stability sidecar" }, new[] { "higgs", "branch", "true", "passed" }),
    new FieldSpec("higgs.stabilitySidecars.refinement=true", "A Higgs refinement stability sidecar.", new[] { "refinement stability", "stabilitySidecars", "stability sidecar" }, new[] { "higgs", "refinement", "true", "passed" }),
    new FieldSpec("higgs.stabilitySidecars.environment=true", "A Higgs environment stability sidecar.", new[] { "environment stability", "stabilitySidecars", "stability sidecar" }, new[] { "higgs", "environment", "true", "passed" }),
    new FieldSpec("higgs.stabilitySidecars.representation=true", "A Higgs representation stability sidecar.", new[] { "representation stability", "stabilitySidecars", "stability sidecar" }, new[] { "higgs", "representation", "true", "passed" }),
    new FieldSpec("higgs.stabilitySidecars.coupling=true", "A Higgs coupling stability sidecar.", new[] { "coupling stability", "stabilitySidecars", "stability sidecar" }, new[] { "higgs", "coupling", "true", "passed" }),
    new FieldSpec("higgs.predictionRow.sourceRowId", "A Higgs prediction-row source row id.", new[] { "predictionRow", "sourceRowId", "higgs source row" }, new[] { "higgs", "physical-higgs-mass-gev", "target-independent" }),
    new FieldSpec("higgs.predictionRow.targetComparisonGatePassed=true", "A Higgs target comparison gate passed after source construction.", new[] { "targetComparisonGatePassed", "post-construction target comparison", "higgs target comparison" }, new[] { "higgs", "passed", "after source construction", "target-independent" }),
    new FieldSpec("higgs.predictionRow.derivationId", "A Higgs prediction-row derivation id.", new[] { "derivationId", "higgs derivation" }, new[] { "higgs", "predictionRow", "source row", "target-independent" }),
};

var fieldSpecs = wzSpecs.Concat(higgsSpecs).ToArray();
var templateFieldCount = CountTemplateRequiredFields(wzTemplate.RootElement, higgsTemplate.RootElement);
var scannedFiles = EnumerateScanFiles(scanRoots).Distinct(StringComparer.Ordinal).ToArray();
var lineHits = ScanFiles(scannedFiles, fieldSpecs);
var fieldResults = fieldSpecs.Select(spec =>
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
}).ToArray();

var wzFieldResults = fieldResults.Take(wzSpecs.Length).ToArray();
var higgsFieldResults = fieldResults.Skip(wzSpecs.Length).ToArray();
var scannedBuckets = scannedFiles
    .GroupBy(TopLevelBucket, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => new ScannedBucket(group.Key, group.Count()))
    .ToArray();

var totalCandidateLineCount = fieldResults.Sum(result => result.CandidateLineCount);
var fieldsWithCandidateLineCount = fieldResults.Count(result => result.CandidateLineCount > 0);
var fieldsWithIntakeReadyCandidateCount = fieldResults.Count(result => result.IntakeReadyCandidateCount > 0);
var intakeReadySourceLineageFieldCandidateCount = fieldResults.Sum(result => result.IntakeReadyCandidateCount);
var wzFieldsWithIntakeReadyCandidateCount = wzFieldResults.Count(result => result.IntakeReadyCandidateCount > 0);
var higgsFieldsWithIntakeReadyCandidateCount = higgsFieldResults.Count(result => result.IntakeReadyCandidateCount > 0);
var allWzFieldsHaveIntakeReadyCandidate = wzFieldResults.All(result => result.Filled);
var allHiggsFieldsHaveIntakeReadyCandidate = higgsFieldResults.All(result => result.Filled);
var anySourceLineageCandidateFillsContract = allWzFieldsHaveIntakeReadyCandidate && allHiggsFieldsHaveIntakeReadyCandidate;

var anySourceLineagePromotable = JsonBool(phase201.RootElement, "anySourceLineagePromotable") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase204IntakeReadyCandidateCount = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? -1;
var phase205IntakeReadyFindingCount = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? -1;
var phase207IntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? -1;
var evidenceRequestPackageMaterialized = JsonBool(phase209.RootElement, "evidenceRequestPackageMaterialized") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase295Passed = JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true;
var phase295AnyCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;

var checks = new[]
{
    new Check(
        "phase201-contract-fields-loaded",
        templateFieldCount == fieldSpecs.Length && wzSpecs.Length == 15 && higgsSpecs.Length == 14,
        $"templateFieldCount={templateFieldCount}; fieldSpecCount={fieldSpecs.Length}; wzFieldCount={wzSpecs.Length}; higgsFieldCount={higgsSpecs.Length}"),
    new Check(
        "broad-source-lineage-corpus-scanned",
        scannedFiles.Length > 0 && scannedBuckets.Length > 0,
        $"scannedFileCount={scannedFiles.Length}; bucketCount={scannedBuckets.Length}; scanRoots={string.Join(",", scanRoots)}"),
    new Check(
        "source-lineage-field-candidate-lines-classified",
        totalCandidateLineCount >= 0 && fieldsWithCandidateLineCount > 0 && fieldResults.Length == fieldSpecs.Length,
        $"totalCandidateLineCount={totalCandidateLineCount}; fieldsWithCandidateLineCount={fieldsWithCandidateLineCount}; fieldResultCount={fieldResults.Length}"),
    new Check(
        "no-intake-ready-source-lineage-field-candidate-found",
        intakeReadySourceLineageFieldCandidateCount == 0
            && fieldsWithIntakeReadyCandidateCount == 0
            && !anySourceLineageCandidateFillsContract,
        $"intakeReadySourceLineageFieldCandidateCount={intakeReadySourceLineageFieldCandidateCount}; fieldsWithIntakeReadyCandidateCount={fieldsWithIntakeReadyCandidateCount}; anySourceLineageCandidateFillsContract={anySourceLineageCandidateFillsContract}"),
    new Check(
        "upstream-source-lineage-scans-remain-empty",
        phase204IntakeReadyCandidateCount == 0 && phase205IntakeReadyFindingCount == 0 && phase207IntakeReadyFindingCount == 0,
        $"phase204IntakeReadyCandidateCount={phase204IntakeReadyCandidateCount}; phase205IntakeReadyFindingCount={phase205IntakeReadyFindingCount}; phase207IntakeReadyFindingCount={phase207IntakeReadyFindingCount}"),
    new Check(
        "source-contracts-remain-unfilled",
        !anySourceLineagePromotable
            && !allRequiredLineagesPromotable
            && evidenceRequestPackageMaterialized
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !unlockContractFilled,
        $"anySourceLineagePromotable={anySourceLineagePromotable}; allRequiredLineagesPromotable={allRequiredLineagesPromotable}; evidenceRequestPackageMaterialized={evidenceRequestPackageMaterialized}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; unlockContractFilled={unlockContractFilled}"),
    new Check(
        "observed-field-extraction-scan-remains-unfilled",
        phase295Passed && !phase295AnyCandidateFillsContract,
        $"phase295Passed={phase295Passed}; phase295AnyCandidateFillsContract={phase295AnyCandidateFillsContract}"),
};

var sourceLineageContractFieldCandidateScanPassed = checks.All(check => check.Passed)
    && !anySourceLineageCandidateFillsContract
    && fieldsWithIntakeReadyCandidateCount == 0
    && !allRequiredLineagesPromotable;
var terminalStatus = sourceLineageContractFieldCandidateScanPassed
    ? "source-lineage-contract-field-candidate-scan-no-intake-ready-artifact"
    : "source-lineage-contract-field-candidate-scan-review-required";

var result = new
{
    phaseId = "phase296-source-lineage-contract-field-candidate-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    sourceLineageContractFieldCandidateScanPassed,
    scannedFileCount = scannedFiles.Length,
    scannedBuckets,
    contractFieldCount = fieldSpecs.Length,
    wzContractFieldCount = wzSpecs.Length,
    higgsContractFieldCount = higgsSpecs.Length,
    totalCandidateLineCount,
    fieldsWithCandidateLineCount,
    fieldsWithIntakeReadyCandidateCount,
    intakeReadySourceLineageFieldCandidateCount,
    wzFieldsWithIntakeReadyCandidateCount,
    higgsFieldsWithIntakeReadyCandidateCount,
    allWzFieldsHaveIntakeReadyCandidate,
    allHiggsFieldsHaveIntakeReadyCandidate,
    anySourceLineageCandidateFillsContract,
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
        phase201 = new
        {
            anySourceLineagePromotable,
            allRequiredLineagesPromotable,
        },
        phase204 = new
        {
            phase204IntakeReadyCandidateCount,
        },
        phase205 = new
        {
            phase205IntakeReadyFindingCount,
        },
        phase207 = new
        {
            phase207IntakeReadyFindingCount,
        },
        phase209 = new
        {
            evidenceRequestPackageMaterialized,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
        },
        phase295 = new
        {
            phase295Passed,
            phase295AnyCandidateFillsContract,
        },
    },
    checks,
    decision = sourceLineageContractFieldCandidateScanPassed
        ? "Do not promote W/Z/H masses on source-lineage contract grounds. The corpus contains candidate mentions for P201/P209 fields, but no intake-ready artifact that fills the W/Z theorem/source rows/gates or Higgs scalar-source/profile/coupling/stability/prediction-row fields."
        : "Review source-lineage field candidates before relying on the current blocker classification.",
    nextRequiredArtifact = new[]
    {
        "A W/Z target-independent source-lineage artifact with theoremOrDerivationId, sourceLineageId, separate W and Z source rows, raw-amplitude/common-bridge/target-comparison gates, stability sidecars, and derivation ids.",
        "A Higgs target-independent scalar-source artifact with scalar source operator, identity envelope, massive profile, potential/self-coupling or excitation source, stability sidecars, and a prediction row.",
        "After both artifacts are filled, rerun P201/P209/P210/P213, P101, P192, P193, and P202.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        wzTemplatePath = WzTemplatePath,
        higgsTemplatePath = HiggsTemplatePath,
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase207Path = Phase207Path,
        phase209SummaryPath = Phase209SummaryPath,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase295Path = Phase295Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_lineage_contract_field_candidate_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_lineage_contract_field_candidate_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sourceLineageContractFieldCandidateScanPassed,
        result.scannedFileCount,
        result.scannedBuckets,
        result.contractFieldCount,
        result.wzContractFieldCount,
        result.higgsContractFieldCount,
        result.totalCandidateLineCount,
        result.fieldsWithCandidateLineCount,
        result.fieldsWithIntakeReadyCandidateCount,
        result.intakeReadySourceLineageFieldCandidateCount,
        result.wzFieldsWithIntakeReadyCandidateCount,
        result.higgsFieldsWithIntakeReadyCandidateCount,
        result.allWzFieldsHaveIntakeReadyCandidate,
        result.allHiggsFieldsHaveIntakeReadyCandidate,
        result.anySourceLineageCandidateFillsContract,
        result.fieldResults,
        result.scanBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceLineageContractFieldCandidateScanPassed={sourceLineageContractFieldCandidateScanPassed}");
Console.WriteLine($"scannedFileCount={scannedFiles.Length}");
Console.WriteLine($"contractFieldCount={fieldSpecs.Length}");
Console.WriteLine($"totalCandidateLineCount={totalCandidateLineCount}");
Console.WriteLine($"fieldsWithCandidateLineCount={fieldsWithCandidateLineCount}");
Console.WriteLine($"fieldsWithIntakeReadyCandidateCount={fieldsWithIntakeReadyCandidateCount}");
Console.WriteLine($"intakeReadySourceLineageFieldCandidateCount={intakeReadySourceLineageFieldCandidateCount}");
Console.WriteLine($"anySourceLineageCandidateFillsContract={anySourceLineageCandidateFillsContract}");

static int CountTemplateRequiredFields(JsonElement wzTemplate, JsonElement higgsTemplate)
{
    var count = 0;
    if (wzTemplate.TryGetProperty("externalTargetValuesUsed", out _))
    {
        count++;
    }

    if (wzTemplate.TryGetProperty("theoremOrDerivationId", out _))
    {
        count++;
    }

    if (wzTemplate.TryGetProperty("sourceLineageId", out _))
    {
        count++;
    }

    if (wzTemplate.TryGetProperty("particleRows", out var wzRows) && wzRows.ValueKind == JsonValueKind.Array)
    {
        count += wzRows.GetArrayLength() * 6;
    }

    if (higgsTemplate.TryGetProperty("externalTargetValuesUsed", out _))
    {
        count++;
    }

    if (higgsTemplate.TryGetProperty("sourceLineageId", out _))
    {
        count++;
    }

    foreach (var property in new[] { "scalarSourceOperatorId", "higgsIdentityEnvelopeId", "massiveScalarProfileId" })
    {
        if (higgsTemplate.TryGetProperty(property, out _))
        {
            count++;
        }
    }

    if (higgsTemplate.TryGetProperty("potentialOrSelfCouplingSourceId", out _)
        || higgsTemplate.TryGetProperty("excitationRelationId", out _))
    {
        count++;
    }

    if (higgsTemplate.TryGetProperty("stabilitySidecars", out var stability) && stability.ValueKind == JsonValueKind.Object)
    {
        count += stability.EnumerateObject().Count();
    }

    if (higgsTemplate.TryGetProperty("predictionRow", out var predictionRow) && predictionRow.ValueKind == JsonValueKind.Object)
    {
        foreach (var property in new[] { "sourceRowId", "targetComparisonGatePassed", "derivationId" })
        {
            if (predictionRow.TryGetProperty(property, out _))
            {
                count++;
            }
        }
    }

    return count;
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
                    "derivation-backed",
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
                    "rawAmplitudeGatePassed=true",
                    "commonBridgeGatePassed=true",
                    "targetComparisonGatePassed=true",
                    "stabilitySidecarsPresent=true",
                    "promotable=true",
                    "filled=true",
                    "TheoremId",
                    "DerivationId",
                    "SourceLineageId",
                    "SourceRowId",
                    "OperatorId",
                    "EnvelopeId",
                    "ProfileId",
                    "RelationId",
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
    || normalizedPath.StartsWith("studies/phase296_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase296_", StringComparison.Ordinal)
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
    || normalizedPath.StartsWith("studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase528_a5_even_sector_premise_applicability_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase529_a5_action_premise_route_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase530_o4_g4_authentication_admissibility_audit_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase531_o4_g4_disposition_resolution_semantics_001/", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/phase532_phase458_g4_consumer_correction_adjudicator_001/", StringComparison.Ordinal)
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
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P527.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P528.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P529.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P530.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P531.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P532.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P533.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P534.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P535.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P536.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P537.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P538.md"
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
