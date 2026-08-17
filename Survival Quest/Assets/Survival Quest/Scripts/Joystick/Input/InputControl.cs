using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame
{
    public class InputControl : MonoBehaviour
    {
        [HideInInspector]
        public Vector3 m_WorldAimPosition;

        //--inputs
        [HideInInspector]
        public Vector3 m_Movement;
        [HideInInspector]
        public bool m_Fire;

        public Camera m_WorldCamera;

        public static InputControl m_Main;

        public ControlSetting m_Setting;

        void Awake()
        {
            m_Main = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_WorldAimPosition = Vector3.zero;
            if (m_WorldCamera == null)
            {
                m_WorldCamera = Camera.main;
            }
        }

        // Update is called once per frame
        void Update()
        {
            m_Movement = Vector3.zero;
            m_Fire = false;

            if (m_Setting != null && m_Setting.ControlType == PlatformControlType.PC)
            {
                m_Movement.x = Input.GetAxis("Horizontal");
                m_Movement.z = Input.GetAxis("Vertical");

                if (m_Setting.AimType == AimType.Full360)
                {
                    if (Input.GetMouseButton(0))
                        m_Fire = true;
                }
                else
                {
                    if (Input.GetKey(KeyCode.Z))
                        m_Fire = true;
                }

                if (m_WorldCamera != null)
                {
                    Vector3 mousePos = Input.mousePosition;
                    if (!float.IsNaN(mousePos.x) && !float.IsInfinity(mousePos.x) &&
                        !float.IsNaN(mousePos.y) && !float.IsInfinity(mousePos.y))
                    {
                        float screenW = Screen.width > 0 ? (float)Screen.width : 1920f;
                        float screenH = Screen.height > 0 ? (float)Screen.height : 1080f;
                        if (mousePos.x >= 0 && mousePos.x <= screenW && mousePos.y >= 0 && mousePos.y <= screenH)
                        {
                            Ray ray = m_WorldCamera.ScreenPointToRay(mousePos);
                            float dis = 0;
                            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out dis))
                            {
                                m_WorldAimPosition = ray.origin + dis * ray.direction;
                            }
                        }
                    }
                }
            }
            else
            {
                if (Joystick.m_Main != null)
                {
                    if (Joystick.m_Main.LeftStick != null)
                    {
                        m_Movement.x = Joystick.m_Main.LeftStick.StickDirection.x;
                        m_Movement.z = Joystick.m_Main.LeftStick.StickDirection.y;
                    }

                    if (Joystick.m_Main.ButtonA != null && Joystick.m_Main.ButtonA.Hold)
                        m_Fire = true;
                }
            }

            m_Movement = Vector3.ClampMagnitude(m_Movement, 1.0f);
        }
    }
}