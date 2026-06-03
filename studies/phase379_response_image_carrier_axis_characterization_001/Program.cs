using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase379_response_image_carrier_axis_characterization_001/output";
const string Phase378FullPath = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output/full_connection_carrier_shell_response_gram_audit.json";
const string Phase378SummaryPath = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output/full_connection_carrier_shell_response_gram_audit_summary.json";
const string Phase307SummaryPath = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const int ExpectedBackgroundCount = 2;
const int ExpectedCarrierDimension = 156;
const int ExpectedEdgeCount = 52;
const int ExpectedGaugeDimension = 3;
const int ExpectedPositiveRank = 3;
const double TwoAxisDominanceSuppressionTolerance = 0.005;
const double StrictTransportSingularValueTolerance = 0.99;
const double LooseTransportSingularValueTolerance = 0.75;

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
var outputDir = Environment.GetEnvironmentVariable("PHASE379_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase378Full = JsonDocument.Parse(File.ReadAllText(Phase378FullPath));
using var phase378Summary = JsonDocument.Parse(File.ReadAllText(Phase378SummaryPath));
using var phase307Summary = JsonDocument.Parse(File.ReadAllText(Phase307SummaryPath));
var p378 = phase378Summary.RootElement;
var p307 = phase307Summary.RootElement;

bool phase378FullCarrierAuditPresent =
    JsonBool(p378, "fullConnectionCarrierShellResponseGramAuditPassed") is true &&
    JsonBool(p378, "targetBlindConstruction") is true &&
    JsonBool(p378, "physicalTargetsConsultedForConstruction") is false &&
    JsonInt(p378, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(p378, "minFullCarrierResponseRank") == ExpectedPositiveRank &&
    JsonInt(p378, "maxFullCarrierResponseRank") == ExpectedPositiveRank &&
    JsonInt(p378, "minFullCarrierResponseNullity") == ExpectedCarrierDimension - ExpectedPositiveRank &&
    JsonInt(p378, "maxFullCarrierResponseNullity") == ExpectedCarrierDimension - ExpectedPositiveRank &&
    JsonBool(p378, "routePromotesWzMasses") is false &&
    JsonBool(p378, "routePromotesHiggsMass") is false &&
    JsonBool(p378, "routeCompletesBosonPredictions") is false;

string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase379-response-image-carrier-axis-characterization-v1",
    $"phase378TargetBlindConstructionHash={JsonString(p378, "targetBlindConstructionHash")}",
    $"phase378FullPath={Phase378FullPath}",
    "input=Phase378 full-carrier response Gram Q",
    "carrierLayout=edge-major 52 x 3 connection-1form",
    "positiveImage=eigenspace(Q, lambda > Phase378 positiveRankTolerance)",
    "axisCapture=sum positive eigenvector coordinate squares by gauge axis",
    "transport=singular values between background positive images",
    "physicalTargetsConsultedForConstruction=false"));

var backgroundAnalyses = phase378Full.RootElement
    .GetProperty("backgroundAudits")
    .EnumerateArray()
    .Select(BuildBackgroundAnalysis)
    .ToArray();

var transport = BuildInterBackgroundTransport(backgroundAnalyses);
int backgroundPassedCount = backgroundAnalyses.Count(row => row.backgroundAnalysisPassed);
int stableSuppressedGaugeAxis = backgroundAnalyses
    .Select(row => row.suppressedGaugeAxisIndex)
    .Distinct()
    .Count() == 1
        ? backgroundAnalyses[0].suppressedGaugeAxisIndex
        : -1;
double maxSuppressedGaugeAxisProjectorFraction = backgroundAnalyses.Max(row => row.suppressedGaugeAxisProjectorFraction);
double minDominantGaugePairProjectorFraction = backgroundAnalyses.Min(row => row.dominantGaugePairProjectorFraction);
bool stableTwoGaugeAxisDominance =
    stableSuppressedGaugeAxis >= 0 &&
    maxSuppressedGaugeAxisProjectorFraction <= TwoAxisDominanceSuppressionTolerance;
bool strictBackgroundImageTransportPassed = transport.MinimumSingularValue >= StrictTransportSingularValueTolerance;
bool looseBackgroundImageTransportPassed = transport.MinimumSingularValue >= LooseTransportSingularValueTolerance;

