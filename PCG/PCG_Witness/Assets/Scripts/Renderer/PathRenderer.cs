using System.Collections.Generic;
using Data;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    [Header("样式")]
    public Color pathColor = new Color(0.91f, 0.875f, 0.78f);
    public float pathWidth = 0.15f;

    LineRenderer _lr;
    GridRenderer _grid;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.startColor = _lr.endColor = pathColor;
        _lr.startWidth = _lr.endWidth = pathWidth;
        _lr.numCapVertices = 8; // 圆头
        _lr.numCornerVertices = 8;
        _lr.sortingOrder = 1; // 显示在格线上方
    }

    public void SetGrid(GridRenderer grid) => _grid = grid;

    public void UpdatePath(List<GridNode> path)
    {
        _lr.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            _lr.SetPosition(i, _grid.NodeToWorld(path[i]));
    }

    public void Clear()
    {
        _lr.positionCount = 0;
    }
}