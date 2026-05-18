using System.Text.Json;

const string DefaultOutputDir = "studies/phase218_official_gu_public_source_audit_001/output";

var outputDir = Environment.GetEnvironmentVariable("PHASE218_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var reviewedSources = new[]
{
    new ReviewedSource(
        "official-gu-site-draft-release",
        "https://geometricunity.org/",
        "lines 25-28",
        "Official site identifies the April 1, 2021 GU manuscript draft.",
        "Establishes the public draft source that was checked."),
    new ReviewedSource(
        "official-2013-oxford-lecture-higgs-naturality",
        "https://geometricunity.org/2013-oxford-lecture/",
        "transcript lines 269-270 and 879-887",
        "The official lecture transcript frames the Higgs sector as geometrically artificial in the Standard Model and later places Yang-Mills/Higgs content in a Dirac-square program.",
        "Research-program evidence only; no W/Z absolute-mass theorem, Higgs scalar-source operator, or prediction row is supplied."),
    new ReviewedSource(
        "official-draft-second-order-bosonic-equation",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 44, lines 2352-2403",
        "Displays the second-order Yang-Mills-Maxwell-like bosonic equation.",
        "Architecture evidence only; no W/Z particle rows, normalization law, or mass comparison gates."),
    new ReviewedSource(
        "official-draft-bosonic-decompositions",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 52, lines 3259-3266",
        "Introduces bosonic decompositions through structure-group reduction stages.",
        "Decomposition evidence only; no derivation-backed W/Z absolute-mass source lineage."),
    new ReviewedSource(
        "official-draft-equation-summary",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 54, lines 3419-3427",
        "Frames Einstein, Dirac, Yang-Mills, and Klein-Gordon sectors as not directly unified.",
        "Supports a research program, not a solved Higgs scalar-source operator."),
    new ReviewedSource(
        "official-draft-dirac-pair-table",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 57, lines 3588-3617",
        "Places Yang-Mills and Klein-Gordon on a second-order stratum in a Dirac-pair picture.",
        "Suggestive sector placement only; no Higgs potential/self-coupling lineage or prediction row."),
    new ReviewedSource(
        "pdg-standard-electroweak-input-dependency",
        "https://pdg.lbl.gov/2024/reviews/rpp2024-rev-standard-model.pdf",
        "section 10.1 and section 10.2.4",
        "The Standard Model electroweak sector uses SU(2)xU(1), couplings, Higgs vacuum expectation value, and renormalized weak-angle relations for W/Z physics.",
        "Context only; these relations require electroweak input parameters and are not a GU target-independent bridge-source law."),
    new ReviewedSource(
        "pdg-w-z-higgs-target-values",
        "https://pdg.lbl.gov/2024/listings/contents_listings.html",
        "Gauge & Higgs Bosons listings",
        "PDG listings provide current W, Z, and Higgs mass values used as experimental targets.",
        "Target comparison only; measured values cannot fill sourceLineageId, theoremOrDerivationId, scalarSourceOperatorId, or stability sidecars."),
};

var acceptanceChecks = new[]
{
    new Check("official-public-source-reviewed", true, "Official GU site, 2013 Oxford transcript, and April 1, 2021 draft passages were checked."),
    new Check("public-source-wz-direct-law-found", false, "No checked public passage supplies a target-independent W/Z absolute-mass theorem with W and Z source rows, raw gates, common bridge gates, stability sidecars, and post-construction target comparison."),
    new Check("public-source-higgs-scalar-source-found", false, "No checked public passage supplies a solved target-independent Higgs scalar source/operator with identity envelope, massive profile, potential/excitation relation, stability sidecars, and prediction row."),
    new Check("standard-electroweak-context-reviewed", true, "PDG electroweak references were reviewed as physics context and target-comparison context, not as GU source-lineage evidence."),
    new Check("public-source-shortcut-free", true, "The audit does not use W/Z target masses, external electroweak inputs, CODATA/Fermi values, observed Higgs mass, or target-implied couplings as source evidence."),
};

var officialDraftProvidesDirectWzLaw = false;
var officialDraftProvidesSolvedHiggsSource = false;
var officialDraftProvidesCompletionSource = officialDraftProvidesDirectWzLaw || officialDraftProvidesSolvedHiggsSource;
var officialPublicSourceAuditMaterialized = true;
var terminalStatus = officialDraftProvidesCompletionSource
    ? "official-gu-public-source-audit-completion-source-found"
    : "official-gu-public-source-audit-no-completion-source-found";

var result = new
{
    phaseId = "phase218-official-gu-public-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    officialPublicSourceAuditMaterialized,
    officialDraftProvidesDirectWzLaw,
    officialDraftProvidesSolvedHiggsSource,
    officialDraftProvidesCompletionSource,
    reviewedSources,
    acceptanceChecks,
    conclusion = "The official GU public source check did not find the missing W/Z direct bridge-source theorem or solved Higgs scalar source/operator. The public draft supports the architectural research program but not the Phase201/P209/P210 source-lineage evidence needed for W/Z absolute masses or Higgs mass.",
    standardModelContext = "Standard electroweak W/Z mass relations depend on electroweak inputs such as gauge couplings, weak mixing angle, and Higgs vacuum expectation value, while PDG W/Z/Higgs masses are experimental target values. These are valid comparison data but do not provide a target-independent GU source lineage.",
    implication = "Do not promote W/Z absolute masses or Higgs mass from official public GU draft passages alone.",
    nextRequiredWork = new[]
    {
        "W/Z: a new derivation-backed target-independent bridge-source theorem with separate W and Z source rows and all source-lineage gates.",
        "Higgs: a new solved scalar-sector source/operator lineage with identity, massive profile, potential/excitation, stability, and prediction gates.",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "official_gu_public_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "official_gu_public_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.officialPublicSourceAuditMaterialized,
        result.officialDraftProvidesDirectWzLaw,
        result.officialDraftProvidesSolvedHiggsSource,
        result.officialDraftProvidesCompletionSource,
        result.reviewedSources,
        result.acceptanceChecks,
        result.conclusion,
        result.standardModelContext,
        result.implication,
        result.nextRequiredWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"officialDraftProvidesDirectWzLaw={officialDraftProvidesDirectWzLaw}");
Console.WriteLine($"officialDraftProvidesSolvedHiggsSource={officialDraftProvidesSolvedHiggsSource}");

sealed record ReviewedSource(
    string SourceId,
    string Url,
    string Locator,
    string Finding,
    string PredictionImpact);

sealed record Check(string CheckId, bool Passed, string Detail);
