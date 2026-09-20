using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartNewGame()
    {
        Setting.fromSave = false;
        SceneManager.LoadScene("Load");
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
