using System.Collections.Generic;

namespace Data
{
    public class PuzzleData
    {
        public readonly int gridSize;
        public GridNode startNode; // Bottom-left corner
        public GridNode endNode; // Top-right corner
        public List<GridNode> solutionPath;
        public readonly Dictionary<string, CellType> cells; // "r,c" → CellType
        public readonly HashSet<GridNode> requiredDots;
        public readonly HashSet<GridEdge> brokenEdges;
        public Dictionary<string, int> solutionRegions;

        public PuzzleData(int size)
        {
            gridSize = size;
            cells = new Dictionary<string, CellType>();
            requiredDots = new HashSet<GridNode>();
            brokenEdges = new HashSet<GridEdge>();
        }

        public static string CellKey(int r, int c) => $"{r},{c}";
    }
}