using UnityEngine;

namespace Controllers
{
    public class Controller : MonoBehaviour
    {
        public int ID { get; set; }

        private void Start()
        {
            // Register this controller with the ControllerManager
            ControllerManager.Instance.RegisterController(this);
        }
    }
}