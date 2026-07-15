using System.Numerics;

internal static class CompositeTransportAnalysis
{
    internal static CompositeTransportResult Run()
    {
        // Every finite-support ensemble below is invariant under independent
        // sign flips.  Consequently E[X] = E[Y] = E[XY] = 0 exactly.  Only
        // the coupling between the magnitudes differs:
        //
        //   positive: |Y| = |X|,
        //   zero:     |X| and |Y| independent,
        //   negative: |Y| = 3 - |X|.
        //
        // In all three cases |X| and |Y| are uniform on {1, 2}, so their
        // complete two-point matrices agree exactly.
        var positive = EvaluateFiniteEnsemble(
            "shared-magnitude-positive-composite",
            MagnitudePairs((1, 1), (2, 2)));
        var zero = EvaluateFiniteEnsemble(
            "independent-magnitudes-zero-composite",
            MagnitudePairs((1, 1), (1, 2), (2, 1), (2, 2)));
        var negative = EvaluateFiniteEnsemble(
            "complementary-magnitudes-negative-composite",
            MagnitudePairs((1, 2), (2, 1)));

        var expectedSecondMoment = new Rational(5, 2);
        bool exactTwoPointMatch = new[] { positive, zero, negative }.All(row =>
            row.MeanX == Rational.Zero &&
            row.MeanY == Rational.Zero &&
            row.SecondMomentX == expectedSecondMoment &&
            row.SecondMomentY == expectedSecondMoment &&
            row.CrossMoment == Rational.Zero);
        bool exactCompositeSeparation =
            positive.ConnectedSquareCorrelation == new Rational(9, 4) &&
            zero.ConnectedSquareCorrelation == Rational.Zero &&
            negative.ConnectedSquareCorrelation == new Rational(-9, 4);
        bool independentSignFlipSymmetryExact =
            positive.IndependentSignFlipSymmetryExact &&
            zero.IndependentSignFlipSymmetryExact &&
            negative.IndependentSignFlipSymmetryExact;

        // Positive control: for a centered jointly Gaussian pair, Wick's
        // identity gives Cov(X^2,Y^2) = 2 Cov(X,Y)^2.  The rational covariance
        // matrix [[2,1],[1,3]] is positive definite (determinant 5), and the
        // identity therefore predicts the exact connected value 2.  This is
        // an analytic identity check, not a claim that the committed
        // simplicial measure is Gaussian.
        var gaussianVarX = new Rational(2);
        var gaussianVarY = new Rational(3);
        var gaussianCovariance = Rational.One;
        var gaussianDeterminant = gaussianVarX * gaussianVarY - gaussianCovariance * gaussianCovariance;
        var gaussianConnectedByWick = new Rational(2) * gaussianCovariance * gaussianCovariance;
        bool gaussianPositiveControlPassed =
            gaussianDeterminant == new Rational(5) &&
            gaussianConnectedByWick == new Rational(2);

        bool finiteSupportBatteryPassed = exactTwoPointMatch && exactCompositeSeparation &&
            independentSignFlipSymmetryExact;
        bool scalarCounterexampleEmbedsInMatrixField = finiteSupportBatteryPassed;
        bool genericTwoPointTransportRefuted = finiteSupportBatteryPassed && scalarCounterexampleEmbedsInMatrixField;
        bool inputsValid = finiteSupportBatteryPassed && gaussianPositiveControlPassed;

        return new CompositeTransportResult
        {
            InputsValid = inputsValid,
            ArithmeticKind = "exact-integer-and-rational",
            CounterexampleRows = new[] { positive.ToOutput(), zero.ToOutput(), negative.ToOutput() },
            CommonTwoPointMatrix = new[]
            {
                new[] { expectedSecondMoment.ToString(), Rational.Zero.ToString() },
                new[] { Rational.Zero.ToString(), expectedSecondMoment.ToString() },
            },
            IndependentSignFlipSymmetryExact = independentSignFlipSymmetryExact,
            ExactTwoPointMatricesMatch = exactTwoPointMatch,
            ExactCompositeSeparation = exactCompositeSeparation,
            FiniteSupportCounterexampleBatteryPassed = finiteSupportBatteryPassed,
            ScalarCounterexampleEmbedsInMatrixField = scalarCounterexampleEmbedsInMatrixField,
            EmbeddingStatement = "Set the matrix-valued field to X times any fixed nonzero generator at one site and Y times that generator at the other. A universal implication valid for every matrix-valued field would restrict to this scalar rank-one subspace, where the exact counterexample applies.",
            GenericTwoPointToConnectedCompositeTransportRefuted = genericTwoPointTransportRefuted,
            GaussianPositiveControlPassed = gaussianPositiveControlPassed,
            GaussianPositiveControl = new GaussianPositiveControlOutput
            {
                VarianceX = gaussianVarX.ToString(),
                VarianceY = gaussianVarY.ToString(),
                Covariance = gaussianCovariance.ToString(),
                CovarianceMatrixDeterminant = gaussianDeterminant.ToString(),
                ConnectedSquareCorrelationByWick = gaussianConnectedByWick.ToString(),
                Identity = "For a centered jointly Gaussian pair, Cov(X^2,Y^2)=2*Cov(X,Y)^2.",
            },
            StrongGeneratingFunctionDominationTested = false,
            ActionSpecificWardIdentityTested = false,
            ActionSpecificCompositeTheoremProved = false,
            CommittedActionRefuted = false,
            ScopeStatement = "The exact witnesses refute transport based only on field means and two-point functions, including any universal zero, sign, or value inference for the connected square channel. They do not refute stronger generating-function domination, an action-specific Ward identity, or a separately proved four-point inequality.",
            ObstructionSurvives = genericTwoPointTransportRefuted,
            ClosesLimbL8 = false,
            AuthorizesSampling = false,
            TerminalImplication = inputsValid && genericTwoPointTransportRefuted
                ? "composite-transport-obstruction-survives"
                : "composite-transport-analysis-invalid",
            OverallPhase482Recommendation = inputsValid && genericTwoPointTransportRefuted
                ? "obstructions-survive-no-theorem"
                : "invalid-proof-package",
            ProofGateCountImplemented = 5,
            ProofGateCountPassed = new[]
            {
                exactTwoPointMatch,
                exactCompositeSeparation,
                independentSignFlipSymmetryExact,
                scalarCounterexampleEmbedsInMatrixField,
                gaussianPositiveControlPassed,
            }.Count(value => value),
        };
    }

