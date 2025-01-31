using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuOptions : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resDropdown;
    Resolution[] resolutions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currResIdx = 0;
        for (int i = 0; i < resolutions.Length; i++) {
            string opt = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(opt);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                currResIdx = i;
            }
        }

        resDropdown.AddOptions(options);
        resDropdown.value = currResIdx;
        resDropdown.RefreshShownValue();
    }

    public void SetVolume(float volume) {
        audioMixer.SetFloat("MasterVol", volume);
    }

    public void SetResolution(int resIdx) {
        Resolution resolution = resolutions[resIdx];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetFullscreen(bool wantsFull) {
        Screen.fullScreen = wantsFull;
    }
}
