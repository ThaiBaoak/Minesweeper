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

        private bool solveProbability(BoardInfo boardInfo, out AIActionData actionData)
        {
            Dictionary<Vector2Int, float> probabilities = new Dictionary<Vector2Int, float>();
            Dictionary<Vector2Int, int> Counts = new Dictionary<Vector2Int, int>();

            // Tính toán xác suất mìn sơ bộ cho các ô ẩn ở vùng biên
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
                    
                    if(hiddenNeighbors.Count > 0)
                    {
                        float probability = (float)remainingMines / hiddenNeighbors.Count;
                        foreach(var neighbor in hiddenNeighbors)
                        {
                            if(probabilities.ContainsKey(neighbor))
                            {
                                probabilities[neighbor] += probability;
                                Counts[neighbor]++;
                            }
                            else
                            {
                                probabilities[neighbor] = probability;
                                Counts[neighbor] = 1;
                            }
                        }
                    }
                }
            }

            // Chọn ô có xác suất dính mìn trung bình thấp nhất
            if(probabilities.Count > 0)
            {
                Vector2Int bestCell = new Vector2Int(-1, -1);
                float minProbability = float.MaxValue;

                foreach(var kvp in probabilities)
                {
                    Vector2Int cell = kvp.Key;
                    float avgProbability = kvp.Value / Counts[cell];

                    if(avgProbability < minProbability)
                    {
                        minProbability = avgProbability;
                        bestCell = cell;
                    }
                }

                actionData = new AIActionData(bestCell.x, bestCell.y, AIAction.Reveal);
                return true;
            }
            List<Vector2Int> hiddenCells = new List<Vector2Int>();
            for(int x=0; x < boardInfo.width; x++)
            {
                for(int y = 0; y < boardInfo.height; y++)
                {
                    if(boardInfo.CellStates[x, y] == -1)
                    {
                        hiddenCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            if(hiddenCells.Count > 0)
            {
                Vector2Int center= new Vector2Int(boardInfo.width / 2, boardInfo.height / 2);
                hiddenCells.Sort((a, b) => Vector2Int.Distance(a, center).CompareTo(Vector2Int.Distance(b, center)));
                actionData = new AIActionData(hiddenCells[0].x, hiddenCells[0].y, AIAction.Reveal);
                return true;
            }
            actionData = new AIActionData(-1, -1, AIAction.Reveal);
            return false;
        }


    }
}
