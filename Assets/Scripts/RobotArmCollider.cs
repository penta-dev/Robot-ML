using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmCollider : MonoBehaviour
{
    public RobotControllerAgent _agent;

    void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterOrStay(other);
    }
    void OnTriggerStay(Collider other)
    {
        OnTriggerEnterOrStay(other);
    }
    void OnTriggerEnterOrStay(Collider other)
    {
        // Debug.Log("Detected collision between " + gameObject.tag + " and " + other.gameObject.tag);
        if(!other.gameObject.CompareTag("Water"))
        {
            _agent.OnFailed();
        }
    }
}
