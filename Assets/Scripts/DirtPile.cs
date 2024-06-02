using System.Collections;
using DG.Tweening;
using Systems;
using UnityEngine;
using UnityEngine.UI;

public class DirtPile : MonoBehaviour
{
    [SerializeField] private Transform _bodyTransform = default;
    [SerializeField] private Transform _carrotSpot = default;
    [SerializeField] private CarrotPool _pool = default;
    [SerializeField] private float _carrotGrowthTime = 2f;
    [SerializeField] private Image _timerImage = default;
    [SerializeField] private ParticleSystem _particles = default;
    
    [Header("Sprout Scale Punch")]
    [SerializeField] private Vector3 _sproutPunch = default;
    [SerializeField] private float _sproutDuration = 0.25f;
    [SerializeField] private int _sproutVibrato = 10;
    [SerializeField] private float _sproutElasticity = 1f;
    
    [Header("Harvest Scale Punch")]
    [SerializeField] private Vector3 _harvestPunch = default;
    [SerializeField] private float _harvestDuration = 0.25f;
    [SerializeField] private int _harvestVibrato = 10;
    [SerializeField] private float _harvestElasticity = 1f;
    
    private Carrot _currentCarrot;
    private bool _isSelectable = false;

    private void Start()
    {
        StartCoroutine(CreateCarrot());
    }

    private IEnumerator CreateCarrot()
    {
        _timerImage.fillAmount = 0f;
        _timerImage.rectTransform.DOScale(Vector3.one, 0f);
        _timerImage.gameObject.SetActive(true);
        
        float currentGrowthTime = _carrotGrowthTime;
        float progress = 0f;

        while (currentGrowthTime > 0f)
        {
            progress = (_carrotGrowthTime - currentGrowthTime) / _carrotGrowthTime;
            _timerImage.fillAmount = progress;
            currentGrowthTime -= Time.deltaTime;
            yield return null;
        }

        _timerImage.fillAmount = 1f;
        _timerImage.rectTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            _currentCarrot = _pool.Get();
            _currentCarrot.Setup(this, _carrotSpot, _pool);
            _timerImage.gameObject.SetActive(false);
            _particles.Play();
            _bodyTransform.DOPunchScale(_sproutPunch, _sproutDuration, _sproutVibrato, _sproutElasticity).OnComplete(
                () =>
                {
                    _isSelectable = true;
                });
        });
    }

    public void OnSelected()
    {
        if(_isSelectable)
        {
            _currentCarrot?.SetSelected();
        }
    }

    public void OnDeselected()
    {
        _currentCarrot?.SetDeselected();
    }

    public void OnCarrotHarvested()
    {
        _isSelectable = false;
        _particles.Play();
        _bodyTransform.DOPunchScale(_harvestPunch, _harvestDuration, _harvestVibrato, _harvestElasticity);
    }

    public void OnCarrotReturnedToPool()
    {
        _currentCarrot = null;
        StartCoroutine(CreateCarrot());
    }
}
