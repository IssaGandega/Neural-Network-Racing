using System;
using UnityEngine;

public class Agent : MonoBehaviour, IComparable<Agent>
{
	
	public NeuralNetwork net;
	[SerializeField] private CarController carController;
	[SerializeField] private Rigidbody rb;

	public float fitness;
	private float distanceTravelled;
	private float[] inputs;
	
	public Transform nextCheckpoint;
	private float totalCheckpointDist;
	private float nextCheckpointDist;
	
	[SerializeField] private float rayRange = 5;
	private RaycastHit hit;
	[SerializeField] private LayerMask layer;

	private Vector3 forward;
	private Vector3 position;
	private Vector3 right;

	public MeshRenderer meshRenderer;
	[SerializeField] private Material firstMat;
	[SerializeField] private  Material defaultMat;
	[SerializeField] private  Material mutantMat;

	public void ResetAgent()
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		inputs = new float[net.layers[0]];
		
		carController.Reset();

		fitness = 0;

		totalCheckpointDist = 0;

		nextCheckpoint = CheckpointManager.instance.firstCheckpoint;

		nextCheckpointDist = (nextCheckpoint.position - transform.position).magnitude;
	}

	private void FixedUpdate()
	{
		InputUpdate();
		OutputUpdate();
		FitnessUpdate();
	}

	private void InputUpdate()
	{
		forward = transform.forward;
		position = transform.position;
		right = transform.right;
		
		inputs[0] = RaySensor(position, forward, 4f);
		inputs[1] = RaySensor(position, -right, 1.5f);
		inputs[2] = RaySensor(position, right, 1.5f);
		inputs[3] = RaySensor(position, forward - right, 2f);
		inputs[4] = RaySensor(position, forward + right, 2f);

		inputs[5] = (float) Math.Tanh(rb.velocity.magnitude * 0.05f);
		inputs[6] = (float) Math.Tanh(rb.angularVelocity.y * 0.1f);

		inputs[7] = 1; //init the net
	}

	float RaySensor(Vector3 origin, Vector3 dir, float length)
	{
		if (Physics.Raycast(origin, dir,out hit, length*rayRange, layer))
		{
			Debug.DrawRay(origin, dir*hit.distance, Color.Lerp(Color.red, Color.green, 1 - hit.distance / (rayRange * length)));
			
			return 1 - hit.distance / (rayRange * length);
		}
		
		Debug.DrawRay(origin, dir*length*rayRange, Color.red);
		
		return 0;
	}
	
	private void OutputUpdate()
	{
		net.FeedForward(inputs);
		carController.horizontalInput = net.neurons[net.layers.Length-1][0];
		carController.verticalInput = net.neurons[net.layers.Length-1][1];
	}
	
	private void FitnessUpdate()
	{
		distanceTravelled = totalCheckpointDist +
		                    (nextCheckpointDist - (nextCheckpoint.position - transform.position).magnitude);

		if (fitness < distanceTravelled)
		{
			fitness = distanceTravelled;
		}

		if (transform.position.y < -10)
		{
			fitness = 0;
		}
	}

	public void CheckpointReached(Transform checkpoint)
	{
		totalCheckpointDist += nextCheckpointDist;
		nextCheckpoint = checkpoint;

		nextCheckpointDist = (nextCheckpoint.position - transform.position).magnitude;
	}

	public void SetFirstMaterial()
	{
		meshRenderer.material = firstMat;
	}
	
	public void SetDefaultMaterial()
	{
		meshRenderer.material = defaultMat;
	}
	
	public void SetMutantMaterial()
	{
		meshRenderer.material = mutantMat;
	}

	public int CompareTo(Agent other)
	{
		if (fitness < other.fitness)
		{
			return 1;
		}

		if (fitness > other.fitness)
		{
			return -1;
		}
		return 0;
	}
}
