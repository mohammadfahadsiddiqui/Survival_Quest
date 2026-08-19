using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace SurvivalGame
{
public class Resource : MonoBehaviour
{
    [HideInInspector]
    public float m_Health;

    public float m_MaxHealth = 100;

    [HideInInspector]
    Transform m_Position;

    public Transform m_ShakePoint;

    [HideInInspector]
    public bool m_Shake;

    public GameObject m_ResourcePickUp;
    public int m_ResourceCount = 1;

    public GameObject m_BreakEffect;
    // Start is called before the first frame update
    void Start()
    {
        m_Shake = false;
        m_Health = m_MaxHealth;
        m_Position = transform;
        if (GameControl.m_Current != null)
        {
            if (GameControl.m_Current.m_Resources == null)
            {
                GameControl.m_Current.m_Resources = new List<Resource>();
            }
            GameControl.m_Current.m_Resources.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Shake && m_ShakePoint != null)
        {
            m_ShakePoint.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 35f) * 5f, 0f, 0f);
        }

        HandleHealth();
    }

    public void HandleHealth()
    {
        if (m_Health <= 0)
        {
            if (GameControl.m_Current != null)
            {
                GameControl.m_Current.m_DestroyedResource = this;
                if (GameControl.m_Current.m_Resources != null)
                {
                    GameControl.m_Current.m_Resources.Remove(this);
                }
            }

            GenerateResource();

            if (m_BreakEffect != null)
            {
                GameObject obj = Instantiate(m_BreakEffect);
                obj.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                Destroy(obj, 3f);
            }

            Destroy(gameObject);
        }
    }

    public void GenerateResource()
    {
        if (m_ResourcePickUp == null) return;

        for (int i = 0; i < m_ResourceCount; i++)
        {
            Vector2 circle = Random.insideUnitCircle;
            if (circle.sqrMagnitude < 0.001f)
                circle = Vector2.right;
            else
                circle.Normalize();

            Vector3 pos = new Vector3(circle.x, 0f, circle.y) * 3f;
            GameObject obj = Instantiate(m_ResourcePickUp);
            obj.transform.position = transform.position + new Vector3(0f, 1f, 0f) + pos;
        }
    }

    public void HandleHit()
    {
        m_Shake = true;
        StartCoroutine(Co_StopShaking());
    }

    IEnumerator Co_StopShaking()
    {
        yield return new WaitForSeconds(.5f);
        m_Shake = false;
        m_ShakePoint.localRotation = Quaternion.identity;
    }
}
}