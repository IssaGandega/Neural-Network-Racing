using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


#region Structs
public struct NeuronDisplay
{
	public GameObject neuronObj;
	public RectTransform neuronTransform;
	public Image neuronImage;
	public TMP_Text neuronValue;

	public void Init(float xPos, float yPos)
	{
		neuronTransform = neuronObj.GetComponent<RectTransform>();
		neuronImage = neuronObj.GetComponent<Image>();
		neuronValue = neuronObj.GetComponentInChildren<TMP_Text>();

		neuronTransform.anchoredPosition = new Vector2(xPos, yPos);
	}

	public void Refresh(float value, Gradient colorGradient)
	{
		neuronImage.color = colorGradient.Evaluate((value + 1) * .5f);
		neuronValue.text = value.ToString("F2");
	}
}

public struct AxonDisplay
{
	public GameObject axonObj;
	public RectTransform axonTransform;
	public Image axonImage;

	public void Init(RectTransform start, RectTransform end, float thickness, float neuronDiameter)
	{
		axonTransform = axonObj.GetComponent<RectTransform>();
		axonImage = axonObj.GetComponent<Image>();

		axonTransform.anchoredPosition = start.anchoredPosition + (end.anchoredPosition - start.anchoredPosition) * .5f; // *.5 mieux que /2
		axonTransform.sizeDelta = new Vector2((end.anchoredPosition - start.anchoredPosition).magnitude - neuronDiameter, thickness);
		
		axonTransform.rotation = Quaternion.FromToRotation(axonTransform.right, (end.anchoredPosition - start.anchoredPosition).normalized);
		//axonTransform.rotation = Quaternion.FromToRotation(end.anchoredPosition, start.anchoredPosition);
		
		axonTransform.SetAsFirstSibling();
	}

	public void Refresh(float value, Gradient colorGradient)
	{
		axonImage.color = colorGradient.Evaluate((value + 1) * .5f);
	}
}

#endregion

public class NeuralNetworkViewer : MonoBehaviour
{
	[Header("Neurons Parameters")]
	[SerializeField] private float layerSpacing = 100;
	[SerializeField] private float neuronSpacing = 5;
	[SerializeField] private float neuronDiameter = 32;
	[SerializeField] private Gradient neuronGradient;
	
	[Space]
	[Header("Axons Parameters")]
	[SerializeField] private float axonThickness;
	[SerializeField] private Gradient axonGradient;
	
	[Space]
	[Header("Objects")]
	public Agent agent;

	[SerializeField] private GameObject neuronPrefab;
	[SerializeField] private GameObject axonPrefab;
	[SerializeField] private GameObject fitnessPrefab;
	[SerializeField] private Transform viewerGroup;

	private NeuronDisplay[][] neurons;
	private AxonDisplay[][][] axons;

	private bool isInitialized;

	private float neuronsHeight;
	private float padding;

	private TMP_Text fitnessDisplay;

	public static NeuralNetworkViewer instance;

	private int x;
	private int y;
	private int z;
	
	private void Awake()
	{
		if (agent == null)
		{
			instance = this;
		}
		else
		{
			Destroy(this);
		}
	}
	
	private void Update()
	{
		for (x = 0; x < neurons.Length; x++)
		{
			for (y = 0; y < neurons[x].Length; y++)
			{
				neurons[x][y].Refresh(agent.net.neurons[x][y], neuronGradient);
			}
		}

		fitnessDisplay.text = agent.fitness.ToString("F1");
	}

	void Init(Agent paramAgent)
	{
		agent = paramAgent;
		NeuralNetwork net = agent.net;
		
		int maxNeurons = 0;

		for (x = 0; x < net.layers.Length; x++)
		{
			if (net.layers[x] > maxNeurons)
			{
				maxNeurons = net.layers[x];
			}
		}

		#region NeuronDisplay

		neurons = new NeuronDisplay[net.layers.Length][];

		for (x = 0; x < net.layers.Length; x++)
		{
			if (net.layers[x] < maxNeurons)
			{
				padding = (maxNeurons - net.layers[x]) * .5f * (neuronsHeight + neuronSpacing);

				if (net.layers[x] % 2 != maxNeurons % 2)
				{
					padding += (neuronsHeight + neuronSpacing) * .5f;
				}
			}
			else
			{
				padding = 0;
			}
			
			neurons[x] = new NeuronDisplay[net.layers[x]];

			for (y = 0; y < net.layers[x]; y++)
			{
				neurons[x][y] = new NeuronDisplay
				{
					neuronObj = Instantiate(neuronPrefab, viewerGroup)
				};
				
				neurons[x][y].Init(x * layerSpacing, -padding - (neuronsHeight + neuronSpacing) * y);
			}
		}

			#endregion

		#region AxonDisplay

		axons = new AxonDisplay[net.layers.Length - 1][][];

		for (x = 0; x < net.layers.Length - 1; x++)
		{
			axons[x] = new AxonDisplay[net.layers[x+1]][];

			for (y = 0; y < net.layers[x+1]; y++)
			{
				axons[x][y] = new AxonDisplay[net.layers[x]];

				for (z = 0; z < net.layers[x]; z++)
				{
					axons[x][y][z] = new AxonDisplay();
					axons[x][y][z].axonObj = Instantiate(axonPrefab, viewerGroup);


					axons[x][y][z].Init(neurons[x][z].neuronTransform, neurons[x+1][y].neuronTransform, axonThickness, neuronDiameter);
				}
			}
		}

		#endregion
		
		#region FitnessDisplay

		GameObject fitness = Instantiate(fitnessPrefab, viewerGroup);
		Vector2 pos = new Vector2(net.layers.Length * layerSpacing, -(float)maxNeurons * .5f * (neuronsHeight+neuronSpacing));
		fitness.GetComponent<RectTransform>().anchoredPosition = pos;

		fitnessDisplay = fitness.GetComponent<TMP_Text>();

		#endregion
	}

	void RefreshAxons()
	{
		for (x = 0; x < axons.Length; x++)
		{
			for (y = 0; y < axons[x].Length; y++)
			{
				for (z = 0; z < axons[x][y].Length; z++)
				{
					axons[x][y][z].Refresh(agent.net.axons[x][y][z], axonGradient);
				}
			}
		}
	}

	public void RefreshAgent(Agent tempAgent)
	{
		agent = tempAgent;
		
		if (!isInitialized)
		{
			isInitialized = true;
			Init(agent);
		}
		
		RefreshAxons();
	} 
}
