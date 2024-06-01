using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Systems;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Carrot : MonoBehaviour
{
    [SerializeField] private Transform _transform = default;
    [SerializeField] private Transform _bodyTransform = default;
    [SerializeField] private Transform _innerTransform = default;
    [SerializeField] private Transform[] _leaves = default;
    [SerializeField] private Rigidbody _rigidbody = default;
    [SerializeField] private ParticleSystem _particles = default;

    [SerializeField] private float _leavesDelay = 1f;
    [SerializeField] private float _leavesDance = 10f;
    [SerializeField] private float _leavesDanceDuration = 1f;
    [SerializeField] private float _dragZ = 3.5f;
    [SerializeField] private float _dragForceMultiplier = 10.0f;
    [SerializeField] private float _angularForceMultiplier = 1f;

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
    
    [Header("Harvest")]
    [SerializeField] private Vector3 _harvestStrenght = default;
    [SerializeField] private float _harvestDuration = 0.25f;
    [SerializeField] private int _harvestVibrato = 10;
    //[SerializeField] private float _harvestElasticity = 1f;
    
    private Sequence _danceSequence = default;
    private Sequence _harvestSequence = default;
    private bool _isDragged = false;
    private GameObject _sphere;
    private Camera _mainCamera;
    private bool _isSelected = false;
    private CarrotPool _pool;
    private DirtPile _dirtPile;
    private Vector3 _previousMousePosition;
    private Vector3 _mouseVelocity;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        ShowLeaves();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Pulse();
            PlayHarvestAnimation();
        }

        if (_isDragged)
        {
            // Vector3 direction = _transform.position - Input.mousePosition;
            // Quaternion rotation = Quaternion.LookRotation(direction);
            // _transform.rotation = rotation;
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _dragZ));
            _transform.position = mouseWorldPosition;
            Debug.Log(_rigidbody.velocity);
            _mouseVelocity = (mouseWorldPosition - _previousMousePosition) / Time.deltaTime;
            _previousMousePosition = mouseWorldPosition;
        }
    }

    public void Setup(DirtPile dirtPile, Transform carrotSpot, CarrotPool pool)
    {
        _dirtPile = dirtPile;
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        _transform.position = carrotSpot.position;
        _transform.rotation = Quaternion.identity;
        ShowLeaves();
        _pool = pool;
    }
    
    private void RotateObjectToMouse()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        // Raycast to find the ground hit position
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
        {
            Vector3 newPosition = new Vector3(hit.point.x, 1f, hit.point.z);//hit.point + Vector3.up * 0.5f;
            //newPosition = new Vector3(newPosition.x, Mathf.Clamp(newPosition.y, minY, maxY), newPosition.z);
            _sphere.transform.position = newPosition;
            _sphere.transform.localScale = Vector3.one * 0.1f;
            
            Vector3 direction = (_sphere.transform.position - _bodyTransform.position).normalized;

            // Calculate the rotation towards the target point
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            _bodyTransform.rotation = Quaternion.Slerp(_bodyTransform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
        }
    }

    private void Dance()
    {
        _danceSequence?.Kill();
        _danceSequence = DOTween.Sequence();
        _danceSequence.Append(
            _bodyTransform.DORotate(new Vector3(0f, 0f, -_leavesDance), _leavesDanceDuration)
                .SetEase(Ease.InOutBack)
            );
        _danceSequence.Append(
            _bodyTransform.DORotate(new Vector3(0f, 0f, _leavesDance), _leavesDanceDuration)
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

    // private void Pulse()
    // {
    //     _danceSequence = DOTween.Sequence();
    //     _danceSequence.Append(_bodyTransform.DOPunchScale(_pulsePunch, _pulseDuration, _pulseVibrato, _pulseElasticity).SetEase(Ease.InOutBack));
    //     _danceSequence.Play();
    // }

    public void SetSelected()
    {
        _isSelected = true;
        _danceSequence.Kill();
        
        //_innerTransform.RotateAround(_innerTransform.position, Vector3.right, 90f);

        PlayHarvestAnimation();
    }

    private void PlayHarvestAnimation()
    {
        SetHarvestSequence();
        DOVirtual.DelayedCall(0.25f, () => { BeginDrag(); });
    }

    private void SetHarvestSequence()
    {
        _harvestSequence = DOTween.Sequence();
        _harvestSequence.Append(_bodyTransform.DOShakePosition(_harvestDuration, _harvestStrenght, _harvestVibrato));
        _harvestSequence.Join(_bodyTransform.DOScale(new Vector3(1f, 1.5f, 1f), 0.25f));
        _harvestSequence.Play();
    }

    private void BeginDrag()
    {
        _bodyTransform.DOScale(Vector3.one, 0.25f);
        if (_isSelected)
        {
            Vector3 mouseWorldPosition =
                _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _dragZ));
            _dirtPile.OnCarrotHarvested();
            _particles.Play();
            _transform.DOMove(mouseWorldPosition, 0.25f).OnComplete(() =>
            {
                _isDragged = true;
                _rigidbody.isKinematic = true;
                _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
            });
        }
        else
        {
            Dance();
        }
    }

    public void SetDeselected()
    {
        if(_isDragged)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.constraints = RigidbodyConstraints.None;
            DOVirtual.DelayedCall(2f, ReturnToPool);
            
            Vector3 force = _mouseVelocity * _dragForceMultiplier;
            _rigidbody.AddForce(force, ForceMode.Impulse);

            Vector3 angularForce = _mouseVelocity * _angularForceMultiplier;//new Vector3(-_mouseVelocity.y, _mouseVelocity.x, 0) * _angularForceMultiplier;
            _rigidbody.AddTorque(angularForce, ForceMode.Impulse);
        }
        _isDragged = false;
        _isSelected = false;
        
        
    }
    
    private void ReturnToPool()
    {
        _dirtPile.OnCarrotReturnedToPool();
        _pool.ReturnToPool(this);
    }

    private void OnCollisionEnter(Collision other)
    {
        // if (other.collider.name.CompareTo("Plane") == 0)
        // {
        //     _bodyTransform.DOPunchScale(_pulsePunch, _pulseDuration, _pulseVibrato, _pulseElasticity)
        //         .SetEase(Ease.InOutBack);
        // }
    }
}
