using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class Move : MonoBehaviourPun
    {
        public float moveSpeed = 10f;

        private void Update()
        {
            if (!photonView.IsMine)
                return;

            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
            transform.Translate(movement * (moveSpeed * Time.deltaTime));
        }
    }
}