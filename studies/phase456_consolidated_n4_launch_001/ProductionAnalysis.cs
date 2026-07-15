using System.Text.Json;

internal sealed record Phase456RowAnalysis(
    string RowId,
    string Channel,
    string Observable,
    double? Value,
    double? NullValue,
    double? Sigma,
    double? Z,
    double? NEff,
    double? PowerAtReferenceThreeSigmaEffect,
    bool PowerGate,
    bool ThresholdGate,
    string Terminal,
    double?[] JackknifeEffect);

internal sealed record Phase456ProductionAnalysis(
    bool InputShapeValid,
    string[] InputErrors,
    bool ProductionDefaultsVerified,
    bool PackHashVerified,
    bool PerSiteStorageVerified,
    bool SamplerAndControlGatesPassed,
    bool ExactGaussianControlsVerified,
    double? MeasuredRuntimeSeconds,
    double? MeasuredCpuWeeks,
    double CpuWeekBudget,
    bool G2WithinBudget,
    int RowCount,
    double PerRowSigmaThreshold,
    double? FamilyWiseSigmaThreshold,
    double? AppliedSigmaThreshold,
    string FamilyWiseCalibrationMethod,
    int FamilyWiseCalibrationSamples,
    double[][] NullCorrelation,
    Phase456RowAnalysis[] Rows,
    bool AllRowsAdequatelyPowered,
    bool AllRowsFreeCompatible,
    bool CoherentDistinctIrrepDeparture,
    bool MandatoryN5Escalation,
    bool G3Motivated,
    string VerdictKind,
    string Decision);

internal static class Phase456ProductionAnalyzer
{
    private const int JackknifeBlocks = 50;
    private const double NEffFloor = 100.0;
    private const double PerRowThreshold = 3.0;
    private const int CalibrationSamples = 500_000;
    private const int CalibrationSeed = 20260712;
    private const string PinnedPackSha256 = "40fd3c3488f94d18f50961e85d0bb3a3eabd1a31a071b61149875b8cf3d437aa";

