
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject defaultValuesPanel;

    [Header("Controles")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Botones")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button defaultValuesButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    [SerializeField] private Button defaultsYesButton;
    [SerializeField] private Button defaultsNoButton;

    [Header("Fundido")]
    [SerializeField] private float fadeDuration = 0.25f;

    private bool isPaused;
    private bool hasChanges;
    private float targetAlpha;

    private float savedMusicVolume;
    private float savedSFXVolume;
    private bool savedFullscreen;
    private int savedWidth;
    private int savedHeight;

    private List<Resolution> availableResolutions = new List<Resolution>();

    private int selectedWidth;
    private int selectedHeight;

    private void Start()
    {
        pausePanel.SetActive(false);
        confirmPanel.SetActive(false);
        defaultValuesPanel.SetActive(false);

        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;

        SetupResolutions();
        LoadSettings();
        ConnectButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }

        if (pauseCanvasGroup.alpha != targetAlpha)
        {
            pauseCanvasGroup.alpha = Mathf.MoveTowards(
                pauseCanvasGroup.alpha,
                targetAlpha,
                Time.unscaledDeltaTime / fadeDuration
            );

            if (pauseCanvasGroup.alpha == 0f && !isPaused)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1f;
            }

            if (pauseCanvasGroup.alpha == 1f && isPaused)
            {
                pauseCanvasGroup.interactable = true;
                pauseCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void HandleEscape()
    {
        if (!isPaused)
        {
            OpenMenu();
        }
        else if (confirmPanel.activeSelf)
        {
            confirmPanel.SetActive(false);
        }
        else if (defaultValuesPanel.activeSelf)
        {
            defaultValuesPanel.SetActive(false);
        }
        else
        {
            CloseMenuOrConfirm();
        }
    }

    private void ConnectButtons()
    {
        resumeButton.onClick.AddListener(CloseMenuOrConfirm);
        applyButton.onClick.AddListener(ApplySettings);
        defaultValuesButton.onClick.AddListener(ShowDefaultValues);
        exitButton.onClick.AddListener(QuitGame);

        confirmYesButton.onClick.AddListener(DiscardChanges);
        confirmNoButton.onClick.AddListener(CancelConfirmation);

        defaultsYesButton.onClick.AddListener(ResetDefaults);
        defaultsNoButton.onClick.AddListener(CancelDefaultValues);

        musicSlider.onValueChanged.AddListener(OnSettingsChanged);
        sfxSlider.onValueChanged.AddListener(OnSettingsChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnSettingsChanged);
    }

    private void OpenMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;

        targetAlpha = 1f;
    }

    private void CloseMenuOrConfirm()
    {
        if (hasChanges)
            confirmPanel.SetActive(true);
        else
            CloseMenu();
    }

    private void CloseMenu()
    {
        isPaused = false;
        targetAlpha = 0f;

        confirmPanel.SetActive(false);
        defaultValuesPanel.SetActive(false);

        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
    }

    private void SetupResolutions()
    {
        availableResolutions.Clear();
        resolutionDropdown.ClearOptions();

        Resolution[] systemResolutions = Screen.resolutions;
        List<string> options = new List<string>();

        foreach (Resolution resolution in systemResolutions)
        {
            bool alreadyAdded = false;

            foreach (Resolution savedResolution in availableResolutions)
            {
                if (savedResolution.width == resolution.width &&
                    savedResolution.height == resolution.height)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
            {
                availableResolutions.Add(resolution);
                options.Add(resolution.width + " x " + resolution.height);
            }
        }

        if (availableResolutions.Count == 0)
        {
            Resolution currentResolution = Screen.currentResolution;
            availableResolutions.Add(currentResolution);
            options.Add(currentResolution.width + " x " + currentResolution.height);
        }

        resolutionDropdown.AddOptions(options);
    }

    private void LoadSettings()
    {
        savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.width);
        savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.height);

        musicSlider.SetValueWithoutNotify(savedMusicVolume);
        sfxSlider.SetValueWithoutNotify(savedSFXVolume);
        fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

        int index = FindResolutionIndex(savedWidth, savedHeight);

        resolutionDropdown.SetValueWithoutNotify(index);
        resolutionDropdown.RefreshShownValue();

        selectedWidth = availableResolutions[index].width;
        selectedHeight = availableResolutions[index].height;

        if (GameAudio.Instance != null)
        {
            GameAudio.Instance.SetMusicVolume(savedMusicVolume);
            GameAudio.Instance.SetSFXVolume(savedSFXVolume);
        }

        Screen.SetResolution(savedWidth, savedHeight, savedFullscreen);
        hasChanges = false;
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == width &&
                availableResolutions[i].height == height)
            {
                return i;
            }
        }

        return 0;
    }

    private void OnSettingsChanged(float value)
    {
        hasChanges = true;
    }

    private void OnSettingsChanged(bool value)
    {
        hasChanges = true;
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        selectedWidth = availableResolutions[index].width;
        selectedHeight = availableResolutions[index].height;
        hasChanges = true;
    }

    private void ApplySettings()
    {
        savedMusicVolume = musicSlider.value;
        savedSFXVolume = sfxSlider.value;
        savedFullscreen = fullscreenToggle.isOn;
        savedWidth = selectedWidth;
        savedHeight = selectedHeight;

        if (GameAudio.Instance != null)
        {
            GameAudio.Instance.SetMusicVolume(savedMusicVolume);
            GameAudio.Instance.SetSFXVolume(savedSFXVolume);
            GameAudio.Instance.SaveAudioSettings();
        }

        Screen.SetResolution(savedWidth, savedHeight, savedFullscreen);

        PlayerPrefs.SetInt("Fullscreen", savedFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionWidth", savedWidth);
        PlayerPrefs.SetInt("ResolutionHeight", savedHeight);
        PlayerPrefs.Save();

        hasChanges = false;
        confirmPanel.SetActive(false);
        defaultValuesPanel.SetActive(false);
    }

    private void ShowDefaultValues()
    {
        defaultValuesPanel.SetActive(true);
    }

    private void ResetDefaults()
    {
        musicSlider.value = 0.7f;
        sfxSlider.value = 0.7f;
        fullscreenToggle.isOn = true;

        int index = FindResolutionIndex(1920, 1080);

        resolutionDropdown.value = index;
        resolutionDropdown.RefreshShownValue();

        selectedWidth = availableResolutions[index].width;
        selectedHeight = availableResolutions[index].height;

        hasChanges = true;
        defaultValuesPanel.SetActive(false);
    }

    private void DiscardChanges()
    {
        musicSlider.SetValueWithoutNotify(savedMusicVolume);
        sfxSlider.SetValueWithoutNotify(savedSFXVolume);
        fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

        int index = FindResolutionIndex(savedWidth, savedHeight);

        resolutionDropdown.SetValueWithoutNotify(index);
        resolutionDropdown.RefreshShownValue();

        selectedWidth = savedWidth;
        selectedHeight = savedHeight;

        hasChanges = false;
        CloseMenu();
    }

    private void CancelConfirmation()
    {
        confirmPanel.SetActive(false);
    }

    private void CancelDefaultValues()
    {
        defaultValuesPanel.SetActive(false);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
