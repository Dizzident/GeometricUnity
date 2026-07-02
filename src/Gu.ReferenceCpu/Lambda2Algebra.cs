namespace Gu.ReferenceCpu;

/// <summary>
/// The self-dual / anti-self-dual invariant-element algebra on the 6-dimensional
/// exterior square Lambda^2(T*X^4) of a Euclidean (Cl(4,0)) 4D base (design §3.3,
/// physics-decisions §1/§3). This is pure, mesh-independent linear algebra: the
/// Hodge involution ∗ (with ∗^2 = +1), the real self-dual/anti-self-dual
/// projectors P_+ / P_- (each rank 3), the four invariant-element endomorphisms
/// {id0, sd2, asd2, vol4}, and the two-term Einsteinian member endomorphism
///   R = A(Phi1) ( I - c * A(Phi2) ),  or  R = A(Phi1) when Phi2 = "none".
///
/// Basis order for Lambda^2(R^4), axis indices 0..3:
///   [ e01, e02, e03, e12, e13, e23 ]  (index 0..5).
///
/// The Euclidean Hodge star (orientation dx0∧dx1∧dx2∧dx3):
///   ∗e01=+e23, ∗e02=-e13, ∗e03=+e12, ∗e12=+e03, ∗e13=-e02, ∗e23=+e01,
/// which satisfies ∗^2 = +I, giving the real 3+3 split needed for the
/// Ricci/Weyl decomposition (the capability the 2D toy provably lacks).
///
/// All matrices are dense double[dim,dim]. Small helpers (multiply, invert,
/// matrix exponential) live here so both this class and
/// <see cref="EinsteinianShiabOperator"/> share one implementation.
/// </summary>
public static class Lambda2Algebra
{
    /// <summary>Dimension of Lambda^2(R^4).</summary>
    public const int Dim = 6;

    /// <summary>
    /// The (i,j) axis pair (i&lt;j) carried by each Lambda^2 basis index 0..5.
    /// </summary>
    public static readonly (int I, int J)[] BasisPairs =
    {
        (0, 1), (0, 2), (0, 3), (1, 2), (1, 3), (2, 3),
    };

    /// <summary>
    /// Wedge of two R^4 vectors u ∧ v as a 6-component Lambda^2 coefficient vector,
    /// (u∧v)_{ij} = u_i v_j - u_j v_i in the <see cref="BasisPairs"/> order.
    /// </summary>
    public static double[] Wedge(ReadOnlySpan<double> u, ReadOnlySpan<double> v)
    {
        var w = new double[Dim];
        for (int k = 0; k < Dim; k++)
        {
            int i = BasisPairs[k].I;
            int j = BasisPairs[k].J;
            w[k] = u[i] * v[j] - u[j] * v[i];
        }
        return w;
    }

    /// <summary>The Hodge involution ∗ on Lambda^2(R^4), Euclidean signature (∗^2 = +I).</summary>
    public static double[,] HodgeStar()
    {
        var s = new double[Dim, Dim];
        // ∗e01=+e23, ∗e02=-e13, ∗e03=+e12, ∗e12=+e03, ∗e13=-e02, ∗e23=+e01
        s[5, 0] = +1; // e01 -> +e23
        s[4, 1] = -1; // e02 -> -e13
        s[3, 2] = +1; // e03 -> +e12
        s[2, 3] = +1; // e12 -> +e03
        s[1, 4] = -1; // e13 -> -e02
        s[0, 5] = +1; // e23 -> +e01
        return s;
    }

    /// <summary>Self-dual projector P_+ = (I + ∗)/2 (rank 3).</summary>
    public static double[,] SelfDualProjector() => ScaleAdd(Identity(Dim), 0.5, HodgeStar(), 0.5);

    /// <summary>Anti-self-dual projector P_- = (I - ∗)/2 (rank 3).</summary>
    public static double[,] AntiSelfDualProjector() => ScaleAdd(Identity(Dim), 0.5, HodgeStar(), -0.5);

