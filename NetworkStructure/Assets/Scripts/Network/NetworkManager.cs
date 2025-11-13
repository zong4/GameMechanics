using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master Server");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined Lobby");
        }

        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"Joined Room: {room.Name}");
            Debug.Log($"Players: {room.PlayerCount}/{room.MaxPlayers}");
            Debug.Log($"IsOpen: {room.IsOpen}, IsVisible: {room.IsVisible}");

            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        }
    }
}