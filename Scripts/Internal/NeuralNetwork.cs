using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeuralNetwork {

	public int numInputs { get; private set; }
	public int numOutputs { get; private set; }
	public int numHiddenLayers { get; private set; }
	public int numNeuronsPerHidden { get; private set; }
	public List<Layer> layers { get; private set; }

	public List<double> layerSnapshot { get; private set; }

	private List<double> outTest;
	private List<double> out1;
	private List<double> out0;

	/// <summary>
	/// Creates a new neural network of the given shape.
	/// </summary>
	public NeuralNetwork(int numInputs, int numOutputs, int numHiddenLayers, int numNeuronsPerHidden)
	{
		this.numInputs = numInputs;
		this.numOutputs = numOutputs;
		this.numHiddenLayers = numHiddenLayers;
		this.numNeuronsPerHidden = numNeuronsPerHidden;
		layers = new List<Layer>();
		layerSnapshot = new List<double>();
		out0 = new List<double>();
		out1 = new List<double>();
		outTest = new List<double>();
		// Add hidden layer connected to input layer.
		layers.Add(new Layer(numNeuronsPerHidden, numInputs));

		// Add hidden layers connected to previous hidden layers.
		for (int i = 0; i < numHiddenLayers; i++)
		{
			layers.Add(new Layer(numNeuronsPerHidden, numNeuronsPerHidden));
		}

		// Add output layer.
		layers.Add(new Layer(numOutputs, numNeuronsPerHidden));

	}

	/// <summary>
	/// Feed state through the network to decide actions.
	/// </summary>
	public List<double> DecideActions(List<double> inputValues, bool train = false)
	{
		// Copy input values.
		List<double> input = new List<double>(inputValues);

		layerSnapshot.Clear();
		out0.Clear();
		out1.Clear();

		List<double> output = new List<double>();
		int currentInput = 0;

		// Exit if input sizes don't match.
		if (inputValues.Count != numInputs)
		{
			Debug.Log(string.Format(
				"ERROR: Number of inputs must match input size. \nExpected: {0} - Recieved: {1}",
				numInputs, inputValues.Count
			));
			return output;
		}

		// Loop through hidden layers.
		for (int i = 0; i < numHiddenLayers; i++)
		{
			// If not the the first layer, get the output values from the previous layer then clear output.
			if (i > 0)
			{
				input = new List<double>(output);
			}
			output.Clear();

			// Loop through neurons in each layer.
			for (int j = 0; j < layers[i].numNeurons; j++)
			{
				// Reset neuron and input values.
				double value = 0;
				layers[i].neurons[j].inputs.Clear();

				// Loop through neuron's inputs.
				for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
				{
					// Calculate weighted sum of all inputs to neuron.
					layers[i].neurons[j].inputs.Add(input[currentInput]);
					value += layers[i].neurons[j].weights[k] * input[currentInput];
					currentInput++;
				}

				// Perform hyperbolic tangent activation.
				layers[i].neurons[j].output = TanH(value);

				if (train)
				{
					if (i == 0) out0.Add(layers[i].neurons[j].output = TanH(value));
					else if (i == 1) out1.Add(layers[i].neurons[j].output = TanH(value));
					else outTest.Add(layers[i].neurons[j].output = TanH(value));
				}

				output.Add(layers[i].neurons[j].output);
				layerSnapshot.Add(layers[i].neurons[j].output);
				currentInput = 0;
			}
		}
		double outVal = output.Sum();
		output.Clear();
		output.Add(TanH(outVal));
		return output;
	}

	public List<string> BackPropagate(List<double> inputVals, List<double> outputs, double advantage, double alpha)
	{
		List<string> logs = new List<string>();
		if (outputs.Count != numOutputs)
		{
			Debug.Log(string.Format(
				"ERROR: Number of outputs must match output size. \nExpected: {0} - Recieved: {1}",
				numOutputs, outputs.Count
			));
			return logs;
		}

		DecideActions(inputVals);
		
		double error;
		// Loop back through layers.
		for(int i = numHiddenLayers; i >= 0; i--)
		{
			for(int j = 0; j < layers[i].numNeurons; j++)
			{
				// If last layer ger error.
				if(i == numHiddenLayers)
				{
					double scaleVal = .5 * (outputs[0] + 1);
					//Debug.Log("I: " + i.ToString() + " - J: " + j.ToString());
					error = -System.Math.Log(scaleVal) * advantage;
					//logs.Add("Adv: " + advantage.ToString("F3") + " | Log: " + System.Math.Log(outputs[0]));
					layers[i].neurons[j].errorGradient = outputs[0] * (1-outputs[0]) * error;
					//logs.Add("Err: " + error + " | Num: " + outputs[0] * (1-outputs[0]));
					//logs.Add("Out: " + outputs[0] + " | Grad: " + layers[i].neurons[j].errorGradient.ToString("F3"));
				}
				else
				{
					// Calculate error relative to activation
					layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1-layers[i].neurons[j].output);
					double errorGradSum = 0;
					// Sum error gradient 
					for(int p = 0; p < layers[i+1].numNeurons; p++)
					{
						errorGradSum += layers[i+1].neurons[p].errorGradient * layers[i+1].neurons[p].weights[j];
					}
					layers[i].neurons[j].errorGradient *= errorGradSum;
				}
				for(int k = 0; k < layers[i].neurons[j].numInputs; k++)
				{
					// Update weights in direction of grad with step size alpha
					if(i == numHiddenLayers)
					{
						double scaleVal = .5 * (outputs[0] + 1);
						error = -System.Math.Log(scaleVal) * advantage;
						if (k != 0) layers[i].neurons[j].weights[k] += alpha * (layers[i-1].neurons[k].output * error);
						double delta = alpha * layers[i-1].neurons[k].output * error;
						//logs.Add("Layer " + i + " delta: " + delta.ToString("F5"));
						// Debug. Remove..
					}
					else
					{
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
					}
					
					
				}
			}
		}
		return logs;
	}

	public List<string> HackPropagate(List<double> inputVals, List<double> outputs, double advantage, double alpha)
	{
		List<string> logs = new List<string>();
		if (outputs.Count != numOutputs)
		{
			Debug.Log(string.Format(
				"ERROR: Number of outputs must match output size. \nExpected: {0} - Recieved: {1}",
				numOutputs, outputs.Count
			));
			return logs;
		}
		DecideActions(inputVals, true);

		double scaleVal = .5 * (outputs[0] + 1);
		layers[layers.Count-1].neurons[0].errorGradient = -System.Math.Log(scaleVal) * advantage;
		logs.Add("Initial: " + layers[layers.Count-1].neurons[0].errorGradient.ToString("F4"));

		for (int i = 0; i < 4; i++)
		{
			double eg = out0[i]
				* (1 - out0[i])
				* layers[layers.Count-1].neurons[0].weights[i]
				* layers[layers.Count-1].neurons[0].errorGradient;

			layers[layers.Count-2].neurons[i].errorGradient = eg;

			//logs.Add("In to HL" + i + ": " + out0[i].ToString("F4"));
		}

		

		for (int i = layers.Count-1; i > 1; i--)
		{
			for (int j = 0; j < layers[i].neurons.Count; j++)
			{
				for (int k = 0; k < layers[i].neurons[j].weights.Count; k++)
				{
					double delta = layers[i].neurons[j].weights[k]
						+ (alpha + out0[k] * layers[i].neurons[j].errorGradient);
					
					layers[i].neurons[j].weights[k] += delta;

					logs.Add("Delta [" + i + ", " + j + ", " + k + "] = " + delta.ToString("F4"));
				}
			}
		}

		return logs;
	}

	/// <summary>
	/// Activation function.
	/// Maps range: 0, 1.
	/// </summary>
	private double LogSigmoid(double value)
	{
		double k = (double) System.Math.Exp(value);
    	return k / (1.0f + k);
	}

	/// <summary>
	/// Activation function.
	/// Maps range: -1, 1.
	/// </summary>
	private double TanH(double value)
	{
		double k = (double) System.Math.Exp(-2*value);
    	return 2 / (1.0f + k) - 1;
	}
}
