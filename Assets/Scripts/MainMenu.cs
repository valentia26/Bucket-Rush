using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject adjustPanel;

    [SerializeField]
    private Slider volumeSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlayBGM(0);

        volumeSlider.value = AudioManager.instance.LoadCurrentMasterVolume();


        volumeSlider.onValueChanged.AddListener(AudioManager.instance.AdjustMasterVolume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartNewGame()
    {
        SceneManager.LoadScene("Loading");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void ShowHideAdjustPanel(bool flag)
    {
        adjustPanel.SetActive(flag);
    }

    public void SetVolume(float volume)
    {
        AudioManager.instance.AdjustMasterVolume(volume);
    }
}
