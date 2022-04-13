using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float maxSteerAngle = 60;
    [SerializeField] private float motorForce = 1000;

    [SerializeField] private WheelCollider 
        wheelFrontLeftCollider,
        wheelFrontRightCollider,
        wheelBackLeftCollider,
        wheelBackRightCollider;
    
    [SerializeField] private Transform 
        wheelFrontLeftModel,
        wheelFrontRightModel,
        wheelBackLeftModel,
        wheelBackRightModel;

    public float horizontalInput;
    public float verticalInput;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    
    private Vector3 pos;
    private Quaternion quat;
    
    void Start()
    {
        rb.centerOfMass = centerOfMass.localPosition;
    }

    private void FixedUpdate()
    {
        Steer();
        Accelerate();
        UpdateWheelPoses();
    }

    private void Steer()
    {
        wheelFrontRightCollider.steerAngle = horizontalInput * maxSteerAngle;
        wheelFrontLeftCollider.steerAngle = horizontalInput * maxSteerAngle;
    }
    
    private void Accelerate()
    {
        wheelBackRightCollider.motorTorque = verticalInput * motorForce;
        wheelBackLeftCollider.motorTorque = verticalInput * motorForce;
    }
    
    private void UpdateWheelPoses()
    {
        UpdateWheelPose(wheelFrontLeftCollider, wheelFrontLeftModel);
        UpdateWheelPose(wheelBackLeftCollider, wheelBackLeftModel);
        UpdateWheelPose(wheelFrontRightCollider, wheelFrontRightModel);
        UpdateWheelPose(wheelBackRightCollider, wheelBackRightModel);
    }

    void UpdateWheelPose(WheelCollider col, Transform tr)
    {
        pos = tr.position;
        quat = tr.rotation;

        col.GetWorldPose(out pos, out quat);

        tr.position = pos;
        tr.rotation = quat;
    }

    public void Reset()
    {
        horizontalInput = 0;
        verticalInput = 0;
    }
}
