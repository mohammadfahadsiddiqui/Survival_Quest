using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class Trap : MonoBehaviour
{
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
        if (other != null && other.gameObject.CompareTag("NormalEnemy"))
        {
            NormalEnemy enemy = other.gameObject.GetComponent<NormalEnemy>();
            if (enemy == null)
            {
                enemy = other.gameObject.GetComponentInParent<NormalEnemy>();
            }
            if (enemy != null)
            {
                enemy.DisableMovement();
            }
            Destroy(gameObject, 1f);
        }
    }
}
}