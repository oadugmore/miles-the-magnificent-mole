using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax;
}

public class PlayerController : MonoBehaviour
{
    #region Properties

    public float moveForce;
    public float horizontalSpeed;
    //public float fastMaxHorizontalSpeed;
    public float diggingSpeed;
    public float jumpForce;
    public float jetpackForce;
    public float horizontalSpeedIncrement;
    public float diggingSpeedIncrement;
    public float jumpForceIncrement;
    //public float fastMaxDiggingSpeed;
    //public float powerUpDuration;
    //public float fastJumpForce;
    public float fallForce;
    public float maxFallSpeed;
    public float correctionSmooth;
    public float groundCheckRadius;
    public float floatUpOnDeathSpeed;
    public AudioSource powerUpAudio;
    public AudioSource deathAudio;
    public Transform groundCheckStanding;
    public Transform groundCheckDigging;
    public Digger digger;
    public Boundary boundary;
    public Collider2D lowerBody;
    //public Collider lowerBody;
    public Vector3 groundCheckOffset;
    public float geyserPushForce;
    public float maxGeyserPushSpeed;
    public PhysicsMaterial2D playerPhysics;

    private bool _fallSpeedChanged = false;
    private bool _isDrowning;
    private bool _controlEnabled = true;
    private bool _playSound;
    private bool _jumpAnimating = false;
    private int _jetpackCount = 0;
    private float _jumpByTime = 0;
    private float diggingRestrainThreshold;
    private bool _isPoweredUp;
    private float _powerUpTime;
    private bool _jump = false;
    private bool _useJetpack = false;
    private CircleCollider2D _lowerBodyCollider;
    private LayerMask groundMask;
    private GameSceneManager _menuSceneManager;
    //private SphereCollider _lowerBodyCollider;
    private RaycastHit2D _solidGroundCast;
    private Collider2D _groundCheckCollider;
    private Animator _anim;
    private Rigidbody2D _rb2d;
    
    private bool _facingRight = false;
    public bool facingRight
    {
        get { return _facingRight; }
        set
        {
            //_anim.SetBool("FacingRight", value);
            _facingRight = value;
        }
    }

    private bool _grounded = false;
    public bool grounded
    {
        get { return _grounded; }
        set
        {
            _anim.SetBool("Grounded", value);
            _grounded = value;
        }
    }

    private bool _digging = false;
    public bool digging
    {
        get { return _digging; }
        set
        {
            if (_digging != value)
            {
                _rb2d.gravityScale = value ? 0 : 1;
                //Physics2D.gravity = value ? Vector2.zero : new Vector2(0, -30);
                //Physics.gravity = value ? Vector3.zero : new Vector3(0, -9.81f);
                digger.SetCanDig(value);
                _anim.SetBool("Digging", value);
                _digging = value;
            }
        }
    }

    #endregion

