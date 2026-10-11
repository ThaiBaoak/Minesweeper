using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AISolve
    {
        public AIActionData NextMove(BoardInfo boardInfo)
        {
            if(sovleBasic(boardInfo, out AIActionData actionData))
            {
                return actionData;
            }
            if(solveGroup(boardInfo, out actionData))
            {
                return actionData;
            }

            if(solveEndGame(boardInfo, out actionData))
            {
                return actionData;
            }
            if(solveProbability(boardInfo, out actionData))
            {
                return actionData;
            }

            return new AIActionData(-1, -1, AIAction.Reveal);

        }

        private List<Vector2Int> GetNeighbors(BoardInfo boardInfo, int x, int y, int Type)    
        {
            List<Vector2Int> result = new List<Vector2Int>();
            for(int dx = -1; dx <= 1; dx++)
            {
                for(int dy = -1; dy <= 1; dy++)
                {
                    if(dx == 0 && dy == 0)
                    {
                        continue;
                    }
                    int nx = x + dx;
                    int ny = y + dy;
                    if(nx >= 0 && nx < boardInfo.width && ny >= 0 && ny < boardInfo.height)
                    {
                        if(boardInfo.CellStates[nx, ny] == Type)
                        {
                            result.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
            return result;
        }

        private bool sovleBasic(BoardInfo boardInfo, out AIActionData actionData)
        {
            for(int x=0; x < boardInfo.width; x++)
            {
                for(int y = 0; y < boardInfo.height; y++)
                {
                    int val = boardInfo.CellStates[x, y];
                    if(val <= 0)
                    {
                        continue;
                    }

                    List<Vector2Int> hiddenNeighbors = GetNeighbors(boardInfo, x, y, -1);
                    List<Vector2Int> flaggedNeighbors = GetNeighbors(boardInfo, x, y, -2);

                    int remainingMines = val - flaggedNeighbors.Count;
                    if(remainingMines > 0 && hiddenNeighbors.Count == remainingMines)
                    {
                        // Flag first hidden neighbors
                        Vector2Int neighbor = hiddenNeighbors[0];
                        actionData = new AIActionData(neighbor.x, neighbor.y, AIAction.Flag);
                        return true;
                    }
                    else if(remainingMines == 0 && hiddenNeighbors.Count > 0)
                    {
                        // Reveal first hidden neighbors
                        Vector2Int neighbor = hiddenNeighbors[0];
                        actionData = new AIActionData(neighbor.x, neighbor.y, AIAction.Reveal);
                        return true;
                    }
                }
            }
            actionData = new AIActionData(-1, -1, AIAction.Reveal);
            return false;
        }
        private bool solveGroup(BoardInfo boardInfo, out AIActionData actionData)
        {
            List<Vector2Int> activeCells = new List<Vector2Int>();
            for(int x=0; x < boardInfo.width; x++)
            {
                for(int y = 0; y < boardInfo.height; y++)
                {
                    if(boardInfo.CellStates[x, y] > 0 && GetNeighbors(boardInfo, x, y, -1).Count > 0)
                    {
                        activeCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            foreach(Vector2Int cell in activeCells)
            {
                var hiddenA =GetNeighbors(boardInfo, cell.x, cell.y, -1);
                int flagsA = GetNeighbors(boardInfo, cell.x, cell.y, -2).Count;
                int remainingMinesA = boardInfo.CellStates[cell.x, cell.y] - flagsA;
                HashSet<Vector2Int> hiddenSetA = new HashSet<Vector2Int>(hiddenA);

                for(int dx = -2; dx <= 2; dx++)
                {
                    for(int dy = -2; dy <= 2; dy++)
                    {
                        if(dx == 0 && dy == 0)
                        {
                            continue;
                        }
                        int nx = cell.x + dx;
                        int ny = cell.y + dy;
                        if(nx <0 || nx >= boardInfo.width || ny < 0 || ny >= boardInfo.height){continue;}
                        if(boardInfo.CellStates[nx, ny] <= 0){continue;}

                        var hiddenB = GetNeighbors(boardInfo, nx, ny, -1);
                        if(hiddenB.Count <= hiddenSetA.Count){continue;}

                        HashSet<Vector2Int> hiddenSetB = new HashSet<Vector2Int>(hiddenB);

                        if(hiddenSetA.IsSubsetOf(hiddenSetB))
                        {
                            int flagsB = GetNeighbors(boardInfo, nx, ny, -2).Count;
                            int remainingMinesB = boardInfo.CellStates[nx, ny] - flagsB;

                            if(remainingMinesB - remainingMinesA == hiddenSetB.Count - hiddenSetA.Count)
                            {
                                // Flag the difference
                                foreach(var pos in hiddenSetB)
                                {
                                    if(!hiddenSetA.Contains(pos))
                                    {
                                        actionData = new AIActionData(pos.x, pos.y, AIAction.Flag);
                                        return true;
                                    }
                                }
                            }
                            else if(remainingMinesA == remainingMinesB)
                            {
                                // Reveal the difference
                                foreach(var pos in hiddenSetB)
                                {
                                    if(!hiddenSetA.Contains(pos))
                                    {
                                        actionData = new AIActionData(pos.x, pos.y, AIAction.Reveal);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            actionData = new AIActionData(-1, -1, AIAction.Reveal);
            return false;
        }

        private bool solveEndGame(BoardInfo boardInfo, out AIActionData actionData)
        {
            int totalMines = boardInfo.mineCount;
            int totalFlags = 0;
            List<Vector2Int> allHiddenCells = new List<Vector2Int>();

            for (int x = 0; x < boardInfo.width; x++)
            {
                for (int y = 0; y < boardInfo.height; y++)
                {
                    if (boardInfo.CellStates[x, y] == -2) totalFlags++;
                    else if (boardInfo.CellStates[x, y] == -1) allHiddenCells.Add(new Vector2Int(x, y));
                }
            }
            int remainingMinesToFind = totalMines - totalFlags;

            //Đã cắm đủ cờ -> mở toàn bộ ô ẩn còn lại
            if (remainingMinesToFind == 0 && allHiddenCells.Count > 0)
            {
                actionData = new AIActionData(allHiddenCells[0].x, allHiddenCells[0].y, AIAction.Reveal);
                return true;
            }

            //Số ô ẩn còn lại ĐÚNG BẰNG số mìn còn thiếu -> Đặt cờ tất cả ô ẩn còn lại
            if (remainingMinesToFind > 0 && allHiddenCells.Count == remainingMinesToFind)
            {
                actionData = new AIActionData(allHiddenCells[0].x, allHiddenCells[0].y, AIAction.Flag);
                return true;
            }
            actionData = new AIActionData(-1, -1, AIAction.Reveal);
            return false;
        }

        private sealed class ProbabilityConstraint
        {
            public readonly List<int> variables = new List<int>();
            public readonly int mineCount;
            public int assignedMines;
            public int unassigned;

            public ProbabilityConstraint(int mineCount)
            {
                this.mineCount = mineCount;
            }
        }

        private sealed class ProbabilitySearch
        {
            private const int NodeLimit = 1000000;
            private readonly List<ProbabilityConstraint> constraints;
            private readonly List<int>[] variableConstraints;
            private readonly int[] variableOrder;
            private readonly int[] assignments;
            private readonly double[] waysByMineCount;
            private readonly double[][] mineWaysByCount;
            private readonly int minimumMines;
            private readonly int maximumMines;
            private int visitedNodes;

            public bool LimitReached { get; private set; }
            public double[] WaysByMineCount => waysByMineCount;
            public double[][] MineWaysByCount => mineWaysByCount;

            public ProbabilitySearch(
                List<ProbabilityConstraint> constraints,
                int variableCount,
                int minimumMines,
                int maximumMines)
            {
                this.constraints = constraints;
                this.minimumMines = minimumMines;
                this.maximumMines = maximumMines;
                variableConstraints = new List<int>[variableCount];
                assignments = new int[variableCount];
                variableOrder = new int[variableCount];
                waysByMineCount = new double[variableCount + 1];
                mineWaysByCount = new double[variableCount + 1][];

                for (int i = 0; i < variableCount; i++)
                {
                    variableConstraints[i] = new List<int>();
                    variableOrder[i] = i;
                    mineWaysByCount[i] = new double[variableCount];
                }
                mineWaysByCount[variableCount] = new double[variableCount];

                for (int constraintIndex = 0; constraintIndex < constraints.Count; constraintIndex++)
                {
                    ProbabilityConstraint constraint = constraints[constraintIndex];
                    constraint.unassigned = constraint.variables.Count;
                    foreach (int variable in constraint.variables)
                    {
                        variableConstraints[variable].Add(constraintIndex);
                    }
                }

                System.Array.Sort(variableOrder, (a, b) =>
                    variableConstraints[b].Count.CompareTo(variableConstraints[a].Count));
            }

            public void Run()
            {
                foreach (ProbabilityConstraint constraint in constraints)
                {
                    if (constraint.mineCount < 0 || constraint.mineCount > constraint.variables.Count)
                    {
                        return;
                    }
                }
                Search(0, 0);
            }

            private void Search(int depth, int assignedMines)
            {
                if (LimitReached) return;
                if (++visitedNodes > NodeLimit)
                {
                    LimitReached = true;
                    return;
                }

                if (depth == variableOrder.Length)
                {
                    waysByMineCount[assignedMines]++;
                    for (int variable = 0; variable < assignments.Length; variable++)
                    {
                        if (assignments[variable] == 1)
                        {
                            mineWaysByCount[assignedMines][variable]++;
                        }
                    }
                    return;
                }

                int variableIndex = variableOrder[depth];
                for (int value = 0; value <= 1; value++)
                {
                    assignments[variableIndex] = value;
                    bool valid = true;

                    foreach (int constraintIndex in variableConstraints[variableIndex])
                    {
                        ProbabilityConstraint constraint = constraints[constraintIndex];
                        constraint.unassigned--;
                        constraint.assignedMines += value;
                        if (constraint.assignedMines > constraint.mineCount ||
                            constraint.assignedMines + constraint.unassigned < constraint.mineCount)
                        {
                            valid = false;
                        }
                    }

                    int unassignedVariables = variableOrder.Length - depth - 1;
                    int nextMineCount = assignedMines + value;
                    if (valid &&
                        nextMineCount <= maximumMines &&
                        nextMineCount + unassignedVariables >= minimumMines)
                    {
                        Search(depth + 1, nextMineCount);
                    }

                    foreach (int constraintIndex in variableConstraints[variableIndex])
                    {
                        ProbabilityConstraint constraint = constraints[constraintIndex];
                        constraint.assignedMines -= value;
                        constraint.unassigned++;
                    }

                    if (LimitReached) return;
                }
            }
        }

        private bool solveProbability(BoardInfo boardInfo, out AIActionData actionData)
        {
            List<Vector2Int> frontier = new List<Vector2Int>();
            List<Vector2Int> nonFrontier = new List<Vector2Int>();
            List<Vector2Int> allHidden = new List<Vector2Int>();
            HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();
            List<ProbabilityConstraint> constraints = new List<ProbabilityConstraint>();
            int totalFlags = 0;

            for (int x = 0; x < boardInfo.width; x++)
            {
                for (int y = 0; y < boardInfo.height; y++)
                {
                    int state = boardInfo.CellStates[x, y];
                    if (state == -2)
                    {
                        totalFlags++;
                    }
                    else if (state == -1)
                    {
                        allHidden.Add(new Vector2Int(x, y));
                    }
                    else if (state > 0)
                    {
                        List<Vector2Int> hiddenNeighbors = GetNeighbors(boardInfo, x, y, -1);
                        int remainingMines = state - GetNeighbors(boardInfo, x, y, -2).Count;
                        if (hiddenNeighbors.Count > 0)
                        {
                            ProbabilityConstraint constraint = new ProbabilityConstraint(remainingMines);
                            foreach (Vector2Int neighbor in hiddenNeighbors)
                            {
                                constraint.variables.Add(neighbor.x * boardInfo.height + neighbor.y);
                                frontierSet.Add(neighbor);
                            }
                            constraints.Add(constraint);
                        }
                        else if (remainingMines != 0)
                        {
                            constraints.Add(new ProbabilityConstraint(remainingMines));
                        }
                    }
                }
            }

            if (allHidden.Count == 0)
            {
                actionData = new AIActionData(-1, -1, AIAction.Reveal);
                return false;
            }

            foreach (Vector2Int cell in allHidden)
            {
                if (frontierSet.Contains(cell)) frontier.Add(cell);
                else nonFrontier.Add(cell);
            }

            int remainingMineBudget = boardInfo.mineCount - totalFlags;
            if (remainingMineBudget >= 0 && remainingMineBudget <= allHidden.Count)
            {
                Dictionary<Vector2Int, int> frontierIndices = new Dictionary<Vector2Int, int>();
                for (int i = 0; i < frontier.Count; i++)
                {
                    frontierIndices[frontier[i]] = i;
                }

                foreach (ProbabilityConstraint constraint in constraints)
                {
                    for (int i = 0; i < constraint.variables.Count; i++)
                    {
                        Vector2Int cell = new Vector2Int(
                            constraint.variables[i] / boardInfo.height,
                            constraint.variables[i] % boardInfo.height);
                        constraint.variables[i] = frontierIndices[cell];
                    }
                }

                int minimumFrontierMines = System.Math.Max(0, remainingMineBudget - nonFrontier.Count);
                int maximumFrontierMines = System.Math.Min(remainingMineBudget, frontier.Count);
                ProbabilitySearch search = new ProbabilitySearch(
                    constraints,
                    frontier.Count,
                    minimumFrontierMines,
                    maximumFrontierMines);
                search.Run();
                if (!search.LimitReached)
                {
                    double totalWays = 0;
                    for (int frontierMines = 0; frontierMines < search.WaysByMineCount.Length; frontierMines++)
                    {
                        int nonFrontierMines = remainingMineBudget - frontierMines;
                        totalWays += search.WaysByMineCount[frontierMines] *
                                     Binomial(nonFrontier.Count, nonFrontierMines);
                    }

                    if (totalWays > 0)
                    {
                        Vector2Int bestCell = SelectBestProbabilityCell(
                            boardInfo,
                            allHidden,
                            frontier,
                            nonFrontier.Count,
                            remainingMineBudget,
                            search,
                            totalWays);
                        actionData = new AIActionData(bestCell.x, bestCell.y, AIAction.Reveal);
                        return true;
                    }
                    Debug.LogWarning("[AISolve] No mine layout matches the current clues and flags; using a center guess.");
                }
                else
                {
                    Debug.LogWarning("[AISolve] Probability search limit reached; using global mine density.");
                }
            }
            else
            {
                Debug.LogWarning("[AISolve] Remaining mine count is inconsistent with hidden cells; using a center guess.");
            }

            Vector2Int fallbackCell = SelectClosestToCenter(boardInfo, allHidden);
            actionData = new AIActionData(fallbackCell.x, fallbackCell.y, AIAction.Reveal);
            return true;
        }

        private Vector2Int SelectBestProbabilityCell(
            BoardInfo boardInfo,
            List<Vector2Int> allHidden,
            List<Vector2Int> frontier,
            int nonFrontierCount,
            int remainingMineBudget,
            ProbabilitySearch search,
            double totalWays)
        {
            Vector2Int bestCell = SelectClosestToCenter(boardInfo, allHidden);
            double lowestProbability = double.MaxValue;

            foreach (Vector2Int cell in allHidden)
            {
                double mineWays = 0;
                if (frontier.Contains(cell))
                {
                    int variable = frontier.IndexOf(cell);
                    for (int frontierMines = 0; frontierMines < search.WaysByMineCount.Length; frontierMines++)
                    {
                        int nonFrontierMines = remainingMineBudget - frontierMines;
                        mineWays += search.MineWaysByCount[frontierMines][variable] *
                                    Binomial(nonFrontierCount, nonFrontierMines);
                    }
                }
                else
                {
                    for (int frontierMines = 0; frontierMines < search.WaysByMineCount.Length; frontierMines++)
                    {
                        int otherNonFrontierMines = remainingMineBudget - frontierMines - 1;
                        mineWays += search.WaysByMineCount[frontierMines] *
                                    Binomial(nonFrontierCount - 1, otherNonFrontierMines);
                    }
                }

                double probability = mineWays / totalWays;
                if (probability < lowestProbability)
                {
                    lowestProbability = probability;
                    bestCell = cell;
                }
            }

            return bestCell;
        }

        private static double Binomial(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            k = System.Math.Min(k, n - k);
            double result = 1;
            for (int i = 1; i <= k; i++)
            {
                result = result * (n - k + i) / i;
            }
            return result;
        }

        private static Vector2Int SelectClosestToCenter(BoardInfo boardInfo, List<Vector2Int> cells)
        {
            Vector2Int center = new Vector2Int(boardInfo.width / 2, boardInfo.height / 2);
            cells.Sort((a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude));
            return cells[0];
        }


    }
}
