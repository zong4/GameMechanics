using UnityEngine;

namespace Controllers.Enemies
{
    public class WalkBackAndForth : Controller
    {
        public Vector3 startPosition;
        public Vector3 targetPosition;

        protected override void Start()
        {
            base.Start();

            // Initialize positions
            transform.position = startPosition;
        }

        protected void Update()
        {
            if (ReplayManager.IsReplaying())
                return;

            controllerState = ControllerState.Idle;
        }

        protected override void OnMove()
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                targetPosition = startPosition;
                startPosition = transform.position;
            }

            var direction = (targetPosition - transform.position).normalized;
            transform.position += direction * (Controller.WalkSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);

            controllerState = ControllerState.Walk;
        }
    }
}