    // Use this for initialization
    void Start()
    {
        _anim = GetComponent<Animator>();
        _rb2d = GetComponent<Rigidbody2D>();
        _lowerBodyCollider = lowerBody.GetComponent<CircleCollider2D>();
        //_lowerBodyCollider = lowerBody.GetComponent<SphereCollider>();
        _menuSceneManager = FindObjectOfType<GameSceneManager>();
        groundMask = 1 << LayerMask.NameToLayer("SoftGround") | 1 << LayerMask.NameToLayer("SolidGround");
        diggingRestrainThreshold = diggingSpeed + 2;
        _playSound = SettingsManager.effectsOn;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Pizza":
                //_isPoweredUp = true;
                //_powerUpTime = Time.time;
                horizontalSpeed += horizontalSpeedIncrement;
                diggingSpeed += diggingSpeedIncrement;
                diggingRestrainThreshold = diggingSpeed + 2;
                jumpForce += jumpForceIncrement;
                collision.gameObject.SendMessage("OnCollected");
                if (_playSound) powerUpAudio.Play();
                _anim.SetTrigger("Flash");
                break;
            case "Jetpack":
                collision.gameObject.SendMessage("OnCollected");
                _jetpackCount++;
                _menuSceneManager.SetJetpackButtonActive(true);
                break;
            case "Water":
                if (!_isDrowning)
                {
                    _isDrowning = true;
                    _controlEnabled = false;
                    _touchingGeyser = false;
                    //_anim.SetTrigger("Drowning");
                    _rb2d.isKinematic = true;
                    //_rb2d.AddForce(Vector2.up * floatUpOnDeathForce);
                    //disable collisions
                    digging = false;
                    foreach (BoxCollider2D box in GetComponentsInChildren<BoxCollider2D>())
                    {
                        box.enabled = false;
                    }
                    _rb2d.velocity = new Vector2(floatUpOnDeathSpeed / 5, floatUpOnDeathSpeed);
                    if (_playSound) deathAudio.Play();
                    GameController.current.GameOver();
                }
                break;
            case "Lava":
                //_rb2d.isKinematic = true;
                _controlEnabled = false;
                if (_playSound) deathAudio.Play();
                GameController.current.GameOver();
                digging = false;
                _isDrowning = true;
                //_rb2d.velocity = new Vector2(0, 0);
                break;
            //case "SoftGround":
            //    if (digging)
            //    {
            //        collision.gameObject.SetActive(false);
            //    }
            //    break;
            default:
                break;
        } //end switch (tag)
    }

    private bool _touchingGeyser = false;
    private float _nextGeyserPushTime = 0;
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("GeyserWater"))
        {
            _touchingGeyser = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("GeyserWater"))
        {
            _touchingGeyser = false;
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, maxGeyserPushSpeed / 2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!digging)
        {
            //_solidGroundCast = Physics2D.Linecast(gameObject.transform.position, groundCheckStanding.position, 1 << LayerMask.NameToLayer("Ground"));
            _groundCheckCollider = Physics2D.OverlapCircle(lowerBody.transform.position + groundCheckOffset, _lowerBodyCollider.radius * 2, groundMask);

            //is there something directly below us?
            if (_groundCheckCollider)
            {
                //is it going to stop us?
                if (_rb2d.velocity.y <= 0 /*&& (_groundCheckCollider.gameObject.CompareTag("SolidGround") || _groundCheckCollider.gameObject.CompareTag("SoftGround"))*/ )
                {
                    grounded = true;
                    _jumpAnimating = false;
                    _anim.ResetTrigger("Flip");
                }
            }
            //there is nothing below us
            else
            {
                grounded = false;
            }
        }

        if (CrossPlatformInputManager.GetButtonDown("Jetpack") && _jetpackCount > 0)
        {
            _jetpackCount--;
            _useJetpack = true;
            digging = false;
            if (_jetpackCount < 1) _menuSceneManager.SetJetpackButtonActive(false);
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump") && _controlEnabled)
        {
            if (grounded)
            {
                _jump = true;
            }
            else
            {
                _jumpByTime = Time.time + 0.3f;
                if (!digging && !_jumpAnimating)
                {
                    FlipOver();
                }
            }
        }

        else if (CrossPlatformInputManager.GetButtonUp("Jump") && digging)
        {
            digging = false;
            _fallSpeedChanged = true;
        }

        if (!grounded && !CrossPlatformInputManager.GetButton("Jump"))
        {
            digging = false;
        }

        else if (grounded && Time.time < _jumpByTime && CrossPlatformInputManager.GetButton("Jump"))
        {
            _jump = true;
        }
    }

    public void OnJumpAnimComplete()
    {
        _jumpAnimating = false;
        if (CrossPlatformInputManager.GetButton("Jump") && !grounded && _controlEnabled)
        {
            FlipOver();
        }
    }

    private void FlipOver()
    {
        //_jumpAnimating = false;
        _rb2d.velocity = new Vector2(_rb2d.velocity.x, 0);
        _anim.SetTrigger("Flip");
    }

    public void FlippedOver()
    {
        digging = true;
        _anim.ResetTrigger("Flip");
        _fallSpeedChanged = true;
    }

    void FixedUpdate()
    {
        if (GameController.current.gameState != GameState.Playing)
        {
            return;
        }
        Vector2 force = Vector2.zero;
        float h = _controlEnabled ? CrossPlatformInputManager.GetAxis("Horizontal") : 0;
        _anim.SetFloat("Speed", Mathf.Abs(h));

        if (Mathf.Sign(h) * _rb2d.velocity.x < horizontalSpeed * Mathf.Abs(h))
            //_rb2d.AddForce(Vector2.right * h * moveForce);
            force += Vector2.right * h * moveForce;

        else if (Mathf.Abs(_rb2d.velocity.x) > horizontalSpeed && h != 0)
            _rb2d.velocity = new Vector2(Mathf.Sign(_rb2d.velocity.x) * horizontalSpeed, _rb2d.velocity.y);

        if (digging && -_rb2d.velocity.y < diggingSpeed && !_touchingGeyser) //if we're FastFalling but moving too slow and not touching a geyser
        {
            //_rb2d.AddForce(Vector2.down * fallForce);
            force += Vector2.down * fallForce;
            //Debug.Log("Speeding up!");
        }
        else if (-_rb2d.velocity.y > diggingRestrainThreshold) //if we're moving too fast
        {
            //Debug.Log("Slow down there! You were going " + _rb2d.velocity.y + " and the threshold is " + diggingRestrainThreshold);
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, -(diggingSpeed + 0.5f));
        }

        if (digging && h < 0.1f)
        {
            _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
        }

        if (_touchingGeyser && Time.time > _nextGeyserPushTime)
        {
            if (_rb2d.velocity.y < 0) _rb2d.velocity = new Vector2(_rb2d.velocity.x, _rb2d.velocity.y / 2);
            if (-_rb2d.velocity.y < maxGeyserPushSpeed) force += Vector2.up * geyserPushForce; //_rb2d.AddForce(new Vector2(0, geyserPushForce));
            _nextGeyserPushTime = Time.time + 0.05f;
        }

        if (_fallSpeedChanged)
        {
            _fallSpeedChanged = false;

            if (!_touchingGeyser)
            {
                if (digging)
                {
                    //_rb2d.AddForce(Vector2.down * fallForce);
                    force += Vector2.down * fallForce;
                }
                else
                {
                    _rb2d.velocity = new Vector2(_rb2d.velocity.x, _rb2d.velocity.y / 2);
                    //_rb2d.AddForce(new Vector2(0f, fallForce));
                }
            } //end if touching geyser
        } //end if fall speed changed

        if (_useJetpack)
        {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, 0);
            _jump = false;
            _grounded = false;
            _useJetpack = false;
            lowerBody.isTrigger = true;
            Invoke("ResetPlayerFriction", 0.5f);
            force.y = 0;
            force += Vector2.up * jetpackForce;
        }

        if (_jump)
        {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, 0);
            _anim.SetTrigger("Jump");
            _jumpAnimating = true;
            lowerBody.isTrigger = true;
            //playerPhysics.friction = 0;
            Invoke("ResetPlayerFriction", 0.5f);
            //_rb2d.AddForce(new Vector2(0f, jumpForce));
            force += Vector2.up * jumpForce;
            grounded = false;
            _jump = false;
        }

        _rb2d.position = new Vector2
        (
            Mathf.Clamp(_rb2d.position.x, boundary.xMin, boundary.xMax),
            _rb2d.position.y
        );

        _rb2d.AddForce(force);

        if (h > 0 && !_facingRight)
            Flip();
        else if (h < 0 && _facingRight)
            Flip();
    }

    void ResetPlayerFriction()
    {
        lowerBody.isTrigger = false;
        //playerPhysics.friction = 1;
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}