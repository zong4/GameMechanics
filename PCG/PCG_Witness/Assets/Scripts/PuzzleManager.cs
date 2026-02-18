using Data;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [Header("子系统引用")]
    public PuzzleGenerator generator;
    public GridRenderer gridRenderer;
    public PathRenderer pathRenderer;
    public CellRenderer cellRenderer;
    public PuzzleInputHandler inputHandler;
    public PathValidator validator;

    [Header("UI")]
    public Text statusText;
    public Button generateBtn;
    public Button resetBtn;

    PuzzleData _currentPuzzle;

    private void Start()
    {
        generateBtn.onClick.AddListener(GenerateNew);
        resetBtn.onClick.AddListener(() => inputHandler.ResetPath());
        pathRenderer.SetGrid(gridRenderer);
        cellRenderer.SetGrid(gridRenderer);
        GenerateNew();
    }

    private void GenerateNew()
    {
        _currentPuzzle = generator.Generate();
        if (_currentPuzzle == null) return;

        gridRenderer.Render(_currentPuzzle);
        cellRenderer.Render(_currentPuzzle);
        inputHandler.SetPuzzle(_currentPuzzle);

        SetStatus("从 ● 画线到 ◎");
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }
}