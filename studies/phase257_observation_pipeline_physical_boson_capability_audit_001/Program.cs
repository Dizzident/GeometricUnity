using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output";
const string ObservationPipelinePath = "src/Gu.Observation/ObservationPipeline.cs";
const string ObservationTransformsDir = "src/Gu.Observation/Transforms";
const string Phase3ObservationPipelinePath = "src/Gu.Phase3.Observables/ObservationPipeline.cs";
const string Phase3ObservedModeMapperPath = "src/Gu.Phase3.Observables/ObservedModeMapper.cs";
const string Phase3ObservedModeSignaturePath = "src/Gu.Phase3.Observables/ObservedModeSignature.cs";
const string Phase3LinearizedObservationOperatorPath = "src/Gu.Phase3.Observables/LinearizedObservationOperator.cs";
const string FullHessianOperatorPath = "src/Gu.Phase3.Spectra/FullHessianOperator.cs";
const string StateMassOperatorPath = "src/Gu.Phase3.Spectra/StateMassOperator.cs";
const string Minimal4dEnvironmentPath = "examples/minimal_v1_4d/environment.json";
const string Minimal4dBranchPath = "examples/minimal_v1_4d/branch.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE257_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var minimal4dEnvironment = JsonDocument.Parse(File.ReadAllText(Minimal4dEnvironmentPath));
using var minimal4dBranch = JsonDocument.Parse(File.ReadAllText(Minimal4dBranchPath));

var observationPipelineText = File.ReadAllText(ObservationPipelinePath);
var transformFiles = Directory.EnumerateFiles(ObservationTransformsDir, "*.cs", SearchOption.TopDirectoryOnly)
    .Order(StringComparer.Ordinal)
    .ToArray();
var transformRows = transformFiles
    .Select(file =>
    {
        var text = File.ReadAllText(file);
        var observableId = ExtractStringProperty(text, "ObservableId") ?? "unknown";
        var transformId = ExtractStringProperty(text, "TransformId") ?? "unknown";
        var physicalBosonTokenCount = CountPhysicalBosonTokens(text);
        return new TransformRow(NormalizePath(file), observableId, transformId, physicalBosonTokenCount);
    })
    .ToArray();

var directObservableIds = ExtractDirectObservableIds(observationPipelineText);
var directPhysicalBosonObservableIdCount = directObservableIds.Count(IsPhysicalBosonObservableId);
var transformPhysicalBosonObservableIdCount = transformRows.Count(row => IsPhysicalBosonObservableId(row.ObservableId));
var transformPhysicalBosonTokenCount = transformRows.Sum(row => row.PhysicalBosonTokenCount);

var phase3Files = new[]
{
    Phase3ObservationPipelinePath,
    Phase3ObservedModeMapperPath,
    Phase3ObservedModeSignaturePath,
    Phase3LinearizedObservationOperatorPath,
};
var phase3Texts = phase3Files.Select(File.ReadAllText).ToArray();
var phase3PhysicalBosonTokenCount = phase3Texts.Sum(CountPhysicalBosonTokens);
var phase3HasGenericObservedCoefficients = File.ReadAllText(Phase3ObservedModeSignaturePath).Contains("ObservedCoefficients", StringComparison.Ordinal)
    && !File.ReadAllText(Phase3ObservedModeSignaturePath).Contains("ParticleId", StringComparison.Ordinal)
    && !File.ReadAllText(Phase3ObservedModeSignaturePath).Contains("WZ", StringComparison.Ordinal)
    && !File.ReadAllText(Phase3ObservedModeSignaturePath).Contains("Higgs", StringComparison.OrdinalIgnoreCase);

var spectrumOperatorText = File.ReadAllText(FullHessianOperatorPath) + "\n" + File.ReadAllText(StateMassOperatorPath);
var spectrumPhysicalBosonTokenCount = CountPhysicalBosonTokens(spectrumOperatorText);
var fullHessianExists = File.Exists(FullHessianOperatorPath);
var stateMassOperatorExists = File.Exists(StateMassOperatorPath);
var physicalElectroweakMassMatrixApiPresent = Regex.IsMatch(
    spectrumOperatorText,
    """(WBoson|ZBoson|Higgs|Photon|ElectroweakMassMatrix|PhysicalBosonMassMatrix|WeakMixing|VEV)""",
    RegexOptions.CultureInvariant);

