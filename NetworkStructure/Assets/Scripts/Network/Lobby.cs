using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class Lobby : MonoBehaviour
    {
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