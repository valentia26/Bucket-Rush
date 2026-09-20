using UnityEngine;
using TMPro; // ถ้าใช้ Text ธรรมดาแทน TextMeshPro ให้เปลี่ยนเป็น using UnityEngine.UI; แล้วเปลี่ยนชนิดตัวแปร scoreText เป็น Text
using System.Collections;

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
    public float minX = -16f;          // ขอบซ้ายของพื้นที่ spawn
    public float maxX = 19f;           // ขอบขวาของพื้นที่ spawn
    public float spawnY = 20f;         // ตำแหน่ง Y ที่ไอเทมจะเริ่มร่วงลงมา

    [Header("Combo Settings")]
    public int comboToTriggerRain = 5;     // เก็บติดกันกี่ครั้งถึงเข้าโหมดฝนเพชร
    public float diamondRainDuration = 3f; // ฝนเพชรอยู่นานกี่วินาที
    private int comboCount = 0;
    private bool diamondRainActive = false;

    [Header("Life Settings")]
    public float maxHealth = 100f;
    public float lifeLossPerMiss = 20f; // ของร่วงพื้น 1 ครั้ง ลดพลังชีวิตเท่านี้
    private float currentHealth;

    [Header("UI")]
    public TMP_Text scoreText;         // ลาก UI Text มาใส่เพื่อแสดงคะแนน
    public TMP_Text livesText;         // (ไม่บังคับ) ลาก UI Text มาใส่เพื่อแสดงพลังชีวิต
    public TMP_Text comboText;         // (ไม่บังคับ) ลาก UI Text มาใส่เพื่อแสดงคอมโบปัจจุบัน
    public GameObject gameOverPanel;   // (ไม่บังคับ) แผง Game Over เมื่อพลังชีวิตหมด

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
        currentHealth = maxHealth;

        UpdateScoreUI();
        UpdateLivesUI();
        UpdateComboUI();

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
        GameObject prefabToSpawn;

        if (diamondRainActive)
        {
            // โหมดฝนเพชร: spawn เป็น Diamond ล้วน ไม่สุ่มชนิดอื่น
            prefabToSpawn = diamondPrefab;
        }
        else
        {
            // สุ่มเลือกไอเทม 1 ใน 4 ชนิดตามปกติ
            int randomIndex = Random.Range(0, 4);
            switch (randomIndex)
            {
                case 0: prefabToSpawn = rockPrefab; break;
                case 1: prefabToSpawn = starPrefab; break;
                case 2: prefabToSpawn = diamondPrefab; break;
                case 3: prefabToSpawn = bombPrefab; break;
                default: prefabToSpawn = null; break;
            }
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

    // เรียกจาก FallingItem.cs ทุกครั้งที่ player เก็บไอเทมได้สำเร็จ (ใช้คุมคอมโบ)
    public void ItemCaught(FallingItem.ItemType type)
    {
        if (isGameOver) return;

        if (type == FallingItem.ItemType.Bomb)
        {
            // โดน bomb ถือว่าคอมโบขาด
            comboCount = 0;
        }
        else
        {
            comboCount++;

            if (comboCount >= comboToTriggerRain && !diamondRainActive)
            {
                StartCoroutine(DiamondRainRoutine());
                comboCount = 0;
            }
        }

        UpdateComboUI();
    }

    // เรียกจาก FallingItem.cs เมื่อไอเทมร่วงพ้นจอไปโดยไม่โดน player เก็บ
    public void ItemMissed(FallingItem.ItemType type)
    {
        if (isGameOver) return;

        // พลาดของ ถือว่าคอมโบขาดเช่นกัน
        comboCount = 0;
        UpdateComboUI();

        LoseLife(lifeLossPerMiss);
    }

    private IEnumerator DiamondRainRoutine()
    {
        diamondRainActive = true;
        yield return new WaitForSeconds(diamondRainDuration);
        diamondRainActive = false;
    }

    private void LoseLife(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateLivesUI();

        if (currentHealth <= 0f)
        {
            GameOver();
        }
    }

   
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
            scoreText.text = "Score: " + score.ToString("0.0");
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "HP: " + currentHealth.ToString("0");
    }

    void UpdateComboUI()
    {
        if (comboText != null)
            comboText.text = "Combo: " + comboCount;
    }
}