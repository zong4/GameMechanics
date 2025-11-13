using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public List<RoomInfo> roomInfos;

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

            SceneManager.sceneLoaded += OnRoomSceneLoaded;
            PhotonNetwork.LoadLevel("Room");
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left Room");
            PhotonNetwork.LoadLevel("Lobby");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            roomInfos = roomList;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Create Room Failed: {message}");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join Room Failed: {message}");
        }

        private static void OnRoomSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Room")
            {
                PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
                SceneManager.sceneLoaded -= OnRoomSceneLoaded;
            }
        }
    }
}