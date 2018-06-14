using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeuronDisplay : MonoBehaviour {

	private TextMeshProUGUI value;
	private Image fillPositive;
	private Image fillNegative;

	void Awake()
	{
		value = GetComponentInChildren<TextMeshProUGUI>();
		fillPositive = transform.Find("_fillPositive").GetComponent<Image>();
		fillNegative = transform.Find("_fillNegative").GetComponent<Image>();
	}

	public void Set(double val)
	{
		value.text = val.ToString("F1");
		float fill = Mathf.Abs((float)val) * .5f;

		if (val < 0) {
			fillPositive.gameObject.SetActive(false);
			fillNegative.gameObject.SetActive(true);
			fillNegative.fillAmount = fill;
		} else {
			fillNegative.gameObject.SetActive(false);
			fillPositive.gameObject.SetActive(true);
			fillPositive.fillAmount = fill;
		}
	}
}