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
	private float _landingTransitionTime = 1f;
	private float _landingTransitionDistance;

	private bool _onPark;
	private bool _upwardVelocityReset = false;
	public bool JustPopped;
	public bool IsOnPlane;

	// Use this for initialization
	void Start () {
		_skateboardCollider = GetComponent<BoxCollider>();
		_skateboardRigidbody = GetComponent<Rigidbody>();
	}
	
	void LandingLerp() {
		float distCovered = (Time.time - _startLerpTime) * _landingTransitionTime;
		float fractionCovered = distCovered / _landingTransitionDistance;
		if (fractionCovered >= 1f) {
			_landingInProgress = false;
			Avatar.instance.Land();
			return;
		}
		var groundYourself = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0, transform.position.z), fractionCovered);
		var temp = Vector3.Lerp(_landingStartPosition, _landingDestinationPosition, fractionCovered);
		Debug.Log(temp);
		transform.forward = temp;
		// transform.position = groundYourself;
		// somehow we need ot reorient the velocity in the new direction
		// _skateboardRigidbody.velocity = temp * _skateboardRigidbody.velocity.magnitude;
	}

	void VerticalTakeoffLerp() {
		float distCovered = (Time.time / _startLerpTime) * _landingTransitionTime;
		float fractionCovered = distCovered / _landingTransitionDistance;
		if (fractionCovered >= 1f) {
			_landingInProgress = false;
			Avatar.instance.Land();
			return;
		}
		var temp = Vector3.Lerp(_landingStartPosition, _landingDestinationPosition, fractionCovered);
		transform.right = temp;
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
			_skateboardRigidbody.constraints = RigidbodyConstraints.None;
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.name == "pPlane2") {
			// now we have to detect whether or not this should be a valid landing
			_colliderSaysJumping = true;
			Debug.Log("Collider says jumping");
		} else if (collision.gameObject.name == "SkateboardPark") {
			_colliderSaysJumping = true;
			Debug.Log("Collider says jumping");
			if (JustPopped && !_upwardVelocityReset && Vector3.Distance(transform.forward, Vector3.up) < 1.5f) {
				_skateboardRigidbody.velocity = Vector3.up * _skateboardRigidbody.velocity.magnitude;
				_upwardVelocityReset = true;
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
					_landingStartPosition = transform.forward;
					_landingDestinationPosition = tempVelocityVector;
					_landingTransitionDistance = Vector3.Distance(_landingStartPosition, _landingDestinationPosition);
					_colliderSaysJumping = false;
					_upwardVelocityReset = false;
					_landingInProgress = true;
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
