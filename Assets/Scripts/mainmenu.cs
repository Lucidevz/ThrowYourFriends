using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainmenu : MonoBehaviour
{
    private PlayerInput mainMenuInput;

    [Header("Menu Screens")]
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas optionsMenu;

    [Header("Individual Main Menu UI Elements")]
    [SerializeField]
    private Animator playButtonAnim;
    [SerializeField]
    private Animator optionsButtonAnim;
    [SerializeField]
    private Animator tutorialButtonAnim;
    [SerializeField]
    private Animator trainingButtonAnim;
    [SerializeField]
    private Animator exitButtonAnim;
    [SerializeField]
    private Image blackOverlay;

    [Header("Individual Options Menu UI Elements")]
    [SerializeField]
    private TextMeshProUGUI volumeText;
    [SerializeField]
    private Slider volumeSlider;
    [SerializeField]
    private TextMeshProUGUI soundText;
    [SerializeField]
    private Toggle soundToggle;
    [SerializeField]
    private TextMeshProUGUI musicText;
    [SerializeField]
    private Toggle musicToggle;
    [SerializeField]
    private TextMeshProUGUI screenResText;
    [SerializeField]
    private TextMeshProUGUI screenResDisplayText;

    private enum ScreenResolutions {
        _640x360,
        _1280x720,
        _1920x1080,
        _3840x2160,
        fullScreen,
    }

    private ScreenResolutions ScreenRes;

    [SerializeField]
    private Animator backButtonAnim;

    [SerializeField]
    private SaveData optionsSettings;

    [SerializeField]
    private AudioSource clickSound;
    [SerializeField]
    private MusicLooper music;

    private enum UIScreens {
        mainMenuScreen,
        optionsMenuScreen
    }

    private UIScreens currentScreen;

    private enum MainMenuButtonsToPress {
        play,
        options,
        tutorial,
        training,
        quit
    }

    private enum OptionMenuButtonsToPress {
        volumeSlider,
        soundToggle,
        musicToggle,
        screenRes,
        back
    }

    private MainMenuButtonsToPress currentMenuButton;
    private OptionMenuButtonsToPress currentOptionsButton;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        mainMenuInput = new PlayerInput();

        mainMenuInput.Enable();
        currentScreen = UIScreens.mainMenuScreen;
        currentMenuButton = MainMenuButtonsToPress.play;
        SetMainMenuButtonAnimations(true, false, false, false, false);

        optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
        FadeOutBlackScreen();
    }

    private void Update() {
        if (currentScreen == UIScreens.mainMenuScreen) {
            switch (currentMenuButton) {
                case (MainMenuButtonsToPress.play):
                    SetMainMenuButtonAnimations(true, false, false, false, false);
                    break;
                case (MainMenuButtonsToPress.options):
                    SetMainMenuButtonAnimations(false, true, false, false, false);
                    break;
                case (MainMenuButtonsToPress.tutorial):
                    SetMainMenuButtonAnimations(false, false, true, false, false);
                    break;
                case (MainMenuButtonsToPress.training):
                    SetMainMenuButtonAnimations(false, false, false, true, false);
                    break;
                case (MainMenuButtonsToPress.quit):
                    SetMainMenuButtonAnimations(false, false, false, false, true);
                    break;
            }
        } else if (currentScreen == UIScreens.optionsMenuScreen) {
            switch (currentOptionsButton) {
                case (OptionMenuButtonsToPress.volumeSlider):
                    SetOptionsMenuButtonAnimations(true, false, false, false, false);
                    break;
                case (OptionMenuButtonsToPress.soundToggle):
                    SetOptionsMenuButtonAnimations(false, true, false, false, false);
                    break;
                case (OptionMenuButtonsToPress.musicToggle):
                    SetOptionsMenuButtonAnimations(false, false, true, false, false);
                    break;
                case (OptionMenuButtonsToPress.screenRes):
                    SetOptionsMenuButtonAnimations(false, false, false, true, false);
                    break;
                case (OptionMenuButtonsToPress.back):
                    SetOptionsMenuButtonAnimations(false, false, false, false, true);
                    break;
            }
        }
    }

    void SetMainMenuButtonAnimations(bool playHighlighted, bool optionsHighlighted, bool tutoriallighted, bool trainginHighlighted, bool exitHighlighted) {
        playButtonAnim.SetBool("Highlighted", playHighlighted);
        optionsButtonAnim.SetBool("Highlighted", optionsHighlighted);
        tutorialButtonAnim.SetBool("Highlighted", tutoriallighted);
        trainingButtonAnim.SetBool("Highlighted", trainginHighlighted);
        exitButtonAnim.SetBool("Highlighted", exitHighlighted);
    }

    void SetOptionsMenuButtonAnimations(bool volumeHighlighted, bool soundHighlighted, bool musicHighlighted, bool screenResHighlighted, bool backHighlighted) {
        if (volumeHighlighted) {
            volumeText.color = Color.white;
        } else {
            volumeText.color = Color.black;
        }
        if (soundHighlighted) {
            soundText.color = Color.white;
        } else {
            soundText.color = Color.black;
        }
        if (musicHighlighted) {
            musicText.color = Color.white;
        } else {
            musicText.color = Color.black;
        }
        if (screenResHighlighted) {
            screenResText.color = Color.white;
        } else {
            screenResText.color = Color.black;
        }
        backButtonAnim.SetBool("Highlighted", backHighlighted);
    }

    public void MoveDown(InputAction.CallbackContext context) {
        if (context.performed) {
            if(currentScreen == UIScreens.mainMenuScreen) {
                if (currentMenuButton != MainMenuButtonsToPress.quit) {
                    currentMenuButton++;
                }
            } else if(currentScreen == UIScreens.optionsMenuScreen) {
                if (currentOptionsButton != OptionMenuButtonsToPress.back) {
                    currentOptionsButton++;
                }
            }
        }
    }

    public void MoveUp(InputAction.CallbackContext context) {
        if (context.performed) {
            if (currentScreen == UIScreens.mainMenuScreen) {
                if (currentMenuButton != MainMenuButtonsToPress.play) {
                    currentMenuButton--;
                }
            } else if (currentScreen == UIScreens.optionsMenuScreen) {
                if (currentOptionsButton != OptionMenuButtonsToPress.volumeSlider) {
                    currentOptionsButton--;
                }
            }
        }
    }

    public void MoveLeft(InputAction.CallbackContext context) {
        if (context.performed) {
             if (currentScreen == UIScreens.optionsMenuScreen) {
                switch (currentOptionsButton) {
                    case (OptionMenuButtonsToPress.volumeSlider):
                        clickSound.Play();
                        volumeSlider.value -= 0.05f;
                        optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                        break;
                    case (OptionMenuButtonsToPress.screenRes):
                        clickSound.Play();
                        int screenResIndex = (int)ScreenRes;
                        screenResIndex--;
                        if(screenResIndex < 0) {
                            screenResIndex = 4;
                        }
                        ScreenRes = (ScreenResolutions)screenResIndex;
                        SetScreenResolutionText();
                        optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                        break;
                }
             }
        }
    }

    public void MoveRight(InputAction.CallbackContext context) {
        if (context.performed) {
            if (currentScreen == UIScreens.optionsMenuScreen) {
                switch (currentOptionsButton) {
                    case (OptionMenuButtonsToPress.volumeSlider):
                        clickSound.Play();
                        volumeSlider.value += 0.05f;
                        optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                        break;
                    case (OptionMenuButtonsToPress.screenRes):
                        clickSound.Play();
                        int screenResIndex = (int)ScreenRes;
                        screenResIndex++;
                        if (screenResIndex > 4) {
                            screenResIndex = 0;
                        }
                        ScreenRes = (ScreenResolutions)screenResIndex;
                        SetScreenResolutionText();
                        optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                        break;
                }
            }
        }
    }

    public void SelectButton(InputAction.CallbackContext context) {
        if (context.performed) {
            clickSound.Play();
            if (currentScreen == UIScreens.mainMenuScreen) {
                switch (currentMenuButton) {
                    case (MainMenuButtonsToPress.play):
                        PlayGame();
                        break;
                    case (MainMenuButtonsToPress.options):
                        LoadOptions();
                        break;
                    case (MainMenuButtonsToPress.tutorial):
                        LoadTutorial();
                        break;
                    case (MainMenuButtonsToPress.training):
                        LoadTraining();
                        break;
                    case (MainMenuButtonsToPress.quit):
                        QuitGame();
                        break;
                }
            } else if (currentScreen == UIScreens.optionsMenuScreen) {
                if (currentScreen == UIScreens.optionsMenuScreen) {
                    switch (currentOptionsButton) {
                        case (OptionMenuButtonsToPress.soundToggle):
                            soundToggle.isOn = !soundToggle.isOn;
                            optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                            break;
                        case (OptionMenuButtonsToPress.musicToggle):
                            musicToggle.isOn = !musicToggle.isOn;
                            optionsSettings.UpdateObjectsLive(volumeSlider.value, soundToggle.isOn, musicToggle.isOn);
                            break;
                        case (OptionMenuButtonsToPress.screenRes):
                            SetScreenResolution();
                            break;
                        case (OptionMenuButtonsToPress.back):
                            ReturnToMenu();
                            break;
                    }
                }
            }
        }
    }

    public void Back(InputAction.CallbackContext context) {
        if (context.performed) {
            if(currentScreen == UIScreens.optionsMenuScreen) {
                ReturnToMenu();
            }
        }
    }


    public void PlayGame()
    {
        StartCoroutine(LoadLevel(1));
    }

    public void LoadOptions() {
        mainMenu.enabled = false;
        optionsMenu.enabled = true;
        currentScreen = UIScreens.optionsMenuScreen;
        currentOptionsButton = OptionMenuButtonsToPress.back;
    }

    public void SetScreenResolution() {
        switch (ScreenRes) {
            case (ScreenResolutions._640x360):
                Screen.SetResolution(640, 360, false);
                break;
            case (ScreenResolutions._1280x720):
                Screen.SetResolution(1280, 720, false);
                break;
            case (ScreenResolutions._1920x1080):
                Screen.SetResolution(1920, 1080, false);
                break;
            case (ScreenResolutions._3840x2160):
                Screen.SetResolution(3840, 2160, false);
                break;
            case (ScreenResolutions.fullScreen):
                Screen.SetResolution(1920, 1080, true);
                break;
        }
    }

    public void SetScreenResolutionText() {
        switch (ScreenRes) {
            case (ScreenResolutions._640x360):
                screenResDisplayText.text = "640x360";
                break;
            case (ScreenResolutions._1280x720):
                screenResDisplayText.text = "1280x720";
                break;
            case (ScreenResolutions._1920x1080):
                screenResDisplayText.text = "1920x1080";
                break;
            case (ScreenResolutions._3840x2160):
                screenResDisplayText.text = "3840x2160";
                break;
            case (ScreenResolutions.fullScreen):
                screenResDisplayText.text = "FullScreen";
                break;
        }
    }

    void SaveOptionPreferences() {
        PlayerPrefs.SetFloat("VolumeValue", volumeSlider.value);
        if (soundToggle.isOn) {
            PlayerPrefs.SetInt("SoundOn", 1);
        } else {
            PlayerPrefs.SetInt("SoundOn", 0);
        }
        if (musicToggle.isOn) {
            PlayerPrefs.SetInt("MusicOn", 1);
        } else {
            PlayerPrefs.SetInt("MusicOn", 0);
        }
        PlayerPrefs.SetInt("ScreenResolution", (int)ScreenRes);
    }

        public void ReturnToMenu() {
        SaveOptionPreferences();
        mainMenu.enabled = true;
        optionsMenu.enabled = false;
        currentScreen = UIScreens.mainMenuScreen;
        currentMenuButton = MainMenuButtonsToPress.options;
        SetMainMenuButtonAnimations(false, true, false, false, false);
    }

    public void LoadTutorial() {
        StartCoroutine(LoadLevel(2));
    }

    public void LoadTraining() {
        StartCoroutine(LoadLevel(3));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadLevel(int index) {
        SaveOptionPreferences();
        music.FadeOutMusic(0.75f);
        FadeInBlackScreen();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(index);
    }

    public void FadeInBlackScreen() {
        blackOverlay.CrossFadeColor(Color.blue, 1f, false, true);
    }

    public void FadeOutBlackScreen() {
        blackOverlay.CrossFadeColor(Color.clear, 1f, false, true);
    }
}
