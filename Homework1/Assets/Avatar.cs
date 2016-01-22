using UnityEngine;
using System.Collections;

public class Avatar : MonoBehaviour {

	public KeyCode SpinLeft;
	public KeyCode SpinRight;
	public KeyCode FlipUp;
	public KeyCode FlipDown;

	public KeyCode Pop;

	public WheelCollider[] WHeelColliders;

	private float _maxTorque = 5000f;

	// Use this for initialization
	void Start () {
	}
	
    void FixedUpdate()
    {
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
