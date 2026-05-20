using System.Text.Json;

const string DefaultOutputDir = "studies/phase316_ucsd_transcript_source_strength_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase315Path = "studies/phase315_ucsd_dark_geometric_energy_source_audit_001/output/ucsd_dark_geometric_energy_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE316_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase315 = JsonDocument.Parse(File.ReadAllText(Phase315Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase315SourceAuditPassed = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergySourceAuditPassed") is true;
var phase315EditedTranscriptAvailable = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyEditedTranscriptAvailable") is true;
var phase315PromotesWzMasses = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesWzMasses") is true;
var phase315PromotesHiggsMass = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesHiggsMass") is true;
var phase315CompletesBosonPredictions = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyCompletesBosonPredictions") is true;

const string youtubeVideoId = "fBozSSLxFvI";
const string vimeoThumbnailId = "1098731053";
const bool portalGroupPageAvailable = true;
const bool portalWikiMetadataAvailable = true;
const bool portalWikiEditedTranscriptAvailable = false;
const bool portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess = true;
const bool localYtDlpAvailable = false;
const bool localYoutubeDlAvailable = false;
const bool directTimedTextCaptionListEndpointChecked = true;
const bool directTimedTextCaptionListReturnedEmpty = true;
const int directTimedTextCaptionListTrackCount = 0;
const bool directTimedTextCaptionListUsableAsSourceLineage = false;
const bool publicSearchExactVideoTranscriptFound = false;
const bool publicSearchExactVideoCaptionsFound = false;
const bool thirdPartyShimpsSummaryFound = true;
const bool thirdPartyShimpsSummaryIsPrimarySource = false;
const bool thirdPartyShimpsSummaryIsTranscript = false;
const bool thirdPartyShimpsSummaryProvidesWzSourceRows = false;
const bool thirdPartyShimpsSummaryProvidesHiggsScalarSource = false;
const bool timedTextArtifactMaterializedInRepo = false;
const bool captionOrTranscriptUsableAsSourceLineage = false;
const bool transcriptAuditProvidesGuLocalWzTheorem = false;
const bool transcriptAuditProvidesSeparateWzSourceRows = false;
const bool transcriptAuditProvidesLowEnergyWeakCouplingSource = false;
const bool transcriptAuditProvidesTargetIndependentVevSource = false;
const bool transcriptAuditProvidesPhotonWzEigenstateProjectionRows = false;
const bool transcriptAuditProvidesObservedFieldExtraction = false;
const bool transcriptAuditProvidesHiggsScalarSourceOperator = false;
const bool transcriptAuditProvidesHiggsSelfCouplingSource = false;
const bool transcriptAuditPromotesWzMasses = false;
const bool transcriptAuditPromotesHiggsMass = false;
const bool transcriptAuditCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var sourceStrengthRows = new[]
{
    new SourceStrengthRow(
        "portal-group-public-page",
        "Official Portal Group page",
        "https://theportal.group/from-dark-to-geometric-energy-a-sector-of-geometric-unity/",
        IsPrimaryOrOfficialPortalSource: true,
        IsTranscriptOrCaptionArtifact: false,
        IsRepoMaterialized: false,
        IsPromotableSourceLineage: false,
        Finding: "The page exposes the public abstract and YouTube-linked video lead, but not transcript or derivation rows that can fill Phase201/256 fields."),
    new SourceStrengthRow(
        "portal-wiki-no-edited-transcript",
        "Portal Wiki metadata and transcript status",
        "https://theportal.wiki/wiki/From_Dark_to_Geometric_Energy_-_A_Sector_of_Geometric_Unity_%28YouTube_Content%29",
        IsPrimaryOrOfficialPortalSource: false,
        IsTranscriptOrCaptionArtifact: false,
        IsRepoMaterialized: false,
        IsPromotableSourceLineage: false,
        Finding: "The wiki records host, release date, YouTube link, length, and the absence of an edited transcript; it points to private machine-generated transcript access rather than a public artifact."),
    new SourceStrengthRow(
        "youtube-timedtext-caption-list",
        "YouTube TimedText caption-list endpoint",
        "https://video.google.com/timedtext?type=list&v=fBozSSLxFvI",
        IsPrimaryOrOfficialPortalSource: false,
        IsTranscriptOrCaptionArtifact: false,
        IsRepoMaterialized: false,
        IsPromotableSourceLineage: false,
        Finding: "A direct caption-list endpoint probe returned an empty response, so no public TimedText caption track was materialized for source-lineage use."),
    new SourceStrengthRow(
        "public-search-exact-video-transcript",
        "Exact public transcript/caption search",
        "search: \"fBozSSLxFvI\" transcript; \"fBozSSLxFvI\" captions; \"From Dark to Geometric Energy\" transcript",
        IsPrimaryOrOfficialPortalSource: false,
        IsTranscriptOrCaptionArtifact: false,
        IsRepoMaterialized: false,
        IsPromotableSourceLineage: false,
        Finding: "Search found Portal metadata, no-transcript category pages, and third-party summaries, but no public transcript or caption artifact suitable for repo promotion."),
    new SourceStrengthRow(
        "shimpsblog-geometric-unity-iii",
        "Third-party Geometric Unity III summary",
        "https://blog.shimps.org/blogpost557-Geometric-Unity-III",
        IsPrimaryOrOfficialPortalSource: false,
        IsTranscriptOrCaptionArtifact: false,
        IsRepoMaterialized: false,
        IsPromotableSourceLineage: false,
        Finding: "The page confirms the YouTube video identifier and summarizes GU context, but it is not a primary source, transcript, theorem, or W/Z/H source-lineage derivation."),
};

var checks = new[]
{
    new Check(
        "public-video-identity-recorded",
        portalGroupPageAvailable
            && portalWikiMetadataAvailable
            && youtubeVideoId == "fBozSSLxFvI",
        $"portalGroupPageAvailable={portalGroupPageAvailable}; portalWikiMetadataAvailable={portalWikiMetadataAvailable}; youtubeVideoId={youtubeVideoId}; vimeoThumbnailId={vimeoThumbnailId}"),
    new Check(
        "edited-transcript-not-publicly-available",
        !portalWikiEditedTranscriptAvailable
            && !phase315EditedTranscriptAvailable,
        $"portalWikiEditedTranscriptAvailable={portalWikiEditedTranscriptAvailable}; phase315EditedTranscriptAvailable={phase315EditedTranscriptAvailable}; privateMachineTranscriptAccessNoted={portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess}"),
    new Check(
        "local-caption-tools-not-available",
        !localYtDlpAvailable
            && !localYoutubeDlAvailable,
        $"localYtDlpAvailable={localYtDlpAvailable}; localYoutubeDlAvailable={localYoutubeDlAvailable}"),
    new Check(
        "direct-timedtext-caption-list-empty",
        directTimedTextCaptionListEndpointChecked
            && directTimedTextCaptionListReturnedEmpty
            && directTimedTextCaptionListTrackCount == 0
            && !directTimedTextCaptionListUsableAsSourceLineage,
        $"endpointChecked={directTimedTextCaptionListEndpointChecked}; returnedEmpty={directTimedTextCaptionListReturnedEmpty}; trackCount={directTimedTextCaptionListTrackCount}; usableAsSourceLineage={directTimedTextCaptionListUsableAsSourceLineage}"),
    new Check(
        "public-transcript-search-not-promotable",
        !publicSearchExactVideoTranscriptFound
            && !publicSearchExactVideoCaptionsFound
            && !timedTextArtifactMaterializedInRepo
            && !captionOrTranscriptUsableAsSourceLineage,
        $"publicTranscriptFound={publicSearchExactVideoTranscriptFound}; publicCaptionsFound={publicSearchExactVideoCaptionsFound}; timedTextArtifactMaterializedInRepo={timedTextArtifactMaterializedInRepo}; usableAsSourceLineage={captionOrTranscriptUsableAsSourceLineage}"),
    new Check(
        "third-party-summary-not-source-lineage",
        thirdPartyShimpsSummaryFound
            && !thirdPartyShimpsSummaryIsPrimarySource
            && !thirdPartyShimpsSummaryIsTranscript
            && !thirdPartyShimpsSummaryProvidesWzSourceRows
            && !thirdPartyShimpsSummaryProvidesHiggsScalarSource,
        $"thirdPartySummaryFound={thirdPartyShimpsSummaryFound}; primarySource={thirdPartyShimpsSummaryIsPrimarySource}; transcript={thirdPartyShimpsSummaryIsTranscript}; wzRows={thirdPartyShimpsSummaryProvidesWzSourceRows}; higgsScalarSource={thirdPartyShimpsSummaryProvidesHiggsScalarSource}"),
    new Check(
        "transcript-audit-does-not-fill-wz-contract",
        !transcriptAuditProvidesGuLocalWzTheorem
            && !transcriptAuditProvidesSeparateWzSourceRows
            && !transcriptAuditProvidesLowEnergyWeakCouplingSource
            && !transcriptAuditProvidesTargetIndependentVevSource
            && !transcriptAuditProvidesPhotonWzEigenstateProjectionRows
            && !transcriptAuditPromotesWzMasses
            && !canFillPhase201WzContract,
        $"guLocalWzTheorem={transcriptAuditProvidesGuLocalWzTheorem}; separateWzRows={transcriptAuditProvidesSeparateWzSourceRows}; weakCoupling={transcriptAuditProvidesLowEnergyWeakCouplingSource}; targetIndependentVev={transcriptAuditProvidesTargetIndependentVevSource}; photonWzRows={transcriptAuditProvidesPhotonWzEigenstateProjectionRows}; promotesWzMasses={transcriptAuditPromotesWzMasses}; canFillWz={canFillPhase201WzContract}"),
    new Check(
        "transcript-audit-does-not-fill-higgs-contract",
        !transcriptAuditProvidesHiggsScalarSourceOperator
            && !transcriptAuditProvidesHiggsSelfCouplingSource
            && !transcriptAuditPromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"higgsScalarSource={transcriptAuditProvidesHiggsScalarSourceOperator}; higgsSelfCoupling={transcriptAuditProvidesHiggsSelfCouplingSource}; promotesHiggsMass={transcriptAuditPromotesHiggsMass}; canFillHiggs={canFillPhase201HiggsContract}"),
    new Check(
        "transcript-audit-does-not-fill-observed-field-extraction",
        !transcriptAuditProvidesObservedFieldExtraction
            && !canFillPhase256ObservedFieldExtractionContract
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"observedFieldExtraction={transcriptAuditProvidesObservedFieldExtraction}; canFillPhase256={canFillPhase256ObservedFieldExtractionContract}; phase256FilledRequiredFieldCount={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}"),
    new Check(
        "phase315-negative-source-audit-remains-binding",
        phase315SourceAuditPassed
            && !phase315PromotesWzMasses
            && !phase315PromotesHiggsMass
            && !phase315CompletesBosonPredictions
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase315SourceAuditPassed={phase315SourceAuditPassed}; phase315PromotesWzMasses={phase315PromotesWzMasses}; phase315PromotesHiggsMass={phase315PromotesHiggsMass}; phase315CompletesBosonPredictions={phase315CompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var ucsdTranscriptSourceStrengthAuditPassed = checks.All(check => check.Passed)
    && !transcriptAuditPromotesWzMasses
    && !transcriptAuditPromotesHiggsMass
    && !transcriptAuditCompletesBosonPredictions;
var terminalStatus = ucsdTranscriptSourceStrengthAuditPassed
    ? "ucsd-transcript-source-strength-audit-no-promotable-transcript-lineage"
    : "ucsd-transcript-source-strength-audit-review-required";

var result = new
{
    phaseId = "phase316-ucsd-transcript-source-strength-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    researchPerformedOn = "2026-05-20",
    ucsdTranscriptSourceStrengthAuditPassed,
    youtubeVideoId,
    vimeoThumbnailId,
    portalGroupPageAvailable,
    portalWikiMetadataAvailable,
    portalWikiEditedTranscriptAvailable,
    portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess,
    localYtDlpAvailable,
    localYoutubeDlAvailable,
    directTimedTextCaptionListEndpointChecked,
    directTimedTextCaptionListReturnedEmpty,
    directTimedTextCaptionListTrackCount,
    directTimedTextCaptionListUsableAsSourceLineage,
    publicSearchExactVideoTranscriptFound,
    publicSearchExactVideoCaptionsFound,
    thirdPartyShimpsSummaryFound,
    thirdPartyShimpsSummaryIsPrimarySource,
    thirdPartyShimpsSummaryIsTranscript,
    thirdPartyShimpsSummaryProvidesWzSourceRows,
    thirdPartyShimpsSummaryProvidesHiggsScalarSource,
    timedTextArtifactMaterializedInRepo,
    captionOrTranscriptUsableAsSourceLineage,
    transcriptAuditPromotesWzMasses,
    transcriptAuditPromotesHiggsMass,
    transcriptAuditCompletesBosonPredictions,
    transcriptSourceBoundary = new
    {
        transcriptAuditProvidesGuLocalWzTheorem,
        transcriptAuditProvidesSeparateWzSourceRows,
        transcriptAuditProvidesLowEnergyWeakCouplingSource,
        transcriptAuditProvidesTargetIndependentVevSource,
        transcriptAuditProvidesPhotonWzEigenstateProjectionRows,
        transcriptAuditProvidesObservedFieldExtraction,
        transcriptAuditProvidesHiggsScalarSourceOperator,
        transcriptAuditProvidesHiggsSelfCouplingSource,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    phase315Evidence = new
    {
        phase315SourceAuditPassed,
        phase315EditedTranscriptAvailable,
        phase315PromotesWzMasses,
        phase315PromotesHiggsMass,
        phase315CompletesBosonPredictions,
    },
    sourceStrengthRows,
    checks,
    decision = "Do not promote W/Z or Higgs predictions from the UCSD video transcript path. The public Portal materials identify a relevant GU lecture and abstract, but no public edited transcript, caption artifact, or primary theorem/source-lineage row is currently repo-materialized. Third-party summaries confirm the lead but cannot fill Phase201 or Phase256.",
    nextRequiredArtifact = new[]
    {
        "A public or repo-materialized transcript/caption excerpt with enough context to cite a GU-local W/Z theorem and separate W/Z source rows.",
        "A primary source theorem or derivation connecting the UCSD geometry to low-energy weak coupling, VEV scale, neutral projection, and observed field extraction.",
        "A primary Higgs scalar-source/operator/profile/self-coupling derivation independent of observed boson targets.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase315Path = Phase315Path,
        portalGroupUrl = "https://theportal.group/from-dark-to-geometric-energy-a-sector-of-geometric-unity/",
        portalWikiUrl = "https://theportal.wiki/wiki/From_Dark_to_Geometric_Energy_-_A_Sector_of_Geometric_Unity_%28YouTube_Content%29",
        shimpsBlogUrl = "https://blog.shimps.org/blogpost557-Geometric-Unity-III",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "ucsd_transcript_source_strength_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "ucsd_transcript_source_strength_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"ucsdTranscriptSourceStrengthAuditPassed={ucsdTranscriptSourceStrengthAuditPassed}");
Console.WriteLine($"youtubeVideoId={youtubeVideoId}");
Console.WriteLine($"portalWikiEditedTranscriptAvailable={portalWikiEditedTranscriptAvailable}");
Console.WriteLine($"captionOrTranscriptUsableAsSourceLineage={captionOrTranscriptUsableAsSourceLineage}");
Console.WriteLine($"transcriptAuditPromotesWzMasses={transcriptAuditPromotesWzMasses}");
Console.WriteLine($"transcriptAuditPromotesHiggsMass={transcriptAuditPromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

public sealed record Check(
    string CheckId,
    bool Passed,
    string Detail);

public sealed record SourceStrengthRow(
    string SourceId,
    string Title,
    string UrlOrQuery,
    bool IsPrimaryOrOfficialPortalSource,
    bool IsTranscriptOrCaptionArtifact,
    bool IsRepoMaterialized,
    bool IsPromotableSourceLineage,
    string Finding);
