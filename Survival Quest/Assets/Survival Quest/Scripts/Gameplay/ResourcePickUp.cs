using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class ResourcePickUp : MonoBehaviour
{
    public int m_ID;

    public GameObject m_PickParticlePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameControl.m_Current.m_Data.m_Resources[m_ID]++;
            GameControl.m_Current.m_Data.SaveData();

            GameObject obj = Instantiate(m_PickParticlePrefab);
            obj.transform.position = transform.position;
            Destroy(obj, 3);

            Destroy(gameObject);
        }
    }
}
}