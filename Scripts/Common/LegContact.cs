using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegContact : MonoBehaviour {

    public int legIndex;
	public RobotAgent agent;
	
	void OnCollisionStay(Collision other)
	{
		if (other.gameObject.name == "Ground" || other.gameObject.tag == "Obstacle")
		{
			agent.legsTouching[legIndex] = true;
		}
	}
}
