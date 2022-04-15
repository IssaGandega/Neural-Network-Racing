using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{

    //CAR SETUP

      [Space(20)]
      //[Header("CAR SETUP")]
      [Space(10)]
      [Range(20, 190)]
      public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
      [Range(10, 120)]
      public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
      [Range(1, 10)]
      public int accelerationMultiplier = 2; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
      [Space(10)]
      [Range(10, 45)]
      public int maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
      [Range(0.1f, 1f)]
      public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
      [Space(10)]
      [Range(100, 600)]
      public int brakeForce = 350; // The strength of the wheel brakes.
      [Range(1, 10)]
      public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
      [Range(1, 10)]
      public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
      [Space(10)]
      public Vector3 bodyMassCenter;

      //[Header("WHEELS")]
      public GameObject frontLeftMesh;
      public WheelCollider frontLeftCollider;
      [Space(10)]
      public GameObject frontRightMesh;
      public WheelCollider frontRightCollider;
      [Space(10)]
      public GameObject rearLeftMesh;
      public WheelCollider rearLeftCollider;
      [Space(10)]
      public GameObject rearRightMesh;
      public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS

      [Space(20)]
      [Header("EFFECTS")]
      [Space(10)]
      //The following variable lets you to set up particle systems in your car
      public bool useEffects = false;

      // The following particle systems are used as tire smoke when the car drifts.
      public ParticleSystem RLWParticleSystem;
      public ParticleSystem RRWParticleSystem;

      [Space(10)]
      // The following trail renderers are used as tire skids when the car loses traction.
      public TrailRenderer RLWTireSkid;
      public TrailRenderer RRWTireSkid;
      
    //CONTROLS
    [Space(20)]
    [Header("CONTROLS")]

      //CAR DATA
      [HideInInspector]
      public float carSpeed; // Used to store the speed of the car.
      [HideInInspector]
      public bool isDrifting; // Used to know whether the car is drifting or not.
      [HideInInspector]
      public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.

      //PRIVATE VARIABLES
      Rigidbody carRigidbody; // Stores the car's rigidbody.
      float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
      float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
      float driftingAxis;
      float localVelocityZ;
      float localVelocityX;
      bool deceleratingCar;
      bool touchControlsSetup = false;

      WheelFrictionCurve FLwheelFriction;
      float FLWextremumSlip;
      WheelFrictionCurve FRwheelFriction;
      float FRWextremumSlip;
      WheelFrictionCurve RLwheelFriction;
      float RLWextremumSlip;
      WheelFrictionCurve RRwheelFriction;
      float RRWextremumSlip;
      
      public float horizontalInput;
      public float verticalInput;
      public float handBrakeInput;

    // Start is called before the first frame update
    void Start()
    {
      carRigidbody = gameObject.GetComponent<Rigidbody>();
      carRigidbody.centerOfMass = bodyMassCenter;
      
      //save the default friction values of the car wheels so we can set a drifting value later.
      (FLwheelFriction, FLWextremumSlip) = InitWheelFriction(FLwheelFriction, FLWextremumSlip, frontLeftCollider);
      (FRwheelFriction, FRWextremumSlip) = InitWheelFriction(FRwheelFriction, FRWextremumSlip, frontRightCollider);
      (RLwheelFriction, RLWextremumSlip) = InitWheelFriction(RLwheelFriction, RLWextremumSlip, rearLeftCollider);
      (RRwheelFriction, RRWextremumSlip) = InitWheelFriction(RRwheelFriction, RRWextremumSlip, rearRightCollider);
    }
    
    private (WheelFrictionCurve, float)  InitWheelFriction(WheelFrictionCurve wheelFriction, float wheelSlip, WheelCollider wheel)
    {
      wheelFriction = new WheelFrictionCurve ();
      wheelFriction.extremumSlip = wheel.sidewaysFriction.extremumSlip;
      wheelSlip = wheel.sidewaysFriction.extremumSlip;
      wheelFriction.extremumValue = wheel.sidewaysFriction.extremumValue;
      wheelFriction.asymptoteSlip = wheel.sidewaysFriction.asymptoteSlip;
      wheelFriction.asymptoteValue = wheel.sidewaysFriction.asymptoteValue;
      wheelFriction.stiffness = wheel.sidewaysFriction.stiffness;

      return (wheelFriction, wheelSlip);
    }

    void FixedUpdate()
    {
      carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
      // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
      localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
      // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
      localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;
      //PARTICLES
      if(!useEffects){
        if(RLWParticleSystem != null || RRWTireSkid != null){
          RLWParticleSystem.Stop();
          RRWParticleSystem.Stop();
          RLWTireSkid.emitting = false;
          RRWTireSkid.emitting = false;
        }
      }
      //CAR PHYSICS
      GoForward();
      Turn();
      if(handBrakeInput > 0.8f){ Handbrake(); }

    }
    
    //STEERING METHODS
    //The following method turns the front car wheels to the left. The speed of this movement will depend on the steeringSpeed variable.
    public void Turn(){
      steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
      steeringAxis = 1f * horizontalInput;
      var steeringAngle = steeringAxis * maxSteeringAngle;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //
    //ENGINE AND BRAKING METHODS
    //
    public void GoForward(){
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
        DriftCarPS();
      }else{
        isDrifting = false;
        DriftCarPS();
      }
      throttleAxis = throttleAxis + (Time.deltaTime * 3f);
      if(throttleAxis > 1f){
        throttleAxis = 1f;
      }
      if(localVelocityZ < -1f){
        Brakes();
      }else
      {
        ApplyPositionTorque();
      }
    }

    private void ApplyPositionTorque()
    {
      if(Mathf.RoundToInt(carSpeed) < maxSpeed){
        //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
        frontLeftCollider.brakeTorque = 0;
        frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        frontRightCollider.brakeTorque = 0;
        frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        rearLeftCollider.brakeTorque = 0;
        rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        rearRightCollider.brakeTorque = 0;
        rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
      }else {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
      }
    }

    public void Brakes(){
      frontLeftCollider.brakeTorque = brakeForce;
      frontRightCollider.brakeTorque = brakeForce;
      rearLeftCollider.brakeTorque = brakeForce;
      rearRightCollider.brakeTorque = brakeForce;
    }


    public void Handbrake(){
      CancelInvoke("RecoverTraction");
      driftingAxis = driftingAxis + (Time.deltaTime);
      float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

      if(secureStartingPoint < FLWextremumSlip){
        driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
      }
      if(driftingAxis > 1f){
        driftingAxis = 1f;
      }
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
      }else{
        isDrifting = false;
      }
      if (driftingAxis < 1f)
      {
        ApplyDrift();
      }
      isTractionLocked = true;
      DriftCarPS();
    }

    private void ApplyDrift()
    {
      FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
      frontLeftCollider.sidewaysFriction = FLwheelFriction;
      FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
      frontRightCollider.sidewaysFriction = FRwheelFriction;
      RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
      rearLeftCollider.sidewaysFriction = RLwheelFriction;
      RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
      rearRightCollider.sidewaysFriction = RRwheelFriction;
    }

    public void RecoverTraction(){
      isTractionLocked = false;
      driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
      if(driftingAxis < 0f){
        driftingAxis = 0f;
      }
      if(FLwheelFriction.extremumSlip > FLWextremumSlip){
        ApplyDrift();
        Invoke("RecoverTraction", Time.deltaTime);

      }else if (FLwheelFriction.extremumSlip < FLWextremumSlip){
        FLwheelFriction.extremumSlip = FLWextremumSlip;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;
        FRwheelFriction.extremumSlip = FRWextremumSlip;
        frontRightCollider.sidewaysFriction = FRwheelFriction;
        RLwheelFriction.extremumSlip = RLWextremumSlip;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;
        RRwheelFriction.extremumSlip = RRWextremumSlip;
        rearRightCollider.sidewaysFriction = RRwheelFriction;
        driftingAxis = 0f;
      }
    }

    public void Reset()
    {
      horizontalInput = 0;
      verticalInput = 0;
      RLWParticleSystem.Stop();
      RRWParticleSystem.Stop();
      RRWTireSkid.emitting = false;
      RLWTireSkid.emitting = false;
    }

    #region Optionnal

    public void ActivateWheels()
    {
      useEffects = true;
      frontLeftMesh.GetComponent<MeshRenderer>().enabled = true;
      frontRightMesh.GetComponent<MeshRenderer>().enabled = true;
      rearLeftMesh.GetComponent<MeshRenderer>().enabled = true;
      rearRightMesh.GetComponent<MeshRenderer>().enabled = true;
    }
    
    public void DesactivateWheels()
    {
      useEffects = false;
      frontLeftMesh.GetComponent<MeshRenderer>().enabled = false;
      frontRightMesh.GetComponent<MeshRenderer>().enabled = false;
      rearLeftMesh.GetComponent<MeshRenderer>().enabled = false;
      rearRightMesh.GetComponent<MeshRenderer>().enabled = false;
    }
    
    //DRIFT PARTICLES
    public void DriftCarPS(){

      if(useEffects){
        try{
          if(isDrifting){
            RLWParticleSystem.Play();
            RRWParticleSystem.Play();
          }else if(!isDrifting){
            RLWParticleSystem.Stop();
            RRWParticleSystem.Stop();
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }

        try{
          if((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f){
            RLWTireSkid.emitting = true;
            RRWTireSkid.emitting = true;
          }else {
            RLWTireSkid.emitting = false;
            RRWTireSkid.emitting = false;
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }
      }else if(!useEffects){
        if(RLWParticleSystem != null || RRWTireSkid != null){
          RLWParticleSystem.Stop();
          RRWParticleSystem.Stop();
          RLWTireSkid.emitting = false;
          RRWTireSkid.emitting = false;
        }
      }
    }
    
    //Animate Wheels
    void AnimateWheelMeshes(){
      try{
        Quaternion FLWRotation;
        Vector3 FLWPosition;
        frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
        frontLeftMesh.transform.position = FLWPosition;
        frontLeftMesh.transform.rotation = FLWRotation;

        Quaternion FRWRotation;
        Vector3 FRWPosition;
        frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
        frontRightMesh.transform.position = FRWPosition;
        frontRightMesh.transform.rotation = FRWRotation;

        Quaternion RLWRotation;
        Vector3 RLWPosition;
        rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
        rearLeftMesh.transform.position = RLWPosition;
        rearLeftMesh.transform.rotation = RLWRotation;

        Quaternion RRWRotation;
        Vector3 RRWPosition;
        rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
        rearRightMesh.transform.position = RRWPosition;
        rearRightMesh.transform.rotation = RRWRotation;
      }catch(Exception ex){
        Debug.LogWarning(ex);
      }
    }

    #endregion
    
    
}
