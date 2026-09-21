using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource[] bgm;
    public AudioSource[] BGM { get { return bgm; } }

    [SerializeField]
    private AudioMixer mixer;

    public static AudioManager instance;

    void Awake()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }

    public void PlayBGM(int i)
    {
        StopAllBGM();
        if (i < bgm.Length)
        {
            bgm[i].Play();
        }
    }

    
    public void AdjustMasterVolume(float volume)
    {
        mixer.SetFloat("master", volume);
        PlayerPrefs.SetFloat("master", volume);
        PlayerPrefs.Save();
    }

    public float LoadCurrentMasterVolume()
    {
        return PlayerPrefs.GetFloat("master", 0f);
    }
}
