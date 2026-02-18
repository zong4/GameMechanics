using Data;
using Renderer;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    // Puzzle
    public int gridSize = 5;
    public int difficulty = 3;
    public bool useBlackWhite = true;
    public bool useStars = true;
    public bool useDots = true;
    public bool useBrokenEdges = true;
    private PuzzleData _currentPuzzle;
    private GridRenderer _gridRenderer;
    private PathRenderer _pathRenderer;
    private CellRenderer _cellRenderer;
    private PuzzleInputHandler _inputHandler;

    // UI
    public Text statusText;
    public Button generateBtn;
    public Button resetBtn;

    private void Awake()
    {
        generateBtn.onClick.AddListener(GenerateNew);
        resetBtn.onClick.AddListener(() => _inputHandler.ResetPath());
    }

    private void Start()
    {
        _gridRenderer = FindObjectOfType<GridRenderer>();
        _cellRenderer = FindObjectOfType<CellRenderer>();
        _inputHandler = FindObjectOfType<PuzzleInputHandler>();
    }

    private void Update()
    {
        if (_currentPuzzle == null)
            GenerateNew();
    }

    private void GenerateNew()
    {
        _currentPuzzle =
            PuzzleGenerator.Generate(gridSize, difficulty, useBlackWhite, useStars, useDots, useBrokenEdges);
        _gridRenderer.Render(_currentPuzzle);
        _cellRenderer.Render(_currentPuzzle);
        _inputHandler.SetPuzzle(_currentPuzzle);
        SetStatus("Draw a path from the start node to the end node, following the rules!");
        FitCamera(_currentPuzzle.gridSize);
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    private static void FitCamera(int gridSize, float cellSize = 1f)
    {
        var halfH = (gridSize * cellSize) / 2f + cellSize * 0.8f;
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = halfH;
            var center = (gridSize * cellSize) / 2f;
            Camera.main.transform.position = new Vector3(center, -center, -10f);
        }
    }
}