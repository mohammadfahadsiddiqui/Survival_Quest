using System.Collections;
using UnityEngine;

namespace SurvivalGame
{
    public class NormalEnemy : MonoBehaviour
    {
        [HideInInspector] public float m_Health;
        public Rigidbody m_Body;
        public GameObject m_KillEffect;
        [HideInInspector] public bool m_CanMove;
        public float m_HitTimer;

        private Player m_Target;

        void Awake()
        {
            if (m_Body == null)
                m_Body = GetComponent<Rigidbody>();

            if (m_Body == null)
                m_Body = gameObject.AddComponent<Rigidbody>();

            m_Body.constraints = RigidbodyConstraints.FreezeRotation;
            m_Target = Player.m_Current;
        }

        void Start()
        {
            m_CanMove = true;
            m_HitTimer = 0f;
            m_Health = 50f;

            if (m_Target == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                    m_Target = playerObject.GetComponent<Player>();
            }
        }

        void Update()
        {
            if (m_Target == null || !m_CanMove)
                return;

            Vector3 dir = m_Target.transform.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

            float distance = Vector3.Distance(m_Target.transform.position, transform.position);

            if (distance <= 2f)
            {
                if (m_HitTimer <= 0f)
                {
                    m_Target.m_Health -= 5f;
                    Vector3 hitDirection = m_Target.transform.position - transform.position;
                    hitDirection.y = 0f;
                    if (hitDirection.sqrMagnitude > 0.0001f)
                    {
                        hitDirection.Normalize();
                        m_Target.HitImpulse(.5f * hitDirection);
                    }
                    m_HitTimer = 2f;
                }
                else
                {
                    m_HitTimer -= Time.deltaTime;
                }
            }
            else
            {
                Vector3 movementDirection = m_Target.transform.position - transform.position;
                movementDirection.y = 0f;
                if (movementDirection.sqrMagnitude > 0.0001f)
                {
                    movementDirection.Normalize();
                    if (m_Body != null)
                        m_Body.linearVelocity = movementDirection * 5f;
                }
                else if (m_Body != null)
                {
                    m_Body.linearVelocity = Vector3.zero;
                }
            }

            HandleHealth();
        }

        public void HandleHealth()
        {
            if (m_Health > 0f) return;

            if (m_KillEffect != null)
            {
                GameObject obj = Instantiate(m_KillEffect, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
                Destroy(obj, 3f);
            }

            Destroy(gameObject);
        }

        public void DisableMovement()
        {
            m_CanMove = false;
            StartCoroutine(StartMoving());
        }

        IEnumerator StartMoving()
        {
            yield return new WaitForSeconds(3f);
            m_CanMove = true;
        }
    }
}