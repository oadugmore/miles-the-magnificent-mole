using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class TutorialPlayerController : MonoBehaviour
{
    #region Properties

    public float moveForce;
    public float normalMaxHorizontalSpeed;
    public float fastMaxHorizontalSpeed;
    public float normalMaxDiggingSpeed;
    public float fastMaxDiggingSpeed;
    public float powerUpDuration;
    public float normalJumpForce;
    public float fastJumpForce;
    public float fallForce;
    public float maxFallSpeed;
    public float correctionSmooth;
    public float groundCheckRadius;
    public float floatUpOnDeathSpeed;
    public AudioSource powerUpAudio;
    public Transform groundCheckStanding;
    public Transform groundCheckDigging;
    public Digger digger;
    public Boundary boundary;
    public Collider2D lowerBody;

    private LayerMask groundMask;
    private bool _isPoweredUp;
    private float _powerUpTime;
    private bool _jump = false;
    private RaycastHit2D _solidGroundCast;
    private Collider2D _groundCheckCollider;
    private Animator _anim;
    private Rigidbody2D _rb2d;
    private bool _fallSpeedChanged = false;
    private bool _isDrowning;
    //private Controller _controller = new Controller();

    private bool _facingRight = false;
    [HideInInspector]
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
    [HideInInspector]
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
    [HideInInspector]
    public bool digging
    {
        get { return _digging; }
        set
        {
            if (_digging != value)
            {
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
        groundMask = 1 << LayerMask.NameToLayer("SoftGround") | 1 << LayerMask.NameToLayer("SolidGround");
    }

    public void FlippedOver()
    {
        digging = true;
        _anim.ResetTrigger("Flip");
        _fallSpeedChanged = true;
    }

    //public void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Water") && !_isDrowning)
    //    {
    //        _isDrowning = true;
    //        _anim.SetTrigger("Drowning");
    //        _rb2d.isKinematic = true;
    //        //_rb2d.AddForce(Vector2.up * floatUpOnDeathForce);
    //        GetComponentInChildren<BoxCollider2D>().enabled = false;
    //        _rb2d.velocity = new Vector2(floatUpOnDeathSpeed / 5, floatUpOnDeathSpeed);
    //        GameController.current.OnPlayerDeath();
    //    }
    //    else if (collision.gameObject.CompareTag("Powerup"))
    //    {
    //        _isPoweredUp = true;
    //        _powerUpTime = Time.time;
    //        collision.gameObject.SendMessage("OnCollected");
    //        powerUpAudio.Play();
    //        _anim.SetTrigger("Flash");
    //    }
    //}

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (digging && collision.gameObject.CompareTag("SoftGround"))
        {
            //collision.gameObject.SendMessage("InitiateDestruction");
            Destroy(collision.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!digging)
        {
            //_solidGroundCast = Physics2D.Linecast(gameObject.transform.position, groundCheckStanding.position, 1 << LayerMask.NameToLayer("Ground"));
            _groundCheckCollider = Physics2D.OverlapCircle(lowerBody.transform.position, groundCheckRadius, groundMask);

            //is there something directly below us?
            if (_groundCheckCollider)
            {
                //is it going to stop us?
                if (_rb2d.velocity.y <= 0 /*&& (_groundCheckCollider.gameObject.CompareTag("SolidGround") || _groundCheckCollider.gameObject.CompareTag("SoftGround"))*/ )
                {
                    grounded = true;
                    _anim.ResetTrigger("Flip");
                }
            }
            //there is nothing below us
            else
            {
                grounded = false;
            }
        }

        //else //we're digging
        //{
        //    _solidGroundCast = Physics2D.Linecast(gameObject.transform.position, groundCheckDigging.position, 1 << LayerMask.NameToLayer("Ground"));
        //    //is there something directly below us?
        //    if (_solidGroundCast)
        //    {
        //        //is it going to stop us?
        //        if (_rb2d.velocity.y <= 0 && (_solidGroundCast.collider.gameObject.CompareTag("SolidGround") /*|| (_solidGroundCast.collider.gameObject.CompareTag("SoftGround") && !digging)*/ ))
        //        {
        //            digging = false;
        //            //grounded = true;
        //        }
        //    }
        //}

        if (_isPoweredUp && Time.time > powerUpDuration + _powerUpTime)
        {
            _isPoweredUp = false;
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                _jump = true;
            }

            else if (!digging)
            {
                //digging = true;
                _anim.SetTrigger("Flip");
                //_fallSpeedChanged = true;
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
    }

    public void OnJumpAnimComplete()
    {
        if (CrossPlatformInputManager.GetButton("Jump") && !grounded)
        {
            _anim.SetTrigger("Flip");
        }
    }

    void FixedUpdate()
    {
        //if (GameController.current.menuButtonHandler.MenuPhase != MenuPhase.Tutorial)
        //{
        //    return;
        //}
        float h = CrossPlatformInputManager.GetAxis("Horizontal"); //why is this always 0?
        //Debug.Log("Horizontal input: " + h);
        _anim.SetFloat("Speed", Mathf.Abs(h));

        if (_isPoweredUp)
        {
            if (Mathf.Sign(h) * _rb2d.velocity.x < fastMaxHorizontalSpeed * Mathf.Abs(h))
                _rb2d.AddForce(Vector2.right * h * moveForce);

            if (Mathf.Abs(_rb2d.velocity.x) > fastMaxHorizontalSpeed)
                _rb2d.velocity = new Vector2(Mathf.Sign(_rb2d.velocity.x) * fastMaxHorizontalSpeed, _rb2d.velocity.y);

            if (digging && _rb2d.velocity.y > -fastMaxDiggingSpeed) //if we're FastFalling but moving too slow
            {
                //_rb2d.velocity = new Vector2(_rb2d.velocity.x, -fastMaxDiggingSpeed);
                _rb2d.AddForce(Vector2.down * fallForce);
            }

            else if (_rb2d.velocity.y < -fastMaxDiggingSpeed) //if we're moving too fast
            {
                _rb2d.velocity = new Vector2(_rb2d.velocity.x, -fastMaxDiggingSpeed);
            }
        } //end if powered up

        else //not powered up
        {
            if (Mathf.Sign(h) * _rb2d.velocity.x < normalMaxHorizontalSpeed * Mathf.Abs(h))
                _rb2d.AddForce(Vector2.right * h * moveForce);

            if (Mathf.Abs(_rb2d.velocity.x) > normalMaxHorizontalSpeed)
                _rb2d.velocity = new Vector2(Mathf.Sign(_rb2d.velocity.x) * normalMaxHorizontalSpeed, _rb2d.velocity.y);

            if (digging && _rb2d.velocity.y > -normalMaxDiggingSpeed) //if we're FastFalling but moving too slow
            {
                //_rb2d.velocity = new Vector2(_rb2d.velocity.x, -normalMaxDiggingSpeed);
                _rb2d.AddForce(Vector2.down * fallForce);
            }

            else if (_rb2d.velocity.y < -normalMaxDiggingSpeed) //if we're moving too fast
            {
                _rb2d.velocity = new Vector2(_rb2d.velocity.x, -normalMaxDiggingSpeed);
            }
        } //end else (not powered up)

        if (!digging && !grounded && _rb2d.velocity.y < -maxFallSpeed)
        {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, -maxFallSpeed);
        }

        if (digging && h < 0.1f)
        {
            _rb2d.velocity = new Vector2(0, _rb2d.velocity.y);
        }

        if (h > 0 && !_facingRight)
            Flip();
        else if (h < 0 && _facingRight)
            Flip();

        if (_fallSpeedChanged)
        {
            _fallSpeedChanged = false;

            if (digging)
            {
                _rb2d.AddForce(Vector2.down * fallForce);
            }
            else
            {
                _rb2d.velocity = new Vector2(_rb2d.velocity.x, 0);
                //_rb2d.AddForce(new Vector2(0f, fallForce));
            }
            //else
            //{
            //    _fallSpeedChanged = true;
            //}
        }

        if (_jump)
        {
            _anim.SetTrigger("Jump");
            _rb2d.AddForce(new Vector2(0f, (_isPoweredUp ? fastJumpForce : normalJumpForce)));
            grounded = false;
            _jump = false;
        }

        _rb2d.position = new Vector3
        (
            Mathf.Clamp(_rb2d.position.x, boundary.xMin, boundary.xMax),
            _rb2d.position.y,
            0.0f
        );
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}