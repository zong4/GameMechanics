using UnityEngine;

namespace Controllers.Players
{
    public class PlayerController3D : Controller
    {
        private Camera _mainCamera;

        protected override void Start()
        {
            // base.Start();
            {
                // Register this controller with the ControllerManager
                ControllerManager.Instance.RegisterPlayerController(this);

                // Shoot
                {
                    shootPoint = transform.Find("ShootPoint");

                    if (!bullet || !shootPoint)
                    {
                        Debug.LogWarning("Bullet prefab or shoot point not set for controller: " + name);
                    }
                }
            }

            TimeScaleManager.ResetTimeScale();

            _mainCamera = Camera.main;
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (ReplayManager.IsReplaying())
                return;

            controllerState = ControllerState.Idle;

            OnMove();
            OnTurnAround();
            OnAttack();

            OnPlayerStateChange();
        }

        private void OnDestroy()
        {
            Application.Quit();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("GameController"))
            {
                Destroy(gameObject);
            }
        }

        protected override void OnMove()
        {
            var movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            if (movement.magnitude > 0.1f)
            {
                transform.Translate(movement *
                                    (Controller.WalkSpeed * Controller.WalkSpeedMultiplier * Time.deltaTime));

                controllerState = ControllerState.Walk;
            }
        }

        private void OnTurnAround()
        {
            if (_mainCamera)
            {
                var direction = Input.mousePosition - _mainCamera.WorldToScreenPoint(transform.position);
                var angle = Mathf.Atan2(direction.y, -direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, angle - 90, 0));

                // controllerState = ControllerState.TurnAround;
            }
        }

        private void OnAttack()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Attack();
            }
        }

        private void OnPlayerStateChange()
        {
            if (controllerState == ControllerState.Idle)
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