var minimal4dScenarioType = JsonString(minimal4dEnvironment.RootElement, "scenarioType");
var minimal4dBaseDimension = JsonInt(minimal4dBranch.RootElement, "baseDimension");
var minimal4dAmbientDimension = JsonInt(minimal4dBranch.RootElement, "ambientDimension");
var minimal4dLieAlgebraId = JsonString(minimal4dBranch.RootElement, "lieAlgebraId");
var minimal4dActiveShiabBranch = JsonString(minimal4dBranch.RootElement, "activeShiabBranch");
var minimal4dObservableIds = minimal4dEnvironment.RootElement.GetProperty("observableRequests")
    .EnumerateArray()
    .Select(row => JsonString(row, "observableId") ?? "")
    .Where(value => value.Length > 0)
    .ToArray();
var minimal4dPhysicalBosonObservableRequestCount = minimal4dObservableIds.Count(IsPhysicalBosonObservableId);
var minimal4dIsToyConsistency = string.Equals(minimal4dScenarioType, "toy-consistency", StringComparison.OrdinalIgnoreCase);

var phase255NoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true;
var phase255BridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var phase256ContractPassed = JsonBool(phase256.RootElement, "observedFieldExtractionIntakeContractPassed") is true;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? 0;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var directObservationPipelineBosonCapable = directPhysicalBosonObservableIdCount > 0
    || transformPhysicalBosonObservableIdCount > 0;
var phase3ObservationPipelineBosonCapable = phase3PhysicalBosonTokenCount > 0 && !phase3HasGenericObservedCoefficients;
var spectrumPhysicalBosonMassMatrixCapable = fullHessianExists
    && stateMassOperatorExists
    && physicalElectroweakMassMatrixApiPresent;
var minimal4dExamplePromotableForBosons = !minimal4dIsToyConsistency
    && minimal4dBaseDimension == 4
    && minimal4dAmbientDimension == 14
    && minimal4dPhysicalBosonObservableRequestCount > 0
    && !string.Equals(minimal4dActiveShiabBranch, "identity-shiab-v1", StringComparison.OrdinalIgnoreCase);
var currentImplementationCanFillObservedFieldExtractionContract = directObservationPipelineBosonCapable
    && phase3ObservationPipelineBosonCapable
    && spectrumPhysicalBosonMassMatrixCapable
    && minimal4dExamplePromotableForBosons
    && phase256ContractPromotable;

