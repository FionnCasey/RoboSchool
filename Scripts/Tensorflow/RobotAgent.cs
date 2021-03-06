﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAgent : Agent {

	[Header("Robot Settings")]

	[Tooltip("Force multiplier when appling torque.")]
	public float strength;
	[Tooltip("Number of legs that will touch the ground.")]
	public int numLegs;
	[Tooltip("All limbs and their movement information.")]
	public Limb[] limbs;

	public int agentNum;
	public MainGui gui;

	[HideInInspector]
	public bool isMonitorAgent;
	[HideInInspector]
	public bool[] legsTouching;
	[HideInInspector]
	public bool fell;

	private RobotAcademy academy;
	
	// Ensures a minimum amount of time has passed before moving goal position.
	private int repositionThreshold;
	private const int minStepThreshold = 500;

	private float previousDistance;
	private Vector3 previousVelocity;
	private Vector3 localVelocity;

	private Transform goal;
	private Transform body;
	private Rigidbody bodyRb;

	// Store initial limb positions for agent reset.
	private Dictionary<GameObject, Vector3> positions;
	private Dictionary<GameObject, Quaternion> rotations;

	/// <summary>
	/// Store references for all limbs and relevant GameObjects.
	/// </summary>
	public override void InitializeAgent()
	{
		academy = GameObject.FindObjectOfType<RobotAcademy>();
		goal = transform.Find("Goal");
		body = transform.Find("Body");
		bodyRb = body.gameObject.GetComponent<Rigidbody>();
		legsTouching = new bool[numLegs];

		positions = new Dictionary<GameObject, Vector3>();
		rotations = new Dictionary<GameObject, Quaternion>();

		// Store starting position and rotation for all limbs.
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

	/// <summary>
	/// Collect state information:
	/// Rotation, velocity, angular velocity, distance to goal.
	/// State size: num limbs * 13 + num legs + 9.
	/// </summary>
	/// <returns>Current state information as a list of floats</returns>
	public override List<float> CollectState()
	{
		RandomGoalPosition();
		//Debug.Log("collecting state");
		List<float> state = new List<float>();
		
		// Body rotation.
		state.Add(body.transform.rotation.eulerAngles.x / 180f - 1f);
        state.Add(body.transform.rotation.eulerAngles.y / 180f - 1f);
        state.Add(body.transform.rotation.eulerAngles.z / 180f - 1f);

		// World velocity.
        state.Add(bodyRb.velocity.x);
        state.Add(bodyRb.velocity.y);
        state.Add(bodyRb.velocity.z);

		// Local velocity.
		localVelocity = body.InverseTransformDirection(bodyRb.velocity);
		state.Add(localVelocity.x);
		state.Add(localVelocity.y);
		state.Add(localVelocity.z);

		// Relative goal position.
		state.Add(goal.position.x - body.position.x);
		state.Add(goal.position.y - body.position.y);
		state.Add(goal.position.z - body.position.z);

		// Position, rotation, velocity and angular velocity of each limb.
		for (int i = 0; i < limbs.Length; i++)
		{
			Rigidbody rb = limbs[i].limbTranform.gameObject.GetComponent<Rigidbody>();
			state.Add(limbs[i].limbTranform.localPosition.x);
            state.Add(limbs[i].limbTranform.localPosition.y);
            state.Add(limbs[i].limbTranform.localPosition.z);
            state.Add(limbs[i].limbTranform.localRotation.x);
            state.Add(limbs[i].limbTranform.localRotation.y);
            state.Add(limbs[i].limbTranform.localRotation.z);
            state.Add(limbs[i].limbTranform.localRotation.w);
            state.Add(rb.velocity.x);
            state.Add(rb.velocity.y);
            state.Add(rb.velocity.z);
            state.Add(rb.angularVelocity.x);
            state.Add(rb.angularVelocity.y);
            state.Add(rb.angularVelocity.z);
		}
		// 1 for each leg touching the ground, 0 otherwise.
		for (int i = 0; i < legsTouching.Length; i++)
		{
			state.Add(legsTouching[i] ? 1.0f : 0.0f);
		}
		// Check state magnitudes.
		// if (isMonitorAgent) RobotMonitor.DebugState(state);

        return state;
	}

	/// <summary>
	/// Take actions and receive reward.
	/// </summary>
	/// <param name="act">Array of actions represented as floats.</param>
	public override void AgentStep(float[] act)
	{
		if (act.Length == 0) Debug.Log("no actions");
		// Clamp action values between -1 and 1.
		for (int i = 0; i < act.Length; i++)
        {
            act[i] = Mathf.Clamp(act[i], -1f, 1f);
        }

		// Apply torque based on limb.
		int actionIndex = 0;
		float torquePenalty = 0;
		for (int i = 0; i < limbs.Length; i++)
		{
			Rigidbody rb = limbs[i].limbTranform.gameObject.GetComponent<Rigidbody>();
			float torqueX = 0;
			float torqueY = 0;
			float torqueZ = 0;

			if (limbs[i].rotateX)
			{
				torqueX = strength * act[actionIndex];
				torquePenalty += act[actionIndex] * act[actionIndex];
				actionIndex++;
			}
			if (limbs[i].rotateY)
			{
				torqueY = strength * act[actionIndex];
				torquePenalty += act[actionIndex] * act[actionIndex];
				actionIndex++;
			}
			if (limbs[i].rotateZ)
			{
				torqueZ = strength * act[actionIndex];
				torquePenalty += act[actionIndex] * act[actionIndex];
				actionIndex++;
			}
			rb.AddTorque(torqueX, torqueY, torqueZ);
		}

		// Update distance to goal.
		float distance = Vector3.Distance(body.position, goal.position);
		float deltaDistance = (previousDistance - distance) / Time.fixedDeltaTime;
		localVelocity = body.InverseTransformDirection(bodyRb.velocity);

		if (distance < 1f)
		{
			Debug.Log("Done");
			reward = 1f;
			done = true;
		}
		previousDistance = distance;
		/*
		Reward:
			+ 1.00 * change in distance since last step.
			+ 0.10 * local z velocity.
			- 0.01 * absolute y velocity.
			- 0.01 * sum of squared action values.
		 */
		if (!done)
		{
			reward += (0
				+ (1.00f * deltaDistance)
				+ (0.10f * localVelocity.z)
				- (0.01f * Mathf.Abs(bodyRb.velocity.y))
				- (0.01f * torquePenalty)
			);
			repositionThreshold++;
		}
		// If the agent fell set reward -1 and done.
		if (fell)
		{
			reward = -1f;
			done = true;
			fell = false;
		}

		// Display debug info.
		if (isMonitorAgent)
		{
			RobotMonitor.Log("Distance", deltaDistance);
			RobotMonitor.Log("Velocity Z", 0.1f * localVelocity.z);
			RobotMonitor.Log("Velocity Y", -0.01f * Mathf.Abs(bodyRb.velocity.y));
			RobotMonitor.Log("Torque", -0.01f * torquePenalty);
			RobotMonitor.Log("Reward", reward);
			RobotMonitor.AddReward(reward);
		}
		if (gui != null)
			gui.UpdateDisplay(agentNum, reward, distance, stepCounter);
	}

	/// <summary>
	/// Resets agent position.
	/// Called on done if reset on done is true.
	/// </summary>
	public override void AgentReset()
	{
		previousDistance = academy.goalStartDistance;
		repositionThreshold = 0;

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
		
		SetGoalPosition();
	}

	/// <summary>
	///	Repositions the goal in a random location.
	/// Angle deviation from agent depends on academy difficulty level.
	/// </summary>
	private void SetGoalPosition()
	{
		Vector3 goalPosition = new Vector3();
		goalPosition.y = transform.position.y;

		float angle = academy.difficultyLevel == 1 ? Random.Range(-45, 45)
					: academy.difficultyLevel == 2 ? Random.Range(-135, 135)
					: academy.difficultyLevel == 3 ? Random.Range(-180, 180)
					: 0;

		angle *= Mathf.PI / 180f;
		goalPosition.x = Mathf.Sin(angle) * academy.goalStartDistance;
		goalPosition.z = Mathf.Cos(angle) * academy.goalStartDistance;
		goal.position = transform.position + goalPosition;
	}

	/// <summary>
	/// If the academy difficulty level is high enough, has a chance to move the goal mid training.
	/// </summary>
	private void RandomGoalPosition()
	{
		if (academy.difficultyLevel == 3 &&
			repositionThreshold >= minStepThreshold &&
			Random.value <= academy.goalRepositionFrequency)
			{
				SetGoalPosition();
				repositionThreshold = 0;
			}
	}
}