using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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

    public Text _display_text;

    private float _dist0;   // distance between droppos to mouse at first
    private int _success_episode_num = 0;

    public override void Initialize()
    {
        if(!_is_training && !_repeated_simulation)
        {
            Time.timeScale = 0.1f;
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array.SetValue(0, 0);
        actionsOut.ContinuousActions.Array.SetValue(0, 1);
        actionsOut.ContinuousActions.Array.SetValue(0, 2);
        actionsOut.ContinuousActions.Array.SetValue(0, 3);
        actionsOut.ContinuousActions.Array.SetValue(0, 4);
        actionsOut.ContinuousActions.Array.SetValue(0, 5);
        actionsOut.ContinuousActions.Array.SetValue(-1, 6);

        // Rotate
        if (Input.GetKey(KeyCode.Q)) actionsOut.ContinuousActions.Array.SetValue(-1, 0);   // Axis0 -
        else if (Input.GetKey(KeyCode.A)) actionsOut.ContinuousActions.Array.SetValue(1, 0);   // Axis0 +

        if (Input.GetKey(KeyCode.W)) actionsOut.ContinuousActions.Array.SetValue(-1, 1);   // Axis1 -
        else if (Input.GetKey(KeyCode.S)) actionsOut.ContinuousActions.Array.SetValue(1, 1);   // Axis1 +

        if (Input.GetKey(KeyCode.E)) actionsOut.ContinuousActions.Array.SetValue(-1, 2);   // Axis2 -
        else if (Input.GetKey(KeyCode.D)) actionsOut.ContinuousActions.Array.SetValue(1, 2);   // Axis2 +

        if (Input.GetKey(KeyCode.R)) actionsOut.ContinuousActions.Array.SetValue(-1, 3);   // Axis3 -
        else if (Input.GetKey(KeyCode.F)) actionsOut.ContinuousActions.Array.SetValue(1, 3);   // Axis3 +

        if (Input.GetKey(KeyCode.T)) actionsOut.ContinuousActions.Array.SetValue(-1, 4);   // Axis4 -
        else if (Input.GetKey(KeyCode.G)) actionsOut.ContinuousActions.Array.SetValue(1, 4);   // Axis4 +

        if (Input.GetKey(KeyCode.Y)) actionsOut.ContinuousActions.Array.SetValue(-1, 5);   // Axis4 -
        else if (Input.GetKey(KeyCode.H)) actionsOut.ContinuousActions.Array.SetValue(1, 5);   // Axis4 +

        if (Input.GetKey(KeyCode.U)) actionsOut.ContinuousActions.Array.SetValue(1, 6);   // Axis6 +
    }

    public override void OnEpisodeBegin()
    {
        Reset();

        // update display info
        if (!_is_training)
        {
            _display_text.text = _success_episode_num + " / " + CompletedEpisodes + " - " + 100.0f * _success_episode_num / CompletedEpisodes + " %";
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_frozen) return;

        float prev_angle = _cup._angleY;
        float prev_dist = Vector3.Distance(_cup._water_droppos, _human.GetMousePos());

        _sawyer.RotateAxis(0, actions.ContinuousActions[0]);
        _sawyer.RotateAxis(1, actions.ContinuousActions[1]);
        _sawyer.RotateAxis(2, actions.ContinuousActions[2]);
        _sawyer.RotateAxis(3, actions.ContinuousActions[3]);
        _sawyer.RotateAxis(4, actions.ContinuousActions[4]);
        _sawyer.RotateAxis(5, actions.ContinuousActions[5]);

        float a6 = (actions.ContinuousActions[6] + 1) / 2 ;
        _sawyer.RotateAxis(6, a6);

        _cup.UpdateProperties();

        if (_is_training) AddReward(-0.05f); // for min steps

        // Reward
        var angle = _cup._angleY;
        float dist = Vector3.Distance(_cup._water_droppos, _human.GetMousePos());

        float angle_reward = (prev_angle - angle) / 90;
        float dist_reward = (prev_dist - dist) / _dist0;

        if (dist < _human.GetMouseRadius())
        {
            if (_is_training) AddReward(angle_reward);
        }
        else
        {
            if (_is_training) AddReward(dist_reward);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 18 var
        for (int i = 1; i < 7; i++)
        {
            sensor.AddObservation(_sawyer.AxisPos(i) - _human.GetHeadPos());
        }

        // 3 variables - vector from head to cup
        var vec_h_c = _cup.transform.position - _human.GetHeadPos();
        sensor.AddObservation(vec_h_c);

        // 3 variables - vector from mouse to drop_pos
        var vec_m_drop = _cup._water_droppos - _human.GetMousePos();
        sensor.AddObservation(vec_m_drop);

        // 1 variable
        var aY = 1 - _cup._angleY / 90;
        sensor.AddObservation(aY);
    }

    public void Reset()
    {
        /*
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
        */

        // reset Human
        _human.Reset();

        // reset sawyer
        _sawyer.Reset(_wheelchair);

        // reset cup
        _cup.Reset();

        // reset
        _dist0 = Vector3.Distance(_cup._water_droppos, _human.GetMousePos());
    }
    void FixedUpdate()
    {
        if (StepCount > 100)
        {
            EndEpisode();

            MeshRenderer mesh = _floor.GetComponent<MeshRenderer>();
            mesh.material = _f_material;
        }

        Debug.DrawLine(_cup._water_droppos, _human.GetMousePos(), Color.red); // for debug        
    }
    public void OnWaterDrop()
    {
        float distance = Vector3.Distance(_cup._water_droppos, _human.GetMousePos());
        if (distance < _human.GetMouseRadius() && _cup._drop_index == 14)
        {
            OnRightDrop();  // drop water to mouse
        }
        else
        {
            OnWrongDrop(); // wrong
        }
    }
    public void OnFailed()
    {
        if (_is_training)
        {
            SetReward(-1);
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
        _success_episode_num++;

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
}
