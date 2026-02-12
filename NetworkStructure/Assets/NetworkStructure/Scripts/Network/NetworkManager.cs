using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        private static NetworkManager _instance;
        private bool _isConnected = false;
        private bool _isRoomListUpdated = false;
        private List<RoomInfo> _roomList;

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Singleton pattern
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Connect to Photon server
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
            _isConnected = true;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Create Room Failed: {message}");
        }

        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"Joined Room: {room.Name}");
            Debug.Log($"Players: {room.PlayerCount}/{room.MaxPlayers}");
            Debug.Log($"IsOpen: {room.IsOpen}, IsVisible: {room.IsVisible}");

            SceneManager.sceneLoaded += OnRoomSceneLoaded;
            SceneManager.sceneLoaded += OnGameSceneLoaded;
            PhotonNetwork.LoadLevel("Room");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Join Room Failed: {message}");
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left Room");

            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel("Menu");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _roomList = roomList;
            _isRoomListUpdated = true;
        }

        public static NetworkManager Instance()
        {
            return _instance;
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        public bool IsRoomListUpdated()
        {
            return _isRoomListUpdated;
        }

        public List<RoomInfo> RoomList()
        {
            _isRoomListUpdated = false;
            return _roomList;
        }

        private static void OnRoomSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Room")
            {
                PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
                SceneManager.sceneLoaded -= OnRoomSceneLoaded;
            }
        }

        private static void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game")
            {
                PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
                SceneManager.sceneLoaded -= OnGameSceneLoaded;
            }
        }
    }
}