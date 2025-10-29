using Controllers;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // todo
    public float speed = 10f;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("GameController"))
        {
            Destroy(gameObject);

            other.gameObject.SetActive(false);
            ControllerManager.Instance.CheckStartReplay();
        }
    }
}