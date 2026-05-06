using System.Text.Json;

const string DefaultOutputDir = "studies/phase112_scalar_sector_relation_revision_attempt_001/output";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase69RelationPath = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase70ScalarBridgePath = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase75MissDiagnosticPath = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76NormalizationAuditPath = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE112_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var relation = JsonDocument.Parse(File.ReadAllText(Phase69RelationPath));
using var bridge = JsonDocument.Parse(File.ReadAllText(Phase70ScalarBridgePath));
using var miss = JsonDocument.Parse(File.ReadAllText(Phase75MissDiagnosticPath));
using var normalization = JsonDocument.Parse(File.ReadAllText(Phase76NormalizationAuditPath));

double? currentBridge = JsonDouble(relation.RootElement, "dimensionlessBridgeValue");
double? requiredScale = JsonDouble(miss.RootElement, "meanRequiredScaleFactor");
double? targetFittedBridge = Product(currentBridge, requiredScale);
bool scalarBridgeDerived = string.Equals(
    JsonString(bridge.RootElement, "terminalStatus"),
    "scalar-sector-bridge-evidence-derived",
    StringComparison.Ordinal);
bool independentRevisionEvidencePresent = false;

var result = new
{
    phaseId = "phase112-scalar-sector-relation-revision-attempt",
    terminalStatus = "scalar-sector-relation-revision-blocked-no-independent-evidence",
    strategyId = "scalar-sector-relation-revision",
    scalarBridgeDerived,
    independentRevisionEvidencePresent,
    repairAccepted = false,
    currentRelation = new
    {
        relationId = JsonString(relation.RootElement, "massGenerationRelationId"),
        dimensionlessBridgeValue = currentBridge,
        dimensionlessBridgeUncertainty = JsonDouble(relation.RootElement, "dimensionlessBridgeUncertainty"),
        excludedTargetObservableIds = relation.RootElement.GetProperty("excludedTargetObservableIds").Clone(),
    },
    diagnosticOnlyTargetImpliedRevision = new
    {
        meanRequiredScaleFactor = requiredScale,
        targetFittedBridgeValue = targetFittedBridge,
        targetImpliedWeakCoupling = JsonDouble(normalization.RootElement, "targetImpliedWeakCoupling"),
        targetImpliedRawMatrixElementMagnitude = JsonDouble(normalization.RootElement, "targetImpliedRawMatrixElementMagnitude"),
        mayBeUsedAsCalibration = false,
    },
    diagnosis = new[]
    {
        "existing scalar-sector bridge evidence is present, but no independent revision evidence is supplied",
        "the Phase75/76 target-implied correction is diagnostic-only and cannot define the scalar-sector relation",
        "a valid revision must come from target-independent scalar-sector derivation or replay evidence",
    },
    closureRequirements = new[]
    {
        "derive a scalar-sector relation revision without using W/Z target residuals",
        "propagate uncertainty through the revised relation",
        "rerun absolute W/Z projection and comparison only after a target-independent revision validates",
    },
    sourceEvidence = new
    {
        contractPath = Phase110ContractPath,
        electroweakMassGenerationRelationPath = Phase69RelationPath,
        scalarSectorBridgePath = Phase70ScalarBridgePath,
        absoluteMassMissDiagnosticPath = Phase75MissDiagnosticPath,
        weakCouplingNormalizationAuditPath = Phase76NormalizationAuditPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "scalar_sector_relation_revision_attempt.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "scalar_sector_relation_revision_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase112-scalar-sector-relation-revision-attempt",
        result.terminalStatus,
        result.scalarBridgeDerived,
        result.independentRevisionEvidencePresent,
        result.repairAccepted,
        result.diagnosticOnlyTargetImpliedRevision,
    }, options));

Console.WriteLine(result.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static double? Product(double? a, double? b) =>
    a is { } x && b is { } y && double.IsFinite(x) && double.IsFinite(y) ? x * y : null;
