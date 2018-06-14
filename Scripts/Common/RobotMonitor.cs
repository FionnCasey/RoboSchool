using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotMonitor : MonoBehaviour {

	private static Dictionary<string, float> logs;
	private static List<int> stateIndices;
	private static Text display;
	private static float minReward = 0;
	private static float maxReward = 0;

	public static void Log(string label, float value)
	{
		if (logs == null)
		{
			logs = new Dictionary<string, float>();
		}
		if (display == null)
		{
			display = GameObject.Find("Debug Log").GetComponent<Text>();
		}
		if (!logs.ContainsKey(label))
		{
			logs.Add(label, value);
		}
		else
		{
			logs[label] = value;
		}
		display.text = "";
		foreach (KeyValuePair<string, float> log in logs)
		{
			display.text += string.Format("{0}:  {1}\n", log.Key, log.Value);
		}
		display.text += string.Format("Min reward:  {0}\nMax reward:  {1}", minReward, maxReward);
	}

	public static void AddReward(float reward)
	{
		if (reward < minReward)
		{
			minReward = reward;
		}
		else if (reward > maxReward)
		{
			maxReward = reward;
		}
	}

	public static void DebugState(List<float> state)
	{
		if (stateIndices == null)
		{
			stateIndices = new List<int>();
		}
		for (int i = 0; i < state.Count; i++)
		{
			if ((state[i] > 1f || state[i] < -1f) && !stateIndices.Contains(i))
			{
				Debug.Log(string.Format("State index [{0}] exceeds magnitude limit.\nValue:  {1}", i, state[i]));
				stateIndices.Add(i);
			}
		}
	}
}
