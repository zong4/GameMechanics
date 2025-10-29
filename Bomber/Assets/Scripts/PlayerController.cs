using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Move
    public float moveSpeed = 3f;

    // Foot
    private Foot _foot;
    public List<string> validFootTags = new List<string> { "Ground", "Obstacle" };

    // Obstacle
    public GameObject obstaclePrefab;
    public int maxObstacles = 2;
    public List<Obstacle> obstacles = new List<Obstacle>();

    // Explosion
    private BombCharacter _bombCharacter;

    // todo
    public Bridge bridge;

    private void Start()
    {
        // Foot
        _foot = transform.GetComponentInChildren<Foot>();
        if (!_foot)
        {
            Debug.LogError("Foot component is not found in the children of PlayerController.");
        }

        // Obstacle
        if (!obstaclePrefab)
        {
            Debug.LogError("Bomb Prefab is not assigned in the inspector.");
        }

        // Explosion
        _bombCharacter = GetComponent<BombCharacter>();
        if (!_bombCharacter)
        {
            Debug.LogError("Bomb component is not assigned in the inspector.");
        }
    }

    private void Update()
    {
        // todo
        if (Input.GetKeyDown(KeyCode.C))
        {
            while (obstacles.Count > 0)
            {
                var oldObstacle = obstacles[^1];
                obstacles.Remove(oldObstacle);
                Destroy(oldObstacle.gameObject);
            }
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            if (obstacles.Count < maxObstacles)
            {
                obstacles.Add(Instantiate(obstaclePrefab, new Vector3(transform.position.x,
                        transform.position.y + 1.1f, transform.position.z),
                    Quaternion.identity).GetComponent<Obstacle>());
            }
        }

        if (validFootTags.Contains(_foot.touchTag))
        {
            bridge.Open();
        }

        Move();

        PutObstacle();
        Explode();
    }

    private void Move()
    {
        var moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        transform.Translate(moveInput * (Time.deltaTime * moveSpeed));
    }

    private void PutObstacle()
    {
        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && validFootTags.Contains(_foot.touchTag) &&
            obstaclePrefab)
        {
            if (obstacles.Count >= maxObstacles)
                return;

            // if (obstacles.Count >= maxObstacles)
            // {
            //     var oldObstacle = obstacles[0];
            //     obstacles.Remove(oldObstacle);
            //     Destroy(oldObstacle.gameObject);
            // }

            var oldPosition = transform.position;
            transform.position = new Vector3(transform.position.x,
                transform.position.y + obstaclePrefab.transform.localScale.y, transform.position.z);
            obstacles.Add(Instantiate(obstaclePrefab, oldPosition, Quaternion.identity).GetComponent<Obstacle>());
        }
    }

    private void Explode()
    {
        if ((Input.GetKeyDown(KeyCode.K) || Input.GetMouseButtonDown(1)))
        {
            if (_bombCharacter)
            {
                _bombCharacter.Explode();
            }
        }
    }
}