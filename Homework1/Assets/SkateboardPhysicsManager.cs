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
	public bool IsOnPlane;

	// Use this for initialization
	void Start () {
		_skateboardCollider = GetComponent<BoxCollider>();
		_skateboardRigidbody = GetComponent<Rigidbody>();
	}
	
	void LandingLerp() {
		float distCovered = (Time.time / _startLerpTime) * _landingTransitionTime;
		float fractionCovered = distCovered / _landingTransitionDistance;
		if (fractionCovered >= 1f) {
			_landingInProgress = false;
			return;
		}
		var temp = Vector3.Lerp(_landingStartPosition, _landingDestinationPosition, fractionCovered);
		Debug.Log(temp);
		transform.right = temp;
		// somehow we need ot reorient the velocity in the new direction
		// _skateboardRigidbody.velocity = temp * _skateboardRigidbody.velocity.magnitude;
	}
	// Update is called once per frame
	void Update () {
		if (_landingInProgress) {
			LandingLerp();
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.name == "pPlane2") {
			// now we have to detect whether or not this should be a valid landing
			_colliderSaysJumping = true;
			Debug.Log("Collider says jumping");
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
				if (_colliderSaysJumping) {
					// reset normals to correct direction
					_landingStartPosition = transform.right;
					_landingDestinationPosition = tempVelocityVector;
					_landingTransitionDistance = Vector3.Distance(_landingStartPosition, _landingDestinationPosition);
					_colliderSaysJumping = false;
					_landingInProgress = true;
					_startLerpTime = Time.time;
					// change velocity to account for change in forward

				}
    			break;
    		}
    	}
	}
}
