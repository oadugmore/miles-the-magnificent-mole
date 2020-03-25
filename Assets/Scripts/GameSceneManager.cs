using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;
using GameSparks.Api.Requests;
using System;

public enum PauseMenuAction
{
    Quit, Restart
}

public class GameSceneManager : MonoBehaviour
{
    #region Properties

    public int geyserTriggerDistance = 20;
    public int geyserShortTriggerDistance = 15;
    public AudioController audioController;
    public Animator pauseMenuAnimator;
    public Animator resultsAnimator;
    public Animator scoreInfoPopup;
    public Animator pauseMenuActionConfirmationAnimator;
    public Image loadingScreen;
    //public Sprite playButtonSprite;
    //public Sprite pauseButtonSprite;
    //public Image pauseButtonCurrentImage;
    public Text usernameBoxText;
    //public Text leaderboardRankText;
    //public Text leaderboardNameText;
    //public Text leaderboardScoreText;
    public Button infoButton;
    //public Text myScoreText;
    //public Text leaderboardDataText;
    public Text distanceDisplay;
    public const int meterScale = 10;
    public AsyncOperation async;

    private Action _enterAction;
    private LeaderboardController _leaderboard;
    private CameraController _cameraController;
    private MobileControlRig _mobileControlRig;
    private WaterController _waterController;
    private DeactivateOnLeave _garbageCollector;
    private PlayerController _player;
    private RandomTerrainGenerator _generator;
    private bool _resultsShown = false;
    private Animator _loadingAnimator;
    private bool _showUsernamePopupOnLoad = false;
    private bool _easyMode;
    private PauseMenuAction _menuAction;

    private int _metersDug = 0;
    public int metersDug
    {
        get { return _metersDug; }
        private set
        {
            _metersDug = value;
            distanceDisplay.text = "Distance: " + metersDug + 'm';
        }
    }

    #endregion

    public void Start()
    {
        _loadingAnimator = loadingScreen.GetComponent<Animator>();
        _leaderboard = FindObjectOfType<LeaderboardController>();
        _mobileControlRig = FindObjectOfType<MobileControlRig>();
        _waterController = FindObjectOfType<WaterController>();
        _garbageCollector = FindObjectOfType<DeactivateOnLeave>();
        _player = FindObjectOfType<PlayerController>();
        _generator = FindObjectOfType<RandomTerrainGenerator>();
        _cameraController = FindObjectOfType<CameraController>();
        _easyMode = SettingsManager.easyMode;
        bool enableTouchControls = SettingsManager.touchControlsEnabled;
        _mobileControlRig.EnableControlRig(enableTouchControls);
        CrossPlatformInputManager.SwitchActiveInputMethod(enableTouchControls ? CrossPlatformInputManager.ActiveInputMethod.Touch : CrossPlatformInputManager.ActiveInputMethod.Hardware);
        audioController.FadeIn(1);
        if (!PlayerPrefs.HasKey("SeenScoreInfo") && _easyMode)
        {
            PlayerPrefs.SetInt("SeenScoreInfo", 1);
            ShowScoreInfoPopup();
        }
        _generator.GenerateChunk();
    }

    public void Update()
    {
        if (GameController.current.gameState == GameState.Playing)
        {
            int currentPosInMeters = (int)_player.transform.position.y / -meterScale;
            if (currentPosInMeters > metersDug)
            {
                metersDug = currentPosInMeters;
                _waterController.UpdateSpeed(true);
            }

            if (!_waterController.isChasing && metersDug > 0)
            {
                _waterController.BeginChasing(_easyMode);
            }

            if (!_generator.generatingChunk && _player.transform.position.y < _generator.generationNextTrigger)
            {
                _generator.GenerateChunk();
            }
        } //end if gamePhase == Playing
    }

    public void SetJetpackButtonActive(bool active)
    {

    }

    public void GameOver()
    {
        LoadResults(_easyMode);
        _waterController.SlowDown();
        _garbageCollector.StopDestroying();
        audioController.FadeOut(2, false);
        _cameraController.StopTracking();
    }

