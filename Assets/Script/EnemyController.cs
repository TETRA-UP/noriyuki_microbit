using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public MazeGenerator mazeGen;
    public float moveSpeed = 3f;
    public float updateInterval = 1f; // �o�H�X�V�Ԋu�i�b�j

    private Vector2Int currentPos;
    private Vector2Int targetPos;
    private Queue<Vector2Int> pathQueue = new Queue<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform;

        if (player != null)
        {
            StartCoroutine(UpdatePath());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pathQueue.Count > 0)
        {
            Vector3 targetWorld = GridToWorld(pathQueue.Peek());

            // �������^�[�Q�b�g�����ɑ����ɕύX
            Vector3 direction = (targetWorld - transform.position).normalized;
            direction.y = 0f; // �����̉e�������O
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // �O�i
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetWorld) < 0.1f)
            {
                currentPos = pathQueue.Dequeue();
            }
        }
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            currentPos = WorldToGrid(transform.position);
            targetPos = WorldToGrid(player.position);

            pathQueue = FindPath(currentPos, targetPos);

            yield return new WaitForSeconds(updateInterval);
        }
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x / mazeGen.cellSize),
            Mathf.RoundToInt(pos.z / mazeGen.cellSize)
        );
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * mazeGen.cellSize, 0.5f, gridPos.y * mazeGen.cellSize);
    }

    Queue<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var maze = mazeGen.GetMaze();
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal) break;

            foreach (var dir in new[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x >= 0 && neighbor.x < mazeGen.width &&
                    neighbor.y >= 0 && neighbor.y < mazeGen.height &&
                    maze[neighbor.x, neighbor.y] == 0 && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        var path = new Queue<Vector2Int>();
        Vector2Int cur = goal;
        while (cur != start)
        {
            if (!cameFrom.ContainsKey(cur)) return new Queue<Vector2Int>(); // �s���~�܂�
            path.Enqueue(cur);
            cur = cameFrom[cur];
        }

        var reversed = new Queue<Vector2Int>();
        foreach (var p in path)
            reversed.Enqueue(p);

        return new Queue<Vector2Int>(new Stack<Vector2Int>(path));
    }

}
