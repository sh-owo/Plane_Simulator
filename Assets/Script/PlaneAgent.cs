using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class PlaneAgent : Agent
{
    private PlaneController planecontroller;
    
    /*
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add observations to the sensor
        // Example: observe position, velocity, throttle, controls
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(PlaneController.GetVelocity());
        sensor.AddObservation(PlaneController.GetThrottle());
        sensor.AddObservation(PlaneController.GetPitchControl());
        sensor.AddObservation(PlaneController.GetYawControl());
        sensor.AddObservation(PlaneController.GetRollControl());
        
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        // Apply actions to the plane based on vectorAction
        float throttle = Mathf.Clamp(vectorAction[0], -1f, 1f);  // Example: throttle
        float pitch = Mathf.Clamp(vectorAction[1], -1f, 1f);     // Example: pitch control
        float yaw = Mathf.Clamp(vectorAction[2], -1f, 1f);       // Example: yaw control
        float roll = Mathf.Clamp(vectorAction[3], -1f, 1f);      // Example: roll controlㅌㅌ

        PlaneController.ApplyAction(throttle, pitch, yaw, roll);  // Implement this method in PlaneAgent
    }
    */
    
    
}
