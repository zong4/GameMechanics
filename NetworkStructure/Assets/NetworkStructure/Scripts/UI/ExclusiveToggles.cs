using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ExclusiveToggles : MonoBehaviour
    {
        public List<Toggle> toggles;

        private void Awake()
        {
            // Set the first toggle to be on by default
            foreach (var toggle in toggles)
                toggle.isOn = false;
            toggles[0].isOn = true;

            // Add listeners to ensure only one toggle is on at a time
            for (var i = 0; i < toggles.Count; i++)
            {
                var index = i;
                toggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        for (var j = 0; j < toggles.Count; j++)
                        {
                            if (j != index)
                                toggles[j].isOn = false;
                        }
                    }
                    else
                    {
                        // Prevent all toggles from being off; at least one should be on
                        if (!toggles.Exists(t => t.isOn))
                        {
                            toggles[index].isOn = true;
                        }
                    }
                });
            }
        }
    }
}