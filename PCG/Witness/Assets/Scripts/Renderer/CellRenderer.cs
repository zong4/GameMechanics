using Data;
using UnityEngine;

namespace Renderer
{
    public class CellRenderer : MonoBehaviour
    {
        public GameObject whiteCellPrefab;
        public GameObject blackCellPrefab;
        public GameObject starGoldPrefab;
        public GameObject starTealPrefab;
        public GameObject dotPrefab;
        private GridRenderer _gridRenderer;

        private void Start()
        {
            _gridRenderer = FindObjectOfType<GridRenderer>();
        }

        public void Render(PuzzleData puzzle)
        {
            // Destroy old cells
            foreach (Transform child in transform) Destroy(child.gameObject);

            // Cells
            foreach (var kv in puzzle.cells)
            {
                var parts = kv.Key.Split(',');
                int r = int.Parse(parts[0]), c = int.Parse(parts[1]);
                var tl = GridRenderer.NodeToWorld(new GridNode(r, c));
                var br = GridRenderer.NodeToWorld(new GridNode(r + 1, c + 1));
                var center = (tl + br) * 0.5f;
                var prefab = kv.Value switch
                {
                    CellType.White => whiteCellPrefab,
                    CellType.Black => blackCellPrefab,
                    CellType.StarGold => starGoldPrefab,
                    CellType.StarTeal => starTealPrefab,
                    _ => null
                };
                if (!prefab) continue;
                var go = Instantiate(prefab, center, Quaternion.identity, transform);
                go.transform.localScale = Vector3.one * 0.7f;
            }

            // Required dots
            foreach (var dotKey in puzzle.requiredDots)
            {
                var pos = GridRenderer.NodeToWorld(dotKey);
                Instantiate(dotPrefab, pos + Vector3.back * 0.01f, Quaternion.identity, transform);
            }
        }
    }
}