using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawyer : MonoBehaviour
{
    public List<Axis> _axis_list = new List<Axis>();
    public Cup _cup;

    void Start()
    {
    }

    void Update()
    {
        UpdateAxis5();
    }
    
    // make Arm5 horizontal
    public void UpdateAxis5()
    {
        float y5 = _axis_list[5].transform.position.y;
        float y6 = _axis_list[6].transform.position.y;

        // 0.01f to avoid vibration
        if (y5 > y6 + 0.01f) _axis_list[5].RotateAngle(1f);
        if (y5 < y6 - 0.01f) _axis_list[5].RotateAngle(-1f);
    }
}
