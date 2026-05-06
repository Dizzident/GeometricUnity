using System.Text.Json;

const string DefaultOutputDir = "studies/phase104_candidate3_physical_mapping_attempt_001/output";
const string Phase101PackagePath = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE104_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var package = JsonDocument.Parse(File.ReadAllText(Phase101PackagePath));
string candidateId = JsonString(package.RootElement, "candidateId") ?? "candidate-3";
var internalPrediction = package.RootElement.GetProperty("internalPrediction");
string sourceObservableId = JsonString(internalPrediction, "observableId")
    ?? "phase99-candidate-3-replayed-coupling-proxy-magnitude";

var mapping = new
{
    mappingId = "phase104-candidate-3-coupling-proxy-to-physical-boson-observable",
    candidateId,
    particleId = "undetermined-boson",
    physicalObservableType = "blocked",
    sourceComputedObservableId = sourceObservableId,
    targetPhysicalObservableId = (string?)null,
    unitFamily = "internal-native",
    status = "blocked",
    assumptions = new[]
    {
        "Phase99/101 source value is an internal weak-coupling proxy magnitude.",
        "No derivation currently identifies this single candidate-3 proxy as W mass, Z mass, W/Z mass ratio, Higgs mass, or photon masslessness.",
        "No external target value was used to choose the candidate or compute the proxy.",
    },
    closureRequirements = new[]
    {
        "derive a candidate-3 observable map from the replay coupling proxy to a named physical boson observable",
        "show the map is target-independent",
        "declare the physical target observable and unit family before calibration",
    },
};

var calibration = new
{
    calibrationId = "phase104-candidate-3-coupling-proxy-physical-calibration",
    candidateId,
    mappingId = mapping.mappingId,
    sourceComputedObservableId = sourceObservableId,
    sourceUnitFamily = "internal-native",
    targetUnitFamily = "blocked",
    targetUnit = "blocked",
    scaleFactor = 1.0,
    scaleUncertainty = 0.0,
    status = "blocked",
    method = "not-calibrated-candidate-3-coupling-proxy",
    source = "phase104-candidate3-physical-mapping-attempt",
    assumptions = new[]
    {
        "A physical scale cannot be assigned until the source observable has a validated physical mapping.",
    },
    closureRequirements = new[]
    {
        "complete the candidate-3 physical mapping first",
        "derive a unit or dimensionless normalization contract independent of the target comparison",
    },
};

var result = new
{
    phaseId = "phase104-candidate3-physical-mapping-attempt",
    terminalStatus = "candidate3-physical-mapping-and-calibration-blocked",
    candidateId,
    sourceComputedObservableId = sourceObservableId,
    physicalObservableMapping = mapping,
    physicalCalibration = calibration,
    candidateSpecificPhysicalMappingValidated = false,
    candidateSpecificCalibrationValidated = false,
    nextFixes = mapping.closureRequirements.Concat(calibration.closureRequirements).Distinct(StringComparer.Ordinal).ToArray(),
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_mapping_attempt.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_mapping_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase104-candidate3-physical-mapping-attempt",
        result.terminalStatus,
        result.candidateId,
        result.sourceComputedObservableId,
        result.candidateSpecificPhysicalMappingValidated,
        result.candidateSpecificCalibrationValidated,
        result.nextFixes,
    }, options));

Console.WriteLine(result.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;