var phase307Context = new
{
    phase307SummaryPath = Phase307SummaryPath,
    targetIndependentDecoupledWzRowSelectionLawAuditPassed = JsonBool(p307, "targetIndependentDecoupledWzRowSelectionLawAuditPassed"),
    targetObservablesUsedForConstruction = JsonBool(p307, "targetObservablesUsedForConstruction"),
    chargedAxis0CandidateCount = JsonArrayLength(p307, "chargedAxis0CandidateIds"),
    chargedAxis1CandidateCount = JsonArrayLength(p307, "chargedAxis1CandidateIds"),
    neutralAxisCandidateCount = JsonArrayLength(p307, "neutralAxisCandidateIds"),
    stableAssessmentCount = JsonInt(p307, "stableAssessmentCount"),
    rawStableCommonSelectionLawCount = JsonInt(p307, "rawStableCommonSelectionLawCount"),
    p302ScaledStableCommonSelectionLawCount = JsonInt(p307, "p302ScaledStableCommonSelectionLawCount"),
    sourceRowsPromotable = JsonBool(p307, "sourceRowsPromotable"),
    canFillPhase201WzContract = JsonBool(p307, "canFillPhase201WzContract"),
    decision = JsonString(p307, "decision"),
};

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool routeProvidesDiscreteResponseImageCharacterization = true;
const bool routeProvidesCanonicalGaugeAxisSelector = false;
const bool routeProvidesObservedElectroweakFieldMap = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
bool sourceLawPromotionBlocked =
    !strictBackgroundImageTransportPassed ||
    !routeProvidesCanonicalGaugeAxisSelector ||
    !routeProvidesObservedElectroweakFieldMap ||
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw ||
    !routeProvidesSeparateWzSourceRows ||
    !routeProvidesGeVUnitNormalization;
bool responseImageCarrierAxisCharacterizationAuditPassed =
    phase378FullCarrierAuditPresent &&
    backgroundAnalyses.Length == ExpectedBackgroundCount &&
    backgroundPassedCount == ExpectedBackgroundCount &&
    stableTwoGaugeAxisDominance &&
    looseBackgroundImageTransportPassed &&
    sourceLawPromotionBlocked &&
    !physicalTargetsConsultedForConstruction &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = responseImageCarrierAxisCharacterizationAuditPassed
    ? "response-image-carrier-axis-characterized-two-axis-dominance-nonpromotional"
    : "response-image-carrier-axis-characterization-review-required";
string decision = responseImageCarrierAxisCharacterizationAuditPassed
    ? $"The Phase378 rank-3 response image is target-blind and dominated by carrier gauge axes other than axis {stableSuppressedGaugeAxis}; the suppressed-axis projector fraction is at most {maxSuppressedGaugeAxisProjectorFraction:R}. This is a useful geometric diagnostic, but it is not a W/Z bridge-source law because the image lacks strict background identity, observed electroweak projection rows, source-derived normalization, and theorem-backed W/Z source rows."
    : "Do not use the response-image characterization until Phase378 inheritance, rank-3 eigenspace recovery, two-axis dominance, loose transport diagnostics, and explicit physical nonclaims are all resolved.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};

var result = new
{
    phaseId = "phase379-response-image-carrier-axis-characterization",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    responseImageCarrierAxisCharacterizationAuditPassed,
    phase378FullCarrierAuditPresent,
    implementedObjectClassification = "target-blind discrete full-carrier response-image eigenspace characterization",
    physicalInterpretationBoundary = "carrier image diagnostic only",
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    carrier = new
    {
        layout = "edge-major",
        edgeCount = ExpectedEdgeCount,
        gaugeDimension = ExpectedGaugeDimension,
        carrierDimension = ExpectedCarrierDimension,
    },
    tolerances = new
    {
        twoAxisDominanceSuppressionTolerance = TwoAxisDominanceSuppressionTolerance,
        strictTransportSingularValueTolerance = StrictTransportSingularValueTolerance,
        looseTransportSingularValueTolerance = LooseTransportSingularValueTolerance,
        positiveRankToleranceSource = "Phase378 per-background fullCarrierResponse.positiveRankTolerance",
    },
    backgroundCount = backgroundAnalyses.Length,
    backgroundPassedCount,
    stableTwoGaugeAxisDominance,
    stableSuppressedGaugeAxis,
    maxSuppressedGaugeAxisProjectorFraction,
    minDominantGaugePairProjectorFraction,
    strictBackgroundImageTransportPassed,
    looseBackgroundImageTransportPassed,
    interBackgroundTransport = transport,
    phase307Context,
    routeProvidesDiscreteResponseImageCharacterization,
    routeProvidesCanonicalGaugeAxisSelector,
    routeProvidesObservedElectroweakFieldMap,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesSeparateWzSourceRows,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    sourceLawPromotionBlocked,
    explicitDiscreteOnlyNonclaims = new[]
    {
        "response-image eigenspace diagnostic only",
        "not a canonical electroweak gauge-axis selector",
        "not an observed photon/W/Z/H projection map",
        "not a physical effective-action Hessian",
        "not a W/Z source row",
        "not a Higgs scalar source",
        "not GeV-normalized",
        "no predictions",
        "no contract fills",
    },
    backgroundAnalyses,
    sourceEvidence = new
    {
        phase378FullPath = Phase378FullPath,
        phase378SummaryPath = Phase378SummaryPath,
        phase307SummaryPath = Phase307SummaryPath,
    },
    decision,
};

