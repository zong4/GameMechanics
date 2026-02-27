using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator3 : MonoBehaviour
{
    public int gridSize = 3;
    public float nodeSize = 0.2f;
    public float cellSize = 0.8f;
    public GameObject puzzleBoard;
    public GameObject nodePrefab;
    public GameObject startNodePrefab;
    public GameObject endNodePrefab;
    public GameObject dotPrefab;
    public GameObject cellPrefab;
    public GameObject whiteCellPrefab;
    public GameObject blackCellPrefab;
    private Camera _camera;
    
    // Puzzle
    public int difficulty = 1;
    public bool useWhiteBlackCells = true;
    public bool useRequiredDots = true;
    private Vector2Int _startPosition;
    private Vector2Int _endPosition;
    private List<Vector2Int> _solutionPath;
    private Dictionary<int, List<Vector2Int>> _solutionRegions;
    private List<Vector2Int> _whiteCells;
    private List<Vector2Int> _blackCells;
    private List<Vector2Int> _requiredDots;
    public Vector2Int StartPosition => _startPosition;
    public Vector3 Center => new Vector3((gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f, 0f);
    
    private void Start()
    {
        _camera = Camera.main;
        GeneratePuzzle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            GeneratePuzzle();
        }
    }

    public void GeneratePuzzle()
    {
        GenerateSolution();
        RenderPuzzle();
        puzzleBoard.transform.localScale = new Vector3(gridSize + nodeSize, gridSize + nodeSize, 1f);
        _camera.orthographicSize = (gridSize + 1) * 0.5f;
    }
    
    private void GenerateSolution()
    {
        // Use DFS to generate a random solution path from start to end
        _startPosition = new Vector2Int(0, 0);
        _endPosition = new Vector2Int(gridSize, gridSize);
        _solutionPath = new List<Vector2Int> { _startPosition };
        var visited = new bool[gridSize + 1, gridSize + 1];
        visited[_startPosition.x, _startPosition.y] = true;
        DFS(_solutionPath, visited);
        
        // Use BFS to find all cells reachable from the solution path and group them into regions
        var regionIndex = 0;
        _solutionRegions = new Dictionary<int, List<Vector2Int>>();
        visited = new bool[gridSize, gridSize];
        for (var i = 0; i < gridSize; i++)
        {
            for (var j = 0; j < gridSize; j++)
            {
                // Skip cells that are already visited or are part of the solution path
                if (visited[i, j])
                    continue;

                // Start a new region
                var queue = new Queue<Vector2Int>();
                var regionCells = new List<Vector2Int>();
                queue.Enqueue(new Vector2Int(i, j));
                visited[i, j] = true;
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    regionCells.Add(current);
                    var directions = new List<Vector2Int>
                    {
                        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                    };
                    foreach (var direction in directions)
                    {
                        var next = current + direction;
                        if (IsNotVisited(next, visited) && IsConnected(current, next, _solutionPath))
                        {
                            queue.Enqueue(next);
                            visited[next.x, next.y] = true;
                        }
                    }
                }
                _solutionRegions[regionIndex] = regionCells;
                regionIndex++;
            }
        }
        Debug.Log($"Generated puzzle with {_solutionPath.Count} solution nodes and {_solutionRegions.Count} regions");
        
        // Follow Rules
        _whiteCells = new List<Vector2Int>();
        _blackCells = new List<Vector2Int>();
        _requiredDots = new List<Vector2Int>();
        if (useWhiteBlackCells)
            AddWhiteBlackCells();
        if (useRequiredDots)            
            AddRequiredDots();
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
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        while (directions.Count > 0)
        {
            var directionIndex = Random.Range(0, directions.Count);
            var nextPosition = currentPosition + directions[directionIndex];
            if (IsNotVisited(nextPosition, visited))
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
    
    private bool IsNotVisited(Vector2Int position, bool[,] visited)
    {
        return position.x >= 0 && position.x < visited.GetLength(0) &&
               position.y >= 0 && position.y < visited.GetLength(1) &&
               !visited[position.x, position.y];
    }
    
    private static bool IsConnected(Vector2Int a, Vector2Int b, List<Vector2Int> path)
    {
        // Return false if a and b are not adjacent
        var delta = b - a;
        if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1)
            return false;

        // Determine the two nodes that would be connected by this edge
        Vector2Int n1, n2;
        if (delta == Vector2Int.right)
        {
            n1 = new Vector2Int(a.x + 1, a.y);
            n2 = new Vector2Int(a.x + 1, a.y + 1);
        }
        else if (delta == Vector2Int.left)
        {
            n1 = new Vector2Int(a.x, a.y);
            n2 = new Vector2Int(a.x, a.y + 1);
        }
        else if (delta == Vector2Int.up)
        {
            n1 = new Vector2Int(a.x, a.y + 1);
            n2 = new Vector2Int(a.x + 1, a.y + 1);
        }
        else // down
        {
            n1 = new Vector2Int(a.x, a.y);
            n2 = new Vector2Int(a.x + 1, a.y);
        }

        // Check if n1 and n2 are connected in the solution path
        for (var i = 0; i < path.Count - 1; i++)
        {
            var p1 = path[i];
            var p2 = path[i + 1];
            if ((p1 == n1 && p2 == n2) || (p1 == n2 && p2 == n1))
                return false;
        }
        return true;
    }
    
    private void RenderPuzzle()
    {
        // Render cells
        for (var i = 0; i < gridSize; i++)
        {
            for (var j = 0; j < gridSize; j++)
            {
                if (_whiteCells.Contains(new Vector2Int(i, j)))
                    Instantiate(whiteCellPrefab, new Vector3(i, j, 0) - Center,
                        Quaternion.identity, transform).transform.localScale = Vector3.one * cellSize;
                else if(_blackCells.Contains(new Vector2Int(i, j)))
                    Instantiate(blackCellPrefab, new Vector3(i, j, 0) - Center,
                        Quaternion.identity, transform).transform.localScale = Vector3.one * cellSize;
                else
                    Instantiate(cellPrefab, new Vector3(i, j, 0) - Center, 
                        Quaternion.identity, transform).transform.localScale = Vector3.one * cellSize;
            }
        }

        // Render nodes
        for (var x = 0; x < gridSize + 1; x++)
        {
            for (var y = 0; y < gridSize + 1; y++)
            {
                GameObject go;
                var nodePosition = new Vector3(x - 0.5f, y - 0.5f, 0) - Center;
                if (_startPosition == new Vector2Int(x, y))
                    go = Instantiate(startNodePrefab, nodePosition, Quaternion.identity, transform);
                else if (_endPosition == new Vector2Int(x, y))
                    go = Instantiate(endNodePrefab, nodePosition, Quaternion.identity, transform);
                else if (_requiredDots.Contains(new Vector2Int(x, y)))
                    go = Instantiate(dotPrefab, nodePosition, Quaternion.identity, transform);
                else
                    go = Instantiate(nodePrefab, nodePosition, Quaternion.identity, transform);
                go.transform.localScale = Vector3.one * nodeSize;
                go.transform.position += new Vector3(0, 0, -2f);
                go.AddComponent<Node>().gridPos = new Vector2Int(x, y);
            }
        }
    }
    
    private bool IsConnectRegions(List<Vector2Int> region1, List<Vector2Int> region2)
    {
        foreach (var cell1 in region1)
        {
            foreach (var cell2 in region2)
            {
                if (Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y) == 1)
                    return true;
            }
        }
        return false;
    }

    private void AddWhiteBlackCells()
    {
        // First region is always white
        var cnt = difficulty + 1;
        for (var i = 0; i < cnt; i++)
        {
            var cell = _solutionRegions[0][Random.Range(0, _solutionRegions[0].Count)];
            if (!_whiteCells.Contains(cell))
                _whiteCells.Add(cell);
        }
        
        // Other regions can be either white or black, but must follow the rule that connected regions have different colors
        for (var i = 1; i < _solutionRegions.Count; i++)
        {
            if (IsConnectRegions(_solutionRegions[0], _solutionRegions[i]))
            {
                for (var j = 0; j < cnt; j++)
                {
                    var cell = _solutionRegions[i][Random.Range(0, _solutionRegions[i].Count)];
                    if (!_blackCells.Contains(cell))
                        _blackCells.Add(cell);
                }
            }
            else
            {
                for (var j = 0; j < cnt; j++)
                {
                    var cell = _solutionRegions[i][Random.Range(0, _solutionRegions[i].Count)];
                    if (!_whiteCells.Contains(cell))
                        _whiteCells.Add(cell);
                }
            }
        }
    }
    
    private void AddRequiredDots()
    {
        var cnt = difficulty + 1;
        for (var i = 0; i < cnt; i++)
        {
            var cell = _solutionPath[Random.Range(0, _solutionPath.Count)];
            if (!_requiredDots.Contains(cell))
                _requiredDots.Add(cell);
        }
    }
    
    public bool ValidateResult(List<Vector2Int> playerPath)
    {
        if (useWhiteBlackCells && !CheckWhiteBlackCells(playerPath)) return false;
        if (useRequiredDots && !CheckRequiredDots(playerPath)) return false;
        return true;
    }
    
    private bool CheckWhiteBlackCells(List<Vector2Int> path)
    {
        // Split regions based on the solution path and check if the player path follows the white/black cell rules
        var regionIndex = 0;
        var solutionRegions = new Dictionary<int, List<Vector2Int>>();
        var visited = new bool[gridSize, gridSize];
        for (var i = 0; i < gridSize; i++)
        {
            for (var j = 0; j < gridSize; j++)
            {
                // Skip cells that are already visited or are part of the solution path
                if (visited[i, j])
                    continue;

                // Start a new region
                var queue = new Queue<Vector2Int>();
                var regionCells = new List<Vector2Int>();
                queue.Enqueue(new Vector2Int(i, j));
                visited[i, j] = true;
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    regionCells.Add(current);
                    var directions = new List<Vector2Int>
                    {
                        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                    };
                    foreach (var direction in directions)
                    {
                        var next = current + direction;
                        if (IsNotVisited(next, visited) && IsConnected(current, next, path))
                        {
                            queue.Enqueue(next);
                            visited[next.x, next.y] = true;
                        }
                    }
                }
                solutionRegions[regionIndex] = regionCells;
                regionIndex++;
            }
        }
        
        // Check if player path follows the white/black cell rules
        foreach (var region in solutionRegions.Values)
        {
            var hasWhiteCell = false;
            var hasBlackCell = false;
            foreach (var cell in region)
            {
                if (_whiteCells.Contains(cell) && !hasWhiteCell)
                    hasWhiteCell = true;
                if (_blackCells.Contains(cell) && !hasBlackCell)
                    hasBlackCell = true;
            }

            // If the region has both white and black cells, the player path must not pass through both types of cells in that region
            if (hasWhiteCell && hasBlackCell)
                return false;
        }
        return true;
    }
    
    private bool CheckRequiredDots(List<Vector2Int> path)
    {
        foreach (var dot in _requiredDots)
        {
            if (!path.Contains(dot))
                return false;
        }
        return true;
    }

}
