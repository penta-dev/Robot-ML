using System;
using UnityEngine;

public class Axis : MonoBehaviour
{
    public Vector3 rotationAxis;
    public int AxisID;

    // initial position and rotation
    private Vector3 _position0;
    private Vector3 _rotation0;
    private void Start()
    {
        _position0 = transform.position;
        _rotation0 = transform.eulerAngles;
    }

    public void InitAngle()
    {
        transform.position = _position0;
        transform.eulerAngles = _rotation0;
    }
    public void RotateAngle(float angle)
    {
        transform.Rotate(rotationAxis, angle);
    }
    public float GetAngle()
    {
        if (rotationAxis.x > 0)
        {
            return transform.localEulerAngles.x;
        }
        else if (rotationAxis.y > 0)
        {
            return transform.localEulerAngles.y;
        }
        else if (rotationAxis.z > 0)
        {
            return transform.localEulerAngles.z;
        }
        return 0;
    }
}