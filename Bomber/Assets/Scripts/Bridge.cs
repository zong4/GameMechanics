using UnityEngine;

public class Bridge : MonoBehaviour
{
    private Collider2D _collider2D;

    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
        if (!_collider2D)
        {
            Debug.LogError("Collider2D component is not found on the Bridge.");
        }
    }

    public void Open()
    {
        _collider2D.enabled = true;
    }

    public void Close()
    {
        _collider2D.enabled = false;
    }
}