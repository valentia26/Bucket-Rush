using UnityEngine;
using TMPro; // ถ้าใช้ Text ธรรมดาแทน TextMeshPro ให้เปลี่ยนเป็น using UnityEngine.UI; แล้วเปลี่ยนชนิดตัวแปร scoreText เป็น Text

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton เผื่อให้ FallingItem เรียกใช้ง่าย

    [Header("Item Prefabs (ลาก Prefab มาใส่ตามลำดับ)")]
    public GameObject rockPrefab;
    public GameObject starPrefab;
    public GameObject diamondPrefab;
    public GameObject bombPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;   // ระยะเวลาระหว่างการ spawn แต่ละครั้ง (วินาที)
    public float minX = -4f;           // ขอบซ้ายของพื้นที่ spawn
    public float maxX = 4f;            // ขอบขวาของพื้นที่ spawn
    public float spawnY = 6f;          // ตำแหน่ง Y ที่ไอเทมจะเริ่มร่วงลงมา

    [Header("UI")]
    public TMP_Text scoreText;         // ลาก UI Text มาใส่เพื่อแสดงคะแนน
    public GameObject gameOverPanel;   // (ไม่บังคับ) แผง Game Over เมื่อโดน bomb

    private float timer;
    private float score = 0f;
    private bool isGameOver = false;

    void Awake()
    {
        // ตั้งค่า Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        timer = spawnInterval;
        UpdateScoreUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnRandomItem();
            timer = spawnInterval;
        }
    }

    void SpawnRandomItem()
    {
        // สุ่มเลือกไอเทม 1 ใน 4 ชนิด
        int randomIndex = Random.Range(0, 4);
        GameObject prefabToSpawn = null;

        switch (randomIndex)
        {
            case 0: prefabToSpawn = rockPrefab; break;
            case 1: prefabToSpawn = starPrefab; break;
            case 2: prefabToSpawn = diamondPrefab; break;
            case 3: prefabToSpawn = bombPrefab; break;
        }

        if (prefabToSpawn == null) return;

        // สุ่มตำแหน่ง X
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    // เรียกจาก FallingItem.cs เมื่อ player เก็บไอเทมได้ (amount เป็น float เพราะ rock ให้แต้มแบบ 0.5)
    public void AddScore(float amount)
    {
        if (isGameOver) return;

        score += amount;
        // หมายเหตุ: ตอนนี้ปล่อยให้คะแนนติดลบได้ (เช่นโดน bomb ตอนแต้มน้อย)
        // ถ้าต้องการกันไม่ให้ติดลบ ให้เปิดใช้บรรทัดล่างนี้แทน:
        // if (score < 0) score = 0;

        UpdateScoreUI();
    }

    // เรียกเมื่อโดนระเบิด (bomb)
    public void GameOver()
    {
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("Game Over! Final Score: " + score);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("0.0"); // แสดงทศนิยม 1 ตำแหน่ง เช่น 0.5, 1.0, 10.0
    }
}