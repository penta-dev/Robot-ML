using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class RobotControllerAgent : Agent
{
    public bool _is_training;
    public bool _repeated_simulation;
    private bool _frozen = false;

    public GameObject _wheelchair;
    public Human _human;
    public GameObject _floor;
    public Cup _cup;

    public Sawyer _sawyer;
    
    public Material _s_material;    // for success
    public Material _f_material;    // for failed
    public Material _p_material;    // for progress

    public override void Initialize()
    {        
        
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array.SetValue(0, 0);
        actionsOut.ContinuousActions.Array.SetValue(0, 1);
        actionsOut.ContinuousActions.Array.SetValue(0, 2);
        actionsOut.ContinuousActions.Array.SetValue(0, 3);

        // Rotate
        if (Input.GetKey(KeyCode.Q)) actionsOut.ContinuousActions.Array.SetValue(-0.3f, 0);   // Axis0 -
        else if (Input.GetKey(KeyCode.A)) actionsOut.ContinuousActions.Array.SetValue(0.3f, 0);   // Axis0 +

        if (Input.GetKey(KeyCode.W)) actionsOut.ContinuousActions.Array.SetValue(-0.5f, 1);   // Axis1 -
        else if (Input.GetKey(KeyCode.S)) actionsOut.ContinuousActions.Array.SetValue(0.5f, 1);   // Axis1 +

        if (Input.GetKey(KeyCode.E)) actionsOut.ContinuousActions.Array.SetValue(-0.5f, 2);   // Axis3 -
        else if (Input.GetKey(KeyCode.D)) actionsOut.ContinuousActions.Array.SetValue(0.5f, 2);   // Axis3 +

        if (Input.GetKey(KeyCode.R)) actionsOut.ContinuousActions.Array.SetValue(0.5f, 3);   // Axis6 +
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_frozen) return;

        _sawyer.RotateAxis(0, actions.ContinuousActions[0]*0.3f);    // -1 ~ 1
        _sawyer.RotateAxis(1, actions.ContinuousActions[1]*0.5f);    // -1 ~ 1
        _sawyer.RotateAxis(3, actions.ContinuousActions[2]*0.5f);    // -1 ~ 1
        
        float a6 = (actions.ContinuousActions[3] + 1) / 2 * 0.5f;  // 0 ~ 1
        _sawyer.RotateAxis(6, a6);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // (-1.0, 0.9, 0.3),(-0.4, 1.1, 0.1),0.4232951,(-0.1, 0.3, 0.1), 0.01889691  

        // 3 variables - vector from wheelchair to _axis[3]
        var vec_w_3 = _sawyer.AxisPos(3) - _wheelchair.transform.position;
        sensor.AddObservation(vec_w_3);

        // 3 variables - vector from wheelchair to _axis[5]
        var vec_w_5 = _sawyer.AxisPos(5) - _wheelchair.transform.position;
        sensor.AddObservation(vec_w_5);

        // 1 variable - distance between cup and head
        float d_cup_head = Vector3.Distance(_cup.transform.position, _human.GetHeadPos());
        sensor.AddObservation(d_cup_head * 2.5f);   // normalize

        // 3 variables - vector from mouse to drop_pos
        var vec_m_drop = _cup._drop_pos.position - _human.GetMousePos();
        sensor.AddObservation(vec_m_drop * 3.5f);   // normalize

        // 1 variable = angle_cup
        float a_cup = _cup._angleY / 90;
        sensor.AddObservation(a_cup);
    }

    public void Reset()
    {
        if(_is_training)
        {
            // wheelchair position
            float r = 1.2f;// UnityEngine.Random.Range(1f, 1.3f);
            float angle = UnityEngine.Random.Range(0, 2 * 3.14159f);
            float x = r * (float)Math.Cos(angle) + _floor.transform.position.x;
            float z = r * (float)Math.Sin(angle) + _floor.transform.position.z;
            Vector3 pos = new Vector3(x, _wheelchair.transform.position.y, z);
            _wheelchair.transform.position = pos;

            // wheelchair rotation
            _wheelchair.transform.LookAt(_floor.transform);
            _wheelchair.transform.eulerAngles = new Vector3(0, _wheelchair.transform.eulerAngles.y + 180, 0);
        }

        // reset Human
        _human.Reset();

        // reset sawyer
        _sawyer.Reset(_wheelchair);

        // reset cup
        _cup.Reset();
    }
    
    public void OnFailed()
    {
        if (_is_training)
        {
            AddReward(-1);
            EndEpisode();
        }
        else
        {
            if (_repeated_simulation)
            {
                EndEpisode();
            }
            else
            {
                _frozen = true;
            }
        }

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _f_material;
    }
    void OnSuccess()
    {
        if (_is_training)
        {
            EndEpisode();
        }
        else
        {
            if (_repeated_simulation)
            {
                EndEpisode();
            }
            else
            {
                _frozen = true;
            }
        }

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _s_material;
    }
    void OnWrongDrop()
    {
        if (_is_training)
        {
            AddReward(-0.5f);
            EndEpisode();
        }
        else
        {
            if(_repeated_simulation)
            {
                EndEpisode();
            }
            else
            {
                _frozen = true;
            }
        }

        MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
        mesh.material = _f_material;
    }
    void OnRightDrop()
    {
        _human.EatFood(1);

        if (_is_training)
        {
            AddReward(1f);
        }

        if (_human.IsSatisfied())
        {
            OnSuccess();
        }
        else
        {
            MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
            mesh.material = _p_material;    // progress
        }
    }
    public void OnWaterDrop()
    {
        if (_cup.GetDropPos().y < _human.GetMousePos().y)
        {
            OnFailed(); // Water drops lower than mouse's height
        }
        else
        {
            float distance = GetXZDistance(_cup.GetDropPos(), _human.GetMousePos());
            if (distance < _human.GetMouseRadius())
            {
                OnRightDrop();  // drop water to mouse
            }
            else
            {
                OnWrongDrop(); // out of mouse
            }
        }
    }
    void FixedUpdate()
    {
        if (StepCount > 500) EndEpisode();

        Debug.DrawLine(_cup.GetDropPos(), _human.GetMousePos(), Color.red); // for debug

        if(_is_training)
        {
            float distance = Vector3.Distance(_cup.GetDropPos(), _human.GetMousePos());
            float distance_reward = distance < _human.GetMouseRadius() ? 1 : _human.GetMouseRadius() / distance;    // 0 ~ 1
            float angle_reward = 1 - _cup._angleY / 90; // 0 ~ 1
            AddReward(distance_reward * angle_reward * 0.1f);
        }
    }
    float GetXZDistance(Vector3 v1, Vector3 v2)
    {
        v1.y = 0;
        v2.y = 0;
        return Vector3.Distance(v1, v2);
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
