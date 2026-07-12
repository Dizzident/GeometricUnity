using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase462: Blocking Set Resolution - Team A Wave-2 (WAVE2_ACTION_PLAN_2026-07-12
// item 2; §2(A)). ELIMINATION-support computation: deterministic, target-blind,
// fail-closed. This session implements STAGE P (quote-pinning), STAGE 0
// (closure-sensitivity certificates, exact SNF kernel-monotonicity) and STAGE 1
// (conjunction-gated machine excision). STAGE 2 (the physicist adjudication
// session) does NOT run here; the phase lands at its pre-registered terminal
// determined by the actual pinning coverage.
//
// The blocking set is the 31 corpus-dimensionally-ambiguous items decided by the
// committed phase460 units-equivariance kernel theorem. 1 item is mirror-resident
// (PA tier); 30 are audit-key items linked by sha12 unique-preimage to committed
// audit findings, whose gradable corpus text lives only in the official April-2021
// draft (PB tier). Audit-authored finding strings are labelled
// AUDIT-AUTHORED-NOT-CORPUS and are NEVER gradable. An automated extractor/NLP
// route is deliberately discarded: without a committed per-item verbatim
// excerpt + page locator, a PB item is UNPINNABLE (no manufactured pins).
//
// MANDATORY FRAMING. No GeV content; nothing is measured, filled, or promoted;
// promotedPhysicalMassClaimCount remains 0; physicistReviewPending is carried
// explicitly. Lattice-unit quantities stay in lattice units. Every terminal is an
// elimination-support record or an honestly-blocked record.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "blocking-set-resolution";
const string TerminalPrefix = "blocking-set-resolution-";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 2; §2(A)";
const string DefaultOutputDir = "studies/phase462_blocking_set_resolution_001/output";

const string MirrorPath = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md";
const string Phase460OutputPath = "studies/phase460_source_corpus_units_equivariance_kernel_001/output/source_corpus_units_equivariance_kernel.json";

// --- committed drift pins (fail-closed) ---
const string MirrorPinnedSha256 = "53eaf32825b53cc9c4e871e6ada6a3b046ee817dcc3623cf5c8a0636018fecdb";
const string DraftPdfUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string DraftPdfPinnedSha256 = "3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4";
const string DraftPdfProvenance = "docs/Reference/ExperimentReferences/GU-DRAFT-2021.md";

// --- pre-registered budgets (committed ex ante) ---
const int ExpectedSymbolCount = 33;
const int ExpectedBlockingSetCount = 31;
const int UnpinnableTripwire = 7;      // > 7/31 UNPINNABLE => pinning-insufficient
const int ExAnteExcisionYieldMax = 1;  // machine excision yield <= 1, likely 0
const int MinDecoyCount = 40;          // >= 40 paraphrase decoys per set
const int DecoySeed = 20260712;        // committed decoy generator seed

var outputDir = Environment.GetEnvironmentVariable("PHASE462_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// The sealed-redo escrow lives OUTSIDE the scanned repo roots (scratchpad).
var escrowDir = Environment.GetEnvironmentVariable("PHASE462_ESCROW_DIR")
    ?? "/tmp/claude-1000/-home-josh-Documents-GitHub-GeometricUnity/69018c76-9a8e-41d1-bf5c-9afa5af1644e/scratchpad/p462_sealed_decoys";

// --- standing claim boundary (verbatim across the program) ---
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;

var guards = new List<Guard>();
void FailClosed(string id, string detail) => guards.Add(new Guard(id, detail));

static string Sha256Hex(string s) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
static string Sha256HexBytes(byte[] b) => Convert.ToHexStringLower(SHA256.HashData(b));
static string Sha12(string s) => Sha256Hex(s)[..12];

// ---------------------------------------------------------------------------
// 0. Load the committed phase460 theorem output (the blocking set of record).
//    Input-hash guard: symbol table size, blocking-set size, and blocking-set
//    id list are pinned; any mismatch is fail-closed (input-hash-mismatch).
// ---------------------------------------------------------------------------

string[] symbols = [];
var blockingIds = new List<string>();
var inventory = new List<Phase460Item>();
var phase460Present = File.Exists(Phase460OutputPath);
string phase460Sha256 = "absent";

if (!phase460Present)
{
    FailClosed("input-hash-mismatch", $"phase460 output missing: {Phase460OutputPath}");
}
else
{
    var raw = File.ReadAllBytes(Phase460OutputPath);
    phase460Sha256 = Sha256HexBytes(raw);
    using var doc = JsonDocument.Parse(raw);
    var root = doc.RootElement;

    symbols = root.GetProperty("symbols").EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
    var symbolSet = symbols.ToHashSet(StringComparer.Ordinal);

    foreach (var e in root.GetProperty("corpusDimensionallyAmbiguousBlockingSet").EnumerateArray())
    {
        blockingIds.Add(e.GetString() ?? "");
    }

    foreach (var e in root.GetProperty("inventory").EnumerateArray())
    {
        var exps = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var p in e.GetProperty("exponents").EnumerateObject())
        {
            exps[p.Name] = p.Value.GetInt32();
        }

        inventory.Add(new Phase460Item(
            e.GetProperty("itemId").GetString() ?? "",
            e.GetProperty("corpusPath").GetString() ?? "",
            e.GetProperty("evidenceKind").GetString() ?? "",
            e.GetProperty("evidenceRef").GetString() ?? "",
            e.GetProperty("classification").GetString() ?? "",
            exps,
            e.GetProperty("note").GetString() ?? ""));
    }

    if (symbols.Length != ExpectedSymbolCount)
    {
        FailClosed("input-hash-mismatch", $"symbol count {symbols.Length} != {ExpectedSymbolCount}");
    }

    if (blockingIds.Count != ExpectedBlockingSetCount)
    {
        FailClosed("input-hash-mismatch", $"blocking-set count {blockingIds.Count} != {ExpectedBlockingSetCount}");
    }

    // symbol-outside-33-table: every pinned-row exponent references a table symbol.
    foreach (var it in inventory.Where(i => i.Classification == "pinned-row"))
    {
        foreach (var sym in it.Exponents.Keys)
        {
            if (!symbolSet.Contains(sym))
            {
                FailClosed("symbol-outside-33-table", $"{it.ItemId} references {sym} not in the {ExpectedSymbolCount}-symbol table");
            }
        }
    }
}

// Per-audit freshness pins (mirrors phase460's committed pins; independent
// re-verification keeps the sha12 preimage search honest).
var auditPins = new (int Phase, int TrueKeyCount, string TrueKeyListSha16, string Path)[]
{
    (331, 14, "e03c3708666a4088", AuditPath(331)),
    (332, 19, "6cec6183aa92b58f", AuditPath(332)),
    (333, 20, "b24aa34f7ee62a01", AuditPath(333)),
    (334, 23, "c8ec258efc04923d", AuditPath(334)),
    (335, 25, "2f555cbb567e47f5", AuditPath(335)),
    (336, 23, "b0455fabc446ae0d", AuditPath(336)),
    (338, 36, "814c97980797e2fd", AuditPath(338)),
    (339, 34, "509cecb5f87a5e70", AuditPath(339)),
    (340, 31, "9792f2013d868652", AuditPath(340)),
    (341, 31, "7f8818b0d5f74edf", AuditPath(341)),
    (342, 37, "9463bb28a99622b2", AuditPath(342)),
    (343, 29, "ae0536267d1cbea6", AuditPath(343)),
    (344, 22, "5e34dcb99e255d4f", AuditPath(344)),
    (345, 20, "c2316083c8484dfd", AuditPath(345)),
};

static string AuditPath(int phase) => phase switch
{
    331 => "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output/theta_omega_inhomogeneous_gauge_source_audit.json",
    332 => "studies/phase332_string_m_theory_compactification_source_audit_001/output/string_m_theory_compactification_source_audit.json",
    333 => "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit.json",
    334 => "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit.json",
    335 => "studies/phase335_graviweak_plebanski_source_audit_001/output/graviweak_plebanski_source_audit.json",
    336 => "studies/phase336_heft_scalar_geometry_source_law_audit_001/output/heft_scalar_geometry_source_law_audit.json",
    338 => "studies/phase338_metric_affine_torsion_source_audit_001/output/metric_affine_torsion_source_audit.json",
    339 => "studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output/macdowell_mansouri_cartan_breaking_source_audit.json",
    340 => "studies/phase340_bf_topological_mass_source_audit_001/output/bf_topological_mass_source_audit.json",
    341 => "studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/output/scherk_schwarz_twisted_compactification_source_audit.json",
    342 => "studies/phase342_higgsless_boundary_condition_source_audit_001/output/higgsless_boundary_condition_source_audit.json",
    343 => "studies/phase343_stueckelberg_vector_mass_source_audit_001/output/stueckelberg_vector_mass_source_audit.json",
    344 => "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit.json",
    345 => "studies/phase345_fradkin_shenker_complementarity_source_audit_001/output/fradkin_shenker_complementarity_source_audit.json",
    _ => throw new InvalidOperationException($"unregistered audit phase {phase}"),
};

// Build a per-audit true-key set (with freshness verification).
var auditTrueKeys = new Dictionary<int, string[]>();
foreach (var (phase, expectedCount, expectedSha, pathRaw) in auditPins)
{
    var path = pathRaw;
    if (!File.Exists(path))
    {
        FailClosed("input-hash-mismatch", $"audit p{phase} output missing: {path}");
        continue;
    }

    using var adoc = JsonDocument.Parse(File.ReadAllText(path));
    var trueKeys = adoc.RootElement.EnumerateObject()
        .Where(p => p.Value.ValueKind == JsonValueKind.True)
        .Select(p => p.Name)
        .OrderBy(n => n, StringComparer.Ordinal)
        .ToArray();
    auditTrueKeys[phase] = trueKeys;

    var listSha = Sha256Hex(string.Join("\n", trueKeys))[..16];
    if (trueKeys.Length != expectedCount || listSha != expectedSha)
    {
        FailClosed("input-hash-mismatch", $"audit p{phase} freshness pin mismatch (count={trueKeys.Length}/{expectedCount}, sha={listSha}/{expectedSha})");
    }
}

static int AuditPhaseOf(string itemId)
{
    // itemId like "p331-a2" -> 331; mirror items return 0.
    if (itemId.Length >= 4 && itemId[0] == 'p' && int.TryParse(itemId.AsSpan(1, 3), out var ph))
    {
        return ph;
    }

    return 0;
}

// ---------------------------------------------------------------------------
// 1. STAGE P - quote-pinning.
//    (a) 1 mirror-resident item (PA tier): whitespace-normalized exact match
//        against the committed mirror snapshot; commit normalized byte-offset +
//        sha256(normalized passage) + mirror sha256 drift gate.
//    (b) 30 audit-key items (PB tier): sha12 = sha256(auditKeyName)[:12] with
//        UNIQUE-preimage verification over the referenced audit's true-key set;
//        the preimage is an AUDIT-AUTHORED-NOT-CORPUS finding string (never
//        gradable). Attempt the official April-2021 draft PDF pin; the draft
//        sha256 drift gate is pinned. Without a committed per-item verbatim
//        excerpt + page locator (NLP auto-locate deliberately discarded), a PB
//        item is UNPINNABLE (no manufactured pins).
// ---------------------------------------------------------------------------

// Mirror drift gate.
var mirrorPresent = File.Exists(MirrorPath);
string mirrorSha256 = "absent";
string normalizedMirror = string.Empty;
if (mirrorPresent)
{
    var mirrorBytes = File.ReadAllBytes(MirrorPath);
    mirrorSha256 = Sha256HexBytes(mirrorBytes);
    normalizedMirror = System.Text.RegularExpressions.Regex.Replace(Encoding.UTF8.GetString(mirrorBytes), @"\s+", " ");
    if (mirrorSha256 != MirrorPinnedSha256)
    {
        FailClosed("mirror-or-pdf-drift", $"mirror sha256 {mirrorSha256} != pinned {MirrorPinnedSha256}");
    }
}
else
{
    FailClosed("mirror-or-pdf-drift", $"mirror missing: {MirrorPath}");
}

// Optional live PDF re-fetch (env-gated; the deterministic verdict does NOT
// depend on network). A fetched-but-mismatched PDF is a hard drift failure; a
// network failure is recorded as an honest UNPINNABLE reason, not a red gate.
var pdfRefetchRequested = Environment.GetEnvironmentVariable("PHASE462_PDF_REFETCH") == "1";
string pdfRefetchStatus = pdfRefetchRequested ? "attempted" : "not-attempted-reproducible-mode";
string? pdfLiveSha256 = null;
if (pdfRefetchRequested)
{
    try
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        var bytes = http.GetByteArrayAsync(DraftPdfUrl).GetAwaiter().GetResult();
        pdfLiveSha256 = Sha256HexBytes(bytes);
        if (pdfLiveSha256 != DraftPdfPinnedSha256)
        {
            FailClosed("mirror-or-pdf-drift", $"live draft PDF sha256 {pdfLiveSha256} != pinned {DraftPdfPinnedSha256}");
            pdfRefetchStatus = "fetched-drift";
        }
        else
        {
            pdfRefetchStatus = "fetched-sha256-verified";
        }
    }
    catch (Exception ex)
    {
        pdfRefetchStatus = "network-unavailable";
        Console.Error.WriteLine($"draft PDF refetch failed (recorded, not fatal): {ex.GetType().Name}");
    }
}

