using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Item Prefabs (ลาก Prefab มาใส่ตามลำดับ)")]
    public GameObject rockPrefab;
    public GameObject starPrefab;
    public GameObject diamondPrefab;
    public GameObject bombPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.75f;
    public float minX = -16f;
    public float maxX = 19f;
    public float spawnY = 20f;

    [Header("Combo Settings")]
    public int comboToTriggerRain = 5;
    public float diamondRainDuration = 10f;
    public float diamondRainSpawnInterval = 0.75f;
    public float postRainGracePeriod = 2.5f;
    private int comboCount = 0;
    private bool diamondRainActive = false;
    private bool inPostRainGrace = false;
    private float normalSpawnInterval;

    [Header("Life Settings")]
    public float maxHealth = 100f;
    public float lifeLossPerMiss = 20f;
    private float currentHealth;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text comboText;
    public TMP_Text countdownText;
    public GameObject gameOverPanel;

    [Header("Game Over UI - Score Summary")]
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;

    [Header("Pause Settings")]
    public GameObject pausePanel;
    private bool isPaused = false;

    private const string BestScoreKey = "BestScore";

    private float timer;
    private float score = 0f;
    private bool isGameOver = false;

    void Awake()
    {
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

        UpdateScoreUI();
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

        UpdateComboUI();
    }

    public void ItemMissed(FallingItem.ItemType type)
    {
        if (isGameOver) return;

        comboCount = 0;
        UpdateComboUI();

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
            UpdateCountdownUI(remaining);
            yield return null;
            remaining -= Time.deltaTime;
        }

        UpdateCountdownUI(0f);
        diamondRainActive = false;

        spawnInterval = normalSpawnInterval;

        comboCount = 0;
        UpdateComboUI();

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
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateLivesUI();

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
        UpdateCountdownUI(0f);

        float previousBest = PlayerPrefs.GetFloat(BestScoreKey, 0f);

        bool isNewBest = score > previousBest;
        float bestScore = isNewBest ? score : previousBest;

        if (isNewBest)
        {
            PlayerPrefs.SetFloat(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("0.0");

        if (bestScoreText != null)
            bestScoreText.text = "Best Score: " + bestScore.ToString("0.0");

        Time.timeScale = 0f;
    }

    public void PauseGame()
    {
        if (isGameOver || isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (isGameOver || !isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
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

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [ContextMenu("Reset Best Score")]
    public void ResetBestScore()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
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