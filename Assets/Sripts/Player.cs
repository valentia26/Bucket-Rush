using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float minX = -7f;

    [SerializeField]
    private float maxX = 7f;

    [Header("Slow Debuff (โดน rock)")]
    [SerializeField]
    private float slowAmount = 5f;     // ลดความเร็วลงเท่าไหร่ตอนโดน rock

    [SerializeField]
    private float slowDuration = 3f;   // โดนดีบัฟนานกี่วินาที

    private float currentSpeed;
    private Coroutine slowRoutine;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        if (Keyboard.current.dKey.isPressed)
        {
            MoveRight();
        }

        if (Keyboard.current.aKey.isPressed)
        {
            MoveLeft();
        }
    }

    private void MoveRight()
    {
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        float x = Mathf.Clamp(transform.position.x, minX, maxX);

        transform.position = new Vector3(
            x,
            transform.position.y,
            transform.position.z
        );
    }

    private void MoveLeft()
    {
        transform.position += Vector3.left * currentSpeed * Time.deltaTime;

        float x = Mathf.Clamp(transform.position.x, minX, maxX);

        transform.position = new Vector3(
            x,
            transform.position.y,
            transform.position.z
        );
    }
}