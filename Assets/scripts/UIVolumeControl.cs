using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum VolumeType { Music, SFX, Dialogue }

[RequireComponent(typeof(Slider))]
public class UIVolumeControl : MonoBehaviour
{
    public VolumeType volumeType;
    Slider slider;

    IEnumerator Start()
    {
        slider = GetComponent<Slider>();

        // Wait one frame so the persistent AudioSettings initializes
        yield return null;

        var mgr = AudioSettings.instance;

        // Initialize the slider value
        float startValue = 0f;
        switch (volumeType)
        {
            case VolumeType.Music: startValue = mgr.GetMusicVolume(); break;
            case VolumeType.SFX: startValue = mgr.GetSFXVolume(); break;
            case VolumeType.Dialogue: startValue = mgr.GetDialogueVolume(); break;
        }

        slider.SetValueWithoutNotify(startValue);

        // Hook change events
        slider.onValueChanged.AddListener((v) =>
        {
            switch (volumeType)
            {
                case VolumeType.Music: mgr.ApplyMusicVolume(v); break;
                case VolumeType.SFX: mgr.ApplySFXVolume(v); break;
                case VolumeType.Dialogue: mgr.ApplyDialogueVolume(v); break;
            }
        });
    }
}