var checks = new[]
{
    new Check(
        "direct-observation-pipeline-is-generic-only",
        !directObservationPipelineBosonCapable
            && directObservableIds.Contains("curvature")
            && directObservableIds.Contains("torsion")
            && directObservableIds.Contains("shiab")
            && directObservableIds.Contains("residual"),
        $"directObservableIds=[{string.Join(",", directObservableIds)}]; directPhysicalBosonObservableIdCount={directPhysicalBosonObservableIdCount}; transformPhysicalBosonObservableIdCount={transformPhysicalBosonObservableIdCount}; transformPhysicalBosonTokenCount={transformPhysicalBosonTokenCount}"),
    new Check(
        "phase3-observation-signatures-are-generic-not-particle-rows",
        !phase3ObservationPipelineBosonCapable && phase3HasGenericObservedCoefficients,
        $"phase3PhysicalBosonTokenCount={phase3PhysicalBosonTokenCount}; phase3HasGenericObservedCoefficients={phase3HasGenericObservedCoefficients}"),
    new Check(
        "spectra-operators-are-generic-not-physical-electroweak-mass-matrix",
        fullHessianExists && stateMassOperatorExists && !spectrumPhysicalBosonMassMatrixCapable,
        $"fullHessianExists={fullHessianExists}; stateMassOperatorExists={stateMassOperatorExists}; spectrumPhysicalBosonTokenCount={spectrumPhysicalBosonTokenCount}; physicalElectroweakMassMatrixApiPresent={physicalElectroweakMassMatrixApiPresent}"),
    new Check(
        "minimal-4d-example-is-toy-not-promotable-boson-source",
        !minimal4dExamplePromotableForBosons
            && minimal4dIsToyConsistency
            && minimal4dBaseDimension == 4
            && minimal4dAmbientDimension == 14
            && minimal4dPhysicalBosonObservableRequestCount == 0
            && string.Equals(minimal4dActiveShiabBranch, "identity-shiab-v1", StringComparison.OrdinalIgnoreCase),
        $"scenarioType={minimal4dScenarioType}; baseDimension={minimal4dBaseDimension}; ambientDimension={minimal4dAmbientDimension}; lieAlgebraId={minimal4dLieAlgebraId}; activeShiabBranch={minimal4dActiveShiabBranch}; minimal4dObservableIds=[{string.Join(",", minimal4dObservableIds)}]; minimal4dPhysicalBosonObservableRequestCount={minimal4dPhysicalBosonObservableRequestCount}"),
    new Check(
        "phase255-and-phase256-boundary-inherited",
        phase255NoGoPassed
            && !phase255BridgePromotable
            && phase256ContractPassed
            && phase256RequiredFieldCount >= 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable,
        $"phase255NoGoPassed={phase255NoGoPassed}; phase255BridgePromotable={phase255BridgePromotable}; phase256ContractPassed={phase256ContractPassed}; phase256RequiredFieldCount={phase256RequiredFieldCount}; phase256FilledRequiredFieldCount={phase256FilledRequiredFieldCount}; phase256ContractPromotable={phase256ContractPromotable}"),
    new Check(
        "current-implementation-cannot-fill-observed-field-extraction-contract",
        !currentImplementationCanFillObservedFieldExtractionContract,
        $"currentImplementationCanFillObservedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}; directObservationPipelineBosonCapable={directObservationPipelineBosonCapable}; phase3ObservationPipelineBosonCapable={phase3ObservationPipelineBosonCapable}; spectrumPhysicalBosonMassMatrixCapable={spectrumPhysicalBosonMassMatrixCapable}; minimal4dExamplePromotableForBosons={minimal4dExamplePromotableForBosons}; phase256ContractPromotable={phase256ContractPromotable}"),
};

var observationPipelinePhysicalBosonCapabilityAuditPassed = checks.All(check => check.Passed);
var terminalStatus = observationPipelinePhysicalBosonCapabilityAuditPassed
    ? "observation-pipeline-physical-boson-capability-audit-no-current-implementation-fill"
    : "observation-pipeline-physical-boson-capability-audit-review-required";

