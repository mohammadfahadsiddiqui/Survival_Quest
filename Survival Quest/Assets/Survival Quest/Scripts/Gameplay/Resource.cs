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
        GameControl.m_Current.m_Resources.Add(this);
    }

    // Update is called once per frame
    void Update()
    {

        if(m_Shake)
        {
            m_ShakePoint.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 35) * 5, 0, 0);
        }


        
        HandleHealth();
    }

    public void HandleHealth()
    {
        if (m_Health <= 0)
        {
            GameControl.m_Current.m_DestroyedResource = this;
            GenerateResource();
            GameControl.m_Current.m_Resources.Remove(this);

            GameObject obj = Instantiate(m_BreakEffect);
            obj.transform.position = transform.position + new Vector3(0, 1, 0);
            Destroy(obj,3);

            Destroy(gameObject);
        }
    }

    public void GenerateResource()
    {
        for (int i = 0; i < m_ResourceCount; i++)
        {
            
            Vector3 pos = Random.insideUnitSphere;
            pos.y = 0;
            pos.Normalize();
            pos = 3 * pos;
            GameObject obj = Instantiate(m_ResourcePickUp);
            obj.transform.position = transform.position + new Vector3(0, 1f, 0)+pos;
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