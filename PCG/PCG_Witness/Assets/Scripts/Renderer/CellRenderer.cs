using Data;
using UnityEngine;

public class CellRenderer : MonoBehaviour
{
    public GameObject whiteCellPrefab;
    public GameObject blackCellPrefab;
    public GameObject starGoldPrefab;
    public GameObject starTealPrefab;
    public GameObject dotPrefab;
    private GridRenderer _grid;

    public void SetGrid(GridRenderer grid) => _grid = grid;

    public void Render(PuzzleData puzzle)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        var cell = _grid.GetComponent<GridRenderer>() ? 1f : 1f; // cellSize

        // 格子符号（黑白块、星星）
        foreach (var kv in puzzle.Cells)
        {
            var parts = kv.Key.Split(',');
            int r = int.Parse(parts[0]), c = int.Parse(parts[1]);

            // 格子中心 = 两个对角节点的中点
            var tl = _grid.NodeToWorld(new GridNode(r, c));
            var br = _grid.NodeToWorld(new GridNode(r + 1, c + 1));
            var center = (tl + br) * 0.5f;

            GameObject prefab = kv.Value switch
            {
                CellType.White => whiteCellPrefab,
                CellType.Black => blackCellPrefab,
                CellType.StarGold => starGoldPrefab,
                CellType.StarTeal => starTealPrefab,
                _ => null
            };
            if (prefab == null) continue;

            var go = Instantiate(prefab, center, Quaternion.identity, transform);
            // 根据 cellSize 缩放（留出边距 0.7）
            go.transform.localScale = Vector3.one * 0.7f;
        }

        // 强制点
        foreach (var dotKey in puzzle.RequiredDots)
        {
            var pos = _grid.NodeToWorld(dotKey);
            Instantiate(dotPrefab, pos + Vector3.back * 0.01f, Quaternion.identity, transform);
        }
    }
}