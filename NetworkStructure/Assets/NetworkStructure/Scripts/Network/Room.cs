using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class Room : MonoBehaviourPunCallbacks
    {
        public Button readyButton;
        private bool _isReady = false;
        private const string PlayerReadyKey = "Ready";

        private void Awake()
        {
            UpdateReadyButtonText();
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                PhotonNetwork.LeaveRoom();
        }

        private void OnReadyButtonClicked()
        {
            _isReady = !_isReady;
            UpdateReadyButtonText();

            var props = new Hashtable
            {
                { PlayerReadyKey, _isReady }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        private void UpdateReadyButtonText()
        {
            readyButton.GetComponentInChildren<TMP_Text>().text =
                _isReady ? "Unready" : "Ready";
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player player, Hashtable changedProps)
        {
            if (!changedProps.ContainsKey(PlayerReadyKey))
                return;

            if (PhotonNetwork.IsMasterClient)
                TryStartGame();
        }

        private static void TryStartGame()
        {
            // check if all players are ready
            var readyPlayers = 0;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!player.CustomProperties.TryGetValue(PlayerReadyKey, out var readyObj))
                    return;
                if (!(bool)readyObj)
                    return;

                readyPlayers++;
            }

            if (readyPlayers == PhotonNetwork.CurrentRoom.MaxPlayers)
                PhotonNetwork.LoadLevel("Game");
        }
    }
}