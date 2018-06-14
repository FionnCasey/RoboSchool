using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience {

	public double[] action;
	public List<double> input;
	public double reward;

	public Experience(List<double> input, double[] action, double reward = 0)
	{
		this.input = input;
		this.action = action;
		this.reward = reward;
	}
}
