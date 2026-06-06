using UnityEngine;

public class Draggable : MonoBehaviour
{
    // Dragging
    private bool _isDragging = false;
    private Camera _mainCamera;
    private Vector3 _offset;
    private Vector3 _startPosition;

    // Selection
    private bool _isSelected = false;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    public Color selectedColor = Color.gray;

    // Ion tube
    private IonTube _ionTube;

    private void Start()
    {
        // Dragging
        _mainCamera = Camera.main;
        _startPosition = transform.position;

        // Selection
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;

        // Ion tube
        _ionTube = GetComponent<IonTube>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            PreDragging();
        if (Input.GetMouseButtonUp(0) && _isDragging)
            PostDragging();
        if (_isDragging)
            OnDragging();

        if (_isSelected)
            OnSelect();
    }

    private void PreDragging()
    {
        // Use raycast to detect mouse down on this object
        var hit = Physics2D.OverlapPoint(GetMouseWorldPos(), 1 << gameObject.layer);
        if (hit && hit.gameObject == gameObject)
        {
            // Selection
            if (!_isSelected)
            {
                _isSelected = true;
                _spriteRenderer.color = selectedColor;
                return;
            }

            // Dragging
            _isDragging = true;
            _offset = transform.position - GetMouseWorldPos();

            // Ion tube
            _ionTube.PreDragging();
        }
        else
        {
            _isSelected = false;
            _spriteRenderer.color = _originalColor;
        }
    }

    private void OnDragging()
    {
        // Dragging
        transform.position = GetMouseWorldPos() + _offset;

        // Ion tube
        _ionTube.OnDragging();
    }

    private void PostDragging()
    {
        // Dragging
        _isDragging = false;

        // Ion tube
        if (_ionTube.PostDragging())
            return;

        // Dragging
        transform.position = _startPosition;
    }

    private void OnSelect()
    {
        // Ion tube
        _ionTube.OnSelect();
    }

    // private void OnMouseDown()
    // {
    //     // Basic
    //     _isDragging = true;
    //     _offset = transform.position - GetMouseWorldPos();
    //
    //     // Ion tube specific
    //     _ionTube.PreDragging();
    // }

    // private void OnMouseUp()
    // {
    //     // Basic
    //     _isDragging = false;
    //
    //     // Ion tube specific
    //     if (_ionTube.PostDragging())
    //         return;
    //
    //     // Basic
    //     transform.position = _startPosition;
    // }

    private Vector3 GetMouseWorldPos()
    {
        var mousePos = Input.mousePosition; // mousePos.z is always 0
        var mouseWorldPos = _mainCamera.ScreenToWorldPoint(mousePos); // mouseWorldPos.z is camera's z position
        return mouseWorldPos;
    }
}