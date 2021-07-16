using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class RobotControllerAgent : Agent
{
    public GameObject _wheelchair;
    public GameObject _head;
    public GameObject _mouse;
    public GameObject _floor;
    public Cup _cup;

    public List<Axis> _axis = new List<Axis>();
    
    public Material _s_material;    // for success
    public Material _f_material;    // for failed

    private float _angle_wheelchair;

    public override void Initialize()
    {        
        
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array.SetValue(0, 0);
        actionsOut.DiscreteActions.Array.SetValue(0, 1);
        actionsOut.DiscreteActions.Array.SetValue(0, 2);
        actionsOut.DiscreteActions.Array.SetValue(0, 3);

        // Rotate
        if (Input.GetKey(KeyCode.Q)) actionsOut.DiscreteActions.Array.SetValue(1, 0);   // Axis0 +
        else if (Input.GetKey(KeyCode.A)) actionsOut.DiscreteActions.Array.SetValue(2, 0);   // Axis0 -

        if (Input.GetKey(KeyCode.W)) actionsOut.DiscreteActions.Array.SetValue(1, 1);   // Axis1 +
        else if (Input.GetKey(KeyCode.S)) actionsOut.DiscreteActions.Array.SetValue(2, 1);   // Axis1 -

        if (Input.GetKey(KeyCode.E)) actionsOut.DiscreteActions.Array.SetValue(1, 2);   // Axis3 +
        else if (Input.GetKey(KeyCode.D)) actionsOut.DiscreteActions.Array.SetValue(2, 2);   // Axis3 -

        if (Input.GetKey(KeyCode.R)) actionsOut.DiscreteActions.Array.SetValue(1, 3);   // Axis6 +
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1) _axis[0].RotateAngle(0.3f);
        else if (actions.DiscreteActions[0] == 2) _axis[0].RotateAngle(-0.3f);

        if (actions.DiscreteActions[1] == 1) _axis[1].RotateAngle(0.5f);
        else if (actions.DiscreteActions[1] == 2) _axis[1].RotateAngle(-0.5f);

        if (actions.DiscreteActions[2] == 1) _axis[3].RotateAngle(0.7f);
        else if (actions.DiscreteActions[2] == 2) _axis[3].RotateAngle(-0.7f);

        if (actions.DiscreteActions[3] == 1) _axis[6].RotateAngle(1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // (-1.0, 0.9, 0.3),(-0.4, 1.1, 0.1),0.4232951,(-0.1, 0.3, 0.1), 0.01889691  

        // 3 variables - vector from wheelchair to _axis[3]
        var vec_w_3 = _axis[3].transform.position - _wheelchair.transform.position;
        sensor.AddObservation(vec_w_3);

        // 3 variables - vector from wheelchair to _axis[5]
        var vec_w_5 = _axis[5].transform.position - _wheelchair.transform.position;
        sensor.AddObservation(vec_w_5);

        // 1 variable - distance between cup and head
        float d_cup_head = Vector3.Distance(_cup.transform.position, _head.transform.position);
        sensor.AddObservation(d_cup_head * 2.5f);

        // 3 variables - vector from mouse to drop_pos
        var vec_m_drop = _cup._drop_pos.position - _mouse.transform.position;
        sensor.AddObservation(vec_m_drop * 3.5f);

        // 1 variable = angle_cup
        float a_cup = 1 - (_cup._angle_cup - _cup._water_drop_angle_limit) / (90 - _cup._water_drop_angle_limit);
        sensor.AddObservation(a_cup);

        //Debug.Log(vec_w_3 + "," + vec_w_5 + "," + d_cup_head + "," + vec_m_drop + "," + a_cup);
    }

    public void Reset()
    {
        // wheelchair reset
        // set distance from center pos, set rotationY
        float r = 1.2f;// UnityEngine.Random.Range(1f, 1.3f);
        float angle = UnityEngine.Random.Range(0, 2 * 3.14159f);

        float x = r * (float)Math.Cos(angle) + _floor.transform.position.x;
        float z = r * (float)Math.Sin(angle) + _floor.transform.position.z;
        Vector3 pos = new Vector3(x, _wheelchair.transform.position.y, z);
        _wheelchair.transform.position = pos;
        _wheelchair.transform.LookAt(_floor.transform);
        _wheelchair.transform.eulerAngles = new Vector3(0, _wheelchair.transform.eulerAngles.y + 180, 0);
        
        // init sawyer
        foreach (Axis axis in _axis)
        {
            axis.InitAngle();
        }

        // init axis[0]
        _axis[0].transform.LookAt(_wheelchair.transform);
        _axis[0].transform.Rotate(new Vector3(0, 1, 0), 40);
    }
    // for collision
    public void OnFailed()
    {
        AddReward(-1);
        EndEpisode();

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _f_material;
    }
    public void OnSuccess()
    {
        AddReward(100);   // success
        EndEpisode();

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _s_material;
    }
    // wrong drop water
    public void OnDropWrong()
    {
        AddReward(-0.1f);
        EndEpisode();

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _f_material;
    }
    public void OnWaterDrop(Vector3 pos)
    {
        if (pos.y < _mouse.transform.position.y)
        {
            OnFailed(); // Water drops lower than mouse's height
        }
        else
        {
            pos.y = 0;
            Vector3 mpos = _mouse.transform.position; mpos.y = 0;
            if (Vector3.Distance(pos, mpos) < 0.03) // 0.03 is mouse' radius
            {
                OnSuccess();
            }
            else
            {
                OnDropWrong(); // out of mouse
            }
        }
    }
    void Update()
    {
        var pos = _cup._drop_pos.position;
        if (pos.y < _mouse.transform.position.y)
        {
            OnFailed(); // Water drops lower than mouse's height
            return;
        }
        if(pos.y > _mouse.transform.position.y + 0.5f)
        {
            OnFailed(); // prevent too high dropping
            return;
        }

        ///////////////// Reward for drop_pos position ///////////////////
        Debug.DrawLine(pos, _mouse.transform.position);
        pos.y = 0;
        var v = _mouse.transform.position; v.y = 0;
        float distance = Vector3.Distance(pos, v);
        float angle_reward = 1 - (_cup._angle_cup - _cup._water_drop_angle_limit) / (90 - _cup._water_drop_angle_limit); // 0 ~ 1
        if (distance > 0.3f)
        {
            OnFailed();
            return;
        }
        else
        if (distance > 0.03f)
        {
            float distance_reward = 0.03f / distance; // 0 ~ 1
            AddReward(distance_reward * angle_reward * 0.001f);
        }
        else
        {   // around mouse
            AddReward(angle_reward * 0.01f);
        }
    }
    
    /*
    // test - Reset function
    float _time = 0;
    float _lastTime = 0;
    void Update()
    {
        _time += Time.deltaTime;
        if(_lastTime < _time)
        {
            _lastTime = _time + 3;
            Reset();
        }
    }
    */
}
