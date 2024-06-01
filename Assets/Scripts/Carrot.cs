using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Carrot : MonoBehaviour
{
    [SerializeField] private Transform _transform = default;
    [SerializeField] private Transform _innerTransform = default;
    [SerializeField] private Transform[] _leaves = default;

    [SerializeField] private float _leavesDelay = 1f;
    [SerializeField] private float _leavesDance = 10f;
    [SerializeField] private float _leavesDanceDuration = 1f;

    [Header("Scale Punch")]
    [SerializeField] private Vector3 _punch = default;
    [SerializeField] private float _duration = 0.25f;
    [SerializeField] private int _vibrato = 10;
    [SerializeField] private float _elasticity = 1f;
    
    [Header("Carrot Pulse")]
    [SerializeField] private Vector3 _pulsePunch = default;
    [SerializeField] private float _pulseDuration = 0.25f;
    [SerializeField] private int _pulseVibrato = 10;
    [SerializeField] private float _pulseElasticity = 1f;
    [SerializeField] private LayerMask _groundLayerMask = default;
    
    private Sequence _danceSequence = default;
    private bool _isSelected = false;
    private GameObject _sphere;

    private void Start()
    {
        ShowLeaves();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pulse();
        }

        if (_isSelected)
        {
            // Vector3 direction = _transform.position - Input.mousePosition;
            // Quaternion rotation = Quaternion.LookRotation(direction);
            // _transform.rotation = rotation;
        }
    }
    
    void RotateObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast to find the ground hit position
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
        {
            Vector3 newPosition = new Vector3(hit.point.x, 1f, hit.point.z);//hit.point + Vector3.up * 0.5f;
            //newPosition = new Vector3(newPosition.x, Mathf.Clamp(newPosition.y, minY, maxY), newPosition.z);
            _sphere.transform.position = newPosition;
            _sphere.transform.localScale = Vector3.one * 0.1f;
            
            Vector3 direction = (_sphere.transform.position - _transform.position).normalized;

            // Calculate the rotation towards the target point
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
        }
    }

    private void Dance()
    {
        _danceSequence = DOTween.Sequence();
        _danceSequence.Append(
            _transform.DORotate(new Vector3(0f, 0f, -_leavesDance), _leavesDanceDuration)
                .SetEase(Ease.InOutBack)
            );
        _danceSequence.Append(
            _transform.DORotate(new Vector3(0f, 0f, _leavesDance), _leavesDanceDuration)
                .SetEase(Ease.InOutBack)
        );
        _danceSequence.SetLoops(-1, LoopType.Yoyo);
        _danceSequence.Play();
    }

    private void ShowLeaves()
    {
        foreach (var leaf in _leaves)
        {
            leaf.DOScale(Vector3.zero, 0f).OnComplete(() =>
            {
                leaf.gameObject.SetActive(true);
                leaf.DOScale(Vector3.one, 0.1f).OnComplete(() =>
                {
                    leaf.DOPunchScale(_punch, _duration, _vibrato, _elasticity);
                });
            });
        }

        DOVirtual.DelayedCall(0.35f, Dance);
    }

    private void Pulse()
    {
        _danceSequence = DOTween.Sequence();
        _danceSequence.Append(_transform.DOPunchScale(_pulsePunch, _pulseDuration, _pulseVibrato, _pulseElasticity).SetEase(Ease.InOutBack));
        _danceSequence.Play();
    }

    public void SetSelected()
    {
        _danceSequence.Kill();
        _isSelected = true;
        _innerTransform.RotateAround(_innerTransform.position, Vector3.right, 90f);
    }
}
