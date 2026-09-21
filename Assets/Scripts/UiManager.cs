using UnityEngine;
using TMPro; // ถ้าใช้ Text ธรรมดาแทน TextMeshPro ให้เปลี่ยนเป็น using UnityEngine.UI; แล้วเปลี่ยนชนิดตัวแปร Text เป็น Text

// คลาสนี้รับหน้าที่แสดงผลข้อความ/UI ทั้งหมดในเกมแทน GameManager
// GameManager จะไม่ยุ่งกับ TMP_Text หรือ GameObject panel โดยตรงอีกต่อไป
// มีหน้าที่แค่เรียก UiManager.Instance.XxxUI(...) แล้วส่งค่าที่เปลี่ยนแปลงมาให้เท่านั้น
public class UiManager : MonoBehaviour
{
    public static UiManager Instance; // Singleton เผื่อให้ GameManager เรียกใช้ง่าย

    [Header("Score & Status")]
    public TMP_Text scoreText;         // แสดงคะแนน
    public TMP_Text livesText;         // (ไม่บังคับ) แสดงพลังชีวิต
    public TMP_Text comboText;         // (ไม่บังคับ) แสดงคอมโบปัจจุบัน
    public TMP_Text countdownText;     // (ไม่บังคับ) แสดงตัวนับถอยหลังฝนเพชร (เช่น "3", "2", "1")

    [Header("Game Over UI")]
    public GameObject gameOverPanel;   // (ไม่บังคับ) แผง Game Over เมื่อพลังชีวิตหมด
    public TMP_Text finalScoreText;    // ลาก UI Text (บน gameOverPanel) มาใส่เพื่อแสดงคะแนนที่ได้ในตานี้
    public TMP_Text bestScoreText;     // ลาก UI Text (บน gameOverPanel) มาใส่เพื่อแสดง Best Score ตลอดกาล

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
        // ตอนเริ่มเกม gameOverPanel ต้องซ่อนไว้ก่อนเสมอ
        HideGameOver();
    }

    public void UpdateScore(float score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("0.0"); // แสดงทศนิยม 1 ตำแหน่ง เช่น 0.5, 1.0, 10.0
    }

    public void UpdateLives(float currentHealth)
    {
        if (livesText != null)
            livesText.text = "HP: " + currentHealth.ToString("0");
    }

    public void UpdateCombo(int comboCount)
    {
        if (comboText != null)
            comboText.text = "Combo: " + comboCount;
    }

    public void UpdateCountdown(float secondsRemaining)
    {
        if (countdownText == null) return;

        if (secondsRemaining <= 0f)
        {
            countdownText.text = "";
        }
        else
        {
            // ปัดขึ้นให้ขึ้นเลขเต็ม เช่น 2.9 วิ ให้แสดง "3"
            countdownText.text = Mathf.CeilToInt(secondsRemaining).ToString();
        }
    }

    // เรียกตอนเกมจบ เพื่อโชว์ panel และใส่ข้อความคะแนน/best score
    public void ShowGameOver(float score, float bestScore, bool isNewBest)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("0.0");

        if (bestScoreText != null)
            bestScoreText.text = "Best Score: " + bestScore.ToString("0.0");
    }

    // เรียกตอนเริ่มเกม/รีสตาร์ท เพื่อซ่อน panel game over
    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}