var pinRecords = new List<PinRecord>();
foreach (var itemId in blockingIds)
{
    var item = inventory.FirstOrDefault(i => i.ItemId == itemId);
    if (item is null)
    {
        FailClosed("input-hash-mismatch", $"blocking id {itemId} absent from phase460 inventory");
        continue;
    }

    if (item.EvidenceKind == "snapshot-quote")
    {
        // PA tier: mirror-resident, whitespace-normalized exact match.
        var passage = item.EvidenceRef;
        var offset = normalizedMirror.IndexOf(passage, StringComparison.Ordinal);
        if (mirrorPresent && offset >= 0)
        {
            pinRecords.Add(new PinRecord(
                itemId, "PA-mirror", "PINNED", "mirror-resident-verbatim-corpus-text",
                NormalizedByteOffset: offset,
                NormalizedPassageSha256: Sha256Hex(passage),
                PinnedCorpusText: passage,
                AuditKeyPreimage: null,
                DriftGateSha256: MirrorPinnedSha256));
        }
        else
        {
            // The one gradable corpus row could not be located: honest UNPINNABLE.
            pinRecords.Add(new PinRecord(
                itemId, "PA-mirror", "UNPINNABLE", "mirror-passage-not-located",
                null, null, null, null, MirrorPinnedSha256));
        }
    }
    else
    {
        // PB tier: sha12 unique-preimage linkage, then PDF corpus-locator attempt.
        var phase = AuditPhaseOf(itemId);
        var uniquePreimage = "";
        var linkageStatus = "sha12-preimage-missing";
        if (auditTrueKeys.TryGetValue(phase, out var keys))
        {
            var preimages = keys.Where(k => Sha12(k) == item.EvidenceRef).ToArray();
            if (preimages.Length == 1)
            {
                uniquePreimage = preimages[0];
                linkageStatus = "sha12-unique-preimage";
            }
            else if (preimages.Length == 0)
            {
                linkageStatus = "sha12-preimage-absent";
                FailClosed("input-hash-mismatch", $"{itemId}: sha12 {item.EvidenceRef} has no preimage in audit p{phase}");
            }
            else
            {
                linkageStatus = "sha12-preimage-collision";
                FailClosed("input-hash-mismatch", $"{itemId}: sha12 {item.EvidenceRef} collides ({preimages.Length}) in audit p{phase}");
            }
        }

        // The preimage is AUDIT-AUTHORED-NOT-CORPUS: never gradable corpus text.
        // A PB pin needs a committed verbatim draft excerpt + page locator; none
        // is committed and NLP auto-locate is discarded. UNPINNABLE (no fake pins).
        var reason = pdfRefetchStatus == "network-unavailable"
            ? "pb-corpus-locator-unavailable-network-unavailable"
            : "pb-corpus-locator-unavailable-no-committed-excerpt-nlp-route-discarded";
        pinRecords.Add(new PinRecord(
            itemId, "PB-draft-pdf", "UNPINNABLE", reason,
            NormalizedByteOffset: null,
            NormalizedPassageSha256: null,
            PinnedCorpusText: null,
            AuditKeyPreimage: linkageStatus == "sha12-unique-preimage" ? uniquePreimage : null,
            DriftGateSha256: DraftPdfPinnedSha256));
    }
}

