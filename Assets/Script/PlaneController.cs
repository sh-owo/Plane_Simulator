using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour
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
    [SerializeField] private float liftfactor = 0.8f;
    
    private float responseModifier {
        get {
            return(rb.mass / 10f) * responsiveness;
        }
    }

    Rigidbody rb;
    [Header("HUD")]
    [SerializeField] TextMeshProUGUI hud;
    [SerializeField] Slider sliderthrottle;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInput() {
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.LeftShift)) throttle += throttleIncrease;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrease;
        throttle = Math.Clamp(throttle, 0f, 100f);
    }
    

    private void Update() {
        HandleInput(); 
        updatehud();
        throttleSlider();
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
        
        rb.AddForce(transform.forward * engine_power * engineefficiency);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(transform.forward * roll * responseModifier);

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

        pitchAngle = -Mathf.DeltaAngle(0, pitchAngle);
        Debug.Log(pitchAngle);
        float aoavalue = Mathf.Sin(pitchAngle);
        return pitchAngle;
    }



    private void updatehud()
    {
        hud.text = "Throttle: " + throttle.ToString("F0") + "%\n\n";
        hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "Km/h\n";
        hud.text += "Altitude: " + transform.position.y.ToString("F0") + "m\n";
        hud.text += "Vs: " + (rb.velocity.y * 3.28084f).ToString("F0") + "fpm";
    }
    private void throttleSlider()
    {
        sliderthrottle.minValue = 0f;
        sliderthrottle.maxValue = 100f;

        sliderthrottle.value = throttle;
    }
}
