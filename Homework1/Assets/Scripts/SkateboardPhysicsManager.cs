using UnityEngine;
using System.Collections;

public class SkateboardPhysicsManager : MonoBehaviour {

	private Collider _skateboardCollider;
	private Rigidbody _skateboardRigidbody;

	private Vector3 _landingStartPosition;
	private Vector3 _landingDestinationPosition;
	private bool _colliderSaysJumping;
	private bool _landingInProgress;
	private float _startLerpTime;
	private float _landingTransitionTime = 400f;
	private float _landingTransitionDistance;
    private float _popModifier = 290.0f;
    private float _switchTolerance = 1.0f;
    public bool Switch;
	private bool _onPark;
	private bool _upwardVelocityReset = false;
	private AudioSource _audioSource;
	public AudioClip PopSound;
	public AudioClip LandSound;
	public bool JustPopped;
	public bool IsOnPlane;

	public Vector3 AerialRotationVector = Vector3.up;

	public SmoothFollow CameraFollower;
    public Joint SkateboardJoint;

	// Use this for initialization
	void Start () {
		_skateboardRigidbody = GetComponent<Rigidbody>();
		_audioSource = GetComponent<AudioSource>();
	}
	
	void LandingLerp() {
		float distCovered = (Time.time - _startLerpTime) * _landingTransitionTime;
		float fractionCovered = distCovered / _landingTransitionDistance;
		if (fractionCovered >= 1f) {
			_landingInProgress = false;
			Avatar.instance.Land();
			return;
		}
		var temp = Vector3.Lerp(_landingStartPosition, _landingDestinationPosition, fractionCovered);
		if (!Switch) {
			transform.forward = temp;
		} else {
			transform.forward = -temp;
		}
	}

    public void Pedal(float strength) {
    	if (Switch) {
        	_skateboardRigidbody.AddRelativeForce(-Vector3.forward * strength * 80f, ForceMode.Acceleration);
    	} else {
        	_skateboardRigidbody.AddRelativeForce(Vector3.forward * strength * 80f, ForceMode.Acceleration);
    	}
    }

	// Update is called once per frame
	void Update () {
		if (_landingInProgress) {
			LandingLerp();
		}
		if (JustPopped) {
			JustPopped = false;
		}
		if (IsGrounded() && !_onPark) {
			// _skateboardRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
			// transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		} else {
			// _skateboardRigidbody.constraints = RigidbodyConstraints.None;
		}
	}
	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.name == "pPlane2") {
			// now we have to detect whether or not this should be a valid landing
			_colliderSaysJumping = true;
		} else if (collision.gameObject.name == "SkateboardPark") {
			_colliderSaysJumping = true;
			if (JustPopped && !_upwardVelocityReset && Vector3.Distance(transform.forward, Vector3.up) < 1.5f) {
				_skateboardRigidbody.velocity = Vector3.up * _skateboardRigidbody.velocity.magnitude;
				_upwardVelocityReset = true;
				// find where the up vector poinds to find normal to surface
				float normalToSurface = Vector3.Distance(transform.up, Vector3.right);
				Debug.Log(normalToSurface);
			} else if (JustPopped && Vector3.Distance(transform.forward, Vector3.forward) < 1.5f) {
				AerialRotationVector = Vector3.up;
			}
		}
	}

   public bool IsGrounded() {
    	RaycastHit hit;
    	if (Physics.Raycast(this.transform.position, -Vector3.up, out hit, 0.5f)) {
            if (hit.collider.gameObject.name == "pPlane2")
            {
                IsOnPlane = true;
            }
            return true;
    	}
    	IsOnPlane = false;
    	return false;
    }

    public void Pop() {
        JustPopped = true;
        _skateboardRigidbody.AddForce(Vector3.up * _popModifier);
        _audioSource.clip = PopSound;
        _audioSource.volume = 1f;
        _audioSource.Play();
    }

	void OnCollisionEnter(Collision collision) {
    	foreach (ContactPoint point in collision.contacts) {
    		var thisCollider = point.thisCollider;
			if (thisCollider.GetType() == typeof(CapsuleCollider) && point.otherCollider.gameObject.name == "pPlane2") {
    			// now we have to detect whether or not this should be a valid landing
    			var tempVelocityVector = new Vector3(_skateboardRigidbody.velocity.x, 0, _skateboardRigidbody.velocity.z);
    			if (tempVelocityVector.magnitude < 0.1f) {
    				break;
    			}
    			_onPark = false;
				if (_colliderSaysJumping) {
					// reset normals to correct direction

					// to handle switch
					// check the difference betweewn normalized direction vectors -if they are opposite
					// it should be almost 0
					float normalizedDIfference = (tempVelocityVector.normalized - transform.forward.normalized).magnitude;
					if (normalizedDIfference >= _switchTolerance) {
						_landingStartPosition = transform.forward;
						Switch = true;
						CameraFollower.Switch = true;
					} else {
						Switch = false;
						CameraFollower.Switch = false;
						_landingStartPosition = transform.forward;
					}
					_landingDestinationPosition = tempVelocityVector;
					_landingTransitionDistance = Vector3.Distance(_landingStartPosition, _landingDestinationPosition);
					_colliderSaysJumping = false;
					_upwardVelocityReset = false;
					_landingInProgress = true;
					if (_audioSource.isPlaying) {
						_audioSource.clip = LandSound;
						_audioSource.Play();
					}
                    //SkateboardJoint.breakForce = float.PositiveInfinity;
                    //SkateboardJoint.breakTorque = float.PositiveInfinity;
					_startLerpTime = Time.time;
					// change velocity to account for change in forward

				}
    			break;
    		} else if (point.otherCollider.gameObject.name == "SkateboardPark") {
    			_onPark = true;
    		} else {
    			_onPark = false;
    		}
    	}
	}
}
