using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAcademy : Academy {

    // Controls the angle deviation of the goal as well as wether to move the goal mid training.
    public int difficultyLevel;

    // Controls how often the goal will be moved mid training.
    [HideInInspector]
    public float goalRepositionFrequency;

    [Header("Training Environment Settings")]
    public float goalStartDistance = 10f;

    /// <summary>
    /// Initialises academy settings.
    /// Calls InitializeAgent() for each agent.
    /// </summary>
	public override void InitializeAcademy()
    {
        Debug.Log("initialise");
    }

    /// <summary>
    /// Called when academy reaches max steps.
    /// Calls AgentRest() for each agent.
    /// Used to update the training conditions as per the curriculum.
    /// </summary>
    public override void AcademyReset()
    {
        //difficultyLevel = (int)resetParameters["difficulty"];
        //goalRepositionFrequency = (float)resetParameters["reposition_freq"];
    }
}
