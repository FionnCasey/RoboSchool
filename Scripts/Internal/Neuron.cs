using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron {

	public int numInputs { get; private set; }
	public double output { get; set; }
	public double errorGradient { get; set; }
	public List<double> weights { get; set; }
	public List<double> inputs { get; set; }

	/// <summary>
	/// Creates a neuron with random initial weights.
	/// </summary>
	public Neuron(int numInputs)
	{
		this.numInputs = numInputs;
		weights = new List<double>();
		inputs = new List<double>();

		float initalMaxVal = 2.4f / numInputs;
		for(int i = 0; i < numInputs; i++)
		{
			if (i == 0) {
				weights.Add(UnityEngine.Random.Range(0, 1));
			}
			else {
				weights.Add(UnityEngine.Random.Range(-initalMaxVal, initalMaxVal));
			}
		}
	}

	/// <summary>
	/// Creates a neuron with loaded weights.
	/// </summary>
	public Neuron(int numInputs, List<double>weights)
	{
		this.numInputs = numInputs;
		this.weights = weights;
		this.errorGradient = 0;
		inputs = new List<double>();
	}
}
