using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;

public class InternalBrain : MonoBehaviour {

	public List<InternalAgent> agents = new List<InternalAgent>();

	public NetworkDisplay networkDisplay;

	private Dictionary<InternalAgent, bool> dones = new Dictionary<InternalAgent, bool>();
	private Dictionary<InternalAgent, List<double>> states = new Dictionary<InternalAgent, List<double>>();
	private Dictionary<InternalAgent, double[]> actions = new Dictionary<InternalAgent, double[]>();

	// Experience buffer for gradient descent.
	private List<Experience> experienceBuffer;
	private double meanReward;
	private double cumulativeReward;
	private int currentStep;
	private int fails = 0;
	private int batchNum;

	public bool training = false;

	private List<Advantage> advantages;

	[Header("Save and Load")]

	[Tooltip("Text asset representation of current brain. For saving and loading")]
	public TextAsset brainGraph;

	public bool enableTraining;

	public TextMeshProUGUI _fails;
	public TextMeshProUGUI _meanR;
	public TextMeshProUGUI _steps;

	// Stores current brain.
	private NeuralNetwork activeBrain;
	public CartBrain brainSO;
	//private NeuralNetwork criticBrain;

	[Header("Network Structure")]

	[Tooltip("Number of neurons in input layer.")]
	public int stateSize;

	[Tooltip("Number of neurons in output layer.")]
	public int actionSize;

	[Tooltip("Number of hidden layers.")]
	public int numHiddenLayers = 2;

	[Tooltip("Number of neuron in each hidden layer.")]
	public int numNeuronsInHiddenLayers = 256;

	[Header("Hyperparameters")]

	/*
	How many experiences are used in each gradient descent update.
	Shouuld always be a fraction of the buffer size.
	 */
	[Tooltip("Number of experiences per gradient descent update.")]
	public int batchSize = 2048;

	/*
	Amount of memories to store before performing gradient descent.
	Should always be a multiple of batch size.
	 */
	[Tooltip("How large the experience buffer should be before gradient descent.")]
	public int bufferSize = 16384;

	/*
	Time-horizon should be large enough to allow the agent to recieve some meaningful reward,
	but not long enough to add too much variance.
	If the environment has high-variance in the expected rewards,
	using a smaller time-horizon will allow for a more consistent learning
	 */
	 [Tooltip("How many steps to collect per agent before adding to experience buffer.")]
	 public int timeHorizon = 1024;

	/*
	The number of times the memories are run through for gradient descent.
	Decreasing ensures more stable updates, at the cost of slower learning.
	Reccomended range: 3 - 10
	 */
	[Tooltip("Number of passes for each gradient descent update.")]
	 public int numEpochs = 4;

	/*
	Multiplier for controlling the strength each gradient descent update.
	Recommended range: 1e-5 - 1e-3
	 */
	[Tooltip("Model learning rate.")]
	public double alpha = 1e-3;

	/*
	Controls rate at which actions become less random.
	Used for discrete control brains.
	 */
	[Tooltip("Entropy decay rate.")]
	public double beta = 1e-2;

	/*
	Used for clipping error values in surrogate loss function.
	 */
	 [Tooltip("The ratio of old and new policy values.")]
	public double epsilon = 0.2;

	/*
	This determines how far ahead agents should be predicting rewards.
	For environments where agents need to think thousands of steps ahead, a higher value is used.
	Recommended range: 0.9 - 0.999
	 */
	[Tooltip("Reward discount factor.")]
	public double gamma = 0.99;

	/*
	When using the Generalized Advantage Estimate, the lambda parameter will control the trade-off between bias and variance.
	The GAE formulation allows for an interpolation between pure Temporal Difference learning and pure Monte-Carlo sampling.
	Recommended range: 0.95 - 0.99
	 */
	[Tooltip("Trade-off between bias and variance.")]
	public double lambda = 0.95;

	void Start()
	{
		//CreateBrain();
		LoadBrain();
	}

	public void CreateBrain()
	{
		if (ValidateNetworkStructure())
		{
			activeBrain = new NeuralNetwork(stateSize, actionSize, numHiddenLayers, numNeuronsInHiddenLayers);
			enableTraining = true;
			if (networkDisplay) {
				networkDisplay.network = activeBrain;
			}
			Initialise();
		}
	}