var paPinned = pinRecords.Count(p => p.Tier == "PA-mirror" && p.Status == "PINNED");
var pbPinned = pinRecords.Count(p => p.Tier == "PB-draft-pdf" && p.Status == "PINNED");
var unpinnable = pinRecords.Count(p => p.Status == "UNPINNABLE");
var pinningInsufficient = unpinnable > UnpinnableTripwire;

// ---------------------------------------------------------------------------
// 2. Exact BigInteger SNF battery + kernel-monotonicity proof.
//    Reuses the phase460 SNF discipline. Kernel-monotonicity: adjudicating an
//    ambiguous item can only ADD a constraint row, and adding rows can only
//    SHRINK the integer kernel; therefore an unadjudicated ambiguous item can
//    never HELP the closure verdict - the blocking set genuinely blocks.
// ---------------------------------------------------------------------------

var snfBatteryOk = true;
var snfCases = new List<object>();
void SnfCase(string id, BigInteger[][] m, BigInteger[] expectedDiag)
{
    var snf = SmithNormalForm.Compute(m);
    var ok = snf.Diagonal.Length == expectedDiag.Length && snf.Diagonal.SequenceEqual(expectedDiag);
    foreach (var v in snf.KernelBasis)
    {
        for (var r = 0; r < m.Length; r++)
        {
            var acc = BigInteger.Zero;
            for (var c = 0; c < m[0].Length; c++)
            {
                acc += m[r][c] * v[c];
            }

            if (acc != 0)
            {
                ok = false;
            }
        }
    }

    if (!ok)
    {
        snfBatteryOk = false;
    }

    snfCases.Add(new { caseId = id, passed = ok, diagonal = snf.Diagonal.Select(d => d.ToString(CultureInfo.InvariantCulture)).ToArray() });
}