    internal static Phase456ProductionAnalysis Evaluate(JsonElement root)
    {
        var errors = new List<string>();
        bool defaults = false, pack = false, storage = false, sampler = false, exactGaussian = false;
        double? measuredRuntimeSeconds = null, measuredCpuWeeks = null;
        JsonElement torus = default, prod = default, free = default, gaussian = default;
        try
        {
            var run = root.GetProperty("phase456Run");
            defaults = S(run, "mode") == "production" && B(run, "committedDefaults")
                && B(run, "environmentOverridesRefused");
            pack = S(run, "pinnedPackSha256") == PinnedPackSha256
                && S(run, "computedPackSha256") == PinnedPackSha256;
            storage = B(run, "perSiteCorrelatorStorage") && B(run, "perFaceTypeResolutionRetained");
            torus = root.GetProperty("tori").EnumerateArray().Single(x => I(x, "torusN") == 4);
            var columns = torus.GetProperty("columns").EnumerateArray().ToArray();
            measuredRuntimeSeconds = D(root, "runtimeSeconds");
            measuredCpuWeeks = columns.Sum(x => D(x, "msPerTrajectory") * (I(x, "trajectories") + I(x, "warmup")))
                / (7.0 * 24.0 * 60.0 * 60.0 * 1000.0);
            prod = columns.Single(x => S(x, "member") == "sd2-id0/c0.5" && S(x, "kind") == "production" && D(x, "beta") == 1.0);
            free = columns.Single(x => S(x, "member") == "sd2-id0/c0.5" && S(x, "kind") == "free-field-control" && D(x, "beta") == 400.0);
            gaussian = torus.GetProperty("gaussianSimBattery").EnumerateArray()
                .Single(x => S(x, "member") == "sd2-id0/c0.5").GetProperty("phase456ExactGaussianControl");
            exactGaussian = I(gaussian, "independentSamples") == 4000
                && B(gaussian.GetProperty("identityIrrep2x2Gevp"), "c0PositiveDefinite")
                && NullableRows(gaussian.GetProperty("identityIrrep2x2Gevp"), "jackknifeDominantEigenvalue").Length == JackknifeBlocks
                && NullableRows(gaussian.GetProperty("a2").GetProperty("o1"), "jackknife").Length == JackknifeBlocks
                && NullableRows(gaussian.GetProperty("kMin").GetProperty("spatialAxisAverageO1"), "jackknife").Length == JackknifeBlocks
                && NullableArray(gaussian.GetProperty("binder"), "binderJackknife").Length == JackknifeBlocks
                && NullableArray(gaussian.GetProperty("binder"), "susceptibilityJackknife").Length == JackknifeBlocks
                && D(gaussian.GetProperty("binder"), "nEffPhi") >= NEffFloor;
            storage &= B(prod.GetProperty("phase456"), "perSiteCorrelatorStorage")
                && B(prod.GetProperty("phase456"), "perFaceTypeResolutionRetained")
                && B(free.GetProperty("phase456"), "perSiteCorrelatorStorage")
                && B(free.GetProperty("phase456"), "perFaceTypeResolutionRetained");
            storage &= SpatialStorageValid(prod.GetProperty("phase456"))
                && SpatialStorageValid(free.GetProperty("phase456"));
            var batteries = root.GetProperty("batteries");
            sampler = B(batteries, "batteriesAllPassed") && B(batteries, "samplerGates")
                && B(batteries, "freeFieldGate") && B(batteries, "nEffGateAll");
        }
        catch (Exception ex)
        {
            errors.Add("production input shape: " + ex.Message);
        }

        if (!defaults) errors.Add("committed production defaults were not verified");
        if (!pack) errors.Add("production output is not bound to the pinned pack hash");
        if (!storage) errors.Add("mandatory per-site/per-face-type resolution was not verified");
        if (!exactGaussian) errors.Add("full-row exact independent-Gaussian controls were not verified");

        var drafts = new List<RowDraft>();
        if (errors.Count == 0)
        {
            try
            {
                var p456 = prod.GetProperty("phase456");
                var g456 = gaussian;
                var pA1 = MassFromGevp(p456.GetProperty("identityIrrep2x2Gevp"));
                var gA1 = MassFromGevp(g456.GetProperty("identityIrrep2x2Gevp"));
                var pA2 = MassFromCorrelator(p456.GetProperty("a2").GetProperty("o1"));
                var gA2 = MassFromCorrelator(g456.GetProperty("a2").GetProperty("o1"));
                var pK = MassFromCorrelator(p456.GetProperty("kMin").GetProperty("spatialAxisAverageO1"));
                var gK = MassFromCorrelator(g456.GetProperty("kMin").GetProperty("spatialAxisAverageO1"));

                double a1NEff = System.Math.Min(D(prod, "nEff"), I(g456, "independentSamples"));
                var rowNEff = p456.GetProperty("rowEffectiveSampleSizes");
                double a2NEff = System.Math.Min(D(rowNEff, "nEffA2"), I(g456, "independentSamples"));
                double kNEff = System.Math.Min(D(rowNEff, "nEffKMin"), I(g456, "independentSamples"));
                drafts.Add(Difference("a1-gevp-gap", "A1-like", "a*m(A1, k=0) - direct exact-Gaussian control", pA1, gA1, a1NEff));
                drafts.Add(Difference("a2-gap", "A2-like", "a*m(A2, k=0) - direct exact-Gaussian control", pA2, gA2, a2NEff));

                var dispersion = DispersionDifference(pA1, pK);
                var gaussianDispersion = DispersionDifference(gA1, gK);
                drafts.Add(new RowDraft("kmin-dispersion", "A1-like dispersion",
                    "lattice-cosh dispersion residual - direct exact-Gaussian dispersion residual",
                    dispersion.Value is double dv && gaussianDispersion.Value is double gv ? dv - gv : null,
                    0.0, Combine(dispersion.Jackknife, gaussianDispersion.Jackknife, (a, b) => a - b), System.Math.Min(a1NEff, kNEff)));

                var pb = p456.GetProperty("binder"); var gb = g456.GetProperty("binder");
                double phiNEff = System.Math.Min(D(pb, "nEffPhi"), D(gb, "nEffPhi"));
                double? pBinder = DN(pb, "binderCumulant"), gBinder = DN(gb, "binderCumulant");
                drafts.Add(new RowDraft("binder-gaussian-null", "invariant-ray", "centered Binder cumulant - direct exact-Gaussian control",
                    pBinder is double pbu && gBinder is double gbu ? pbu - gbu : null, 0.0,
                    Combine(NullableArray(pb, "binderJackknife"), NullableArray(gb, "binderJackknife"), (a, b) => a - b), phiNEff));

                double? pChi = DN(pb, "susceptibility"), gChi = DN(gb, "susceptibility");
                var pChiJk = NullableArray(pb, "susceptibilityJackknife");
                var gChiJk = NullableArray(gb, "susceptibilityJackknife");
                drafts.Add(new RowDraft("susceptibility-gaussian-control", "invariant-ray",
                    "chi(beta=1) - direct beta=1 exact-Gaussian control",
                    pChi is null || gChi is null ? null : pChi - gChi,
                    0.0, Combine(pChiJk, gChiJk, (a, b) => a - b), phiNEff));
            }
            catch (Exception ex)
            {
                errors.Add("production row extraction: " + ex.Message);
            }
        }

        int rowCount = 5;
        double[][] correlation = Identity(rowCount);
        double? familyThreshold = null;
        string calibrationMethod = "unavailable-input-invalid";
        if (errors.Count == 0 && drafts.Count == rowCount && drafts.All(x => Valid(x.Jackknife) && x.Value is double))
        {
            correlation = JackknifeCorrelation(drafts.Select(x => x.Jackknife.Select(v => v!.Value).ToArray()).ToArray());
            familyThreshold = CalibrateMaxAbsThreshold(correlation, CalibrationSamples, CalibrationSeed);
            calibrationMethod = "fixed-seed multivariate-normal max-|Z| calibration from aligned null jackknife correlation";
        }
        else if (errors.Count == 0)
        {
            errors.Add($"one or more analysis rows lacks a finite full estimate or all {JackknifeBlocks} aligned jackknife replicates");
        }

        double? applied = familyThreshold is double ft ? System.Math.Max(PerRowThreshold, ft) : null;
        var rows = drafts.Select(d => FinalizeRow(d, applied)).ToArray();
        var invalidRowIds = rows.Where(r => r.Terminal == "invalid").Select(r => r.RowId).ToArray();
        if (invalidRowIds.Length != 0)
            errors.Add("invalid production rows (non-finite estimate, zero/non-finite uncertainty, or missing threshold): " + string.Join(", ", invalidRowIds));
        bool allPowered = rows.Length == rowCount && rows.All(r => r.PowerGate);
        bool allCompatible = allPowered && rows.All(r => !r.ThresholdGate && r.Terminal == "free-compatible");
        var a1 = rows.FirstOrDefault(r => r.RowId == "a1-gevp-gap");
        var a2 = rows.FirstOrDefault(r => r.RowId == "a2-gap");
        bool coherent = a1 is not null && a2 is not null && a1.ThresholdGate && a2.ThresholdGate
            && a1.Value is double v1 && a2.Value is double v2 && System.Math.Sign(v1) == System.Math.Sign(v2)
            && a1.Z is double z1 && a2.Z is double z2 && System.Math.Abs(z1 - z2) <= 2.0;
        bool n5 = sampler && allPowered && coherent;
        var binderRow = rows.FirstOrDefault(r => r.RowId == "binder-gaussian-null");
        var susceptibilityRow = rows.FirstOrDefault(r => r.RowId == "susceptibility-gaussian-control");
        bool inputValid = errors.Count == 0;
        bool g3 = inputValid && ((binderRow?.Z ?? 0.0) >= 2.0 || (susceptibilityRow?.Z ?? 0.0) >= 2.0);

        string verdict = !inputValid ? "production-analysis-invalid"
            : !sampler ? "production-controls-failed"
            : !allPowered ? "production-underpowered"
            : n5 ? "t2-coherent-distinct-irrep-departure-n5-mandatory"
            : allCompatible ? "t1-quasi-free-compatible-at-probed-volume"
            : "mixed-row-outcome-no-coherent-distinct-irrep-departure";
        string decision = verdict switch
        {
            "t1-quasi-free-compatible-at-probed-volume" =>
                "All pre-registered rows are adequately powered and remain within the applied family-wise/per-row threshold. Record only a quasi-free massive lattice-spectrum characterization at n=4; this is not evidence of interacting dynamics and carries no physical-mass claim.",
            "t2-coherent-distinct-irrep-departure-n5-mandatory" =>
                "A coherent threshold-clearing departure occurs in both A1-like and A2-like realized irreps. The pre-registered mandatory n=5 escalation fires before any claim.",
            "production-underpowered" =>
                "At least one row fails the pre-registered power gate. Emit underpowered for that row and make no claim either way.",
            "production-controls-failed" =>
                "At least one sampler or exact-free control gate failed. The production result fail-closes; preserve the run as a negative diagnostic and make no spectrum claim.",
            "mixed-row-outcome-no-coherent-distinct-irrep-departure" =>
                "At least one powered row departs from its null, but the distinct-irrep coherence rule does not fire. Record the mixed row table; no n=5 trigger and no dynamical-structure claim.",
            _ => "One or more pre-registered rows is non-finite or otherwise invalid, so the entire production analysis is invalid. Fail-closed: do not interpret the measurements, motivate G3, close a ledger limb, or authorize a rerun from this artifact.",
        };

        const double cpuWeekBudget = 2.0;
        bool g2WithinBudget = measuredCpuWeeks is double cpuWeeks && cpuWeeks <= cpuWeekBudget;
        return new Phase456ProductionAnalysis(inputValid, errors.ToArray(), defaults, pack, storage, sampler, exactGaussian,
            measuredRuntimeSeconds, measuredCpuWeeks, cpuWeekBudget, g2WithinBudget,
            rowCount, PerRowThreshold, familyThreshold, applied, calibrationMethod, CalibrationSamples,
            correlation, rows, allPowered, allCompatible, coherent, n5, g3, verdict, decision);
    }

