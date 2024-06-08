using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform[] fov;
    [SerializeField] float smooth;

    private int index = 0;
    private Vector3 target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) index += 1;
        if (index > 4) index = 0;


        target = fov[index].position;

    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target,Time.deltaTime * smooth);
        transform.forward = fov[index].forward;
    }
}
