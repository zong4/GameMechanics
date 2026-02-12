using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class Menu : MonoBehaviour
    {
        private void Awake()
        {
            if (NetworkManager.Instance())
                Destroy(NetworkManager.Instance().gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        public void OnOnlineButtonClicked()
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}