    private static Phase456RowAnalysis FinalizeRow(RowDraft d, double? threshold)
    {
        double? sigma = Valid(d.Jackknife) ? JackknifeSigma(d.Jackknife.Select(x => x!.Value).ToArray()) : null;
        double? effect = d.Value is double v && d.NullValue is double n ? v - n : null;
        double? z = effect is double e && sigma is > 0 ? System.Math.Abs(e) / sigma : null;
        double? power = threshold is double th && d.NEff is double ne && ne > 0
            ? TwoSidedPower(th, 3.0 * System.Math.Sqrt(ne / NEffFloor)) : null;
        bool powerGate = sigma is > 0 && z is not null && d.NEff is >= NEffFloor && power is >= 0.8;
        bool thresholdGate = powerGate && z is double zv && threshold is double tv && zv >= tv;
        string terminal = sigma is null || z is null || threshold is null ? "invalid"
            : !powerGate ? "underpowered"
            : thresholdGate ? "threshold-clearing-departure"
            : "free-compatible";
        return new Phase456RowAnalysis(d.RowId, d.Channel, d.Observable, d.Value, d.NullValue, sigma, z,
            d.NEff, power, powerGate, thresholdGate, terminal, d.Jackknife);
    }

    private static RowDraft Difference(string id, string channel, string observable, Estimate a, Estimate b, double neff) =>
        new(id, channel, observable,
            a.Value is double av && b.Value is double bv ? av - bv : null, 0.0,
            Combine(a.Jackknife, b.Jackknife, (x, y) => x - y), neff);

