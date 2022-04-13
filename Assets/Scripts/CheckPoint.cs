using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public Transform nextCheckpoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Agent>())
        {
            if (other.GetComponent<Agent>().nextCheckpoint == transform)
            {
                other.GetComponent<Agent>().CheckpointReached(nextCheckpoint);
            }
        }
    }
}
