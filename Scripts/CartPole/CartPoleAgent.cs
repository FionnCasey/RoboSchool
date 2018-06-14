using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartPoleAgent : InternalAgent {

	public float strength = 40f;
	public float angleRange = 30f;
	public HingeJoint hinge;
	public GameObject pole;
	public GameObject leftWall;
	public GameObject rightWall;

	public bool manualControl = false;
	public bool initialised = false;

	private float leftDist;
	private float rightDist;
	private float leftMaxDist;
	private float rightMaxDist;

	private Experience recentExp;

	private Rigidbody cartRb;
	private Dictionary<GameObject, Vector3> positions;
	private Dictionary<GameObject, Quaternion> rotations;

	public override void Initialise()
	{
		if (!initialised)
		{
			cartRb = GetComponent<Rigidbody>();
			positions = new Dictionary<GameObject, Vector3>();
			rotations = new Dictionary<GameObject, Quaternion>();
			experiences = new List<Experience>();

			leftMaxDist = Vector3.Distance(transform.position, leftWall.transform.position);
			rightMaxDist = Vector3.Distance(transform.position, rightWall.transform.position);

			Transform[] allChildren = GetComponentsInChildren<Transform>();
			foreach(Transform child in allChildren)
			{
				// Ignore children without rigidbodies.
				if (child.gameObject.GetComponent<Rigidbody>() == null)
				{
					continue;
				}
				positions[child.gameObject] = child.position;
				rotations[child.gameObject] = child.rotation;
			}
			initialised = true;
		}
	}

	public override List<double> CollectState()
	{
		List<double> state = new List<double>();

		leftDist = Vector3.Distance(transform.position, leftWall.transform.position) * .5f;
		rightDist = Vector3.Distance(transform.position, rightWall.transform.position) * .5f;

		state.Add(hinge.angle / 90);
		state.Add(cartRb.velocity.x / 10f);
		state.Add(leftDist / leftMaxDist);
		state.Add(rightDist / rightMaxDist);

		savedState = state;
		return state;
	}

	public override void Step(double[] actions)
	{
		float f = Mathf.Clamp((float)actions[0], -1f, 1f);
		
		if (!manualControl) cartRb.velocity = new Vector3(f * strength, 0, 0);

		//transform.position = new Vector3(transform.position.x + f, transform.position.y, transform.position.z);
		
		reward = 0.1f;
		//Debug.Log(reward);

		if (hinge.angle > 85 || hinge.angle < -85) {
			reward = -1;
			done = true;
		}
		recentExp = new Experience(savedState, actions, reward);
	}

	public override Experience CollectExperiences()
	{
		return recentExp;
	}

	public override void Reset()
	{
		Transform[] allChildren = GetComponentsInChildren<Transform>();
		foreach(Transform child in allChildren)
		{
			// Ignore children without rigidbodies.
			if (child.gameObject.GetComponent<Rigidbody>() == null)
			{
				continue;
			}
			// Reset position and rotation.
			child.position = positions[child.gameObject];
            child.rotation = rotations[child.gameObject];

			// Set velocity and angular velocity to 0, 0, 0.
            child.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            child.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}

		cartRb.velocity = Vector3.zero;
		float a = Random.Range(-angleRange, angleRange);
		pole.transform.rotation = Quaternion.Euler(0, 0, a);
		done = false;
	}

	private void ManualControl()
	{
		if (manualControl)
		{
			float f = Input.GetAxis("Horizontal");
			cartRb.velocity = new Vector3(f * strength, 0, 0);
		}
	}

	void Update()
	{
		ManualControl();
	}
}
