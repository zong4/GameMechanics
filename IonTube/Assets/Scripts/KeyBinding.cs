using System;
using TMPro;
using UnityEngine;

public class KeyBinding : MonoBehaviour
{
    private bool _waitingForKey = false;
    private bool _inputReady = false;

    // UI
    private GameObject _keyBindingUI;
    private TMP_Text _keyUIText;

    // Double click
    public float doubleClickTime = 0.3f;
    private float _lastClickTime;

    private void Start()
    {
        var canvas = GameObject.Find("Canvas").transform;
        _keyBindingUI =
            canvas.GetChild(canvas.childCount - 1).gameObject; // Assuming the last child is the key binding UI
        _keyUIText = _keyBindingUI.GetComponentInChildren<TMP_Text>(true);
    }

    private void Update()
    {
        if (_waitingForKey && _inputReady)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    OnKeySelected(key);
                }
            }
        }

        // Wait for the next frame to avoid immediate key detection
        _inputReady = true;
    }

    private void OnMouseDown()
    {
        var timeSinceLastClick = Time.time - _lastClickTime;
        if (timeSinceLastClick <= doubleClickTime)
            ShowKeyUI();

        _lastClickTime = Time.time;
    }

    private void ShowKeyUI()
    {
        _keyUIText.text = GetComponent<IonTube>().shootKey.ToString();
        _keyBindingUI.SetActive(true);

        _waitingForKey = true;
        _inputReady = false;
    }

    private void OnKeySelected(KeyCode key)
    {
        _waitingForKey = false;

        GetComponent<IonTube>().shootKey = key;
        _keyUIText.text = GetComponent<IonTube>().shootKey.ToString();
        _keyBindingUI.SetActive(false);
    }
}