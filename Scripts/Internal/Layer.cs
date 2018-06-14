using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer {

	public int numNeurons { get; private set; }
	public List<Neuron> neurons { get; set; }

	public Layer(int numNeurons, int numInputsPerNeuron)
	{
		this.numNeurons = numNeurons;
		this.neurons = new List<Neuron>();
		for(int i = 0; i < numNeurons; i++)
		{
			neurons.Add(new Neuron(numInputsPerNeuron));
		}
	}
}
