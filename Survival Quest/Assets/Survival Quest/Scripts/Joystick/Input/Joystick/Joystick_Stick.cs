using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame
{
    public class Joystick_Stick : MonoBehaviour
    {
        public Image Back;
        public Image Stick;

        [HideInInspector]
        public bool Hold;
        [HideInInspector]
        public Vector3 HitPosition;
        [HideInInspector]
        public Vector3 StickDirection;

        [HideInInspector]
        public Vector3 m_OriginPosition;

        [HideInInspector]
        public Vector2 m_StartPosition;

        [HideInInspector]
        public bool m_PrevTouch = false;

        public RectTransform m_MainRect;

        public bool m_Left = true;

        void Start()
        {
            Hold = false;
            if (Back != null && Back.rectTransform != null)
                m_StartPosition = Back.rectTransform.anchoredPosition;
        }

        void Update()
        {
            Hold = false;
            Vector3[] PointerPos;

            if (Application.platform == RuntimePlatform.Android)
            {
                PointerPos = new Vector3[Input.touchCount];
                for (int i = 0; i < Input.touchCount; i++)
                {
                    PointerPos[i] = Input.touches[i].position;
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    PointerPos = new Vector3[1];
                    PointerPos[0] = Input.mousePosition;
                }
                else
                {
                    PointerPos = new Vector3[0];
                }
            }

            HitPosition = Vector3.zero;
            bool found = false;
            Vector3 foundPos = Vector3.zero;

            float screenW = Screen.width > 0 ? (float)Screen.width : 1920f;
            float screenH = Screen.height > 0 ? (float)Screen.height : 1080f;

            for (int i = 0; i < PointerPos.Length; i++)
            {
                Vector3 p = PointerPos[i];
                if (float.IsNaN(p.x) || float.IsInfinity(p.x) || float.IsNaN(p.y) || float.IsInfinity(p.y))
                    continue;

                if ((m_Left && p.x < 0.5f * screenW) || (!m_Left && p.x > 0.5f * screenW))
                {
                    foundPos = p;
                    found = true;
                    if (!m_PrevTouch)
                    {
                        m_PrevTouch = true;
                        m_OriginPosition = p;
                    }
                    break;
                }
            }

            if (!found)
            {
                m_PrevTouch = false;
                StickDirection = Vector3.zero;

                if (Back != null && Back.rectTransform != null)
                    Back.rectTransform.anchoredPosition = m_StartPosition;
                if (Stick != null && Stick.rectTransform != null)
                    Stick.rectTransform.anchoredPosition = m_StartPosition;
            }
            else
            {
                Vector2 mainSize = m_MainRect != null ? m_MainRect.sizeDelta : new Vector2(screenW, screenH);
                if (float.IsNaN(mainSize.x) || float.IsInfinity(mainSize.x) || mainSize.x <= 0)
                    mainSize.x = 1600f;
                if (float.IsNaN(mainSize.y) || float.IsInfinity(mainSize.y) || mainSize.y <= 0)
                    mainSize.y = 900f;

                // back
                Vector3 pos = m_OriginPosition;
                pos.z = 0;
                pos.x = pos.x / screenW;
                pos.y = pos.y / screenH;

                Vector2 p2 = Vector2.zero;
                p2.x = pos.x * mainSize.x;
                p2.y = pos.y * mainSize.y;

                if (!float.IsNaN(p2.x) && !float.IsInfinity(p2.x) && !float.IsNaN(p2.y) && !float.IsInfinity(p2.y))
                {
                    if (Back != null && Back.rectTransform != null)
                        Back.rectTransform.anchoredPosition = p2;
                }

                // stick
                if (Stick != null)
                {
                    Stick.enabled = true;
                    pos = foundPos;
                    pos.z = 0;
                    pos.x = pos.x / screenW;
                    pos.y = pos.y / screenH;

                    p2 = Vector2.zero;
                    p2.x = pos.x * mainSize.x;
                    p2.y = pos.y * mainSize.y;

                    if (!float.IsNaN(p2.x) && !float.IsInfinity(p2.x) && !float.IsNaN(p2.y) && !float.IsInfinity(p2.y))
                    {
                        if (Stick.rectTransform != null)
                            Stick.rectTransform.anchoredPosition = p2;
                    }
                }

                float MaxDistance = screenH / 5f;
                if (MaxDistance > 0.0001f)
                {
                    StickDirection = foundPos - m_OriginPosition;
                    StickDirection = StickDirection / MaxDistance;
                    StickDirection = Vector3.ClampMagnitude(StickDirection, 1);

                    Vector3 dir = foundPos - m_OriginPosition;
                    if (dir.magnitude > MaxDistance)
                    {
                        m_OriginPosition = Vector3.Lerp(m_OriginPosition, foundPos, 5 * Time.deltaTime);
                    }
                }
            }
        }
    }
}
