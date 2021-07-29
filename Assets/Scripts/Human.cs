using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField]
    private GameObject _head;
    private Vector3 _head_rotation0;
    private Vector3 _head_position0;

    [SerializeField]
    public GameObject _mouse;

    [SerializeField]
    private Collider _head_collider;

    //[SerializeField]
    private int _max_food_amount; // max food amount    
    private int _food_amount; // food amount

    void Start()
    {
        _head_rotation0 = _head.transform.localEulerAngles;
        _head_position0 = _head.transform.localPosition;
    }

    public void Reset()
    {
        _head.transform.localEulerAngles = _head_rotation0;
        _head.transform.localPosition = _head_position0;
        _max_food_amount = 30;//Random.Range(10, 30);
        _food_amount = 0;
    }
    public float GetMouseRadius()
    {
        return _mouse.transform.localScale.x / 2;
    }
    public Vector3 GetHeadPos()
    {
        return _head_collider.transform.position;
    }
    public Vector3 GetMousePos()
    {
        return _mouse.transform.position;
    }

    public bool IsSatisfied()
    {
        if (_food_amount >= _max_food_amount) return true;
        return false;
    }

    public bool EatFood(int amount)
    {
        _food_amount += amount;
        if (_food_amount == _max_food_amount) ShakeHead();

        return _food_amount <= _max_food_amount;
    }
    public int GetAmount()
    {
        return _food_amount;
    }
    public int GetMaxAmount()
    {
        return _max_food_amount;
    }
    private void ShakeHead()
    {
        _head.transform.Rotate(new Vector3(0, 1, 0), 45);
    }
}
