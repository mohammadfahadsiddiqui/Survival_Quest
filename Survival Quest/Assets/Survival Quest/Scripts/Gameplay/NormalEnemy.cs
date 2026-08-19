using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class NormalEnemy : MonoBehaviour
{
    [HideInInspector]
    public float m_Health;

    public Rigidbody m_Body;

    public GameObject m_KillEffect;

    [HideInInspector]
    public bool m_CanMove;

    public float m_HitTimer;
    // Start is called before the first frame update
    void Start()
    {
        m_CanMove = true;
        m_HitTimer = 0;
        m_Health = 50;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.m_Current == null || !Player.m_Current.gameObject.activeInHierarchy)
        {
            if (m_Body != null)
            {
                m_Body.linearVelocity = Vector3.zero;
            }
            return;
        }

        Vector3 playerPos = Player.m_Current.transform.position;
        if (float.IsNaN(playerPos.x) || float.IsInfinity(playerPos.x) ||
            float.IsNaN(playerPos.y) || float.IsInfinity(playerPos.y) ||
            float.IsNaN(playerPos.z) || float.IsInfinity(playerPos.z))
        {
            return;
        }

        Vector3 dir = playerPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        float distance = Vector3.Distance(playerPos, transform.position);

        if (distance <= 2f)
        {
            if (m_Body != null)
            {
                m_Body.linearVelocity = Vector3.zero;
            }

            if (m_CanMove)
            {
                if (m_HitTimer <= 0)
                {
                    Player.m_Current.m_Health -= 5f;
                    Vector3 dir2 = playerPos - transform.position;
                    dir2.y = 0f;
                    if (dir2.sqrMagnitude > 0.001f)
                    {
                        dir2.Normalize();
                        Player.m_Current.HitImpulse(0.5f * dir2);
                    }
                    m_HitTimer = 2f;
                }
                else
                {
                    m_HitTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            if (m_CanMove && m_Body != null)
            {
                Vector3 movementDirection = playerPos - transform.position;
                movementDirection.y = 0f;
                if (movementDirection.sqrMagnitude > 0.001f)
                {
                    movementDirection.Normalize();
                    m_Body.linearVelocity = movementDirection * 5f;
                }
                else
                {
                    m_Body.linearVelocity = Vector3.zero;
                }
            }
        }

        HandleHealth();
    }


    public void HandleHealth()
    {
        if (m_Health <= 0f)
        {
            if (m_KillEffect != null)
            {
                GameObject obj = Instantiate(m_KillEffect);
                obj.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                Destroy(obj, 3f);
            }

            Destroy(gameObject);
        }
    }

    public void DisableMovement()
    {
        m_CanMove = false;
        StartCoroutine(StartMoving());
    }

    IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(3);
        m_CanMove = true;
    }
}
}