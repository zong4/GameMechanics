using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Player
{
    public class Spawn : MonoBehaviourPun
    {
        public Vector2 boundaryX = new Vector2(-6f, 6f);
        public Vector2 boundaryY = new Vector2(-4f, 4f);
        private const string PlayerPosKey = "Position";

        private void Awake()
        {
            if (!photonView.IsMine)
                return;

            if (photonView.Owner.CustomProperties.TryGetValue(PlayerPosKey, out var posObj))
            {
                // Retrieve and apply the position from custom properties
                var arr = (float[])posObj;
                transform.position = new Vector3(arr[0], arr[1], arr[2]);
            }
            else
            {
                // Default random position if none is set
                var randomPos = new Vector3(
                    Random.Range(boundaryX.x, boundaryX.y),
                    Random.Range(boundaryY.x, boundaryY.y),
                    0f
                );
                transform.position = randomPos;
                UploadPosition();
            }
        }

        private void UploadPosition()
        {
            // only own player uploads
            if (!photonView.IsMine) return;

            var pos = transform.position;
            var props = new Hashtable
            {
                { PlayerPosKey, new[] { pos.x, pos.y, pos.z } }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }
}