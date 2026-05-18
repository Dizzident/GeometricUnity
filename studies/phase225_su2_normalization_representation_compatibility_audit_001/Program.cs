using System.Text.Json;

const string DefaultOutputDir = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output";
const string Phase63Path = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";
const string Phase64Path = "studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase222Path = "studies/phase222_wz_raw_amplitude_source_obstruction_audit_001/output/wz_raw_amplitude_source_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE225_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase63 = JsonDocument.Parse(File.ReadAllText(Phase63Path));
using var phase64 = JsonDocument.Parse(File.ReadAllText(Phase64Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase222 = JsonDocument.Parse(File.ReadAllText(Phase222Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));

var traceHalfConventionDerived = string.Equals(JsonString(phase63.RootElement, "terminalStatus"), "su2-generator-normalization-derived", StringComparison.Ordinal)
    && string.Equals(JsonString(phase63.RootElement, "normalizationConventionId"), "physical-weak-coupling-normalization:su2-canonical-trace-half-v1", StringComparison.Ordinal);
var phase64FermionCurrentDerived = string.Equals(JsonString(phase64.RootElement, "terminalStatus"), "non-proxy-fermion-current-matrix-element-derived", StringComparison.Ordinal)
    && string.Equals(JsonString(phase64.RootElement, "sourceKind"), "non-proxy-fermion-current-matrix-element", StringComparison.Ordinal);
var phase64UsesTraceHalfConvention = string.Equals(JsonString(phase64.RootElement, "normalizationConventionId"), JsonString(phase63.RootElement, "normalizationConventionId"), StringComparison.Ordinal);
var phase221NumericalLead = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var phase221Promotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var rawAmplitudeObstructionCertified = JsonBool(phase222.RootElement, "rawAmplitudeSourceObstructionCertified") is true;
var electroweakParameterClosureBlocked = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true
    && phase224.RootElement.TryGetProperty("closure", out var closure)
    && JsonBool(closure, "wAbsoluteMassParameterClosure") is false
    && JsonBool(closure, "zAbsoluteMassParameterClosure") is false;

const double FundamentalSpin = 0.5;
const double AdjointSpin = 1.0;
const double Su2AlgebraDimension = 3.0;
var fundamentalCasimir = FundamentalSpin * (FundamentalSpin + 1.0);
var adjointCasimir = AdjointSpin * (AdjointSpin + 1.0);
var traceHalfSingleGeneratorScale = JsonDouble(phase63.RootElement, "internalToPhysicalGeneratorScale");
var adjointTripletRmsScale = Math.Sqrt(adjointCasimir / Su2AlgebraDimension);
var adjointToTraceHalfRatio = traceHalfSingleGeneratorScale is > 0.0
    ? adjointTripletRmsScale / traceHalfSingleGeneratorScale.Value
    : (double?)null;

var checks = new[]
{
    new Check("phase63-trace-half-convention-derived", traceHalfConventionDerived, $"normalizationConventionId={JsonString(phase63.RootElement, "normalizationConventionId")}; physicalTraceNormalization={JsonString(phase63.RootElement, "physicalTraceNormalization")}"),
    new Check("phase64-fermion-current-derived", phase64FermionCurrentDerived, $"sourceKind={JsonString(phase64.RootElement, "sourceKind")}; matrixElementFormula={JsonString(phase64.RootElement, "matrixElementFormula")}"),
    new Check("phase64-uses-phase63-trace-half-convention", phase64UsesTraceHalfConvention, $"phase64NormalizationConventionId={JsonString(phase64.RootElement, "normalizationConventionId")}"),
    new Check("phase221-casimir-lead-numerical-only", phase221NumericalLead && !phase221Promotable, $"numericalTargetComparisonPassed={phase221NumericalLead}; sourceLineagePromotable={phase221Promotable}"),
    new Check("phase222-raw-amplitude-obstruction-certified", rawAmplitudeObstructionCertified, $"rawAmplitudeSourceObstructionCertified={rawAmplitudeObstructionCertified}"),
    new Check("phase224-parameter-closure-blocked", electroweakParameterClosureBlocked, $"electroweakParameterAuditPassed={JsonBool(phase224.RootElement, "electroweakParameterAuditPassed")}"),
};

var representationNormalizationObstructionCertified = checks.All(check => check.Passed);
var terminalStatus = representationNormalizationObstructionCertified
    ? "su2-normalization-representation-compatibility-blocked-adjoint-rms-not-fermion-current-source"
    : "su2-normalization-representation-compatibility-review-required";

var result = new
{
    phaseId = "phase225-su2-normalization-representation-compatibility-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    representationNormalizationObstructionCertified,
    externalPhysicsContext = new
    {
        source = "Particle Data Group 2025 Review, Electroweak Model and Constraints on New Physics",
        url = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        relevantConvention = "The charged-current weak interaction uses weak-isospin generators T+/-=(sigma1 +/- i sigma2)/2 acting on fermion SU(2) doublets; T3=sigma3/2.",
        implication = "A fermion-current matrix element normalized by the fundamental trace-half convention cannot be promoted by replacing its scale with an adjoint spin-1 triplet RMS unless a new GU source derivation changes the operator being evaluated.",
    },
    representationMath = new
    {
        fundamentalRepresentation = new
        {
            spin = FundamentalSpin,
            dimension = 2,
            quadraticCasimir = fundamentalCasimir,
            standardGeneratorConvention = "T_a=sigma_a/2, tr(T_a T_b)=1/2 delta_ab",
        },
        adjointRepresentation = new
        {
            spin = AdjointSpin,
            dimension = 3,
            quadraticCasimir = adjointCasimir,
            tripletRmsScale = adjointTripletRmsScale,
        },
        phase63TraceHalfSingleGeneratorScale = traceHalfSingleGeneratorScale,
        phase221AdjointToTraceHalfRatio = adjointToTraceHalfRatio,
    },
    currentRepoEvidence = new
    {
        phase63 = new
        {
            status = JsonString(phase63.RootElement, "terminalStatus"),
            normalizationConventionId = JsonString(phase63.RootElement, "normalizationConventionId"),
            physicalTraceNormalization = JsonString(phase63.RootElement, "physicalTraceNormalization"),
        },
        phase64 = new
        {
            status = JsonString(phase64.RootElement, "terminalStatus"),
            sourceKind = JsonString(phase64.RootElement, "sourceKind"),
            matrixElementFormula = JsonString(phase64.RootElement, "matrixElementFormula"),
            normalizationConventionId = JsonString(phase64.RootElement, "normalizationConventionId"),
            operatorSource = JsonString(phase64.RootElement, "operatorSource"),
        },
        phase221 = new
        {
            status = JsonString(phase221.RootElement, "terminalStatus"),
            numericalTargetComparisonPassed = phase221NumericalLead,
            sourceLineagePromotable = phase221Promotable,
            derivationHypothesis = JsonString(phase221.RootElement, "derivationHypothesis"),
        },
        phase222 = new
        {
            status = JsonString(phase222.RootElement, "terminalStatus"),
            rawAmplitudeSourceObstructionCertified = rawAmplitudeObstructionCertified,
            bestRawToTargetRatio = phase222.RootElement.TryGetProperty("bestProductionReplay", out var bestProductionReplay)
                ? JsonDouble(bestProductionReplay, "bestRawToTargetRatio")
                : null,
        },
    },
    checks,
    decision = representationNormalizationObstructionCertified
        ? "Do not promote the P221 SU(2) Casimir/RMS W/Z lead from the current Phase64 source. The current source is a fermion-current matrix element under the trace-half fundamental weak-isospin convention, while the successful numerical factor is an adjoint/spin-1 triplet RMS hypothesis with no source derivation and no raw-amplitude closure."
        : "Review SU(2) normalization compatibility before relying on this obstruction.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU derivation that the W/Z source operator is an isotropic SU(2) triplet RMS object rather than the existing Phase64 fermion-current matrix element, or a replayed analytic matrix element using the correct source operator.",
        "The revised source must then pass Phase201/P209/P210/P213, including raw-amplitude, common-bridge, target-comparison, and stability gates.",
    },
    sourceEvidence = new
    {
        phase63Path = Phase63Path,
        phase64Path = Phase64Path,
        phase221Path = Phase221Path,
        phase222Path = Phase222Path,
        phase224Path = Phase224Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "su2_normalization_representation_compatibility_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "su2_normalization_representation_compatibility_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.representationNormalizationObstructionCertified,
        result.externalPhysicsContext,
        result.representationMath,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"representationNormalizationObstructionCertified={representationNormalizationObstructionCertified}");
Console.WriteLine($"phase221AdjointToTraceHalfRatio={adjointToTraceHalfRatio:R}");
Console.WriteLine($"phase221Promotable={phase221Promotable}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);
