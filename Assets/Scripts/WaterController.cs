using UnityEngine;
using System.Collections;
using System;

public class WaterController : MonoBehaviour
{
    public float currentSpeed;
    public float normalMode_InitialSpeed;
    //was 11.5
    //public float normalMode_MaxSpeed;
    public float normalMode_SpeedReduction;
    public float easyMode_InitialSpeed;
    //was 6
    //public float easyMode_MaxSpeed;
    public float easyMode_SpeedReduction;
    public float closeDistance;
    //public int maxWaterHeight;
    public float maxFollowDistance;
    //public Vector3 waterScaleIncrementValue;
    public float slowDownForce;
    public float initialSlowDownSpeed;

    private PlayerController player;
    private Rigidbody2D _rb2d;
    //private float _maxSpeed;
    private float _initialSpeed;
    private float _desiredSpeed;
    private float _speedReduction;
    private bool _slowingDown;
    private float _lastSlowDownTime;
    private bool _isChasing;
    private bool _easyMode;
    private bool _isCloseToPlayer;
    private bool _initialSlowDown;

    public bool isChasing
    {
        get { return _isChasing; }
        private set { _isChasing = value; }
    }

    // Use this for initialization
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        isChasing = false;
        _lastSlowDownTime = 0;
    }

    public void BeginChasing(bool easyMode)
    {
        _easyMode = easyMode;
        if (_easyMode)
        {
            _initialSpeed = easyMode_InitialSpeed;
            //_maxSpeed = easyMode_MaxSpeed;
            _speedReduction = easyMode_SpeedReduction;
        }
        else
        {
            _initialSpeed = normalMode_InitialSpeed;
            //_maxSpeed = normalMode_MaxSpeed;
            _speedReduction = normalMode_SpeedReduction;
        }
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        //SetActive(true);
        //StartCoroutine(GrowWaterHeight());
        _rb2d.velocity = new Vector2(0, -_initialSpeed);
        currentSpeed = _initialSpeed;
        _desiredSpeed = _initialSpeed;
        isChasing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.y < transform.position.y - maxFollowDistance)
        {
            transform.position = new Vector3(transform.position.x, player.transform.position.y + maxFollowDistance, transform.position.z);
        }

        if (!_isCloseToPlayer && player.transform.position.y > transform.position.y - closeDistance)
        {
            _isCloseToPlayer = true;
            UpdateSpeed();
            //_reducedSpeed = _desiredSpeed - _speedReduction;
        }

        else if (_isCloseToPlayer && player.transform.position.y < transform.position.y - closeDistance)
        {
            _isCloseToPlayer = false;
            UpdateSpeed();
            //currentSpeed = _desiredSpeed;
        }
        
        if (_slowingDown && Time.time > _lastSlowDownTime + 0.5f)
        {
            //if (!_initialSlowDown)
            //{
            //    _rb2d.velocity = new Vector2(0, -initialSlowDownSpeed);
            //    _initialSlowDown = true;
            //}
            _lastSlowDownTime = Time.time;
            //_rb2d.AddForce(Vector2.up * slowDownForce);
            _rb2d.velocity = new Vector2(0, _rb2d.velocity.y + 0.8f);
            if (_rb2d.velocity.y > 0)
            {
                _slowingDown = false;
                _rb2d.velocity = Vector2.zero;
            }
        }
    }

    public void UpdateSpeed(bool speedUp = false)
    {
        //if (currentSpeed < _maxSpeed)
        {
            if (speedUp)
            {
                _desiredSpeed += 0.02f;
                maxFollowDistance += 0.1f;
            }

            if (_isCloseToPlayer)
            {
                currentSpeed = _desiredSpeed - _speedReduction;
            }
            else
            {
                currentSpeed = _desiredSpeed;
            }
            _rb2d.velocity = new Vector2(0, -currentSpeed);
        }
    }

    public void SlowDown()
    {
        //_slowingDown = true;
        Invoke("Stop", 5f);
    }

    public void Stop()
    {
        _rb2d.velocity = Vector2.zero;
    }
}
