using System.Collections.Generic;
using UnityEngine;

public class BombCharacter : MonoBehaviour
{
    public float explosionForce = 5f;

    private const float ExplosionWidth = 3f - 0.25f;
    private const float ExplosionHeight = 1f - 0.25f;
    private readonly string[] _horizontalValidLayers = new string[] { "Ground", "Obstacle" };
    private readonly string[] _verticalValidLayers = new string[] { "Ground", "Obstacle" };

    private PlayerController _playerController;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        if (!_playerController)
        {
            Debug.LogError("PlayerController component is not found on the BombCharacter.");
        }
    }

    public void Explode()
    {
        var directionX = 0;
        var horizontalObjects = CollectHorizontal();
        foreach (var horizontal in horizontalObjects)
        {
            if (horizontal.position.x > transform.position.x + 0.1f)
            {
                directionX -= 1;
            }
            else if (horizontal.position.x < transform.position.x - 0.1f)
            {
                directionX += 1;
            }

            // if (information.CompareTag("Obstacle"))
            // {
            //     _playerController.obstacles.Remove(information.GetComponent<Obstacle>());
            //     Destroy(information.gameObject);
            // }
        }

        var directionY = 0;
        var verticalObjects = CollectVertical();
        foreach (var vertical in verticalObjects)
        {
            if (vertical.position.y > transform.position.y + 0.1f)
            {
                directionY -= 1;
            }
            else if (vertical.position.y < transform.position.y - 0.1f)
            {
                directionY += 1;
            }
        }

        if (directionY < 0)
        {
            _playerController.bridge.Close();
        }

        var direction = new Vector3(directionX, directionY, 0f);
        Debug.Log("Explosion direction: " + direction);

        this.GetComponent<Rigidbody2D>()
            .AddForce(direction * explosionForce, ForceMode2D.Impulse);
    }

    private HashSet<Transform> CollectHorizontal()
    {
        var hashSet = new HashSet<Transform>();

        // Horizontal
        {
            var results = new Collider2D[10];
            var size = Physics2D.OverlapBoxNonAlloc(transform.position,
                new Vector2(ExplosionWidth,
                    ExplosionHeight),
                0f,
                results,
                LayerMask.GetMask(_horizontalValidLayers));

            for (var i = 0; i < size; i++)
            {
                hashSet.Add(results[i].GetComponent<Transform>());
            }
        }

        return hashSet;
    }

    private HashSet<Transform> CollectVertical()
    {
        var hashSet = new HashSet<Transform>();

        // Vertical
        {
            var results = new Collider2D[10];
            var size = Physics2D.OverlapBoxNonAlloc(transform.position,
                new Vector2(ExplosionHeight,
                    ExplosionWidth), 0f,
                results,
                LayerMask.GetMask(_verticalValidLayers));

            for (var i = 0; i < size; i++)
            {
                hashSet.Add(results[i].GetComponent<Transform>());
            }
        }

        return hashSet;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(ExplosionWidth, ExplosionHeight));
        Gizmos.DrawWireCube(transform.position, new Vector2(ExplosionHeight, ExplosionWidth));
    }
}