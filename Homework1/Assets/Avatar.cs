using UnityEngine;
using System.Collections;

public enum GravityChange {
    X, nX, Y, nY, None
}

public class Avatar : MonoBehaviour {

	public KeyCode SpinLeft;
	public KeyCode SpinRight;
	public KeyCode FlipUp;
	public KeyCode FlipDown;

	public KeyCode Pop;

    public BoxCollider PersonCollider;

    public GravityChange CurrentGravityChange = GravityChange.None;

    public SmoothFollow CameraFollower;

    public SkateboardPhysicsManager SkateboardPhysics;
    public Rigidbody Skateboard;
    public Rigidbody Rider;
    public Joint SkateJoint;

    public GameManager GameManager;

    public GameObject SkeletonParent;

    private float _pedalStrength = 5.0f;

    private float _gravityRotationModifier = 1.0f;
    // private float _rotationModifier = 50f;
    private float _rotationModifier = 5f;
    private float _currentOrientation = 0f;
    private float _aerialRotationModifier = 200f;
    private float _aerialTiltModifier = 50f;
    private Vector3 _curNormal = Vector3.up; // smoothed terrain normal
    private float _speedModifier = 0.2f;
    private float _maxSpeed = 50f;
    private float _speed = 0.0f;
    private float _decay = 0.001f;
    private float _gravity = 3.0f;
    private float _decayAirtime = 0.005f;
    private float _previousAccel = 0.0f; // used for compound acceleration
    private long _framesMaxAccel = 0; // used for compound acceleration
    private float _groundDistance;

    private float _currentJumpScore = 0.0f;
    private float _rotationScoreModifier = 50.0f;
    private float _timeScoreModifier = 20.0f;
    public float _jumpStartTime = 0.0f;
    private bool _dead = false;

    private bool _isOnPlane = true;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _launchNormal;
    private Vector3 _launchForward;
    private Vector3 moveDirection = Vector3.zero;
    private Animator _animator;
    private AudioSource _audioSource;
    public AudioClip[] Clips;
    private Collider _riderCollider;
    private Collider _skateboardCollider;
    private bool _ragdoll = false;

    public static Avatar instance;

	// Use this for initialization
	void Start () {
        if (!Avatar.instance) {
            instance = this;
        }

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _riderCollider = Rider.GetComponent<CapsuleCollider>();
        _skateboardCollider = Skateboard.GetComponent<BoxCollider>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = Clips[0];
		TurnRagdoll(false);
        Rider.centerOfMass = Skateboard.transform.position;
	}
	
    IEnumerator RotateMe(Vector3 byAngles, float inTime, Transform obj = null)
    {
        if (obj == null) {
        	obj = Rider.transform;
        }
        Quaternion fromAngle = obj.rotation;
        Quaternion toAngle = Quaternion.Euler(obj.eulerAngles + byAngles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime / inTime)
        {
            obj.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }

    void TurnRagdoll(bool onOff) {
    	_animator.enabled = !onOff;
    	var childrenRigidbodies = SkeletonParent.GetComponentsInChildren<Rigidbody>();
    	var childrenCollidres = SkeletonParent.GetComponentsInChildren<Collider>();
    	if (onOff) {
    		// CameraFollower.target = GameObject.Find("EthanGlasses").transform;
    		CameraFollower.IgnoreAllRotation = true;
    	}
    	Rider.isKinematic = onOff;
    	for (int i = 0; i < childrenRigidbodies.Length; i++) {
    		childrenRigidbodies[i].isKinematic = !onOff;
    		childrenRigidbodies[i].useGravity = onOff;
    	}
    	for (int i = 0; i < childrenCollidres.Length; i++) {
    		childrenCollidres[i].enabled = onOff;
    	}
    }

    bool isDead() {
        return Mathf.Abs(PersonCollider.transform.position.y) < 0.2;
    }

    void FixedUpdate() {
        switch (CurrentGravityChange) {
            case GravityChange.X:
                StartCoroutine(RotateMe(new Vector3(180f, 0f, 0f), _gravityRotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.nX:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, 0f), _gravityRotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.Y:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, -90f), _gravityRotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.nY:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, 90f), _gravityRotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            default:
                break;
        }
        float steer = Input.GetAxis("Horizontal");
        if (SkateboardPhysics.IsGrounded() && Mathf.Abs(steer) > 0.1f) {
            Quaternion q = Quaternion.AngleAxis(_rotationModifier * steer, Skateboard.transform.up) * Skateboard.transform.rotation;
            Skateboard.MoveRotation(q);
            float mag = Skateboard.velocity.magnitude;
            Skateboard.velocity = Skateboard.transform.forward * mag * 1f;
            // Skateboard.AddRelativeTorque(Vector3.up * steer * _rotationModifier * Time.deltaTime);
        }


    }

