using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    // Shooting mapping
    private Dictionary<IonTube, Dictionary<KeyCode, float>> _shootMap;

    // Cooldown tracking
    private KeyCode _keycode = KeyCode.None;
    private Dictionary<KeyCode, float> _cooldownMap;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        _shootMap = new Dictionary<IonTube, Dictionary<KeyCode, float>>();
        _cooldownMap = new Dictionary<KeyCode, float>();
    }

    private void Update()
    {
        foreach (var (ionTube, scaleMap) in _shootMap)
        {
            foreach (var (key, scale) in scaleMap)
            {
                if (Input.GetKeyDown(key))
                {
                    _cooldownMap.TryAdd(key, 0);
                    if (Time.time - _cooldownMap[key] < ionTube.cooldown)
                        continue;

                    _keycode = key;
                    var force = ionTube.Shoot(gameObject, scale);
                    _rigidbody2D.AddForce(-force);
                }
            }
        }

        if (_keycode != KeyCode.None)
        {
            _cooldownMap[_keycode] = Time.time;
            _keycode = KeyCode.None;
        }
    }

    public void ResetScaleMap()
    {
        _shootMap.Clear();
        _cooldownMap.Clear();
        foreach (Transform child in transform)
        {
            GatherScales(child, new Dictionary<KeyCode, float>());
        }

        foreach (var (ionTube, scaleMap) in _shootMap)
        {
            foreach (var (key, scale) in scaleMap)
            {
                Debug.Log($"IonTube {ionTube.name} has key {key} at scale {scale}.");
            }
        }
    }

    private void GatherScales(Transform current, Dictionary<KeyCode, float> scaleMap)
    {
        // Copy current scale map
        var newScaleMap = new Dictionary<KeyCode, float>(scaleMap);

        // Record self
        {
            var ionTube = current.GetComponent<IonTube>();
            newScaleMap.TryAdd(ionTube.shootKey, 0);
            newScaleMap[ionTube.shootKey] += 1;

            // Close useless components
            current.GetComponent<Draggable>().enabled = false;
            current.GetComponent<KeyBinding>().enabled = false;
        }

        var topSlot = current.GetChild(0);
        var childCount = topSlot.childCount;
        if (childCount == 0)
        {
            _shootMap[current.GetComponent<IonTube>()] = newScaleMap;
        }
        else
        {
            // Average scales among children
            {
                var tempScaleMap = new Dictionary<KeyCode, float>(newScaleMap);
                foreach (var (key, scale) in tempScaleMap)
                {
                    newScaleMap[key] = scale / childCount;
                }
            }

            for (var i = 0; i < childCount; i++)
            {
                GatherScales(topSlot.GetChild(i), newScaleMap);
            }
        }

        // Backtrack
        // scaleMap[ionTube.shootKey] -= 1;
    }
}