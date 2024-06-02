using UnityEngine;

public class WorldCanvasRotation : MonoBehaviour
{
    [SerializeField] private Transform _transform = default;
    [SerializeField] private Canvas _canvas = default;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _canvas.worldCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 direction = transform.position - _mainCamera.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        _transform.rotation = rotation;
    }
}