static BigInteger[][] Mat(params long[][] rows) => rows.Select(r => r.Select(v => new BigInteger(v)).ToArray()).ToArray();
SnfCase("classic-3x3", Mat([2, 4, 4], [-6, 6, 12], [10, 4, 16]), [new BigInteger(2), new BigInteger(2), new BigInteger(156)]);
SnfCase("identity-3x3", Mat([1, 0, 0], [0, 1, 0], [0, 0, 1]), [BigInteger.One, BigInteger.One, BigInteger.One]);
SnfCase("rank1-2x2", Mat([1, 2], [2, 4]), [BigInteger.One, BigInteger.Zero]);

if (!snfBatteryOk)
{
    FailClosed("snf-nonconvergent", "SNF battery diagonal/annihilation check failed");
}

// Build the committed squared-repair pinned-row matrix from the phase460
// inventory (the reading in which the one-parameter scale family CLOSES).
var symbolIndex = symbols.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i, StringComparer.Ordinal);
BigInteger[] GradingVector(int ccGrade)
{
    var baseGrade = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["metricG"] = 2, ["connOmega"] = -1, ["curvF"] = -2, ["curvR"] = -2,
        ["massGeneric"] = -1, ["massW"] = -1, ["massZ"] = -1, ["massH"] = -1,
        ["massDirac"] = -1, ["massMode"] = -1, ["massVectorGeom"] = -1, ["massVectorGauge"] = -1,
        ["massVectorExtra"] = -1, ["massComposite"] = -1, ["massElementary"] = -1, ["massPlanck"] = -1,
        ["massTopo"] = -1, ["massParamVector"] = -1, ["condensateScale"] = -1, ["vevScalar"] = -1,
        ["scalarField"] = -1, ["scalarCompensator"] = -1, ["potentialV"] = -4, ["ccTerm"] = ccGrade,
        ["newtonConst"] = 2, ["radiusCompact"] = 1, ["radiusWarp"] = 1, ["couplingA"] = 0,
        ["couplingB"] = 0, ["couplingQ"] = 0, ["quarticSelf"] = 0, ["twistParam"] = 0, ["wilsonPhase"] = 0,
    };
    var v = new BigInteger[symbols.Length];
    for (var i = 0; i < symbols.Length; i++)
    {
        v[i] = baseGrade[symbols[i]];
    }

    return v;
}

BigInteger[][] SquaredPinnedRows()
{
    var rows = new List<BigInteger[]>();
    foreach (var it in inventory.Where(i => i.Classification == "pinned-row"))
    {
        if (it.ItemId == "sp12c-lit")
        {
            continue; // literal variant excluded from the squared-repair reading
        }

        var row = new BigInteger[symbols.Length];
        foreach (var (sym, exp) in it.Exponents)
        {
            row[symbolIndex[sym]] = exp;
        }

        rows.Add(row);
    }

    return rows.ToArray();
}

var squaredRows = SquaredPinnedRows();
var squaredSnf = squaredRows.Length > 0 ? SmithNormalForm.Compute(squaredRows) : new SnfResult([], []);
var squaredNullity = squaredSnf.KernelBasis.Length;
var scaleVec = GradingVector(-2); // prescribed cc grade; the squared-repair reading (phase460 R1) closes here

// Verify closure: every squared pinned row annihilates the scale vector.
var squaredCloses = squaredRows.All(row =>
{
    var acc = BigInteger.Zero;
    for (var c = 0; c < symbols.Length; c++)
    {
        acc += row[c] * scaleVec[c];
    }

    return acc == 0;
});

// Kernel-monotonicity + synthetic-overturn battery: add a synthetic RELATION
// row that is inconsistent with the scale grades; it must (i) flip the closure
// verdict to breaking and (ii) shrink the integer kernel by exactly one.
var syntheticRow = new BigInteger[symbols.Length];
syntheticRow[symbolIndex["massW"]] = 1; // single mass symbol => residual = grade(massW) = -1 != 0
var augmentedRows = squaredRows.Append(syntheticRow).ToArray();
var augmentedSnf = SmithNormalForm.Compute(augmentedRows);
var augmentedNullity = augmentedSnf.KernelBasis.Length;
var syntheticResidual = BigInteger.Zero;
for (var c = 0; c < symbols.Length; c++)
{
    syntheticResidual += syntheticRow[c] * scaleVec[c];
}

