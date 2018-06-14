using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyContact : MonoBehaviour {

	private RobotAgent agent;

	void Start ()
	{
		agent = gameObject.GetComponentInParent<RobotAgent>();	
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Ground")
		{
			agent.fell = true;
		}
	}
}
