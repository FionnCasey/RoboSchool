using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainGui : MonoBehaviour {

	private int currentAgent = 1;

	public TextMeshProUGUI _steps;
	public TextMeshProUGUI _reward;
	public TextMeshProUGUI _distance;

	public Camera cam1, cam2;
	public Button _b1, _b2;

	public void UpdateDisplay(int agent, float reward, float distance, int steps)
	{
		if (agent == currentAgent)
		{
			_reward.text = "Reward: " + reward.ToString("F3");
			_distance.text = "Distance: " + distance.ToString("F1") + "m";
			_steps.text = "Steps: " + steps;
		}
	}

	public void SwapCameras()
	{
		if (currentAgent == 1)
		{
			cam1.gameObject.SetActive(false);
			cam2.gameObject.SetActive(true);
			currentAgent = 2;
			_b1.interactable = true;
			_b2.interactable = false;
		}
		else
		{
			cam2.gameObject.SetActive(false);
			cam1.gameObject.SetActive(true);
			currentAgent = 1;
			_b2.interactable = true;
			_b1.interactable = false;
		}
	}

	public void QuitToMenu()
	{
		SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
	}
}
