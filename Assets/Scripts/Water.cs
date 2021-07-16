using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    void FixedUpdate()
    {
        if(transform.position.y < 0)
        {
            Destroy(gameObject);
        }
    }
}
