using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InternalAgent : MonoBehaviour {

	[Tooltip("The brain controlling this agent.")]
	public InternalBrain brain;

	[Header("Training Configuration")]

	[Tooltip("Max number of steps before reseting. If 0, will only reset on done.")]
	public int maxSteps = 0;
	[Tooltip("Reset agent when done is set true.")]
	public bool resetOnDone;

	protected int stepCount;

	public int bufferSize { get { return experiences.Count; } }

	[HideInInspector]
	public bool done;

	// Stores previous memories for this agent.
	public List<Experience> experiences;

	// Reward is reset to 0 at the beginning of every step.
    public double reward;

	// Stores previous state for adding to experiences.
	protected List<double> savedState;

	public abstract void Initialise();
	public abstract List<double> CollectState();
	public abstract void Step(double[] actions);
	public abstract void Reset();

	public virtual Experience CollectExperiences()
	{
		return null;
	}

	public virtual void OnDone()
	{
		if (resetOnDone)
		{
			Reset();
		}
	}
}
