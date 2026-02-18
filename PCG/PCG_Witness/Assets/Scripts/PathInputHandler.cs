using System.Collections.Generic;
using Data;
using UnityEngine;

public class PuzzleInputHandler : MonoBehaviour
{
    private GridRenderer _gridRenderer;
    private PathRenderer _pathRenderer;
    private PathValidator _validator;

    private PuzzleData _puzzle;
    private readonly List<GridNode> _path = new List<GridNode>();
    private bool _isDrawing;

    private void Start()
    {
        _gridRenderer = FindObjectOfType<GridRenderer>();
        _pathRenderer = FindObjectOfType<PathRenderer>();
        _validator = FindObjectOfType<PathValidator>();
    }

    private void Update()
    {
        if (_puzzle == null) return;

        var worldPos = GetWorldMousePos();

        // 开始绘制：鼠标按下且命中起点
        if (Input.GetMouseButtonDown(0))
        {
            if (_gridRenderer.TryGetNearestNode(worldPos, out GridNode node))
            {
                if (node.Equals(_puzzle.StartNode))
                {
                    _isDrawing = true;
                    _path.Clear();
                    _path.Add(node);
                    _pathRenderer.UpdatePath(_path);
                }
            }
        }

        // 绘制中：鼠标移动
        if (_isDrawing && Input.GetMouseButton(0))
        {
            if (_gridRenderer.TryGetNearestNode(worldPos, out GridNode node))
                TryExtendPath(node);
        }

        // 松开鼠标：停止绘制
        if (Input.GetMouseButtonUp(0))
        {
            _isDrawing = false;
        }
    }

    void TryExtendPath(GridNode node)
    {
        if (_path.Count == 0) return;
        var last = _path[_path.Count - 1];

        // 回退（后退一步）
        if (_path.Count >= 2 && node.Equals(_path[_path.Count - 2]))
        {
            _path.RemoveAt(_path.Count - 1);
            _pathRenderer.UpdatePath(_path);
            return;
        }

        // 必须与最后一个节点相邻
        if (!node.IsAdjacentTo(last)) return;

        // 不能经过断裂边
        var edge = new GridEdge(last, node);
        if (_puzzle.BrokenEdges.Contains(edge)) return;

        // 不能重复访问
        if (_path.Contains(node)) return; // 小网格直接 Contains 就够

        _path.Add(node);
        _pathRenderer.UpdatePath(_path);

        // 到达终点：校验
        if (node.Equals(_puzzle.EndNode))
        {
            _isDrawing = false;
            var errors = _validator.Validate(_path, _puzzle);
            if (errors.Count == 0)
                OnPuzzleSolved();
            else
                OnPuzzleFailed(errors[0]);
        }
    }

    public void SetPuzzle(PuzzleData puzzle)
    {
        _puzzle = puzzle;
        _path.Clear();
        _isDrawing = false;
        _pathRenderer.Clear();
    }

    void OnPuzzleSolved()
    {
        Debug.Log("✓ 解题成功！");
        // TODO: 播放成功动画、粒子效果
    }

    void OnPuzzleFailed(string reason)
    {
        Debug.Log($"✗ 错误：{reason}");
        // 短暂延迟后清空路径
        Invoke(nameof(ResetPath), 0.6f);
    }

    public void ResetPath()
    {
        _path.Clear();
        _pathRenderer.Clear();
    }

    private static Vector3 GetWorldMousePos()
    {
        var mp = Input.mousePosition;
        mp.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mp);
    }
}