var kernelMonotone = augmentedNullity <= squaredNullity;
var syntheticOverturnFlips = squaredCloses && syntheticResidual != 0 && augmentedNullity == squaredNullity - 1;
if (!kernelMonotone)
{
    FailClosed("snf-nonconvergent", $"kernel non-monotone under row addition (before={squaredNullity}, after={augmentedNullity})");
}

var monotonicityBatteryOk = snfBatteryOk && squaredCloses && kernelMonotone && syntheticOverturnFlips;

// ---------------------------------------------------------------------------
// 3. STAGE 0 - closure-sensitivity certificates.
//    Default all 31 CLOSURE-SENSITIVE. CLOSURE-IRRELEVANT is granted ONLY by the
//    exact certificate: the item has a committed non-empty symbol support AND
//    every symbol in that support has mass-dimension 0 under EVERY grading in the
//    committed reading menu. A prose-only item (empty committed support) can
//    never earn the certificate. Injection battery (consistency-check): two
//    synthetic items exercise both certificate outcomes.
// ---------------------------------------------------------------------------

int[] readingMenuCcGrades = [-2, -1, -4];
bool ClosureIrrelevantCertificate(Dictionary<string, int> support, out string detail)
{
    if (support.Count == 0)
    {
        detail = "empty-committed-support-no-certificate";
        return false;
    }

    foreach (var cc in readingMenuCcGrades)
    {
        var g = GradingVector(cc);
        foreach (var sym in support.Keys)
        {
            if (g[symbolIndex[sym]] != 0)
            {
                detail = $"symbol {sym} has nonzero mass-dimension under cc={cc}";
                return false;
            }
        }
    }

    detail = "all-support-symbols-zero-mass-dimension-under-every-reading";
    return true;
}

var stage0 = new List<object>();
var closureSensitiveCount = 0;
foreach (var itemId in blockingIds)
{
    var item = inventory.First(i => i.ItemId == itemId);
    var irrelevant = ClosureIrrelevantCertificate(item.Exponents, out var detail);
    if (!irrelevant)
    {
        closureSensitiveCount++;
    }

    stage0.Add(new
    {
        itemId,
        committedSupportSize = item.Exponents.Count,
        classification = irrelevant ? "CLOSURE-IRRELEVANT" : "CLOSURE-SENSITIVE",
        certificateDetail = detail,
    });
}

// Injection battery (consistency-check): a grade-0-only support certifies
// irrelevant; a mass-bearing support stays sensitive.
var injCoupling = ClosureIrrelevantCertificate(new() { ["couplingA"] = 1, ["couplingB"] = -1 }, out _);
var injVev = ClosureIrrelevantCertificate(new() { ["vevScalar"] = 1 }, out _);
var injectionBatteryOk = injCoupling && !injVev;

// ---------------------------------------------------------------------------
// 4. STAGE 1 - conjunction-gated machine excision.
//    excision(item) = CLOSURE-IRRELEVANT(Stage 0)
//                     AND syntactic-non-relation (no committed symbolic relation)
//                     AND pinned-corpus-text (gradable corpus text pinned).
//    Committed ex-ante yield <= 1 (likely 0). Assert the actual yield.
// ---------------------------------------------------------------------------

var excised = new List<string>();
var stage1 = new List<object>();
foreach (var itemId in blockingIds)
{
    var item = inventory.First(i => i.ItemId == itemId);
    var closureIrrelevant = ClosureIrrelevantCertificate(item.Exponents, out _);
    var syntacticNonRelation = item.Exponents.Count == 0; // prose-only, no committed relation
    var pinnedCorpusText = pinRecords.Any(p => p.ItemId == itemId && p.Status == "PINNED");
    var excise = closureIrrelevant && syntacticNonRelation && pinnedCorpusText;
    if (excise)
    {
        excised.Add(itemId);
    }

    stage1.Add(new { itemId, closureIrrelevant, syntacticNonRelation, pinnedCorpusText, excise });
}

var excisionYield = excised.Count;
var excisionWithinExAnteBound = excisionYield <= ExAnteExcisionYieldMax;
var pendingAfterStage1 = blockingIds.Count - excisionYield;

// ---------------------------------------------------------------------------
// 5. The >= 40 paraphrase-decoy generator + sealed-redo escrow.
//    Deterministic template engine over the committed pinned-row relation notes.
//    Emits decoy sha256 lists in the output (plaintext only to the output JSON /
//    the escrow). decoy-battery-failed if < 40 unique or a decoy collides with a
//    real pinned statement.
// ---------------------------------------------------------------------------

var pinnedRowNotes = inventory
    .Where(i => i.Classification == "pinned-row")
    .Select(i => i.Note)
    .Distinct(StringComparer.Ordinal)
    .OrderBy(n => n, StringComparer.Ordinal)
    .ToArray();

List<string> GenerateDecoys(int seed)
{
    // Deterministic linear-congruential paraphrase driver (no System.Random).
    var lead = new[]
    {
        "Restated paraphrase:", "Decoy rendering:", "Alternate wording:", "Paraphrased claim:",
        "Reworded statement:", "Surrogate phrasing:", "Substitute reading:", "Variant text:",
    };
    var tail = new[]
    {
        "(paraphrase; not corpus)", "(decoy; ungradable)", "(surrogate)", "(reworded)",
        "(non-corpus paraphrase)", "(alternate)",
    };
    var decoys = new List<string>();
    ulong state = (ulong)seed * 6364136223846793005UL + 1442695040888963407UL;
    ulong Next()
    {
        state = state * 6364136223846793005UL + 1442695040888963407UL;
        return state >> 33;
    }

    var idx = 0;
    while (decoys.Count < MinDecoyCount + 8 && idx < pinnedRowNotes.Length * 4)
    {
        var note = pinnedRowNotes[(int)(Next() % (ulong)pinnedRowNotes.Length)];
        var l = lead[(int)(Next() % (ulong)lead.Length)];
        var t = tail[(int)(Next() % (ulong)tail.Length)];
        var salt = Next() % 9973;
        var decoy = $"{l} {note} {t} #{salt}";
        if (!decoys.Contains(decoy))
        {
            decoys.Add(decoy);
        }

        idx++;
    }

    return decoys;
}

