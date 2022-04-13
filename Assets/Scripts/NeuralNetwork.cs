using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class NeuralNetwork
{
	public int[] layers = {8,8,8,2};
	public float[][] neurons;
	public float[][][] axons;

	public NeuralNetwork(){} //Permet de save

	private int x;
	private int y;
	private int z;
	private float sum;
	
	public NeuralNetwork(int[] _layers)
	{
		layers = new int[_layers.Length];
		
		for (x = 0; x < _layers.Length; x++)
		{
			layers[x] = _layers[x];
		}

		InitNeurons();
		InitAxons();
	}

	private void InitNeurons()
	{
		neurons = new float[layers.Length][];
		
		for (x = 0; x < layers.Length; x++)
		{
			neurons[x] = new float[layers[x]];
		}
	}
	
	private void InitAxons()
	{
		axons = new float[layers.Length-1][][];
		
		for (x = 0; x < layers.Length-1; x++)
		{
			axons[x] = new float[layers[x+1]][];
			
			for ( y = 0; y < axons[x].Length; y++)
			{
				axons[x][y] = new float[layers[x]];
				
				for (z = 0; z < axons[x][y].Length; z++)
				{
					axons[x][y][z] = Random.Range(-1f, 1f);
				}
			}
		}
	}

	public void CopyNet(NeuralNetwork netCopy)
	{
		for (x = 0; x < netCopy.axons.Length; x++)
		{
			for (y = 0; y < netCopy.axons[x].Length; y++)
			{
				for (z = 0; z < netCopy.axons[x][y].Length; z++)
				{
					axons[x][y][z] = netCopy.axons[x][y][z];
				}
			}
		}
	}

	public void FeedForward(float[] input)
	{
		neurons[0] = input;

		for (x = 1; x < layers.Length; x++)
		{
			for (y = 0; y < neurons[x].Length; y++)
			{
				sum = 0;
				
				for (z = 0; z < axons[x-1][y].Length; z++)
				{
					sum += neurons[x-1][z] * axons[x-1][y][z];
				}
				neurons[x][y] = (float) Math.Tanh(sum);
			}
		}
	}

	public void Mutate(float mutationChance)
	{
		sum = Random.Range(-1f, 1f);
		for (x = 0; x < axons.Length; x++)
		{
			for (y = 0; y < axons[x].Length; y++)
			{
				for (z = 0; z < axons[x][y].Length; z++)
				{
					sum = UnityEngine.Random.Range(0f, 100f);
					if (sum < 0.06f * mutationChance)
					{
						axons[x][y][z] = UnityEngine.Random.Range(-1f, 1f);
					}
					else if (sum < 0.07f * mutationChance)
					{
						axons[x][y][z] = -1f;
					}
					else if (sum < 0.5f * mutationChance)
					{
						axons[x][y][z] += 0.1f * UnityEngine.Random.Range(-1f,1f);
					}
					else if (sum < 0.75f * mutationChance)
					{
						axons[x][y][z] = UnityEngine.Random.Range(1f,2f);
					}
					else if (sum < 1f * mutationChance)
					{
						axons[x][y][z] *= UnityEngine.Random.Range(0f,1f);
					}
				}
			}
		}
	}
}
