using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Renderer
{
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderer : MonoBehaviour
    {
        public Color pathColor = new Color(0.91f, 0.875f, 0.78f);
        public float pathWidth = 0.15f;
        private LineRenderer _lr;
        private GridRenderer _gridRenderer;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.startColor = _lr.endColor = pathColor;
            _lr.startWidth = _lr.endWidth = pathWidth;
            _lr.numCapVertices = 8; // Round corners
            _lr.numCornerVertices = 8;
        }

        private void Start()
        {
            _gridRenderer = FindObjectOfType<GridRenderer>();
        }

        public void UpdatePath(List<GridNode> path)
        {
            _lr.positionCount = path.Count;
            for (var i = 0; i < path.Count; i++)
                _lr.SetPosition(i, GridRenderer.NodeToWorld(path[i]));
        }

        public void Clear()
        {
            _lr.positionCount = 0;
        }
    }
}