using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target = default;
    [SerializeField] private float _smoothFactor = 10f;
    [SerializeField] private Vector3 _offset; 

    private Transform _transform;

    private Transform Transform
    {
        get
        {
            if (_transform == null)
            {
                _transform = GetComponent<Transform>();
            }

            return _transform;
        }
    }
    
    private void LateUpdate()
    {
        var targetPosition = _target.position + _offset;
        var smoothPosition = Vector3.Lerp(Transform.position, targetPosition, _smoothFactor * Time.deltaTime);
        Transform.position = smoothPosition;
        Transform.LookAt(_target);
    }
}
