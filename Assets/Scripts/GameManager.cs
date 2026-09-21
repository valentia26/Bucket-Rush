using UnityEngine;
using UnityEngine.InputSystem; // เพิ่ม using นี้เพื่อใช้ Input System ตัวใหม่แทน UnityEngine.Input
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
    public float minX = -16f;          // ขอบซ้ายของพื้นที่ spawn
    public float maxX = 19f;           // ขอบขวาของพื้นที่ spawn
    public float spawnY = 20f;         // ตำแหน่ง Y ที่ไอเทมจะเริ่มร่วงลงมา

    [Header("Combo Settings")]
    public int comboToTriggerRain = 5;     // เก็บติดกันกี่ครั้งถึงเข้าโหมดฝนเพชร
    public float diamondRainDuration = 10f; // ฝนเพชรอยู่นานกี่วินาที
    public float diamondRainSpawnInterval = 0.75f; // ระหว่างฝนเพชร spawn ถี่ขึ้นทุกกี่วินาที
    public float postRainGracePeriod = 2.5f; // หลังฝนเพชรจบ ผ่อนผันไม่ลด HP กี่วิ (2-3 วิ) หรือจนกว่าเพชรบนจอจะหมด แล้วแต่อะไรถึงก่อน
    private int comboCount = 0;
    private bool diamondRainActive = false;
    private bool inPostRainGrace = false; // อยู่ในช่วงผ่อนผันหลังฝนเพชรจบหรือไม่ (ระหว่างนี้ยังไม่ลด HP)
    private float normalSpawnInterval; // เก็บค่า spawnInterval ปกติไว้ เพื่อคืนค่ากลับหลังฝนเพชรจบ

    [Header("Life Settings")]
    public float maxHealth = 100f;
    public float lifeLossPerMiss = 20f; // ของร่วงพื้น 1 ครั้ง ลดพลังชีวิตเท่านี้
    private float currentHealth;

    // หมายเหตุ: ข้อความ/UI ทั้งหมด (score, lives, combo, countdown, game over panel)
    // ย้ายไปให้ UiManager.cs จัดการแทนแล้ว GameManager แค่เรียก UiManager.Instance.Xxx(...)
    // เพื่อบอกค่าที่เปลี่ยนไป ไม่ต้องลาก UI Text มาใส่ที่นี่อีกต่อไป

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

        if (UiManager.Instance != null)
        {
            UiManager.Instance.UpdateScore(score);
            UiManager.Instance.UpdateLives(currentHealth);
            UiManager.Instance.UpdateCombo(comboCount);
        }

     
    }

    void Update()
    {
        
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
           
            prefabToSpawn = diamondPrefab;
        }
        else
        {
           
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

       float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
       
    }

    public void AddScore(float amount)
    {
        if (isGameOver) return;

        score += amount;

        if (UiManager.Instance != null)
            UiManager.Instance.UpdateScore(score);
    }

   
    public void ItemCaught(FallingItem.ItemType type)
    {
        if (isGameOver) return;

        if (type == FallingItem.ItemType.Bomb)
        {
           
            comboCount = 0;
        }
        else if (!diamondRainActive)
        {
           
            comboCount++;

            if (comboCount >= comboToTriggerRain)
            {
                StartCoroutine(DiamondRainRoutine());
                comboCount = 0;
            }
        }

        if (UiManager.Instance != null)
            UiManager.Instance.UpdateCombo(comboCount);
    }

    public void ItemMissed(FallingItem.ItemType type)
    {
        if (isGameOver) return;

        
        comboCount = 0;
        if (UiManager.Instance != null)
            UiManager.Instance.UpdateCombo(comboCount);

        LoseLife(lifeLossPerMiss);
    }

    private IEnumerator DiamondRainRoutine()
    {
        diamondRainActive = true;

        ClearNonDiamondItemsOnScreen();

        normalSpawnInterval = spawnInterval;
        spawnInterval = diamondRainSpawnInterval;
        timer = 0f; 

        float remaining = diamondRainDuration;
        while (remaining > 0f)
        {
            if (UiManager.Instance != null)
                UiManager.Instance.UpdateCountdown(remaining);
            yield return null;
            remaining -= Time.deltaTime;
        }

        if (UiManager.Instance != null)
            UiManager.Instance.UpdateCountdown(0f); // ซ่อนตัวเลขนับถอยหลังเมื่อหมดเวลา
        diamondRainActive = false;

      
        spawnInterval = normalSpawnInterval;

        comboCount = 0;
        if (UiManager.Instance != null)
            UiManager.Instance.UpdateCombo(comboCount);

     
        yield return StartCoroutine(PostRainGraceRoutine());
    }

    
    private IEnumerator PostRainGraceRoutine()
    {
        inPostRainGrace = true;

        float elapsed = 0f;
        while (elapsed < postRainGracePeriod)
        {
            
            if (!HasDiamondsOnScreen())
                break;

            yield return null;
            elapsed += Time.deltaTime;
        }

        inPostRainGrace = false;
    }

    
    private bool HasDiamondsOnScreen()
    {
        FallingItem[] itemsOnScreen = FindObjectsOfType<FallingItem>();
        foreach (FallingItem item in itemsOnScreen)
        {
            if (item.itemType == FallingItem.ItemType.Diamond)
                return true;
        }
        return false;
    }

    
    private void ClearNonDiamondItemsOnScreen()
    {
       
        FallingItem[] itemsOnScreen = FindObjectsOfType<FallingItem>();

        foreach (FallingItem item in itemsOnScreen)
        {
            if (item.itemType != FallingItem.ItemType.Diamond)
            {
                Destroy(item.gameObject);
            }
        }
    }

 
    private void LoseLife(float amount)
    {
        if (diamondRainActive || inPostRainGrace)
        {
            Debug.Log("[GameManager] อยู่ระหว่างฝนเพชร หรือช่วงผ่อนผันหลังฝนเพชร ไม่มีการลดเลือด");
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (UiManager.Instance != null)
            UiManager.Instance.UpdateLives(currentHealth);

        if (currentHealth <= 0f)
        {
            GameOver();
        }
    }

    public void TakeBombDamage(float amount)
    {
        if (isGameOver) return;

        LoseLife(amount);
    }

    
    public void GameOver()
    {
        if (isGameOver) return; 

        isGameOver = true;

       
        StopAllCoroutines();

        
        if (diamondRainActive)
        {
            spawnInterval = normalSpawnInterval;
        }
        diamondRainActive = false;
        inPostRainGrace = false; 

        if (UiManager.Instance != null)
            UiManager.Instance.UpdateCountdown(0f); // เคลียร์ตัวเลขนับถอยหลังบนจอ ถ้ามีค้างอยู่

        float previousBest = PlayerPrefs.GetFloat(BestScoreKey, 0f);

      
        bool isNewBest = score > previousBest;
        float bestScore = isNewBest ? score : previousBest;

      
        if (isNewBest)
        {
            PlayerPrefs.SetFloat(BestScoreKey, bestScore);
            PlayerPrefs.Save(); 
        }

        
        if (UiManager.Instance != null)
            UiManager.Instance.ShowGameOver(score, bestScore, isNewBest);

        Debug.Log($"Game Over! Final Score: {score} | Best Score: {bestScore} {(isNewBest ? "(สถิติใหม่!)" : "")}");

       
        Time.timeScale = 0f;
    }

   
    public void PauseGame()
    {
        if (isGameOver || isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; 
    }

 
    public void ResumeGame()
    {
        if (isGameOver || !isPaused) return; 

        isPaused = false;
        Time.timeScale = 1f; 
    }

    public void RestartGame()
    {
        isPaused = false; 
        Time.timeScale = 1f; 
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
      
        Application.Quit();
#endif
    }

    [ContextMenu("Reset Best Score")]
    public void ResetBestScore()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        Debug.Log("[GameManager] Best Score ถูกรีเซ็ตแล้ว");
    }
}