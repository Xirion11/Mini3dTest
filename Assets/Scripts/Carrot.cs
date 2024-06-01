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

    private Sequence _danceSequence = default;

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
}