    private static Estimate MassFromGevp(JsonElement node) =>
        MassFromArrays(NullableArray(node, "dominantEigenvalue"), NullableRows(node, "jackknifeDominantEigenvalue"));

    private static Estimate MassFromCorrelator(JsonElement node) =>
        MassFromArrays(NullableArray(node, "c"), NullableRows(node, "jackknife"));

    private static Estimate MassFromArrays(double?[] full, double?[][] jackknife)
    {
        double? mass = full.Length > 2 && full[1] is double c1 && full[2] is double c2 ? SolveCoshMass(c1 / c2, 1, 4) : null;
        var jk = jackknife.Select(row => row.Length > 2 && row[1] is double x1 && row[2] is double x2 ? SolveCoshMass(x1 / x2, 1, 4) : null).ToArray();
        return new Estimate(mass, jk);
    }

    private static Estimate DispersionDifference(Estimate m0, Estimate ek)
    {
        static double? F(double? m, double? e)
        {
            if (m is null || e is null) return null;
            return 4.0 * System.Math.Pow(System.Math.Sinh(e.Value / 2.0), 2)
                - 4.0 * System.Math.Pow(System.Math.Sinh(m.Value / 2.0), 2) - 2.0;
        }
        return new Estimate(F(m0.Value, ek.Value), Combine(m0.Jackknife, ek.Jackknife, (m, e) => F(m, e)!.Value));
    }

