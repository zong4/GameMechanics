using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public void LoadBattleScene()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            DontDestroyOnLoad(player);
            player.GetComponent<Player>().ResetScaleMap();
            player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
    }
}