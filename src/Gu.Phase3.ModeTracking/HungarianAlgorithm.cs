namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Hungarian algorithm (Kuhn-Munkres) for the assignment problem.
/// Given an n x m cost matrix, finds the minimum-cost one-to-one assignment
/// in O(n³) time using the classical Munkres procedure.
///
/// For mode matching, we negate the score matrix to find the maximum-score assignment.
/// </summary>
internal static class HungarianAlgorithm
{
    /// <summary>
    /// Solve the assignment problem: minimize total cost.
    /// Handles rectangular matrices (m x n where m != n) by padding to square.
    /// Returns assignment[i] = column assigned to row i, or -1 if unassigned
    /// (only possible if original matrix had fewer columns than rows).
    /// </summary>
    public static int[] Solve(double[,] costMatrix)
    {
        int rows = costMatrix.GetLength(0);
        int cols = costMatrix.GetLength(1);
        int size = System.Math.Max(rows, cols);

        // Pad to square with large value (effectively infinity) for rectangular inputs.
        // Using the actual max cost + 1 as the padding value ensures padding assignments
        // are never preferred over real assignments.
        double padValue = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                if (costMatrix[i, j] > padValue) padValue = costMatrix[i, j];
        padValue = padValue * size + 1;

        var cost = new double[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                cost[i, j] = (i < rows && j < cols) ? costMatrix[i, j] : padValue;

        // --- Munkres / Hungarian algorithm ---
        // u[i]: row potential; v[j]: column potential
        var u = new double[size + 1];
        var v = new double[size + 1];
        // p[j]: which row is matched to column j (1-indexed; 0 = unmatched)
        var p = new int[size + 1];
        // way[j]: which column j came from in the augmentation path
        var way = new int[size + 1];

        // Standard O(n³) implementation using the potential/augmentation approach
        // (Jonker & Volgenant style, 1-indexed internally).
        for (int i = 1; i <= size; i++)
        {
            // Add row i to the matching
            p[0] = i;
            int j0 = 0;

            var minVal = new double[size + 1];
            var used = new bool[size + 1];

            for (int j = 0; j <= size; j++)
                minVal[j] = double.MaxValue;

            do
            {
                used[j0] = true;
                int i0 = p[j0];
                double delta = double.MaxValue;
                int j1 = -1;

                for (int j = 1; j <= size; j++)
                {
                    if (!used[j])
                    {
                        double cur = cost[i0 - 1, j - 1] - u[i0] - v[j];
                        if (cur < minVal[j])
                        {
                            minVal[j] = cur;
                            way[j] = j0;
                        }
                        if (minVal[j] < delta)
                        {
                            delta = minVal[j];
                            j1 = j;
                        }
                    }
                }

                for (int j = 0; j <= size; j++)
                {
                    if (used[j])
                    {
                        u[p[j]] += delta;
                        v[j] -= delta;
                    }
                    else
                    {
                        minVal[j] -= delta;
                    }
                }

                j0 = j1;
            } while (p[j0] != 0);

            // Augment along the path
            do
            {
                int j1 = way[j0];
                p[j0] = p[j1];
                j0 = j1;
            } while (j0 != 0);
        }

        // Extract assignment: for each original row, find its matched column
        // p[j] = row (1-indexed) assigned to column j
        var rowToCol = new int[size + 1]; // 1-indexed
        for (int j = 1; j <= size; j++)
            if (p[j] != 0) rowToCol[p[j]] = j;

        var result = new int[rows];
        for (int i = 0; i < rows; i++)
        {
            int col = rowToCol[i + 1] - 1; // convert to 0-indexed
            // If assigned to a padding column, mark as unassigned
            result[i] = col < cols ? col : -1;
        }
        return result;
    }

    /// <summary>
    /// Solve the maximization variant: maximize total score.
    /// Negates the score matrix and delegates to Solve().
    /// </summary>
    public static int[] SolveMaximization(double[,] scoreMatrix)
    {
        int rows = scoreMatrix.GetLength(0);
        int cols = scoreMatrix.GetLength(1);

        // Find max to construct cost = max - score (all non-negative)
        double maxVal = double.MinValue;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                if (scoreMatrix[i, j] > maxVal) maxVal = scoreMatrix[i, j];

        var cost = new double[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                cost[i, j] = maxVal - scoreMatrix[i, j];

        return Solve(cost);
    }
}
