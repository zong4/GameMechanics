using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public static PuzzleData Generate(int gridSize, int difficulty, bool useBlackWhite, bool useStars, bool useDots,
        bool useBrokenEdges)
    {
        var puzzle =
            new PuzzleData(gridSize) { startNode = new GridNode(gridSize, 0), endNode = new GridNode(0, gridSize) };
        var solution = GenerateSolutionPath(puzzle.startNode, puzzle.endNode, gridSize);
        puzzle.solutionPath = solution;
        puzzle.solutionRegions = RegionSolver.ComputeRegions(solution, gridSize);
        if (useBlackWhite) PlaceBlackWhiteCells(puzzle, difficulty);
        if (useStars) PlaceStars(puzzle, difficulty);
        if (useDots) PlaceDots(puzzle, difficulty);
        if (useBrokenEdges) PlaceBrokenEdges(puzzle, difficulty);
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

    private static void PlaceBlackWhiteCells(PuzzleData puzzle, int difficulty)
    {
        // Pre-assign black/white to regions
        var idx = 0;
        var regionAssign = new Dictionary<int, CellType>();
        var regionIds = new HashSet<int>(puzzle.solutionRegions.Values);
        foreach (var id in regionIds)
        {
            regionAssign[id] = idx % 2 == 0 ? CellType.White : CellType.Black;
            idx++;
        }

        // Adjust density based on difficulty
        var density = Mathf.Lerp(0.15f, 0.75f, (difficulty - 1) / 4f);
        for (var r = 0; r < puzzle.gridSize; r++)
        {
            for (var c = 0; c < puzzle.gridSize; c++)
            {
                if (Random.value > density) continue;
                var key = PuzzleData.CellKey(r, c);
                if (puzzle.solutionRegions.TryGetValue(key, out var reg))
                    puzzle.cells[key] = regionAssign[reg];
            }
        }
    }

    private static void PlaceStars(PuzzleData puzzle, int difficulty)
    {
        // Collect cells by region
        var regionCells = new Dictionary<int, List<Vector2Int>>();
        for (var r = 0; r < puzzle.gridSize; r++)
        {
            for (var c = 0; c < puzzle.gridSize; c++)
            {
                var key = PuzzleData.CellKey(r, c);
                if (!puzzle.solutionRegions.TryGetValue(key, out var reg)) continue;
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
            puzzle.cells[PuzzleData.CellKey(node1.x, node1.y)] = color;
            puzzle.cells[PuzzleData.CellKey(node2.x, node2.y)] = color;
        }
    }

    private static void PlaceDots(PuzzleData puzzle, int difficulty)
    {
        var interior = puzzle.solutionPath.GetRange(1, puzzle.solutionPath.Count - 2);
        var count = Mathf.Max(1,
            Mathf.FloorToInt(interior.Count * 0.1f + difficulty)); // Adjust count based on difficulty
        var chosen = Shuffle(interior);
        for (var i = 0; i < Mathf.Min(count, chosen.Count); i++)
            puzzle.requiredDots.Add(chosen[i]);
    }

    private static void PlaceBrokenEdges(PuzzleData puzzle, int difficulty)
    {
        // Collect edges on the solution path
        var solutionEdges = new HashSet<GridEdge>();
        for (var i = 0; i < puzzle.solutionPath.Count - 1; i++)
            solutionEdges.Add(new GridEdge(puzzle.solutionPath[i], puzzle.solutionPath[i + 1]));

        // Collect candidate edges that are not on the solution path
        var candidates = new List<GridEdge>();
        var size = puzzle.gridSize;
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

        // Place some broken edges based on difficulty
        var breakCount = Mathf.FloorToInt(difficulty * 0.6f);
        var shuffled = Shuffle(candidates);
        for (var i = 0; i < Mathf.Min(breakCount, shuffled.Count); i++)
            puzzle.brokenEdges.Add(shuffled[i]);
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

    public static List<string> Validate(List<GridNode> path, PuzzleData puzzle)
    {
        var errors = new List<string>();
        foreach (var dot in puzzle.requiredDots)
        {
            if (!path.Contains(dot))
            {
                errors.Add("Must pass through all required dots");
                return errors;
            }
        }

        // Store regions for all cells
        var regions = RegionSolver.ComputeRegions(path, puzzle.gridSize);
        var regionTypes = new Dictionary<int, HashSet<CellType>>();
        foreach (var kv in puzzle.cells)
        {
            if (kv.Value != CellType.White && kv.Value != CellType.Black) continue;
            if (!regions.TryGetValue(kv.Key, out int reg)) continue;
            if (!regionTypes.ContainsKey(reg)) regionTypes[reg] = new HashSet<CellType>();
            regionTypes[reg].Add(kv.Value);
        }

        // Check that no region contains both white and black cells
        foreach (var kv in regionTypes)
        {
            if (kv.Value.Contains(CellType.White) && kv.Value.Contains(CellType.Black))
            {
                errors.Add("Black and white cells cannot be in the same region");
                return errors;
            }
        }

        // Store star counts for each region
        var starsByRegion = new Dictionary<int, Dictionary<CellType, int>>();
        foreach (var kv in puzzle.cells)
        {
            if (kv.Value != CellType.StarGold && kv.Value != CellType.StarTeal) continue;
            if (!regions.TryGetValue(kv.Key, out int reg)) continue;
            if (!starsByRegion.ContainsKey(reg)) starsByRegion[reg] = new Dictionary<CellType, int>();
            starsByRegion[reg].TryGetValue(kv.Value, out int cnt);
            starsByRegion[reg][kv.Value] = cnt + 1;
        }

        // Check that each star color appears exactly twice in the same region
        foreach (var region in starsByRegion)
        {
            foreach (var kv in region.Value)
            {
                if (kv.Value != 2)
                {
                    errors.Add($"Star color {kv.Key} must appear exactly twice in the same region");
                    return errors;
                }
            }
        }

        return errors; // Empty list means valid
    }
}