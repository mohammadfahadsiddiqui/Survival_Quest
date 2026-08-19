using UnityEngine;
using System.Collections;
using SurvivalGame.UI;

namespace SurvivalGame
{
    public class JoystickButton : MonoBehaviour
    {
        [HideInInspector]
        public bool Hold;
        [HideInInspector]
        public bool PreHold;
        [HideInInspector]
        public bool Pressed;

        [HideInInspector]
        public Joystick MyJoystick;

        public Transform ButtonShape;

        [HideInInspector]
        public Vector3 HitPosition;

        private Camera m_CachedCam;

        void Start()
        {
            MyJoystick = Joystick.m_Main;
            Hold = false;
        }

        void Update()
        {
            Hold = false;
            Pressed = false;

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
                // On PC/Editor, only process pointer if mouse button is down or held
                if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
                {
                    PointerPos = new Vector3[1];
                    PointerPos[0] = Input.mousePosition;
                }
                else
                {
                    PointerPos = new Vector3[0];
                }
            }

            bool TempHold = false;
            HitPosition = Vector3.zero;

            if (m_CachedCam == null)
            {
                if (UISystem.m_Main != null)
                    m_CachedCam = UISystem.m_Main.GetComponentInChildren<Camera>();
                if (m_CachedCam == null)
                    m_CachedCam = Camera.main;
            }

            if (m_CachedCam != null && m_CachedCam.pixelWidth > 0 && m_CachedCam.pixelHeight > 0 && PointerPos.Length > 0)
            {
                Rect camRect = m_CachedCam.pixelRect;
                if (camRect.width > 0 && camRect.height > 0)
                {
                    for (int i = 0; i < PointerPos.Length; i++)
                    {
                        Vector3 p = PointerPos[i];
                        if (float.IsNaN(p.x) || float.IsInfinity(p.x) || float.IsNaN(p.y) || float.IsInfinity(p.y))
                            continue;

                        // Bounds check against camera viewport rect
                        if (!camRect.Contains(p))
                            continue;

                        Ray ray = m_CachedCam.ScreenPointToRay(p);
                        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("UI"));

                        foreach (RaycastHit r in hits)
                        {
                            if (r.collider != null && r.collider.gameObject == gameObject)
                            {
                                TempHold = true;
                                HitPosition = r.point;
                                break;
                            }
                        }

                        if (TempHold) break;
                    }
                }
            }

            if (TempHold)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (!PreHold)
                    {
                        Pressed = true;
                    }
                    Hold = true;
                    PreHold = true;
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Pressed = true;
                    }

                    if (Input.GetMouseButton(0))
                    {
                        Hold = true;
                    }
                }
            }
            else
            {
                PreHold = false;
                Hold = false;
            }

            if (ButtonShape != null)
            {
                if (Hold)
                {
                    ButtonShape.localScale = 0.9f * Vector3.one;
                }
                else
                {
                    ButtonShape.localScale = Vector3.one;
                }
            }
        }
    }
}