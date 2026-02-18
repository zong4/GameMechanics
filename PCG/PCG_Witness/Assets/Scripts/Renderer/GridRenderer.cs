using Data;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [Header("引用")]
    public GameObject linePrefab; // 挂有 LineRenderer 的预制体
    public GameObject nodePrefab; // 球体节点预制体
    public GameObject brokenPrefab; // 断裂边预制体（带X标记）

    [Header("样式")]
    public float cellSize = 1f; // 每格世界单位大小
    public float lineWidth = 0.05f;
    public Color gridColor = new Color(0.22f, 0.22f, 0.20f);
    public Color nodeColor = new Color(0.25f, 0.25f, 0.23f);
    public Color startColor = new Color(0.78f, 0.66f, 0.43f);
    public Color endColor = new Color(0.78f, 0.66f, 0.43f);
    public Color brokenColor = new Color(0.75f, 0.31f, 0.25f);

    PuzzleData _puzzle;

    public void Render(PuzzleData puzzle)
    {
        _puzzle = puzzle;
        // 清空旧对象
        foreach (Transform child in transform) Destroy(child.gameObject);

        int G = puzzle.GridSize;

        // 绘制格线（水平 + 垂直）
        for (int r = 0; r <= G; r++)
        {
            for (int c = 0; c <= G; c++)
            {
                // 水平边 →
                if (c < G)
                {
                    var edge = new GridEdge(new GridNode(r, c), new GridNode(r, c + 1));
                    DrawEdge(new GridNode(r, c), new GridNode(r, c + 1), puzzle.BrokenEdges.Contains(edge));
                }

                // 垂直边 ↓
                if (r < G)
                {
                    var edge = new GridEdge(new GridNode(r, c), new GridNode(r + 1, c));
                    DrawEdge(new GridNode(r, c), new GridNode(r + 1, c), puzzle.BrokenEdges.Contains(edge));
                }
            }
        }

        // 绘制节点
        for (int r = 0; r <= G; r++)
        {
            for (int c = 0; c <= G; c++)
            {
                var node = new GridNode(r, c);
                bool isStart = node.Equals(puzzle.StartNode);
                bool isEnd = node.Equals(puzzle.EndNode);
                SpawnNode(node, isStart ? startColor : (isEnd ? endColor : nodeColor),
                    isStart ? 0.18f : (isEnd ? 0.16f : 0.08f));
            }
        }
    }

    void DrawEdge(GridNode a, GridNode b, bool broken)
    {
        var go = Instantiate(broken ? brokenPrefab : linePrefab, transform);
        var lr = go.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, NodeToWorld(a));
        lr.SetPosition(1, NodeToWorld(b));
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = lr.endColor = broken ? brokenColor : gridColor;
    }

    void SpawnNode(GridNode n, Color col, float radius)
    {
        var go = Instantiate(nodePrefab, NodeToWorld(n), Quaternion.identity, transform);
        go.transform.localScale = Vector3.one * radius * 2;
        go.GetComponent<Renderer>().material.color = col;
    }

    public Vector3 NodeToWorld(GridNode n)
    {
        // row 增大 → 向下（Y 减小）
        return new Vector3(n.col * cellSize, -n.row * cellSize, 0f);
    }

    // 鼠标点击时找最近节点
    public bool TryGetNearestNode(Vector3 worldPos, out GridNode node, float threshold = 0.35f)
    {
        int G = _puzzle.GridSize;
        float minDist = float.MaxValue;
        node = default;

        for (int r = 0; r <= G; r++)
        {
            for (int c = 0; c <= G; c++)
            {
                float d = Vector3.Distance(worldPos, NodeToWorld(new GridNode(r, c)));
                if (d < minDist)
                {
                    minDist = d;
                    node = new GridNode(r, c);
                }
            }
        }

        return minDist < threshold * cellSize;
    }
}