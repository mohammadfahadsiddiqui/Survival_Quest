using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame
{
    [ExecuteAlways]
    public class CanvasSizeFix : MonoBehaviour
    {
        private RectTransform m_RectTransform;
        private int m_LastScreenWidth = -1;
        private int m_LastScreenHeight = -1;

        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            UpdateCanvasSize();
        }

        void Start()
        {
            UpdateCanvasSize();
        }

        void Update()
        {
            if (Screen.width != m_LastScreenWidth || Screen.height != m_LastScreenHeight)
            {
                UpdateCanvasSize();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCanvasSize();
        }

        public void UpdateCanvasSize()
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();

            if (m_RectTransform == null) return;

            float screenW = Screen.width > 0 ? (float)Screen.width : 1920f;
            float screenH = Screen.height > 0 ? (float)Screen.height : 1080f;

            m_LastScreenWidth = Screen.width;
            m_LastScreenHeight = Screen.height;

            float ratio = screenW / screenH;
            if (float.IsNaN(ratio) || float.IsInfinity(ratio) || ratio <= 0f)
            {
                ratio = 16f / 9f;
            }

            m_RectTransform.sizeDelta = new Vector2(ratio * 900f, 900f);
        }
    }
}