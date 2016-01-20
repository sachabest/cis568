using UnityEngine;
using System.Collections;

public class Avatar : MonoBehaviour {

	public KeyCode Forward;
	public KeyCode Backward;
	public KeyCode Left;
	public KeyCode Right;

	public KeyCode SpinLeft;
	public KeyCode SpinRight;
	public KeyCode FlipUp;
	public KeyCode FlipDown;

	public KeyCode Pop;

	public Rigidbody PersonRigidBody;

	private Vector3 _speedVector;
	private Vector3 _leanVector;

	// Use this for initialization
	void Start () {
		_speedVector = new Vector3(400.0f, 0.0f, 0.0f);
		_leanVector = new Vector3(0.0f, 0.0f, 50.0f);
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetKey(Forward)) {
			this.PersonRigidBody.AddForce(-1 * _speedVector);
		} if (Input.GetKey(Backward)) {
			this.PersonRigidBody.AddForce(_speedVector);
		} if (Input.GetKey(Left)) {
			this.PersonRigidBody.AddForce(-1 * _leanVector);
		} if (Input.GetKey(Right)) {
			this.PersonRigidBody.AddForce(_leanVector);
		}
	}

}
