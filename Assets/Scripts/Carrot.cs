using DG.Tweening;
using Systems;
using UnityEngine;

public class Carrot : MonoBehaviour
{
    [SerializeField] private Transform _transform = default;
    [SerializeField] private Transform _bodyTransform = default;
    [SerializeField] private Transform[] _leaves = default;
    [SerializeField] private Rigidbody _rigidbody = default;
    [SerializeField] private ParticleSystem _particles = default;

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
    
    [SerializeField] private LayerMask _groundLayerMask = default;
    
    [Header("Harvest")]
    [SerializeField] private Vector3 _harvestStrenght = default;
    [SerializeField] private float _harvestDuration = 0.25f;
    [SerializeField] private int _harvestVibrato = 10;
    
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
        if (_isDragged)
        {
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _dragZ));
            _transform.position = Vector3.Lerp(_transform.position, mouseWorldPosition, 0.1f);;
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

    public void SetSelected()
    {
        _isSelected = true;
        _danceSequence.Kill();

        PlayHarvestAnimation();
    }

    private void PlayHarvestAnimation()
    {
        SetHarvestSequence();
        DOVirtual.DelayedCall(0.25f, BeginDrag);
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

            Vector3 angularForce = _mouseVelocity * _angularForceMultiplier;
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
}