var decoySetA = GenerateDecoys(DecoySeed);
var decoySetB = GenerateDecoys(DecoySeed + 104729); // sealed-redo escrow set
var realStatementSet = pinnedRowNotes.ToHashSet(StringComparer.Ordinal);
var decoyUnique = decoySetA.Distinct(StringComparer.Ordinal).Count() == decoySetA.Count;
var decoyNoCollision = decoySetA.All(d => !realStatementSet.Contains(d));
var decoyBatteryOk = decoySetA.Count >= MinDecoyCount && decoyUnique && decoyNoCollision
    && decoySetB.Count >= MinDecoyCount;
if (!decoyBatteryOk)
{
    FailClosed("decoy-battery-failed", $"setA={decoySetA.Count} unique={decoyUnique} noCollision={decoyNoCollision} setB={decoySetB.Count}");
}

// Escrow the sealed-redo plaintext OUTSIDE the scanned roots; commit only sha256.
var decoySetASha256 = decoySetA.Select(Sha256Hex).ToArray();
var decoySetBSha256 = decoySetB.Select(Sha256Hex).ToArray();
string escrowSealSha256 = "not-written";
try
{
    Directory.CreateDirectory(escrowDir);
    var escrowPayload = string.Join("\n", decoySetB);
    File.WriteAllText(Path.Combine(escrowDir, "p462_sealed_decoys_setB.txt"), escrowPayload);
    escrowSealSha256 = Sha256Hex(escrowPayload);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"escrow write failed (recorded): {ex.GetType().Name}");
}

// ---------------------------------------------------------------------------
// 6. Terminal decision (pre-registered taxonomy).
//    Fail-closed guards (input-hash-mismatch / symbol-outside-33-table /
//    snf-nonconvergent / mirror-or-pdf-drift / decoy-battery-failed) exit red.
//    Otherwise the pinning coverage decides: > 7/31 UNPINNABLE =>
//    pinning-insufficient; else awaiting-adjudication(k) at k = post-Stage-1
//    pending count (Stage 2 does not run in this session).
// ---------------------------------------------------------------------------

var machineIntegrityOk = guards.Count == 0 && monotonicityBatteryOk && injectionBatteryOk && decoyBatteryOk;

string verdictKind;
string decision;
if (!machineIntegrityOk)
{
    verdictKind = guards.Count > 0 ? guards[0].Id : "review-required-battery-failed";
    decision = $"Review required: fail-closed guard(s) tripped [{string.Join("; ", guards.Select(g => g.Id + ":" + g.Detail))}]. No resolution verdict is emitted.";
}
else if (pinningInsufficient)
{
    verdictKind = "pinning-insufficient";
    decision = $"Blocking-set resolution is machine-dead at the automated route: {unpinnable} of {blockingIds.Count} prose-only statements are UNPINNABLE (> {UnpinnableTripwire} tripwire). One mirror-resident statement pins at PA tier; the {pbUnpinnableSummary(pinRecords)} audit-key statements link by sha12 unique-preimage to AUDIT-AUTHORED-NOT-CORPUS findings but have no committed verbatim draft excerpt + page locator (the NLP auto-locate route is deliberately discarded), so they cannot be pinned to gradable corpus text without human Stage-2 work. The official April-2021 draft drift gate is pinned (sha256 {DraftPdfPinnedSha256}). Stage 0 certifies all {closureSensitiveCount} items CLOSURE-SENSITIVE (kernel-monotonicity proven exactly: adjudication can only add constraint rows and shrink the integer kernel); Stage 1 machine excision yields {excisionYield} (ex-ante bound {ExAnteExcisionYieldMax}). ELIMINATION-support only: this honestly proves the machine resolution route dead and pins phase464 to BLOCKED-UPSTREAM-AMBIGUOUS; nothing is promoted.";
}
else
{
    verdictKind = "awaiting-adjudication";
    decision = $"Stage P/0/1 complete; {pendingAfterStage1} of {blockingIds.Count} statements remain pending the Stage-2 physicist adjudication (not run in this session). PA-pinned={paPinned}, PB-pinned={pbPinned}, UNPINNABLE={unpinnable} (within the {UnpinnableTripwire}-item budget). Stage 0: {closureSensitiveCount} CLOSURE-SENSITIVE; Stage 1 machine excision yield {excisionYield} (ex-ante bound {ExAnteExcisionYieldMax}). Nothing is promoted; physicistReviewPending is carried explicitly.";
}

static string pbUnpinnableSummary(List<PinRecord> recs) =>
    recs.Count(p => p.Tier == "PB-draft-pdf" && p.Status == "UNPINNABLE").ToString(CultureInfo.InvariantCulture);

var terminalStatus = TerminalPrefix + verdictKind;
var blockingSetResolutionPassed = machineIntegrityOk; // internal-consistency pass

string targetBlindConstructionHash = Sha256Hex(string.Join("|",
    ApplicationSubjectKind, verdictKind, PlanSection, phase460Sha256,
    $"pa={paPinned};pb={pbPinned};unpinnable={unpinnable};excised={excisionYield};sensitive={closureSensitiveCount}"));

