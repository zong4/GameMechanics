using System;
using UnityEngine;

namespace Controllers.Enemies
{
    public class Follow : Controller
    {
        public Transform target;

        protected override void Start()
        {
            base.Start();

            if (!target)
                throw new ArgumentNullException(nameof(target), "Target must be assigned in the inspector.");
        }

        protected void Update()
        {
            if (ReplayManager.IsReplaying())
                return;

            controllerState = ControllerState.Idle;

            OnMove();
        }

        protected override void OnMove()
        {
            if (target)
            {
                var direction = (target.position - transform.position).normalized;
                transform.position += direction * (Controller.WalkSpeed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(direction);

                controllerState = ControllerState.Walk;
            }
        }
    }
}