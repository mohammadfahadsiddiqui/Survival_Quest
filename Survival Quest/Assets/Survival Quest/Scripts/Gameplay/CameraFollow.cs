using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame
{
    public class CameraFollow : MonoBehaviour
    {
        public GameObject m_Target;

        public float m_SmoothSpeed = 5f;
        public Vector3 m_Offset;

        void LateUpdate()
        {
            if (m_Target == null) return;

            Vector3 targetPos = m_Target.transform.position;
            if (float.IsNaN(targetPos.x) || float.IsInfinity(targetPos.x) ||
                float.IsNaN(targetPos.y) || float.IsInfinity(targetPos.y) ||
                float.IsNaN(targetPos.z) || float.IsInfinity(targetPos.z))
                return;

            Vector3 desiredPosition = targetPos + m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_SmoothSpeed * Time.deltaTime);

            if (!float.IsNaN(smoothedPosition.x) && !float.IsInfinity(smoothedPosition.x) &&
                !float.IsNaN(smoothedPosition.y) && !float.IsInfinity(smoothedPosition.y) &&
                !float.IsNaN(smoothedPosition.z) && !float.IsInfinity(smoothedPosition.z))
            {
                transform.position = smoothedPosition;
            }
        }
    }
}