using System.Collections.Generic;
using UnityEngine;

public struct Action
{
    public float Time;
    public int ID;
    public Vector3 Position;
    public Vector3 Direction;
    public bool Fire;
}

namespace Controllers
{
    public class ControllerManager : MonoBehaviour
    {
        private static ControllerManager _instance;

        // Data
        private static int _nextId;
        public List<Controller> controllers = new();
        public List<List<Action>> Actions = new();

        // Player
        public GameObject player;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            Instantiate(player);
        }

        private void LateUpdate()
        {
            if (ReplayManager.IsReplaying())
                return;

            // Process actions
            var actions = new List<Action>();
            foreach (var controller in controllers)
            {
                actions.Add(new Action
                {
                    Time = Time.time,
                    ID = controller.ID,
                    Position = controller.transform.position,
                    Direction = controller.transform.forward,
                    Fire = Input.GetButton("Fire1") // Example input check
                });
            }

            Actions.Add(actions);
        }

        public static ControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("ControllerManager").AddComponent<ControllerManager>();
                }

                return _instance;
            }
        }

        public void RegisterController(Controller controller)
        {
            if (controller == null) return;

            controller.ID = _nextId++;
            controllers.Add(controller);
            Debug.Log($"Controller {controller.ID} registered.");
        }

        public Controller GetController(int id)
        {
            return controllers[id];
        }

        public List<Action> GetActions(int time)
        {
            return Actions[time];
        }
    }
}