	public void LoadBrain()
	{
		if (brainSO == null)
		{
			Debug.Log("ERROR: No brain asset to load from.");
		}
		else
		{
			enableTraining = false;
			activeBrain = new NeuralNetwork(4, 1, 1, 4);

			TestLoadWeights();

			if (networkDisplay) {
				networkDisplay.network = activeBrain;
			}
			Initialise();
		}
	}

	public void SaveBrain()
	{

	}

	private void UpdateDisplay()
	{
		if (currentStep != 0)
		{
			meanReward = cumulativeReward / currentStep;

			_meanR.text = "Mean Reward: " + meanReward.ToString("F3");
			_steps.text = "Steps: " + currentStep.ToString();
			_fails.text = "Fails: " + fails;
		}
	}

	private bool ValidateNetworkStructure()
	{
		if (stateSize < 1 || actionSize < 1 || numHiddenLayers < 1)
		{
			Debug.Log("ERROR: Action size, state size and number of hidden layers must be at least 1.");
			return false;
		}
		if (numNeuronsInHiddenLayers < 2)
		{
			Debug.Log("ERROR: Number of neurons in hidden layers must be at least 2.");
			return false;
		}
		return true;
	}

	public void RegisterAgent(InternalAgent agent)
	{
		this.agents.Add(agent);
	}

	public void Initialise()
	{
		experienceBuffer = new List<Experience>();
		advantages = new List<Advantage>();
		currentStep = 0;
		cumulativeReward = 0;
		batchNum = 0;
		InitialiseAgents();
		Reset();
	}

	/// <summary>
	/// Collects states, memories and decides actions for all agents.
	/// </summary>
	public void Step()
	{
		CollectStates();
		DecideActions();
		AgentStep();
		CollectDones();
		CollectExperiences();
		CollectRewards();
		OnAgentDone();
		currentStep++;
		UpdateDisplay();	
	}

	public void Reset()
	{
		foreach (InternalAgent agent in agents)
		{
			agent.Reset();
		}
	}

	private void InitialiseAgents()
	{
		foreach (InternalAgent agent in agents)
		{
			agent.Initialise();
		}
	}

	private void CollectStates()
	{
		foreach (InternalAgent agent in agents)
		{
			if (!states.ContainsKey(agent))
			{
				states.Add(agent, agent.CollectState());
			}
			else
			{
				states[agent] = agent.CollectState();
			}
			if (networkDisplay) {
				networkDisplay.UpdateInput(agent.CollectState());
			}
		}
	}

	/// <summary>
	/// Collects memories from each agent to add to the experience buffer.
	/// If experience buffer is large enough, start gradient descent update.
	/// </summary>
	private void CollectExperiences()
	{
		if (currentStep != 0) {
			foreach (InternalAgent agent in agents)
			{
				experienceBuffer.Add(agent.CollectExperiences());
			}
			if (experienceBuffer.Count >= bufferSize && !training && enableTraining)
			{
				Train();
			}
		}
	}

	/// <summary>
	/// Feeds state through neural network to determine actions then calls step for each agent.
	/// </summary>
	private void DecideActions()
	{
		foreach (KeyValuePair<InternalAgent, List<double>> state in states)
		{
			double[] action = activeBrain.DecideActions(state.Value).ToArray();

			if (!actions.ContainsKey(state.Key))
			{
				actions.Add(state.Key, action);
			}
			else
			{
				actions[state.Key] = action;
			}
		}
	}

	private void AgentStep()
	{
		foreach (KeyValuePair<InternalAgent, double[]> action in actions)
		{
			action.Key.Step(action.Value);
			if (networkDisplay) {
				networkDisplay.UpdateOutput(action.Value.ToList());
			}
		}
		
	}

	/// <summary>
	/// Collects the done state for each agent.
	/// </summary>
	private void CollectDones()
	{
		foreach (InternalAgent agent in agents)
		{
			if (!dones.ContainsKey(agent))
			{
				dones.Add(agent, agent.done);
			}
			else
			{
				dones[agent] = agent.done;
			}
			if (agent.done) fails++;
		}
	}

	private void CollectRewards()
	{
		foreach (InternalAgent agent in agents)
		{
			cumulativeReward += agent.reward;
			agent.reward = 0;
		}
	}

