using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator : MonoBehaviour
{
    public int gridSize = 3;
    public float nodeSize = 0.2f;
    public float cellSize = 0.8f;
    public GameObject nodePrefab;
    public GameObject startNodePrefab;
    public GameObject endNodePrefab;
    public GameObject whiteCellPrefab;
    public GameObject blackCellPrefab;
    private LineRenderer _lineRenderer;
    
    // Puzzle
    private Vector2Int _startPosition;
    private Vector2Int _endPosition;
    private List<Vector2Int> _solutionPath;
    
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }
    
    private void Start()
    {
        GeneratePuzzle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            _lineRenderer.positionCount = 0;
            GeneratePuzzle();
        }
    }

    private void GeneratePuzzle()
    {
        GenerateSolution();
        RenderPuzzle();
    }
    
    private void GenerateSolution()
    {
        _startPosition = new Vector2Int(0, 0);
        _endPosition = new Vector2Int(gridSize, gridSize);
        _solutionPath = new List<Vector2Int> { _startPosition };
        var visited = new bool[gridSize + 1, gridSize + 1];
        visited[_startPosition.x, _startPosition.y] = true;
        DFS(_solutionPath, visited);
    }
    
    private bool DFS(List<Vector2Int> path, bool[,] visited)
    {
        // Return true if we reached the end position
        var currentPosition = path[^1];
        if (currentPosition == _endPosition)
            return true;
        
        // DFS in 4 directions
        var directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        while (directions.Count > 0)
        {
            var directionIndex = Random.Range(0, directions.Count);
            var nextPosition = currentPosition + directions[directionIndex];
            if (IsValidMove(nextPosition, visited))
            {
                visited[nextPosition.x, nextPosition.y] = true;
                path.Add(nextPosition);
                if (DFS(path, visited))
                    return true;
                path.RemoveAt(path.Count - 1);
                visited[nextPosition.x, nextPosition.y] = false;
            }
            directions.RemoveAt(directionIndex);
        }
        return false;
    }
    
    private bool IsValidMove(Vector2Int position, bool[,] visited)
    {
        return position.x >= 0 && position.x < gridSize + 1 &&
               position.y >= 0 && position.y < gridSize + 1 &&
               !visited[position.x, position.y];
    }
    
    private void RenderPuzzle()
    {
        // Render cells
        var centerOffset = new Vector3((gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f, 0);
        for (var x = 0; x < gridSize; x++)
        {
            for (var y = 0; y < gridSize; y++)
            {
                var cellPosition = new Vector3(x, y, 0) - centerOffset;
                if (_solutionPath.Contains(new Vector2Int(x, y)))
                {
                    var go = Instantiate(whiteCellPrefab, cellPosition, Quaternion.identity, transform);
                    go.transform.localScale = Vector3.one * cellSize;
                }
                else
                {
                    var go = Instantiate(blackCellPrefab, cellPosition, Quaternion.identity, transform);
                    go.transform.localScale = Vector3.one * cellSize;
                }
            }
        }
        
        // Render nodes
        for (var x = 0; x < gridSize + 1; x++)
        {
            for (var y = 0; y < gridSize + 1; y++)
            {
                GameObject go;
                var nodePosition = new Vector3(x - 0.5f, y - 0.5f, 0) - centerOffset;
                if (_startPosition == new Vector2Int(x, y))
                    go = Instantiate(startNodePrefab, nodePosition, Quaternion.identity, transform);
                else if (_endPosition == new Vector2Int(x, y))
                    go = Instantiate(endNodePrefab, nodePosition, Quaternion.identity, transform);
                else
                    go = Instantiate(nodePrefab, nodePosition, Quaternion.identity, transform);
                go.transform.localScale = Vector3.one * nodeSize;
                go.transform.position += new Vector3(0, 0, -2f);
            }
        }

        // Render lines between solution path
        _lineRenderer.positionCount = _solutionPath.Count;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
        for (var i = 0; i < _solutionPath.Count; i++)        {
            var cellPosition = new Vector3(_solutionPath[i].x - 0.5f, _solutionPath[i].y - 0.5f, -1f) - centerOffset;
            _lineRenderer.SetPosition(i, cellPosition);
        }
    }
}
