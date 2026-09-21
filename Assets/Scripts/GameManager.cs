using UnityEngine;
using UnityEngine.InputSystem; // เพิ่ม using นี้เพื่อใช้ Input System ตัวใหม่แทน UnityEngine.Input
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
    public float spawnInterval = 0.75f;   // ระยะเวลาระหว่างการ spawn แต่ละครั้ง (วินาที)
    public float minX = -12f;          // ขอบซ้ายของพื้นที่ spawn
    public float maxX = 11f;           // ขอบขวาของพื้นที่ spawn
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
    public TMP_Text countdownText;     // (ไม่บังคับ) ลาก UI Text มาใส่เพื่อแสดงตัวนับถอยหลังฝนเพชร (เช่น "3", "2", "1")
    public GameObject gameOverPanel;   // (ไม่บังคับ) แผง Game Over เมื่อพลังชีวิตหมด

    [Header("Game Over UI - Score Summary")]
    public TMP_Text finalScoreText;    // ลาก UI Text (บน gameOverPanel) มาใส่เพื่อแสดงคะแนนที่ได้ในตานี้
    public TMP_Text bestScoreText;     // ลาก UI Text (บน gameOverPanel) มาใส่เพื่อแสดง Best Score ตลอดกาล

    [Header("Pause Settings")]
    public GameObject pausePanel;      // ลาก Panel ที่จะโชว์ตอนกด Pause มาใส่ (มีปุ่ม Resume / Exit อยู่ข้างใน)
    // หมายเหตุ: ใช้ Keyboard.current.escapeKey (Input System ใหม่) แทน KeyCode แบบเก่า
    // ถ้าอยากเปลี่ยนปุ่ม ให้แก้ตรง Update() ตรงบรรทัดเช็ค escapeKey โดยตรง
    private bool isPaused = false;

    private const string BestScoreKey = "BestScore"; // key สำหรับเก็บ Best Score ใน PlayerPrefs

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

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // กดปุ่ม Escape เพื่อ Pause/Resume สลับกันไปมา (ใช้ Input System ตัวใหม่แทน UnityEngine.Input)
        // เช็ค Keyboard.current != null กันเคส build บางแพลตฟอร์มที่ไม่มีคีย์บอร์ดต่ออยู่ (เช่นมือถือ)
        if (!isGameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (isGameOver || isPaused) return;

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
    // (FallingItem.cs จะไม่เรียกฟังก์ชันนี้เลยถ้าไอเทมเป็น Rock หรือ Bomb)
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

        float remaining = diamondRainDuration;
        while (remaining > 0f)
        {
            UpdateCountdownUI(remaining);
            yield return null;
            remaining -= Time.deltaTime;
        }

        UpdateCountdownUI(0f); // ซ่อนตัวเลขนับถอยหลังเมื่อหมดเวลา
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

    // เรียกเมื่อพลังชีวิตหมด (หรือจะเรียกตอนโดน bomb ก็ได้ถ้าต้องการให้จบเกมทันที)
    public void GameOver()
    {
        if (isGameOver) return; // กันเรียกซ้ำ

        isGameOver = true;

        // หยุด coroutine ทั้งหมดที่ยังค้างอยู่ (เช่นฝนเพชรที่กำลังนับถอยหลัง)
        StopAllCoroutines();
        diamondRainActive = false;
        UpdateCountdownUI(0f); // เคลียร์ตัวเลขนับถอยหลังบนจอ ถ้ามีค้างอยู่

        // ถ้าเผลอกด Pause ค้างไว้ตอนจบเกมพอดี ให้ปิด Pause Panel ทิ้งไปด้วย
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // ดึง Best Score เดิมที่เคยบันทึกไว้ในเครื่อง (ถ้าไม่เคยมีมาก่อนให้เริ่มที่ 0)
        float previousBest = PlayerPrefs.GetFloat(BestScoreKey, 0f);

        // เช็คว่าตานี้ทำคะแนนได้มากกว่าสถิติเดิมหรือไม่
        bool isNewBest = score > previousBest;
        float bestScore = isNewBest ? score : previousBest;

        // ถ้าทำลายสถิติ ให้บันทึกค่าใหม่ลง PlayerPrefs แบบถาวร
        if (isNewBest)
        {
            PlayerPrefs.SetFloat(BestScoreKey, bestScore);
            PlayerPrefs.Save(); // เขียนลงดิสก์ทันที กันเกม/แอปปิดกะทันหันแล้วข้อมูลหาย
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // แสดงคะแนนที่ได้ในตานี้ และ Best Score บน UI ตอนจบเกม
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("0.0");

        if (bestScoreText != null)
            bestScoreText.text = "Best Score: " + bestScore.ToString("0.0");

        Debug.Log($"Game Over! Final Score: {score} | Best Score: {bestScore} {(isNewBest ? "(สถิติใหม่!)" : "")}");

        // หยุดเวลาทั้งเกม — ไอเทมที่กำลังร่วงอยู่ (เคลื่อนที่ด้วย Time.deltaTime) จะหยุดนิ่งทันที
        Time.timeScale = 0f;
    }

    // เรียกตอนกดปุ่ม Pause (ปุ่ม UI หรือคีย์บอร์ด)
    public void PauseGame()
    {
        if (isGameOver || isPaused) return; // กันกด pause ตอนเกมจบ หรือกดซ้ำตอน pause อยู่แล้ว

        isPaused = true;
        Time.timeScale = 0f; // หยุดเวลาทั้งเกม ไอเทมที่ร่วงอยู่จะหยุดนิ่งทันที

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("[GameManager] Pause Game");
    }

    // เรียกตอนกดปุ่ม Resume บน Pause Panel (หรือกด Escape ซ้ำ)
    public void ResumeGame()
    {
        if (isGameOver || !isPaused) return; // กันกด resume ตอนเกมจบ หรือกดตอนไม่ได้ pause อยู่

        isPaused = false;
        Time.timeScale = 1f; // คืนเวลาให้เกมเดินต่อตามปกติ

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Debug.Log("[GameManager] Resume Game");
    }

    // เรียกตอนกดปุ่ม Restart บนหน้า Game Over Panel
    public void RestartGame()
    {
        isPaused = false; // เคลียร์สถานะ pause ก่อนโหลดฉากใหม่
        Time.timeScale = 1f; // คืนค่าเวลาปกติก่อนโหลดฉากใหม่ ไม่งั้นเกมรอบใหม่จะค้างนิ่ง
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    
    public void ExitGame()
    {
        Time.timeScale = 1f; 
        Debug.Log("Exit Game ถูกกด");

#if UNITY_EDITOR
        // ตอนทดสอบใน Unity Editor ให้หยุด Play Mode แทน เพราะ Application.Quit() ไม่ทำงานใน Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ตอน build จริงเป็น .exe / .apk ฯลฯ จะปิดแอปพลิเคชันจริง
        Application.Quit();
#endif
    }

    [ContextMenu("Reset Best Score")]
    public void ResetBestScore()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        Debug.Log("[GameManager] Best Score ถูกรีเซ็ตแล้ว");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("0.0"); // แสดงทศนิยม 1 ตำแหน่ง เช่น 0.5, 1.0, 10.0
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "HP: " + currentHealth.ToString("0");
    }

    void UpdateComboUI()
    {
        if (comboText != null)
            comboText.text = "Combo " + comboCount;
    }

    void UpdateCountdownUI(float secondsRemaining)
    {
        if (countdownText == null) return;

        if (secondsRemaining <= 0f)
        {
            countdownText.text = "";
        }
        else
        {
          
            countdownText.text = Mathf.CeilToInt(secondsRemaining).ToString();
        }
    }
}