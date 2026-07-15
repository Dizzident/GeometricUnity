using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string outputDir = "studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/output";
const string phase493Path = "studies/phase493_phase456_stored_artifact_failure_decomposition_001/output/phase456_stored_artifact_failure_decomposition_summary.json";
const string phase494Path = "studies/phase494_phase456_estimator_oracle_battery_001/output/phase456_estimator_oracle_battery_summary.json";
const string expectedPhase493ByteSha256 = "dd2d00e7bb1a5dc3869a0b8362d8f2a855cd6b2c80cee62818ad4eb0671b5d85";
const string expectedPhase494ByteSha256 = "e90f8e008c7d413275699f353603bb6dab9c4f3490ec5039ca6c9a50a6dbc25f";
const string expectedPhase493DecisionContractSha256 = "ccf3711817ec93a4a5ea1fdad80132ce9229ebfe05b4855fd5497f7548b0e130";
const string expectedPhase494DecisionContractSha256 = "da9ef99033769f39885b4ac75a09f787ee2103f9c9af7011b6472f5eaf403d1e";
const string expectedPhase493Id = "phase493-phase456-stored-artifact-failure-decomposition";
const string expectedPhase494Id = "phase494-phase456-estimator-oracle-battery";

string[] readinessTaxonomy =
{
    "invalid-precursor",
    "sampler-implementation-defect",
    "estimator-structurally-unidentifiable",
    "insufficient-statistics",
    "observable-channel-invalid",
    "prospective-repair-pack-justified",
    "unresolved",
};
string[] phase493Taxonomy =
{
    "sampler-defect-supported", "estimator-domain-failure-supported", "statistics-limited-supported",
    "mixed-failure-supported", "failure-source-unresolved", "invalid-committed-artifact",
};
string[] phase494Taxonomy =
{
    "original-estimator-identifiable", "alternative-estimator-feasible", "channel-nonidentifiable",
    "mixed-estimator-outcome", "invalid-oracle-battery",
};
const string schemaContract = "phase495-a8-input-schema-v1|p493:phaseId:string,terminalStatus:string,verdictKind:string,inputsValid:boolean,failureClassification:string,decisionContractSha256:string,componentFindings.sampler.samplerDefectSupported:boolean,componentFindings.estimatorDomain.estimatorDomainFailureSupported:boolean,componentFindings.statistics.statisticsLimitationSupported:boolean,componentFindings.channelValidity.channelValidityFailureSupported:boolean,componentFindings.channelValidity.channelEstimatorCompatibilityFailureSupported:boolean,componentFindings.channelValidity.underlyingObservableInvalidSupported:boolean,componentFindings.familyFirewall.familyFirewallPropagationVerified:boolean,a8BoundaryHeld:boolean|p494:phaseId:string,terminalStatus:string,verdictKind:string,inputsValid:boolean,oracleClassification:string,decisionContractSha256:string,originalEstimatorIdentifiableOnSinglePoleDomain:boolean,originalEstimatorStructurallyIdentifiable:boolean,t4OriginalRatioUnderdeterminesSpectralContent:boolean,alternativeEstimatorFeasible:boolean,covarianceAwareResidualsPassed:boolean,covarianceAwareFullCorrelatorDiscriminatesMixture:boolean,channelNonidentifiabilityObserved:boolean,positiveOracleChannelValid:boolean,allOracleChannelsValid:boolean,statisticsOrPowerLimitationSupported:boolean,invalidRowPropagationPassed:boolean";
const string precedenceContract = "phase495-a8-readiness-v1|precedence=invalid-precursor>sampler-implementation-defect>estimator-structurally-unidentifiable>insufficient-statistics>observable-channel-invalid>prospective-repair-pack-justified>unresolved|invalid=missing-or-byte-hash-schema-contract-taxonomy-or-input-validity-failure|sampler=p493-sampler-defect-supported|estimator=p493-estimator-domain-or-channel-compatibility-failure-and-p494-original-structurally-unidentifiable-and-t4-underdetermines-content|statistics=p493-statistics-limitation-supported|observable=p493-underlying-observable-invalid-supported|justified=no-earlier-condition-and-p494-alternative-feasible-positive-channel-valid-covariance-residuals-pass-full-correlator-discriminates-mixture-invalid-propagation-pass|zero-authority";

