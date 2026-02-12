using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class Appearance : MonoBehaviourPunCallbacks
    {
        private SpriteRenderer _spriteRenderer;
        private const string PlayerColorKey = "Color";

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (photonView.Owner.CustomProperties.TryGetValue(PlayerColorKey, out var colorObj))
            {
                // Retrieve and apply the color from custom properties
                var rgb = (float[])colorObj;
                _spriteRenderer.color = new Color(rgb[0], rgb[1], rgb[2]);
            }
            else
            {
                // Default color if none is set
                _spriteRenderer.color = Color.white;
            }
        }

        private void Update()
        {
            if (!photonView.IsMine)
                return;

            if (SceneManager.GetActiveScene().name == "Game") return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetColor(Color.red);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SetColor(Color.green);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SetColor(Color.blue);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SetColor(Color.yellow);
        }

        private void SetColor(Color color)
        {
            _spriteRenderer.color = color;
            var rgb = new[] { color.r, color.g, color.b };
            var props = new Hashtable
            {
                { PlayerColorKey, rgb }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerColorKey))
            {
                if (photonView.Owner.Equals(targetPlayer))
                {
                    var rgb = (float[])changedProps[PlayerColorKey];
                    _spriteRenderer.color = new Color(rgb[0], rgb[1], rgb[2]);
                }
            }
        }
    }
}