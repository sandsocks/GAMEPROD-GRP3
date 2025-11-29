using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings instance;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Apply saved volumes on boot
            ApplyMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0f), save: false);
            ApplySFXVolume(PlayerPrefs.GetFloat("SFXVolume", 0f), save: false);
            ApplyDialogueVolume(PlayerPrefs.GetFloat("DialogueVolume", 0f), save: false);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // -------------------------
    // Volume Apply Functions
    // -------------------------

    public void ApplyMusicVolume(float value, bool save = true)
    {
        audioMixer.SetFloat("MusicVolume", value);
        if (save) PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void ApplySFXVolume(float value, bool save = true)
    {
        audioMixer.SetFloat("SFXVolume", value);
        if (save) PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void ApplyDialogueVolume(float value, bool save = true)
    {
        audioMixer.SetFloat("DialogueVolume", value);
        if (save) PlayerPrefs.SetFloat("DialogueVolume", value);
    }

    // -------------------------
    // Getters (for sliders)
    // -------------------------

    public float GetMusicVolume()
    {
        float v;
        if (audioMixer.GetFloat("MusicVolume", out v)) return v;
        return PlayerPrefs.GetFloat("MusicVolume", 0f);
    }

    public float GetSFXVolume()
    {
        float v;
        if (audioMixer.GetFloat("SFXVolume", out v)) return v;
        return PlayerPrefs.GetFloat("SFXVolume", 0f);
    }

    public float GetDialogueVolume()
    {
        float v;
        if (audioMixer.GetFloat("DialogueVolume", out v)) return v;
        return PlayerPrefs.GetFloat("DialogueVolume", 0f);
    }
}
