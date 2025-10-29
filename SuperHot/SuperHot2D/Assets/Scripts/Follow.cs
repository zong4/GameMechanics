using System;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    private float _smoothSpeed = 0.125f;
    private Vector3 _offset;

    private void Start()
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target), "Target must be assigned in the inspector.");
        }

        // Calculate the initial offset
        _offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position
        var desiredPosition = target.position + _offset;

        // Smoothly interpolate to the desired position
        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);

        // Update the position of this GameObject
        transform.position = smoothedPosition;
    }
}