string fullPath = Path.Combine(outputDir, "response_image_carrier_axis_characterization.json");
string summaryPath = Path.Combine(outputDir, "response_image_carrier_axis_characterization_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    responseImageCarrierAxisCharacterizationAuditPassed,
    phase378FullCarrierAuditPresent,
    result.implementedObjectClassification,
    result.physicalInterpretationBoundary,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    result.carrier,
    backgroundCount = result.backgroundCount,
    backgroundPassedCount,
    stableTwoGaugeAxisDominance,
    stableSuppressedGaugeAxis,
    maxSuppressedGaugeAxisProjectorFraction,
    minDominantGaugePairProjectorFraction,
    strictBackgroundImageTransportPassed,
    looseBackgroundImageTransportPassed,
    interBackgroundMinimumSingularValue = transport.MinimumSingularValue,
    interBackgroundMaximumPrincipalAngleDegrees = transport.MaximumPrincipalAngleDegrees,
    phase307SourceRowsPromotable = phase307Context.sourceRowsPromotable,
    phase307CanFillPhase201WzContract = phase307Context.canFillPhase201WzContract,
    routeProvidesDiscreteResponseImageCharacterization,
    routeProvidesCanonicalGaugeAxisSelector,
    routeProvidesObservedElectroweakFieldMap,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesSeparateWzSourceRows,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    sourceLawPromotionBlocked,
    result.explicitDiscreteOnlyNonclaims,
    backgroundSummaries = backgroundAnalyses.Select(row => new
    {
        row.fermionBackgroundId,
        row.positiveResponseRank,
        row.responseNullity,
        row.suppressedGaugeAxisIndex,
        row.suppressedGaugeAxisProjectorFraction,
        row.dominantGaugePairProjectorFraction,
        row.gaugeAxisProjectorFractions,
        row.gaugeAxisTraceFractions,
        row.topEdgesByProjectorCapture,
    }),
    decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"responseImageCarrierAxisCharacterizationAuditPassed={responseImageCarrierAxisCharacterizationAuditPassed}");
