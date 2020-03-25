using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class TutorialController : MonoBehaviour
{
    //public Graphic[] images;
    public Button leftButton;
    public Button rightButton;
    public ButtonHandler jumpButton;
    public TutorialJoystick joystick;
    public GameObject digExamplePrefab;
    public GameObject jumpExamplePrefab;
    public Vector3 playerWalkingStartPosition;
    public Vector3 baseSoftGroundPosition;
    public Vector3 playerDiggingStartPosition;
    public float softGroundHeight;
    public int tutorialCount;
    public TutorialPlayerController player;
    public CanvasGroup touchControlsGroup;
    public CanvasGroup keyboardControlsGroup;

    private int activeTutorialId;
    private Animator tutorialAnimator;
    private CanvasGroup canvasGroup;
    private Object spawnedGround;
    //private Object softGround2;

    // Use this for initialization
    void Start()
    {
        tutorialAnimator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        //player = GetComponentInChildren<TutorialPlayerController>();
        //tutorialAnimator.SetTrigger("StartWalkingClip"); //debug only
    }
    
    public void ClearGround()
    {
        Destroy(spawnedGround);
        //Destroy(softGround2);
    }

    public void RebuildDigGround()
    {
        spawnedGround = Instantiate(digExamplePrefab);
        //Vector3 newPosition = new Vector3(baseSoftGroundPosition.x, baseSoftGroundPosition.y + softGroundHeight);
        //softGround2 = Instantiate(diggingExamplePrefab, newPosition, Quaternion.identity);
    }

    public void RebuildJumpGround()
    {
        spawnedGround = Instantiate(jumpExamplePrefab);
    }

    public void MovePlayerToDigStartPosition()
    {
        player.transform.position = playerDiggingStartPosition;
    }

    #region Controller Functions
    public void MoveJoystickLeft()
    {
        joystick.SetAxisLeft();
    }

    public void MoveJoystickRight()
    {
        joystick.SetAxisRight();
    }

    public void MoveJoystickCenter()
    {
        joystick.SetAxisCenter();
    }

    public void PushJumpButton()
    {
        jumpButton.SetDownState();
    }

    public void ReleaseJumpButton()
    {
        jumpButton.SetUpState();
    }
    #endregion

    public void ShowTutorial()
    {
        //canvasGroup.interactable = true;
        //canvasGroup.alpha = 1;
        leftButton.interactable = false;
        rightButton.interactable = true;
        //tutorialAnimator.SetTrigger("OpenUp");
        activeTutorialId = 1;
        Invoke("ShowWalkingClip", 0.1f);
        if (SettingsManager.touchControlsEnabled)
        {
            touchControlsGroup.alpha = 1;
            keyboardControlsGroup.alpha = 0;
        }
        else
        {
            touchControlsGroup.alpha = 0;
            keyboardControlsGroup.alpha = 1;
        }
        //ShowWalkingClip();
        //if (!SettingsManager.touchControlsEnabled && !PlayerPrefs.HasKey("SeenKeyboardPrompt"))
        //{
        //    ShowKeyboardPrompt();
        //    PlayerPrefs.SetInt("SeenKeyboardPrompt", 1);
        //}
        //BuildSoftGround();
        //StartCoroutine(GameController.Fade(true, canvasGroup));
    }

    public void HideTutorial()
    {
        tutorialAnimator.SetTrigger("End");
        //canvasGroup.interactable = false;
        //tutorialAnimator.SetTrigger("CloseDown");
        //StartCoroutine(GameController.Fade(false, canvasGroup));
        //_fadeTimerStart = true;
        //_closingMenu = true;
    }

    #region Clips
    private void ShowWalkingClip()
    {
        tutorialAnimator.SetTrigger("StartWalkingClip"); //unity crashes here if ShowWalkingClip was not delayed (Invoked) from ShowTutorial
        ClearGround();
        ReleaseJumpButton();
        player.transform.position = playerWalkingStartPosition;
    }

    private void ShowDiggingClip()
    {
        tutorialAnimator.SetTrigger("StartDiggingClip");
        ReleaseJumpButton();
        joystick.SetAxisCenter();
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    private void ShowJumpingClip()
    {
        tutorialAnimator.SetTrigger("StartJumpingClip");
        ReleaseJumpButton();
    }

    private void ShowKeyboardPrompt()
    {
        tutorialAnimator.SetTrigger("ShowPrompt");
    }

    private void HideKeyboardPrompt()
    {
        tutorialAnimator.SetTrigger("HidePrompt");
    }
    #endregion

    public void LeftButtonClicked()
    {
        rightButton.interactable = true;
        activeTutorialId--;
        switch (activeTutorialId)
        {
            case 1:
                {
                    ShowWalkingClip();
                    leftButton.interactable = false;
                    break;
                }
            case 2:
                {
                    ShowDiggingClip();
                    break;
                }
            default:
                break;
        } //end switch
    }

    public void RightButtonClicked()
    {
        leftButton.interactable = true;
        activeTutorialId++;
        switch (activeTutorialId)
        {
            case 2:
                {
                    ShowDiggingClip();
                    break;
                }
            case 3:
                {
                    ShowJumpingClip();
                    rightButton.interactable = false;
                    break;
                }
            default:
                break;
        } //end switch
    }

    public void CloseKeyboardPromptButtonClicked()
    {
        HideKeyboardPrompt();
    }

}
