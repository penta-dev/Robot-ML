using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisTester : MonoBehaviour
{
    public List<Axis> axis = new List<Axis>();
    
    // Update is called once per frame
    void Update()
    {
        foreach(Axis a in axis)
        {
            a.RotateAngle(Time.deltaTime * 10);
        }
    }
}
