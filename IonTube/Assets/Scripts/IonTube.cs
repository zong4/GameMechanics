using UnityEngine;

public class IonTube : MonoBehaviour
{
    // Slots
    private Slot _topSlot;
    private Slot _bottomSlot;

    // OnSelect
    // public float rotationSpeed = 30f;

    // Shoot
    public float shootForce = 150f;
    public KeyCode shootKey = KeyCode.Space;
    public GameObject ionPrefab;
    private Transform _shootPoint;

    // Cooldown
    public float cooldown = 0.5f;

    private void Start()
    {
        _topSlot = transform.GetChild(0).GetComponent<Slot>();
        _bottomSlot = transform.GetChild(1).GetComponent<Slot>();
        _shootPoint = transform.GetChild(2);
    }

    public Vector3 Shoot(GameObject player, float scale)
    {
        // Instantiate ion
        var ion = Instantiate(ionPrefab, _shootPoint.position, _shootPoint.rotation);

        // Ignore collision between player and ion
        // var playerCollider = player.GetComponent<Collider2D>();
        // var ionCollider = ion.GetComponent<Collider2D>();
        // Physics2D.IgnoreCollision(playerCollider, ionCollider);

        // Apply force
        var force = transform.up * (scale * shootForce);
        ion.GetComponent<Rigidbody2D>().AddForce(force);
        // player.GetComponent<Rigidbody2D>().AddForce(-force); // Handled in Player.cs
        Debug.Log("Ion tube " + gameObject.name + " shooting with force " + force);
        return force;
    }

    public void OnSelect()
    {
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
        // }
    }

    public void PreDragging()
    {
        _bottomSlot.isOccupied = false;
        transform.parent = null;
    }

    public bool PostDragging()
    {
        if (_bottomSlot.targetTransform && !_topSlot.targetTransform)
        {
            // Bottom slot only can be connected once
            _bottomSlot.isOccupied = true;

            // After this, the ion tube will be a child of the slot's target transform
            // Get rigidbody from parent
            // Ignore collision with parent
            transform.parent = _bottomSlot.targetTransform;
            Debug.Log("Ion tube connected to " + _bottomSlot.targetTransform.name);
            return true;
        }

        return false;
    }

    public void OnDragging()
    {
        AdjustDirection(!_bottomSlot.targetTransform
            ? Vector3.zero // Player position
            : _bottomSlot.targetTransform.position);
    }

    private void AdjustDirection(Vector3 targetPos)
    {
        var directionToCenter = (targetPos - transform.position).normalized;
        var angle = Mathf.Atan2(directionToCenter.x, directionToCenter.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle + 180); // Back to center
    }
}