var phase493 = BindPrecursor(
    phase493Path, expectedPhase493ByteSha256, expectedPhase493Id,
    expectedPhase493DecisionContractSha256, "failureClassification", phase493Taxonomy,
    new[]
    {
        Required("phaseId", JsonValueKind.String), Required("terminalStatus", JsonValueKind.String),
        Required("verdictKind", JsonValueKind.String), Required("inputsValid", JsonValueKind.True, JsonValueKind.False),
        Required("failureClassification", JsonValueKind.String), Required("decisionContractSha256", JsonValueKind.String),
        Required("componentFindings.sampler.samplerDefectSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.estimatorDomain.estimatorDomainFailureSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.statistics.statisticsLimitationSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.channelValidity.channelValidityFailureSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.channelValidity.channelEstimatorCompatibilityFailureSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.channelValidity.underlyingObservableInvalidSupported", JsonValueKind.True, JsonValueKind.False),
        Required("componentFindings.familyFirewall.familyFirewallPropagationVerified", JsonValueKind.True, JsonValueKind.False),
        Required("a8BoundaryHeld", JsonValueKind.True, JsonValueKind.False),
    });
var phase494 = BindPrecursor(
    phase494Path, expectedPhase494ByteSha256, expectedPhase494Id,
    expectedPhase494DecisionContractSha256, "oracleClassification", phase494Taxonomy,
    new[]
    {
        Required("phaseId", JsonValueKind.String), Required("terminalStatus", JsonValueKind.String),
        Required("verdictKind", JsonValueKind.String), Required("inputsValid", JsonValueKind.True, JsonValueKind.False),
        Required("oracleClassification", JsonValueKind.String), Required("decisionContractSha256", JsonValueKind.String),
        Required("originalEstimatorIdentifiableOnSinglePoleDomain", JsonValueKind.True, JsonValueKind.False),
        Required("originalEstimatorStructurallyIdentifiable", JsonValueKind.True, JsonValueKind.False),
        Required("t4OriginalRatioUnderdeterminesSpectralContent", JsonValueKind.True, JsonValueKind.False),
        Required("alternativeEstimatorFeasible", JsonValueKind.True, JsonValueKind.False),
        Required("covarianceAwareResidualsPassed", JsonValueKind.True, JsonValueKind.False),
        Required("covarianceAwareFullCorrelatorDiscriminatesMixture", JsonValueKind.True, JsonValueKind.False),
        Required("channelNonidentifiabilityObserved", JsonValueKind.True, JsonValueKind.False),
        Required("positiveOracleChannelValid", JsonValueKind.True, JsonValueKind.False),
        Required("allOracleChannelsValid", JsonValueKind.True, JsonValueKind.False),
        Required("statisticsOrPowerLimitationSupported", JsonValueKind.True, JsonValueKind.False),
        Required("invalidRowPropagationPassed", JsonValueKind.True, JsonValueKind.False),
    });

bool precursorsValid = phase493.Valid && phase494.Valid;
bool samplerImplementationDefectSupported = precursorsValid && phase493.Bool("componentFindings.sampler.samplerDefectSupported");
bool estimatorStructurallyUnidentifiableSupported = precursorsValid
    && (phase493.Bool("componentFindings.estimatorDomain.estimatorDomainFailureSupported")
        || phase493.Bool("componentFindings.channelValidity.channelEstimatorCompatibilityFailureSupported"))
    && !phase494.Bool("originalEstimatorStructurallyIdentifiable")
    && phase494.Bool("t4OriginalRatioUnderdeterminesSpectralContent");
bool insufficientStatisticsSupported = precursorsValid
    && (phase493.Bool("componentFindings.statistics.statisticsLimitationSupported")
        || phase494.Bool("statisticsOrPowerLimitationSupported"));
