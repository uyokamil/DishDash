// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicSliderValue;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxSliderValue;

    [SerializeField] private GameObject settingsPanel;

    private bool bIsOpen;

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(value => SetMusicVolume(value));
        sfxSlider.onValueChanged.AddListener(value => SetSFXVolume(value));

        mixer.GetFloat("MusicVolume", out float musicVolume);
        musicSlider.value = Mathf.Pow(10, musicVolume / 20);

        mixer.GetFloat("SFXVolume", out float sfxVolume);
        sfxSlider.value = Mathf.Pow(10, sfxVolume / 20);
    }

    void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        musicSliderValue.text = Mathf.RoundToInt(value * 100) + "%";
    }
    void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        sfxSliderValue.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void OpenSettings()
    {
        if(bIsOpen)
        {
            CloseSettings();
            return;
        }

        bIsOpen = true;

        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        bIsOpen = false;

        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