    private static IReadOnlyList<SupportPoint> MagnitudePairs(params (int X, int Y)[] magnitudePairs)
    {
        var points = new List<SupportPoint>();
        foreach (var (magnitudeX, magnitudeY) in magnitudePairs)
        {
            foreach (int signX in new[] { -1, 1 })
            {
                foreach (int signY in new[] { -1, 1 })
                {
                    points.Add(new SupportPoint(signX * magnitudeX, signY * magnitudeY));
                }
            }
        }

        return points;
    }

    private static ExactEnsembleResult EvaluateFiniteEnsemble(string ensembleId, IReadOnlyList<SupportPoint> points)
    {
        Rational Average(Func<SupportPoint, BigInteger> observable) =>
            new(points.Aggregate(BigInteger.Zero, (sum, point) => sum + observable(point)), points.Count);

        var meanX = Average(point => point.X);
        var meanY = Average(point => point.Y);
        var secondMomentX = Average(point => point.X * point.X);
        var secondMomentY = Average(point => point.Y * point.Y);
        var crossMoment = Average(point => point.X * point.Y);
        var squareProductMoment = Average(point => point.X * point.X * point.Y * point.Y);
        var connectedSquareCorrelation = squareProductMoment - secondMomentX * secondMomentY;

        var support = points.Select(point => (point.X, point.Y)).ToHashSet();
        bool independentSignFlipSymmetryExact = points.All(point =>
            support.Contains((-point.X, point.Y)) &&
            support.Contains((point.X, -point.Y)) &&
            support.Contains((-point.X, -point.Y)));

        return new ExactEnsembleResult
        {
            EnsembleId = ensembleId,
            SupportPointCount = points.Count,
            MeanX = meanX,
            MeanY = meanY,
            SecondMomentX = secondMomentX,
            SecondMomentY = secondMomentY,
            CrossMoment = crossMoment,
            SquareProductMoment = squareProductMoment,
            ConnectedSquareCorrelation = connectedSquareCorrelation,
            IndependentSignFlipSymmetryExact = independentSignFlipSymmetryExact,
        };
    }

    private readonly struct SupportPoint
    {
        internal SupportPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal BigInteger X { get; }
        internal BigInteger Y { get; }
    }

    private readonly struct Rational : IEquatable<Rational>
    {
        internal static Rational Zero => new(BigInteger.Zero);
        internal static Rational One => new(BigInteger.One);

        internal Rational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero)
            {
                throw new DivideByZeroException("An exact rational denominator cannot be zero.");
            }

            if (denominator.Sign < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
        }

        internal Rational(BigInteger integer) : this(integer, BigInteger.One)
        {
        }

        public static Rational operator +(Rational left, Rational right) =>
            new(left.Numerator * right.Denominator + right.Numerator * left.Denominator,
                left.Denominator * right.Denominator);