    /// <summary>
    /// The Lambda^2 endomorphism A(Phi) realizing an invariant element:
    /// id0 -> I, sd2 -> P_+, asd2 -> P_-, vol4 -> ∗. ("none" is not an endomorphism;
    /// it is handled at the member level as term-absence.)
    /// </summary>
    public static double[,] InvariantElement(string element) => element switch
    {
        "id0" => Identity(Dim),
        "sd2" => SelfDualProjector(),
        "asd2" => AntiSelfDualProjector(),
        "vol4" => HodgeStar(),
        _ => throw new ArgumentException(
            $"'{element}' is not a Lambda^2 invariant element (expected id0|sd2|asd2|vol4).",
            nameof(element)),
    };

    /// <summary>
    /// Build the two-term Einsteinian member endomorphism on Lambda^2:
    ///   R = A(Phi1) ( I - c * A(Phi2) )       (both-terms case)
    ///   R = A(Phi1)                            (Phi2 = "none": one-term / Ricci-like only)
    /// With Phi1 = id0 and Phi2 = none this is exactly the identity on Lambda^2, so the
    /// operator reduces to identity-Shiab (the control anchor). With Phi1/Phi2 = id0/id0
    /// this is (1-c)·I — a scalar multiple, which correctly FAILS the richness certificate.
    /// </summary>
    public static double[,] MemberEndomorphism(EinsteinianShiabFamilyMember member)
    {
        var a1 = InvariantElement(member.Phi1.InvariantElement);
        if (member.Phi2.InvariantElement == "none")
            return a1;

        var a2 = InvariantElement(member.Phi2.InvariantElement);
        // inner = I - c * A(Phi2)
        var inner = ScaleAdd(Identity(Dim), 1.0, a2, -member.EinsteinCoefficient);
        return Multiply(a1, inner);
    }

