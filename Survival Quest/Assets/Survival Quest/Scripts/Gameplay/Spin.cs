using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
public class Spin : MonoBehaviour {

    public Vector3 m_Speed = Vector3.zero;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.localRotation *= Quaternion.Euler(Time.deltaTime* m_Speed);
	}
}
}