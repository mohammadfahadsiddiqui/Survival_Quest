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
        //       