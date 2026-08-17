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
        if(other.gameObject.tag == "NormalEnemy")
        {
            other.gameObject.GetComponent<NormalEnemy>().DisableMovement();
            Destroy(gameObject, 1);
        }
    }
}
}