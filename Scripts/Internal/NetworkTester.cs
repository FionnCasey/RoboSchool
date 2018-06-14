using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTester : InternalAgent {

	public float speed = 10f;
	public int numRaycasts = 8;
	public float raycastDistance;

	public Transform goal;
	public float minDistance;
	public float maxDistance;

	private Rigidbody rb;

	public override void Initialise()
	{
		rb = GetComponent<Rigidbody>();
		experiences = new List<Experience>();
		savedState = new List<double>();
		brain.RegisterAgent(this);
		Reset();
	}

	public override void Step(double[] actions)
	{
		float[] act = new float[actions.Length];
		for (int i = 0; i < actions.Length; i++)
        {
            act[i] = Mathf.Clamp((float)actions[i], -1f, 1f);
        }
		rb.AddForce(act[0], act[1], 0);
	}

	public override List<double> CollectState()
	{
		List<double> state = new List<double>();

		for (int i = 0; i <= 360; i += numRaycasts)
		{
			RaycastHit hit;
			//Ray ray = new Ray(transform.position, );
		}

		return state;
	}

	public override void Reset()
	{
		reward = 0;
		done = false;

		float x = Random.Range(minDistance, maxDistance);
		float y = Random.Range(minDistance, maxDistance);
		x *= Random.value < .5f ? -1 : 1;
		y *= Random.value < .5f ? -1 : 1;
		Vector3 pos = new Vector3(x, y, goal.position.z);
		goal.localPosition = pos;
	}
}
