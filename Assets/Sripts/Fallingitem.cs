using UnityEngine;

public class FallingItem : MonoBehaviour
{
    public enum ItemType { Rock, Star, Diamond, Bomb }

    [Header("Item Settings")]
    public ItemType itemType;      // ตั้งค่าให้ตรงกับชนิดของ Prefab นี้ (Rock/Star/Diamond/Bomb) ใน Inspector
    public float fallSpeed = 3f;   // ความเร็วในการร่วง
    public float destroyBelowY = -6f; // ถ้าร่วงต่ำกว่าค่านี้แล้วยังไม่โดนเก็บ ให้ทำลายทิ้ง

    void Update()
    {
        // ทำให้ไอเทมร่วงลงเรื่อยๆ
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // ทำลายตัวเองถ้าหลุดจอไปแล้ว (ไม่โดนเก็บ)
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
    }

    // ใช้ระบบ Trigger 2D: ต้องมี Collider2D (Is Trigger = true) ที่ไอเทม
    // และ player ต้องมี tag ว่า "Player" + มี Collider2D เช่นกัน
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.Rock:
                GameManager.Instance.AddScore(0.5f); // หิน +0.5

                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.ApplySlow(); // ทำให้ player เดินช้าลงชั่วคราว
                }
                break;

            case ItemType.Star:
                GameManager.Instance.AddScore(1f);   // ดาว +1
                break;

            case ItemType.Diamond:
                GameManager.Instance.AddScore(10f);  // เพชร +10
                break;

            case ItemType.Bomb:
                GameManager.Instance.AddScore(-2f);  // ระเบิด -2 (ไม่จบเกม แค่หักแต้ม)
                break;
        }

        Destroy(gameObject); // เก็บแล้วให้ไอเทมหายไป
    }
}