using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cup : MonoBehaviour
{
    public RobotControllerAgent _agent;

    // indicate where water drops
    public Transform _drop_pos;

    // water prefab
    public GameObject _water_prefab;
    
    // if angle_cup is less than this value, water drops
    public float _water_drop_angle_limit;

    [System.NonSerialized]
    public float _angle_cup;
    
    private float _time;    // current time
    private float _water_drop_time;

    // water drops periodically
    public float _water_drop_dur;

    void Start()
    {
        _time = 0;
        _water_drop_time = 0;
    }
    public void UpdateProperties()
    {
        _angle_cup = Vector3.Angle(transform.up, new Vector3(transform.up.x, 0, transform.up.z));
        if (transform.up.y < 0) _angle_cup = -_angle_cup;
        //Debug.Log("Angle of cup = " + _angle_cup + "     , y = " + transform.up.y);
    }

    void FixedUpdate()
    {
        _time += Time.deltaTime;
        UpdateProperties();

        if (_angle_cup < _water_drop_angle_limit)
        {
            _agent.OnWaterDrop(_drop_pos.position);

            if (_water_drop_time < _time)
            {
                //Instantiate(_water_prefab, newpos, Quaternion.Euler(0,0,0));
                //_water_drop_time = _time + _water_drop_dur;
            }
        }
    }
}
