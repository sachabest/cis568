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

	public WheelCollider[] WHeelColliders;

    public GravityChange CurrentGravityChange = GravityChange.None;

	private float _maxTorque = 5000f;
    private float _equalityTolerance = 10f;
    private float _rotationModifier = 0.5f;
	// Use this for initialization
	void Start () {
	}
	
    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        Quaternion fromAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        Debug.Log(transform.rotation);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            Debug.Log(transform.rotation);
            yield return null;
        }
    }

    void Update() {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        switch (CurrentGravityChange) {
            case GravityChange.X:
                StartCoroutine(RotateMe(new Vector3(180f, 0f, 0f), _rotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.nX:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, 0f), _rotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.Y:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, -90f), _rotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            case GravityChange.nY:
                StartCoroutine(RotateMe(new Vector3(0f, 0f, 90f), _rotationModifier));
                CurrentGravityChange = GravityChange.None;
                break;
            default:
                break;
        }
    }

    void FixedUpdate() {
        float steer = Input.GetAxis("Horizontal");
        float accelerate = Input.GetAxis("Vertical");

        float finalAngle = steer * 5f;
        WHeelColliders[0].steerAngle = finalAngle;
        WHeelColliders[1].steerAngle = finalAngle;

        if (steer != 0) {
            return;
            // cant speed up while turning
        }

        for(int i = 0; i < 4; i++)
        {
            WHeelColliders[i].motorTorque = accelerate * _maxTorque;
        }
    }

}