// ---------------------------------------------------------------------------
// 7. Output.
// ---------------------------------------------------------------------------

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var result = new
{
    phaseId = "phase462-blocking-set-resolution",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    blockingSetResolutionPassed,
    machineIntegrityOk,
    verdictTaxonomy = new[]
    {
        "awaiting-adjudication(k)", "pinning-insufficient", "closure-resolved(-with-excision)(k)",
        "corpus-supplies-scale-breaking-relation(namedItem)", "still-blocked(k)",
        "input-hash-mismatch", "symbol-outside-33-table", "snf-nonconvergent",
        "mirror-or-pdf-drift", "decoy-battery-failed(sessionIndex)",
    },
    stagesImplementedThisSession = new[] { "P", "0", "1" },
    stage2Runs = false,
    blockingSetCount = blockingIds.Count,
    tripwireDenominator = ExpectedBlockingSetCount,
    symbolCount = symbols.Length,
    phase460InputSha256 = phase460Sha256,
    stageP = new
    {
        paPinnedCount = paPinned,
        pbPinnedCount = pbPinned,
        unpinnableCount = unpinnable,
        unpinnableTripwire = UnpinnableTripwire,
        pinningInsufficient,
        mirrorPinnedSha256 = MirrorPinnedSha256,
        mirrorSha256,
        draftPdfUrl = DraftPdfUrl,
        draftPdfPinnedSha256 = DraftPdfPinnedSha256,
        draftPdfProvenance = DraftPdfProvenance,
        pdfRefetchStatus,
        pdfLiveSha256,
        pins = pinRecords.Select(p => new
        {
            p.ItemId,
            p.Tier,
            p.Status,
            p.Reason,
            p.NormalizedByteOffset,
            p.NormalizedPassageSha256,
            auditKeyPreimageLabel = p.AuditKeyPreimage is null ? null : "AUDIT-AUTHORED-NOT-CORPUS",
            auditKeyPreimage = p.AuditKeyPreimage,
            pinnedCorpusText = p.PinnedCorpusText, // corpus text lives ONLY here
            p.DriftGateSha256,
        }).ToArray(),
    },
    stage0 = new
    {
        readingMenuCcGrades,
        closureSensitiveCount,
        closureIrrelevantCount = blockingIds.Count - closureSensitiveCount,
        kernelMonotonicity = new
        {
            squaredReadingCloses = squaredCloses,
            squaredNullity,
            augmentedNullity,
            syntheticResidual = syntheticResidual.ToString(CultureInfo.InvariantCulture),
            kernelMonotone,
            syntheticOverturnFlips,
            note = "rulings only ADD rows => integer kernel can only shrink; an unadjudicated ambiguous item can never help closure",
        },
        injectionBattery = new { grade0OnlyCertifiesIrrelevant = injCoupling, massBearingStaysSensitive = !injVev, injectionBatteryOk },
        certificates = stage0,
    },
    stage1 = new
    {
        excisionYield,
        exAnteExcisionYieldMax = ExAnteExcisionYieldMax,
        excisionWithinExAnteBound,
        excisedIds = excised.ToArray(),
        pendingAfterStage1,
        conjunction = "CLOSURE-IRRELEVANT AND syntactic-non-relation AND pinned-corpus-text",
        rows = stage1,
    },
    decoyBattery = new
    {
        minDecoyCount = MinDecoyCount,
        decoySeed = DecoySeed,
        decoySetACount = decoySetA.Count,
        decoySetBCount = decoySetB.Count,
        decoyUnique,
        decoyNoCollision,
        decoyBatteryOk,
        decoySetASha256,
        decoySetBSha256,
        decoySetAPlaintext = decoySetA, // plaintext only in the output JSON
        sealedRedoEscrowDir = escrowDir,
        sealedRedoEscrowSealSha256 = escrowSealSha256,
        secondSignerRequiredBeforeStage2 = true,
    },
    snfBattery = new { passed = snfBatteryOk, cases = snfCases },
    guards = guards.Select(g => new { id = g.Id, detail = g.Detail }).ToArray(),
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256Contract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    phase464Impact = new
    {
        deliversConjunctC1 = machineIntegrityOk,
        pinsPhase464ToBlockedUpstreamAmbiguous = pinningInsufficient,
    },
    recordedBoundary = new { physicistReviewPending, secondSignerRequiredBeforeStage2 = true },
    explicitCandidateOnlyNonclaims = new[]
    {
        "Stage P/0/1 is elimination-support machinery over a prose-only blocking set; no measurement, contract fill, or promotion is claimed.",
        "Audit-key preimages are AUDIT-AUTHORED-NOT-CORPUS and are never gradable; pinned corpus text is committed ONLY for mirror-resident PA rows.",
        "pinning-insufficient honestly proves the machine resolution route dead; it forbids any partial-closure citation and pins phase464 to BLOCKED-UPSTREAM-AMBIGUOUS.",
        "physicistReviewPending = true; promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
    runtimeSeconds,
};

File.WriteAllText(Path.Combine(outputDir, "blocking_set_resolution.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(Path.Combine(outputDir, "blocking_set_resolution_summary.json"), JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.verdictKind,
    result.blockingSetResolutionPassed,
    result.blockingSetCount,
    stagePCoverage = new { paPinned, pbPinned, unpinnable, pinningInsufficient },
    stage0 = new { closureSensitiveCount },
    stage1 = new { excisionYield, pendingAfterStage1 },
    decoy = new { decoySetA.Count, decoyBatteryOk },
    result.physicistReviewPending,
    result.noGevPromotion,
    result.decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} pass={blockingSetResolutionPassed}");
Console.WriteLine($"stageP: PA={paPinned} PB={pbPinned} UNPINNABLE={unpinnable} pinningInsufficient={pinningInsufficient}");
Console.WriteLine($"stage0: CLOSURE-SENSITIVE={closureSensitiveCount} monotone={kernelMonotone} overturnFlips={syntheticOverturnFlips}");
Console.WriteLine($"stage1: excisionYield={excisionYield} (<= {ExAnteExcisionYieldMax}); pendingAfterStage1={pendingAfterStage1}");
Console.WriteLine($"decoys: setA={decoySetA.Count} setB={decoySetB.Count} ok={decoyBatteryOk}");
foreach (var g in guards)
{
    Console.WriteLine($"GUARD {g.Id}: {g.Detail}");
}

Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

if (!machineIntegrityOk)
{
    Environment.Exit(1);
}

internal sealed record Guard(string Id, string Detail);

internal sealed record Phase460Item(
    string ItemId,
    string CorpusPath,
    string EvidenceKind,
    string EvidenceRef,
    string Classification,
    Dictionary<string, int> Exponents,
    string Note);

internal sealed record PinRecord(
    string ItemId,
    string Tier,
    string Status,
    string Reason,
    int? NormalizedByteOffset,
    string? NormalizedPassageSha256,
    string? PinnedCorpusText,
    string? AuditKeyPreimage,
    string DriftGateSha256);

internal sealed record SnfResult(BigInteger[] Diagonal, BigInteger[][] KernelBasis);

/// <summary>
/// Exact integer Smith normal form over BigInteger with right-transform
/// tracking (kernel = trailing columns of the unimodular right transform).
/// Ported verbatim in discipline from phase460.
/// </summary>
internal static class SmithNormalForm
{
    public static SnfResult Compute(BigInteger[][] input)
    {
        var rows = input.Length;
        var cols = rows == 0 ? 0 : input[0].Length;
        var a = input.Select(r => (BigInteger[])r.Clone()).ToArray();

        var v = new BigInteger[cols][];
        for (var i = 0; i < cols; i++)
        {
            v[i] = new BigInteger[cols];
            v[i][i] = BigInteger.One;
        }

        var t = 0;
        var limit = Math.Min(rows, cols);
        while (t < limit)
        {
            var pr = -1;
            var pc = -1;
            BigInteger best = 0;
            for (var r = t; r < rows; r++)
            {
                for (var c = t; c < cols; c++)
                {
                    var x = BigInteger.Abs(a[r][c]);
                    if (x != 0 && (pr < 0 || x < best))
                    {
                        best = x;
                        pr = r;
                        pc = c;
                    }
                }
            }

            if (pr < 0)
            {
                break;
            }

            SwapRows(a, t, pr);
            SwapCols(a, v, t, pc);

            var clean = true;
            for (var r = t + 1; r < rows; r++)
            {
                if (a[r][t] == 0)
                {
                    continue;
                }

                var q = BigInteger.Divide(a[r][t], a[t][t]);
                if (q != 0)
                {
                    for (var c = t; c < cols; c++)
                    {
                        a[r][c] -= q * a[t][c];
                    }
                }

                if (a[r][t] != 0)
                {
                    SwapRows(a, t, r);
                    clean = false;
                }
            }

            for (var c = t + 1; c < cols; c++)
            {
                if (a[t][c] == 0)
                {
                    continue;
                }

                var q = BigInteger.Divide(a[t][c], a[t][t]);
                if (q != 0)
                {
                    for (var r = 0; r < rows; r++)
                    {
                        a[r][c] -= q * a[r][t];
                    }

                    for (var k = 0; k < cols; k++)
                    {
                        v[k][c] -= q * v[k][t];
                    }
                }

                if (a[t][c] != 0)
                {
                    SwapCols(a, v, t, c);
                    clean = false;
                }
            }

            if (!clean)
            {
                continue;
            }

            var repaired = false;
            for (var r = t + 1; r < rows && !repaired; r++)
            {
                for (var c = t + 1; c < cols && !repaired; c++)
                {
                    if (a[r][c] % a[t][t] != 0)
                    {
                        for (var rr = 0; rr < rows; rr++)
                        {
                            a[rr][t] += a[rr][c];
                        }

                        for (var k = 0; k < cols; k++)
                        {
                            v[k][t] += v[k][c];
                        }

                        repaired = true;
                    }
                }
            }

            if (repaired)
            {
                continue;
            }

            if (a[t][t] < 0)
            {
                for (var c = t; c < cols; c++)
                {
                    a[t][c] = -a[t][c];
                }
            }

            t++;
        }

        var diag = new BigInteger[limit];
        for (var i = 0; i < limit; i++)
        {
            diag[i] = a[i][i] < 0 ? -a[i][i] : a[i][i];
        }

        var rank = diag.Count(d => d != 0);
        var kernel = new BigInteger[cols - rank][];
        for (var j = rank; j < cols; j++)
        {
            var col = new BigInteger[cols];
            for (var k = 0; k < cols; k++)
            {
                col[k] = v[k][j];
            }

            kernel[j - rank] = col;
        }

        return new SnfResult(diag, kernel);
    }

    private static void SwapRows(BigInteger[][] a, int i, int j)
    {
        if (i == j)
        {
            return;
        }

        (a[i], a[j]) = (a[j], a[i]);
    }

    private static void SwapCols(BigInteger[][] a, BigInteger[][] v, int i, int j)
    {
        if (i == j)
        {
            return;
        }

        foreach (var row in a)
        {
            (row[i], row[j]) = (row[j], row[i]);
        }

        foreach (var row in v)
        {
            (row[i], row[j]) = (row[j], row[i]);
        }
    }
}
