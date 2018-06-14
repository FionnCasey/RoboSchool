using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CartPoleGui : MonoBehaviour {

	public TextMeshProUGUI angleLabel;
	public Color[] labelColours = new Color[3];

	private HingeJoint hinge;

	void Start()
	{
		hinge = transform.GetComponentInChildren<HingeJoint>();
	}

	void Update()
	{
		angleLabel.transform.position = Camera.main.WorldToScreenPoint(new Vector2(
			transform.position.x,
			transform.position.y + 2f
		));
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		Color labelColour;

		if (hinge.angle >= 45 || hinge.angle <= -45) {
			labelColour = labelColours[2];
		}
		else if (hinge.angle >= 15 || hinge.angle <= -15) {
			labelColour = labelColours[1];
		}
		else {
			labelColour = labelColours[0];
		}

		angleLabel.color = labelColour;
		angleLabel.text = hinge.angle.ToString("F2");
	}

	public void QuitToMenu()
	{
		SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
	}
}
