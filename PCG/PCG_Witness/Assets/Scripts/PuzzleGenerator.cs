using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    [Min(3)] public int gridSize = 4;
    [Range(1, 5)] public int difficulty = 2;
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    // Rules
    public bool useBlackWhite = true;
    public bool useStars = false;
    public bool useDots = true;
    public bool useBrokenEdges = false;

    public PuzzleData Generate()
    {
        var puzzle =
            new PuzzleData(gridSize) { StartNode = new GridNode(gridSize, 0), EndNode = new GridNode(0, gridSize) };
        var solution = GenerateSolutionPath(puzzle.StartNode, puzzle.EndNode, gridSize);
        while (solution == null)
            solution = GenerateSolutionPath(puzzle.StartNode, puzzle.EndNode, gridSize);
        puzzle.SolutionPath = solution;
        puzzle.SolutionRegions = RegionSolver.ComputeRegions(solution, gridSize);
        if (useBlackWhite) PlaceBlackWhiteCells(puzzle);
        if (useStars) PlaceStars(puzzle);
        if (useDots) PlaceDots(puzzle);
        if (useBrokenEdges) PlaceBrokenEdges(puzzle);
        return puzzle;
    }

    private static List<GridNode> GenerateSolutionPath(GridNode start, GridNode end, int size)
    {
        var visited = new HashSet<string> { start.ToString() };
        var path = new List<GridNode> { start };
        return DFS(path, visited, end, size);
    }

    private static List<GridNode> DFS(List<GridNode> path, HashSet<string> visited, GridNode end, int size)
    {
        var cur = path[^1];
        if (cur.Equals(end)) return new List<GridNode>(path);
        if (path.Count > (size + 1) * (size + 1))
        {
            Debug.LogWarning("Too long path, backtracking...");
            return null;
        }

        // Deep First Search with random direction order
        var dirs = Directions;
        while (dirs.Length > 0)
        {
            var dir = dirs[Random.Range(0, dirs.Length)];
            var next = new GridNode(cur.row + dir.x, cur.col + dir.y);
            if (next.row < 0 || next.row > size || next.col < 0 || next.col > size) continue; // Skip if out of bounds
            var key = next.ToString();
            if (!visited.Add(key)) continue; // Skip if already visited
            path.Add(next);
            var result = DFS(path, visited, end, size);
            if (result != null) return result;
            path.RemoveAt(path.Count - 1);
            visited.Remove(key);
            ((IList)dirs).Remove(dir);
        }

        return null;
    }

    private void PlaceBlackWhiteCells(PuzzleData puzzle)
    {
        // Pre-assign black/white to regions
        var idx = 0;
        var regionAssign = new Dictionary<int, CellType>();
        var regionIds = new HashSet<int>(puzzle.SolutionRegions.Values);
        foreach (var id in regionIds)
        {
            regionAssign[id] = idx % 2 == 0 ? CellType.White : CellType.Black;
            idx++;
        }

        // Adjust density based on difficulty
        var density = Mathf.Lerp(0.15f, 0.75f, (difficulty - 1) / 4f);
        for (var r = 0; r < puzzle.GridSize; r++)
        {
            for (var c = 0; c < puzzle.GridSize; c++)
            {
                if (Random.value > density) continue;
                var key = PuzzleData.CellKey(r, c);
                if (puzzle.SolutionRegions.TryGetValue(key, out var reg))
                    puzzle.Cells[key] = regionAssign[reg];
            }
        }
    }

    private void PlaceStars(PuzzleData puzzle)
    {
        // Collect cells by region
        var regionCells = new Dictionary<int, List<Vector2Int>>();
        for (var r = 0; r < puzzle.GridSize; r++)
        {
            for (var c = 0; c < puzzle.GridSize; c++)
            {
                var key = PuzzleData.CellKey(r, c);
                if (!puzzle.SolutionRegions.TryGetValue(key, out var reg)) continue;
                if (!regionCells.ContainsKey(reg)) regionCells[reg] = new List<Vector2Int>();
                regionCells[reg].Add(new Vector2Int(r, c));
            }
        }

        // Place star pairs in some regions
        var colorIdx = 0;
        var starColors = new[] { CellType.StarGold, CellType.StarTeal };
        var pairs = Mathf.Min(difficulty - 1, regionCells.Count); // Adjust number of pairs based on difficulty
        foreach (var kv in regionCells)
        {
            if (pairs-- <= 0) break;
            if (kv.Value.Count < 2) continue;
            var node1 = kv.Value[Random.Range(0, kv.Value.Count)];
            var node2 = kv.Value[Random.Range(0, kv.Value.Count)];
            var color = starColors[colorIdx++ % starColors.Length];
            puzzle.Cells[PuzzleData.CellKey(node1.x, node1.y)] = color;
            puzzle.Cells[PuzzleData.CellKey(node2.x, node2.y)] = color;
        }
    }

    private void PlaceDots(PuzzleData puzzle)
    {
        var interior = puzzle.SolutionPath.GetRange(1, puzzle.SolutionPath.Count - 2);
        var count = Mathf.Max(1, Mathf.FloorToInt(interior.Count * 0.3f)); // Adjust number of dots based on difficulty
        var chosen = Shuffle(interior);
        for (var i = 0; i < Mathf.Min(count, chosen.Count); i++)
            puzzle.RequiredDots.Add(chosen[i]);
    }

    private void PlaceBrokenEdges(PuzzleData puzzle)
    {
        if (difficulty < 3) return;

        // 收集解路径上的边
        var solutionEdges = new HashSet<GridEdge>();
        for (var i = 0; i < puzzle.SolutionPath.Count - 1; i++)
            solutionEdges.Add(new GridEdge(puzzle.SolutionPath[i], puzzle.SolutionPath[i + 1]));

        // 枚举所有边并过滤掉解路径上的
        var candidates = new List<GridEdge>();
        var size = puzzle.GridSize;
        for (var r = 0; r <= size; r++)
        {
            for (var c = 0; c <= size; c++)
            {
                if (r < size)
                {
                    var e = new GridEdge(new GridNode(r, c), new GridNode(r + 1, c));
                    if (!solutionEdges.Contains(e)) candidates.Add(e);
                }

                if (c < size)
                {
                    var e = new GridEdge(new GridNode(r, c), new GridNode(r, c + 1));
                    if (!solutionEdges.Contains(e)) candidates.Add(e);
                }
            }
        }

        var breakCount = Mathf.FloorToInt(difficulty * 0.6f);
        var shuffled = Shuffle(candidates);
        for (var i = 0; i < Mathf.Min(breakCount, shuffled.Count); i++)
            puzzle.BrokenEdges.Add(shuffled[i]);
    }


    private static List<T> Shuffle<T>(List<T> list)
    {
        var result = new List<T>(list);
        for (var i = result.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
}