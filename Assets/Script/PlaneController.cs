using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaneController : MonoBehaviour
{
    [Header("Plane Stats")]
    public float throttleIncrease = 0.1f;
    public float MaxThrust = 500f;
    public float responsiveness = 10f;

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
    }
    
    
    private void FixedUpdate() {
        float dragForce = 0.5f * 1 * 10 * Airdensity * rb.velocity.sqrMagnitude;
        Vector3 dragDirection = -rb.velocity.normalized;
        Vector3 dragForceVector = dragDirection * dragForce;

        rb.AddForce(transform.forward * MaxThrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(transform.forward * roll * responseModifier);
        
        rb.AddForce(dragForceVector);
        rb.AddForce(Vector3.up * 0.5f * Airdensity * (rb.velocity.magnitude) * (rb.velocity.magnitude) * C_L * wingArea);
        
    }

    private void updatehud()
    {
        hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "Km/h\n";
        hud.text += "Altitude: " + transform.position.y.ToString("F0") + "m";
    }
}
