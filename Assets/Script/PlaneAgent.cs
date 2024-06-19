using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlaneAgent : MonoBehaviour
{
    [Header("Plane Stats")]
    public float throttleIncrease = 0.1f;
    public float MaxThrust = 500f;
    public float responsiveness = 6f;
    [SerializeField] private float Maxheight;

    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    
    [Header("Lift Setting")]
    [SerializeField] private float C_L = 1f;
    [SerializeField] private float Airdensity = 1.225f;
    [SerializeField] private float wingArea = 12f;
    
    private float responseModifier {
        get
        {
            float new_responseiveness = responsiveness * (( MaxThrust / rb.velocity.magnitude) / 10);
            new_responseiveness = Mathf.Clamp(new_responseiveness, responsiveness, responsiveness + 12);
            Debug.Log(new_responseiveness);
            return(rb.mass / 10f) * new_responseiveness;
        }
    }
    

    Rigidbody rb;
    [Header("HUD")]

    [Header("ControlSurface")] 
    [SerializeField] private GameObject PitchSurface;
    [SerializeField] private GameObject YawSurface;
    [SerializeField] private GameObject LeftRolluface;
    [SerializeField] private GameObject RightRollSurface;



    private float PitchControl = 0;
    private float YawControl = 0;
    private float LeftRollControl = 0;
    private float RightRollControl = 0;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInput() {
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        PitchControl += pitch;
        PitchControl = Mathf.Clamp(PitchControl, -25, 25);
        YawControl += yaw;
        YawControl = Mathf.Clamp(YawControl, -30, 30);

        LeftRollControl += roll;
        RightRollControl += -roll;
        
        LeftRollControl = Mathf.Clamp(LeftRollControl, -20, 20);
        RightRollControl = Mathf.Clamp(RightRollControl, -20, 20);
        
        

        if (Input.GetKey(KeyCode.LeftShift)) throttle += throttleIncrease;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrease;
        throttle = Math.Clamp(throttle, 0f, 100f);
    }
    

    private void Update() {
        HandleInput(); 
        ControlSurface();
    }
    

    private void FixedUpdate()
    {
        float engineefficiency = 1 - (rb.position.y / Maxheight);
        float engine_power = throttle * MaxThrust;
        Mathf.Lerp(0, engine_power, Time.deltaTime * 2);
        
        float gravityConstant = 9.8f;
        float dragForce = 0.5f * Airdensity * rb.velocity.sqrMagnitude * 1 * 10;
        Vector3 dragDirection = -rb.velocity.normalized;
        Vector3 dragForceVector = dragDirection * dragForce;
        
        float AOA = aoa();
        
        
        Vector3 liftForceVector = Vector3.up * 0.5f * Airdensity * (rb.velocity.magnitude) * (rb.velocity.magnitude) * C_L * wingArea;
        
        rb.AddForce(transform.forward * engine_power * engineefficiency * AOA);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(transform.forward * roll * responseModifier * 0.6f);

        rb.AddForce(dragForceVector);
        rb.AddForce(liftForceVector);
        

        
        Vector3 gravity = Vector3.down * gravityConstant * rb.mass;
        rb.AddForce(gravity);
        
        float heightChange = rb.velocity.y * Time.fixedDeltaTime;
        float potentialEnergyChange = gravityConstant * rb.mass * heightChange;
        float kineticEnergyChange = -potentialEnergyChange; 

        float currentSpeed = rb.velocity.magnitude;
        float newSpeed = Mathf.Sqrt(Mathf.Max(0, currentSpeed * currentSpeed + (2 * kineticEnergyChange) / rb.mass));
        Vector3 newVelocity = rb.velocity.normalized * newSpeed;
        rb.velocity = newVelocity;
    }
    
    private float aoa()
    {
        Vector3 rotation = transform.eulerAngles;
        float pitchAngle = rotation.x;
        
        pitchAngle = Mathf.DeltaAngle(0, pitchAngle);

        float radianAngle = pitchAngle * Mathf.Deg2Rad;
        float aoaValue = Mathf.Sin(-radianAngle) * 2/3 + 1;
        
        aoaValue = Mathf.Round(aoaValue * 100f) / 100f;
        
        return aoaValue;
    }


    private void ControlSurface()
    {
        Quaternion PitchAngle = Quaternion.Euler(-PitchControl, 0f, 0f);
        PitchSurface.transform.rotation = transform.rotation * PitchAngle;
        if(pitch == 0) PitchControl = Mathf.Lerp(PitchControl, 0, Time.deltaTime*2);
        
        Quaternion YawAngle = Quaternion.Euler(0f, -YawControl, 90f);
        YawSurface.transform.rotation = transform.rotation * YawAngle;
        if(yaw == 0) YawControl = Mathf.Lerp(YawControl, 0, Time.deltaTime*2);
        
        Quaternion LeftRollAngle = Quaternion.Euler(-LeftRollControl , 0f, 0f);
        LeftRolluface.transform.rotation = transform.rotation * LeftRollAngle;
        if(roll == 0) LeftRollControl = Mathf.Lerp(LeftRollControl, 0, Time.deltaTime*2);
        
        Quaternion RightRollAngle = Quaternion.Euler(-RightRollControl, 0f,0f);
        RightRollSurface.transform.rotation = transform.rotation * RightRollAngle;
        if(roll == 0) RightRollControl = Mathf.Lerp(RightRollControl, 0, Time.deltaTime*2);


    }
}
