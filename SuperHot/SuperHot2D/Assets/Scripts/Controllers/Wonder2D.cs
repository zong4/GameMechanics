using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class Wonder2D : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _wanderRadius = 5f;
        private float _moveSpeed = 1f;

        private void Awake()
        {
            _startPosition = transform.position;
            _targetPosition = _startPosition + Random.insideUnitSphere * _wanderRadius;
            _targetPosition.z = _startPosition.z; // Keep the z position constant
        }

        private void Update()
        {
            if (ReplayManager.IsReplaying())
                return;

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                _startPosition = transform.position;
                _targetPosition = _startPosition + Random.insideUnitSphere * _wanderRadius;
                _targetPosition.z = _startPosition.z; // Keep the z position constant
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
        }
    }
}