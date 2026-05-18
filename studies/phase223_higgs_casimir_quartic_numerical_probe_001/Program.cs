using System.Text.Json;

const string DefaultOutputDir = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE223_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));

var targetDiagnostic = phase215.RootElement.GetProperty("targetImpliedDiagnostic");
var vev = RequiredDouble(targetDiagnostic, "vev");
var targetHiggsMass = RequiredDouble(targetDiagnostic, "targetHiggsMass");
var targetImpliedQuartic = RequiredDouble(targetDiagnostic, "targetImpliedQuartic");
var targetImpliedMassReplay = RequiredDouble(targetDiagnostic, "targetImpliedMassReplay");

var casimirWeakCoupling = RequiredDouble(phase221.RootElement, "casimirWeakCoupling");
var casimirWeakCouplingSquared = casimirWeakCoupling * casimirWeakCoupling;
var targetQuarticOverCasimirG2 = targetImpliedQuartic / casimirWeakCouplingSquared;

var factors = new[]
{
    new FactorDefinition("one-quarter", "1/4", 1.0 / 4.0, "standard-simple-rational-probe"),
    new FactorDefinition("three-tenths", "3/10", 3.0 / 10.0, "nearby-simple-rational-probe"),
    new FactorDefinition("five-sixteenths", "5/16", 5.0 / 16.0, "nearby-simple-rational-probe"),
    new FactorDefinition("inverse-pi", "1/pi", 1.0 / Math.PI, "transcendental-control-probe"),
    new FactorDefinition("inverse-three", "1/3", 1.0 / 3.0, "standard-simple-rational-probe"),
    new FactorDefinition("three-eighths", "3/8", 3.0 / 8.0, "standard-simple-rational-probe"),
    new FactorDefinition("one-over-two-root-three", "1/(2*sqrt(3))", 1.0 / (2.0 * Math.Sqrt(3.0)), "casimir-related-control-probe"),
};

var candidateProbes = factors
    .Select(factor =>
    {
        var lambda = casimirWeakCouplingSquared * factor.Value;
        var mass = Math.Sqrt(2.0 * lambda) * vev;
        return new FactorProbe(
            factor.Id,
            factor.Expression,
            factor.Kind,
            factor.Value,
            lambda,
            mass,
            mass - targetHiggsMass,
            Math.Abs(mass - targetHiggsMass),
            factor.Value - targetQuarticOverCasimirG2,
            Math.Abs(factor.Value - targetQuarticOverCasimirG2),
            false,
            "diagnostic-only: no solved scalar source/operator derives this factor");
    })
    .OrderBy(probe => probe.AbsMassResidualGeV)
    .ToArray();

var bestProbe = candidateProbes.First();
var numericalLeadPresent = bestProbe.AbsMassResidualGeV < 1.0;

var higgsSourceIdentityValidated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
var scalarSourceCensusPromotable = JsonBool(phase189.RootElement, "censusPromotable") is true;
var higgsPredictionAttemptAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
var potentialSelfCouplingPromotable = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var scalarSourceLineagePromotable = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var quarticSourceScanPromotable = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var targetImpliedSelfCouplingPromotable = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var casimirWzPromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;

var blockers = new List<string>();
if (!higgsSourceIdentityValidated)
    blockers.Add("Phase187 has no validated Higgs source identity.");
if (!scalarSourceCensusPromotable)
    blockers.Add("Phase189 has no promotable solved scalar source/operator census.");
if (!higgsPredictionAttemptAllowed)
    blockers.Add("Phase189 does not allow a Higgs prediction attempt.");
if (!potentialSelfCouplingPromotable)
    blockers.Add("Phase196 has no promotable Higgs potential or self-coupling source.");
if (!scalarSourceLineagePromotable)
    blockers.Add("Phase199 has no promotable Higgs scalar-source lineage.");
if (!quarticSourceScanPromotable)
    blockers.Add("Phase207 found no intake-ready Higgs quartic/self-coupling source.");
if (targetImpliedSelfCouplingPromotable)
    blockers.Add("Phase215 unexpectedly allows target-implied Higgs self-coupling promotion.");
if (!casimirWzPromotable)
    blockers.Add("Phase221 SU(2) Casimir W/Z normalization remains nonpromotional and cannot be reused as a Higgs source derivation.");

var sourceLineagePromotable = false;
var canPromoteHiggsCasimirQuarticLead = sourceLineagePromotable && blockers.Count == 0;
var terminalStatus = numericalLeadPresent
    ? "higgs-casimir-quartic-numerical-lead-found-not-promotable"
    : "higgs-casimir-quartic-numerical-probe-no-close-lead";

var result = new
{
    phaseId = "phase223-higgs-casimir-quartic-numerical-probe",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    numericalLeadPresent,
    canPromoteHiggsCasimirQuarticLead,
    sourceLineagePromotable,
    targetDiagnostic = new
    {
        vev,
        targetHiggsMass,
        targetImpliedQuartic,
        targetImpliedMassReplay,
        targetQuarticOverCasimirG2,
        warning = "targetImpliedQuartic uses the observed Higgs mass and is diagnostic context only",
    },
    casimirInput = new
    {
        casimirWeakCoupling,
        casimirWeakCouplingSquared,
        wzCasimirSourceLineagePromotable = casimirWzPromotable,
    },
    bestProbe,
    candidateProbes,
    blockers,
    decision = numericalLeadPresent
        ? "The 3/10 factor is a close numerical Higgs lead when multiplied by the Phase221 Casimir weak-coupling square, but it is not scientifically promotable because no scalar source/operator derives the factor and the Higgs source-lineage gates remain blocked."
        : "No checked factor supplies even a close Higgs numerical lead; in any case, no scalar source/operator lineage is promotable.",
    nextRequiredArtifact = "A solved target-independent Higgs scalar source/operator that derives a quartic or excitation relation, supplies Higgs identity and massive profile evidence, and passes Phase201/P209/P210/P213 without using the observed Higgs mass.",
    sourceEvidence = new
    {
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase196Path = Phase196Path,
        phase199Path = Phase199Path,
        phase207Path = Phase207Path,
        phase215Path = Phase215Path,
        phase221Path = Phase221Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_casimir_quartic_numerical_probe.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_casimir_quartic_numerical_probe_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.numericalLeadPresent,
        result.canPromoteHiggsCasimirQuarticLead,
        result.sourceLineagePromotable,
        result.targetDiagnostic,
        result.casimirInput,
        result.bestProbe,
        result.candidateProbes,
        result.blockers,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"numericalLeadPresent={numericalLeadPresent}");
Console.WriteLine($"canPromoteHiggsCasimirQuarticLead={canPromoteHiggsCasimirQuarticLead}");
Console.WriteLine($"bestFactor={bestProbe.FactorExpression}");
Console.WriteLine($"bestMassGeV={bestProbe.ReplayedHiggsMassGeV:R}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

sealed record FactorDefinition(string Id, string Expression, double Value, string Kind);

sealed record FactorProbe(
    string FactorId,
    string FactorExpression,
    string ProbeKind,
    double FactorValue,
    double QuarticFromCasimirG2,
    double ReplayedHiggsMassGeV,
    double MassResidualGeV,
    double AbsMassResidualGeV,
    double FactorResidualToTargetQuarticOverG2,
    double AbsFactorResidualToTargetQuarticOverG2,
    bool SourceDerived,
    string PromotionStatus);
