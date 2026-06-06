using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool isOccupied = false;
    public Transform targetTransform;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetTransform = other.transform;
            Debug.Log("Slot connected to " + other.name);
        }
        else if (other.CompareTag("Slot"))
        {
            if (!other.GetComponent<Slot>().isOccupied)
            {
                targetTransform = other.transform;
                Debug.Log("Slot connected to " + other.name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Slot"))
        {
            if (targetTransform == other.transform)
            {
                targetTransform = null;
                Debug.Log("Slot disconnected from " + other.name);
            }
        }
    }
}