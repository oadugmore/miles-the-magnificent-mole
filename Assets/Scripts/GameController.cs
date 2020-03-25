using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityStandardAssets.CrossPlatformInput;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using GameSparks.Core;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum GameState
{
    MainMenu, Playing, Paused, Results
}

public class GameController : MonoBehaviour
{
    public static GameController current;

    #region Properties

    public bool navigationEnabled;
    [HideInInspector]
    public MenuSceneManager menuSceneManager;
    [HideInInspector]
    public GameSceneManager gameSceneManager;

    private List<Action> authenticatedActions;
    private int _leaderboardDisplayCount = 25;
    private bool fadeInOnLoad;
    private bool _gameOver;
    private bool _waitUntilGameLoads;
    private bool _waitUntilMenuLoads;
    private bool _waitUntilGameUnloads;
    private bool _pausing;
    private bool _resuming;
    //private bool _easyMode;
    //private const string k_distanceDisplayName = "DistanceText";
    //private CameraController _gameCamera;
    private bool _showUsernamePopupOnLoad = false;
    //private bool fadeInAudioOnLoad = true;

    private GameState _gameState;
    public GameState gameState
    {
        get { return _gameState; }
        private set { _gameState = value; }
    }

    public static bool backButtonLeavesApp
    {
        get { return Input.backButtonLeavesApp; }
        set
        {
#if !UNITY_ANDROID
            Input.backButtonLeavesApp = value;
#endif
        }
    }

    #endregion

    #region Enter Key

    //public void StopWaitForEnterKeyCoroutine()
    //{
    //    //Debug.Log("Invoking StopCoroutine(WaitForEnterKeyCoroutine)");
    //    StopCoroutine("WaitForEnterKeyCoroutine");
    //    //Debug.Log("Invoked StopCoroutine(WaitForEnterKeyCoroutine)");
    //}

    //public void WaitForEnterKey(Action action)
    //{
    //    StartCoroutine(WaitForEnterKeyCoroutine(action));
    //}

    //private IEnumerator WaitForEnterKeyCoroutine(Action action)
    //{
    //    while (!CrossPlatformInputManager.GetButtonDown("Submit"))
    //    {
    //        yield return null;
    //    }
    //    //Enter key pressed
    //    action();
    //}
    #endregion

    public int GetLeaderboardDisplayCount()
    {
        return _leaderboardDisplayCount;
    }

    private void CustomizeScrollers()
    {
        var scrollers = FindObjectsOfType<ScrollRect>();
        foreach (ScrollRect scroller in scrollers)
        {
#if UNITY_ANDROID
            scroller.movementType = ScrollRect.MovementType.Clamped;
#elif WINDOWS_UWP
            scroller.scrollSensitivity *= -1;
#endif
        }
    }

