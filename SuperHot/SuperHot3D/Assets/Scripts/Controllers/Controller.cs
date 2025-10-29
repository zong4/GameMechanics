using UnityEngine;

namespace Controllers
{
    public abstract class Controller : MonoBehaviour
    {
        public int ID { get; set; }

        public enum ControllerState
        {
            Idle,
            Walk,
            TurnAround,
            Attack,
        }

        public ControllerState controllerState = ControllerState.Idle;

        // Walk todo
        protected const float WalkSpeed = 3f;
        protected const float WalkSpeedMultiplier = 1.5f;

        // Shoot
        public GameObject bullet;
        protected Transform shootPoint;

        protected virtual void Start()
        {
            // Register this controller with the ControllerManager
            ControllerManager.Instance.RegisterController(this);

            // Shoot
            {
                shootPoint = transform.Find("ShootPoint");

                if (!bullet || !shootPoint)
                {
                    Debug.LogWarning("Bullet prefab or shoot point not set for controller: " + name);
                }
            }
        }

        protected abstract void OnMove();

        public void Attack()
        {
            if (bullet && shootPoint)
            {
                Instantiate(bullet, shootPoint.position, shootPoint.rotation);

                controllerState = ControllerState.Attack;
            }
        }
    }
}