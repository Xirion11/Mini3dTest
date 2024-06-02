using UnityEngine;

public class PointerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _dirtPileLayerMask = default;
    
    private Camera _camera;
    private DirtPile _selectedDirtPile;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _dirtPileLayerMask))
            {
                _selectedDirtPile = hit.collider.GetComponent<DirtPile>();
                _selectedDirtPile.OnSelected();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_selectedDirtPile != null)
            {
                _selectedDirtPile.OnDeselected();
            }
        }
    }
}