    public void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            OrientationHelper.LockRotation(_gameState == GameState.Playing);
            //InitLandscapeSupportAndroid();
        }
    }

    #region Navigation

    void ConfigureNavigation(bool enabled)
    {
        FindObjectOfType<EventSystem>().sendNavigationEvents = enabled;
        var buttons = FindObjectsOfType<Button>();
        var autoNav = new Navigation() { mode = Navigation.Mode.Automatic };
        if (enabled)
        {
            foreach (Button b in buttons)
            {
                b.navigation = autoNav;
            }
        }
    }

    #endregion

    private void InitializeGameScene()
    {
        ConfigureNavigation(navigationEnabled);
        CustomizeScrollers();
        OrientationHelper.LockRotation(true);
        if (!gameSceneManager)
        {
            gameSceneManager = FindObjectOfType<GameSceneManager>();
            //player = FindObjectOfType<PlayerController>();
            //generator = FindObjectOfType<RandomTerrainGenerator>();
            //waterController = FindObjectOfType<WaterController>();
            //distanceDisplay = GameObject.Find(k_distanceDisplayName).GetComponent<Text>();

            //garbageCollector = FindObjectOfType<DeactivateOnLeave>();
        }
        if (fadeInOnLoad)
        {
            gameSceneManager.FadeOutLoadingScreen();
        }
    }

    private void InitializeMenuScene()
    {
        ConfigureNavigation(navigationEnabled);
        CustomizeScrollers();
        OrientationHelper.LockRotation(false);
        if (!menuSceneManager)
        {
            //tutorialController = FindObjectOfType<TutorialController>();
            //settingsCanvasGroup = FindObjectOfType<CanvasGroup>();
            menuSceneManager = FindObjectOfType<MenuSceneManager>();
        }
        if (fadeInOnLoad)
        {
            menuSceneManager.FadeOutLoadingScreen();
        }
    }

    void Awake()
    {
        SettingsManager.Initialize();
        OrientationHelper.InitLandscapeSupportAndroid();
        if (current)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            current = this;
        }
    }

    void Start()
    {
        authenticatedActions = new List<Action>();
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals("Menu"))
        {
            gameState = GameState.MainMenu;
            InitializeMenuScene();
            menuSceneManager.FadeOutLoadingScreen();
        }
        else if (sceneName.Equals("Game"))
        {
            gameState = GameState.Playing;
            InitializeGameScene();
            GameController.backButtonLeavesApp = false;
        }
        else
        {
            Debug.LogError("Unsupported state!");
        }

        //menuButtonHandler.loadingAnimator.SetTrigger("FadeOut");
    }

    public void OnGameSparksAuthenticated()
    {
        menuSceneManager.OnGameSparksReady();
        if (authenticatedActions.Count > 0)
        {
            Debug.Log("It's a miracle! Carrying out authenticated actions that were requested a while ago...");
            foreach (Action a in authenticatedActions)
            {
                if (a != null) a();
            }
            authenticatedActions.Clear();
        }
    }

    public void LoadMainMenu()
    {
        //StopWaitForEnterKeyCoroutine();
        //fadeInAudioOnLoad = true;
        Time.timeScale = 1;
        _pausing = false;
        fadeInOnLoad = false;
        gameSceneManager.audioController.FadeOut(0.5f, false);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Menu");
        asyncOperation.allowSceneActivation = false;
        gameSceneManager.async = asyncOperation;
        gameSceneManager.StartUpdatingLoadingScreen();
        gameState = GameState.MainMenu;
        _waitUntilMenuLoads = true;
        fadeInOnLoad = true;
        backButtonLeavesApp = true;
    }

    public void LoadGame(bool fadeOut)
    {
        //StopWaitForEnterKeyCoroutine();
        //_easyMode = SettingsManager.easyMode;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Game");
        if (fadeOut)
        {
            asyncOperation.allowSceneActivation = false;
            menuSceneManager.async = asyncOperation;
            menuSceneManager.FadeInLoadingScreen();
        }
        Time.timeScale = 1;
        _waitUntilGameLoads = true;
        fadeInOnLoad = true;
        GameController.backButtonLeavesApp = false;
        //gamePhase = GamePhase.Playing;
    }

    public void ReloadGame()
    {
        Time.timeScale = 1;
        _pausing = false;
        //fadeInAudioOnLoad = false;
        fadeInOnLoad = false;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Loading");
        asyncOperation.allowSceneActivation = false;
        gameSceneManager.async = asyncOperation;
        gameSceneManager.StartUpdatingLoadingScreen();
        gameSceneManager.audioController.FadeOut(0.5f, false);
        _waitUntilGameUnloads = true;
    }

    void Update()
    {
        #region Scene Management
        if (_waitUntilGameLoads && SceneManager.GetSceneByName("Game").isLoaded)
        {
            _waitUntilGameLoads = false;
            InitializeGameScene();
            StartPlaying();
        }
        else if (_waitUntilMenuLoads && SceneManager.GetSceneByName("Menu").isLoaded)
        {
            _waitUntilMenuLoads = false;
            InitializeMenuScene();
        }
        else if (_waitUntilGameUnloads && !SceneManager.GetSceneByName("Game").isLoaded)
        {
            //Color c = menuButtonHandler.loadingScreen.color;
            //c.a = 1;
            //menuButtonHandler.loadingScreen.color = c;
            _waitUntilGameUnloads = false;
            LoadGame(false);
        }
        #endregion

        #region Pause and Resume

        if (_pausing)
        {
            if (Time.timeScale > 0.1f)
            {
                Time.timeScale -= 0.03f;
            }
            else
            {
                Time.timeScale = 0;
                _pausing = false;
            }
        }
        else if (_resuming)
        {
            if (Time.timeScale < 0.9f)
            {
                Time.timeScale += 0.03f;
            }
            else
            {
                Time.timeScale = 1;
                _resuming = false;
            }
        }
        #endregion

        #region Button events
        //check if Back Button (escape key) was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    menuSceneManager.BackRequested();
                    break;
                case GameState.Playing:
                case GameState.Paused:
                case GameState.Results:
                    gameSceneManager.BackRequested();
                    break;
                default:
                    break;
            }
        }
        //check if Enter was pressed this frame
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    menuSceneManager.EnterRequested();
                    break;
                case GameState.Playing:
                case GameState.Paused:
                case GameState.Results:
                    gameSceneManager.EnterRequested();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }

    private void StartPlaying()
    {
        gameState = GameState.Playing;
        _gameOver = false;
        //metersDug = 0;
    }

    public void Pause()
    {
        if (gameState != GameState.Paused)
        {
            //do stuff to pause game
            Time.timeScale = 1;
            _pausing = true;
            _resuming = false;
            gameSceneManager.audioController.FadeOut(0.5f, true);
            gameState = GameState.Paused;
        }
        else
        {
            //do stuff to resume game
            Time.timeScale = 0;
            _resuming = true;
            _pausing = false;
            gameSceneManager.audioController.FadeIn(0.5f);
            gameState = GameState.Playing;
        }
    }

    public void GameOver()
    {
        if (gameState == GameState.Paused)
        {
            gameSceneManager.PauseButtonClicked();
        }
        OrientationHelper.LockRotation(false);
        //TryLoadResults();
        gameSceneManager.GameOver();
        postAttempts = 0;
        gameState = GameState.Results;
        //Invoke("ShowResultsScreen", 2.5f);
    }

    int postAttempts;
    /// <summary>
    /// If the user is not currently authenticated, the action is added to the authenticatedActions list.
    /// </summary>
    /// <param name="callback"></param>
    public void PostScore(int score, Action callback = null)
    {
        new LogEventRequest_SUBMIT_SCORE().Set_SCORE(score).SetDurable(true).Send((response) =>
        {
            //ShowLeaderboardData();
            //gameSceneManager.PopulateLeaderboard();
            if (!response.HasErrors)
            {
                Debug.Log("Score Posted Successfully!");
            }
            else
            {
                Debug.Log("Error Posting Score! Adding action to authenticatedActions...");
                authenticatedActions.Add(() => PostScore(score, callback));
            }
            if (callback != null) callback();
        });
        if (!GS.Authenticated)
        {
            Debug.Log("User is not authenticated! Adding action to authenticatedActions...");
            authenticatedActions.Add(() => PostScore(score, callback));
        }
    }

}
