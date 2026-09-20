using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 50f;

    [SerializeField]
    private float minX = -16f;

    [SerializeField]
    private float maxX = 19f;

    [SerializeField]
    private float slowDuration = 3f;  

    [SerializeField]
    private float slowMultiplier = 0.5f; // ความช้าตอนโดนดีบัฟ (0.5 = ช้าลงครึ่งหนึ่ง)

    private float currentSpeed;
    private Coroutine slowRoutine;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        float move = 0f;

        if (Keyboard.current.dKey.isPressed)
        {
            move += 1f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            move -= 1f;
        }

        if (move != 0f)
        {
            Move(move);
        }
    }

    private void Move(float direction)
    {
        transform.position += Vector3.right * direction * currentSpeed * Time.deltaTime;

        float x = Mathf.Clamp(transform.position.x, minX, maxX);

        transform.position = new Vector3(
            x,
            transform.position.y,
            transform.position.z
        );
    }

    
    public void ApplySlow()
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(SlowRoutine());
    }

    private IEnumerator SlowRoutine()
    {
        currentSpeed = moveSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        currentSpeed = moveSpeed;
        slowRoutine = null;
    }
}