using System.Text.Json;

const string DefaultOutputDir = "studies/phase290_charged_lepton_threshold_source_replacement_audit_001/output";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase4StudyPath = "studies/phase4_fermion_family_atlas_001/STUDY.md";
const string Phase4FamilyAtlasPath = "studies/phase4_fermion_family_atlas_001/output/fermion_family_atlas.json";
const string Phase4RegistryPath = "studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json";
const string Phase12FamiliesPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_families.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase273Path = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE290_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase4Atlas = JsonDocument.Parse(File.ReadAllText(Phase4FamilyAtlasPath));
using var phase4Registry = JsonDocument.Parse(File.ReadAllText(Phase4RegistryPath));
using var phase12Families = JsonDocument.Parse(File.ReadAllText(Phase12FamiliesPath));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase273 = JsonDocument.Parse(File.ReadAllText(Phase273Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var phase4StudyText = File.Exists(Phase4StudyPath) ? File.ReadAllText(Phase4StudyPath) : string.Empty;
var chargedLeptonThresholds = phase286.RootElement.GetProperty("chargedLeptonThresholds")
    .EnumerateArray()
    .Select(row => new ChargedLeptonThreshold(
        RequiredString(row, "leptonId"),
        RequiredDouble(row, "massGeV"),
        RequiredString(row, "sourceClass")))
    .ToArray();

var candidates = new List<FermionThresholdCandidate>();
candidates.AddRange(ReadPhase4FamilyAtlasCandidates(phase4Atlas.RootElement));
candidates.AddRange(ReadPhase4RegistryCandidates(phase4Registry.RootElement));
candidates.AddRange(ReadPhase12FamilyCandidates(phase12Families.RootElement));

var textHits = ScanThresholdTextEvidence(new[]
{
    Phase4StudyPath,
    Phase4FamilyAtlasPath,
    Phase4RegistryPath,
    Phase12FamiliesPath,
}).ToArray();

var targetBasedFits = candidates
    .GroupBy(candidate => candidate.SourceId, StringComparer.Ordinal)
    .Select(group => BuildBestTripletFit(group.Key, group.OrderBy(candidate => candidate.InternalValue).ToArray(), chargedLeptonThresholds))
    .Where(fit => fit is not null)
    .Cast<TripletFit>()
    .OrderBy(fit => fit.MaxLogResidual)
    .ToArray();

var bestTargetBasedTripletFit = targetBasedFits.FirstOrDefault();
var phase286SourceBoundary = phase286.RootElement.GetProperty("sourceLineageBoundary");
var phase286ExternalLeptonMassesUsed = JsonBool(phase286SourceBoundary, "externalLeptonMassesUsed") is true;
var phase286GuChargedLeptonThresholdSourceFound = JsonBool(phase286SourceBoundary, "guChargedLeptonThresholdSourceFound") is true;
var phase286LeptonicRunningNumericallyClosesWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz") is true;
var phase4PhysicalDisclaimerPresent = ContainsAll(
    phase4StudyText,
    new[] { "does NOT constitute physical validation", "No claims are made", "masses" });
var phase273Passed = JsonBool(phase273.RootElement, "couplingProxySourceAuditPassed") is true;
var phase273CouplingProxyPromotesWzMasses = JsonBool(phase273.RootElement, "couplingProxyPromotesWzMasses") is true;
var phase273CouplingProxyPromotesHiggsMass = JsonBool(phase273.RootElement, "couplingProxyPromotesHiggsMass") is true;
var phase237YukawaPromotableForHiggsMass = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var candidateWithPhysicalLeptonIdentityCount = candidates.Count(candidate => candidate.PhysicalLeptonIdentityPresent);
var candidateWithGeVScaleCount = candidates.Count(candidate => candidate.GeVScalePresent);
var candidateWithSourceLineageCount = candidates.Count(candidate => candidate.SourceLineagePresent);
var intakeReadyThresholdSourceCandidateCount = candidates.Count(candidate => candidate.IntakeReadyThresholdSourceCandidate)
    + textHits.Count(hit => hit.IntakeReadyThresholdSourceCandidate);
var anyThresholdSourceCandidateFillsContract = intakeReadyThresholdSourceCandidateCount > 0;

var checks = new[]
{
    new Check(
        "phase286-external-threshold-gap-active",
        phase286LeptonicRunningNumericallyClosesWz && phase286ExternalLeptonMassesUsed && !phase286GuChargedLeptonThresholdSourceFound,
        $"phase286LeptonicRunningNumericallyClosesWz={phase286LeptonicRunningNumericallyClosesWz}; phase286ExternalLeptonMassesUsed={phase286ExternalLeptonMassesUsed}; phase286GuChargedLeptonThresholdSourceFound={phase286GuChargedLeptonThresholdSourceFound}"),
    new Check(
        "fermion-artifacts-scanned",
        candidates.Count > 0 && textHits.Length >= 0,
        $"candidateCount={candidates.Count}; textHitCount={textHits.Length}"),
    new Check(
        "phase4-physical-disclaimer-preserved",
        phase4PhysicalDisclaimerPresent,
        $"phase4PhysicalDisclaimerPresent={phase4PhysicalDisclaimerPresent}"),
    new Check(
        "no-fermion-artifact-supplies-lepton-threshold-source-row",
        candidateWithPhysicalLeptonIdentityCount == 0
            && candidateWithGeVScaleCount == 0
            && candidateWithSourceLineageCount == 0
            && intakeReadyThresholdSourceCandidateCount == 0
            && !anyThresholdSourceCandidateFillsContract,
        $"candidateWithPhysicalLeptonIdentityCount={candidateWithPhysicalLeptonIdentityCount}; candidateWithGeVScaleCount={candidateWithGeVScaleCount}; candidateWithSourceLineageCount={candidateWithSourceLineageCount}; intakeReadyThresholdSourceCandidateCount={intakeReadyThresholdSourceCandidateCount}; anyThresholdSourceCandidateFillsContract={anyThresholdSourceCandidateFillsContract}"),
    new Check(
        "fermion-coupling-and-yukawa-leads-remain-nonpromotional",
        phase273Passed && !phase273CouplingProxyPromotesWzMasses && !phase273CouplingProxyPromotesHiggsMass && !phase237YukawaPromotableForHiggsMass,
        $"phase273Passed={phase273Passed}; phase273CouplingProxyPromotesWzMasses={phase273CouplingProxyPromotesWzMasses}; phase273CouplingProxyPromotesHiggsMass={phase273CouplingProxyPromotesHiggsMass}; phase237YukawaPromotableForHiggsMass={phase237YukawaPromotableForHiggsMass}"),
    new Check(
        "source-contracts-remain-unfilled",
        !unlockContractFilled && newSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var chargedLeptonThresholdSourceReplacementAuditPassed = checks.All(check => check.Passed)
    && !anyThresholdSourceCandidateFillsContract
    && intakeReadyThresholdSourceCandidateCount == 0;
var terminalStatus = chargedLeptonThresholdSourceReplacementAuditPassed
    ? "charged-lepton-threshold-source-replacement-audit-no-gu-threshold-source"
    : "charged-lepton-threshold-source-replacement-audit-review-required";

var result = new
{
    phaseId = "phase290-charged-lepton-threshold-source-replacement-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    chargedLeptonThresholdSourceReplacementAuditPassed,
    chargedLeptonThresholds,
    candidateCount = candidates.Count,
    candidateWithPhysicalLeptonIdentityCount,
    candidateWithGeVScaleCount,
    candidateWithSourceLineageCount,
    intakeReadyThresholdSourceCandidateCount,
    anyThresholdSourceCandidateFillsContract,
    phase4PhysicalDisclaimerPresent,
    candidateSummary = candidates
        .GroupBy(candidate => candidate.SourceId, StringComparer.Ordinal)
        .OrderBy(group => group.Key, StringComparer.Ordinal)
        .Select(group => new
        {
            sourceId = group.Key,
            candidateCount = group.Count(),
            minInternalValue = group.Min(candidate => candidate.InternalValue),
            maxInternalValue = group.Max(candidate => candidate.InternalValue),
            physicalLeptonIdentityCount = group.Count(candidate => candidate.PhysicalLeptonIdentityPresent),
            geVScaleCount = group.Count(candidate => candidate.GeVScalePresent),
            sourceLineageCount = group.Count(candidate => candidate.SourceLineagePresent),
        })
        .ToArray(),
    strongestTextHits = textHits
        .OrderByDescending(hit => hit.ReadinessScore)
        .ThenBy(hit => hit.Path, StringComparer.Ordinal)
        .ThenBy(hit => hit.LineNumber)
        .Take(10)
        .ToArray(),
    targetBasedTripletFitBoundary = new
    {
        targetBasedTripletFitPerformed = targetBasedFits.Length > 0,
        targetValuesUsedForAssignmentAndScale = targetBasedFits.Length > 0,
        targetBasedFitsPromotable = false,
        bestTargetBasedTripletFit,
        fitInterpretation = "Triplet fits are diagnostic only. They assign internal fermion envelopes to electron/muon/tau targets and fit a scale after the fact, so they cannot replace source-lineage threshold rows.",
    },
    inheritedEvidence = new
    {
        phase286 = new
        {
            phase286LeptonicRunningNumericallyClosesWz,
            phase286ExternalLeptonMassesUsed,
            phase286GuChargedLeptonThresholdSourceFound,
        },
        phase273 = new
        {
            phase273Passed,
            phase273CouplingProxyPromotesWzMasses,
            phase273CouplingProxyPromotesHiggsMass,
        },
        phase237 = new
        {
            phase237YukawaPromotableForHiggsMass,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2024-particle-properties",
            "https://pdg.lbl.gov/2024/listings/particle_properties.html",
            "The charged-lepton threshold values used by Phase286 are external particle-property inputs, not derived by this repository."),
        new ExternalSource(
            "pdg-2024-standard-model-review",
            "https://pdg.lbl.gov/2024/reviews/rpp2024-rev-standard-model.pdf",
            "In the Standard Model, charged-lepton masses are independent Yukawa-sector inputs after electroweak symmetry breaking unless a beyond-SM source derives the Yukawa matrix."),
    },
    checks,
    decision = chargedLeptonThresholdSourceReplacementAuditPassed
        ? "Do not replace Phase286 external electron/muon/tau thresholds with current GU fermion artifacts. Phase4/Phase12 provide internal toy or branch-family fermion envelopes and coupling proxies, but no physical charged-lepton identities, no GeV mass scale, no sourceLineageId/theoremOrDerivationId threshold rows, and no target-independent assignment to electron, muon, or tau."
        : "Review charged-lepton threshold replacement candidates before relying on Phase286's external-threshold boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived charged-lepton identity table selecting electron, muon, and tau rows target-independently.",
        "A GU-derived GeV mass or dimensionless threshold source for those three charged leptons with sourceLineageId and theoremOrDerivationId.",
        "A GU-derived running/threshold transport rule that consumes those thresholds without importing PDG charged-lepton masses.",
    },
    sourceEvidence = new
    {
        phase286Path = Phase286Path,
        phase4StudyPath = Phase4StudyPath,
        phase4FamilyAtlasPath = Phase4FamilyAtlasPath,
        phase4RegistryPath = Phase4RegistryPath,
        phase12FamiliesPath = Phase12FamiliesPath,
        phase237Path = Phase237Path,
        phase273Path = Phase273Path,
        phase245Path = Phase245Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "charged_lepton_threshold_source_replacement_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "charged_lepton_threshold_source_replacement_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.chargedLeptonThresholdSourceReplacementAuditPassed,
        result.candidateCount,
        result.candidateWithPhysicalLeptonIdentityCount,
        result.candidateWithGeVScaleCount,
        result.candidateWithSourceLineageCount,
        result.intakeReadyThresholdSourceCandidateCount,
        result.anyThresholdSourceCandidateFillsContract,
        result.phase4PhysicalDisclaimerPresent,
        result.candidateSummary,
        result.strongestTextHits,
        result.targetBasedTripletFitBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"chargedLeptonThresholdSourceReplacementAuditPassed={chargedLeptonThresholdSourceReplacementAuditPassed}");
Console.WriteLine($"candidateCount={candidates.Count}");
Console.WriteLine($"candidateWithPhysicalLeptonIdentityCount={candidateWithPhysicalLeptonIdentityCount}");
Console.WriteLine($"candidateWithGeVScaleCount={candidateWithGeVScaleCount}");
Console.WriteLine($"intakeReadyThresholdSourceCandidateCount={intakeReadyThresholdSourceCandidateCount}");
Console.WriteLine($"anyThresholdSourceCandidateFillsContract={anyThresholdSourceCandidateFillsContract}");

static IReadOnlyList<FermionThresholdCandidate> ReadPhase4FamilyAtlasCandidates(JsonElement root)
{
    var candidates = new List<FermionThresholdCandidate>();
    if (!root.TryGetProperty("families", out var families) || families.ValueKind != JsonValueKind.Array)
    {
        return candidates;
    }

    foreach (var family in families.EnumerateArray())
    {
        var values = ReadDoubleArray(family, "EigenvalueMagnitudeEnvelope");
        if (values.Length == 0)
        {
            continue;
        }

        var familyId = RequiredString(family, "FamilyId");
        candidates.Add(new FermionThresholdCandidate(
            "phase4-family-atlas",
            familyId,
            Median(values),
            "EigenvalueMagnitudeEnvelope",
            PhysicalLeptonIdentityPresent: false,
            GeVScalePresent: false,
            SourceLineagePresent: false,
            IntakeReadyThresholdSourceCandidate: false,
            "Toy2D su(2) family envelope; Phase4 study disclaims physical particle and mass validation."));
    }

    return candidates;
}

static IReadOnlyList<FermionThresholdCandidate> ReadPhase4RegistryCandidates(JsonElement root)
{
    var candidates = new List<FermionThresholdCandidate>();
    if (!root.TryGetProperty("candidates", out var rows) || rows.ValueKind != JsonValueKind.Array)
    {
        return candidates;
    }

    foreach (var row in rows.EnumerateArray())
    {
        var values = ReadDoubleArray(row, "massLikeEnvelope");
        if (values.Length == 0)
        {
            continue;
        }

        candidates.Add(new FermionThresholdCandidate(
            "phase4-unified-particle-registry",
            RequiredString(row, "particleId"),
            Median(values),
            "massLikeEnvelope",
            PhysicalLeptonIdentityPresent: ContainsPhysicalLeptonIdentity(row),
            GeVScalePresent: false,
            SourceLineagePresent: false,
            IntakeReadyThresholdSourceCandidate: false,
            $"claimClass={JsonString(row, "claimClass")}; observationConfidence={JsonDouble(row, "observationConfidence")}; comparisonEvidenceScore={JsonDouble(row, "comparisonEvidenceScore")}"));
    }

    return candidates;
}

static IReadOnlyList<FermionThresholdCandidate> ReadPhase12FamilyCandidates(JsonElement root)
{
    var candidates = new List<FermionThresholdCandidate>();
    if (!root.TryGetProperty("families", out var families) || families.ValueKind != JsonValueKind.Array)
    {
        return candidates;
    }

    foreach (var family in families.EnumerateArray())
    {
        var values = ReadDoubleArray(family, "eigenvalueMagnitudeEnvelope");
        if (values.Length == 0)
        {
            continue;
        }

        candidates.Add(new FermionThresholdCandidate(
            "phase12-fermion-families",
            RequiredString(family, "familyId"),
            Median(values),
            "eigenvalueMagnitudeEnvelope",
            PhysicalLeptonIdentityPresent: false,
            GeVScalePresent: false,
            SourceLineagePresent: false,
            IntakeReadyThresholdSourceCandidate: false,
            $"dominantChiralityProfile={JsonString(family, "dominantChiralityProfile")}; branchPersistenceScore={JsonDouble(family, "branchPersistenceScore")}; refinementPersistenceScore={JsonDouble(family, "refinementPersistenceScore")}"));
    }

    return candidates;
}

static TripletFit? BuildBestTripletFit(string sourceId, IReadOnlyList<FermionThresholdCandidate> sourceCandidates, IReadOnlyList<ChargedLeptonThreshold> thresholds)
{
    if (sourceCandidates.Count < 3 || thresholds.Count != 3)
    {
        return null;
    }

    var bestFit = (TripletFit?)null;
    var masses = thresholds.Select(threshold => threshold.MassGeV).ToArray();
    for (var i = 0; i < sourceCandidates.Count - 2; i++)
    {
        for (var j = i + 1; j < sourceCandidates.Count - 1; j++)
        {
            for (var k = j + 1; k < sourceCandidates.Count; k++)
            {
                var triplet = new[] { sourceCandidates[i], sourceCandidates[j], sourceCandidates[k] };
                if (triplet.Any(candidate => candidate.InternalValue <= 0.0))
                {
                    continue;
                }

                var scale = Math.Exp(Enumerable.Range(0, 3).Average(index => Math.Log(masses[index] / triplet[index].InternalValue)));
                var rows = Enumerable.Range(0, 3)
                    .Select(index =>
                    {
                        var predicted = triplet[index].InternalValue * scale;
                        var logResidual = Math.Abs(Math.Log(predicted / masses[index]));
                        return new TripletFitRow(thresholds[index].LeptonId, triplet[index].CandidateId, triplet[index].InternalValue, predicted, masses[index], logResidual);
                    })
                    .ToArray();
                var fit = new TripletFit(sourceId, scale, rows.Max(row => row.LogResidual), rows);
                if (bestFit is null || fit.MaxLogResidual < bestFit.MaxLogResidual)
                {
                    bestFit = fit;
                }
            }
        }
    }

    return bestFit;
}

static IEnumerable<TextHit> ScanThresholdTextEvidence(IEnumerable<string> files)
{
    foreach (var file in files)
    {
        if (!File.Exists(file))
        {
            continue;
        }

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
            if (!ContainsAny(line, new[] { "electron", "muon", "tau", "charged lepton", "charged-lepton", "lepton" })
                || !ContainsAny(line, new[] { "mass", "threshold", "source", "GeV", "MeV", "theorem", "derivation" }))
            {
                continue;
            }

            var context = BuildContext(lines, i, 2);
            var sourceRowLike = ContainsAny(context, new[] { "sourceLineageId", "source row", "source-row", "sourceRowId", "theoremOrDerivationId", "derivation-backed" });
            var targetIndependentClaim = ContainsAny(context, new[] { "target-independent", "externalTargetValuesUsed=false", "without target", "not from target" });
            var negativeContext = ContainsAny(context, new[] { "not ", "no ", "placeholder", "disclaim", "external", "requires", "missing", "blocked", "candidate" });
            var intakeReady = sourceRowLike && targetIndependentClaim && !negativeContext;
            yield return new TextHit(
                file,
                i + 1,
                line.Length > 260 ? line[..260] : line,
                (sourceRowLike ? 2 : 0) + (targetIndependentClaim ? 2 : 0) - (negativeContext ? 2 : 0),
                sourceRowLike,
                targetIndependentClaim,
                negativeContext,
                intakeReady);
        }
    }
}

static string BuildContext(IReadOnlyList<string> lines, int center, int radius)
{
    var start = Math.Max(0, center - radius);
    var end = Math.Min(lines.Count - 1, center + radius);
    return string.Join('\n', Enumerable.Range(start, end - start + 1).Select(index => lines[index].Trim()));
}

static bool ContainsPhysicalLeptonIdentity(JsonElement row)
{
    var text = row.ToString();
    return ContainsAny(text, new[] { "electron", "muon", "tau", "charged lepton", "charged-lepton" });
}

static double[] ReadDoubleArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
    {
        return Array.Empty<double>();
    }

    return property.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.Number)
        .Select(item => item.GetDouble())
        .Where(value => double.IsFinite(value))
        .ToArray();
}

static double Median(IReadOnlyList<double> values)
{
    var sorted = values.OrderBy(value => value).ToArray();
    return sorted.Length % 2 == 1
        ? sorted[sorted.Length / 2]
        : 0.5 * (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]);
}

