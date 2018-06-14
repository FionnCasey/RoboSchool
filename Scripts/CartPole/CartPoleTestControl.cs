using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartPoleTestControl : MonoBehaviour {

	public float strength = 40f;

	private Rigidbody cartRb;
	private Dictionary<GameObject, Vector3> positions;
	private Dictionary<GameObject, Quaternion> rotations;
	
	void Start()
	{
		cartRb = GetComponent<Rigidbody>();
		positions = new Dictionary<GameObject, Vector3>();
		rotations = new Dictionary<GameObject, Quaternion>();

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
	}

	void FixedUpdate()
	{
		float f = Input.GetAxis("Horizontal");
		//cartRb.AddForce(Input.GetAxis("Horizontal") * strength, 0, 0);

		//transform.position = new Vector3(transform.position.x + f, transform.position.y, transform.position.z);

		cartRb.velocity = new Vector3(f * strength, 0, 0);

		if (Input.GetKeyDown(KeyCode.Space)) Reset();
	}

	void Reset()
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
	}
}
