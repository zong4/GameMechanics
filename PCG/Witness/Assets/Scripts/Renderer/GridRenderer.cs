using Data;
using UnityEngine;

namespace Renderer
{
    public class GridRenderer : MonoBehaviour
    {
        public GameObject linePrefab;
        public GameObject nodePrefab;
        public GameObject brokenPrefab;
        public float lineWidth = 0.05f;
        public Color gridColor = new Color(0.22f, 0.22f, 0.20f);
        public Color nodeColor = new Color(0.25f, 0.25f, 0.23f);
        public Color startColor = new Color(0.78f, 0.66f, 0.43f);
        public Color endColor = new Color(0.78f, 0.66f, 0.43f);
        public Color brokenColor = new Color(0.75f, 0.31f, 0.25f);
        private PuzzleData _puzzle;

        public void Render(PuzzleData puzzle)
        {
            // Destroy old edges and nodes
            foreach (Transform child in transform) Destroy(child.gameObject);

            // Edges
            _puzzle = puzzle;
            var size = puzzle.gridSize;
            for (var r = 0; r <= size; r++)
            {
                for (var c = 0; c <= size; c++)
                {
                    if (c < size)
                    {
                        var edge = new GridEdge(new GridNode(r, c), new GridNode(r, c + 1));
                        DrawEdge(new GridNode(r, c), new GridNode(r, c + 1), puzzle.brokenEdges.Contains(edge));
                    }

                    if (r < size)
                    {
                        var edge = new GridEdge(new GridNode(r, c), new GridNode(r + 1, c));
                        DrawEdge(new GridNode(r, c), new GridNode(r + 1, c), puzzle.brokenEdges.Contains(edge));
                    }
                }
            }

            // Nodes
            for (var r = 0; r <= size; r++)
            {
                for (var c = 0; c <= size; c++)
                {
                    var node = new GridNode(r, c);
                    var isStart = node.Equals(puzzle.startNode);
                    var isEnd = node.Equals(puzzle.endNode);
                    SpawnNode(node, isStart ? startColor : (isEnd ? endColor : nodeColor),
                        isStart ? 0.18f : (isEnd ? 0.16f : 0.08f));
                }
            }
        }

        private void DrawEdge(GridNode a, GridNode b, bool broken)
        {
            var go = Instantiate(broken ? brokenPrefab : linePrefab, transform);
            var lr = go.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, NodeToWorld(a));
            lr.SetPosition(1, NodeToWorld(b));
            lr.startWidth = lr.endWidth = lineWidth;
            lr.startColor = lr.endColor = broken ? brokenColor : gridColor;
        }

        private void SpawnNode(GridNode n, Color col, float radius)
        {
            var go = Instantiate(nodePrefab, NodeToWorld(n), Quaternion.identity, transform);
            go.transform.localScale = Vector3.one * (radius * 2);
            go.GetComponent<SpriteRenderer>().material.color = col;
        }

        public static Vector3 NodeToWorld(GridNode n)
        {
            return new Vector3(n.col, -n.row, 0f);
        }

        public bool TryGetNearestNode(Vector3 worldPos, out GridNode node, float threshold = 0.35f)
        {
            var size = _puzzle.gridSize;
            var minDist = float.MaxValue;
            node = default;
            for (var r = 0; r <= size; r++)
            {
                for (var c = 0; c <= size; c++)
                {
                    var d = Vector3.Distance(worldPos, NodeToWorld(new GridNode(r, c)));
                    if (d < minDist)
                    {
                        minDist = d;
                        node = new GridNode(r, c);
                    }
                }
            }

            return minDist < threshold;
        }
    }
}