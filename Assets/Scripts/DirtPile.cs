using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Systems;
using UnityEngine;
using UnityEngine.UI;

public class DirtPile : MonoBehaviour
{
    [SerializeField] private Transform _carrotSpot = default;
    [SerializeField] private CarrotPool _pool = default;
    [SerializeField] private float _carrotGrowthTime = 2f;
    [SerializeField] private Image _timerImage = default;
    
    private Carrot _currentCarrot;

    private void Start()
    {
        StartCoroutine(CreateCarrot());
    }

    private IEnumerator CreateCarrot()
    {
        _timerImage.fillAmount = 0f;
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
            _currentCarrot = _pool.Get(_carrotSpot);
            _timerImage.gameObject.SetActive(false);
        });
    }

    public void OnSelected()
    {
        _currentCarrot?.SetSelected();
    }

    public void OnDeselected()
    {
        _currentCarrot?.SetDeselected();
        DOVirtual.DelayedCall(2f, ReturnCarrotToPool);
    }

    private void ReturnCarrotToPool()
    {
        _pool.ReturnToPool(_currentCarrot);
        _currentCarrot = null;
    }
}
