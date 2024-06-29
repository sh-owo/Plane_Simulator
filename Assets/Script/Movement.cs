using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Plane Stats")]
    public float throttleIncrease;
    public float MaxSpeed;
    public float responsiveness;
    [SerializeField] private float Maxheight;

    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;
    
    //Lift Setting
     private float C_L = 1f;
     private float Airdensity = 1.225f;
    [SerializeField] private float wingArea = 12f;

    private float GravityConstant = 9.8f;
    


    Rigidbody rb;
    [Header("HUD")]
    [SerializeField] TextMeshProUGUI hud;
    [SerializeField] Slider sliderthrottle;

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
        throttle = Math.Clamp(throttle, -5f, 100f);
    }
    
    private float responseModifier {
        get
        {
            float new_responseiveness = responsiveness * (( MaxSpeed / rb.velocity.magnitude) / 10);
            new_responseiveness = Mathf.Clamp(new_responseiveness, 4f, responsiveness + 12);
            return(rb.mass / 10f) * new_responseiveness;
        }
    }
    

    private void Update() {
        HandleInput(); 
        updatehud();
        throttleSlider();
        ControlSurface();
    }
    
    private void FixedUpdate()
    {

        Vector3 dragDirection = -rb.velocity.normalized;
        Vector3 dragForceVector = dragDirection * Calculate_DragForce();
        
        float aoa = AOA();
        
        
        Vector3 liftForceVector = Vector3.up * 0.5f * Airdensity * (rb.velocity.magnitude) * (rb.velocity.magnitude) * C_L * wingArea * aoa;
        
        rb.AddForce(transform.forward * Calculate_Thrust());
        rb.AddTorque(transform.up * yaw * responseModifier * 1.1f);
        rb.AddTorque(transform.right * pitch * responseModifier * 2f);
        rb.AddTorque(transform.forward * roll * responseModifier * 0.6f);
        Vector3 gravity = Vector3.down * GravityConstant * rb.mass;

        rb.AddForce(dragForceVector);
        rb.AddForce(liftForceVector);
        rb.AddForce(gravity);
        
        Vector3 newVelocity = rb.velocity.normalized * Calculate_Mechanical();
        rb.velocity = newVelocity;
        

    }

    private float Calculate_DragForce()
    {
        float radangle = AOA();
        float reference_Area = 10 * Mathf.Atan(radangle / 5f);
        float dragforce = 0.5f * AirDensity() * rb.velocity.sqrMagnitude * 1 * reference_Area;
        return reference_Area;
    }
    
    private float AOA()
    {
        Vector3 rotation = transform.eulerAngles;
        float pitchAngle = rotation.x;
        
        pitchAngle = Mathf.DeltaAngle(0, pitchAngle);

        float radianAngle = pitchAngle * Mathf.Deg2Rad;
        float aoaValue = Mathf.Cos(radianAngle) / 2f;
        
        aoaValue = Mathf.Round(aoaValue * 100f) / 100f;
        
        return aoaValue;
    }

    private float Calculate_Mechanical()
    {
        float heightChange = rb.velocity.y * Time.fixedDeltaTime;
        float potentialEnergyChange = GravityConstant * rb.mass * heightChange;
        float kineticEnergyChange = -potentialEnergyChange; 

        float currentSpeed = rb.velocity.magnitude;
        float newSpeed = Mathf.Sqrt(Mathf.Max(0, currentSpeed * currentSpeed + (2 * kineticEnergyChange) / rb.mass));
        return newSpeed;
    }
    private float Calculate_Thrust()
    {
        float ambient_Pressure = AirDensity();
        
        float massFlowRate = 80.0f;     // 공기의 질량 유량 (kg/s)
        float exhaustVelocity = 120.0f;  // 엔진에서 배출되는 가스의 속도 (m/s)
        float aircraftVelocity = rb.velocity.magnitude; // 비행기의 속도 (m/s)
        float exhaustPressure = 5000.0f; // 배출된 가스의 압력 (Pa)
        float exhaustArea = 1.2f;        // 배출구의 단면적 (m²)

        // 추력 계산
        float thrust = Mathf.Lerp(0f,massFlowRate * (exhaustVelocity - aircraftVelocity) + (exhaustPressure - ambient_Pressure) * exhaustArea,Time.deltaTime * 2);
        // Debug.Log(thrust);
        
        float Engine_efficiency = 1 - (rb.position.y / Maxheight);
        float engine_power = throttle * thrust;
        Mathf.Lerp(0, engine_power, Time.deltaTime * 2);
        return engine_power * Engine_efficiency;
    }
    private float AirDensity()
    {
        float L = 0.0065f;    // 온도 감율 (K/m)
        float T0 = 288.15f;   // 해수면에서의 기준 온도 (K)
        float g = 9.81f;      // 중력 가속도 (m/s²)
        float M = 0.029f;     // 공기의 몰질량 (kg/mol)
        float R = 8.314f;     // 기체 상수 (J/(mol·K))
        
        float h = rb.position.y;  

        float new_Aisdensity = Airdensity * Mathf.Pow(1 - (L * h) / T0, (g * M) / (R * L));
        return new_Aisdensity;
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


    //HUD 부분
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