    private static double? SolveCoshMass(double ratio, int t, int extent)
    {
        double a = extent / 2.0 - t, b = extent / 2.0 - t - 1;
        if (b < -1e-12 || !double.IsFinite(ratio) || ratio <= 1.0) return null;
        double F(double m) => System.Math.Cosh(m * a) / System.Math.Cosh(m * b) - ratio;
        double lo = 0.0, hi = 1.0;
        while (F(hi) < 0 && hi < 60) hi *= 2;
        if (F(hi) < 0) return null;
        for (int i = 0; i < 200; i++) { double mid = (lo + hi) / 2; if (F(mid) < 0) lo = mid; else hi = mid; }
        return (lo + hi) / 2;
    }

    private static double?[] Combine(double?[] a, double?[] b, Func<double, double, double> f)
    {
        int n = System.Math.Min(a.Length, b.Length); var result = new double?[n];
        for (int i = 0; i < n; i++) result[i] = a[i] is double av && b[i] is double bv ? f(av, bv) : null;
        return result;
    }

    private static bool Valid(double?[] x) => x.Length == JackknifeBlocks && x.All(v => v is double d && double.IsFinite(d));

    private static double JackknifeSigma(double[] x)
    {
        double mean = x.Average();
        return System.Math.Sqrt((x.Length - 1.0) / x.Length * x.Sum(v => (v - mean) * (v - mean)));
    }

    private static double[][] JackknifeCorrelation(double[][] rows)
    {
        int n = rows.Length, b = rows[0].Length; var corr = Identity(n);
        for (int i = 0; i < n; i++) for (int j = 0; j < i; j++)
        {
            double mi = rows[i].Average(), mj = rows[j].Average();
            double cov = 0, vi = 0, vj = 0;
            for (int k = 0; k < b; k++)
            {
                double di = rows[i][k] - mi, dj = rows[j][k] - mj;
                cov += di * dj; vi += di * di; vj += dj * dj;
            }
            double r = vi > 0 && vj > 0 ? cov / System.Math.Sqrt(vi * vj) : 0.0;
            corr[i][j] = corr[j][i] = System.Math.Clamp(r, -0.999999, 0.999999);
        }
        return corr;
    }

    private static double CalibrateMaxAbsThreshold(double[][] correlation, int samples, int seed)
    {
        var l = CholeskyWithJitter(correlation); var rng = new Random(seed); var maxima = new double[samples];
        for (int s = 0; s < samples; s++)
        {
            var z = new double[correlation.Length];
            for (int i = 0; i < z.Length; i += 2)
            {
                double u1 = System.Math.Max(rng.NextDouble(), 1e-300), u2 = rng.NextDouble();
                double r = System.Math.Sqrt(-2.0 * System.Math.Log(u1)), p = 2.0 * System.Math.PI * u2;
                z[i] = r * System.Math.Cos(p); if (i + 1 < z.Length) z[i + 1] = r * System.Math.Sin(p);
            }
            double max = 0;
            for (int i = 0; i < z.Length; i++)
            {
                double y = 0; for (int j = 0; j <= i; j++) y += l[i][j] * z[j];
                max = System.Math.Max(max, System.Math.Abs(y));
            }
            maxima[s] = max;
        }
        Array.Sort(maxima);
        double alpha3 = 2.0 * (1.0 - NormalCdf(3.0));
        int index = System.Math.Clamp((int)System.Math.Ceiling((1.0 - alpha3) * samples) - 1, 0, samples - 1);
        return System.Math.Max(3.0, maxima[index]);
    }