Console.WriteLine($"backgroundPassedCount={backgroundPassedCount}/{backgroundAnalyses.Length}");
Console.WriteLine($"stableTwoGaugeAxisDominance={stableTwoGaugeAxisDominance}");
Console.WriteLine($"stableSuppressedGaugeAxis={stableSuppressedGaugeAxis}");
Console.WriteLine($"maxSuppressedGaugeAxisProjectorFraction={maxSuppressedGaugeAxisProjectorFraction:R}");
Console.WriteLine($"minDominantGaugePairProjectorFraction={minDominantGaugePairProjectorFraction:R}");
Console.WriteLine($"strictBackgroundImageTransportPassed={strictBackgroundImageTransportPassed}");
Console.WriteLine($"looseBackgroundImageTransportPassed={looseBackgroundImageTransportPassed}");
Console.WriteLine($"interBackgroundMinimumSingularValue={transport.MinimumSingularValue:R}");
Console.WriteLine($"interBackgroundMaximumPrincipalAngleDegrees={transport.MaximumPrincipalAngleDegrees:R}");
Console.WriteLine($"routeProvidesDirectTargetIndependentWzBridgeSourceLaw={routeProvidesDirectTargetIndependentWzBridgeSourceLaw}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

BackgroundImageAnalysis BuildBackgroundAnalysis(JsonElement background)
{
    string backgroundId = JsonString(background, "fermionBackgroundId");
    var response = background.GetProperty("fullCarrierResponse");
    double[][] responseGram = ParseMatrix(response.GetProperty("responseGram"));
    if (responseGram.Length != ExpectedCarrierDimension || responseGram.Any(row => row.Length != ExpectedCarrierDimension))
        throw new InvalidDataException($"Phase378 response Gram for {backgroundId} is not {ExpectedCarrierDimension} x {ExpectedCarrierDimension}.");
    var eigen = SymmetricEigenDecomposition(responseGram);
    double positiveRankTolerance = JsonDouble(response, "positiveRankTolerance");
    var positive = eigen
        .Where(pair => pair.Value > positiveRankTolerance)
        .OrderByDescending(pair => pair.Value)
        .Take(ExpectedPositiveRank)
        .ToArray();
    double[] positiveEigenvalues = positive.Select(pair => pair.Value).ToArray();
    double[][] positiveEigenvectors = positive.Select(pair => pair.Vector).ToArray();
    int positiveRank = positive.Length;
    int responseNullity = ExpectedCarrierDimension - positiveRank;
    double[] diagonal = Enumerable.Range(0, ExpectedCarrierDimension).Select(index => responseGram[index][index]).ToArray();
    double trace = diagonal.Sum();
    double[] gaugeAxisTrace = AxisSums(diagonal);
    double[] gaugeAxisTraceFractions = gaugeAxisTrace.Select(value => value / Math.Max(trace, 1e-300)).ToArray();
    double[] gaugeAxisProjectorCapture = new double[ExpectedGaugeDimension];
    double[] edgeProjectorCapture = new double[ExpectedEdgeCount];
    double[] coordinateProjectorDiagonal = new double[ExpectedCarrierDimension];
    foreach (double[] vector in positiveEigenvectors)
        for (int coordinate = 0; coordinate < ExpectedCarrierDimension; coordinate++)
        {
            double contribution = vector[coordinate] * vector[coordinate];
            coordinateProjectorDiagonal[coordinate] += contribution;
            edgeProjectorCapture[coordinate / ExpectedGaugeDimension] += contribution;
            gaugeAxisProjectorCapture[coordinate % ExpectedGaugeDimension] += contribution;
        }
    double[] gaugeAxisProjectorFractions = gaugeAxisProjectorCapture
        .Select(value => value / Math.Max(positiveRank, 1e-300))
        .ToArray();
    int suppressedAxis = Enumerable.Range(0, ExpectedGaugeDimension)
        .OrderBy(axis => gaugeAxisProjectorFractions[axis])
        .First();
    double suppressedFraction = gaugeAxisProjectorFractions[suppressedAxis];
    double dominantPairFraction = 1.0 - suppressedFraction;
    var topEdges = edgeProjectorCapture
        .Select((value, edgeIndex) => new EdgeCapture(edgeIndex, value, value / Math.Max(positiveRank, 1e-300)))
        .OrderByDescending(row => row.ProjectorCapture)
        .Take(12)
        .ToArray();
    var topCoordinates = coordinateProjectorDiagonal
        .Select((value, coordinateIndex) => new CoordinateCapture(
            coordinateIndex,
            coordinateIndex / ExpectedGaugeDimension,
            coordinateIndex % ExpectedGaugeDimension,
            value,
            value / Math.Max(positiveRank, 1e-300),
            diagonal[coordinateIndex]))
        .OrderByDescending(row => row.ProjectorDiagonal)
        .Take(16)
        .ToArray();
    bool rankPassed =
        positiveRank == ExpectedPositiveRank &&
        responseNullity == ExpectedCarrierDimension - ExpectedPositiveRank;
    bool twoAxisDominancePassed = suppressedFraction <= TwoAxisDominanceSuppressionTolerance;
    bool backgroundAnalysisPassed = rankPassed && twoAxisDominancePassed;
    return new()
    {
        fermionBackgroundId = backgroundId,
        positiveRankTolerance = positiveRankTolerance,
        positiveEigenvalues = positiveEigenvalues,
        positiveResponseRank = positiveRank,
        responseNullity = responseNullity,
        trace = trace,
        gaugeAxisTrace = gaugeAxisTrace,
        gaugeAxisTraceFractions = gaugeAxisTraceFractions,
        gaugeAxisProjectorCapture = gaugeAxisProjectorCapture,
        gaugeAxisProjectorFractions = gaugeAxisProjectorFractions,
        suppressedGaugeAxisIndex = suppressedAxis,
        suppressedGaugeAxisProjectorFraction = suppressedFraction,
        dominantGaugePairProjectorFraction = dominantPairFraction,
        topEdgesByProjectorCapture = topEdges,
        topCoordinatesByProjectorDiagonal = topCoordinates,
        positiveEigenvectors = positiveEigenvectors,
        rankPassed = rankPassed,
        twoAxisDominancePassed = twoAxisDominancePassed,
        backgroundAnalysisPassed = backgroundAnalysisPassed,
    };
}

InterBackgroundTransport BuildInterBackgroundTransport(IReadOnlyList<BackgroundImageAnalysis> analyses)
{
    if (analyses.Count != 2)
        return new()
        {
            BackgroundPair = "not-available",
            SingularValues = Array.Empty<double>(),
            PrincipalAnglesDegrees = Array.Empty<double>(),
            MinimumSingularValue = 0.0,
            MaximumPrincipalAngleDegrees = 90.0,
            StrictTransportPassed = false,
            LooseTransportPassed = false,
        };
    double[][] overlap = CrossGramRows(analyses[0].positiveEigenvectors, analyses[1].positiveEigenvectors);
    double[] singularValues = SingularValues(overlap);
    double[] angles = singularValues
        .Select(value => Math.Acos(Math.Clamp(value, -1.0, 1.0)) * 180.0 / Math.PI)
        .ToArray();
    double minSingular = singularValues.Min();
    double maxAngle = angles.Max();
    return new()
    {
        BackgroundPair = $"{analyses[0].fermionBackgroundId}|{analyses[1].fermionBackgroundId}",
        SingularValues = singularValues,
        PrincipalAnglesDegrees = angles,
        MinimumSingularValue = minSingular,
        MaximumPrincipalAngleDegrees = maxAngle,
        StrictTransportPassed = minSingular >= StrictTransportSingularValueTolerance,
        LooseTransportPassed = minSingular >= LooseTransportSingularValueTolerance,
    };
}

static double[] AxisSums(double[] coordinateValues)
{
    var result = new double[ExpectedGaugeDimension];
    for (int coordinate = 0; coordinate < coordinateValues.Length; coordinate++)
        result[coordinate % ExpectedGaugeDimension] += coordinateValues[coordinate];
    return result;
}

static double[][] CrossGramRows(double[][] leftRows, double[][] rightRows) => leftRows
    .Select(left => rightRows.Select(right => Dot(left, right)).ToArray())
    .ToArray();

static double[] SingularValues(double[][] matrix)
{
    int rows = matrix.Length;
    int cols = matrix[0].Length;
    var gram = Enumerable.Range(0, cols).Select(_ => new double[cols]).ToArray();
    for (int row = 0; row < rows; row++)
        for (int left = 0; left < cols; left++)
            for (int right = 0; right < cols; right++)
                gram[left][right] += matrix[row][left] * matrix[row][right];
    return SymmetricEigenDecomposition(gram)
        .Select(pair => Math.Sqrt(Math.Max(0.0, pair.Value)))
        .OrderByDescending(value => value)
        .ToArray();
}

static EigenPair[] SymmetricEigenDecomposition(double[][] input)
{
    int n = input.Length;
    var matrix = input.Select(row => row.ToArray()).ToArray();
    var vectors = Enumerable.Range(0, n)
        .Select(row => Enumerable.Range(0, n).Select(col => row == col ? 1.0 : 0.0).ToArray())
        .ToArray();
    for (int iteration = 0; iteration < 100 * n * n; iteration++)
    {
        int pivotRow = 0;
        int pivotCol = 1;
        double maxOffDiagonal = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = row + 1; col < n; col++)
                if (Math.Abs(matrix[row][col]) > maxOffDiagonal)
                {
                    maxOffDiagonal = Math.Abs(matrix[row][col]);
                    pivotRow = row;
                    pivotCol = col;
                }
        if (maxOffDiagonal <= 1e-14)
            break;
        double angle = 0.5 * Math.Atan2(
            2.0 * matrix[pivotRow][pivotCol],
            matrix[pivotCol][pivotCol] - matrix[pivotRow][pivotRow]);
        double cosine = Math.Cos(angle);
        double sine = Math.Sin(angle);
        for (int index = 0; index < n; index++)
            if (index != pivotRow && index != pivotCol)
            {
                double left = matrix[index][pivotRow];
                double right = matrix[index][pivotCol];
                matrix[index][pivotRow] = matrix[pivotRow][index] = cosine * left - sine * right;
                matrix[index][pivotCol] = matrix[pivotCol][index] = sine * left + cosine * right;
            }
        double diagonalLeft = matrix[pivotRow][pivotRow];
        double diagonalRight = matrix[pivotCol][pivotCol];
        double offDiagonal = matrix[pivotRow][pivotCol];
        matrix[pivotRow][pivotRow] =
            cosine * cosine * diagonalLeft -
            2.0 * sine * cosine * offDiagonal +
            sine * sine * diagonalRight;
        matrix[pivotCol][pivotCol] =
            sine * sine * diagonalLeft +
            2.0 * sine * cosine * offDiagonal +
            cosine * cosine * diagonalRight;
        matrix[pivotRow][pivotCol] = matrix[pivotCol][pivotRow] = 0.0;
        for (int row = 0; row < n; row++)
        {
            double left = vectors[row][pivotRow];
            double right = vectors[row][pivotCol];
            vectors[row][pivotRow] = cosine * left - sine * right;
            vectors[row][pivotCol] = sine * left + cosine * right;
        }
    }
    return Enumerable.Range(0, n)
        .Select(index =>
        {
            double[] vector = vectors.Select(row => row[index]).ToArray();
            double norm = Math.Sqrt(Dot(vector, vector));
            if (norm > 0.0)
                vector = vector.Select(value => value / norm).ToArray();
            return new EigenPair(matrix[index][index], vector);
        })
        .OrderByDescending(pair => pair.Value)
        .ToArray();
}

