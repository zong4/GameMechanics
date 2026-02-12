using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class JoinRoom : MonoBehaviour
    {
        public GameObject roomEntryPrefab;
        public Transform roomListContent;
        public TMP_InputField roomNameInputField;
        private readonly Dictionary<string, GameObject> _roomEntries = new();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                PhotonNetwork.LoadLevel("Lobby");

            if (!NetworkManager.Instance().IsRoomListUpdated() && _roomEntries.Count > 0)
                return;

            // Delete old entries
            foreach (var entry in _roomEntries.Values)
                Destroy(entry);
            _roomEntries.Clear();

            foreach (var roomInfo in NetworkManager.Instance().RoomList())
            {
                if (roomInfo.RemovedFromList)
                    continue;

                // Only show open rooms with available slots
                if (!roomInfo.IsOpen || roomInfo.PlayerCount >= roomInfo.MaxPlayers)
                    continue;

                var entryObj = Instantiate(roomEntryPrefab, roomListContent);
                _roomEntries[roomInfo.Name] = entryObj;

                // Set room info text
                var text = entryObj.GetComponentInChildren<TMP_Text>();
                text.text = $"Room: {roomInfo.Name}   " +
                            $"Players: {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";

                // Set button click listener
                var button = entryObj.GetComponent<Button>();
                var capturedName = roomInfo.Name;
                button.onClick.AddListener(() => { roomNameInputField.text = capturedName; });
            }
        }

        public void OnJoinButtonClicked()
        {
            if (string.IsNullOrEmpty(roomNameInputField.text))
                return;

            PhotonNetwork.JoinRoom(roomNameInputField.text);
        }
    }
}