using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private MazeGenerator mazeGen;
    public WallColor wallColor;
    private float moveDuration = 0.2f;
    private float moveInterval = 0.3f;

    private int[,] maze;
    private Vector2Int currentPos;
    private float cellSize;
    private bool isMoving = false;

    private Animator animator;
    [SerializeField] AudioSource AS;
    [SerializeField] AudioSource bgmAS;
    [SerializeField] AudioClip wallBroken;
    [SerializeField] AudioClip swing;
    [SerializeField] AudioClip Goal;
    [SerializeField] AudioClip gameOver;

    public int brokenCount;
    public int maxAttack = 0;
    public bool Attack = false;
    private bool hit;
    // micro:bit�̃{�^���̏��(0: �Ȃ��A1: A�{�^���A-1: B�{�^��)
    private int buttonState = 0;

    private bool canRotate = true;
    private float tiltX = 0f;
    private float tiltZ = 0f;
    private float tiltThreshold = 0.3f;

    [SerializeField] Text Finishtxt;
    [SerializeField] Text Counttxt;

    // Start is called before the first frame update
    void Start()
    {
        brokenCount = 3;
        maxAttack = 0;

        Finishtxt.gameObject.SetActive(false);
        animator = GetComponent<Animator>();
        maze = mazeGen.GetMaze();
        cellSize = mazeGen.cellSize;
        currentPos = mazeGen.GetStartPos();
        Counttxt.text = "BROKEN:" + brokenCount;

        transform.position = new Vector3(currentPos.x * cellSize, -1f, currentPos.y * cellSize);
    }

    void Update()
    {

        if (!isMoving && !Attack)
        {
            Rotation();

            // �������ɑΉ�
            if (GetForwardInput())
            {
                StartCoroutine(MoveContinuousLoop());
            }

        }

        //�U��
        if (Input.GetKeyUp(KeyCode.Q) && brokenCount > 0 && !Attack)
        {

            StartCoroutine(PerformTripleAttack());

        }
        else if (buttonState == 1 && brokenCount > 0 && !Attack)
        {
            StartCoroutine(PerformTripleAttack());
        }



    }

    //�O�ւ�Input���擾
    bool GetForwardInput()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || tiltZ < -tiltThreshold;
    }

    //�����]��
    void Rotation()
    {
        if (canRotate)
        {
            if (Input.GetKeyDown(KeyCode.A) || tiltX > 0.4f)
            {
                StartCoroutine(SmoothRotate(-90f));
                canRotate = false;
            }
            else if (Input.GetKeyDown(KeyCode.D) || tiltX < -0.4f)
            {
                StartCoroutine(SmoothRotate(90f));
                canRotate = false;
            }

            if (Mathf.Abs(tiltX) < 0.2f)
            {
                canRotate = true;
            }
        }
    }

    //��������
    IEnumerator PerformTripleAttack()
    {
        Attack=true;
        for (int i = 0; i < 3; i++)
        {
            animator.SetTrigger("isAttack");
            maxAttack++;

            if (hit)
            {
                AS.PlayOneShot(wallBroken);
            }
            else
            {
                AS.PlayOneShot(swing);
            }
            yield return new WaitForSeconds(0.5f);
        }

        Counttxt.text = "BROKEN:" + brokenCount;
        maxAttack = 0;
        Attack = false;

    }

    //�u�O�ɐi�ށv�L�[�����������Ă���ԁA����Ԃ��L�����𓮂��������鏈��
    IEnumerator MoveContinuousLoop()
    {
        while (GetForwardInput())
        {
            Vector2Int direction = GetForwardDirection();
            Vector2Int target = currentPos + direction;

            if (IsWalkable(target))
            {
                Vector3 startPos = transform.position;
                Vector3 endPos = new Vector3(target.x * cellSize, -1f, target.y * cellSize);
                yield return MoveSmoothly(startPos, endPos, target);
            }

            yield return new WaitForSeconds(moveInterval);
        }
    }

    //�������Ă�������𒲂ׂāA�u�ǂ����ɐi�ނ��v�����߂܂��B
    Vector2Int GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (Mathf.Abs(forward.z) > Mathf.Abs(forward.x))
        {
            return (forward.z > 0) ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        }
        else
        {
            return (forward.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        }
    }

    //�i�����Ƃ��Ă���}�X���u�i��ł����ꏊ�v���ǂ������`�F�b�N���܂��B
    bool IsWalkable(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < maze.GetLength(0) &&
               pos.y >= 0 && pos.y < maze.GetLength(1) &&
               maze[pos.x, pos.y] == 0;
    }

    //�L�����N�^�[���X�^�[�g�n�_����S�[���n�_�܂ŁA�Ȃ߂炩�Ɉړ�������֐��ł��B
    IEnumerator MoveSmoothly(Vector3 start, Vector3 end, Vector2Int nextPos)
    {
        isMoving = true;
        animator.SetBool("isMoving", true);
        float elapsed = 0f;

        Vector3 direction = (end - start).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        currentPos = nextPos;
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    //�L�������Ȃ߂炩�ɍ��E��90�x�񂷏����ł��B
    IEnumerator SmoothRotate(float angle)
    {
        isMoving = true;  // ��]���͈ړ��ł��Ȃ��悤�ɂ���
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);
        float duration = 0.2f;  // ��]�ɂ����鎞��
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isMoving = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            bgmAS.Stop();
            AS.PlayOneShot(gameOver);
            Finishtxt.text = "GameOver";
            Finishtxt.gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        if (collision.gameObject.tag == "Goal")
        {
            bgmAS.Stop();
            AS.PlayOneShot(Goal);
            Finishtxt.text = "GameClear";
            Finishtxt.gameObject.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("�S�[���ɓ��B���܂����I");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wall") hit = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "wall") hit = false;
    }

    // micro:bit �X���f�[�^�iX���j
    public void OnAccelerometerChangedx(int x)
    {
        const float MAX_X = 1600f;
        tiltX = Mathf.Clamp(x / MAX_X, -1f, 1f);
    }

    // micro:bit �X���f�[�^�iY�� �� Z�ړ��j
    public void OnAccelerometerChangedy(int y)
    {
        const float MAX_Y = 1600f;
        tiltZ = Mathf.Clamp(y / MAX_Y, -1f, 1f);
    }

    public void OnButtonAChanged(int state)
    {
        buttonState = (state == 0 ? 0 : 1);
    }

}
