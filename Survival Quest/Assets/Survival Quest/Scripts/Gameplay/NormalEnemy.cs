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

        //Collider[] hit = Physics.OverlapSphere(transform.position, .7f);

        //foreach (Collider c in hit)
        //{
        //    if (c.gameObject.tag == "Player")
        //    {
        //        Player.m_Current.m_Health -= 10;
        //        Destroy(gameObject);
        //    }
        //}

        Vector3 dir = Player.m_Current.transform.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);

        float distance = Vector3.Distance(Player.m_Current.transform.position, transform.position);


        if(distance <= 2)
        {
            if(m_CanMove)
            {
                if (m_HitTimer <= 0)
                {
                    Player.m_Current.m_Health -= 5;
                    Vector3 dir2 = Player.m_Current.transform.position - transform.position;
                    dir2.y = 0;
                    dir2.Normalize();
                    Player.m_Current.HitImpulse(.5f*dir2);
                    m_HitTimer = 2;
                }
                else
                {
                    m_HitTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            if(m_CanMove)
            {
                Vector3 movementDirection = Player.m_Current.transform.position - transform.position;
                movementDirection.y = 0;
                movementDirection.Normalize();
                m_Body.linearVelocity = movementDirection * 5;
            }
        }
        

        HandleHealth();
    }


    public void HandleHealth()
    {
        if (m_Health <= 0)
        {
            GameObject obj = Instantiate(m_KillEffect);
            obj.transform.position = transform.position+new Vector3(0,1,0);
            Destroy(obj, 3);

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