using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame

{
    public class Explosion : MonoBehaviour
    {

        // Use this for initialization
        public float Radius = 5;
        public float M_Damage = 10;
        void Start()
        {
            //CameraControl.MainCameraControl.StartShake(1f, 1f);

            //if (transform.position.y <= 2)
            //{
            //    GameObject obj = Instantiate(GlobalContents.MainGlobalContent.DecalsPrefabs[0]);
            //    Vector3 pos = transform.position;
            //    pos.y = 0.1f;
            //    obj.transform.position = pos;
            //    Destroy(obj, 30);
            //}


            Collider[] colls = Physics.OverlapSphere(transform.position, Radius);
            foreach (Collider col in colls)
            {
                if (col == null || col.gameObject == null) continue;

                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 forceDir = col.transform.position - transform.position;
                    if (forceDir.sqrMagnitude > 0.001f)
                    {
                        rb.AddForceAtPosition(forceDir.normalized * 5f, transform.position, ForceMode.Impulse);
                    }
                }

                switch (col.gameObject.tag)
                {
                    case "Resource":
                        Resource res = col.gameObject.GetComponent<Resource>();
                        if (res != null)
                        {
                            res.m_Health -= M_Damage;
                            res.HandleHit();
                        }
                        break;
                    case "NormalEnemy":
                        NormalEnemy enemy = col.gameObject.GetComponent<NormalEnemy>();
                        if (enemy != null)
                        {
                            enemy.m_Health -= M_Damage;
                        }
                        break;
                }
            }

            //Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}