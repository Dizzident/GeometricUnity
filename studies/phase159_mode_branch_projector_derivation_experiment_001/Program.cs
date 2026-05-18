using System.Numerics;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase159_mode_branch_projector_derivation_experiment_001/output";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase133Path = "studies/phase133_fermion_identity_feature_extractor_001/output/fermion_identity_feature_extractor.json";
const string Phase138Path = "studies/phase138_repaired_row_coupling_transition_graph_001/output/repaired_row_coupling_transition_graph.json";
const string Phase158Path = "studies/phase158_external_gu_source_unblocker_audit_001/output/external_gu_source_unblocker_audit.json";

const int VertexCount = 27;
const int GaugeDimension = 3;
const int SpinorComponents = 4;
const double PurityThreshold = 0.80;
const double GapThreshold = 0.20;
const double DirectionalityThreshold = 0.20;

var outputDir = Environment.GetEnvironmentVariable("PHASE159_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var phase133 = JsonDocument.Parse(File.ReadAllText(Phase133Path));
using var phase138 = JsonDocument.Parse(File.ReadAllText(Phase138Path));
using var phase158 = File.Exists(Phase158Path) ? JsonDocument.Parse(File.ReadAllText(Phase158Path)) : null;

var featureRows = phase133.RootElement.GetProperty("featureRecords")
    .EnumerateArray()
    .Select(row => new
    {
        FamilyId = RequiredString(row, "familyId"),
        ModeId = RequiredString(row, "modeId"),
        CandidateId = RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId = RequiredString(row, "sourceCanonicalFermionModeId"),
    })
    .ToArray();

var modeById = phase95.RootElement.GetProperty("modes")
    .EnumerateArray()
    .ToDictionary(mode => RequiredString(mode, "modeId"), mode => mode.Clone(), StringComparer.Ordinal);

var projectorRows = new List<ProjectorRow>();
foreach (var featureRow in featureRows)
{
    if (!modeById.TryGetValue(featureRow.ModeId, out var mode))
        throw new InvalidOperationException($"Mode {featureRow.ModeId} not found in {Phase95ModesPath}");

    var coefficients = mode.GetProperty("eigenvectorCoefficients")
        .EnumerateArray()
        .Select(x => x.GetDouble())
        .ToArray();
    var complex = ToComplexCoefficients(coefficients);
    var gaugeDensity = BuildGaugeDensity(complex);
    var spinorDensity = BuildSpinorDensity(complex);
    var gaugeEigenvalues = EigenvaluesHermitianViaRealEmbedding(gaugeDensity, GaugeDimension);
    var spinorEigenvalues = EigenvaluesHermitianViaRealEmbedding(spinorDensity, SpinorComponents);
    var purity = gaugeEigenvalues[0];
    var gap = gaugeEigenvalues[0] - gaugeEigenvalues[1];
    var projectorPromotable = purity >= PurityThreshold && gap >= GapThreshold;

    projectorRows.Add(new ProjectorRow(
        featureRow.FamilyId,
        featureRow.CandidateId,
        featureRow.SourceCanonicalFermionModeId,
        featureRow.ModeId,
        gaugeDensity.Select(row => row.Select(z => new ComplexCell(z.Real, z.Imaginary)).ToArray()).ToArray(),
        gaugeEigenvalues,
        spinorEigenvalues,
        purity,
        gap,
        projectorPromotable,
        projectorPromotable
            ? "projector-branch-candidate"
            : "mixed-gauge-density-no-canonical-branch"));
}

var strongestTransition = phase138.RootElement.GetProperty("strongestOffDiagonalTransition");
double directionalityRatio = JsonDouble(strongestTransition, "directionalityRatio") ?? 0.0;
double offDiagonalDominanceRatio = JsonDouble(strongestTransition, "offDiagonalDominanceRatio") ?? 0.0;
bool allProjectorsPromotable = projectorRows.All(row => row.ProjectorPromotable);
bool transitionDirectionPromotable = directionalityRatio >= DirectionalityThreshold;
bool p140CandidatePromotable = allProjectorsPromotable && transitionDirectionPromotable;

string terminalStatus = p140CandidatePromotable
    ? "mode-branch-projector-derivation-p140-candidate-ready"
    : allProjectorsPromotable
        ? "mode-branch-projector-derivation-projectors-ready-transition-direction-blocked"
        : "mode-branch-projector-derivation-mixed-current-modes";

var result = new
{
    phaseId = "phase159-mode-branch-projector-derivation-experiment",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    calculation = new
    {
        method = "reduced Hermitian gauge-density projector diagonalization",
        layout = "((vertex * dimG + gauge) * spinorComponents + spinor), complex coefficients from real/imag interleaved vector",
        vertexCount = VertexCount,
        gaugeDimension = GaugeDimension,
        spinorComponents = SpinorComponents,
        externalTargetValuesUsed = false,
    },
    gates = new
    {
        purityThreshold = PurityThreshold,
        gapThreshold = GapThreshold,
        directionalityThreshold = DirectionalityThreshold,
        allProjectorsPromotable,
        transitionDirectionPromotable,
        p140CandidatePromotable,
    },
    projectorRows,
    transitionEvidence = new
    {
        strongestBosonModeId = JsonString(strongestTransition, "bosonModeId"),
        offDiagonalDominanceRatio,
        directionalityRatio,
        directionalityPromotable = transitionDirectionPromotable,
        evidenceId = JsonString(strongestTransition, "variationEvidenceId"),
    },
    interpretation = p140CandidatePromotable
        ? "The repaired modes select target-independent gauge-density projectors and the transition graph has enough directionality to attempt a P140 intake artifact."
        : allProjectorsPromotable
            ? "The repaired modes select clean target-independent gauge-density projectors, but the current transition graph remains magnitude-symmetric; this supplies a branch-projector candidate but not yet a directed W/Z bridge."
            : "The current repaired modes do not select pure gauge-density projectors; this calculation does not supply the missing P140 mode-to-branch bridge.",
    nextCalculationIfBlocked = new[]
    {
        "construct a nontrivial charge-conjugation operator from the full Clifford convention instead of the current Hermitian placeholder",
        "rerun repaired modes in an even-dimensional spinor representation with nontrivial chirality",
        "repeat this projector test across refinement levels and backgrounds after nontrivial chirality/conjugation is available",
    },
    sourceEvidence = new
    {
        phase95ModesPath = Phase95ModesPath,
        phase133Path = Phase133Path,
        phase138Path = Phase138Path,
        phase158Path = File.Exists(Phase158Path) ? Phase158Path : null,
        phase158Status = phase158 is null ? null : JsonString(phase158.RootElement, "terminalStatus"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "mode_branch_projector_derivation_experiment.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "mode_branch_projector_derivation_experiment_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.gates,
        result.transitionEvidence,
        result.interpretation,
        result.nextCalculationIfBlocked,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "mode_branch_projector_derivation_experiment.md"),
    BuildMarkdown(terminalStatus, projectorRows, directionalityRatio, p140CandidatePromotable));

Console.WriteLine(terminalStatus);
Console.WriteLine($"allProjectorsPromotable={allProjectorsPromotable}");
Console.WriteLine($"transitionDirectionPromotable={transitionDirectionPromotable}");
Console.WriteLine($"p140CandidatePromotable={p140CandidatePromotable}");

static Complex[] ToComplexCoefficients(IReadOnlyList<double> values)
{
    if (values.Count != VertexCount * GaugeDimension * SpinorComponents * 2)
        throw new InvalidOperationException($"Expected {VertexCount * GaugeDimension * SpinorComponents * 2} real coefficients, found {values.Count}.");

    var complex = new Complex[VertexCount * GaugeDimension * SpinorComponents];
    for (int i = 0; i < complex.Length; i++)
        complex[i] = new Complex(values[2 * i], values[2 * i + 1]);
    return complex;
}

static Complex[][] BuildGaugeDensity(IReadOnlyList<Complex> coefficients)
{
    var density = Matrix(GaugeDimension);
    for (int vertex = 0; vertex < VertexCount; vertex++)
    for (int leftGauge = 0; leftGauge < GaugeDimension; leftGauge++)
    for (int rightGauge = 0; rightGauge < GaugeDimension; rightGauge++)
    for (int spinor = 0; spinor < SpinorComponents; spinor++)
    {
        var left = coefficients[Index(vertex, leftGauge, spinor)];
        var right = coefficients[Index(vertex, rightGauge, spinor)];
        density[leftGauge][rightGauge] += left * Complex.Conjugate(right);
    }

    return NormalizeTrace(density);
}

static Complex[][] BuildSpinorDensity(IReadOnlyList<Complex> coefficients)
{
    var density = Matrix(SpinorComponents);
    for (int vertex = 0; vertex < VertexCount; vertex++)
    for (int gauge = 0; gauge < GaugeDimension; gauge++)
    for (int leftSpinor = 0; leftSpinor < SpinorComponents; leftSpinor++)
    for (int rightSpinor = 0; rightSpinor < SpinorComponents; rightSpinor++)
    {
        var left = coefficients[Index(vertex, gauge, leftSpinor)];
        var right = coefficients[Index(vertex, gauge, rightSpinor)];
        density[leftSpinor][rightSpinor] += left * Complex.Conjugate(right);
    }

    return NormalizeTrace(density);
}

static int Index(int vertex, int gauge, int spinor) =>
    ((vertex * GaugeDimension + gauge) * SpinorComponents) + spinor;

static Complex[][] Matrix(int n) =>
    Enumerable.Range(0, n).Select(_ => new Complex[n]).ToArray();

static Complex[][] NormalizeTrace(Complex[][] matrix)
{
    double trace = 0.0;
    for (int i = 0; i < matrix.Length; i++)
        trace += matrix[i][i].Real;

    if (trace <= 0.0)
        throw new InvalidOperationException("Cannot normalize density matrix with non-positive trace.");

    for (int i = 0; i < matrix.Length; i++)
    for (int j = 0; j < matrix.Length; j++)
        matrix[i][j] /= trace;

    return matrix;
}

static double[] EigenvaluesHermitianViaRealEmbedding(Complex[][] hermitian, int n)
{
    int m = 2 * n;
    var real = new double[m, m];
    for (int i = 0; i < n; i++)
    for (int j = 0; j < n; j++)
    {
        var z = hermitian[i][j];
        real[i, j] = z.Real;
        real[i, j + n] = -z.Imaginary;
        real[i + n, j] = z.Imaginary;
        real[i + n, j + n] = z.Real;
    }

    var embedded = JacobiEigenvalues(real, 100, 1e-14)
        .OrderDescending()
        .ToArray();

    var values = new double[n];
    for (int i = 0; i < n; i++)
        values[i] = embedded[2 * i];
    return values;
}

static double[] JacobiEigenvalues(double[,] a, int maxSweeps, double tolerance)
{
    int n = a.GetLength(0);
    for (int sweep = 0; sweep < maxSweeps; sweep++)
    {
        int p = 0;
        int q = 1;
        double max = 0.0;
        for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++)
        {
            double value = Math.Abs(a[i, j]);
            if (value > max)
            {
                max = value;
                p = i;
                q = j;
            }
        }

        if (max < tolerance)
            break;

        double app = a[p, p];
        double aqq = a[q, q];
        double apq = a[p, q];
        double tau = (aqq - app) / (2.0 * apq);
        double t = Math.Sign(tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
        if (tau == 0.0)
            t = 1.0;
        double c = 1.0 / Math.Sqrt(1.0 + t * t);
        double s = t * c;

        for (int k = 0; k < n; k++)
        {
            if (k == p || k == q)
                continue;
            double akp = a[k, p];
            double akq = a[k, q];
            a[k, p] = c * akp - s * akq;
            a[p, k] = a[k, p];
            a[k, q] = s * akp + c * akq;
            a[q, k] = a[k, q];
        }

        a[p, p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
        a[q, q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
        a[p, q] = 0.0;
        a[q, p] = 0.0;
    }

    var values = new double[n];
    for (int i = 0; i < n; i++)
        values[i] = Math.Abs(a[i, i]) < 1e-12 ? 0.0 : a[i, i];
    return values;
}

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<ProjectorRow> rows,
    double directionalityRatio,
    bool p140CandidatePromotable)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Mode-Branch Projector Derivation Experiment");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine($"P140 candidate promotable: `{p140CandidatePromotable}`");
    builder.AppendLine();
    builder.AppendLine("## Projector Rows");
    foreach (var row in rows)
        builder.AppendLine($"- `{row.FamilyId}`: purity `{row.GaugeDensityPurity:G6}`, gap `{row.GaugeDensityLeadingGap:G6}`, status `{row.ProjectorStatus}`");
    builder.AppendLine();
    builder.AppendLine($"Transition directionality ratio: `{directionalityRatio:G6}`");
    return builder.ToString();
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;

sealed record ComplexCell(double Re, double Im);

sealed record ProjectorRow(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    string ModeId,
    ComplexCell[][] GaugeDensityMatrix,
    double[] GaugeDensityEigenvaluesDescending,
    double[] SpinorDensityEigenvaluesDescending,
    double GaugeDensityPurity,
    double GaugeDensityLeadingGap,
    bool ProjectorPromotable,
    string ProjectorStatus);
