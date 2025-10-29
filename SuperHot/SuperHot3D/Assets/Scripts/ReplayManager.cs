using System.Collections;
using Controllers;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    private static ReplayManager _instance;

    private static bool _isReplaying;

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

    public static ReplayManager Instance
    {
        get
        {
            if (!_instance)
                _instance = new GameObject("ReplayManager").AddComponent<ReplayManager>();
            return _instance;
        }
    }

    public static bool IsReplaying()
    {
        return _isReplaying;
    }

    public void StartReplay()
    {
        _isReplaying = true;
        TimeScaleManager.ResetTimeScale();

        StartCoroutine(ReplayCoroutine());
    }

    private static void StopReplay()
    {
        _isReplaying = false;

        Application.Quit();
    }

    private static IEnumerator ReplayCoroutine()
    {
        for (var i = 0; i < ControllerManager.Instance.playerControllerMessages.Count && _isReplaying; i++)
        {
            var cnt = 0;
            var deltaTime = 0.0f;
            while (ControllerManager.Instance.playerControllerMessages[i].state == Controller.ControllerState.Idle &&
                   i < ControllerManager.Instance.playerControllerMessages.Count - 1 && cnt < 10) //todo
            {
                cnt++;

                i++;
                deltaTime += ControllerManager.Instance.playerControllerMessages[i].deltaTime;
            }

            // Player
            {
                var playerMessage = ControllerManager.Instance.playerControllerMessages[i];

                var playerController = ControllerManager.Instance.playerController;
                if (playerController)
                {
                    playerController.transform.position = playerMessage.position;
                    playerController.transform.forward = playerMessage.direction;

                    if (playerMessage.state == Controller.ControllerState.Attack)
                    {
                        playerController.Attack();
                    }
                }
            }

            // All other controllers
            {
                var allControllerMessages = ControllerManager.Instance.allControllersMessages[i];

                foreach (var controllerMessage in allControllerMessages)
                {
                    var controller = ControllerManager.Instance.GetController(controllerMessage.id);
                    if (controller)
                    {
                        controller.transform.position = controllerMessage.position;
                        controller.transform.forward = controllerMessage.direction;

                        if (controllerMessage.state == Controller.ControllerState.Attack)
                        {
                            controller.Attack();
                        }
                    }
                }
            }

            yield return new WaitForSeconds(deltaTime);
        }

        StopReplay();
    }
}