static bool ContainsAll(string text, IEnumerable<string> terms) =>
    terms.All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static bool ContainsAny(string text, IEnumerable<string> terms) =>
    terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidOperationException($"Missing required numeric property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;

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

sealed record ChargedLeptonThreshold(string LeptonId, double MassGeV, string SourceClass);

sealed record FermionThresholdCandidate(
    string SourceId,
    string CandidateId,
    double InternalValue,
    string QuantityKind,
    bool PhysicalLeptonIdentityPresent,
    bool GeVScalePresent,
    bool SourceLineagePresent,
    bool IntakeReadyThresholdSourceCandidate,
    string Boundary);

sealed record TextHit(
    string Path,
    int LineNumber,
    string Excerpt,
    int ReadinessScore,
    bool SourceRowLike,
    bool TargetIndependentClaim,
    bool NegativeContext,
    bool IntakeReadyThresholdSourceCandidate);

sealed record TripletFit(string SourceId, double FittedGeVPerInternalUnit, double MaxLogResidual, IReadOnlyList<TripletFitRow> Rows);

sealed record TripletFitRow(string LeptonId, string CandidateId, double InternalValue, double FittedMassGeV, double ExternalMassGeV, double LogResidual);

sealed record ExternalSource(string SourceId, string Url, string Finding);

sealed record Check(string CheckId, bool Passed, string Detail);
