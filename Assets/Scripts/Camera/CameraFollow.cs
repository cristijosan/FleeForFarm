using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public Transform target;
    public Vector3 offsetCameraGame;
    public Vector3 offsetCameraMenu;
    public float smoothSpeed = 0.125f;

    private Vector3 desiredPosition;
    private Vector3 offsetCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeMenuOffset();
    }

    private void FixedUpdate()
    {
        desiredPosition = target.position + offsetCamera;
        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition; 
    }

    public void ChangeMenuOffset()
    {
        offsetCamera = offsetCameraMenu;
    }
    public void ChangeGameOffset()
    {
        offsetCamera = offsetCameraGame;
    }
}