bool observableChannelInvalidSupported = precursorsValid
    && phase493.Bool("componentFindings.channelValidity.underlyingObservableInvalidSupported");
bool prospectiveRepairPackEvidenceComplete = precursorsValid
    && phase494.Bool("alternativeEstimatorFeasible")
    && phase494.Bool("positiveOracleChannelValid")
    && phase494.Bool("covarianceAwareResidualsPassed")
    && phase494.Bool("covarianceAwareFullCorrelatorDiscriminatesMixture")
    && phase494.Bool("invalidRowPropagationPassed");

string readinessClassification = !precursorsValid ? "invalid-precursor"
    : samplerImplementationDefectSupported ? "sampler-implementation-defect"
    : estimatorStructurallyUnidentifiableSupported ? "estimator-structurally-unidentifiable"
    : insufficientStatisticsSupported ? "insufficient-statistics"
    : observableChannelInvalidSupported ? "observable-channel-invalid"
    : prospectiveRepairPackEvidenceComplete ? "prospective-repair-pack-justified"
    : "unresolved";
string terminalStatus = "phase456-prospective-repair-readiness-adjudicator-" + readinessClassification;

var result = new
{
    phaseId = "phase495-phase456-prospective-repair-readiness-adjudicator",
    terminalStatus,
    verdictKind = readinessClassification,
    readinessClassification,
    readinessTaxonomy,
    inputsValid = precursorsValid,
    applicationSubjectKind = "phase456-prospective-repair-readiness-zero-physics-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A8; PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15 Phase495",
    explorationOnly = true,
    zeroPhysicsCompute = true,
    frozenBeforeFindingsConsumption = true,
    frozenContracts = new
    {
        schemaContract,
        schemaContractSha256 = Sha256(Encoding.UTF8.GetBytes(schemaContract)),
        precedenceContract,
        precedenceContractSha256 = Sha256(Encoding.UTF8.GetBytes(precedenceContract)),
        precedence = readinessTaxonomy,
        expectedPhase493ByteSha256,
        expectedPhase494ByteSha256,
        expectedPhase493DecisionContractSha256,
        expectedPhase494DecisionContractSha256,
    },
    precursorBindings = new
    {
        phase493 = phase493.Output(),
        phase494 = phase494.Output(),
        bothExactBytesAndSchemasValid = precursorsValid,
    },
    precursorClassifications = new
    {
        phase493 = phase493.Classification,
        phase494 = phase494.Classification,
    },
    supportedConditions = new
    {
        samplerImplementationDefectSupported,
        estimatorStructurallyUnidentifiableSupported,
        insufficientStatisticsSupported,
        observableChannelInvalidSupported,
        prospectiveRepairPackEvidenceComplete,
        phase493ChannelEstimatorCompatibilityFailureSupported = precursorsValid && phase493.Bool("componentFindings.channelValidity.channelEstimatorCompatibilityFailureSupported"),
        phase493UnderlyingObservableInvalidSupported = precursorsValid && phase493.Bool("componentFindings.channelValidity.underlyingObservableInvalidSupported"),
        phase494AlternativeEstimatorFeasible = precursorsValid && phase494.Bool("alternativeEstimatorFeasible"),
        phase494FullCorrelatorDiscriminatesMixture = precursorsValid && phase494.Bool("covarianceAwareFullCorrelatorDiscriminatesMixture"),
        phase494PositiveOracleChannelValid = precursorsValid && phase494.Bool("positiveOracleChannelValid"),
        phase494AllOracleChannelsValid = precursorsValid && phase494.Bool("allOracleChannelsValid"),
    },
    precedenceTrace = readinessTaxonomy.Select((classification, index) => new
    {
        order = index + 1,
        classification,
        conditionSupported = classification switch
        {
            "invalid-precursor" => !precursorsValid,
            "sampler-implementation-defect" => samplerImplementationDefectSupported,
            "estimator-structurally-unidentifiable" => estimatorStructurallyUnidentifiableSupported,
            "insufficient-statistics" => insufficientStatisticsSupported,
            "observable-channel-invalid" => observableChannelInvalidSupported,
            "prospective-repair-pack-justified" => prospectiveRepairPackEvidenceComplete,
            _ => precursorsValid && !samplerImplementationDefectSupported
                && !estimatorStructurallyUnidentifiableSupported && !insufficientStatisticsSupported
                && !observableChannelInvalidSupported && !prospectiveRepairPackEvidenceComplete,
        },
        selected = classification == readinessClassification,
    }).ToArray(),
    lowerPrecedenceSupportedConditionsPreserved = new[]
    {
        new { classification = "insufficient-statistics", supported = insufficientStatisticsSupported },
    },
    prospectiveRepairPackDesignMayBeConsidered = readinessClassification == "prospective-repair-pack-justified",
    existingPhase481GatesRemainBinding = true,
    newWrittenSamplingAuthorizationOrApplicableO4CoverageStillRequired = true,
    phase456ArtifactReadOrReinterpreted = false,
    phase456ArtifactMutated = false,
    phase456TerminalChanged = false,
    phase456InvalidRowsReinterpreted = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481PackAuthorized = false,
    phase481RepairPackConstructed = false,
    freshRepairPackCreated = false,
    hmcRun = false,
    benchmarkRun = false,
    zeroNewSampling = true,
    samplingRunOrAuthorized = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    confirmationEvidence = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458GateSatisfied = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a8BoundaryHeld = true,
    decision = readinessClassification switch
    {
        "invalid-precursor" => "At least one exact precursor byte, schema, contract, taxonomy, or validity check failed. No readiness conclusion is available.",
        "sampler-implementation-defect" => "The highest-precedence supported condition is a sampler implementation defect. No repair pack or rerun is authorized.",
        "estimator-structurally-unidentifiable" => "The original T=4 ratio is structurally unable to identify the admitted spectral content and the retained artifact has channel-estimator compatibility failures. Statistics are also limited, but that lower-precedence condition does not override the estimator blocker. The underlying observable is not declared invalid, and no repair pack or rerun is authorized.",
        "insufficient-statistics" => "The highest-precedence supported condition is insufficient retained statistics. No fresh sampling is authorized.",
        "observable-channel-invalid" => "The retained evidence supports invalidity of the underlying observable or channel, not merely estimator incompatibility. No repair pack or rerun is authorized.",
        "prospective-repair-pack-justified" => "The frozen evidence justifies only considering a new prospective pack after all existing Phase481 and written-authorization gates. This phase does not create that pack or authorize sampling.",
        _ => "The valid precursor evidence does not resolve repair readiness under the frozen precedence. No repair pack or rerun is authorized.",
    },
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_prospective_repair_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_prospective_repair_readiness_adjudicator_summary.json"), json);

Require(readinessTaxonomy.Contains(readinessClassification, StringComparer.Ordinal), "readiness taxonomy drifted");
Require(precursorsValid, terminalStatus);
Require(readinessClassification == "estimator-structurally-unidentifiable", "expected conservative readiness terminal drifted");
Require(!result.phase456ArtifactReadOrReinterpreted && !result.phase456ArtifactMutated && !result.phase481PackCreated && !result.phase481PackMutated, "artifact or pack firewall failed");
Require(!result.samplingAuthorized && !result.productionAuthorized && !result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.phase458EvaluationAuthorized, "gate or launch firewall failed");
Require(!result.o4Discharged && !result.sourceContractApplicationAllowed && result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0, "authority or claim firewall failed");
Console.WriteLine(terminalStatus);

static Requirement Required(string path, params JsonValueKind[] allowedKinds) => new(path, allowedKinds);

static PrecursorBinding BindPrecursor(string path, string expectedByteHash, string expectedPhaseId,
    string expectedDecisionContractHash, string classificationProperty, string[] taxonomy, Requirement[] requirements)
{
    if (!File.Exists(path))
        return PrecursorBinding.Invalid(path, expectedByteHash, "missing-input");
    byte[] bytes = File.ReadAllBytes(path);
    string actualHash = Sha256(bytes);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        JsonElement root = document.RootElement;
        bool schemaValid = requirements.All(requirement => requirement.Matches(root));
        string phaseId = GetString(root, "phaseId");
        string classification = GetString(root, classificationProperty);
        string contractHash = GetString(root, "decisionContractSha256");
        bool inputValidity = GetBool(root, "inputsValid");
        bool valid = actualHash == expectedByteHash
            && schemaValid
            && phaseId == expectedPhaseId
            && contractHash == expectedDecisionContractHash
            && taxonomy.Contains(classification, StringComparer.Ordinal)
            && GetString(root, "verdictKind") == classification
            && inputValidity;
        var values = requirements.ToDictionary(requirement => requirement.Path, requirement => requirement.Read(root), StringComparer.Ordinal);
        return new PrecursorBinding(path, expectedByteHash, actualHash, phaseId, classification, contractHash,
            schemaValid, inputValidity, valid, valid ? "exact-byte-schema-contract-and-taxonomy-bound" : "byte-schema-contract-taxonomy-or-validity-drift", values);
    }
    catch (JsonException)
    {
        return PrecursorBinding.Invalid(path, expectedByteHash, "malformed-json", actualHash);
    }
}

