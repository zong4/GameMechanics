using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Network
{
    public class HostRoom : MonoBehaviour
    {
        public TMP_InputField roomNameInputField;
        public List<Toggle> maxPlayersToggles;
        public Toggle visibleToggle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                PhotonNetwork.LoadLevel("Lobby");
        }

        public void OnHostButtonClicked()
        {
            if (string.IsNullOrEmpty(roomNameInputField.text))
                return;

            var maxPlayers = 0;
            foreach (var toggle in maxPlayersToggles)
            {
                if (toggle.isOn)
                {
                    maxPlayers = int.Parse(toggle.name);
                    break;
                }
            }

            var isVisible = visibleToggle.isOn;

            var roomOptions = new Photon.Realtime.RoomOptions
            {
                MaxPlayers = maxPlayers,
                IsVisible = isVisible,
                IsOpen = true
            };
            PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        }
    }
}