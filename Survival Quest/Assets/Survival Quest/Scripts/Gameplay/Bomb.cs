using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class Bomb : MonoBehaviour
{
    public GameObject m_ExplodePrefab;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Explode", 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Explode()
    {
        if (m_ExplodePrefab != null)
        {
            GameObject obj = Instantiate(m_ExplodePrefab);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
        }

        Destroy(gameObject);
    }
}
}