static string GetString(JsonElement root, string property) => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
    ? value.GetString() ?? "" : "";

static bool GetBool(JsonElement root, string property) => root.TryGetProperty(property, out var value)
    && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class Requirement
{
    public Requirement(string path, JsonValueKind[] allowedKinds) => (Path, AllowedKinds) = (path, allowedKinds);
    public string Path { get; }
    private JsonValueKind[] AllowedKinds { get; }

    public bool Matches(JsonElement root) => TryResolve(root, out var value) && AllowedKinds.Contains(value.ValueKind);

    public object? Read(JsonElement root)
    {
        if (!TryResolve(root, out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.True or JsonValueKind.False => value.GetBoolean(),
            JsonValueKind.Number => value.GetDouble(),
            _ => value.GetRawText(),
        };
    }

    private bool TryResolve(JsonElement root, out JsonElement value)
    {
        value = root;
        foreach (string segment in Path.Split('.'))
        {
            if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(segment, out value)) return false;
        }
        return true;
    }
}

sealed class PrecursorBinding
{
    public PrecursorBinding(string path, string expectedByteSha256, string actualByteSha256, string phaseId,
        string classification, string decisionContractSha256, bool schemaValid, bool inputValidity,
        bool valid, string status, Dictionary<string, object?> values)
    {
        Path = path; ExpectedByteSha256 = expectedByteSha256; ActualByteSha256 = actualByteSha256;
        PhaseId = phaseId; Classification = classification; DecisionContractSha256 = decisionContractSha256;
        SchemaValid = schemaValid; InputValidity = inputValidity; Valid = valid; Status = status; Values = values;
    }
    public string Path { get; }
    public string ExpectedByteSha256 { get; }
    public string ActualByteSha256 { get; }
    public string PhaseId { get; }
    public string Classification { get; }
    public string DecisionContractSha256 { get; }
    public bool SchemaValid { get; }
    public bool InputValidity { get; }
    public bool Valid { get; }
    public string Status { get; }
    private Dictionary<string, object?> Values { get; }

    public bool Bool(string path) => Values.TryGetValue(path, out object? value) && value is bool boolean && boolean;

    public object Output() => new
    {
        path = Path, expectedByteSha256 = ExpectedByteSha256, actualByteSha256 = ActualByteSha256,
        byteHashMatches = ExpectedByteSha256 == ActualByteSha256, phaseId = PhaseId,
        classification = Classification, decisionContractSha256 = DecisionContractSha256,
        schemaValid = SchemaValid, precursorInputsValid = InputValidity, valid = Valid, status = Status,
    };

    public static PrecursorBinding Invalid(string path, string expectedHash, string status, string actualHash = "missing")
        => new(path, expectedHash, actualHash, "", "", "", false, false, false, status,
            new Dictionary<string, object?>(StringComparer.Ordinal));
}
