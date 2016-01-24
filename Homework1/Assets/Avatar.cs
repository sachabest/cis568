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

	public CapsuleCollider[] WheelColliders;
    public BoxCollider PersonCollider;

    public GravityChange CurrentGravityChange = GravityChange.None;

    public SmoothFollow CameraFollower;

    private float _gravityRotationModifier = 0.0f;
    private float _rotationModifier = 2f;
    private float _aerialRotationModifier = 10f;
    private float _speedModifier = 0.0001f;
    private float _speed = 0.0f;
    private float _decay = 0.01f;
    private float _decayAirtime = 0.005f;
    private float _previousAccel = 0.0f; // used for compound acceleration
    private long _framesMaxAccel = 0; // used for compound acceleration
    private float _groundDistance;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

	// Use this for initialization
	void Start () {
        _groundDistance = WheelColliders[0].bounds.extents.y;
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
	}
	
    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        Quaternion fromAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }

    bool isGrounded() {
        return Physics.Raycast(WheelColliders[0].transform.position, -Vector3.up, _groundDistance + 0.1f);
    }

    bool isDead() {
        return Mathf.Abs(PersonCollider.transform.position.y) < 0.2;
    }

    void Update() {
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

    void FixedUpdate() {

        // death detection - useless to put in oncollider enter bc constant collisions
        if (isDead()) {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _speed = 0;
            return;
        }

        float steer = Input.GetAxis("Horizontal");
        float accelerate = Input.GetAxis("Vertical");
        _speed -= _decay * _speed;

        if (isGrounded()) {
        	CameraFollower.IgnoreLateral = false;
            if (accelerate == _previousAccel) {
                // compound this if at max
                _framesMaxAccel += 1;
                _speed += _framesMaxAccel * accelerate * _speedModifier;
            } else {
                _framesMaxAccel = 0;
                _speed += accelerate * _speedModifier;
            }

            transform.Rotate(0, steer * _rotationModifier, 0);
        } else {
        	CameraFollower.IgnoreLateral = true;
            // airtime deceleration
            _speed -= _decayAirtime * _speed;

            transform.Rotate(0, steer * _aerialRotationModifier, 0);
        }
        transform.Translate(Vector3.forward * _speed);
        _previousAccel = accelerate;
    }
}
