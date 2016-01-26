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

    public Rigidbody Skateboard;
    public Rigidbody Rider;
    public Joint SkateJoint;

    public GameManager GameManager;

    private float _pedalStrength = 5.0f;

    private float _gravityRotationModifier = 0.0f;
    private float _rotationModifier = 50f;
    private float _currentOrientation = 0f;
    private float _aerialRotationModifier = 90f;
    private Vector3 _curNormal = Vector3.up; // smoothed terrain normal
    private float _speedModifier = 0.2f;
    private float _maxSpeed = 10f;
    private float _speed = 0.0f;
    private float _decay = 0.001f;
    private float _gravity = 3.0f;
    private float _decayAirtime = 0.005f;
    private float _previousAccel = 0.0f; // used for compound acceleration
    private long _framesMaxAccel = 0; // used for compound acceleration
    private float _groundDistance;
    private float _popModifier = 290.0f;

    private bool _isOnPlane = true;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 moveDirection = Vector3.zero;
    private Animator _animator;

	// Use this for initialization
	void Start () {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _animator = GetComponent<Animator>();
        Rider.centerOfMass = Skateboard.transform.position;
	}
	
    IEnumerator RotateMe(Vector3 byAngles, float inTime, Transform obj = null)
    {
        if (obj == null) {
        	obj = this.transform;
        }
        Quaternion fromAngle = obj.rotation;
        Quaternion toAngle = Quaternion.Euler(obj.eulerAngles + byAngles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime / inTime)
        {
            obj.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }

    bool IsGrounded() {
    	RaycastHit hit;
    	if (Physics.Raycast(Skateboard.transform.position, -Vector3.up, out hit, 0.5f)) {
            if (hit.collider.gameObject.name == "pPlane2")
            {
                _isOnPlane = true;
            }
            return true;
    	}
    	return false;
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
    }

    void OnJointBreak(float breakForce)
    {
        Rider.useGravity = true;
        Rider.mass = 200f;
        Rider.constraints = RigidbodyConstraints.None;
        Rider.velocity = Vector3.zero;
        Rider.angularVelocity = Vector3.zero;
        Rider.isKinematic = true;
        //Skateboard.velocity = Vector3.zero;
        //Skateboard.angularVelocity = Vector3.zero;
        GameManager.ShowDeadUI();
    }

    void Pedal() {
    	Skateboard.velocity += Skateboard.transform.forward * _pedalStrength;
    }

	void Update()
	{
		// if (Input.GetKey(KeyCode.W)) {
		// 	Rider.isKinematic = true;
		// } else {
		// 	Rider.isKinematic = false;
		// }
        float steer = Input.GetAxis("Horizontal");
        float accelerate = Input.GetAxis("Vertical");
        if (IsGrounded()) {
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
                Skateboard.AddForce(Vector3.up * _popModifier);
            }
            if (_isOnPlane)
            {
                //Rider.constraints = RigidbodyConstraints.FreezeRotationY;
                //Rider.constraints = RigidbodyConstraints.FreezeRotationZ;
                Skateboard.constraints = RigidbodyConstraints.FreezeRotationZ;
            } else
            {
                Rider.constraints = RigidbodyConstraints.None;
                Skateboard.constraints = RigidbodyConstraints.None;
            }

            transform.Rotate(Vector3.up * steer * _rotationModifier * Time.deltaTime);
        } else {
        	CameraFollower.IgnoreLateral = true;
            if (_animator.GetBool("Crouch"))
            {
                _animator.SetBool("Crouch", true);
            }
            Rider.constraints = RigidbodyConstraints.FreezeRotationX;
            // ignore lateral movement while in air to visualize 360, 180, etc.
            // airtime deceleration
            if (Input.GetKey(SpinRight)) {
        		StartCoroutine(RotateMe(transform.up * 90f, 0.5f, Rider.transform));
    		}
    		if (Input.GetKey(SpinLeft)) {
        		StartCoroutine(RotateMe(transform.up * -90f, 0.5f, Rider.transform));
    		}
    		if (Input.GetKey(FlipUp)) {
        		StartCoroutine(RotateMe(transform.forward * 180f, 0.5f, Rider.transform));
    		}
    		if (Input.GetKey(SpinRight)) {
        		StartCoroutine(RotateMe(transform.forward * -80f, 0.5f, Rider.transform));
    		}
        }
      //   if (IsGrounded()) {
	     //    _speed -= _decay * _speed;
      //   	// ignore lateral movement while in air to visualize 360, 180, etc.
      //   	CameraFollower.IgnoreLateral = false;
	     //    _speed = _speedModifier * accelerate * Time.Touch.deltaTime;
	     //    _speed = Mathf.Min(_maxSpeed, _speed);
	     //    _previousAccel = accelerate;
	     //    _currentOrientation = steer * _rotationModifier;
      //   } else {
      //   	// ignore lateral movement while in air to visualize 360, 180, etc.
      //   	CameraFollower.IgnoreLateral = true;

      //       // airtime deceleration
      //       _speed -= _decayAirtime * _speed;

      //       // rotate with air speed
    		// _currentOrientation = steer * _aerialRotationModifier;
      //   }
	}

    // void Update() {

    //     // death detection - useless to put in oncollider enter bc constant collisions
    //     // if (isDead()) {
    //     //     transform.position = _initialPosition;
    //     //     transform.rotation = _initialRotation;
    //     //     _speed = 0;
    //     //     return;
    //     // }
    //     float steer = Input.GetAxis("Horizontal");
    //     float accelerate = Input.GetAxis("Vertical");

    //     _speed -= _decay * _speed;
    //     if (_character.isGrounded) {

    //     	// follow lateral movement
    //     	CameraFollower.IgnoreLateral = false;

    // 		// acceleration calculation
    //         if (accelerate == _previousAccel) {
    //             // compound this if at max
    //             _framesMaxAccel += 1;
    //             _speed += _framesMaxAccel * accelerate * _speedModifier * Time.deltaTime;
    //         } else {
    //             _framesMaxAccel = 0;
    //             _speed += accelerate * _speedModifier * Time.deltaTime;
    //         }
    //         _speed = Mathf.Min(_maxSpeed, _speed);

    //     	RaycastHit hit;
    //     	// Get surface normal to adjust rotation
    //     	if (Physics.Raycast(transform.position, -_curNormal, out hit, 10.0f))
    // 	    {
				// float turn = steer * _rotationModifier * 100 * Time.deltaTime;
				// _curNormal = Vector3.Lerp(_curNormal, hit.normal, 4*Time.deltaTime);
				// Quaternion grndTilt = Quaternion.FromToRotation(Vector3.up, _curNormal);
		  //   	_currentOrientation = (this._currentOrientation + turn) % 360;
    //     		transform.rotation = grndTilt * Quaternion.Euler(0, _currentOrientation, 0);
    // 		}
    //     	// apply movement direction
    //         moveDirection = new Vector3(0, 0, _speed);
    //         moveDirection = transform.forward * _speed;
    //     } else {
    //     	// ignore lateral movement while in air to visualize 360, 180, etc.
    //     	CameraFollower.IgnoreLateral = true;

    //         // airtime deceleration
    //         _speed -= _decayAirtime * _speed;

    //         // rotate with air speed
	   //  	float turn = steer * _aerialRotationModifier * 100 * Time.deltaTime,
	   //  	_currentOrientation = (this._currentOrientation + turn) % 360;
    // 		transform.rotation = Quaternion.Euler(0, _currentOrientation, 0);

	   //      // simulate gravity
    // 		moveDirection.y -= _gravity * Time.deltaTime;
    //     }


    //     // final movement vector
    //     _character.Move(moveDirection * Time.deltaTime);

    //     // for acceleartion calculation
    //     _previousAccel = accelerate;
    // }
}
