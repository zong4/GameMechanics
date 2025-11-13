using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviourPun
{
    public float moveSpeed = 10f;
    
    private void Update()
    {
        if (!photonView.IsMine)
            return;
        
        var moveHorizontal = Input.GetAxis("Horizontal");
        var moveVertical = Input.GetAxis("Vertical");
        var movement = new Vector3(moveHorizontal, moveVertical, 0.0f);
        transform.Translate(movement * (moveSpeed * Time.deltaTime));
    }
}
