using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class InternalAcademy : MonoBehaviour {

	[Header("Academy Settings")]
	public int maxSteps;
	public int frameSkip;
	public int summaryFrequency;
	public int saveFrequency;

	[HideInInspector]
	public float goalStartDistance = 10f;
	[HideInInspector]
	public float goalRepositionFrequency = 0;
	[HideInInspector]
	public int difficultyLevel = 1;

	[HideInInspector]
	public bool running;

	public CartPoleAgent cpa;
	public Button toggleBtn;
	public List<Sprite> icons = new List<Sprite>();

	private List<InternalBrain> brains;
	private int stepCount;
	private int frameCount;

	public void Initialise()
	{
		brains = transform.GetComponentsInChildren<InternalBrain>().ToList();
		
		foreach (InternalBrain brain in brains)
		{
			brain.Initialise();
		}
		frameSkip = frameSkip == 0 ? 4 : frameSkip;
		running = true;
	}

	/// <summary>
	/// Calls step on all brains.
	/// </summary>
	public void Step()
	{
		if (stepCount < maxSteps || maxSteps == 0)
		{
			foreach (InternalBrain brain in brains)
			{
				if (!brain.training)
				{
					brain.Step();
					stepCount++;
				}
			}
		}
		else
		{
			running = false;
		}
	}

	/// <summary>
	/// Calls reset on all brains.
	/// </summary>
	public void Reset()
	{
		stepCount = 0;
		frameCount = 0;
		foreach (InternalBrain brain in brains)
		{
			brain.Reset();
		}
	}

	public void AddBrain(InternalBrain brain)
	{
		brains.Add(brain);
	}

	void Start()
	{
		Initialise();
	}

	void FixedUpdate()
	{
		if (running)
		{
			if (frameCount % frameSkip == 0)
			{
				Step();
			}
			frameCount++;
		}
	}

	public void Stop()
	{
		running = false;
		stepCount = 0;
		frameCount = 0;
	}

	public void LoadBrain()
	{
		Stop();
		brains[0].LoadBrain();
		running = true;
	}

	public void CreateNewBrain()
	{
		Stop();
		brains[0].CreateBrain();
		running = true;
	}

	public void ToggleControl()
	{
		cpa.manualControl = !cpa.manualControl;

		if (cpa.manualControl)
		{
			toggleBtn.GetComponent<Image>().sprite = icons[1];
		}
		else
		{
			toggleBtn.GetComponent<Image>().sprite = icons[0];
		}
	}
}