        public static Rational operator -(Rational left, Rational right) =>
            new(left.Numerator * right.Denominator - right.Numerator * left.Denominator,
                left.Denominator * right.Denominator);

        public static Rational operator *(Rational left, Rational right) =>
            new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

        public static bool operator ==(Rational left, Rational right) => left.Equals(right);
        public static bool operator !=(Rational left, Rational right) => !left.Equals(right);

        public bool Equals(Rational other) => Numerator == other.Numerator && Denominator == other.Denominator;
        public override bool Equals(object? obj) => obj is Rational other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
        public override string ToString() => Denominator == BigInteger.One ? Numerator.ToString() : $"{Numerator}/{Denominator}";

        private BigInteger Numerator { get; }
        private BigInteger Denominator { get; }
    }

    private sealed class ExactEnsembleResult
    {
        internal required string EnsembleId { get; init; }
        internal required int SupportPointCount { get; init; }
        internal required Rational MeanX { get; init; }
        internal required Rational MeanY { get; init; }
        internal required Rational SecondMomentX { get; init; }
        internal required Rational SecondMomentY { get; init; }
        internal required Rational CrossMoment { get; init; }
        internal required Rational SquareProductMoment { get; init; }
        internal required Rational ConnectedSquareCorrelation { get; init; }
        internal required bool IndependentSignFlipSymmetryExact { get; init; }

        internal CompositeCounterexampleOutput ToOutput() => new()
        {
            EnsembleId = EnsembleId,
            SupportPointCount = SupportPointCount,
            MeanX = MeanX.ToString(),
            MeanY = MeanY.ToString(),
            SecondMomentX = SecondMomentX.ToString(),
            SecondMomentY = SecondMomentY.ToString(),
            CrossMoment = CrossMoment.ToString(),
            SquareProductMoment = SquareProductMoment.ToString(),
            ConnectedSquareCorrelation = ConnectedSquareCorrelation.ToString(),
            IndependentSignFlipSymmetryExact = IndependentSignFlipSymmetryExact,
        };
    }
}

internal sealed class CompositeTransportResult
{
    public required bool InputsValid { get; init; }
    public required string ArithmeticKind { get; init; }
    public required CompositeCounterexampleOutput[] CounterexampleRows { get; init; }
    public required string[][] CommonTwoPointMatrix { get; init; }
    public required bool IndependentSignFlipSymmetryExact { get; init; }
    public required bool ExactTwoPointMatricesMatch { get; init; }
    public required bool ExactCompositeSeparation { get; init; }
    public required bool FiniteSupportCounterexampleBatteryPassed { get; init; }
    public required bool ScalarCounterexampleEmbedsInMatrixField { get; init; }
    public required string EmbeddingStatement { get; init; }
    public required bool GenericTwoPointToConnectedCompositeTransportRefuted { get; init; }
    public required bool GaussianPositiveControlPassed { get; init; }
    public required GaussianPositiveControlOutput GaussianPositiveControl { get; init; }
    public required bool StrongGeneratingFunctionDominationTested { get; init; }
    public required bool ActionSpecificWardIdentityTested { get; init; }
    public required bool ActionSpecificCompositeTheoremProved { get; init; }
    public required bool CommittedActionRefuted { get; init; }
    public required string ScopeStatement { get; init; }
    public required bool ObstructionSurvives { get; init; }
    public required bool ClosesLimbL8 { get; init; }
    public required bool AuthorizesSampling { get; init; }
    public required string TerminalImplication { get; init; }
    public required string OverallPhase482Recommendation { get; init; }
    public required int ProofGateCountImplemented { get; init; }
    public required int ProofGateCountPassed { get; init; }
}

internal sealed class CompositeCounterexampleOutput
{
    public required string EnsembleId { get; init; }
    public required int SupportPointCount { get; init; }
    public required string MeanX { get; init; }
    public required string MeanY { get; init; }
    public required string SecondMomentX { get; init; }
    public required string SecondMomentY { get; init; }
    public required string CrossMoment { get; init; }
    public required string SquareProductMoment { get; init; }
    public required string ConnectedSquareCorrelation { get; init; }
    public required bool IndependentSignFlipSymmetryExact { get; init; }
}

internal sealed class GaussianPositiveControlOutput
{
    public required string VarianceX { get; init; }
    public required string VarianceY { get; init; }
    public required string Covariance { get; init; }
    public required string CovarianceMatrixDeterminant { get; init; }
    public required string ConnectedSquareCorrelationByWick { get; init; }
    public required string Identity { get; init; }
}
