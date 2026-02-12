using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class Lobby : MonoBehaviour
    {
        public List<GameObject> waitingConnectedObjects;
        private bool _isConnected = false;

        private void Awake()
        {
            // Disable connected objects until connected to Photon server
            foreach (var obj in waitingConnectedObjects)
                obj.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneManager.LoadScene("Menu");

            // Enable connected objects
            if (!_isConnected && NetworkManager.Instance().IsConnected())
            {
                _isConnected = true;
                foreach (var obj in waitingConnectedObjects)
                    obj.SetActive(true);
            }
        }

        public void OnHostButtonClicked()
        {
            SceneManager.LoadScene("HostRoom");
        }

        public void OnJoinButtonClicked()
        {
            SceneManager.LoadScene("JoinRoom");
        }
    }
}