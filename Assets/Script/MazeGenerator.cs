using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    //���H�̏c�Ɖ��̑傫��(�����)
    public int width = 31;
    public int height = 31;

    //�ǂƏ�����邽�߂̍ޗ�
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    //�S�[���ƓG�����ޗ�
    public GameObject goalPrefab;
    public GameObject enemyPrefab;

    //�}�X��1�̑傫��
    public float cellSize = 2f;
    public Transform playerTransform;

    //�X�^�[�g�ƃS�[���̃}�X�̍��W
    public Vector2Int startPos;
    private Vector2Int goalPos;

    //�G�̂���ꏊ�����X�g�ŊǗ�
    private List<Vector2Int> enemyPositions = new List<Vector2Int>();

    //���H�̏��(0 = �ʘH�A1 = ��)
    public static int[,] maze;

    //���̃X�N���v�g������H��ꏊ�����o����悤�ɂ���
    public int[,] GetMaze() => maze;
    public Vector2Int GetStartPos() => startPos;

    private void Awake()
    {
        GenerateMaze();
        SetStartGoalAndEnemies();
        DrawMaze3D();
    }

    //���H�����֐�
    void GenerateMaze()
    {
        maze = new int[width, height];

        //�܂��S�Ă��u�ǁv�ɂ���
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1; // �S���ǂɂ��ď�����

        //�ʘH��(1, 1)����@��n�߂�
        RecursiveDig(1, 1);
    }

    //�ʘH���@��֐�
    void RecursiveDig(int x, int y)
    {
        maze[x, y] = 0; // ������}�X��ʘH�ɂ���
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(2, 0), //�E
            new Vector2Int(-2, 0), //��
            new Vector2Int(0, 2), //��
            new Vector2Int(0, -2) //��
        };

        Shuffle(directions);//���񃉃��_���ȏ��ԂŌ@��

        //���ꂼ��̕����Ɍ@���Ă���
        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            //���H���ł���A�܂��ǂȂ�
            if (IsInBounds(nx, ny) && maze[nx, ny] == 1)
            {
                //�ǂ�1�}�X��������
                maze[x + dir.x / 2, y + dir.y / 2] = 0;
                //���̐�ɂ܂��i��(�J��Ԃ�)
                RecursiveDig(nx, ny);
            }
        }
    }

    //��������H�ɕǂ⏰��z�u
    void DrawMaze3D()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                if (floorPrefab != null)
                {
                    Instantiate(floorPrefab, pos + Vector3.down * 1.5f, Quaternion.identity, transform);
                }

                if (maze[x, y] == 1) // ��
                {
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);

                    // �O�g���ǂ����𔻒�
                    bool isOuterWall = (x == 0 || x == width - 1 || y == 0 || y == height - 1);

                    if (isOuterWall)
                    {
                        // �O�g�̏ꍇ�� BoxCollider ���O��
                        BoxCollider collider = wall.GetComponent<BoxCollider>();
                        if (collider != null)
                        {
                            Destroy(collider); // ���̕ǂ��� BoxCollider ���폜
                        }
                    }

                    WallColor script = wall.GetComponent<WallColor>();
                    if (script != null)
                    {
                        script.x = x;
                        script.y = y;
                    }
                }

            }
        }

        // �S�[���̐ݒu
        if (goalPrefab != null)
        {
            Vector3 gpos = new Vector3(goalPos.x * cellSize, 0f, goalPos.y * cellSize);
            Instantiate(goalPrefab, gpos, Quaternion.identity, transform);
        }

        // �G�̐���
        foreach (Vector2Int enemyPos in enemyPositions)
        {
            Vector3 epos = new Vector3(enemyPos.x * cellSize, 0.5f, enemyPos.y * cellSize);
            GameObject enemy = Instantiate(enemyPrefab, epos, Quaternion.identity, transform);

            //�G�̓����𐧌䂷��X�N���v�g��t����
            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<EnemyController>();
            }

            //�v���C���[�̈ʒu����H�̏���n��
            controller.mazeGen = this;
            controller.player = playerTransform;
        }

    }

    // ���X�g�̒��g�������_���ɃV���b�t���i���בւ��j
    void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    // ���H�̒��ɂ��邩�ǂ������`�F�b�N
    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width - 1 && y > 0 && y < height - 1;
    }

    // �X�^�[�g�E�S�[���E�G�̈ʒu�����߂�
    void SetStartGoalAndEnemies()
    {
        // 4�̊p�����X�g�ɒǉ����ăV���b�t��
        List<Vector2Int> corners = new List<Vector2Int>
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, height - 2),
            new Vector2Int(width - 2, 1),
            new Vector2Int(width - 2, height - 2)
        };

        Shuffle(corners);// �����_���ɕ��т�����

        // �ŏ��̊p���X�^�[�g�A���̊p���S�[���ɂ���
        startPos = corners[0];
        goalPos = corners[1];

        // �c���2�̊p�ɓG��z�u
        enemyPositions.Add(corners[2]);
        enemyPositions.Add(corners[3]);

        // �ʘH�ɂ���
        maze[startPos.x, startPos.y] = 0;
        maze[goalPos.x, goalPos.y] = 0;
        foreach (var pos in enemyPositions)
        {
            maze[pos.x, pos.y] = 0;
        }
    }

}
