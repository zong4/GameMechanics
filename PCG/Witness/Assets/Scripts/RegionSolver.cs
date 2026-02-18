using System.Collections.Generic;
using Data;
using UnityEngine;

public static class RegionSolver
{
    public static Dictionary<string, int> ComputeRegions(List<GridNode> path, int size)
    {
        var edgeSet = BuildEdgeSet(path);
        var regionId = 0;
        var result = new Dictionary<string, int>();
        var visited = new HashSet<string>();
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                var key = $"{r},{c}";
                if (visited.Contains(key)) continue;

                // BFS
                var queue = new Queue<Vector2Int>();
                queue.Enqueue(new Vector2Int(r, c));
                visited.Add(key);
                while (queue.Count > 0)
                {
                    var cell = queue.Dequeue();
                    result[$"{cell.x},{cell.y}"] = regionId;
                    foreach (var neighbor in GetCellNeighbors(cell.x, cell.y, size))
                    {
                        var nk = $"{neighbor.x},{neighbor.y}";
                        if (visited.Contains(nk)) continue;
                        if (!CanPassBetween(cell.x, cell.y, neighbor.x, neighbor.y, edgeSet)) continue;
                        visited.Add(nk);
                        queue.Enqueue(neighbor);
                    }
                }

                regionId++;
            }
        }

        return result;
    }

    private static HashSet<string> BuildEdgeSet(List<GridNode> path)
    {
        var set = new HashSet<string>();
        for (var i = 0; i < path.Count - 1; i++)
        {
            var edge = new GridEdge(path[i], path[i + 1]);
            set.Add(edge.ToString());
        }

        return set;
    }

    private static bool CanPassBetween(int r1, int c1, int r2, int c2, HashSet<string> edgeSet)
    {
        var edge = new GridEdge(new GridNode(r1, c1), new GridNode(r2, c2));
        return !edgeSet.Contains(edge.ToString());
    }

    private static IEnumerable<Vector2Int> GetCellNeighbors(int r, int c, int size)
    {
        if (r > 0) yield return new Vector2Int(r - 1, c);
        if (r < size - 1) yield return new Vector2Int(r + 1, c);
        if (c > 0) yield return new Vector2Int(r, c - 1);
        if (c < size - 1) yield return new Vector2Int(r, c + 1);
    }
}