using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    public class AnchorsAdjuster : MonoBehaviour
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        private RectTransform _rectTransform;

        private void Awake()
        {
            // Clear position and size
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.sizeDelta = Vector2.zero;

            // Prevent zero size
            anchorMin = _rectTransform.anchorMin;
            anchorMax = _rectTransform.anchorMax;
            if (anchorMin == anchorMax)
            {
                anchorMin -= new Vector2(0.5f, 0.5f);
                anchorMax += new Vector2(0.5f, 0.5f);
            }
        }

        private void Update()
        {
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}