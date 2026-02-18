using System;
using UnityEngine;

namespace Data
{
    public enum CellType
    {
        Empty,
        White,
        Black,
        StarGold,
        StarTeal
    }

    [Serializable]
    public struct GridNode : IEquatable<GridNode>
    {
        public int row, col;

        public GridNode(int r, int c)
        {
            row = r;
            col = c;
        }

        public bool Equals(GridNode other)
        {
            return row == other.row && col == other.col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(row, col);
        }

        public override string ToString() => $"{row},{col}";

        public bool IsAdjacentTo(GridNode other) => Mathf.Abs(row - other.row) + Mathf.Abs(col - other.col) == 1;
    }

    [Serializable]
    public struct GridEdge : IEquatable<GridEdge>
    {
        public GridNode nodeA, nodeB;

        public GridEdge(GridNode a, GridNode b)
        {
            // A is always the "smaller" node
            if (a.row < b.row || (a.row == b.row && a.col < b.col))
            {
                nodeA = a;
                nodeB = b;
            }
            else
            {
                nodeA = b;
                nodeB = a;
            }
        }

        public bool Equals(GridEdge other)
        {
            return nodeA.Equals(other.nodeA) && nodeB.Equals(other.nodeB);
        }

        public override int GetHashCode() => nodeA.GetHashCode() * 397 ^ nodeB.GetHashCode();

        public override string ToString() => $"{nodeA}-{nodeB}";

        public bool Contains(GridNode n) => nodeA.Equals(n) || nodeB.Equals(n);
    }
}