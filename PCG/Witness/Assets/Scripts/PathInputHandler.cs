using System.Collections.Generic;
using Data;
using Renderer;
using UnityEngine;

public class PuzzleInputHandler : MonoBehaviour
{
    private PuzzleData _puzzle;
    private GridRenderer _gridRenderer;
    private PathRenderer _pathRenderer;
    private readonly List<GridNode> _path = new List<GridNode>();
    private bool _isDrawing;
    private Camera _camera;

    private void Start()
    {
        _gridRenderer = FindObjectOfType<GridRenderer>();
        _pathRenderer = FindObjectOfType<PathRenderer>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_puzzle == null) return;

        // Start drawing
        var worldPos = GetWorldMousePos();
        if (Input.GetMouseButtonDown(0))
        {
            if (_gridRenderer.TryGetNearestNode(worldPos, out var node))
            {
                if (node.Equals(_puzzle.startNode))
                {
                    _isDrawing = true;
                    _path.Clear();
                    _path.Add(node);
                    _pathRenderer.UpdatePath(_path);
                }
            }
        }

        // Continue drawing
        if (_isDrawing && Input.GetMouseButton(0))
        {
            if (_gridRenderer.TryGetNearestNode(worldPos, out var node))
                TryExtendPath(node);
        }

        // Stop drawing
        if (Input.GetMouseButtonUp(0))
        {
            _isDrawing = false;
            _path.Clear();
            _pathRenderer.UpdatePath(_path);
        }
    }

    private void TryExtendPath(GridNode node)
    {
        Debug.Log($"Trying to extend path to {node}");

        // Shrink path
        if (_path.Count >= 2 && node.Equals(_path[^2]))
        {
            _path.RemoveAt(_path.Count - 1);
            _pathRenderer.UpdatePath(_path);
            return;
        }

        // Return if not adjacent
        var last = _path[^1];
        if (!node.IsAdjacentTo(last)) return;

        // Return if edge is broken
        var edge = new GridEdge(last, node);
        if (_puzzle.brokenEdges.Contains(edge)) return;

        // Return if node already in path (no loops)
        if (_path.Contains(node)) return;

        // Extend path
        _path.Add(node);
        _pathRenderer.UpdatePath(_path);
        if (node.Equals(_puzzle.endNode))
        {
            _isDrawing = false;
            var errors = PuzzleGenerator.Validate(_path, _puzzle);
            if (errors.Count == 0)
                OnPuzzleSolved();
            else
                OnPuzzleFailed(errors[0]);
        }
    }

    public void SetPuzzle(PuzzleData puzzle)
    {
        _puzzle = puzzle;
        _isDrawing = false;
        _path.Clear();
        _pathRenderer.Clear();
    }

    private void OnPuzzleSolved()
    {
        Debug.Log("✓ 解题成功！");
    }

    private void OnPuzzleFailed(string reason)
    {
        Debug.Log($"✗ 错误：{reason}");
        Invoke(nameof(ResetPath), 0.6f);
    }

    public void ResetPath()
    {
        _isDrawing = false;
        _path.Clear();
        _pathRenderer.Clear();
    }

    private Vector3 GetWorldMousePos()
    {
        var mp = Input.mousePosition;
        mp.z = Mathf.Abs(_camera.transform.position.z);
        return _camera.ScreenToWorldPoint(mp);
    }
}