var result = new
{
    phaseId = "phase257-observation-pipeline-physical-boson-capability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    observationPipelinePhysicalBosonCapabilityAuditPassed,
    currentImplementationCanFillObservedFieldExtractionContract,
    directObservationPipelineBosonCapable,
    phase3ObservationPipelineBosonCapable,
    spectrumPhysicalBosonMassMatrixCapable,
    minimal4dExamplePromotableForBosons,
    directObservationPipeline = new
    {
        path = ObservationPipelinePath,
        directObservableIds,
        directPhysicalBosonObservableIdCount,
        transformRows,
        transformPhysicalBosonObservableIdCount,
        transformPhysicalBosonTokenCount,
    },
    phase3ObservationPipeline = new
    {
        phase3Files = phase3Files.Select(NormalizePath).ToArray(),
        phase3PhysicalBosonTokenCount,
        phase3HasGenericObservedCoefficients,
    },
    spectraOperators = new
    {
        fullHessianOperatorPath = FullHessianOperatorPath,
        stateMassOperatorPath = StateMassOperatorPath,
        fullHessianExists,
        stateMassOperatorExists,
        spectrumPhysicalBosonTokenCount,
        physicalElectroweakMassMatrixApiPresent,
    },
    minimal4dExample = new
    {
        environmentPath = Minimal4dEnvironmentPath,
        branchPath = Minimal4dBranchPath,
        minimal4dScenarioType,
        minimal4dBaseDimension,
        minimal4dAmbientDimension,
        minimal4dLieAlgebraId,
        minimal4dActiveShiabBranch,
        minimal4dObservableIds,
        minimal4dPhysicalBosonObservableRequestCount,
    },
    currentBlockerEvidence = new
    {
        phase255 = new
        {
            phase255NoGoPassed,
            phase255BridgePromotable,
        },
        phase256 = new
        {
            phase256ContractPassed,
            phase256RequiredFieldCount,
            phase256FilledRequiredFieldCount,
            phase256ContractPromotable,
        },
    },
    checks,
    decision = observationPipelinePhysicalBosonCapabilityAuditPassed
        ? "Do not treat existing observation/linearized-observation/Hessian implementation as filling the observed-field extraction contract. The code supports generic pullback, residual signatures, and generic spectra, but it does not implement physical W/Z/H observable transforms, particle eigenstate rows, or a physical electroweak mass matrix."
        : "Review observation pipeline physical-boson capability audit before relying on current implementation capability boundaries.",
    nextRequiredArtifact = new[]
    {
        "Implement or supply a theorem-backed physical electroweak observation transform with photon/W/Z/H particle rows.",
        "Implement or supply a source-backed physical electroweak mass matrix/eigenstate projection, not only generic FullHessian spectra.",
        "Replace or supplement the toy 4D example with a source-derived observed-sector vacuum and branch-declared Shiab/Upsilon operator that fills Phase256.",
    },
    sourceEvidence = new
    {
        observationPipelinePath = ObservationPipelinePath,
        observationTransformsDir = ObservationTransformsDir,
        phase3ObservationPipelinePath = Phase3ObservationPipelinePath,
        phase3ObservedModeMapperPath = Phase3ObservedModeMapperPath,
        phase3ObservedModeSignaturePath = Phase3ObservedModeSignaturePath,
        phase3LinearizedObservationOperatorPath = Phase3LinearizedObservationOperatorPath,
        fullHessianOperatorPath = FullHessianOperatorPath,
        stateMassOperatorPath = StateMassOperatorPath,
        minimal4dEnvironmentPath = Minimal4dEnvironmentPath,
        minimal4dBranchPath = Minimal4dBranchPath,
        phase255Path = Phase255Path,
        phase256Path = Phase256Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "observation_pipeline_physical_boson_capability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observation_pipeline_physical_boson_capability_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observationPipelinePhysicalBosonCapabilityAuditPassed,
        result.currentImplementationCanFillObservedFieldExtractionContract,
        result.directObservationPipelineBosonCapable,
        result.phase3ObservationPipelineBosonCapable,
        result.spectrumPhysicalBosonMassMatrixCapable,
        result.minimal4dExamplePromotableForBosons,
        result.directObservationPipeline,
        result.phase3ObservationPipeline,
        result.spectraOperators,
        result.minimal4dExample,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observationPipelinePhysicalBosonCapabilityAuditPassed={observationPipelinePhysicalBosonCapabilityAuditPassed}");
Console.WriteLine($"currentImplementationCanFillObservedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}");

static IReadOnlyList<string> ExtractDirectObservableIds(string text)
{
    var ids = new List<string>();
    var switchPattern = new Regex("\"([^\"]+)\"\\s*=>\\s*pulledBack\\.", RegexOptions.CultureInvariant);
    foreach (Match match in switchPattern.Matches(text))
    {
        ids.Add(match.Groups[1].Value);
    }

    return ids.Order(StringComparer.Ordinal).ToArray();
}

static string? ExtractStringProperty(string text, string propertyName)
{
    var pattern = new Regex($@"{Regex.Escape(propertyName)}\s*=>\s*""([^""]+)""", RegexOptions.CultureInvariant);
    var match = pattern.Match(text);
    return match.Success ? match.Groups[1].Value : null;
}

static bool IsPhysicalBosonObservableId(string value) =>
    Regex.IsMatch(value, """(^|[-_])(w|z|higgs|photon|electroweak|weak|mass|vev)([-_]|$)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

static int CountPhysicalBosonTokens(string text) =>
    Regex.Matches(text, """\b(WBoson|ZBoson|Higgs|Photon|Electroweak|WeakMixing|VEV|PhysicalBoson|mass[-\s]?matrix|w-boson|z-boson|higgs)\b""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static string NormalizePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/');

sealed record TransformRow(
    string Path,
    string ObservableId,
    string TransformId,
    int PhysicalBosonTokenCount);

sealed record Check(string CheckId, bool Passed, string Detail);
