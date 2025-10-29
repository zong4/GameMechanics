using System.Collections.Generic;
using Controllers.Players;
using UnityEngine;

namespace Controllers
{
    public struct PlayerControllerMessage
    {
        public float deltaTime;
        public Vector3 position;
        public Vector3 direction;
        public Controller.ControllerState state;
    }

    public struct ControllerMessage
    {
        public int id;
        public Vector3 position;
        public Vector3 direction;
        public Controller.ControllerState state;
    }

    public class ControllerManager : MonoBehaviour
    {
        private static ControllerManager _instance;

        // Player controller
        public Controller playerController;
        public readonly List<PlayerControllerMessage> playerControllerMessages = new();

        // Other controllers
        private static int _nextId;
        public List<Controller> controllers = new();
        public readonly List<List<ControllerMessage>> allControllersMessages = new();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("Duplicate ControllerManager destroyed!", gameObject);
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (ReplayManager.IsReplaying())
                return;

            // Add player controller message
            if (playerController)
            {
                playerControllerMessages.Add(new PlayerControllerMessage
                {
                    deltaTime = Time.deltaTime,
                    position = playerController.transform.position,
                    direction = playerController.transform.forward,
                    state = playerController.controllerState,
                });
            }

            // Add all other controllers messages
            var allControllerMessages = new List<ControllerMessage>();
            foreach (var controller in controllers)
            {
                allControllerMessages.Add(new ControllerMessage
                {
                    id = controller.ID,
                    position = controller.transform.position,
                    direction = controller.transform.forward,
                    state = controller.controllerState,
                });
            }

            allControllersMessages.Add(allControllerMessages);
        }

        public static ControllerManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = new GameObject("ControllerManager").AddComponent<ControllerManager>();
                return _instance;
            }
        }

        public Controller GetController(int id)
        {
            return controllers[id];
        }

        public void RegisterPlayerController(PlayerController3D controller)
        {
            if (controller == null) return;

            controller.ID = -1;
            playerController = controller;
            Debug.Log($"Player controller {controller.ID} registered.");
        }

        public void RegisterController(Controller controller)
        {
            if (controller == null) return;

            controller.ID = _nextId++;
            controllers.Add(controller);
            Debug.Log($"Controller {controller.ID} registered.");
        }

        public void CheckStartReplay()
        {
            if (ReplayManager.IsReplaying())
                return;

            foreach (var controller in controllers)
            {
                if (controller.gameObject.activeSelf)
                    return;
            }

            foreach (var controller in controllers)
            {
                controller.gameObject.SetActive(true);
            }

            ReplayManager.Instance.StartReplay();
        }
    }
}