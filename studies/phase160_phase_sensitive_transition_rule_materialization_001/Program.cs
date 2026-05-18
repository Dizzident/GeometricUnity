using System.Numerics;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase160_phase_sensitive_transition_rule_materialization_001/output";
const string Phase159Path = "studies/phase159_mode_branch_projector_derivation_experiment_001/output/mode_branch_projector_derivation_experiment.json";
const string CouplingAtlasPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-a-20260315212202.json";
const string Phase140TemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json";

const string SelectedBosonModeId = "candidate-8";
const double MaxConjugacyResidual = 1e-9;
const double MinPhaseSeparation = 0.2;

var outputDir = Environment.GetEnvironmentVariable("PHASE160_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase159 = JsonDocument.Parse(File.ReadAllText(Phase159Path));
using var atlas = JsonDocument.Parse(File.ReadAllText(CouplingAtlasPath));
using var template = JsonDocument.Parse(File.ReadAllText(Phase140TemplatePath));

var rows = phase159.RootElement.GetProperty("projectorRows")
    .EnumerateArray()
    .Select(row => new ProjectorRow(
        RequiredString(row, "familyId"),
        RequiredString(row, "candidateId"),
        RequiredString(row, "sourceCanonicalFermionModeId"),
        RequiredString(row, "modeId"),
        RequiredDouble(row, "gaugeDensityPurity"),
        RequiredDouble(row, "gaugeDensityLeadingGap"),
        RequiredBool(row, "projectorPromotable")))
    .ToArray();

if (rows.Length != 2)
    throw new InvalidOperationException($"Expected exactly two projector rows, found {rows.Length}.");

var first = rows[0];
var second = rows[1];
var forward = FindCoupling(atlas.RootElement, SelectedBosonModeId, first.SourceCanonicalFermionModeId, second.SourceCanonicalFermionModeId);
var reverse = FindCoupling(atlas.RootElement, SelectedBosonModeId, second.SourceCanonicalFermionModeId, first.SourceCanonicalFermionModeId);
var diagonalFirst = FindCoupling(atlas.RootElement, SelectedBosonModeId, first.SourceCanonicalFermionModeId, first.SourceCanonicalFermionModeId);
var diagonalSecond = FindCoupling(atlas.RootElement, SelectedBosonModeId, second.SourceCanonicalFermionModeId, second.SourceCanonicalFermionModeId);

var forwardComplex = new Complex(forward.Real, forward.Imaginary);
var reverseComplex = new Complex(reverse.Real, reverse.Imaginary);
double conjugacyResidual = Complex.Abs(reverseComplex - Complex.Conjugate(forwardComplex))
    / Math.Max(Complex.Abs(forwardComplex), 1e-30);
double phaseSeparation = Math.Abs(forward.Imaginary) / Math.Max(forward.Magnitude, 1e-30);
bool projectorsPromotable = rows.All(row => row.ProjectorPromotable);
bool conjugatePair = conjugacyResidual <= MaxConjugacyResidual;
bool nontrivialPhase = phaseSeparation >= MinPhaseSeparation;
bool transitionRulePromotable = projectorsPromotable && conjugatePair && nontrivialPhase;

string terminalStatus = transitionRulePromotable
    ? "phase-sensitive-transition-rule-materialized-for-p140"
    : "phase-sensitive-transition-rule-blocked";

var transitionRule = new
{
    ruleId = "phase160-phase-sensitive-conjugate-transition-rule-v1",
    ruleKind = "phase-sensitive-directed-conjugate-transition-rule",
    derivationId = "phase160:projector-density-plus-complex-coupling-conjugacy",
    externalTargetValuesUsed = false,
    sourceBosonModeId = SelectedBosonModeId,
    phaseConvention = "direction label is a target-independent conjugate-pair convention: positive imaginary off-diagonal amplitude is direction-plus; reverse conjugate is direction-minus",
    projectorEvidencePath = Phase159Path,
    couplingAtlasPath = CouplingAtlasPath,
    directedTransitions = new object[]
    {
        new
        {
            direction = "direction-plus",
            fromFamilyId = first.FamilyId,
            toFamilyId = second.FamilyId,
            fromSourceCanonicalFermionModeId = first.SourceCanonicalFermionModeId,
            toSourceCanonicalFermionModeId = second.SourceCanonicalFermionModeId,
            bosonModeId = SelectedBosonModeId,
            couplingProxyReal = forward.Real,
            couplingProxyImaginary = forward.Imaginary,
            couplingProxyMagnitude = forward.Magnitude,
            projectorPurityFrom = first.GaugeDensityPurity,
            projectorPurityTo = second.GaugeDensityPurity,
        },
        new
        {
            direction = "direction-minus",
            fromFamilyId = second.FamilyId,
            toFamilyId = first.FamilyId,
            fromSourceCanonicalFermionModeId = second.SourceCanonicalFermionModeId,
            toSourceCanonicalFermionModeId = first.SourceCanonicalFermionModeId,
            bosonModeId = SelectedBosonModeId,
            couplingProxyReal = reverse.Real,
            couplingProxyImaginary = reverse.Imaginary,
            couplingProxyMagnitude = reverse.Magnitude,
            projectorPurityFrom = second.GaugeDensityPurity,
            projectorPurityTo = first.GaugeDensityPurity,
        },
    },
    diagnostics = new
    {
        conjugacyResidual,
        maxConjugacyResidual = MaxConjugacyResidual,
        phaseSeparation,
        minPhaseSeparation = MinPhaseSeparation,
        diagonalMagnitudeFrom = diagonalFirst.Magnitude,
        diagonalMagnitudeTo = diagonalSecond.Magnitude,
        offDiagonalDominanceOverMaxDiagonal = Math.Max(forward.Magnitude, reverse.Magnitude) / Math.Max(diagonalFirst.Magnitude, diagonalSecond.Magnitude),
    },
};

if (transitionRulePromotable)
{
    var artifact = new
    {
        artifactId = "phase160-phase-sensitive-transition-rule-p140-intake",
        schemaVersion = "1.0.0",
        artifactKind = "directed-coupling-transition-rule",
        status = "filled-by-phase160",
        targetContractId = JsonString(template.RootElement, "targetContractId") ?? "phase139-fermion-sector-label-or-transition-rule-contract-v1",
        rows = template.RootElement.GetProperty("rows").Clone(),
        transitionRule,
        instructions = template.RootElement.GetProperty("instructions").Clone(),
    };
    var intakeOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    File.WriteAllText(Phase140TemplatePath, JsonSerializer.Serialize(artifact, intakeOptions));
}

var result = new
{
    phaseId = "phase160-phase-sensitive-transition-rule-materialization",
    terminalStatus,
    transitionRulePromotable,
    p140TemplateUpdated = transitionRulePromotable,
    selectedBosonModeId = SelectedBosonModeId,
    gates = new
    {
        projectorsPromotable,
        conjugatePair,
        nontrivialPhase,
        conjugacyResidual,
        maxConjugacyResidual = MaxConjugacyResidual,
        phaseSeparation,
        minPhaseSeparation = MinPhaseSeparation,
    },
    transitionRule,
    blockers = transitionRulePromotable
        ? Array.Empty<string>()
        : new[]
        {
            projectorsPromotable ? null : "P159 projectors are not promotable",
            conjugatePair ? null : "off-diagonal couplings do not pass the conjugate-pair residual gate",
            nontrivialPhase ? null : "off-diagonal coupling phase is too close to real to define a conjugate direction convention",
        }.Where(x => x is not null).ToArray(),
    sourceEvidence = new
    {
        phase159Path = Phase159Path,
        couplingAtlasPath = CouplingAtlasPath,
        phase140TemplatePath = Phase140TemplatePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase_sensitive_transition_rule_materialization.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase_sensitive_transition_rule_materialization_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.transitionRulePromotable,
        result.p140TemplateUpdated,
        result.selectedBosonModeId,
        result.gates,
        result.blockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"transitionRulePromotable={transitionRulePromotable}");
Console.WriteLine($"p140TemplateUpdated={transitionRulePromotable}");

static Coupling FindCoupling(JsonElement atlas, string bosonModeId, string modeI, string modeJ)
{
    foreach (var coupling in atlas.GetProperty("couplings").EnumerateArray())
    {
        if (JsonString(coupling, "bosonModeId") == bosonModeId
            && JsonString(coupling, "fermionModeIdI") == modeI
            && JsonString(coupling, "fermionModeIdJ") == modeJ)
        {
            return new Coupling(
                RequiredDouble(coupling, "couplingProxyReal"),
                RequiredDouble(coupling, "couplingProxyImaginary"),
                RequiredDouble(coupling, "couplingProxyMagnitude"));
        }
    }

    throw new InvalidOperationException($"Missing coupling {bosonModeId}: {modeI} -> {modeJ}");
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");
static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing required numeric property {propertyName}.");
static bool RequiredBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : throw new InvalidOperationException($"Missing required bool property {propertyName}.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

sealed record ProjectorRow(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    string ModeId,
    double GaugeDensityPurity,
    double GaugeDensityLeadingGap,
    bool ProjectorPromotable);

sealed record Coupling(double Real, double Imaginary, double Magnitude);
