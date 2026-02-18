using System.Collections.Generic;

namespace Data
{
    public class PuzzleData
    {
        public readonly int GridSize;
        public GridNode StartNode; // Bottom-left corner
        public GridNode EndNode; // Top-right corner
        public List<GridNode> SolutionPath;
        public readonly Dictionary<string, CellType> Cells; // "r,c" → CellType
        public readonly HashSet<GridNode> RequiredDots;
        public readonly HashSet<GridEdge> BrokenEdges;
        public Dictionary<string, int> SolutionRegions;

        public PuzzleData(int size)
        {
            GridSize = size;
            Cells = new Dictionary<string, CellType>();
            RequiredDots = new HashSet<GridNode>();
            BrokenEdges = new HashSet<GridEdge>();
        }

        public static string CellKey(int r, int c) => $"{r},{c}";
    }
}