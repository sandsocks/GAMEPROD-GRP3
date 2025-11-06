using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class OptionsPanel : MonoBehaviour
{
    Resolution[] resolutions;
    public TMP_Dropdown resolutionDropdown;

    [SerializeField] private Slider BGMSlider;
    [SerializeField] public AudioMixer audioMixer;
    [SerializeField] public Toggle Fullscreen;

    private void Start()
    {
        InitializeResolutionDropdown();
        SetVolumeSliderValue();

        Fullscreen.isOn = Screen.fullScreen;
        Fullscreen.onValueChanged.AddListener(SetFullscreen);
    }

    private void InitializeResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetVolume()
    {
        float volume = BGMSlider.value;
        audioMixer.SetFloat("volume", volume);
    }

    private void SetVolumeSliderValue()
    {
        float defaultVolume;
        bool result = audioMixer.GetFloat("volume", out defaultVolume);

        if (result)
        {
            BGMSlider.value = defaultVolume;
        }
    }

    private void OnEnable()
    {
        SetVolumeSliderValue();
    }
}
