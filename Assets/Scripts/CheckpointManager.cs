using System;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
	public static CheckpointManager instance;
	public Transform firstCheckpoint;
	
	private void Awake()
	{
		instance = this;
		Init();
	}

	[ContextMenu("Init")]
	void Init()
	{
		firstCheckpoint = transform.GetChild(0);
		
		for (int i = 0; i < transform.childCount - 1; i++)
		{
			transform.GetChild(i).GetComponent<CheckPoint>().nextCheckpoint = transform.GetChild(i+1);
		}
		transform.GetChild(transform.childCount-1).GetComponent<CheckPoint>().nextCheckpoint = transform.GetChild(0);
	}
}
