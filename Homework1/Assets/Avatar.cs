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

    private float _gravityRotationModifier = 0.0f;
    // private float _rotationModifier = 50f;
    private float _rotationModifier = 5f;
    private float _currentOrientation = 0f;
    private float _aerialRotationModifier = 200f;
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
    private Vector3 _launchNormal;
    private Vector3 _launchForward;
    private Vector3 moveDirection = Vector3.zero;
    private Animator _animator;
    private Collider _riderCollider;
    private Collider _skateboardCollider;
    private bool _ragdoll = false;

	// Use this for initialization
	void Start () {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _riderCollider = Rider.GetComponent<CapsuleCollider>();
        _skateboardCollider = Skateboard.GetComponent<BoxCollider>();
        _animator = GetComponent<Animator>();
		TurnRagdoll(false);
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
            Debug.Log("grounded");
            Quaternion q = Quaternion.AngleAxis(_rotationModifier * steer, Skateboard.transform.up) * Skateboard.transform.rotation;
            Skateboard.MoveRotation(q);
            float mag = Skateboard.velocity.magnitude;
            Skateboard.velocity = Skateboard.transform.forward * mag * 1f;
            // Skateboard.AddRelativeTorque(Vector3.up * steer * _rotationModifier * Time.deltaTime);
        }


    }

    void OnJointBreak(float breakForce)
    {
    	EndGame();
    }

    void EndGame() {
        Rider.useGravity = true;
        Rider.mass = 200f;
        Rider.constraints = RigidbodyConstraints.None;
        // Rider.velocity = Vector3.zero;
        // Rider.angularVelocity = Vector3.zero;
        // Rider.isKinematic = true;
        _ragdoll = true;
        //Skateboard.velocity = Vector3.zero;
        //Skateboard.angularVelocity = Vector3.zero;
        GameManager.ShowDeadUI();
    }

    void OnCollisionEnter(Collision collision) {
    	foreach (ContactPoint point in collision.contacts) {
    		if (point.thisCollider == _riderCollider) {
				EndGame();
				break;
    		};
    	}
    }

    void Pedal() {
    	// Skateboard.velocity += Skateboard.transform.forward * _pedalStrength;
        Skateboard.AddRelativeForce(Vector3.forward * _pedalStrength * 80, ForceMode.Acceleration);
    }

	void Update()
	{
		CameraFollower.target.position = SkeletonParent.transform.position;

		TurnRagdoll(_ragdoll);
		// if (Input.GetKey(KeyCode.W)) {
		// 	Rider.isKinematic = true;
		// } else {
		// 	Rider.isKinematic = false;
		// }
        if (SkateboardPhysics.IsGrounded()) {
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
                Skateboard.AddForce(Vector3.up * _popModifier);
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
        	CameraFollower.IgnoreLateral = true;
            if (_animator.GetBool("Crouch"))
            {
                _animator.SetBool("Crouch", false);
                _launchNormal = Rider.transform.up;
                _launchForward = Rider.transform.forward;
            }
            Rider.constraints = RigidbodyConstraints.FreezeRotationX;
        	// we need to rotate relative to the point of departure from the surface

            // Unity does some wacky shit with Gimball lock at z 270 degrees. 
            // Damn undocumented features ruin this program

            if (Input.GetKey(SpinRight)) {
        		Rider.transform.Rotate(_launchNormal * Time.deltaTime * _aerialRotationModifier * 1f);
    		}
    		if (Input.GetKey(SpinLeft)) {
        		Rider.transform.Rotate(_launchNormal * Time.deltaTime * _aerialRotationModifier * -1f);
    		}
    		if (Input.GetKey(FlipUp)) {
                var targetAerialRotation = _launchForward * Time.deltaTime * _aerialRotationModifier * 1f;
                if (90 - Rider.transform.eulerAngles.z < 3) {
                    // Rider.transform.rotation = 
                }
                targetAerialRotation.z = CalcEulerSafeX(targetAerialRotation.z);
        		Rider.transform.Rotate(targetAerialRotation);
    		}
    		if (Input.GetKey(FlipDown)) {
        		Rider.transform.Rotate(_launchForward * Time.deltaTime * _aerialRotationModifier * -1f);
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
