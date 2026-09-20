using UnityEngine;

public class FallingItem : MonoBehaviour
{
    public enum ItemType { Rock, Star, Diamond, Bomb }

    [Header("Item Settings")]
    public ItemType itemType;          // ตั้งค่าให้ตรงกับชนิดของ Prefab นี้ ใน Inspector
    public float fallSpeed = 10f;       // ความเร็วในการร่วง
    public float destroyBelowY = 1f;  // ถ้าร่วงต่ำกว่าค่านี้แล้วยังไม่โดนเก็บ ให้ทำลายทิ้ง

    [Header("Score Values")]
    public float rockScore = 0.5f;
    public float starScore = 1f;
    public float diamondScore = 10f;
    public float bombScore = -2f;

    void Update()
    {
        
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
    }

   
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.Rock:
                GameManager.Instance.AddScore(rockScore);

                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.ApplySlow(); // ทำให้ player เดินช้าลงชั่วคราว
                }
                break;

            case ItemType.Star:
                GameManager.Instance.AddScore(starScore);
                break;

            case ItemType.Diamond:
                GameManager.Instance.AddScore(diamondScore);
                break;

            case ItemType.Bomb:
                GameManager.Instance.AddScore(bombScore); // ไม่จบเกม แค่หักแต้ม
                break;
        }

        Destroy(gameObject); // เก็บแล้วให้ไอเทมหายไป
    }
}