    public void ShowUsernamePopup()
    {
        if (this == null) return;
        resultsAnimator.SetTrigger("ShowUsernamePopup");
        //GameController.current.WaitForEnterKey(ConfirmUsernameButtonClicked);
        _enterAction = ConfirmUsernameButtonClicked;
    }

    public void HideUsernamePopup()
    {
        resultsAnimator.SetTrigger("HideUsernamePopup");
        //GameController.current.StopWaitForEnterKeyCoroutine();
        _enterAction = null;
    }

    private bool scoreInfoPopupIsActive = false;
    public void ShowScoreInfoPopup()
    {
        scoreInfoPopup.SetTrigger("ShowPopup");
        scoreInfoPopupIsActive = true;
        //GameController.current.WaitForEnterKey(HideScoreInfoPopup);
        _enterAction = HideScoreInfoPopup;
        //resultsAnimator.SetTrigger("ShowScoreInfoPopup");
    }

    public void HideScoreInfoPopup()
    {
        //Debug.Log("HideScoreInfoPopup method invoked. GameSceneManager = " + this);
        if (this == null)
        {
            Debug.LogWarning("HideScoreInfoPopup was mistakenly called. This is really unexpected lol. Ignoring.");
            return;
        }
        scoreInfoPopup.SetTrigger("HidePopup");
        scoreInfoPopupIsActive = false;
        //GameController.current.StopWaitForEnterKeyCoroutine();
        _enterAction = null;
        //Debug.Log("HideScoreInfoPopup method completed");
        //resultsAnimator.SetTrigger("HideScoreInfoPopup");
    }

