using System.Collections.Generic;
using UnityEngine;

public class PuzzleInput3 : MonoBehaviour
{
    [Header("Refs")]
    public PuzzleGenerator3 generator;
    public LineRenderer mainLine;
    public LineRenderer tailLine;

    [Header("Settings")]
    public float lineWidth = 0.1f;
    public float snapDistance = 0.25f;

    private Camera _camera;
    private bool _isDrawing;

    private readonly List<Vector2Int> _path = new();
    private Vector2Int _lastNode;

    private void Awake()
    {
        _camera = Camera.main;

        SetupLine(mainLine);
        SetupLine(tailLine);
    }

    private void SetupLine(LineRenderer lr)
    {
        lr.positionCount = 0;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCornerVertices = 8;
        lr.numCapVertices = 8;
        lr.useWorldSpace = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryStart();

        if (_isDrawing && Input.GetMouseButton(0))
            UpdateDrawing();

        if (_isDrawing && Input.GetMouseButtonUp(0))
            Finish();
    }

    // =========================
    // Start
    // =========================
    private void TryStart()
    {
        var node = RaycastNode();
        if (!node.HasValue) return;

        if (node.Value != generator.StartPosition)
            return;

        _isDrawing = true;
        _path.Clear();
        _path.Add(node.Value);
        _lastNode = node.Value;

        UpdateMainLine();
    }

    // =========================
    // Drag
    // =========================
    private void UpdateDrawing()
    {
        var mouseWorld = MouseWorld();

        var hover = RaycastNode();
        if (hover.HasValue)
        {
            var next = hover.Value;

            // 回退
            if (_path.Count >= 2 && next == _path[^2])
            {
                _path.RemoveAt(_path.Count - 1);
                _lastNode = _path[^1];
                UpdateMainLine();
                return;
            }

            // 前进
            if (!_path.Contains(next) && IsAdjacent(_lastNode, next))
            {
                _path.Add(next);
                _lastNode = next;
                UpdateMainLine();
                return;
            }
        }

        UpdateTailLine(mouseWorld);
    }

    // =========================
    // End
    // =========================
    private void Finish()
    {
        _isDrawing = false;
        tailLine.positionCount = 0;

        var success = generator.ValidateResult(_path);
        if (success)
        {
            generator.difficulty += 1;
            generator.GeneratePuzzle();
        }
        ClearAll();
    }

    private void ClearAll()
    {
        _path.Clear();
        mainLine.positionCount = 0;
        tailLine.positionCount = 0;
    }

    // =========================
    // Rendering
    // =========================
    private void UpdateMainLine()
    {
        mainLine.positionCount = _path.Count;
        for (int i = 0; i < _path.Count; i++)
            mainLine.SetPosition(i, NodeToWorld(_path[i]));

        tailLine.positionCount = 0;
    }

    private void UpdateTailLine(Vector3 mouseWorld)
    {
        tailLine.positionCount = 2;
        tailLine.SetPosition(0, NodeToWorld(_lastNode));
        tailLine.SetPosition(1, mouseWorld);
    }

    // =========================
    // Utils
    // =========================
    private Vector3 NodeToWorld(Vector2Int p)
    {
        return new Vector3(
            p.x - 0.5f,
            p.y - 0.5f,
            -1f
        ) - generator.Center;
    }

    private Vector3 MouseWorld()
    {
        var w = _camera.ScreenToWorldPoint(Input.mousePosition);
        w.z = -1f;
        return w;
    }

    private Vector2Int? RaycastNode()
    {
        var world = MouseWorld();
        var hit = Physics2D.Raycast(world, Vector2.zero);
        if (!hit.collider) return null;

        if (hit.collider.TryGetComponent<Node>(out var node))
            return node.gridPos;

        return null;
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}