    private static double[][] CholeskyWithJitter(double[][] a)
    {
        int n = a.Length;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            double jitter = attempt == 0 ? 0.0 : System.Math.Pow(10, attempt - 12);
            var l = Enumerable.Range(0, n).Select(_ => new double[n]).ToArray(); bool ok = true;
            for (int i = 0; i < n && ok; i++) for (int j = 0; j <= i; j++)
            {
                double sum = a[i][j] + (i == j ? jitter : 0.0);
                for (int k = 0; k < j; k++) sum -= l[i][k] * l[j][k];
                if (i == j) { if (sum <= 1e-14) { ok = false; break; } l[i][j] = System.Math.Sqrt(sum); }
                else l[i][j] = sum / l[j][j];
            }
            if (ok) return l;
        }
        // Conservative fail-closed calibration fallback: independent rows.
        return Identity(n);
    }

    private static double TwoSidedPower(double threshold, double noncentrality) =>
        1.0 - NormalCdf(threshold - noncentrality) + NormalCdf(-threshold - noncentrality);

    // Abramowitz-Stegun 7.1.26; error < 7.5e-8, ample for the fixed calibration/power gates.
    private static double NormalCdf(double x)
    {
        double ax = System.Math.Abs(x), t = 1.0 / (1.0 + 0.2316419 * ax);
        double density = 0.3989422804014327 * System.Math.Exp(-0.5 * ax * ax);
        double tail = density * t * (0.319381530 + t * (-0.356563782 + t * (1.781477937 + t * (-1.821255978 + t * 1.330274429))));
        return x >= 0 ? 1.0 - tail : tail;
    }

    private static double[][] Identity(int n) => Enumerable.Range(0, n)
        .Select(i => Enumerable.Range(0, n).Select(j => i == j ? 1.0 : 0.0).ToArray()).ToArray();

    private static bool SpatialStorageValid(JsonElement phase456)
    {
        if (DN(phase456, "spatialKMinReconstructionResidual") is not double residual || residual > 1e-10) return false;
        var rows = phase456.GetProperty("perSiteSpatialCorrelators").EnumerateArray().ToArray();
        if (rows.Length != 64) return false;
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var k = row.GetProperty("k").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            if (k.Length != 3 || k.Any(x => x < 0 || x > 3) || !keys.Add(string.Join(',', k))) return false;
            if (row.GetProperty("c").GetArrayLength() != 4 || row.GetProperty("sigma").GetArrayLength() != 4) return false;
            var jk = row.GetProperty("jackknife").EnumerateArray().ToArray();
            if (jk.Length != JackknifeBlocks || jk.Any(x => x.GetArrayLength() != 4)) return false;
        }
        return keys.Count == 64;
    }
    private static string S(JsonElement e, string p) => e.GetProperty(p).GetString() ?? "";
    private static bool B(JsonElement e, string p) => e.GetProperty(p).GetBoolean();
    private static int I(JsonElement e, string p) => e.GetProperty(p).GetInt32();
    private static double D(JsonElement e, string p) => e.GetProperty(p).GetDouble();
    private static double? DN(JsonElement e, string p) => e.GetProperty(p).ValueKind == JsonValueKind.Number ? e.GetProperty(p).GetDouble() : null;
    private static double?[] NullableArray(JsonElement e, string p) => e.GetProperty(p).EnumerateArray()
        .Select(x => x.ValueKind == JsonValueKind.Number ? (double?)x.GetDouble() : null).ToArray();
    private static double?[][] NullableRows(JsonElement e, string p) => e.GetProperty(p).EnumerateArray()
        .Select(row => row.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Number ? (double?)x.GetDouble() : null).ToArray()).ToArray();

    private sealed record Estimate(double? Value, double?[] Jackknife);
    private sealed record RowDraft(string RowId, string Channel, string Observable, double? Value, double? NullValue,
        double?[] Jackknife, double? NEff);
}
