using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class SpawnPoint : MonoBehaviour
{

    public GameObject m_EnemyPrefab;

    [HideInInspector]
    public bool m_StartSpawning;

    private float m_SpawnTimer;
    // Start is called before the first frame update
    void Start()
    {
        m_SpawnTimer = Random.Range(3f, 7f);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_StartSpawning)
        {
            if (m_SpawnTimer <= 0f)
            {
                if (m_EnemyPrefab != null)
                {
                    GameObject obj = Instantiate(m_EnemyPrefab);
                    obj.transform.position = transform.position;
                }
                m_SpawnTimer = Random.Range(3f, 7f);
            }
            else
            {
                m_SpawnTimer -= Time.deltaTime;
            }
        }
        else
        {
            m_SpawnTimer = Random.Range(3f, 7f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.CompareTag("Player"))
        {
            m_StartSpawning = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject.CompareTag("Player"))
        {
            m_StartSpawning = false;
        }
    }
}
}