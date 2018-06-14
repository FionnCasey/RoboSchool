using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkDisplay : MonoBehaviour {

	public NeuronDisplay[] input;
	public NeuronDisplay[] hidden;
	public NeuronDisplay output;

	public NeuralNetwork network { get; set; }

	public void UpdateInput(List<double> input)
	{
		for (int i = 0; i < input.Count; i++)
		{
			this.input[i].Set(input[i]);
		}
	}

	private void UpdateHiddenLayer()
	{
		for (int i = 0; i < network.layerSnapshot.Count; i++)
		{
			hidden[i].Set(network.layerSnapshot[i]);
		}
	}

	public void UpdateOutput(List<double> output)
	{
		UpdateHiddenLayer();
		this.output.Set(output[0]);
	}
}