static double[][] ParseMatrix(JsonElement element) => element
    .EnumerateArray()
    .Select(row => row.EnumerateArray().Select(value => value.GetDouble()).ToArray())
    .ToArray();

static double Dot(double[] left, double[] right) =>
    left.Zip(right, (l, r) => l * r).Sum();

static string HashText(string text) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Number &&
    value.TryGetInt32(out int result)
        ? result
        : null;

static double JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidDataException($"{propertyName} must be a number.");

static string JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? ""
        : "";

static int JsonArrayLength(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.GetArrayLength()
        : 0;

public sealed record EigenPair(double Value, double[] Vector);
public sealed record EdgeCapture(int EdgeIndex, double ProjectorCapture, double ProjectorFraction);
public sealed record CoordinateCapture(
    int CoordinateIndex,
    int EdgeIndex,
    int GaugeAxisIndex,
    double ProjectorDiagonal,
    double ProjectorFraction,
    double ResponseGramDiagonal);

public sealed class BackgroundImageAnalysis
{
    public required string fermionBackgroundId { get; init; }
    public required double positiveRankTolerance { get; init; }
    public required double[] positiveEigenvalues { get; init; }
    public required int positiveResponseRank { get; init; }
    public required int responseNullity { get; init; }
    public required double trace { get; init; }
    public required double[] gaugeAxisTrace { get; init; }
    public required double[] gaugeAxisTraceFractions { get; init; }
    public required double[] gaugeAxisProjectorCapture { get; init; }
    public required double[] gaugeAxisProjectorFractions { get; init; }
    public required int suppressedGaugeAxisIndex { get; init; }
    public required double suppressedGaugeAxisProjectorFraction { get; init; }
    public required double dominantGaugePairProjectorFraction { get; init; }
    public required EdgeCapture[] topEdgesByProjectorCapture { get; init; }
    public required CoordinateCapture[] topCoordinatesByProjectorDiagonal { get; init; }
    public required bool rankPassed { get; init; }
    public required bool twoAxisDominancePassed { get; init; }
    public required bool backgroundAnalysisPassed { get; init; }
    public required double[][] positiveEigenvectors { get; init; }
}

public sealed class InterBackgroundTransport
{
    public required string BackgroundPair { get; init; }
    public required double[] SingularValues { get; init; }
    public required double[] PrincipalAnglesDegrees { get; init; }
    public required double MinimumSingularValue { get; init; }
    public required double MaximumPrincipalAngleDegrees { get; init; }
    public required bool StrictTransportPassed { get; init; }
    public required bool LooseTransportPassed { get; init; }
}
