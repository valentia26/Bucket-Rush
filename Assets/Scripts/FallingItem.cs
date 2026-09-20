using UnityEngine;

public class FallingItem : MonoBehaviour
{
    public enum ItemType { Rock, Star, Diamond, Bomb }

    [Header("Item Settings")]
    public ItemType itemType;          // ตั้งค่าให้ตรงกับชนิดของ Prefab นี้ ใน Inspector
    public float fallSpeed = 3f;       // ความเร็วในการร่วง
    public float destroyBelowY = -6f;  // ถ้าร่วงต่ำกว่าค่านี้แล้วยังไม่โดนเก็บ ให้ทำลายทิ้ง

    [Header("Score Values")]
    public float rockScore = -1f;
    public float starScore = 1f;
    public float diamondScore = 10f;
    public float bombScore = -2f;

    private bool collected = false; // กันไม่ให้นับซ้ำหรือ trigger ซ้ำ

    void Update()
    {
        // ทำให้ไอเทมร่วงลงเรื่อยๆ
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // ร่วงพ้นจอไปโดยไม่โดนเก็บ
        if (!collected && transform.position.y < destroyBelowY)
        {
            collected = true; // กันการเรียกซ้ำ

            // Rock กับ Bomb: ถ้าไม่โดน Player เก็บ ปล่อยร่วงพ้นจอเฉยๆ
            // ไม่ถือว่า "พลาด" จึงไม่เรียก ItemMissed (ไม่ลดเลือด/ไม่หักคะแนน)
            if (itemType == ItemType.Bomb || itemType == ItemType.Rock)
            {
                Debug.Log($"[FallingItem] {itemType} ร่วงพ้นจอ (ไม่นับพลาด ไม่หักคะแนน)");
            }
            else
            {
                Debug.Log($"[FallingItem] {itemType} ร่วงพ้นจอ (missed)");
                GameManager.Instance.ItemMissed(itemType);
            }

            Destroy(gameObject);
        }
    }

    // ใช้ระบบ Trigger แบบ 3D: ต้องมี Collider (3D, Is Trigger = true) ที่ไอเทม
    // และ player ต้องมี tag ว่า "Player" + มี Collider (3D) เช่นกัน
    void OnTriggerEnter(Collider other)
    {
        // ---- DEBUG: log ทุก object ที่มาชน ไม่ว่าจะเป็น Player หรือไม่ ----
        Debug.Log($"[FallingItem] {itemType} ชนกับ '{other.name}' (tag = {other.tag})");

        if (collected)
        {
            Debug.Log($"[FallingItem] {itemType} ถูกเก็บไปแล้ว ข้าม event นี้");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[FallingItem] '{other.name}' ไม่ใช่ Player (tag ไม่ตรง) ข้าม");
            return;
        }

        collected = true; // กันการเรียกซ้ำ
        Debug.Log($"[FallingItem] {itemType} ถูก Player เก็บ! กำลังเพิ่มคะแนน...");

        switch (itemType)
        {
            case ItemType.Rock:
                GameManager.Instance.AddScore(rockScore);

                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.ApplySlow(); // ทำให้ player เดินช้าลงชั่วคราว
                }
                else
                {
                    Debug.LogWarning("[FallingItem] หา component Player บน object ที่ชนไม่เจอ (ApplySlow จะไม่ทำงาน)");
                }
                break;

            case ItemType.Star:
                GameManager.Instance.AddScore(starScore);
                break;

            case ItemType.Diamond:
                GameManager.Instance.AddScore(diamondScore);
                break;

            case ItemType.Bomb:
                GameManager.Instance.AddScore(bombScore); // โดน Player เก็บระเบิดโดยตรง ถึงจะหักแต้ม/เลือด
                break;
        }

        GameManager.Instance.ItemCaught(itemType); // แจ้ง GameManager ไปคุมคอมโบ

        Destroy(gameObject); // เก็บแล้วให้ไอเทมหายไป
    }
}