using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawyer : MonoBehaviour
{
    public List<Axis> _axis = new List<Axis>();

//     public List<GameObject> _items = new List<GameObject>();
//     private List<Vector3> _pos0 = new List<Vector3>();
//     private List<Vector3> _angle0 = new List<Vector3>();

    void Start()
    {
//         int len = _items.Count;
//         for (int i = 0; i < len; i++)
//         {
//             _pos0.Add(_items[i].transform.position);
//             _angle0.Add(_items[i].transform.eulerAngles);
//         }
    }

    public void Reset(GameObject target)
    {
//         for (int i = 0; i < _items.Count; i++)
//         {
//             _items[i].transform.position = _pos0[i];
//             _items[i].transform.eulerAngles = _angle0[i];
//         }

        foreach (Axis axis in _axis)
        {
            axis.Reset();
        }

        // Rotate Axis0 for target direction
        //_axis[0].transform.LookAt(target.transform);
        //_axis[0].transform.Rotate(new Vector3(0, 1, 0), 45);
    }
    void FixedUpdate()
    {
        //Make5Horizontal();
    }
    // make Axis5 horizontal
    public void Make5Horizontal()
    {
        float y5 = _axis[5].transform.position.y;
        float y6 = _axis[6].transform.position.y;

        // 0.01f to avoid vibration
        if (y5 > y6 + 0.01f) _axis[5].RotateAngle(1f);
        else if (y5 < y6 - 0.01f) _axis[5].RotateAngle(-1f);
    }
    public void RotateAxis(int index, float angle)
    {
        _axis[index].RotateAngle(angle);
    }
    public Vector3 AxisPos(int index)
    {
        return _axis[index].transform.position;
    }
}
