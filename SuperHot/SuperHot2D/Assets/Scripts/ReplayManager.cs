using System.Collections;
using Controllers;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    // Singleton instance
    private static ReplayManager _instance;

    // Data
    private static bool _isReplaying;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static ReplayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("ReplayManager").AddComponent<ReplayManager>();
            }

            return _instance;
        }
    }

    public void StartReplay()
    {
        _isReplaying = true;
        TimeScaleManager.ResetTimeScale();

        StartCoroutine(ReplayCoroutine());
    }

    private static IEnumerator ReplayCoroutine()
    {
        for (var i = 0; i < ControllerManager.Instance.Actions.Count && _isReplaying; i++)
        {
            var time = ControllerManager.Instance.Actions[i];
            foreach (var action in time)
            {
                var controller = ControllerManager.Instance.GetController(action.ID);

                if (controller != null)
                {
                    controller.transform.position = action.Position;
                    controller.transform.forward = action.Direction;

                    if (action.Fire)
                    {
                        // controller.Fire();
                    }
                }
            }

            if (i < ControllerManager.Instance.Actions.Count - 1)
            {
                // Wait for the next action time
                yield return new WaitForSeconds(ControllerManager.Instance.Actions[i + 1][0].Time - time[0].Time);
            }
            else
            {
                // Last action, wait for a short duration before stopping
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void StopReplay()
    {
        _isReplaying = false;
        // Additional logic to stop replaying
    }

    public static bool IsReplaying()
    {
        return _isReplaying;
    }
}