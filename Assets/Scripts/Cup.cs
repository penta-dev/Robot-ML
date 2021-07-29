using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cup : MonoBehaviour
{
    public RobotControllerAgent _agent;

    // indicate where water drops
    public List<Transform> _droppos = new List<Transform>();

    // water prefab
    public GameObject _water_prefab;
    
    // if angle_cup is less than this value, water drops
    public float _water_drop_angle_limit;

    // Angle between Y axis and XZ flat, -90 ~ 90
    public float _angleY;

    // flag if water drops for the angle, each angle indicates one water unit, 0 ~ 90
    private List<bool> _water;

    public Vector3 _water_droppos;    // water drop position
    public int _drop_index;  // drop pos index

    void Start()
    {
        _water = new List<bool>();
        for (int i = 0; i < 91; i++)
            _water.Add(false);
    }

    public void Reset()
    {
        for(int i=0;i<91;i++)
        {
            _water[i] = false;
        }

        UpdateProperties();
    }

    private void UpdateDropPos()
    {
        _water_droppos = _droppos[0].transform.position;
        _drop_index = 0;
        for (int i = 0; i < _droppos.Count; i++)
        {
            if (_water_droppos.y > _droppos[i].position.y)
            {
                _water_droppos = _droppos[i].position;
                _drop_index = i;
            }
        }
    }
    
    public void UpdateProperties()
    {
        UpdateDropPos();

        // update _angleY
        _angleY = Vector3.Angle(transform.up, new Vector3(transform.up.x, 0, transform.up.z));
        if (transform.up.y < 0) _angleY = -_angleY;
        //Debug.Log("Angle of cup = " + _angleY + "     , y = " + transform.up.y);

        if (0 < _angleY && _angleY < _water_drop_angle_limit)
        {
            int index = (int)_angleY;    // angle index
            for (int angle = index; angle < _water_drop_angle_limit; angle++)
            {
                if (_water[angle] == true) break;   // already dropped

                // drop water for angle
                _water[angle] = true;
                _agent.OnWaterDrop();

                if(!_agent._is_training)
                {
                    Instantiate(_water_prefab, _water_droppos, Quaternion.Euler(0, 0, 0));
                }
            }
        }
    }
}
