using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;

public class DirtPile : MonoBehaviour
{
    [SerializeField] private Transform _carrotSpot = default;
    [SerializeField] private CarrotPool _pool = default;

    private void Start()
    {
        var carrot = _pool.Get(_carrotSpot);
    }
}