    /// <summary>
    /// Off-scalar (off-identity) deviation of a 6x6 endomorphism: the Frobenius norm
    /// of R - (trace(R)/Dim)·I. Zero iff R is a scalar multiple of the identity.
    /// This is the richness certificate on Lambda^2 (design §3.7 / physics §6e battery 1):
    /// a member is "genuinely rich" iff this exceeds a floor. The control (R scalar) is 0.
    /// </summary>
    public static double ScalarDeviation(double[,] r)
    {
        int n = r.GetLength(0);
        double trace = 0;
        for (int i = 0; i < n; i++) trace += r[i, i];
        double mean = trace / n;

        double sum = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double v = r[i, j] - (i == j ? mean : 0.0);
                sum += v * v;
            }
        return System.Math.Sqrt(sum);
    }

    // ------------------------------------------------------------------
    // Small dense-matrix helpers (shared with EinsteinianShiabOperator).
    // ------------------------------------------------------------------

    /// <summary>n x n identity matrix.</summary>
    public static double[,] Identity(int n)
    {
        var m = new double[n, n];
        for (int i = 0; i < n; i++) m[i, i] = 1.0;
        return m;
    }

    /// <summary>alpha*a + beta*b for equally shaped matrices.</summary>
    public static double[,] ScaleAdd(double[,] a, double alpha, double[,] b, double beta)
    {
        int r = a.GetLength(0), c = a.GetLength(1);
        var m = new double[r, c];
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                m[i, j] = alpha * a[i, j] + beta * b[i, j];
        return m;
    }

    /// <summary>Matrix product a (r x k) * b (k x c).</summary>
    public static double[,] Multiply(double[,] a, double[,] b)
    {
        int r = a.GetLength(0), k = a.GetLength(1), c = b.GetLength(1);
        if (b.GetLength(0) != k)
            throw new ArgumentException("Inner dimensions do not agree.");
        var m = new double[r, c];
        for (int i = 0; i < r; i++)
            for (int p = 0; p < k; p++)
            {
                double aip = a[i, p];
                if (aip == 0.0) continue;
                for (int j = 0; j < c; j++)
                    m[i, j] += aip * b[p, j];
            }
        return m;
    }

    /// <summary>Transpose.</summary>
    public static double[,] Transpose(double[,] a)
    {
        int r = a.GetLength(0), c = a.GetLength(1);
        var m = new double[c, r];
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                m[j, i] = a[i, j];
        return m;
    }

    /// <summary>
    /// Inverse of a small square matrix via Gauss-Jordan elimination with partial
    /// pivoting. Throws if singular (below <paramref name="pivotFloor"/>).
    /// </summary>
    public static double[,] Invert(double[,] a, double pivotFloor = 1e-12)
    {
        int n = a.GetLength(0);
        if (a.GetLength(1) != n) throw new ArgumentException("Matrix must be square.");

        var m = (double[,])a.Clone();
        var inv = Identity(n);

        for (int col = 0; col < n; col++)
        {
            // Partial pivot.
            int pivot = col;
            double best = System.Math.Abs(m[col, col]);
            for (int r = col + 1; r < n; r++)
            {
                double v = System.Math.Abs(m[r, col]);
                if (v > best) { best = v; pivot = r; }
            }
            if (best < pivotFloor)
                throw new InvalidOperationException(
                    $"Matrix is singular (pivot {best:G3} below floor {pivotFloor:G3}).");

            if (pivot != col)
            {
                SwapRows(m, col, pivot);
                SwapRows(inv, col, pivot);
            }

            double diag = m[col, col];
            for (int j = 0; j < n; j++) { m[col, j] /= diag; inv[col, j] /= diag; }

            for (int r = 0; r < n; r++)
            {
                if (r == col) continue;
                double factor = m[r, col];
                if (factor == 0.0) continue;
                for (int j = 0; j < n; j++)
                {
                    m[r, j] -= factor * m[col, j];
                    inv[r, j] -= factor * inv[col, j];
                }
            }
        }
        return inv;
    }

    /// <summary>
    /// Matrix exponential exp(a) via scaling-and-squaring with a truncated Taylor
    /// series. Used to build the ad-conjugator Ad_eps = exp(ad_theta) on the
    /// Lie-algebra index (physics-decisions §6e).
    /// </summary>
    public static double[,] MatrixExp(double[,] a, int taylorTerms = 16)
    {
        int n = a.GetLength(0);

        // Scale so ||a/2^s||_inf < 0.5 for fast, accurate Taylor convergence.
        double norm = InfinityNorm(a);
        int s = 0;
        double scaled = norm;
        while (scaled > 0.5) { scaled *= 0.5; s++; }
        double factor = System.Math.Pow(2.0, -s);
        var b = ScaleAdd(a, factor, a, 0.0); // = factor * a

        // Taylor: exp(b) = sum_{k>=0} b^k / k!
        var result = Identity(n);
        var term = Identity(n);
        for (int k = 1; k <= taylorTerms; k++)
        {
            term = Multiply(term, b);
            double invFact = 1.0 / Factorial(k);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    result[i, j] += term[i, j] * invFact;
        }

        // Square s times.
        for (int q = 0; q < s; q++) result = Multiply(result, result);
        return result;
    }

    private static double InfinityNorm(double[,] a)
    {
        int r = a.GetLength(0), c = a.GetLength(1);
        double max = 0;
        for (int i = 0; i < r; i++)
        {
            double rowSum = 0;
            for (int j = 0; j < c; j++) rowSum += System.Math.Abs(a[i, j]);
            if (rowSum > max) max = rowSum;
        }
        return max;
    }

    private static double Factorial(int k)
    {
        double f = 1;
        for (int i = 2; i <= k; i++) f *= i;
        return f;
    }

    private static void SwapRows(double[,] m, int r1, int r2)
    {
        int cols = m.GetLength(1);
        for (int j = 0; j < cols; j++)
        {
            (m[r1, j], m[r2, j]) = (m[r2, j], m[r1, j]);
        }
    }
}