    void OnJointBreak(float breakForce)
    {
        if (!_dead) {
            EndGame();

        }
    }

    void EndGame() {
        _dead = true;
        Rider.useGravity = true;
        Rider.mass = 200f;
        _audioSource.Stop();
        _audioSource.clip = Clips[1];
        _audioSource.loop = false;
        _audioSource.volume = 100;
        Rider.constraints = RigidbodyConstraints.None;
        Rider.velocity = 0.10f * Skateboard.velocity;
        Skateboard.gameObject.GetComponent<AudioSource>().volume = 0;
        Skateboard.velocity *= 0.1f;
        Rider.mass = 10f;
        _ragdoll = true;
        GameManager.ShowDeadUI();
        _audioSource.Play();
        Debug.Log(_audioSource.volume);
    }

    void OnCollisionEnter(Collision collision) {
    	foreach (ContactPoint point in collision.contacts) {
    		if (point.thisCollider == _riderCollider) {
                if (!_dead) {
                    EndGame();
                }
				break;
    		};
    	}
    }

    void Pedal() {
        Skateboard.AddRelativeForce(Vector3.forward * _pedalStrength * 80, ForceMode.Acceleration);
    }

    public void Land() {
        if (!_dead && _jumpStartTime != 0.0f) {
            Debug.Log("score");
            GameManager.instance.AddScore(_currentJumpScore);
            _currentJumpScore = 0.0f;
            _jumpStartTime = 0.0f;
        }
    }

	void Update()
	{
		CameraFollower.target.position = SkeletonParent.transform.position;

		TurnRagdoll(_ragdoll);

        if (SkateboardPhysics.IsGrounded()) {
            if (Skateboard.velocity.magnitude > 0.1 && !_dead) {
                if (!_audioSource.isPlaying) {
                    _audioSource.Play();
                }
                _audioSource.volume = (Skateboard.velocity.magnitude / _maxSpeed) * 1;
            } else if (!_dead) {
                _audioSource.Stop();
            }
        	CameraFollower.IgnoreLateral = false;
        	if (Input.GetKeyDown(KeyCode.W)) {
        		Pedal();
        	}
            if (Input.GetKeyDown(Pop))
            {
                _animator.SetBool("Crouch", true);
            }
            if (Input.GetKeyUp(Pop))
            {
                _animator.SetBool("Crouch", false);
                _launchNormal = Rider.transform.up;
                _launchForward = Rider.transform.forward;
                _jumpStartTime = Time.time;
                SkateboardPhysics.Pop();
            }
            if (SkateboardPhysics.IsOnPlane)
            {
                //Rider.constraints = RigidbodyConstraints.FreezeRotationY;
                //Rider.constraints = RigidbodyConstraints.FreezeRotationZ;
                Skateboard.constraints = RigidbodyConstraints.FreezeRotationZ;
                // Skateboard.transform.up = Vector3.up * Rider.transform.rotation.y;
            } else
            {
                Rider.constraints = RigidbodyConstraints.None;
                Skateboard.constraints = RigidbodyConstraints.None;
            }
        } else {
            if (!_dead && _jumpStartTime != 0.0f) {
                _currentJumpScore =+ _timeScoreModifier * (Time.time - _jumpStartTime);
            }
            if (_audioSource.isPlaying && !_dead) {
                _audioSource.Stop();
            }
        	CameraFollower.IgnoreLateral = true;
            if (_animator.GetBool("Crouch"))
            {
                _animator.SetBool("Crouch", false);
                SkateboardPhysics.JustPopped = true;
                _launchNormal = Rider.transform.up;
                _launchForward = Rider.transform.forward;
            }
            Rider.constraints = RigidbodyConstraints.FreezeRotationX;

            // Unity does some wacky shit with Gimball lock at z 270 degrees. 
            // Damn undocumented features ruin this program
            if (Input.GetKey(SpinRight)) {
                _currentJumpScore += _aerialRotationModifier * _rotationScoreModifier;
        		Rider.transform.Rotate(_launchNormal * Time.deltaTime * _aerialRotationModifier * 1f);
    		}
    		if (Input.GetKey(SpinLeft)) {
        		Rider.transform.Rotate(_launchNormal * Time.deltaTime * _aerialRotationModifier * -1f);
    		}
    		if (Input.GetKey(FlipUp)) {
                var targetAerialRotation = _launchForward * Time.deltaTime * _aerialTiltModifier * 1f;
        		Rider.transform.Rotate(targetAerialRotation);
    		}
    		if (Input.GetKey(FlipDown)) {
        		Rider.transform.Rotate(_launchForward * Time.deltaTime * _aerialTiltModifier * -1f);
    		}
        }
	}

    static float CalcEulerSafeX(float x)
    {
        if (Mathf.Abs(x) < 90)
            return x;
        x = x % 90;
        if (x > 0)
            x -= 270;
        else
            x += 270;
        return x;
    }

}
