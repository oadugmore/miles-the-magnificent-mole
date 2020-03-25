using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using System;
using GameSparks.Api.Requests;
using GameSparks.Core;
using System.Collections.Generic;

public enum MenuState
{
    Main, Settings, Tutorial, About, Leaderboard, RateReminder
}

public class MenuSceneManager : MonoBehaviour
{
    //public Button playButton;
    public CanvasGroup tutorialCanvasGroup;
    public Toggle keyboardToggle;
    public Toggle touchToggle;
    public Toggle easyModeToggle;
    public Toggle shareScoreToggle;
    public Toggle musicToggle;
    public Toggle effectsToggle;
    public Image loadingScreen;
    public Button clearPlayerPrefsButton;
    public InputField usernameBox;
    public Text versionText;
    public Animator leaderboardAnim;
    public Animator easyToggleAnim;
    public Animator rateReminderAnim;
    public Animator privacyAnim;
    public Toggle reminderDontAskAgainToggle;
    public AsyncOperation async;

    private MenuState menuPhase;
    public MenuState MenuPhase
    {
        get { return menuPhase; }
        private set { menuPhase = value; }
    }

    private bool init;
    private Action _enterAction;
    private LeaderboardController _leaderboard;
    private MobileControlRig _controlRig;
    private CanvasGroup menuCanvasGroup;
    private TutorialController _tutorialController;
    private string username = string.Empty;
    private string _userId = string.Empty;
    private int score = 0;
    private int usernameLoadAttempts = 0;
    private Animator loadingAnimator;
    private Animator _anim;
    private bool proceedToGame;
    private const string k_uwpStoreListing = "ms-windows-store://review/?ProductId=9nblggh4rdlz";
    private const string k_androidStoreListing = "market://details?id=com.oadugmore.milesthemole";
    private const string k_iosId = "1141116446";
    //private const string k_iosStoreListing = "itms-apps://itunes.apple.com/app/id" + k_iosId;
    private const string k_iosStoreListing = "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=" + k_iosId;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        loadingAnimator = loadingScreen.GetComponent<Animator>();
        menuCanvasGroup = GetComponent<CanvasGroup>();
        _controlRig = FindObjectOfType<MobileControlRig>();
        _tutorialController = FindObjectOfType<TutorialController>();
        _leaderboard = FindObjectOfType<LeaderboardController>();
    }

    void Start()
    {
        init = true;
        versionText.text = Application.version;
        easyModeToggle.isOn = SettingsManager.easyMode;
        EasyModeToggleSelectionChanged();
        if (SettingsManager.touchControlsEnabled)
        {
            touchToggle.isOn = true;
        }
        else
        {
            keyboardToggle.isOn = true;
        }
        musicToggle.isOn = SettingsManager.musicOn;
        effectsToggle.isOn = SettingsManager.effectsOn;

        init = false;

        //foreach (Text t in leaderboardNamesHost.GetComponentsInChildren<Text>())
        //{
        //    leaderboardNameTexts.Add(t);
        //}

        if (!PlayerPrefs.HasKey("SeenPrivacyPolicy"))
        {
            PlayerPrefs.SetInt("SeenPrivacyPolicy", 1);
            //menuCanvasGroup.interactable = false;
            Invoke("ShowPrivacyDialogue", 0.2f);
            //ShowPrivacyDialogue();
        }

        if (!SettingsManager.rateCompleted && SettingsManager.loadCount >= SettingsManager.nextReminderLoadCount)
        {
            ShowRateReminder();
        }

        if (GS.Authenticated)
        {
            OnGameSparksReady();
        }

#if UNITY_EDITOR
        var group = clearPlayerPrefsButton.GetComponent<CanvasGroup>();
        group.interactable = true;
        group.alpha = 1;
        group.blocksRaycasts = true;
#endif
    }

    #region GameSparks

    public void OnGameSparksReady()
    {
        if (this == null) return;
        _leaderboard.Refresh();
        LoadUsername();
        LoadMyScore();
    }

    int _loadMyScoreAttempts = 0;
    private void LoadMyScore()
    {
        new LeaderboardDataRequest_SCORE_LEADERBOARD().SetDurable(true).SetEntryCount(int.MaxValue).Send(response =>
        {
            if (this == null) return;
            if (!response.HasErrors)
            {
                foreach (var entry in response.Data_SCORE_LEADERBOARD)
                {
                    //string playerName = entry.UserName;
                    string id = entry.UserId;
                    //string playerScore = entry.SCORE.ToString();
                    if (id.Equals(_userId))
                    {
                        score = (int)entry.SCORE;
                        _leaderboard.myScoreText.text = "My score: " + score;
                        Debug.Log("My score: " + score);
                        return;
                    }
                }
                _leaderboard.myScoreText.text = "My score: 0";
                Debug.Log("Couldn't find player's score!");
            }
            else if (_loadMyScoreAttempts < 3) //and response had errors
                Invoke("LoadMyScore", 1f);
                //leaderboard.myScoreText.text = "My score: 0";
        });
    } //end LoadMyScore method

    private void LoadUsername()
    {
        if (!string.IsNullOrEmpty(username)) return;

        usernameLoadAttempts++;
        new AccountDetailsRequest().SetDurable(true).Send((response) =>
        {
            if (this == null) return;
            if (!response.HasErrors)
            {
                if (!string.IsNullOrEmpty(response.DisplayName))
                {
                    username = response.DisplayName;
                    _userId = response.UserId;
                    Debug.Log("Player's username is " + username);
                    usernameBox.text = username;
                }
                else
                {
                    Debug.Log("Player does not have a username!");
                }
            }
            else if (usernameLoadAttempts < 3) //and response had errors
            {
                Invoke("LoadUsername", 1f);
            }
            else
            {
                Debug.Log("Failed to find player's username!");
            }
        });
    }

    #endregion

    #region Loading screen

    public void FadeOutLoadingScreen()
    {
        _anim.SetTrigger("ReturnFromGame");
        loadingAnimator.SetTrigger("FadeOut");
    }

    public void FadeInLoadingScreen()
    {
        _anim.SetTrigger("PlayGame");
        loadingAnimator.SetTrigger("FadeIn");
    }

    private IEnumerator WaitToCompleteLoad()
    {
        while (true)
        {
            if (async.progress >= 0.9f)
            {
                break;
            }
            yield return null;
        }
        CompleteSceneLoad();
    }

    public void FadeInComplete()
    {
        StartCoroutine(WaitToCompleteLoad());
    }

    public void CompleteSceneLoad()
    {
        async.allowSceneActivation = true;
    }

    #endregion

    #region Event handlers

    public void RefreshLeaderboardButtonClicked()
    {
        _leaderboard.Refresh();
    }

    public void ShowLeaderboardButtonClicked()
    {
        leaderboardAnim.SetTrigger("ShowResults");
        GameController.backButtonLeavesApp = false;
        MenuPhase = MenuState.Leaderboard;
    }

    public void HideLeaderboardButtonClicked()
    {
        leaderboardAnim.SetTrigger("HideResults");
        GameController.backButtonLeavesApp = true;
        MenuPhase = MenuState.Main;
    }

    public void RateReminderYesButtonClicked()
    {
        HideRateReminder();
        SettingsManager.rateCompleted = true;
        Invoke("RateButtonClicked", 1f);
    }

    public void RateReminderNoButtonClicked()
    {
        HideRateReminder();
        if (reminderDontAskAgainToggle.isOn)
        {
            SettingsManager.rateCompleted = true;
        }
        else
        {
            SettingsManager.nextReminderLoadCount += 5;
        }
    }

    //public void ShareScoreToggleChanged()
    //{
    //    bool shareScore = shareScoreToggle.isOn;
    //    SettingsManager.uploadScore = shareScore;
    //    if (shareScore)
    //    {
    //        new GameSparks.Api.Requests.LogEventRequest().SetEventKey("LOAD_SCORE").Send((response) =>
    //        {
    //            if (!response.HasErrors)
    //            {
    //                GSData data = response.ScriptData.GetGSData("player_Data");
    //                int score = int.Parse(data.GetString("playerScore"));
    //                Debug.Log(string.Format("Received Player Data From GameSparks. Score: {0}", score));
    //                new GameSparks.Api.Requests.LogEventRequest().SetEventKey("SUBMIT_SCORE").SetEventAttribute("SCORE", score).Send((postResponse) =>
    //                {
    //                    if (!postResponse.HasErrors)
    //                    {
    //                        Debug.Log("Score Posted Successfully...");
    //                    }
    //                    else
    //                    {
    //                        Debug.Log("Error Posting Score...");
    //                    }
    //                });
    //            }
    //            else //LOAD_SCORE had errors
    //            {
    //                Debug.Log("Error Loading Player Data...");
    //            }
    //        });
    //    }

    //    else //don't share score
    //    {

    //        //new GetLeaderboardEntriesRequest().SetLeaderboards(new System.Collections.Generic.List<string> { "SCORE_LEADERBOARD" }).Send((response) =>
    //        //   {
    //        //       if (!response.HasErrors)
    //        //       {

    //        //       }
    //        //   });
    //    }
    //}

    public void ShowAboutButtonClicked()
    {
        MenuPhase = MenuState.About;
        _anim.SetTrigger("ShowAboutPopup");
    }

    public void HideAboutButtonClicked()
    {
        MenuPhase = MenuState.Settings;
        _anim.SetTrigger("HideAboutPopup");
    }

    public void EasyModeToggleSelectionChanged()
    {
        if (!init)
        {
            SettingsManager.easyMode = easyModeToggle.isOn;
            string trigger = easyModeToggle.isOn ? "TurnOn" : "TurnOff";
            easyToggleAnim.SetTrigger(trigger);
        }
        else if (!easyModeToggle.isOn)
        {
            easyToggleAnim.SetTrigger("TurnOffNoAnim");
        }
    }

    public void PlayButtonClicked()
    {
        if (!PlayerPrefs.HasKey("SeenTutorial"))
        {
            StartTutorial(true);
        }
        else
        {
            GameController.current.LoadGame(true);
        }
    }

    public void BackRequested()
    {
        switch (MenuPhase)
        {
            case MenuState.Main:
                break;
            case MenuState.Leaderboard:
                HideLeaderboardButtonClicked();
                break;
            case MenuState.RateReminder:
                RateReminderNoButtonClicked();
                break;
            case MenuState.Settings:
                CloseSettingsButtonClicked();
                break;
            case MenuState.Tutorial:
                CloseTutorialButtonClicked();
                break;
            case MenuState.About:
                HideAboutButtonClicked();
                break;
            default:
                break;
        }
    }

    public void EnterRequested()
    {
        if (_enterAction != null)
        {
            _enterAction();
        }
    }

    public void StartTutorialButtonClicked()
    {
        StartTutorial(false);
    }

    public void CloseTutorialButtonClicked()
    {
        CloseTutorial();
    }

    public void CreativeCommonsButtonClicked()
    {
        Application.OpenURL("http://creativecommons.org/licenses/by/3.0/");
    }

    public void RainAttribButtonClicked()
    {
        Application.OpenURL("http://soundbible.com/2011-Rain-Background.html");
    }

    public void PrivacyButtonClicked()
    {
        Application.OpenURL("https://oadugmore.wordpress.com/projects/miles-privacy/");
    }

    public void MusicAttribButtonClicked()
    {
        Application.OpenURL("https://soundcloud.com/bitkrushofficial");
    }

    public void SettingsButtonClicked()
    {
        _anim.SetTrigger("OpenSettings");
        GameController.backButtonLeavesApp = false;
        MenuPhase = MenuState.Settings;

        //tutorialController.ShowTutorial(); //testing purposes only
        //settingsCanvasGroup.GetComponent<Animator>().SetTrigger("Open");
        //SettingsManager.touchControlsEnabled = !SettingsManager.touchControlsEnabled;
    }

    public void CloseSettingsButtonClicked()
    {
        GameController.backButtonLeavesApp = true;
        _anim.SetTrigger("OpenMenu");
        MenuPhase = MenuState.Main;
    }

    public void ClearPlayerPrefsButtonClicked()
    {
        PlayerPrefs.DeleteAll();
    }

    public void ControlModeToggleSelectionChanged()
    {
        if (keyboardToggle.isOn)
        {
            SettingsManager.touchControlsEnabled = false;
        }
        else
        {
            SettingsManager.touchControlsEnabled = true;
        }
    }

    public void AudioCheckboxChanged()
    {
        if (!init)
        {
            SettingsManager.musicOn = musicToggle.isOn;
            SettingsManager.effectsOn = effectsToggle.isOn;
        }
    }

    public void RateButtonClicked()
    {
#if WINDOWS_UWP
        UnityEngine.WSA.Launcher.LaunchUri(k_uwpStoreListing, true);
#elif UNITY_ANDROID
        Application.OpenURL(k_androidStoreListing);
#elif UNITY_IOS
        //Application.OpenURL((double.Parse(UnityEngine.iOS.Device.systemVersion) >= 7.0) ? k_ios7StoreListing : k_iosStoreListing);
        Application.OpenURL(k_iosStoreListing);
#else
        UnityEngine.Application.OpenURL("www.example.org");
#endif
    }

    #endregion

    public void StartTutorial(bool proceedToGame)
    {
        PlayerPrefs.SetInt("SeenTutorial", 1);
        CrossPlatformInputManager.SwitchActiveInputMethod(CrossPlatformInputManager.ActiveInputMethod.Touch);
        this.proceedToGame = proceedToGame;
        _tutorialController.ShowTutorial();
        GameController.backButtonLeavesApp = false;
        MenuPhase = MenuState.Tutorial;
        tutorialCanvasGroup.interactable = true;
        if (this.proceedToGame)
        {
            _anim.SetTrigger("OpenTutorialFirstTime");
        }
        else
        {
            _anim.SetTrigger("OpenTutorial");
        }
        _controlRig.EnableControlRig(true);
    }

    private void CloseTutorial()
    {
        CrossPlatformInputManager.SwitchActiveInputMethod(SettingsManager.touchControlsEnabled ? CrossPlatformInputManager.ActiveInputMethod.Touch : CrossPlatformInputManager.ActiveInputMethod.Hardware);
        if (proceedToGame)
        {
            //MenuPhase = MenuPhase.Main;
            GameController.current.LoadGame(true);
            //PlayButtonClicked();
        }
        else
        {
            _tutorialController.HideTutorial();
            _anim.SetTrigger("CloseTutorial");
            MenuPhase = MenuState.Settings;
        }
    }

    private void ShowRateReminder()
    {
        rateReminderAnim.SetTrigger("ShowPopup");
        //GameController.current.WaitForEnterKey(RateReminderYesButtonClicked);
        _enterAction = RateReminderYesButtonClicked;
        GameController.backButtonLeavesApp = false;
        MenuPhase = MenuState.RateReminder;
    }

    private void HideRateReminder()
    {
        rateReminderAnim.SetTrigger("HidePopup");
        //GameController.current.StopWaitForEnterKeyCoroutine();
        _enterAction = null;
        GameController.backButtonLeavesApp = true;
        MenuPhase = MenuState.Main;
    }

    private void ShowPrivacyDialogue()
    {
        privacyAnim.SetTrigger("ShowPrivacyDialogue");
        //GameController.current.WaitForEnterKey(HidePrivacyDialogue);
        _enterAction = HidePrivacyDialogue;
    }

    public void HidePrivacyDialogue()
    {
        //Debug.Log("HidePrivacyDialogue method invoked. MenuSceneManager = " + this);
        if (this == null)
        {
            Debug.LogWarning("HidePrivacyDialogue was mistakenly called. Ignoring.");
            return;
        }
        privacyAnim.SetTrigger("HidePrivacyDialogue");
        //GameController.current.StopWaitForEnterKeyCoroutine();
        _enterAction = null;
        //Debug.Log("HidePrivacyDialogue method finished");
    }

    public void DeactivateTutorialCanvas()
    {
        tutorialCanvasGroup.interactable = false;
    }

    public void ChangeUsername()
    {
        string newUsername = usernameBox.text.Trim();
        if (!string.IsNullOrEmpty(newUsername) /*&& newUsername.Length <= 20*/)
        {
            new ChangeUserDetailsRequest().SetDisplayName(newUsername).Send((response) =>
            {
                username = newUsername;
                Debug.Log("Changed username successfully!");
                _leaderboard.Refresh();
            });
        }
        else
        {
            usernameBox.text = username;
        }
    }

}
