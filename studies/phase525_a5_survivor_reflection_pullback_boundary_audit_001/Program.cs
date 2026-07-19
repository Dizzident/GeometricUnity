using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001";
const string ContractPath = PhaseDir + "/preregistration/phase525_survivor_reflection_pullback_boundary_contract_v1.json";
const string OutputDir = PhaseDir + "/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
Binding[] bindings = contract.GetProperty("exactInputBindings").EnumerateArray()
    .Select(row => Bind(S(row, "id"), S(row, "path"), S(row, "sha256"))).ToArray();
int[] extents = Ints(contract, "extentMenu");
PairSpec[] pairs = contract.GetProperty("survivorPairMenu").EnumerateArray()
    .Select(row => new PairSpec(S(row, "candidateId"), S(row, "reflectionId"), I(row, "axis"), I(row, "offset"),
        S(row, "inactiveAxis0BaseMap"), S(row, "activeAxis0BaseMap"))).ToArray();
string[] precedence = Strings(contract, "precedence");

bool contractValid = I(contract, "schemaVersion") == 1
    && S(contract, "contractId") == "phase525-a19-survivor-reflection-pullback-boundary-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A19"
    && S(contract, "contractStatus") == "frozen-final-before-survivor-scoring"
    && B(contract, "menuFrozenBeforeScoring")
    && ExactBindingInventoryValid(bindings)
    && S(contract, "geometryId") == "periodic-cubical-cell-poset-barycentric"
    && extents.SequenceEqual(new[] { 3, 4 })
    && pairs.Length == 2
    && pairs[0].EqualsValues("periodic-cubical-cell-poset-barycentric::site-centered-axis0",
        "site-centered-axis0", 0, 0, "b0 -> -b0 mod n", "b0 -> -b0-1 mod n")
    && pairs[1].EqualsValues("periodic-cubical-cell-poset-barycentric::link-centered-axis0",
        "link-centered-axis0", 0, 1, "b0 -> 1-b0 mod n", "b0 -> -b0 mod n")
    && S(contract, "cellRepresentation") == "canonical-base-coordinate-in-Zn4-plus-active-axis-mask-0-through-15"
    && S(contract, "orientedSimplexRule") == "strictly-increasing-cell-rank-order"
    && S(contract, "boundaryRule") == "d[v0,...,vk]=sum_i(-1)^i[v0,...,omit(vi),...,vk]"
    && S(contract, "cubicalCarrierOrientationRule") == "reflection-sign-minus-iff-axis0-is-active"
    && S(contract, "doubledSliceRule") == "tau2=(2*base0+axis0-active-bit)-offset mod 2n"
    && precedence.SequenceEqual(new[]
    {
        "invalid-or-drifted-input",
        "no-survivor-complete-finite-pullback-boundary",
        "partial-survivor-finite-pullback-boundary",
        "finite-dual-survivor-pullback-boundary-closed",
    }, StringComparer.Ordinal)
    && S(contract, "expectedCurrentVerdict") == "finite-dual-survivor-pullback-boundary-closed"
    && !B(contract, "candidateRegistrationAllowed") && !B(contract, "productionGeometrySelectionAllowed")
    && !B(contract, "allVolumeEmbeddingOrGluingClaimAllowed") && !B(contract, "positiveTimeAlgebraClaimAllowed")
    && !B(contract, "reflectionPositivityClaimAllowed") && !B(contract, "samplingOrProductionAllowed")
    && !B(contract, "phase515Or516CreationOrUnlockAllowed") && B(contract, "externalReviewPending")
    && !B(contract, "physicalClaimAllowed")
    && S(contract, "positiveResultScope") == "finite-combinatorial-chain-and-carrier-closure-only"
    && !B(contract, "actionCovarianceEvaluated") && !B(contract, "actionCarrierTransformationClaimAllowed")
    && AuthorityFirewallsValid(contract.GetProperty("authorityFirewalls"))
    && I(contract, "promotedPhysicalMassClaimCount") == 0;

JsonElement phase521 = ReadRoot(bindings.Single(x => x.Id == "phase521-summary").Path);
JsonElement phase522 = ReadRoot(bindings.Single(x => x.Id == "phase522-summary").Path);
string[] expectedPairIds = pairs.Select(x => x.CandidateId).Order(StringComparer.Ordinal).ToArray();
string[] phase522PairIds = phase522.GetProperty("candidateReduction").GetProperty("totalCompatibleCandidates")
    .EnumerateArray().Select(x => x.GetString() ?? "").Order(StringComparer.Ordinal).ToArray();
JsonElement[] phase521Rows = phase521.GetProperty("auditAuthoredCandidates").EnumerateArray().Select(x => x.Clone()).ToArray();
bool phase521RowsCoverFrozenMenu = phase521Rows.Length == extents.Length * pairs.Length
    && phase521Rows.All(row => extents.Contains(I(row, "extent"))
        && pairs.Any(pair => pair.ReflectionId == S(row, "reflectionId"))
        && S(row, "geometryId") == S(contract, "geometryId")
        && B(row, "vertexBijection") && B(row, "vertexInvolution") && B(row, "incidenceValid")
        && B(row, "reflectedIncidencePreserved") && B(row, "totalSimplexClosure"))
    && phase521Rows.Select(row => $"{I(row, "extent")}::{S(row, "reflectionId")}")
        .Distinct(StringComparer.Ordinal).Count() == extents.Length * pairs.Length;
