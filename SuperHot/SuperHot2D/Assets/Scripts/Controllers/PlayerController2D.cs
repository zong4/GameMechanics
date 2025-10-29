using UnityEngine;

namespace Controllers
{
    public class PlayerController2D : MonoBehaviour
    {
        private enum PlayerState
        {
            Idle,
            Walking,
            Shooting,
        }

        private PlayerState _playerState = PlayerState.Idle;

        // Walking
        private float _moveSpeed = 5f;

        private void Start()
        {
            TimeScaleManager.ResetTimeScale();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                ReplayManager.Instance.StartReplay();
            }

            if (ReplayManager.IsReplaying())
                return;

            OnMove();
            OnAttack();

            OnPlayerStateChange();
        }

        private void OnMove()
        {
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

            if (movement.magnitude > 0.1f)
            {
                _playerState = PlayerState.Walking;
                transform.Translate(movement * (_moveSpeed * Time.deltaTime));
            }
            else
            {
                _playerState = PlayerState.Idle;
            }
        }

        private void OnAttack()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                _playerState = PlayerState.Shooting;
                Debug.Log("Player is shooting.");
            }
        }

        private void OnPlayerStateChange()
        {
            // Handle player state changes
            if (_playerState == PlayerState.Idle)
            {
                TimeScaleManager.SetTimeScale(0.1f);
            }
            else
            {
                TimeScaleManager.ResetTimeScale();
            }
        }
    }
}