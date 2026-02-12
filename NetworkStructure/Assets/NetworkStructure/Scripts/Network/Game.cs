using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class Game : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                PhotonNetwork.LeaveRoom();
        }
    }
}