bool precursorSemanticsValid = S(phase521, "verdictKind") == "finite-dual-reflection-compatible-candidate-survives"
    && B(phase521, "inputsValid") && B(phase521, "contractValid") && B(phase521, "finiteCandidateSurvives")
    && B(phase521, "candidatesRemainUnregistered") && !B(phase521, "boundaryOrPullbackRulesRegistered")
    && !B(phase521, "finiteSurvivalSupportsAllVolumeInference")
    && Strings(phase521, "survivingGeometryIds").SequenceEqual(new[] { S(contract, "geometryId") }, StringComparer.Ordinal)
    && Strings(phase521, "survivingReflectionIds").Order(StringComparer.Ordinal)
        .SequenceEqual(pairs.Select(x => x.ReflectionId).Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && phase521RowsCoverFrozenMenu
    && S(phase522, "verdictKind") == "action-member-or-omega-parity-unresolved"
    && B(phase522, "inputsValid") && B(phase522, "contractValid") && phase522PairIds.SequenceEqual(expectedPairIds, StringComparer.Ordinal)
    && I(phase522.GetProperty("candidateReduction"), "survivorCount") == 2
    && !B(phase522.GetProperty("candidateReduction"), "candidateSelected")
    && !B(phase522.GetProperty("candidateReduction"), "candidateRegistered")
    && !B(phase522, "reviewReady") && B(phase522, "externalReviewPending")
    && I(phase521, "promotedPhysicalMassClaimCount") == 0 && I(phase522, "promotedPhysicalMassClaimCount") == 0;

var complexes = extents.Select(BuildOrderComplex).ToDictionary(x => x.Extent);
var pairAudits = pairs.Select(pair => new PairAudit(pair.CandidateId, pair.ReflectionId,
    extents.Select(extent => Audit(complexes[extent], pair)).ToArray())).ToArray();
int completePairCount = pairAudits.Count(x => x.CompleteFinitePullbackBoundary);
bool invalidOrDrifted = !contractValid || bindings.Any(x => !x.HashMatches) || !precursorSemanticsValid;
string verdict = EvaluatePrecedence(invalidOrDrifted, completePairCount, pairAudits.Length, precedence);
var precedenceBattery = new[]
{
    PrecedenceCase("invalid", true, 2, 2, precedence, "invalid-or-drifted-input"),
    PrecedenceCase("none-complete", false, 0, 2, precedence, "no-survivor-complete-finite-pullback-boundary"),
    PrecedenceCase("partial-complete", false, 1, 2, precedence, "partial-survivor-finite-pullback-boundary"),
    PrecedenceCase("all-complete", false, 2, 2, precedence, "finite-dual-survivor-pullback-boundary-closed"),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

var result = new
{
    schemaVersion = 1,
    phase = 525,
    phaseId = "phase525-a5-survivor-reflection-pullback-boundary-audit",
    terminalStatus = "a5-survivor-reflection-pullback-boundary-audit-" + verdict,
    verdictKind = verdict,
    inputsValid = !invalidOrDrifted,
    contractValid,
    precursorSemanticsValid,
    exactBindingInventoryValid = ExactBindingInventoryValid(bindings),
    precedenceBatteryPassed,
    precedenceBattery,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A19",
    contract = new { path = ContractPath, sha256 = Sha(ContractPath), menuFrozenBeforeScoring = true },
    exactInputBindings = bindings,
    frozenMenu = new
    {
        geometryId = S(contract, "geometryId"),
        extentMenu = extents,
        survivorPairIds = pairs.Select(x => x.CandidateId).ToArray(),
        menuFrozenBeforeScoring = true,
        targetBlindConstruction = true,
        resultBasedTuningPerformed = false,
    },
    rules = new
    {
        cellRepresentation = S(contract, "cellRepresentation"),
        orientedSimplexRule = S(contract, "orientedSimplexRule"),
        boundaryRule = S(contract, "boundaryRule"),
        combinatorialCubicalCarrierOrientationRule = S(contract, "cubicalCarrierOrientationRule"),
        doubledSliceRule = S(contract, "doubledSliceRule"),
        orderComplexPullbackCoefficient = "+1 because reflection preserves strict cell-rank order",
        combinatorialCubicalCarrierPullbackCoefficient = "independently derived from the restricted diagonal reflection Jacobian and compared with the frozen axis-zero rule",
    },
    pairAudits,
    completePairCount,
    failedPairCount = pairAudits.Length - completePairCount,
    finiteDualSurvivorPullbackBoundaryClosed = completePairCount == 2,
    finiteCombinatorialChainAndCarrierClosure = completePairCount == 2,
    positiveResultScope = "finite-combinatorial-chain-and-carrier-closure-only",
    actionCovarianceEvaluated = false,
    actionCarrierTransformationEstablished = false,
    authorityFirewalls = contract.GetProperty("authorityFirewalls").Clone(),
    candidateSelected = false,
    candidateGeometryInstalled = false,
    candidateGeometryRegistered = false,
    reflectionRegistered = false,
    productionGeometrySelected = false,
    allowedVolumeEmbeddingEstablished = false,
    sharedEdgeGluingTheoremEstablished = false,
    allVolumeGluingTheoremEstablished = false,
    positiveTimeAlgebraMembershipEstablished = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    finiteOnly = true,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    phase515Unlocked = false,
    phase516Unlocked = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    phase458EvaluationPerformed = false,
    phase481PackWorkPerformed = false,
    actionOrObservableEvaluated = false,
    normalizedMeasureEvaluated = false,
    candidateToTargetEqualityEstablished = false,
    hermiticityEstablished = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    launchAuthorized = false,
    accelerationAuthorized = false,
    o4Discharged = false,
    humanRulingPerformed = false,
    externalReviewPending = true,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    physicalUnitOrGevClaimMade = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a19BoundaryHeld = true,
    decision = verdict switch
    {
        "invalid-or-drifted-input" => "At least one frozen input, pair, extent, or rule is invalid or drifted; no finite pullback/boundary conclusion is accepted.",
        "finite-dual-survivor-pullback-boundary-closed" => "Both frozen barycentric survivor pairs pass exact finite combinatorial chain, boundary, incidence, slice, and cubical-carrier controls at extents 3 and 4. No action or observable was evaluated, so action covariance and an action-carrier transformation remain unestablished. This does not install or register a geometry or establish an embedding, gluing, positive-time-algebra, or reflection-positivity result.",
        _ => "Only the tested survivor pairs and finite extents are adjudicated. A failed row refutes only that pair and scope; nothing is installed, registered, or authorized.",
    },
};

Require(contractValid, "Phase525 frozen contract drifted.");
Require(!invalidOrDrifted, "Phase525 exact inputs or precursor semantics drifted.");
Require(precedenceBatteryPassed, "Phase525 deterministic precedence evaluator battery failed.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase525 expected terminal drifted.");
Require(pairAudits.Length == 2 && pairAudits.All(x => x.ExtentAudits.Length == 2 && x.CompleteFinitePullbackBoundary),
    "Phase525 finite survivor pullback/boundary controls drifted.");
Require(!result.candidateGeometryInstalled && !result.candidateGeometryRegistered && !result.reflectionRegistered
    && !result.productionGeometrySelected && !result.allowedVolumeEmbeddingEstablished
    && !result.sharedEdgeGluingTheoremEstablished && !result.allVolumeGluingTheoremEstablished
    && !result.positiveTimeAlgebraMembershipEstablished && !result.reflectionPositivityEstablished
    && !result.reflectionPositivityRefuted && !result.theoremClaimed && !result.targetCounterexampleClaimed,
    "Phase525 geometry, gluing, algebra, positivity, or theorem firewall drifted.");
Require(result.finiteOnly && !result.phase515MayBeCreated && !result.phase516MayBeCreated
    && !result.phase515Unlocked && !result.phase516Unlocked && !result.phase458G1Satisfied && !result.closesLimbL8
    && !result.samplingOrReprocessingRun && !result.productionAuthorized && result.externalReviewPending
    && !result.physicalUnitOrGevClaimMade && result.promotedPhysicalMassClaimCount == 0,
    "Phase525 scope, execution, review, or physical-claim firewall drifted.");
Require(result.finiteCombinatorialChainAndCarrierClosure && !result.actionCovarianceEvaluated
    && !result.actionCarrierTransformationEstablished && AuthorityFirewallsValid(result.authorityFirewalls)
    && !result.candidateSelected && !result.actionOrObservableEvaluated
    && !result.normalizedMeasureEvaluated && !result.phase458EvaluationPerformed
    && !result.phase481PackWorkPerformed && !result.samplingOrReprocessingRun && !result.hmcRun
    && !result.benchmarkRun && !result.productionAuthorized && !result.launchAuthorized
    && !result.accelerationAuthorized && !result.o4Discharged && !result.humanRulingPerformed
    && !result.sourceContractApplicationAllowed,
    "Phase525 combinatorial-only or complete A19 authority firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "a5_survivor_reflection_pullback_boundary_audit.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_survivor_reflection_pullback_boundary_audit_summary.json"), json);
Console.WriteLine(result.terminalStatus);

static ExtentAudit Audit(OrderComplex complex, PairSpec pair)
{
    int[] cellMap = complex.Cells.Select(cell => complex.Index[ReflectCell(cell, complex.Extent, pair).Key]).ToArray();
    bool vertexBijection = cellMap.Distinct().Count() == cellMap.Length;
    bool vertexInvolution = Enumerable.Range(0, cellMap.Length).All(i => cellMap[cellMap[i]] == i);
    bool masksPreserved = complex.Cells.Select((cell, i) => cell.Mask == complex.Cells[cellMap[i]].Mask).All(x => x);
    bool mappedCellsCanonical = cellMap.All(i => i >= 0 && i < complex.Cells.Length);

    long sharedCellIncidenceCheckCount = 0;
    bool sharedCellIncidenceRetained = true;
    foreach (string cover in complex.CoverRelations)
    {
        int[] edge = ParseKey(cover);
        sharedCellIncidenceCheckCount++;
        sharedCellIncidenceRetained &= complex.CoverRelations.Contains(OrderedKey(cellMap[edge[0]], cellMap[edge[1]]));
    }

    long mappedSimplexCount = 0;
    long simplexInvolutionCheckCount = 0;
    long boundaryCommutationCheckCount = 0;
    bool orientedChainPullbackClosed = true;
    bool chainInvolution = true;
    bool boundaryCommutation = true;
    bool faceOrientationPreserved = true;
    int orderComplexEdgeOrientationReversalCount = 0;
    int orderComplexFaceOrientationReversalCount = 0;
    int[] simplexCounts = complex.Simplices.Select(x => x.Count).ToArray();
    int[] mappedSimplexCounts = new int[5];
    for (int dimension = 0; dimension <= 4; dimension++)
    foreach (string key in complex.Simplices[dimension])
    {
        int[] source = ParseKey(key);
        int[] imageRaw = source.Select(v => cellMap[v]).ToArray();
        int pullbackOrientationSign = PermutationSignToRankOrder(imageRaw, complex.Cells);
        int[] image = imageRaw.OrderBy(v => PopCount(complex.Cells[v].Mask)).ToArray();
        string imageKey = OrderedKey(image);
        bool imagePresent = complex.Simplices[dimension].Contains(imageKey);
        orientedChainPullbackClosed &= imagePresent;
        if (dimension == 1 && pullbackOrientationSign < 0) orderComplexEdgeOrientationReversalCount++;
        if (dimension == 2 && pullbackOrientationSign < 0) orderComplexFaceOrientationReversalCount++;
        faceOrientationPreserved &= pullbackOrientationSign == 1
            && StrictlyIncreasingRanks(source, complex.Cells) && StrictlyIncreasingRanks(image, complex.Cells)
            && source.Select(v => PopCount(complex.Cells[v].Mask))
                .SequenceEqual(image.Select(v => PopCount(complex.Cells[v].Mask)));
        if (!imagePresent) continue;
        mappedSimplexCount++;
        mappedSimplexCounts[dimension]++;
        simplexInvolutionCheckCount++;
        chainInvolution &= OrderedEnumerableKey(image.Select(v => cellMap[v])) == key;
        if (dimension == 0) continue;
        for (int omitted = 0; omitted < source.Length; omitted++)
        {
            boundaryCommutationCheckCount++;
            int sourceCoefficient = (omitted & 1) == 0 ? 1 : -1;
            int mappedOmittedVertex = cellMap[source[omitted]];
            int canonicalOmitted = Array.IndexOf(image, mappedOmittedVertex);
            int[] reflectedFaceRaw = source.Where((_, i) => i != omitted).Select(v => cellMap[v]).ToArray();
            int facePullbackSign = PermutationSignToRankOrder(reflectedFaceRaw, complex.Cells);
            string reflectedBoundaryFace = OrderedEnumerableKey(reflectedFaceRaw.OrderBy(v => PopCount(complex.Cells[v].Mask)));
            string boundaryOfReflected = OrderedEnumerableKey(image.Where((_, i) => i != canonicalOmitted));
            int leftCoefficient = sourceCoefficient * facePullbackSign;
            int rightCoefficient = pullbackOrientationSign * ((canonicalOmitted & 1) == 0 ? 1 : -1);
            boundaryCommutation &= reflectedBoundaryFace == boundaryOfReflected
                && complex.Simplices[dimension - 1].Contains(reflectedBoundaryFace)
                && canonicalOmitted >= 0 && leftCoefficient == rightCoefficient;
            faceOrientationPreserved &= facePullbackSign == 1 && canonicalOmitted == omitted;
        }
    }

    int doubledPeriod = 2 * complex.Extent;
    long sliceAssignmentCheckCount = 0;
    bool sliceAssignmentCovariant = true;
    int boundaryCarrierCount = 0;
    int positiveCarrierCount = 0;
    int negativeCarrierCount = 0;
    int positiveToNegativeCount = 0;
    int negativeToPositiveCount = 0;
    var carrierRows = new CarrierRankAudit[5];
    for (int rank = 0; rank <= 4; rank++)
    {
        CubicalCell[] rankCells = complex.Cells.Where(cell => PopCount(cell.Mask) == rank).ToArray();
        int reversed = 0;
        int preserved = 0;
        bool mappedCarrierControlsPassed = true;
        foreach (CubicalCell cell in rankCells)
        {
            CubicalCell image = complex.Cells[cellMap[complex.Index[cell.Key]]];
            bool axisActive = ((cell.Mask >> pair.Axis) & 1) != 0;
            int sign = RestrictedOrientationSign(cell.Mask, ReflectionJacobianDiagonal(pair));
            int frozenRuleSign = axisActive ? -1 : 1;
            if (sign < 0) reversed++; else preserved++;
            int imageSign = RestrictedOrientationSign(image.Mask, ReflectionJacobianDiagonal(pair));
            mappedCarrierControlsPassed &= image.Mask == cell.Mask && sign == frozenRuleSign && sign * imageSign == 1;

            int tau2 = Mod(2 * cell.Base[0] + (axisActive ? 1 : 0) - pair.Offset, doubledPeriod);
            bool imageAxisActive = (image.Mask & 1) != 0;
            int imageTau2 = Mod(2 * image.Base[0] + (imageAxisActive ? 1 : 0) - pair.Offset, doubledPeriod);
            sliceAssignmentCheckCount++;
            sliceAssignmentCovariant &= imageTau2 == Mod(-tau2, doubledPeriod);
            if (tau2 == 0 || tau2 == complex.Extent) boundaryCarrierCount++;
            else if (tau2 < complex.Extent)
            {
                positiveCarrierCount++;
                if (imageTau2 > complex.Extent) positiveToNegativeCount++;
            }
            else
            {
                negativeCarrierCount++;
                if (imageTau2 < complex.Extent && imageTau2 > 0) negativeToPositiveCount++;
            }
        }
        int volume = Pow4(complex.Extent);
        int expectedReversed = rank == 0 ? 0 : Binomial3(rank - 1) * volume;
        int expectedPreserved = rank == 4 ? 0 : Binomial3(rank) * volume;
        carrierRows[rank] = new CarrierRankAudit(rank, rankCells.Length, reversed, preserved,
            expectedReversed, expectedPreserved, mappedCarrierControlsPassed
                && reversed == expectedReversed && preserved == expectedPreserved);
    }

    long expectedBoundaryChecks = simplexCounts.Select((count, dimension) => dimension == 0 ? 0L : (long)count * (dimension + 1)).Sum();
    bool exactCountsValid = complex.Cells.Length == 16 * Pow4(complex.Extent)
        && simplexCounts.SequenceEqual(new[] { 16, 240, 800, 960, 384 }.Select(x => x * Pow4(complex.Extent)))
        && complex.CoverRelations.Count == 64 * Pow4(complex.Extent)
        && mappedSimplexCounts.SequenceEqual(simplexCounts)
        && mappedSimplexCount == simplexCounts.Sum(x => (long)x)
        && boundaryCommutationCheckCount == expectedBoundaryChecks;
    bool slicePartitionValid = boundaryCarrierCount + positiveCarrierCount + negativeCarrierCount == complex.Cells.Length
        && positiveToNegativeCount == positiveCarrierCount && negativeToPositiveCount == negativeCarrierCount;
    bool complete = vertexBijection && vertexInvolution && masksPreserved && mappedCellsCanonical
        && sharedCellIncidenceRetained && orientedChainPullbackClosed && chainInvolution
        && boundaryCommutation && faceOrientationPreserved && sliceAssignmentCovariant && slicePartitionValid
        && carrierRows.All(x => x.ControlsPassed) && exactCountsValid;

    return new ExtentAudit(complex.Extent, complex.Cells.Length, simplexCounts, mappedSimplexCounts,
        complex.CoverRelations.Count, sharedCellIncidenceCheckCount, sharedCellIncidenceRetained,
        mappedSimplexCount, simplexInvolutionCheckCount, orientedChainPullbackClosed, chainInvolution,
        boundaryCommutationCheckCount, expectedBoundaryChecks, boundaryCommutation, faceOrientationPreserved,
        vertexBijection, vertexInvolution, masksPreserved, mappedCellsCanonical,
        orderComplexEdgeOrientationReversalCount,
        orderComplexFaceOrientationReversalCount,
        cubicalRankOneCarrierReversalCount: carrierRows[1].ReversedCount,
        cubicalRankTwoFaceOrientationReversalCount: carrierRows[2].ReversedCount,
        sliceAssignmentCheckCount, sliceAssignmentCovariant, boundaryCarrierCount,
        positiveCarrierCount, negativeCarrierCount, positiveToNegativeCount, negativeToPositiveCount,
        carrierRows, exactCountsValid, complete);
}

static OrderComplex BuildOrderComplex(int extent)
{
    var cells = new List<CubicalCell>();
    var index = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (int[] coordinates in Coordinates(extent))
    for (int mask = 0; mask < 16; mask++)
    {
        var cell = new CubicalCell(coordinates, mask);
        index.Add(cell.Key, cells.Count);
        cells.Add(cell);
    }

    var topChains = new HashSet<string>(StringComparer.Ordinal);
    foreach (int[] cubeBase in Coordinates(extent))
    for (int cornerMask = 0; cornerMask < 16; cornerMask++)
    foreach (int[] permutation in Permutations4())
    {
        int activeMask = 0;
        var chain = new int[5];
        for (int rank = 0; rank <= 4; rank++)
        {
            int[] cellBase = new int[4];
            for (int axis = 0; axis < 4; axis++)
                cellBase[axis] = Mod(cubeBase[axis] + (((activeMask >> axis) & 1) == 0
                    && ((cornerMask >> axis) & 1) == 1 ? 1 : 0), extent);
            chain[rank] = index[new CubicalCell(cellBase, activeMask).Key];
            if (rank < 4) activeMask |= 1 << permutation[rank];
        }
        topChains.Add(OrderedKey(chain));
    }

    HashSet<string>[] simplices = Enumerable.Range(0, 5)
        .Select(_ => new HashSet<string>(StringComparer.Ordinal)).ToArray();
    var coverRelations = new HashSet<string>(StringComparer.Ordinal);
    foreach (string topKey in topChains)
    {
        int[] chain = ParseKey(topKey);
        for (int subset = 1; subset < 32; subset++)
        {
            int[] simplex = Enumerable.Range(0, 5).Where(bit => ((subset >> bit) & 1) == 1)
                .Select(bit => chain[bit]).ToArray();
            simplices[simplex.Length - 1].Add(OrderedKey(simplex));
        }
        for (int rank = 0; rank < 4; rank++) coverRelations.Add(OrderedKey(chain[rank], chain[rank + 1]));
    }
    return new OrderComplex(extent, cells.ToArray(), index, simplices, coverRelations);
}

static CubicalCell ReflectCell(CubicalCell cell, int extent, PairSpec pair)
{
    int[] mapped = (int[])cell.Base.Clone();
    bool active = ((cell.Mask >> pair.Axis) & 1) == 1;
    mapped[pair.Axis] = Mod(pair.Offset - cell.Base[pair.Axis] - (active ? 1 : 0), extent);
    return new CubicalCell(mapped, cell.Mask);
}

static int[] ReflectionJacobianDiagonal(PairSpec pair)
{
    var diagonal = new[] { 1, 1, 1, 1 };
    diagonal[pair.Axis] = -1;
    return diagonal;
}

static int RestrictedOrientationSign(int activeMask, int[] reflectionJacobianDiagonal)
{
    int determinant = 1;
    for (int axis = 0; axis < 4; axis++)
        if (((activeMask >> axis) & 1) != 0)
            determinant *= reflectionJacobianDiagonal[axis];
    return determinant;
}

static IEnumerable<int[]> Coordinates(int extent)
{
    for (int a = 0; a < extent; a++) for (int b = 0; b < extent; b++)
    for (int c = 0; c < extent; c++) for (int d = 0; d < extent; d++) yield return new[] { a, b, c, d };
}
static IEnumerable<int[]> Permutations4()
{
    for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++) if (b != a)
    for (int c = 0; c < 4; c++) if (c != a && c != b)
    for (int d = 0; d < 4; d++) if (d != a && d != b && d != c) yield return new[] { a, b, c, d };
}
static int Binomial3(int k) => k switch { 0 => 1, 1 => 3, 2 => 3, 3 => 1, _ => 0 };
static int Pow4(int n) => checked(n * n * n * n);
static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
static int PopCount(int value)
{
    int count = 0;
    while (value != 0) { count += value & 1; value >>= 1; }
    return count;
}
static bool StrictlyIncreasingRanks(int[] simplex, CubicalCell[] cells) => simplex
    .Select(v => PopCount(cells[v].Mask)).Zip(simplex.Select(v => PopCount(cells[v].Mask)).Skip(1))
    .All(pair => pair.First < pair.Second);
static int PermutationSignToRankOrder(int[] simplex, CubicalCell[] cells)
{
    int inversions = 0;
    for (int i = 0; i < simplex.Length; i++)
    for (int j = i + 1; j < simplex.Length; j++)
        if (PopCount(cells[simplex[i]].Mask) > PopCount(cells[simplex[j]].Mask)) inversions++;
    return (inversions & 1) == 0 ? 1 : -1;
}
static string OrderedKey(params int[] vertices) => string.Join(",", vertices);
static string OrderedEnumerableKey(IEnumerable<int> vertices) => string.Join(",", vertices);
static int[] ParseKey(string key) => key.Split(',').Select(int.Parse).ToArray();
static JsonElement ReadRoot(string path) => JsonDocument.Parse(File.ReadAllBytes(path)).RootElement.Clone();
static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(path) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}
static bool ExactBindingInventoryValid(Binding[] bindings)
{
    var expected = new (string Id, string Path)[]
    {
        ("phase521-summary", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/output/a5_frozen_reflection_compatible_triangulation_census_summary.json"),
        ("phase521-contract", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/preregistration/phase521_frozen_reflection_compatible_triangulation_census_contract_v1.json"),
        ("phase521-program", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/Program.cs"),
        ("phase522-summary", "studies/phase522_a5_foundation_candidate_reduction_001/output/a5_foundation_candidate_reduction_summary.json"),
        ("phase522-contract", "studies/phase522_a5_foundation_candidate_reduction_001/preregistration/phase522_a5_foundation_candidate_reduction_contract_v1.json"),
        ("phase522-program", "studies/phase522_a5_foundation_candidate_reduction_001/Program.cs"),
    };
    return bindings.Length == expected.Length
        && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == expected.Length
        && bindings.Select((x, i) => x.Id == expected[i].Id && x.Path == expected[i].Path).All(x => x);
}

static string EvaluatePrecedence(bool invalid, int completeCount, int pairCount, string[] precedence) =>
    invalid ? precedence[0]
    : completeCount == 0 ? precedence[1]
    : completeCount < pairCount ? precedence[2]
    : precedence[3];

static PrecedenceBatteryCase PrecedenceCase(string id, bool invalid, int completeCount, int pairCount,
    string[] precedence, string expected)
{
    string actual = EvaluatePrecedence(invalid, completeCount, pairCount, precedence);
    return new PrecedenceBatteryCase(id, expected, actual, actual == expected);
}

static bool AuthorityFirewallsValid(JsonElement firewalls)
{
    string[] expectedNames =
    [
        "candidateSelectionAllowed", "candidateRegistrationAllowed", "productionGeometrySelectionAllowed",
        "allVolumeEmbeddingOrGluingClaimAllowed", "positiveTimeAlgebraClaimAllowed",
        "reflectionPositivityRulingAllowed", "theoremOrCounterexampleAllowed",
        "phase515CreationOrUnlockAllowed", "phase516CreationOrUnlockAllowed",
        "phase458G1SatisfactionAllowed", "limbL8ClosureAllowed", "phase458EvaluationAllowed",
        "phase481PackWorkAllowed", "actionOrObservableEvaluationAllowed",
        "normalizedMeasureEvaluationAllowed", "samplingOrReprocessingAllowed", "hmcAllowed",
        "benchmarkAllowed", "productionAllowed", "launchAllowed", "accelerationAllowed",
        "o4OrHumanRulingAllowed", "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
    ];
    return firewalls.EnumerateObject().Select(x => x.Name).SequenceEqual(expectedNames, StringComparer.Ordinal)
        && firewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False);
}
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string S(JsonElement value, string name) => value.GetProperty(name).GetString() ?? "";
static int I(JsonElement value, string name) => value.GetProperty(name).GetInt32();
static bool B(JsonElement value, string name) => value.GetProperty(name).GetBoolean();
static int[] Ints(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
static string[] Strings(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed class CubicalCell
{
    public CubicalCell(int[] cellBase, int mask) { Base = (int[])cellBase.Clone(); Mask = mask; }
    public int[] Base { get; }
    public int Mask { get; }
    public string Key => $"{string.Join(",", Base)}|{Mask}";
}

internal sealed class PairSpec
{
    public PairSpec(string candidateId, string reflectionId, int axis, int offset,
        string inactiveAxis0BaseMap, string activeAxis0BaseMap)
    {
        CandidateId = candidateId; ReflectionId = reflectionId; Axis = axis; Offset = offset;
        InactiveAxis0BaseMap = inactiveAxis0BaseMap; ActiveAxis0BaseMap = activeAxis0BaseMap;
    }
    public string CandidateId { get; }
    public string ReflectionId { get; }
    public int Axis { get; }
    public int Offset { get; }
    public string InactiveAxis0BaseMap { get; }
    public string ActiveAxis0BaseMap { get; }
    public bool EqualsValues(string candidateId, string reflectionId, int axis, int offset, string inactive, string active) =>
        CandidateId == candidateId && ReflectionId == reflectionId && Axis == axis && Offset == offset
        && InactiveAxis0BaseMap == inactive && ActiveAxis0BaseMap == active;
}

internal sealed class Binding
{
    public Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
    { Id = id; Path = path; ExpectedSha256 = expectedSha256; ActualSha256 = actualSha256; HashMatches = hashMatches; }
    public string Id { get; }
    public string Path { get; }
    public string ExpectedSha256 { get; }
    public string ActualSha256 { get; }
    public bool HashMatches { get; }
}

internal sealed class OrderComplex
{
    public OrderComplex(int extent, CubicalCell[] cells, Dictionary<string, int> index,
        HashSet<string>[] simplices, HashSet<string> coverRelations)
    { Extent = extent; Cells = cells; Index = index; Simplices = simplices; CoverRelations = coverRelations; }
    public int Extent { get; }
    public CubicalCell[] Cells { get; }
    public Dictionary<string, int> Index { get; }
    public HashSet<string>[] Simplices { get; }
    public HashSet<string> CoverRelations { get; }
}

internal sealed class CarrierRankAudit
{
    public CarrierRankAudit(int rank, int carrierCount, int reversedCount, int preservedCount,
        int expectedReversedCount, int expectedPreservedCount, bool controlsPassed)
    { Rank = rank; CarrierCount = carrierCount; ReversedCount = reversedCount; PreservedCount = preservedCount;
      ExpectedReversedCount = expectedReversedCount; ExpectedPreservedCount = expectedPreservedCount; ControlsPassed = controlsPassed; }
    public int Rank { get; }
    public int CarrierCount { get; }
    public int ReversedCount { get; }
    public int PreservedCount { get; }
    public int ExpectedReversedCount { get; }
    public int ExpectedPreservedCount { get; }
    public bool ControlsPassed { get; }
}

internal sealed class ExtentAudit
{
    public ExtentAudit(int extent, int cellCount, int[] simplexCounts, int[] mappedSimplexCounts,
        int sharedCellIncidenceCount, long sharedCellIncidenceCheckCount, bool sharedCellIncidenceRetained,
        long mappedSimplexCount, long simplexInvolutionCheckCount, bool orientedChainPullbackClosed,
        bool chainInvolution, long boundaryCommutationCheckCount, long expectedBoundaryCommutationCheckCount,
        bool boundaryCommutation, bool faceOrientationPreserved, bool vertexBijection, bool vertexInvolution,
        bool activeMasksPreserved, bool mappedCellsCanonical, int orderComplexEdgeOrientationReversalCount,
        int orderComplexFaceOrientationReversalCount,
        int cubicalRankOneCarrierReversalCount, int cubicalRankTwoFaceOrientationReversalCount,
        long sliceAssignmentCheckCount, bool sliceAssignmentCovariant, int boundaryCarrierCount,
        int positiveCarrierCount, int negativeCarrierCount, int positiveToNegativeCount, int negativeToPositiveCount,
        CarrierRankAudit[] carrierRankAudits, bool exactCountsValid, bool completeFinitePullbackBoundary)
    {
        Extent = extent; CellCount = cellCount; SimplexCounts = simplexCounts; MappedSimplexCounts = mappedSimplexCounts;
        SharedCellIncidenceCount = sharedCellIncidenceCount; SharedCellIncidenceCheckCount = sharedCellIncidenceCheckCount;
        SharedCellIncidenceRetained = sharedCellIncidenceRetained; MappedSimplexCount = mappedSimplexCount;
        SimplexInvolutionCheckCount = simplexInvolutionCheckCount; OrientedChainPullbackClosed = orientedChainPullbackClosed;
        ChainInvolution = chainInvolution; BoundaryCommutationCheckCount = boundaryCommutationCheckCount;
        ExpectedBoundaryCommutationCheckCount = expectedBoundaryCommutationCheckCount; BoundaryCommutation = boundaryCommutation;
        FaceOrientationPreserved = faceOrientationPreserved; VertexBijection = vertexBijection; VertexInvolution = vertexInvolution;
        ActiveMasksPreserved = activeMasksPreserved; MappedCellsCanonical = mappedCellsCanonical;
        OrderComplexEdgeOrientationReversalCount = orderComplexEdgeOrientationReversalCount;
        OrderComplexFaceOrientationReversalCount = orderComplexFaceOrientationReversalCount;
        CubicalRankOneCarrierReversalCount = cubicalRankOneCarrierReversalCount;
        CubicalRankTwoFaceOrientationReversalCount = cubicalRankTwoFaceOrientationReversalCount;
        SliceAssignmentCheckCount = sliceAssignmentCheckCount; SliceAssignmentCovariant = sliceAssignmentCovariant;
        BoundaryCarrierCount = boundaryCarrierCount; PositiveCarrierCount = positiveCarrierCount; NegativeCarrierCount = negativeCarrierCount;
        PositiveToNegativeCount = positiveToNegativeCount; NegativeToPositiveCount = negativeToPositiveCount;
        CarrierRankAudits = carrierRankAudits; ExactCountsValid = exactCountsValid;
        CompleteFinitePullbackBoundary = completeFinitePullbackBoundary;
    }
    public int Extent { get; }
    public int CellCount { get; }
    public int[] SimplexCounts { get; }
    public int[] MappedSimplexCounts { get; }
    public int SharedCellIncidenceCount { get; }
    public long SharedCellIncidenceCheckCount { get; }
    public bool SharedCellIncidenceRetained { get; }
    public long MappedSimplexCount { get; }
    public long SimplexInvolutionCheckCount { get; }
    public bool OrientedChainPullbackClosed { get; }
    public bool ChainInvolution { get; }
    public long BoundaryCommutationCheckCount { get; }
    public long ExpectedBoundaryCommutationCheckCount { get; }
    public bool BoundaryCommutation { get; }
    public bool FaceOrientationPreserved { get; }
    public bool VertexBijection { get; }
    public bool VertexInvolution { get; }
    public bool ActiveMasksPreserved { get; }
    public bool MappedCellsCanonical { get; }
    public int OrderComplexEdgeOrientationReversalCount { get; }
    public int OrderComplexFaceOrientationReversalCount { get; }
    public int CubicalRankOneCarrierReversalCount { get; }
    public int CubicalRankTwoFaceOrientationReversalCount { get; }
    public long SliceAssignmentCheckCount { get; }
    public bool SliceAssignmentCovariant { get; }
    public int BoundaryCarrierCount { get; }
    public int PositiveCarrierCount { get; }
    public int NegativeCarrierCount { get; }
    public int PositiveToNegativeCount { get; }
    public int NegativeToPositiveCount { get; }
    public CarrierRankAudit[] CarrierRankAudits { get; }
    public bool ExactCountsValid { get; }
    public bool CompleteFinitePullbackBoundary { get; }
}

internal sealed class PairAudit
{
    public PairAudit(string candidateId, string reflectionId, ExtentAudit[] extentAudits)
    { CandidateId = candidateId; ReflectionId = reflectionId; ExtentAudits = extentAudits; }
    public string CandidateId { get; }
    public string ReflectionId { get; }
    public ExtentAudit[] ExtentAudits { get; }
    public bool CompleteFinitePullbackBoundary => ExtentAudits.All(x => x.CompleteFinitePullbackBoundary);
}

internal sealed record PrecedenceBatteryCase(string CaseId, string ExpectedVerdict, string ActualVerdict, bool Passed);