	private void TestLoadWeights()
	{
		activeBrain.layers[0].neurons[0].weights[0] = brainSO.a1;
		activeBrain.layers[0].neurons[0].weights[1] = brainSO.a2;
		activeBrain.layers[0].neurons[0].weights[2] = brainSO.a3;
		activeBrain.layers[0].neurons[0].weights[3] = brainSO.a4;

		activeBrain.layers[0].neurons[1].weights[0] = brainSO.b1;
		activeBrain.layers[0].neurons[1].weights[1] = brainSO.b2;
		activeBrain.layers[0].neurons[1].weights[2] = brainSO.b3;
		activeBrain.layers[0].neurons[1].weights[3] = brainSO.b4;

		activeBrain.layers[0].neurons[2].weights[0] = brainSO.c1;
		activeBrain.layers[0].neurons[2].weights[1] = brainSO.c2;
		activeBrain.layers[0].neurons[2].weights[2] = brainSO.c3;
		activeBrain.layers[0].neurons[2].weights[3] = brainSO.c4;

		activeBrain.layers[0].neurons[3].weights[0] = brainSO.d1;
		activeBrain.layers[0].neurons[3].weights[1] = brainSO.d2;
		activeBrain.layers[0].neurons[3].weights[2] = brainSO.d3;
		activeBrain.layers[0].neurons[3].weights[3] = brainSO.d4;

		activeBrain.layers[activeBrain.layers.Count-1].neurons[0].weights[0] = brainSO.o1;
		activeBrain.layers[activeBrain.layers.Count-1].neurons[0].weights[1] = brainSO.o2;
		activeBrain.layers[activeBrain.layers.Count-1].neurons[0].weights[2] = brainSO.o3;
		activeBrain.layers[activeBrain.layers.Count-1].neurons[0].weights[3] = brainSO.o4;
	}

	/// <summary>
	/// Calls on done for each agent.
	/// <summary>
	private void OnAgentDone()
	{
		foreach (KeyValuePair<InternalAgent, bool> done in dones)
		{
			if (done.Value)
			{
				done.Key.OnDone();
			}
		}
	}

	/// <summary>
	/// Sets all agents to done.
	/// </summary>
	private void SetAllDone()
	{
		foreach (InternalAgent agent in agents)
		{
			agent.done = true;
		}
	}

	/// <summary>
	/// Uses the GAE algorithm.
	/// Lambda coefficient used to interpolate between pure temporal difference and monte carlo sampling.
	/// <summary>
	// public void CalculateGAE()
	// {
	// 	List<double> returns = new List<double>();
	// 	List<double> tdErrors = new List<double>();

	// 	double advantageEstimate = 0;

	// 	foreach (List<Experience> exp in experienceBuffer)
	// 	{	
	// 		int n = 0;
	// 		for (int i = 0; i < exp.Count - 1; i++)
	// 		{
	// 			double discountedR = exp[i].reward;
	// 			double pow = 1;
	// 			for (int j = i + 1; j < exp.Count - 1; j++)
	// 			{
	// 				discountedR += exp[j].reward;
	// 				pow++;
	// 			}
	// 			returns.Add(discountedR);
	// 			tdErrors.Add(exp[i].reward + (gamma * returns[i + 1]) - returns[i]);
	// 		}
	// 		advantageEstimate += tdErrors[n] + Math.Pow(gamma * lambda, n + 1);
	// 		advantages.Add(new Advantage(exp, advantageEstimate));
	// 	}
	// }

	public Advantage CalculateReturns()
	{
		List<Experience> exp = new List<Experience>();
		double returns = 0;

		for (int i = 0; i < experienceBuffer.Count; i++)
		{
			if (i == 0){
				returns += experienceBuffer[i].reward;
			}
			else {
				returns += Math.Pow(gamma, i) * experienceBuffer[i].reward;
			}
			exp.Add(experienceBuffer[i]);
		}
		//Debug.Log("Adv: " + returns.ToString("F3"));
		return new Advantage(exp, returns);
	}

	private void Train()
	{
		training = true;
		//Debug.Log("Training");
		Advantage batch = CalculateReturns();

		//Debug.Log("Batch Advantage: " + batch.advantage.ToString("F3"));

		List<string> logs = new List<string>();

		for (int i = 0; i < numEpochs; i++)
		{	
			foreach (Experience e in batch.experiences)
			{
				logs = activeBrain.BackPropagate(e.input, e.action.ToList(), batch.advantage, alpha);
			}
		}

		Debug.Log("Training Complete");
		
		logs.ForEach(log => Debug.Log(log));

		training = false;
		experienceBuffer.Clear();
	}
}

public struct Advantage {
	public List<Experience> experiences;
	public double advantage;

	public Advantage(List<Experience> experiences, double advantage)
	{
		this.experiences = experiences;
		this.advantage = advantage;
	}
}
