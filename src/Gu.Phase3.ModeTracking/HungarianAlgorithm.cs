namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Hungarian algorithm for the assignment problem.
/// Given an n x m cost matrix, finds the minimum-cost one-to-one assignment.
///
/// For mode matching, we negate the score matrix to find the maximum-score assignment.
/// </summary>
internal static class HungarianAlgorithm
{
    /// <summary>
    /// Solve the assignment problem: minimize total cost for a square cost matrix.
    /// Returns assignment[i] = column assigned to row i, or -1 if unassigned.
    /// </summary>
    public static int[] Solve(double[,] costMatrix)
    {
        int n = costMatrix.GetLength(0);
        int m = costMatrix.GetLength(1);
        int size = System.Math.Max(n, m);

        // Pad to square if needed
        var cost = new double[size, size];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                cost[i, j] = costMatrix[i, j];

        // Step 1: Subtract row minima
        for (int i = 0; i < size; i++)
        {
            double rowMin = double.MaxValue;
            for (int j = 0; j < size; j++)
                if (cost[i, j] < rowMin) rowMin = cost[i, j];
            for (int j = 0; j < size; j++)
                cost[i, j] -= rowMin;
        }

        // Step 2: Subtract column minima
        for (int j = 0; j < size; j++)
        {
            double colMin = double.MaxValue;
            for (int i = 0; i < size; i++)
                if (cost[i, j] < colMin) colMin = cost[i, j];
            for (int i = 0; i < size; i++)
                cost[i, j] -= colMin;
        }

        // Simple greedy assignment for small matrices (adequate for toy problems)
        var rowAssign = new int[size];
        var colAssign = new int[size];
        Array.Fill(rowAssign, -1);
        Array.Fill(colAssign, -1);

        // Iterative augmenting path approach
        for (int iter = 0; iter < size * size; iter++)
        {
            // Find unassigned row with a zero
            bool found = false;
            for (int i = 0; i < size; i++)
            {
                if (rowAssign[i] != -1) continue;
                for (int j = 0; j < size; j++)
                {
                    if (colAssign[j] != -1) continue;
                    if (cost[i, j] < 1e-12)
                    {
                        rowAssign[i] = j;
                        colAssign[j] = i;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            if (!found)
            {
                // Check if complete
                bool complete = true;
                for (int i = 0; i < size; i++)
                {
                    if (rowAssign[i] == -1) { complete = false; break; }
                }
                if (complete) break;

                // Find minimum uncovered element and adjust
                double minUncovered = double.MaxValue;
                var rowCovered = new bool[size];
                var colCovered = new bool[size];

                // Cover columns that have assignments
                for (int j = 0; j < size; j++)
                    colCovered[j] = colAssign[j] != -1;

                // Find uncovered zeros and adjust
                for (int i = 0; i < size; i++)
                {
                    if (rowAssign[i] != -1) continue; // unassigned row
                    for (int j = 0; j < size; j++)
                    {
                        if (colCovered[j]) continue;
                        if (cost[i, j] < minUncovered)
                            minUncovered = cost[i, j];
                    }
                }

                if (minUncovered >= double.MaxValue * 0.5) break;

                // Subtract from uncovered rows, add to covered columns
                for (int i = 0; i < size; i++)
                {
                    if (rowAssign[i] != -1) continue;
                    for (int j = 0; j < size; j++)
                        cost[i, j] -= minUncovered;
                }
                for (int j = 0; j < size; j++)
                {
                    if (!colCovered[j]) continue;
                    for (int i = 0; i < size; i++)
                        cost[i, j] += minUncovered;
                }
            }
        }

        // Return only valid assignments (within original dimensions)
        var result = new int[n];
        for (int i = 0; i < n; i++)
            result[i] = rowAssign[i] < m ? rowAssign[i] : -1;
        return result;
    }
}
