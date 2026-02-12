using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    public class PageLayout : MonoBehaviour
    {
        public enum Type
        {
            Horizontal,
            Vertical,
        }

        public Type type = Type.Horizontal;
        public List<Vector2> sizes;
        private int _childrenCount;

        private void Awake()
        {
            _childrenCount = transform.childCount;
            CreateSizes();
        }

        private void Update()
        {
            if (_childrenCount != transform.childCount)
            {
                _childrenCount = transform.childCount;
                CreateSizes();
            }


            var start = new Vector2(0, 1);
            for (var i = 0; i < _childrenCount; i++)
            {
                var rectTransform = transform.GetChild(i).GetComponent<RectTransform>();

                switch (type)
                {
                    case Type.Horizontal:
                        rectTransform.anchorMin = new Vector2(start.x, 0.5f - sizes[i].y * 0.5f);
                        rectTransform.anchorMax = new Vector2(start.x + sizes[i].x, 0.5f + sizes[i].y * 0.5f);
                        break;
                    case Type.Vertical:
                        rectTransform.anchorMin = new Vector2(0.5f - sizes[i].x * 0.5f, start.y - sizes[i].y);
                        rectTransform.anchorMax = new Vector2(0.5f + sizes[i].x * 0.5f, start.y);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                start = new Vector2(rectTransform.anchorMax.x, rectTransform.anchorMin.y);
            }
        }

        private void CreateSizes()
        {
            sizes = new List<Vector2>(_childrenCount);
            for (var i = 0; i < _childrenCount; i++)
            {
                var rectTransform = transform.GetChild(i).GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                sizes.Add(rectTransform.anchorMax - rectTransform.anchorMin);
            }
        }
    }
}