    private void ShowResultsScreen()
    {
        _resultsShown = true;
        resultsAnimator.SetTrigger("ShowResults");
        audioController.FadeOut(2, false);
        if (!_easyMode)
        {
            var group = infoButton.GetComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
        if (_showUsernamePopupOnLoad)
        {
            ShowUsernamePopup();
        }
    }

    public void LoadResults(bool easyMode)
    {
        _easyMode = easyMode;
        _leaderboard.myScoreText.text = "My score: " + metersDug;
        _leaderboard.ShowLoading();
        Invoke("ShowResultsScreen", 2.5f);
        if (!easyMode)
        {
            //Debug.Log("Score recorded in normal mode.");
            new AccountDetailsRequest().SetDurable(true).Send((response) =>
            {
                if (!response.HasErrors)
                {
                    if (string.IsNullOrEmpty(response.DisplayName))
                    {
                        _showUsernamePopupOnLoad = true;
                        if (_resultsShown)
                        {
                            ShowUsernamePopup();
                        }
                    }
                }
                else //response has errors
                {
                    _leaderboard.ShowError();
                }
                GameController.current.PostScore(metersDug, _leaderboard.Refresh);
            });
        }
        else //easy mode
        {
            _leaderboard.Refresh();
            //PopulateLeaderboard();
        }
    }

    //public void PopulateLeaderboard()
    //{
    //    new LeaderboardDataRequest_SCORE_LEADERBOARD().SetEntryCount(GameController.current.leaderboardDisplayCount).Send((response) =>
    //    {
    //        if (!response.HasErrors)
    //        {
    //            Debug.Log("Found Leaderboard Data...");
    //            leaderboardNameText.text = string.Empty;
    //            leaderboardRankText.text = string.Empty;
    //            leaderboardScoreText.text = string.Empty;
    //            foreach (GameSparks.Api.Responses.LeaderboardDataResponse._LeaderboardData entry in response.Data)
    //            {
    //                int rank = (int)entry.Rank; // we can get the rank directly
    //                string playerName = entry.UserName;
    //                string score = entry.JSONData["SCORE"].ToString(); // we need to get the key, in order to get the score
    //                leaderboardRankText.text += "\n" + rank;
    //                leaderboardNameText.text += "\n" + playerName;
    //                leaderboardScoreText.text += "\n" + score;
    //                //leaderboardDataText.text += rank + "   Name: " + playerName + "        Score:" + score + "\n"; // add the score to the output text
    //            }
    //        }
    //        else
    //        {
    //            //ShowLeaderboardError();
    //        }
    //    });
    //}

    //public void ShowLeaderboardError()
    //{
    //    Debug.Log("Error Retrieving Leaderboard Data...");
    //    leaderboardNameText.text += "\n\n\nError loading leaderboard!";
    //}

    #region Event Handlers

    public void BackRequested()
    {
        if (_confirmDialogShown)
        {
            CancelPauseMenuAction();
        }
        else if (scoreInfoPopupIsActive)
        {
            HideScoreInfoPopup();
        }
        else if (GameController.current.gameState != GameState.Results)
        {
            PauseButtonClicked();
        }
    }

    public void EnterRequested()
    {
        if (_enterAction != null)
        {
            _enterAction();
        }
    }

    public void ConfirmUsernameButtonClicked()
    {
        string name = usernameBoxText.text.Trim();
        if (!string.IsNullOrEmpty(name) /*&& name.Length <= 20*/)
        {
            new ChangeUserDetailsRequest().SetDisplayName(name).Send((response) =>
            {
                GameController.current.PostScore(metersDug, _leaderboard.Refresh);
            });
            HideUsernamePopup();
        }
    }

    //public void CancelScoresButtonClicked()
    //{
    //    HideUsernamePopup();
    //    PopulateLeaderboard();
    //    SettingsManager.uploadScore = false;
    //}

    public void ShowScoreInfoButtonClicked()
    {
        ShowScoreInfoPopup();
    }

    //public void HideScoreInfoButtonClicked()
    //{
    //    HideScoreInfoPopup();
    //}

    public void PauseButtonClicked()
    {
        if (GameController.current.gameState != GameState.Results)
        {
            GameController.current.Pause();
            //pauseButtonCurrentImage.sprite = (GameController.current.gamePhase == GamePhase.Paused) ? playButtonSprite : pauseButtonSprite;
            pauseMenuAnimator.SetBool("Paused", GameController.current.gameState == GameState.Paused);
        }
    }

    public void ReloadButtonClicked()
    {
        _menuAction = PauseMenuAction.Restart;
        pauseMenuActionConfirmationAnimator.SetTrigger("ShowPopup");
        _enterAction = () => ConfirmPauseMenuAction();
        _confirmDialogShown = true;
    }

    public void MenuButtonClicked()
    {
        _menuAction = PauseMenuAction.Quit;
        pauseMenuActionConfirmationAnimator.SetTrigger("ShowPopup");
        _enterAction = () => ConfirmPauseMenuAction();
        _confirmDialogShown = true;
    }

    private bool _confirmDialogShown = false;
    public void ConfirmPauseMenuAction()
    {
        _confirmDialogShown = false;
        _enterAction = null;
        pauseMenuActionConfirmationAnimator.SetTrigger("HidePopup");
        if (_menuAction == PauseMenuAction.Restart)
        {
            pauseMenuAnimator.SetBool("Paused", false);
            GameController.current.ReloadGame();
        }
        else //quit
        {
            pauseMenuAnimator.SetBool("Paused", false);
            GameController.current.LoadMainMenu();
        }
    }

    public void CancelPauseMenuAction()
    {
        pauseMenuActionConfirmationAnimator.SetTrigger("HidePopup");
        _confirmDialogShown = false;
        _enterAction = null;
    }

    public void ResultsReloadButtonClicked()
    {
        GameController.current.ReloadGame();
    }

    public void ResultsMenuButtonClicked()
    {
        GameController.current.LoadMainMenu();
    }

    #endregion

    #region Loading Screen

    public void StartUpdatingLoadingScreen()
    {
        _loadingAnimator.SetTrigger("FadeIn");
        //StartCoroutine(WaitToFinishLoading());
    }

    public void FadeOutLoadingScreen()
    {
        _loadingAnimator.SetTrigger("FadeOut");
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

}
